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
//#define DEBUG_VOICE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

using OpenMetaverse;
using OpenMetaverse.StructuredData;

namespace OpenMetaverse.Voice
{
    public partial class VoiceGateway : IDisposable
    {
        // These states should be in increasing order of 'completeness'
        // so that the (int) values can drive a progress bar.
        public enum ConnectionState
        {
            None = 0,
            Provisioned,
            DaemonStarted,
            DaemonConnected,
            ConnectorConnected,
            AccountLogin,
            RegionCapAvailable,
            SessionRunning
        }

        internal string sipServer = "";
        private string acctServer = "https://www.bhr.vivox.com/api2/";
        private string connectionHandle;
        private string accountHandle;
        private string sessionHandle;

        // Parameters to Vivox daemon
        private string slvoicePath = "";
        private string slvoiceArgs = "-ll 5";
        private string daemonNode = "127.0.0.1";
        private int daemonPort = 37331;

        private string voiceUser;
        private string voicePassword;
        private string spatialUri;
        private string spatialCredentials;

        // Session management
        private Dictionary<string, VoiceSession> sessions;
        private VoiceSession spatialSession;
        private Uri currentParcelCap;
        private Uri nextParcelCap;
        private string regionName;

        // Position update thread
        private Thread posThread;
        private ManualResetEvent posRestart;
        public GridClient Client;
        private VoicePosition position;
        private Vector3d oldPosition;
        private Vector3d oldAt;

        // Audio interfaces
        private List<string> inputDevices;
        /// <summary>
        /// List of audio input devices
        /// </summary>
        public List<string> CaptureDevices { get { return inputDevices; } }
        private List<string> outputDevices;
        /// <summary>
        /// List of audio output devices
        /// </summary>
        public List<string> PlaybackDevices { get { return outputDevices; } }
        private string currentCaptureDevice;
        private string currentPlaybackDevice;
        private bool testing = false;

        public event EventHandler OnSessionCreate;
        public event EventHandler OnSessionRemove;
        public delegate void VoiceConnectionChangeCallback(ConnectionState state);
        public event VoiceConnectionChangeCallback OnVoiceConnectionChange;
        public delegate void VoiceMicTestCallback(float level);
        public event VoiceMicTestCallback OnVoiceMicTest;

        public VoiceGateway(GridClient c)
        {
            Client = c;

            sessions = new Dictionary<string, VoiceSession>();
            position = new VoicePosition();
            position.UpOrientation = new Vector3d(0.0, 1.0, 0.0);
            position.Velocity = new Vector3d(0.0, 0.0, 0.0);
            oldPosition = new Vector3d(0, 0, 0);
            oldAt = new Vector3d(1, 0, 0);

            slvoiceArgs = " -ll -1";    // Min logging
            slvoiceArgs += " -i 127.0.0.1:" + daemonPort.ToString();
            //            slvoiceArgs += " -lf " + control.instance.ClientDir;
        }

