#include "ProtocolManager.h"

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

	return false;
}

ProtocolManager::ProtocolManager()
{
	llTypes[0]  = "U8";
	llTypes[1]  = "U16";
	llTypes[2]  = "U32";
	llTypes[3]  = "U64";
	llTypes[4]  = "S8";
	llTypes[5]  = "S16";
	llTypes[6]  = "S32";
	llTypes[7]  = "S64";
	llTypes[8]  = "F8";
	llTypes[9]  = "F16";
	llTypes[10] = "F32";
	llTypes[11] = "F64";
	llTypes[12] = "LLUUID";
	llTypes[13] = "BOOL";
	llTypes[14] = "LLVector3";
	llTypes[15] = "LLVector3d";
	llTypes[16] = "LLVector4";
	llTypes[17] = "LLQuaternion";
	llTypes[18] = "IPADDR";
	llTypes[19] = "IPPORT";
	llTypes[20] = "Variable";
	llTypes[21] = "Fixed";
	llTypes[22] = "Single";
	llTypes[23] = "Multiple";
	llTypes[24] = "";

	llTypesSizes[0]  = 1;
	llTypesSizes[1]  = 2;
	llTypesSizes[2]  = 4;
	llTypesSizes[3]  = 8;
	llTypesSizes[4]  = 1;
	llTypesSizes[5]  = 2;
	llTypesSizes[6]  = 4;
	llTypesSizes[7]  = 8;
	llTypesSizes[8]  = 1;
	llTypesSizes[9]  = 2;
	llTypesSizes[10] = 4;
	llTypesSizes[11] = 8;
	llTypesSizes[12] = 16;
	llTypesSizes[13] = 1;
	llTypesSizes[14] = sizeof(llVector3);
	llTypesSizes[15] = sizeof(llVector3d);
	llTypesSizes[16] = sizeof(llVector4);
	llTypesSizes[17] = sizeof(llQuaternion);
	llTypesSizes[18] = 4;
	llTypesSizes[19] = 2;
	llTypesSizes[20] = -1;
}

ProtocolManager::~ProtocolManager()
{
#ifdef DEBUG
	std::cout << "ProtocolManager::~ProtocolManager() destructor called" << std::endl;
#endif

	int i;
	std::list<packetBlock*>::iterator j;
	std::list<packetField*>::iterator k;

	for (i = 0; i < 65536; ++i) {
		if (i < 256) {
			for (j = _mediumPackets[i].blocks.begin(); j != _mediumPackets[i].blocks.end(); ++j) {
				for (k = (*j)->fields.begin(); k != (*j)->fields.end(); ++k) {
					delete (*k);
				}
				
				delete (*j);
			}

			for (j = _highPackets[i].blocks.begin(); j != _highPackets[i].blocks.end(); ++j) {
				for (k = (*j)->fields.begin(); k != (*j)->fields.end(); ++k) {
					delete (*k);
				}
				
				delete (*j);
			}
		}

		for (j = _lowPackets[i].blocks.begin(); j != _lowPackets[i].blocks.end(); ++j) {
			for (k = (*j)->fields.begin(); k != (*j)->fields.end(); ++k) {
				delete (*k);
			}
				
			delete (*j);
		}
	}
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
					printf("\t\t%04u %s (%s)\n", (*k)->keywordPosition, (*k)->name.c_str(), typeName((*k)->type).c_str());
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
					printf("\t\t%04u %s (%s)\n", (*k)->keywordPosition, (*k)->name.c_str(), typeName((*k)->type).c_str());
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
					printf("\t\t%04u %s (%s)\n", (*k)->keywordPosition, (*k)->name.c_str(), typeName((*k)->type).c_str());
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
			log("ProtocolManager::getFields(): Found fourth tier elements", ERROR);
			return false;
		}

		std::string temp = protocolMap.substr(fieldStart + 1, (fieldEnd - 1) - fieldStart);
		temp = trim(temp);
		field = new packetField();

		size_t delimiter = temp.find_first_of(" ");
		if (delimiter == std::string::npos) {
			log("ProtocolManager::getFields(): Couldn't parse field: " + temp, ERROR);
			return false;
		}

		// Get the field name
		field->name = temp.substr(0, delimiter);

		// Get the keyword position
		field->keywordPosition = keywordPosition(field->name);

		temp = temp.substr(delimiter + 1, temp.length() - delimiter - 1);

		// Get the field type
		delimiter = temp.find_first_of(" ");
		if (delimiter != std::string::npos) {
			field->frequency = atoi(temp.substr(delimiter + 1, temp.length() - delimiter - 1).c_str());
			temp = temp.substr(0, delimiter);
		} else {
			field->frequency = 1;
		}
		field->type = fieldType(temp);

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
		block->keywordPosition = keywordPosition(block->name);

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
		log("ProtocolManager::loadKeywords(): Error opening keyword file: " + filename, ERROR);
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
		log("ProtocolManager::decryptCommFile(): Couldn't open comm file: " + source, ERROR);
		return -1;
	}

	FILE* output = fopen(destination.c_str(), "wb");
	if (!output) {
		log("ProtocolManager::decryptCommFile(): Couldn't open output file: " + destination, ERROR);
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

int ProtocolManager::keywordPosition(std::string keyword)
{
	std::map<std::string, int>::iterator result;

	result = _keywordMap.find(keyword);
	if (result == _keywordMap.end()) {
		log("ProtocolManager::keywordPosition(): Couldn't find keyword: " + keyword, WARNING);
		return -1;
	}

	return result->second;
}

int ProtocolManager::buildProtocolMap(std::string filename)
{
	std::string protocolMap;
	byte buffer[2048];
	size_t nread;
	packetDiagram* layout;
	size_t end;
	size_t cmdStart = 0;
	size_t cmdEnd;
	size_t cmdChildren = 0;
	size_t low = 1;
	size_t medium = 1;
	size_t high = 1;

	FILE* input = fopen(filename.c_str(), "rb");
	if (!input) {
		log("ProtocolManager::buildProtocolMap(): Couldn't open output file: " + filename, ERROR);
		return -1;
	}

	// Read the file in to memory
	while ((nread = fread(buffer, sizeof(char), sizeof(buffer), input)) > 0) {
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
			int fixed = httoi(temp.c_str()) ^ 0xffff0000;
			layout = &_lowPackets[fixed];
			layout->id = fixed;
			layout->frequency = ll::Low;
		} else if (temp == "Low") {
			layout = &_lowPackets[low];
			layout->id = low;
			layout->frequency = ll::Low;
			low++;
		} else if (temp == "Medium") {
			layout = &_mediumPackets[medium];
			layout->id = medium;
			layout->frequency = ll::Medium;
			medium++;
		} else if (temp == "High") {
			layout = &_highPackets[high];
			layout->id = high;
			layout->frequency = ll::High;
			high++;
		} else {
			log("ProtocolManager::buildProtocolMap(): Unknown frequency: " + temp, ERROR);

			// Increment our position in protocol map
			cmdStart = cmdEnd + 1;
			cmdEnd = end;
			continue;
		}

		// Get the command name
		layout->name = header_tokens.at(0);

		// Trusted?
		layout->trusted = (header_tokens.at(2) == "Trusted");

		// Encoded?
		layout->encoded = (header_tokens.at(3) == "Zerocoded");

		// Get the blocks
		getBlocks(layout, protocolMap, cmdStart + 1, cmdEnd - 1);

		// Increment our position in protocol map
		cmdStart = cmdEnd + 1;
		cmdEnd = end;
	}

	fclose(input);

	return 0;
}

