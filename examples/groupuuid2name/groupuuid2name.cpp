/*
 * Copyright (c) 2006, John Hurliman
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without 
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the Second Life Reverse Engineering Team nor the names 
 *   of its contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
 * POSSIBILITY OF SUCH DAMAGE.
 */

#include <stdio.h>

#include <boost/program_options.hpp>
#include <boost/asio.hpp>
#include <boost/date_time/posix_time/posix_time.hpp>

#include "SecondLife.h"

using namespace std;
namespace po = boost::program_options;

enum state {
	state_login,
	state_sendlogout,
	state_loggingout,
	state_loggedout
};

SecondLife* client;
string groupUUID;
int currentState = state_login;
boost::asio::demuxer demuxer;
boost::asio::deadline_timer timer(demuxer);

void runDemuxer()
{
	demuxer.run();
}

void logout(string command, PacketPtr packet)
{
	currentState = state_loggedout;
}

void timerExpired(const boost::asio::error& e)
{
	currentState = state_loggedout;
}

void loginHandler(loginParameters login)
{
	if (login.reason.length()) {
		cout << "ERROR: Login failed. Reason: " << login.reason << ", Message: " << login.message << endl;
	} else {
		PacketPtr packet;

		// Setup a sanity check timer
		timer.expires_from_now(boost::posix_time::seconds(9));
		timer.async_wait(timerExpired);
		boost::thread thread(&runDemuxer);

		//Send the UUIDGroupNameRequest packet
		packet = UUIDGroupNameRequest(client->protocol(), groupUUID);
		client->sendPacket(packet);

		while (currentState != state_loggedout) {
			if (currentState == state_sendlogout) {
				packet = LogoutRequest(client->protocol(), client->agent_id(), client->session_id());
				client->sendPacket(packet);

				currentState = state_loggingout;
			}

			client->tick();
		}

		timer.cancel();
	}
}

void uuidGroupNameReply(string command, PacketPtr packet)
{
	PacketFieldPtr field;
	BlockList blocks = packet->getBlocks();
	BlockList::iterator block;

	for (block = blocks.begin(); block != blocks.end(); ++block) {
		if ((*block)->name() == "UUIDNameBlock") {
			 field = (*block)->fields.at(1);

			if (field->name() == "GroupName") {
				cout << "GROUP: " << (char*)field->data << endl;
			} else {
				cout << "ERROR: Unexpected field format encountered" << endl;
			}
		} else {
			cout << "ERROR: Unexpected block format encountered" << endl;
		}
	}

	currentState = state_sendlogout;
}

int main(int ac, char** av)
{
	string keywordFile;
	string mapFile;
	string firstName;
	string lastName;
	string password;

	// Declare the supported options
	po::options_description desc("Allowed options");
	desc.add_options()
			("help", "produce help message")
			("keywords,k", po::value<string>(&keywordFile)->default_value("keywords.txt"), "keywords.txt file")
			("map,o", po::value<string>(&mapFile)->default_value("protocol.txt"), "decrypted protocol file")
			("first,f", po::value<string>(&firstName), "account first name")
			("last,l", po::value<string>(&lastName), "account last name")
			("password,pass,p", po::value<string>(&password), "account password")
			("uuid,u", po::value<string>(&groupUUID), "group uuid")
			;

	po::variables_map vm;
	po::store(po::command_line_parser(ac, av).options(desc).run(), vm);
	po::notify(vm);

	if (vm.count("help") || !vm.count("first") || !vm.count("last") || !vm.count("password") || !vm.count("uuid")) {
		cout << desc << "\n";
		return 1;
	}

	if (groupUUID.length() != 32 && groupUUID.length() != 36) {
		cout << "ERROR: Group UUID is in the incorrect format" << endl;
		return 2;
	}

	client = new SecondLife();

	if (client->loadKeywords(keywordFile) != 0) {
		cout << "ERROR: Failed to load the keyword file" << endl;

		delete client;
		return 3;
	}

	if (client->buildProtocolMap(mapFile) != 0) {
		cout << "ERROR: Failed to build the protocol map" << endl;

		delete client;
		return 4;
	}

	client->registerCallback("LogoutReply", &logout);
	client->registerCallback("UUIDGroupNameReply", &uuidGroupNameReply);

	client->login(firstName, lastName, password, "00:00:00:00:00:00", 1, 10, 1, 0, "Win", "0", "groupuuid2name",
				  "jhurliman@wsu.edu", loginHandler);

	delete client;
	return 0;
}
