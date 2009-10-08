using System;
using System.Collections.Generic;
using System.Text;

namespace OpenMetaverse.TestClient.Commands
{
    class SearchPlacesCommand : Command
    {
        System.Threading.AutoResetEvent waitQuery = new System.Threading.AutoResetEvent(false);
        int resultCount;

        public SearchPlacesCommand(TestClient testClient)
        {
            Name = "searchplaces";
            Description = "Searches Places. Usage: searchplaces [search text]";
            Category = CommandCategory.Other;
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            if (args.Length < 1)
                return "Usage: searchplaces [search text]";

            string searchText = string.Empty;
            for (int i = 0; i < args.Length; i++)
                searchText += args[i] + " ";
            searchText = searchText.TrimEnd();
            waitQuery.Reset();

            StringBuilder result = new StringBuilder();

            DirectoryManager.DirPlacesReplyCallback callback = delegate(UUID queryID, List<DirectoryManager.DirectoryParcel> matchedParcels)
            {
                result.AppendFormat("Your search string '{0}' returned {1} results" + System.Environment.NewLine,
                    searchText, matchedParcels.Count);
                foreach (DirectoryManager.DirectoryParcel place in matchedParcels)
                {
                    result.AppendLine(place.ToString());
                }

                waitQuery.Set();
            };

            Client.Directory.OnDirPlacesReply += callback;
            
            UUID searchID = Client.Directory.StartClassifiedSearch(searchText, DirectoryManager.ClassifiedCategories.Any, DirectoryManager.ClassifiedQueryFlags.Mature | DirectoryManager.ClassifiedQueryFlags.PG);

            if (!waitQuery.WaitOne(20000, false) && Client.Network.Connected)
            {
                result.AppendLine("Timeout waiting for simulator to respond to query.");
            }

            Client.Directory.OnDirPlacesReply -= callback;

            return result.ToString();
        }
    }
}
