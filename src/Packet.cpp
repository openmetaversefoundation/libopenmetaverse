#include "Packet.h"

Packet::Packet()
{
	_buffer = (byte*)malloc(DEFAULT_PACKET_SIZE);
	if (!_buffer) {
		//FIXME: Log memory error
	}

	_length = 0;
	_protocol = NULL;
}

Packet::Packet(ProtocolManager* protocol, size_t length)
{
	_buffer = (byte*)malloc(length ? length : DEFAULT_PACKET_SIZE);
	if (!_buffer) {
		//FIXME: Log memory error
		_length = 0;
	} else {
		_length = length;
	}

	_protocol = protocol;
}

Packet::~Packet()
{
	free(_buffer);
}

bool Packet::setCommand(std::string command)
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

int Packet::getRawData(byte* buffer, size_t length)
{
	return (memcpy((void*)buffer, _buffer, length) != NULL);
}

byte* Packet::getRawDataPtr()
{
	return _buffer;
}

void Packet::setRawData(byte* buffer, size_t length)
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
