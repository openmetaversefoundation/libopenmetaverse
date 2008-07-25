using System;
using OpenMetaverse;

namespace OpenMetaverse.TestClient
{
    public class PrimInfoCommand : Command
    {
        public PrimInfoCommand(TestClient testClient)
        {
            Name = "priminfo";
            Description = "Dumps information about a specified prim. " + "Usage: priminfo [prim-uuid]";
            Category = CommandCategory.Objects;
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            UUID primID;

            if (args.Length != 1)
                return "Usage: priminfo [prim-uuid]";

            if (UUID.TryParse(args[0], out primID))
            {
                Primitive target = Client.Network.CurrentSim.ObjectsPrimitives.Find(
                    delegate(Primitive prim) { return prim.ID == primID; }
                );

                if (target != null)
                {
                    Logger.Log("Light: " + target.Light.ToString(), Helpers.LogLevel.Info, Client);

                    if (target.ParticleSys.CRC != 0)
                        Logger.Log("Particles: " + target.ParticleSys.ToString(), Helpers.LogLevel.Info, Client);

                    Logger.Log("TextureEntry:", Helpers.LogLevel.Info, Client);
                    if (target.Textures != null)
                    {
                        Logger.Log(String.Format("Default texure: {0}",
                            target.Textures.DefaultTexture.TextureID.ToString()),
                            Helpers.LogLevel.Info);

                        for (int i = 0; i < target.Textures.FaceTextures.Length; i++)
                        {
                            if (target.Textures.FaceTextures[i] != null)
                            {
                                Logger.Log(String.Format("Face {0}: {1}", i,
                                    target.Textures.FaceTextures[i].TextureID.ToString()),
                                    Helpers.LogLevel.Info, Client);
                            }
                        }
                    }
                    else
                    {
                        Logger.Log("null", Helpers.LogLevel.Info, Client);
                    }

                    return "Done.";
                }
                else
                {
                    return "Could not find prim " + primID.ToString();
                }
            }
            else
            {
                return "Usage: priminfo [prim-uuid]";
            }
        }
    }
}
