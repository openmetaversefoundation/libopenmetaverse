using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace OpenMetaverse.TestClient
{
    public class HelpCommand: Command
    {
        public HelpCommand(TestClient testClient)
		{
			Name = "help";
			Description = "Lists available commands. usage: help [command] to display information on commands";
            Category = CommandCategory.TestClient;
		}

        public override string Execute(string[] args, Guid fromAgentID)
		{
            if (args.Length > 0)
            {
                if (Client.Commands.ContainsKey(args[0]))
                    return Client.Commands[args[0]].Description;
                else
                    return "Command " + args[0] + " Does not exist. \"help\" to display all available commands.";
            }
			StringBuilder result = new StringBuilder();
            SortedDictionary<CommandCategory, List<Command>> CommandTree = new SortedDictionary<CommandCategory, List<Command>>();

            CommandCategory cc;
			foreach (Command c in Client.Commands.Values)
			{
                if (c.Category.Equals(null))
                    cc = CommandCategory.Unknown;
                else
                    cc = c.Category;

                if (CommandTree.ContainsKey(cc))
                    CommandTree[cc].Add(c);
                else
                {
                    List<Command> l = new List<Command>();
                    l.Add(c);
                    CommandTree.Add(cc, l);
                }
			}

            foreach (KeyValuePair<CommandCategory, List<Command>> kvp in CommandTree)
            {
                result.AppendFormat(System.Environment.NewLine + "* {0} Related Commands:" + System.Environment.NewLine, kvp.Key.ToString());
                int colMax = 0;
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    if (colMax >= 120)
                    {
                        result.AppendLine();
                        colMax = 0;
                    }

                    result.AppendFormat(" {0,-15}", kvp.Value[i].Name);
                    colMax += 15;
                }
                result.AppendLine();
            }
            result.AppendLine(System.Environment.NewLine + "Help [command] for usage/information");
            
            return result.ToString();
		}
    }
}
