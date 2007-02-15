using System;
using System.Collections.Generic;
using System.Threading;

using libsecondlife;
using libsecondlife.AssetSystem;
using libsecondlife.InventorySystem;
using libsecondlife.Packets;



namespace libsecondlife.AssetSystem
{
    public class AppearanceManager
    {
    	public enum	WearableType
		{
			Shape = 0,
			Skin = 1,
			Hair = 2,
			Eyes = 3,
			Shirt = 4,
			Pants = 5,
			Shoes = 6,
			Socks = 7,
			Jacket = 8,
			Gloves = 9,
			Undershirt = 10,
			Underpants = 11,
			Skirt = 12,
			Count = 13,
			Invalid = 255
		};
    
        protected SecondLife Client;
        protected AssetManager AManager;

        protected uint SerialNum = 1;

        protected ManualResetEvent AgentWearablesSignal = new ManualResetEvent(false);

        protected Dictionary<LLUUID, AssetWearable> WearableCache = new Dictionary<LLUUID, AssetWearable>();
        protected List<LLUUID> WearableAssetQueue = new List<LLUUID>();

        // This data defines all appearance info for an avatar
        public AgentWearablesUpdatePacket.WearableDataBlock[] AgentWearablesData;
        public SerializableDictionary<int, float> AgentAppearanceParams = new SerializableDictionary<int, float>();
        public LLObject.TextureEntry AgentTextureEntry = new LLObject.TextureEntry("C228D1CF4B5D4BA884F4899A0796AA97"); // if this isn't valid, blame JH ;-)


        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        public AppearanceManager(SecondLife client)
        {
            Client = client;
            Client.Network.RegisterCallback(libsecondlife.Packets.PacketType.AgentWearablesUpdate, new NetworkManager.PacketCallback(AgentWearablesUpdateCallbackHandler));

            AManager = client.Assets;
            AManager.TransferRequestCompletedEvent += new AssetManager.On_TransferRequestCompleted(AManager_TransferRequestCompletedEvent);
        }

        #region Wear Stuff

        /// <summary>
        /// Add a single wearable to your outfit, replacing if nessesary.
        /// </summary>
        /// <param name="wearable"></param>
        public void Wear(InventoryWearable wearable)
        {
            List<InventoryWearable> x = new List<InventoryWearable>();
            x.Add(wearable);
            Wear(x);
        }

        /// <summary>
        /// Add the specified wearables to your outfit, replace existing ones if nessesary.
        /// </summary>
        /// <param name="wearables"></param>
        public void Wear(List<InventoryWearable> wearables)
        {
            // Make sure we have some Wearable Data to start with.
            if ((AgentWearablesData == null) || (AgentWearablesData.Length == 0))
            {
                GetWearables();
            }

            // Update with specified wearables
            foreach (InventoryWearable iw in wearables)
            {
                byte type = (byte)((AssetWearable)iw.Asset).AppearanceLayer;
                AgentWearablesData[type].ItemID = iw.ItemID;
                AgentWearablesData[type].AssetID = iw.AssetID;
            }

            // Create AgentIsNowWearing Packet, and send it
            SendAgentIsNowWearing();

            // Update local Appearance Info
            GetAvatarAppearanceInfoFromWearableAssets();

            // Send updated AgentSetAppearance to the grid
            BeginAgentSendAppearance();
        }

        /// <summary>
        /// Equivalent to the SL "Replace Outfit" command.  All clothing is removed, and replaced with wearables in given folder.  Body wearables will be replaced if provided.
        /// </summary>
        /// <param name="outfitFolder">Contains the wearable items to put on.</param>
        public void WearOutfit(InventoryFolder outfitFolder)
        {
            WearOutfit(outfitFolder, 10000);
        }


