using System;
using System.IO;
using System.Runtime.InteropServices; // For DllImport().

public struct jas_matrix_t
{            /* A matrix of pixels (one matrix per component) */
    public int flags;    		/* Additional state information. */
    public int xstart;  		/* The starting horizontal index. */
    public int ystart;			/* The starting vertical index. */
    public int xend; 	 		/* The ending horizontal index. */
    public int yend;  			/* The ending vertical index. */
    public int numrows;        	/* The number of rows in the matrix. */
    public int numcols;        	/* The number of columns in the matrix. */
    public IntPtr row_ptrs;		/* Pointers to the start of each row. */
    public int maxrows;        	/* The allocated size of the rows array. */
    public IntPtr data;  			/* The matrix data buffer. */
    public int datasize;       	/* The allocated size of the data array. */

    public static jas_matrix_t fromPtr(IntPtr ptr)
    {
        return (jas_matrix_t)Marshal.PtrToStructure(ptr, typeof(jas_matrix_t));
    }

    public int[] get_int_array()
    {
        int[] retval = new int[datasize];
        Marshal.Copy(data, retval, 0, datasize); /* only the low byte has any meaning */
        return retval;
    }

    public int get_pixel(int x, int y)
    {
        return Marshal.ReadInt32(data, y * numcols + x);
    }
}

public struct jas_image_cmpt_t
{        /* Image component class. */
    public int tlx;                    /* The x-coordinate of the top-left corner of the component. */
    public int tly;                    /* The y-coordinate of the top-left corner of the component. */
    public int hstep;                  /* The horizontal sampling period in units of the reference grid. */
    public int vstep;                  /* The vertical sampling period in units of the reference grid. */
    public int width;                  /* The component width in samples. */
    public int height;                 /* The component height in samples. */
    public int prec;                   /* precision */
    public int sgnd;                   /* signed-ness */
    public IntPtr stream;                 /* The stream containing the component data. */
    public int cps;                    /* The number of characters per sample in the stream. */
    public int type;                   /* The type of component (e.g., opacity, red, green, blue, luma). */

    public static jas_image_cmpt_t fromPtr(IntPtr ptr)
    {
        return (jas_image_cmpt_t)Marshal.PtrToStructure(ptr, typeof(jas_image_cmpt_t));
    }
}

public struct jas_image_t
{
    public int tlx;                     /* The x-coordinate of the top-left corner of the image bounding box. */
    public int tly;                     /* The y-coordinate of the top-left corner of the image bounding box. */
    public int brx;                     /* The x-coordinate of the bottom-right corner of the image bounding
		                            box (plus one). */
    public int bry;                     /* The y-coordinate of the bottom-right corner of the image bounding
		                            box (plus one). */
    public int numcmpts;                /* The number of components. */
    public int maxcmpts;                /* The maximum number of components that this image can have (i.e., the
		                            allocated size of the components array). */
    public IntPtr cmpts;                   /* array of pointers to jas_image_cmpt_t: Per-component information. */
    public int clrspc;
    public IntPtr cmprof;
    public bool inmem;

    public static jas_image_t fromPtr(IntPtr ptr)
    {
        return (jas_image_t)Marshal.PtrToStructure(ptr, typeof(jas_image_t));
    }

    public int width { get { return brx - tlx; } }
    public int height { get { return bry - tly; } }

    public jas_image_cmpt_t[] get_components()
    {
        jas_image_cmpt_t[] retval = new jas_image_cmpt_t[numcmpts];
        int i;

        for (i = 0; i < numcmpts; i++) retval[i] = jas_image_cmpt_t.fromPtr(Marshal.ReadIntPtr(cmpts, i * 4));
        return retval;
    }

}

public class JasperWrapper
{
    const string JASPER_LIBRARY = "libjasper.dll";

    [DllImport(JASPER_LIBRARY)]
    public static extern int jas_init();

    [DllImport(JASPER_LIBRARY)]
    private static extern IntPtr jas_getversion();

    [DllImport(JASPER_LIBRARY)]
    private static extern int jas_setdbglevel(int level);

    [DllImport(JASPER_LIBRARY)]
    public static extern IntPtr jas_stream_fopen(string filename, string mode);

    [DllImport(JASPER_LIBRARY)]
    private static extern IntPtr jas_stream_memopen(IntPtr buf, int bufsize);

