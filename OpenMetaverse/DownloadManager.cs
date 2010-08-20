/*
 * Copyright (c) 2009, openmetaverse.org
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
using System.Linq;
using System.Text;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using OpenMetaverse.Http;

namespace OpenMetaverse
{
    /// <summary>
    /// Represends individual HTTP Download request
    /// </summary>
    public class DownloadRequest
    {
        /// <summary>URI of the item to fetch</summary>
        public Uri Address;
        /// <summary>Timout specified in milliseconds</summary>
        public int MillisecondsTimeout;
        /// <summary>Download progress callback</summary>
        public CapsBase.DownloadProgressEventHandler DownloadProgressCallback;
        /// <summary>Download completed callback</summary>
        public CapsBase.RequestCompletedEventHandler CompletedCallback;
        /// <summary>Accept the following content type</summary>
        public string ContentType;

        /// <summary>Default constructor</summary>
        public DownloadRequest()
        {
        }

        /// <summary>Constructor</summary>
        public DownloadRequest(Uri address, int millisecondsTimeout,
            string contentType,
            CapsBase.DownloadProgressEventHandler downloadProgressCallback,
            CapsBase.RequestCompletedEventHandler completedCallback)
        {
            this.Address = address;
            this.MillisecondsTimeout = millisecondsTimeout;
            this.DownloadProgressCallback = downloadProgressCallback;
            this.CompletedCallback = completedCallback;
            this.ContentType = contentType;
        }
    }

    /// <summary>
    /// Manages async HTTP downloads with a limit on maximum
    /// concurrent downloads
    /// </summary>
    public class DownloadManager
    {
        Queue<DownloadRequest> queue = new Queue<DownloadRequest>();
        List<HttpWebRequest> activeDownloads = new List<HttpWebRequest>();

        int m_ParallelDownloads = 20;
        X509Certificate2 m_ClientCert;

        /// <summary>Maximum number of parallel downloads from a single endpoint</summary>
        public int ParallelDownloads
        {
            get { return m_ParallelDownloads; }
            set { m_ParallelDownloads = value; }
        }

        /// <summary>Client certificate</summary>
        public X509Certificate2 ClientCert
        {
            get { return m_ClientCert; }
            set { m_ClientCert = value; }
        }

        /// <summary>Default constructor</summary>
        public DownloadManager()
        {
        }

        /// <summary>Cleanup method</summary>
        public virtual void Dispose()
        {
            lock (activeDownloads)
            {
                for (int i = 0; i < activeDownloads.Count; i++)
                {
                    try
                    {
                        activeDownloads[i].Abort();
                    }
                    catch { }
                }
            }
        }

        /// <summary>Setup http download request</summary>
        protected virtual HttpWebRequest SetupRequest(Uri address, string acceptHeader)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(address);
            request.Method = "GET";

            if (!string.IsNullOrEmpty(acceptHeader))
                request.Accept = acceptHeader;

            // Add the client certificate to the request if one was given
            if (m_ClientCert != null)
                request.ClientCertificates.Add(m_ClientCert);

            // Leave idle connections to this endpoint open for up to 60 seconds
            request.ServicePoint.MaxIdleTime = 0;
            // Disable stupid Expect-100: Continue header
            request.ServicePoint.Expect100Continue = false;
            // Crank up the max number of connections per endpoint (default is 2!)
            request.ServicePoint.ConnectionLimit = m_ParallelDownloads;

            return request;
        }

        /// <summary>Check the queue for pending work</summary>
        private void EnqueuePending()
        {
            lock (queue)
            {
                if (queue.Count > 0)
                {
                    int nr = 0;
                    lock (activeDownloads) nr = activeDownloads.Count;

                    for (int i = nr; i < ParallelDownloads && queue.Count > 0; i++)
                    {
                        DownloadRequest item = queue.Dequeue();
                        Logger.DebugLog("Requesting " + item.Address.ToString());
                        HttpWebRequest req = SetupRequest(item.Address, item.ContentType);
                        CapsBase.DownloadDataAsync(
                            req,
                            item.MillisecondsTimeout,
                            item.DownloadProgressCallback,
                            (HttpWebRequest request, HttpWebResponse response, byte[] responseData, Exception error) =>
                            {
                                lock (activeDownloads) activeDownloads.Remove(request);
                                item.CompletedCallback(request, response, responseData, error);
                                EnqueuePending();
                            }
                        );

                        lock (activeDownloads) activeDownloads.Add(req);
                    }
                }
            }
        }

        /// <summary>Enqueue a new HTPP download</summary>
        public void QueueDownlad(DownloadRequest req)
        {
            lock (queue)
            {
                queue.Enqueue(req);
            }
            EnqueuePending();
        }
    }
}
