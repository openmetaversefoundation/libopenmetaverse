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
