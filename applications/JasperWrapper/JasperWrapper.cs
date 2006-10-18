using System;
using System.Runtime.InteropServices; // For DllImport().

struct jas_matrix_t {
	int    flags;    		/* Additional state information. */
    int    xstart;  		/* The starting horizontal index. */
    int    ystart; 			/* The starting vertical index. */
    int    xend; 	 		/* The ending horizontal index. */
    int    yend;  			/* The ending vertical index. */
    int    numrows;        	/* The number of rows in the matrix. */
    int    numcols;        	/* The number of columns in the matrix. */
    IntPtr row_ptrs;		/* Pointers to the start of each row. */
    int    maxrows;        	/* The allocated size of the rows array. */      
    IntPtr data;  			/* The matrix data buffer. */
    int    datasize;       	/* The allocated size of the data array. */
}

struct jas_image_cmpt_t { /* Image component class. */
        int tlx; /* The x-coordinate of the top-left corner of the component. */
        int tly; /* The y-coordinate of the top-left corner of the component. */
        int hstep; /* The horizontal sampling period in units of the reference grid. */
        int vstep; /* The vertical sampling period in units of the reference grid. */
        int width; /* The component width in samples. */
        int height;/* The component height in samples. */
        int prec_;
        int sgnd_;
        IntPtr stream;        /* The stream containing the component data. */
        int cps;   /* The number of characters per sample in the stream. */
        int type;/* The type of component (e.g., opacity, red, green, blue, luma). */
}

struct jas_image_t {
        int tlx; /* The x-coordinate of the top-left corner of the image bounding box. */
        int tly; /* The y-coordinate of the top-left corner of the image bounding box. */
        int brx;/* The x-coordinate of the bottom-right corner of the image bounding
                   box (plus one). */
        int bry;/* The y-coordinate of the bottom-right corner of the image bounding
                   box (plus one). */
        int numcmpts; /* The number of components. */
        int maxcmpts; /* The maximum number of components that this image can have (i.e., the
                         allocated size of the components array). */
        IntPtr cmpts;  /* array of pointers, actually, to jas_image_cmpt_t
        /* Per-component information. */
        int clrspc;
        IntPtr cmprof;
        bool inmem;
}

/* jas_stream_t and jas_image_t are opaque types, therefore we may save ourselves
   a lot of effort by just keeping them as IntPtrs in the functions below. */
class JasperWrapper {
	[DllImport("libjasper.dylib")]
	private static extern int jas_init();

	[DllImport("libjasper.dylib")]
	private static extern IntPtr jas_getversion();

	[DllImport("libjasper.dylib")]
	private static extern int jas_setdbglevel(int level);
	
	[DllImport("libjasper.dylib")]
	private static extern IntPtr jas_stream_fopen(string filename, string mode);

	[DllImport("libjasper.dylib")]
	private static extern int jas_stream_flush(IntPtr stream);

	[DllImport("libjasper.dylib")]
	private static extern int jas_stream_close(IntPtr stream);

	[DllImport("libjasper.dylib")]
	private static extern int jas_image_getfmt(IntPtr stream);
	
	[DllImport("libjasper.dylib")]
	private static extern int jas_image_fmtfromname(string filename);

	[DllImport("libjasper.dylib")]
	private static extern int jas_image_encode(IntPtr image, 
							IntPtr out_stream, int fmt, string optstr);
	
    [DllImport("libjasper.dylib")]
	private static extern IntPtr jas_image_decode(IntPtr in_stream, int fmt, 
														string optstr);

    [DllImport("libjasper.dylib")]
	private static extern IntPtr jas_image_destroy(IntPtr image_ptr);
	
    [DllImport("libjasper.dylib")]
	private static extern int jas_image_readcmpt(IntPtr image_ptr, int cmptno,
		  int x, int y, int width, int height, IntPtr data);

	public static string get_jasper_version() {
		string text = Marshal.PtrToStringAuto(jas_getversion());
		if(text==null) throw new Exception("jas_getversion returned NULL\n");
		return text;
	}
	/*
	public int[] get_image_data(IntPtr image_ptr) {
		// alloc some memory
		// pass it to jas_image_readcmpt
		// the sucky part here is you have to pull out all of the dimension info from
		// those structs :( :( :(
		} */
	public static void Main(string[] args) {
		jas_init();
		Console.WriteLine("get_jasper_version="+get_jasper_version());
		jas_setdbglevel(1000);
		
		if(args.Length==1) {
			Console.WriteLine("Opening file: "+args[0]);
			IntPtr input_stream_ptr=jas_stream_fopen(args[0], "rb");	
			
			if(input_stream_ptr==IntPtr.Zero) {
				Console.Error.WriteLine("Error opening input file\n");
				return;
			}
			
			IntPtr output_stream_ptr=jas_stream_fopen("foo.jpg", "w+b");	
			
			if(output_stream_ptr==IntPtr.Zero) {
				Console.Error.WriteLine("Error opening output file\n");
				return;
			}
			
			int format=jas_image_getfmt(input_stream_ptr);
			
			Console.WriteLine("file is format # "+format);
			
			IntPtr image_ptr=jas_image_decode(input_stream_ptr, format, "");
			if(image_ptr==IntPtr.Zero) {
				Console.Error.WriteLine("Error decoding image\n");
				return;
			}
			
			int output_format=jas_image_fmtfromname("foo.jpg");
			
			int retval=jas_image_encode(image_ptr, output_stream_ptr,
										output_format, "");
										
			if(retval!=0) {
				Console.Error.WriteLine("Error encoding image: "+retval);
				return;
			}
			
			jas_stream_flush(output_stream_ptr);
			jas_stream_close(output_stream_ptr);
			jas_stream_close(input_stream_ptr);
			
			jas_image_destroy(image_ptr);
		}
	}
}
