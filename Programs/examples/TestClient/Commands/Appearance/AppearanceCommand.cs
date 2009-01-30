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
            Description = "Set your current appearance to your last saved appearance";
            Category = CommandCategory.Appearance;
        }

        public override string Execute(string[] args, Guid fromAgentID)
        {
            bool success = false;

            // Register a handler for the appearance event
            AutoResetEvent appearanceEvent = new AutoResetEvent(false);
            AppearanceManager.AppearanceUpdatedCallback callback =
                delegate(Primitive.TextureEntry te) { appearanceEvent.Set(); };
            Client.Appearance.OnAppearanceUpdated += callback;

            // Start the appearance setting process (with baking enabled or disabled)
            Client.Appearance.SetPreviousAppearance(!(args.Length > 0 && args[0].Equals("nobake")));

            // Wait for the process to complete or time out
            if (appearanceEvent.WaitOne(1000 * 120, false))
                success = true;

            // Unregister the handler
            Client.Appearance.OnAppearanceUpdated -= callback;

            // Return success or failure message
            if (success)
                return "Successfully set appearance";
            else
                return "Timed out while setting appearance";
        }
    }
}
