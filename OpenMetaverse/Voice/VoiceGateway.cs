using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;
using System.Text;

namespace OpenMetaverse.Voice
{
    public partial class VoiceGateway
    {
        public delegate void DaemonRunningCallback();
        public delegate void DaemonExitedCallback();
        public delegate void DaemonCouldntRunCallback();
        public delegate void DaemonConnectedCallback();
        public delegate void DaemonDisconnectedCallback();
        public delegate void DaemonCouldntConnectCallback();
        
        public event DaemonRunningCallback OnDaemonRunning;
        public event DaemonExitedCallback OnDaemonExited;
        public event DaemonCouldntRunCallback OnDaemonCouldntRun;
        public event DaemonConnectedCallback OnDaemonConnected;
        public event DaemonDisconnectedCallback OnDaemonDisconnected;
        public event DaemonCouldntConnectCallback OnDaemonCouldntConnect;

        public bool DaemonIsRunning { get { return daemonIsRunning; } }
        public bool DaemonIsConnected { get { return daemonIsConnected; } }
        public int RequestId { get { return requestId; } }

        protected Process daemonProcess;
        protected ManualResetEvent daemonLoopSignal = new ManualResetEvent(false);
        protected TCPPipe daemonPipe;
        protected bool daemonIsRunning = false;
        protected bool daemonIsConnected = false;
        protected int requestId = 0;

        #region Daemon Management

