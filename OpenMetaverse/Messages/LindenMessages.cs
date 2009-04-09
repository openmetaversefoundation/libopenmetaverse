using System;
using OpenMetaverse.StructuredData;

namespace OpenMetaverse.Messages.Linden
{
    public class PrimOwnersListMessage
    {
        public class DataBlock
        {
            public UUID OwnerID;
            public int Count;
            public bool IsGroupOwned;
            public bool OnlineStatus;
            public DateTime TimeStamp;
        }

        public DataBlock[] DataBlocks;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(2);

            OSDArray dataArray = new OSDArray(DataBlocks.Length);
            OSDArray dataExtendedArray = new OSDArray(DataBlocks.Length);

            for (int i = 0; i < DataBlocks.Length; i++)
            {
                OSDMap dataMap = new OSDMap(4);
                dataMap["OwnerID"] = OSD.FromUUID(DataBlocks[i].OwnerID);
                dataMap["Count"] = OSD.FromInteger(DataBlocks[i].Count);
                dataMap["IsGroupOwned"] = OSD.FromBoolean(DataBlocks[i].IsGroupOwned);
                dataMap["OnlineStatus"] = OSD.FromBoolean(DataBlocks[i].OnlineStatus);
                dataArray.Add(dataMap);

                OSDMap dataExtendedMap = new OSDMap(1);
                dataExtendedMap["TimeStamp"] = OSD.FromDate(DataBlocks[i].TimeStamp);
                dataExtendedArray.Add(dataExtendedMap);

            }

            map["Data"] = dataArray;
            map["DataExtended"] = dataExtendedArray;

            return map;
        }

