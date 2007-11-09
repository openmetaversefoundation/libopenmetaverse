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
        public object State;

        internal int ResponseDataPos = 0;

        public HttpRequestState(HttpWebRequest webRequest)
        {
            WebRequest = webRequest;
            BufferRead = new byte[BUFFER_SIZE];
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

        protected HttpRequestState _RequestState;
        protected string _RequestURL;
        protected string _ProxyURL;
        protected bool _Aborted = false;

        public HttpBase(string requestURL)
            : this(requestURL, String.Empty)
        { }

        public HttpBase(string requestURL, string proxyURL)
        {
            _RequestURL = requestURL;
            _ProxyURL = proxyURL;
        }

        public void MakeRequest()
        {
            MakeRequest(null, null, 0, null);
        }

        public void MakeRequest(byte[] postData, string contentType, int udpListeningPort, object state)
        {
            // Create a new HttpWebRequest
            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(_RequestURL);
            _RequestState = new HttpRequestState(httpRequest);
            _RequestState.State = state;

            if (_ProxyURL != String.Empty)
            {
                // Create a proxy object
                WebProxy proxy = new WebProxy();

                // Associate a new Uri object to the _wProxy object, using the proxy address
                // selected by the user
                proxy.Address = new Uri(_ProxyURL);

                // Finally, initialize the Web request object proxy property with the _wProxy
                // object
                httpRequest.Proxy = proxy;
            }

            // Always disable keep-alive for our purposes
            httpRequest.KeepAlive = false;

            try
            {
                if (postData != null)
                {
                    // POST request
                    _RequestState.WebRequest.Method = "POST";
                    _RequestState.WebRequest.ContentLength = postData.Length;
                    if (udpListeningPort > 0)
                        _RequestState.WebRequest.Headers.Add("X-SecondLife-UDP-Listen-Port", udpListeningPort.ToString());
                    if (String.IsNullOrEmpty(contentType))
                        _RequestState.WebRequest.ContentType = "application/xml";
                    else
                        _RequestState.WebRequest.ContentType = contentType;
                    _RequestState.RequestData = postData;

                    IAsyncResult result = (IAsyncResult)_RequestState.WebRequest.BeginGetRequestStream(
                        new AsyncCallback(RequestStreamCallback), _RequestState);

                    // If there is a timeout, the callback fires and the request becomes aborted
                    ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, new WaitOrTimerCallback(TimeoutCallback),
                        _RequestState, HTTP_TIMEOUT, true);
                }
                else
                {
                    // GET request
                    IAsyncResult result = (IAsyncResult)_RequestState.WebRequest.BeginGetResponse(
                        new AsyncCallback(ResponseCallback), _RequestState);

                    // If there is a timeout, the callback fires and the request becomes aborted
                    ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, new WaitOrTimerCallback(TimeoutCallback),
                        _RequestState, HTTP_TIMEOUT, true);

                    // If we get here the request has been initialized, so fire the callback for a request being started
                    RequestSent(_RequestState);
                }
            }
            catch (WebException e)
            {
                Abort(false, e);
            }
            catch (Exception e)
            {
                Log(String.Format("HttpBase.MakeRequest(): {0} (Source: {1})", e.Message, e.Source),
                    Helpers.LogLevel.Warning);

                Abort(false, null);
            }
        }

        public void Abort()
        {
            Abort(false, null);
        }

        protected void TimeoutCallback(object state, bool timedOut)
        {
            if (timedOut) Abort(true, null);
        }

        protected void Abort(bool fromTimeout, WebException exception)
        {
            if (fromTimeout)
            {
                Log("HttpBase.Abort(): HTTP request timed out", Helpers.LogLevel.Debug);
            }
            else
            {
                if (exception == null)
                {
                    _Aborted = true;
                    //Log("HttpBase.Abort(): HTTP request aborted", Helpers.LogLevel.Debug);
                }
                else if (exception.Message.Contains("404") || exception.Message.Contains("410"))
                {
                    _Aborted = true;
                    Log("HttpBase.Abort(): HTTP request target is missing", Helpers.LogLevel.Debug);
                }
                else if (exception.Message.Contains("Aborted") || exception.Message.Contains("aborted"))
                {
                    // A callback threw an exception because the request is aborting, return to
                    // avoid circular problems
                    return;
                }
                else if (exception.Message.Contains("502"))
                {
                    // Don't log anything since 502 errors are so common
                }
                else
                {
                    Log(String.Format("HttpBase.Abort(): {0} (Status: {1})", exception.Message, exception.Status),
                        Helpers.LogLevel.Warning);
                }
            }

            // Abort the callback if it hasn't been already
            _RequestState.WebRequest.Abort();

            // Fire the callback for the request completing
            try { RequestReply(_RequestState, false, exception); }
            catch (Exception e) { Log(e.ToString(), Helpers.LogLevel.Error); }
        }

        protected virtual void Log(string message, Helpers.LogLevel level)
        {
            if (level == Helpers.LogLevel.Debug)
                SecondLife.DebugLogStatic(message);
            else
                SecondLife.LogStatic(message, level);
        }

        protected void RequestStreamCallback(IAsyncResult result)
        {
            try
            {
                _RequestState = (HttpRequestState)result.AsyncState;
                Stream reqStream = _RequestState.WebRequest.EndGetRequestStream(result);

                reqStream.Write(_RequestState.RequestData, 0, _RequestState.RequestData.Length);
                reqStream.Close();

                IAsyncResult newResult = _RequestState.WebRequest.BeginGetResponse(new AsyncCallback(ResponseCallback), _RequestState);

                // If there is a timeout, the callback fires and the request becomes aborted
                ThreadPool.RegisterWaitForSingleObject(newResult.AsyncWaitHandle, new WaitOrTimerCallback(TimeoutCallback),
                    _RequestState, HTTP_TIMEOUT, true);
            }
            catch (WebException e)
            {
                Abort(false, e);
            }
        }

        private void ResponseCallback(IAsyncResult result)
        {
            try
            {
                _RequestState = (HttpRequestState)result.AsyncState;
                _RequestState.WebResponse = (HttpWebResponse)_RequestState.WebRequest.EndGetResponse(result);

                // Read the response into a Stream object
                Stream responseStream = _RequestState.WebResponse.GetResponseStream();
                _RequestState.ResponseStream = responseStream;

                // Begin reading of the contents of the response
                IAsyncResult asynchronousInputRead = responseStream.BeginRead(_RequestState.BufferRead, 0, BUFFER_SIZE, 
                    new AsyncCallback(ReadCallback), _RequestState);

                // If there is a timeout, the callback fires and the request becomes aborted
                ThreadPool.RegisterWaitForSingleObject(asynchronousInputRead.AsyncWaitHandle, new WaitOrTimerCallback(TimeoutCallback),
                    _RequestState, HTTP_TIMEOUT, true);
            }
            catch (WebException e)
            {
                Abort(false, e);
            }
        }

        protected void ReadCallback(IAsyncResult result)
        {
            try
            {
                _RequestState = (HttpRequestState)result.AsyncState;
                Stream responseStream = _RequestState.ResponseStream;
                int read = responseStream.EndRead(result);

                // Check if we have read the entire response
                if (read > 0)
                {
                    // Create the byte array if it hasn't been created yet
                    if (_RequestState.ResponseData == null || _RequestState.ResponseData.Length != _RequestState.WebResponse.ContentLength)
                        _RequestState.ResponseData = new byte[_RequestState.WebResponse.ContentLength];

                    // Copy the current buffer data in to the response variable
                    Buffer.BlockCopy(_RequestState.BufferRead, 0, _RequestState.ResponseData, _RequestState.ResponseDataPos, read);
                    // Increment our writing position in the response variable
                    _RequestState.ResponseDataPos += read;

                    // Continue reading the response until EndRead() returns 0
                    IAsyncResult asynchronousResult = responseStream.BeginRead(_RequestState.BufferRead, 0, BUFFER_SIZE, 
                        new AsyncCallback(ReadCallback), _RequestState);

                    return;
                }
                else
                {
                    // Fire the callback for receiving a response
                    try { RequestReply(_RequestState, true, null); }
                    catch (Exception e) { Log(e.ToString(), Helpers.LogLevel.Error); }

                    responseStream.Close();
                }
            }
            catch (WebException e)
            {
                Abort(false, e);
            }
        }
    }
}
