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

        public static void DisconnectHandler(DisconnectType type, string message)
        {
            if (type == DisconnectType.NetworkTimeout)
            {
                Console.WriteLine("Network connection timed out, disconnected");
            }
            else if (type == DisconnectType.ServerInitiated)
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

			if (args.Length == 0 || (args.Length < 3 && args[0] != "--printmap"))
			{
				Console.WriteLine("Usage: sldump [--printmap] [--decrypt] [inputfile] [outputfile] [--protocol] [firstname] " +
					"[lastname] [password]");
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
			client.Network.RegisterCallback(PacketType.Default, new PacketCallback(DefaultHandler));
            client.Network.OnDisconnected += new DisconnectCallback(DisconnectHandler);

            Dictionary<string, object> loginParams = NetworkManager.DefaultLoginValues(args[0], args[1], args[2], 
                "0", "last", "Win", "0", "sldump", "contact@libsecondlife.org");

			// An example of how to pass additional options to the login server
            //loginParams["id0"] = "65e142a8d3c1ee6632259f111cb168c9";
            //loginParams["viewer_digest"] = "0e63550f-0991-a092-3158-b4206e728ffa";

			if (!client.Network.Login(loginParams/*, "http://127.0.0.1:8080/"*/))
			{
				// Login failed
				Console.WriteLine("Error logging in: " + client.Network.LoginError);
				return;
			}

			// Login was successful
			Console.WriteLine("Message of the day: " + client.Network.LoginValues["message"]);

			while (true)
			{
				client.Tick();
			}
		}
	}
}
