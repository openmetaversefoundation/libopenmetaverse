/*
 * Copyright (c) 2006, Second Life Reverse Engineering Team
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without 
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Redistributions in binary form must reproduce the above copyright notice, 
 *   this list of conditions and the following disclaimer in the documentation 
 *   and/or other materials provided with the distribution.
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

#ifndef _SL_BUILDER_
#define _SL_BUILDER_

#include "includes.h"
#include "Fields.h"
#include "Packet.h"

PacketPtr PacketAck(ProtocolManager* protocol, std::vector<unsigned int> IDList);
PacketPtr PacketAck(ProtocolManager* protocol, unsigned int ID);
PacketPtr UseCircuitCode(ProtocolManager* protocol, SimpleLLUUID agentID, SimpleLLUUID sessionID, unsigned int code);
PacketPtr CompleteAgentMovement(ProtocolManager* protocol, SimpleLLUUID AgentID, SimpleLLUUID SessionID, unsigned int CircuitCode);
PacketPtr RegionHandshakeReply(ProtocolManager* protocol, unsigned int Flags);
PacketPtr CompletePingCheck(ProtocolManager* protocol, byte PingID);
PacketPtr DirLandQuery(ProtocolManager* protocol, bool ReservedNewbie, bool ForSale, SimpleLLUUID QueryID, bool Auction, 
					   unsigned int QueryFlags, SimpleLLUUID AgentID, SimpleLLUUID SessionID);

#endif //_SL_BUILDER_