        /// <summary>
        /// Equivalent to the SL "Replace Outfit" command.  All clothing is removed, and replaced with wearables in given folder.  Body wearables will be replaced if provided.
        /// </summary>
        /// <param name="outfitFolder">Contains the wearable items to put on.</param>
        /// <param name="TimeOut">How long to wait for outfit directory information to download</param>
        public void WearOutfit(InventoryFolder outfitFolder, int TimeOut)
        {
            // Refresh download of outfit folder
            if (!outfitFolder.RequestDownloadContents(false, false, true, true).RequestComplete.WaitOne(TimeOut, false))
            {
                Console.WriteLine("An error occured while downloads the folder contents of : " + outfitFolder.Name);
            }

            // Make sure we have some Wearable Data to start with.
            if ((AgentWearablesData == null) || (AgentWearablesData.Length == 0))
            {
                GetWearables();
            }

            // Flush the cached clothing wearables so we can redefine them
            for (byte i = 4; i <= 12; i++)
            {
                AgentWearablesData[i].ItemID  = LLUUID.Zero;
                AgentWearablesData[i].AssetID = LLUUID.Zero;
            }

            // Replace with wearables from Outfit folder
            foreach (InventoryBase ib in outfitFolder.GetContents())
            {
                if (ib is InventoryWearable)
                {
                    try
                    {
                        InventoryWearable iw = (InventoryWearable)ib;
                        Client.Log("Retrieving asset for " + iw.Name + "("+iw.AssetID+")", Helpers.LogLevel.Info);
                        AssetWearable.AppearanceLayerType AppearanceLayer = ((AssetWearable)iw.Asset).AppearanceLayer;

                        Client.Log("Adding skin/clothing layer for " + AppearanceLayer, Helpers.LogLevel.Info);
                        AgentWearablesData[(byte)AppearanceLayer].ItemID = iw.ItemID;
                        AgentWearablesData[(byte)AppearanceLayer].AssetID = iw.AssetID;

                    }
                    catch (Exception e)
                    {
                        Client.Log("Asset for " + ib._Name + " unavailable: " + e.Message, Helpers.LogLevel.Error);
                    }
                }
            }

            // Create AgentIsNowWearing Packet, and send it
            SendAgentIsNowWearing();

            // Update local Appearance Info
            GetAvatarAppearanceInfoFromWearableAssets();

            // Send updated AgentSetAppearance to the grid
            BeginAgentSendAppearance();
        }

        #endregion


        /// <summary>
        /// Creates and sends an AgentIsNowWearing packet based on the local cached AgentWearablesData array.
        /// </summary>
        protected void SendAgentIsNowWearing()
        {
            AgentIsNowWearingPacket nowWearing = new AgentIsNowWearingPacket();
            nowWearing.AgentData.AgentID = Client.Network.AgentID;
            nowWearing.AgentData.SessionID = Client.Network.SessionID;
            nowWearing.WearableData = new AgentIsNowWearingPacket.WearableDataBlock[13];
            for (byte i = 0; i <= 12; i++)
            {
                nowWearing.WearableData[i] = new AgentIsNowWearingPacket.WearableDataBlock();
                nowWearing.WearableData[i].WearableType = i;
                nowWearing.WearableData[i].ItemID = AgentWearablesData[i].ItemID;
            }

            Client.Network.SendPacket(nowWearing);
        }

        /// <summary>
        /// Request from the server what wearables we're currently wearing.  Update cached info.
        /// </summary>
        /// <returns>The wearable info for what we're currently wearing</returns>
        protected AgentWearablesUpdatePacket.WearableDataBlock[] GetWearables()
        {
            AgentWearablesSignal.Reset();

            AgentWearablesRequestPacket p = new AgentWearablesRequestPacket();
            p.AgentData.AgentID = Client.Network.AgentID;
            p.AgentData.SessionID = Client.Network.SessionID;
            Client.Network.SendPacket(p);

            AgentWearablesSignal.WaitOne();

            return AgentWearablesData;
        }

        /// <summary>
        /// Update the local Avatar Appearance information based on the contents of the assets as defined in the cached wearable data info.
        /// </summary>
        protected void GetAvatarAppearanceInfoFromWearableAssets()
        {
            // Only request wearable data, if we have to.
            if ((AgentWearablesData == null) || (AgentWearablesData.Length == 0))
            {
                GetWearables();
            }

            foreach (AgentWearablesUpdatePacket.WearableDataBlock wdb in AgentWearablesData)
            {
                if (wdb.ItemID == LLUUID.Zero)
                {
                    continue;
                }


                AssetWearable wearableAsset;

                switch (wdb.WearableType)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                        wearableAsset = new AssetWearable_Body(wdb.AssetID, null);
                        break;
                    default:
                        wearableAsset = new AssetWearable_Clothing(wdb.AssetID, null);
                        break;
                }

                AssetRequestDownload request = Client.Assets.RequestInventoryAsset(wearableAsset.AssetID, wearableAsset.Type);
                if (request.Wait(AssetManager.DefaultTimeout) != AssetRequestDownload.RequestStatus.Success)
                {
                    Client.Log("Asset (" + wearableAsset.AssetID.ToStringHyphenated() + ") unavailable (" + request.StatusMsg + ")", Helpers.LogLevel.Error);
                }
                else
                {
                    wearableAsset.SetAssetData(request.GetAssetData());
                }

                if ((wearableAsset.AssetData == null) || (wearableAsset.AssetData.Length == 0))
                {
                    Client.Log("Asset retrieval failed for AssetID: " + wearableAsset.AssetID, Helpers.LogLevel.Warning);
                }

                UpdateAgentTextureEntryAndAppearanceParams(wearableAsset);
            }


