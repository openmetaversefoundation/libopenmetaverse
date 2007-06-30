

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

#define DLLEXPORT extern "C" __declspec(dllexport)

// uncompresed images are raw RGBA 8bit/channel
DLLEXPORT bool _stdcall LibslEncode(LibslImage* image);
DLLEXPORT bool _stdcall LibslDecode(LibslImage* image);
DLLEXPORT bool _stdcall LibslAllocEncoded(LibslImage* image);
DLLEXPORT bool _stdcall LibslAllocDecoded(LibslImage* image);
DLLEXPORT void _stdcall LibslFree(LibslImage* image);


#endif
