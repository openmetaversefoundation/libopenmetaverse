using System;
using System.Collections.Generic;
using System.Threading;
using libsecondlife;
using libsecondlife.Packets;
using System.Text;

namespace libsecondlife.TestClient
{
    /// <summary>
    /// Changes Avatars currently active group
    /// </summary>
    public class ActivateGroupCommand : Command
    {
        ManualResetEvent GroupsEvent = new ManualResetEvent(false);
        Dictionary<LLUUID, Group> groups = new Dictionary<LLUUID, Group>();
        string activeGroup;

        public ActivateGroupCommand(TestClient testClient)
        {
            Name = "activategroup";
            Description = "Set a group as active. Usage: activategroup GroupName";
        }
        public override string Execute(string[] args, LLUUID fromAgentID)
        {
            if (args.Length < 1)
                return Description;

            groups.Clear();
            activeGroup = string.Empty;

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
                        NetworkManager.PacketCallback pcallback = new NetworkManager.PacketCallback(AgentDataUpdateHandler);
                        Client.Network.RegisterCallback(PacketType.AgentDataUpdate, pcallback);

                        Console.WriteLine("setting " + currentGroup.Name + " as active group");
                        Client.Groups.ActivateGroup(currentGroup.ID);
                        GroupsEvent.WaitOne(30000, false);

                        Client.Network.UnregisterCallback(PacketType.AgentDataUpdate, pcallback);
                        GroupsEvent.Reset();

                        /* A.Biondi 
                         * TODO: Handle titles choosing.
                         */

                        if (String.IsNullOrEmpty(activeGroup))
                            return Client.ToString() + " failed to activate the group " + groupName;

                        return "Active group is now " + activeGroup;
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

        private void AgentDataUpdateHandler(Packet packet, Simulator sim)
        {
            AgentDataUpdatePacket p = (AgentDataUpdatePacket)packet;
            if (p.AgentData.AgentID == Client.Self.AgentID)
            {
                activeGroup = Helpers.FieldToUTF8String(p.AgentData.GroupName) + " ( " + Helpers.FieldToUTF8String(p.AgentData.GroupTitle) + " )";
                GroupsEvent.Set();
            }
        }
    }
}
