

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
#define _STDCALL 
#else
#define DLLEXPORT
#define _STDCALL
#endif

// uncompresed images are raw RGBA 8bit/channel
DLLEXPORT bool _STDCALL LibslEncode(LibslImage* image);
DLLEXPORT bool _STDCALL LibslDecode(LibslImage* image);
DLLEXPORT bool _STDCALL LibslAllocEncoded(LibslImage* image);
DLLEXPORT bool _STDCALL LibslAllocDecoded(LibslImage* image);
DLLEXPORT void _STDCALL LibslFree(LibslImage* image);


#endif
