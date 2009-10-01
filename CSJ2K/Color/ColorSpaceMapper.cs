/// <summary>**************************************************************************
/// 
/// $Id: ColorSpaceMapper.java,v 1.2 2002/07/25 16:30:55 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>
using System;
using CSJ2K.j2k.image;
using CSJ2K.j2k.util;
using CSJ2K.Icc;
namespace CSJ2K.Color
{
	
	/// <summary> This is the base class for all modules in the colorspace and icc
	/// profiling steps of the decoding chain.  It is responsible for the
	/// allocation and iniitialization of all working storage.  It provides
	/// several utilities which are of generic use in preparing DataBlks
	/// for use and provides default implementations for the getCompData
	/// and getInternCompData methods.
	/// 
	/// </summary>
	/// <seealso cref="jj2000.j2k.colorspace.ColorSpace">
	/// </seealso>
	/// <version> 	1.0
	/// </version>
	/// <author> 	Bruce A. Kern
	/// </author>
	public abstract class ColorSpaceMapper:ImgDataAdapter, BlkImgDataSrc
	{
		private void  InitBlock()
		{
			computed = new ComputedComponents(this);
		}
		/// <summary> Returns the parameters that are used in this class and implementing
		/// classes. It returns a 2D String array. Each of the 1D arrays is for a
		/// different option, and they have 3 elements. The first element is the
		/// option name, the second one is the synopsis and the third one is a long
		/// description of what the parameter is. The synopsis or description may
		/// be 'null', in which case it is assumed that there is no synopsis or
		/// description of the option, respectively. Null may be returned if no
		/// options are supported.
		/// 
		/// </summary>
		/// <returns> the options name, their synopsis and their explanation, or null
		/// if no options are supported.
		/// 
		/// </returns>
		public static System.String[][] ParameterInfo
		{
			get
			{
				return pinfo;
			}
			
		}
		/// <summary> Arrange for the input DataBlk to receive an
		/// appropriately sized and typed data buffer
		/// </summary>
		/// <param name="db">input DataBlk
		/// </param>
		/// <seealso cref="jj2000.j2k.image.DataBlk">
		/// </seealso>
		protected internal static DataBlk InternalBuffer
		{
			set
			{
				switch (value.DataType)
				{
					
					
					case DataBlk.TYPE_INT: 
						if (value.Data == null || ((int[]) value.Data).Length < value.w * value.h)
							value.Data = new int[value.w * value.h];
						break;
					
					
					case DataBlk.TYPE_FLOAT: 
						if (value.Data == null || ((float[]) value.Data).Length < value.w * value.h)
						{
							value.Data = new float[value.w * value.h];
						}
						break;
					
					
					default: 
						throw new System.ArgumentException("Invalid output datablock" + " type");
					
				}
			}
			
		}
		
		/// <summary>The prefix for ICC Profiler options </summary>
		public const char OPT_PREFIX = 'I';
		
		/// <summary>Platform dependant end of line String. </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'eol '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		protected internal static readonly System.String eol = System.Environment.NewLine;
		
		// Temporary data buffers needed during profiling.
		protected internal DataBlkInt[] inInt; // Integer input data.
		protected internal DataBlkFloat[] inFloat; // Floating point input data.
		protected internal DataBlkInt[] workInt; // Input data shifted to zero-offset
		protected internal DataBlkFloat[] workFloat; // Input data shifted to zero-offset.
		protected internal int[][] dataInt; // Points to input data.
		protected internal float[][] dataFloat; // Points to input data.
		protected internal float[][] workDataFloat; // References working data pixels.
		protected internal int[][] workDataInt; // References working data pixels.
		
		
		/* input data parameters by component */
		protected internal int[] shiftValueArray = null;
		protected internal int[] maxValueArray = null;
		protected internal int[] fixedPtBitsArray = null;
		
