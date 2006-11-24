using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class SayCommand: Command
    {
		public SayCommand()
		{
			Name = "say";
			Description = "Say something.";
		}

		public override string Execute(string[] args, LLUUID fromAgentID)
		{
			if (args.Length < 1)
                return "usage: say whatever";

			string message = String.Empty;
			foreach (string s in args)
				message += s + " ";

			Client.Self.Chat(message, 0, MainAvatar.ChatType.Normal);

            return "Said " + message;
		}
    }
}
