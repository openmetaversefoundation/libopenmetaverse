#include "Packet.h"

/*BlockContainer::BlockContainer(packetBlock* _layout)
{
	layout = _layout;

	if (layout) {
		variable = ((layout->frequency == -1) ? true : false);
	}
}

BlockContainer::BlockContainer(ProtocolManager* protocol, std::string command, std::string blockName)
{
	std::list<packetBlock*>::iterator block;

	packetDiagram* _layout = protocol->command(command);

	if (_layout) {
		for (block = _layout->blocks.begin(); block != _layout->blocks.end(); ++block) {
			if ((*block)->name == blockName) {
				layout = (*block);
				variable = ((layout->frequency == -1) ? true : false);
				return;
			}
		}

		log("BlockContainer::BlockContainer(): Block lookup: " + blockName + ", failed in command: " + command, LOGERROR);
	} else {
		log("BlockContainer::BlockContainer(): Command lookup failed: " + command, LOGERROR);
	}
}*/

Packet::Packet(std::string command, ProtocolManager* protocol)
{
	unsigned short id;

	_protocol = protocol;
	_command = command;
	_layout = _protocol->command(_command);

	if (!_layout) {
		log("Packet::Packet(): Initializing with invalid command: \"" + _command + "\"", LOGERROR);
		_buffer = NULL;
		_length = 0;
		return;
	}

	_frequency = _layout->frequency;

	switch (_layout->frequency) {
		case frequencies::Low:
			_buffer = (byte*)malloc(8);
			_buffer[4] = _buffer[5] = 0xFF;
			id = _layout->id;
			id = htons(id);
			memcpy(_buffer + 6, &id, 2);
			_length = 8;
			break;
		case frequencies::Medium:
			_buffer = (byte*)malloc(6);
			_buffer[4] = 0xFF;
			_buffer[5] = (byte)_layout->id;
			_length = 6;
			break;
		case frequencies::High:
			_buffer = (byte*)malloc(5);
			_buffer[4] = (byte)_layout->id;
			_length = 5;
			break;
		case frequencies::Invalid:
			log("Packet::Packet(): Command \"" + _command + "\" has an invalid frequency", LOGERROR);
			_buffer = NULL;
			_length = 0;
			return;
		default:
			log("Packet::Packet(): Command \"" + _command + "\" has an unknown frequency", LOGERROR);
			_buffer = NULL;
			_length = 0;
			return;
	}
}

Packet::Packet(byte* buffer, size_t length, ProtocolManager* protocol)
{
	unsigned short command;

	_protocol = protocol;

	_buffer = (byte*)malloc(length);
	if (!_buffer) {
		log("Packet::Packet(): malloc() failed", LOGERROR);
		_length = 0;
		return;
	}

	memcpy(_buffer, buffer, length);
	_length = length;

	// Determine the packet frequency
	if (_length > 4) {
		if (_buffer[4] == 0xFF) {
			if (_buffer[5] == 0xFF) {
				// Low frequency
				_frequency = frequencies::Low;
				memcpy(&command, &_buffer[6], 2);
				command = ntohs(command);
				_layout = _protocol->command(command, frequencies::Low);
				_command = _protocol->commandString(command, frequencies::Low);
			} else {
				// Medium frequency
				_frequency = frequencies::Medium;
				command = _buffer[5];
				_layout = _protocol->command(command, frequencies::Medium);
				_command = _protocol->commandString(command, frequencies::Medium);
			}
		} else {
			// High frequency
			_frequency = frequencies::High;
			command = _buffer[4];
			_layout = _protocol->command(command, frequencies::High);
			_command = _protocol->commandString(command, frequencies::High);
		}
	} else {
		log("Received a datagram less than five bytes", LOGWARNING);
	}
}

