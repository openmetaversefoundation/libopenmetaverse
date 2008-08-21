/*
 * Copyright (c) 2006-2008, openmetaverse.org
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
using System.Threading;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace PacketDump
{
	class PacketDump
	{
        static bool LoginSuccess = false;
        static AutoResetEvent LoginEvent = new AutoResetEvent(false);

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			GridClient client;

			if (args.Length != 4)
			{
				Console.WriteLine("Usage: PacketDump [firstname] [lastname] [password] [seconds (0 for infinite)]");
				return;
			}

            client = new GridClient();
            // Turn off some unnecessary things
            client.Settings.MULTIPLE_SIMS = false;
            // Throttle packets that we don't want all the way down
            client.Throttle.Land = 0;
            client.Throttle.Wind = 0;
            client.Throttle.Cloud = 0;

			// Setup a packet callback that is called for every packet (PacketType.Default)
            client.Network.RegisterCallback(PacketType.Default, new NetworkManager.PacketCallback(DefaultHandler));
            
            // Register handlers for when we login, and when we are disconnected
            client.Network.OnLogin += new NetworkManager.LoginCallback(LoginHandler);
            client.Network.OnDisconnected += new NetworkManager.DisconnectedCallback(DisconnectHandler);

            // Start the login process
            client.Network.BeginLogin(client.Network.DefaultLoginParams(args[0], args[1], args[2], "PacketDump", "1.0.0"));

            // Wait until LoginEvent is set in the LoginHandler callback, or we time out
            if (LoginEvent.WaitOne(1000 * 20, false))
            {
                if (LoginSuccess)
                {
                    // Network.LoginMessage is set after a successful login
                    Logger.Log("Message of the day: " + client.Network.LoginMessage, Helpers.LogLevel.Info);

                    // Determine how long to run for
                    int start = Environment.TickCount;
                    int milliseconds = Int32.Parse(args[3]) * 1000;
                    bool forever = (milliseconds > 0) ? false : true;

                    // Packet handling is done with asynchronous callbacks. Run a sleeping loop in the main
                    // thread until we run out of time or the program is closed
                    while (true)
                    {
                        System.Threading.Thread.Sleep(100);

                        if (!forever && Environment.TickCount - start > milliseconds)
                            break;
                    }

                    // Finished running, log out
                    client.Network.Logout();
                }
                else
                {
                    Logger.Log("Login failed: " + client.Network.LoginMessage, Helpers.LogLevel.Error);
                }
            }
            else
            {
                Logger.Log("Login timed out", Helpers.LogLevel.Error);
            }
		}

        static void LoginHandler(LoginStatus login, string message)
        {
            Logger.Log(String.Format("Login: {0} ({1})", login, message), Helpers.LogLevel.Info);

            switch (login)
            {
                case LoginStatus.Failed:
                    LoginEvent.Set();
                    break;
                case LoginStatus.Success:
                    LoginSuccess = true;
                    LoginEvent.Set();
                    break;
            }
        }

        public static void DisconnectHandler(NetworkManager.DisconnectType type, string message)
        {
            if (type == NetworkManager.DisconnectType.NetworkTimeout)
            {
                Console.WriteLine("Network connection timed out, disconnected");
            }
            else if (type == NetworkManager.DisconnectType.ServerInitiated)
            {
                Console.WriteLine("Server disconnected us: " + message);
            }
        }

        public static void DefaultHandler(Packet packet, Simulator simulator)
        {
            Logger.Log(packet.ToString(), Helpers.LogLevel.Info);
        }
	}
}
