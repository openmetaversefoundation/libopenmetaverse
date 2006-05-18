#include "Packet.h"

Packet::Packet(std::string command, ProtocolManager* protocol, size_t length)
{
	_length = length;
	_protocol = protocol;

	_layout = _protocol->getCommand(command);
	if (!_layout) {
		//FIXME: Log invalid packet name
		_buffer = NULL;
		_length = 0;
	} else {
		_buffer = (byte*)malloc(_length ? _length : DEFAULT_PACKET_SIZE);

		if (!_buffer) {
			//FIXME: Log memory error
			_length = 0;
		}

		// Setup the flags and the frequency right now
		;
	}	
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
	unsigned short hostSequence = htons(sequence);

	if (_length < 4) {
		// Make room, assume the default packet size
		_buffer = (byte*)realloc(_buffer, DEFAULT_PACKET_SIZE);
		_length = 4;
	}

	if (!_buffer) {
		//FIXME: Log memory error
		_length = 0;
		return;
	}

	memcpy(_buffer + 2, &hostSequence, sizeof(sequence));
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

	return (void*)(_buffer + blockSize * blockNumber + fieldOffset);
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
	size_t offset = blockSize * (blockNumber - 1) + fieldOffset;

	// Reallocate memory if necessary
	if ((offset + fieldSize) > _length) {
		if ((offset + fieldSize) > DEFAULT_PACKET_SIZE) {
			_buffer = (byte*)realloc(_buffer, offset + fieldSize);
			if (!_buffer) {
				//FIXME: Log memory error
				_length = 0;
				return -5;
			}
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

boost::asio::ipv4::udp::endpoint Packet::getRemoteHost()
{
	return _remoteHost;
}

void Packet::setRemoteHost(boost::asio::ipv4::udp::endpoint remoteHost)
{
	_remoteHost = remoteHost;
}
