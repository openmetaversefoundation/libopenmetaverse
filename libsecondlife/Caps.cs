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
using libsecondlife.StructuredData;
using libsecondlife.Capabilities;

namespace libsecondlife
{
    /// <summary>
    /// Capabilities is the name of the bi-directional HTTP REST protocol that
    /// Second Life uses to communicate transactions such as teleporting or
    /// group messaging
    /// </summary>
    public class Caps
    {
        /// <summary>
        /// Triggered when an event is received via the EventQueueGet 
        /// capability
        /// </summary>
        /// <param name="message">Event name</param>
        /// <param name="body">Decoded event data</param>
        /// <param name="simulator">The simulator that generated the event</param>
        public delegate void EventQueueCallback(string message, StructuredData.LLSD body, Simulator simulator);

        /// <summary>Reference to the simulator this system is connected to</summary>
        public Simulator Simulator;

        internal string _SeedCapsURI;
        internal Dictionary<string, Uri> _Caps = new Dictionary<string, Uri>();

        private CapsClient _SeedRequest;
        private EventQueueClient _EventQueueCap = null;

        /// <summary>Capabilities URI this system was initialized with</summary>
        public string SeedCapsURI { get { return _SeedCapsURI; } }

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
        /// <param name="simulator"></param>
        /// <param name="seedcaps"></param>
        internal Caps(Simulator simulator, string seedcaps)
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
                _SeedRequest.Cancel();

            if (_EventQueueCap != null)
                _EventQueueCap.Stop(immediate);
        }

        /// <summary>
        /// Request the URI of a named capability
        /// </summary>
        /// <param name="capability">Name of the capability to request</param>
        /// <returns>The URI of the requested capability, or String.Empty if
        /// the capability does not exist</returns>
        public Uri CapabilityURI(string capability)
        {
            Uri cap;

            if (_Caps.TryGetValue(capability, out cap))
                return cap;
            else
                return null;
        }

        private void MakeSeedRequest()
        {
            if (Simulator == null || !Simulator.Client.Network.Connected)
                return;

            // Create a request list
            LLSDArray req = new LLSDArray();
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

            _SeedRequest = new CapsClient(new Uri(_SeedCapsURI));
            _SeedRequest.OnComplete += new CapsClient.CompleteCallback(SeedRequestCompleteHandler);
            _SeedRequest.StartRequest(req);
        }

        private void SeedRequestCompleteHandler(CapsClient client, LLSD result, Exception error)
        {
            if (result != null && result.Type == LLSDType.Map)
            {
                LLSDMap respTable = (LLSDMap)result;

                StringBuilder capsList = new StringBuilder();

                foreach (string cap in respTable.Keys)
                {
                    capsList.Append(cap);
                    capsList.Append(' ');

                    _Caps[cap] = respTable[cap].AsUri();
                }

                Simulator.Client.DebugLog("Got capabilities: " + capsList.ToString());

                if (_Caps.ContainsKey("EventQueueGet"))
                {
                    Simulator.Client.DebugLog("Starting event queue for " + Simulator.ToString());

                    _EventQueueCap = new EventQueueClient(_Caps["EventQueueGet"]);
                    _EventQueueCap.OnConnected += new EventQueueClient.ConnectedCallback(EventQueueConnectedHandler);
                    _EventQueueCap.OnEvent += new EventQueueClient.EventCallback(EventQueueEventHandler);
                    _EventQueueCap.Start();
                }
            }
            else
            {
                // The initial CAPS connection failed, try again
                MakeSeedRequest();
            }
        }

        private void EventQueueConnectedHandler()
        {
            Simulator.Client.Network.RaiseConnectedEvent(Simulator);
        }

        private void EventQueueEventHandler(string eventName, LLSDMap body)
        {
            if (Simulator.Client.Settings.SYNC_PACKETCALLBACKS)
                Simulator.Client.Network.CapsEvents.RaiseEvent(eventName, body, Simulator);
            else
                Simulator.Client.Network.CapsEvents.BeginRaiseEvent(eventName, body, Simulator);
        }
    }
}
