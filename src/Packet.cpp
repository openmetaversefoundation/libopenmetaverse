#include "Packet.h"

Packet::Packet(std::string command, ProtocolManager* protocol, size_t length)
{
	// Make the minimum length sane to avoid excessive bounds checking
	_length = (length > 5) ? length : 0;
	_protocol = protocol;
	unsigned short flags;

	_layout = _protocol->command(command);

	if (!_layout) {
		log("Packet::Packet(): Trying to initialize with invalid command: " + command, ERROR);
		_buffer = NULL;
		_length = 0;
	} else {
		_buffer = (byte*)calloc(_length ? _length : DEFAULT_PACKET_SIZE, sizeof(byte));

		if (!_buffer) {
			std::stringstream message;
			message << "Packet::Packet(): " << (_length ? _length : DEFAULT_PACKET_SIZE) << " byte calloc failed";
			log(message.str(), ERROR);
			_length = 0;
		}

		// Setup the flags
		//FIXME: The flags are wrong right now, UseCircuitCode is supposed to be 0x4000 but
		//       the current setup has it at 0x0000 according to the protocol map. Was
		//       snowcrash wrong? Why are virtually all packets (except acks) sent with
		//       0x4000 when lots of commands are untrusted? trusted != reliable?
		flags = (_layout->trusted & MSG_RELIABLE) + (_layout->encoded & MSG_ZEROCODED);
		
		//FIXME: Serious hack to temporarily work around the aforementioned problem
		if (!flags) {
			flags = 0x40;
		}

		memcpy(_buffer, &flags, 2);

		// Setup the frequency/id bytes
		switch (_layout->frequency) {
			case ll::Low:
				if (_length < 8) {
					_buffer = (byte*)realloc(_buffer, 8);
					if (!_buffer) {
						log("Packet::Packet(): 8 byte realloc failed", ERROR);
						_length = 0;
					} else {
						_length = 8;
					}
				}

				_buffer[4] = 0xFF;
				_buffer[5] = 0xFF;
				flags = htons(_layout->id);
				memcpy(_buffer + 6, &flags, 2);
				_headerLength = 8;
				break;
			case ll::Medium:
				if (_length < 6) {
					_buffer = (byte*)realloc(_buffer, 6);
					if (!_buffer) {
						log("Packet::Packet(): 6 byte realloc failed", ERROR);
						_length = 0;
					} else {
						_length = 6;
					}
				}

				_buffer[4] = 0xFF;
				_buffer[5] = _layout->id;
				_headerLength = 6;
				break;
			case ll::High:
				if (_length < 5) {
					_buffer = (byte*)realloc(_buffer, 5);
					if (!_buffer) {
						log("Packet::Packet(): 5 byte realloc failed", ERROR);
						_length = 0;
					} else {
						_length = 5;
					}
				}

				_buffer[4] = _layout->id;
				_headerLength = 5;
				break;
			case ll::Invalid:
				//FIXME: Log
				break;
		}
	}
}

Packet::Packet(unsigned short command, ProtocolManager* protocol, byte* buffer, size_t length, ll::frequency frequency)
{
	_length = length;
	_protocol = protocol;
	_layout = _protocol->command(command, frequency);

	if (!_layout) {
		std::stringstream message;
		message << "Packet::Packet(): Trying to build a packet from unknown command code " << command <<
				", frequency " << frequency;
		log(message.str(), ERROR);
	}

	_buffer = (byte*)malloc(length);
	if (!_buffer) {
		//FIXME: Log memory error
		_length = 0;
		return;
	}

	memcpy(_buffer, buffer, length);

	switch (frequency) {
		case ll::Low:
			_headerLength = 8;
		case ll::Medium:
			_headerLength = 6;
		case ll::High:
			_headerLength = 5;
		case ll::Invalid:
			_headerLength = 0;
	}
}

Packet::~Packet()
{
	free(_buffer);
}

ll::frequency Packet::frequency()
{
	return _layout ? _layout->frequency : ll::Invalid;
}

unsigned short Packet::flags()
{
	if (_length < 2) {
		log("Packet::flags(): Flags requested on a datagram less than 2 bytes", WARNING);
		return 0;
	}

	return (unsigned short)*_buffer;
}

void Packet::flags(unsigned short flags)
{
	if (_length < 2) {
		// Make room, assume the default packet size
		_buffer = (byte*)realloc(_buffer, DEFAULT_PACKET_SIZE);

		if (!_buffer) {
			std::stringstream message;
			message << "Packet::flags(): " << DEFAULT_PACKET_SIZE << " byte realloc failed";
			log(message.str(), ERROR);

			_length = 0;
			return;
		} else {
			_length = 2;
		}
	}

	memcpy(_buffer, &flags, 2);
}

unsigned short Packet::sequence()
{
	if (_length < 4) {
		log("Packet::sequence(): Sequence requested on a datagram less than 4 bytes", WARNING);
		return 0;
	}
	
	unsigned short sequence;
	memcpy(&sequence, _buffer + 2, 2);
	//return ntohs((unsigned short)*(_buffer + 2));
	return ntohs(sequence);
}

