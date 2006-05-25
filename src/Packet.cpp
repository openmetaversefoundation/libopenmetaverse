#include "Packet.h"

Packet::Packet(std::string command, ProtocolManager* protocol, size_t length)
{
	// Make the minimum length sane to avoid excessive bounds checking
	_length = (length > 8) ? length : 8;
	_protocol = protocol;
	unsigned short flags;

	_layout = _protocol->getCommand(command);
	if (!_layout) {
		//FIXME: Log invalid packet name
		_buffer = NULL;
		_length = 0;
	} else {
		_buffer = (byte*)calloc(_length ? _length : DEFAULT_PACKET_SIZE, sizeof(byte));

		if (!_buffer) {
			//FIXME: Log memory error
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
		//flags = htons(flags);
		memcpy(_buffer, &flags, 2);

		// Setup the frequency/id bytes
		switch (_layout->frequency) {
			case ll::Low:
				_buffer[4] = 0xFF;
				_buffer[5] = 0xFF;
				flags = htons(_layout->id);
				memcpy(_buffer + 6, &flags, 2);
				_headerLength = 8;
				break;
			case ll::Medium:
				_buffer[4] = 0xFF;
				_buffer[5] = _layout->id;
				_headerLength = 6;
				break;
			case ll::High:
				_buffer[4] = _layout->id;
				_headerLength = 5;
				break;
			case ll::Invalid:
				//FIXME: Log
				break;
		}
	}
}

Packet::Packet(unsigned short command, ProtocolManager* protocol, byte* buffer, size_t length, byte headerLength,
			   ll::frequency frequency)
{
	_length = length;
	_protocol = protocol;
	_layout = _protocol->getCommand(command, frequency);

	_buffer = (byte*)malloc(length);
	if (!_buffer) {
		//FIXME: Log memory error
		_length = 0;
		return;
	}

	memcpy(_buffer, buffer, length);

	_headerLength = headerLength;
}

Packet::~Packet()
{
	free(_buffer);
}

ll::frequency Packet::frequency()
{
	if (!_layout) {
		return ll::Invalid;
	}

	return _layout->frequency;
}

unsigned short Packet::flags()
{
	if (_length < 2) {
		return 0;
	}

	return (unsigned short)*_buffer;
}

void Packet::flags(unsigned short flags)
{
	if (_length < 2) {
		// Make room, assume the default packet size
		_buffer = (byte*)realloc(_buffer, DEFAULT_PACKET_SIZE);
		_length = 2;
	}

	if (!_buffer) {
		//FIXME: Log memory error
		_length = 0;
		return;
	}

	memcpy(_buffer, &flags, sizeof(flags));
}

unsigned short Packet::sequence()
{
	if (_length < 4) {
		return 0;
	}

	return ntohs((unsigned short)*(_buffer + 2));
}

void Packet::sequence(unsigned short sequence)
{
	unsigned short nSequence = htons(sequence);
	memcpy(_buffer + 2, &nSequence, sizeof(sequence));
}

std::string Packet::command()
{
	return _layout->name;
}

bool Packet::command(std::string command)
{
	packetDiagram* layout = _protocol->getCommand(command);
	if (!layout) return false;

	_layout = layout;
	return true;
}

ll::llType Packet::getFieldType(std::string block, std::string field)
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

		//FIXME: Log
		return ll::INVALID_TYPE;
	}

	//FIXME: Log
	return ll::INVALID_TYPE;
}

void* Packet::getField(std::string block, size_t blockNumber, std::string field)
{
	// Check how many blocks this field can hold, and if blockNumber is in range
	int frequency = _protocol->getBlockFrequency(_layout, block);
	if (frequency != -1 && (int)blockNumber > frequency) {
		// blockNumber is out of range
		//FIXME: Log
		return NULL;
	}

	// Find the total offset for the field
	size_t blockSize = _protocol->getBlockSize(_layout, block);
	if (!blockSize) {
		//FIXME: Log
		return NULL;
	}

	int fieldOffset = _protocol->getFieldOffset(_layout, block, field);
	if (fieldOffset < 0) {
		//FIXME: Log
		return NULL;
	}
	//

	return (void*)(_buffer + _headerLength + blockSize * blockNumber + fieldOffset);
}

int Packet::setField(std::string block, size_t blockNumber, std::string field, void* value)
{
	if (!_layout) {
		//FIXME: Log
		return -1;
	}

	// Find out what type of field this is
	ll::llType type = getFieldType(block, field);

	// Check how many blocks this field can hold, and if blockNumber is in range
	int frequency = _protocol->getBlockFrequency(_layout, block);
	if (blockNumber <= 0 || (frequency != -1 && (int)blockNumber > frequency)) {
		// blockNumber is out of range
		//FIXME: Log
		return -2;
	}

	// Find the total offset for the field
	size_t blockSize = _protocol->getBlockSize(_layout, block);
	if (!blockSize) {
		//FIXME: Log
		return -3;
	}

	int fieldOffset = _protocol->getFieldOffset(_layout, block, field);
	if (fieldOffset < 0) {
		//FIXME: Log
		return -4;
	}
	
	size_t fieldSize = _protocol->getTypeSize(type);
	size_t offset = _headerLength + blockSize * (blockNumber - 1) + fieldOffset;

	// Reallocate memory if necessary
	if ((offset + fieldSize) > _length) {
		_buffer = (byte*)realloc(_buffer, offset + fieldSize);
		if (!_buffer) {
			//FIXME: Log memory error
			_length = 0;
			return -5;
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
			//FIXME: Log memory error
			_length = 0;
			return;
		}
	}

	memcpy(_buffer, buffer, length);
	_length = length;
}
