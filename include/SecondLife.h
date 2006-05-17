#ifndef _SL_SECONDLIFE_
#define _SL_SECONDLIFE_

#include "includes.h"
#include "ProtocolManager.h"
#include "Network.h"
#include "Decoder.h"

class LIBSECONDLIFE_CLASS_DECL SecondLife
{
//protected:
public:
	ProtocolManager* _protocol;
	Network* _network;
	Decoder* _decoder;
//public:
	SecondLife();
	virtual ~SecondLife();

	// Pass-through functions to make life easier on the client
	int loadKeywords(std::string filename) { return _protocol->loadKeywords(filename); };
	int decryptCommFile(std::string source, std::string destination) { return _protocol->decryptCommFile(source, destination); };
	int buildProtocolMap(std::string filename) { return _protocol->buildProtocolMap(filename); };
};

#endif //_SL_SECONDLIFE_
