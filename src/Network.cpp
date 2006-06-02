#include <curl/curl.h>

#include "Network.h"
#include "PacketBuilder.h"
#include "md5.h"

Network::Network(ProtocolManager* protocol, SecondLife* secondlife)
{
	_protocol = protocol;
	_secondlife = secondlife;
	_agent_id = 0;
	_session_id = 0;
	_secure_session_id = 0;
}

Network::~Network()
{
#ifdef DEBUG
	std::cout << "Network::~Network() destructor called" << std::endl;
#endif
}

size_t loginReply(void* buffer, size_t size, size_t nmemb, void* userp)
{
	loginParameters login;
	Network* network = (Network*)userp;
	loginCallback callback = network->callback;
	char* reply = (char*)buffer;
	std::string msg;
	int realsize = size * nmemb;
	
	if (!reply) {
		login.reason = "libsecondlife";
		login.message = "There was an error connecting to the login server, check the log file for details";
		callback(login);
		return realsize;
	}

	msg = rpcGetString(reply, "<name>reason</name>");
	if (msg.length()) {
		login.reason = msg;
		login.message = rpcGetString(reply, "<name>message</name>");
	} else {
		msg = rpcGetString(reply, "login</name><value><string>true");
		if (msg.length()) {
			// Put all of the login parameters in to our struct
			login.session_id = rpcGetString(reply, "<name>session_id</name>");
			login.secure_session_id = rpcGetString(reply, "<name>secure_session_id</name>");
			login.start_location = rpcGetString(reply, "<name>start_location</name>");
			login.first_name = rpcGetString(reply, "<name>first_name</name>");
			login.last_name = rpcGetString(reply, "<name>last_name</name>");
			login.region_x = rpcGetU32(reply, "<name>region_x</name>");
			login.region_y = rpcGetU32(reply, "<name>region_y</name>");
			login.home = rpcGetString(reply, "<name>home</name>");
			login.message = rpcGetString(reply, "<name>message</name>");
			login.circuit_code = rpcGetU32(reply, "<name>circuit_code</name>");
			login.sim_port = rpcGetU32(reply, "<name>sim_port</name>");
			login.sim_ip = rpcGetString(reply, "<name>sim_ip</name>");
			login.look_at = rpcGetString(reply, "<name>look_at</name>");
			login.agent_id = rpcGetString(reply, "<name>agent_id</name>");
			login.seconds_since_epoch = rpcGetU32(reply, "<name>seconds_since_epoch</name>");
		} else {
			std::stringstream message;
			message << "Network::loginReply(): Unknown login error, dumping server response:\n" << reply;
			log(message.str(), LOGERROR);
		}
	}

	if (login.reason.length()) {
		log("Network::loginReply(): Login failed. Reason: " + login.reason + ". Message: " + login.message, LOGWARNING);
	} else {
		// Set the variables received from login
		network->session_id((SimpleLLUUID)login.session_id);
		network->secure_session_id((SimpleLLUUID)login.secure_session_id);
		network->agent_id((SimpleLLUUID)login.agent_id);

		boost::asio::ipv4::address address(login.sim_ip);
		network->connectSim(address, login.sim_port, login.circuit_code, true);

		// Build and send the packet to move our avatar in to the sim
		PacketPtr packetPtr = CompleteAgentMovement(network->protocol(), network->agent_id(),
													network->session_id(), login.circuit_code);
		network->sendPacket(packetPtr);
	}

	callback(login);

	return realsize;
}

