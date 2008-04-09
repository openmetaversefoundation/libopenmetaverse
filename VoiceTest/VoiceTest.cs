/*
 * Copyright (c) 2006-2007, Second Life Reverse Engineering Team
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the Second Life Reverse Engineering Team nor the names
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
using System.Threading;
using libsecondlife;
using libsecondlife.Utilities;

namespace VoiceTest
{
    public class VoiceException: Exception
    {
        public bool LoggedIn = false;

        public VoiceException(string msg): base(msg) 
        {
        }

        public VoiceException(string msg, bool loggedIn): base(msg) 
        {
            LoggedIn = loggedIn;
        }
    }

    class VoiceTest
    {
        static AutoResetEvent EventQueueRunningEvent = new AutoResetEvent(false);
        static AutoResetEvent ProvisionEvent = new AutoResetEvent(false);
        static AutoResetEvent ParcelVoiceInfoEvent = new AutoResetEvent(false);
        static string VoiceAccount = String.Empty;
        static string VoicePassword = String.Empty;
        static string VoiceRegionName = String.Empty;
        static int VoiceLocalID = 0;
        static string VoiceChannelURI = String.Empty;

        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: VoiceTest.exe [firstname] [lastname] [password]");
                return;
            }

            string firstName = args[0];
            string lastName = args[1];
            string password = args[2];
            

            SecondLife client = new SecondLife();
            client.Settings.MULTIPLE_SIMS = false;
            client.Settings.DEBUG = true;
            client.Settings.LOG_RESENDS = false;
            client.Settings.STORE_LAND_PATCHES = true;
            client.Settings.ALWAYS_DECODE_OBJECTS = true;
            client.Settings.ALWAYS_REQUEST_OBJECTS = true;
            client.Settings.SEND_AGENT_UPDATES = true;

            string loginURI = client.Settings.LOGIN_SERVER;
            if (4 == args.Length) {
                loginURI = args[3];
            }

            VoiceManager voice = new VoiceManager(client);
            voice.OnProvisionAccount += voice_OnProvisionAccount;
            voice.OnParcelVoiceInfo += voice_OnParcelVoiceInfo;

            client.Network.OnEventQueueRunning += client_OnEventQueueRunning;

            try {
                if (!voice.ConnectToDaemon()) throw new VoiceException("Failed to connect to the voice daemon");

                List<string> captureDevices = voice.CaptureDevices();

                Console.WriteLine("Capture Devices:");
                for (int i = 0; i < captureDevices.Count; i++)
                    Console.WriteLine(String.Format("{0}. \"{1}\"", i, captureDevices[i]));
                Console.WriteLine();

                List<string> renderDevices = voice.RenderDevices();

                Console.WriteLine("Render Devices:");
                for (int i = 0; i < renderDevices.Count; i++)
                    Console.WriteLine(String.Format("{0}. \"{1}\"", i, renderDevices[i]));
                Console.WriteLine();


                // Login to SL
                Console.WriteLine("Logging in to Second Life as " + firstName + " " + lastName + "...");
                LoginParams loginParams = 
                    client.Network.DefaultLoginParams(firstName, lastName, password, "Voice Test", 
                                                      "Metaverse Industries LLC <jhurliman@metaverseindustries.com>");
                loginParams.URI = loginURI;
                if (!client.Network.Login(loginParams))
                    throw new VoiceException("Login to SL failed: " + client.Network.LoginMessage);
                Console.WriteLine("Logged in: " + client.Network.LoginMessage);


                Console.WriteLine("Creating voice connector...");
                int status;
                string connectorHandle = voice.CreateConnector(out status);
                if (String.IsNullOrEmpty(connectorHandle)) 
                    throw new VoiceException("Failed to create a voice connector, error code: " + status, true);
                Console.WriteLine("Voice connector handle: " + connectorHandle);


                Console.WriteLine("Waiting for OnEventQueueRunning");
                if (!EventQueueRunningEvent.WaitOne(45 * 1000, false)) 
                    throw new VoiceException("EventQueueRunning event did not occur", true);
                Console.WriteLine("EventQueue running");


                Console.WriteLine("Asking the current simulator to create a provisional account...");
                if (!voice.RequestProvisionAccount()) 
                    throw new VoiceException("Failed to request a provisional account", true); 
                if (!ProvisionEvent.WaitOne(120 * 1000, false)) 
                    throw new VoiceException("Failed to create a provisional account", true);
                Console.WriteLine("Provisional account created. Username: " + VoiceAccount + 
                                  ", Password: " + VoicePassword);


                Console.WriteLine("Logging in to voice server " + voice.VoiceServer);
                string accountHandle = voice.Login(VoiceAccount, VoicePassword, connectorHandle, out status);
                if (String.IsNullOrEmpty(accountHandle)) 
                    throw new VoiceException("Login failed, error code: " + status, true);
                Console.WriteLine("Login succeeded, account handle: " + accountHandle);


                if (!voice.RequestParcelVoiceInfo()) 
                    throw new Exception("Failed to request parcel voice info");
                if (!ParcelVoiceInfoEvent.WaitOne(45 * 1000, false)) 
                    throw new VoiceException("Failed to obtain parcel info voice", true);


                Console.WriteLine("Parcel Voice Info obtained. Region name {0}, local parcel ID {1}, channel URI {2}",
                                  VoiceRegionName, VoiceLocalID, VoiceChannelURI);

                client.Network.Logout();
            }
            catch(Exception e) 
            {
                Console.WriteLine(e.Message);
                if (e is VoiceException && (e as VoiceException).LoggedIn) 
                {
                    client.Network.Logout();
                }
                
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
        
        static void client_OnEventQueueRunning(Simulator sim) {
            EventQueueRunningEvent.Set();
        }

        static void client_OnLogMessage(string message, Helpers.LogLevel level)
        {
            if (level == Helpers.LogLevel.Warning || level == Helpers.LogLevel.Error)
                Console.WriteLine(level.ToString() + ": " + message);
        }

        static void voice_OnProvisionAccount(string username, string password)
        {
            VoiceAccount = username;
            VoicePassword = password;

            ProvisionEvent.Set();
        }

        static void voice_OnParcelVoiceInfo(string regionName, int localID, string channelURI)
        {
            VoiceRegionName = regionName;
            VoiceLocalID = localID;
            VoiceChannelURI = channelURI;

            ParcelVoiceInfoEvent.Set();
        }
    }
}
