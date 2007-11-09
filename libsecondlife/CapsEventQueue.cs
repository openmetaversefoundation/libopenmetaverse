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
using libsecondlife.LLSD;

namespace libsecondlife
{
    public class CapsEventQueue : HttpBase
    {
        public Simulator Simulator;

        protected bool _Running = false;
        protected bool _Dead = false;

        public bool Running { get { return _Running; } }

        public CapsEventQueue(Simulator simulator, string eventQueueURI)
            : this(simulator, eventQueueURI, String.Empty)
        {
        }

        public CapsEventQueue(Simulator simulator, string eventQueueURI, string proxy)
            : base(eventQueueURI, proxy)
        {
            Simulator = simulator;
        }

        protected override void RequestSent(HttpRequestState request)
        {
            ;
        }

        public new void MakeRequest()
        {
            // Create an EventQueueGet request
            Dictionary<string, object> request = new Dictionary<string, object>();
            request["ack"] = null;
            request["done"] = false;

            byte[] postData = LLSDParser.SerializeXmlBytes(request);

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

            // POST request
            _RequestState.WebRequest.Method = "POST";
            _RequestState.WebRequest.ContentLength = postData.Length;
            _RequestState.WebRequest.Headers.Add("X-SecondLife-UDP-Listen-Port", Simulator.udpPort.ToString());
            _RequestState.WebRequest.ContentType = "application/xml";
            _RequestState.RequestData = postData;

            IAsyncResult result = (IAsyncResult)_RequestState.WebRequest.BeginGetRequestStream(
                new AsyncCallback(EventRequestStreamCallback), _RequestState);
        }

        public new void MakeRequest(byte[] postData, string contentType, int udpListeningPort, object state)
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
                    _RequestState.WebRequest.Headers.Add("X-SecondLife-UDP-Listen-Port", Simulator.udpPort.ToString());
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
                SecondLife.LogStatic(String.Format("CapsEventQueue.MakeRequest(): {0} (Source: {1})", e.Message, e.Source),
                    Helpers.LogLevel.Warning);

                Abort(false, null);
            }
        }

        public new void Abort()
        {
            Disconnect(true);
        }

        public void Disconnect(bool immediate)
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

        protected override void RequestReply(HttpRequestState state, bool success, WebException exception)
        {
            List<object> events = null;
            int ack = 0;

            #region Exception Handling

            if (exception != null)
            {
                // Check what kind of exception happened
                if (exception.Message.Contains("404") || exception.Message.Contains("410"))
                {
                    Simulator.Client.Log("Closing event queue for " + Simulator.ToString() + " due to missing caps URI",
                        Helpers.LogLevel.Info);

                    _Running = false;
                    _Dead = true;
                }
                else if (!exception.Message.ToLower().Contains("aborted") && !exception.Message.Contains("502"))
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
                Dictionary<string, object> response = (Dictionary<string, object>)LLSDParser.DeserializeXml(state.ResponseData);

                if (response != null)
                {
                    // Parse any events returned by the event queue
                    events = (List<object>)response["events"];
                    ack = (int)response["id"];
                }
            }

            #endregion Reply Decoding

            #region Make New Request

            if (_Running)
            {
                Dictionary<string, object> request = new Dictionary<string, object>();
                if (ack != 0) request["ack"] = ack;
                else request["ack"] = null;
                request["done"] = _Dead;

                byte[] postData = LLSDParser.SerializeXmlBytes(request);

                MakeRequest(postData, "application/xml", 0, null);

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
                foreach (Dictionary<string, object> evt in events)
                {
                    string msg = (string)evt["message"];
                    Dictionary<string, object> body = (Dictionary<string, object>)evt["body"];

                    //Simulator.Client.DebugLog(
                    //    String.Format("[{0}] Event {1}: {2}{3}", Simulator, msg, Environment.NewLine, LLSD.LLSDDump(body, 0)));

                    if (Simulator.Client.Settings.SYNC_PACKETCALLBACKS)
                        Simulator.Client.Network.CapsEvents.RaiseEvent(msg, body, this);
                    else
                        Simulator.Client.Network.CapsEvents.BeginRaiseEvent(msg, body, this);
                }
            }

            #endregion Callbacks
        }
    }
}
