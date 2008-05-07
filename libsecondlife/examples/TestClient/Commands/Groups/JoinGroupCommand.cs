using System;
using System.Collections.Generic;
using System.Threading;
using libsecondlife;
using libsecondlife.Packets;
using System.Text;

namespace libsecondlife.TestClient
{
    public class JoinGroupCommand : Command
    {
        ManualResetEvent GetGroupsSearchEvent = new ManualResetEvent(false);
        private LLUUID queryID = LLUUID.Zero;
        private LLUUID resolvedGroupID;
        private string groupName;
        private string resolvedGroupName;
        private bool joinedGroup;

        public JoinGroupCommand(TestClient testClient)
        {
            Name = "joingroup";
            Description = "join a group. Usage: joingroup GroupName | joingroup UUID GroupId";
        }

        public override string Execute(string[] args, LLUUID fromAgentID)
        {
            if (args.Length < 1)
                return Description;

            groupName = String.Empty;
            resolvedGroupID = LLUUID.Zero;
            resolvedGroupName = String.Empty;

            if (args[0].ToLower() == "uuid")
            {
                if (args.Length < 2)
                    return Description;

                if (!LLUUID.TryParse((resolvedGroupName = groupName = args[1]), out resolvedGroupID))
                    return resolvedGroupName + " doesn't seem a valid UUID";
            }
            else
            {
                for (int i = 0; i < args.Length; i++)
                    groupName += args[i] + " ";
                groupName = groupName.Trim();
                DirectoryManager.DirGroupsReplyCallback callback = new DirectoryManager.DirGroupsReplyCallback(Directory_OnDirGroupsReply);
                Client.Directory.OnDirGroupsReply += callback;
                queryID = Client.Directory.StartGroupSearch(DirectoryManager.DirFindFlags.Groups, groupName, 0);

                GetGroupsSearchEvent.WaitOne(60000, false);

                Client.Directory.OnDirGroupsReply -= callback;
                GetGroupsSearchEvent.Reset();
            }

            if (resolvedGroupID == LLUUID.Zero)
            {
                if (string.IsNullOrEmpty(resolvedGroupName))
                    return "Unable to obtain UUID for group " + groupName;
                else
                    return resolvedGroupName;
            }

            GroupManager.GroupJoinedCallback gcallback = new GroupManager.GroupJoinedCallback(Groups_OnGroupJoined);
            Client.Groups.OnGroupJoined += gcallback;
            Client.Groups.RequestJoinGroup(resolvedGroupID);

            /* A.Biondi 
             * TODO: implement the pay to join procedure.
             */

            GetGroupsSearchEvent.WaitOne(60000, false);

            Client.Groups.OnGroupJoined -= gcallback;
            GetGroupsSearchEvent.Reset();

            if (joinedGroup)
                return "Joined the group " + resolvedGroupName;
            return "Unable to join the group " + resolvedGroupName;
        }

        void Groups_OnGroupJoined(LLUUID groupID, bool success)
        {
            Console.WriteLine(Client.ToString() + (success ? " joined " : " failed to join ") + groupID.ToString());

            /* A.Biondi 
             * This code is not necessary because it is yet present in the 
             * GroupCommand.cs as well. So the new group will be activated by 
             * the mentioned command. If the GroupCommand.cs would change, 
             * just uncomment the following two lines.
                
            if (success)
            {
                Console.WriteLine(Client.ToString() + " setting " + groupID.ToString() + " as the active group");
                Client.Groups.ActivateGroup(groupID);
            }
                
            */

            joinedGroup = success;
            GetGroupsSearchEvent.Set();
        }

        void Directory_OnDirGroupsReply(LLUUID queryid, List<DirectoryManager.GroupSearchData> matchedGroups)
        {
            if (queryID == queryid)
            {
                queryID = LLUUID.Zero;
                if (matchedGroups.Count < 1)
                {
                    Console.WriteLine("ERROR: Got an empty reply");
                }
                else
                {
                    if (matchedGroups.Count > 1)
                    {
                        /* A.Biondi 
                         * The Group search doesn't work as someone could expect...
                         * It'll give back to you a long list of groups even if the 
                         * searchText (groupName) matches esactly one of the groups 
                         * names present on the server, so we need to check each result.
                         * UUIDs of the matching groups are written on the console.
                         */
                        Console.WriteLine("Matching groups are:\n");
                        foreach (DirectoryManager.GroupSearchData groupRetrieved in matchedGroups)
                        {
                            Console.WriteLine(groupRetrieved.GroupName + "\t\t\t(" +
                                Name + " UUID " + groupRetrieved.GroupID.ToString() + ")");

                            if (groupRetrieved.GroupName.ToLower() == groupName.ToLower())
                            {
                                resolvedGroupID = groupRetrieved.GroupID;
                                resolvedGroupName = groupRetrieved.GroupName;
                                break;
                            }
                        }
                        if (string.IsNullOrEmpty(resolvedGroupName))
                            resolvedGroupName = "Ambiguous name. Found " + matchedGroups.Count.ToString() + " groups (UUIDs on console)";
                    }

                }
                GetGroupsSearchEvent.Set();
            }
        }
    }
}
