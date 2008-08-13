using System;
using System.Collections.Generic;
using System.Text;

namespace OpenMetaverse.TestClient.Commands
{
    class SearchEventsCommand : Command
    {
        System.Threading.AutoResetEvent waitQuery = new System.Threading.AutoResetEvent(false);
        int resultCount;

        public SearchEventsCommand(TestClient testClient)
        {
            Name = "searchevents";
            Description = "Searches Events list. Usage: searchevents [search text]";
            Category = CommandCategory.Other;
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            if (args.Length < 1)
                return "Usage: searchevents [search text]";

            string searchText = string.Empty;
            for (int i = 0; i < args.Length; i++)
                searchText += args[i] + " ";
            searchText = searchText.TrimEnd();
            waitQuery.Reset();

            Client.Directory.OnEventsReply += new DirectoryManager.EventReplyCallback(Directory_OnEventsReply);
            Client.Directory.StartEventsSearch(searchText, true, "u", 0, DirectoryManager.EventCategories.All, UUID.Random());
            string result;
            if (waitQuery.WaitOne(20000, false) && Client.Network.Connected)
            {
                result =  "Your query '" + searchText + "' matched " + resultCount + " Events. ";
            }
            else
            {
                result =  "Timeout waiting for simulator to respond.";
            }            
            Client.Directory.OnEventsReply -= new DirectoryManager.EventReplyCallback(Directory_OnEventsReply);
            return result;
        }

        void Directory_OnEventsReply(UUID queryID, List<DirectoryManager.EventsSearchData> matchedEvents)
        {
            if (matchedEvents[0].ID == 0 && matchedEvents.Count == 1)
            {
                Console.WriteLine("No Results matched your search string");
            }
            else
            {
                foreach (DirectoryManager.EventsSearchData ev in matchedEvents)
                {
                    Console.WriteLine("Event ID: {0} Event Name: {1} Event Date: {2}", ev.ID, ev.Name, ev.Date);
                }
            }
            resultCount = matchedEvents.Count;
            waitQuery.Set();
        }
    }
}