void Network::login(std::string firstName, std::string lastName, std::string password, std::string mac,
					  		   size_t major, size_t minor, size_t patch, size_t build,
							   std::string platform, std::string viewerDigest, std::string userAgent,
							   std::string author, loginCallback handler, std::string url)
{
	char loginError[CURL_ERROR_SIZE] = {0x00};
	struct curl_slist* headers = NULL;
	CURLcode response;
	CURL* curl = curl_easy_init();
	
	if (!curl) {
		log("Network::login(): curl_easy_init() returned NULL", LOGERROR);
		
		// Synthesize the callback to keep the client informed
		loginReply(NULL, 0, 0, &handler);
		
		return;
	}

	// Build an md5 hash of the password
	md5_state_t state;
	md5_byte_t digest[16];
	char passwordDigest[36] = {'$', '1', '$'};
	passwordDigest[16] = 0x00; // Null terminate for the conversion to a std::string
	md5_init(&state);
	md5_append(&state, (const md5_byte_t*)password.c_str(), password.length());
	md5_finish(&state, digest);
	for (int i = 0; i < 16; ++i) {
		sprintf(passwordDigest + (i * 2) + 3, "%02x", digest[i]);
	}
	passwordDigest[35] = 0x00;
	
	std::stringstream loginStream;
	loginStream <<
		"<?xml version=\"1.0\"?><methodCall><methodName>login_to_simulator</methodName><params><param><value><struct>"
		"<member><name>first</name><value><string>" << firstName << "</string></value></member>"
		"<member><name>last</name><value><string>" << lastName << "</string></value></member>"
		"<member><name>passwd</name><value><string>" << passwordDigest << "</string></value></member>"
		"<member><name>start</name><value><string>last</string></value></member>"
		"<member><name>major</name><value><string>" << major << "</string></value></member>"
		"<member><name>minor</name><value><string>" << minor << "</string></value></member>"
		"<member><name>patch</name><value><string>" << patch << "</string></value></member>"
		"<member><name>build</name><value><string>" << build << "</string></value></member>"
		"<member><name>platform</name><value><string>" << platform << "</string></value></member>"
		"<member><name>mac</name><value><string>" << mac << "</string></value></member>"
		"<member><name>viewer_digest</name><value><string>" << viewerDigest << "</string></value></member>"
		"<member><name>user-agent</name><value><string>" << userAgent << " (" << VERSION << ")</string></value></member>"
		"<member><name>author</name><value><string>" << author << "</string></value></member>"
		"</struct></value></param></params></methodCall>";
	std::string loginRequest = loginStream.str();

	// Set the accepted encoding and content type of the request
	headers = curl_slist_append(headers, "Accept-Encoding: gzip");
	headers = curl_slist_append(headers, "Content-Type: text/xml");

	//TODO: Maybe find a more elegant solution?
	callback = handler;

	curl_easy_setopt(curl, CURLOPT_ERRORBUFFER, loginError);
	curl_easy_setopt(curl, CURLOPT_URL, url.c_str());
	curl_easy_setopt(curl, CURLOPT_TIMEOUT, 20);
	curl_easy_setopt(curl, CURLOPT_SSL_VERIFYPEER, 0); // Ignore certificate authenticity (for now)
	curl_easy_setopt(curl, CURLOPT_POSTFIELDS, loginRequest.c_str());
	curl_easy_setopt(curl, CURLOPT_POSTFIELDSIZE, loginRequest.length());
	curl_easy_setopt(curl, CURLOPT_HTTPHEADER, headers);
	curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, loginReply);
	curl_easy_setopt(curl, CURLOPT_WRITEDATA, this);

	response = curl_easy_perform(curl);

	if (response) {
		std::stringstream message;
		message << "Network::login(): libcurl error: " << loginError;
		log(message.str(), LOGERROR);
		
		// Synthesize the callback to keep the client informed
		loginReply(NULL, 0, 0, &handler);
	}

	curl_slist_free_all(headers);
	curl_easy_cleanup(curl);
}

int Network::connectSim(boost::asio::ipv4::address ip, unsigned short port, unsigned int code, bool setCurrent)
{
	// Check if we are already connected to this sim
	for (size_t i = 0; i < _connections.size(); i++) {
		if (ip == _connections[i]->ip() && port == _connections[i]->port()) {
			log("Network::connectSim(): Attempting to connect to a sim we're already connected to", LOGWARNING);
			return -1;
		}
	}

	// Build the connection packet
	PacketPtr packetPtr = UseCircuitCode(_protocol, _agent_id, _session_id, code);

	// Create the SimConnection
	SimConnectionPtr sim(new SimConnection(ip, port, code));
	if (setCurrent || !_currentSim) _currentSim = sim;

	// Set the packet sequence number
	packetPtr->sequence(sim->sequence());

	boost::asio::datagram_socket* socket = new boost::asio::datagram_socket(_demuxer, boost::asio::ipv4::udp::endpoint(0));
	sim->socket(socket);

	// Push this connection on to the list
	_connections.push_back(sim);

	// Send the packet
	try {
		size_t bytesSent = socket->send_to(boost::asio::buffer(packetPtr->buffer(), packetPtr->length()), 0, sim->endpoint());
#ifdef DEBUG
		std::stringstream message;
		message << "Network::connectSim(): Sent " << bytesSent << " byte connection packet";
		log(message.str(), LOGINFO);
#endif
	} catch (boost::asio::error& e) {
		std::stringstream message;
		message << "Network::connectSim(): " << e;
		log(message.str(), LOGERROR);

		return -2;
	}

	// Start listening in a new thread
	boost::thread thread(boost::bind(&Network::listen, this, sim));

	return 0;
}

void Network::listen(SimConnectionPtr sim)
{
	// Start listening on this socket
	while (sim && sim->running()) {
		sim->socket()->async_receive(boost::asio::buffer(sim->buffer(), sim->bufferSize()), 0,
							  boost::bind(&Network::receivePacket, this, boost::asio::placeholders::error,
							  boost::asio::placeholders::bytes_transferred, sim->buffer()));
		//FIXME: I'm pretty sure multiple threads shouldn't be calling run() and reset() on the same demuxer
		//       simultaneously
		_demuxer.run();
		_demuxer.reset();
		
		// Sleep for 1000 nanoseconds
		boost::xtime xt;
		boost::xtime_get(&xt, boost::TIME_UTC);
		xt.nsec += 1000;
		boost::thread::sleep(xt);
	}

#ifdef DEBUG
	std::stringstream message;
	message << "Closed connection to sim " << sim->code();
	log(message.str(), LOGINFO);
#endif
}