        /// <summary>
        /// Start up the Voice service.
        /// </summary>
        public void Start()
        {
            // Start the background thread
            if (posThread != null && posThread.IsAlive)
                posThread.Abort();
            posThread = new Thread(new ThreadStart(PositionThreadBody));
            posThread.Name = "VoicePositionUpdate";
            posThread.IsBackground = true;
            posRestart = new ManualResetEvent(false);
            posThread.Start();

            Client.Network.EventQueueRunning += new EventHandler<EventQueueRunningEventArgs>(Network_EventQueueRunning);

            // Connection events
            OnDaemonRunning +=
                 new VoiceGateway.DaemonRunningCallback(connector_OnDaemonRunning);
            OnDaemonCouldntRun +=
                new VoiceGateway.DaemonCouldntRunCallback(connector_OnDaemonCouldntRun);
            OnConnectorCreateResponse +=
                new EventHandler<VoiceGateway.VoiceConnectorEventArgs>(connector_OnConnectorCreateResponse);
            OnDaemonConnected +=
                new DaemonConnectedCallback(connector_OnDaemonConnected);
            OnDaemonCouldntConnect +=
                new DaemonCouldntConnectCallback(connector_OnDaemonCouldntConnect);
            OnAuxAudioPropertiesEvent +=
                new EventHandler<AudioPropertiesEventArgs>(connector_OnAuxAudioPropertiesEvent);

            // Session events
            OnSessionStateChangeEvent +=
                new EventHandler<SessionStateChangeEventArgs>(connector_OnSessionStateChangeEvent);
            OnSessionAddedEvent +=
                new EventHandler<SessionAddedEventArgs>(connector_OnSessionAddedEvent);

            // Session Participants events
            OnSessionParticipantUpdatedEvent +=
                new EventHandler<ParticipantUpdatedEventArgs>(connector_OnSessionParticipantUpdatedEvent);
            OnSessionParticipantAddedEvent +=
                new EventHandler<ParticipantAddedEventArgs>(connector_OnSessionParticipantAddedEvent);

            // Device events
            OnAuxGetCaptureDevicesResponse +=
                new EventHandler<VoiceDevicesEventArgs>(connector_OnAuxGetCaptureDevicesResponse);
            OnAuxGetRenderDevicesResponse +=
                new EventHandler<VoiceDevicesEventArgs>(connector_OnAuxGetRenderDevicesResponse);

            // Generic status response
            OnVoiceResponse += new EventHandler<VoiceResponseEventArgs>(connector_OnVoiceResponse);

            // Account events
            OnAccountLoginResponse +=
                new EventHandler<VoiceAccountEventArgs>(connector_OnAccountLoginResponse);

            Logger.Log("Voice initialized", Helpers.LogLevel.Info);

            // If voice provisioning capability is already available,
            // proceed with voice startup.   Otherwise the EventQueueRunning
            // event will do it.
            System.Uri vCap =
                 Client.Network.CurrentSim.Caps.CapabilityURI("ProvisionVoiceAccountRequest");
            if (vCap != null)
                RequestVoiceProvision(vCap);

        }

        /// <summary>
        /// Handle miscellaneous request status
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// ///<remarks>If something goes wrong, we log it.</remarks>
        void connector_OnVoiceResponse(object sender, VoiceGateway.VoiceResponseEventArgs e)
        {
            if (e.StatusCode == 0)
                return;

            Logger.Log(e.Message + " on " + sender as string, Helpers.LogLevel.Error);
        }

        public void Stop()
        {
            Client.Network.EventQueueRunning -= new EventHandler<EventQueueRunningEventArgs>(Network_EventQueueRunning);

            // Connection events
            OnDaemonRunning -=
                     new VoiceGateway.DaemonRunningCallback(connector_OnDaemonRunning);
            OnDaemonCouldntRun -=
                    new VoiceGateway.DaemonCouldntRunCallback(connector_OnDaemonCouldntRun);
            OnConnectorCreateResponse -=
                new EventHandler<VoiceGateway.VoiceConnectorEventArgs>(connector_OnConnectorCreateResponse);
            OnDaemonConnected -=
                    new VoiceGateway.DaemonConnectedCallback(connector_OnDaemonConnected);
            OnDaemonCouldntConnect -=
                    new VoiceGateway.DaemonCouldntConnectCallback(connector_OnDaemonCouldntConnect);
            OnAuxAudioPropertiesEvent -=
                    new EventHandler<AudioPropertiesEventArgs>(connector_OnAuxAudioPropertiesEvent);

            // Session events
            OnSessionStateChangeEvent -=
                    new EventHandler<SessionStateChangeEventArgs>(connector_OnSessionStateChangeEvent);
            OnSessionAddedEvent -=
                    new EventHandler<SessionAddedEventArgs>(connector_OnSessionAddedEvent);

            // Session Participants events
            OnSessionParticipantUpdatedEvent -=
                    new EventHandler<ParticipantUpdatedEventArgs>(connector_OnSessionParticipantUpdatedEvent);
            OnSessionParticipantAddedEvent -=
                    new EventHandler<ParticipantAddedEventArgs>(connector_OnSessionParticipantAddedEvent);
            OnSessionParticipantRemovedEvent -=
                    new EventHandler<ParticipantRemovedEventArgs>(connector_OnSessionParticipantRemovedEvent);

            // Tuning events
            OnAuxGetCaptureDevicesResponse -=
                    new EventHandler<VoiceGateway.VoiceDevicesEventArgs>(connector_OnAuxGetCaptureDevicesResponse);
            OnAuxGetRenderDevicesResponse -=
                    new EventHandler<VoiceGateway.VoiceDevicesEventArgs>(connector_OnAuxGetRenderDevicesResponse);

            // Account events
            OnAccountLoginResponse -=
                    new EventHandler<VoiceGateway.VoiceAccountEventArgs>(connector_OnAccountLoginResponse);

            // Stop the background thread
            if (posThread != null)
            {
                PosUpdating(false);

                if (posThread.IsAlive)
                    posThread.Abort();
                posThread = null;
            }

            // Close all sessions
            foreach (VoiceSession s in sessions.Values)
            {
                s.Close();
            }

            // Clear out lots of state so in case of restart we begin at the beginning.
            currentParcelCap = null;
            sessions.Clear();
            accountHandle = null;
            voiceUser = null;
            voicePassword = null;

            SessionTerminate(sessionHandle);
            sessionHandle = null;
            AccountLogout(accountHandle);
            accountHandle = null;
            ConnectorInitiateShutdown(connectionHandle);
            connectionHandle = null;
            StopDaemon();
        }

