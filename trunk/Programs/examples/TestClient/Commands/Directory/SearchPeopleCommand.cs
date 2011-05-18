using System;
using System.Collections.Generic;
using System.Text;

namespace OpenMetaverse.TestClient.Commands
{
    class SearchPeopleCommand : Command
    {
        System.Threading.AutoResetEvent waitQuery = new System.Threading.AutoResetEvent(false);
        int resultCount = 0;

        public SearchPeopleCommand(TestClient testClient)
        {
            Name = "searchpeople";
            Description = "Searches for other avatars. Usage: searchpeople [search text]";
            Category = CommandCategory.Search;
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            // process command line arguments
            if (args.Length < 1)
                return "Usage: searchpeople [search text]";

            string searchText = string.Empty;
            for (int i = 0; i < args.Length; i++)
                searchText += args[i] + " ";
            searchText = searchText.TrimEnd();

            waitQuery.Reset();

            
            Client.Directory.DirPeopleReply += Directory_DirPeople;

            // send the request to the directory manager
            Client.Directory.StartPeopleSearch(searchText, 0);
            
            string result;
            if (waitQuery.WaitOne(20000, false) && Client.Network.Connected)
            {
                result = "Your query '" + searchText + "' matched " + resultCount + " People. ";
            }
            else
            {
                result = "Timeout waiting for simulator to respond.";
            }

            Client.Directory.DirPeopleReply -= Directory_DirPeople;

            return result;
        }

        void Directory_DirPeople(object sender, DirPeopleReplyEventArgs e)
        {
            if (e.MatchedPeople.Count > 0)
            {
                foreach (DirectoryManager.AgentSearchData agent in e.MatchedPeople)
                {
                    Console.WriteLine("{0} {1} ({2})", agent.FirstName, agent.LastName, agent.AgentID);                   
                }
            }
            else
            {
                Console.WriteLine("Didn't find any people that matched your query :(");
            }
            waitQuery.Set();
        }
    }
}