void Network::receivePacket(const boost::asio::error& error, std::size_t length, char* receiveBuffer)
{
	PacketPtr packet;

	if (receiveBuffer[0] & MSG_RELIABLE) {
		// This packet requires an ACK
		//TODO: Instead of generating an ACK for each incoming packet, we're supposed to be appending 
		// these to any outgoing low commands. An implementation idea would be to add this sequence 
		// number to a list, and if it's the first on the list set a short timer that will send any 
		// ACKs in the list in a single packet. Meanwhile, any time a Low packet goes out it can check 
		// this list and append the ACKs. Packet class will need an appendACKs() function. Would be a 
		// good use of the asynchronous design of the sending and receiving.
		unsigned short id = ntohs(*(unsigned short*)(receiveBuffer + 2));
		PacketPtr ackPacket = PacketAck(_protocol, id);
		sendPacket(ackPacket);
	}

	if (receiveBuffer[0] & MSG_APPENDED_ACKS) {
		//TODO: Run through the packet backwards picking up the ACKs, then adjust length
	}

	if (receiveBuffer[0] & MSG_ZEROCODED) {
		//TODO: Can we optimize the size of this buffer?
		byte zeroBuffer[8192];

		size_t zeroLength = zeroDecode((byte*)receiveBuffer, length, zeroBuffer);
		packet.reset(new Packet(zeroBuffer, zeroLength, _protocol));
	} else {
		packet.reset(new Packet((byte*)receiveBuffer, length, _protocol));
	}

	//TODO: The library-level callback handler std::map will replace these if/else statements
	if (packet->command() == "PacketAck") {
		// TODO: Keep a list of outgoing reliable packets and check for incoming ACKs on them
	} else if (packet->command() == "StartPingCheck") {
		//TODO: Handle OldestUnacked
		byte* buffer = packet->buffer();
		byte pingID = buffer[5];

		packet = CompletePingCheck(_protocol, pingID);
		sendPacket(packet);
	} else if (packet->command() == "RegionHandshake") {
		//FIXME: What are the Flags supposed to be for this packet?
		PacketPtr replyPacket = RegionHandshakeReply(_protocol, 0);
		sendPacket(replyPacket);

		boost::mutex::scoped_lock lock(inboxMutex);
		_inbox.push_back(packet);
	} else {
		boost::mutex::scoped_lock lock(inboxMutex);
		_inbox.push_back(packet);
	}
}

int Network::sendPacket(boost::asio::ipv4::address ip, unsigned short port, PacketPtr packet)
{
	size_t sent;
	bool found = false;
	size_t i;

	// Check if we are connected to this sim
	for (i = 0; i < _connections.size(); i++) {
		if (ip == _connections[i]->ip() && port == _connections[i]->port()) {
			found = true;
			break;
		}
	}

	if (!found) {
		log("Network::sendPacket(): Trying to send a packet to a sim we're not connected to", LOGERROR);
		return -1;
	}

	// Set the packet sequence number
	packet->sequence(_connections[i]->sequence());

	if (packet->frequency() == frequencies::Low) {
		//TODO: If any ACKs need to be sent append them to this packet and set the flag
	}

	if (packet->buffer()[0] & MSG_RELIABLE) {
		//TODO: Append this packet to a list of outgoing MSG_RELIABLE packets, and create a timeout for
		// resending unACKed packets
	}

	if (packet->buffer()[0] & MSG_ZEROCODED) {
		//TODO: This shouldn't need to be much larger than the raw packet itself
		byte zeroBuffer[8192];
		size_t length = zeroEncode(packet->buffer(), packet->length(), zeroBuffer);

		try {
			sent = _connections[i]->socket()->send_to(boost::asio::buffer(zeroBuffer, length), 0, 
													  _connections[i]->endpoint());
		} catch (boost::asio::error& e) {
			std::stringstream message;
			message << "Network::sendPacket(): " << e << " (1)";
			log(message.str(), LOGERROR);

			return -2;
		}
	} else {
		try {
			sent = _connections[i]->socket()->send_to(boost::asio::buffer(packet->buffer(), packet->length()),
													  0, _connections[i]->endpoint());
		} catch (boost::asio::error& e) {
			std::stringstream message;
			message << "Network::sendPacket(): " << e << " (2)";
			log(message.str(), LOGERROR);

			return -3;
		}
	}

#ifdef DEBUG
	std::stringstream message;
	message << "Network::sendPacket(): Sent " << sent << " byte " << packet->command() << " datagram";
	log(message.str(), LOGINFO);
#endif

	return 0;
}

int Network::sendPacket(PacketPtr packet)
{
	if (!_currentSim) {
		log("Network::sendPacket() called when there is no current sim", LOGERROR);
		return -1;
	}

	return sendPacket(_currentSim->endpoint().address(), _currentSim->endpoint().port(), packet);
}
