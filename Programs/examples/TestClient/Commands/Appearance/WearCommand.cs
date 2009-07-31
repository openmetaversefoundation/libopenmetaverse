using System;
using OpenMetaverse;

namespace OpenMetaverse.TestClient
{
    public class WearCommand : Command
    {
        public WearCommand(TestClient testClient)
        {
            Client = testClient;
            Name = "wear";
            Description = "Wear an outfit folder from inventory. Usage: wear [outfit name] [nobake]";
            Category = CommandCategory.Appearance;
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            if (args.Length < 1)
                return "Usage: wear [outfit name] eg: 'wear /My Outfit/Dance Party";

            string target = String.Empty;
            bool bake = true;

            for (int ct = 0; ct < args.Length; ct++)
            {
                if (args[ct].Equals("nobake"))
                    bake = false;
                else
                    target = target + args[ct] + " ";
            }

            target = target.TrimEnd();

            //Client.Appearance.WearOutfit(target.Split('/'), bake);

            return "FIXME: Implement this";
        }
    }
}
