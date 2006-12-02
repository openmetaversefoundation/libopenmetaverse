using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class WhisperCommand : Command
    {
        public WhisperCommand()
        {
            Name = "whisper";
            Description = "Whisper something.";
        }

        public override string Execute(SecondLife Client, string[] args, LLUUID fromAgentID)
        {
            if (args.Length < 1)
                return "usage: whisper whatever";

            string message = String.Empty;
            foreach (string s in args)
                message += s + " ";

            Client.Self.Chat(message, 0, MainAvatar.ChatType.Whisper);

            return "Whispered " + message;
        }
    }
}
