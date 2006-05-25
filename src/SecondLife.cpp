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
	bool returnValue;
	std::list<Packet*>* inbox = _network->inbox();
	
	// When we get to stream handling, this function will build data stream
	// classes and append new data, for sounds/images/animations/etc

	// tick() will process all of the outstanding packets, building classes and
	// firing callbacks as it goes
	if (_network) {
		while (inbox->size() > 0) {
			packet = inbox->front();
			inbox->pop_front();
			std::string command = packet->command();
			
			std::map<std::string, callback>::iterator handler = _callbacks.find(command);
			
			if (handler == _callbacks.end()) {
				handler = _callbacks.find("Default");
				
				if (handler != _callbacks.end()) {
					returnValue = (handler->second)(command, packet);
				}
			} else {
				returnValue = (handler->second)(command, packet);
			}
			
			if (returnValue) {
				//FIXME: What is the purpose of the return value?
			}
		}
	}
	
	// Sleep for 1000 nanoseconds
	boost::xtime xt;
	boost::xtime_get(&xt, boost::TIME_UTC);
	xt.nsec += 1000;
	boost::thread::sleep(xt);
}
