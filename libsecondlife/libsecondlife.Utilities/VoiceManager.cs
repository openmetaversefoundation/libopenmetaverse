using System;
using System.Net.Sockets;
using System.Text;
using libsecondlife;

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

    public class VoiceManager
    {
        public const string DAEMON_ARGS = " -p tcp -h -c -ll ";
        public const int DAEMON_LOG_LEVEL = 1;
        public const int DAEMON_PORT = 44124;
        public const string VOICE_RELEASE_SERVER = "bhr.vivox.com";
        public const string VOICE_DEBUG_SERVER = "bhd.vivox.com";
        public const string REQUEST_TERMINATOR = "\n\n\n";

        public SecondLife Client;

        protected TCPPipe _DaemonPipe;
        protected VoiceStatus _Status;
        protected int _CommandCookie = 0;
        protected string _TuningSoundFile = String.Empty;
        protected string _VoiceServer = VOICE_RELEASE_SERVER;

        public VoiceManager(SecondLife client)
        {
            Client = client;
        }

        public bool IsDaemonRunning()
        {
            return false;
        }

        public bool StartDaemon()
        {
            return false;
        }

        public void StopDaemon()
        {
        }

        public bool ConnectToDaemon()
        {
            return ConnectToDaemon("127.0.0.1", DAEMON_PORT);
        }

        public bool ConnectToDaemon(string address, int port)
        {
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
            return String.Format("sip:{0}@{1}", account, _VoiceServer);
        }

        public void RequestCaptureDevices()
        {
            if (_DaemonPipe.Connected)
            {
                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(String.Format(
                    "<Request requestId=\"{0}\" action=\"Aux.GetCaptureDevices.1\"></Request>{1}",
                    _CommandCookie++,
                    REQUEST_TERMINATOR)));
            }
            else
            {
                Client.Log("VoiceManager.RequestCaptureDevices() called when the daemon pipe is disconnected", Helpers.LogLevel.Error);
            }
        }

        public void RequestRenderDevices()
        {
            if (_DaemonPipe.Connected)
            {
                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(String.Format(
                    "<Request requestId=\"{0}\" action=\"Aux.GetRenderDevices.1\"></Request>{1}",
                    _CommandCookie++,
                    REQUEST_TERMINATOR)));
            }
            else
            {
                Client.Log("VoiceManager.RequestRenderDevices() called when the daemon pipe is disconnected", Helpers.LogLevel.Error);
            }
        }

        public void RequestCreateConnector()
        {
            RequestCreateConnector(VOICE_RELEASE_SERVER);
        }

        public void RequestCreateConnector(string voiceServer)
        {
            if (_DaemonPipe.Connected)
            {
                _VoiceServer = voiceServer;

                string accountServer = String.Format("https://www.{0}/api2/", _VoiceServer);
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
            }
            else
            {
                Client.Log("VoiceManager.CreateConnector() called when the daemon pipe is disconnected", Helpers.LogLevel.Error);
            }
        }

        public void RequestLogin(string accountName, string password, string connectorHandle)
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
            }
            else
            {
                Client.Log("VoiceManager.Login() called when the daemon pipe is disconnected", Helpers.LogLevel.Error);
            }
        }

        public void RequestSetRenderDevice(string deviceName)
        {
            if (_DaemonPipe.Connected)
            {
                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(String.Format(
                    "<Request requestId=\"{0}\" action=\"Aux.SetRenderDevice.1\"><RenderDeviceSpecifier>{1}</RenderDeviceSpecifier></Request>{2}",
                    _CommandCookie, deviceName, REQUEST_TERMINATOR)));
            }
            else
            {
                Client.Log("VoiceManager.RequestSetRenderDevice() called when the daemon pipe is disconnected", Helpers.LogLevel.Error);
            }
        }

        public void RequestStartTuningMode(int duration)
        {
            if (_DaemonPipe.Connected)
            {
                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(String.Format(
                    "<Request requestId=\"{0}\" action=\"Aux.CaptureAudioStart.1\"><Duration>{1}</Duration></Request>{2}",
                    _CommandCookie, duration, REQUEST_TERMINATOR)));
            }
            else
            {
                Client.Log("VoiceManager.RequestStartTuningMode() called when the daemon pipe is disconnected", Helpers.LogLevel.Error);
            }
        }

        public void RequestStopTuningMode()
        {
            if (_DaemonPipe.Connected)
            {
                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(String.Format(
                    "<Request requestId=\"{0}\" action=\"Aux.CaptureAudioStop.1\"></Request>{1}",
                    _CommandCookie, REQUEST_TERMINATOR)));
            }
            else
            {
                Client.Log("VoiceManager.RequestStopTuningMode() called when the daemon pipe is disconnected", Helpers.LogLevel.Error);
            }
        }

        public void RequestSetSpeakerVolume(int volume)
        {
            if (volume < 0 || volume > 100)
                throw new ArgumentException("volume must be between 0 and 100", "volume");

            if (_DaemonPipe.Connected)
            {
                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(String.Format(
                    "<Request requestId=\"{0}\" action=\"Aux.SetSpeakerLevel.1\"><Level>{1}</Level></Request>{2}",
                    _CommandCookie, volume, REQUEST_TERMINATOR)));
            }
            else
            {
                Client.Log("VoiceManager.RequestSetSpeakerVolume() called when the daemon pipe is disconnected", Helpers.LogLevel.Error);
            }
        }

        public void RequestSetCaptureVolume(int volume)
        {
            if (volume < 0 || volume > 100)
                throw new ArgumentException("volume must be between 0 and 100", "volume");

            if (_DaemonPipe.Connected)
            {
                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(String.Format(
                    "<Request requestId=\"{0}\" action=\"Aux.SetMicLevel.1\"><Level>{1}</Level></Request>{2}",
                    _CommandCookie, volume, REQUEST_TERMINATOR)));
            }
            else
            {
                Client.Log("VoiceManager.RequestSetCaptureVolume() called when the daemon pipe is disconnected", Helpers.LogLevel.Error);
            }
        }

        /// <summary>
        /// Does not appear to be working
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="loop"></param>
        public void RequestRenderAudioStart(string fileName, bool loop)
        {
            if (_DaemonPipe.Connected)
            {
                _TuningSoundFile = fileName;

                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(String.Format(
                    "<Request requestId=\"{0}\" action=\"Aux.RenderAudioStart.1\"><SoundFilePath>{1}</SoundFilePath><Loop>{2}</Loop></Request>{3}",
                    _CommandCookie++, _TuningSoundFile, (loop ? "1" : "0"), REQUEST_TERMINATOR)));
            }
            else
            {
                Client.Log("VoiceManager.RequestRenderAudioStart() called when the daemon pipe is disconnected", Helpers.LogLevel.Error);
            }
        }

        public void RequestRenderAudioStop()
        {
            if (_DaemonPipe.Connected)
            {
                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(String.Format(
                    "<Request requestId=\"{0}\" action=\"Aux.RenderAudioStop.1\"><SoundFilePath>{1}</SoundFilePath></Request>{2}",
                    _CommandCookie++, _TuningSoundFile, REQUEST_TERMINATOR)));
            }
            else
            {
                Client.Log("VoiceManager.RequestRenderAudioStop() called when the daemon pipe is disconnected", Helpers.LogLevel.Error);
            }
        }

        private void _DaemonPipe_OnDisconnected(SocketException se)
        {
            if (se != null) Console.WriteLine("Disconnected! " + se.Message);
            else Console.WriteLine("Disconnected!");
        }

        private void _DaemonPipe_OnReceiveLine(string line)
        {
            Client.DebugLog("VOICE: " + line);
        }
    }
}
