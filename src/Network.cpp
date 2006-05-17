#include "Network.h"

Network::Network(ProtocolManager* protocol)
{
	_protocol = protocol;
	_currentSim = NULL;
	_avatar_id = 0;
	_session_id = 0;
	_secure_session_id = 0;
}

Network::~Network()
{
	//FIXME: Close and free all the sockets
	//       Delete all remaining packets
}

void Network::receivePacket(const boost::asio::error& error, std::size_t length, char* receiveBuffer)
{
	// Debug
	printf("Received datagram, length: %u\n", length);
	for (size_t i = 0; i < length; i++) {
		printf("%02x ", receiveBuffer[i]);
	}
	printf("\n");

	// Build a Packet object and fill it with the incoming data
	Packet* packet = new Packet();
	packet->setRawData((byte*)receiveBuffer, length);

	// Push it on to the list
	boost::mutex::scoped_lock lock(_inboxMutex);
	_inbox.push_back(packet);
	lock.unlock();
}

int Network::connectSim(boost::asio::ipv4::address ip, unsigned short port, U32 code, bool setCurrent)
{
	// Check if we are already connected to this sim
	for (size_t i = 0; i < _connections.size(); i++) {
		if (ip == _connections[i]->ip() && port == _connections[i]->port()) {
			//FIXME: Log
			return -1;
		}
	}

	// Build a connection packet
	Packet* packet = new Packet(_protocol, 36);
	packet->setCommand("UseCircuitCode");
	packet->setField("CircuitCode", 1, "ID", &_session_id);
	packet->setField("CircuitCode", 1, "SessionID", &_session_id);
	packet->setField("CircuitCode", 1, "Code", &code);

	// Create the SimConnection
	SimConnection* sim = new SimConnection(ip, port, code);
	if (setCurrent) _currentSim = sim;

	boost::asio::datagram_socket* socket = new boost::asio::datagram_socket(_demuxer, boost::asio::ipv4::udp::endpoint(0));
	sim->socket(socket);

	// Send the packet
	try {
		size_t bytesSent = socket->send_to(boost::asio::buffer(packet->getRawDataPtr(), packet->getLength()), 0, sim->endpoint());
		// Debug
		printf("Sent %i byte connection packet\n", bytesSent);

		delete packet;
	} catch (boost::asio::error& e) {
		delete packet;

		// Debug
		std::cerr << e << std::endl;

		return -2;
	}
	
	// Start listening on this socket
	while (sim && sim->running()) {
		socket->async_receive(boost::asio::buffer(sim->buffer(), sim->bufferSize()), 0, 
							  boost::bind(&Network::receivePacket, this, boost::asio::placeholders::error, 
							  boost::asio::placeholders::bytes_transferred, sim->buffer()));
		_demuxer.run();
		_demuxer.reset();
	}

	// Debug
	printf("Closed connection to %u\n", (unsigned int)sim);

	return 0;
}

int Network::sendPacket(boost::asio::ipv4::address ip, unsigned short port, Packet* packet)
{
	//boost::asio::ipv4::udp::endpoint sim;
	bool found = false;
	size_t i;

	// Check if we are connected to this sim
	for (i = 0; i < _connections.size(); i++) {
		if (ip == _connections[i]->ip() && port == _connections[i]->port()) {
			//sim = _connections[i]->endpoint();
			found = true;
			break;
		}
	}

	if (!found) {
		//FIXME: Log
		return -1;
	}

	try {
		size_t bytesSent = _connections[i]->socket()->send_to(boost::asio::buffer(packet->getRawDataPtr(), packet->getLength()), 0, _connections[i]->endpoint());

		// Debug
		printf("Sent %i bytes\n", bytesSent);
	} catch (boost::asio::error& e) {
		// Debug
		std::cerr << e << std::endl;

		return -2;
	}

	return 0;
}