		/// <summary>The list of parameters that are accepted for ICC profiling.</summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'pinfo'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		private static readonly System.String[][] pinfo = new System.String[][]{new System.String[]{"IcolorSpacedebug", null, "Print debugging messages during colorspace mapping.", "off"}};
		
		/// <summary>Parameter Specs </summary>
		protected internal ParameterList pl = null;
		
		/// <summary>ColorSpace info </summary>
		protected internal ColorSpace csMap = null;
		
		/// <summary>Number of image components </summary>
		protected internal int ncomps = 0;
		
		/// <summary>The image source. </summary>
		protected internal BlkImgDataSrc src = null;
		
		/// <summary>The image source data per component. </summary>
		protected internal DataBlk[] srcBlk = null;
		
		
		//UPGRADE_NOTE: Field 'EnclosingInstance' was added to class 'ComputedComponents' to access its enclosing instance. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1019'"
		protected internal class ComputedComponents
		{
			private void  InitBlock(ColorSpaceMapper enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private ColorSpaceMapper enclosingInstance;
			public ColorSpaceMapper Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			//private int tIdx = - 1;
			private int h = - 1;
			private int w = - 1;
			private int ulx = - 1;
			private int uly = - 1;
			private int offset = - 1;
			private int scanw = - 1;
			
			public ComputedComponents(ColorSpaceMapper enclosingInstance)
			{
				InitBlock(enclosingInstance);
				clear();
			}
			
			public ComputedComponents(ColorSpaceMapper enclosingInstance, DataBlk db)
			{
				InitBlock(enclosingInstance);
				set_Renamed(db);
			}
			
			public virtual void  set_Renamed(DataBlk db)
			{
				h = db.h;
				w = db.w;
				ulx = db.ulx;
				uly = db.uly;
				offset = db.offset;
				scanw = db.scanw;
			}
			
			public virtual void  clear()
			{
				h = w = ulx = uly = offset = scanw = - 1;
			}
			
			public bool Equals(ComputedComponents cc)
			{
				return (h == cc.h && w == cc.w && ulx == cc.ulx && uly == cc.uly && offset == cc.offset && scanw == cc.scanw);
			}
			
			/* end class ComputedComponents */
		}
		
		//UPGRADE_NOTE: The initialization of  'computed' was moved to method 'InitBlock'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1005'"
		protected internal ComputedComponents computed;
		
		/// <summary> Copy the DataBlk geometry from source to target
		/// DataBlk and assure that the target has an appropriate
		/// data buffer.
		/// </summary>
		/// <param name="tgt">has its geometry set.
		/// </param>
		/// <param name="src">used to get the new geometric parameters.
		/// </param>
		protected internal static void  copyGeometry(DataBlk tgt, DataBlk src)
		{
			tgt.offset = 0;
			tgt.h = src.h;
			tgt.w = src.w;
			tgt.ulx = src.ulx;
			tgt.uly = src.uly;
			tgt.scanw = src.w;
			
			// Create data array if necessary
			
			InternalBuffer = tgt;
		}
		
		
		/// <summary> Factory method for creating instances of this class.</summary>
		/// <param name="src">-- source of image data
		/// </param>
		/// <param name="csMap">-- provides colorspace info
		/// </param>
		/// <returns> ColorSpaceMapper instance
		/// </returns>
		/// <exception cref="IOException">profile access exception
		/// </exception>
		public static BlkImgDataSrc createInstance(BlkImgDataSrc src, ColorSpace csMap)
		{
			
			// Check parameters
			csMap.pl.checkList(OPT_PREFIX, CSJ2K.j2k.util.ParameterList.toNameArray(pinfo));
			
			// Perform ICCProfiling or ColorSpace tranfsormation.
			if (csMap.Method == ColorSpace.MethodEnum.ICC_PROFILED)
			{
				return ICCProfiler.createInstance(src, csMap);
			}
			else
			{
				ColorSpace.CSEnum colorspace = csMap.getColorSpace();
				
				if (colorspace == ColorSpace.CSEnum.sRGB)
				{
					return EnumeratedColorSpaceMapper.createInstance(src, csMap);
				}
				else if (colorspace == ColorSpace.CSEnum.GreyScale)
				{
					return EnumeratedColorSpaceMapper.createInstance(src, csMap);
				}
				else if (colorspace == ColorSpace.CSEnum.sYCC)
				{
					return SYccColorSpaceMapper.createInstance(src, csMap);
				}
                if (colorspace == ColorSpace.CSEnum.esRGB)
                {
                    return EsRgbColorSpaceMapper.createInstance(src, csMap);
                }
                else if (colorspace == ColorSpace.CSEnum.Unknown)
                {
                    return null;
                }
                else
                {
                    throw new ColorSpaceException("Bad color space specification in image");
                }
			}
		}
		
