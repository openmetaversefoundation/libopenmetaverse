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
	//FIXME: Close and free all the sockets
	//       Delete all remaining packets
}

size_t loginReply(void* buffer, size_t size, size_t nmemb, void* userp)
{
	loginParameters login;
	loginCallback* handler = (loginCallback*)userp;
	char* reply = (char*)buffer;
	std::string msg;
	int realsize = size * nmemb;

	msg = rpcGetString(reply, "<name>reason</name>");
	if (msg.length()) {
		//FIXME: Log
		login.reason = msg;
		login.message = rpcGetString(reply, "<name>message</name>");
		// Debug
		std::cout << "Login failed. Reason: " << login.reason << " Message: " << login.message << std::endl;
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
			//FIXME: Log
			// Debug
			std::cout << "Unknown login error, dumping server response\n\n" << reply << std::endl;
		}
	}

	(*handler)(login);
	
	return realsize;
}

void Network::login(std::string firstName, std::string lastName, std::string password, std::string mac,
					  		   std::string platform, std::string viewerDigest, std::string userAgent,
							   std::string author, loginCallback handler, std::string url)
{
	char loginError[CURL_ERROR_SIZE] = {0x00};
	struct curl_slist* headers = NULL;
	CURLcode response;
	CURL* curl = curl_easy_init();

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
	
	std::string loginRequest =
		"<?xml version=\"1.0\"?><methodCall><methodName>login_to_simulator</methodName><params><param><value><struct>"
		"<member><name>first</name><value><string>" + firstName + "</string></value></member>"
		"<member><name>last</name><value><string>" + lastName + "</string></value></member>"
		"<member><name>passwd</name><value><string>" + passwordDigest + "</string></value></member>"
		"<member><name>start</name><value><string>last</string></value></member>"
		"<member><name>major</name><value><string>1</string></value></member>"
		"<member><name>minor</name><value><string>9</string></value></member>"
		"<member><name>patch</name><value><string>0</string></value></member>"
		"<member><name>build</name><value><string>21</string></value></member>"
		"<member><name>platform</name><value><string>" + platform + "</string></value></member>"
		"<member><name>mac</name><value><string>" + mac + "</string></value></member>"
		"<member><name>viewer_digest</name><value><string>" + viewerDigest + "</string></value></member>"
		"<member><name>user-agent</name><value><string>" + userAgent + " (" + VERSION + ")</string></value></member>"
		"<member><name>author</name><value><string>" + author + "</string></value></member>"
		"</struct></value></param></params></methodCall>";

	if (!curl) {
		//FIXME: Log, and fire the callback
		return;
	}

	// Set the accepted encoding and content type of the request
	headers = curl_slist_append(headers, "Accept-Encoding: gzip");
	headers = curl_slist_append(headers, "Content-Type: text/xml");

	curl_easy_setopt(curl, CURLOPT_ERRORBUFFER, loginError);
	curl_easy_setopt(curl, CURLOPT_URL, url.c_str());
	curl_easy_setopt(curl, CURLOPT_TIMEOUT, 9); // 10 second timeout
	curl_easy_setopt(curl, CURLOPT_SSL_VERIFYPEER, 0); // Ignore certificate authenticity (for now)
	curl_easy_setopt(curl, CURLOPT_POSTFIELDS, loginRequest.c_str());
	curl_easy_setopt(curl, CURLOPT_POSTFIELDSIZE, loginRequest.length());
	curl_easy_setopt(curl, CURLOPT_HTTPHEADER, headers);
	curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, loginReply);
	curl_easy_setopt(curl, CURLOPT_WRITEDATA, &handler);
	
	response = curl_easy_perform(curl);

	if (response) {
		//FIXME: Log, and fire the callback
		// Debug
		std::cout << "Error logging in: " << loginError << std::endl;
	}

	curl_slist_free_all(headers);
	curl_easy_cleanup(curl);
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
	}

	// Debug
	printf("Closed connection to %u\n", (unsigned int)sim);
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
	Packet* packet = new Packet("UseCircuitCode", _protocol, 44);
	packet->setField("CircuitCode", 1, "ID", &_agent_id);
	packet->setField("CircuitCode", 1, "SessionID", &_session_id);
	packet->setField("CircuitCode", 1, "Code", &code);

	// Create the SimConnection
	SimConnection* sim = new SimConnection(ip, port, code);
	if (setCurrent || !_currentSim) _currentSim = sim;

	// Set the packet sequence number
	packet->sequence(sim->sequence());

	boost::asio::datagram_socket* socket = new boost::asio::datagram_socket(_demuxer, boost::asio::ipv4::udp::endpoint(0));
	sim->socket(socket);

	// Send the packet
	try {
		size_t bytesSent = socket->send_to(boost::asio::buffer(packet->rawData(), packet->length()), 0, sim->endpoint());
		// Debug
		printf("Sent %i byte connection packet\n", bytesSent);

		delete packet;
	} catch (boost::asio::error& e) {
		delete packet;

		// Debug
		std::cerr << e << std::endl;

		return -2;
	}

	// Start listening in a new thread
	boost::thread thread(boost::bind(&Network::listen, this, sim));

	return 0;
}

void Network::receivePacket(const boost::asio::error& error, std::size_t length, char* receiveBuffer)
{
	// Debug
	printf("Received datagram, length: %u\n", length);
	for (size_t i = 0; i < length; i++) {
		printf("%02x ", receiveBuffer[i]);
	}
	printf("\n");

	//FIXME: Decode the command name from the packet so we can call the Packet constructor properly

	// Build a Packet object and fill it with the incoming data
	//FIXME: Something in here is segfaulting, fix it
	/*Packet* packet = new Packet();
	packet->rawData((byte*)receiveBuffer, length);

	// Push it on to the list
	boost::mutex::scoped_lock lock(_inboxMutex);
	_inbox.push_back(packet);*/
}

int Network::sendPacket(boost::asio::ipv4::address ip, unsigned short port, Packet* packet)
{
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

	try {
		size_t sent = _connections[i]->socket()->send_to(boost::asio::buffer(packet->rawData(), packet->length()),
										   				 0, _connections[i]->endpoint());

		// Debug
		printf("Sent %u bytes\n", sent);
	} catch (boost::asio::error& e) {
		// Debug
		std::cerr << e << std::endl;

		return -2;
	}

	return 0;
}

int Network::sendPacket(Packet* packet)
{
	if (!_currentSim) {
		//FIXME: Log
		return -1;
	}

	return sendPacket(_currentSim->endpoint().address(), _currentSim->endpoint().port(), packet);
}
