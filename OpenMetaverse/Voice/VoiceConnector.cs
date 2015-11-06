/*
 * Copyright (c) 2006-2016, openmetaverse.co
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.co nor the names
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
using System.Text;

namespace OpenMetaverse.Voice
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
        /// <param name="MaximumPort"></param>
        /// <param name="MinimumPort"></param>
        public int ConnectorCreate(string ClientName, string AccountManagementServer, ushort MinimumPort,
            ushort MaximumPort, VoiceLoggingSettings Logging)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(VoiceGateway.MakeXML("ClientName", ClientName));
            sb.Append(VoiceGateway.MakeXML("AccountManagementServer", AccountManagementServer));
            sb.Append(VoiceGateway.MakeXML("MinimumPort", MinimumPort.ToString()));
            sb.Append(VoiceGateway.MakeXML("MaximumPort", MaximumPort.ToString()));
            sb.Append(VoiceGateway.MakeXML("Mode", "Normal"));
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
