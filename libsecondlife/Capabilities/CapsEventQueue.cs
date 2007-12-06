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
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.IO;
using System.Threading;
using libsecondlife.StructuredData;

namespace libsecondlife.Capabilities
{
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

    public class CapsEventQueue
    {
        public const int HTTP_TIMEOUT = 1 * 30 * 1000;
        public const int BUFFER_SIZE = 1024;

        public Simulator Simulator;

        protected bool _Running = false;
        protected bool _Dead = false;

        protected HttpRequestState _RequestState;
        protected string _RequestURL;
        protected string _ProxyURL;
        protected bool _Aborted = false;

        public bool Running { get { return _Running; } }

        public CapsEventQueue(Simulator simulator, string eventQueueURI)
            : this(simulator, eventQueueURI, String.Empty)
        {
        }

        public CapsEventQueue(Simulator simulator, string eventQueueURI, string proxy)
        {
            Simulator = simulator;
            _RequestURL = eventQueueURI;
            _ProxyURL = proxy;
        }

        public void MakeRequest()
        {
            // Create an EventQueueGet request
            LLSDMap request = new LLSDMap();
            request["ack"] = new LLSD();
            request["done"] = LLSD.FromBoolean(false);

            byte[] postData = LLSDParser.SerializeXmlBytes(request);

            SendRequest(postData, "application/xml", null);
        }

