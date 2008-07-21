using System;
using System.Collections.Generic;
using System.Threading;
using libsecondlife;
using libsecondlife.Packets;
using System.Text;

namespace libsecondlife.TestClient
{
    public class LeaveGroupCommand : Command
    {
        ManualResetEvent GroupsEvent = new ManualResetEvent(false);
        Dictionary<LLUUID, Group> groups = new Dictionary<LLUUID, Group>();
        private bool leftGroup;

        public LeaveGroupCommand(TestClient testClient)
        {
            Name = "leavegroup";
            Description = "Leave a group. Usage: leavegroup GroupName";
        }
        public override string Execute(string[] args, LLUUID fromAgentID)
        {
            if (args.Length < 1)
                return Description;

            groups.Clear();

            string groupName = String.Empty;
            for (int i = 0; i < args.Length; i++)
                groupName += args[i] + " ";
            groupName = groupName.Trim();

            GroupManager.CurrentGroupsCallback callback = new GroupManager.CurrentGroupsCallback(Groups_OnCurrentGroups);
            Client.Groups.OnCurrentGroups += callback;
            Client.Groups.RequestCurrentGroups();

            GroupsEvent.WaitOne(30000, false);

            Client.Groups.OnCurrentGroups -= callback;
            GroupsEvent.Reset();

            if (groups.Count > 0)
            {
                foreach (Group currentGroup in groups.Values)
                    if (currentGroup.Name.ToLower() == groupName.ToLower())
                    {
                        GroupManager.GroupLeftCallback lcallback = new GroupManager.GroupLeftCallback(Groups_OnGroupLeft);
                        Client.Groups.OnGroupLeft += lcallback;
                        Client.Groups.LeaveGroup(currentGroup.ID);

                        /* A.Biondi 
                         * TODO: modify GroupsCommand.cs
                         * GroupsCommand.cs doesn't refresh the groups list until a new
                         * CurrentGroupsCallback occurs, so if you'd issue the command
                         * 'Groups' right after have left a group, it'll display still yet 
                         * the group you just left (unless you have 0 groups, because it 
                         * would force the refresh with Client.Groups.RequestCurrentGroups).
                         */

                        GroupsEvent.WaitOne(30000, false);

                        Client.Groups.OnGroupLeft -= lcallback;
                        GroupsEvent.Reset();

                        if (leftGroup)
                            return Client.ToString() + " has left the group " + groupName;
                        return "failed to left the group " + groupName;
                    }
                return Client.ToString() + " doesn't seem to be member of the group " + groupName;
            }

            return Client.ToString() + " doesn't seem member of any group";
        }

        void Groups_OnCurrentGroups(Dictionary<LLUUID, Group> cGroups)
        {
            groups = cGroups;
            GroupsEvent.Set();
        }

        void Groups_OnGroupLeft(LLUUID groupID, bool success)
        {
            Console.WriteLine(Client.ToString() + (success ? " has left group " : " failed to left group ") + groupID.ToString());

            leftGroup = success;
            GroupsEvent.Set();
        }
    }
}