packetDiagram* ProtocolManager::command(std::string command)
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

packetDiagram* ProtocolManager::command(unsigned short command, ll::frequency frequency)
{
	switch (frequency)
	{
		case ll::Low:
			return &_lowPackets[command];
		case ll::Medium:
			return &_mediumPackets[command];
		case ll::High:
			return &_highPackets[command];
		case ll::Invalid:
			break;
	}

	log("ProtocolManager::command(): Invalid frequency passed in", WARNING);
	return NULL;
}

std::string ProtocolManager::commandString(unsigned short command, ll::frequency frequency)
{
	switch (frequency)
	{
		case ll::Low:
			return _lowPackets[command].name;
		case ll::Medium:
			return _mediumPackets[command].name;
		case ll::High:
			return _highPackets[command].name;
		case ll::Invalid:
			break;
		default:
			break;
	}
	
	log("ProtocolManager::commandString(): Invalid frequency passed in", WARNING);
	return "";
}

ll::llType ProtocolManager::fieldType(std::string type)
{
	int i = 0;

	while (llTypes[i].length()) {
		if (type == llTypes[i]) {
			return (ll::llType)i;
		}

		i++;
	}

	log("ProtocolManager::fieldType(): Unknown type: " + type, WARNING);
	return ll::INVALID_TYPE;
}

int ProtocolManager::typeSize(ll::llType type)
{
	if (type < 0 || type > 19) {
		std::stringstream message;
		message << "ProtocolManager::typeSize(): Unknown type: " << type;
		log(message.str(), WARNING);

		return 0;
	}

	return llTypesSizes[type];
}

std::string ProtocolManager::typeName(ll::llType type)
{
	std::string typeName;

	if (type < 0 || type > 19) {
		std::stringstream message;
		message << "ProtocolManager::typeName(): Unknown type: " << type;
		log(message.str(), WARNING);

		typeName = "Invalid";
	} else {
		typeName = llTypes[type];
	}

	return typeName;
}

int ProtocolManager::blockFrequency(packetDiagram* layout, std::string block)
{
	std::list<packetBlock*>::iterator i;

	for ( i = layout->blocks.begin(); i != layout->blocks.end(); ++i) {
		if ((*i)->name == block) {
			return (*i)->frequency;
		}
	}

	log("ProtocolManager::blockFrequency(): Unknown block: " + block, WARNING);
	return 0;
}

size_t ProtocolManager::blockSize(packetDiagram* layout, std::string block)
{
	std::list<packetBlock*>::iterator i;

	if (!layout) {
		log("ProtocolManager::blockSize(): NULL layout passed in", WARNING);
		return 0;
	}

	for (i = layout->blocks.begin(); i != layout->blocks.end(); ++i) {
		if ((*i)->name == block) {
			packetBlock* block = (*i);
			size_t size = 0;

			std::list<packetField*>::iterator j;

			for (j = block->fields.begin(); j != block->fields.end(); ++j) {
				size += typeSize((*j)->type);
			}

			return size;
		}
	}

	log("ProtocolManager::blockSize(): Unknown block: " + block, WARNING);
	return 0;
}

int ProtocolManager::fieldOffset(packetDiagram* layout, std::string block, std::string field)
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
					offset += (int)typeSize((*j)->type);
				}
			}

			// The block didn't have the field we're looking for
			std::stringstream message;
			message << "ProtocolManager::fieldOffset(): Couldn't find field: " << field << ", in block: " << block;
			log(message.str(), WARNING);
			return -1;
		}
	}

	log("ProtocolManager::fieldOffset(): Couldn't find block: " + block, WARNING);
	return -2;
}
