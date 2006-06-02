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

#ifndef _SL_FIELDS_
#define _SL_FIELDS_

#include "includes.h"

namespace types
{
	enum Type {
		Invalid = -1,
		U8,
		U16,
		U32,
		U64,
		S8,
		S16,
		S32,
		S64,
		F32,
		F64,
		LLUUID,
		Bool,
		LLVector3,
		LLVector3d,
		LLVector4,
		LLQuaternion,
		IPADDR,
		IPPORT,
		Variable,
		Fixed,
		Single,
		Multiple
	};
};

namespace frequencies
{
	enum Frequency {
		Invalid = 0,
		Low,
		Medium,
		High
	};
};

// This will be phased out in time
struct SimpleLLUUID {
	byte data[16];
	SimpleLLUUID() { *this = 0; };
	SimpleLLUUID(std::string p) { hexstr2bin(packUUID(p).c_str(), data, 16); };
	SimpleLLUUID(const int p) { for (size_t i = 0; i < 16; i++) { data[i] = (byte)p; } };
	SimpleLLUUID operator=(const int p) { for (size_t i = 0; i < 16; i++) { data[i] = (byte)p; } return *this; };
	SimpleLLUUID operator=(SimpleLLUUID p) { memcpy(data, p.data, 16); return *this; };
	SimpleLLUUID operator=(std::string p) { hexstr2bin(packUUID(p).c_str(), data, 16); return *this; };
};

/*
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

#define llVector4 llQuaternion
*/

// Use a void pointer for the field since it is of varying type
typedef boost::shared_ptr<void> FieldPtr;

class Field
{
public:
	types::Type type;

	Field() { type = types::Invalid; };
};

class U8 : public Field
{
public:
	byte data;

	U8(byte* _data) { type = types::U8; data = *(byte*)_data; };
};

class U16 : Field
{
public:
	unsigned short data;

	U16(byte* _data) { type = types::U16; data = *(unsigned short*)_data; };
};

class U32 : Field
{
public:
	unsigned int data;

	U32(byte* _data) { type = types::U32; data = *(unsigned int*)_data; };
};

class U64_ : Field
{
public:
	unsigned long long data:64;

	U64_(byte* _data) { type = types::U64; data = *(unsigned long long int*)_data; };
};

class S8 : Field
{
public:
	char data;

	S8(byte* _data) { type = types::S8; data = *(char*)_data; };
};

class S16 : Field
{
public:
	short data;

	S16(byte* _data) { type = types::S16; data = *(short*)_data; };
};

class S32 : Field
{
public:
	int data;

	S32(byte* _data) { type = types::S32; data = *(int*)_data; };
};

class S64 : Field
{
public:
	long long int data:64;

	S64(byte* _data) { type = types::S64; data = *(long long int*)_data; };
};

class F32 : Field
{
public:
	float data;

	F32(byte* _data) { type = types::F32; data = *(float*)_data; };
};

class F64 : Field
{
public:
	double data;

	F64(byte* _data) { type = types::F64; data = *(double*)_data; };
};

class LLUUID : Field
{
public:
	byte data[16];

	LLUUID(byte* _data) { type = types::LLUUID; memcpy(data, _data, 16); };
};

class Bool : Field
{
public:
	bool data;

	Bool(byte* _data) { type = types::Bool; data = *(bool*)_data; };
};

class LLVector3 : Field
{
public:
	float x;
	float y;
	float z;

	LLVector3(byte* _data)
	{
		type = types::LLVector3;
		x = *(float*)(_data);
		y = *(float*)(_data + sizeof(float));
		z = *(float*)(_data + sizeof(float) + sizeof(float));
	};
};

class LLVector3d : Field
{
public:
	double x;
	double y;
	double z;

	LLVector3d(byte* _data)
	{
		type = types::LLVector3d;
		x = *(double*)(_data);
		y = *(double*)(_data + sizeof(double));
		z = *(double*)(_data + sizeof(double) + sizeof(double));
	};
};

class LLVector4 : Field
{
public:
	float x;
	float y;
	float z;
	float s;

	LLVector4(byte* _data)
	{
		type = types::LLVector4;
		x = *(float*)(_data);
		y = *(float*)(_data + sizeof(float));
		z = *(float*)(_data + sizeof(float) + sizeof(float));
		s = *(float*)(_data + sizeof(float) + sizeof(float) + sizeof(float));
	};
};

class LLQuaternion : Field
{
public:
	float x;
	float y;
	float z;
	float s;

	LLQuaternion(byte* _data)
	{
		type = types::LLQuaternion;
		x = *(float*)(_data);
		y = *(float*)(_data + sizeof(float));
		z = *(float*)(_data + sizeof(float) + sizeof(float));
		s = *(float*)(_data + sizeof(float) + sizeof(float) + sizeof(float));
	};
};

class IPADDR : Field
{
public:
	int data;

	IPADDR(byte* _data) { type = types::IPADDR; data = *(int*)_data; };
};

class IPPORT : Field
{
public:
	unsigned short data;

	IPPORT(byte* _data) { type = types::IPPORT; data = *(unsigned short*)_data; };
};

class Variable : Field
{
public:
	byte length;
	byte* data;

	Variable(byte* _data, byte _length = 0)
	{
		type = types::Variable;
		length = _length;
		data = (byte*)malloc(length);
		memcpy(data, _data, length);
	};
	~Variable() { free(data); };

	std::string dataString() { return std::string((char*)data); };
};

#endif //_SL_FIELDS_
