#include "SecondLife.h"
#include <stdio.h>
#include  <signal.h>

SecondLife* client;

void loginHandler(loginParameters login)
{
	LLUUID tempID;

	if (login.reason.length()) {
		printf("test_app: Login failed\n");
	} else {
		// Set the variables received from login
		client->session_id((LLUUID)login.session_id);
		client->secure_session_id((LLUUID)login.secure_session_id);
		client->agent_id((LLUUID)login.agent_id);

		boost::asio::ipv4::address address(login.sim_ip);
		client->connectSim(address, login.sim_port, login.circuit_code, true);

		Packet* packet = new Packet("CompleteAgentMovement", client->protocol());

		tempID = client->agent_id();
		packet->setField("AgentData", 1, "AgentID", 1, &tempID);
		tempID = client->session_id();
		packet->setField("AgentData", 1, "SessionID", 1, &tempID);
		packet->setField("AgentData", 1, "CircuitCode", 1, &login.circuit_code);

		client->sendPacket(packet);

		while (1) {
			client->tick();
		}
	}
}

void ignorePacket(std::string command, Packet* packet)
{
	//printf("Ignoring...\n");
}

void receivedPacket(std::string command, Packet* packet)
{
	byte* data = packet->rawData();
	size_t length = packet->length();
	
	if (!command.length()) {
		printf("test_app: Received foreign datagram. Possibly %u frequency:\n", packet->frequency());
		
		for (size_t i = 0; i < length; i++) {
			printf("%02x ", data[i]);
		}
		printf("\n");

		return;
	}

	printf("test_app: Received a %u byte %s datagram (%u)\n", length, command.c_str(), packet->sequence());

	return;
}

int main()
{
	client = new SecondLife();

	if (client->loadKeywords("keywords.txt") == 0) {
		printf("test_app: Loaded keyword file\n");
	} else {
		printf("test_app: Failed to load the keyword file\n");

		delete client;
		return -1;
	}

	if (client->decryptCommFile("comm.dat", "output.txt") == 0) {
		printf("test_app: Decrypted comm file\n");
	} else {
		printf("test_app: Failed to decrypt the comm file\n");
	}

	if (client->buildProtocolMap("output.txt") == 0) {
		printf("test_app: Built protocol map\n");
	} else {
		printf("test_app: Failed to build the protocol map\n");

		delete client;
		return -2;
	}

	//client->printMap();

	client->registerCallback("ViewerEffect", &ignorePacket);
	client->registerCallback("SimulatorViewerTimeMessage", &ignorePacket);
	client->registerCallback("CoarseLocationUpdate", &ignorePacket);
	client->registerCallback("Default", &receivedPacket);

	client->login("Chelsea", "Cork", "grape", "00:00:00:00:00:00", 1, 10, 0, 34, "Win", "0", "test_app",
				  "jhurliman@wsu.edu", loginHandler);

	delete client;
	return 0;
}
