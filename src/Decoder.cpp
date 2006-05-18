#include "Decoder.h"

Decoder::Decoder()
{
	_network = NULL;
}

Decoder::Decoder(Network* network)
{
	_network = network;
}

Decoder::~Decoder()
{
	;
}

int Decoder::decodePackets()
{
	if (!_network || !_network->inbox().size()) {
		//FIXME: Log
		return -1;
	}

	

	return 0;	
}
