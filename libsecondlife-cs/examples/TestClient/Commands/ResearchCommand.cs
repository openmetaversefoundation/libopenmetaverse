using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.IO;
using libsecondlife;
using libsecondlife.Utilities.Assets;
using libsecondlife.Utilities.Appearance;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class ResearchCommand : Command
    {
        private ManualResetEvent AssetReceived = new ManualResetEvent(false);
        AssetManager manager;
        //AppearanceManager appearance;
        List<LLUUID> Downloaded = new List<LLUUID>();

        public ResearchCommand(TestClient testClient)
        {
            Name = "research";
            Description = "Does important research for the betterment of mankind";

            testClient.Objects.OnNewAvatar += new ObjectManager.NewAvatarCallback(Objects_OnNewAvatar);

            manager = new AssetManager(testClient);
            manager.OnImageReceived += new AssetManager.ImageReceivedCallback(manager_OnImageReceived);
            //manager.OnAssetReceived += new AssetManager.AssetReceivedCallback(manager_OnAssetReceived);
            //appearance = new AppearanceManager(testClient, manager);
            //appearance.OnAgentWearables += new AppearanceManager.AgentWearablesCallback(appearance_OnAgentWearables);
        }

        void manager_OnImageReceived(ImageDownload image)
        {
            if (image.Success)
            {
                File.WriteAllBytes(image.ID.ToStringHyphenated() + ".j2c", image.AssetData);
                Console.WriteLine("Downloaded " + image.ID.ToStringHyphenated());
            }
            else
            {
                Console.WriteLine("Failed to download " + image.ID.ToStringHyphenated() + ", NotFound=" + image.NotFound);
            }
        }

        void Objects_OnNewAvatar(Simulator simulator, Avatar avatar, ulong regionHandle, ushort timeDilation)
        {
            StringBuilder output = new StringBuilder("Avatar ");
            output.Append(avatar.ID.ToStringHyphenated());
            output.Append(" is wearing: ");

            foreach (KeyValuePair<uint, LLObject.TextureEntryFace> texture in avatar.Textures.FaceTextures)
            {
                AppearanceManager.TextureIndex textureName = (AppearanceManager.TextureIndex)texture.Key;

                switch (textureName)
                {
                    case AppearanceManager.TextureIndex.HeadBaked:
                    case AppearanceManager.TextureIndex.LowerBaked:
                    case AppearanceManager.TextureIndex.UpperBaked:
                    case AppearanceManager.TextureIndex.SkirtBaked:
                        if (!Downloaded.Contains(texture.Value.TextureID) &&
                            texture.Value.TextureID != AppearanceManager.DEFAULT_AVATAR_TEXTURE)
                        {
                            Downloaded.Add(texture.Value.TextureID);
                            //manager.RequestImage(texture.Value.TextureID, ImageType.Baked, 120000.0f, 0);
                        }
                        break;
                    default:
                        break;
                }

                output.Append(textureName.ToString());
                output.Append(" ");
            }

            //Console.WriteLine(output.ToString());
        }

        public override string Execute(string[] args, LLUUID fromAgentID)
        {
            //Dictionary<WearableType, LLUUID> wearables = new Dictionary<WearableType, LLUUID>();
            //for (int i = 0; i < AppearanceManager.WEARABLE_COUNT; i++)
            //{
            //    wearables[(WearableType)i] = LLUUID.Zero;
            //}
            //wearables[WearableType.Shirt] = new LLUUID("4fcf0807-b173-5b4e-90fe-d83d84986b59");
            //appearance.SendAgentWearables(wearables);

            //return "Wearing the payload";

            //libsecondlife.AssetSystem.AssetWearable asset = new libsecondlife.AssetSystem.AssetWearable(
            //    LLUUID.Zero, (sbyte)AssetType.Bodypart, new byte[0]);
            //string exploit = String.Empty;
            //for (int i = 0; i < 3000; i++)
            //    exploit += "A";
            //asset.Name = exploit;
            //asset.Type = (sbyte)WearableType.Shirt;
            //LLUUID newasset = Client.Assets.UploadAsset(asset);

            //return "Created Bodypart " + newasset.ToStringHyphenated();
            

            //if (args.Length != 1)
            //{
            //    appearance.RequestAgentWearables();

            //    return "Appearance fetch complete";
            //}

            //LLUUID id;
            //try
            //{
            //    id = new LLUUID(args[0]);
            //}
            //catch (Exception)
            //{
            //    return "Need a proper UUID";
            //}

            //manager.RequestAsset(id, AssetType.Bodypart, ChannelType.Asset, SourceType.Asset, 125000.0f);

            //Random random = new Random();
            //byte r, g, b;

            //lock (Client.AvatarList)
            //{
            //    foreach (Avatar avatar in Client.AvatarList.Values)
            //    {
            //        //Client.Self.PointAtEffect(avatar.ID, Client.Network.AgentID, LLVector3d.Zero, 
            //        //    MainAvatar.PointAtType.Grab);

            //        r = (byte)random.Next(byte.MaxValue);
            //        g = (byte)random.Next(byte.MaxValue);
            //        b = (byte)random.Next(byte.MaxValue);
            //        Client.Self.BeamEffect(avatar.ID, Client.Network.AgentID, LLVector3d.Zero, 
            //            new LLColor(r, g, b, 255), 300.0f);
            //    }
            //}

            //FetchInventoryPacket fetch = new FetchInventoryPacket();
            //fetch.AgentData.AgentID = Client.Network.AgentID;
            //fetch.AgentData.SessionID = Client.Network.SessionID;
            //fetch.InventoryData = new FetchInventoryPacket.InventoryDataBlock[1];
            //fetch.InventoryData[0] = new FetchInventoryPacket.InventoryDataBlock();
            //fetch.InventoryData[0].ItemID = id;
            //fetch.InventoryData[0].OwnerID = Client.Network.AgentID;

            //Client.Network.SendPacket(fetch);

            return "Research complete";
        }

        //void appearance_OnAgentWearables(Dictionary<WearableType, KeyValuePair<LLUUID, LLUUID>> wearables)
        //{
        //    foreach (KeyValuePair<WearableType, KeyValuePair<LLUUID, LLUUID>> wearable in wearables)
        //    {
        //        Console.WriteLine("Received a WearableType " + wearable.Key.ToString() + ", AssetID: " +
        //            wearable.Value.Key.ToStringHyphenated());
        //    }
        //}

        //void manager_OnAssetReceived(AssetTransfer asset)
        //{
        //    if (asset.Success)
        //    {
        //        Console.WriteLine("Downloaded " + asset.ID.ToStringHyphenated() + Environment.NewLine + 
        //            Helpers.FieldToUTF8String(asset.AssetData));
        //    }
        //    else
        //    {
        //        Console.WriteLine("Failed to retrieve asset " + asset.ID.ToStringHyphenated(), asset.Transferred +
        //            " bytes were transferred");
        //    }
        //}

        //private void FetchInventoryReplyHandler(Packet packet, Simulator simulator)
        //{
        //    FetchInventoryReplyPacket reply = (FetchInventoryReplyPacket)packet;

        //    Console.WriteLine(reply.ToString());
        //}
    }
}
