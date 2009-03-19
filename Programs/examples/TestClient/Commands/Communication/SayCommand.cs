using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;

namespace OpenMetaverse.TestClient
{
    public class SayCommand: Command
    {
        public SayCommand(TestClient testClient)
        {
            Name = "say";
            Description = "Say something.  (usage: say (optional channel) whatever)";
            Category = CommandCategory.Communication;
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            int channel = 0;
            int startIndex = 0;

            if (args.Length < 1)
            {
                return "usage: say (optional channel) whatever";
            }
            else if (args.Length > 1)
            {
                if (Int32.TryParse(args[0], out channel))
                    startIndex = 1;
            }

            String message = String.Empty;

            for (int i = startIndex; i < args.Length; i++)
            {
                // Append a space before the next arg
                if( i > 0 )
                    message += " ";
                message += args[i];
            }

            Client.Self.Chat(message, channel, ChatType.Normal);

            return "Said " + message.ToString();
        }
    }
}
