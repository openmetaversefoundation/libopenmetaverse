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

namespace name2key
{
	class name2key
	{
		static bool waiting = true;

		public static void QueryHandler(Packet packet, Simulator simulator)
		{
            DirPeopleReplyPacket reply = (DirPeopleReplyPacket)packet;

            if (reply.QueryReplies.Length < 1)
            {
                Console.WriteLine("ERROR: Got an empty reply");
            }
            else
            {
                if (reply.QueryReplies.Length > 1)
                {
                    Console.WriteLine("ERROR: Ambiguous name. Returning first match");
                }

                Console.WriteLine("UUID: " + reply.QueryReplies[0].AgentID.ToString());
            }

			waiting = false;
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

			client = new SecondLife();

			// Setup the callback
            // FIXME: Rewrite this code as soon as people searching is added to DirectoryManager
            client.Network.RegisterCallback(PacketType.DirPeopleReply, new NetworkManager.PacketCallback(QueryHandler));

			if (!client.Network.Login(args[0], args[1], args[2], "name2key", "jhurliman@wsu.edu"))
			{
				// Login failed
				Console.WriteLine("ERROR: " + client.Network.LoginMessage);
				return;
			}

			// Send the Query
            DirFindQueryPacket find = new DirFindQueryPacket();
            find.AgentData.AgentID = client.Network.AgentID;
            find.AgentData.SessionID = client.Network.SessionID;
            find.QueryData.QueryFlags = 1;
            find.QueryData.QueryText = Helpers.StringToField(args[3] + " " + args[4]);
            find.QueryData.QueryID = LLUUID.Random();
            find.QueryData.QueryStart = 0;
            
			client.Network.SendPacket((Packet)find);

			while (waiting)
			{
                // FIXME: Sleeping while loops are a poor example, this is supposed to be
                // model code. Replace this with a ManualResetEvent
                System.Threading.Thread.Sleep(500);
			}

			client.Network.Logout();
		}
	}
}