void Packet::payload(byte* payload, size_t payloadLength)
{
	if (_buffer) {
		_buffer = (byte*)realloc(_buffer, _length + payloadLength);

		if (_buffer) {
			memcpy(_buffer + _length, payload, payloadLength);
			_length += payloadLength;
		} else {
			log("Packet::payload(): realloc() failed", LOGERROR);
		}
	} else {
		log("Packet::payload(): Attempting to append a payload to a packet with a null buffer", LOGERROR);
	}
}

unsigned short Packet::flags()
{
	unsigned short* flags = (unsigned short*)_buffer;
	return ntohs(*flags);
}

void Packet::flags(unsigned short flags)
{
	if (_buffer && _length > 2) {
		memcpy(_buffer, &flags, 2);
	} else {
		log("Packet::flags(): Null or too short buffer", LOGERROR);
	}
}

unsigned short Packet::sequence()
{
	unsigned short* sequence = (unsigned short*)(_buffer + 2);
	return ntohs(*sequence);
}

void Packet::sequence(unsigned short sequence)
{
	if (_buffer && _length > 4) {
		sequence = htons(sequence);
		memcpy(_buffer + 2, &sequence, 2);
	} else {
		log("Packet::sequence(): Null or too short buffer", LOGERROR);
	}
}

size_t Packet::headerLength()
{
	if (_layout) {
		switch (_layout->frequency) {
			case frequencies::Low:
				return 8;
			case frequencies::Medium:
				return 6;
			case frequencies::High:
				return 5;
			case frequencies::Invalid:
				log("Packet::headerLength(): Invalid frequency", LOGERROR);
				break;
			default:
				log("Packet::headerLength(): Unknown frequency", LOGERROR);
		}
	} else {
		log("Packet::headerLength(): layout is NULL", LOGERROR);
	}

	return 0;
}

boost::any Packet::getField(std::string blockName, std::string fieldName)
{
	return 0;
}

boost::any Packet::getField(std::string blockName, size_t blockNumber, std::string fieldName)
{
	return 0;
}

PacketBlockPtr Packet::getBlock(std::string blockName)
{
	PacketBlockPtr block;

	return block;
}

PacketBlockPtr Packet::getBlock(std::string blockName, size_t blockNumber)
{
	PacketBlockPtr block;

	return block;
}

BlockList Packet::getBlocks()
{
	std::list<packetBlock*>::iterator blockMap;
	std::list<packetField*>::iterator fieldMap;
	BlockList blockList;
	PacketBlockPtr block;
	size_t pos = headerLength();

	for (blockMap = _layout->blocks.begin(); blockMap != _layout->blocks.end(); ++blockMap) {
		size_t blockCount;

		if ((*blockMap)->count == -1) {
			if (pos < _length) {
				blockCount = _buffer[pos];
				pos++;
			} else {
				log("Packet::getBlocks(): goto 1 reached", LOGWARNING);
				goto done;
			}
		} else {
			blockCount = (*blockMap)->count;
		}
		
		for (size_t i = 0; i < blockCount; ++i) {
			block.reset(new PacketBlock(*blockMap));
			blockList.push_back(block);

			for (fieldMap = (*blockMap)->fields.begin(); fieldMap != (*blockMap)->fields.end(); ++fieldMap) {
				size_t fieldCount = (*fieldMap)->count;

				for (size_t j = 0; j < fieldCount; ++j) {
					size_t fieldSize;

					if ((*fieldMap)->type == types::Variable) {
						if (pos < _length) {
							if ((*fieldMap)->count == 1) {
								fieldSize = _buffer[pos];
								pos++;
							} else if ((*fieldMap)->count == 2) {
								fieldSize = *(unsigned short*)(_buffer + pos);
								pos += 2;
							} else {
								log("Packet::getBlocks(): Abnormally sized Variable field", LOGWARNING);
							}
						} else {
							log("Packet::getBlocks(): goto 2 reached", LOGWARNING);
							goto done;
						}
					} else {
						fieldSize = _protocol->typeSize((*fieldMap)->type);
					}

					if (pos + fieldSize <= _length) {
						PacketFieldPtr packetFieldPtr(new PacketField(*fieldMap, _buffer + pos, fieldSize));
						block->fields.push_back(packetFieldPtr);

						pos += fieldSize;
					} else {
						log("Packet::getBlocks(): goto 3 reached", LOGWARNING);
						goto done;
					}
				}
			}
		}
	}

done:

	return blockList;
}