        /// <summary>
        /// Starts a thread that keeps the daemon running
        /// </summary>
        /// <param name="path"></param>
        /// <param name="args"></param>
        public void StartDaemon(string path, string args)
        {
            StopDaemon();
            daemonLoopSignal.Set();

            Thread thread = new Thread(new ThreadStart(delegate()
            {
                while (daemonLoopSignal.WaitOne(500, false))
                {
                    daemonProcess = new Process();
                    daemonProcess.StartInfo.FileName = path;
                    daemonProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(path);
                    daemonProcess.StartInfo.Arguments = args;
                    daemonProcess.StartInfo.UseShellExecute = false;

                    bool ok = true;

                    if (!File.Exists(path))
                        ok = false;

                    if (ok)
                    {
                        // Attempt to start the process
                        if (!daemonProcess.Start())
                            ok = false;
                    }

                    if (!ok)
                    {
                        daemonIsRunning = false;
                        daemonLoopSignal.Reset();

                        if (OnDaemonCouldntRun != null)
                        {
                            try { OnDaemonCouldntRun(); }
                            catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, null, e); }
                        }

                        return;
                    }
                    else
                    {
                        daemonIsRunning = true;
                        if (OnDaemonRunning != null)
                        {
                            try { OnDaemonRunning(); }
                            catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, null, e); }
                        }

                        Logger.DebugLog("Started voice daemon, waiting for exit...");
                        daemonProcess.WaitForExit();
                        Logger.DebugLog("Voice daemon exited");
                        daemonIsRunning = false;

                        if (OnDaemonExited != null)
                        {
                            try { OnDaemonExited(); }
                            catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, null, e); }
                        }
                    }
                }
            }));

            thread.Start();
        }

        /// <summary>
        /// Stops the daemon and the thread keeping it running
        /// </summary>
        public void StopDaemon()
        {
            daemonLoopSignal.Reset();
            if (daemonProcess != null)
            {
                try
                {
                    daemonProcess.Kill();
                }
                catch (InvalidOperationException ex)
                {
                    Logger.Log("Failed to stop the voice daemon", Helpers.LogLevel.Error, ex);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool ConnectToDaemon(string address, int port)
        {
            daemonIsConnected = false;

            daemonPipe = new TCPPipe();
            daemonPipe.OnDisconnected +=
                delegate(SocketException e)
                {
                    if (OnDaemonDisconnected != null)
                    {
                        try { OnDaemonDisconnected(); }
                        catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, null, ex); }
                    }
                };
            daemonPipe.OnReceiveLine += new TCPPipe.OnReceiveLineCallback(daemonPipe_OnReceiveLine);
            
            SocketException se = daemonPipe.Connect(address, port);
            if (se == null)
            {
                daemonIsConnected = true;

                if (OnDaemonConnected != null)
                {
                    try { OnDaemonConnected(); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, null, e); }
                }

                return true;
            }
            else
            {
                daemonIsConnected = false;

                if (OnDaemonCouldntConnect != null)
                {
                    try { OnDaemonCouldntConnect(); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, null, e); }
                }

                Logger.Log("Voice daemon connection failed: " + se.Message, Helpers.LogLevel.Error);
                return false;
            }
        }

        #endregion Daemon Management

        public int Request(string action)
        {
            return Request(action, null);
        }

        public int Request(string action, string requestXML)
        {
            int returnId = requestId;
            if (daemonIsConnected)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(String.Format("<Request requestId=\"{0}\" action=\"{1}\"", requestId++, action));
                if (string.IsNullOrEmpty(requestXML))
                {
                    sb.Append(" />");
                }
                else
                {
                    sb.Append(">");
                    sb.Append(requestXML);
                    sb.Append("</Request>");
                }
                sb.Append("\n\n\n");

 //               Logger.Log(sb, Helpers.LogLevel.Info);
                daemonPipe.SendData(Encoding.ASCII.GetBytes(sb.ToString()));
                return returnId;
            }
            else
            {
                return -1;
            }
        }

        public static string MakeXML(string name, string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Format("<{0} />", name);
            else
                return string.Format("<{0}>{1}</{0}>", name, text);
        }

        private void daemonPipe_OnReceiveLine(string line)
        {
 //           Logger.Log(line, Helpers.LogLevel.Info);
            if (line.Substring(0, 10) == "<Response ")
            {
                VoiceResponse rsp = null;
                try
                {
                    rsp = (VoiceResponse)ResponseSerializer.Deserialize(new StringReader(line));
                }
                catch (Exception e)
                {
                    Logger.Log("Failed to deserialize voice daemon response", Helpers.LogLevel.Error, e);
                    return;
                }

                switch (rsp.Action)
                {
                    case "Connector.Create.1":
                        if (OnConnectorCreateResponse != null)
                        {
                            OnConnectorCreateResponse(int.Parse(rsp.ReturnCode), rsp.Results.VersionID, int.Parse(rsp.Results.StatusCode), rsp.Results.StatusString, rsp.Results.ConnectorHandle, rsp.InputXml.Request);
                        }
                        break;
                    case "Connector.InitiateShutdown.1":
                        if (OnConnectorInitiateShutdownResponse != null)
                        {
                            OnConnectorInitiateShutdownResponse(int.Parse(rsp.ReturnCode), int.Parse(rsp.Results.StatusCode), rsp.Results.StatusString, rsp.InputXml.Request);
                        }
                        break;
                    case "Connector.MuteLocalMic.1":
                        if (OnConnectorMuteLocalMicResponse != null)
                        {
                            OnConnectorMuteLocalMicResponse(int.Parse(rsp.ReturnCode), int.Parse(rsp.Results.StatusCode), rsp.Results.StatusString, rsp.InputXml.Request);
                        }
                        break;
                    case "Connector.MuteLocalSpeaker.1":
                        if (OnConnectorMuteLocalSpeakerResponse != null)
                        {
                            OnConnectorMuteLocalSpeakerResponse(int.Parse(rsp.ReturnCode), int.Parse(rsp.Results.StatusCode), rsp.Results.StatusString, rsp.InputXml.Request);
                        }
                        break;
                    case "Connector.SetLocalMicVolume.1":
                        if (OnConnectorSetLocalMicVolumeResponse != null)
                        {
                            OnConnectorSetLocalMicVolumeResponse(int.Parse(rsp.ReturnCode), int.Parse(rsp.Results.StatusCode), rsp.Results.StatusString, rsp.InputXml.Request);
                        }
                        break;
                    case "Connector.SetLocalSpeakerVolume.1":
                        if (OnConnectorSetLocalSpeakerVolumeResponse != null)
                        {
                            OnConnectorSetLocalSpeakerVolumeResponse(int.Parse(rsp.ReturnCode),
                                int.Parse(rsp.Results.StatusCode),
                                rsp.Results.StatusString,
                                rsp.InputXml.Request);
                        }
                        break;
                    case "Aux.GetCaptureDevices.1":
                        if (OnAuxGetCaptureDevicesResponse != null && rsp.Results.CaptureDevices.Count > 0)
                        {
                            List<string> CaptureDevices = new List<string>();
                            foreach (CaptureDevice device in rsp.Results.CaptureDevices)
                                CaptureDevices.Add(device.Device);

                            OnAuxGetCaptureDevicesResponse(int.Parse(rsp.ReturnCode),
                                int.Parse(rsp.Results.StatusCode),
                                rsp.Results.StatusString,
                                CaptureDevices,
                                rsp.Results.CurrentCaptureDevice.Device,
                                rsp.InputXml.Request);
                        }
                        break;
                    case "Aux.GetRenderDevices.1":
                        if (OnAuxGetRenderDevicesResponse != null)
                        {
                            List<string> RenderDevices = new List<string>();
                            foreach (RenderDevice device in rsp.Results.RenderDevices)
                                RenderDevices.Add(device.Device);

                            OnAuxGetRenderDevicesResponse(int.Parse(rsp.ReturnCode), int.Parse(rsp.Results.StatusCode), rsp.Results.StatusString, RenderDevices, rsp.Results.CurrentRenderDevice.Device, rsp.InputXml.Request);
                        }
                        break;
                    case "Aux.SetRenderDevice.1":
                        if (OnAuxSetRenderDeviceResponse != null)
                        {
                            OnAuxSetRenderDeviceResponse(int.Parse(rsp.ReturnCode), int.Parse(rsp.Results.StatusCode), rsp.Results.StatusString, rsp.InputXml.Request);
                        }
                        break;
                    case "Aux.SetCaptureDevice.1":
                        if (OnAuxSetCaptureDeviceResponse != null)
                        {
                            OnAuxSetCaptureDeviceResponse(int.Parse(rsp.ReturnCode), int.Parse(rsp.Results.StatusCode), rsp.Results.StatusString, rsp.InputXml.Request);
                        }
                        break;
                    case "Session.RenderAudioStart.1":
                        if (OnSessionRenderAudioStartResponse != null)
                        {
                            OnSessionRenderAudioStartResponse(int.Parse(rsp.ReturnCode), int.Parse(rsp.Results.StatusCode), rsp.Results.StatusString, rsp.InputXml.Request);
                        }
                        break;
                    case "Session.RenderAudioStop.1":
                        if (OnSessionRenderAudioStopResponse != null)
                        {
                            OnSessionRenderAudioStopResponse(int.Parse(rsp.ReturnCode), int.Parse(rsp.Results.StatusCode), rsp.Results.StatusString, rsp.InputXml.Request);
                        }
                        break;
                    case "Aux.CaptureAudioStart.1":
                        if (OnAuxCaptureAudioStartResponse != null)
                        {
                            OnAuxCaptureAudioStartResponse(int.Parse(rsp.ReturnCode), int.Parse(rsp.Results.StatusCode), rsp.Results.StatusString, rsp.InputXml.Request);
                        }
                        break;
                    case "Aux.CaptureAudioStop.1":
                        if (OnAuxCaptureAudioStopResponse != null)
                        {
                            OnAuxCaptureAudioStopResponse(int.Parse(rsp.ReturnCode), int.Parse(rsp.Results.StatusCode), rsp.Results.StatusString, rsp.InputXml.Request);
                        }
                        break;
                    case "Aux.SetMicLevel.1":
                        if (OnAuxSetMicLevelResponse != null)
                        {
                            OnAuxSetMicLevelResponse(int.Parse(rsp.ReturnCode), int.Parse(rsp.Results.StatusCode), rsp.Results.StatusString, rsp.InputXml.Request);
                        }
                        break;
                    case "Aux.SetSpeakerLevel.1":
                        if (OnAuxSetSpeakerLevelResponse != null)
                        {
                            OnAuxSetSpeakerLevelResponse(int.Parse(rsp.ReturnCode), int.Parse(rsp.Results.StatusCode), rsp.Results.StatusString, rsp.InputXml.Request);
                        }
                        break;
                    case "Account.Login.1":
                        if (OnAccountLoginResponse != null)
                        {
                            OnAccountLoginResponse(int.Parse(rsp.ReturnCode), int.Parse(rsp.Results.StatusCode), rsp.Results.StatusString, rsp.Results.AccountHandle, rsp.InputXml.Request);
                        }
                        break;
                    case "Account.Logout.1":
                        if (OnAccountLogoutResponse != null)
                        {
                            OnAccountLogoutResponse(int.Parse(rsp.ReturnCode), int.Parse(rsp.Results.StatusCode), rsp.Results.StatusString, rsp.InputXml.Request);
                        }
                        break;
                    case "Session.Create.1":
                        if (OnSessionCreateResponse != null)
                        {
                            OnSessionCreateResponse(int.Parse(rsp.ReturnCode), int.Parse(rsp.Results.StatusCode), rsp.Results.StatusString, rsp.Results.SessionHandle, rsp.InputXml.Request);
                        }
                        break;
                    case "Session.Connect.1":
                        if (OnSessionConnectResponse != null)
                        {
                            OnSessionConnectResponse(int.Parse(rsp.ReturnCode), int.Parse(rsp.Results.StatusCode), rsp.Results.StatusString, rsp.InputXml.Request);
                        }
                        break;
                    case "Session.Terminate.1":
                        if (OnSessionTerminateResponse != null)
                        {
                            OnSessionTerminateResponse(int.Parse(rsp.ReturnCode), int.Parse(rsp.Results.StatusCode), rsp.Results.StatusString, rsp.InputXml.Request);
                        }
                        break;
                    case "Session.SetParticipantVolumeForMe.1":
                        if (OnSessionSetParticipantVolumeForMeResponse != null)
                        {
                            OnSessionSetParticipantVolumeForMeResponse(int.Parse(rsp.ReturnCode), int.Parse(rsp.Results.StatusCode), rsp.Results.StatusString, rsp.InputXml.Request);
                        }
                        break;
                    case "Session.Set3DPosition.1":
                        if (OnSessionSetPositionResponse != null)
                        {
                            OnSessionSetPositionResponse(
                                int.Parse(rsp.ReturnCode),
                                int.Parse(rsp.Results.StatusCode),
                                rsp.Results.StatusString,
                                rsp.InputXml.Request);
                        }
                        break;
                default:
                    Logger.Log("Unimplemented response from the voice daemon: " + line, Helpers.LogLevel.Error);
                    break;
            }
        }
        else if (line.Substring(0, 7) == "<Event ")
        {
            VoiceEvent evt = null;
            try
            {
                evt = (VoiceEvent)EventSerializer.Deserialize(new StringReader(line));
            }
            catch(Exception e)
            {
                Logger.Log("Failed to deserialize voice daemon event", Helpers.LogLevel.Error, e);
                return;
            }

            switch (evt.Type)
            {
                case "LoginStateChangeEvent":
                case "AccountLoginStateChangeEvent":
                    if (OnAccountLoginStateChangeEvent != null)
                    {
                        OnAccountLoginStateChangeEvent(evt.AccountHandle,
                            int.Parse(evt.StatusCode),
                            evt.StatusString,
                            (LoginState)int.Parse(evt.State));
                    }
                    break;
                case "SessionNewEvent":
                    if (OnSessionNewEvent != null)
                    {
                        OnSessionNewEvent(evt.AccountHandle,
                            evt.SessionHandle,
                            evt.URI,
                            bool.Parse(evt.IsChannel),
                            evt.Name,
                            evt.AudioMedia);
                    }
                    break;
                case "SessionStateChangeEvent":
                    if (OnSessionStateChangeEvent != null)
                    {
                        OnSessionStateChangeEvent(evt.SessionHandle,
                            int.Parse(evt.StatusCode), evt.StatusString,
                            (SessionState)int.Parse(evt.State),
                            evt.URI,
                            bool.Parse(evt.IsChannel),
                            evt.ChannelName);
                    }
                    break;
                case "ParticipantAddedEvent":
                    if (OnSessionParticipantAddedEvent != null)
                    {
                       OnSessionParticipantAddedEvent(
                            evt.SessionGroupHandle,
                            evt.SessionHandle,
                            evt.ParticipantUri,
                            evt.AccountName,
                            evt.DisplayName,
                            (ParticipantType)int.Parse(evt.ParticipantType),
                            evt.Application);
                    }
                    break;

                case "ParticipantRemovedEvent":
                    if (OnSessionParticipantRemovedEvent != null)
                    {
                        OnSessionParticipantRemovedEvent(
                            evt.SessionGroupHandle,
                            evt.SessionHandle,
                            evt.ParticipantUri,
                            evt.AccountName,
                            evt.Reason);
                    }
                    break;

                case "ParticipantStateChangeEvent":
                    if (OnSessionParticipantStateChangeEvent != null)
                    {
                        OnSessionParticipantStateChangeEvent(evt.SessionHandle,
                            int.Parse(evt.StatusCode),
                            evt.StatusString,
                            (ParticipantState)int.Parse(evt.State),
                            evt.ParticipantUri,
                            evt.AccountName,
                            evt.DisplayName,
                            (ParticipantType)int.Parse(evt.ParticipantType));
                    }
                    break;
                case "ParticipantPropertiesEvent":
                    if (OnSessionParticipantPropertiesEvent != null)
                    {
                        OnSessionParticipantPropertiesEvent(evt.SessionHandle,
                            evt.ParticipantUri,
                            bool.Parse(evt.IsLocallyMuted),
                            bool.Parse(evt.IsModeratorMuted),
                            bool.Parse(evt.IsSpeaking),
                            int.Parse(evt.Volume),
                            float.Parse(evt.Energy));
                    }
                    break;
                case "ParticipantUpdatedEvent":
                    if (OnSessionParticipantUpdatedEvent != null)
                    {
                        OnSessionParticipantUpdatedEvent( evt.SessionHandle, evt.ParticipantUri,
                            bool.Parse(evt.IsModeratorMuted),
                            bool.Parse(evt.IsSpeaking),
                            int.Parse(evt.Volume),
                            float.Parse(evt.Energy));
                    }
                    break;
                case "SessionGroupAddedEvent":
                    if (OnSessionGroupAddedEvent != null)
                    {
                        OnSessionGroupAddedEvent(evt.AccountHandle, evt.SessionGroupHandle, evt.Type);
                    }
                    break;
                case "SessionAddedEvent":
                    if (OnSessionAddedEvent != null)
                    {
                        OnSessionAddedEvent( evt.SessionGroupHandle, evt.SessionHandle, evt.Uri,
                            bool.Parse( evt.IsChannel ),
                            bool.Parse( evt.Incoming ));
                    }
                    break;

                case "SessionRemovedEvent":
                    if (OnSessionRemovedEvent != null)
                    {
                        OnSessionRemovedEvent(
                            evt.SessionGroupHandle,
                            evt.SessionHandle,
                            evt.Uri);
                    }
                    break;
// <Response requestId="11" action="Session.Set3DPosition.1"><ReturnCode>1</ReturnCode><Results><StatusCode>1004</StatusCode><StatusString>This command is not valid for non-positional channels.</StatusString></Results><InputXml><Request requestId="11" action="Session.Set3DPosition.1"><SessionHandle>c1_m1000xrjiQgi95QhCzH_D6ZJ8c5A==0</SessionHandle><SpeakerPosition><Position><X>262048</X><Y>21.4914</Y><Z>-257907</Z></Position><Velocity><X>0</X><Y>0</Y><Z>0</Z></Velocity><AtOrientation><X>0</X><Y>0</Y><Z>1</Z></AtOrientation><UpOrientation><X>0</X><Y>1</Y><Z>0</Z></UpOrientation><LeftOrientation><X>-1</X><Y>0</Y><Z>0</Z></LeftOrientation></SpeakerPosition><ListenerPosition><Position><X>262048</X><Y>21.4914</Y><Z>-257907</Z></Position><Velocity><X>0</X><Y>0</Y><Z>0</Z></Velocity><AtOrientation><X>0</X><Y>0</Y><Z>1</Z></AtOrientation><UpOrientation><X>0</X><Y>1</Y><Z>0</Z></UpOrientation><LeftOrientation><X>-1</X><Y>0</Y><Z>0</Z></LeftOrientation></ListenerPosition></Request></InputXml></Response>

                case "SessionUpdatedEvent":
                    if (OnSessionRemovedEvent != null)
                    {
                        OnSessionUpdatedEvent(
                            evt.SessionGroupHandle,
                            evt.SessionHandle,
                            evt.Uri,
                            bool.Parse(evt.IsMuted),
                            int.Parse( evt.Volume ),
                            bool.Parse( evt.TransmitEnabled ),
                            bool.Parse( evt.IsFocused ));
                    }
                    break;

                case "AuxAudioPropertiesEvent":
                    if (OnAuxAudioPropertiesEvent != null)
                    {
                        OnAuxAudioPropertiesEvent(bool.Parse(evt.MicIsActive), float.Parse(evt.MicEnergy), float.Parse(evt.MicVolume), float.Parse(evt.SpeakerVolume));
                    }
                    break;
                case "SessionMediaEvent":
                    if (OnSessionMediaEvent != null)
                    {
                        OnSessionMediaEvent(evt.SessionHandle, bool.Parse(evt.HasText), bool.Parse(evt.HasAudio), bool.Parse(evt.HasVideo), bool.Parse(evt.Terminated));
                    }
                    break;
                
                case "BuddyAndGroupListChangedEvent":
                    //   * <AccountHandle>c1_m1000xrjiQgi95QhCzH_D6ZJ8c5A==</AccountHandle><Buddies /><Groups />
                    break;

                case "MediaStreamUpdatedEvent":
                    // <SessionGroupHandle>c1_m1000xrjiQgi95QhCzH_D6ZJ8c5A==_sg0</SessionGroupHandle>
                    // <SessionHandle>c1_m1000xrjiQgi95QhCzH_D6ZJ8c5A==0</SessionHandle>
                    //<StatusCode>0</StatusCode><StatusString /><State>1</State><Incoming>false</Incoming>

                    break;

                default:
                        Logger.Log("Unimplemented event from the voice daemon: " + line, Helpers.LogLevel.Error);
                        break;
                }
            }
            else
            {
                Logger.Log("Unrecognized data from the voice daemon: " + line, Helpers.LogLevel.Error);
            }
        }
    }
}
