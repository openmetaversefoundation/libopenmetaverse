/*
 * Copyright (c) 2007-2009, openmetaverse.org
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
using OpenMetaverse.StructuredData;

namespace OpenMetaverse.Messages.Simian
{
    public class EnableClientMessage
    {
        public UUID AgentID;
        public UUID SessionID;
        public UUID SecureSessionID;
        public int CircuitCode;
        public string FirstName;
        public string LastName;
        public ulong RegionHandle;
        public Uri CallbackUri;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(7);
            map["agent_id"] = OSD.FromUUID(AgentID);
            map["session_id"] = OSD.FromUUID(SessionID);
            map["secure_session_id"] = OSD.FromUUID(SecureSessionID);
            map["circuit_code"] = OSD.FromInteger(CircuitCode);
            map["first_name"] = OSD.FromString(FirstName);
            map["last_name"] = OSD.FromString(LastName);
            map["region_handle"] = OSD.FromULong(RegionHandle);
            map["callback_uri"] = OSD.FromUri(CallbackUri);
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            AgentID = map["agent_id"].AsUUID();
            SessionID = map["session_id"].AsUUID();
            SecureSessionID = map["secure_session_id"].AsUUID();
            CircuitCode = map["circuit_code"].AsInteger();
            FirstName = map["first_name"].AsString();
            LastName = map["last_name"].AsString();
            RegionHandle = map["region_handle"].AsULong();
            CallbackUri = map["callback_uri"].AsUri();
        }
    }

    public class EnableClientReplyMessage
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

    public class EnableClientCompleteMessage
    {
        public UUID AgentID;
        public Uri SeedCapability;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap(2);
            map["agent_id"] = OSD.FromUUID(AgentID);
            map["seed_capability"] = OSD.FromUri(SeedCapability);
            return map;
        }

        public void Deserialize(OSDMap map)
        {
            AgentID = map["agent_id"].AsUUID();
            SeedCapability = map["seed_capability"].AsUri();
        }
    }

    public class ChildAgentUpdateMessage
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

    public class PassObjectMessage
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

    public class PassObjectReplyMessage
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

    public class FetchTerrainMessage
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

    public class FetchTerrainReplyMessage
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
}
