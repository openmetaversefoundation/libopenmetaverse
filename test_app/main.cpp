#include "SecondLife.h"
#include <stdio.h>

int main()
{
	//Packet* packet;
	bool success;
	SecondLife* client = new SecondLife();

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

	client->_protocol->printMap();

	//printf("Building UseCircuitCode packet\n");
	//packet = new Packet(client->_protocol);
	//success = packet->setCommand("UseCircuitCode");

	if (success) {
		//byte agentID[16] = {0x00,0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x09,0x0A,0x0B,0x0C,0x0D,0x0E,0x0F};
		//packet->setField("CircuitCode", 1, "ID", agentID);
		boost::asio::ipv4::address address("192.168.0.105");
		client->_network->connectSim(address, 1000, 12345, true);
	} else {
		printf("Failed to build the AddCircuitCode packet");
	}

	//delete packet;
	delete client;
	return 0;
}