        /// <summary>
        /// Cleanup oject resources
        /// </summary>
        public void Dispose()
        {
            Stop();
        }

        internal string GetVoiceDaemonPath()
        {
            string myDir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            if (Environment.OSVersion.Platform != PlatformID.MacOSX &&
                Environment.OSVersion.Platform != PlatformID.Unix)
            {
                string localDaemon = Path.Combine(myDir, Path.Combine("voice", "SLVoice.exe"));
                
                if (File.Exists(localDaemon))
                    return localDaemon;

                string progFiles;
                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ProgramFiles(x86)")))
                {
                    progFiles = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
                }
                else
                {
                    progFiles = Environment.GetEnvironmentVariable("ProgramFiles");
                }

                if (System.IO.File.Exists(Path.Combine(progFiles, @"SecondLife" + Path.DirectorySeparatorChar + @"SLVoice.exe")))
                {
                    return Path.Combine(progFiles, @"SecondLife" + Path.DirectorySeparatorChar + @"SLVoice.exe");
                }

                return Path.Combine(myDir, @"SLVoice.exe");

            }
            else
            {
                string localDaemon = Path.Combine(myDir, Path.Combine("voice", "SLVoice"));

                if (File.Exists(localDaemon))
                    return localDaemon;

                return Path.Combine(myDir,"SLVoice");
            }
        }

        void RequestVoiceProvision(System.Uri cap)
        {
            OpenMetaverse.Http.CapsClient capClient =
                new OpenMetaverse.Http.CapsClient(cap);
            capClient.OnComplete +=
                new OpenMetaverse.Http.CapsClient.CompleteCallback(cClient_OnComplete);
            OSD postData = new OSD();

            // STEP 0
            Logger.Log("Requesting voice capability", Helpers.LogLevel.Info);
            capClient.BeginGetResponse(postData, OSDFormat.Xml, 10000);
        }

        /// <summary>
        /// Request voice cap when changing regions
        /// </summary>
        void Network_EventQueueRunning(object sender, EventQueueRunningEventArgs e)
        {
            // We only care about the sim we are in.
            if (e.Simulator != Client.Network.CurrentSim)
                return;

            // Did we provision voice login info?
            if (string.IsNullOrEmpty(voiceUser))
            {
                // The startup steps are
                //  0. Get voice account info
                //  1. Start Daemon
                //  2. Create TCP connection
                //  3. Create Connector
                //  4. Account login
                //  5. Create session

                // Get the voice provisioning data
                System.Uri vCap =
                    Client.Network.CurrentSim.Caps.CapabilityURI("ProvisionVoiceAccountRequest");

                // Do we have voice capability?
                if (vCap == null)
                {
                    Logger.Log("Null voice capability after event queue running", Helpers.LogLevel.Warning);
                }
                else
                {
                    RequestVoiceProvision(vCap);
                }

                return;
            }
            else
            {
                // Change voice session for this region.
                ParcelChanged();
            }
        }


        #region Participants

        void connector_OnSessionParticipantUpdatedEvent(object sender, ParticipantUpdatedEventArgs e)
        {
            VoiceSession s = FindSession(e.SessionHandle, false);
            if (s == null) return;
            s.ParticipantUpdate(e.URI, e.IsMuted, e.IsSpeaking, e.Volume, e.Energy);
        }

