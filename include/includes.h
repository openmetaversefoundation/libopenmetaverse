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
#include <boost/thread/mutex.hpp>
#include <boost/bind.hpp>
#include <boost/asio.hpp>
#include <boost/asio/ssl.hpp>

int httoi(const char* value);

#define SL_BUFFER_SIZE 8192

// Data types
#define byte unsigned char

struct LLUUID {
	byte data[16];
	LLUUID operator=(const int p) { for (size_t i = 0; i < 16; i++) { data[i] = (byte)p; } return *this; };
	// This needs to loop through the string and convert each letter to a byte using (byte)httoi()
	//LLUUID operator=(const std::string p) { memcpy(data, p.c_str(), 16); return *this; };
};

typedef unsigned int U32;
//

#ifdef WIN32
#pragma warning (disable : 4996 4251)
#ifdef LIBSECONDLIFE_EXPORTS
	#define LIBSECONDLIFE_CLASS_DECL       __declspec(dllexport)
#else
	#define LIBSECONDLIFE_CLASS_DECL       __declspec(dllimport)
#endif
#else
#define LIBSECONDLIFE_CLASS_DECL
#endif

#endif //_SL_INCLUDES_
