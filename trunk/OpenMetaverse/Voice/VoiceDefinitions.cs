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

        public enum ResponseType
        {
            None = 0,
            ConnectorCreate,
            ConnectorInitiateShutdown,
            MuteLocalMic,
            MuteLocalSpeaker,
            SetLocalMicVolume,
            SetLocalSpeakerVolume,
            GetCaptureDevices,
            GetRenderDevices,
            SetRenderDevice,
            SetCaptureDevice,
            CaptureAudioStart,
            CaptureAudioStop,
            SetMicLevel,
            SetSpeakerLevel,
            AccountLogin,
            AccountLogout,
            RenderAudioStart,
            RenderAudioStop,
            SessionCreate,
            SessionConnect,
            SessionTerminate,
            SetParticipantVolumeForMe,
            SetParticipantMuteForMe,
            Set3DPosition
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

        public class VoiceResponseEventArgs : EventArgs
        {
            public readonly ResponseType Type;
            public readonly int ReturnCode;
            public readonly int StatusCode;
            public readonly string Message;

            // All Voice Response events carry these properties.
            public VoiceResponseEventArgs(ResponseType type, int rcode, int scode, string text)
            {
                this.Type = type;
                this.ReturnCode = rcode;
                this.StatusCode = scode;
                this.Message = text;
            }
        }

        #region Session Event Args
        public class VoiceSessionEventArgs : VoiceResponseEventArgs
        {
            public readonly string SessionHandle;

            public VoiceSessionEventArgs(int rcode, int scode, string text, string shandle) :
                base(ResponseType.SessionCreate, rcode, scode, text)
            {
                this.SessionHandle = shandle;
            }
        }

        public class NewSessionEventArgs : EventArgs
        {
            public readonly string AccountHandle;
            public readonly string SessionHandle;
            public readonly string URI;
            public readonly string Name;
            public readonly string AudioMedia;

            public NewSessionEventArgs(string AccountHandle, string SessionHandle, string URI, bool IsChannel, string Name, string AudioMedia)
            {
                this.AccountHandle = AccountHandle;
                this.SessionHandle = SessionHandle;
                this.URI = URI;
                this.Name = Name;
                this.AudioMedia = AudioMedia;
            }
        }

        public class SessionMediaEventArgs : EventArgs
        {
            public readonly string SessionHandle;
            public readonly bool HasText;
            public readonly bool HasAudio;
            public readonly bool HasVideo;
            public readonly bool Terminated;

            public SessionMediaEventArgs(string SessionHandle, bool HasText, bool HasAudio, bool HasVideo, bool Terminated)
            {
                this.SessionHandle = SessionHandle;
                this.HasText = HasText;
                this.HasAudio = HasAudio;
                this.HasVideo = HasVideo;
                this.Terminated = Terminated;
            }
        }

        public class SessionStateChangeEventArgs : EventArgs
        {
            public readonly string SessionHandle;
            public readonly int StatusCode;
            public readonly string StatusString;
            public readonly SessionState State;
            public readonly string URI;
            public readonly bool IsChannel;
            public readonly string ChannelName;
            public SessionStateChangeEventArgs(string SessionHandle, int StatusCode, string StatusString, SessionState State, string URI, bool IsChannel, string ChannelName)
            {
                this.SessionHandle = SessionHandle;
                this.StatusCode = StatusCode;
                this.StatusString = StatusString;
                this.State = State;
                this.URI = URI;
                this.IsChannel = IsChannel;
                this.ChannelName = ChannelName;
            }
        }

        // Participants
        public class ParticipantAddedEventArgs : EventArgs
        {
            public readonly string SessionHandle;
            public readonly string SessionGroupHandle;
            public readonly string URI;
            public readonly string AccountName;
            public readonly string DisplayName;
            public readonly ParticipantType Type;
            public readonly string Appllication;
            public ParticipantAddedEventArgs(
                    string SessionGroupHandle,
                    string SessionHandle,
                    string ParticipantUri,
                    string AccountName,
                    string DisplayName,
                    ParticipantType type,
                    string Application)
            {
                this.SessionGroupHandle = SessionGroupHandle;
                this.SessionHandle = SessionHandle;
                this.URI = ParticipantUri;
                this.AccountName = AccountName;
                this.DisplayName = DisplayName;
                this.Type = type;
                this.Appllication = Application;
            }
        }

        public class ParticipantRemovedEventArgs : EventArgs
        {
            public readonly string SessionGroupHandle;
            public readonly string SessionHandle;
            public readonly string URI;
            public readonly string AccountName;
            public readonly string Reason;

            public ParticipantRemovedEventArgs(
                string SessionGroupHandle,
                string SessionHandle,
                string ParticipantUri,
                string AccountName,
                string Reason)
            {
                this.SessionGroupHandle = SessionGroupHandle;
                this.SessionHandle = SessionHandle;
                this.URI = ParticipantUri;
                this.AccountName = AccountName;
                this.Reason = Reason;
            }
        }

        public class ParticipantStateChangeEventArgs : EventArgs
        {
            public readonly string SessionHandle;
            public readonly int StatusCode;
            public readonly string StatusString;
            public readonly ParticipantState State;
            public readonly string URI;
            public readonly string AccountName;
            public readonly string DisplayName;
            public readonly ParticipantType Type;

            public ParticipantStateChangeEventArgs(string SessionHandle, int StatusCode, string StatusString,
                ParticipantState State, string ParticipantURI, string AccountName,
                string DisplayName, ParticipantType ParticipantType)
            {
                this.SessionHandle = SessionHandle;
                this.StatusCode = StatusCode;
                this.StatusString = StatusString;
                this.State = State;
                this.URI = ParticipantURI;
                this.AccountName = AccountName;
                this.DisplayName = DisplayName;
                this.Type = ParticipantType;
            }
        }

        public class ParticipantPropertiesEventArgs : EventArgs
        {
            public readonly string SessionHandle;
            public readonly string URI;
            public readonly bool IsLocallyMuted;
            public readonly bool IsModeratorMuted;
            public readonly bool IsSpeaking;
            public readonly int Volume;
            public readonly float Energy;

            public ParticipantPropertiesEventArgs(string SessionHandle, string ParticipantURI,
                bool IsLocallyMuted, bool IsModeratorMuted, bool IsSpeaking, int Volume, float Energy)
            {
                this.SessionHandle = SessionHandle;
                this.URI = ParticipantURI;
                this.IsLocallyMuted = IsLocallyMuted;
                this.IsModeratorMuted = IsModeratorMuted;
                this.IsSpeaking = IsSpeaking;
                this.Volume = Volume;
                this.Energy = Energy;
            }

        }

        public class ParticipantUpdatedEventArgs : EventArgs
        {
            public readonly string SessionHandle;
            public readonly string URI;
            public readonly bool IsMuted;
            public readonly bool IsSpeaking;
            public readonly int Volume;
            public readonly float Energy;

            public ParticipantUpdatedEventArgs(string sessionHandle, string URI, bool isMuted, bool isSpeaking, int volume, float energy)
            {
                this.SessionHandle = sessionHandle;
                this.URI = URI;
                this.IsMuted = isMuted;
                this.IsSpeaking = isSpeaking;
                this.Volume = volume;
                this.Energy = energy;
            }
        }

        public class SessionAddedEventArgs : EventArgs
        {
            public readonly string SessionGroupHandle;
            public readonly string SessionHandle;
            public readonly string URI;
            public readonly bool IsChannel;
            public readonly bool IsIncoming;

            public SessionAddedEventArgs(string sessionGroupHandle, string sessionHandle,
                string URI, bool isChannel, bool isIncoming)
            {
                this.SessionGroupHandle = sessionGroupHandle;
                this.SessionHandle = sessionHandle;
                this.URI = URI;
                this.IsChannel = isChannel;
                this.IsIncoming = isIncoming;
            }
        }

        public class SessionRemovedEventArgs : EventArgs
        {
            public readonly string SessionGroupHandle;
            public readonly string SessionHandle;
            public readonly string URI;
            public SessionRemovedEventArgs(
                string SessionGroupHandle,
                string SessionHandle,
                string Uri)
            {
                this.SessionGroupHandle = SessionGroupHandle;
                this.SessionHandle = SessionHandle;
                this.URI = Uri;
            }
        }

        public class SessionUpdatedEventArgs : EventArgs
        {
            public readonly string SessionGroupHandle;
            public readonly string SessionHandle;
            public readonly string URI;
            public readonly bool IsMuted;
            public readonly int Volume;
            public readonly bool TransmitEnabled;
            public readonly bool IsFocused;
            public SessionUpdatedEventArgs(string SessionGroupHandle,
                string SessionHandle, string URI, bool IsMuted, int Volume,
                bool TransmitEnabled, bool IsFocused)
            {
                this.SessionGroupHandle = SessionGroupHandle;
                this.SessionHandle = SessionHandle;
                this.URI = URI;
                this.IsMuted = IsMuted;
                this.Volume = Volume;
                this.TransmitEnabled = TransmitEnabled;
                this.IsFocused = IsFocused;
            }

        }

        public class SessionGroupAddedEventArgs : EventArgs
        {
            public readonly string AccountHandle;
            public readonly string SessionGroupHandle;
            public readonly string Type;
            public SessionGroupAddedEventArgs(string acctHandle, string sessionGroupHandle, string type)
            {
                this.AccountHandle = acctHandle;
                this.SessionGroupHandle = sessionGroupHandle;
                this.Type = type;
            }
        }
        #endregion Session Event Args

        #region Connector Delegates
        public class VoiceConnectorEventArgs : VoiceResponseEventArgs
        {
            private readonly string m_Version;
            private readonly string m_ConnectorHandle;
            public string Version { get { return m_Version; } }
            public string Handle { get { return m_ConnectorHandle; } }

            public VoiceConnectorEventArgs(int rcode, int scode, string text, string version, string handle) :
                base(ResponseType.ConnectorCreate, rcode, scode, text)
            {
                m_Version = version;
                m_ConnectorHandle = handle;
            }
        }

        #endregion Connector Delegates


        #region Aux Event Args
        public class VoiceDevicesEventArgs : VoiceResponseEventArgs
        {
            private readonly string m_CurrentDevice;
            private readonly List<string> m_Available;
            public string CurrentDevice { get { return m_CurrentDevice; } }
            public List<string> Devices { get { return m_Available; } }

            public VoiceDevicesEventArgs(ResponseType type, int rcode, int scode, string text, string current, List<string> avail) :
                base(type, rcode, scode, text)
            {
                m_CurrentDevice = current;
                m_Available = avail;
            }
        }


        /// Audio Properties Events are sent after audio capture is started. These events are used to display a microphone VU meter
        public class AudioPropertiesEventArgs : EventArgs
        {
            public readonly bool IsMicActive;
            public readonly float MicEnergy;
            public readonly int MicVolume;
            public readonly int SpeakerVolume;
            public AudioPropertiesEventArgs(bool MicIsActive, float MicEnergy, int MicVolume, int SpeakerVolume)
            {
                this.IsMicActive = MicIsActive;
                this.MicEnergy = MicEnergy;
                this.MicVolume = MicVolume;
                this.SpeakerVolume = SpeakerVolume;
            }
        }

        #endregion Aux Event Args

        #region Account Event Args
        public class VoiceAccountEventArgs : VoiceResponseEventArgs
        {
            private readonly string m_AccountHandle;
            public string AccountHandle { get { return m_AccountHandle; } }

            public VoiceAccountEventArgs(int rcode, int scode, string text, string ahandle) :
                base(ResponseType.AccountLogin, rcode, scode, text)
            {
                this.m_AccountHandle = ahandle;
            }
        }

        public class AccountLoginStateChangeEventArgs : EventArgs
        {
            public readonly string AccountHandle;
            public readonly int StatusCode;
            public readonly string StatusString;
            public readonly LoginState State;
            public AccountLoginStateChangeEventArgs(string AccountHandle, int StatusCode, string StatusString, LoginState State)
            {
                this.AccountHandle = AccountHandle;
                this.StatusCode = StatusCode;
                this.StatusString = StatusString;
                this.State = State;
            }
        }

        #endregion Account Event Args

        /// <summary>
        /// Event for most mundane request reposnses.
        /// </summary>
        public event EventHandler<VoiceResponseEventArgs> OnVoiceResponse;

        #region Session Events
        public event EventHandler<VoiceSessionEventArgs> OnSessionCreateResponse;
        public event EventHandler<NewSessionEventArgs> OnSessionNewEvent;
        public event EventHandler<SessionStateChangeEventArgs> OnSessionStateChangeEvent;
        public event EventHandler<ParticipantStateChangeEventArgs> OnSessionParticipantStateChangeEvent;
        public event EventHandler<ParticipantPropertiesEventArgs> OnSessionParticipantPropertiesEvent;
        public event EventHandler<ParticipantUpdatedEventArgs> OnSessionParticipantUpdatedEvent;
        public event EventHandler<ParticipantAddedEventArgs> OnSessionParticipantAddedEvent;
        public event EventHandler<ParticipantRemovedEventArgs> OnSessionParticipantRemovedEvent;
        public event EventHandler<SessionGroupAddedEventArgs> OnSessionGroupAddedEvent;
        public event EventHandler<SessionAddedEventArgs> OnSessionAddedEvent;
        public event EventHandler<SessionRemovedEventArgs> OnSessionRemovedEvent;
        public event EventHandler<SessionUpdatedEventArgs> OnSessionUpdatedEvent;
        public event EventHandler<SessionMediaEventArgs> OnSessionMediaEvent;
        #endregion Session Events

        #region Connector Events

        /// <summary>Response to Connector.Create request</summary>
        public event EventHandler<VoiceConnectorEventArgs> OnConnectorCreateResponse;

        #endregion Connector Events

        #region Aux Events

        /// <summary>Response to Aux.GetCaptureDevices request</summary>
        public event EventHandler<VoiceDevicesEventArgs> OnAuxGetCaptureDevicesResponse;
        /// <summary>Response to Aux.GetRenderDevices request</summary>
        public event EventHandler<VoiceDevicesEventArgs> OnAuxGetRenderDevicesResponse;

        /// <summary>Audio Properties Events are sent after audio capture is started.
        /// These events are used to display a microphone VU meter</summary>
        public event EventHandler<AudioPropertiesEventArgs> OnAuxAudioPropertiesEvent;

        #endregion Aux Events

        #region Account Events

        /// <summary>Response to Account.Login request</summary>
        public event EventHandler<VoiceAccountEventArgs> OnAccountLoginResponse;

        /// <summary>This event message is sent whenever the login state of the
        /// particular Account has transitioned from one value to another</summary>
        public event EventHandler<AccountLoginStateChangeEventArgs> OnAccountLoginStateChangeEvent;

        #endregion Account Events

        #region XML Serialization Classes

        private XmlSerializer EventSerializer = new XmlSerializer(typeof(VoiceEvent));
        private XmlSerializer ResponseSerializer = new XmlSerializer(typeof(VoiceResponse));

        [XmlRoot("Event")]
        public class VoiceEvent
        {
            [XmlAttribute("type")]
            public string Type;
            public string AccountHandle;
            public string Application;
            public string StatusCode;
            public string StatusString;
            public string State;
            public string SessionHandle;
            public string SessionGroupHandle;
            public string URI;
            public string Uri;  // Yes, they send it with both capitalizations
            public string IsChannel;
            public string IsIncoming;
            public string Incoming;
            public string IsMuted;
            public string Name;
            public string AudioMedia;
            public string ChannelName;
            public string ParticipantUri;
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
            public string Reason;
            public string TransmitEnabled;
            public string IsFocused;
        }

        [XmlRoot("Response")]
        public class VoiceResponse
        {
            [XmlAttribute("requestId")]
            public string RequestId;
            [XmlAttribute("action")]
            public string Action;
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
            [XmlAttribute("requestId")]
            public string RequestId;
            [XmlAttribute("action")]
            public string Action;
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

        #endregion XML Serialization Classes
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

}
