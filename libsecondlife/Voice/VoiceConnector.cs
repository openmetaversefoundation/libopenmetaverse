using System;
using System.Collections.Generic;
using System.Text;

namespace libsecondlife.Voice
{
    public partial class VoiceGateway
    {
        /// <summary>
        /// This is used to initialize and stop the Connector as a whole. The Connector
        /// Create call must be completed successfully before any other requests are made
        /// (typically during application initialization). The shutdown should be called
        /// when the application is shutting down to gracefully release resources
        /// </summary>
        /// <param name="ClientName">A string value indicting the Application name</param>
        /// <param name="AccountManagementServer">URL for the management server</param>
        /// <param name="Logging">LoggingSettings</param>
        public int ConnectorCreate(string ClientName, string AccountManagementServer, ushort MinimumPort,
            ushort MaximumPort, VoiceLoggingSettings Logging)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(VoiceGateway.MakeXML("ClientName", ClientName));
            sb.Append(VoiceGateway.MakeXML("AccountManagementServer", AccountManagementServer));
            sb.Append(VoiceGateway.MakeXML("MinimumPort", MinimumPort.ToString()));
            sb.Append(VoiceGateway.MakeXML("MaximumPort", MaximumPort.ToString()));
            sb.Append("<Logging>");
            sb.Append(VoiceGateway.MakeXML("Enabled", Logging.Enabled ? "true" : "false"));
            sb.Append(VoiceGateway.MakeXML("Folder", Logging.Folder));
            sb.Append(VoiceGateway.MakeXML("FileNamePrefix", Logging.FileNamePrefix));
            sb.Append(VoiceGateway.MakeXML("FileNameSuffix", Logging.FileNameSuffix));
            sb.Append(VoiceGateway.MakeXML("LogLevel", Logging.LogLevel.ToString()));
            sb.Append("</Logging>");
            return Request("Connector.Create.1", sb.ToString());
        }

        /// <summary>
        /// Shutdown Connector -- Should be called when the application is shutting down
        /// to gracefully release resources
        /// </summary>
        /// <param name="ConnectorHandle">Handle returned from successful Connector ‘create’ request</param>
        public int ConnectorInitiateShutdown(string ConnectorHandle)
        {
            string RequestXML = VoiceGateway.MakeXML("ConnectorHandle", ConnectorHandle);
            return Request("Connector.InitiateShutdown.1", RequestXML);
        }

        /// <summary>
        /// Mute or unmute the microphone
        /// </summary>
        /// <param name="ConnectorHandle">Handle returned from successful Connector ‘create’ request</param>
        /// <param name="Mute">true (mute) or false (unmute)</param>
        public int ConnectorMuteLocalMic(string ConnectorHandle, bool Mute)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(VoiceGateway.MakeXML("ConnectorHandle", ConnectorHandle));
            sb.Append(VoiceGateway.MakeXML("Value", Mute ? "true" : "false"));
            return Request("Connector.MuteLocalMic.1", sb.ToString());
        }

        /// <summary>
        /// Mute or unmute the speaker
        /// </summary>
        /// <param name="ConnectorHandle">Handle returned from successful Connector ‘create’ request</param>
        /// <param name="Mute">true (mute) or false (unmute)</param>
        public int ConnectorMuteLocalSpeaker(string ConnectorHandle, bool Mute)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(VoiceGateway.MakeXML("ConnectorHandle", ConnectorHandle));
            sb.Append(VoiceGateway.MakeXML("Value", Mute ? "true" : "false"));
            return Request("Connector.MuteLocalSpeaker.1", sb.ToString());
        }

        /// <summary>
        /// Set microphone volume
        /// </summary>
        /// <param name="ConnectorHandle">Handle returned from successful Connector ‘create’ request</param>
        /// <param name="Value">The level of the audio, a number between -100 and 100 where
        /// 0 represents ‘normal’ speaking volume</param>
        public int ConnectorSetLocalMicVolume(string ConnectorHandle, int Value)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(VoiceGateway.MakeXML("ConnectorHandle", ConnectorHandle));
            sb.Append(VoiceGateway.MakeXML("Value", Value.ToString()));
            return Request("Connector.SetLocalMicVolume.1", sb.ToString());
        }

        /// <summary>
        /// Set local speaker volume
        /// </summary>
        /// <param name="ConnectorHandle">Handle returned from successful Connector ‘create’ request</param>
        /// <param name="Value">The level of the audio, a number between -100 and 100 where
        /// 0 represents ‘normal’ speaking volume</param>
        public int ConnectorSetLocalSpeakerVolume(string ConnectorHandle, int Value)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(VoiceGateway.MakeXML("ConnectorHandle", ConnectorHandle));
            sb.Append(VoiceGateway.MakeXML("Value", Value.ToString()));
            return Request("Connector.SetLocalSpeakerVolume.1", sb.ToString());
        }
    }
}
