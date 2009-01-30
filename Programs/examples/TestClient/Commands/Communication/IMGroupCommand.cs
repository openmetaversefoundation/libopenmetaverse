using System;
using System.Collections.Generic;
using System.Threading;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace OpenMetaverse.TestClient
{
    public class ImGroupCommand : Command
    {
        Guid ToGroupID = Guid.Empty;
        ManualResetEvent WaitForSessionStart = new ManualResetEvent(false);
        public ImGroupCommand(TestClient testClient)
        {

            Name = "imgroup";
            Description = "Send an instant message to a group. Usage: imgroup [group_Guid] [message]";
            Category = CommandCategory.Communication;
        }

        public override string Execute(string[] args, Guid fromAgentID)
        {
            if (args.Length < 2)
                return "Usage: imgroup [group_Guid] [message]";



            if (GuidExtensions.TryParse(args[0], out ToGroupID))
            {
                string message = String.Empty;
                for (int ct = 1; ct < args.Length; ct++)
                    message += args[ct] + " ";
                message = message.TrimEnd();
                if (message.Length > 1023) message = message.Remove(1023);

                Client.Self.OnGroupChatJoin += new AgentManager.GroupChatJoinedCallback(Self_OnGroupChatJoin);
                if (!Client.Self.GroupChatSessions.ContainsKey(ToGroupID))
                {
                    WaitForSessionStart.Reset();
                    Client.Self.RequestJoinGroupChat(ToGroupID);
                }
                else
                {
                    WaitForSessionStart.Set();
                }
                
                if (WaitForSessionStart.WaitOne(20000, false))
                {
                    Client.Self.InstantMessageGroup(ToGroupID, message);
                }
                else
                {
                    return "Timeout waiting for group session start";
                }
                
                Client.Self.OnGroupChatJoin -= new AgentManager.GroupChatJoinedCallback(Self_OnGroupChatJoin);
                return "Instant Messaged group " + ToGroupID.ToString() + " with message: " + message;
            }
            else
            {
                return "failed to instant message group";
            }
        }

        void Self_OnGroupChatJoin(Guid groupChatSessionID, string sessionName, Guid tmpSessionID, bool success)
        {
            if (success)
            {
                Console.WriteLine("Joined {0} Group Chat Success!", sessionName);
                WaitForSessionStart.Set();
            }
            else
            {
                Console.WriteLine("Join Group Chat failed :(");
            }
        }
    }
}
