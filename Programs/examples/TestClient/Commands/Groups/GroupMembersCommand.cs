using System;
using System.Collections.Generic;
using System.Threading;
using OpenMetaverse;
using OpenMetaverse.Packets;
using System.Text;

namespace OpenMetaverse.TestClient
{
    /// <summary>
    /// dumps group members to console
    /// </summary>
    public class GroupMembersCommand : Command
    {
            private ManualResetEvent GroupsEvent = new ManualResetEvent(false);
        private string GroupName;
        private UUID GroupUUID;
        private UUID GroupRequestID;

        public GroupMembersCommand(TestClient testClient)
        {
            Name = "groupmembers";
            Description = "Dump group members to console. Usage: groupmembers GroupName";
            Category = CommandCategory.Groups;
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            if (args.Length < 1)
                return Description;

            GroupName = String.Empty;
            for (int i = 0; i < args.Length; i++)
                GroupName += args[i] + " ";
            GroupName = GroupName.Trim();

            GroupUUID = Client.GroupName2UUID(GroupName);
            if (UUID.Zero != GroupUUID) {
                GroupManager.GroupMembersCallback callback =
                    new GroupManager.GroupMembersCallback(GroupMembersHandler);
                Client.Groups.OnGroupMembers += callback;
                GroupRequestID = Client.Groups.RequestGroupMembers(GroupUUID);
                GroupsEvent.WaitOne(30000, false);
                GroupsEvent.Reset();
                Client.Groups.OnGroupMembers -= callback;
                return Client.ToString() + " got group members";
            }
            return Client.ToString() + " doesn't seem to be member of the group " + GroupName;
        }

        private void GroupMembersHandler(UUID requestID, UUID groupID, Dictionary<UUID, GroupMember> members)
        {
            if (requestID == GroupRequestID) {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine();
                sb.AppendFormat("GroupMembers: RequestID {0}", requestID).AppendLine();
                sb.AppendFormat("GroupMembers: GroupUUID {0}", GroupUUID).AppendLine();
                sb.AppendFormat("GroupMembers: GroupName {0}", GroupName).AppendLine();
                if (members.Count > 0)
                    foreach (KeyValuePair<UUID, GroupMember> member in members)
                        sb.AppendFormat("GroupMembers: MemberUUID {0}", member.Key.ToString()).AppendLine();
                sb.AppendFormat("GroupMembers: MemberCount {0}", members.Count).AppendLine();
                Console.WriteLine(sb.ToString());
                GroupsEvent.Set();
            } 
        }
    }
}
