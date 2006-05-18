#include "ProtocolManager.h"

// Trim an ISO C++ string by Marco Dorantes
std::string trim(std::string &s, const std::string &drop = " ")
{
	std::string r = s.erase(s.find_last_not_of(drop) + 1);
	return r.erase(0, r.find_first_not_of(drop));
}

bool getBlockMarkers(const char* buffer, size_t &start, size_t &end, size_t &children)
{
	size_t startBlock = 0;
	size_t depth = 0;
	
	children = 0;

	for (size_t i = start; i <= end; i++) {
		if (buffer[i] == '{') {
			depth++;

			if (depth == 1) {
				startBlock = i;
			} else if (depth == 2 && !children) {
				children = i;
			}
		} else if (buffer[i] == '}') {
			depth--;

			if (depth == 0 && startBlock) {
				start = startBlock;
				end = i;

				return true;
			}
		}
	}

	//FIXME: Log
	return false;
}

ProtocolManager::ProtocolManager()
{
	;
}

ProtocolManager::~ProtocolManager()
{
	//FIXME: Apparently the linked lists don't automatically destroy the objects
	//       it has pointers for, so we need to iterate through the entire map 
	//       and free memory.
}

void ProtocolManager::printMap()
{
	size_t i;
	std::list<packetBlock*>::iterator j;
	std::list<packetField*>::iterator k;

	for (i = 0; i < 65536; i++) {
		if (_lowPackets[i].name.length()) {
			printf("Low %05u - %s - %s - %s\n", i, _lowPackets[i].name.c_str(), 
				   _lowPackets[i].trusted ? "Trusted" : "Untrusted",
				   _lowPackets[i].encoded ? "Unencoded" : "Zerocoded");
			
			for (j = _lowPackets[i].blocks.begin(); j != _lowPackets[i].blocks.end(); ++j) {
				printf("\t%04u %s (%02i)\n", (*j)->keywordPosition, (*j)->name.c_str(), (*j)->frequency);

				for (k = (*j)->fields.begin(); k != (*j)->fields.end(); ++k) {
					printf("\t\t%04u %s (%s)\n", (*k)->keywordPosition, (*k)->name.c_str(), getTypeName((*k)->type).c_str());
				}
			}
		}
	}

	for (i = 0; i < 256; i++) {
		if (_mediumPackets[i].name.length()) {
			printf("Medium %05u - %s - %s - %s\n", i, _mediumPackets[i].name.c_str(), 
				   _mediumPackets[i].trusted ? "Trusted" : "Untrusted",
				   _mediumPackets[i].encoded ? "Unencoded" : "Zerocoded");
			
			for (j = _mediumPackets[i].blocks.begin(); j != _mediumPackets[i].blocks.end(); ++j) {
				printf("\t%04u %s (%02i)\n", (*j)->keywordPosition, (*j)->name.c_str(), (*j)->frequency);

				for (k = (*j)->fields.begin(); k != (*j)->fields.end(); ++k) {
					printf("\t\t%04u %s (%s)\n", (*k)->keywordPosition, (*k)->name.c_str(), getTypeName((*k)->type).c_str());
				}
			}
		}
	}

	for (i = 0; i < 256; i++) {
		if (_highPackets[i].name.length()) {
			printf("High %05u - %s - %s - %s\n", i, _highPackets[i].name.c_str(), 
				   _highPackets[i].trusted ? "Trusted" : "Untrusted",
				   _highPackets[i].encoded ? "Unencoded" : "Zerocoded");
			
			for (j = _highPackets[i].blocks.begin(); j != _highPackets[i].blocks.end(); ++j) {
				printf("\t%04u %s (%02i)\n", (*j)->keywordPosition, (*j)->name.c_str(), (*j)->frequency);

				for (k = (*j)->fields.begin(); k != (*j)->fields.end(); ++k) {
					printf("\t\t%04u %s (%s)\n", (*k)->keywordPosition, (*k)->name.c_str(), getTypeName((*k)->type).c_str());
				}
			}
		}
	}
}

bool ProtocolManager::getFields(packetBlock* block, std::string protocolMap, size_t start, size_t end)
{
	size_t fieldStart = start;
	size_t fieldEnd = end;
	size_t children = 0;
	packetField* field;

	while(getBlockMarkers(protocolMap.c_str(), fieldStart, fieldEnd, children)) {
		if (children) {
			//FIXME: Log
			return false;
		}

		std::string temp = protocolMap.substr(fieldStart + 1, (fieldEnd - 1) - fieldStart);
		temp = trim(temp);
		field = new packetField();

		size_t delimiter = temp.find_first_of(" ");
		if (delimiter == std::string::npos) {
			//FIXME: Log
			return false;
		}

		// Get the field name
		field->name = temp.substr(0, delimiter);

		// Get the keyword position
		field->keywordPosition = getKeywordPosition(field->name);

		// Get the field type
		temp = temp.substr(delimiter + 1, temp.length() - delimiter - 1);
		field->type = getFieldType(temp);

		// Add this field to the linked list
		block->fields.push_back(field);

		fieldStart = fieldEnd + 1;
		fieldEnd = end;
	}

	// Sort the fields based on the keyword position
	using namespace std;
	block->fields.sort(greater<packetField*>());

	return true;
}

