using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

using libsecondlife;
using libsecondlife.Packets;
using libsecondlife.InventorySystem;

namespace libsecondlife.TestClient
{
    public class WearCommand : Command
    {
		public WearCommand(TestClient testClient)
        {
            Client = testClient;
            Name = "wear";
            Description = "Wear an outfit folder from inventory. Usage: wear [outfit name]";
        }

        public override string Execute(string[] args, LLUUID fromAgentID)
        {
            string target = String.Empty;
            bool oldcode = false;


            for (int ct = 0; ct < args.Length; ct++)
            {
                if (args[ct] == "oldcode")
                    oldcode = true;
                else
                    target = target + args[ct] + " ";
            }
            
            target = target.TrimEnd();

            InventoryFolder folder = Client.Inventory.getFolder(target);
            
            if (folder != null)
            {
                //libsecondlife.Utilities.Appearance.AppearanceManager am = new libsecondlife.Utilities.Appearance.AppearanceManager(Client, new Utilities.Assets.AssetManager(Client));
                
                if (oldcode)
                    Client.Appearance.WearOutfit(folder);
                else
                    Client.NewAppearanceManager.WearOutfit(folder);

                return "Outfit " + target + " worn.";
            }

            return "Unable to find: " + target;
        }
    }
}
