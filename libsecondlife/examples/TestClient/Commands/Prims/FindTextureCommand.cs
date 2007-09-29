using System;
using libsecondlife;

namespace libsecondlife.TestClient
{
    public class FindTextureCommand : Command
    {
        public FindTextureCommand(TestClient testClient)
        {
            Name = "findtexture";
            Description = "Checks if a specified texture is currently visible on a specified face. " +
                "Usage: findtexture [face-index] [texture-uuid]";
        }

        public override string Execute(string[] args, LLUUID fromAgentID)
        {
            int faceIndex;
            LLUUID textureID;

            if (args.Length != 2)
                return "Usage: findtexture [face-index] [texture-uuid]";

            if (Int32.TryParse(args[0], out faceIndex) &&
                LLUUID.TryParse(args[1], out textureID))
            {
                Client.Network.CurrentSim.Objects.ForEach(
                    delegate(Primitive prim)
                    {
                        if (prim.Textures != null && prim.Textures.FaceTextures[faceIndex] != null)
                        {
                            if (prim.Textures.FaceTextures[faceIndex].TextureID == textureID)
                            {
                                Client.Log(String.Format("Primitive {0} ({1}) has face index {2} set to {3}",
                                    prim.ID.ToStringHyphenated(), prim.LocalID, faceIndex, textureID.ToStringHyphenated()),
                                    Helpers.LogLevel.Info);
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
