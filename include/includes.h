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
