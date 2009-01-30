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
                "Usage: findtexture [face-index] [texture-Guid]";
            Category = CommandCategory.Objects;
        }

        public override string Execute(string[] args, Guid fromAgentID)
        {
            int faceIndex;
            Guid textureID;

            if (args.Length != 2)
                return "Usage: findtexture [face-index] [texture-Guid]";

            if (Int32.TryParse(args[0], out faceIndex) &&
                GuidExtensions.TryParse(args[1], out textureID))
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
                return "Usage: findtexture [face-index] [texture-Guid]";
            }
        }
    }
}
