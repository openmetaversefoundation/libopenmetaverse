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
	public class Estate
	{
		public static Packet EstateOwnerMessage(ProtocolManager protocol, LLUUID agentID, LLUUID sessionID, string method, LLUUID invoice, Hashtable parameterlists) 
		{
			Hashtable blocks = new Hashtable();
			Hashtable fields = new Hashtable();

			fields["AgentID"] = agentID;
			fields["SessionID"] = sessionID;
			blocks[fields] = "AgentData";

			fields = new Hashtable();
			fields["Method"] = method;
			fields["Invoice"] = invoice;
			blocks[fields] = "MethodData";

			foreach(Hashtable field in parameterlists.Keys) 
			{
				blocks[field] = parameterlists[field];
			}

			Packet packet = PacketBuilder.BuildPacket("EstateOwnerMessage", protocol, blocks, Helpers.MSG_RELIABLE + Helpers.MSG_ZEROCODED);
			return packet;
		}
		public static Packet EstateKick(ProtocolManager protocol, LLUUID agentID, LLUUID sessionID, LLUUID targetID) 
		{
			Hashtable plist = new Hashtable();
			Hashtable fields = new Hashtable();

			fields["Parameter"] = agentID.ToStringHyphenated();
			plist[fields] = "ParamList";
			fields = new Hashtable();
			fields["Parameter"] = targetID.ToStringHyphenated();
			plist[fields] = "ParamList";

			return Estate.EstateOwnerMessage(protocol,agentID,sessionID,"kick",new LLUUID(true),plist);
		}

		public static Packet EstateBan(ProtocolManager protocol, LLUUID agentID, LLUUID sessionID, LLUUID targetID) 
		{
			Hashtable plist = new Hashtable();
			Hashtable fields = new Hashtable();

			fields["Parameter"] = targetID.ToStringHyphenated();
			plist[fields] = "ParamList";

			fields = new Hashtable();
			fields["Parameter"] = "65";
			plist[fields] = "ParamList";

			fields = new Hashtable();
			fields["Parameter"] = agentID.ToStringHyphenated();
			plist[fields] = "ParamList";

			return Estate.EstateOwnerMessage(protocol,agentID,sessionID,"estateaccessdelta",new LLUUID(true),plist);
		}

		public static Packet EstateUnBan(ProtocolManager protocol, LLUUID agentID, LLUUID sessionID, LLUUID targetID) 
		{
			Hashtable plist = new Hashtable();
			Hashtable fields = new Hashtable();

			fields["Parameter"] = targetID.ToStringHyphenated();
			plist[fields] = "ParamList";

			fields = new Hashtable();
			fields["Parameter"] = "129";
			plist[fields] = "ParamList";

			fields = new Hashtable();
			fields["Parameter"] = agentID.ToStringHyphenated();
			plist[fields] = "ParamList";

			return Estate.EstateOwnerMessage(protocol,agentID,sessionID,"estateaccessdelta",new LLUUID(true),plist);
		}

		public static Packet EstateTeleportUser(ProtocolManager protocol, LLUUID agentID, LLUUID sessionID, LLUUID targetID) 
		{
			Hashtable plist = new Hashtable();
			Hashtable fields = new Hashtable();

			fields["Parameter"] = targetID.ToStringHyphenated();
			plist[fields] = "ParamList";
			fields = new Hashtable();
			fields["Parameter"] = agentID.ToStringHyphenated();
			plist[fields] = "ParamList";

			return Estate.EstateOwnerMessage(protocol,agentID,sessionID,"teleporthomeuser",new LLUUID(true),plist);
		}
	}
}