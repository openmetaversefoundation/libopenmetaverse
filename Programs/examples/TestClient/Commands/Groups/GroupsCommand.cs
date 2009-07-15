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
        public GroupsCommand(TestClient testClient)
        {
            Name = "groups";
            Description = "List avatar groups. Usage: groups";
            Category = CommandCategory.Groups;
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            Client.ReloadGroupsCache();
            return getGroupsString();
        }

        string getGroupsString()
        {
            if (null == Client.GroupsCache)
                    return "Groups cache failed.";
            if (0 == Client.GroupsCache.Count)
                    return "No groups";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("got "+Client.GroupsCache.Count +" groups:");
            foreach (Group group in Client.GroupsCache.Values)
            {
                sb.AppendLine(group.ID + ", " + group.Name);
                
            }
            
            return sb.ToString();
        }
    }
}
