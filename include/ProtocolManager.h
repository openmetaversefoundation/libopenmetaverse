/*
 * Copyright (c) 2006, Second Life Reverse Engineering Team
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without 
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Redistributions in binary form must reproduce the above copyright notice, 
 *   this list of conditions and the following disclaimer in the documentation 
 *   and/or other materials provided with the distribution.
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

#ifndef _SL_PROTOCOLMANAGER_
#define _SL_PROTOCOLMANAGER_

#include "includes.h"

namespace ll
{
	enum llType {
		INVALID_TYPE = -1,
		U8,
		U16,
		U32,
		U64,
		S8,
		S16,
		S32,
		S64,
		F8,
		F16,
		F32,
		F64,
		LLUUID,
		BOOL,
		LLVector3,
		LLVector3d,
		LLQuaternion,
		IPADDR,
		IPPORT,
		Variable,
		Fixed,
		Single,
		Multiple
	};

	enum frequency {
		Invalid,
		Low,
		Medium,
		High
	};
};

typedef struct llVector3 {
	float x;
	float y;
	float z;
} llVector3;

typedef struct llVector3d {
	double x;
	double y;
	double z;
} llVector3d;

typedef struct llQuaternion {
	float x;
	float y;
	float z;
	float s;
} llQuaternion;

struct packetField {
	int keywordPosition;
	std::string name;
	ll::llType type;
};

struct packetBlock {
	int keywordPosition;
	std::string name;
	int frequency;
	std::list<packetField*> fields;
};

namespace std
{
	template<> struct greater<packetField*> {
		bool operator()(packetField const* p1, packetField const* p2) {
			if(!p1) return true;
			if(!p2) return false;
			return p1->keywordPosition < p2->keywordPosition;
		}
	};

	template<> struct greater<packetBlock*> {
		bool operator()(packetBlock const* p1, packetBlock const* p2) {
			if(!p1) return true;
			if(!p2) return false;
			return p1->keywordPosition < p2->keywordPosition;
		}
	};
};

typedef struct packetDiagram {
	unsigned short id;
	std::string name;
	ll::frequency frequency;
	bool trusted;
	bool encoded;
	std::list<packetBlock*> blocks;
} packetDiagram;


class ProtocolManager
{
protected:
	std::map<std::string, int> _keywordMap;

	// At some point these should become maps from command names to packetDiagram*s
	packetDiagram _lowPackets[65536];
	packetDiagram _mediumPackets[256];
	packetDiagram _highPackets[256];

	bool getFields(packetBlock* block, std::string protocolMap, size_t start, size_t end);
	bool getBlocks(packetDiagram* packet, std::string protocolMap, size_t start, size_t end);
public:
	ProtocolManager();
	virtual ~ProtocolManager();

	void printMap();

	int loadKeywords(std::string filename);
	int decryptCommFile(std::string source, std::string destination);
	int buildProtocolMap(std::string filename);

	int getKeywordPosition(std::string keyword);

	packetDiagram* getCommand(std::string command);
	ll::llType getFieldType(std::string type);
	static int getTypeSize(ll::llType type);
	std::string getTypeName(ll::llType type);
	int getBlockFrequency(packetDiagram* layout, std::string block);
	size_t getBlockSize(packetDiagram* layout, std::string block);
	int getFieldOffset(packetDiagram* layout, std::string block, std::string field);
};

#endif //_SL_PROTOCOLMANAGER_