		/// <summary> Ctor which creates an ICCProfile for the image and initializes
		/// all data objects (input, working, and output).
		/// 
		/// </summary>
		/// <param name="src">-- Source of image data
		/// </param>
		/// <param name="csm">-- provides colorspace info
		/// 
		/// </param>
		protected internal ColorSpaceMapper(BlkImgDataSrc src, ColorSpace csMap):base(src)
		{
			InitBlock();
			this.src = src;
			this.csMap = csMap;
			initialize();
			/* end ColorSpaceMapper ctor */
		}
		
		/// <summary>General utility used by ctors </summary>
		private void  initialize()
		{
			
			this.pl = csMap.pl;
			this.ncomps = src.NumComps;
			
			shiftValueArray = new int[ncomps];
			maxValueArray = new int[ncomps];
			fixedPtBitsArray = new int[ncomps];
			
			srcBlk = new DataBlk[ncomps];
			inInt = new DataBlkInt[ncomps];
			inFloat = new DataBlkFloat[ncomps];
			workInt = new DataBlkInt[ncomps];
			workFloat = new DataBlkFloat[ncomps];
			dataInt = new int[ncomps][];
			dataFloat = new float[ncomps][];
			workDataInt = new int[ncomps][];
			workDataFloat = new float[ncomps][];
			dataInt = new int[ncomps][];
			dataFloat = new float[ncomps][];
			
			
			/* For each component, get a reference to the pixel data and
			* set up working DataBlks for both integer and float output.
			*/
			for (int i = 0; i < ncomps; ++i)
			{
				
				shiftValueArray[i] = 1 << (src.getNomRangeBits(i) - 1);
				maxValueArray[i] = (1 << src.getNomRangeBits(i)) - 1;
				fixedPtBitsArray[i] = src.getFixedPoint(i);
				
				inInt[i] = new DataBlkInt();
				inFloat[i] = new DataBlkFloat();
				workInt[i] = new DataBlkInt();
				workInt[i].progressive = inInt[i].progressive;
				workFloat[i] = new DataBlkFloat();
				workFloat[i].progressive = inFloat[i].progressive;
			}
		}
		
		/// <summary> Returns the number of bits, referred to as the "range bits",
		/// corresponding to the nominal range of the data in the specified
		/// component. If this number is <i>b</b> then for unsigned data the
		/// nominal range is between 0 and 2^b-1, and for signed data it is between
		/// -2^(b-1) and 2^(b-1)-1. For floating point data this value is not
		/// applicable.
		/// 
		/// </summary>
		/// <param name="c">The index of the component.
		/// 
		/// </param>
		/// <returns> The number of bits corresponding to the nominal range of the
		/// data. Fro floating-point data this value is not applicable and the
		/// return value is undefined.
		/// </returns>
		public virtual int getFixedPoint(int c)
		{
			return src.getFixedPoint(c);
		}
		
