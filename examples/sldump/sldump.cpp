/*
 * Copyright (c) 2006, Second Life Reverse Engineering Team
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
#include <signal.h>

#include <boost/program_options.hpp>

#include "SecondLife.h"

using namespace std;
namespace po = boost::program_options; 

SecondLife* client;

void loginHandler(loginParameters login)
{
	if (login.reason.length()) {
		cout << "test_app: Login failed. Reason: " << login.reason << ", Message: " << login.message << endl;
	} else {
		while (1) {
			client->tick();
		}
	}
}

void receivedPacket(string command, PacketPtr packet)
{
	FieldList::iterator field;
	BlockList::iterator block;
	BlockList blocks = packet->getBlocks();

	byte* u8;
	unsigned short* u16;
	unsigned int* u32;
	unsigned long long* u64;
	char* s8;
	short* s16;
	int* s32;
	long long* s64;
	float* f32;
	double* f64;

	// Print the packet name
	cout << "----" << packet->name() << "----" << endl;

	for (block = blocks.begin(); block != blocks.end(); ++block) {
		// Print the block name
		cout << "\t" << (*block)->name() << endl;

		for (field = (*block)->fields.begin(); field != (*block)->fields.end(); ++field) {
			// Print the field name
			cout << "\t\t" << (*field)->name() << ": ";

			switch ((*field)->type()) {
				case types::U8:
					u8 = (*field)->data;
					// Prevent this from being misinterpreted as a character
					u16 = (unsigned short*)u8;
					cout << *u16 << endl;
					break;
				case types::U16:
					u16 = (unsigned short*)(*field)->data;
					cout << *u16 << endl;
					break;
				case types::U32:
					u32 = (unsigned int*)(*field)->data;
					cout << *u32 << endl;
					break;
				case types::U64:
					u64 = (unsigned long long*)(*field)->data;
					cout << *u64 << endl;
					break;
				case types::S8:
					s8 = (char*)(*field)->data;
					// Prevent this from being misinterpreted as a character
					s16 = (short*)s8;
					cout << *s16 << endl;
					break;
				case types::S16:
					s16 = (short*)(*field)->data;
					cout << *s16 << endl;
					break;
				case types::S32:
					s32 = (int*)(*field)->data;
					cout << *s32 << endl;
					break;
				case types::S64:
					s64 = (long long*)(*field)->data;
					cout << *s64 << endl;
					break;
				case types::F32:
					f32 = (float*)(*field)->data;
					cout << *f32 << endl;
					break;
				case types::F64:
					f64 = (double*)(*field)->data;
					cout << *f64 << endl;
					break;
				case types::LLUUID:
					for (size_t i = 0; i < 16; ++i) {
						printf("%02x", *((*field)->data + i));
					}
					cout << endl;
					break;
				case types::Bool:
					u8 = (*field)->data;
					cout << ((*u8) ? "true" : "false") << endl;
					break;
				case types::LLVector3:
					//FIXME
					cout << endl;
					break;
				case types::LLVector3d:
					//FIXME
					cout << endl;
					break;
				case types::LLVector4:
					//FIXME
					cout << endl;
					break;
				case types::LLQuaternion:
					//FIXME
					cout << endl;
					break;
				case types::IPADDR:
					//FIXME
					cout << endl;
					break;
				case types::IPPORT:
					u16 = (unsigned short*)(*field)->data;
					cout << *u16 << endl;
					break;
				case types::Variable:
					cout << (char*)(*field)->data << endl;
					break;
				default:
					cout << "PARSING ERROR" << endl;
					break;
			}
		}
	}
}

int main(int ac, char** av)
{
	string keywordFile;
	string commFile;
	string firstName;
	string lastName;
	string password;

	// Declare the supported options
	po::options_description desc("Allowed options");
	desc.add_options()
		("help", "produce help message")
			("protocol-map", "dump the interpreted comm.dat to the console")
			("keywords,k", po::value<string>(&keywordFile)->default_value("keywords.txt"), "keywords.txt file")
			("comm,c", po::value<string>(&commFile)->default_value("comm.dat"), "comm.dat file")
			("first,f", po::value<string>(&firstName), "account first name")
			("last,l", po::value<string>(&lastName), "account last name")
			("password,pass,p", po::value<string>(&password), "account password")
		;

	po::variables_map vm;	
	po::store(po::command_line_parser(ac, av).options(desc).run(), vm);
	po::notify(vm);

	if (!vm.count("protocol-map")) {
		if (vm.count("help") || !vm.count("first") || !vm.count("last") || !vm.count("password")) {
			cout << desc << "\n";
			return 1;
		}
	}

	client = new SecondLife();

	if (client->loadKeywords("keywords.txt") == 0) {
		cout << "test_app: Loaded keyword file" << endl;
	} else {
		cout << "test_app: Failed to load the keyword file" << endl;

		delete client;
		return 2;
	}

	if (client->decryptCommFile("comm.dat", "output.txt") == 0) {
		cout << "test_app: Decrypted comm file" << endl;
	} else {
		cout << "test_app: Failed to decrypt the comm file" << endl;

		delete client;
		return -2;
	}

	if (client->buildProtocolMap("output.txt") == 0) {
		cout << "test_app: Built protocol map" << endl;
	} else {
		cout << "test_app: Failed to build the protocol map" << endl;

		delete client;
		return -2;
	}

	if (vm.count("protocol-map")) {
		client->printMap();
		return 0;
	}

	client->registerCallback("Default", &receivedPacket);

	client->login(firstName, lastName, password, "00:00:00:00:00:00", 1, 10, 1, 0, "Win", "0", "test_app",
				  "jhurliman@wsu.edu", loginHandler);

	delete client;
	return 0;
}
