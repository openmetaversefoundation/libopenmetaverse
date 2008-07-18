using System;
using System.Collections.Generic;
using System.Text;

namespace libsecondlife.Voice
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
