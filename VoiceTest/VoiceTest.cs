using System;
using libsecondlife;
using libsecondlife.Utilities;

namespace VoiceTest
{
    class VoiceTest
    {
        static void Main(string[] args)
        {
            SecondLife client = new SecondLife();
            VoiceManager voice = new VoiceManager(client);

            if (voice.ConnectToDaemon())
            {
                voice.RequestCaptureDevices();
                voice.RequestRenderDevices();

                voice.RequestSetRenderDevice("Speakers (Realtek High Definiti");

                voice.RequestSetSpeakerVolume(75);
                voice.RequestSetCaptureVolume(75);

                voice.RequestStartTuningMode(10000);

                //voice.RequestCreateConnector();

                

                voice.RequestRenderAudioStart("bugsbunny1.wav", true);
                voice.RequestRenderAudioStart("bugsbunny1.wav", false);
                
            }

            Console.ReadKey();

            voice.RequestRenderAudioStop();
            voice.RequestStopTuningMode();

            Console.ReadKey();
        }
    }
}
