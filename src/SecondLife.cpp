#include "SecondLife.h"

SecondLife::SecondLife()
{
	_protocol = new ProtocolManager();
	_network = new Network(_protocol, this);
	_running = true;
}

SecondLife::~SecondLife()
{
#ifdef DEBUG
	std::cout << "SecondLife::~SecondLife() destructor called" << std::endl;
#endif
	delete _protocol;
	delete _network;
}

void SecondLife::tick()
{
	PacketPtr packet;
	std::list<PacketPtr>* inbox = _network->inbox();

	// When we get to stream handling, this function will build data stream
	// classes and append new data, for sounds/images/animations/etc

	// tick() will process all of the outstanding packets, building classes and
	// firing callbacks as it goes
	if (_network) {
		while (inbox->size() > 0) {
			boost::mutex::scoped_lock lock(_network->inboxMutex);
			packet = inbox->front();
			inbox->pop_front();
			lock.unlock();
			
			std::string command = packet->command();
			
			std::map<std::string, callback>::iterator handler = _callbacks.find(command);
			
			if (handler == _callbacks.end()) {
				handler = _callbacks.find("Default");
				
				if (handler != _callbacks.end()) {
					(handler->second)(command, packet);
				}
			} else {
				(handler->second)(command, packet);
			}
		}
	}

	// Sleep for 1000 nanoseconds
	boost::xtime xt;
	boost::xtime_get(&xt, boost::TIME_UTC);
	xt.nsec += 1000;
	boost::thread::sleep(xt);
}
