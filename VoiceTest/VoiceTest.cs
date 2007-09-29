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
    class VoiceTest
    {
        static AutoResetEvent ProvisionEvent = new AutoResetEvent(false);
        static string VoiceAccount = String.Empty;
        static string VoicePassword = String.Empty;

        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Usage: VoiceTest.exe [firstname] [lastname] [password]");
                return;
            }

            string firstName = args[0];
            string lastName = args[1];
            string password = args[2];

            SecondLife client = new SecondLife();
            client.Settings.MULTIPLE_SIMS = false;
            //client.OnLogMessage += new SecondLife.LogCallback(client_OnLogMessage);

            VoiceManager voice = new VoiceManager(client);
            voice.OnProvisionAccount += new VoiceManager.ProvisionAccountCallback(voice_OnProvisionAccount);

            if (voice.ConnectToDaemon())
            {
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

                Console.WriteLine("Logging in to Second Life as " + firstName + " " + lastName + "...");

                // Login to SL
                if (client.Network.Login(firstName, lastName, password, "Voice Test", "Metaverse Industries LLC <jhurliman@metaverseindustries.com>"))
                {
                    Console.WriteLine("Creating voice connector...");

                    int status;
                    string connectorHandle = voice.CreateConnector(out status);

                    if (connectorHandle != String.Empty)
                    {
                        Console.WriteLine("Voice connector handle: " + connectorHandle);

                        // Wait for the simulator capabilities to show up
                        client.Network.CurrentSim.Caps.CapsReceivedEvent.WaitOne(45 * 1000, false);

                        Console.WriteLine("Asking the current simulator to create a provisional account...");

                        if (voice.RequestProvisionAccount())
                        {
                            if (ProvisionEvent.WaitOne(45 * 1000, false))
                            {
                                Console.WriteLine("Provisional account created. Username: " + VoiceAccount + ", Password: " + VoicePassword);
                                Console.WriteLine("Logging in to voice server " + voice.VoiceServer);

                                string accountHandle = voice.Login(VoiceAccount, VoicePassword, connectorHandle, out status);

                                if (accountHandle != String.Empty)
                                {
                                    Console.WriteLine("Login succeeded, account handle: " + accountHandle);
                                }
                                else
                                {
                                    Console.WriteLine("Login failed, error code: " + status);
                                    client.Network.Logout();
                                }
                            }
                            else
                            {
                                Console.WriteLine("Failed to create a provisional account");
                                client.Network.Logout();
                            }
                        }
                        else
                        {
                            Console.WriteLine("Failed to request a provisional account");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Failed to create a voice connector, error code: " + status);
                        client.Network.Logout();
                    }
                }
                else
                {
                    Console.WriteLine("Login to SL failed: " + client.Network.LoginMessage);
                }
            }
            else
            {
                Console.WriteLine("Failed to connect to the voice daemon");
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
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
    }
}
