using System;
using System.Threading;
using OpenMetaverse;

namespace OpenMetaverse.TestClient
{
    /// <summary>
    /// Set avatars current appearance to appearance last stored on simulator
    /// </summary>
    public class AppearanceCommand : Command
    {
		public AppearanceCommand(TestClient testClient)
        {
            Name = "appearance";
            Description = "Set your current appearance to your last saved appearance. Usage: appearance [rebake]";
            Category = CommandCategory.Appearance;
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            Client.Appearance.RequestSetAppearance((args.Length > 0 && args[0].Equals("rebake")));
            return "Appearance sequence started";
        }
    }
}
