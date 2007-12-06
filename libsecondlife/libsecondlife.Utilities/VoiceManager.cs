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
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Xml;
using System.Threading;
using libsecondlife;
using libsecondlife.StructuredData;
using libsecondlife.Capabilities;

namespace libsecondlife.Utilities
{
    public enum VoiceStatus
    {
        StatusLoginRetry,
        StatusLoggedIn,
        StatusJoining,
        StatusJoined,
        StatusLeftChannel,
        BeginErrorStatus,
        ErrorChannelFull,
        ErrorChannelLocked,
        ErrorNotAvailable,
        ErrorUnknown
    }

    public enum VoiceServiceType
    {
        /// <summary>Unknown voice service level</summary>
        Unknown,
        /// <summary>Spatialized local chat</summary>
        TypeA,
        /// <summary>Remote multi-party chat</summary>
        TypeB,
        /// <summary>One-to-one and small group chat</summary>
        TypeC
    }

    public partial class VoiceManager
    {
        public const int VOICE_MAJOR_VERSION = 1;
        public const string DAEMON_ARGS = " -p tcp -h -c -ll ";
        public const int DAEMON_LOG_LEVEL = 1;
        public const int DAEMON_PORT = 44124;
        public const string VOICE_RELEASE_SERVER = "bhr.vivox.com";
        public const string VOICE_DEBUG_SERVER = "bhd.vivox.com";
        public const string REQUEST_TERMINATOR = "\n\n\n";

        public delegate void LoginStateChangeCallback(int cookie, string accountHandle, int statusCode, string statusString, int state);
        public delegate void NewSessionCallback(int cookie, string accountHandle, string eventSessionHandle, int state, string nameString, string uriString);
        public delegate void SessionStateChangeCallback(int cookie, string uriString, int statusCode, string statusString, string eventSessionHandle, int state, bool isChannel, string nameString);
        public delegate void ParticipantStateChangeCallback(int cookie, string uriString, int statusCode, string statusString, int state, string nameString, string displayNameString, int participantType);
        public delegate void ParticipantPropertiesCallback(int cookie, string uriString, int statusCode, string statusString, bool isLocallyMuted, bool isModeratorMuted, bool isSpeaking, int volume, float energy);
        public delegate void AuxAudioPropertiesCallback(int cookie, float energy);
        public delegate void BasicActionCallback(int cookie, int statusCode, string statusString);
        public delegate void ConnectorCreatedCallback(int cookie, int statusCode, string statusString, string connectorHandle);
        public delegate void LoginCallback(int cookie, int statusCode, string statusString, string accountHandle);
        public delegate void SessionCreatedCallback(int cookie, int statusCode, string statusString, string sessionHandle);
        public delegate void DevicesCallback(int cookie, int statusCode, string statusString, string currentDevice);
        public delegate void ProvisionAccountCallback(string username, string password);

        public event LoginStateChangeCallback OnLoginStateChange;
        public event NewSessionCallback OnNewSession;
        public event SessionStateChangeCallback OnSessionStateChange;
        public event ParticipantStateChangeCallback OnParticipantStateChange;
        public event ParticipantPropertiesCallback OnParticipantProperties;
        public event AuxAudioPropertiesCallback OnAuxAudioProperties;
        public event ConnectorCreatedCallback OnConnectorCreated;
        public event LoginCallback OnLogin;
        public event SessionCreatedCallback OnSessionCreated;
        public event BasicActionCallback OnSessionConnected;
        public event BasicActionCallback OnAccountLogout;
        public event BasicActionCallback OnConnectorInitiateShutdown;
        public event BasicActionCallback OnAccountChannelGetList;
        public event BasicActionCallback OnSessionTerminated;
        public event DevicesCallback OnCaptureDevices;
        public event DevicesCallback OnRenderDevices;
        public event ProvisionAccountCallback OnProvisionAccount;

        public SecondLife Client;
        public string VoiceServer = VOICE_RELEASE_SERVER;
        public bool Enabled;

        protected TCPPipe _DaemonPipe;
        protected VoiceStatus _Status;
        protected int _CommandCookie = 0;
        protected string _TuningSoundFile = String.Empty;
        protected Dictionary<string, string> _ChannelMap = new Dictionary<string, string>();
        protected List<string> _CaptureDevices = new List<string>();
        protected List<string> _RenderDevices = new List<string>();

