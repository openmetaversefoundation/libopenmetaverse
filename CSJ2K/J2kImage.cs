#region Using Statements
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using CSJ2K;
using CSJ2K.Color;
using CSJ2K.Icc;
using CSJ2K.j2k;
using CSJ2K.j2k.codestream;
using CSJ2K.j2k.codestream.reader;
using CSJ2K.j2k.decoder;
using CSJ2K.j2k.entropy.decoder;
using CSJ2K.j2k.fileformat.reader;
using CSJ2K.j2k.image;
using CSJ2K.j2k.image.invcomptransf;
using CSJ2K.j2k.io;
using CSJ2K.j2k.quantization.dequantizer;
using CSJ2K.j2k.roi;
using CSJ2K.j2k.util;
using CSJ2K.j2k.wavelet.synthesis;
#endregion

namespace CSJ2K
{
    public class J2kImage
    {
        #region Static Decoder Methods
        public static Image FromFile(string filename)
        {
            Stream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            Image img = FromStream(stream);
            stream.Close();
            return (img);
        }

        public static Image FromBytes(byte[] j2kdata)
        {
            return FromStream(new MemoryStream(j2kdata));
        }

        public static Image FromStream(Stream stream)
        {
            RandomAccessIO in_stream = new ISRandomAccessIO(stream);

            // Initialize default parameters
            ParameterList defpl = GetDefaultParameterList(decoder_pinfo);

            // Create parameter list using defaults
            ParameterList pl = new ParameterList(defpl);

            // **** File Format ****
            // If the codestream is wrapped in the jp2 fileformat, Read the
            // file format wrapper
            FileFormatReader ff = new FileFormatReader(in_stream);
            ff.readFileFormat();
            if (ff.JP2FFUsed)
            {
                in_stream.seek(ff.FirstCodeStreamPos);
            }

            // +----------------------------+
            // | Instantiate decoding chain |
            // +----------------------------+

            // **** Header decoder ****
            // Instantiate header decoder and read main header 
            HeaderInfo hi = new HeaderInfo();
            HeaderDecoder hd;
            try
            {
                hd = new HeaderDecoder(in_stream, pl, hi);
            }
            catch (EndOfStreamException e)
            {
                throw new ApplicationException("Codestream too short or bad header, unable to decode.", e);
            }

            int nCompCod = hd.NumComps;
            int nTiles = hi.sizValue.NumTiles;
            DecoderSpecs decSpec = hd.DecoderSpecs;

            // Get demixed bitdepths
            int[] depth = new int[nCompCod];
            for (int i = 0; i < nCompCod; i++)
            {
                depth[i] = hd.getOriginalBitDepth(i);
            }

            // **** Bit stream reader ****
            BitstreamReaderAgent breader;
            try
            {
                breader = BitstreamReaderAgent.
                    createInstance(in_stream, hd, pl, decSpec,
                                   false, hi);
            }
            catch (IOException e)
            {
                throw new ApplicationException("Error while reading bit stream header or parsing packets.", e);
            }
            catch (ArgumentException e)
            {
                throw new ApplicationException("Cannot instantiate bit stream reader.", e);
            }

            // **** Entropy decoder ****
            EntropyDecoder entdec;
            try
            {
                entdec = hd.createEntropyDecoder(breader, pl);
            }
            catch (ArgumentException e)
            {
                throw new ApplicationException("Cannot instantiate entropy decoder.", e);
            }

            // **** ROI de-scaler ****
            ROIDeScaler roids;
            try
            {
                roids = hd.createROIDeScaler(entdec, pl, decSpec);
            }
            catch (ArgumentException e)
            {
                throw new ApplicationException("Cannot instantiate roi de-scaler.", e);
            }

            // **** Dequantizer ****
            Dequantizer deq;
            try
            {
                deq = hd.createDequantizer(roids, depth, decSpec);
            }
            catch (ArgumentException e)
            {
                throw new ApplicationException("Cannot instantiate dequantizer.", e);
            }

            // **** Inverse wavelet transform ***
            InverseWT invWT;
            try
            {
                // full page inverse wavelet transform
                invWT = InverseWT.createInstance(deq, decSpec);
            }
            catch (ArgumentException e)
            {
                throw new ApplicationException("Cannot instantiate inverse wavelet transform.", e);
            }

            int res = breader.ImgRes;
            invWT.ImgResLevel = res;

            // **** Data converter **** (after inverse transform module)
            ImgDataConverter converter = new ImgDataConverter(invWT, 0);

            // **** Inverse component transformation **** 
            InvCompTransf ictransf = new InvCompTransf(converter, decSpec, depth, pl);

            // **** Color space mapping ****
            BlkImgDataSrc color;
            if (ff.JP2FFUsed && pl.getParameter("nocolorspace").Equals("off"))
            {
                try
                {
                    ColorSpace csMap = new ColorSpace(in_stream, hd, pl);
                    BlkImgDataSrc channels = hd.createChannelDefinitionMapper(ictransf, csMap);
                    BlkImgDataSrc resampled = hd.createResampler(channels, csMap);
                    BlkImgDataSrc palettized = hd.createPalettizedColorSpaceMapper(resampled, csMap);
                    color = hd.createColorSpaceMapper(palettized, csMap);
                }
                catch (ArgumentException e)
                {
                    throw new ApplicationException("Could not instantiate ICC profiler.", e);
                }
                catch (ColorSpaceException e)
                {
                    throw new ApplicationException("Error processing ColorSpace information.", e);
                }
            }
            else
            { // Skip colorspace mapping
                color = ictransf;
            }

            // This is the last image in the decoding chain and should be
            // assigned by the last transformation:
            BlkImgDataSrc decodedImage = color;
            if (color == null)
            {
                decodedImage = ictransf;
            }
            int numComps = decodedImage.NumComps;

            int bytesPerPixel = (numComps == 4 ? 4 : 3);

            // **** Copy to Bitmap ****
            PixelFormat pixelFormat;
            switch (numComps)
            {
                case 1:
                    pixelFormat = PixelFormat.Format24bppRgb; break;
                case 3:
                    pixelFormat = PixelFormat.Format24bppRgb; break;
                case 4:
                    pixelFormat = PixelFormat.Format32bppArgb; break;
                default:
                    throw new ApplicationException("Unsupported PixelFormat.  " + numComps + " components.");
            }

            Bitmap dst = new Bitmap(decodedImage.ImgWidth, decodedImage.ImgHeight, pixelFormat);

            Coord numTiles = decodedImage.getNumTiles(null);

            int tIdx = 0;

            for (int y = 0; y < numTiles.y; y++)
            {
                // Loop on horizontal tiles
                for (int x = 0; x < numTiles.x; x++, tIdx++)
                {
                    decodedImage.setTile(x, y);

                    int height = decodedImage.getTileCompHeight(tIdx, 0);
                    int width = decodedImage.getTileCompWidth(tIdx, 0);

                    int tOffx = decodedImage.getCompULX(0) -
                        (int)Math.Ceiling(decodedImage.ImgULX /
                           (double)decodedImage.getCompSubsX(0));

                    int tOffy = decodedImage.getCompULY(0) -
                        (int)Math.Ceiling(decodedImage.ImgULY /
                            (double)decodedImage.getCompSubsY(0));

                    DataBlkInt[] db = new DataBlkInt[numComps];
                    int[] ls = new int[numComps];
                    int[] mv = new int[numComps];
                    int[] fb = new int[numComps];
                    for (int i = 0; i < numComps; i++)
                    {
                        db[i] = new DataBlkInt();
                        ls[i] = 1 << (decodedImage.getNomRangeBits(0) - 1);
                        mv[i] = (1 << decodedImage.getNomRangeBits(0)) - 1;
                        fb[i] = decodedImage.getFixedPoint(0);
                    }
                    for (int l = 0; l < height; l++)
                    {
                        for (int i = numComps - 1; i >= 0; i--)
                        {
                            db[i].ulx = 0;
                            db[i].uly = l;
                            db[i].w = width;
                            db[i].h = 1;
                            decodedImage.getInternCompData(db[i], i);
                        }
                        int[] k = new int[numComps];
                        for (int i = numComps - 1; i >= 0; i--) k[i] = db[i].offset + width - 1;

                        byte[] rowvalues = new byte[width * bytesPerPixel];

                        for (int i = width - 1; i >= 0; i--)
                        {
                            int[] tmp = new int[numComps];
                            for (int j = numComps - 1; j >= 0; j--)
                            {
                                tmp[j] = (db[j].data_array[k[j]--] >> fb[j]) + ls[j];
                                tmp[j] = (tmp[j] < 0) ? 0 : ((tmp[j] > mv[j]) ? mv[j] : tmp[j]);

                                if (decodedImage.getNomRangeBits(j) != 8)
                                    tmp[j] = (int)Math.Round(((double)tmp[j] / Math.Pow(2D, (double)decodedImage.getNomRangeBits(j))) * 255D);

                            }
                            int offset = i * bytesPerPixel;
                            switch (numComps)
                            {
                                case 1:
                                    rowvalues[offset + 0] = (byte)tmp[0];
                                    rowvalues[offset + 1] = (byte)tmp[0];
                                    rowvalues[offset + 2] = (byte)tmp[0];
                                    break;
                                case 3:
                                    rowvalues[offset + 0] = (byte)tmp[2];
                                    rowvalues[offset + 1] = (byte)tmp[1];
                                    rowvalues[offset + 2] = (byte)tmp[0];
                                    break;
                                case 4:
                                    rowvalues[offset + 0] = (byte)tmp[3];
                                    rowvalues[offset + 1] = (byte)tmp[2];
                                    rowvalues[offset + 2] = (byte)tmp[1];
                                    rowvalues[offset + 3] = (byte)tmp[0];
                                    break;
                            }
                        }

                        BitmapData dstdata = dst.LockBits(
                            new System.Drawing.Rectangle(tOffx, tOffy + l, width, 1),
                            ImageLockMode.ReadWrite, pixelFormat);

                        IntPtr ptr = dstdata.Scan0;
                        System.Runtime.InteropServices.Marshal.Copy(rowvalues, 0, ptr, rowvalues.Length);
                        dst.UnlockBits(dstdata);
                    }
                }
            }
            return dst;
        }
        #endregion

