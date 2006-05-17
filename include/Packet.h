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

#ifndef _SL_PACKET_
#define _SL_PACKET_

#include "includes.h"
#include "ProtocolManager.h"

// Higher value will mean less realloc()s, more wasted memory. Lower value is 
// vice versa.
#define DEFAULT_PACKET_SIZE 128

class LIBSECONDLIFE_CLASS_DECL Packet
{
protected:
	packetDiagram* _layout;
	byte* _buffer;
	size_t _length;
	boost::asio::ipv4::udp::endpoint _remoteHost;
	ProtocolManager* _protocol;

public:
	Packet();
	Packet(ProtocolManager* protocol, size_t length = 0);
	virtual ~Packet();

	bool setCommand(std::string command);
	ll::llType getFieldType(std::string block, std::string field);
	void* getField(std::string block, size_t blockNumber, std::string field);
	int setField(std::string block, size_t blockNumber, std::string field, void* value);

	size_t getLength() { return _length; };
	int getRawData(byte* buffer, size_t length);
	byte* getRawDataPtr();
	void setRawData(byte* buffer, size_t length);

	boost::asio::ipv4::udp::endpoint getRemoteHost();
	void setRemoteHost(boost::asio::ipv4::udp::endpoint remoteHost);
};

#endif //_SL_PACKET_
