using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class SetMasterCommand: Command
    {
		public DateTime Created = DateTime.Now;

        public SetMasterCommand(TestClient testClient)
		{
			Name = "setMaster";
			Description = "Sets the user name of the master user.  The master user can IM to run commands.";
		}

        public override string Execute(string[] args, LLUUID fromAgentID)
		{
			string masterName = String.Empty;
			for (int ct = 0; ct < args.Length;ct++)
				masterName = masterName + args[ct] + " ";
			Client.Master = masterName.TrimEnd();

			foreach (Avatar av in Client.AvatarList.Values)
			{
			    if (av.Name == Client.Master)
			    {
			        Client.Self.InstantMessage(av.ID, "You are now my master.  IM me with \"help\" for a command list.");
			        break;
			    }
			}

			return "Master set to " + masterName;
		}
    }
}