        public string SIPFromUUID(UUID id)
        {
            return "sip:" +
                nameFromID(id) +
                "@" +
                sipServer;
        }

        private static string nameFromID(UUID id)
        {
            string result = null;

            if (id == UUID.Zero)
                return result;

            // Prepending this apparently prevents conflicts with reserved names inside the vivox and diamondware code.
            result = "x";

            // Base64 encode and replace the pieces of base64 that are less compatible 
            // with e-mail local-parts.
            // See RFC-4648 "Base 64 Encoding with URL and Filename Safe Alphabet"
            byte[] encbuff = id.GetBytes();
            result += Convert.ToBase64String(encbuff);
            result = result.Replace('+', '-');
            result = result.Replace('/', '_');

            return result;
        }

        void connector_OnSessionParticipantAddedEvent(object sender, ParticipantAddedEventArgs e)
        {
            VoiceSession s = FindSession(e.SessionHandle, false);
            if (s == null)
            {
                Logger.Log("Orphan participant", Helpers.LogLevel.Error);
                return;
            }
            s.AddParticipant(e.URI);
        }

        void connector_OnSessionParticipantRemovedEvent(object sender, ParticipantRemovedEventArgs e)
        {
            VoiceSession s = FindSession(e.SessionHandle, false);
            if (s == null) return;
            s.RemoveParticipant(e.URI);
        }
        #endregion

        #region Sessions
        void connector_OnSessionAddedEvent(object sender, SessionAddedEventArgs e)
        {
            sessionHandle = e.SessionHandle;

            // Create our session context.
            VoiceSession s = FindSession(sessionHandle, true);
            s.RegionName = regionName;

            spatialSession = s;

            // Tell any user-facing code.
            if (OnSessionCreate != null)
                OnSessionCreate(s, null);

            Logger.Log("Added voice session in " + regionName, Helpers.LogLevel.Info);
        }

        /// <summary>
        /// Handle a change in session state
        /// </summary>
        void connector_OnSessionStateChangeEvent(object sender, SessionStateChangeEventArgs e)
        {
            VoiceSession s;

            switch (e.State)
            {
                case VoiceGateway.SessionState.Connected:
                    s = FindSession(e.SessionHandle, true);
                    sessionHandle = e.SessionHandle;
                    s.RegionName = regionName;
                    spatialSession = s;

                    Logger.Log("Voice connected in " + regionName, Helpers.LogLevel.Info);
                    // Tell any user-facing code.
                    if (OnSessionCreate != null)
                        OnSessionCreate(s, null);
                    break;

                case VoiceGateway.SessionState.Disconnected:
                    s = FindSession(sessionHandle, false);
                    sessions.Remove(sessionHandle);

                    if (s != null)
                    {
                        Logger.Log("Voice disconnected in " + s.RegionName, Helpers.LogLevel.Info);

                        // Inform interested parties
                        if (OnSessionRemove != null)
                            OnSessionRemove(s, null);

                        if (s == spatialSession)
                            spatialSession = null;
                    }

                    // The previous session is now ended.  Check for a new one and
                    // start it going.
                    if (nextParcelCap != null)
                    {
                        currentParcelCap = nextParcelCap;
                        nextParcelCap = null;
                        RequestParcelInfo(currentParcelCap);
                    }
                    break;
            }


        }

        /// <summary>
        /// Close a voice session
        /// </summary>
        /// <param name="sessionHandle"></param>
        internal void CloseSession(string sessionHandle)
        {
            if (!sessions.ContainsKey(sessionHandle))
                return;

            PosUpdating(false);
            ReportConnectionState(ConnectionState.AccountLogin);

            // Clean up spatial pointers.
            VoiceSession s = sessions[sessionHandle];
            if (s.IsSpatial)
            {
                spatialSession = null;
                currentParcelCap = null;
            }

            // Remove this session from the master session list
            sessions.Remove(sessionHandle);

            // Let any user-facing code clean up.
            if (OnSessionRemove != null)
                OnSessionRemove(s, null);

            // Tell SLVoice to clean it up as well.
            SessionTerminate(sessionHandle);
        }