        #region Response Processing Variables

        private bool isEvent = false;
        private bool isChannel = false;
        private bool isLocallyMuted = false;
        private bool isModeratorMuted = false;
        private bool isSpeaking = false;
        private int cookie = 0;
        private int returnCode = 0;
        private int statusCode = 0;
        private int volume = 0;
        private int state = 0;
        private int participantType = 0;
        private float energy = 0f;
        private string statusString = String.Empty;
        private string uuidString = String.Empty;
        private string actionString = String.Empty;
        private string connectorHandle = String.Empty;
        private string accountHandle = String.Empty;
        private string sessionHandle = String.Empty;
        private string eventSessionHandle = String.Empty;
        private string eventTypeString = String.Empty;
        private string uriString = String.Empty;
        private string nameString = String.Empty;
        private string audioMediaString = String.Empty;
        private string displayNameString = String.Empty;

        #endregion Response Processing Variables

        public VoiceManager(SecondLife client)
        {
            Client = client;
            Client.Network.RegisterEventCallback("RequiredVoiceVersion", new Caps.EventQueueCallback(RequiredVoiceVersionEventHandler));

            // Register callback handlers for the blocking functions
            RegisterCallbacks();

            Enabled = true;
        }

        public bool IsDaemonRunning()
        {
            throw new NotImplementedException();
        }

        public bool StartDaemon()
        {
            throw new NotImplementedException();
        }

        public void StopDaemon()
        {
            throw new NotImplementedException();
        }

        public bool ConnectToDaemon()
        {
            if (!Enabled) return false;

            return ConnectToDaemon("127.0.0.1", DAEMON_PORT);
        }

        public bool ConnectToDaemon(string address, int port)
        {
            if (!Enabled) return false;

            _DaemonPipe = new TCPPipe();
            _DaemonPipe.OnDisconnected += new TCPPipe.OnDisconnectedCallback(_DaemonPipe_OnDisconnected);
            _DaemonPipe.OnReceiveLine += new TCPPipe.OnReceiveLineCallback(_DaemonPipe_OnReceiveLine);

            SocketException se = _DaemonPipe.Connect(address, port);

            if (se == null)
            {
                return true;
            }
            else
            {
                Console.WriteLine("Connection failed: " + se.Message);
                return false;
            }
        }

        public Dictionary<string, string> GetChannelMap()
        {
            return new Dictionary<string, string>(_ChannelMap);
        }

        public List<string> CurrentCaptureDevices()
        {
            return new List<string>(_CaptureDevices);
        }

        public List<string> CurrentRenderDevices()
        {
            return new List<string>(_RenderDevices);
        }

        public string VoiceAccountFromUUID(LLUUID id)
        {
            string result = "x" + Convert.ToBase64String(id.GetBytes());
            return result.Replace('+', '-').Replace('/', '_');
        }

        public LLUUID UUIDFromVoiceAccount(string accountName)
        {
            if (accountName.Length == 25 && accountName[0] == 'x' && accountName[23] == '=' && accountName[24] == '=')
            {
                accountName = accountName.Replace('/', '_').Replace('+', '-');
                byte[] idBytes = Convert.FromBase64String(accountName);

                if (idBytes.Length == 16)
                    return new LLUUID(idBytes, 0);
                else
                    return LLUUID.Zero;
            }
            else
            {
                return LLUUID.Zero;
            }
        }

        public string SIPURIFromVoiceAccount(string account)
        {
            return String.Format("sip:{0}@{1}", account, VoiceServer);
        }

        public int RequestCaptureDevices()
        {
            if (_DaemonPipe.Connected)
            {
                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(String.Format(
                    "<Request requestId=\"{0}\" action=\"Aux.GetCaptureDevices.1\"></Request>{1}",
                    _CommandCookie++,
                    REQUEST_TERMINATOR)));

                return _CommandCookie - 1;
            }
            else
            {
                Client.Log("VoiceManager.RequestCaptureDevices() called when the daemon pipe is disconnected", Helpers.LogLevel.Error);
                return -1;
            }
        }

