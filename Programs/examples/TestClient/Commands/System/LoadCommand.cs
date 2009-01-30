using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace OpenMetaverse.TestClient
{
    public class LoadCommand: Command
    {
        public LoadCommand(TestClient testClient)
		{
			Name = "load";
			Description = "Loads commands from a dll. (Usage: load AssemblyNameWithoutExtension)";
            Category = CommandCategory.TestClient;
		}

		public override string Execute(string[] args, Guid fromAgentID)
		{
			if (args.Length < 1)
				return "Usage: load AssemblyNameWithoutExtension";

			string filename = AppDomain.CurrentDomain.BaseDirectory + args[0] + ".dll";
			Client.RegisterAllCommands(Assembly.LoadFile(filename));
            return "Assembly " + filename + " loaded.";
		}
    }
}
