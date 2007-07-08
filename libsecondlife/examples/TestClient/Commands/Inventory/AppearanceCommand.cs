using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class AppearanceCommand : Command
    {
        AssetManager Assets;
        AppearanceManager Appearance;

		public AppearanceCommand(TestClient testClient)
        {
            Name = "appearance";
            Description = "Set your current appearance to your last saved appearance";

            Assets = new AssetManager(testClient);
            Appearance = new AppearanceManager(testClient, Assets);
        }

        public override string Execute(string[] args, LLUUID fromAgentID)
        {
            Appearance.SetPreviousAppearance();
            return "Done.";
        }
    }
}
