using System;
using System.Collections;
using libsecondlife;

namespace sldump
{
	class sldump
	{
		//
		public static void DefaultHandler(Packet packet, Circuit circuit)
		{
			string output = "";
			ArrayList blocks = packet.Blocks();

			output += "---- " + packet.Layout.Name + " ----\n";

			foreach (Block block in blocks)
			{
				output += " -- " + block.Layout.Name + " --\n";

				foreach (Field field in block.Fields)
				{
					output += "  " + field.Layout.Name + ": " + field.Data.ToString() + "\n";
				}
			}

			Console.Write(output);
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			SecondLife client;

			if (args.Length == 0 || (args.Length < 3 && args[0] != "--protocol"))
			{
				Console.WriteLine("Usage: sldump [--protocol] [firstname] [lastname] [password]");
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

			if (args[0] == "--protocol")
			{
				client.Protocol.PrintMap();
				return;
			}

			// Setup the callback
			PacketCallback defaultCallback = new PacketCallback(DefaultHandler);
			client.Network.UserCallbacks["Default"] = defaultCallback;

			if (!client.Network.Login(args[0], args[1], args[2], "00:00:00:00:00:00", 1, 10, 2, 2, "Win", 
				"0", "sldump", "jhurliman@wsu.edu"))
			{
				// Login failed
				Console.WriteLine("Error logging in: " + client.Network.LoginError);
				return;
			}

			// Login was successful
			Console.WriteLine("Message of the day: " + client.Network.LoginValues.Message);

			while (true)
			{
				client.Tick();
			}
		}
	}
}
