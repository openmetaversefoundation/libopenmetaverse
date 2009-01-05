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
using System.Security.Cryptography.X509Certificates;
using OpenMetaverse.StructuredData;
using HttpServer;

namespace OpenMetaverse.Http
{
    public class CapsServer
    {
        struct CapsRedirector
        {
            public HttpRequestCallback LocalCallback;
            public Uri RemoteHandler;
            public bool ClientCertRequired;

            public CapsRedirector(HttpRequestCallback localCallback, Uri remoteHandler, bool clientCertRequired)
            {
                LocalCallback = localCallback;
                RemoteHandler = remoteHandler;
                ClientCertRequired = clientCertRequired;
            }
        }

        WebServer server;
        bool serverOwned;
        HttpRequestHandler capsHandler;
        ExpiringCache<UUID, CapsRedirector> expiringCaps = new ExpiringCache<UUID, CapsRedirector>();
        Dictionary<UUID, CapsRedirector> fixedCaps = new Dictionary<UUID, CapsRedirector>();
        object syncRoot = new object();

        public CapsServer(IPAddress address, int port)
        {
            serverOwned = true;
            capsHandler = BuildCapsHandler("^/");
            server = new WebServer(address, port);
            server.LogWriter = new log4netLogWriter(Logger.Log);
        }

        public CapsServer(IPAddress address, int port, X509Certificate sslCertificate, X509Certificate rootCA, bool requireClientCertificate)
        {
            serverOwned = true;
            capsHandler = BuildCapsHandler("^/");
            server = new WebServer(address, port, sslCertificate, rootCA, requireClientCertificate);
            server.LogWriter = new log4netLogWriter(Logger.Log);
        }

        public CapsServer(WebServer httpServer, string handlerPath)
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

        public UUID CreateCapability(HttpRequestCallback localHandler, bool clientCertRequired)
        {
            UUID id = UUID.Random();
            CapsRedirector redirector = new CapsRedirector(localHandler, null, clientCertRequired);

            lock (syncRoot)
                fixedCaps.Add(id, redirector);

            return id;
        }

        public UUID CreateCapability(Uri remoteHandler, bool clientCertRequired)
        {
            UUID id = UUID.Random();
            CapsRedirector redirector = new CapsRedirector(null, remoteHandler, clientCertRequired);

            lock (syncRoot)
                fixedCaps.Add(id, redirector);

            return id;
        }

        public UUID CreateCapability(HttpRequestCallback localHandler, bool clientCertRequired, double ttlSeconds)
        {
            UUID id = UUID.Random();
            CapsRedirector redirector = new CapsRedirector(localHandler, null, clientCertRequired);

            lock (syncRoot)
                expiringCaps.Add(id, redirector, DateTime.Now + TimeSpan.FromSeconds(ttlSeconds));

            return id;
        }

        public UUID CreateCapability(Uri remoteHandler, bool clientCertRequired, double ttlSeconds)
        {
            UUID id = UUID.Random();
            CapsRedirector redirector = new CapsRedirector(null, remoteHandler, clientCertRequired);

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

        bool CapsCallback(IHttpClientContext client, IHttpRequest request, IHttpResponse response)
        {
            UUID capsID;
            CapsRedirector redirector;
            bool success;
            string uuidString = request.UriParts[request.UriParts.Length - 1];

            if (UUID.TryParse(uuidString, out capsID))
            {
                lock (syncRoot)
                    success = (expiringCaps.TryGetValue(capsID, out redirector) || fixedCaps.TryGetValue(capsID, out redirector));

                if (success)
                {
                    if (redirector.ClientCertRequired)
                    {
                        success = false;
                        // FIXME: Implement this
                        /*X509Certificate2 clientCert = request.GetClientCertificate();
                        if (clientCert != null)
                        {
                            Logger.Log.Info(clientCert.ToString());
                        }*/
                    }

                    if (redirector.RemoteHandler != null)
                        ProxyCapCallback(client, request, response, redirector.RemoteHandler);
                    else
                        return redirector.LocalCallback(client, request, response);

                    return true;
                }
            }

            response.Status = HttpStatusCode.NotFound;
            return true;
        }

        void ProxyCapCallback(IHttpClientContext client, IHttpRequest request, IHttpResponse response, Uri remoteHandler)
        {
            const int BUFFER_SIZE = 2048;
            int numBytes;
            byte[] buffer = new byte[BUFFER_SIZE];

            // Proxy the request
            HttpWebRequest remoteRequest = (HttpWebRequest)HttpWebRequest.Create(remoteHandler);

            remoteRequest.Method = request.Method;
            remoteRequest.Headers.Add(request.Headers);

            // TODO: Support for using our own client certificate during the proxy

            if (request.Body.Length > 0)
            {
                // Copy the request stream
                using (Stream writeStream = remoteRequest.GetRequestStream())
                {
                    while ((numBytes = request.Body.Read(buffer, 0, BUFFER_SIZE)) > 0)
                        writeStream.Write(buffer, 0, numBytes);

                    request.Body.Close();
                }
            }

            // Proxy the response
            HttpWebResponse remoteResponse = (HttpWebResponse)remoteRequest.GetResponse();

            response.Status = remoteResponse.StatusCode;
            response.Reason = remoteResponse.StatusDescription;

            for (int i = 0; i < remoteResponse.Headers.Count; i++)
                response.AddHeader(remoteResponse.Headers.GetKey(i), remoteResponse.Headers[i]);

            // Copy the response stream
            using (Stream readStream = remoteResponse.GetResponseStream())
            {
                while ((numBytes = readStream.Read(buffer, 0, BUFFER_SIZE)) > 0)
                    response.Body.Write(buffer, 0, numBytes);

                response.Body.Close();
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
