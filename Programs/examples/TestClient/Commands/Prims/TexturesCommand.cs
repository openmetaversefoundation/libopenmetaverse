using System;
using System.Collections.Generic;
using OpenMetaverse;

namespace OpenMetaverse.TestClient
{
    public class TexturesCommand : Command
    {
        Dictionary<UUID, UUID> alreadyRequested = new Dictionary<UUID, UUID>();
        bool enabled = false;

        public TexturesCommand(TestClient testClient)
        {
            Name = "textures";
            Description = "Turns automatic texture downloading on or off. Usage: textures [on/off]";
            Category = CommandCategory.Objects;

            testClient.Objects.OnNewPrim += new ObjectManager.NewPrimCallback(Objects_OnNewPrim);
            testClient.Objects.OnNewAvatar += new ObjectManager.NewAvatarCallback(Objects_OnNewAvatar);
            testClient.Assets.OnImageReceived += new AssetManager.ImageReceivedCallback(Assets_OnImageReceived);
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            if (args.Length != 1)
                return "Usage: textures [on/off]";

            if (args[0].ToLower() == "on")
            {
                enabled = true;
                return "Texture downloading is on";
            }
            else if (args[0].ToLower() == "off")
            {
                enabled = false;
                return "Texture downloading is off";
            }
            else
            {
                return "Usage: textures [on/off]";
            }
        }

        void Objects_OnNewAvatar(Simulator simulator, Avatar avatar, ulong regionHandle, ushort timeDilation)
        {
            if (enabled)
            {
                // Search this avatar for textures
                for (int i = 0; i < avatar.Textures.FaceTextures.Length; i++)
                {
                    Primitive.TextureEntryFace face = avatar.Textures.FaceTextures[i];

                    if (face != null)
                    {
                        if (!alreadyRequested.ContainsKey(face.TextureID))
                        {
                            alreadyRequested[face.TextureID] = face.TextureID;

                            // Determine if this is a baked outfit texture or a normal texture
                            ImageType type = ImageType.Normal;
                            AppearanceManager.TextureIndex index = (AppearanceManager.TextureIndex)i;
                            switch (index)
                            {
                                case AppearanceManager.TextureIndex.EyesBaked:
                                case AppearanceManager.TextureIndex.HeadBaked:
                                case AppearanceManager.TextureIndex.LowerBaked:
                                case AppearanceManager.TextureIndex.SkirtBaked:
                                case AppearanceManager.TextureIndex.UpperBaked:
                                    type = ImageType.Baked;
                                    break;
                            }

                            Client.Assets.RequestImage(face.TextureID, type);
                        }
                    }
                }
            }
        }

        void Objects_OnNewPrim(Simulator simulator, Primitive prim, ulong regionHandle, ushort timeDilation)
        {
            if (enabled)
            {
                // Search this prim for textures
                for (int i = 0; i < prim.Textures.FaceTextures.Length; i++)
                {
                    Primitive.TextureEntryFace face = prim.Textures.FaceTextures[i];

                    if (face != null)
                    {
                        if (!alreadyRequested.ContainsKey(face.TextureID))
                        {
                            alreadyRequested[face.TextureID] = face.TextureID;
                            Client.Assets.RequestImage(face.TextureID, ImageType.Normal);
                        }
                    }
                }
            }
        }

        private void Assets_OnImageReceived(ImageDownload image, AssetTexture asset)
        {
            if (enabled && alreadyRequested.ContainsKey(image.ID))
            {
                if (image.Success)
                    Logger.DebugLog(String.Format("Finished downloading texture {0} ({1} bytes)", image.ID, image.Size));
                else
                    Logger.Log("Failed to download texture " + image.ID.ToString(), Helpers.LogLevel.Warning);
            }
        }
    }
}
