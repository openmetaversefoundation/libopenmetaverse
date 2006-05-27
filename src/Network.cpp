#include <curl/curl.h>

#include "Network.h"
#include "md5.h"

Network::Network(ProtocolManager* protocol, SecondLife* secondlife)
{
	_protocol = protocol;
	_secondlife = secondlife;
	_currentSim = NULL;
	_agent_id = 0;
	_session_id = 0;
	_secure_session_id = 0;
}

Network::~Network()
{
#ifdef DEBUG
	std::cout << "Network::~Network() destructor called" << std::endl;
#endif
	std::vector<SimConnection*>::iterator connection;
	std::list<Packet*>::iterator packet;
	
	for (connection = _connections.begin(); connection != _connections.end(); ++connection) {
		delete (*connection);
	}
	
	for (packet = _inbox.begin(); packet != _inbox.end(); ++packet) {
		delete (*packet);
	}
}

size_t loginReply(void* buffer, size_t size, size_t nmemb, void* userp)
{
	loginParameters login;
	loginCallback* handler = (loginCallback*)userp;
	char* reply = (char*)buffer;
	std::string msg;
	int realsize = size * nmemb;
	
	if (!reply) {
		login.reason = "libsecondlife";
		login.message = "There was an error connecting to the login server, check the log file for details";
		(*handler)(login);
		return realsize;
	}

	msg = rpcGetString(reply, "<name>reason</name>");
	if (msg.length()) {
		login.reason = msg;
		login.message = rpcGetString(reply, "<name>message</name>");
		log("Network::loginReply(): Login failed. Reason: " + login.reason + ". Message: " + login.message, WARNING);
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
			log(message.str(), ERROR);
		}
	}

	(*handler)(login);
	
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
		log("Network::login(): curl_easy_init() returned NULL", ERROR);
		
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

	curl_easy_setopt(curl, CURLOPT_ERRORBUFFER, loginError);
	curl_easy_setopt(curl, CURLOPT_URL, url.c_str());
	curl_easy_setopt(curl, CURLOPT_TIMEOUT, 20);
	curl_easy_setopt(curl, CURLOPT_SSL_VERIFYPEER, 0); // Ignore certificate authenticity (for now)
	curl_easy_setopt(curl, CURLOPT_POSTFIELDS, loginRequest.c_str());
	curl_easy_setopt(curl, CURLOPT_POSTFIELDSIZE, loginRequest.length());
	curl_easy_setopt(curl, CURLOPT_HTTPHEADER, headers);
	curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, loginReply);
	curl_easy_setopt(curl, CURLOPT_WRITEDATA, &handler);
	
	response = curl_easy_perform(curl);

	if (response) {
		std::stringstream message;
		message << "Network::login(): libcurl error: " << loginError;
		log(message.str(), ERROR);
		
		// Synthesize the callback to keep the client informed
		loginReply(NULL, 0, 0, &handler);
	}

	curl_slist_free_all(headers);
	curl_easy_cleanup(curl);
}

int Network::connectSim(boost::asio::ipv4::address ip, unsigned short port, U32 code, bool setCurrent)
{
	// Check if we are already connected to this sim
	for (size_t i = 0; i < _connections.size(); i++) {
		if (ip == _connections[i]->ip() && port == _connections[i]->port()) {
			log("Network::connectSim(): Attempting to connect to a sim we're already connected to", WARNING);
			return -1;
		}
	}

	// Build a connection packet
	Packet* packet = new Packet("UseCircuitCode", _protocol, 44);
	packet->setField("CircuitCode", 1, "ID", 1, &_agent_id);
	packet->setField("CircuitCode", 1, "SessionID", 1, &_session_id);
	packet->setField("CircuitCode", 1, "Code", 1, &code);

	// Create the SimConnection
	SimConnection* sim = new SimConnection(ip, port, code);
	if (setCurrent || !_currentSim) _currentSim = sim;

	// Set the packet sequence number
	packet->sequence(sim->sequence());

	boost::asio::datagram_socket* socket = new boost::asio::datagram_socket(_demuxer, boost::asio::ipv4::udp::endpoint(0));
	sim->socket(socket);

	// Push this connection on to the list
	_connections.push_back(sim);
	
	// Send the packet
	try {
		size_t bytesSent = socket->send_to(boost::asio::buffer(packet->rawData(), packet->length()), 0, sim->endpoint());
		// Debug
		printf("Network::connectSim(): Sent %i byte connection packet\n", bytesSent);

		delete packet;
	} catch (boost::asio::error& e) {
		std::stringstream message;
		message << "Network::connectSim(): " << e;
		log(message.str(), ERROR);

		delete packet;
		return -2;
	}

	// Start listening in a new thread
	boost::thread thread(boost::bind(&Network::listen, this, sim));

	return 0;
}

