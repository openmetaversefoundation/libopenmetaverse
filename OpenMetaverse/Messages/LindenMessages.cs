using System;
using OpenMetaverse.StructuredData;

namespace OpenMetaverse.Messages.Linden
{
    public class TeleportFinishMessage
    {
        public UUID AgentID;
        public int LocationID;
        public ulong RegionHandle;
        public Uri SeedCapability;
        public SimAccess SimAccess;
        public IPAddress IP;
        public int Port;
        public TeleportFlags Flags;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(1);

            OSDArray infoArray = new OSDArray(1);

            OSDMap info = new OSDMap(8);
            info.Add("AgentID", OSD.FromUUID(AgentID));
            info.Add("LocationID", OSD.FromInteger(LocationID)); // Unused by the client
            info.Add("RegionHandle", OSD.FromULong(RegionHandle));
            info.Add("SeedCapability", OSD.FromUri(SeedCapability));
            info.Add("SimAccess", OSD.FromInteger((byte)SimAccess));
            info.Add("SimIP", OSD.FromBinary(IP.GetAddressBytes()));
            info.Add("SimPort", OSD.FromInteger(Port));
            info.Add("TeleportFlags", OSD.FromUInteger((uint)Flags));

            infoArray.Add(info);

            map.Add("Info", infoArray);

            return map;
        }

