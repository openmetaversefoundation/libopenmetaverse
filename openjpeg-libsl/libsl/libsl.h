

#ifndef LIBSL_H
#define LIBSL_H

struct MarshalledImage
{
	unsigned char* encoded;
	int length;
	int dummy; // padding for 64-bit alignment

	unsigned char* decoded;
	int width;
	int height;
	int components;
};

#ifdef WIN32
#define DLLEXPORT extern "C" __declspec(dllexport)
#else
#define DLLEXPORT extern "C"
#endif

// uncompresed images are raw RGBA 8bit/channel
DLLEXPORT bool LibslEncode(MarshalledImage* image, bool lossless);
DLLEXPORT bool LibslDecode(MarshalledImage* image);
DLLEXPORT bool LibslAllocEncoded(MarshalledImage* image);
DLLEXPORT bool LibslAllocDecoded(MarshalledImage* image);
DLLEXPORT void LibslFree(MarshalledImage* image);


#endif
