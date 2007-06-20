using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace OpenJPEGNet
{
    public class OpenJPEG
    {
        public const string OPENJPEG_VERSION = "1.0.0";
        public const int TRUE = 1;
        public const int FALSE = 0;
        public const int MAX_PATH = 260;
        public const int J2K_MAXRLVLS = 33;
        public const int J2K_MAXBANDS = 3 * J2K_MAXRLVLS - 2;
        public const int TGA_HEADER_SIZE = 32;

        public enum OPJ_PROG_ORDER
        {
            PROG_UNKNOWN = -1,
            LRCP = 0,
            RLCP = 1,
            RPCL = 2,
            PCRL = 3,
            CPRL = 4
        };

        public enum OPJ_COLOR_SPACE
        {
            CLRSPC_UNKNOWN = -1,
            CLRSPC_SRGB = 1,
            CLRSPC_GRAY = 2,
            CLRSPC_SYCC = 3
        };

        public enum OPJ_CODEC_FORMAT
        {
            CODEC_UNKNOWN = -1,
            CODEC_J2K = 0,
            CODEC_JPT = 1,
            CODEC_JP2 = 2
        };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void opj_msg_callback(string msg, IntPtr client_data);

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public unsafe struct opj_event_mgr_t
        {
            public opj_msg_callback error_handler;
            public opj_msg_callback warning_handler;
            public opj_msg_callback info_handler;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public unsafe struct opj_poc_t
        {
            public int resno0;
            public int compno0;
            public int layno1;
            public int resno1;
            public int compno1;
            public OPJ_PROG_ORDER prg;
            public int tile;
            public fixed char progorder[4];
        };

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public unsafe struct opj_cparameters_t
        {
            public bool tile_size_on;
            public int cp_tx0;
            public int cp_ty0;
            public int cp_tdx;
            public int cp_tdy;
            public int cp_disto_alloc;
            public int cp_fixed_alloc;
            public int cp_fixed_quality;
            public int* cp_matrice;
            public string cp_comment;
            public int csty;
            public OPJ_PROG_ORDER prog_order;
            //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            //public opj_poc_t[] POC;
            public fixed int POC[256];
            public int numpocs;
            public int tcp_numlayers;
            public fixed int tcp_rates[100];
            public fixed float tcp_distoratio[100];
            public int numresolution;
            public int cblockw_init;
            public int cblockh_init;
            public int mode;
            public int irreversible;
            public int roi_compno;
            public int roi_shift;
            public int res_spec;
            public fixed int prcw_init[J2K_MAXRLVLS];
            public fixed int prch_init[J2K_MAXRLVLS];
            public fixed char infile[MAX_PATH];
            public fixed char outfile[MAX_PATH];
            public int index_on;
            public fixed char index[MAX_PATH];
            public int image_offset_x0;
            public int image_offset_y0;
            public int subsampling_dx;
            public int subsampling_dy;
            public int decod_format;
            public int cod_format;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public unsafe struct opj_dparameters_t
        {
            public int cp_reduce;
            public int cp_layer;
            public fixed char infile[MAX_PATH];
            public fixed char outfile[MAX_PATH];
            public int decod_format;
            public int cod_format;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public unsafe struct opj_common_struct_t
        {
            void* event_mgr; //opj_event_mgr_t*
            void* client_data;
            bool is_decompressor;
            OPJ_CODEC_FORMAT codec_format;
            void* j2k_handle;
            void* jp2_handle;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public unsafe struct opj_cinfo_t
        {
            void* event_mgr; //opj_event_mgr_t*
            void* client_data;
            int is_decompressor;
            int codec_format;
            void* j2k_handle;
            void* jp2_handle;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public unsafe struct opj_dinfo_t
        {
            void* event_mgr; //opj_event_mgr_t*
            void* client_data;
            bool is_decompressor;
            OPJ_CODEC_FORMAT codec_format;
            void* j2k_handle;
            void* jp2_handle;
        };

        public const int OPJ_STREAM_READ = 0x0001;
        public const int OPJ_STREAM_WRITE = 0x0002;

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public unsafe struct opj_cio_t
        {
            public IntPtr cinfo;
            public int openmode;
            public byte* buffer;
            public int length;
            public byte* start;
            public byte* end;
            public byte* bp;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public unsafe struct opj_image_comp_t
        {
            public int dx;
            public int dy;
            public int w;
            public int h;
            public int x0;
            public int y0;
            public int prec;
            public int bpp;
            public int sgnd;
            public int resno_decoded;
            public int factor;
            public int* data;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public unsafe struct opj_image_t
        {
            public int x0;
            public int y0;
            public int x1;
            public int y1;
            public int numcomps;
            public OPJ_COLOR_SPACE color_space;
            public opj_image_comp_t* comps;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct opj_image_cmptparm_t
        {
            public int dx;
            public int dy;
            public int w;
            public int h;
            public int x0;
            public int y0;
            public int prec;
            public int bpp;
            public int sgnd;
        }

        [DllImport("openjpeg.dll", CharSet = CharSet.Ansi)]
        public static extern string opj_version();

        [DllImport("openjpeg.dll")]
        public unsafe static extern opj_image_t* opj_image_create(int numcmpts, opj_image_cmptparm_t[] cmptparms, OPJ_COLOR_SPACE clrspc);

        [DllImport("openjpeg.dll")]
        public unsafe static extern void opj_image_destroy(opj_image_t* image);

        [DllImport("openjpeg.dll")]
        // TODO: Can we get rid of this MarshalAs?
        public unsafe static extern opj_cio_t* opj_cio_open(void* cio, [MarshalAs(UnmanagedType.LPArray)]byte[] buffer, int length);

        [DllImport("openjpeg.dll")]
        public unsafe static extern void opj_cio_close(opj_cio_t* cio);

        [DllImport("openjpeg.dll")]
        public unsafe static extern int cio_tell(opj_cio_t* cio);

        [DllImport("openjpeg.dll")]
        public unsafe static extern void cio_seek(opj_cio_t* cio, int pos);

        //[DllImport("openjpeg.dll")]
        // opj_event_mgr_t*
        //public unsafe static extern opj_event_mgr_t* opj_set_event_mgr(opj_common_struct_t* cinfo, opj_event_mgr_t* event_mgr, void* context);

        [DllImport("openjpeg.dll", EntryPoint = "opj_create_decompress")]
        // opj_dinfo_t*
        public static extern IntPtr opj_create_decompress(OPJ_CODEC_FORMAT format);

        [DllImport("openjpeg.dll")]
        public static extern void opj_destroy_decompress(IntPtr dinfo);

        [DllImport("openjpeg.dll")]
        public static extern void opj_set_default_decoder_parameters(ref opj_dparameters_t parameters);

        [DllImport("openjpeg.dll")]
        public static extern void opj_setup_decoder(ref opj_dinfo_t dinfo, ref opj_dparameters_t parameters);

        [DllImport("openjpeg.dll")]
        // opj_image_t*
        public static extern IntPtr opj_decode(ref opj_dinfo_t dinfo, ref opj_cio_t cio);

        [DllImport("openjpeg.dll")]
        // opj_cinfo_t*
        public unsafe static extern opj_cinfo_t* opj_create_compress(OPJ_CODEC_FORMAT format);

        [DllImport("openjpeg.dll")]
        public unsafe static extern void opj_destroy_compress(opj_cinfo_t* cinfo);

        [DllImport("openjpeg.dll")]
        public static extern void opj_set_default_encoder_parameters(IntPtr parameters);

        [DllImport("openjpeg.dll")]
        public unsafe static extern void opj_setup_encoder(opj_cinfo_t* cinfo, IntPtr parameters, opj_image_t* image);

        [DllImport("openjpeg.dll")]
        public unsafe static extern bool opj_encode(opj_cinfo_t* cinfo, opj_cio_t* cio, opj_image_t* image, char* index);


        ///////////////////////////////////

        /// <summary>
        /// Decodes a byte array containing JPEG2000 data using the J2K codec
        /// in to a byte array containing a Targa file
        /// </summary>
        /// <param name="j2cdata">Byte array containing JPEG2000 data using the
        /// J2K codec</param>
        /// <returns>Byte array containing a Targa file</returns>
        public unsafe static byte[] DecodeToTGA(byte[] j2kdata)
        {
            byte[] output;

            try
            {

                opj_dparameters_t parameters = new opj_dparameters_t();
                opj_image_t image;

                opj_dinfo_t dinfo;
                opj_cio_t cio;

                // TODO: configure the event callbacks

                // setup the decoding parameters
                opj_set_default_decoder_parameters(ref parameters);

                // get a decoder handle
                IntPtr dinfo_ptr = opj_create_decompress(OPJ_CODEC_FORMAT.CODEC_J2K);
                dinfo = (opj_dinfo_t)Marshal.PtrToStructure(dinfo_ptr, typeof(opj_dinfo_t));

                // TODO: setup the callbacks

                // setup the decoder
                opj_setup_decoder(ref dinfo, ref parameters);

                // open a byte stream
                IntPtr cio_ptr = (IntPtr)opj_cio_open((opj_common_struct_t*)dinfo_ptr, j2kdata, j2kdata.Length);
                cio = (opj_cio_t)Marshal.PtrToStructure(cio_ptr, typeof(opj_cio_t));

                // decode
                IntPtr image_ptr = opj_decode(ref dinfo, ref cio);
                image = (opj_image_t)Marshal.PtrToStructure(image_ptr, typeof(opj_image_t));

                int width = image.x1 - image.x0;
                int height = image.y1 - image.y0;
                int components = image.numcomps;

                // create the targa file in memory
                output = new byte[width * height * 4 + TGA_HEADER_SIZE];

                int offset = 0;
                output[offset++] = 0; // idlength
                output[offset++] = 0; // colormaptype = 0: no colormap
                output[offset++] = 2; // image type = 2: uncompressed RGB
                output[offset++] = 0; // color map spec is five zeroes for no color map
                output[offset++] = 0; // color map spec is five zeroes for no color map
                output[offset++] = 0; // color map spec is five zeroes for no color map
                output[offset++] = 0; // color map spec is five zeroes for no color map
                output[offset++] = 0; // color map spec is five zeroes for no color map
                output[offset++] = 0; // x origin = two bytes
                output[offset++] = 0; // x origin = two bytes
                output[offset++] = 0; // y origin = two bytes
                output[offset++] = 0; // y origin = two bytes
                output[offset++] = (byte)(width & 0xFF); // width - low byte
                output[offset++] = (byte)(width >> 8); // width - hi byte
                output[offset++] = (byte)(height & 0xFF); // height - low byte
                output[offset++] = (byte)(height >> 8); // height - hi byte
                output[offset++] = 32; // 32 bits per pixel
                output[offset++] = 40; // image descriptor byte

                switch (image.numcomps)
                {
                    case 5:
                    // We can't represent the fifth 
                    case 4:
                        for (int i = 0; i < (width * height); i++)
                        {
                            output[offset++] = (byte)image.comps[2].data[i]; // red
                            output[offset++] = (byte)image.comps[1].data[i]; // green
                            output[offset++] = (byte)image.comps[0].data[i]; // blue
                            output[offset++] = (byte)image.comps[3].data[i]; // alpha
                        }
                        break;
                    case 3:
                        for (int i = 0; i < (width * height); i++)
                        {
                            output[offset++] = (byte)image.comps[2].data[i]; // red
                            output[offset++] = (byte)image.comps[1].data[i]; // green
                            output[offset++] = (byte)image.comps[0].data[i]; // blue
                            output[offset++] = 0xFF; // alpha
                        }
                        break;
                    default:
                        Console.WriteLine("Unhandled numcomps: " + image.numcomps);
                        return null;
                }

                File.WriteAllBytes("out.tga", output);

                opj_cio_close((opj_cio_t*)cio_ptr);
                opj_destroy_decompress(dinfo_ptr);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }

            return output;
        }

        /// <summary>
        /// Decodes a byte array containing JPEG2000 data using the J2K codec
        /// in to a GDI+ Image object
        /// </summary>
        /// <param name="j2cdata"></param>
        /// <returns></returns>
        public static Image DecodeToImage(byte[] j2kdata)
        {
            return LoadTGAClass.LoadTGA(new MemoryStream(DecodeToTGA(j2kdata)));
        }

        public unsafe static byte[] EncodeFromImage(Bitmap bitmap, string comment)
        {
            byte[] output = null;

            try
            {
                const int MAX_COMPS = 4;

                // setup the parameters
                IntPtr parameters_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(opj_cparameters_t)));
                opj_set_default_encoder_parameters(parameters_ptr);

                opj_cparameters_t parameters = (opj_cparameters_t)Marshal.PtrToStructure(parameters_ptr, typeof(opj_cparameters_t));

                parameters.tcp_rates[0] = 0;
                parameters.tcp_numlayers++;
                parameters.cp_disto_alloc = 1;
                parameters.cod_format = 0;
                parameters.subsampling_dx = 1;
                parameters.subsampling_dy = 1;
                parameters.cp_comment = comment;

                OPJ_COLOR_SPACE color_space = OPJ_COLOR_SPACE.CLRSPC_SRGB;
                opj_image_cmptparm_t[] cmptparm = new opj_image_cmptparm_t[MAX_COMPS];
                int width = bitmap.Width;
                int height = bitmap.Height;

                for (int c = 0; c < MAX_COMPS; c++)
                {
                    cmptparm[c] = new opj_image_cmptparm_t();

                    cmptparm[c].prec = 8;
                    cmptparm[c].bpp = 8;
                    cmptparm[c].sgnd = 0;
                    cmptparm[c].dx = parameters.subsampling_dx;
                    cmptparm[c].dy = parameters.subsampling_dy;
                    cmptparm[c].w = width;
                    cmptparm[c].h = height;
                }

                // create the image
                int i = 0;
                int numcomps;
                PixelFormat format = bitmap.PixelFormat;

                if ((format & PixelFormat.Alpha) != 0 || (format & PixelFormat.PAlpha) != 0)
                    numcomps = 4;
                else if (format == PixelFormat.Format16bppGrayScale)
                    numcomps = 1;
                else
                    numcomps = 3;

                opj_image_t* image_ptr = opj_image_create(numcomps, cmptparm, color_space);
                image_ptr->x1 = width;
                image_ptr->y1 = height;

                BitmapData data;

                // Build the raw image buffer for openjpeg to read
                if ((format & PixelFormat.Alpha) != 0 || (format & PixelFormat.PAlpha) != 0)
                {
                    // four layers, RGBA
                    data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly,
                        PixelFormat.Format32bppArgb);

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            byte* pixel = (byte*)data.Scan0;
                            pixel += (y * width + x) * numcomps;

                            // GDI+ gives us BGRA and we need to turn that in to RGBA
                            (*image_ptr).comps[0].data[i] = *(pixel + 2);
                            (*image_ptr).comps[1].data[i] = *(pixel + 1);
                            (*image_ptr).comps[2].data[i] = *(pixel);
                            (*image_ptr).comps[3].data[i] = *(pixel + 3);

                            pixel += 4;
                            i++;
                        }
                    }
                }
                else if (format == PixelFormat.Format16bppGrayScale)
                {
                    // one layer
                    data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly,
                        PixelFormat.Format16bppGrayScale);

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            byte* pixel = (byte*)data.Scan0;
                            pixel += (y * width + x) * numcomps;

                            // turn 16 bit data in to 8 bit data (TODO: Does this work?)
                            (*image_ptr).comps[0].data[i] = *(pixel);

                            pixel += 2;
                            i++;
                        }
                    }
                }
                else
                {
                    // three layers, RGB
                    data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly,
                        PixelFormat.Format24bppRgb);

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            byte* pixel = (byte*)data.Scan0;
                            pixel += (y * width + x) * numcomps;

                            for (int c = numcomps - 1; c >= 0; c--)
                            {
                                (*image_ptr).comps[c].data[i] = *pixel;
                                pixel++;
                            }
                            i++;
                        }
                    }
                }

                bitmap.UnlockBits(data);

                // get a J2K compressor handle
                opj_cinfo_t* cinfo_ptr = opj_create_compress(OPJ_CODEC_FORMAT.CODEC_J2K);

                // TODO: setup the callbacks

                // setup the encoder parameters
                Marshal.StructureToPtr(parameters, parameters_ptr, true);
                opj_setup_encoder(cinfo_ptr, parameters_ptr, image_ptr);

                // open a byte stream for writing
                opj_cio_t* cio_ptr = opj_cio_open((void*)cinfo_ptr, null, 0);

                // encode the image
                bool success = opj_encode(cinfo_ptr, cio_ptr, image_ptr, null);
                if (!success)
                {
                    opj_cio_close(cio_ptr);
                    return null;
                }

                int codestream_length = cio_tell(cio_ptr);

                output = new byte[codestream_length];
                Marshal.Copy((IntPtr)(*cio_ptr).buffer, output, 0, codestream_length);

                opj_cio_close(cio_ptr);
                opj_destroy_compress(cinfo_ptr);
                opj_image_destroy(image_ptr);
                Marshal.FreeHGlobal(parameters_ptr);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }

            return output;
        }

        /// <summary>
        /// Convert a raw 32bit RGBA image to a 5 component j2c stream
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public unsafe static byte[] EncodeSecondLifeBaked(int width, int height, byte[] data)
        {
            byte[] output = null;
            const int NUM_COMPS = 4;

            try
            {
                // setup the parameters
                IntPtr parameters_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(opj_cparameters_t)));
                opj_set_default_encoder_parameters(parameters_ptr);

                opj_cparameters_t parameters = (opj_cparameters_t)Marshal.PtrToStructure(parameters_ptr, typeof(opj_cparameters_t));

                parameters.tcp_rates[0] = 0;
                parameters.tcp_numlayers++;
                parameters.cp_disto_alloc = 1;
                parameters.cod_format = 0;
                parameters.subsampling_dx = 1;
                parameters.subsampling_dy = 1;
                parameters.cp_comment = "";

                OPJ_COLOR_SPACE color_space = OPJ_COLOR_SPACE.CLRSPC_SRGB;
                opj_image_cmptparm_t[] cmptparm = new opj_image_cmptparm_t[NUM_COMPS];

                for (int c = 0; c < NUM_COMPS; c++)
                {
                    cmptparm[c].x0 = 0;
                    cmptparm[c].y0 = 0;
                    cmptparm[c].prec = 8;
                    cmptparm[c].bpp = 8;
                    cmptparm[c].sgnd = 0;
                    cmptparm[c].dx = parameters.subsampling_dx;
                    cmptparm[c].dy = parameters.subsampling_dy;
                    cmptparm[c].w = width;
                    cmptparm[c].h = height;
                }

                // create the image
                opj_image_t* image_ptr = opj_image_create(NUM_COMPS, cmptparm, color_space);
                image_ptr->x1 = width;
                image_ptr->y1 = height;

                int i = 0;

                for (int y = height - 1; y >= 0; y--)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int dataindex = (y * width + x) * NUM_COMPS;

                        // Order of the pixels in baked images is: red, green, blue, alpha, bump
                        for (int c = 0; c < NUM_COMPS; c++)
                        {
                            (*image_ptr).comps[c].data[i] = data[dataindex + c];
                        }

                        i++;
                    }
                }

                // get a J2K compressor handle
                opj_cinfo_t* cinfo_ptr = opj_create_compress(OPJ_CODEC_FORMAT.CODEC_J2K);

                // TODO: setup the callbacks

                // setup the encoder parameters
                Marshal.StructureToPtr(parameters, parameters_ptr, true);
                opj_setup_encoder(cinfo_ptr, parameters_ptr, image_ptr);

                // open a byte stream for writing
                opj_cio_t* cio_ptr = opj_cio_open((void*)cinfo_ptr, null, 0);

                // encode the image
                bool success;
                fixed(char* index = parameters.index) {
                        success = opj_encode(cinfo_ptr, cio_ptr, image_ptr, index);
                }

                // Free the parameters pointer (no longer needed)
                Marshal.FreeHGlobal(parameters_ptr);

                // Check if the encoding was successful
                if (!success)
                {
                    opj_cio_close(cio_ptr);
                    return null;
                }

                int codestream_length = cio_tell(cio_ptr);

                output = new byte[codestream_length];
                Marshal.Copy((IntPtr)(*cio_ptr).buffer, output, 0, codestream_length);

                // Close and free the byte stream
                opj_cio_close(cio_ptr);

                // Destroy the compression structures
                opj_destroy_compress(cinfo_ptr);

                // Destroy the image data
                opj_image_destroy(image_ptr);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }

            return output;
        }
    }
}
