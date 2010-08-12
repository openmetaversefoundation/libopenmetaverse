/*
 * Copyright (c) 2006-2008, openmetaverse.org
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
using OpenMetaverse.Packets;

namespace OpenMetaverse
{
    /// <summary>
    /// Class for controlling various system settings.
    /// </summary>
    /// <remarks>Some values are readonly because they affect things that
    /// happen when the GridClient object is initialized, so changing them at 
    /// runtime won't do any good. Non-readonly values may affect things that 
    /// happen at login or dynamically</remarks>
    public class Settings
    {
        #region Login/Networking Settings

        /// <summary>Main grid login server</summary>
        public const string AGNI_LOGIN_SERVER = "https://login.agni.lindenlab.com/cgi-bin/login.cgi";

        /// <summary>Beta grid login server</summary>
        public const string ADITI_LOGIN_SERVER = "https://login.aditi.lindenlab.com/cgi-bin/login.cgi";

        /// <summary>The relative directory where external resources are kept</summary>
        public static string RESOURCE_DIR = "openmetaverse_data";

        /// <summary>Login server to connect to</summary>
        public string LOGIN_SERVER = AGNI_LOGIN_SERVER;

        /// <summary>IP Address the client will bind to</summary>
        public static System.Net.IPAddress BIND_ADDR = System.Net.IPAddress.Any;

        /// <summary>Use XML-RPC Login or LLSD Login, default is XML-RPC Login</summary>
        public bool USE_LLSD_LOGIN = false;
        #endregion
        #region Inventory
        /// <summary>
        /// InventoryManager requests inventory information on login,
        /// GridClient initializes an Inventory store for main inventory.
        /// </summary>
        public const bool ENABLE_INVENTORY_STORE = true;
        /// <summary>
        /// InventoryManager requests library information on login,
        /// GridClient initializes an Inventory store for the library.
        /// </summary>
        public const bool ENABLE_LIBRARY_STORE = true;
        #endregion
        #region Timeouts and Intervals

        /// <summary>Number of milliseconds before an asset transfer will time
        /// out</summary>
        public int TRANSFER_TIMEOUT = 90 * 1000;

        /// <summary>Number of milliseconds before a teleport attempt will time
        /// out</summary>
        public int TELEPORT_TIMEOUT = 40 * 1000;

        /// <summary>Number of milliseconds before NetworkManager.Logout() will
        /// time out</summary>
        public int LOGOUT_TIMEOUT = 5 * 1000;

        /// <summary>Number of milliseconds before a CAPS call will time out</summary>
        /// <remarks>Setting this too low will cause web requests time out and
        /// possibly retry repeatedly</remarks>
        public int CAPS_TIMEOUT = 60 * 1000;

        /// <summary>Number of milliseconds for xml-rpc to timeout</summary>
        public int LOGIN_TIMEOUT = 60 * 1000;

        /// <summary>Milliseconds before a packet is assumed lost and resent</summary>
        public int RESEND_TIMEOUT = 4000;

        /// <summary>Milliseconds without receiving a packet before the 
        /// connection to a simulator is assumed lost</summary>
        public int SIMULATOR_TIMEOUT = 30 * 1000;

        /// <summary>Milliseconds to wait for a simulator info request through
        /// the grid interface</summary>
        public int MAP_REQUEST_TIMEOUT = 5 * 1000;

        /// <summary>Number of milliseconds between sending pings to each sim</summary>
        public const int PING_INTERVAL = 2200;

        /// <summary>Number of milliseconds between sending camera updates</summary>
        public const int DEFAULT_AGENT_UPDATE_INTERVAL = 500;

        /// <summary>Number of milliseconds between updating the current
        /// positions of moving, non-accelerating and non-colliding objects</summary>
        public const int INTERPOLATION_INTERVAL = 250;

        /// <summary>Millisecond interval between ticks, where all ACKs are 
        /// sent out and the age of unACKed packets is checked</summary>
        public const int NETWORK_TICK_INTERVAL = 500;

        #endregion
        #region Sizes

        /// <summary>The initial size of the packet inbox, where packets are
        /// stored before processing</summary>
        public const int PACKET_INBOX_SIZE = 100;
        /// <summary>Maximum size of packet that we want to send over the wire</summary>
        public const int MAX_PACKET_SIZE = 1200;
        /// <summary>The maximum value of a packet sequence number before it
        /// rolls over back to one</summary>
        public const int MAX_SEQUENCE = 0xFFFFFF;
        /// <summary>The maximum size of the sequence number archive, used to
        /// check for resent and/or duplicate packets</summary>
        public const int PACKET_ARCHIVE_SIZE = 200;
        /// <summary>Maximum number of queued ACKs to be sent before SendAcks()
        /// is forced</summary>
        public int MAX_PENDING_ACKS = 10;
        /// <summary>Network stats queue length (seconds)</summary>
        public int STATS_QUEUE_SIZE = 5;

        #endregion
        #region Configuration options (mostly booleans)

        /// <summary>Enable/disable storing terrain heightmaps in the 
        /// TerrainManager</summary>
        public bool STORE_LAND_PATCHES = false;

        /// <summary>Enable/disable sending periodic camera updates</summary>
        public bool SEND_AGENT_UPDATES = true;

        /// <summary>Enable/disable automatically setting agent appearance at
        /// login and after sim crossing</summary>
        public bool SEND_AGENT_APPEARANCE = true;

        /// <summary>Enable/disable automatically setting the bandwidth throttle
        /// after connecting to each simulator</summary>
        /// <remarks>The default throttle uses the equivalent of the maximum
        /// bandwidth setting in the official client. If you do not set a
        /// throttle your connection will by default be throttled well below
        /// the minimum values and you may experience connection problems</remarks>
        public bool SEND_AGENT_THROTTLE = true;

        /// <summary>Enable/disable the sending of pings to monitor lag and 
        /// packet loss</summary>
        public bool SEND_PINGS = true;

        /// <summary>Should we connect to multiple sims? This will allow
        /// viewing in to neighboring simulators and sim crossings
        /// (Experimental)</summary>
        public bool MULTIPLE_SIMS = true;

        /// <summary>If true, all object update packets will be decoded in to
        /// native objects. If false, only updates for our own agent will be
        /// decoded. Registering an event handler will force objects for that
        /// type to always be decoded. If this is disabled the object tracking
        /// will have missing or partial prim and avatar information</summary>
        public bool ALWAYS_DECODE_OBJECTS = true;

        /// <summary>If true, when a cached object check is received from the
        /// server the full object info will automatically be requested</summary>
        public bool ALWAYS_REQUEST_OBJECTS = true;

        /// <summary>Whether to establish connections to HTTP capabilities
        /// servers for simulators</summary>
        public bool ENABLE_CAPS = true;

        /// <summary>Whether to decode sim stats</summary>
        public bool ENABLE_SIMSTATS = true;

        /// <summary>The capabilities servers are currently designed to
        /// periodically return a 502 error which signals for the client to
        /// re-establish a connection. Set this to true to log those 502 errors</summary>
        public bool LOG_ALL_CAPS_ERRORS = false;

        /// <summary>If true, any reference received for a folder or item
        /// the library is not aware of will automatically be fetched</summary>
        public bool FETCH_MISSING_INVENTORY = true;

        /// <summary>If true, and <code>SEND_AGENT_UPDATES</code> is true,
        /// AgentUpdate packets will continuously be sent out to give the bot
        /// smoother movement and autopiloting</summary>
        public bool DISABLE_AGENT_UPDATE_DUPLICATE_CHECK = true;

        /// <summary>If true, currently visible avatars will be stored
        /// in dictionaries inside <code>Simulator.ObjectAvatars</code>.
        /// If false, a new Avatar or Primitive object will be created
        /// each time an object update packet is received</summary>
        public bool AVATAR_TRACKING = true;

        /// <summary>If true, currently visible avatars will be stored
        /// in dictionaries inside <code>Simulator.ObjectPrimitives</code>.
        /// If false, a new Avatar or Primitive object will be created
        /// each time an object update packet is received</summary>
        public bool OBJECT_TRACKING = true;

        /// <summary>If true, position and velocity will periodically be
        /// interpolated (extrapolated, technically) for objects and 
        /// avatars that are being tracked by the library. This is
        /// necessary to increase the accuracy of speed and position
        /// estimates for simulated objects</summary>
        public bool USE_INTERPOLATION_TIMER = true;

        /// <summary>
        /// If true, utilization statistics will be tracked. There is a minor penalty
        /// in CPU time for enabling this option.
        /// </summary>
        public bool TRACK_UTILIZATION = false;
        #endregion
        #region Parcel Tracking

        /// <summary>If true, parcel details will be stored in the 
        /// <code>Simulator.Parcels</code> dictionary as they are received</summary>
        public bool PARCEL_TRACKING = true;

        /// <summary>
        /// If true, an incoming parcel properties reply will automatically send
        /// a request for the parcel access list
        /// </summary>
        public bool ALWAYS_REQUEST_PARCEL_ACL = true;

        /// <summary>
        /// if true, an incoming parcel properties reply will automatically send 
        /// a request for the traffic count.
        /// </summary>
        public bool ALWAYS_REQUEST_PARCEL_DWELL = true;

        #endregion
        #region Asset Cache

        /// <summary>
        /// If true, images, and other assets downloaded from the server 
        /// will be cached in a local directory
        /// </summary>
        public bool USE_ASSET_CACHE = true;

        /// <summary>Path to store cached texture data</summary>
        public string ASSET_CACHE_DIR = RESOURCE_DIR + "/cache";

        /// <summary>Maximum size cached files are allowed to take on disk (bytes)</summary>
        public long ASSET_CACHE_MAX_SIZE = 1024 * 1024 * 1024; // 1GB

        #endregion
        #region Misc

        /// <summary>Default color used for viewer particle effects</summary>
        public Color4 DEFAULT_EFFECT_COLOR = new Color4(255, 0, 0, 255);

        /// <summary>Cost of uploading an asset</summary>
        /// <remarks>Read-only since this value is dynamically fetched at login</remarks>
        public int UPLOAD_COST { get { return priceUpload; } }

        /// <summary>Maximum number of times to resend a failed packet</summary>
        public int MAX_RESEND_COUNT = 3;

        /// <summary>Throttle outgoing packet rate</summary>
        public bool THROTTLE_OUTGOING_PACKETS = true;

        /// <summary>UUID of a texture used by some viewers to indentify type of client used</summary>
        public UUID CLIENT_IDENTIFICATION_TAG = UUID.Zero;

        #endregion
        #region Texture Pipeline

        /// <summary>The maximum number of concurrent texture downloads allowed</summary>
        /// <remarks>Increasing this number will not necessarily increase texture retrieval times due to
        /// simulator throttles</remarks>
        public int MAX_CONCURRENT_TEXTURE_DOWNLOADS = 4;

        /// <summary>
        /// The Refresh timer inteval is used to set the delay between checks for stalled texture downloads
        /// </summary>
        /// <remarks>This is a static variable which applies to all instances</remarks>
        public static float PIPELINE_REFRESH_INTERVAL = 500.0f;

        /// <summary>
        /// Textures taking longer than this value will be flagged as timed out and removed from the pipeline
        /// </summary>
        public int PIPELINE_REQUEST_TIMEOUT = 45*1000;
        #endregion

        #region Logging Configuration

        /// <summary>
        /// Get or set the minimum log level to output to the console by default
        /// 
        /// If the library is not compiled with DEBUG defined and this level is set to DEBUG
        /// You will get no output on the console. This behavior can be overriden by creating
        /// a logger configuration file for log4net
        /// </summary>
        public static Helpers.LogLevel LOG_LEVEL = Helpers.LogLevel.Debug;

        /// <summary>Attach avatar names to log messages</summary>
        public bool LOG_NAMES = true;

        /// <summary>Log packet retransmission info</summary>
        public bool LOG_RESENDS = true;

        #endregion
        #region Private Fields

        private GridClient Client;
        private int priceUpload = 0;

        /// <summary>Constructor</summary>
        /// <param name="client">Reference to a GridClient object</param>
        public Settings(GridClient client)
        {
            Client = client;
            Client.Network.RegisterCallback(Packets.PacketType.EconomyData, EconomyDataHandler);
        }

        #endregion
        #region Packet Callbacks

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected void EconomyDataHandler(object sender, PacketReceivedEventArgs e)
        {
            EconomyDataPacket econ = (EconomyDataPacket)e.Packet;

            priceUpload = econ.Info.PriceUpload;
        }

        #endregion
    }
}
