using System;
using System.Collections.Generic;
using System.Threading;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class ImGroupCommand : Command
    {
        LLUUID ToGroupID = LLUUID.Zero;
        ManualResetEvent WaitForSessionStart = new ManualResetEvent(false);
        public ImGroupCommand(TestClient testClient)
        {

            Name = "imgroup";
            Description = "Send an instant message to a group. Usage: imgroup [group_uuid] [message]";
        }

        public override string Execute(string[] args, LLUUID fromAgentID)
        {
            if (args.Length < 2)
                return "Usage: imgroup [group_uuid] [message]";



            if (LLUUID.TryParse(args[0], out ToGroupID))
            {
                string message = String.Empty;
                for (int ct = 1; ct < args.Length; ct++)
                    message += args[ct] + " ";
                message = message.TrimEnd();
                if (message.Length > 1023) message = message.Remove(1023);

                Client.Self.OnGroupChatJoin += new AgentManager.GroupChatJoined(Self_OnGroupChatJoin);
                Client.Self.RequestJoinGroupChat(ToGroupID);
                WaitForSessionStart.Reset();
                if (WaitForSessionStart.WaitOne(10000, false))
                {
                    Client.Self.InstantMessageGroup(ToGroupID, message);
                }
                else
                {
                    return "Timeout waiting for group session start";
                }
                Client.Self.OnGroupChatJoin -= new AgentManager.GroupChatJoined(Self_OnGroupChatJoin);
                Client.Self.RequestLeaveGroupChat(ToGroupID);
                return "Instant Messaged group " + ToGroupID.ToString() + " with message: " + message;
            }
            else
            {
                return "failed to instant message group";
            }
        }

        void Self_OnGroupChatJoin(LLUUID groupChatSessionID, LLUUID tmpSessionID, bool success)
        {
            if(success)
            WaitForSessionStart.Set();
        }
    }
}
