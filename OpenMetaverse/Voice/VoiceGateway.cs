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
                    
                    if (Environment.OSVersion.Platform == PlatformID.Unix)
                    {
                        string ldPath = string.Empty;
                        try
                        {
                            ldPath = Environment.GetEnvironmentVariable("LD_LIBRARY_PATH");
                        }
                        catch { }
                        string newLdPath = daemonProcess.StartInfo.WorkingDirectory;
                        if (!string.IsNullOrEmpty(ldPath))
                            newLdPath += ":" + ldPath;
                        daemonProcess.StartInfo.EnvironmentVariables.Add("LD_LIBRARY_PATH", newLdPath);
                    }

                    Logger.DebugLog("Voice folder: " + daemonProcess.StartInfo.WorkingDirectory);
                    Logger.DebugLog(path + " " + args);
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
                        Thread.Sleep(2000);
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

#if DEBUG
                Logger.Log("Request: " + sb.ToString(), Helpers.LogLevel.Debug);
#endif
                try
                {
                    daemonPipe.SendData(Encoding.ASCII.GetBytes(sb.ToString()));
                }
                catch
                {
                    returnId = -1;
                }

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
#if DEBUG
            Logger.Log(line, Helpers.LogLevel.Debug);
#endif

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

                ResponseType genericResponse = ResponseType.None;

                switch (rsp.Action)
                {
                    // These first responses carry useful information beyond simple status,
                    // so they each have a specific Event.
                    case "Connector.Create.1":
                        if (OnConnectorCreateResponse != null)
                        {
                            OnConnectorCreateResponse(
                                rsp.InputXml.Request,
                                new VoiceConnectorEventArgs(
                                    int.Parse(rsp.ReturnCode),
                                    int.Parse(rsp.Results.StatusCode),
                                    rsp.Results.StatusString,
                                    rsp.Results.VersionID,
                                    rsp.Results.ConnectorHandle));
                        }
                        break;
                    case "Aux.GetCaptureDevices.1":
                        inputDevices = new List<string>();
                        foreach (CaptureDevice device in rsp.Results.CaptureDevices)
                            inputDevices.Add(device.Device);
                        currentCaptureDevice = rsp.Results.CurrentCaptureDevice.Device;
 
                        if (OnAuxGetCaptureDevicesResponse != null && rsp.Results.CaptureDevices.Count > 0)
                        {
                            OnAuxGetCaptureDevicesResponse(
                                rsp.InputXml.Request,
                                new VoiceDevicesEventArgs(
                                    ResponseType.GetCaptureDevices,
                                    int.Parse(rsp.ReturnCode),
                                    int.Parse(rsp.Results.StatusCode),
                                    rsp.Results.StatusString,
                                    rsp.Results.CurrentCaptureDevice.Device,
                                    inputDevices));
                        }
                        break;
                    case "Aux.GetRenderDevices.1":
                        outputDevices = new List<string>();
                        foreach (RenderDevice device in rsp.Results.RenderDevices)
                            outputDevices.Add(device.Device);
                        currentPlaybackDevice = rsp.Results.CurrentRenderDevice.Device;

                        if (OnAuxGetRenderDevicesResponse != null)
                        {
                            OnAuxGetRenderDevicesResponse(
                                rsp.InputXml.Request,
                                new VoiceDevicesEventArgs(
                                    ResponseType.GetCaptureDevices,
                                    int.Parse(rsp.ReturnCode),
                                    int.Parse(rsp.Results.StatusCode),
                                    rsp.Results.StatusString,
                                    rsp.Results.CurrentRenderDevice.Device,
                                    outputDevices));
                        }
                        break;

                    case "Account.Login.1":
                        if (OnAccountLoginResponse != null)
                        {
                            OnAccountLoginResponse(rsp.InputXml.Request,
                                new VoiceAccountEventArgs(
                                    int.Parse(rsp.ReturnCode),
                                    int.Parse(rsp.Results.StatusCode),
                                    rsp.Results.StatusString,
                                    rsp.Results.AccountHandle));
                        }
                        break;

                    case "Session.Create.1":
                        if (OnSessionCreateResponse != null)
                        {
                            OnSessionCreateResponse(
                                rsp.InputXml.Request,
                                new VoiceSessionEventArgs(
                                    int.Parse(rsp.ReturnCode),
                                    int.Parse(rsp.Results.StatusCode),
                                    rsp.Results.StatusString,
                                    rsp.Results.SessionHandle));
                        }
                        break;

                    // All the remaining responses below this point just report status,
                    // so they all share the same Event.  Most are useful only for
                    // detecting coding errors.
                    case "Connector.InitiateShutdown.1":
                        genericResponse = ResponseType.ConnectorInitiateShutdown;
                        break;
                    case "Aux.SetRenderDevice.1":
                        genericResponse = ResponseType.SetRenderDevice;
                        break;
                    case "Connector.MuteLocalMic.1":
                        genericResponse = ResponseType.MuteLocalMic;
                        break;
                    case "Connector.MuteLocalSpeaker.1":
                        genericResponse = ResponseType.MuteLocalSpeaker;
                        break;
                    case "Connector.SetLocalMicVolume.1":
                        genericResponse = ResponseType.SetLocalMicVolume;
                        break;
                    case "Connector.SetLocalSpeakerVolume.1":
                        genericResponse = ResponseType.SetLocalSpeakerVolume;
                        break;
                    case "Aux.SetCaptureDevice.1":
                        genericResponse = ResponseType.SetCaptureDevice;
                        break;
                    case "Session.RenderAudioStart.1":
                        genericResponse = ResponseType.RenderAudioStart;
                        break;
                    case "Session.RenderAudioStop.1":
                        genericResponse = ResponseType.RenderAudioStop;
                        break;
                    case "Aux.CaptureAudioStart.1":
                        genericResponse = ResponseType.CaptureAudioStart;
                        break;
                    case "Aux.CaptureAudioStop.1":
                        genericResponse = ResponseType.CaptureAudioStop;
                        break;
                    case "Aux.SetMicLevel.1":
                        genericResponse = ResponseType.SetMicLevel;
                        break;
                    case "Aux.SetSpeakerLevel.1":
                        genericResponse = ResponseType.SetSpeakerLevel;
                        break;
                    case "Account.Logout.1":
                        genericResponse = ResponseType.AccountLogout;
                        break;
                    case "Session.Connect.1":
                        genericResponse = ResponseType.SessionConnect;
                        break;
                    case "Session.Terminate.1":
                        genericResponse = ResponseType.SessionTerminate;
                        break;
                    case "Session.SetParticipantVolumeForMe.1":
                        genericResponse = ResponseType.SetParticipantVolumeForMe;
                        break;
                    case "Session.SetParticipantMuteForMe.1":
                        genericResponse = ResponseType.SetParticipantMuteForMe;
                        break;
                    case "Session.Set3DPosition.1":
                        genericResponse = ResponseType.Set3DPosition;
                        break;
                    default:
                        Logger.Log("Unimplemented response from the voice daemon: " + line, Helpers.LogLevel.Error);
                        break;
                }

                // Send the Response Event for all the simple cases.
                if (genericResponse != ResponseType.None && OnVoiceResponse != null)
                {
                    OnVoiceResponse(rsp.InputXml.Request,
                        new VoiceResponseEventArgs(
                            genericResponse,
                            int.Parse(rsp.ReturnCode),
                            int.Parse(rsp.Results.StatusCode),
                            rsp.Results.StatusString));
                }
            }
            else if (line.Substring(0, 7) == "<Event ")
            {
                VoiceEvent evt = null;
                try
                {
                    evt = (VoiceEvent)EventSerializer.Deserialize(new StringReader(line));
                }
                catch (Exception e)
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
                            OnAccountLoginStateChangeEvent(this,
                                new AccountLoginStateChangeEventArgs(
                                    evt.AccountHandle,
                                    int.Parse(evt.StatusCode),
                                    evt.StatusString,
                                    (LoginState)int.Parse(evt.State)));
                        }
                        break;

                    case "SessionNewEvent":
                        if (OnSessionNewEvent != null)
                        {
                            OnSessionNewEvent(this,
                                new NewSessionEventArgs(
                                    evt.AccountHandle,
                                    evt.SessionHandle,
                                    evt.URI,
                                    bool.Parse(evt.IsChannel),
                                    evt.Name,
                                    evt.AudioMedia));
                        }
                        break;

                    case "SessionStateChangeEvent":
                        if (OnSessionStateChangeEvent != null)
                        {
                            OnSessionStateChangeEvent(this,
                                new SessionStateChangeEventArgs(
                                    evt.SessionHandle,
                                    int.Parse(evt.StatusCode),
                                    evt.StatusString,
                                    (SessionState)int.Parse(evt.State),
                                    evt.URI,
                                    bool.Parse(evt.IsChannel),
                                    evt.ChannelName));
                        }
                        break;

                    case "ParticipantAddedEvent":
                        Logger.Log("Add participant " + evt.ParticipantUri, Helpers.LogLevel.Debug);
                        if (OnSessionParticipantAddedEvent != null)
                        {
                            OnSessionParticipantAddedEvent(this,
                                new ParticipantAddedEventArgs(
                                    evt.SessionGroupHandle,
                                    evt.SessionHandle,
                                    evt.ParticipantUri,
                                    evt.AccountName,
                                    evt.DisplayName,
                                    (ParticipantType)int.Parse(evt.ParticipantType),
                                    evt.Application));
                        }
                        break;

                    case "ParticipantRemovedEvent":
                        if (OnSessionParticipantRemovedEvent != null)
                        {
                            OnSessionParticipantRemovedEvent(this,
                                new ParticipantRemovedEventArgs(
                                    evt.SessionGroupHandle,
                                    evt.SessionHandle,
                                    evt.ParticipantUri,
                                    evt.AccountName,
                                    evt.Reason));
                        }
                        break;

                    case "ParticipantStateChangeEvent":
                        // Useful in person-to-person calls
                        if (OnSessionParticipantStateChangeEvent != null)
                        {
                            OnSessionParticipantStateChangeEvent(this,
                                new ParticipantStateChangeEventArgs(
                                    evt.SessionHandle,
                                    int.Parse(evt.StatusCode),
                                    evt.StatusString,
                                    (ParticipantState)int.Parse(evt.State), // Ringing, Connected, etc
                                    evt.ParticipantUri,
                                    evt.AccountName,
                                    evt.DisplayName,
                                    (ParticipantType)int.Parse(evt.ParticipantType)));
                        }
                        break;

                    case "ParticipantPropertiesEvent":
                        if (OnSessionParticipantPropertiesEvent != null)
                        {
                            OnSessionParticipantPropertiesEvent(this,
                                new ParticipantPropertiesEventArgs(
                                    evt.SessionHandle,
                                    evt.ParticipantUri,
                                    bool.Parse(evt.IsLocallyMuted),
                                    bool.Parse(evt.IsModeratorMuted),
                                    bool.Parse(evt.IsSpeaking),
                                    int.Parse(evt.Volume),
                                    float.Parse(evt.Energy)));
                        }
                        break;

                    case "ParticipantUpdatedEvent":
                        if (OnSessionParticipantUpdatedEvent != null)
                        {
                            OnSessionParticipantUpdatedEvent(this,
                                new ParticipantUpdatedEventArgs(
                                    evt.SessionHandle,
                                    evt.ParticipantUri,
                                    bool.Parse(evt.IsModeratorMuted),
                                    bool.Parse(evt.IsSpeaking),
                                    int.Parse(evt.Volume),
                                    float.Parse(evt.Energy)));
                        }
                        break;

                    case "SessionGroupAddedEvent":
                        if (OnSessionGroupAddedEvent != null)
                        {
                            OnSessionGroupAddedEvent(this,
                                new SessionGroupAddedEventArgs(
                                    evt.AccountHandle,
                                    evt.SessionGroupHandle,
                                    evt.Type));
                        }
                        break;

                    case "SessionAddedEvent":
                        if (OnSessionAddedEvent != null)
                        {
                            OnSessionAddedEvent(this,
                                new SessionAddedEventArgs(
                                    evt.SessionGroupHandle,
                                    evt.SessionHandle,
                                    evt.Uri,
                                    bool.Parse(evt.IsChannel),
                                    bool.Parse(evt.Incoming)));
                        }
                        break;

                    case "SessionRemovedEvent":
                        if (OnSessionRemovedEvent != null)
                        {
                            OnSessionRemovedEvent(this,
                                new SessionRemovedEventArgs(
                                    evt.SessionGroupHandle,
                                    evt.SessionHandle,
                                    evt.Uri));
                        }
                        break;

                    case "SessionUpdatedEvent":
                        if (OnSessionRemovedEvent != null)
                        {
                            OnSessionUpdatedEvent(this,
                                new SessionUpdatedEventArgs(
                                    evt.SessionGroupHandle,
                                    evt.SessionHandle,
                                    evt.Uri,
                                    int.Parse(evt.IsMuted) != 0,
                                    int.Parse(evt.Volume),
                                    int.Parse(evt.TransmitEnabled) != 0,
+                                   int.Parse(evt.IsFocused) != 0));
                        }
                        break;

                    case "AuxAudioPropertiesEvent":
                        if (OnAuxAudioPropertiesEvent != null)
                        {
                            OnAuxAudioPropertiesEvent(this,
                                new AudioPropertiesEventArgs(
                                    bool.Parse(evt.MicIsActive),
                                    float.Parse(evt.MicEnergy),
                                    int.Parse(evt.MicVolume),
                                    int.Parse(evt.SpeakerVolume)));
                        }
                        break;

                    case "SessionMediaEvent":
                        if (OnSessionMediaEvent != null)
                        {
                            OnSessionMediaEvent(this,
                                new SessionMediaEventArgs(
                                    evt.SessionHandle,
                                    bool.Parse(evt.HasText),
                                    bool.Parse(evt.HasAudio),
                                    bool.Parse(evt.HasVideo),
                                    bool.Parse(evt.Terminated)));
                        }
                        break;

                    case "BuddyAndGroupListChangedEvent":
                        // TODO   * <AccountHandle>c1_m1000xrjiQgi95QhCzH_D6ZJ8c5A==</AccountHandle><Buddies /><Groups />
                        break;

                    case "MediaStreamUpdatedEvent":
                        // TODO <SessionGroupHandle>c1_m1000xrjiQgi95QhCzH_D6ZJ8c5A==_sg0</SessionGroupHandle>
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