            UpdateAgentTextureEntryOrder();
        }

        protected void UpdateAgentTextureEntryOrder()
        {
            // Correct the order of the textures
            foreach (uint faceid in AgentTextureEntry.FaceTextures.Keys)
            {
                if (faceid > 18)
                {
                    Client.Log("Unknown order for FaceID: " + faceid + Environment.NewLine +
                        "Your wearables define a face that we don't know the order of.  Please " +
                        "capture a AgentSetAppearance packet for your current outfit and submit to " +
                        "static.sprocket@gmail.com, thanks!", Helpers.LogLevel.Info);
                    break;
                }
            }

            //Re-order texture faces to match Linden Labs internal data structure.
            LLObject.TextureEntry te2 = new LLObject.TextureEntry(AgentTextureEntry.DefaultTexture.TextureID);
            te2.CreateFace(18).TextureID = AgentTextureEntry.GetFace(18).TextureID;
            te2.CreateFace(17).TextureID = AgentTextureEntry.GetFace(17).TextureID;
            te2.CreateFace(16).TextureID = AgentTextureEntry.GetFace(16).TextureID;
            te2.CreateFace(15).TextureID = AgentTextureEntry.GetFace(15).TextureID;
            te2.CreateFace(14).TextureID = AgentTextureEntry.GetFace(14).TextureID;
            te2.CreateFace(13).TextureID = AgentTextureEntry.GetFace(13).TextureID;
            te2.CreateFace(12).TextureID = AgentTextureEntry.GetFace(12).TextureID;
            // I wonder if shoes are somewhere in here?
            te2.CreateFace(7).TextureID = AgentTextureEntry.GetFace(7).TextureID;
            te2.CreateFace(6).TextureID = AgentTextureEntry.GetFace(6).TextureID;
            te2.CreateFace(5).TextureID = AgentTextureEntry.GetFace(5).TextureID;
            te2.CreateFace(4).TextureID = AgentTextureEntry.GetFace(4).TextureID;
            te2.CreateFace(3).TextureID = AgentTextureEntry.GetFace(3).TextureID;
            te2.CreateFace(2).TextureID = AgentTextureEntry.GetFace(2).TextureID;
            te2.CreateFace(1).TextureID = AgentTextureEntry.GetFace(1).TextureID;
            te2.CreateFace(0).TextureID = AgentTextureEntry.GetFace(0).TextureID;

            AgentTextureEntry = te2;
        }

        protected void UpdateAgentTextureEntryAndAppearanceParams(AssetWearable wearableAsset)
        {
            try
            {
                foreach (KeyValuePair<uint, LLUUID> texture in wearableAsset.Textures)
                {
                    AgentTextureEntry.CreateFace(texture.Key).TextureID = texture.Value;
                }

                lock (AgentAppearanceParams)
                {
                    foreach (KeyValuePair<int, float> kvp in wearableAsset.Parameters)
                    {
                        AgentAppearanceParams[kvp.Key] = kvp.Value;
                    }
                }
            }
            catch (Exception e)
            {
                Client.Log(e.ToString() + Environment.NewLine + wearableAsset.AssetDataToString(), Helpers.LogLevel.Error);
            }
        }

        /// <summary>
        /// Non-blocking async request of wearables, construction and sending of AgentSetAppearance
        /// </summary>
        public void BeginAgentSendAppearance()
        {
            AgentWearablesSignal.Reset();

            AgentWearablesRequestPacket p = new AgentWearablesRequestPacket();
            p.AgentData.AgentID = Client.Network.AgentID;
            p.AgentData.SessionID = Client.Network.SessionID;
            Client.Network.SendPacket(p);
        }

        /// <summary>
        /// Send an AgentSetAppearance packet to the server to update your appearance.
        /// </summary>
        
        protected void SendAgentSetAppearance()
        {
            // Get latest appearance info
            if (AgentAppearanceParams.Count == 0)
            {
                GetAvatarAppearanceInfoFromWearableAssets();
            }

            AgentSetAppearancePacket p = new AgentSetAppearancePacket();
            p.AgentData.AgentID = Client.Network.AgentID;
            p.AgentData.SessionID = Client.Network.SessionID;
            p.AgentData.SerialNum = ++SerialNum;

            // Add Texture Data
            p.ObjectData.TextureEntry = AgentTextureEntry.ToBytes();


            p.VisualParam = new AgentSetAppearancePacket.VisualParamBlock[218];

            // Add Visual Params
            lock (AgentAppearanceParams)
            {
                for (int i = 0; i < 218; i++)
                {
                    VisualParam param = VisualParams.Params[i];
                    p.VisualParam[i] = new AgentSetAppearancePacket.VisualParamBlock();

                    if (AgentAppearanceParams.ContainsKey(param.ParamID))
                    {
                        p.VisualParam[i].ParamValue = Helpers.FloatToByte(AgentAppearanceParams[param.ParamID],
                            param.MinValue, param.MaxValue);
                    }
                    else
                    {
                        // Use the default value for this parameter
                        p.VisualParam[i].ParamValue = Helpers.FloatToByte(param.DefaultValue, param.MinValue,
                            param.MaxValue);
                    }
                }
            }

            // Add Size Data
            p.AgentData.Size = GetAgentSizeFromVisualParam(Helpers.ByteToFloat(p.VisualParam[25].ParamValue,
                VisualParams.Params[25].MinValue, VisualParams.Params[25].MaxValue));

            Client.Network.SendPacket(p);
        }

