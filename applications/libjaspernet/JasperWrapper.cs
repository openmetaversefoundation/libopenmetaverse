using System;
using System.Runtime.InteropServices; // For DllImport().

public struct jas_matrix_t {            /* A matrix of pixels (one matrix per component) */
  public int    flags;    		/* Additional state information. */
  public int    xstart;  		/* The starting horizontal index. */
  public int    ystart;			/* The starting vertical index. */
  public int    xend; 	 		/* The ending horizontal index. */
  public int    yend;  			/* The ending vertical index. */
  public int    numrows;        	/* The number of rows in the matrix. */
  public int    numcols;        	/* The number of columns in the matrix. */
  public IntPtr row_ptrs;		/* Pointers to the start of each row. */
  public int    maxrows;        	/* The allocated size of the rows array. */      
  public IntPtr data;  			/* The matrix data buffer. */
  public int    datasize;       	/* The allocated size of the data array. */
  
  public static jas_matrix_t fromPtr(IntPtr ptr) {
    return (jas_matrix_t)Marshal.PtrToStructure(ptr, typeof(jas_matrix_t));
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

public struct jas_image_cmpt_t {        /* Image component class. */
  public int    tlx;                    /* The x-coordinate of the top-left corner of the component. */
  public int    tly;                    /* The y-coordinate of the top-left corner of the component. */
  public int    hstep;                  /* The horizontal sampling period in units of the reference grid. */
  public int    vstep;                  /* The vertical sampling period in units of the reference grid. */
  public int    width;                  /* The component width in samples. */
  public int    height;                 /* The component height in samples. */
  public int    prec;                   /* precision */
  public int    sgnd;                   /* signed-ness */
  public IntPtr stream;                 /* The stream containing the component data. */
  public int    cps;                    /* The number of characters per sample in the stream. */
  public int    type;                   /* The type of component (e.g., opacity, red, green, blue, luma). */
  
  public static jas_image_cmpt_t fromPtr(IntPtr ptr) {
    return (jas_image_cmpt_t)Marshal.PtrToStructure(ptr, typeof(jas_image_cmpt_t));
  }
}

public struct jas_image_t {
  public int    tlx;                     /* The x-coordinate of the top-left corner of the image bounding box. */
  public int    tly;                     /* The y-coordinate of the top-left corner of the image bounding box. */
  public int    brx;                     /* The x-coordinate of the bottom-right corner of the image bounding
		                            box (plus one). */
  public int    bry;                     /* The y-coordinate of the bottom-right corner of the image bounding
		                            box (plus one). */
  public int    numcmpts;                /* The number of components. */
  public int    maxcmpts;                /* The maximum number of components that this image can have (i.e., the
		                            allocated size of the components array). */
  public IntPtr cmpts;                   /* array of pointers to jas_image_cmpt_t: Per-component information. */
  public int    clrspc;
  public IntPtr cmprof;
  public bool   inmem;
  
  public static jas_image_t fromPtr(IntPtr ptr) {
    return (jas_image_t)Marshal.PtrToStructure(ptr, typeof(jas_image_t));
  }
  
  public int width { get { return brx-tlx; } }
  public int height { get { return bry-tly; } }

  public jas_image_cmpt_t[] get_components() {
    jas_image_cmpt_t[] retval=new jas_image_cmpt_t[numcmpts];
    int i;
    
    for(i=0;i<numcmpts;i++) retval[i]=jas_image_cmpt_t.fromPtr(Marshal.ReadIntPtr(cmpts,i*4));
    return retval;
  }
  
}

class JasperWrapper {
  const string JASPER_LIBRARY = "libjasper.dll";
  
  [DllImport(JASPER_LIBRARY)]
    private static extern int jas_init();
  
  [DllImport(JASPER_LIBRARY)]
    private static extern IntPtr jas_getversion();
  
  [DllImport(JASPER_LIBRARY)]
    private static extern int jas_setdbglevel(int level);
  
  [DllImport(JASPER_LIBRARY)]
    public static extern IntPtr jas_stream_fopen(string filename, string mode);
  
  [DllImport(JASPER_LIBRARY)]
    private static extern IntPtr jas_stream_memopen(IntPtr buf, int bufsize);
  
  public static IntPtr jas_stream_memopen(byte[] buf) {
    IntPtr bufPtr = Marshal.AllocHGlobal(buf.Length);
    Marshal.Copy(buf,0,bufPtr,buf.Length);
    
    return jas_stream_memopen(bufPtr, buf.Length);
  }
  
  [DllImport(JASPER_LIBRARY)]
    public static extern int jas_stream_flush(IntPtr stream);
  
  [DllImport(JASPER_LIBRARY)]
    public static extern int jas_stream_close(IntPtr stream);
  
  [DllImport(JASPER_LIBRARY)]
    public static extern int jas_image_getfmt(IntPtr stream);
  
  [DllImport(JASPER_LIBRARY)]
    public static extern int jas_image_fmtfromname(string filename);
  
  [DllImport(JASPER_LIBRARY)]
    public static extern int jas_image_encode(IntPtr image, IntPtr out_stream, int fmt, string optstr);
  
  [DllImport(JASPER_LIBRARY)]
    public static extern IntPtr jas_image_decode(IntPtr in_stream, int fmt, string optstr);
  
  [DllImport(JASPER_LIBRARY)]
    public static extern IntPtr jas_image_destroy(IntPtr image_ptr);
  
  public static int jas_image_cmpttype(jas_image_t image, int cmptno) {
    jas_image_cmpt_t cmpt=
      (jas_image_cmpt_t)Marshal.PtrToStructure(Marshal.ReadIntPtr(image.cmpts, cmptno*4),
					       typeof(jas_image_cmpt_t));
    return cmpt.type;
  }
  
  [DllImport(JASPER_LIBRARY)]
    public static extern int jas_image_readcmpt(IntPtr image_ptr, int cmptno,
						int x, int y, int width, int height, IntPtr data);
  
  public static string get_jasper_version() {
    string text = Marshal.PtrToStringAuto(jas_getversion());
    if(text==null) throw new Exception("jas_getversion returned NULL\n");
    return text;
  }
  
  [DllImport(JASPER_LIBRARY)]
    public static extern IntPtr jas_matrix_create(int width, int height);
  
  [DllImport(JASPER_LIBRARY)]
    public static extern void jas_matrix_destroy(IntPtr matrix);

  public static int[] get_multiplexed_image_data(IntPtr image_ptr, int[] order) {
    if(order.Length>4) throw new Exception("order.Length must be <= 4");
    jas_image_t image_struct=jas_image_t.fromPtr(image_ptr);
    
    IntPtr matrix=jas_matrix_create(image_struct.height, image_struct.width);
    int [] pixels = new int [image_struct.height*image_struct.width];

    for(int c=0;c<order.Length;c++) {
      int retval=jas_image_readcmpt(image_ptr, order[c], 0, 0, 
				    image_struct.width, image_struct.height, matrix);
      if(retval!=0) {
	jas_matrix_destroy(matrix);
	throw new Exception("jas_iamge_readcmpt returned "+retval);
      }

      int[] temp_array=(jas_matrix_t.fromPtr(matrix)).get_int_array();
      for(int i=0;i<temp_array.Length;i++) pixels[i] |= (temp_array[i]&0xFF) << (c*8);
    }
    jas_matrix_destroy(matrix);
    return pixels;
  }
  
  public static String version {
    get { return get_jasper_version(); }
  }
  
  public static int[] jasper_decode_j2c(byte[] input) {
    IntPtr input_stream_ptr=jas_stream_memopen(input);
    int format=jas_image_getfmt(input_stream_ptr);
      
    Console.WriteLine("file is format # "+format);
      
    IntPtr image_ptr=jas_image_decode(input_stream_ptr, format, "");
    if(image_ptr==IntPtr.Zero) throw new Exception("Error decoding image");
      
    jas_image_t image_struct=(jas_image_t)Marshal.PtrToStructure(image_ptr, typeof(jas_image_t));
    
    Console.WriteLine("image has "+image_struct.numcmpts+" components");
    int[] mux_order=null;	
    switch(image_struct.numcmpts) {
        case 4: mux_order=new int[] {0, 1, 2, 3}; break; /* ARGB? */
        case 3: mux_order=new int[] {0, 1, 2};    break; /* RGB? */
        case 2: mux_order=new int[] {0, 1};       break; /* ??? */
        case 1: mux_order=new int[] {0};          break; /* grayscale */
	default: throw new Exception("Unhandled number of components:" + image_struct.numcmpts);
    }
      
    int[] pixels=get_multiplexed_image_data(image_ptr, mux_order);

    Console.WriteLine("Image dimensions: "+image_struct.width	+ " x " + image_struct.height);
    jas_stream_close(input_stream_ptr);
    jas_image_destroy(image_ptr);
    return pixels;
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
      
      jas_image_t image_struct=(jas_image_t)Marshal.PtrToStructure(image_ptr, typeof(jas_image_t));

      Console.WriteLine("image has "+image_struct.numcmpts+" components");
      int[] mux_order=null;
      switch(image_struct.numcmpts) {
        case 4: mux_order=new int[] {0, 1, 2, 3}; break; /* ARGB? */
        case 3: mux_order=new int[] {0, 1, 2};    break; /* RGB? */
        case 2: mux_order=new int[] {0, 1};       break; /* ??? */
        case 1: mux_order=new int[] {0};          break; /* grayscale */
	default: throw new Exception("Unhandled number of components:" + image_struct.numcmpts);
      }
      
      int[] pixels=get_multiplexed_image_data(image_ptr, mux_order);

      int row, col;
      Console.WriteLine("Image dimensions: "+image_struct.width
			+ " x " + image_struct.height);
			
      for(row=0; row<image_struct.height; row++) 
	for(col=0; col< image_struct.width; col++) {
	  Console.WriteLine("("+row+","+col+"): "+pixels[image_struct.width*row+col].ToString("x8"));
	}
      /*
	int output_format=jas_image_fmtfromname("foo.jpg");
	
	int retval=jas_image_encode(image_ptr, output_stream_ptr, output_format, "");
	
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
