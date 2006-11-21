/*
 * Copyright (c) 2006, Second Life Reverse Engineering Team
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
using libsecondlife.Packets;

namespace sldump
{
	class sldump
	{
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
			SecondLife client;

			if (args.Length == 0 || (args.Length < 4 && args[0] != "--printmap"))
			{
				Console.WriteLine("Usage: sldump [--printmap] [--decrypt] [inputfile] [outputfile] "
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

            client = new SecondLife();

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
            client.Network.OnDisconnected += new NetworkManager.DisconnectCallback(DisconnectHandler);

			if (!client.Network.Login(args[0], args[1], args[2], "sldump", "contact@libsecondlife.org"))
			{
				// Login failed
				Console.WriteLine("Error logging in: " + client.Network.LoginError);
				return;
			}

			// Login was successful
			Console.WriteLine("Message of the day: " + client.Network.LoginValues["message"]);

            // Throttle packets that we don't want all the way down
			AgentThrottle throttle = new AgentThrottle(50000);
			throttle.Land = 0;
			throttle.Wind = 0;
			throttle.Cloud = 0;
			throttle.Texture = 0;
		    throttle.Send(client);

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
	}
}
