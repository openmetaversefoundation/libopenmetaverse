using System;
using System.Collections.Generic;
using System.Threading;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace OpenMetaverse.TestClient
{
    public class ImGroupCommand : Command
    {
        UUID ToGroupID = UUID.Zero;
        ManualResetEvent WaitForSessionStart = new ManualResetEvent(false);
        public ImGroupCommand(TestClient testClient)
        {

            Name = "imgroup";
            Description = "Send an instant message to a group. Usage: imgroup [group_uuid] [message]";
            Category = CommandCategory.Communication;
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            if (args.Length < 2)
                return "Usage: imgroup [group_uuid] [message]";



            if (UUID.TryParse(args[0], out ToGroupID))
            {
                string message = String.Empty;
                for (int ct = 1; ct < args.Length; ct++)
                    message += args[ct] + " ";
                message = message.TrimEnd();
                if (message.Length > 1023) message = message.Remove(1023);

                Client.Self.OnGroupChatJoin += new AgentManager.GroupChatJoined(Self_OnGroupChatJoin);
                if (!Client.Self.GroupChatSessions.ContainsKey(ToGroupID))
                {
                    WaitForSessionStart.Reset();
                    Client.Self.RequestJoinGroupChat(ToGroupID);
                }
                else
                {
                    WaitForSessionStart.Set();
                }
                
                if (WaitForSessionStart.WaitOne(10000, false))
                {
                    Client.Self.InstantMessageGroup(ToGroupID, message);
                }
                else
                {
                    return "Timeout waiting for group session start";
                }
                
                Client.Self.OnGroupChatJoin -= new AgentManager.GroupChatJoined(Self_OnGroupChatJoin);
                return "Instant Messaged group " + ToGroupID.ToString() + " with message: " + message;
            }
            else
            {
                return "failed to instant message group";
            }
        }

        void Self_OnGroupChatJoin(UUID groupChatSessionID, UUID tmpSessionID, bool success)
        {
            if (success)
            {
                Console.WriteLine("Join Group Chat Success!");
                WaitForSessionStart.Set();
            }
            else
            {
                Console.WriteLine("Join Group Chat failed :(");
            }
        }
    }
}