bool ProtocolManager::getBlocks(packetDiagram* packet, std::string protocolMap, size_t start, size_t end)
{
	size_t blockStart = start;
	size_t blockEnd = end;
	size_t children = 0;
	packetBlock* block;

	while (getBlockMarkers(protocolMap.c_str(), blockStart, blockEnd, children)) {
		std::string temp = protocolMap.substr(blockStart + 1, (children ? children - 1 : blockEnd - 1) - blockStart);
		temp = trim(temp);
		std::stringstream stream(temp);
		std::vector<std::string> block_tokens;
		block = new packetBlock();

		while (stream >> temp) {
			block_tokens.push_back(temp);
		}

		// Get the block name
		block->name = block_tokens.at(0);

		// Find the frequency of this block (-1 for variable, 1 for single)
		temp = block_tokens.at(1);
		if (temp == "Variable") {
			block->frequency = -1;
		} else if (temp == "Single") {
			block->frequency = 1;
		} else if (temp == "Multiple") {
			std::istringstream int_stream(block_tokens.at(2));
			int_stream >> block->frequency;
		} else {
			block->frequency = 5;
		}

		// Get the keyword position of this block
		block->keywordPosition = getKeywordPosition(block->name);

		// Add this block to the linked list
		packet->blocks.push_back(block);

		// Populate the fields linked list
		getFields(block, protocolMap, blockStart + 1, blockEnd - 1);

		blockStart = blockEnd + 1;
		blockEnd = end;
	}

	// Sort the blocks based on the keyword position
	using namespace std;
	packet->blocks.sort(greater<packetBlock*>());

	return true;
}

int ProtocolManager::loadKeywords(std::string filename)
{
	std::ifstream input(filename.c_str());
	std::string line;
	int i = 0;

	if (!input.is_open()) {
		//FIXME: Log
		return -1;
	}

	while (!input.eof()) {
		getline(input, line);
		line = trim(line, "\r");
		_keywordMap[line] = i++;
	}

	input.close();
	return 0;
}

int ProtocolManager::decryptCommFile(std::string source, std::string destination)
{
	byte magicKey = 0;
	byte buffer[2048];
	size_t nread;

	FILE* commFile = fopen(source.c_str(), "rb");
	if (!commFile) {
		//FIXME: Debug log this
		return -1;
	}

	FILE* output = fopen(destination.c_str(), "wb");
	if (!output) {
		//FIXME: Debug log
		return -2;
	}

	while ((nread = fread(buffer, sizeof(char), sizeof(buffer), commFile)) > 0) {
		// Decryption
		for (size_t i = 0; i < nread; i++)
		{
			buffer[i] ^= magicKey;
			magicKey += 43;
		}

		fwrite(buffer, sizeof(char), nread, output);
    }

	fclose(commFile);
	fclose(output);

	return 0;
}

int ProtocolManager::getKeywordPosition(std::string keyword)
{
	std::map<std::string, int>::iterator result;

	result = _keywordMap.find(keyword);
	if (result == _keywordMap.end()) {
		//FIXME: Log
		return -1;
	}

	return result->second;
}

int ProtocolManager::buildProtocolMap(std::string filename)
{
	std::string protocolMap;
	byte buffer[2048];
	size_t nread;
	packetDiagram* packet;
	size_t end;
	size_t cmdStart = 0;
	size_t cmdEnd;
	size_t cmdChildren = 0;
	size_t low = 1;
	size_t medium = 1;
	size_t high = 1;

	FILE* input = fopen(filename.c_str(), "rb");
	if (!input) {
		//FIXME: Debug log
		return -1;
	}

	// Read the file in to memory
	while ((nread = fread(buffer, sizeof(char), sizeof(buffer), input)) > 0) {
		/*if (nread != BUFFER_SIZE) {
			buffer[nread] = '\0';
		} else {
			buffer[BUFFER_SIZE] = '\0';
		}*/

		protocolMap.append((const char*)&buffer, nread);
	}

	cmdEnd = end = protocolMap.length();

	while (getBlockMarkers(protocolMap.c_str(), cmdStart, cmdEnd, cmdChildren)) {
		std::string temp = protocolMap.substr(cmdStart + 1, (cmdChildren ? cmdChildren - 1 : cmdEnd - 1) - cmdStart);
		temp = trim(temp);
		std::stringstream header(temp);
		std::vector<std::string> header_tokens;
		
		while (header >> temp) {
			header_tokens.push_back(temp);
		}
		
		// Get the frequency first so we know where to put this command
		temp = header_tokens.at(1);
		if (temp == "Fixed") {
			// Get the fixed position
			temp = header_tokens.at(2);
			// Truncate it to a short
			int fixed = httoi((char*)temp.c_str()) ^ 0xffff0000;
			packet = &_lowPackets[fixed];
			packet->frequency = ll::Low;
		} else if (temp == "Low") {
			packet = &_lowPackets[low++];
			packet->frequency = ll::Low;
		} else if (temp == "Medium") {
			packet = &_mediumPackets[medium++];
			packet->frequency = ll::Medium;
		} else if (temp == "High") {
			packet = &_highPackets[high++];
			packet->frequency = ll::High;
		} else {
			//FIXME: Debug log
			return -2;
		}

		// Get the command name
		packet->name = header_tokens.at(0);

		// Trusted?
		packet->trusted = (header_tokens.at(2) == "Trusted");

		// Encoded?
		packet->encoded = (header_tokens.at(3) == "Zerocoded");

		// Get the blocks
		getBlocks(packet, protocolMap, cmdStart + 1, cmdEnd - 1);

		// Increment our position in protocol map
		cmdStart = cmdEnd + 1;
		cmdEnd = end;
	}

	fclose(input);

	return 0;
}

