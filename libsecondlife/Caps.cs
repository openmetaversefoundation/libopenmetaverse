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

//HACK: Removes the 502 error messages 
//uncomment this line to reenable the 502 CAP Error Spam 
//#define CAPS_502_DEBUG 

using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.IO;


namespace libsecondlife
{
    /// <summary>
    /// Capabilities is the name of the bi-directional HTTP REST protocol that
    /// Second Life uses to communicate transactions such as teleporting or
    /// asset transfers
    /// </summary>
    public class Caps
    {
        /// <summary>
        /// Triggered when an event is received via the EventQueueGet capability
        /// </summary>
        /// <param name="message">Event name</param>
        /// <param name="body">Decoded event data</param>
        /// <param name="caps">The CAPS system that made the call</param>
        public delegate void EventQueueCallback(string message, Hashtable body, Caps caps);
        /// <summary>
        /// Triggered when an HTTP call in the queue is executed and a response
        /// is received
        /// </summary>
        /// <param name="body">Decoded response</param>
        public delegate void HTTPResponseCallback(Hashtable body);

        /// <summary>Reference to the SecondLife client this system is connected to</summary>
        public SecondLife Client;
        /// <summary>Reference to the simulator this system is connected to</summary>
        public Simulator Simulator;

        /// <summary></summary>
        public string SeedCapsURI { get { return Seedcaps; } }
        public bool IsEventQueueRunning { get { lock (SyncEventQueueRunning) return EventQueueRunning; } }

        internal string Seedcaps;
        internal StringDictionary Capabilities = new StringDictionary();

        private bool Dead = false;
        private bool EventQueueRunning = false;
        private object SyncEventQueueRunning = new object();
        private Thread CapsThread;
        private string EventQueueCap = String.Empty;
        private HttpWebRequest CapsRequest = null;
        private HttpWebRequest EventQueueRequest = null;
        private AsyncCallback EventQueueAsyncCallback;
        private AutoResetEvent KillEvent = new AutoResetEvent(false);

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="simulator"></param>
        /// <param name="seedcaps"></param>
        internal Caps(SecondLife client, Simulator simulator, string seedcaps)
        {
            Client = client;
            Simulator = simulator;
            Seedcaps = seedcaps;

            EventQueueAsyncCallback = new AsyncCallback(EventQueueHandler);

            CapsThread = new Thread(new ThreadStart(Run));
            CapsThread.Start();
        }

