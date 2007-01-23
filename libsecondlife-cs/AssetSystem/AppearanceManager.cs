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

        protected ManualResetEvent AgentWearablesSignal = null;

        protected Dictionary<LLUUID, AssetWearable> WearableCache = new Dictionary<LLUUID, AssetWearable>();
        protected List<LLUUID> WearableAssetQueue = new List<LLUUID>();

        // This data defines all appearance info for an avatar
        public AgentWearablesUpdatePacket.WearableDataBlock[] AgentWearablesData;
        public SerializableDictionary<int, float> AgentAppearanceParams = new SerializableDictionary<int, float>();
        public TextureEntry AgentTextureEntry = new TextureEntry("C228D1CF4B5D4BA884F4899A0796AA97"); // if this isn't valid, blame JH ;-)


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
                byte type = ((AssetWearable)iw.Asset).TypeFromAsset;
                AgentWearablesData[type].ItemID = iw.ItemID;
                AgentWearablesData[type].AssetID = iw.AssetID;
            }

            // Create AgentIsNowWearing Packet, and send it
            SendAgentIsNowWearing();

            // Update local Appearance Info
            GetAvatarAppearanceInfoFromWearableAssets();

            // Send updated AgentSetAppearance to the grid
            SendAgentSetAppearance();
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
                    InventoryWearable iw = (InventoryWearable)ib;
                    byte type = ((AssetWearable)iw.Asset).TypeFromAsset;
                    AgentWearablesData[type].ItemID  = iw.ItemID;
                    AgentWearablesData[type].AssetID = iw.AssetID;
                }
            }

            // Create AgentIsNowWearing Packet, and send it
            SendAgentIsNowWearing();

            // Update local Appearance Info
            GetAvatarAppearanceInfoFromWearableAssets();

            // Send updated AgentSetAppearance to the grid
            SendAgentSetAppearance();
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
            AgentWearablesSignal = new ManualResetEvent(false);

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
                    throw new Exception("Asset (" + wearableAsset.AssetID.ToStringHyphenated() + ") unavailable (" + request.StatusMsg + ")");
                }
                wearableAsset.SetAssetData(request.GetAssetData());

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
            TextureEntry te2 = new TextureEntry(AgentTextureEntry.DefaultTexture.TextureID);
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

                foreach (KeyValuePair<int, float> kvp in wearableAsset.Parameters)
                {
                    AgentAppearanceParams[kvp.Key] = kvp.Value;
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
            AgentWearablesSignal = new ManualResetEvent(false);

            AgentWearablesRequestPacket p = new AgentWearablesRequestPacket();
            p.AgentData.AgentID = Client.Network.AgentID;
            p.AgentData.SessionID = Client.Network.SessionID;
            Client.Network.SendPacket(p);
        }

        /// <summary>
        /// Send an AgentSetAppearance packet to the server to update your appearance.
        /// </summary>
        public void SendAgentSetAppearance()
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

            // Add Visual Params
            Dictionary<int, byte> VisualParams = GetAssetParamsAsVisualParams();
            p.VisualParam = new AgentSetAppearancePacket.VisualParamBlock[218];
            for (int i = 0; i < 218; i++)
            {
                p.VisualParam[i] = new AgentSetAppearancePacket.VisualParamBlock();

                if (VisualParams.ContainsKey(i))
                {
                    p.VisualParam[i].ParamValue = VisualParams[i];
                }
                else
                {
                    int paramid = GetParamID(i + 1);

                    if (!libsecondlife.VisualParams.Params.ContainsKey(paramid))
                    {
                        Client.Log("Unknown VisualParam ID encountered :: " + paramid, Helpers.LogLevel.Debug);
                    }

                    VisualParam vp = libsecondlife.VisualParams.Params[paramid];
                    p.VisualParam[i].ParamValue = Helpers.FloatToByte(vp.DefaultValue, vp.MinValue, vp.MaxValue);
                }
            }

            // Add Size Data
            p.AgentData.Size = GetAgentSizeFromVisualParams(VisualParams);

            Client.Network.SendPacket(p);
        }


        /// <summary>
        /// Convert the morph params as they are stored in assets, to the byte values needed for
        /// AgentSetAppearance packet
        /// </summary>
        /// <returns>Visual Param information for AgentSetAppearance packets</returns>
        protected Dictionary<int, byte> GetAssetParamsAsVisualParams()
        {
            Dictionary<int, byte> VisualParams = new Dictionary<int, byte>();

            int packetIdx = 0;
            float percentage = 0;
            byte packetVal = 0;

            foreach (KeyValuePair<int, float> kvp in AgentAppearanceParams)
            {
                packetIdx = AppearanceManager.GetAgentSetAppearanceIndex(kvp.Key) - 1; //TODO/FIXME: this should be zero indexed, not 1 based.

                VisualParam vp = libsecondlife.VisualParams.Params[kvp.Key];
                VisualParams[packetIdx] = Helpers.FloatToByte(kvp.Value, vp.MinValue, vp.MaxValue);
            }

            return VisualParams;
        }

        /// <summary>
        /// Determine agent size for AgentSetAppearance based on Visual Param data.
        /// </summary>
        /// <param name="VisualParams"></param>
        /// <returns></returns>
        protected LLVector3 GetAgentSizeFromVisualParams(Dictionary<int, byte> VisualParams)
        {
            if (VisualParams.ContainsKey(25))
            {
                float AV_Height_Range = 2.025506f - 1.50856f;
                float AV_Height = 1.50856f + (((float)VisualParams[25] / 255.0f) * AV_Height_Range);
                return new LLVector3(0.45f, 0.6f, AV_Height);
            }
            else
            {
                return new LLVector3(0.45f, 0.6f, 1.0f);
            }
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
                WearableAssetQueue.Add(wdb.AssetID);

                AssetRequestDownload request = Client.Assets.RequestInventoryAsset(wearableAsset.AssetID, wearableAsset.Type);

            }

            
        }

        void AManager_TransferRequestCompletedEvent(AssetRequest request)
        {
            if( !(request is AssetRequestDownload) )
            {
                return;
            }

            AssetRequestDownload dlrequest = (AssetRequestDownload)request;

            // Remove from the download queue
            if (WearableAssetQueue.Contains(dlrequest.AssetID))
            {
                WearableAssetQueue.Remove(dlrequest.AssetID);

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
                        SendAgentSetAppearance();
                    }
                }
            }
        }


        #endregion

        #region Lookup Tables

        /// <summary>
        /// Convert a Visual Params index number from the ParamID provided in Assets and from avatar_lad.xml
        /// </summary>
        /// <param name="AssetParamID"></param>
        /// <returns></returns>
        protected static int GetAgentSetAppearanceIndex(int AssetParamID)
        {
            switch (AssetParamID)
            {
                case 1: return 1;
                case 2: return 2;
                case 4: return 3;
                case 5: return 4;
                case 6: return 5;
                case 7: return 6;
                case 8: return 7;
                case 10: return 8;
                case 11: return 9;
                case 12: return 10;
                case 13: return 11;
                case 14: return 12;
                case 15: return 13;
                case 16: return 14;
                case 17: return 15;
                case 18: return 16;
                case 19: return 17;
                case 20: return 18;
                case 21: return 19;
                case 22: return 20;
                case 23: return 21;
                case 24: return 22;
                case 25: return 23;
                case 27: return 24;
                case 31: return 25;
                case 33: return 26;
                case 34: return 27;
                case 35: return 28;
                case 36: return 29;
                case 37: return 30;
                case 38: return 31;
                case 80: return 32;
                case 93: return 33;
                case 98: return 34;
                case 99: return 35;
                case 105: return 36;
                case 108: return 37;
                case 110: return 38;
                case 111: return 39;
                case 112: return 40;
                case 113: return 41;
                case 114: return 42;
                case 115: return 43;
                case 116: return 44;
                case 117: return 45;
                case 119: return 46;
                case 130: return 47;
                case 131: return 48;
                case 132: return 49;
                case 133: return 50;
                case 134: return 51;
                case 135: return 52;
                case 136: return 53;
                case 137: return 54;
                case 140: return 55;
                case 141: return 56;
                case 142: return 57;
                case 143: return 58;
                case 150: return 59;
                case 155: return 60;
                case 157: return 61;
                case 162: return 62;
                case 163: return 63;
                case 165: return 64;
                case 166: return 65;
                case 167: return 66;
                case 168: return 67;
                case 169: return 68;
                case 177: return 69;
                case 181: return 70;
                case 182: return 71;
                case 183: return 72;
                case 184: return 73;
                case 185: return 74;
                case 192: return 75;
                case 193: return 76;
                case 196: return 77;
                case 198: return 78;
                case 503: return 79;
                case 505: return 80;
                case 506: return 81;
                case 507: return 82;
                case 508: return 83;
                case 513: return 84;
                case 514: return 85;
                case 515: return 86;
                case 517: return 87;
                case 518: return 88;
                case 603: return 89;
                case 604: return 90;
                case 605: return 91;
                case 606: return 92;
                case 607: return 93;
                case 608: return 94;
                case 609: return 95;
                case 616: return 96;
                case 617: return 97;
                case 619: return 98;
                case 624: return 99;
                case 625: return 100;
                case 629: return 101;
                case 637: return 102;
                case 638: return 103;
                case 646: return 104;
                case 647: return 105;
                case 649: return 106;
                case 650: return 107;
                case 652: return 108;
                case 653: return 109;
                case 654: return 110;
                case 656: return 111;
                case 659: return 112;
                case 662: return 113;
                case 663: return 114;
                case 664: return 115;
                case 665: return 116;
                case 674: return 117;
                case 675: return 118;
                case 676: return 119;
                case 678: return 120;
                case 682: return 121;
                case 683: return 122;
                case 684: return 123;
                case 685: return 124;
                case 690: return 125;
                case 692: return 126;
                case 693: return 127;
                case 700: return 128;
                case 701: return 129;
                case 702: return 130;
                case 703: return 131;
                case 704: return 132;
                case 705: return 133;
                case 706: return 134;
                case 707: return 135;
                case 708: return 136;
                case 709: return 137;
                case 710: return 138;
                case 711: return 139;
                case 712: return 140;
                case 713: return 141;
                case 714: return 142;
                case 715: return 143;
                case 750: return 144;
                case 752: return 145;
                case 753: return 146;
                case 754: return 147;
                case 755: return 148;
                case 756: return 149;
                case 757: return 150;
                case 758: return 151;
                case 759: return 152;
                case 760: return 153;
                case 762: return 154;
                case 763: return 155;
                case 764: return 156;
                case 765: return 157;
                case 769: return 158;
                case 773: return 159;
                case 775: return 160;
                case 779: return 161;
                case 780: return 162;
                case 781: return 163;
                case 785: return 164;
                case 789: return 165;
                case 795: return 166;
                case 796: return 167;
                case 799: return 168;
                case 800: return 169;
                case 801: return 170;
                case 802: return 171;
                case 803: return 172;
                case 804: return 173;
                case 805: return 174;
                case 806: return 175;
                case 807: return 176;
                case 808: return 177;
                case 812: return 178;
                case 813: return 179;
                case 814: return 180;
                case 815: return 181;
                case 816: return 182;
                case 817: return 183;
                case 818: return 184;
                case 819: return 185;
                case 820: return 186;
                case 821: return 187;
                case 822: return 188;
                case 823: return 189;
                case 824: return 190;
                case 825: return 191;
                case 826: return 192;
                case 827: return 193;
                case 828: return 194;
                case 829: return 195;
                case 830: return 196;
                case 834: return 197;
                case 835: return 198;
                case 836: return 199;
                case 840: return 200;
                case 841: return 201;
                case 842: return 202;
                case 844: return 203;
                case 848: return 204;
                case 858: return 205;
                case 859: return 206;
                case 860: return 207;
                case 861: return 208;
                case 862: return 209;
                case 863: return 210;
                case 868: return 211;
                case 869: return 212;
                case 877: return 213;
                case 879: return 214;
                case 880: return 215;
                case 921: return 216;
                case 922: return 217;
                case 923: return 218;

                default:
                    throw new Exception("Unknown Asset/Avatar_lad.xml ParamID: " + AssetParamID);
            }
        }

        /// <summary>
        /// Get the Asset ParamID (avatar_lad.xml) value based on a Visual Param index from AgentSetApperance
        /// </summary>
        /// <param name="VisualParamIdx"></param>
        /// <returns></returns>
        protected static int GetParamID(int VisualParamIdx)
        {
            switch (VisualParamIdx)
            {
                case 1: return 1;
                case 2: return 2;
                case 3: return 4;
                case 4: return 5;
                case 5: return 6;
                case 6: return 7;
                case 7: return 8;
                case 8: return 10;
                case 9: return 11;
                case 10: return 12;
                case 11: return 13;
                case 12: return 14;
                case 13: return 15;
                case 14: return 16;
                case 15: return 17;
                case 16: return 18;
                case 17: return 19;
                case 18: return 20;
                case 19: return 21;
                case 20: return 22;
                case 21: return 23;
                case 22: return 24;
                case 23: return 25;
                case 24: return 27;
                case 25: return 31;
                case 26: return 33;
                case 27: return 34;
                case 28: return 35;
                case 29: return 36;
                case 30: return 37;
                case 31: return 38;
                case 32: return 80;
                case 33: return 93;
                case 34: return 98;
                case 35: return 99;
                case 36: return 105;
                case 37: return 108;
                case 38: return 110;
                case 39: return 111;
                case 40: return 112;
                case 41: return 113;
                case 42: return 114;
                case 43: return 115;
                case 44: return 116;
                case 45: return 117;
                case 46: return 119;
                case 47: return 130;
                case 48: return 131;
                case 49: return 132;
                case 50: return 133;
                case 51: return 134;
                case 52: return 135;
                case 53: return 136;
                case 54: return 137;
                case 55: return 140;
                case 56: return 141;
                case 57: return 142;
                case 58: return 143;
                case 59: return 150;
                case 60: return 155;
                case 61: return 157;
                case 62: return 162;
                case 63: return 163;
                case 64: return 165;
                case 65: return 166;
                case 66: return 167;
                case 67: return 168;
                case 68: return 169;
                case 69: return 177;
                case 70: return 181;
                case 71: return 182;
                case 72: return 183;
                case 73: return 184;
                case 74: return 185;
                case 75: return 192;
                case 76: return 193;
                case 77: return 196;
                case 78: return 198;
                case 79: return 503;
                case 80: return 505;
                case 81: return 506;
                case 82: return 507;
                case 83: return 508;
                case 84: return 513;
                case 85: return 514;
                case 86: return 515;
                case 87: return 517;
                case 88: return 518;
                case 89: return 603;
                case 90: return 604;
                case 91: return 605;
                case 92: return 606;
                case 93: return 607;
                case 94: return 608;
                case 95: return 609;
                case 96: return 616;
                case 97: return 617;
                case 98: return 619;
                case 99: return 624;
                case 100: return 625;
                case 101: return 629;
                case 102: return 637;
                case 103: return 638;
                case 104: return 646;
                case 105: return 647;
                case 106: return 649;
                case 107: return 650;
                case 108: return 652;
                case 109: return 653;
                case 110: return 654;
                case 111: return 656;
                case 112: return 659;
                case 113: return 662;
                case 114: return 663;
                case 115: return 664;
                case 116: return 665;
                case 117: return 674;
                case 118: return 675;
                case 119: return 676;
                case 120: return 678;
                case 121: return 682;
                case 122: return 683;
                case 123: return 684;
                case 124: return 685;
                case 125: return 690;
                case 126: return 692;
                case 127: return 693;
                case 128: return 700;
                case 129: return 701;
                case 130: return 702;
                case 131: return 703;
                case 132: return 704;
                case 133: return 705;
                case 134: return 706;
                case 135: return 707;
                case 136: return 708;
                case 137: return 709;
                case 138: return 710;
                case 139: return 711;
                case 140: return 712;
                case 141: return 713;
                case 142: return 714;
                case 143: return 715;
                case 144: return 750;
                case 145: return 752;
                case 146: return 753;
                case 147: return 754;
                case 148: return 755;
                case 149: return 756;
                case 150: return 757;
                case 151: return 758;
                case 152: return 759;
                case 153: return 760;
                case 154: return 762;
                case 155: return 763;
                case 156: return 764;
                case 157: return 765;
                case 158: return 769;
                case 159: return 773;
                case 160: return 775;
                case 161: return 779;
                case 162: return 780;
                case 163: return 781;
                case 164: return 785;
                case 165: return 789;
                case 166: return 795;
                case 167: return 796;
                case 168: return 799;
                case 169: return 800;
                case 170: return 801;
                case 171: return 802;
                case 172: return 803;
                case 173: return 804;
                case 174: return 805;
                case 175: return 806;
                case 176: return 807;
                case 177: return 808;
                case 178: return 812;
                case 179: return 813;
                case 180: return 814;
                case 181: return 815;
                case 182: return 816;
                case 183: return 817;
                case 184: return 818;
                case 185: return 819;
                case 186: return 820;
                case 187: return 821;
                case 188: return 822;
                case 189: return 823;
                case 190: return 824;
                case 191: return 825;
                case 192: return 826;
                case 193: return 827;
                case 194: return 828;
                case 195: return 829;
                case 196: return 830;
                case 197: return 834;
                case 198: return 835;
                case 199: return 836;
                case 200: return 840;
                case 201: return 841;
                case 202: return 842;
                case 203: return 844;
                case 204: return 848;
                case 205: return 858;
                case 206: return 859;
                case 207: return 860;
                case 208: return 861;
                case 209: return 862;
                case 210: return 863;
                case 211: return 868;
                case 212: return 869;
                case 213: return 877;
                case 214: return 879;
                case 215: return 880;
                case 216: return 921;
                case 217: return 922;
                case 218: return 923;

                default:
                    throw new Exception("Unknown Visual Param (AgentSetApperance) index: " + VisualParamIdx);
            }
        }

        #endregion
    }
}
