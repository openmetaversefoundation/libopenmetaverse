using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;

namespace OpenMetaverse.TestClient.Commands.Appearance
{
    public class AvatarInfoCommand : Command
    {
        public AvatarInfoCommand(TestClient testClient)
        {
            Name = "avatarinfo";
            Description = "Print out information on a nearby avatar. Usage: avatarinfo [firstname] [lastname]";
            Category = CommandCategory.Appearance;
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            if (args.Length != 2)
                return "Usage: avatarinfo [firstname] [lastname]";

            string targetName = String.Format("{0} {1}", args[0], args[1]);

            Avatar foundAv = Client.Network.CurrentSim.ObjectsAvatars.Find(
                delegate(Avatar avatar) { return (avatar.Name == targetName); }
            );

            if (foundAv != null)
            {
                StringBuilder output = new StringBuilder();

                output.AppendFormat("{0} ({1})", targetName, foundAv.ID);
                output.AppendLine();

                for (int i = 0; i < foundAv.Textures.FaceTextures.Length; i++)
                {
                    if (foundAv.Textures.FaceTextures[i] != null)
                    {
                        LLObject.TextureEntryFace face = foundAv.Textures.FaceTextures[i];
                        AppearanceManager.TextureIndex type = (AppearanceManager.TextureIndex)i;

                        output.AppendFormat("{0}: {1}", type, face.TextureID);
                        output.AppendLine();
                    }
                }

                return output.ToString();
            }
            else
            {
                return "No nearby avatar with the name " + targetName;
            }
        }
    }
}
