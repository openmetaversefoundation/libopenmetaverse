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
using System.Collections;
using System.Net;
using System.Text;
using System.Threading;

namespace libsecondlife
{
    public class CapsEventQueue : HttpBase
    {
        public Simulator Simulator;
        public bool Running = false;

        internal bool Dead = false;

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
            Hashtable request = new Hashtable();
            request["ack"] = null;
            request["done"] = false;

            byte[] postData = LLSD.LLSDSerialize(request);

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

            // POST request
            RequestState.WebRequest.Method = "POST";
            RequestState.WebRequest.ContentLength = postData.Length;
            RequestState.RequestData = postData;

            IAsyncResult result = (IAsyncResult)RequestState.WebRequest.BeginGetRequestStream(
                new AsyncCallback(EventRequestStreamCallback), RequestState);

            // If there is a timeout, the callback fires and the request becomes aborted
            ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, new WaitOrTimerCallback(AbortCallback),
                RequestState, HTTP_TIMEOUT, true);
        }

        public new void Abort()
        {
            SecondLife.LogStatic("CapsEventQueue.Abort(): Aborting event queue", Helpers.LogLevel.Info);

            Dead = true;

            // Abort the callback if it hasn't been already
            RequestState.WebRequest.Abort();
        }

        protected void EventRequestStreamCallback(IAsyncResult result)
        {
            if (!Dead)
            {
                // We are connected to the event queue
                Running = true;

                SecondLife.DebugLogStatic("CapsEventQueue.RequestStreamCallback(): Event queue connected");

                RequestStreamCallback(result);
            }
        }

        protected override void RequestReply(HttpRequestState state, bool success, WebException exception)
        {
            if (Simulator == null)
                return;

            ArrayList events = null;
            int ack = 0;

            #region Exception Handling

            if (!Dead && exception != null)
            {
                // Check what kind of exception happened
                if (exception.Message.Contains("404") || exception.Message.Contains("410"))
                {
                    SecondLife.LogStatic("CapsEventQueue.RequestReply(): Closing event queue due to missing caps URI", 
                        Helpers.LogLevel.Error);

                    Running = false;
                    Dead = true;
                }
                else if (!exception.Message.Contains("aborted") && !exception.Message.Contains("502"))
                {
                    SecondLife.LogStatic("CapsEventQueue.RequestReply(): Unrecognized CAPS exception: " + exception.Message, 
                        Helpers.LogLevel.Warning);
                }
            }

            #endregion Exception Handling

            #region Reply Decoding

            // Decode successful replies from the event queue
            if (success)
            {
                Hashtable response = (Hashtable)LLSD.LLSDDeserialize(state.ResponseData);

                if (response != null)
                {
                    // Parse any events returned by the event queue
                    events = (ArrayList)response["events"];
                    ack = (int)response["id"];
                }
            }

            #endregion Reply Decoding

            #region Make New Request

            if (Running)
            {
                Hashtable request = new Hashtable();
                if (ack != 0) request["ack"] = ack;
                else request["ack"] = null;
                request["done"] = Dead;

                byte[] postData = LLSD.LLSDSerialize(request);

                MakeRequest(postData);

                // If the event queue is dead at this point, turn it off since
                // that was the last thing we want to do
                if (Dead) Running = false;
            }

            #endregion Make New Request

            #region Callbacks

            if (events != null && events.Count > 0)
            {
                // Fire callbacks for each event received
                lock (Simulator.Client.Network.EventQueueCallbacks)
                {
                    foreach (Hashtable evt in events)
                    {
                        string msg = (string)evt["message"];
                        Hashtable body = (Hashtable)evt["body"];

                        Simulator.Client.DebugLog(
                            String.Format("Event {0}: {1}{2}", msg, Environment.NewLine, LLSD.LLSDDump(body, 0)));

                        for (int i = 0; i < Simulator.Client.Network.EventQueueCallbacks.Count; i++)
                        {
                            try { Simulator.Client.Network.EventQueueCallbacks[i](msg, body, this); }
                            catch (Exception e) { Simulator.Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                        }
                    }
                }
            }

            #endregion Callbacks
        }
    }
}
