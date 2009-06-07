/*
 * Copyright (c) 2007-2009, openmetaverse.org
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
using System.Net;
using System.Text;
using System.Threading;
using OpenMetaverse.Packets;
using OpenMetaverse.StructuredData;
using OpenMetaverse.Interfaces;
using OpenMetaverse.Http;

namespace OpenMetaverse
{
    /// <summary>
    /// Capabilities is the name of the bi-directional HTTP REST protocol
    /// used to communicate non real-time transactions such as teleporting or
    /// group messaging
    /// </summary>
    public partial class Caps
    {
        /// <summary>
        /// Triggered when an event is received via the EventQueueGet 
        /// capability
        /// </summary>
        /// <param name="capsKey">Event name</param>
        /// <param name="message">Decoded event data</param>
        /// <param name="simulator">The simulator that generated the event</param>
        //public delegate void EventQueueCallback(string message, StructuredData.OSD body, Simulator simulator);

        public delegate void EventQueueCallback(string capsKey, IMessage message, Simulator simulator);

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
            Simulator.Log.Log(String.Format("Caps system for {0} is {1}", Simulator,
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
            if (Simulator == null || !Simulator.Network.Connected)
                return;

            // Create a request list
            OSDArray req = new OSDArray();
            // This list can be updated by using the following command to obtain a current list of capabilities the official linden viewer supports:
            // wget -q -O - http://svn.secondlife.com/svn/linden/trunk/indra/newview/llviewerregion.cpp | grep 'capabilityNames.append'  | sed 's/^[ \t]*//;s/capabilityNames.append("/req.Add("/'
            req.Add("ChatSessionRequest");
            req.Add("CopyInventoryFromNotecard");
            req.Add("DispatchRegionInfo");
            req.Add("EstateChangeInfo");
            req.Add("EventQueueGet");
            req.Add("FetchInventory");
            req.Add("WebFetchInventoryDescendents");
            req.Add("FetchLib");
            req.Add("FetchLibDescendents");
            req.Add("GroupProposalBallot");
            req.Add("HomeLocation");
            req.Add("MapLayer");
            req.Add("MapLayerGod");
            req.Add("NewFileAgentInventory");
            req.Add("ParcelPropertiesUpdate");
            req.Add("ParcelVoiceInfoRequest");
            req.Add("ProductInfoRequest");
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
            req.Add("UntrustedSimulatorMessage");
            req.Add("UpdateAgentInformation");
            req.Add("UpdateAgentLanguage");
            req.Add("UpdateGestureAgentInventory");
            req.Add("UpdateNotecardAgentInventory");
            req.Add("UpdateScriptAgent");
            req.Add("UpdateGestureTaskInventory");
            req.Add("UpdateNotecardTaskInventory");
            req.Add("UpdateScriptTask");
            req.Add("UploadBakedTexture");
            req.Add("ViewerStartAuction");
            req.Add("ViewerStats");


            _SeedRequest = new CapsClient(new Uri(_SeedCapsURI));
            _SeedRequest.OnComplete += new CapsClient.CompleteCallback(SeedRequestCompleteHandler);
            _SeedRequest.BeginGetResponse(req, OSDFormat.Xml, Settings.CAPS_TIMEOUT);
        }

        private void SeedRequestCompleteHandler(CapsClient client, OSD result, Exception error)
        {
            if (result != null && result.Type == OSDType.Map)
            {
                OSDMap respTable = (OSDMap)result;

                foreach (string cap in respTable.Keys)
                {
                    _Caps[cap] = respTable[cap].AsUri();
                }

                if (_Caps.ContainsKey("EventQueueGet"))
                {
                    Simulator.Log.DebugLog("Starting event queue for " + Simulator.ToString());

                    _EventQueueCap = new EventQueueClient(_Caps["EventQueueGet"]);
                    _EventQueueCap.OnConnected += EventQueueConnectedHandler;
                    _EventQueueCap.OnEvent += EventQueueEventHandler;
                    _EventQueueCap.Start();
                }
            }
            else if (
                error != null &&
                error is WebException &&
                ((WebException)error).Response != null &&
                ((HttpWebResponse)((WebException)error).Response).StatusCode == HttpStatusCode.NotFound)
            {
                // 404 error
                Logger.Log("Seed capability returned a 404, capability system is aborting", Helpers.LogLevel.Error);
            }
            else
            {
                // The initial CAPS connection failed, try again
                MakeSeedRequest();
            }
        }

        private void EventQueueConnectedHandler()
        {
            Simulator.Network.RaiseConnectedEvent(Simulator);
        }

        /// <summary>
        /// Process any incoming events, check to see if we have a message created for the event, 
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="body"></param>
        private void EventQueueEventHandler(string eventName, OSDMap body)
        {
            IMessage message = Messages.MessageUtils.DecodeEvent(eventName, body);
            if (message != null)
            {
                if (Settings.SYNC_PACKETCALLBACKS)
                    Simulator.Network.CapsEvents.RaiseEvent(eventName, message, Simulator);
                else
                    Simulator.Network.CapsEvents.BeginRaiseEvent(eventName, message, Simulator);
            }
            else
            {
                Logger.Log("No Message handler exists for event " + eventName + ". Unable to decode. Will try Generic Handler next", Helpers.LogLevel.Warning);
                Logger.Log("Please report this information to http://jira.openmv.org/: \n" + body, Helpers.LogLevel.Debug);

                // try generic decoder next which takes a caps event and tries to match it to an existing packet
                if (body.Type == OSDType.Map)
                {
                    OSDMap map = (OSDMap)body;
                    Packet packet = Packet.BuildPacket(eventName, map);
                    if (packet != null)
                    {
                        NetworkManager.IncomingPacket incomingPacket;
                        incomingPacket.Simulator = Simulator;
                        incomingPacket.Packet = packet;

                        Simulator.Log.DebugLog("Serializing " + packet.Type.ToString() + " capability with generic handler");

                        Simulator.Network.PacketInbox.Enqueue(incomingPacket);
                    }
                    else
                    {
                        Logger.Log("No Packet or Message handler exists for " + eventName, Helpers.LogLevel.Warning);
                    }
                }
            }
        }
    }
}
