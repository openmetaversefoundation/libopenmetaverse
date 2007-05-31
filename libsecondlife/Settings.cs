/*
 * Copyright (c) 2006-2007, Second Life Reverse Engineering Team
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
using libsecondlife.Packets;

namespace libsecondlife
{
    /// <summary>
    /// Class for controlling various system settings.
    /// </summary>
    /// <remarks>Some values are readonly because they affect things that
    /// happen when the SecondLife object is initialized, so changing them at 
    /// runtime won't do any good. Non-readonly values may affect things that 
    /// happen at login or dynamically</remarks>
    public class Settings
    {
        /// <summary>The version of libsecondlife (not the SL protocol itself)</summary>
        public string VERSION = "libsecondlife 0.0.9";
        /// <summary>XML-RPC login server to connect to</summary>
        public string LOGIN_SERVER = "https://login.agni.lindenlab.com/cgi-bin/login.cgi";

	/// Timeouts:
        /// <summary>Number of milliseconds before a teleport attempt will time
        /// out</summary>
        public int TELEPORT_TIMEOUT = 40 * 1000;
        /// <summary>Number of milliseconds before NetworkManager.Logout() will
        /// time out</summary>
        public int LOGOUT_TIMEOUT = 5 * 1000;
        /// <summary>Number of milliseconds before a CAPS call will time out 
        /// and try again</summary>
        /// <remarks>Setting this too low will cause web requests to repeatedly
        /// time out and retry</remarks>
        public int CAPS_TIMEOUT = 60 * 1000;
        /// <summary>Number of milliseconds for xml-rpc to timeout</summary>
        public int LOGIN_TIMEOUT = 60 * 1000;
        /// <summary>Milliseconds before a packet is assumed lost and resent</summary>
        public int RESEND_TIMEOUT = 4000;
        /// <summary>Milliseconds without receiving a packet before the 
        /// connection to a simulator is assumed lost</summary>
        public int SIMULATOR_TIMEOUT = 15 * 1000;
        /// <summary>Milliseconds to wait for a simulator info request through
        /// the grid interface</summary>
        public int MAP_REQUEST_TIMEOUT = 5 * 1000;

	/// Sizes

        /// <summary>The initial size of the packet inbox, where packets are
        /// stored before processing</summary>
        public const int PACKET_INBOX_SIZE = 100;
        /// <summary>Maximum size of packet that we want to send over the wire</summary>
        public const int MAX_PACKET_SIZE = 1200;
        /// <summary>Millisecond interval between ticks, where all ACKs are 
        /// sent out and the age of unACKed packets is checked</summary>
        public const int NETWORK_TICK_LENGTH = 500;
        /// <summary>The maximum value of a packet sequence number before it
        /// rolls over back to one</summary>
        public const int MAX_SEQUENCE = 0xFFFFFF;
        /// <summary>The maximum size of the sequence number archive, used to
        /// check for resent and/or duplicate packets</summary>
        public const int PACKET_ARCHIVE_SIZE = 50;
        /// <summary>Number of milliseconds between sending pings to each sim</summary>
        public const int PING_INTERVAL = 2200;
        /// <summary>Number of milliseconds between sending camera updates</summary>
        public const int AGENT_UPDATE_INTERVAL = 500;
        /// <summary>Number of milliseconds between updating the current
        /// positions of moving, non-accelerating and non-colliding objects</summary>
        public const int INTERPOLATION_UPDATE = 250;
        /// <summary>Maximum number of queued ACKs to be sent before SendAcks()
        /// is forced</summary>
        public int MAX_PENDING_ACKS = 10;
        /// <summary>Maximum number of ACKs to append to a packet</summary>
        public int MAX_APPENDED_ACKS = 10;
        /// <summary>Network stats queue length (seconds)</summary>
        public int STATS_QUEUE_SIZE = 5;

	/// Configuration options (mostly booleans)
        /// <summary>Whether or not to process packet callbacks async. This is
        /// better off being true, but the option exists to set it to false and
        /// use the old behavior. Please fix your packet callback to return to
        /// the pump rather than just setting this back to false, if you can</summary>
        /// <remarks>This is an experimental feature and is not completely
        /// reliable yet</remarks>
        public bool SYNC_PACKETCALLBACKS = false;
        /// <summary>Enable/disable debugging log messages</summary>
        public bool DEBUG = true;
        /// <summary>Attach avatar names to log messages</summary>
        public bool LOG_NAMES = true;
	/// <summary>Log packet retransmission info</summary>
	public bool LOG_RESENDS = true;
        /// <summary>Enable/disable storing terrain heightmaps in the 
        /// TerrainManager</summary>
        public bool STORE_LAND_PATCHES = false;
        /// <summary>Enable/disable sending periodic camera updates</summary>
        public bool SEND_AGENT_UPDATES = true;
        /// <summary>Enable/disable libsecondlife automatically setting the
        /// bandwidth throttle after connecting to each simulator</summary>
        /// <remarks>The default libsecondlife throttle uses the equivalent of
        /// the maximum bandwidth setting in the official client. If you do not
        /// set a throttle your connection will by default be throttled well
        /// below the minimum values and you may experience connection problems</remarks>
        public bool SEND_AGENT_THROTTLE = true;
        /// <summary></summary>
        public bool OUTBOUND_THROTTLE = false;
        /// <summary>Maximum outgoing bytes/sec, per sim</summary>
        public int OUTBOUND_THROTTLE_RATE = 1500;
        /// <summary>Enable/disable the sending of pings to monitor lag and 
        /// packet loss</summary>
        public bool SEND_PINGS = false;
        /// <summary>Should we connect to multiple sims? This will allow
        /// viewing in to neighboring simulators and sim crossings
        /// (Experimental)</summary>
        public bool MULTIPLE_SIMS = true;
        /// <summary>If true, all object update packets will be decoded in to
        /// native objects. If false, only updates for our own agent will be
        /// decoded. Registering an event handler will force objects for that
        /// type to always be decoded</summary>
        public bool ALWAYS_DECODE_OBJECTS = false;
        /// <summary>If true, when a cached object check is received from the
        /// server the full object info will automatically be requested</summary>
        public bool ALWAYS_REQUEST_OBJECTS = false;
        /// <summary>Whether to establish connections to HTTP capabilities
        /// servers for simulators</summary>
        public bool ENABLE_CAPS = true;
	/// <summary>Whether to decode sim stats</summary>
	public bool ENABLE_SIMSTATS = true;

        /// <summary>Cost of uploading an asset</summary>
        /// <remarks>Read-only since this value is dynamically fetched at login</remarks>
        public int UPLOAD_COST { get { return priceUpload; } }

        private SecondLife Client;
        private int priceUpload = 0;

        /// <summary>Constructor</summary>
        /// <param name="client">Client connection Object to use</param>
        public Settings(SecondLife client)
        {
            Client = client;

            Client.Network.RegisterCallback(Packets.PacketType.EconomyData, new NetworkManager.PacketCallback(EconomyDataHandler));
        }

        /// <summary>
        /// Presumably for outputting asset upload costs.
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="simulator"></param>
        private void EconomyDataHandler(Packet packet, Simulator simulator)
        {
            EconomyDataPacket econ = (EconomyDataPacket)packet;

            priceUpload = econ.Info.PriceUpload;
        }
    }
}
