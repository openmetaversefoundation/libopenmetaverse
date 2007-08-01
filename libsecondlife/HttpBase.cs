/*
 * Copyright (c) 2007, Second Life Reverse Engineering Team
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the Second Life Reverse Engineering Team nor the names
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
using System.IO;
using System.Text;
using System.Threading;

namespace libsecondlife
{
    /// <summary>
    /// Stores the current state of an HTTP request
    /// </summary>
    public class HttpRequestState
    {
        const int BUFFER_SIZE = 1024;
        public byte[] RequestData;
        public byte[] ResponseData;
        public byte[] BufferRead;
        public HttpWebRequest WebRequest;
        public HttpWebResponse WebResponse;
        public Stream ResponseStream;

        internal int ResponseDataPos = 0;

        public HttpRequestState(HttpWebRequest webRequest)
        {
            WebRequest = webRequest;

            BufferRead = new byte[BUFFER_SIZE];
            RequestData = null;
            ResponseData = null;
            ResponseStream = null;
        }
    }

    public abstract class HttpBase
    {
        protected abstract void RequestReply(HttpRequestState state, bool success, WebException exception);
        protected abstract void RequestSent(HttpRequestState request);

        /// <summary>Buffer size for reading incoming responses</summary>
        protected const int BUFFER_SIZE = 1024;
        /// <summary>A default timeout for HTTP connections</summary>
        protected const int HTTP_TIMEOUT = 1 * 30 * 1000;

        protected HttpRequestState RequestState;
        protected string RequestURL;
        protected string ProxyURL;

        public HttpBase(string requestURL)
            : this(requestURL, String.Empty)
        { }

        public HttpBase(string requestURL, string proxyURL)
        {
            RequestURL = requestURL;
            ProxyURL = proxyURL;
        }

        public void MakeRequest()
        {
            MakeRequest(null);
        }

        public void MakeRequest(byte[] postData)
        {
            // Create a new HttpWebRequest
            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(RequestURL);
            RequestState = new HttpRequestState(httpRequest);

            if (ProxyURL != String.Empty)
            {
                // Create a proxy object
                WebProxy proxy = new WebProxy();

                // Associate a new Uri object to the _wProxy object, using the proxy address
                // selected by the user
                proxy.Address = new Uri(ProxyURL);

                // Finally, initialize the Web request object proxy property with the _wProxy
                // object
                httpRequest.Proxy = proxy;
            }

            // Always disable keep-alive for our purposes
            httpRequest.KeepAlive = false;

            try
            {
                if (postData != null && postData.Length > 0)
                {
                    //SecondLife.DebugLogStatic(String.Format("HttpBase.MakeRequest(): {0} byte POST", postData.Length));

                    // POST request
                    RequestState.WebRequest.Method = "POST";
                    RequestState.WebRequest.ContentLength = postData.Length;
                    RequestState.RequestData = postData;

                    IAsyncResult result = (IAsyncResult)RequestState.WebRequest.BeginGetRequestStream(
                        new AsyncCallback(RequestStreamCallback), RequestState);

                    // If there is a timeout, the callback fires and the request becomes aborted
                    ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, new WaitOrTimerCallback(AbortCallback),
                        RequestState, HTTP_TIMEOUT, true);
                }
                else
                {
                    // GET request
                    IAsyncResult result = (IAsyncResult)RequestState.WebRequest.BeginGetResponse(
                        new AsyncCallback(ResponseCallback), RequestState);

                    // If there is a timeout, the callback fires and the request becomes aborted
                    ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, new WaitOrTimerCallback(AbortCallback),
                        RequestState, HTTP_TIMEOUT, true);

                    // If we get here the request has been initialized, so fire the callback for a request being started
                    RequestSent(RequestState);
                }
            }
            catch (WebException e)
            {
                SecondLife.LogStatic(String.Format("HttpBase.MakeRequest(): {0} (Status: {1})", e.Message, e.Status),
                    Helpers.LogLevel.Warning);

                AbortCallback(false, e);
            }
            catch (Exception e)
            {
                SecondLife.LogStatic(String.Format("HttpBase.MakeRequest(): {0} (Source: {1})", e.Message, e.Source),
                    Helpers.LogLevel.Warning);

                AbortCallback(false, null);
            }
        }

        public void Abort()
        {
            AbortCallback(false, null);
        }

        protected void AbortCallback(object state, bool timedOut)
        {
            if (timedOut) AbortCallback(true, null);
            //else SecondLife.DebugLogStatic("HttpBase.AbortCallback(): timedOut = false");
        }

        protected void AbortCallback(bool fromTimeout, WebException exception)
        {
            if (fromTimeout)
                SecondLife.DebugLogStatic("HttpBase.AbortCallback(): HTTP request timed out");
            else if (exception == null)
                SecondLife.DebugLogStatic("HttpBase.AbortCallback(): HTTP request cancelled");

            // Abort the callback if it hasn't been already
            RequestState.WebRequest.Abort();

            // Fire the callback for the request completing
            try { RequestReply(RequestState, false, exception); }
            catch (Exception e) { SecondLife.LogStatic(e.ToString(), Helpers.LogLevel.Error); }
        }

        protected void RequestStreamCallback(IAsyncResult result)
        {
            //SecondLife.DebugLogStatic("HttpBase.RequestStreamCallback()");

            try
            {
                RequestState = (HttpRequestState)result.AsyncState;
                Stream reqStream = RequestState.WebRequest.EndGetRequestStream(result);

                reqStream.Write(RequestState.RequestData, 0, RequestState.RequestData.Length);
                reqStream.Close();

                IAsyncResult newResult = RequestState.WebRequest.BeginGetResponse(new AsyncCallback(ResponseCallback), RequestState);

                // If there is a timeout, the callback fires and the request becomes aborted
                ThreadPool.RegisterWaitForSingleObject(newResult.AsyncWaitHandle, new WaitOrTimerCallback(AbortCallback),
                    RequestState, HTTP_TIMEOUT, true);
            }
            catch (WebException e)
            {
                AbortCallback(false, e);
            }
        }

        private void ResponseCallback(IAsyncResult result)
        {
            //SecondLife.DebugLogStatic("HttpBase.ResponseCallback()");

            try
            {
                RequestState = (HttpRequestState)result.AsyncState;
                RequestState.WebResponse = (HttpWebResponse)RequestState.WebRequest.EndGetResponse(result);

                // Read the response into a Stream object
                Stream responseStream = RequestState.WebResponse.GetResponseStream();
                RequestState.ResponseStream = responseStream;

                // Begin reading of the contents of the response
                IAsyncResult asynchronousInputRead = responseStream.BeginRead(RequestState.BufferRead, 0, BUFFER_SIZE, 
                    new AsyncCallback(ReadCallback), RequestState);

                // If there is a timeout, the callback fires and the request becomes aborted
                ThreadPool.RegisterWaitForSingleObject(asynchronousInputRead.AsyncWaitHandle, new WaitOrTimerCallback(AbortCallback),
                    RequestState, HTTP_TIMEOUT, true);
            }
            catch (WebException e)
            {
                AbortCallback(false, e);
            }
        }

        private void ReadCallback(IAsyncResult result)
        {
            //SecondLife.DebugLogStatic("HttpBase.ReadCallback()");

            try
            {
                RequestState = (HttpRequestState)result.AsyncState;
                Stream responseStream = RequestState.ResponseStream;
                int read = responseStream.EndRead(result);

                // Check if we have read the entire response
                if (read > 0)
                {
                    // Create the byte array if it hasn't been created yet
                    if (RequestState.ResponseData == null || RequestState.ResponseData.Length != RequestState.WebResponse.ContentLength)
                        RequestState.ResponseData = new byte[RequestState.WebResponse.ContentLength];

                    // Copy the current buffer data in to the response variable
                    Buffer.BlockCopy(RequestState.BufferRead, 0, RequestState.ResponseData, RequestState.ResponseDataPos, read);
                    // Increment our writing position in the response variable
                    RequestState.ResponseDataPos += read;

                    // Continue reading the response until EndRead() returns 0
                    IAsyncResult asynchronousResult = responseStream.BeginRead(RequestState.BufferRead, 0, BUFFER_SIZE, 
                        new AsyncCallback(ReadCallback), RequestState);

                    return;
                }
                else
                {
                    // Fire the callback for receiving a response
                    try { RequestReply(RequestState, true, null); }
                    catch (Exception e) { SecondLife.LogStatic(e.ToString(), Helpers.LogLevel.Error); }

                    responseStream.Close();
                }
            }
            catch (WebException e)
            {
                SecondLife.LogStatic(String.Format("HttpBase.ReadCallback(): {0} (Status: {1})", e.Message, e.Status),
                    Helpers.LogLevel.Warning);

                AbortCallback(false, e);
            }
        }
    }
}