    public static IntPtr jas_stream_memopen(byte[] buf)
    {
        IntPtr bufPtr = Marshal.AllocHGlobal(buf.Length);
        Marshal.Copy(buf, 0, bufPtr, buf.Length);

        return jas_stream_memopen(bufPtr, buf.Length);
    }

    [DllImport(JASPER_LIBRARY)]
    public static extern int jas_stream_flush(IntPtr stream);

    [DllImport(JASPER_LIBRARY)]
    public static extern int jas_stream_close(IntPtr stream);

    [DllImport(JASPER_LIBRARY)]
    public static extern int jas_image_getfmt(IntPtr stream);

    /* Get the ID for the image format with the specified name. */
    [DllImport(JASPER_LIBRARY)]
    private static extern int jas_image_strtofmt(string name);

    [DllImport(JASPER_LIBRARY)]
    public static extern int jas_image_fmtfromname(string filename);

    [DllImport(JASPER_LIBRARY)]
    public static extern int jas_image_encode(IntPtr image, IntPtr out_stream, int fmt, string optstr);

    [DllImport(JASPER_LIBRARY)]
    public static extern IntPtr jas_image_decode(IntPtr in_stream, int fmt, string optstr);

    [DllImport(JASPER_LIBRARY)]
    public static extern IntPtr jas_image_destroy(IntPtr image_ptr);

    public static int jas_image_cmpttype(jas_image_t image, int cmptno)
    {
        jas_image_cmpt_t cmpt =
          (jas_image_cmpt_t)Marshal.PtrToStructure(Marshal.ReadIntPtr(image.cmpts, cmptno * 4),
                               typeof(jas_image_cmpt_t));
        return cmpt.type;
    }

    [DllImport(JASPER_LIBRARY)]
    public static extern int jas_image_readcmpt(IntPtr image_ptr, int cmptno,
                        int x, int y, int width, int height, IntPtr data);

    public static string get_jasper_version()
    {
        string text = Marshal.PtrToStringAuto(jas_getversion());
        if (text == null) throw new Exception("jas_getversion returned NULL\n");
        return text;
    }

    [DllImport(JASPER_LIBRARY)]
    public static extern IntPtr jas_matrix_create(int width, int height);

    [DllImport(JASPER_LIBRARY)]
    public static extern void jas_matrix_destroy(IntPtr matrix);

    public static int[] get_multiplexed_image_data(IntPtr image_ptr, int[] order)
    {
        if (order.Length > 4) throw new Exception("order.Length must be <= 4");
        jas_image_t image_struct = jas_image_t.fromPtr(image_ptr);

        IntPtr matrix = jas_matrix_create(image_struct.height, image_struct.width);
        int[] pixels = new int[image_struct.height * image_struct.width];

        for (int c = 0; c < order.Length; c++)
        {
            if (order[c] == -1) for (int i = 0; i < pixels.Length; i++) pixels[i] |= 0xFF << (c * 8);
            else
            {
                int retval = jas_image_readcmpt(image_ptr, order[c], 0, 0,
                                  image_struct.width, image_struct.height, matrix);
                if (retval != 0)
                {
                    jas_matrix_destroy(matrix);
                    throw new Exception("jas_image_readcmpt returned " + retval);
                }

                int[] temp_array = (jas_matrix_t.fromPtr(matrix)).get_int_array();
                for (int i = 0; i < temp_array.Length; i++) pixels[i] |= (temp_array[i] & 0xFF) << (c * 8);
            }
        }
        jas_matrix_destroy(matrix);
        return pixels;
    }

    public static String version
    {
        get { return get_jasper_version(); }
    }