void Network::listen(SimConnection* sim)
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
	log(message.str(), INFO);
#endif
}

void Network::receivePacket(const boost::asio::error& error, std::size_t length, char* receiveBuffer)
{
	Packet* packet;
	unsigned short command;
	byte* buffer = (byte*)receiveBuffer;

	if (length < 6) {
		log("Network::receivePacket(): Received packet less than six bytes, ignoring", WARNING);
		return;
	}

	if (buffer[4] == 0xFF) {
		if (buffer[5] == 0xFF) {
			// Low frequency packet
			memcpy(&command, &buffer[6], 2);
			command = ntohs(command);

			if (_protocol->commandString(command, ll::Low) == "PacketAck") {
				// TODO: At some point we'll want to track these Acks
#ifdef DEBUG
				log("Network::receivePacket(): Received PacketAck", INFO);
#endif
				return;
			} else {
				packet = new Packet(command, _protocol, buffer, length, ll::Low);
			}
		} else {
			// Medium frequency packet
			command = (unsigned short)buffer[5];
			packet = new Packet(command, _protocol, buffer, length, ll::Medium);
		}
	} else {
		// High frequency packet
		command = (unsigned short)buffer[4];

		if (_protocol->commandString(command, ll::High) == "StartPingCheck") {
			// Ping request from the server, respond
			packet = new Packet(command, _protocol, buffer, length, ll::High);
			U8 pingID = *(U8*)packet->getField("PingID", 1, "PingID", 1);

			//TODO: Should we be looking at OldestUnacked as well?
			delete packet;

			packet = new Packet("CompletePingCheck", _protocol, 6);
			packet->setField("PingID", 1, "PingID", 1, &pingID);
			sendPacket(packet);

			return;
		} else {
			packet = new Packet(command, _protocol, buffer, length, ll::High);
		}
	}

	// Push it on to the list
	boost::mutex::scoped_lock lock(_inboxMutex);
	_inbox.push_back(packet);
}

int Network::sendPacket(boost::asio::ipv4::address ip, unsigned short port, Packet* packet)
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
		//FIXME: Log
		return -1;
	}

	// Set the packet sequence number
	packet->sequence(_connections[i]->sequence());

	try {
		sent = _connections[i]->socket()->send_to(boost::asio::buffer(packet->rawData(), packet->length()),
														 0, _connections[i]->endpoint());
	} catch (boost::asio::error& e) {
		std::stringstream message;
		message << "Network::sendPacket(): " << e;
		log(message.str(), ERROR);

		return -2;
	}

#ifdef DEBUG
	std::stringstream message;
	message << "Network::sendPacket(): Sent " << sent << " byte " << packet->command() << " datagram";
	log(message.str(), INFO);
#endif

	return 0;
}

int Network::sendPacket(Packet* packet)
{
	if (!_currentSim) {
		log("Network::sendPacket() called when there is no current sim", ERROR);
		return -1;
	}

	return sendPacket(_currentSim->endpoint().address(), _currentSim->endpoint().port(), packet);
}