        #region Static Encoder Methods
        public static void ToFile(Bitmap bitmap, string filename)
        {
            FileStream stream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            ToStream(bitmap, stream);
            stream.Close();
        }
        public static byte[] ToArray(Bitmap bitmap)
        {
            MemoryStream stream = new MemoryStream();
            ToStream(bitmap, stream);
            return stream.ToArray();
        }
        public static void ToStream(Bitmap bitmap, Stream stream)
        {
            throw new NotImplementedError();
        }
        #endregion

        #region Default Parameter Loader
        public static ParameterList GetDefaultParameterList(string[][] pinfo)
        {
            ParameterList pl = new ParameterList();
            string[][] str;

            str = BitstreamReaderAgent.ParameterInfo;
            if (str != null) for (int i = str.Length - 1; i >= 0; i--)
                    pl.Set(str[i][0], str[i][3]);

            str = EntropyDecoder.ParameterInfo;
            if (str != null) for (int i = str.Length - 1; i >= 0; i--)
                    pl.Set(str[i][0], str[i][3]);

            str = ROIDeScaler.ParameterInfo;
            if (str != null) for (int i = str.Length - 1; i >= 0; i--)
                    pl.Set(str[i][0], str[i][3]);

            str = Dequantizer.ParameterInfo;
            if (str != null) for (int i = str.Length - 1; i >= 0; i--)
                    pl.Set(str[i][0], str[i][3]);

            str = InvCompTransf.ParameterInfo;
            if (str != null) for (int i = str.Length - 1; i >= 0; i--)
                    pl.Set(str[i][0], str[i][3]);

            str = HeaderDecoder.ParameterInfo;
            if (str != null) for (int i = str.Length - 1; i >= 0; i--)
                    pl.Set(str[i][0], str[i][3]);

            str = ICCProfiler.ParameterInfo;
            if (str != null) for (int i = str.Length - 1; i >= 0; i--)
                    pl.Set(str[i][0], str[i][3]);

            str = pinfo;
            if (str != null) for (int i = str.Length - 1; i >= 0; i--)
                    pl.Set(str[i][0], str[i][3]);

            return pl;
        }
        #endregion

