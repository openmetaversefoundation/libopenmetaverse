using System;
using System.Collections.Generic;
using System.Threading;

using libsecondlife;
using libsecondlife.AssetSystem;
using libsecondlife.InventorySystem;
using libsecondlife.Packets;

using System.Text;


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
        protected Mutex WearableCacheQueueMutex = new Mutex();

        // This data defines all appearance info for an avatar
        public AgentWearablesUpdatePacket.WearableDataBlock[] AgentWearablesData;
        public SerializableDictionary<int, float> AgentAppearanceParams = new SerializableDictionary<int, float>();
        public LLObject.TextureEntry AgentTextureEntry = new LLObject.TextureEntry("C228D1CF4B5D4BA884F4899A0796AA97"); // if this isn't valid, blame JH ;-)

        public bool LogWearableAssetQueue = false;

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
            if (AgentWearablesSignal.WaitOne(1000, false) == false)
            {
                Client.Log("You must have set appearance at least once, before calling Wear().  AgentWearablesSignal not set.", Helpers.LogLevel.Error);
                return;
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
            WearOutfit(outfitFolder, 10000, true);
        }


        /// <summary>
        /// Equivalent to the SL "Replace Outfit" command.  All clothing is removed, and replaced with wearables in given folder.  Body wearables will be replaced if provided.
        /// </summary>
        /// <param name="outfitFolder">Contains the wearable items to put on.</param>
        /// <param name="TimeOut">How long to wait for outfit directory information to download</param>
        public void WearOutfit(InventoryFolder outfitFolder, int TimeOut, bool removeExistingAttachments)
        {
            // Refresh download of outfit folder
            if (!outfitFolder.RequestDownloadContents(false, false, true).RequestComplete.WaitOne(TimeOut, false))
            {
                Client.Log("Outfit not changed. An error occured while downloads the folder contents of : " + outfitFolder.Name, Helpers.LogLevel.Error);
                return;
            }

            // Make sure we have some Wearable Data to start with.
            if (AgentWearablesSignal.WaitOne(1000, false) == false)
            {
                Client.Log("You must have set appearance at least once, before calling WearOutfit().  AgentWearablesSignal not set.", Helpers.LogLevel.Error);
                return;
            }

            // Flush the cached clothing wearables so we can redefine them
            for (byte i = 4; i <= 12; i++)
            {
                AgentWearablesData[i].ItemID  = LLUUID.Zero;
                AgentWearablesData[i].AssetID = LLUUID.Zero;
            }

            List<InventoryItem> attachments = new List<InventoryItem>();

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
                else if (ib is InventoryItem)
                {
                    InventoryItem ii = (InventoryItem)ib;
                    attachments.Add(ii);
                }
            }

            // Change attachments
            AddAttachments(attachments, removeExistingAttachments);

            // Create AgentIsNowWearing Packet, and send it
            SendAgentIsNowWearing();

            // Send updated AgentSetAppearance to the grid
            SendAgentSetAppearance();
        }

        public void AddAttachments(List<InventoryItem> attachments, bool removeExistingFirst)
        {
            // Use RezMultipleAttachmentsFromInv  to clear out current attachments, and attach new ones
            RezMultipleAttachmentsFromInvPacket attachmentsPacket = new RezMultipleAttachmentsFromInvPacket();
            attachmentsPacket.AgentData.AgentID = Client.Network.AgentID;
            attachmentsPacket.AgentData.SessionID = Client.Network.SessionID;

            attachmentsPacket.HeaderData.CompoundMsgID = LLUUID.Random();
            attachmentsPacket.HeaderData.FirstDetachAll = true;
            attachmentsPacket.HeaderData.TotalObjects = (byte)attachments.Count;

            attachmentsPacket.ObjectData = new RezMultipleAttachmentsFromInvPacket.ObjectDataBlock[attachments.Count];
            for (int i = 0; i < attachments.Count; i++)
            {
                attachmentsPacket.ObjectData[i] = new RezMultipleAttachmentsFromInvPacket.ObjectDataBlock();
                attachmentsPacket.ObjectData[i].AttachmentPt = 0;
                attachmentsPacket.ObjectData[i].EveryoneMask = attachments[i].EveryoneMask;
                attachmentsPacket.ObjectData[i].GroupMask = attachments[i].GroupMask;
                attachmentsPacket.ObjectData[i].ItemFlags = attachments[i].Flags;
                attachmentsPacket.ObjectData[i].ItemID = attachments[i].ItemID;
                attachmentsPacket.ObjectData[i].Name = Helpers.StringToField(attachments[i].Name);
                attachmentsPacket.ObjectData[i].Description = Helpers.StringToField(attachments[i].Description);
                attachmentsPacket.ObjectData[i].NextOwnerMask = attachments[i].NextOwnerMask;
                attachmentsPacket.ObjectData[i].OwnerID = attachments[i].OwnerID;
            }

            Client.Network.SendPacket(attachmentsPacket);
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
        /// Update the local Avatar Appearance information based on the contents of the assets as defined in the cached wearable data info.
        /// </summary>
        protected void GetAvatarAppearanceInfoFromWearableAssets()
        {
            // Make sure we have some Wearable Data to start with.
            if (AgentWearablesSignal.WaitOne(1000, false) == false)
            {
                Client.Log("Cannot get Visual Param data from wearable assets.  AgentWearablesSignal not set.", Helpers.LogLevel.Error);
                return;
            }

            // Clear current look
            AgentTextureEntry = new LLObject.TextureEntry("C228D1CF4B5D4BA884F4899A0796AA97"); // if this isn't valid, blame JH ;-)
            AgentAppearanceParams = new SerializableDictionary<int, float>();


            // Build params and texture entries from wearable data
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

        /// <summary>
        /// TextureEntry must have it's face textures in a specific order for avatars.  
        /// Should be called at least once before sending an AgentSetAppearance packet.
        /// </summary>
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

        /// <summary>
        /// Updates the TextureEntry and Appearance Param structures with the data from an asset wearable.
        /// Called once for each weable asset.
        /// </summary>
        /// <param name="wearableAsset"></param>
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
            GetAvatarAppearanceInfoFromWearableAssets();

            AgentSetAppearancePacket p = new AgentSetAppearancePacket();
            p.AgentData.AgentID = Client.Network.AgentID;
            p.AgentData.SessionID = Client.Network.SessionID;
            p.AgentData.SerialNum = SerialNum++;

            // Add Texture Data
            p.ObjectData.TextureEntry = AgentTextureEntry.ToBytes();


            p.VisualParam = new AgentSetAppearancePacket.VisualParamBlock[218];

            string visualParamData = "";
            int vpIndex = 0;

            // Add Visual Params
            lock (AgentAppearanceParams)
            {
                foreach (KeyValuePair<int,VisualParam> kvp in VisualParams.Params)
                {
                    VisualParam param = kvp.Value;
                    p.VisualParam[vpIndex] = new AgentSetAppearancePacket.VisualParamBlock();

                    visualParamData += vpIndex + "," + param.ParamID + ",";

                    if (AgentAppearanceParams.ContainsKey(param.ParamID))
                    {
                        p.VisualParam[vpIndex].ParamValue = Helpers.FloatToByte(AgentAppearanceParams[param.ParamID],
                            param.MinValue, param.MaxValue);

                        visualParamData += AgentAppearanceParams[param.ParamID] + "," + p.VisualParam[vpIndex].ParamValue + Environment.NewLine;
                    }
                    else
                    {
                        // Use the default value for this parameter
                        p.VisualParam[vpIndex].ParamValue = Helpers.FloatToByte(param.DefaultValue, param.MinValue,
                            param.MaxValue);

                        visualParamData += "NA," + p.VisualParam[vpIndex].ParamValue + Environment.NewLine;

                    }

                    vpIndex++;
                }


            }

            // Add Size Data
            p.AgentData.Size = GetAgentSizeFromVisualParam(Helpers.ByteToFloat(p.VisualParam[33].ParamValue,
                VisualParams.Params[33].MinValue, VisualParams.Params[33].MaxValue));

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

            // Grab access mutex...
            WearableCacheQueueMutex.WaitOne();

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
                lock (WearableAssetQueue)
                {
                    if (WearableAssetQueue.Contains(wdb.AssetID))
                    {
                        continue;
                    }
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

                        LogWearableAssetQueueActivity("Added wearable asset to download queue: " + wearableAsset.GetType().Name + " : " + wdb.AssetID);
                    }
                }
            }

            RequestNextQueuedWearableAsset();

            WearableCacheQueueMutex.ReleaseMutex();
        }

        /// <summary>
        /// Sends a request for the next wearable asset.
        /// </summary>
        protected void RequestNextQueuedWearableAsset()
        {
            lock (WearableAssetQueue)
            {
                if (WearableAssetQueue.Count > 0)
                {
                    AssetWearable wearableAsset = WearableCache[WearableAssetQueue[0]];
                    /*AssetRequestDownload request =*/Client.Assets.RequestInventoryAsset(wearableAsset.AssetID, wearableAsset.Type);
                    LogWearableAssetQueueActivity("Requesting: " + wearableAsset.AssetID);
                }
                else
                {
                    if (AgentWearablesSignal.WaitOne(0, false) == true)
                    {
                        // Send updated AgentSetAppearance
                        SendAgentSetAppearance();
                    }
                }
            }            
        }

        /// <summary>
        /// use to debug wearable asset queue activity
        /// </summary>
        /// <param name="msg"></param>
        protected void LogWearableAssetQueueActivity(string msg)
        {
            if (LogWearableAssetQueue)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("==================");
                sb.AppendLine(msg);
                sb.AppendLine("Current Queue:");
                foreach (LLUUID uuid in WearableAssetQueue)
                {
                    sb.AppendLine(" ** " + uuid.ToStringHyphenated());
                }
                Client.Log(sb.ToString(), Helpers.LogLevel.Info);
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

            WearableCacheQueueMutex.WaitOne();

            // Remove from the download queue
            lock (WearableAssetQueue)
            {
                if (!WearableAssetQueue.Contains(dlrequest.AssetID))
                {
                    // Looks like we got an asset for something other then what we're waiting for, ignore it
                    WearableCacheQueueMutex.ReleaseMutex();

                    return;
                }
            }

            // Since we got a response for this asset, remove it from the queue
            WearableAssetQueue.Remove(dlrequest.AssetID);
            LogWearableAssetQueueActivity("Received queued asset, and removed: " + dlrequest.AssetID);

            // If the request wasn't successful, then don't try to process it.
            if (request.Status != AssetRequest.RequestStatus.Success)
            {
                Client.Log("Error downloading wearable asset: " + dlrequest.AssetID, Helpers.LogLevel.Error);
                WearableCacheQueueMutex.ReleaseMutex();

                return;
            }


            AssetWearable wearableAsset = WearableCache[dlrequest.AssetID];
            wearableAsset.SetAssetData(dlrequest.GetAssetData());

            if ((wearableAsset.AssetData == null) || (wearableAsset.AssetData.Length == 0))
            {
                Client.Log("Asset retrieval failed for AssetID: " + wearableAsset.AssetID, Helpers.LogLevel.Error);
                WearableCacheQueueMutex.ReleaseMutex();
                return;
            }
            else
            {
                UpdateAgentTextureEntryAndAppearanceParams(wearableAsset);

                UpdateAgentTextureEntryOrder();

                lock(WearableAssetQueue)
                {
                    if (WearableAssetQueue.Count > 0)
                    {
                        RequestNextQueuedWearableAsset();
                        WearableCacheQueueMutex.ReleaseMutex();
                        return;

                    }
                }

                // Now that all the wearable assets are done downloading,
                // send an appearance packet
                SendAgentSetAppearance();

                WearableCacheQueueMutex.ReleaseMutex();
                return;
            }
        }

        #endregion
    }
}
