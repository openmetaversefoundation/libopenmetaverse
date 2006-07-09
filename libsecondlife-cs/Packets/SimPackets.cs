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
			LLVector3 cameraCenter)
		{
			Hashtable blocks = new Hashtable();
			Hashtable fields = new Hashtable();

			fields["ID"] = agentID;
			fields["ControlFlags"] = (uint)0;
			fields["CameraAtAxis"] = new LLVector3(0.0F, 1.0F, 0.0F); // Looking straight ahead, north
			fields["Far"] = drawDistance;
			fields["CameraCenter"] = cameraCenter;
			fields["CameraLeftAxis"] = new LLVector3(0.0F, 0.0F, 0.0F); //FIXME
			fields["HeadRotation"] = new LLQuaternion(0.0F, 0.0F, 0.0F, 0.0F);
			fields["CameraUpAxis"] = new LLVector3(0.0F, 0.0F, 0.0F); //FIXME
			fields["BodyRotation"] = new LLQuaternion(0.0F, 0.0F, 0.0F, 0.0F);
			fields["Flags"] = (byte)221; // Why 221?
			fields["State"] = (byte)221; // Why 221?
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