        /// <summary>
        /// Locate a Session context from its handle
        /// </summary>
        /// <remarks>Creates the session context if it does not exist.</remarks>
        VoiceSession FindSession(string sessionHandle, bool make)
        {
            if (sessions.ContainsKey(sessionHandle))
                return sessions[sessionHandle];

            if (!make) return null;

            // Create a new session and add it to the sessions list.
            VoiceSession s = new VoiceSession(this, sessionHandle);

            // Turn on position updating for spatial sessions
            // (For now, only spatial sessions are supported)
            if (s.IsSpatial)
                PosUpdating(true);

            // Register the session by its handle
            sessions.Add(sessionHandle, s);
            return s;
        }

        #endregion

        #region MinorResponses

        void connector_OnAuxAudioPropertiesEvent(object sender, AudioPropertiesEventArgs e)
        {
            if (OnVoiceMicTest != null)
                OnVoiceMicTest(e.MicEnergy);
        }

        #endregion

        private void ReportConnectionState(ConnectionState s)
        {
            if (OnVoiceConnectionChange == null) return;

            OnVoiceConnectionChange(s);
        }

        /// <summary>
        /// Handle completion of main voice cap request.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="result"></param>
        /// <param name="error"></param>
        void cClient_OnComplete(OpenMetaverse.Http.CapsClient client,
            OpenMetaverse.StructuredData.OSD result,
            Exception error)
        {
            if (error != null)
            {
                Logger.Log("Voice cap error " + error.Message, Helpers.LogLevel.Error);
                return;
            }

            Logger.Log("Voice provisioned", Helpers.LogLevel.Info);
            ReportConnectionState(ConnectionState.Provisioned);

            OpenMetaverse.StructuredData.OSDMap pMap = result as OpenMetaverse.StructuredData.OSDMap;

            // We can get back 4 interesting values:
            //      voice_sip_uri_hostname
            //      voice_account_server_name   (actually a full URI)
            //      username
            //      password
            if (pMap.ContainsKey("voice_sip_uri_hostname"))
                sipServer = pMap["voice_sip_uri_hostname"].AsString();
            if (pMap.ContainsKey("voice_account_server_name"))
                acctServer = pMap["voice_account_server_name"].AsString();
            voiceUser = pMap["username"].AsString();
            voicePassword = pMap["password"].AsString();

            // Start the SLVoice daemon
            slvoicePath = GetVoiceDaemonPath();

            // Test if the executable exists
            if (!System.IO.File.Exists(slvoicePath))
            {
                Logger.Log("SLVoice is missing", Helpers.LogLevel.Error);
                return;
            }

            // STEP 1
            StartDaemon(slvoicePath, slvoiceArgs);
        }

        #region Daemon
        void connector_OnDaemonCouldntConnect()
        {
            Logger.Log("No voice daemon connect", Helpers.LogLevel.Error);
        }

        void connector_OnDaemonCouldntRun()
        {
            Logger.Log("Daemon not started", Helpers.LogLevel.Error);
        }

        /// <summary>
        /// Daemon has started so connect to it.
        /// </summary>
        void connector_OnDaemonRunning()
        {
            OnDaemonRunning -=
                new VoiceGateway.DaemonRunningCallback(connector_OnDaemonRunning);

            Logger.Log("Daemon started", Helpers.LogLevel.Info);
            ReportConnectionState(ConnectionState.DaemonStarted);

            // STEP 2
            ConnectToDaemon(daemonNode, daemonPort);

        }

        /// <summary>
        /// The daemon TCP connection is open.
        /// </summary>
        void connector_OnDaemonConnected()
        {
            Logger.Log("Daemon connected", Helpers.LogLevel.Info);
            ReportConnectionState(ConnectionState.DaemonConnected);

            // The connector is what does the logging.
            VoiceGateway.VoiceLoggingSettings vLog =
                new VoiceGateway.VoiceLoggingSettings();
            
#if DEBUG_VOICE
            vLog.Enabled = true;
            vLog.FileNamePrefix = "OpenmetaverseVoice";
            vLog.FileNameSuffix = ".log";
            vLog.LogLevel = 4;
#endif
            // STEP 3
            int reqId = ConnectorCreate(
                "V2 SDK",       // Magic value keeps SLVoice happy
                acctServer,     // Account manager server
                30000, 30099,   // port range
                vLog);
            if (reqId < 0)
            {
                Logger.Log("No voice connector request", Helpers.LogLevel.Error);
            }
        }