        #region Decoder Parameters
        private static String[][] decoder_pinfo = {
        new string[] { "u", "[on|off]",
          "Prints usage information. "+
          "If specified all other arguments (except 'v') are ignored","off"},
        new string[] { "v", "[on|off]",
          "Prints version and copyright information","off"},
        new string[] { "verbose", "[on|off]",
          "Prints information about the decoded codestream","on"},
        new string[] { "pfile", "<filename>",
          "Loads the arguments from the specified file. Arguments that are "+
          "specified on the command line override the ones from the file.\n"+
          "The arguments file is a simple text file with one argument per "+
          "line of the following form:\n" +
          "  <argument name>=<argument value>\n"+
          "If the argument is of boolean type (i.e. its presence turns a "+
          "feature on), then the 'on' value turns it on, while the 'off' "+
          "value turns it off. The argument name does not include the '-' "+
          "or '+' character. Long lines can be broken into several lines "+
          "by terminating them with '\\'. Lines starting with '#' are "+
          "considered as comments. This option is not recursive: any 'pfile' "+
          "argument appearing in the file is ignored.",null},
	    new string[] { "res", "<resolution level index>",
          "The resolution level at which to reconstruct the image "+
	      " (0 means the lowest available resolution whereas the maximum "+
          "resolution level corresponds to the original image resolution). "+
          "If the given index"+
	      " is greater than the number of available resolution levels of the "+
	      "compressed image, the image is reconstructed at its highest "+
	      "resolution (among all tile-components). Note that this option"+
          " affects only the inverse wavelet transform and not the number "+
          " of bytes read by the codestream parser: this number of bytes "+
          "depends only on options '-nbytes' or '-rate'.", null},
        new string[] { "i", "<filename or url>",
          "The file containing the JPEG 2000 compressed data. This can be "+
          "either a JPEG 2000 codestream or a JP2 file containing a "+
          "JPEG 2000 "+
          "codestream. In the latter case the first codestream in the file "+
          "will be decoded. If an URL is specified (e.g., http://...) "+
          "the data will be downloaded and cached in memory before decoding. "+
          "This is intended for easy use in applets, but it is not a very "+
          "efficient way of decoding network served data.", null},
        new string[] { "o", "<filename>",
          "This is the name of the file to which the decompressed image "+
          "is written. If no output filename is given, the image is "+
          "displayed on the screen. "+
          "Output file format is PGX by default. If the extension"+
          " is '.pgm' then a PGM file is written as output, however this is "+
          "only permitted if the component bitdepth does not exceed 8. If "+
          "the extension is '.ppm' then a PPM file is written, however this "+
          "is only permitted if there are 3 components and none of them has "+
          "a bitdepth of more than 8. If there is more than 1 component, "+
          "suffices '-1', '-2', '-3', ... are added to the file name, just "+
          "before the extension, except for PPM files where all three "+
          "components are written to the same file.",null},
        new string[] { "rate","<decoding rate in bpp>",
          "Specifies the decoding rate in bits per pixel (bpp) where the "+
          "number of pixels is related to the image's original size (Note:"+
          " this number is not affected by the '-res' option). If it is equal"+
          "to -1, the whole codestream is decoded. "+
          "The codestream is either parsed (default) or truncated depending "+
          "the command line option '-parsing'. To specify the decoding "+
          "rate in bytes, use '-nbytes' options instead.","-1"},
        new string[] { "nbytes","<decoding rate in bytes>",
          "Specifies the decoding rate in bytes. "+
          "The codestream is either parsed (default) or truncated depending "+
          "the command line option '-parsing'. To specify the decoding "+
          "rate in bits per pixel, use '-rate' options instead.","-1"},
	    new string[] { "parsing", null,
	      "Enable or not the parsing mode when decoding rate is specified "+
          "('-nbytes' or '-rate' options). If it is false, the codestream "+
          "is decoded as if it were truncated to the given rate. If it is "+
          "true, the decoder creates, truncates and decodes a virtual layer"+
          " progressive codestream with the same truncation points in each "+
          "code-block.","on"},
        new string[] { "ncb_quit","<max number of code blocks>",
          "Use the ncb and lbody quit conditions. If state information is "+
          "found for more code blocks than is indicated with this option, "+
          "the decoder "+
          "will decode using only information found before that point. "+
          "Using this otion implies that the 'rate' or 'nbyte' parameter "+
          "is used to indicate the lbody parameter which is the number of "+
          "packet body bytes the decoder will decode.","-1"},
        new string[] { "l_quit","<max number of layers>",
          "Specifies the maximum number of layers to decode for any code-"+
          "block","-1"},
        new string[] { "m_quit","<max number of bit planes>",
          "Specifies the maximum number of bit planes to decode for any code"+
          "-block","-1"},
        new string[] { "poc_quit",null,
          "Specifies the whether the decoder should only decode code-blocks "+
          "included in the first progression order.","off"},
        new string[] { "one_tp",null,
          "Specifies whether the decoder should only decode the first "+
          "tile part of each tile.","off"},
        new string[] { "comp_transf",null,
          "Specifies whether the component transform indicated in the "+
          "codestream should be used.","on"},
        new string[] { "debug", null,
          "Print debugging messages when an error is encountered.","off"},
        new string[] { "cdstr_info", null,
          "Display information about the codestream. This information is: "+
          "\n- Marker segments value in main and tile-part headers,"+
          "\n- Tile-part length and position within the code-stream.", "off"},
	    new string[] { "nocolorspace",null,
	      "Ignore any colorspace information in the image.","off"},
	    new string[] { "colorspace_debug", null,
	      "Print debugging messages when an error is encountered in the"+
	      " colorspace module.","off"}
        };
        #endregion

