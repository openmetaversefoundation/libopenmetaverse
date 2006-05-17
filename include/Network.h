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
