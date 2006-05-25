#include "SecondLife.h"
#include <stdio.h>

SecondLife* client;

void loginHandler(loginParameters login)
{
	LLUUID tempID;

	if (login.reason.length()) {
		printf("Login failed\n");
	} else {
		// Set the variables received from login
		client->session_id((LLUUID)login.session_id);
		client->secure_session_id((LLUUID)login.secure_session_id);
		client->agent_id((LLUUID)login.agent_id);

		boost::asio::ipv4::address address(login.sim_ip);
		client->_network->connectSim(address, login.sim_port, login.circuit_code, true);

		Packet* packet = new Packet("CompleteAgentMovement", client->_protocol);

		tempID = client->agent_id();
		packet->setField("AgentData", 1, "AgentID", &tempID);
		tempID = client->session_id();
		packet->setField("AgentData", 1, "SessionID", &tempID);
		packet->setField("AgentData", 1, "CircuitCode", &login.circuit_code);

		client->_network->sendPacket(packet);

		while (1) {
			client->tick();
		}
	}
}

bool receivedPacket(std::string command, Packet* packet)
{
	byte* data = packet->rawData();
	size_t length = packet->length();
	
	// Debug
	printf("Received datagram, length: %u\n", length);
	for (size_t i = 0; i < length; i++) {
		printf("%02x ", data[i]);
	}
	printf("\n");
	
	return true;
}

int main()
{
	client = new SecondLife();

	if (client->loadKeywords("keywords.txt") == 0) {
		printf("Loaded keyword file\n");
	} else {
		printf("Failed to load the keyword file\n");

		delete client;
		return -1;
	}
	
	if (client->decryptCommFile("comm.dat", "output.txt") == 0) {
		printf("Decrypted comm file\n");
	} else {
		printf("Failed to decrypt the comm file\n");
	}

	if (client->buildProtocolMap("output.txt") == 0) {
		printf("Built protocol map\n");
	} else {
		printf("Failed to build the protocol map\n");

		delete client;
		return -2;
	}

	//client->_protocol->printMap();

	client->registerCallback("Default", &receivedPacket);
	
	client->login("First", "Last", "password", "00:00:00:00:00:00", "Win", "0", "test_app", "email@address.com", loginHandler);

	delete client;
	return 0;
}
