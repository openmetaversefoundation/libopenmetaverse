using System;
using System.Collections.Generic;
using System.Threading;
using OpenMetaverse;
using OpenMetaverse.Packets;
using System.Text;

namespace OpenMetaverse.TestClient
{
    public class GroupsCommand : Command
    {        
        ManualResetEvent GetCurrentGroupsEvent = new ManualResetEvent(false);
        Dictionary<UUID, Group> groups = new Dictionary<UUID, Group>();

        public GroupsCommand(TestClient testClient)
        {
            testClient.Groups.OnCurrentGroups += new GroupManager.CurrentGroupsCallback(Groups_OnCurrentGroups);

            Name = "groups";
            Description = "List avatar groups. Usage: groups";
            Category = CommandCategory.Groups;
        }
        public override string Execute(string[] args, UUID fromAgentID)
        {
            if (groups.Count == 0)
            {
                Client.Groups.RequestCurrentGroups();
                GetCurrentGroupsEvent.WaitOne(10000, false);
            }
            if (groups.Count > 0)
            {
                return getGroupsString();
            }
            else
            {
                return "No groups";
            }
        }

        void Groups_OnCurrentGroups(Dictionary<UUID, Group> pGroups)
        {
            groups = pGroups;
            GetCurrentGroupsEvent.Set();
        }
        string getGroupsString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Group group in groups.Values)
            {
                sb.AppendLine(group.Name + " " + group.ID);
                
            }
            
            return sb.ToString();
        }
    }
}
