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
using System.Collections.Generic;
using System.Text;
using System.Threading;
using OpenMetaverse.StructuredData;
using OpenMetaverse.Capabilities;

namespace OpenMetaverse
{
    /// <summary>
    /// Capabilities is the name of the bi-directional HTTP REST protocol
    /// used to communicate non real-time transactions such as teleporting or
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
            Logger.Log(String.Format("Caps system for {0} is {1}", Simulator,
                (immediate ? "aborting" : "disconnecting")), Helpers.LogLevel.Info, Simulator.Client);

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
            req.Add("ChatSessionRequest");
            req.Add("CopyInventoryFromNotecard");
            req.Add("DispatchRegionInfo");
            req.Add("EstateChangeInfo");
            req.Add("EventQueueGet");
            req.Add("FetchInventoryDescendents");
            req.Add("GroupProposalBallot");
            req.Add("MapLayer");
            req.Add("MapLayerGod");
            req.Add("NewFileAgentInventory");
            req.Add("ParcelPropertiesUpdate");
            req.Add("ParcelVoiceInfoRequest");
            req.Add("ProvisionVoiceAccountRequest");
            req.Add("RemoteParcelRequest");
            req.Add("RequestTextureDownload");
            req.Add("SearchStatRequest");
            req.Add("SearchStatTracking");
            req.Add("SendPostcard");
            req.Add("SendUserReport");
            req.Add("SendUserReportWithScreenshot");
            req.Add("ServerReleaseNotes");
            req.Add("StartGroupProposal");
            req.Add("UpdateGestureAgentInventory");
            req.Add("UpdateNotecardAgentInventory");
            req.Add("UpdateScriptAgentInventory");
            req.Add("UpdateGestureTaskInventory");
            req.Add("UpdateNotecardTaskInventory");
            req.Add("UpdateScriptTaskInventory");
            req.Add("ViewerStartAuction");
            req.Add("UntrustedSimulatorMessage");
            req.Add("ViewerStats");

            _SeedRequest = new CapsClient(new Uri(_SeedCapsURI));
            _SeedRequest.OnComplete += new CapsClient.CompleteCallback(SeedRequestCompleteHandler);
            _SeedRequest.StartRequest(req);
        }

        private void SeedRequestCompleteHandler(CapsClient client, LLSD result, Exception error)
        {
            if (result != null && result.Type == LLSDType.Map)
            {
                LLSDMap respTable = (LLSDMap)result;

                foreach (string cap in respTable.Keys)
                {
                    _Caps[cap] = respTable[cap].AsUri();
                }

                if (_Caps.ContainsKey("EventQueueGet"))
                {
                    Logger.DebugLog("Starting event queue for " + Simulator.ToString(), Simulator.Client);

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