        /// <summary>
        /// Handle creation of the Connector.
        /// </summary>
        void connector_OnConnectorCreateResponse(
            object sender,
            VoiceGateway.VoiceConnectorEventArgs e)
        {
            Logger.Log("Voice daemon protocol started " + e.Message, Helpers.LogLevel.Info);

            connectionHandle = e.Handle;

            if (e.StatusCode != 0)
                return;

            // STEP 4
            AccountLogin(
                connectionHandle,
                voiceUser,
                voicePassword,
                "VerifyAnswer",   // This can also be "AutoAnswer"
                "",             // Default account management server URI
                10,            // Throttle state changes
                true);          // Enable buddies and presence
        }
        #endregion

        void connector_OnAccountLoginResponse(
            object sender,
            VoiceGateway.VoiceAccountEventArgs e)
        {
            Logger.Log("Account Login " + e.Message, Helpers.LogLevel.Info);
            accountHandle = e.AccountHandle;
            ReportConnectionState(ConnectionState.AccountLogin);
            ParcelChanged();
        }

        #region Audio devices
        /// <summary>
        /// Handle response to audio output device query
        /// </summary>
        void connector_OnAuxGetRenderDevicesResponse(
            object sender,
            VoiceGateway.VoiceDevicesEventArgs e)
        {
            outputDevices = e.Devices;
            currentPlaybackDevice = e.CurrentDevice;
        }

        /// <summary>
        /// Handle response to audio input device query
        /// </summary>
        void connector_OnAuxGetCaptureDevicesResponse(
            object sender,
            VoiceGateway.VoiceDevicesEventArgs e)
        {
            inputDevices = e.Devices;
            currentCaptureDevice = e.CurrentDevice;
        }

        public string CurrentCaptureDevice
        {
            get { return currentCaptureDevice; }
            set
            {
                currentCaptureDevice = value;
                AuxSetCaptureDevice(value);
            }
        }
        public string PlaybackDevice
        {
            get { return currentPlaybackDevice; }
            set
            {
                currentPlaybackDevice = value;
                AuxSetRenderDevice(value);
            }
        }

        public int MicLevel
        {
            set
            {
                ConnectorSetLocalMicVolume(connectionHandle, value);
            }
        }
        public int SpkrLevel
        {
            set
            {
                ConnectorSetLocalSpeakerVolume(connectionHandle, value);
            }
        }

        public bool MicMute
        {
            set
            {
                ConnectorMuteLocalMic(connectionHandle, value);
            }
        }

        public bool SpkrMute
        {
            set
            {
                ConnectorMuteLocalSpeaker(connectionHandle, value);
            }
        }

        /// <summary>
        /// Set audio test mode
        /// </summary>
        public bool TestMode
        {
            get { return testing; }
            set
            {
                testing = value;
                if (testing)
                {
                    if (spatialSession != null)
                    {
                        spatialSession.Close();
                        spatialSession = null;
                    }
                    AuxCaptureAudioStart(0);
                }
                else
                {
                    AuxCaptureAudioStop();
                    ParcelChanged();
                }
            }
        }
        #endregion




        /// <summary>
        /// Set voice channel for new parcel
        /// </summary>
        ///
        internal void ParcelChanged()
        {
            // Get the capability for this parcel.
            Caps c = Client.Network.CurrentSim.Caps;
            System.Uri pCap = c.CapabilityURI("ParcelVoiceInfoRequest");

            if (pCap == null)
            {
                Logger.Log("Null voice capability", Helpers.LogLevel.Error);
                return;
            }

            if (pCap == currentParcelCap)
            {
                // Parcel has not changed, so nothing to do.
                return;
            }

            // Parcel has changed.  If we were already in a spatial session, we have to close it first.
            if (spatialSession != null)
            {
                nextParcelCap = pCap;
                CloseSession(spatialSession.Handle);
            }

            // Not already in a session, so can start the new one.
            RequestParcelInfo(pCap);
        }

        private OpenMetaverse.Http.CapsClient parcelCap;