        #region Encoder Parameters
        private static String[][] encoder_pinfo = {
        new string[] { "debug", null,
          "Print debugging messages when an error is encountered.","off"},
        new string[] { "disable_jp2_extension", "[on|off]",
          "JJ2000 automatically adds .jp2 extension when using 'file_format'"+
          "option. This option disables it when on.", "off"},
        new string[] { "file_format", "[on|off]",
          "Puts the JPEG 2000 codestream in a JP2 file format wrapper.","off"},
        new string[] { "pph_tile", "[on|off]",
          "Packs the packet headers in the tile headers.","off"},
        new string[] { "pph_main", "[on|off]",
          "Packs the packet headers in the main header.","off"},
        new string[] { "pfile", "<filename of arguments file>",
          "Loads the arguments from the specified file. Arguments that are "+
          "specified on the command line override the ones from the file.\n"+
          "The arguments file is a simple text file with one argument per "+
          "line of the following form:\n" +
          "  <argument name>=<argument value>\n"+
          "If the argument is of boolean type (i.e. its presence turns a "+
          "feature on), then the 'on' value turns it on, while the 'off' "+
          "value turns it off. The argument name does not include the '-' "+
          "or '+' character. Long lines can be broken into several lines "+
          "by terminating them with '\'. Lines starting with '#' are "+
          "considered as comments. This option is not recursive: any 'pfile' "+
          "argument appearing in the file is ignored.",null},
        new string[] { "tile_parts", "<packets per tile-part>",
          "This option specifies the maximum number of packets to have in "+
          "one tile-part. 0 means include all packets in first tile-part "+
          "of each tile","0"},
        new string[] { "tiles", "<nominal tile width> <nominal tile height>",
          "This option specifies the maximum tile dimensions to use. "+
          "If both dimensions are 0 then no tiling is used.","0 0"},
        new string[] { "ref", "<x> <y>",
          "Sets the origin of the image in the canvas system. It sets the "+
          "coordinate of the top-left corner of the image reference grid, "+
          "with respect to the canvas origin","0 0"},
        new string[] { "tref", "<x> <y>",
          "Sets the origin of the tile partitioning on the reference grid, "+
          "with respect to the canvas origin. The value of 'x' ('y') "+
          "specified can not be larger than the 'x' one specified in the ref "+
          "option.","0 0"},
        new string[] { "rate", "<output bitrate in bpp>",
          "This is the output bitrate of the codestream in bits per pixel."+
          " When equal to -1, no image information (beside quantization "+
          "effects) is discarded during compression.\n"+
          "Note: In the case where '-file_format' option is used, the "+
          "resulting file may have a larger bitrate.","-1"},
        new string[] { "lossless", "[on|off]", 
          "Specifies a lossless compression for the encoder. This options"+
          " is equivalent to use reversible quantization ('-Qtype "+
          "reversible')"+
          " and 5x3 wavelet filters pair ('-Ffilters w5x3'). Note that "+
          "this option cannot be used with '-rate'. When this option is "+
          "off, the quantization type and the filters pair is defined by "+
          "'-Qtype' and '-Ffilters' respectively.","off"},
        new string[] { "i", "<image file> [,<image file> [,<image file> ... ]]",
          "Mandatory argument. This option specifies the name of the input "+
          "image files. If several image files are provided, they have to be"+
          " separated by commas in the command line. Supported formats are "+
          "PGM (raw), PPM (raw) and PGX, "+
          "which is a simple extension of the PGM file format for single "+
          "component data supporting arbitrary bitdepths. If the extension "+
          "is '.pgm', PGM-raw file format is assumed, if the extension is "+
          "'.ppm', PPM-raw file format is assumed, otherwise PGX file "+
          "format is assumed. PGM and PPM files are assumed to be 8 bits "+
          "deep. A multi-component image can be specified by either "+
          "specifying several PPM and/or PGX files, or by specifying one "+
          "PPM file.",null},
        new string[] { "o", "<file name>",
          "Mandatory argument. This option specifies the name of the output "+
          "file to which the codestream will be written.",null},
        new string[] { "verbose", null,
          "Prints information about the obtained bit stream.","on"},
        new string[] { "v", "[on|off]",
          "Prints version and copyright information.","off"},
        new string[] { "u", "[on|off]",
          "Prints usage information. "+
          "If specified all other arguments (except 'v') are ignored","off"},
        };
        #endregion
    }
}
