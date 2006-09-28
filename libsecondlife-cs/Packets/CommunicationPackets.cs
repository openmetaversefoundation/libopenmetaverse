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
	public class Communication
	{
		public static Packet UUIDNameRequest(ProtocolManager protocol, LLUUID ID) 
		{
			Hashtable blocks = new Hashtable();
			Hashtable fields = new Hashtable();

			fields["ID"] = ID;
			blocks[fields] = "UUIDNameBlock";

			Packet packet = PacketBuilder.BuildPacket("UUIDNameRequest", protocol, blocks, Helpers.MSG_RELIABLE);
			return packet;
		}

		public static Packet ImprovedInstantMessage(ProtocolManager protocol, LLUUID targetAgentID, 
			LLUUID myAgentID, uint parentEstateID, LLUUID regionID, LLVector3 position, byte offline, 
			byte dialog, LLUUID id, uint timestamp, string myAgentName, string message, 
			byte[] binaryBucket, LLUUID mySessionID) 
		{
			Hashtable blocks = new Hashtable();
			Hashtable fields = new Hashtable();

			fields["FromAgentID"] = myAgentID;
			fields["ToAgentID"] = targetAgentID;
			fields["ParentEstateID"] = parentEstateID;
			fields["RegionID"] = regionID;
			fields["Position"] = position;
			fields["Offline"] = offline;
			fields["Dialog"] = dialog;
			fields["ID"] = id;
			fields["Timestamp"] = timestamp;
			fields["FromAgentName"] = myAgentName;
			fields["Message"] = message;
			fields["BinaryBucket"] = binaryBucket;
			blocks[fields] = "MessageBlock";

			// Agent Data Block
			Hashtable agentData = new Hashtable();
			agentData["AgentID"] = myAgentID;
			agentData["SessionID"] = mySessionID;
			blocks[agentData] = "AgentData";


			return PacketBuilder.BuildPacket("ImprovedInstantMessage", protocol, blocks, Helpers.MSG_RELIABLE);
		}

		public static Packet ChatFromViewer(ProtocolManager protocol, LLUUID myAgentID, LLUUID mySessionID, string message,
			byte type, int channel, byte command, LLUUID commandID, float radius, LLVector3 position) 
		{
			Hashtable blocks = new Hashtable();
			Hashtable agentData = new Hashtable();
			Hashtable chatData = new Hashtable();
			Hashtable conversationData = new Hashtable();

			// Agent Data Block
			agentData["AgentID"] = myAgentID;
			agentData["SessionID"] = mySessionID;
			blocks[agentData] = "AgentData";

			// Chat Data Block
			chatData["Message"] = message;
			chatData["Type"] = type;
			chatData["Channel"] = channel;
			blocks[chatData] = "ChatData";

			// Conversation Data Block
			conversationData["Command"] = command;
			conversationData["CommandID"] = commandID;
			conversationData["Radius"] = radius;
			conversationData["Position"] = position;
			blocks[conversationData] = "ConversationData";

			return PacketBuilder.BuildPacket("ChatFromViewer", protocol, blocks, Helpers.MSG_RELIABLE);
		}
	}
}
