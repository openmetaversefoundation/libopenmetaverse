using System;
using OpenMetaverse;

namespace OpenMetaverse.TestClient
{
    public class FindTextureCommand : Command
    {
        public FindTextureCommand(TestClient testClient)
        {
            Name = "findtexture";
            Description = "Checks if a specified texture is currently visible on a specified face. " +
                "Usage: findtexture [face-index] [texture-uuid]";
            Category = CommandCategory.Objects;
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            int faceIndex;
            UUID textureID;

            if (args.Length != 2)
                return "Usage: findtexture [face-index] [texture-uuid]";

            if (Int32.TryParse(args[0], out faceIndex) &&
                UUID.TryParse(args[1], out textureID))
            {
                Client.Network.CurrentSim.ObjectsPrimitives.ForEach(
                    delegate(Primitive prim)
                    {
                        if (prim.Textures != null && prim.Textures.FaceTextures[faceIndex] != null)
                        {
                            if (prim.Textures.FaceTextures[faceIndex].TextureID == textureID)
                            {
                                Logger.Log(String.Format("Primitive {0} ({1}) has face index {2} set to {3}",
                                    prim.ID.ToString(), prim.LocalID, faceIndex, textureID.ToString()),
                                    Helpers.LogLevel.Info, Client);
                            }
                        }
                    }
                );

                return "Done searching";
            }
            else
            {
                return "Usage: findtexture [face-index] [texture-uuid]";
            }
        }
    }
}