/*void Packet::unserialize()
{
	size_t pos;
	size_t i;
	size_t j;
	size_t fieldSize;
	byte frequency;
	byte fieldFrequency;
	unsigned short command;
	std::list<packetBlock*>::iterator block;
	std::list<packetField*>::iterator field;

	// Determine the packet frequency
	if (_length > 4) {
		if (_buffer[4] == 0xFF) {
			if (_buffer[5] == 0xFF) {
				// Low frequency
				_frequency = frequencies::Low;
				memcpy(&command, &_buffer[6], 2);
				command = ntohs(command);
				_layout = _protocol->command(command, frequencies::Low);
				_command = _protocol->commandString(command, frequencies::Low);
				pos = 8;
			} else {
				// Medium frequency
				_frequency = frequencies::Medium;
				command = _buffer[5];
				_layout = _protocol->command(command, frequencies::Medium);
				_command = _protocol->commandString(command, frequencies::Medium);
				pos = 6;
			}
		} else {
			// High frequency
			_frequency = frequencies::High;
			command = _buffer[4];
			_layout = _protocol->command(command, frequencies::High);
			_command = _protocol->commandString(command, frequencies::High);
			pos = 5;
		}
	} else {
		pos = _length;
	}

	// Clear any old structures
	blockContainers.clear();

	// Iterate through the packet map
	if (_layout) {
		for (block = _layout->blocks.begin(); block != _layout->blocks.end(); ++block) {
			if (pos <= _length) {
				// Get the number of occurrences for this block
				if ((*block)->frequency == -1) {
					// First byte of a variable block is the number of occurrences (frequency)
					memcpy(&frequency, _buffer + pos, 1);
					pos++;
				} else {
					frequency = (*block)->frequency;
				}

				// Create a BlockContainer for this block
				BlockContainerPtr blockContainerPtr(new BlockContainer((*block)));
				blockContainers.push_back(blockContainerPtr);

				// Iterate through this set of blocks
				for (i = 0; i < frequency; ++i) {
					BlockPtr blockPtr(new Block());
					blockContainerPtr->blocks.push_back(blockPtr);

					// Iterate through this block
					for (field = (*block)->fields.begin(); field != (*block)->fields.end(); ++field) {
						if (pos <= _length) {
							fieldFrequency = (*field)->frequency;

							for (j = 0; j < fieldFrequency; j++) {
								if ((*field)->type == types::Variable) {
									// First byte of a variable field is the length in bytes
									size_t length;
									memcpy(&length, _buffer + pos, 1);
									pos++;

									FieldPtr fieldPtr(new Variable(_buffer + pos, length));
									blockPtr->push_back(fieldPtr);
									pos += length;
								} else {
									// Get the size of this field
									fieldSize = _protocol->typeSize((*field)->type);

									if (pos + fieldSize <= _length) {
										FieldPtr fieldPtr = createField((*field), _buffer + pos);
										blockPtr->push_back(fieldPtr);
										pos += fieldSize;
									} else {
										log("Reached the end of the packet before the end of the map (1)", LOGWARNING);
										goto done;
									}
								}
							}
						} else {
							//log("Reached the end of the packet before the end of the map (2)", LOGWARNING);
							goto done;
						}
					}
				}
			} else {
				log("Reached the end of the packet before the end of the map (3)", LOGWARNING);
				goto done;
			}
		}
	} else {
		log("Packet::unserialize(): Trying to unserialize a packet with no layout", LOGERROR);
	}

done:
	return;
}*/
