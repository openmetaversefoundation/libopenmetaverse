using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class WearCommand : Command
    {
		public WearCommand(TestClient testClient)
        {
            Client = testClient;
            Name = "wear";
            Description = "Wear an outfit folder from inventory. Usage: wear [outfit name] [nobake]";
        }

        public override string Execute(string[] args, LLUUID fromAgentID)
        {
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

            try
            {
                Client.Appearance.WearOutfit(target.Split('/'), bake);
            }
            catch (InvalidOutfitException ex)
            {
                return "Invalid outfit (" + ex.Message + ")";
            }

            return String.Empty;
        }
    }
}
