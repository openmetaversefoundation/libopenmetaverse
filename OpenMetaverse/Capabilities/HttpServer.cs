/*
 * Copyright (c) 2008, openmetaverse.org
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.org nor the names
 *   of its contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Net;

namespace OpenMetaverse.Capabilities
{
    public class HttpServer
    {
        #region HttpRequestHandler struct

        public struct HttpRequestHandler : IEquatable<HttpRequestHandler>
        {
            public HttpRequestSignature Signature;
            public HttpRequestCallback Callback;

            public HttpRequestHandler(HttpRequestSignature signature, HttpRequestCallback callback)
            {
                Signature = signature;
                Callback = callback;
            }

            public override bool Equals(object obj)
            {
                return (obj is HttpRequestHandler) ? this.Signature == ((HttpRequestHandler)obj).Signature : false;
            }

            public override int GetHashCode()
            {
                return Signature.GetHashCode();
            }

            public override string ToString()
            {
                return Signature.ToString();
            }

            public bool Equals(HttpRequestHandler handler)
            {
                return this.Signature == handler.Signature;
            }
        }

        #endregion HttpRequestHandler struct

        public delegate bool HttpRequestCallback(ref HttpListenerContext context);

        HttpListener server = null;
        HttpRequestHandler[] requestHandlers = new HttpRequestHandler[0];
        bool isRunning = false;

        public HttpServer(int port, bool ssl)
        {
            server = new HttpListener();

            if (ssl)
                server.Prefixes.Add(String.Format("https://+:{0}/", port));
            else
                server.Prefixes.Add(String.Format("http://+:{0}/", port));
        }

        public HttpServer(List<string> prefixes)
        {
            server = new HttpListener();

            for (int i = 0; i < prefixes.Count; i++)
                server.Prefixes.Add(prefixes[i]);
        }

        public void AddHandler(string method, string contentType, string path, HttpRequestCallback callback)
        {
            HttpRequestSignature signature = new HttpRequestSignature();
            signature.Method = method;
            signature.ContentType = contentType;
            signature.Path = path;
            AddHandler(new HttpRequestHandler(signature, callback));
        }

        public void AddHandler(HttpRequestHandler handler)
        {
            HttpRequestHandler[] newHandlers = new HttpRequestHandler[requestHandlers.Length + 1];

            for (int i = 0; i < requestHandlers.Length; i++)
                newHandlers[i] = requestHandlers[i];
            newHandlers[requestHandlers.Length] = handler;

            // CLR guarantees this is an atomic operation
            requestHandlers = newHandlers;
        }

        public void RemoveHandler(HttpRequestHandler handler)
        {
            HttpRequestHandler[] newHandlers = new HttpRequestHandler[requestHandlers.Length - 1];

            int j = 0;
            for (int i = 0; i < requestHandlers.Length; i++)
                if (!requestHandlers[i].Signature.ExactlyEquals(handler.Signature))
                    newHandlers[j++] = handler;

            // CLR guarantees this is an atomic operation
            requestHandlers = newHandlers;
        }

        public void Start()
        {
            server.Start();
            server.BeginGetContext(BeginGetContextCallback, server);
            isRunning = true;
        }

        public void Stop()
        {
            isRunning = false;
            try { server.Stop(); }
            catch (ObjectDisposedException) { }
        }

        protected void BeginGetContextCallback(IAsyncResult result)
        {
            HttpListenerContext context = null;

            // Retrieve the incoming request
            try { context = server.EndGetContext(result); }
            catch (Exception) { }

            if (isRunning)
            {
                // Immediately start listening again
                try { server.BeginGetContext(BeginGetContextCallback, server); }
                catch (Exception)
                {
                    // Something went wrong, can't resume listening. Bail out now
                    // since this is a shutdown (whether it was meant to be or not)
                    isRunning = false;
                    return;
                }

                // Process the incoming request
                if (context != null)
                {
                    // Create a request signature
                    HttpRequestSignature signature = new HttpRequestSignature(context);

                    // Look for a signature match in our handlers
                    for (int i = 0; i < requestHandlers.Length; i++)
                    {
                        HttpRequestHandler handler = requestHandlers[i];

                        if (handler.Signature != null && signature == handler.Signature)
                        {
                            bool closeConnection = true;

                            // Request signature matched, handle it
                            try { closeConnection = handler.Callback(ref context); }
                            catch (Exception ex) { Logger.Log("Exception in HTTP handler: " + ex.Message, Helpers.LogLevel.Error, ex); }

                            if (closeConnection)
                            {
                                // Close the connection
                                try { context.Response.Close(); }
                                catch (Exception) { }
                            }

                            return;
                        }
                    }

                    // No registered handler matched this request's signature. Send a 404
                    try
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        context.Response.StatusDescription = String.Format(
                            "No request handler registered for Method=\"{0}\", Content-Type=\"{1}\", Path=\"{2}\"",
                            signature.Method, signature.ContentType, signature.Path);
                        context.Response.Close();
                    }
                    catch (Exception) { }
                }
            }
        }
    }
}