        private void SendRequest(byte[] postData, string contentType, object state)
        {
            // Create a new HttpWebRequest
            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(_RequestURL);
            _RequestState = new HttpRequestState(httpRequest);

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
                    _RequestState.WebRequest.ContentType = "application/xml";
                    _RequestState.RequestData = postData;

                    IAsyncResult result = (IAsyncResult)_RequestState.WebRequest.BeginGetRequestStream(
                        new AsyncCallback(EventRequestStreamCallback), _RequestState);
                }
                else
                {
                    throw new ArgumentException("postData cannot be null for the event queue", "postData");
                }
            }
            catch (WebException e)
            {
                Abort(false, e);
            }
            catch (Exception e)
            {
                SecondLife.LogStatic("CapsEventQueue.MakeRequest(): " + e.ToString(), Helpers.LogLevel.Warning);
                Abort(false, null);
            }
        }

        public void Abort()
        {
            Stop(true);
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
                else
                {
                    string message = exception.Message.ToLower();

                    if (Helpers.StringContains(message, "502"))
                    {
                        // Don't log anything since 502 errors are so common
                    }
                    else if (Helpers.StringContains(message, "404") || Helpers.StringContains(message, "410"))
                    {
                        _Aborted = true;
                        Log("HttpBase.Abort(): HTTP request target is missing", Helpers.LogLevel.Debug);
                    }
                    else if (Helpers.StringContains(message, "aborted"))
                    {
                        // A callback threw an exception because the request is aborting, return to
                        // avoid circular problems
                        return;
                    }
                    else
                    {
                        Log(String.Format("HttpBase.Abort(): {0} (Status: {1})", exception.Message, exception.Status),
                            Helpers.LogLevel.Warning);
                    }
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
#if PocketPC
                Thread thread = new Thread(
                    delegate()
                    {
                        if (!newResult.AsyncWaitHandle.WaitOne(HTTP_TIMEOUT, false))
                            TimeoutCallback(_RequestState, true);
                    }
                );
                thread.Start();
#else
                ThreadPool.RegisterWaitForSingleObject(newResult.AsyncWaitHandle, new WaitOrTimerCallback(TimeoutCallback),
                    _RequestState, HTTP_TIMEOUT, true);
#endif
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
#if PocketPC
                Thread thread = new Thread(
                    delegate()
                    {
                        if (!asynchronousInputRead.AsyncWaitHandle.WaitOne(HTTP_TIMEOUT, false))
                            TimeoutCallback(_RequestState, true);
                    }
                );
                thread.Start();
#else
                ThreadPool.RegisterWaitForSingleObject(asynchronousInputRead.AsyncWaitHandle, new WaitOrTimerCallback(TimeoutCallback),
                    _RequestState, HTTP_TIMEOUT, true);
#endif
            }
            catch (WebException e)
            {
                Abort(false, e);
            }
        }

        public void Stop(bool immediate)
        {
            Simulator.Client.Log(String.Format("Event queue for {0} is {1}", Simulator, 
                (immediate ? "aborting" : "disconnecting")), Helpers.LogLevel.Info);

            _Dead = true;

            if (immediate)
            {
                _Running = false;

                // Abort the callback if it hasn't been already
                _RequestState.WebRequest.Abort();
            }
        }

        protected void EventRequestStreamCallback(IAsyncResult result)
        {
            bool raiseEvent = false;

            if (!_Dead)
            {
                if (!_Running) raiseEvent = true;

                // We are connected to the event queue
                _Running = true;
            }

            try
            {
                _RequestState = (HttpRequestState)result.AsyncState;
                Stream reqStream = _RequestState.WebRequest.EndGetRequestStream(result);

                reqStream.Write(_RequestState.RequestData, 0, _RequestState.RequestData.Length);
                reqStream.Close();

                IAsyncResult newResult = _RequestState.WebRequest.BeginGetResponse(new AsyncCallback(EventResponseCallback), _RequestState);
            }
            catch (WebException e)
            {
                Abort(false, e);
                return;
            }

            if (raiseEvent)
            {
                Simulator.Client.DebugLog("Capabilities event queue connected for " + Simulator.ToString());

                // The event queue is starting up for the first time
                Simulator.Client.Network.RaiseConnectedEvent(Simulator);
            }
        }

        protected void EventResponseCallback(IAsyncResult result)
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

        protected void RequestReply(HttpRequestState state, bool success, WebException exception)
        {
            LLSDArray events = null;
            int ack = 0;

            #region Exception Handling

            if (exception != null)
            {
                string message = exception.Message.ToLower();

                // Check what kind of exception happened
                if (Helpers.StringContains(message, "404") || Helpers.StringContains(message, "410"))
                {
                    Simulator.Client.Log("Closing event queue for " + Simulator.ToString() + " due to missing caps URI",
                        Helpers.LogLevel.Info);

                    _Running = false;
                    _Dead = true;
                }
                else if (!Helpers.StringContains(message, "aborted") && !Helpers.StringContains(message, "502"))
                {
                    Simulator.Client.Log(String.Format("Unrecognized caps exception for {0}: {1}", Simulator, exception.Message),
                        Helpers.LogLevel.Warning);
                }
            }

            #endregion Exception Handling

            #region Reply Decoding

            // Decode successful replies from the event queue
            if (success)
            {
                LLSDMap response = (LLSDMap)LLSDParser.DeserializeXml(state.ResponseData);

                if (response != null)
                {
                    // Parse any events returned by the event queue
                    events = (LLSDArray)response["events"];
                    ack = response["id"].AsInteger();
                }
            }

            #endregion Reply Decoding

            #region Make New Request

            if (_Running)
            {
                LLSDMap request = new LLSDMap();
                if (ack != 0) request["ack"] = LLSD.FromInteger(ack);
                else request["ack"] = new LLSD();
                request["done"] = LLSD.FromBoolean(_Dead);

                byte[] postData = LLSDParser.SerializeXmlBytes(request);

                SendRequest(postData, "application/xml", null);

                // If the event queue is dead at this point, turn it off since
                // that was the last thing we want to do
                if (_Dead)
                {
                    _Running = false;
                    SecondLife.DebugLogStatic("Sent event queue shutdown message");
                }
            }

            #endregion Make New Request

            #region Callbacks

            if (events != null && events.Count > 0)
            {
                // Fire callbacks for each event received
                foreach (LLSDMap evt in events)
                {
                    string msg = evt["message"].AsString();
                    LLSDMap body = (LLSDMap)evt["body"];

                    if (Simulator.Client.Settings.SYNC_PACKETCALLBACKS)
                        Simulator.Client.Network.CapsEvents.RaiseEvent(msg, body, Simulator);
                    else
                        Simulator.Client.Network.CapsEvents.BeginRaiseEvent(msg, body, Simulator);
                }
            }

            #endregion Callbacks
        }
    }
}