        /// <summary>
        /// Request info from a parcel capability Uri.
        /// </summary>
        /// <param name="cap"></param>

        void RequestParcelInfo(Uri cap)
        {
            Logger.Log("Requesting region voice info", Helpers.LogLevel.Info);

            parcelCap = new OpenMetaverse.Http.CapsClient(cap);
            parcelCap.OnComplete +=
                new OpenMetaverse.Http.CapsClient.CompleteCallback(pCap_OnComplete);
            OSD postData = new OSD();

            currentParcelCap = cap;
            parcelCap.BeginGetResponse(postData, OSDFormat.Xml, 10000);
        }

        /// <summary>
        /// Receive parcel voice cap
        /// </summary>
        /// <param name="client"></param>
        /// <param name="result"></param>
        /// <param name="error"></param>
        void pCap_OnComplete(OpenMetaverse.Http.CapsClient client,
            OpenMetaverse.StructuredData.OSD result,
            Exception error)
        {
            parcelCap.OnComplete -=
                new OpenMetaverse.Http.CapsClient.CompleteCallback(pCap_OnComplete);
            parcelCap = null;

            if (error != null)
            {
                Logger.Log("Region voice cap " + error.Message, Helpers.LogLevel.Error);
                return;
            }

            OpenMetaverse.StructuredData.OSDMap pMap = result as OpenMetaverse.StructuredData.OSDMap;

            regionName = pMap["region_name"].AsString();
            ReportConnectionState(ConnectionState.RegionCapAvailable);

            if (pMap.ContainsKey("voice_credentials"))
            {
                OpenMetaverse.StructuredData.OSDMap cred =
                    pMap["voice_credentials"] as OpenMetaverse.StructuredData.OSDMap;

                if (cred.ContainsKey("channel_uri"))
                    spatialUri = cred["channel_uri"].AsString();
                if (cred.ContainsKey("channel_credentials"))
                    spatialCredentials = cred["channel_credentials"].AsString();
            }

            if (spatialUri == null || spatialUri == "")
            {
                // "No voice chat allowed here");
                return;
            }

            Logger.Log("Voice connecting for region " + regionName, Helpers.LogLevel.Info);

            // STEP 5
            int reqId = SessionCreate(
                accountHandle,
                spatialUri, // uri
                "", // Channel name seems to be always null
                spatialCredentials, // spatialCredentials, // session password
                true,   // Join Audio
                false,   // Join Text
                "");
            if (reqId < 0)
            {
                Logger.Log("Voice Session ReqID " + reqId.ToString(), Helpers.LogLevel.Error);
            }
        }

        #region Location Update
        /// <summary>
        /// Tell Vivox where we are standing
        /// </summary>
        /// <remarks>This has to be called when we move or turn.</remarks>
        internal void UpdatePosition(AgentManager self)
        {
            // Get position in Global coordinates
            Vector3d OMVpos = new Vector3d(self.GlobalPosition);

            // Do not send trivial updates.
            if (OMVpos.ApproxEquals(oldPosition, 1.0))
                return;

            oldPosition = OMVpos;

            // Convert to the coordinate space that Vivox uses
            // OMV X is East, Y is North, Z is up
            // VVX X is East, Y is up, Z is South
            position.Position = new Vector3d(OMVpos.X, OMVpos.Z, -OMVpos.Y);

            // TODO Rotate these two vectors

            // Get azimuth from the facing Quaternion.
            // By definition, facing.W = Cos( angle/2 )
            double angle = 2.0 * Math.Acos(self.Movement.BodyRotation.W);

            position.LeftOrientation = new Vector3d(-1.0, 0.0, 0.0);
            position.AtOrientation = new Vector3d((float)Math.Acos(angle), 0.0, -(float)Math.Asin(angle));

            SessionSet3DPosition(
                sessionHandle,
                position,
                position);
        }

        /// <summary>
        /// Start and stop updating out position.
        /// </summary>
        /// <param name="go"></param>
        internal void PosUpdating(bool go)
        {
            if (go)
                posRestart.Set();
            else
                posRestart.Reset();
        }

        private void PositionThreadBody()
        {
            while (true)
            {
                posRestart.WaitOne();
                Thread.Sleep(1500);
                UpdatePosition(Client.Self);
            }
        }
        #endregion

    }
}
