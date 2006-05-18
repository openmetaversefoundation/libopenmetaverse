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
#include "Decoder.h"

// Model for the callbacks:
//  bool functionname(std::string command, Packet*)
typedef boost::function2<bool, std::string, Packet*> callback;

class LIBSECONDLIFE_CLASS_DECL SecondLife
{
//protected:
public:
	ProtocolManager* _protocol;
	Network* _network;
	Decoder* _decoder;
	bool _running;
	// Later on we'll want internal callbacks and client callbacks, so the client doesn't overwrite
	// for example the stream building functions
	std::map<std::string, callback> _callbacks;

//public:
	SecondLife();
	virtual ~SecondLife();

	void registerCallback(std::string command, callback handler) { _callbacks[command] = handler; };
	
	// Pass-through functions to make life easier on the client
	int loadKeywords(std::string filename) { return _protocol->loadKeywords(filename); };
	int decryptCommFile(std::string source, std::string destination) { return _protocol->decryptCommFile(source, destination); };
	int buildProtocolMap(std::string filename) { return _protocol->buildProtocolMap(filename); };

	void connectSim(boost::asio::ipv4::address ip, unsigned short port, U32 code, bool setCurrent = false);
	void tick();
};

#endif //_SL_SECONDLIFE_
