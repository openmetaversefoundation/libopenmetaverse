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
using System.Collections;
using libsecondlife;

namespace name2key
{
	class name2key
	{
		static bool waiting = true;

		//
		public static void QueryHandler(Packet packet, Circuit circuit)
		{
			if (packet.Layout.Name.IndexOf("Dir") > -1)
			{
				ArrayList blocks = packet.Blocks();

				if (blocks.Count > 3)
				{
					Console.WriteLine("ERROR: Ambiguous name. Returning first match");
				}

				foreach (Block block in blocks)
				{
					if (block.Layout.Name == "QueryReplies")
					{
						foreach (Field field in block.Fields)
						{
							if (field.Layout.Name == "AgentID")
							{
								Console.WriteLine("UUID: " + field.Data.ToString());
								goto Done;
							}
						}
					}
				}

			Done:
				waiting = false;
			}
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			SecondLife client;

			if (args.Length < 5)
			{
				Console.WriteLine("Usage: name2key [loginfirstname] [loginlastname] [password] [firstname] [lastname]");
				return;
			}

			try
			{
				client = new SecondLife("keywords.txt", "protocol.txt");
			}
			catch (Exception e)
			{
				// Error initializing the client, probably missing file(s)
				Console.WriteLine(e.ToString());
				return;
			}

			// Setup the callback
			PacketCallback queryCallback = new PacketCallback(QueryHandler);
			client.Network.UserCallbacks["DirPeopleReply"] = queryCallback;

			Hashtable loginParams = NetworkManager.DefaultLoginValues(args[0], args[1], args[2], "00:00:00:00:00:00",
				"last", 1, 10, 10, 10, "Win", "0", "name2key", "jhurliman@wsu.edu");
			Hashtable loginReply = new Hashtable();

			if (!client.Network.Login(loginParams, out loginReply))
			{
				// Login failed
				Console.WriteLine("ERROR: " + client.Network.LoginError);
				return;
			}

			// Send the Query
			string name = args[3] + " " + args[4];
			LLUUID queryID = new LLUUID("00000000000000000000000000000001");
			Packet packet = PacketBuilder.DirFindQuery(client.Protocol, name, queryID,
				client.Network.AgentID, client.Network.SessionID);
			client.Network.SendPacket(packet);

			while (waiting)
			{
				client.Tick();
			}

			client.Network.Logout();
		}
	}
}
