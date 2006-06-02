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

#ifndef _SL_PACKET_
#define _SL_PACKET_

#include "includes.h"
#include "Fields.h"
#include "ProtocolManager.h"

// Forward definitions for the smart pointers
class PacketField;
class PacketBlock;
class Packet;

// Smart pointers for the packet-related classes
typedef boost::shared_ptr<Packet> PacketPtr;
typedef boost::shared_ptr<PacketBlock> PacketBlockPtr;
typedef boost::shared_ptr<PacketField> PacketFieldPtr;

// Smart pointer lists
typedef std::vector<PacketBlockPtr> BlockList;
typedef std::vector<PacketFieldPtr> FieldList;

//
class PacketField
{
public:
	packetField* layout;
	byte* data;
	size_t length;

	PacketField(packetField* _layout, byte* _data, size_t _length)
	{ layout = _layout; data = _data; length = _length; };

	std::string name() { return layout->name; };
	types::Type type() { return layout->type; };
};

//
class PacketBlock
{
public:
	packetBlock* layout;
	FieldList fields;

	PacketBlock(packetBlock* _layout) { layout = _layout; };

	std::string name() { return layout->name; };
};

//
class LIBSECONDLIFE_CLASS_DECL Packet
{
protected:
	byte* _buffer;
	size_t _length;
	frequencies::Frequency _frequency;
	std::string _command;
	ProtocolManager* _protocol;
	packetDiagram* _layout;

public:
	//std::vector<BlockContainerPtr> blockContainers;

	Packet(std::string command, ProtocolManager* protocol);
	Packet(byte* buffer, size_t length, ProtocolManager* protocol);
	~Packet() { free(_buffer); };

	void payload(byte* payload, size_t payloadLength);

	byte* buffer() { return _buffer; };
	size_t length() { return _length; };

	std::string name() { return _command; };
	std::string command() { return _command; };
	packetDiagram* layout() { return _layout; };
	frequencies::Frequency frequency() { return _frequency; };
	unsigned short flags();
	void flags(unsigned short flags);

	unsigned short sequence();
	void sequence(unsigned short sequence);

	size_t headerLength();

	boost::any getField(std::string blockName, std::string fieldName);
	boost::any getField(std::string blockName, size_t blockNumber, std::string fieldName);
	PacketBlockPtr getBlock(std::string blockName);
	PacketBlockPtr getBlock(std::string blockName, size_t blockNumber);
	BlockList getBlocks();
};

#endif //_SL_PACKET_
