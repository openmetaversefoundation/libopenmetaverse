#ifndef _SL_SIMCONNECTION_
#define _SL_SIMCONNECTION_

#include "includes.h"

class SimConnection
{
protected:
	std::string _name;
	U32 _code;
	boost::asio::ipv4::udp::endpoint _endpoint;
	boost::asio::datagram_socket* _socket;
	bool _running;
	char* _buffer;

public:
	SimConnection();
	SimConnection(boost::asio::ipv4::address ip, unsigned short port, U32 code);
	virtual ~SimConnection();

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

	bool operator==(SimConnection &p);
	bool operator!=(SimConnection &p);
};

#endif //_SL_SIMCONNECTION_
