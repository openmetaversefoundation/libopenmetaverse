using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace OpenMetaverse.TestClient
{
    public class EchoMasterCommand: Command
    {
        public EchoMasterCommand(TestClient testClient)
		{
			Name = "echoMaster";
			Description = "Repeat everything that master says.";
            Category = CommandCategory.Communication;
		}

        public override string Execute(string[] args, Guid fromAgentID)
		{
			if (!Active)
			{
				Active = true;
                Client.Self.OnChat += new AgentManager.ChatCallback(Self_OnChat);
				return "Echoing is now on.";
			}
			else
			{
				Active = false;
                Client.Self.OnChat -= new AgentManager.ChatCallback(Self_OnChat);
				return "Echoing is now off.";
			}
		}

		void Self_OnChat(string message, ChatAudibleLevel audible, ChatType type, 
            ChatSourceType sourcetype, string fromName, Guid id, Guid ownerid, Vector3 position)
		{
			if (message.Length > 0 && (Client.MasterKey == id || (Client.MasterName == fromName && !Client.AllowObjectMaster)))
			    Client.Self.Chat(message, 0, ChatType.Normal);
		}
    }
}
