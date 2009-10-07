using System;
using System.Collections.Generic;
using System.Text;

namespace OpenMetaverse.TestClient.Commands
{
    class SearchClassifiedsCommand : Command
    {
        System.Threading.AutoResetEvent waitQuery = new System.Threading.AutoResetEvent(false);
        int resultCount;

        public SearchClassifiedsCommand(TestClient testClient)
        {
            Name = "searchclassifieds";
            Description = "Searches Classified Ads. Usage: searchclassifieds [search text]";
            Category = CommandCategory.Other;
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            if (args.Length < 1)
                return "Usage: searchclassifieds [search text]";

            string searchText = string.Empty;
            for (int i = 0; i < args.Length; i++)
                searchText += args[i] + " ";
            searchText = searchText.TrimEnd();
            waitQuery.Reset();

            StringBuilder result = new StringBuilder();
            DirectoryManager.ClassifiedReplyCallback callback = delegate(List<DirectoryManager.Classified> classifieds)
            {
                result.AppendFormat("Your search string '{0}' returned {1} classified ads" + System.Environment.NewLine,
                    searchText, classifieds.Count);
                foreach (DirectoryManager.Classified ad in classifieds)
                {
                    result.AppendLine(ad.ToString());
                }

                // classifieds are sent 16 ads at a time
                if (classifieds.Count < 16)
                {
                    waitQuery.Set();
                }
            };

            Client.Directory.OnClassifiedReply += callback;

            UUID searchID = Client.Directory.StartClassifiedSearch(searchText, DirectoryManager.ClassifiedCategories.Any,  DirectoryManager.ClassifiedQueryFlags.Mature | DirectoryManager.ClassifiedQueryFlags.PG);

            if (!waitQuery.WaitOne(20000, false) && Client.Network.Connected)
            {
                result.AppendLine("Timeout waiting for simulator to respond to query.");
            }

            Client.Directory.OnClassifiedReply -= callback;

            return result.ToString();
        }
    }
}
