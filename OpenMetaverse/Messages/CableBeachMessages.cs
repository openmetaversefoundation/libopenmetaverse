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
    #region Identity Messages

    public class RequestCapabilitiesMessage : IMessage
    {
        public Uri Identity;
        public Uri[] Capabilities;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(1);
            map["identity"] = OSD.FromUri(Identity);

            OSDArray array = new OSDArray(Capabilities.Length);
            for (int i = 0; i < Capabilities.Length; i++)
                array.Add(OSD.FromUri(Capabilities[i]));
            map["capabilities"] = array;

            return map;
        }

        public void Deserialize(OSDMap map)
        {
            Identity = map["identity"].AsUri();

            OSDArray array = (OSDArray)map["capabilities"];
            Capabilities = new Uri[array.Count];
            for (int i = 0; i < array.Count; i++)
                Capabilities[i] = array[i].AsUri();
        }
    }

    public class RequestCapabilitiesReplyMessage : IMessage
    {
        public Dictionary<Uri, Uri> Capabilities;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(1);
            OSDMap caps = new OSDMap(Capabilities.Count);
            foreach (KeyValuePair<Uri, Uri> entry in Capabilities)
                caps.Add(entry.Key.ToString(), OSD.FromUri(entry.Value));
            map["capabilities"] = caps;
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            OSDMap caps = (OSDMap)map["capabilities"];
            Capabilities = new Dictionary<Uri, Uri>(caps.Count);
            foreach (KeyValuePair<string, OSD> entry in caps)
                Capabilities.Add(new Uri(entry.Key), entry.Value.AsUri());
        }
    }

    #endregion Identity Messages

    #region Inventory Messages

    public class CreateInventoryMessage : IMessage
    {
        public Uri Identity;
        public UUID AccessToken;
        public string Name;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(3);
            map["identity"] = OSD.FromUri(Identity);
            map["access_token"] = OSD.FromUUID(AccessToken);
            map["name"] = OSD.FromString(Name);
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            Identity = map["identity"].AsUri();
            AccessToken = map["access_token"].AsUUID();
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

    #endregion Inventory Messages

    #region Region Messages

    public class EnableClientMessage : IMessage
    {
        public class Service
        {
            public Uri Type;
            public Uri Uri;
            public UUID AccessToken;
        }

        public UUID AgentID;
        public UUID SessionID;
        public UUID SecureSessionID;
        public int CircuitCode;
        public ulong RegionHandle;
        public bool ChildAgent;
        public IPAddress IP;
        public string ClientVersion;
        public Dictionary<Uri, OSD> Attributes;
        public Dictionary<Uri, Service> Services;
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
            foreach (KeyValuePair<Uri, Service> entry in Services)
            {
                OSDMap service = new OSDMap(3);
                service["type"] = OSD.FromUri(entry.Value.Type);
                service["uri"] = OSD.FromUri(entry.Value.Uri);
                service["access_token"] = OSD.FromUUID(entry.Value.AccessToken);
                services.Add(entry.Key.ToString(), service);
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
            Services = new Dictionary<Uri, Service>(servicesMap.Count);
            foreach (KeyValuePair<string, OSD> entry in servicesMap)
            {
                OSDMap serviceMap = (OSDMap)entry.Value;
                Service service = new Service();
                service.Type = serviceMap["type"].AsUri();
                service.Uri = serviceMap["uri"].AsUri();
                service.AccessToken = serviceMap["access_token"].AsUUID();
                Services.Add(new Uri(entry.Key), service);
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
            OSDMap map = new OSDMap(5);
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

    #endregion Region Messages
}
