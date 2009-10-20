using System;
using System.Collections.Generic;
using System.Threading;
using OpenMetaverse;
using OpenMetaverse.Packets;
using System.Text;

namespace OpenMetaverse.TestClient
{
    public class LeaveGroupCommand : Command
    {
        ManualResetEvent GroupsEvent = new ManualResetEvent(false);
        private bool leftGroup;

        public LeaveGroupCommand(TestClient testClient)
        {
            Name = "leavegroup";
            Description = "Leave a group. Usage: leavegroup GroupName";
            Category = CommandCategory.Groups;
        }
        public override string Execute(string[] args, UUID fromAgentID)
        {
            if (args.Length < 1)
                return Description;

            string groupName = String.Empty;
            for (int i = 0; i < args.Length; i++)
                groupName += args[i] + " ";
            groupName = groupName.Trim();

            UUID groupUUID = Client.GroupName2UUID(groupName);
            if (UUID.Zero != groupUUID) {                
                Client.Groups.GroupLeaveReply += Groups_GroupLeft;
                Client.Groups.LeaveGroup(groupUUID);

                GroupsEvent.WaitOne(30000, false);
                Client.Groups.GroupLeaveReply -= Groups_GroupLeft;

                GroupsEvent.Reset();
                Client.ReloadGroupsCache();

                if (leftGroup)
                    return Client.ToString() + " has left the group " + groupName;
                return "failed to leave the group " + groupName;
            }
            return Client.ToString() + " doesn't seem to be member of the group " + groupName;
        }

        void Groups_GroupLeft(object sender, GroupOperationEventArgs e)
        {
            Console.WriteLine(Client.ToString() + (e.Success ? " has left group " : " failed to left group ") + e.GroupID.ToString());

            leftGroup = e.Success;
            GroupsEvent.Set();
        }

    }
}
