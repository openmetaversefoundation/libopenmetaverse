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

#ifndef _SL_NETWORK_
#define _SL_NETWORK_

#include "includes.h"
#include "Packet.h"
#include "SimConnection.h"

class LIBSECONDLIFE_CLASS_DECL Network
{
protected:
    boost::asio::demuxer _demuxer;
	SimConnection* _currentSim;
	std::vector<SimConnection*> _connections;
	std::list<Packet*> _inbox;
	ProtocolManager* _protocol;

	LLUUID _avatar_id;
	LLUUID _session_id;
	LLUUID _secure_session_id;

public:
	boost::mutex _inboxMutex;

	//Network();
	Network(ProtocolManager* protocol);
	virtual ~Network();

	int connectSim(boost::asio::ipv4::address ip, unsigned short port, U32 code, bool setCurrent = false);
    int sendPacket(boost::asio::ipv4::address ip, unsigned short port, Packet* packet);
	void receivePacket(const boost::asio::error& error, std::size_t length, char* receiveBuffer);
};

#endif //_SL_NETWORK_
