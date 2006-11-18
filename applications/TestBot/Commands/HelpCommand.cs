using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace TestBot
{
    public class HelpCommand: Command
    {
		public HelpCommand()
		{
			Name = "help";
			Description = "Lists available commands.";
		}

		public override string Execute(string[] args, LLUUID fromAgentID)
		{
			StringBuilder result = new StringBuilder();
			foreach (Command c in Bot.Commands.Values)
			{
				result.AppendFormat("{0} - {1}\n", c.Name, c.Description);
			}

            return result.ToString();
		}
    }
}
