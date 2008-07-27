

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
DLLEXPORT bool DotNetEncode(MarshalledImage* image, bool lossless);
DLLEXPORT bool DotNetDecode(MarshalledImage* image);
DLLEXPORT bool DotNetAllocEncoded(MarshalledImage* image);
DLLEXPORT bool DotNetAllocDecoded(MarshalledImage* image);
DLLEXPORT void DotNetFree(MarshalledImage* image);


#endif
