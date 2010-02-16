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
using System.Text;

namespace OpenMetaverse.Voice
{
    public partial class VoiceGateway
    {
        /// <summary>
        /// This is used to get a list of audio devices that can be used for capture (input) of voice.
        /// </summary>
        /// <returns></returns>
        public int AuxGetCaptureDevices()
        {
            return Request("Aux.GetCaptureDevices.1");
        }

        /// <summary>
        /// This is used to get a list of audio devices that can be used for render (playback) of voice.
        /// </summary>
        public int AuxGetRenderDevices()
        {
            return Request("Aux.GetRenderDevices.1");
        }

        /// <summary>
        /// This command is used to select the render device.
        /// </summary>
        /// <param name="RenderDeviceSpecifier">The name of the device as returned by the Aux.GetRenderDevices command.</param>
        public int AuxSetRenderDevice(string RenderDeviceSpecifier)
        {
            string RequestXML = VoiceGateway.MakeXML("RenderDeviceSpecifier", RenderDeviceSpecifier);
            return Request("Aux.SetRenderDevice.1", RequestXML);
        }

        /// <summary>
        /// This command is used to select the capture device.
        /// </summary>
        /// <param name="CaptureDeviceSpecifier">The name of the device as returned by the Aux.GetCaptureDevices command.</param>
        public int AuxSetCaptureDevice(string CaptureDeviceSpecifier)
        {
            string RequestXML = VoiceGateway.MakeXML("CaptureDeviceSpecifier", CaptureDeviceSpecifier);
            return Request("Aux.SetCaptureDevice.1", RequestXML);
        }

        /// <summary>
        /// This command is used to start the audio capture process which will cause
        /// AuxAudioProperty Events to be raised. These events can be used to display a
        /// microphone VU meter for the currently selected capture device. This command
        /// should not be issued if the user is on a call.
        /// </summary>
        /// <param name="Duration">(unused but required)</param>
        /// <returns></returns>
        public int AuxCaptureAudioStart(int Duration)
        {
            string RequestXML = VoiceGateway.MakeXML("Duration", Duration.ToString());
            return Request("Aux.CaptureAudioStart.1", RequestXML);
        }

        /// <summary>
        /// This command is used to stop the audio capture process.
        /// </summary>
        /// <returns></returns>
        public int AuxCaptureAudioStop()
        {
            return Request("Aux.CaptureAudioStop.1");
        }

        /// <summary>
        /// This command is used to set the mic volume while in the audio tuning process.
        /// Once an acceptable mic level is attained, the application must issue a
        /// connector set mic volume command to have that level be used while on voice
        /// calls.
        /// </summary>
        /// <param name="Level">the microphone volume (-100 to 100 inclusive)</param>
        /// <returns></returns>
        public int AuxSetMicLevel(int Level)
        {
            string RequestXML = VoiceGateway.MakeXML("Level", Level.ToString());
            return Request("Aux.SetMicLevel.1", RequestXML);
        }

        /// <summary>
        /// This command is used to set the speaker volume while in the audio tuning
        /// process. Once an acceptable speaker level is attained, the application must
        /// issue a connector set speaker volume command to have that level be used while
        /// on voice calls.
        /// </summary>
        /// <param name="Level">the speaker volume (-100 to 100 inclusive)</param>
        /// <returns></returns>
        public int AuxSetSpeakerLevel(int Level)
        {
            string RequestXML = VoiceGateway.MakeXML("Level", Level.ToString());
            return Request("Aux.SetSpeakerLevel.1", RequestXML);
        }
    }
}
