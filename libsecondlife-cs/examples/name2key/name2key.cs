using System;
using System.Collections;
using libsecondlife;

namespace name2key
{
	class name2key
	{
		static bool waiting = true;

		//
		public static void QueryHandler(Packet packet)
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
			client.Network.Callbacks["DirPeopleReply"] = queryCallback;

			if (!client.Network.Login(args[0], args[1], args[2], "00:00:00:00:00:00", 1, 10, 2, 2, "Win", 
				"0", "sldump", "jhurliman@wsu.edu"))
			{
				// Login failed
				Console.WriteLine("ERROR: " + client.Network.LoginError);
				return;
			}

			// Send the Query
			string name = args[3] + " " + args[4];
			LLUUID queryID = new LLUUID("00000000000000000000000000000001");
			Packet packet = PacketBuilder.DirFindQuery(client.Protocol, name, queryID,
				client.Network.LoginValues.AgentID, client.Network.LoginValues.SessionID);
			client.Network.SendPacket(packet);

			while (waiting)
			{
				client.Tick();
			}

			client.Network.Logout();
		}
	}
}
