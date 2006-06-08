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

#ifndef _SL_SIMCONNECTION_
#define _SL_SIMCONNECTION_

#include "includes.h"

// Incoming datagram buffer
#define SL_BUFFER_SIZE 8192

class SimConnection
{
protected:
	std::string _name;
	unsigned int _code;
	boost::asio::ipv4::udp::endpoint _endpoint;
	boost::asio::datagram_socket* _socket;
	bool _running;
	char* _buffer;
	unsigned short _sequence;

public:
	SimConnection();
	SimConnection(boost::asio::ipv4::address ip, unsigned short port, unsigned int code);
	virtual ~SimConnection();
	
	std::string name() { return _name; };
	void name(std::string name) { _name = name; };
	
	unsigned int code() { return _code; };
	void code(unsigned int code) { _code = code; };

	boost::asio::ipv4::udp::endpoint endpoint() { return _endpoint; };
	void endpoint(boost::asio::ipv4::udp::endpoint endpoint) { _endpoint = endpoint; };

	boost::asio::datagram_socket* socket() { return _socket; };
	void socket(boost::asio::datagram_socket* socket) { _socket = socket; };

	boost::asio::ipv4::address ip() { return _endpoint.address(); };
	void ip(boost::asio::ipv4::address ip) { _endpoint.address(ip); };

	unsigned short port() { return _endpoint.port(); };
	void port(unsigned short port) { return _endpoint.port(port); };

	bool running() { return _running; };
	void running(bool running) { _running = running; };

	char* buffer() { return _buffer; };
	size_t bufferSize() { return SL_BUFFER_SIZE; };

	unsigned short sequence() { return _sequence++; };

	bool operator==(SimConnection &p);
	bool operator!=(SimConnection &p);
};

typedef boost::shared_ptr<SimConnection> SimConnectionPtr;

#endif //_SL_SIMCONNECTION_
