using System;
using System.Runtime.InteropServices; // For DllImport().

public struct jas_matrix_t {
	int    flags;    		/* Additional state information. */
    int    xstart;  		/* The starting horizontal index. */
    int    ystart; 			/* The starting vertical index. */
    int    xend; 	 		/* The ending horizontal index. */
    int    yend;  			/* The ending vertical index. */
    public int    numrows;        	/* The number of rows in the matrix. */
    public int    numcols;        	/* The number of columns in the matrix. */
    IntPtr row_ptrs;		/* Pointers to the start of each row. */
    int    maxrows;        	/* The allocated size of the rows array. */      
    public IntPtr data;  			/* The matrix data buffer. */
    int    datasize;       	/* The allocated size of the data array. */
    
    public static jas_matrix_t fromPtr(IntPtr ptr) {
    	return (jas_matrix_t)Marshal.PtrToStructure(ptr, typeof(jas_matrix_t))	;
    }
    
    public int[] get_int_array() {
    	int[] retval=new int[datasize];
    	Marshal.Copy(data, retval, 0, datasize); /* only the low byte has any meaning */
    	return retval;
    }

    public int get_pixel(int x, int y) {
    	return Marshal.ReadInt32(data, y*numcols+x);
    }
 }

public struct jas_image_cmpt_t { /* Image component class. */
    int tlx; /* The x-coordinate of the top-left corner of the component. */
    int tly; /* The y-coordinate of the top-left corner of the component. */
    int hstep; /* The horizontal sampling period in units of the reference grid. */
    int vstep; /* The vertical sampling period in units of the reference grid. */
    int width; /* The component width in samples. */
    int height;/* The component height in samples. */
    int prec;
    int sgnd;
    IntPtr stream;        /* The stream containing the component data. */
    int cps;   /* The number of characters per sample in the stream. */
    public int type;/* The type of component (e.g., opacity, red, green, blue, luma). */
    
    public static jas_image_cmpt_t fromPtr(IntPtr ptr) {
    	return (jas_image_cmpt_t)Marshal.PtrToStructure(ptr, typeof(jas_image_cmpt_t));
    }
}

public struct jas_image_t {
	public int tlx; /* The x-coordinate of the top-left corner of the image bounding box. */
    public int tly; /* The y-coordinate of the top-left corner of the image bounding box. */
    public int brx;/* The x-coordinate of the bottom-right corner of the image bounding
                      box (plus one). */
    public int bry;/* The y-coordinate of the bottom-right corner of the image bounding
                      box (plus one). */
    public int numcmpts; /* The number of components. */
    int maxcmpts; /* The maximum number of components that this image can have (i.e., the
                     allocated size of the components array). */
    public IntPtr cmpts;  /* array of pointers, actually, to jas_image_cmpt_t */
        									 /* Per-component information. */
    int clrspc;
    IntPtr cmprof;
    bool inmem;

    public static jas_image_t fromPtr(IntPtr ptr) {
    	return (jas_image_t)Marshal.PtrToStructure(ptr, typeof(jas_image_t));
    }

	public int width { get { return brx-tlx; } }
	public int height { get { return bry-tly; }	}
	public jas_image_cmpt_t[] get_components()	 {
		jas_image_cmpt_t[] retval=new jas_image_cmpt_t[numcmpts];
		int i;
		
		for(i=0;i<numcmpts;i++) retval[i]=jas_image_cmpt_t.fromPtr(Marshal.ReadIntPtr(cmpts,i*4));
		return retval;
	}
	
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
	private static extern IntPtr jas_stream_memopen(IntPtr buf, int bufsize);
	
	public static IntPtr jas_stream_memopen(byte[] buf) {
	    IntPtr bufPtr = Marshal.AllocHGlobal(buf.Length);
		Marshal.Copy(buf,0,bufPtr,buf.Length);
        
		return jas_stream_memopen(bufPtr, buf.Length);
	}

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

