#include "SecondLife.h"

SecondLife::SecondLife()
{
	_protocol = new ProtocolManager();
	_network = new Network(_protocol, this);
	_running = true;
}

SecondLife::~SecondLife()
{
	delete _protocol;
	delete _network;
}

void SecondLife::connectSim(boost::asio::ipv4::address ip, unsigned short port, U32 code, bool setCurrent)
{
	_network->connectSim(ip, port, code, setCurrent);
}

void SecondLife::tick()
{
	Packet* packet;
	// When we get to stream handling, this function will build data stream 
	// classes and append new data, for sounds/images/animations/etc

	// tick() will process all of the outstanding packets, building classes and 
	// firing callbacks as it goes
	if (_network) {
		while (_network->inbox().size() > 0) {
			packet = _network->inbox().front();
			_network->inbox().pop_front();
			std::string command = packet->command();
			
			callback handler = _callbacks[command];
			bool returnValue = handler(command, packet);
			
			if (returnValue) {
				;
			}
		}
	}
}
