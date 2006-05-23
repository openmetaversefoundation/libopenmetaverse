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

#ifndef _SL_SECONDLIFE_
#define _SL_SECONDLIFE_

#include "includes.h"
#include "ProtocolManager.h"
#include "SimConnection.h"
#include "Network.h"

// Model for the callbacks:
//  bool functionname(std::string command, Packet*)
typedef boost::function2<bool, std::string, Packet*> callback;

class LIBSECONDLIFE_CLASS_DECL SecondLife
{
protected:
	
public:
	ProtocolManager* _protocol;
	Network* _network;
	bool _running;
	// Later on we'll want internal callbacks and client callbacks, so the client doesn't overwrite
	// for example the stream building functions
	std::map<std::string, callback> _callbacks;

//public:
	SecondLife();
	virtual ~SecondLife();

	void registerCallback(std::string command, callback handler) { _callbacks[command] = handler; };

	int loadKeywords(std::string filename) { return _protocol->loadKeywords(filename); };
	int decryptCommFile(std::string source, std::string destination) { return _protocol->decryptCommFile(source, destination); };
	int buildProtocolMap(std::string filename) { return _protocol->buildProtocolMap(filename); };

	void login(std::string firstName, std::string lastName, std::string password, std::string mac, std::string platform,
			   std::string viewerDigest, std::string userAgent, std::string author, loginCallback handler,
			   std::string url = "https://login.agni.lindenlab.com/cgi-bin/login.cgi")
	{ _network->login(firstName, lastName, password, mac, platform, viewerDigest, userAgent, author, handler, url); };
	void connectSim(boost::asio::ipv4::address ip, unsigned short port, U32 code, bool setCurrent = false);

	LLUUID agent_id() { return _network->agent_id(); };
	void agent_id(LLUUID agent_id) { _network->agent_id(agent_id); };
	LLUUID session_id() { return _network->session_id(); };
	void session_id(LLUUID session_id) { _network->session_id(session_id); };
	LLUUID secure_session_id() { return _network->secure_session_id(); };
	void secure_session_id(LLUUID secure_session_id) { _network->secure_session_id(secure_session_id); };

	void tick();
};

#endif //_SL_SECONDLIFE_
