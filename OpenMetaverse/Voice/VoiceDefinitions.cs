using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace OpenMetaverse.Voice
{
    public partial class VoiceGateway
    {
        #region Enums

        public enum LoginState
        {
            LoggedOut = 0,
            LoggedIn = 1,
            Error = 4
        }

        public enum SessionState
        {
            Idle = 1,
            Answering = 2,
            InProgress = 3,
            Connected = 4,
            Disconnected = 5,
            Hold = 6,
            Refer = 7,
            Ringing = 8
        }

        public enum ParticipantState
        {
            Idle = 1,
            Pending = 2,
            Incoming = 3,
            Answering = 4,
            InProgress = 5,
            Ringing = 6,
            Connected = 7,
            Disconnecting = 8,
            Disconnected = 9
        }

        public enum ParticipantType
        {
            User = 0,
            Moderator = 1,
            Focus = 2
        }

        #endregion Enums

        #region Logging

        public class VoiceLoggingSettings
        {
            /// <summary>Enable logging</summary>
            public bool Enabled;
            /// <summary>The folder where any logs will be created</summary>
            public string Folder;
            /// <summary>This will be prepended to beginning of each log file</summary>
            public string FileNamePrefix;
            /// <summary>The suffix or extension to be appended to each log file</summary>
            public string FileNameSuffix;
            /// <summary>
            /// 0: NONE - No logging
            /// 1: ERROR - Log errors only
            /// 2: WARNING - Log errors and warnings
            /// 3: INFO - Log errors, warnings and info
            /// 4: DEBUG - Log errors, warnings, info and debug
            /// </summary>
            public int LogLevel;

            /// <summary>
            /// Constructor for default logging settings
            /// </summary>
            public VoiceLoggingSettings()
            {
                Enabled = false;
                Folder = String.Empty;
                FileNamePrefix = "Connector";
                FileNameSuffix = ".log";
                LogLevel = 0;
            }
        }

        #endregion Logging

        #region Session Delegates

        /// <summary>Response to Session.Create request</summary>
        public delegate void SessionCreateResponseCallback(int ReturnCode, int StatusCode, string StatusString, string SessionHandle, VoiceRequest Request);
        /// <summary>Response to Session.Connect request</summary>
        public delegate void SessionConnectResponseCallback(int ReturnCode, int StatusCode, string StatusString, VoiceRequest Request);
        /// <summary>Response to Session.RenderAudioStart request</summary>
        public delegate void SessionRenderAudioStartResponseCallback(int ReturnCode, int StatusCode, string StatusString, VoiceRequest Request);
        /// <summary>Response to Session.RenderAudioStop request</summary>
        public delegate void SessionRenderAudioStopResponseCallback(int ReturnCode, int StatusCode, string StatusString, VoiceRequest Request);
        /// <summary>Response to Session.Terminate request</summary>
        public delegate void SessionTerminateResponseCallback(int ReturnCode, int StatusCode, string StatusString, VoiceRequest Request);
        /// <summary>Response to Session.SetParticipantVolumeForMe request</summary>
        public delegate void SessionSetParticipantVolumeForMeResponseCallback(int ReturnCode, int StatusCode, string StatusString, VoiceRequest Request);
        public delegate void SessionNewEventCallback(string AccountHandle, string SessionHandle, string URI, bool IsChannel, string Name, string AudioMedia);
        public delegate void SessionStateChangeEventCallback(string SessionHandle, int StatusCode, string StatusString, SessionState State, string URI, bool IsChannel, string ChannelName);
        public delegate void SessionParticipantStateChangeEventCallback(string SessionHandle, int StatusCode, string StatusString, ParticipantState State, string ParticipantURI, string AccountName, string DisplayName, ParticipantType ParticipantType);
        public delegate void SessionParticipantPropertiesEventCallback(string SessionHandle, string ParticipantURI, bool IsLocallyMuted, bool IsModeratorMuted, bool IsSpeaking, int Volume, float Energy);
        public delegate void SessionMediaEventCallback(string SessionHandle, bool HasText, bool HasAudio, bool HasVideo, bool Terminated);

        #endregion Session Delegates

        #region Connector Delegates

        public delegate void ConnectorCreateResponseCallback(int ReturnCode, string VersionID, int StatusCode, string StatusString, string ConnectorHandle, VoiceRequest Request);
        public delegate void ConnectorInitiateShutdownResponseCallback(int ReturnCode, int StatusCode, string StatusString, VoiceRequest Request);
        public delegate void ConnectorMuteLocalMicResponseCallback(int ReturnCode, int StatusCode, string StatusString, VoiceRequest Request);
        public delegate void ConnectorMuteLocalSpeakerResponseCallback(int ReturnCode, int StatusCode, string StatusString, VoiceRequest Request);
        public delegate void ConnectorSetLocalMicVolumeResponseCallback(int ReturnCode, int StatusCode, string StatusString, VoiceRequest Request);
        public delegate void ConnectorSetLocalSpeakerVolumeResponseCallback(int ReturnCode, int StatusCode, string StatusString, VoiceRequest Request);

        #endregion Connector Delegates

        #region Aux Delegates

        public delegate void AuxGetCaptureDevicesResponseCallback(int ReturnCode, int StatusCode, string StatusString, List<string> CaptureDevices, string CurrentCaptureDevice, VoiceRequest Request);
        public delegate void AuxGetRenderDevicesResponseCallback(int ReturnCode, int StatusCode, string StatusString, List<string> RenderDevices, string CurrentRenderDevice, VoiceRequest Request);
        public delegate void AuxSetRenderDeviceResponseCallback(int ReturnCode, int StatusCode, string StatusString, VoiceRequest Request);
        public delegate void AuxSetCaptureDeviceResponseCallback(int ReturnCode, int StatusCode, string StatusString, VoiceRequest Request);
        public delegate void AuxCaptureAudioStartResponseCallback(int ReturnCode, int StatusCode, string StatusString, VoiceRequest Request);
        public delegate void AuxCaptureAudioStopResponseCallback(int ReturnCode, int StatusCode, string StatusString, VoiceRequest Request);
        public delegate void AuxSetMicLevelResponseCallback(int ReturnCode, int StatusCode, string StatusString, VoiceRequest Request);
        public delegate void AuxSetSpeakerLevelResponseCallback(int ReturnCode, int StatusCode, string StatusString, VoiceRequest Request);
        /// <summary>
        /// Audio Properties Events are sent after audio capture is started. These events are used to display a microphone VU meter
        /// </summary>
        /// <param name="MicIsActive">True if voice is detected on the microphone</param>
        /// <param name="MicEnergy">audio energy, from 0 to 1</param>
        /// <param name="MicVolume">current mic volume</param>
        /// <param name="SpeakerVolume">currently unimplemented, and always 0</param>
        public delegate void AuxAudioPropertiesEventCallback(bool MicIsActive, float MicEnergy, float MicVolume, float SpeakerVolume);
        // FIXME: Should MicVolume and SpeakerVolume be ints?

        #endregion Aux Delegates

        #region Account Delegates

        public delegate void AccountLoginResponseCallback(int ReturnCode, int StatusCode, string StatusString, string AccountHandle, VoiceRequest Request);
        public delegate void AccountLogoutResponseCallback(int ReturnCode, int StatusCode, string StatusString, VoiceRequest Request);
        public delegate void AccountLoginStateChangeEventCallback(string AccountHandle, int StatusCode, string StatusString, LoginState State);

        #endregion Account Delegates

        #region Session Events

        /// <summary>Response to Session.Create request</summary>
        public event SessionCreateResponseCallback OnSessionCreateResponse;
        /// <summary>Response to Session.Connect request</summary>
        public event SessionConnectResponseCallback OnSessionConnectResponse;
        /// <summary>Response to Session.RenderAudioStart request</summary>
        public event SessionRenderAudioStartResponseCallback OnSessionRenderAudioStartResponse;
        /// <summary>Response to Session.RenderAudioStop request</summary>
        public event SessionRenderAudioStopResponseCallback OnSessionRenderAudioStopResponse;
        /// <summary>Response to Session.Terminate request</summary>
        public event SessionTerminateResponseCallback OnSessionTerminateResponse;
        /// <summary>Response to Session.SetParticipantVolumeForMe request</summary>
        public event SessionSetParticipantVolumeForMeResponseCallback OnSessionSetParticipantVolumeForMeResponse;
        /// <summary>Sent when an incoming session occurs</summary>
        public event SessionNewEventCallback OnSessionNewEvent;
        /// <summary>Sent for specific Session state changes (connected, disconnected)</summary>
        public event SessionStateChangeEventCallback OnSessionStateChangeEvent;
        /// <summary>Sent for specific Participant state changes (new participants, dropped participants)</summary>
        public event SessionParticipantStateChangeEventCallback OnSessionParticipantStateChangeEvent;
        /// <summary>Sent for specific Participant Property changes (IsSpeaking, Volume, Energy, etc.)</summary>
        public event SessionParticipantPropertiesEventCallback OnSessionParticipantPropertiesEvent;
        /// <summary></summary>
        public event SessionMediaEventCallback OnSessionMediaEvent;

        #endregion Session Events

        #region Connector Events

        /// <summary>Response to Connector.Create request</summary>
        public event ConnectorCreateResponseCallback OnConnectorCreateResponse;
        /// <summary>Response to Connector.InitiateShutdown request</summary>
        public event ConnectorInitiateShutdownResponseCallback OnConnectorInitiateShutdownResponse;
        /// <summary>Response to Connector.MuteLocalMic request</summary>
        public event ConnectorMuteLocalMicResponseCallback OnConnectorMuteLocalMicResponse;
        /// <summary>Response to Connector.MuteLocalSpeaker request</summary>
        public event ConnectorMuteLocalSpeakerResponseCallback OnConnectorMuteLocalSpeakerResponse;
        /// <summary>Response to Connector.SetLocalMicVolume request</summary>
        public event ConnectorSetLocalMicVolumeResponseCallback OnConnectorSetLocalMicVolumeResponse;
        /// <summary>Response to Connector.SetLocalSpeakerVolume request</summary>
        public event ConnectorSetLocalSpeakerVolumeResponseCallback OnConnectorSetLocalSpeakerVolumeResponse;

        #endregion Connector Events

        #region Aux Events

        /// <summary>Response to Aux.GetCaptureDevices request</summary>
        public event AuxGetCaptureDevicesResponseCallback OnAuxGetCaptureDevicesResponse;
        /// <summary>Response to Aux.GetRenderDevices request</summary>
        public event AuxGetRenderDevicesResponseCallback OnAuxGetRenderDevicesResponse;
        /// <summary>Response to Aux.SetRenderDevice request</summary>
        public event AuxSetRenderDeviceResponseCallback OnAuxSetRenderDeviceResponse;
        /// <summary>Response to Aux.SetCaptureDevice request</summary>
        public event AuxSetCaptureDeviceResponseCallback OnAuxSetCaptureDeviceResponse;
        /// <summary>Response to Aux.CaptureAudioStart request</summary>
        public event AuxCaptureAudioStartResponseCallback OnAuxCaptureAudioStartResponse;
        /// <summary>Response to Aux.CaptureAudioStop request</summary>
        public event AuxCaptureAudioStopResponseCallback OnAuxCaptureAudioStopResponse;
        /// <summary>Response to Aux.SetMicLevel request</summary>
        public event AuxSetMicLevelResponseCallback OnAuxSetMicLevelResponse;
        /// <summary>Response to Aux.SetSpeakerLevel request</summary>
        public event AuxSetSpeakerLevelResponseCallback OnAuxSetSpeakerLevelResponse;
        /// <summary>Audio Properties Events are sent after audio capture is started.
        /// These events are used to display a microphone VU meter</summary>
        public event AuxAudioPropertiesEventCallback OnAuxAudioPropertiesEvent;

        #endregion Aux Events

        #region Account Events

        /// <summary>Response to Account.Login request</summary>
        public event AccountLoginResponseCallback OnAccountLoginResponse;
        /// <summary>Response to Account.Logout request</summary>
        public event AccountLogoutResponseCallback OnAccountLogoutResponse;
        /// <summary>This event message is sent whenever the login state of the
        /// particular Account has transitioned from one value to another</summary>
        public event AccountLoginStateChangeEventCallback OnAccountLoginStateChangeEvent;

        #endregion Account Events

        #region XML Serialization Classes

        private XmlSerializer EventSerializer = new XmlSerializer(typeof(VoiceEvent));
        private XmlSerializer ResponseSerializer = new XmlSerializer(typeof(VoiceResponse));

        [XmlRoot("Event")]
        public class VoiceEvent
        {
            [XmlAttribute("type")] public string Type;
            public string AccountHandle;
            public string StatusCode;
            public string StatusString;
            public string State;
            public string SessionHandle;
            public string URI;
            public string IsChannel;
            public string Name;
            public string AudioMedia;
            public string ChannelName;
            public string ParticipantURI;
            public string AccountName;
            public string DisplayName;
            public string ParticipantType;
            public string IsLocallyMuted;
            public string IsModeratorMuted;
            public string IsSpeaking;
            public string Volume;
            public string Energy;
            public string MicIsActive;
            public string MicEnergy;
            public string MicVolume;
            public string SpeakerVolume;
            public string HasText;
            public string HasAudio;
            public string HasVideo;
            public string Terminated;
        }

        [XmlRoot("Response")]
        public class VoiceResponse
        {
            [XmlAttribute("requestId")] public string RequestId;
            [XmlAttribute("action")] public string Action;
            public string ReturnCode;
            public VoiceResponseResults Results;
            public VoiceInputXml InputXml;
        }

        public class CaptureDevice
        {
            public string Device;
        }

        public class RenderDevice
        {
            public string Device;
        }

        public class VoiceResponseResults
        {
            public string VersionID;
            public string StatusCode;
            public string StatusString;
            public string ConnectorHandle;
            public string AccountHandle;
            public string SessionHandle;
            public List<CaptureDevice> CaptureDevices;
            public CaptureDevice CurrentCaptureDevice;
            public List<RenderDevice> RenderDevices;
            public RenderDevice CurrentRenderDevice;
        }

        public class VoiceInputXml
        {
            public VoiceRequest Request;
        }

        [XmlRoot("Request")]
        public class VoiceRequest
        {
            [XmlAttribute("requestId")] public string RequestId;
            [XmlAttribute("action")] public string Action;
            public string RenderDeviceSpecifier;
            public string CaptureDeviceSpecifier;
            public string Duration;
            public string Level;
            public string ClientName;
            public string AccountManagementServer;
            public string MinimumPort;
            public string MaximumPort;
            public VoiceLoggingSettings Logging;
            public string ConnectorHandle;
            public string Value;
            public string AccountName;
            public string AccountPassword;
            public string AudioSessionAnswerMode;
            public string AccountURI;
            public string ParticipantPropertyFrequency;
            public string EnableBuddiesAndPresence;
            public string URI;
            public string Name;
            public string Password;
            public string JoinAudio;
            public string JoinText;
            public string PasswordHashAlgorithm;
            public string SoundFilePath;
            public string Loop;
            public string SessionHandle;
            public string OrientationType;
            public VoicePosition SpeakerPosition;
            public VoicePosition ListenerPosition;
            public string ParticipantURI;
            public string Volume;
        }

        public class VoicePosition
        {
            /// <summary>Positional vector of the users position</summary>
            public Vector3d Position;
            /// <summary>Velocity vector of the position</summary>
            public Vector3d Velocity;
            /// <summary>At Orientation (X axis) of the position</summary>
            public Vector3d AtOrientation;
            /// <summary>Up Orientation (Y axis) of the position</summary>
            public Vector3d UpOrientation;
            /// <summary>Left Orientation (Z axis) of the position</summary>
            public Vector3d LeftOrientation;
        }

        #endregion XML Serialization Classes
    }
}