        public int RequestRenderDevices()
        {
            if (_DaemonPipe.Connected)
            {
                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(String.Format(
                    "<Request requestId=\"{0}\" action=\"Aux.GetRenderDevices.1\"></Request>{1}",
                    _CommandCookie++,
                    REQUEST_TERMINATOR)));

                return _CommandCookie - 1;
            }
            else
            {
                Client.Log("VoiceManager.RequestRenderDevices() called when the daemon pipe is disconnected", Helpers.LogLevel.Error);
                return -1;
            }
        }

        public int RequestCreateConnector()
        {
            return RequestCreateConnector(VoiceServer);
        }

        public int RequestCreateConnector(string voiceServer)
        {
            if (_DaemonPipe.Connected)
            {
                VoiceServer = voiceServer;

                string accountServer = String.Format("https://www.{0}/api2/", VoiceServer);
                string logPath = ".";

                StringBuilder request = new StringBuilder();
                request.Append(String.Format("<Request requestId=\"{0}\" action=\"Connector.Create.1\">", _CommandCookie++));
                request.Append("<ClientName>V2 SDK</ClientName>");
                request.Append(String.Format("<AccountManagementServer>{0}</AccountManagementServer>", accountServer));
                request.Append("<Logging>");
                request.Append("<Enabled>false</Enabled>");
                request.Append(String.Format("<Folder>{0}</Folder>", logPath));
                request.Append("<FileNamePrefix>vivox-gateway</FileNamePrefix>");
                request.Append("<FileNameSuffix>.log</FileNameSuffix>");
                request.Append("<LogLevel>0</LogLevel>");
                request.Append("</Logging>");
                request.Append("</Request>");
                request.Append(REQUEST_TERMINATOR);

                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(request.ToString()));

                return _CommandCookie - 1;
            }
            else
            {
                Client.Log("VoiceManager.CreateConnector() called when the daemon pipe is disconnected", Helpers.LogLevel.Error);
                return -1;
            }
        }

        public bool RequestProvisionAccount()
        {
            if (Enabled && Client.Network.Connected)
            {
                if (Client.Network.CurrentSim != null && Client.Network.CurrentSim.Caps != null)
                {
                    Uri url = Client.Network.CurrentSim.Caps.CapabilityURI("ProvisionVoiceAccountRequest");

                    if (url != null)
                    {
                        CapsClient request = new CapsClient(url);
                        request.OnComplete += new CapsClient.CompleteCallback(ProvisionCapsResponse);
                        request.StartRequest();

                        return true;
                    }
                    else
                    {
                        Client.Log("VoiceManager.RequestProvisionAccount(): ProvisionVoiceAccountRequest capability is missing", 
                            Helpers.LogLevel.Info);
                        return false;
                    }
                }
            }

            Client.Log("VoiceManager.RequestProvisionAccount(): Voice system is currently disabled", Helpers.LogLevel.Info);
            return false;
        }

        public int RequestLogin(string accountName, string password, string connectorHandle)
        {
            if (_DaemonPipe.Connected)
            {
                StringBuilder request = new StringBuilder();
                request.Append(String.Format("<Request requestId=\"{0}\" action=\"Account.Login.1\">", _CommandCookie++));
                request.Append(String.Format("<ConnectorHandle>{0}</ConnectorHandle>", connectorHandle));
                request.Append(String.Format("<AccountName>{0}</AccountName>", accountName));
                request.Append(String.Format("<AccountPassword>{0}</AccountPassword>", password));
                request.Append("<AudioSessionAnswerMode>VerifyAnswer</AudioSessionAnswerMode>");
                request.Append("</Request>");
                request.Append(REQUEST_TERMINATOR);

                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(request.ToString()));

                return _CommandCookie - 1;
            }
            else
            {
                Client.Log("VoiceManager.Login() called when the daemon pipe is disconnected", Helpers.LogLevel.Error);
                return -1;
            }
        }

        public int RequestSetRenderDevice(string deviceName)
        {
            if (_DaemonPipe.Connected)
            {
                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(String.Format(
                    "<Request requestId=\"{0}\" action=\"Aux.SetRenderDevice.1\"><RenderDeviceSpecifier>{1}</RenderDeviceSpecifier></Request>{2}",
                    _CommandCookie, deviceName, REQUEST_TERMINATOR)));

                return _CommandCookie - 1;
            }
            else
            {
                Client.Log("VoiceManager.RequestSetRenderDevice() called when the daemon pipe is disconnected", Helpers.LogLevel.Error);
                return -1;
            }
        }

        public int RequestStartTuningMode(int duration)
        {
            if (_DaemonPipe.Connected)
            {
                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(String.Format(
                    "<Request requestId=\"{0}\" action=\"Aux.CaptureAudioStart.1\"><Duration>{1}</Duration></Request>{2}",
                    _CommandCookie, duration, REQUEST_TERMINATOR)));

                return _CommandCookie - 1;
            }
            else
            {
                Client.Log("VoiceManager.RequestStartTuningMode() called when the daemon pipe is disconnected", Helpers.LogLevel.Error);
                return -1;
            }
        }

        public int RequestStopTuningMode()
        {
            if (_DaemonPipe.Connected)
            {
                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(String.Format(
                    "<Request requestId=\"{0}\" action=\"Aux.CaptureAudioStop.1\"></Request>{1}",
                    _CommandCookie, REQUEST_TERMINATOR)));

                return _CommandCookie - 1;
            }
            else
            {
                Client.Log("VoiceManager.RequestStopTuningMode() called when the daemon pipe is disconnected", Helpers.LogLevel.Error);
                return _CommandCookie - 1;
            }
        }

        public int RequestSetSpeakerVolume(int volume)
        {
            if (volume < 0 || volume > 100)
                throw new ArgumentException("volume must be between 0 and 100", "volume");

            if (_DaemonPipe.Connected)
            {
                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(String.Format(
                    "<Request requestId=\"{0}\" action=\"Aux.SetSpeakerLevel.1\"><Level>{1}</Level></Request>{2}",
                    _CommandCookie, volume, REQUEST_TERMINATOR)));

                return _CommandCookie - 1;
            }
            else
            {
                Client.Log("VoiceManager.RequestSetSpeakerVolume() called when the daemon pipe is disconnected", Helpers.LogLevel.Error);
                return -1;
            }
        }

        public int RequestSetCaptureVolume(int volume)
        {
            if (volume < 0 || volume > 100)
                throw new ArgumentException("volume must be between 0 and 100", "volume");

            if (_DaemonPipe.Connected)
            {
                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(String.Format(
                    "<Request requestId=\"{0}\" action=\"Aux.SetMicLevel.1\"><Level>{1}</Level></Request>{2}",
                    _CommandCookie, volume, REQUEST_TERMINATOR)));

                return _CommandCookie - 1;
            }
            else
            {
                Client.Log("VoiceManager.RequestSetCaptureVolume() called when the daemon pipe is disconnected", Helpers.LogLevel.Error);
                return -1;
            }
        }

        /// <summary>
        /// Does not appear to be working
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="loop"></param>
        public int RequestRenderAudioStart(string fileName, bool loop)
        {
            if (_DaemonPipe.Connected)
            {
                _TuningSoundFile = fileName;

                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(String.Format(
                    "<Request requestId=\"{0}\" action=\"Aux.RenderAudioStart.1\"><SoundFilePath>{1}</SoundFilePath><Loop>{2}</Loop></Request>{3}",
                    _CommandCookie++, _TuningSoundFile, (loop ? "1" : "0"), REQUEST_TERMINATOR)));

                return _CommandCookie - 1;
            }
            else
            {
                Client.Log("VoiceManager.RequestRenderAudioStart() called when the daemon pipe is disconnected", Helpers.LogLevel.Error);
                return -1;
            }
        }

        public int RequestRenderAudioStop()
        {
            if (_DaemonPipe.Connected)
            {
                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(String.Format(
                    "<Request requestId=\"{0}\" action=\"Aux.RenderAudioStop.1\"><SoundFilePath>{1}</SoundFilePath></Request>{2}",
                    _CommandCookie++, _TuningSoundFile, REQUEST_TERMINATOR)));

                return _CommandCookie - 1;
            }
            else
            {
                Client.Log("VoiceManager.RequestRenderAudioStop() called when the daemon pipe is disconnected", Helpers.LogLevel.Error);
                return -1;
            }
        }

        #region Callbacks

        private void RequiredVoiceVersionEventHandler(string message, LLSD llsd, Simulator simulator)
        {
            LLSDMap body = (LLSDMap)llsd;

            if (body.ContainsKey("major_version"))
            {
                int majorVersion = body["major_version"].AsInteger();

                if (VOICE_MAJOR_VERSION != majorVersion)
                {
                    Client.Log(String.Format("Voice version mismatch! Got {0}, expecting {1}. Disabling the voice manager",
                        majorVersion, VOICE_MAJOR_VERSION), Helpers.LogLevel.Error);
                    Enabled = false;
                }
                else
                {
                    Client.DebugLog("Voice version " + majorVersion + " verified");
                }
            }
        }

        private void ProvisionCapsResponse(CapsClient client, LLSD response, Exception error)
        {
            if (response is LLSDMap)
            {
                LLSDMap respTable = (LLSDMap)response;

                if (OnProvisionAccount != null)
                {
                    try { OnProvisionAccount(respTable["username"].AsString(), respTable["password"].AsString()); }
                    catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                }
            }
        }

        private void _DaemonPipe_OnDisconnected(SocketException se)
        {
            if (se != null) Console.WriteLine("Disconnected! " + se.Message);
            else Console.WriteLine("Disconnected!");
        }

        private void _DaemonPipe_OnReceiveLine(string line)
        {
            XmlTextReader reader = new XmlTextReader(new StringReader(line));

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                    {
                        if (reader.Depth == 0)
                        {
                            isEvent = (reader.Name == "Event");

                            if (isEvent || reader.Name == "Response")
                            {
                                for (int i = 0; i < reader.AttributeCount; i++)
                                {
                                    reader.MoveToAttribute(i);

                                    switch (reader.Name)
                                    {
                                        case "requestId":
                                            uuidString = reader.Value;
                                            break;
                                        case "action":
                                            actionString = reader.Value;
                                            break;
                                        case "type":
                                            eventTypeString = reader.Value;
                                            break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            switch (reader.Name)
                            {
                                case "InputXml":
                                    cookie = -1;

                                    // Parse through here to get the cookie value
                                    reader.Read();
                                    if (reader.Name == "Request")
                                    {
                                        for (int i = 0; i < reader.AttributeCount; i++)
                                        {
                                            reader.MoveToAttribute(i);

                                            if (reader.Name == "requestId")
                                            {
                                                Int32.TryParse(reader.Value, out cookie);
                                                break;
                                            }
                                        }
                                    }

                                    if (cookie == -1)
                                    {
                                        Client.Log("VoiceManager._DaemonPipe_OnReceiveLine(): Failed to parse InputXml for the cookie",
                                            Helpers.LogLevel.Warning);
                                    }
                                    break;
                                case "CaptureDevices":
                                    _CaptureDevices.Clear();
                                    break;
                                case "RenderDevices":
                                    _RenderDevices.Clear();
                                    break;
                                case "ReturnCode":
                                    returnCode = reader.ReadElementContentAsInt();
                                    break;
                                case "StatusCode":
                                    statusCode = reader.ReadElementContentAsInt();
                                    break;
                                case "StatusString":
                                    statusString = reader.ReadElementContentAsString();
                                    break;
                                case "State":
                                    state = reader.ReadElementContentAsInt();
                                    break;
                                case "ConnectorHandle":
                                    connectorHandle = reader.ReadElementContentAsString();
                                    break;
                                case "AccountHandle":
                                    accountHandle = reader.ReadElementContentAsString();
                                    break;
                                case "SessionHandle":
                                    sessionHandle = reader.ReadElementContentAsString();
                                    break;
                                case "URI":
                                    uriString = reader.ReadElementContentAsString();
                                    break;
                                case "IsChannel":
                                    isChannel = reader.ReadElementContentAsBoolean();
                                    break;
                                case "Name":
                                    nameString = reader.ReadElementContentAsString();
                                    break;
                                case "AudioMedia":
                                    audioMediaString = reader.ReadElementContentAsString();
                                    break;
                                case "ChannelName":
                                    nameString = reader.ReadElementContentAsString();
                                    break;
                                case "ParticipantURI":
                                    uriString = reader.ReadElementContentAsString();
                                    break;
                                case "DisplayName":
                                    displayNameString = reader.ReadElementContentAsString();
                                    break;
                                case "AccountName":
                                    nameString = reader.ReadElementContentAsString();
                                    break;
                                case "ParticipantType":
                                    participantType = reader.ReadElementContentAsInt();
                                    break;
                                case "IsLocallyMuted":
                                    isLocallyMuted = reader.ReadElementContentAsBoolean();
                                    break;
                                case "IsModeratorMuted":
                                    isModeratorMuted = reader.ReadElementContentAsBoolean();
                                    break;
                                case "IsSpeaking":
                                    isSpeaking = reader.ReadElementContentAsBoolean();
                                    break;
                                case "Volume":
                                    volume = reader.ReadElementContentAsInt();
                                    break;
                                case "Energy":
                                    energy = reader.ReadElementContentAsFloat();
                                    break;
                                case "MicEnergy":
                                    energy = reader.ReadElementContentAsFloat();
                                    break;
                                case "ChannelURI":
                                    uriString = reader.ReadElementContentAsString();
                                    break;
                                case "ChannelListResult":
                                    _ChannelMap[nameString] = uriString;
                                    break;
                                case "CaptureDevice":
                                    reader.Read();
                                    _CaptureDevices.Add(reader.ReadElementContentAsString());
                                    break;
                                case "CurrentCaptureDevice":
                                    reader.Read();
                                    nameString = reader.ReadElementContentAsString();
                                    break;
                                case "RenderDevice":
                                    reader.Read();
                                    _RenderDevices.Add(reader.ReadElementContentAsString());
                                    break;
                                case "CurrentRenderDevice":
                                    reader.Read();
                                    nameString = reader.ReadElementContentAsString();
                                    break;
                            }
                        }

                        break;
                    }
                    case XmlNodeType.EndElement:
                        if (reader.Depth == 0)
                            ProcessEvent();
                        break;
                }
            }

            if (isEvent)
            {
            }

            //Client.DebugLog("VOICE: " + line);
        }

        private void ProcessEvent()
        {
            if (isEvent)
            {
                switch (eventTypeString)
                {
                    case "LoginStateChangeEvent":
                        if (OnLoginStateChange != null) OnLoginStateChange(cookie, accountHandle, statusCode, statusString, state);
                        break;
                    case "SessionNewEvent":
                        if (OnNewSession != null) OnNewSession(cookie, accountHandle, eventSessionHandle, state, nameString, uriString);
                        break;
                    case "SessionStateChangeEvent":
                        if (OnSessionStateChange != null) OnSessionStateChange(cookie, uriString, statusCode, statusString, eventSessionHandle, state, isChannel, nameString);
                        break;
                    case "ParticipantStateChangeEvent":
                        if (OnParticipantStateChange != null) OnParticipantStateChange(cookie, uriString, statusCode, statusString, state, nameString, displayNameString, participantType);
                        break;
                    case "ParticipantPropertiesEvent":
                        if (OnParticipantProperties != null) OnParticipantProperties(cookie, uriString, statusCode, statusString, isLocallyMuted, isModeratorMuted, isSpeaking, volume, energy);
                        break;
                    case "AuxAudioPropertiesEvent":
                        if (OnAuxAudioProperties != null) OnAuxAudioProperties(cookie, energy);
                        break;
                }
            }
            else
            {
                switch (actionString)
                {
                    case "Connector.Create.1":
                        if (OnConnectorCreated != null) OnConnectorCreated(cookie, statusCode, statusString, connectorHandle);
                        break;
                    case "Account.Login.1":
                        if (OnLogin != null) OnLogin(cookie, statusCode, statusString, accountHandle);
                        break;
                    case "Session.Create.1":
                        if (OnSessionCreated != null) OnSessionCreated(cookie, statusCode, statusString, sessionHandle);
                        break;
                    case "Session.Connect.1":
                        if (OnSessionConnected != null) OnSessionConnected(cookie, statusCode, statusString);
                        break;
                    case "Session.Terminate.1":
                        if (OnSessionTerminated != null) OnSessionTerminated(cookie, statusCode, statusString);
                        break;
                    case "Account.Logout.1":
                        if (OnAccountLogout != null) OnAccountLogout(cookie, statusCode, statusString);
                        break;
                    case "Connector.InitiateShutdown.1":
                        if (OnConnectorInitiateShutdown != null) OnConnectorInitiateShutdown(cookie, statusCode, statusString);
                        break;
                    case "Account.ChannelGetList.1":
                        if (OnAccountChannelGetList != null) OnAccountChannelGetList(cookie, statusCode, statusString);
                        break;
                    case "Aux.GetCaptureDevices.1":
                        if (OnCaptureDevices != null) OnCaptureDevices(cookie, statusCode, statusString, nameString);
                        break;
                    case "Aux.GetRenderDevices.1":
                        if (OnRenderDevices != null) OnRenderDevices(cookie, statusCode, statusString, nameString);
                        break;
                }
            }
        }

        #endregion Callbacks
    }
}
