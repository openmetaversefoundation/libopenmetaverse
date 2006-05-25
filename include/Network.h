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

typedef struct loginParameters {
	std::string session_id;
	std::string secure_session_id;
	std::string start_location;
	std::string first_name;
	std::string last_name;
	int region_x;
	int region_y;
	std::string home;
	std::string reason;
	std::string message;
	int circuit_code;
	int sim_port;
	std::string sim_ip;
	std::string look_at;
	std::string agent_id;
	int seconds_since_epoch;

	loginParameters() {
		region_x = 0;
		region_y = 0;
		circuit_code = 0;
		sim_port = 0;
		seconds_since_epoch = 0;
	}
} loginParameters;

typedef boost::function1<void, loginParameters> loginCallback;

class SecondLife;

class LIBSECONDLIFE_CLASS_DECL Network
{
protected:
    boost::asio::demuxer _demuxer;
	std::vector<SimConnection*> _connections;
	std::list<Packet*> _inbox;
	ProtocolManager* _protocol;
	SecondLife* _secondlife;

	LLUUID _agent_id;
	LLUUID _session_id;
	LLUUID _secure_session_id;

public:
	// Debug
	SimConnection* _currentSim;
	boost::mutex _inboxMutex;

	Network(ProtocolManager* protocol, SecondLife* secondlife);
	virtual ~Network();

	void login(std::string firstName, std::string lastName, std::string password, std::string mac,
			   std::string platform, std::string viewerDigest, std::string userAgent, std::string author,
			   loginCallback handler, std::string url);

	void listen(SimConnection* sim);
	int connectSim(boost::asio::ipv4::address ip, unsigned short port, U32 code, bool setCurrent = false);
    int sendPacket(boost::asio::ipv4::address ip, unsigned short port, Packet* packet);
	int sendPacket(Packet* packet);
	void receivePacket(const boost::asio::error& error, std::size_t length, char* receiveBuffer);

	std::list<Packet*>* inbox() { return &_inbox; };

	LLUUID agent_id() { return _agent_id; };
	void agent_id(LLUUID agent_id) { _agent_id = agent_id; };
	LLUUID session_id() { return _session_id; };
	void session_id(LLUUID session_id) { _session_id = session_id; };
	LLUUID secure_session_id() { return _secure_session_id; };
	void secure_session_id(LLUUID secure_session_id) { _secure_session_id = secure_session_id; };
};

#endif //_SL_NETWORK_
