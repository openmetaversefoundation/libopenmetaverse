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

		// Default packet handler, registered for all packet types
		public static void DefaultHandler(Packet packet, Simulator simulator)
		{
			Console.WriteLine(packet.ToString());
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

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			GridClient client;

			if (args.Length == 0 || (args.Length < 4 && args[0] != "--printmap"))
			{
				Console.WriteLine("Usage: PacketDump [--printmap] [--decrypt] [inputfile] [outputfile] "
                    + "[--protocol] [firstname] [lastname] [password] [seconds (0 for infinite)]");
				return;
			}

			if (args[0] == "--decrypt")
			{
				try
				{
					ProtocolManager.DecodeMapFile(args[1], args[2]);
				}
				catch (Exception e)
				{
					Console.WriteLine(e.ToString());
				}

				return;
			}

            client = new GridClient();
            // Turn off some unnecessary things
            Settings.LOG_LEVEL = Helpers.LogLevel.None;
            client.Settings.MULTIPLE_SIMS = false;
            // Throttle packets that we don't want all the way down
            client.Throttle.Land = 0;
            client.Throttle.Wind = 0;
            client.Throttle.Cloud = 0;

			if (args[0] == "--printmap")
			{
                ProtocolManager protocol;

                try
                {
                    protocol = new ProtocolManager("message_template.msg", client);
                }
                catch (Exception e)
                {
                    // Error initializing the client, probably missing file(s)
                    Console.WriteLine(e.ToString());
                    return;
                }

                protocol.PrintMap();
				return;
			}

			// Setup the packet callback and disconnect event handler
            client.Network.RegisterCallback(PacketType.Default, new NetworkManager.PacketCallback(DefaultHandler));
            client.Network.OnDisconnected += new NetworkManager.DisconnectedCallback(DisconnectHandler);

            client.Network.OnLogin += new NetworkManager.LoginCallback(Network_OnLogin);
            client.Network.BeginLogin(client.Network.DefaultLoginParams(args[0], args[1], args[2], "PacketDump", "1.0.0"));

            LoginEvent.WaitOne();

            if (!LoginSuccess)
            {
                Console.WriteLine("Login failed: {0}", client.Network.LoginMessage);
                return;
            }

			Console.WriteLine("Message of the day: " + client.Network.LoginMessage);

            int start = Environment.TickCount;
            int milliseconds = Int32.Parse(args[3]) * 1000;
            bool forever = (milliseconds > 0) ? false : true;

			while (true)
			{
                System.Threading.Thread.Sleep(100);

                if (!forever && Environment.TickCount - start > milliseconds)
                {
                    break;
                }
			}

            client.Network.Logout();
		}

        static void Network_OnLogin(LoginStatus login, string message)
        {
            Console.WriteLine("Login: " + login.ToString() + " (" + message + ")");

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
	}
}
