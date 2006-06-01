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

#ifndef _SL_INCLUDES_
#define _SL_INCLUDES_

#include <string>
#include <list>
#include <vector>
#include <map>
#include <sstream>
#include <fstream>
#include <stdlib.h>

// For debugging
#include <stdio.h>
#include <iostream>

#include <boost/thread/thread.hpp>
#include <boost/thread/xtime.hpp>
#include <boost/thread/mutex.hpp>
#include <boost/shared_ptr.hpp>
#include <boost/shared_array.hpp>
#include <boost/bind.hpp>
#include <boost/asio.hpp>
#include <boost/asio/ssl.hpp>
#include <boost/function.hpp>
#include <boost/any.hpp>

#include <netinet/in.h>


// Global defines
#define VERSION "libsecondlife 0.0.2"

#define MSG_APPENDED_ACKS	0x10
#define MSG_RESENT			0x20
#define MSG_RELIABLE		0x40
#define MSG_ZEROCODED		0x80
#define MSG_FREQ_HIGH		0x0000
#define MSG_FREQ_MED		0xFF00
#define MSG_FREQ_LOW		0xFFFF

//
#define byte unsigned char

// Global functions
std::string trim(std::string &s, const std::string &drop = " ");
int httoi(const char* value);
void hexstr2bin(const char* hex, byte* buf, size_t len);
std::string rpcGetString(char* buffer, const char* name);
int rpcGetU32(char* buffer, const char* name);
std::string packUUID(std::string uuid);
int zeroDecode(byte* src, int srclen, byte* dest);
int zeroEncode(byte* src, int srclen, byte* dest);

// Global logging facility
enum LogLevel {
	INFO = 0,
	WARNING,
	ERROR
};

void log(std::string line, LogLevel level);

// Platform-specific defines
#ifdef WIN32
	#pragma warning (disable : 4996 4251)

	#ifdef LIBSECONDLIFE_EXPORTS
		#define LIBSECONDLIFE_CLASS_DECL       __declspec(dllexport)
	#else
		#define LIBSECONDLIFE_CLASS_DECL       __declspec(dllimport)
	#endif //LIBSECONDLIFE_EXPORTS
#elif GCC4
	#ifdef LIBSECONDLIFE_EXPORTS
		#define LIBSECONDLIFE_CLASS_DECL       ((visibility("visible")))
	#else
		#define LIBSECONDLIFE_CLASS_DECL
	#endif //LIBSECONDLIFE_EXPORTS
#else
	#define LIBSECONDLIFE_CLASS_DECL
#endif //WIN32

#endif //_SL_INCLUDES_