	private static int jas_image_cmpttype(jas_image_t image, int cmptno) {
		jas_image_cmpt_t cmpt=
			(jas_image_cmpt_t)Marshal.PtrToStructure(Marshal.ReadIntPtr(image.cmpts, cmptno*4),
													 typeof(jas_image_cmpt_t));
		return cmpt.type;
	}

	
    [DllImport("libjasper.dylib")]
	public static extern int jas_image_readcmpt(IntPtr image_ptr, int cmptno,
		  int x, int y, int width, int height, IntPtr data);


	private static string get_jasper_version() {
		string text = Marshal.PtrToStringAuto(jas_getversion());
		if(text==null) throw new Exception("jas_getversion returned NULL\n");
		return text;
	}

    [DllImport("libjasper.dylib")]
	public static extern IntPtr jas_matrix_create(int width, int height);

	
	public static int[] get_multiplexed_image_data(IntPtr image_ptr, int[] order) {
		if(order.Length>4) throw new Exception("order.Length must me <= 4");
		jas_image_t image_struct=jas_image_t.fromPtr(image_ptr);
		
		IntPtr matrix=jas_matrix_create(image_struct.height, image_struct.width);
		int [] pixels = new int [image_struct.height*image_struct.width];
		int c,i;
		for(c=0;c<order.Length;c++) {
			int retval=jas_image_readcmpt(image_ptr, order[c], 0, 0, 
		  								  image_struct.width, image_struct.height, matrix);
			if(retval!=0) throw new Exception("jas_iamge_readcmpt returned "+retval);
			int[] temp_array=(jas_matrix_t.fromPtr(matrix)).get_int_array();
			for(i=0;i<temp_array.Length;i++) pixels[i] |= (temp_array[i]&0xFF) << (c*8);
		}
		return pixels;
	}
		
		
		
	public static String version {
		get { return get_jasper_version(); }
	}
	
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
			/*
			IntPtr output_stream_ptr=jas_stream_fopen("foo.jpg", "w+b");	
			
			if(output_stream_ptr==IntPtr.Zero) {
				Console.Error.WriteLine("Error opening output file\n");
				return;
			}*/
			
			int format=jas_image_getfmt(input_stream_ptr);
			
			Console.WriteLine("file is format # "+format);
			
			IntPtr image_ptr=jas_image_decode(input_stream_ptr, format, "");
			if(image_ptr==IntPtr.Zero) {
				Console.Error.WriteLine("Error decoding image");
				return;
			}
			
			
			int[] pixels=get_multiplexed_image_data(image_ptr, new int[4] {0, 1, 2, 3});
			jas_image_t image_struct=(jas_image_t)Marshal.PtrToStructure(image_ptr, typeof(jas_image_t));
			int num_components=image_struct.numcmpts;
			Console.WriteLine("Image has "+num_components+" components");

			int i, height, width, row, col;
			height=image_struct.height;
			width=image_struct.width;
			Console.WriteLine("height="+height+", width="+width);
/*			IntPtr matrix=jas_matrix_create(height,width);
			int [][] pixels = new int [num_components] [];
				
			for(i=0;i<num_components;i++) {
				Console.WriteLine("Component "+i+" is type "+jas_image_cmpttype(image_struct,i));
				jas_image_readcmpt(image_ptr, i, 0, 0, width, height, matrix);
				IntPtr buf=jas_matrix_getbuf(matrix);
				pixels[i]=new int[width*height];
				Marshal.Copy(buf, pixels[i], 0, width*height);
			} */
			
			
			Console.WriteLine("Image dimensions: "+image_struct.width
								+ " x " + image_struct.height);
			
			for(row=0; row<height; row++) 
			   for(col=0; col< width; col++) {
			   	Console.WriteLine("("+row+","+col+"): "+pixels[image_struct.width*row+col].ToString("x8"));
				}
			/*
			int output_format=jas_image_fmtfromname("foo.jpg");
			
			int retval=jas_image_encode(image_ptr, output_stream_ptr,
										output_format, "");
										
			if(retval!=0) {
				Console.Error.WriteLine("Error encoding image: "+retval);
				return;
			}
			
			jas_stream_flush(output_stream_ptr);
			jas_stream_close(output_stream_ptr); */
			jas_stream_close(input_stream_ptr);
			
			jas_image_destroy(image_ptr);
		}
	}
}