packetDiagram* ProtocolManager::getCommand(std::string command)
{
	size_t i;

	for (i = 0; i < 65536; i++) {
		if (_lowPackets[i].name == command) return &_lowPackets[i];
	}

	for (i = 0; i < 255; i++) {
		if (_mediumPackets[i].name == command) return &_mediumPackets[i];
	}

	for (i = 0; i < 255; i++) {
		if (_highPackets[i].name == command) return &_highPackets[i];
	}

	return NULL;
}

ll::llType ProtocolManager::getFieldType(std::string type)
{
	const std::string llTypes[] = {"U8", "U16", "U32", "U64", "S8", "S16", "S32", "S64",
								   "F8", "F16", "F32", "F64", "LLUUID", "BOOL", "LLVector3", 
								   "LLVector3d", "LLQuaternion", "IPADDR", "IPPORT", 
								   "Variable", "Fixed", "Single", "Multiple", ""};
	int i = 0;

	while (llTypes[i].length()) {
		if (type == llTypes[i]) {
			return (ll::llType)i;
		}

		i++;
	}

	//FIXME: Log
	return ll::INVALID_TYPE;
}

int ProtocolManager::getTypeSize(ll::llType type)
{
	// U8, U16, U32, U64, S8, S16, S32, S64, F8, F16, F32, F64, LLUUID, BOOL, llVector3, 
	// llVector3d, llQuaternion, IPADDR, IPPORT, Variable
	const int sizes[] = {1, 2, 4, 8, 1, 2, 4, 8, 1, 2, 4, 8, 16, 1, sizeof(llVector3), 
                         sizeof(llVector3d), sizeof(llQuaternion), 4, 2, -1};

	if (type < 0 || type > 19) {
		//FIXME: Log
		return 0;
	} else {
		return sizes[type];
	}
}

std::string ProtocolManager::getTypeName(ll::llType type)
{
	std::string typeName;
	std::string names[] = {"U8", "U16", "U32", "U64", "S8", "S16", "S32", "S64", "F8", "F16", "F32", 
						   "F64", "LLUUID", "BOOL", "llVector3", "llVector3d", "llQuaternion", "IPADDR", 
						   "IPPORT", "Variable"};

	if (type < 0 || type > 19) {
		//FIXME: Log
		typeName = "Invalid";
	} else {
		typeName = names[type];
	}

	return typeName;
}

int ProtocolManager::getBlockFrequency(packetDiagram* layout, std::string block)
{
	std::list<packetBlock*>::iterator i;

	for ( i = layout->blocks.begin(); i != layout->blocks.end(); ++i) {
		if ((*i)->name == block) {
			return (*i)->frequency;
		}
	}

	//FIXME: Log
	return 0;
}

size_t ProtocolManager::getBlockSize(packetDiagram* layout, std::string block)
{
	std::list<packetBlock*>::iterator i;

	for (i = layout->blocks.begin(); i != layout->blocks.end(); ++i) {
		if ((*i)->name == block) {
			packetBlock* block = (*i);
			size_t size = 0;

			std::list<packetField*>::iterator j;

			for (j = block->fields.begin(); j != block->fields.end(); ++j) {
				size += getTypeSize((*j)->type);
			}

			return size;
		}
	}

	//FIXME: Log
	return 0;
}

int ProtocolManager::getFieldOffset(packetDiagram* layout, std::string block, std::string field)
{
	std::list<packetBlock*>::iterator i;

	for (i = layout->blocks.begin(); i != layout->blocks.end(); ++i) {
		if ((*i)->name == block) {
			packetBlock* block = (*i);
			int offset = 0;

			std::list<packetField*>::iterator j;

			for (j = block->fields.begin(); j != block->fields.end(); ++j) {
				if ((*j)->name == field) {
					return offset;
				} else {
					offset += (int)getTypeSize((*j)->type);
				}
			}

			// The block didn't have the field we're looking for
			//FIXME: Log
			return -1;
		}
	}

	//FIXME: Log
	return -2;
}
