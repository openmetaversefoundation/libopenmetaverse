using System;
using libsecondlife;

namespace sldump
{
	class sldump
	{
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
				Console.WriteLine("Error: " + e.Message);
				return;
			}

			if (args[0] == "--protocol")
			{
				client.Protocol.PrintMap();
				return;
			}

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