    public static byte[] jasper_decode_j2c_to_tiff(byte[] input)
    {
	int header_size=1024;  // a guess
        IntPtr input_stream_ptr = jas_stream_memopen(input);
        int format = jas_image_getfmt(input_stream_ptr);

        Console.WriteLine("file is format # " + format);

        IntPtr image_ptr = jas_image_decode(input_stream_ptr, format, "");
        if (image_ptr == IntPtr.Zero) throw new Exception("Error decoding image");
        jas_image_t image_struct = jas_image_t.fromPtr(image_ptr);
	
	int output_buffer_size=image_struct.width*image_struct.height*4+header_size;
        IntPtr bufPtr = Marshal.AllocHGlobal(output_buffer_size);
        IntPtr output_stream_ptr = jas_stream_memopen(bufPtr, output_buffer_size);

        int retval = jas_image_encode(image_ptr, output_stream_ptr, 
				      jas_image_strtofmt("tif"), "");

        if (retval != 0) throw new Exception("Error encoding image: " + retval);

        byte[] buf = new byte[output_buffer_size];
        Marshal.Copy(bufPtr, buf, 0, output_buffer_size);
        Marshal.FreeHGlobal(bufPtr);

        jas_image_destroy(image_ptr);
        jas_stream_close(output_stream_ptr);
        jas_stream_close(input_stream_ptr);

        return buf;
    }

    public static byte[] jasper_decode_j2c_to_tga(byte[] input)
    {
        int header_size = 32;  // roughly
        IntPtr input_stream_ptr = jas_stream_memopen(input);

        int format = jas_image_getfmt(input_stream_ptr);

        Console.WriteLine("file is format # " + format);

        IntPtr image_ptr = jas_image_decode(input_stream_ptr, format, "");
        if (image_ptr == IntPtr.Zero) throw new Exception("Error decoding image");

        jas_image_t image_struct = jas_image_t.fromPtr(image_ptr);

        Console.WriteLine("image has " + image_struct.numcmpts + " components");
        int[] mux_order = null;
        switch (image_struct.numcmpts)
        {
            case 4: mux_order = new int[] { 2, 1, 0, 3 }; break; /* BGRA */
            case 3: mux_order = new int[] { 2, 1, 0, -1 }; break; /* BGR1 */
            case 2: mux_order = new int[] { 0, 0, 0, 1 }; break; /* GGGA */
            case 1: mux_order = new int[] { 0, 0, 0, -1 }; break; /* GGG1 */
            default: throw new Exception("Unhandled number of components:" + image_struct.numcmpts);
        }

        int[] pixels = get_multiplexed_image_data(image_ptr, mux_order);
        byte[] output = new byte[image_struct.height * image_struct.width * 4 + header_size];
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
        output[offset++] = (byte)(image_struct.width & 0xFF); // width - low byte
        output[offset++] = (byte)(image_struct.width >> 8); // width - hi byte
        output[offset++] = (byte)(image_struct.height & 0xFF); // height - low byte
        output[offset++] = (byte)(image_struct.height >> 8); // height - hi byte
        output[offset++] = 32; // 32 bits per pixel
        output[offset++] = 40;//8; // image descriptor byte -- 8 bits of Alpha data per pixel
        for (int i = 0; i < (image_struct.width * image_struct.height); i++)
        {
            output[offset++] = (byte)(pixels[i] & 0xFF);       // red
            output[offset++] = (byte)((pixels[i] >> 8) & 0xFF);  // green
            output[offset++] = (byte)((pixels[i] >> 16) & 0xFF); // blue
            output[offset++] = (byte)((pixels[i] >> 24) & 0xFF); // alpha
        }

        jas_image_destroy(image_ptr);
        jas_stream_close(input_stream_ptr);

        return output;
    }

    public static void Main(string[] args)
    {
        jas_init();
        Console.WriteLine("get_jasper_version=" + get_jasper_version());
        //jas_setdbglevel(1000);

        if (args.Length == 1)
        {
            Console.WriteLine("Opening file: " + args[0]);
            FileStream read_stream = new FileStream(args[0], FileMode.Open, FileAccess.Read);
            BinaryReader binary_reader = new BinaryReader(read_stream);
            byte[] j2c_data = binary_reader.ReadBytes((int)read_stream.Length);
            binary_reader.Close();
            read_stream.Close();

/*            byte[] tga_data = jasper_decode_j2c_to_tga(j2c_data);
            Console.WriteLine("Writing output to output.tga");
            FileStream write_stream = new FileStream("output.tga", FileMode.Create);
            write_stream.Write(tga_data, 0, tga_data.Length);
            write_stream.Close(); */


            byte[] tiff_data = jasper_decode_j2c_to_tiff(j2c_data);
            Console.WriteLine("Writing output to output.tif");
            FileStream write_stream = new FileStream("output.tif", FileMode.Create);
            write_stream.Write(tiff_data, 0, tiff_data.Length);
            write_stream.Close();

            return;
        }
    }
}