void Packet::sequence(unsigned short sequence)
{
	if (_length < 4) {
		// Make room, assume the default packet size
		_buffer = (byte*)realloc(_buffer, DEFAULT_PACKET_SIZE);

		if (!_buffer) {
			std::stringstream message;
			message << "Packet::sequence(): " << DEFAULT_PACKET_SIZE << " byte realloc failed";
			log(message.str(), ERROR);

			_length = 0;
			return;
		} else {
			_length = 4;
		}
	}

	unsigned short nSequence = htons(sequence);
	memcpy(_buffer + 2, &nSequence, sizeof(sequence));
}

std::string Packet::command()
{
	return _layout->name;
}

bool Packet::command(std::string command)
{
	packetDiagram* layout = _protocol->command(command);
	if (!layout) return false;

	_layout = layout;
	return true;
}

ll::llType Packet::fieldType(std::string block, std::string field)
{
	std::list<packetBlock*>::iterator i;

	for (i = _layout->blocks.begin(); i != _layout->blocks.end(); ++i) {
		if ((*i)->name == block) {
			packetBlock* block = (*i);

			std::list<packetField*>::iterator j;

			for (j = block->fields.begin(); j != block->fields.end(); ++j) {
				if ((*j)->name == field) {
					return (*j)->type;
				}
			}
		}

		log("Packet::fieldType(): Couldn't find field " + field + " in block " + block, ERROR);
		return ll::INVALID_TYPE;
	}

	log("Packet::fieldType(): Couldn't find block " + block, ERROR);
	return ll::INVALID_TYPE;
}

void* Packet::getField(std::string block, size_t blockNumber, std::string field, size_t fieldNumber)
{
	// Check how many blocks this field can hold, and if blockNumber is in range
	int frequency = _protocol->blockFrequency(_layout, block);
	if (frequency != -1 && (int)blockNumber > frequency) {
		// blockNumber is out of range
		//FIXME: Log
		return NULL;
	}

	// Find the total offset for the field
	size_t blockSize = _protocol->blockSize(_layout, block);
	if (!blockSize) {
		//FIXME: Log
		return NULL;
	}
	
	// Find out what type of field this is
	ll::llType type = fieldType(block, field);
	if (type == ll::INVALID_TYPE) {
		log("Packet::getField(): Couldn't find field type for: " + field + ", in block: " + block, ERROR);
		return NULL;
	}

	// Get the offset for this field
	int fieldOffset = _protocol->fieldOffset(_layout, block, field);
	if (fieldOffset < 0) {
		log("Packet::getField(): Couldn't get the field offset for: " + field + ", in block: " + block, ERROR);
		return NULL;
	}

	// Get the size of this type of field
	size_t fieldSize = _protocol->typeSize(type);

	return (void*)(_buffer + _headerLength + blockSize * blockNumber + fieldOffset + fieldSize * (fieldNumber - 1));
}

int Packet::setField(std::string block, size_t blockNumber, std::string field, size_t fieldNumber, void* value)
{
	if (!_layout) {
		//FIXME: Log
		return -1;
	}

	// Find out what type of field this is
	ll::llType type = fieldType(block, field);
	if (type == ll::INVALID_TYPE) {
		log("Packet::setField(): Couldn't find field type for: " + field + ", in block: " + block, ERROR);
		return -2;
	}

	// Check how many blocks this field can hold, and if blockNumber is in range
	int frequency = _protocol->blockFrequency(_layout, block);
	if (blockNumber <= 0 || (frequency != -1 && (int)blockNumber > frequency)) {
		// blockNumber is out of range
		//FIXME: Log
		return -3;
	}

	// Find the total offset for the field
	size_t blockSize = _protocol->blockSize(_layout, block);
	if (!blockSize) {
		//FIXME: Log
		return -4;
	}

	int fieldOffset = _protocol->fieldOffset(_layout, block, field);
	if (fieldOffset < 0) {
		log("Packet::setField(): Couldn't get the field offset for: " + field + ", in block: " + block, ERROR);
		return -5;
	}
	
	size_t fieldSize = _protocol->typeSize(type);
	size_t offset = _headerLength + blockSize * (blockNumber - 1) + fieldOffset + fieldSize * (fieldNumber - 1);

	// Reallocate memory if necessary
	if ((offset + fieldSize) > _length) {
		_buffer = (byte*)realloc(_buffer, offset + fieldSize);
		if (!_buffer) {
			std::stringstream message;
			message << "Packet::setField(): " << offset + fieldSize << " byte realloc failed";
			log(message.str(), ERROR);

			_length = 0;
			return -6;
		}

		_length = offset + fieldSize;
	}

	// Write the actual value
	memcpy(_buffer + offset, value, fieldSize);

	return 0;
}

byte* Packet::rawData()
{
	return _buffer;
}

void Packet::rawData(byte* buffer, size_t length)
{
	if (length > _length) {
		_buffer = (byte*)realloc(_buffer, length);
		if (!_buffer) {
			std::stringstream message;
			message << "Packet::rawData(): " << length << " byte realloc failed";
			log(message.str(), ERROR);

			_length = 0;
			return;
		}
	}

	memcpy(_buffer, buffer, length);
	_length = length;
}
