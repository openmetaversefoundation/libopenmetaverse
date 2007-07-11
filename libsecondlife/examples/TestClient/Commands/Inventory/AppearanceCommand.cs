using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class AppearanceCommand : Command
    {
		public AppearanceCommand(TestClient testClient)
        {
            Name = "appearance";
            Description = "Set your current appearance to your last saved appearance";
        }

        public override string Execute(string[] args, LLUUID fromAgentID)
        {
            Client.NewAppearanceManager.SetPreviousAppearance();
            return "Done.";
        }
    }
}
