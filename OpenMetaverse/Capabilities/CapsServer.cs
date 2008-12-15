/*
 * Copyright (c) 2007-2008, openmetaverse.org
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
using System.IO;
using System.Net;
using OpenMetaverse.StructuredData;

namespace OpenMetaverse.Capabilities
{
    public class CapsServer
    {
        private struct CapsRedirector
        {
            public HttpServer.HttpRequestCallback LocalCallback;
            public Uri RemoteResource;

            public CapsRedirector(HttpServer.HttpRequestCallback localCallback, Uri remoteResource)
            {
                LocalCallback = localCallback;
                RemoteResource = remoteResource;
            }
        }

        HttpServer server;
        bool serverOwned;
        HttpServer.HttpRequestHandler capsHandler;
        ExpiringCache<UUID, CapsRedirector> expiringCaps = new ExpiringCache<UUID, CapsRedirector>();
        Dictionary<UUID, CapsRedirector> fixedCaps = new Dictionary<UUID, CapsRedirector>();
        object syncRoot = new object();

        public CapsServer(List<string> listeningPrefixes)
        {
            serverOwned = true;
            capsHandler = BuildCapsHandler("^/");
            server = new HttpServer(listeningPrefixes);
        }

        public CapsServer(HttpServer httpServer, string handlerPath)
        {
            serverOwned = false;
            capsHandler = BuildCapsHandler(handlerPath);
            server = httpServer;
        }

        public void Start()
        {
            server.AddHandler(capsHandler);

            if (serverOwned)
                server.Start();
        }

        public void Stop()
        {
            if (serverOwned)
                server.Stop();

            server.RemoveHandler(capsHandler);
        }

        public UUID CreateCapability(HttpServer.HttpRequestCallback localHandler)
        {
            UUID id = UUID.Random();
            CapsRedirector redirector = new CapsRedirector(localHandler, null);

            lock (syncRoot)
                fixedCaps.Add(id, redirector);

            return id;
        }

        public UUID CreateCapability(Uri remoteResource)
        {
            UUID id = UUID.Random();
            CapsRedirector redirector = new CapsRedirector(null, remoteResource);

            lock (syncRoot)
                fixedCaps.Add(id, redirector);

            return id;
        }

        public UUID CreateCapability(HttpServer.HttpRequestCallback localHandler, double ttlSeconds)
        {
            UUID id = UUID.Random();
            CapsRedirector redirector = new CapsRedirector(localHandler, null);

            lock (syncRoot)
                expiringCaps.Add(id, redirector, DateTime.Now + TimeSpan.FromSeconds(ttlSeconds));

            return id;
        }

        public UUID CreateCapability(Uri remoteResource, double ttlSeconds)
        {
            UUID id = UUID.Random();
            CapsRedirector redirector = new CapsRedirector(null, remoteResource);

            lock (syncRoot)
                expiringCaps.Add(id, redirector, DateTime.Now + TimeSpan.FromSeconds(ttlSeconds));

            return id;
        }

        public bool RemoveCapability(UUID id)
        {
            lock (syncRoot)
            {
                if (expiringCaps.Remove(id))
                    return true;
                else
                    return fixedCaps.Remove(id);
            }
        }

        bool CapsCallback(ref HttpListenerContext context)
        {
            UUID capsID;
            CapsRedirector redirector;
            bool success;
            string uuidString = context.Request.Url.Segments[context.Request.Url.Segments.Length - 1];

            if (UUID.TryParse(uuidString, out capsID))
            {
                lock (syncRoot)
                    success = (expiringCaps.TryGetValue(capsID, out redirector) || fixedCaps.TryGetValue(capsID, out redirector));

                if (success)
                {
                    if (redirector.RemoteResource != null)
                        ProxyCapCallback(ref context, redirector.RemoteResource);
                    else
                        redirector.LocalCallback(ref context);

                    return true;
                }
            }

            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            return true;
        }

        void ProxyCapCallback(ref HttpListenerContext context, Uri remoteResource)
        {
            const int BUFFER_SIZE = 2048;
            int numBytes;
            byte[] buffer = new byte[BUFFER_SIZE];

            // Proxy the request
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(remoteResource);

            request.Method = context.Request.HttpMethod;
            request.Headers.Add(context.Request.Headers);

            if (context.Request.HasEntityBody)
            {
                // Copy the request stream
                using (Stream writeStream = request.GetRequestStream())
                {
                    while ((numBytes = context.Request.InputStream.Read(buffer, 0, BUFFER_SIZE)) > 0)
                        writeStream.Write(buffer, 0, numBytes);

                    context.Request.InputStream.Close();
                }
            }

            System.Security.Cryptography.X509Certificates.X509Certificate2 cert = context.Request.GetClientCertificate();
            ;

            // Proxy the response
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            context.Response.StatusCode = (int)response.StatusCode;
            context.Response.StatusDescription = response.StatusDescription;
            context.Response.Headers = response.Headers;

            // Copy the response stream
            using (Stream readStream = response.GetResponseStream())
            {
                while ((numBytes = readStream.Read(buffer, 0, BUFFER_SIZE)) > 0)
                    context.Response.OutputStream.Write(buffer, 0, numBytes);

                context.Response.OutputStream.Close();
            }
        }

        HttpServer.HttpRequestHandler BuildCapsHandler(string path)
        {
            HttpRequestSignature signature = new HttpRequestSignature();
            signature.ContentType = "application/xml";
            signature.Path = path;
            return new HttpServer.HttpRequestHandler(signature, CapsCallback);
        }
    }
}
