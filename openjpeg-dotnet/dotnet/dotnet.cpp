// This is the main DLL file.

#include "dotnet.h"
extern "C" {
#include "../libopenjpeg/openjpeg.h"
}
#include <algorithm>

bool LibomvFunc(DotNetAllocEncoded)(MarshalledImage* image)
{
	LibomvFunc(DotNetFree)(image);

	try
	{
		image->encoded = new unsigned char[image->length];
		image->decoded = 0;
	}
	catch (...)
	{
		return false;
	}

	return true;
}

bool LibomvFunc(DotNetAllocDecoded)(MarshalledImage* image)
{
	LibomvFunc(DotNetFree)(image);

	try
	{
		image->decoded = new unsigned char[image->width * image->height * image->components];
		image->encoded = 0;
	}
	catch (...)
	{
		return false;
	}

	return true;
}

void LibomvFunc(DotNetFree)(MarshalledImage* image)
{
	if (image->encoded != 0) delete[] image->encoded;
	if (image->decoded != 0) delete[] image->decoded;
}


bool LibomvFunc(DotNetEncode)(MarshalledImage* image, bool lossless)
{
	try
	{
		opj_cparameters cparameters;
		opj_set_default_encoder_parameters(&cparameters);
		cparameters.cp_disto_alloc = 1;

		if (lossless)
		{
			cparameters.tcp_numlayers = 1;
			cparameters.tcp_rates[0] = 0;
		}
		else
		{
			cparameters.tcp_numlayers = 5;
			cparameters.tcp_rates[0] = 1920;
			cparameters.tcp_rates[1] = 480;
			cparameters.tcp_rates[2] = 120;
			cparameters.tcp_rates[3] = 30;
			cparameters.tcp_rates[4] = 10;
			cparameters.irreversible = 1;
			if (image->components >= 3)
			{
				cparameters.tcp_mct = 1;
			}
		}

		cparameters.cp_comment = (char*)"";

		opj_image_comptparm comptparm[5];

		for (int i = 0; i < image->components; i++)
		{
			comptparm[i].bpp = 8;
			comptparm[i].prec = 8;
			comptparm[i].sgnd = 0;
			comptparm[i].dx = 1;
			comptparm[i].dy = 1;
			comptparm[i].x0 = 0;
			comptparm[i].y0 = 0;
			comptparm[i].w = image->width;
			comptparm[i].h = image->height;
		}

		opj_image_t* jp2_image = opj_image_create(image->components, comptparm, CLRSPC_SRGB);
		if (jp2_image == NULL)
			throw "opj_image_create failed";

		jp2_image->x0 = 0;
		jp2_image->y0 = 0;
		jp2_image->x1 = image->width;
		jp2_image->y1 = image->height;
		int n = image->width * image->height;
		
		for (int i = 0; i < image->components; i++)
			std::copy(image->decoded + i * n, image->decoded + (i + 1) * n, jp2_image->comps[i].data);
		
		opj_cinfo* cinfo = opj_create_compress(CODEC_J2K);
		opj_setup_encoder(cinfo, &cparameters, jp2_image);
		opj_cio* cio = opj_cio_open((opj_common_ptr)cinfo, NULL, 0);
		if (cio == NULL)
			throw "opj_cio_open failed";

		if (!opj_encode(cinfo, cio, jp2_image, cparameters.index))
			return false;

		image->length = cio_tell(cio);
		image->encoded = new unsigned char[image->length];
		std::copy(cio->buffer, cio->buffer + image->length, image->encoded);
		
		opj_image_destroy(jp2_image);
		opj_destroy_compress(cinfo);
		opj_cio_close(cio);

		return true;
	}

	catch (...)
	{
		return false;
	}
}

bool LibomvFunc(DotNetDecode)(MarshalledImage* image)
{
	opj_dparameters dparameters;
	
	try
	{
		opj_set_default_decoder_parameters(&dparameters);
		opj_dinfo_t* dinfo = opj_create_decompress(CODEC_J2K);
		opj_setup_decoder(dinfo, &dparameters);
		opj_cio* cio = opj_cio_open((opj_common_ptr)dinfo, image->encoded, image->length);

		opj_image* jp2_image = opj_decode(dinfo, cio); // decode happens here
		if (jp2_image == NULL)
			throw "opj_decode failed";

		image->width = jp2_image->x1 - jp2_image->x0;
		image->height = jp2_image->y1 - jp2_image->y0;
		image->components = jp2_image->numcomps;
		int n = image->width * image->height;
		image->decoded = new unsigned char[n * image->components];
		
		for (int i = 0; i < image->components; i++)
			std::copy(jp2_image->comps[i].data, jp2_image->comps[i].data + n, image->decoded + i * n);

		opj_image_destroy(jp2_image);
		opj_destroy_decompress(dinfo);
		opj_cio_close(cio);

		return true;
	}
	catch (...)
	{
		return false;
	}
}

bool LibomvFunc(DotNetDecodeWithInfo)(MarshalledImage* image)
{
	opj_dparameters dparameters;
	opj_codestream_info_t info;
	
	try
	{
		opj_set_default_decoder_parameters(&dparameters);
		opj_dinfo_t* dinfo = opj_create_decompress(CODEC_J2K);
		opj_setup_decoder(dinfo, &dparameters);
		opj_cio* cio = opj_cio_open((opj_common_ptr)dinfo, image->encoded, image->length);

		opj_image* jp2_image = opj_decode_with_info(dinfo, cio, &info); // decode happens here
		if (jp2_image == NULL)
			throw "opj_decode failed";

		// maximum number of decompositions
		int max_numdecompos = 0;
		for (int compno = 0; compno < info.numcomps; compno++)
		{
			if (max_numdecompos < info.numdecompos[compno])
				max_numdecompos = info.numdecompos[compno];
		}

		image->width = jp2_image->x1 - jp2_image->x0;
		image->height = jp2_image->y1 - jp2_image->y0;
		image->layers = info.numlayers;
		image->resolutions = max_numdecompos + 1;
		image->components = info.numcomps;
		image->packet_count = info.packno;
		image->packets = info.tile->packet;
		int n = image->width * image->height;
		image->decoded = new unsigned char[n * image->components];
		
		for (int i = 0; i < image->components; i++)
			std::copy(jp2_image->comps[i].data, jp2_image->comps[i].data + n, image->decoded + i * n);

		opj_image_destroy(jp2_image);
		opj_destroy_decompress(dinfo);
		opj_cio_close(cio);

		return true;
	}
	catch (...)
	{
		return false;
	}
}
