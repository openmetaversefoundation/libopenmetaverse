using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using OpenMetaverse;
using OpenMetaverse.Imaging;

namespace OpenMetaverse.TestClient
{
    public class DumpOutfitCommand : Command
    {
        List<Guid> OutfitAssets = new List<Guid>();
        AssetManager.ImageReceivedCallback ImageReceivedHandler;

        public DumpOutfitCommand(TestClient testClient)
        {
            Name = "dumpoutfit";
            Description = "Dumps all of the textures from an avatars outfit to the hard drive. Usage: dumpoutfit [avatar-Guid]";
            Category = CommandCategory.Inventory;

            ImageReceivedHandler = new AssetManager.ImageReceivedCallback(Assets_OnImageReceived);
        }

        public override string Execute(string[] args, Guid fromAgentID)
        {
            if (args.Length != 1)
                return "Usage: dumpoutfit [avatar-Guid]";

            Guid target;

            if (!GuidExtensions.TryParse(args[0], out target))
                return "Usage: dumpoutfit [avatar-Guid]";

            lock (Client.Network.Simulators)
            {
                for (int i = 0; i < Client.Network.Simulators.Count; i++)
                {
                    Avatar targetAv;

                    targetAv = Client.Network.Simulators[i].ObjectsAvatars.Find(
                        delegate(Avatar avatar)
                        {
                            return avatar.ID == target;
                        }
                    );

                    if (targetAv != null)
                    {
                        StringBuilder output = new StringBuilder("Downloading ");

                        lock (OutfitAssets) OutfitAssets.Clear();
                        Client.Assets.OnImageReceived += ImageReceivedHandler;

                        for (int j = 0; j < targetAv.Textures.FaceTextures.Length; j++)
                        {
                            Primitive.TextureEntryFace face = targetAv.Textures.FaceTextures[j];

                            if (face != null)
                            {
                                ImageType type = ImageType.Normal;

                                switch ((AppearanceManager.TextureIndex)j)
                                {
                                    case AppearanceManager.TextureIndex.HeadBaked:
                                    case AppearanceManager.TextureIndex.EyesBaked:
                                    case AppearanceManager.TextureIndex.UpperBaked:
                                    case AppearanceManager.TextureIndex.LowerBaked:
                                    case AppearanceManager.TextureIndex.SkirtBaked:
                                        type = ImageType.Baked;
                                        break;
                                }

                                OutfitAssets.Add(face.TextureID);
                                Client.Assets.RequestImage(face.TextureID, type, 100000.0f, 0, 0);

                                output.Append(((AppearanceManager.TextureIndex)j).ToString());
                                output.Append(" ");
                            }
                        }

                        return output.ToString();
                    }
                }
            }

            return "Couldn't find avatar " + target.ToString();
        }

        private void Assets_OnImageReceived(ImageDownload image, AssetTexture assetTexture)
        {
            lock (OutfitAssets)
            {
                if (OutfitAssets.Contains(image.ID))
                {
                    if (image.Success)
                    {
                        try
                        {
                            File.WriteAllBytes(image.ID.ToString() + ".jp2", image.AssetData);
                            Console.WriteLine("Wrote JPEG2000 image " + image.ID.ToString() + ".jp2");

                            ManagedImage imgData;
                            OpenJPEG.DecodeToImage(image.AssetData, out imgData);
                            byte[] tgaFile = imgData.ExportTGA();
                            File.WriteAllBytes(image.ID.ToString() + ".tga", tgaFile);
                            Console.WriteLine("Wrote TGA image " + image.ID.ToString() + ".tga");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                        }
                    }
                    else
                    {
                        Console.WriteLine("Failed to download image " + image.ID.ToString());
                    }

                    OutfitAssets.Remove(image.ID);

                    if (OutfitAssets.Count == 0)
                        Client.Assets.OnImageReceived -= ImageReceivedHandler;
                }
            }
        }
    }
}