        /// <summary>
        /// Determine agent size for AgentSetAppearance based on Visual Param data.
        /// </summary>
        /// <param name="heightParam"></param>
        /// <returns></returns>
        protected LLVector3 GetAgentSizeFromVisualParam(float heightParam)
        {
            float AV_Height_Range = 2.025506f - 1.50856f;
            float AV_Height = 1.50856f + ((heightParam / 255.0f) * AV_Height_Range);
            return new LLVector3(0.45f, 0.6f, AV_Height);
            //return new LLVector3(0.45f, 0.6f, 1.0f);
        }

        #region Callback Handlers

        private void AgentWearablesUpdateCallbackHandler(Packet packet, Simulator simulator)
        {
            AgentWearablesUpdatePacket wearablesPacket = (AgentWearablesUpdatePacket)packet;

            AgentWearablesData = wearablesPacket.WearableData;
            AgentWearablesSignal.Set();


            // Queue download of wearables
            foreach (AgentWearablesUpdatePacket.WearableDataBlock wdb in AgentWearablesData)
            {

                // Don't try to download if AssetID is zero
                if (wdb.AssetID == LLUUID.Zero)
                {
                    continue;
                }

                // Don't try to download, if it's already cached.
                if (WearableCache.ContainsKey(wdb.AssetID))
                {
                    AssetWearable aw = WearableCache[wdb.AssetID];
                    if (aw._AssetData != null)
                    {
                        continue;
                    }
                }

                // Don't try to download, if it's already in the download queue
                if (WearableAssetQueue.Contains(wdb.AssetID))
                {
                    continue;
                }

                AssetWearable wearableAsset;

                switch (wdb.WearableType)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                        wearableAsset = new AssetWearable_Body(wdb.AssetID, null);
                        break;
                    default:
                        wearableAsset = new AssetWearable_Clothing(wdb.AssetID, null);
                        break;
                }

                WearableCache[wdb.AssetID] = wearableAsset;

                lock (WearableAssetQueue)
                {
                    if (!WearableAssetQueue.Contains(wdb.AssetID))
                    {
                        WearableAssetQueue.Add(wdb.AssetID);
                    }
                }

                AssetRequestDownload request = Client.Assets.RequestInventoryAsset(wearableAsset.AssetID, wearableAsset.Type);

            }

            
        }

        /// <summary>
        /// Called each time a wearable asset is done downloading
        /// </summary>
        /// <param name="request"></param>
        void AManager_TransferRequestCompletedEvent(AssetRequest request)
        {
            if( !(request is AssetRequestDownload) )
            {
                return;
            }

            AssetRequestDownload dlrequest = (AssetRequestDownload)request;

            if (dlrequest.AssetID == null)
            {
                Client.Log("AssetID is null in AssetRequestDownload: " + dlrequest.StatusMsg, Helpers.LogLevel.Error);
            }

            lock( WearableAssetQueue )
            {
                // Remove from the download queue
                if (!WearableAssetQueue.Contains(dlrequest.AssetID))
                {
                    return;
                }

                WearableAssetQueue.Remove(dlrequest.AssetID);
            }

            // If the request wasn't successful, then don't try to process it.
            if (request.Status != AssetRequest.RequestStatus.Success)
            {
                return;
            }


            AssetWearable wearableAsset = WearableCache[dlrequest.AssetID];
            wearableAsset.SetAssetData(dlrequest.GetAssetData());

            if ((wearableAsset.AssetData == null) || (wearableAsset.AssetData.Length == 0))
            {
                Client.Log("Asset retrieval failed for AssetID: " + wearableAsset.AssetID, Helpers.LogLevel.Warning);
            }
            else
            {
                UpdateAgentTextureEntryAndAppearanceParams(wearableAsset);

                UpdateAgentTextureEntryOrder();

                if (WearableAssetQueue.Count == 0)
                {

                    // Now that all the wearable assets are done downloading
                    // , we can send an appearance packet
                    SendAgentSetAppearance();
                }
            }
        }

        #endregion
    }
}
