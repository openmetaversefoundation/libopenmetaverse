using System;
using System.Collections.Generic;
using System.Threading;
using OpenMetaverse;
using OpenMetaverse.Packets;
using System.Text;

namespace OpenMetaverse.TestClient
{
    /// <summary>
    /// dumps group roles to console
    /// </summary>
    public class GroupRolesCommand : Command
    {
            private ManualResetEvent GroupsEvent = new ManualResetEvent(false);
        private string GroupName;
        private UUID GroupUUID;
        private UUID GroupRequestID;

        public GroupRolesCommand(TestClient testClient)
        {
            Name = "grouproles";
            Description = "Dump group roles to console. Usage: grouproles GroupName";
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
                Client.Groups.GroupRoleDataReply += Groups_GroupRoles;                
                GroupRequestID = Client.Groups.RequestGroupRoles(GroupUUID);
                GroupsEvent.WaitOne(30000, false);
                GroupsEvent.Reset();
                Client.Groups.GroupRoleDataReply += Groups_GroupRoles;
                return Client.ToString() + " got group roles";
            }
            return Client.ToString() + " doesn't seem to have any roles in the group " + GroupName;
        }

        void Groups_GroupRoles(object sender, GroupRolesDataReplyEventArgs e)
        {
            if (e.RequestID == GroupRequestID)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine();
                sb.AppendFormat("GroupRole: RequestID {0}", e.RequestID).AppendLine();
                sb.AppendFormat("GroupRole: GroupUUID {0}", GroupUUID).AppendLine();
                sb.AppendFormat("GroupRole: GroupName {0}", GroupName).AppendLine();
                if (e.Roles.Count > 0)
                    foreach (KeyValuePair<UUID, GroupRole> role in e.Roles)
                        sb.AppendFormat("GroupRole: Role {0} {1}|{2}", role.Value.ID, role.Value.Name, role.Value.Title).AppendLine();
                sb.AppendFormat("GroupRole: RoleCount {0}", e.Roles.Count).AppendLine();
                Console.WriteLine(sb.ToString());
                GroupsEvent.Set();
            } 
        }       
    }
}
