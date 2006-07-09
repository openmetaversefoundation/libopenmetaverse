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
	public class Network
	{
		public static Packet PacketAck(ProtocolManager protocol, ArrayList acks)
		{
			Hashtable blocks = new Hashtable();
			Hashtable fields;

			foreach (uint ack in acks)
			{
				fields = new Hashtable();
				fields["ID"] = ack;
				blocks[fields] = "Packets";
			}

			return PacketBuilder.BuildPacket("PacketAck", protocol, blocks, 0);
		}

		public static Packet UseCircuitCode(ProtocolManager protocol, LLUUID agentID, LLUUID sessionID,
			uint circuitCode)
		{
			Hashtable blocks = new Hashtable();
			Hashtable fields = new Hashtable();
			fields["ID"] = agentID;
			fields["SessionID"] = sessionID;
			fields["Code"] = circuitCode;
			blocks[fields] = "CircuitCode";

			return PacketBuilder.BuildPacket("UseCircuitCode", protocol, blocks, Helpers.MSG_RELIABLE + Helpers.MSG_ZEROCODED);
		}

		public static Packet CompletePingCheck(ProtocolManager protocol, byte pingID)
		{
			Hashtable blocks = new Hashtable();
			Hashtable fields = new Hashtable();
			fields["PingID"] = pingID;
			blocks[fields] = "PingID";

			return PacketBuilder.BuildPacket("CompletePingCheck", protocol, blocks, Helpers.MSG_ZEROCODED);
		}

		public static Packet LogoutRequest(ProtocolManager protocol, LLUUID agentID, LLUUID sessionID)
		{
			Hashtable blocks = new Hashtable();
			Hashtable fields = new Hashtable();

			fields["AgentID"] = agentID;
			fields["SessionID"] = sessionID;
			blocks[fields] = "AgentData";

			return PacketBuilder.BuildPacket("LogoutRequest", protocol, blocks, Helpers.MSG_RELIABLE + Helpers.MSG_ZEROCODED);
		}
	}
}
