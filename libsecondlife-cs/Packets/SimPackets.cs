/*
 * Copyright (c) 2006, Second Life Reverse Engineering Team
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the Second Life Reverse Engineering Team nor the names
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
using System.Collections;

namespace libsecondlife.Packets
{
       public class Sim
       {
               public static Packet MapNameRequest(ProtocolManager protocol, LLUUID agentID, uint flags, uint estateID, bool godlike, string name)
               {
                       Hashtable blocks = new Hashtable();
                       Hashtable fields = new Hashtable();

                       fields["AgentID"] = agentID;
                       fields["Flags"] = flags;
                       fields["EstateID"] = estateID;
                       fields["Godlike"] = godlike;
                       blocks[fields] = "AgentData";

                       fields = new Hashtable();
                       fields["Name"] = name;
                       blocks[fields] = "NameData";

                       return PacketBuilder.BuildPacket("MapNameRequest", protocol, blocks, Helpers.MSG_RELIABLE + Helpers.MSG_ZEROCODED);
               }

               public static Packet MapBlockRequest(ProtocolManager protocol, LLUUID agentID, uint flags, uint estateID, bool godlike,
                       ushort minX, ushort maxX, ushort minY, ushort maxY)
               {
                       Hashtable blocks = new Hashtable();
                       Hashtable fields = new Hashtable();

                       fields["AgentID"] = agentID;
                       fields["Flags"] = flags;
                       fields["EstateID"] = estateID;
                       fields["Godlike"] = godlike;
                       blocks[fields] = "AgentData";

                       fields = new Hashtable();
                       fields["MinX"] = minX;
                       fields["MaxX"] = maxX;
                       fields["MinY"] = minY;
                       fields["MaxY"] = maxY;
                       blocks[fields] = "PositionData";

                       return PacketBuilder.BuildPacket("MapBlockRequest", protocol, blocks, Helpers.MSG_RELIABLE + Helpers.MSG_ZEROCODED);
               }

               public static Packet CompleteAgentMovement(ProtocolManager protocol, LLUUID agentID, LLUUID sessionID,
                       uint circuitCode)
               {
                       Hashtable blocks = new Hashtable();
                       Hashtable fields = new Hashtable();
                       fields["AgentID"] = agentID;
                       fields["SessionID"] = sessionID;
                       fields["CircuitCode"] = circuitCode;
                       blocks[fields] = "AgentData";

                       return PacketBuilder.BuildPacket("CompleteAgentMovement", protocol, blocks, Helpers.MSG_RELIABLE + Helpers.MSG_ZEROCODED);
               }

               public static Packet AgentUpdate(ProtocolManager protocol, LLUUID agentID, float drawDistance,
                       LLVector3 cameraCenter, byte flags, byte state)
               {
                   LLVector3 cameraAtAxis = new LLVector3(0.0F, 1.0F, 0.0F); // Looking straight ahead, north
                   LLVector3 cameraLeftAxis = new LLVector3(0.0F, 0.0F, 0.0F); //FIXME
                   LLVector3 cameraUpAxis = new LLVector3(0.0F, 0.0F, 0.0F); //FIXME
                   LLQuaternion headRotation = new LLQuaternion(0.0F, 0.0F, 0.0F, 0.0F);

                   return AgentUpdate(protocol, agentID, drawDistance, cameraCenter, 
                       cameraAtAxis, cameraLeftAxis, cameraUpAxis, headRotation, flags, state);
               }

		   public static Packet AgentUpdate(ProtocolManager protocol, LLUUID agentID, float drawDistance,
			   LLVector3 cameraCenter, LLVector3 cameraAtAxis, LLVector3 cameraLeftAxis, LLVector3 cameraUpAxis, 
               LLQuaternion headRotation, byte flags, byte state)
		   {
			   Hashtable blocks = new Hashtable();
			   Hashtable fields = new Hashtable();

			   fields["ID"] = agentID;
			   fields["ControlFlags"] = (uint)0;
			   fields["CameraAtAxis"] = cameraAtAxis;
			   fields["Far"] = drawDistance;
			   fields["CameraCenter"] = cameraCenter;
			   fields["CameraLeftAxis"] = cameraLeftAxis;
			   fields["HeadRotation"] = headRotation;
			   fields["CameraUpAxis"] = cameraUpAxis;
			   fields["BodyRotation"] = new LLQuaternion(0.0F, 0.0F, 0.0F, 0.0F);
			   fields["Flags"] = flags;
			   fields["State"] = state;
			   blocks[fields] = "AgentData";

			   return PacketBuilder.BuildPacket("AgentUpdate", protocol, blocks, (byte)0);
		   }


               public static Packet DirFindQuery(ProtocolManager protocol, string query, int queryStart, LLUUID queryID,
                       LLUUID agentID, LLUUID sessionID)
               {
                       Hashtable blocks = new Hashtable();
                       Hashtable fields = new Hashtable();

                       fields["QueryID"] = queryID;
                       fields["QueryFlags"] = (uint)1;
                       fields["QueryStart"] = queryStart;
                       fields["QueryText"] = query;
                       blocks[fields] = "QueryData";

                       fields = new Hashtable();
                       fields["AgentID"] = agentID;
                       fields["SessionID"] = sessionID;
                       blocks[fields] = "AgentData";

                       return PacketBuilder.BuildPacket("DirFindQuery", protocol, blocks, Helpers.MSG_RELIABLE);
               }

               public static Packet DirLandQuery(ProtocolManager protocol, bool reservedNewbie, bool forSale, LLUUID queryID,
                       bool auction, uint queryFlags, LLUUID agentID, LLUUID sessionID)
               {
                       Hashtable blocks = new Hashtable();
                       Hashtable fields = new Hashtable();

                       fields["ReservedNewbie"] = reservedNewbie;
                       fields["ForSale"] = forSale;
                       fields["QueryID"] = queryID;
                       fields["Auction"] = auction;
                       fields["QueryFlags"] = queryFlags;
                       blocks[fields] = "QueryData";

                       fields = new Hashtable();
                       fields["AgentID"] = agentID;
                       fields["SessionID"] = sessionID;
                       blocks[fields] = "AgentData";

                       return PacketBuilder.BuildPacket("DirLandQuery", protocol, blocks, Helpers.MSG_RELIABLE);
               }
       }
}
