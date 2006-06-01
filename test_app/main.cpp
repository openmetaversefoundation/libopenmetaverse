#include "SecondLife.h"
#include <stdio.h>
#include  <signal.h>

SecondLife* client;

void loginHandler(loginParameters login)
{
	if (login.reason.length()) {
		printf("test_app: Login failed\n");
	} else {
		PacketPtr packetPtr = DirLandQuery(client->protocol(), false, true, SimpleLLUUID(1), true, 0,
								 		   client->agent_id(), client->session_id());
		client->sendPacket(packetPtr);

		while (1) {
			client->tick();
		}
	}
}

/*void writePacket(std::string command, PacketPtr packet)
{
	byte* data = packet->buffer();
	size_t length = packet->length();

	printf("Wrote packet to file\n");

	FILE* file = fopen("dirlandreply.dat", "ab");
	fwrite(data, length, 1, file);
	fclose(file);
}*/

void landPacket(std::string command, PacketPtr packet)
{
	FieldList::iterator field;
	BlockList::iterator block;
	BlockList blocks = packet->getBlocks();
	bool firstLand;
	int area;
	bool forSale;
	byte* parcelID;
	std::string name;
	bool auction;
	int salePrice;

	for (block = blocks.begin(); block != blocks.end(); ++block) {
		if ((*block)->name() == "QueryReplies") {
			for (field = (*block)->fields.begin(); field != (*block)->fields.end(); ++field) {
				if ((*field)->name() == "ReservedNewbie") {
					firstLand = *(*field)->data;
				} else if ((*field)->name() == "ActualArea") {
					area = *(int*)(*field)->data;
				} else if ((*field)->name() == "ForSale") {
					forSale = *(*field)->data;
				} else if ((*field)->name() == "ParcelID") {
					parcelID = (*field)->data;
				} else if ((*field)->name() == "Name") {
					name = (char*)(*field)->data;
				} else if ((*field)->name() == "Auction") {
					auction = *(*field)->data;
				} else if ((*field)->name() == "SalePrice") {
					salePrice = *(int*)(*field)->data;
				}
			}

			std::cout << name << " | Price: " << salePrice << " | Area: " << area << " | For Sale: "
					  << forSale << " | Auction: " << auction << std::endl;
		}
	}
}

void receivedPacket(std::string command, PacketPtr packet)
{
	/*byte* data = packet->buffer();
	size_t length = packet->length();
	
	if (!command.length()) {
		printf("test_app: Received foreign datagram:\n");
		
		for (size_t i = 0; i < length; i++) {
			printf("%02x ", data[i]);
		}
		printf("\n");

		return;
	}

	printf("test_app: Received a %u byte %s datagram (%u)\n", length, command.c_str(), packet->sequence());

	return;*/
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

	client->registerCallback("DirLandReply", &landPacket);
	client->registerCallback("Default", &receivedPacket);

	client->login("Chelsea", "Cork", "grapefruit", "00:00:00:00:00:00", 1, 10, 1, 0, "Win", "0", "test_app",
				  "jhurliman@wsu.edu", loginHandler);

	delete client;
	return 0;
}
