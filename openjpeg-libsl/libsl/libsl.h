

#ifndef LIBSL_H
#define LIBSL_H


struct LibslImage
{
	unsigned char* encoded;
	int length;

	unsigned char* decoded;
	int width;
	int height;
	int components;
};

#ifdef WIN32
#define DLLEXPORT extern "C" __declspec(dllexport)
#else
#define DLLEXPORT
#define _stdcall __attribute__((__stdcall__))
#endif

// uncompresed images are raw RGBA 8bit/channel
DLLEXPORT bool _stdcall LibslEncode(LibslImage* image, bool lossless);
DLLEXPORT bool _stdcall LibslEncodeBake(LibslImage* image);
DLLEXPORT bool _stdcall LibslDecode(LibslImage* image);
DLLEXPORT bool _stdcall LibslAllocEncoded(LibslImage* image);
DLLEXPORT bool _stdcall LibslAllocDecoded(LibslImage* image);
DLLEXPORT void _stdcall LibslFree(LibslImage* image);


#endif