        public void Deserialize(OSDMap map)
        {
            OSDArray array = (OSDArray)map["Info"];
            OSDMap blockMap = (OSDMap)array[0];

            AgentID = blockMap["AgentID"].AsUUID();
            LocationID = blockMap["LocationID"].AsInteger();
            RegionHandle = blockMap["RegionHandle"].AsULong();
            SeedCapability = blockMap["SeedCapability"].AsUri();
            SimAccess = (SimAccess)blockMap["SimAccess"].AsInteger();
            IP = new IPAddress(blockMap["SimIP"].AsBinary());
            Port = blockMap["SimPort"].AsInteger();
            Flags = (TeleportFlags)blockMap["TeleportFlags"].AsUInteger();
        }
    }

    public class EstablishAgentCommunicationMessage
    {
        public UUID AgentID;
        public IPAddress Address;
        public int Port;
        public Uri SeedCapability;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(3);
            map["agent-id"] = OSD.FromUUID(AgentID);
            map["sim-ip-and-port"] = OSD.FromString(String.Format("{0}:{1}", Address, Port));
            map["seed-capability"] = OSD.FromUri(SeedCapability);
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            string ipAndPort = map["sim-ip-and-port"].AsString();
            int i = ipAndPort.IndexOf(':');

            AgentID = map["agent-id"].AsUUID();
            Address = IPAddress.Parse(ipAndPort.Substring(0, i));
            Port = Int32.Parse(ipAndPort.Substring(i + 1));
            SeedCapability = map["seed-capability"].AsUri();
        }
    }

    public class CrossedRegionMessage
    {
        public Vector3 LookAt;
        public Vector3 Position;
        public UUID AgentID;
        public UUID SessionID;
        public ulong RegionHandle;
        public Uri SeedCapability;
        public IPAddress IP;
        public int Port;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(3);

            OSDArray infoArray = new OSDArray(1);
            OSDMap infoMap = new OSDMap(2);
            infoMap["LookAt"] = OSD.FromVector3(LookAt);
            infoMap["Position"] = OSD.FromVector3(Position);
            infoArray.Add(infoMap);
            map["Info"] = infoArray;

            OSDArray agentDataArray = new OSDArray(1);
            OSDMap agentDataMap = new OSDMap(2);
            agentDataMap["AgentID"] = OSD.FromUUID(AgentID);
            agentDataMap["SessionID"] = OSD.FromUUID(SessionID);
            agentDataArray.Add(agentDataMap);
            map["AgentData"] = agentDataArray;

            OSDArray regionDataArray = new OSDArray(1);
            OSDMap regionDataMap = new OSDMap(4);
            regionDataMap["RegionHandle"] = OSD.FromULong(RegionHandle);
            regionDataMap["SeedCapability"] = OSD.FromUri(SeedCapability);
            regionDataMap["SimIP"] = OSD.FromBinary(IP.GetAddressBytes());
            regionDataMap["SimPort"] = OSD.FromInteger(Port);
            regionDataArray.Add(regionDataMap);
            map["RegionData"] = regionDataArray;

            return map;
        }

        public void Deserialize(OSDMap map)
        {
            OSDMap infoMap = (OSDMap)((OSDArray)map["Info"])[0];
            LookAt = infoMap["LookAt"].AsVector3();
            Position = infoMap["Position"].AsVector3();

            OSDMap agentDataMap = (OSDMap)((OSDArray)map["AgentData"])[0];
            AgentID = agentDataMap["AgentID"].AsUUID();
            SessionID = agentDataMap["SessionID"].AsUUID();

            OSDMap regionDataMap = (OSDMap)((OSDArray)map["RegionData"])[0];
            RegionHandle = regionDataMap["RegionHandle"].AsULong();
            SeedCapability = regionDataMap["SeedCapability"].AsUri();
            IP = new IPAddress(regionDataMap["SimIP"].AsBinary());
            Port = regionDataMap["SimPort"].AsInteger();
        }
    }

    public class EnableSimulatorMessage
    {
        public class SimulatorInfoBlock
        {
            public ulong RegionHandle;
            public IPAddress IP;
            public int Port;
        }

        public SimulatorInfoBlock[] Simulators;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(1);

            OSDArray array = new OSDArray(Simulators.Length);
            for (int i = 0; i < Simulators.Length; i++)
            {
                SimulatorInfoBlock block = Simulators[i];

                OSDMap blockMap = new OSDMap(3);
                blockMap["Handle"] = OSD.FromULong(block.RegionHandle);
                blockMap["IP"] = OSD.FromBinary(block.IP.GetAddressBytes());
                blockMap["Port"] = OSD.FromInteger(block.Port);
                array.Add(blockMap);
            }

            map["SimulatorInfo"] = array;
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            OSDArray array = (OSDArray)map["SimulatorInfo"];
            Simulators = new SimulatorInfoBlock[array.Count];

            for (int i = 0; i < array.Count; i++)
            {
                OSDMap blockMap = (OSDMap)array[i];

                SimulatorInfoBlock block = new SimulatorInfoBlock();
                block.RegionHandle = blockMap["Handle"].AsULong();
                block.IP = new IPAddress(blockMap["IP"].AsBinary());
                block.Port = blockMap["Port"].AsInteger();
                Simulators[i] = block;
            }
        }
    }

    /// <summary>
    /// Contains a list of prim owner information for a specific parcel in a simulator
    /// </summary>
    /// <remarks>
    /// A Simulator will always return at least 1 entry
    /// If agent does not have proper permission the OwnerID will be UUID.Zero
    /// If agent does not have proper permission OR there are no primitives on parcel
    /// the DataBlocksExtended map will not be sent from the simulator
    /// </remarks>
    public class ParcelObjectOwnersMessage
    {
        /// <summary>
        /// Prim ownership information for a specified owner on a single parcel
        /// </summary>
        public class PrimOwners
        {
            /// <summary>The <see cref="UUID"/> of the prim owner, 
            /// UUID.Zero if agent has no permission</summary>
            public UUID OwnerID;
            /// <summary>The total number of prims on parcel owned</summary>
            public int Count;
            /// <summary>True if the Owner is a group</summary>
            public bool IsGroupOwned;
            /// <summary>True if the owner is online 
            /// <remarks>This is no longer used by the LL Simulators</remarks></summary>
            public bool OnlineStatus;
            /// <summary>The date of the newest prim</summary>
            public DateTime TimeStamp;
        }

        /// <summary>
        /// An Array of Datablocks containing prim owner information
        /// </summary>
        public PrimOwners[] DataBlocks;

        /// <summary>
        /// Create an OSDMap from the parcel prim owner information
        /// </summary>
        /// <returns></returns>
        public OSDMap Serialize()
        {
            OSDArray dataArray = new OSDArray(DataBlocks.Length);
            OSDArray dataExtendedArray = new OSDArray();

            for (int i = 0; i < DataBlocks.Length; i++)
            {
                OSDMap dataMap = new OSDMap(4);
                dataMap["OwnerID"] = OSD.FromUUID(DataBlocks[i].OwnerID);
                dataMap["Count"] = OSD.FromInteger(DataBlocks[i].Count);
                dataMap["IsGroupOwned"] = OSD.FromBoolean(DataBlocks[i].IsGroupOwned);
                dataMap["OnlineStatus"] = OSD.FromBoolean(DataBlocks[i].OnlineStatus);
                dataArray.Add(dataMap);

                /* If the tmestamp is null, don't create the DataExtended map, this 
                 * is usually when the parcel contains no primitives, or the agent does not have
                 * permissions to see ownership information */
                if (DataBlocks[i].TimeStamp != null)
                {
                    OSDMap dataExtendedMap = new OSDMap(1);
                    dataExtendedMap["TimeStamp"] = OSD.FromDate(DataBlocks[i].TimeStamp);
                    dataExtendedArray.Add(dataExtendedMap);
                }
            }

            OSDMap map = new OSDMap();
            map.Add("Data", dataArray);
            if(dataExtendedArray.Count > 0)
                map.Add("DataExtended", dataExtendedArray);

            return map;
        }

        /// <summary>
        /// Convert an OSDMap into the a strongly typed object containing 
        /// prim ownership information
        /// </summary>
        /// <param name="map"></param>
        public void Deserialize(OSDMap map)
        {
            OSDArray dataArray = (OSDArray)map["Data"];

            // DataExtended is optional, will not exist of parcel contains zero prims
            OSDArray dataExtendedArray;
            if (map.ContainsKey("DataExtended"))
            {
                dataExtendedArray = (OSDArray)map["DataExtended"];
            }
            else
            {
                dataExtendedArray = new OSDArray();
            }

            DataBlocks = new PrimOwners[dataArray.Count];

            for (int i = 0; i < dataArray.Count; i++)
            {
                OSDMap dataMap = (OSDMap)dataArray[i];
                PrimOwners block = new PrimOwners();
                block.OwnerID = dataMap["OwnerID"].AsUUID();
                block.Count = dataMap["Count"].AsInteger();
                block.IsGroupOwned = dataMap["IsGroupOwned"].AsBoolean();
                block.OnlineStatus = dataMap["OnlineStatus"].AsBoolean(); // deprecated

                /* if the agent has no permissions, or there are no prims, the counts
                 * should not match up, so we don't decode the DataExtended map */
                if (dataExtendedArray.Count == dataArray.Count)
                {
                    OSDMap dataExtendedMap = (OSDMap)dataExtendedArray[i];
                    block.TimeStamp = dataExtendedMap["TimeStamp"].AsDate();
                }

                DataBlocks[i] = block;  
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
        public uint AuctionID;
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
            AuctionID = (uint)parcelDataMap["AuctionID"].AsInteger();
            AuthBuyerID = parcelDataMap["AuthBuyerID"].AsUUID();
            Bitmap = parcelDataMap["Bitmap"].AsBinary();
            Category = (ParcelCategory)parcelDataMap["Category"].AsInteger();
            ClaimDate = Utils.UnixTimeToDateTime((uint)parcelDataMap["ClaimDate"].AsInteger());
            ClaimPrice = parcelDataMap["ClaimPrice"].AsInteger();
            Desc = parcelDataMap["Desc"].AsString();

            // LL sends this as binary, we'll convert it here
            if (parcelDataMap["ParcelFlags"].Type == OSDType.Binary)
            {
                byte[] bytes = parcelDataMap["ParcelFlags"].AsBinary();
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(bytes);
                ParcelFlags = (ParcelFlags)BitConverter.ToUInt32(bytes, 0);
            }
            else
            {
                ParcelFlags = (ParcelFlags)parcelDataMap["ParcelFlags"].AsInteger(); // verify this!
            }
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

    public class ParcelPropertiesUpdateMessage
    {
        public UUID AuthBuyerID;
        public bool MediaAutoScale;
        public ParcelCategory Category;
        public string Desc;
        public UUID GroupID;
        public LandingType Landing;
        public int LocalID;
        public string MediaDesc;
        public int MediaHeight;
        public bool MediaLoop;
        public UUID MediaID;
        public string MediaType;
        public string MediaURL;
        public int MediaWidth;
        public string MusicURL;
        public string Name;
        public bool ObscureMedia;
        public bool ObscureMusic;
        public ParcelFlags ParcelFlags;
        public float PassHours;
        public uint PassPrice;
        public uint SalePrice;
        public UUID SnapshotID;
        public Vector3 UserLocation;
        public Vector3 UserLookAt;

        public void Deserialize(OSDMap map)
        {
            AuthBuyerID = map["auth_buyer_id"].AsUUID();
            MediaAutoScale = map["auto_scale"].AsBoolean();
            Category = (ParcelCategory)map["category"].AsInteger();
            Desc = map["description"].AsString();
            GroupID = map["group_id"].AsUUID();
            Landing = (LandingType)map["landing_type"].AsUInteger();
            LocalID = map["local_id"].AsInteger();
            MediaDesc = map["media_desc"].AsString();
            MediaHeight = map["media_height"].AsInteger();
            MediaLoop = map["media_loop"].AsBoolean();
            MediaID = map["media_id"].AsUUID();
            MediaType = map["media_type"].AsString();
            MediaURL = map["media_url"].AsString();
            MediaWidth = map["media_width"].AsInteger();
            MusicURL = map["music_url"].AsString();
            Name = map["name"].AsString();
            ObscureMedia = map["obscure_media"].AsBoolean();
            ObscureMusic = map["obscure_music"].AsBoolean();
            ParcelFlags = (ParcelFlags)map["parcel_flags"].AsInteger();
            PassHours = (float)map["pass_hours"].AsReal();
            PassPrice = map["pass_price"].AsUInteger();
            SalePrice = map["sale_price"].AsUInteger();
            SnapshotID = map["snapshot_id"].AsUUID();
            UserLocation = map["user_location"].AsVector3();
            UserLookAt = map["user_look_at"].AsVector3();
        }

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap();
            map["auth_buyer_id"] = OSD.FromUUID(AuthBuyerID);
            map["auto_scale"] = OSD.FromBoolean(MediaAutoScale);
            map["category"] = OSD.FromInteger((byte)Category);
            map["description"] = OSD.FromString(Desc);
            map["flags"] = OSD.FromBinary(Utils.EmptyBytes);
            map["group_id"] = OSD.FromUUID(GroupID);
            map["landing_type"] = OSD.FromInteger((byte)Landing);
            map["local_id"] = OSD.FromInteger(LocalID);
            map["media_desc"] = OSD.FromString(MediaDesc);
            map["media_height"] = OSD.FromInteger(MediaHeight);
            map["media_id"] = OSD.FromUUID(MediaID);
            map["media_loop"] = OSD.FromBoolean(MediaLoop);
            map["media_type"] = OSD.FromString(MediaType);
            map["media_url"] = OSD.FromString(MediaURL);
            map["media_width"] = OSD.FromInteger(MediaWidth);
            map["music_url"] = OSD.FromString(MusicURL);
            map["name"] = OSD.FromString(Name);
            map["obscure_media"] = OSD.FromBoolean(ObscureMedia);
            map["obscure_music"] = OSD.FromBoolean(ObscureMusic);
            // is this endian correct?
            map["parcel_flags"] = OSD.FromInteger((int)ParcelFlags);
            map["pass_hours"] = OSD.FromReal(PassHours);
            map["pass_price"] = OSD.FromInteger(PassPrice);
            map["sale_price"] = OSD.FromInteger(SalePrice);
            map["snapshot_id"] = OSD.FromUUID(SnapshotID);
            map["user_location"] = OSD.FromVector3(UserLocation);
            map["user_look_at"] = OSD.FromVector3(UserLookAt);

            return map;
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
