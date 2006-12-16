using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class EchoMasterCommand: Command
    {
		public EchoMasterCommand()
		{
			Name = "echoMaster";
			Description = "Repeat everything that master says.";
		}

        public override string Execute(SecondLife Client, string[] args, LLUUID fromAgentID)
		{
			if (!Active)
			{
				Active = true;
				Client.Self.OnChat += new ChatCallback(Self_OnChat);
				return "Echoing is now on.";
			}
			else
			{
				Active = false;
				Client.Self.OnChat -= new ChatCallback(Self_OnChat);
				return "Echoing is now off.";
			}
		}

		void Self_OnChat(string message, byte audible, byte type, byte sourcetype, string fromName, LLUUID id, LLUUID ownerid, LLVector3 position)
		{
			if (message.Length > 0 && TestClient.Master == fromName)
			{
			    TestClient.Self.Chat(message, 0, MainAvatar.ChatType.Normal);
			}
		}
    }
}
