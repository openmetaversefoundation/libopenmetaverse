#include "SecondLife.h"

SecondLife::SecondLife()
{
	boost::thread _placeHolder;
	_protocol = new ProtocolManager();
	_network = new Network(_protocol);
	_decoder = new Decoder();
}

SecondLife::~SecondLife()
{
	delete _protocol;
	delete _network;
	delete _decoder;
}
