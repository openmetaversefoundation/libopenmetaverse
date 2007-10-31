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
using System.Text;
using System.Threading;
using libsecondlife.LLSD;

namespace libsecondlife
{
    /// <summary>
    /// Capabilities is the name of the bi-directional HTTP REST protocol that
    /// Second Life uses to communicate transactions such as teleporting or
    /// group messaging
    /// </summary>
    public class Capabilities
    {
        /// <summary>
        /// Triggered when an event is received via the EventQueueGet 
        /// capability
        /// </summary>
        /// <param name="message">Event name</param>
        /// <param name="body">Decoded event data</param>
        /// <param name="caps">The CAPS system that made the call</param>
        public delegate void EventQueueCallback(string message, Dictionary<string, object> body, CapsEventQueue eventQueue);
        /// <summary>
        /// Triggered when an HTTP call in the queue is executed and a response
        /// is received
        /// </summary>
        /// <param name="body">Decoded response</param>
        /// <param name="request">Original capability request</param>
        public delegate void CapsResponseCallback(Dictionary<string, object> body, HttpRequestState request);

        /// <summary>Reference to the simulator this system is connected to</summary>
        public Simulator Simulator;

        internal string _SeedCapsURI;
        internal Dictionary<string, string> _Caps = new Dictionary<string, string>();

        private CapsRequest _SeedRequest;
        private CapsEventQueue _EventQueueCap = null;

        /// <summary>Capabilities URI this system was initialized with</summary>
        public string SeedCapsURI { get { return _SeedCapsURI; } }
        public ManualResetEvent CapsReceivedEvent = new ManualResetEvent(false);

        /// <summary>Whether the capabilities event queue is connected and
        /// listening for incoming events</summary>
        public bool IsEventQueueRunning
        {
            get
            {
                if (_EventQueueCap != null)
                    return _EventQueueCap.Running;
                else
                    return false;
            }
        }


        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="simulator"></param>
        /// <param name="seedcaps"></param>
        internal Capabilities(Simulator simulator, string seedcaps)
        {
            Simulator = simulator;
            _SeedCapsURI = seedcaps;

            MakeSeedRequest();
        }

        public void Disconnect(bool immediate)
        {
            Simulator.Client.Log(String.Format("Caps system for {0} is {1}", Simulator,
                (immediate ? "aborting" : "disconnecting")), Helpers.LogLevel.Info);

            if (_SeedRequest != null)
            {
                _SeedRequest.Abort();
            }

            if (_EventQueueCap != null)
            {
                _EventQueueCap.Disconnect(immediate);
            }
        }

        /// <summary>
        /// Request the URI of a named capability
        /// </summary>
        /// <param name="capability">Name of the capability to request</param>
        /// <returns>The URI of the requested capability, or String.Empty if
        /// the capability does not exist</returns>
        public string CapabilityURI(string capability)
        {
            string cap;

            if (_Caps.TryGetValue(capability, out cap))
                return cap;
            else
                return String.Empty;
        }

        private void MakeSeedRequest()
        {
            if (Simulator == null || !Simulator.Client.Network.Connected)
                return;

            // Create a request list
            List<object> req = new List<object>();
            req.Add("MapLayer");
            req.Add("MapLayerGod");
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
            req.Add("UntrustedSimulatorMessage");
            req.Add("ParcelVoiceInfoRequest");
            req.Add("ChatSessionRequest");
            req.Add("ProvisionVoiceAccountRequest");

            byte[] postData = LLSDParser.SerializeXmlBytes(req);

            Simulator.Client.DebugLog("Making initial capabilities connection for " + Simulator.ToString());

            _SeedRequest = new CapsRequest(_SeedCapsURI, String.Empty, null);
            _SeedRequest.OnCapsResponse += new CapsRequest.CapsResponseCallback(seedRequest_OnCapsResponse);
            _SeedRequest.MakeRequest(postData, "application/xml", 0, null);
        }

        private void seedRequest_OnCapsResponse(object response, HttpRequestState state)
        {
            if (response is Dictionary<string, object>)
            {
                Dictionary<string, object> respTable = (Dictionary<string, object>)response;

                StringBuilder capsList = new StringBuilder();

                foreach (string cap in respTable.Keys)
                {
                    capsList.Append(cap);
                    capsList.Append(' ');

                    _Caps[cap] = (string)respTable[cap];
                }

                Simulator.Client.DebugLog("Got capabilities: " + capsList.ToString());

                // Signal that we have connected to the CAPS server and received a list of capability URIs
                CapsReceivedEvent.Set();

                if (_Caps.ContainsKey("EventQueueGet"))
                {
                    _EventQueueCap = new CapsEventQueue(Simulator, _Caps["EventQueueGet"]);
                    Simulator.Client.DebugLog("Starting event queue for " + Simulator.ToString());

                    _EventQueueCap.MakeRequest();
                }
            }
            else
            {
                // The initial CAPS connection failed, try again
                MakeSeedRequest();
            }
        }
    }
}
