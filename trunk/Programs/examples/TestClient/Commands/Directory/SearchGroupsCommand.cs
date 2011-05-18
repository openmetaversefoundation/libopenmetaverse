using System;
using System.Collections.Generic;
using System.Text;

namespace OpenMetaverse.TestClient.Commands
{
    class SearchGroupsCommand : Command
    {
        System.Threading.AutoResetEvent waitQuery = new System.Threading.AutoResetEvent(false);
        int resultCount = 0;

        public SearchGroupsCommand(TestClient testClient)
        {
            Name = "searchgroups";
            Description = "Searches groups. Usage: searchgroups [search text]";
            Category = CommandCategory.Groups;
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            // process command line arguments
            if (args.Length < 1)
                return "Usage: searchgroups [search text]";

            string searchText = string.Empty;
            for (int i = 0; i < args.Length; i++)
                searchText += args[i] + " ";
            searchText = searchText.TrimEnd();

            waitQuery.Reset();

            Client.Directory.DirGroupsReply += Directory_DirGroups;
            
            // send the request to the directory manager
            Client.Directory.StartGroupSearch(searchText, 0);
            
            string result;
            if (waitQuery.WaitOne(20000, false) && Client.Network.Connected)
            {
                result = "Your query '" + searchText + "' matched " + resultCount + " Groups. ";
            }
            else
            {
                result = "Timeout waiting for simulator to respond.";
            }

            Client.Directory.DirGroupsReply -= Directory_DirGroups;

            return result;
        }

        void Directory_DirGroups(object sender, DirGroupsReplyEventArgs e)
        {
            if (e.MatchedGroups.Count > 0)
            {
                foreach (DirectoryManager.GroupSearchData group in e.MatchedGroups)
                {
                    Console.WriteLine("Group {1} ({0}) has {2} members", group.GroupID, group.GroupName, group.Members);
                }
            }
            else
            {
                Console.WriteLine("Didn't find any groups that matched your query :(");
            }
            waitQuery.Set();
        }        
    }
}
