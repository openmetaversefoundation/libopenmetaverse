using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestTool
{
    public class SetMasterCommand: Command
    {
		public DateTime Created = DateTime.Now;

		public SetMasterCommand()
		{
			Name = "setMaster";
			Description = "Sets the user name of the master user.  The master user can IM to run commands.";
		}

		public override string Execute(string[] args, LLUUID fromAgentID)
		{
			string masterName = String.Empty;
			for (int ct = 0; ct < args.Length;ct++)
				masterName = masterName + args[ct] + " ";
			TestTool.Master = masterName.TrimEnd();

			return "Master set to " + masterName;
		}
    }
}
