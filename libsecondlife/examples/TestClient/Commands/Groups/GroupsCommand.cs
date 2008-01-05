using System;
using System.Collections.Generic;
using System.Threading;
using libsecondlife;
using libsecondlife.Packets;
using System.Text;

namespace libsecondlife.TestClient
{
    public class GroupsCommand : Command
    {        
        ManualResetEvent GetCurrentGroupsEvent = new ManualResetEvent(false);
        Dictionary<LLUUID, Group> groups = new Dictionary<LLUUID, Group>();

        public GroupsCommand(TestClient testClient)
        {
            testClient.Groups.OnCurrentGroups += new GroupManager.CurrentGroupsCallback(Groups_OnCurrentGroups);

            Name = "groups";
            Description = "List avatar groups. Usage: groups";
        }
        public override string Execute(string[] args, LLUUID fromAgentID)
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

        void Groups_OnCurrentGroups(Dictionary<LLUUID, Group> pGroups)
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