        public void Deserialize(OSDMap map)
        {
            OSDArray dataArray = (OSDArray)map["Data"];
            OSDArray dataExtendedArray = (OSDArray)map["DataExtended"];

            DataBlocks = new DataBlock[dataArray.Count];
            for (int i = 0; i < dataArray.Count; i++)
            {
                OSDMap dataMap = (OSDMap)dataArray[i];
                DataBlock block = new DataBlock();
                block.OwnerID = dataMap["OwnerID"].AsUUID();
                block.Count = dataMap["Count"].AsInteger();
                block.IsGroupOwned = dataMap["IsGroupOwned"].AsBoolean();
                block.OnlineStatus = dataMap["OnlineStatus"].AsBoolean(); // deprecated

                OSDMap dataExtendedMap = (OSDMap)dataExtendedArray[i];
                block.TimeStamp = dataExtendedMap["TimeStamp"].AsDate();
            }
        }
    }

    /// <summary>
    /// The details of a single parcel in a region, also contains some regionwide globals
    /// </summary>
    public class ParcelPropertiesMessage
    {
        public int LocalID;
        public Vector3 AABBMax;
        public Vector3 AABBMin;
        public int Area;
        public int AuctionID;
        public UUID AuthBuyerID;
        public byte[] Bitmap;
        public ParcelCategory Category;
        public DateTime ClaimDate;
        public int ClaimPrice;
        public string Desc;
        public ParcelFlags ParcelFlags;
        public UUID GroupID;
        public int GroupPrims;
        public bool IsGroupOwned;
        public LandingType LandingType;
        public int MaxPrims;
        public UUID MediaID;
        public string MediaURL;
        public bool MediaAutoScale;
        public string MusicURL;
        public string Name;
        public int OtherCleanTime;
        public int OtherCount;
        public int OtherPrims;
        public UUID OwnerID;
        public int OwnerPrims;
        public float ParcelPrimBonus;
        public float PassHours;
        public int PassPrice;
        public int PublicCount;
        public bool RegionDenyAnonymous;
        public bool RegionPushOverride;
        public int RentPrice;
        public ParcelResult RequestResult;
        public int SalePrice;
        public int SelectedPrims;
        public int SelfCount;
        public int SequenceID;
        public int SimWideMaxPrims;
        public int SimWideTotalPrims;
        public bool SnapSelection;
        public UUID SnapshotID;
        public ParcelStatus Status;
        public int TotalPrims;
        public Vector3 UserLocation;
        public Vector3 UserLookAt;

        public bool RegionDenyAgeUnverified;

        public string MediaDesc;
        public int MediaHeight;
        public int MediaWidth;
        public bool MediaLoop;
        public string MediaType;
        public bool ObscureMedia;
        public bool ObscureMusic;

        public OSDMap Serialize()
        {

            OSDMap map = new OSDMap(3);

            OSDArray dataArray = new OSDArray(1);
            OSDMap parcelDataMap = new OSDMap(47);
            parcelDataMap["LocalID"] = OSD.FromInteger(LocalID);
            parcelDataMap["AABBMax"] = OSD.FromVector3(AABBMax);
            parcelDataMap["AABBMin"] = OSD.FromVector3(AABBMin);
            parcelDataMap["Area"] = OSD.FromInteger(Area);
            parcelDataMap["AuctionID"] = OSD.FromInteger(AuctionID);
            parcelDataMap["AuthBuyerID"] = OSD.FromUUID(AuthBuyerID);
            parcelDataMap["Bitmap"] = OSD.FromBinary(Bitmap);
            parcelDataMap["Category"] = OSD.FromInteger((int)Category);
            parcelDataMap["ClaimDate"] = OSD.FromDate(ClaimDate);
            parcelDataMap["ClaimPrice"] = OSD.FromInteger(ClaimPrice);
            parcelDataMap["Desc"] = OSD.FromString(Desc);
            parcelDataMap["ParcelFlags"] = OSD.FromLong((long)ParcelFlags); // verify this!
            parcelDataMap["GroupID"] = OSD.FromUUID(GroupID);
            parcelDataMap["GroupPrims"] = OSD.FromInteger(GroupPrims);
            parcelDataMap["IsGroupOwned"] = OSD.FromBoolean(IsGroupOwned);
            parcelDataMap["LandingType"] = OSD.FromInteger((int)LandingType);
            parcelDataMap["MaxPrims"] = OSD.FromInteger(MaxPrims);
            parcelDataMap["MediaID"] = OSD.FromUUID(MediaID);
            parcelDataMap["MediaURL"] = OSD.FromString(MediaURL);
            parcelDataMap["MediaAutoScale"] = OSD.FromBoolean(MediaAutoScale);
            parcelDataMap["MusicURL"] = OSD.FromString(MusicURL);
            parcelDataMap["Name"] = OSD.FromString(Name);
            parcelDataMap["OtherCleanTime"] = OSD.FromInteger(OtherCleanTime);
            parcelDataMap["OtherCount"] = OSD.FromInteger(OtherCount);
            parcelDataMap["OtherPrims"] = OSD.FromInteger(OtherPrims);
            parcelDataMap["OwnerID"] = OSD.FromUUID(OwnerID);
            parcelDataMap["OwnerPrims"] = OSD.FromInteger(OwnerPrims);
            parcelDataMap["ParcelPrimBonus"] = OSD.FromReal((float)ParcelPrimBonus);
            parcelDataMap["PassHours"] = OSD.FromReal((float)PassHours);
            parcelDataMap["PassPrice"] = OSD.FromInteger(PassPrice);
            parcelDataMap["PublicCount"] = OSD.FromInteger(PublicCount);
            parcelDataMap["RegionDenyAnonymous"] = OSD.FromBoolean(RegionDenyAnonymous);
            parcelDataMap["RegionPushOverride"] = OSD.FromBoolean(RegionPushOverride);
            parcelDataMap["RentPrice"] = OSD.FromInteger(RentPrice);
            parcelDataMap["RequestResult"] = OSD.FromInteger((int)RequestResult);
            parcelDataMap["SalePrice"] = OSD.FromInteger(SalePrice);
            parcelDataMap["SelectedPrims"] = OSD.FromInteger(SelectedPrims);
            parcelDataMap["SelfCount"] = OSD.FromInteger(SelfCount);
            parcelDataMap["SequenceID"] = OSD.FromInteger(SequenceID);
            parcelDataMap["SimWideMaxPrims"] = OSD.FromInteger(SimWideMaxPrims);
            parcelDataMap["SimWideTotalPrims"] = OSD.FromInteger(SimWideTotalPrims);
            parcelDataMap["SnapSelection"] = OSD.FromBoolean(SnapSelection);
            parcelDataMap["SnapshotID"] = OSD.FromUUID(SnapshotID);
            parcelDataMap["Status"] = OSD.FromInteger((int)Status);
            parcelDataMap["TotalPrims"] = OSD.FromInteger(TotalPrims);
            parcelDataMap["UserLocation"] = OSD.FromVector3(UserLocation);
            parcelDataMap["UserLookAt"] = OSD.FromVector3(UserLookAt);
            dataArray.Add(parcelDataMap);
            map["ParcelData"] = dataArray;

            OSDArray mediaDataArray = new OSDArray(1);
            OSDMap mediaDataMap = new OSDMap(7);
            mediaDataMap["MediaDesc"] = OSD.FromString(MediaDesc);
            mediaDataMap["MediaHeight"] = OSD.FromInteger(MediaHeight);
            mediaDataMap["MediaWidth"] = OSD.FromInteger(MediaWidth);
            mediaDataMap["MediaLoop"] = OSD.FromBoolean(MediaLoop);
            mediaDataMap["MediaType"] = OSD.FromString(MediaType);
            mediaDataMap["ObscureMedia"] = OSD.FromBoolean(ObscureMedia);
            mediaDataMap["ObscureMusic"] = OSD.FromBoolean(ObscureMusic);
            mediaDataArray.Add(mediaDataMap);
            map["MediaData"] = mediaDataArray;

            OSDArray ageVerificationBlockArray = new OSDArray(1);
            OSDMap ageVerificationBlockMap = new OSDMap(1);
            ageVerificationBlockMap["RegionDenyAgeUnverified"] = OSD.FromBoolean(RegionDenyAgeUnverified);
            ageVerificationBlockArray.Add(ageVerificationBlockMap);
            map["AgeVerificationBlock"] = ageVerificationBlockArray;

            return map;
        }

        public void Deserialize(OSDMap map)
        {
            OSDMap parcelDataMap = (OSDMap)((OSDArray)map["ParcelData"])[0];
            LocalID = parcelDataMap["LocalID"].AsInteger();
            AABBMax = parcelDataMap["AABBMax"].AsVector3();
            AABBMin = parcelDataMap["AABBMin"].AsVector3();
            Area = parcelDataMap["Area"].AsInteger();
            AuctionID = parcelDataMap["AuctionID"].AsInteger();
            AuthBuyerID = parcelDataMap["AuthBuyerID"].AsUUID();
            Bitmap = parcelDataMap["Bitmap"].AsBinary();
            Category = (ParcelCategory)parcelDataMap["Category"].AsInteger();
            ClaimDate = Utils.UnixTimeToDateTime((uint)parcelDataMap["ClaimDate"].AsInteger());
            ClaimPrice = parcelDataMap["ClaimPrice"].AsInteger();
            Desc = parcelDataMap["Desc"].AsString();
            ParcelFlags = (ParcelFlags)parcelDataMap["ParcelFlags"].AsLong(); // verify this!
            GroupID = parcelDataMap["GroupID"].AsUUID();
            GroupPrims = parcelDataMap["GroupPrims"].AsInteger();
            IsGroupOwned = parcelDataMap["IsGroupOwned"].AsBoolean();
            LandingType = (LandingType)parcelDataMap["LandingType"].AsInteger();
            MaxPrims = parcelDataMap["MaxPrims"].AsInteger();
            MediaID = parcelDataMap["MediaID"].AsUUID();
            MediaURL = parcelDataMap["MediaURL"].AsString();
            MediaAutoScale = parcelDataMap["MediaAutoScale"].AsBoolean(); // 0x1 = yes
            MusicURL = parcelDataMap["MusicURL"].AsString();
            Name = parcelDataMap["Name"].AsString();
            OtherCleanTime = parcelDataMap["OtherCleanTime"].AsInteger();
            OtherCount = parcelDataMap["OtherCount"].AsInteger();
            OtherPrims = parcelDataMap["OtherPrims"].AsInteger();
            OwnerID = parcelDataMap["OwnerID"].AsUUID();
            OwnerPrims = parcelDataMap["OwnerPrims"].AsInteger();
            ParcelPrimBonus = (float)parcelDataMap["ParcelPrimBonus"].AsReal();
            PassHours = (float)parcelDataMap["PassHours"].AsReal();
            PassPrice = parcelDataMap["PassPrice"].AsInteger();
            PublicCount = parcelDataMap["PublicCount"].AsInteger();
            RegionDenyAnonymous = parcelDataMap["RegionDenyAnonymous"].AsBoolean();
            RegionPushOverride = parcelDataMap["RegionPushOverride"].AsBoolean();
            RentPrice = parcelDataMap["RentPrice"].AsInteger();
            RequestResult = (ParcelResult)parcelDataMap["RequestResult"].AsInteger();
            SalePrice = parcelDataMap["SalePrice"].AsInteger();
            SelectedPrims = parcelDataMap["SelectedPrims"].AsInteger();
            SelfCount = parcelDataMap["SelfCount"].AsInteger();
            SequenceID = parcelDataMap["SequenceID"].AsInteger();
            SimWideMaxPrims = parcelDataMap["SimWideMaxPrims"].AsInteger();
            SimWideTotalPrims = parcelDataMap["SimWideTotalPrims"].AsInteger();
            SnapSelection = parcelDataMap["SnapSelection"].AsBoolean();
            SnapshotID = parcelDataMap["SnapshotID"].AsUUID();
            Status = (ParcelStatus)parcelDataMap["Status"].AsInteger();
            TotalPrims = parcelDataMap["TotalPrims"].AsInteger();
            UserLocation = parcelDataMap["UserLocation"].AsVector3();
            UserLookAt = parcelDataMap["UserLookAt"].AsVector3();

            OSDMap mediaDataMap = (OSDMap)((OSDArray)map["MediaData"])[0];
            MediaDesc = mediaDataMap["MediaDesc"].AsString();
            MediaHeight = mediaDataMap["MediaHeight"].AsInteger();
            MediaWidth = mediaDataMap["MediaWidth"].AsInteger();
            MediaLoop = mediaDataMap["MediaLoop"].AsBoolean();
            MediaType = mediaDataMap["MediaType"].AsString();
            ObscureMedia = mediaDataMap["ObscureMedia"].AsBoolean();
            ObscureMusic = mediaDataMap["ObscureMusic"].AsBoolean();

            OSDMap ageVerificationBlockMap = (OSDMap)((OSDArray)map["AgeVerificationBlock"])[0];
            RegionDenyAgeUnverified = ageVerificationBlockMap["RegionDenyAgeUnverified"].AsBoolean();
        }
    }

    public class ChatterboxSessionEventMessage
    {
        public bool Success;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(1);
            map["Success"] = OSD.FromBoolean(Success);

            return map;
        }

        public void Deserialize(OSDMap map)
        {
            Success = map["success"].AsBoolean();
        }
    }

    public class ChatterBoxSessionStartMessage
    {
        public UUID SessionID;
        public UUID TempSessionID;
        public bool Success;

        public string SessionName;
        // FIXME: Replace int with an enum
        public int Type;
        public bool VoiceEnabled;
        public bool ModeratedVoice;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(4);
            map.Add("session_id", OSD.FromUUID(SessionID));
            map.Add("temp_session_id", OSD.FromUUID(TempSessionID));
            map.Add("success", OSD.FromBoolean(Success));

            OSDMap sessionMap = new OSDMap(4);
            sessionMap.Add("type", OSD.FromInteger(Type));
            sessionMap.Add("session_name", OSD.FromString(SessionName));
            sessionMap.Add("voice_enabled", OSD.FromBoolean(VoiceEnabled));


            OSDMap moderatedMap = new OSDMap(1);
            moderatedMap.Add("voice", OSD.FromBoolean(ModeratedVoice));

            sessionMap.Add("moderated_mode", moderatedMap);

            map.Add("session_info", sessionMap);

            return map;
        }

        public void Deserialize(OSDMap map)
        {
            SessionID = map["session_id"].AsUUID();
            TempSessionID = map["temp_session_id"].AsUUID();
            Success = map["success"].AsBoolean();

            if (Success)
            {
                OSDMap sessionInfoMap = (OSDMap)map["session_info"];
                SessionName = sessionInfoMap["session_name"].AsString();
                Type = sessionInfoMap["type"].AsInteger();

                OSDMap moderatedModeMap = (OSDMap)sessionInfoMap["moderated_mode"];
                ModeratedVoice = moderatedModeMap["voice"].AsBoolean();
            }
        }
    }

    public class ChatterBoxInvitationMessage
    {
        /// <summary>Key of sender</summary>
        public UUID FromAgentID;
        /// <summary>Name of sender</summary>
        public string FromAgentName;
        /// <summary>Key of destination avatar</summary>
        public UUID ToAgentID;
        /// <summary>ID of originating estate</summary>
        public uint ParentEstateID;
        /// <summary>Key of originating region</summary>
        public UUID RegionID;
        /// <summary>Coordinates in originating region</summary>
        public Vector3 Position;
        /// <summary>Instant message type</summary>
        public InstantMessageDialog Dialog;
        /// <summary>Group IM session toggle</summary>
        public bool GroupIM;
        /// <summary>Key of IM session, for Group Messages, the groups UUID</summary>
        public UUID IMSessionID;
        /// <summary>Timestamp of the instant message</summary>
        public DateTime Timestamp;
        /// <summary>Instant message text</summary>
        public string Message;
        /// <summary>Whether this message is held for offline avatars</summary>
        public InstantMessageOnline Offline;
        /// <summary>Context specific packed data</summary>
        public byte[] BinaryBucket;

        public OSDMap Serialize()
        {
            throw new NotImplementedException();
        }

        public void Deserialize(OSDMap map)
        {
            OSDMap im = (OSDMap)map["instantmessage"];
            OSDMap msg = (OSDMap)im["message_params"];
            OSDMap msgdata = (OSDMap)msg["data"];

            FromAgentID = map["from_id"].AsUUID();
            FromAgentName = map["from_name"].AsString();
            ToAgentID = msg["to_id"].AsUUID();
            ParentEstateID = (uint)msg["parent_estate_id"].AsInteger();
            RegionID = msg["region_id"].AsUUID();
            Position = msg["position"].AsVector3();
            Dialog = (InstantMessageDialog)msgdata["type"].AsInteger();
            GroupIM = msg["from_group"].AsBoolean();
            IMSessionID = map["session_id"].AsUUID();
            Timestamp = new DateTime(msgdata["timestamp"].AsInteger());
            Message = msg["message"].AsString();
            Offline = (InstantMessageOnline)msg["offline"].AsInteger();
            BinaryBucket = msgdata["binary_bucket"].AsBinary();
        }
    }

    public class ModerateChatSessionsMessage
    {
        public OSDMap Serialize()
        {
            throw new NotImplementedException();
        }

        public void Deserialize(OSDMap map)
        {
            throw new NotImplementedException();
        }
    }

    public class ChatterBoxSessionAgentListMessage
    {
        public OSDMap Serialize()
        {
            throw new NotImplementedException();
        }

        public void Deserialize(OSDMap map)
        {
            throw new NotImplementedException();
        }
    }

    public class NewFileAgentInventoryMessage
    {
        public UUID FolderID;
        public AssetType AssetType;
        public InventoryType InventoryType;
        public string Name;
        public string Description;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(5);
            map["folder_id"] = OSD.FromUUID(FolderID);
            map["asset_type"] = OSD.FromString(Utils.AssetTypeToString(AssetType));
            map["inventory_type"] = OSD.FromString(Utils.InventoryTypeToString(InventoryType));
            map["name"] = OSD.FromString(Name);
            map["description"] = OSD.FromString(Description);

            return map;

        }

        public void Deserialize(OSDMap map)
        {
            FolderID = map["folder_id"].AsUUID();
            AssetType = Utils.StringToAssetType(map["asset_type"].AsString());
            InventoryType = Utils.StringToInventoryType(map["inventory_type"].AsString());
            Name = map["name"].AsString();
            Description = map["description"].AsString();
        }

    }

    public class UpdateNotecardAgentInventoryMessage
    {
        public UUID ItemID;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(1);
            map["item_id"] = OSD.FromUUID(ItemID);

            return map;
        }

        public void Deserialize(OSDMap map)
        {
            ItemID = map["item_id"].AsUUID();
        }
    }

    public class UpdateNotecardTaskInventoryMessage
    {
        public UUID TaskID;
        public UUID ItemID;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(1);
            map["task_id"] = OSD.FromUUID(TaskID);
            map["item_id"] = OSD.FromUUID(ItemID);

            return map;
        }

        public void Deserialize(OSDMap map)
        {
            TaskID = map["task_id"].AsUUID();
            ItemID = map["item_id"].AsUUID();
        }
    }
}