		/// <summary> Returns, in the blk argument, a block of image data containing the
		/// specifed rectangular area, in the specified component. The data is
		/// returned, as a copy of the internal data, therefore the returned data
		/// can be modified "in place".
		/// 
		/// <P>The rectangular area to return is specified by the 'ulx', 'uly', 'w'
		/// and 'h' members of the 'blk' argument, relative to the current
		/// tile. These members are not modified by this method. The 'offset' of
		/// the returned data is 0, and the 'scanw' is the same as the block's
		/// width. See the 'DataBlk' class.
		/// 
		/// <P>This method, in general, is less efficient than the
		/// 'getInternCompData()' method since, in general, it copies the
		/// data. However if the array of returned data is to be modified by the
		/// caller then this method is preferable.
		/// 
		/// <P>If the data array in 'blk' is 'null', then a new one is created. If
		/// the data array is not 'null' then it is reused, and it must be large
		/// enough to contain the block's data. Otherwise an 'ArrayStoreException'
		/// or an 'IndexOutOfBoundsException' is thrown by the Java system.
		/// 
		/// <P>The returned data may have its 'progressive' attribute set. In this
		/// case the returned data is only an approximation of the "final" data.
		/// 
		/// </summary>
		/// <param name="blk">Its coordinates and dimensions specify the area to return,
		/// relative to the current tile. If it contains a non-null data array,
		/// then it must be large enough. If it contains a null data array a new
		/// one is created. Some fields in this object are modified to return the
		/// data.
		/// 
		/// </param>
		/// <param name="c">The index of the component from which to get the data.
		/// 
		/// </param>
		/// <seealso cref="getInternCompData">
		/// 
		/// </seealso>
		public virtual DataBlk getCompData(DataBlk out_Renamed, int c)
		{
			return src.getCompData(out_Renamed, c);
		}
		
		/// <summary> Returns, in the blk argument, a block of image data containing the
		/// specifed rectangular area, in the specified component. The data is
		/// returned, as a reference to the internal data, if any, instead of as a
		/// copy, therefore the returned data should not be modified.
		/// 
		/// <P>The rectangular area to return is specified by the 'ulx', 'uly', 'w'
		/// and 'h' members of the 'blk' argument, relative to the current
		/// tile. These members are not modified by this method. The 'offset' and
		/// 'scanw' of the returned data can be arbitrary. See the 'DataBlk' class.
		/// 
		/// <P>This method, in general, is more efficient than the 'getCompData()'
		/// method since it may not copy the data. However if the array of returned
		/// data is to be modified by the caller then the other method is probably
		/// preferable.
		/// 
		/// <P>If possible, the data in the returned 'DataBlk' should be the
		/// internal data itself, instead of a copy, in order to increase the data
		/// transfer efficiency. However, this depends on the particular
		/// implementation (it may be more convenient to just return a copy of the
		/// data). This is the reason why the returned data should not be modified.
		/// 
		/// <P>If the data array in <tt>blk</tt> is <tt>null</tt>, then a new one
		/// is created if necessary. The implementation of this interface may
		/// choose to return the same array or a new one, depending on what is more
		/// efficient. Therefore, the data array in <tt>blk</tt> prior to the
		/// method call should not be considered to contain the returned data, a
		/// new array may have been created. Instead, get the array from
		/// <tt>blk</tt> after the method has returned.
		/// 
		/// <P>The returned data may have its 'progressive' attribute set. In this
		/// case the returned data is only an approximation of the "final" data.
		/// 
		/// </summary>
		/// <param name="blk">Its coordinates and dimensions specify the area to return,
		/// relative to the current tile. Some fields in this object are modified
		/// to return the data.
		/// 
		/// </param>
		/// <param name="c">The index of the component from which to get the data.
		/// 
		/// </param>
		/// <returns> The requested DataBlk
		/// 
		/// </returns>
		/// <seealso cref="getCompData">
		/// 
		/// </seealso>
		public virtual DataBlk getInternCompData(DataBlk out_Renamed, int c)
		{
			return src.getInternCompData(out_Renamed, c);
		}
		
		/* end class ColorSpaceMapper */
	}
}