        /// <summary>
        /// Add an outgoing message to the HTTP queue, used for making CAPS
        /// calls
        /// </summary>
        /// <param name="url">CAPS URL to make the POST to</param>
        /// <param name="message">LLSD message to post to the URL</param>
        /// <param name="callback">Callback to fire when the request completes
        /// and a response is available</param>
        public void Post(string url, object message, HTTPResponseCallback callback)
        {
            //FIXME:
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="immediate"></param>
        public void Disconnect(bool immediate)
        {
            Client.DebugLog("Disconnecting CAPS for " + Simulator.ToString());

            Dead = true;
            lock (SyncEventQueueRunning) EventQueueRunning = false;

            if (immediate && EventQueueRequest != null)
                EventQueueRequest.Abort();

            KillEvent.Set();
        }

        private void Run()
        {
            ArrayList req = new ArrayList();
            req.Add("MapLayer");
            req.Add("MapLayerGod");
            req.Add("NewAgentInventory");
            req.Add("NewFileAgentInventory");
            req.Add("EventQueueGet");
            req.Add("UpdateGestureAgentInventory");
            req.Add("UpdateNotecardAgentInventory");
            req.Add("UpdateScriptAgentInventory");
            req.Add("UpdateGestureTaskInventory");
            req.Add("UpdateNotecardTaskInventory");
            req.Add("UpdateScriptTaskInventory");
            req.Add("SendPostcard");
            req.Add("ViewerStartAuction");
            req.Add("ParcelGodReserveForNewbie");
            req.Add("SendUserReport");
            req.Add("SendUserReportWithScreenshot");
            req.Add("RequestTextureDownload");

            byte[] data = LLSD.LLSDSerialize(req);

            try
            {
                CapsRequest = (HttpWebRequest)HttpWebRequest.Create(Seedcaps);
                CapsRequest.KeepAlive = false;
                CapsRequest.Timeout = Client.Settings.CAPS_TIMEOUT;
                CapsRequest.Method = "POST";
                CapsRequest.ContentLength = data.Length;

                CapsRequest.BeginGetRequestStream(new AsyncCallback(CapsRequestCallback), data);

                Client.DebugLog("Requesting initial CAPS request stream");
            }
            catch (WebException e)
            {
                HandleException(e, true);
                return;
            }

            KillEvent.WaitOne();
        }

        private void CapsRequestCallback(IAsyncResult result)
        {
            try
            {
                Client.DebugLog("Writing to initial CAPS request stream");

                byte[] data = (byte[])result.AsyncState;
                Stream reqStream = CapsRequest.EndGetRequestStream(result);

                reqStream.Write(data, 0, data.Length);
                reqStream.Close();

                Client.DebugLog("Requesting initial CAPS response stream");

                CapsRequest.BeginGetResponse(new AsyncCallback(CapsResponseCallback), null);
            }
            catch (WebException e)
            {
                HandleException(e, true);
                return;
            }
        }

        private void HandleException(WebException e, bool restartConnection)
        {
            if (Dead)
            {
                return;
            }
            else if (e.Message.Contains("404"))
            {
                // This capability no longer exists, disable it
                Client.Log("Disabling CAPS due to 404", Helpers.LogLevel.Error);
                Disconnect(true);
            }
            else if (restartConnection)
            {
                Client.Log("CAPS error initializing the connection, retrying. " + e.Message,
                    Helpers.LogLevel.Warning);
                Run();
            }
            else
            {
                #if !CAPS_502_DEBUG
                if (!e.Message.Contains("502"))
                {
                #endif
                    Client.DebugLog("CAPS response exception for " + Simulator.ToString() + ": " + e.Message);
                #if !CAPS_502_DEBUG
                }
                #endif
            }
        }

        private void CapsResponseCallback(IAsyncResult result)
        {
            byte[] buffer = null;

            try
            {
                Client.DebugLog("Receiving initial CAPS response stream");

                WebResponse response = CapsRequest.EndGetResponse(result);
                BinaryReader reader = new BinaryReader(response.GetResponseStream());
                buffer = reader.ReadBytes((int)response.ContentLength);
                response.Close();

                Client.DebugLog("Received initial CAPS response stream");
            }
            catch (WebException e)
            {
                HandleException(e, true);
                return;
            }

            if (buffer != null)
            {
                Hashtable resp = (Hashtable)LLSD.LLSDDeserialize(buffer);

                foreach (string cap in resp.Keys)
                {
                    Client.DebugLog(String.Format("Got cap {0}: {1}", cap, (string)resp[cap]));
                    Capabilities[cap] = (string)resp[cap];
                }

                if (Capabilities.ContainsKey("EventQueueGet"))
                {
                    EventQueueCap = Capabilities["EventQueueGet"];
                    Client.DebugLog("Running event queue for " + Simulator.ToString());
                    EventQueueHandler(null);
                }
            }
        }

        private void EventQueueHandler(IAsyncResult result)
        {
            byte[] buffer = null;
            int ack = 0;

            // Special handler for the first request
            if (result == null) goto MakeRequest;

            try
            {
                HttpWebResponse response = (HttpWebResponse)EventQueueRequest.EndGetResponse(result);
                BinaryReader reader = new BinaryReader(response.GetResponseStream());
                buffer = reader.ReadBytes((int)response.ContentLength);
                response.Close();
            }
            catch (WebException e)
            {
                HandleException(e, false);
            }

            if (buffer != null)
            {
                lock (Client.Network.EventQueueCallbacks)
                {
                    Hashtable resp = (Hashtable)LLSD.LLSDDeserialize(buffer);
                    ArrayList events = (ArrayList)resp["events"];
                    ack = (int)resp["id"];

                    foreach (Hashtable evt in events)
                    {
                        string msg = (string)evt["message"];
                        Hashtable body = (Hashtable)evt["body"];

                        Client.DebugLog("Event " + msg + ":" + Environment.NewLine + LLSD.LLSDDump(body, 0));

                        for (int i = 0; i < Client.Network.EventQueueCallbacks.Count; i++)
                        {
                            try { Client.Network.EventQueueCallbacks[i](msg, body, this); }
                            catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                        }
                    }
                }
            }

        MakeRequest:

            // Make a new request
            Hashtable req = new Hashtable();
            if (ack != 0) req["ack"] = ack;
            else req["ack"] = null;
            req["done"] = Dead;

            byte[] data = LLSD.LLSDSerialize(req);

            // Mark that the event queue is alive
            if (!Dead) lock (SyncEventQueueRunning) EventQueueRunning = true;

            try
            {
                EventQueueRequest = (HttpWebRequest)HttpWebRequest.Create(EventQueueCap);
                EventQueueRequest.KeepAlive = false;
                EventQueueRequest.Method = "POST";
                EventQueueRequest.ContentLength = data.Length;

                Stream reqStream = EventQueueRequest.GetRequestStream();
                reqStream.Write(data, 0, data.Length);
                reqStream.Close();

                if (!Dead)
                {
                    // Wait for an event to fire (or the connection to time out)
                    // and mark the queue as running
                    EventQueueRequest.BeginGetResponse(EventQueueAsyncCallback, EventQueueRequest);
                }
                else
                {
                    // CAPS connection is closed, send one last event request with done = true
                    // A null callback is used to prevent an infinite loop
                    EventQueueRequest.BeginGetResponse(null, null);
                }
            }
            catch (WebException e)
            {
                if (!Dead)
                {
                    Client.DebugLog("EventQueue request exception for " + Simulator.ToString() + ": " + e.Message);
                    goto MakeRequest;
                }
            }
        }
    }
}
