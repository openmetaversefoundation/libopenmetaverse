using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;
using libsecondlife.AssetSystem;

namespace libsecondlife.TestClient
{
    public class SetAppearanceCommand : Command
    {
		public SetAppearanceCommand(TestClient testClient)
        {
            Name = "setapp";
            Description = "Set appearance to what's stored in the DB.";
        }

        public override string Execute(string[] args, LLUUID fromAgentID)
        {
			Client.Appearance.SendAgentSetAppearance();
            return "Done.";
        }
    }
}