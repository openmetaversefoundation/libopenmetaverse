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
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using OpenMetaverse.StructuredData;

namespace OpenMetaverse.Http
{
    public class CapsClient
    {
        public delegate void DownloadProgressCallback(CapsClient client, int bytesReceived, int totalBytesToReceive);
        public delegate void CompleteCallback(CapsClient client, OSD result, Exception error);

        public event DownloadProgressCallback OnDownloadProgress;
        public event CompleteCallback OnComplete;

        public object UserData;

        protected Uri _Address;
        protected byte[] _PostData;
        protected X509Certificate2 _ClientCert;
        protected string _ContentType;
        protected HttpWebRequest _Request;
        protected OSD _Response;
        protected AutoResetEvent _ResponseEvent = new AutoResetEvent(false);

        public CapsClient(Uri capability)
            : this(capability, null)
        {
        }

        public CapsClient(Uri capability, X509Certificate2 clientCert)
        {
            _Address = capability;
            _ClientCert = clientCert;
        }

        public void BeginGetResponse(int millisecondsTimeout)
        {
            BeginGetResponse(null, null, millisecondsTimeout);
        }

        public void BeginGetResponse(OSD data, OSDFormat format, int millisecondsTimeout)
        {
            byte[] postData;
            string contentType;

            switch (format)
            {
                case OSDFormat.Xml:
                    postData = OSDParser.SerializeLLSDXmlBytes(data);
                    contentType = "application/llsd+xml";
                    break;
                case OSDFormat.Binary:
                    postData = OSDParser.SerializeLLSDBinary(data);
                    contentType = "application/llsd+binary";
                    break;
                case OSDFormat.Json:
                default:
                    postData = System.Text.Encoding.UTF8.GetBytes(OSDParser.SerializeJsonString(data));
                    contentType = "application/llsd+json";
                    break;
            }

            BeginGetResponse(postData, contentType, millisecondsTimeout);
        }

        public void BeginGetResponse(byte[] postData, string contentType, int millisecondsTimeout)
        {
            _PostData = postData;
            _ContentType = contentType;

            if (_Request != null)
            {
                _Request.Abort();
                _Request = null;
            }

            if (postData == null)
            {
                // GET
                //Logger.Log.Debug("[CapsClient] GET " + _Address);
                _Request = CapsBase.DownloadStringAsync(_Address, _ClientCert, millisecondsTimeout, DownloadProgressHandler,
                    RequestCompletedHandler);
            }
            else
            {
                // POST
                //Logger.Log.Debug("[CapsClient] POST (" + postData.Length + " bytes) " + _Address);
                _Request = CapsBase.UploadDataAsync(_Address, _ClientCert, contentType, postData, millisecondsTimeout, null,
                    DownloadProgressHandler, RequestCompletedHandler);
            }
        }

        public OSD GetResponse(int millisecondsTimeout)
        {
            BeginGetResponse(millisecondsTimeout);
            _ResponseEvent.WaitOne(millisecondsTimeout, false);
            return _Response;
        }

        public OSD GetResponse(OSD data, OSDFormat format, int millisecondsTimeout)
        {
            BeginGetResponse(data, format, millisecondsTimeout);
            _ResponseEvent.WaitOne(millisecondsTimeout, false);
            return _Response;
        }

        public OSD GetResponse(byte[] postData, string contentType, int millisecondsTimeout)
        {
            BeginGetResponse(postData, contentType, millisecondsTimeout);
            _ResponseEvent.WaitOne(millisecondsTimeout, false);
            return _Response;
        }

        public void Cancel()
        {
            if (_Request != null)
                _Request.Abort();
        }

        void DownloadProgressHandler(HttpWebRequest request, HttpWebResponse response, int bytesReceived, int totalBytesToReceive)
        {
            _Request = request;

            if (OnDownloadProgress != null)
            {
                try { OnDownloadProgress(this, bytesReceived, totalBytesToReceive); }
                catch (Exception ex) { Logger.Log.Error(ex.Message, ex); }
            }
        }

        void RequestCompletedHandler(HttpWebRequest request, HttpWebResponse response, byte[] responseData, Exception error)
        {
            _Request = request;

            OSD result = null;

            if (responseData != null)
            {
                try { result = OSDParser.Deserialize(responseData); }
                catch (Exception ex) { error = ex; }
            }

            FireCompleteCallback(result, error);
        }

        private void FireCompleteCallback(OSD result, Exception error)
        {
            CompleteCallback callback = OnComplete;
            if (callback != null)
            {
                try { callback(this, result, error); }
                catch (Exception ex) { Logger.Log.Error(ex.Message, ex); }
            }

            _Response = result;
            _ResponseEvent.Set();
        }
    }
}
