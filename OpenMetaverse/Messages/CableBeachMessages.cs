/*
 * Copyright (c) 2009, openmetaverse.org
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without 
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.org nor the names 
 *   of its contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Net;
using OpenMetaverse.StructuredData;
using OpenMetaverse.Interfaces;

namespace OpenMetaverse.Messages.CableBeach
{
    /// <summary>
    /// Holds information about a grid region
    /// </summary>
    public struct RegionInfo
    {
        public string Name;
        public UUID ID;
        public ulong Handle;
        public bool Online;
        public IPAddress IP;
        public int Port;
        public Uri Address;
        public UUID MapTextureID;
        public Uri Owner;
        public RegionFlags Flags;
        public int AgentCount;
        public Dictionary<string, Uri> Capabilities;
        public float WaterHeight;
        public Vector3 DefaultPosition;
        public Vector3 DefaultLookAt;

        public uint X
        {
            get
            {
                uint x, y;
                OpenMetaverse.Utils.LongToUInts(Handle, out x, out y);
                return x / 256;
            }

            set
            {
                uint x, y;
                OpenMetaverse.Utils.LongToUInts(Handle, out x, out y);
                Handle = OpenMetaverse.Utils.UIntsToLong(value, y);
            }
        }

        public uint Y
        {
            get
            {
                uint x, y;
                OpenMetaverse.Utils.LongToUInts(Handle, out x, out y);
                return y / 256;
            }

            set
            {
                uint x, y;
                OpenMetaverse.Utils.LongToUInts(Handle, out x, out y);
                Handle = OpenMetaverse.Utils.UIntsToLong(x, value);
            }
        }

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap();
            map["name"] = OSD.FromString(Name);
            map["id"] = OSD.FromUUID(ID);
            map["handle"] = OSD.FromULong(Handle);
            map["online"] = OSD.FromBoolean(Online);
            map["ip"] = MessageUtils.FromIP(IP);
            map["port"] = OSD.FromInteger(Port);
            map["address"] = OSD.FromUri(Address);
            map["map_texture_id"] = OSD.FromUUID(MapTextureID);
            map["owner"] = OSD.FromUri(Owner);
            map["region_flags"] = OSD.FromInteger((int)Flags);
            map["agent_count"] = OSD.FromInteger(AgentCount);
            map["capabilities"] = MessageUtils.FromDictionaryUri(Capabilities);
            map["water_height"] = OSD.FromReal(WaterHeight);
            map["default_position"] = OSD.FromVector3(DefaultPosition);
            map["default_look_at"] = OSD.FromVector3(DefaultLookAt);
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            Name = map["name"].AsString();
            ID = map["id"].AsUUID();
            Handle = map["handle"].AsULong();
            Online = map["online"].AsBoolean();
            IP = MessageUtils.ToIP(map["ip"]);
            Port = map["port"].AsInteger();
            Address = map["address"].AsUri();
            MapTextureID = map["map_texture_id"].AsUUID();
            Owner = map["owner"].AsUri();
            Flags = (RegionFlags)map["region_flags"].AsInteger();
            AgentCount = map["agent_count"].AsInteger();
            Capabilities = MessageUtils.ToDictionaryUri(map["capabilities"]);
            WaterHeight = (float)map["water_height"].AsReal();
            DefaultPosition = map["default_position"].AsVector3();
            DefaultLookAt = map["default_look_at"].AsVector3();
        }

        public override string ToString()
        {
            string ret = String.Empty;
            if (!String.IsNullOrEmpty(Name))
                ret += Name + " ";

            ret += Online ? "[Online]" : "[Offline]";
            return ret;
        }
    }

    #region World Messages

    public class CreateRegionMessage : IMessage
    {
        public RegionInfo Region;

        public OSDMap Serialize()
        {
            return Region.Serialize();
        }

        public void Deserialize(OSDMap map)
        {
            Region = new RegionInfo();
            Region.Deserialize(map);
        }
    }

    public class CreateRegionReplyMessage : IMessage
    {
        public bool Success;
        public string Message;
        public UUID SimulationKey;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap();
            map["success"] = OSD.FromBoolean(Success);
            map["message"] = OSD.FromString(Message);
            map["simulation_key"] = OSD.FromUUID(SimulationKey);
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            Success = map["success"].AsBoolean();
            Message = map["message"].AsString();
            SimulationKey = map["simulation_key"].AsUUID();
        }
    }

    public class DeleteRegionMessage : IMessage
    {
        public UUID ID;
        public UUID SimulationKey;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap();
            map["id"] = OSD.FromUUID(ID);
            map["simulation_key"] = OSD.FromUUID(SimulationKey);
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            ID = map["id"].AsUUID();
            SimulationKey = map["simulation_key"].AsUUID();
        }
    }

    public class DeleteRegionReplyMessage : IMessage
    {
        public bool Success;
        public string Message;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap();
            map["success"] = OSD.FromBoolean(Success);
            map["message"] = OSD.FromString(Message);
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            Success = map["success"].AsBoolean();
            Message = map["message"].AsString();
        }
    }

    public class RegionUpdateMessage : IMessage
    {
        public RegionInfo Region;
        public UUID SimulationKey;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap();
            map["region"] = Region.Serialize();
            map["simulation_key"] = OSD.FromUUID(SimulationKey);
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            Region = new RegionInfo();
            Region.Deserialize((OSDMap)map["region"]);
            SimulationKey = map["simulation_key"].AsUUID();
        }
    }

    public class RegionUpdateReplyMessage : IMessage
    {
        public bool Success;
        public string Message;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap();
            map["success"] = OSD.FromBoolean(Success);
            map["message"] = OSD.FromString(Message);
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            Success = map["success"].AsBoolean();
            Message = map["message"].AsString();
        }
    }

    public interface FetchRegionQuery
    {
        OSDMap Serialize();
        void Deserialize(OSDMap map);
    }

    public class FetchRegionQueryID : FetchRegionQuery
    {
        public UUID ID;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap();
            map["id"] = OSD.FromUUID(ID);
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            ID = map["id"].AsUUID();
        }
    }

    public class FetchRegionQueryCoords : FetchRegionQuery
    {
        public int X;
        public int Y;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap();
            map["x"] = OSD.FromInteger(X);
            map["y"] = OSD.FromInteger(Y);
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            X = map["x"].AsInteger();
            Y = map["y"].AsInteger();
        }
    }

    public class FetchRegionQueryName : FetchRegionQuery
    {
        public string Name;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap();
            map["name"] = OSD.FromString(Name);
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            Name = map["name"].AsString();
        }
    }

    public class FetchRegionMessage : IMessage
    {
        public FetchRegionQuery Query;

        public OSDMap Serialize()
        {
            return Query.Serialize();
        }

        public void Deserialize(OSDMap map)
        {
            if (map.ContainsKey("id"))
                Query = new FetchRegionQueryID();
            else if (map.ContainsKey("name"))
                Query = new FetchRegionQueryName();
            else
                Query = new FetchRegionQueryCoords();

            Query.Deserialize(map);
        }
    }

    public class FetchRegionReplyMessage : IMessage
    {
        RegionInfo Region;

        public OSDMap Serialize()
        {
            return Region.Serialize();
        }

        public void Deserialize(OSDMap map)
        {
            Region = new RegionInfo();
            Region.Deserialize(map);
        }
    }

    public class FetchDefaultRegionReplyMessage : IMessage
    {
        public RegionInfo Region;

        public OSDMap Serialize()
        {
            return Region.Serialize();
        }

        public void Deserialize(OSDMap map)
        {
            Region = new RegionInfo();
            Region.Deserialize(map);
        }
    }

    public class RegionSearchMessage : IMessage
    {
        public string Query;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap();
            map["query"] = OSD.FromString(Query);
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            Query = map["query"].AsString();
        }
    }

    public class RegionSearchReplyMessage : IMessage
    {
        public RegionInfo[] Regions;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap();
            OSDArray array = new OSDArray(Regions.Length);
            for (int i = 0; i < Regions.Length; i++)
                array.Add(Regions[i].Serialize());
            map["regions"] = array;
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            OSDArray array = (OSDArray)map["regions"];
            Regions = new RegionInfo[array.Count];
            for (int i = 0; i < array.Count; i++)
            {
                RegionInfo region = new RegionInfo();
                region.Deserialize((OSDMap)array[i]);
                Regions[i] = region;
            }
        }
    }

    public class GetRegionCountReplyMessage : IMessage
    {
        public int Count;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap();
            map["count"] = OSD.FromInteger(Count);
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            Count = map["count"].AsInteger();
        }
    }

    #endregion World Messages

    #region Identity Messages

    public class RequestCapabilitiesMessage : IMessage
    {
        public Uri Identity;
        public string[] Capabilities;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(1);
            map["identity"] = OSD.FromUri(Identity);

            OSDArray array = new OSDArray(Capabilities.Length);
            for (int i = 0; i < Capabilities.Length; i++)
                array.Add(OSD.FromString(Capabilities[i]));
            map["capabilities"] = array;

            return map;
        }

        public void Deserialize(OSDMap map)
        {
            Identity = map["identity"].AsUri();

            OSDArray array = (OSDArray)map["capabilities"];
            Capabilities = new string[array.Count];
            for (int i = 0; i < array.Count; i++)
                Capabilities[i] = array[i].AsString();
        }
    }

    public class RequestCapabilitiesReplyMessage : IMessage
    {
        public Dictionary<string, Uri> Capabilities;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(1);
            OSDMap caps = new OSDMap(Capabilities.Count);
            foreach (KeyValuePair<string, Uri> entry in Capabilities)
                caps.Add(entry.Key, OSD.FromUri(entry.Value));
            map["capabilities"] = caps;
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            OSDMap caps = (OSDMap)map["capabilities"];
            Capabilities = new Dictionary<string, Uri>(caps.Count);
            foreach (KeyValuePair<string, OSD> entry in caps)
                Capabilities.Add(entry.Key, entry.Value.AsUri());
        }
    }

    #endregion Identity Messages

    #region Inventory Messages

    public class CreateInventoryMessage : IMessage
    {
        public Uri Identity;
        public string Name;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap();
            map["identity"] = OSD.FromUri(Identity);
            map["name"] = OSD.FromString(Name);
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            Identity = map["identity"].AsUri();
            Name = map["name"].AsString();
        }
    }

    public class CreateInventoryReplyMessage : IMessage
    {
        public UUID RootFolderID;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(1);
            map["root_folder_id"] = OSD.FromUUID(RootFolderID);
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            RootFolderID = map["root_folder_id"].AsUUID();
        }
    }

    public class GetInventorySkeletonMessage : IMessage
    {
        public Uri Identity;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap();
            map["identity"] = OSD.FromUri(Identity);
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            Identity = map["identity"].AsUri();
        }
    }

    public class GetInventorySkeletonReplyMessage : IMessage
    {
        public class Folder
        {
            public string Name;
            public UUID ParentID;
            public int Version;
            public string PreferredContentType;
            public UUID FolderID;
        }

        public Folder[] Folders;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap();

            OSDArray folders = new OSDArray(Folders.Length);
            for (int i = 0; i < Folders.Length; i++)
            {
                Folder folder = Folders[i];

                OSDMap folderMap = new OSDMap();
                folderMap["name"] = OSD.FromString(folder.Name);
                folderMap["parent_id"] = OSD.FromUUID(folder.ParentID);
                folderMap["version"] = OSD.FromInteger(folder.Version);
                folderMap["preferred_content_type"] = OSD.FromString(folder.PreferredContentType);
                folderMap["folder_id"] = OSD.FromUUID(folder.FolderID);

                folders.Add(folderMap);
            }

            map["folders"] = folders;
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            OSDArray folders = (OSDArray)map["folders"];
            Folders = new Folder[folders.Count];
            for (int i = 0; i < folders.Count; i++)
            {
                OSDMap folderMap = (OSDMap)folders[i];

                Folder folder = new Folder();
                folder.Name = folderMap["name"].AsString();
                folder.ParentID = folderMap["parent_id"].AsUUID();
                folder.Version = folderMap["version"].AsInteger();
                folder.PreferredContentType = folderMap["preferred_content_type"].AsString();
                folder.FolderID = folderMap["folder_id"].AsUUID();

                Folders[i] = folder;
            }
        }
    }

    public class GetActiveGesturesMessage : IMessage
    {
        public Uri Identity;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap();
            map["identity"] = OSD.FromUri(Identity);
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            Identity = map["identity"].AsUri();
        }
    }

    public class GetActiveGesturesReplyMessage : IMessage
    {
        public class Gesture
        {
            public UUID ItemID;
            public UUID AssetID;
        }

        public Gesture[] Gestures;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap();

            OSDArray gestures = new OSDArray();
            for (int i = 0; i < Gestures.Length; i++)
            {
                Gesture gesture = Gestures[i];
                OSDMap gestureMap = new OSDMap();
                gestureMap["item_id"] = OSD.FromUUID(gesture.ItemID);
                gestureMap["asset_id"] = OSD.FromUUID(gesture.AssetID);
                gestures.Add(gestureMap);
            }

            map["gestures"] = gestures;
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            OSDArray gestures = (OSDArray)map["gestures"];
            Gestures = new Gesture[gestures.Count];
            for (int i = 0; i < gestures.Count; i++)
            {
                OSDMap gestureMap = (OSDMap)gestures[i];
                Gesture gesture = new Gesture();
                gesture.ItemID = gestureMap["item_id"].AsUUID();
                gesture.AssetID = gestureMap["asset_id"].AsUUID();
                Gestures[i] = gesture;
            }
        }
    }

    #endregion Inventory Messages

    #region Simulator Messages

    public class EnableClientMessage : IMessage
    {
        public UUID AgentID;
        public UUID SessionID;
        public UUID SecureSessionID;
        public int CircuitCode;
        public ulong RegionHandle;
        public bool ChildAgent;
        public IPAddress IP;
        public string ClientVersion;
        public Dictionary<Uri, OSD> Attributes;
        public Dictionary<Uri, Dictionary<string, Uri>> Services;
        public Uri CallbackUri;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap();
            map["agent_id"] = OSD.FromUUID(AgentID);
            map["session_id"] = OSD.FromUUID(SessionID);
            map["secure_session_id"] = OSD.FromUUID(SecureSessionID);
            map["circuit_code"] = OSD.FromInteger(CircuitCode);
            map["region_handle"] = OSD.FromULong(RegionHandle);
            map["child_agent"] = OSD.FromBoolean(ChildAgent);
            map["ip"] = OSD.FromBinary(IP.GetAddressBytes());
            map["client_version"] = OSD.FromString(ClientVersion);

            OSDMap attributes = new OSDMap(Attributes.Count);
            foreach (KeyValuePair<Uri, OSD> entry in Attributes)
                attributes.Add(entry.Key.ToString(), entry.Value);
            map["attributes"] = attributes;

            OSDMap services = new OSDMap(Services.Count);
            foreach (KeyValuePair<Uri, Dictionary<string, Uri>> serviceEntry in Services)
            {
                OSDMap service = new OSDMap();
                foreach (KeyValuePair<string, Uri> entry in serviceEntry.Value)
                    service.Add(entry.Key, OSD.FromUri(entry.Value));
                services.Add(serviceEntry.Key.ToString(), service);
            }
            map["services"] = services;

            map["callback_uri"] = OSD.FromUri(CallbackUri);
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            AgentID = map["agent_id"].AsUUID();
            SessionID = map["session_id"].AsUUID();
            SecureSessionID = map["secure_session_id"].AsUUID();
            CircuitCode = map["circuit_code"].AsInteger();
            RegionHandle = map["region_handle"].AsULong();
            ChildAgent = map["child_agent"].AsBoolean();
            IP = new IPAddress(map["ip"].AsBinary());
            ClientVersion = map["client_version"].AsString();

            OSDMap attributesMap = (OSDMap)map["attributes"];
            Attributes = new Dictionary<Uri, OSD>(attributesMap.Count);
            foreach (KeyValuePair<string, OSD> entry in attributesMap)
                Attributes.Add(new Uri(entry.Key), entry.Value);

            OSDMap servicesMap = (OSDMap)map["services"];
            Services = new Dictionary<Uri, Dictionary<string, Uri>>(servicesMap.Count);
            foreach (KeyValuePair<string, OSD> serviceEntry in servicesMap)
            {
                OSDMap serviceMap = (OSDMap)serviceEntry.Value;
                Dictionary<string, Uri> service = new Dictionary<string, Uri>(serviceMap.Count);
                foreach (KeyValuePair<string, OSD> entry in serviceMap)
                    service.Add(entry.Key, entry.Value.AsUri());
                Services.Add(new Uri(serviceEntry.Key), service);
            }

            CallbackUri = map["callback_uri"].AsUri();
        }
    }

    public class EnableClientReplyMessage : IMessage
    {
        public bool Success;
        public string Message;
        public Uri SeedCapability;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap();
            map["success"] = OSD.FromBoolean(Success);
            map["message"] = OSD.FromString(Message);
            map["seed_capability"] = OSD.FromUri(SeedCapability);
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            Success = map["success"].AsBoolean();
            Message = map["message"].AsString();
            SeedCapability = map["seed_capability"].AsUri();
        }
    }

    public class EnableClientCompleteMessage : IMessage
    {
        public UUID AgentID;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(1);
            map["agent_id"] = OSD.FromUUID(AgentID);
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            AgentID = map["agent_id"].AsUUID();
        }
    }

    public class CloseAgentConnectionMessage : IMessage
    {
        public UUID AgentID;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap();
            map["agent_id"] = OSD.FromUUID(AgentID);
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            AgentID = map["agent_id"].AsUUID();
        }
    }

    public class NeighborUpdateMessage : IMessage
    {
        public RegionInfo[] Neighbors;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap();
            OSDArray array = new OSDArray(Neighbors.Length);
            for (int i = 0; i < Neighbors.Length; i++)
                array.Add(Neighbors[i].Serialize());
            map["neighbors"] = array;
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            OSDArray array = (OSDArray)map["neighbors"];
            Neighbors = new RegionInfo[array.Count];
            for (int i = 0; i < Neighbors.Length; i++)
            {
                RegionInfo region = new RegionInfo();
                region.Deserialize((OSDMap)array[i]);
                Neighbors[i] = region;
            }
        }
    }

    public class ChildAgentUpdateMessage : IMessage
    {
        public UUID AgentID;
        public UUID SessionID;
        public Vector3 Position;
        public Vector3 Velocity;
        public ulong RegionHandle;
        public Vector3 CameraPosition;
        public Vector3 CameraAtAxis;
        public Vector3 CameraLeftAxis;
        public Vector3 CameraUpAxis;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(8);
            map["agent_id"] = OSD.FromUUID(AgentID);
            map["session_id"] = OSD.FromUUID(SessionID);
            map["position"] = OSD.FromVector3(Position);
            map["velocity"] = OSD.FromVector3(Velocity);
            map["region_handle"] = OSD.FromULong(RegionHandle);
            map["cam_position"] = OSD.FromVector3(CameraPosition);
            map["cam_at_axis"] = OSD.FromVector3(CameraAtAxis);
            map["cam_left_axis"] = OSD.FromVector3(CameraLeftAxis);
            map["cam_up_axis"] = OSD.FromVector3(CameraUpAxis);
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            AgentID = map["agent_id"].AsUUID();
            SessionID = map["session_id"].AsUUID();
            Position = map["position"].AsVector3();
            Velocity = map["velocity"].AsVector3();
            RegionHandle = map["region_handle"].AsULong();
            CameraPosition = map["cam_position"].AsVector3();
            CameraAtAxis = map["cam_at_axis"].AsVector3();
            CameraLeftAxis = map["cam_left_axis"].AsVector3();
            CameraUpAxis = map["cam_up_axis"].AsVector3();
        }
    }

    public class PassObjectMessage : IMessage
    {
        public UUID ID;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(1);
            map["id"] = OSD.FromUUID(ID);
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            ID = map["id"].AsUUID();
        }
    }

    public class PassObjectReplyMessage : IMessage
    {
        public bool Success;
        public string Message;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(2);
            map["success"] = OSD.FromBoolean(Success);
            map["message"] = OSD.FromString(Message);
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            Success = map["success"].AsBoolean();
            Message = map["message"].AsString();
        }
    }

    public class FetchTerrainMessage : IMessage
    {
        public class FetchTerrainBlock
        {
            public int X;
            public int Y;
        }

        public FetchTerrainBlock[] Blocks;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(1);

            OSDArray array = new OSDArray(Blocks.Length);
            for (int i = 0; i < Blocks.Length; i++)
            {
                OSDMap blockMap = new OSDMap(2);
                blockMap["x"] = OSD.FromInteger(Blocks[i].X);
                blockMap["y"] = OSD.FromInteger(Blocks[i].Y);
                array.Add(blockMap);
            }

            map["blocks"] = array;

            return map;
        }

        public void Deserialize(OSDMap map)
        {
            OSDArray array = (OSDArray)map["blocks"];

            Blocks = new FetchTerrainBlock[array.Count];

            for (int i = 0; i < array.Count; i++)
            {
                OSDMap blockMap = (OSDMap)array[i];

                FetchTerrainBlock block = new FetchTerrainBlock();
                block.X = blockMap["x"].AsInteger();
                block.Y = blockMap["y"].AsInteger();
                Blocks[i] = block;
            }
        }
    }

    public class FetchTerrainReplyMessage : IMessage
    {
        public class FetchTerrainReplyBlock
        {
            public int X;
            public int Y;
            public byte[] Data;
        }

        public FetchTerrainReplyBlock[] Blocks;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(1);

            OSDArray array = new OSDArray(Blocks.Length);
            for (int i = 0; i < Blocks.Length; i++)
            {
                OSDMap blockMap = new OSDMap(2);
                blockMap["x"] = OSD.FromInteger(Blocks[i].X);
                blockMap["y"] = OSD.FromInteger(Blocks[i].Y);
                blockMap["data"] = OSD.FromBinary(Blocks[i].Data);
                array.Add(blockMap);
            }

            map["blocks"] = array;

            return map;
        }

        public void Deserialize(OSDMap map)
        {
            OSDArray array = (OSDArray)map["blocks"];
            Blocks = new FetchTerrainReplyBlock[array.Count];

            for (int i = 0; i < array.Count; i++)
            {
                OSDMap blockMap = (OSDMap)array[i];

                FetchTerrainReplyBlock block = new FetchTerrainReplyBlock();
                block.X = blockMap["x"].AsInteger();
                block.Y = blockMap["y"].AsInteger();
                block.Data = blockMap["data"].AsBinary();
                Blocks[i] = block;
            }
        }
    }

    #endregion Simulator Messages
}
