/// <summary>**************************************************************************
/// 
/// $Id: ICCProfiler.java,v 1.2 2002/08/08 14:08:27 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>
using System;
using CSJ2K.j2k.decoder;
using CSJ2K.j2k.image;
using CSJ2K.j2k.util;
using CSJ2K.j2k.io;
using CSJ2K.Color;
using CSJ2K.Icc.Lut;
namespace CSJ2K.Icc
{
	
	/// <summary> This class provides ICC Profiling API for the jj2000.j2k imaging chain
	/// by implementing the BlkImgDataSrc interface, in particular the getCompData
	/// and getInternCompData methods.
	/// 
	/// </summary>
	/// <seealso cref="jj2000.j2k.icc.ICCProfile">
	/// </seealso>
	/// <version> 	1.0
	/// </version>
	/// <author> 	Bruce A. Kern
	/// </author>
	public class ICCProfiler:ColorSpaceMapper
	{
		
		/// <summary>The prefix for ICC Profiler options </summary>
		new public const char OPT_PREFIX = 'I';
		
		/// <summary>Platform dependant end of line String. </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'eol '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		new protected internal static readonly System.String eol = System.Environment.NewLine;
		
		// Renamed for convenience:
		//UPGRADE_NOTE: Final was removed from the declaration of 'GRAY '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		//UPGRADE_NOTE: The initialization of  'GRAY' was moved to static method 'icc.ICCProfiler'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1005'"
		private static readonly int GRAY;
		//UPGRADE_NOTE: Final was removed from the declaration of 'RED '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		//UPGRADE_NOTE: The initialization of  'RED' was moved to static method 'icc.ICCProfiler'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1005'"
		private static readonly int RED;
		//UPGRADE_NOTE: Final was removed from the declaration of 'GREEN '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		//UPGRADE_NOTE: The initialization of  'GREEN' was moved to static method 'icc.ICCProfiler'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1005'"
		private static readonly int GREEN;
		//UPGRADE_NOTE: Final was removed from the declaration of 'BLUE '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		//UPGRADE_NOTE: The initialization of  'BLUE' was moved to static method 'icc.ICCProfiler'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1005'"
		private static readonly int BLUE;
		
		// ICCProfiles.
		internal RestrictedICCProfile ricc = null;
		internal ICCProfile icc = null;
		
		// Temporary variables needed during profiling.
		private DataBlkInt[] tempInt; // Holds the results of the transform.
		private DataBlkFloat[] tempFloat; // Holds the results of the transform.
		
		private System.Object xform = null;
		
		/// <summary>The image's ICC profile. </summary>
		private RestrictedICCProfile iccp = null;
		
		/// <summary> Factory method for creating instances of this class.</summary>
		/// <param name="src">-- source of image data
		/// </param>
		/// <param name="csMap">-- provides colorspace info
		/// </param>
		/// <returns> ICCProfiler instance
		/// </returns>
		/// <exception cref="IOException">profile access exception
		/// </exception>
		/// <exception cref="ICCProfileException">profile content exception
		/// </exception>
		public static new BlkImgDataSrc createInstance(BlkImgDataSrc src, CSJ2K.Color.ColorSpace csMap)
		{
			return new ICCProfiler(src, csMap);
		}
		
		/// <summary> Ctor which creates an ICCProfile for the image and initializes
		/// all data objects (input, working, output).
		/// 
		/// </summary>
		/// <param name="src">-- Source of image data
		/// </param>
		/// <param name="csm">-- provides colorspace info
		/// 
		/// </param>
		/// <exception cref="IOException">
		/// </exception>
		/// <exception cref="ICCProfileException">
		/// </exception>
		/// <exception cref="IllegalArgumentException">
		/// </exception>
		protected internal ICCProfiler(BlkImgDataSrc src, CSJ2K.Color.ColorSpace csMap):base(src, csMap)
		{
			initialize();
			
			iccp = getICCProfile(csMap);
			if (ncomps == 1)
			{
				xform = new MonochromeTransformTosRGB(iccp, maxValueArray[0], shiftValueArray[0]);
			}
			else
			{
				xform = new MatrixBasedTransformTosRGB(iccp, maxValueArray, shiftValueArray);
			}
			
			/* end ICCProfiler ctor */
		}
		
		/// <summary>General utility used by ctors </summary>
		private void  initialize()
		{
			
			tempInt = new DataBlkInt[ncomps];
			tempFloat = new DataBlkFloat[ncomps];
			
			/* For each component, get the maximum data value, a reference
			* to the pixel data and set up working and temporary DataBlks
			* for both integer and float output.
			*/
			for (int i = 0; i < ncomps; ++i)
			{
				tempInt[i] = new DataBlkInt();
				tempFloat[i] = new DataBlkFloat();
			}
		}
		
		/// <summary> Get the ICCProfile information JP2 ColorSpace</summary>
		/// <param name="csm">provides all necessary info about the colorspace
		/// </param>
		/// <returns> ICCMatrixBasedInputProfile for 3 component input and
		/// ICCMonochromeInputProfile for a 1 component source.  Returns
		/// null if exceptions were encountered.
		/// </returns>
		/// <exception cref="ColorSpaceException">
		/// </exception>
		/// <exception cref="ICCProfileException">
		/// </exception>
		/// <exception cref="IllegalArgumentException">
		/// </exception>
		private RestrictedICCProfile getICCProfile(CSJ2K.Color.ColorSpace csm)
		{
			
			switch (ncomps)
			{
				
				case 1: 
					icc = ICCMonochromeInputProfile.createInstance(csm);
					ricc = icc.parse();
					if (ricc.Type != RestrictedICCProfile.kMonochromeInput)
						throw new System.ArgumentException("wrong ICCProfile type" + " for image");
					break;
				
				case 3: 
					icc = ICCMatrixBasedInputProfile.createInstance(csm);
					ricc = icc.parse();
					if (ricc.Type != RestrictedICCProfile.kThreeCompInput)
						throw new System.ArgumentException("wrong ICCProfile type" + " for image");
					break;
				
				default: 
					throw new System.ArgumentException("illegal number of " + "components (" + ncomps + ") in image");
				
			}
			return ricc;
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
		/// <P>If the data array in 'blk' is 'null', then a new one is created. If
		/// the data array is not 'null' then it is reused, and it must be large
		/// enough to contain the block's data. Otherwise an 'ArrayStoreException'
		/// or an 'IndexOutOfBoundsException' is thrown by the Java system.
		/// 
		/// <P>The returned data has its 'progressive' attribute set to that of the
		/// input data.
		/// 
		/// </summary>
		/// <param name="out">Its coordinates and dimensions specify the area to
		/// return. If it contains a non-null data array, then it must have the
		/// correct dimensions. If it contains a null data array a new one is
		/// created. The fields in this object are modified to return the data.
		/// 
		/// </param>
		/// <param name="c">The index of the component from which to get the data. Only 0
		/// and 3 are valid.
		/// 
		/// </param>
		/// <returns> The requested DataBlk
		/// 
		/// </returns>
		/// <seealso cref="getInternCompData">
		/// 
		/// </seealso>
		public override DataBlk getCompData(DataBlk outblk, int c)
		{
			
			try
			{
				if (ncomps != 1 && ncomps != 3)
				{
					System.String msg = "ICCProfiler: icc profile _not_ applied to " + ncomps + " component image";
					FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.WARNING, msg);
					return src.getCompData(outblk, c);
				}
				
				int type = outblk.DataType;
				
				int leftedgeOut = - 1; // offset to the start of the output scanline
				int rightedgeOut = - 1; // offset to the end of the output
				// scanline + 1
				int leftedgeIn = - 1; // offset to the start of the input scanline  
				int rightedgeIn = - 1; // offset to the end of the input
				// scanline + 1
				
				// Calculate all components:
				for (int i = 0; i < ncomps; ++i)
				{
					
					int fixedPtBits = src.getFixedPoint(i);
					int shiftVal = shiftValueArray[i];
					int maxVal = maxValueArray[i];
					
					// Initialize general input and output indexes
					int kOut = - 1;
					int kIn = - 1;
					
					switch (type)
					{
						
						// Int and Float data only
						case DataBlk.TYPE_INT: 
							
							// Set up the DataBlk geometry
							copyGeometry(workInt[i], outblk);
							copyGeometry(tempInt[i], outblk);
							copyGeometry(inInt[i], outblk);
							InternalBuffer = outblk;
							
							// Reference the output array
							workDataInt[i] = (int[]) workInt[i].Data;
							
							// Request data from the source.    
							inInt[i] = (DataBlkInt) src.getInternCompData(inInt[i], i);
							dataInt[i] = inInt[i].DataInt;
							
							// The nitty-gritty.
							
							for (int row = 0; row < outblk.h; ++row)
							{
								leftedgeIn = inInt[i].offset + row * inInt[i].scanw;
								rightedgeIn = leftedgeIn + inInt[i].w;
								leftedgeOut = outblk.offset + row * outblk.scanw;
								rightedgeOut = leftedgeOut + outblk.w;
								
								for (kOut = leftedgeOut, kIn = leftedgeIn; kIn < rightedgeIn; ++kIn, ++kOut)
								{
									int tmpInt = (dataInt[i][kIn] >> fixedPtBits) + shiftVal;
									workDataInt[i][kOut] = ((tmpInt < 0)?0:((tmpInt > maxVal)?maxVal:tmpInt));
								}
							}
							break;
						
						
						case DataBlk.TYPE_FLOAT: 
							
							// Set up the DataBlk geometry
							copyGeometry(workFloat[i], outblk);
							copyGeometry(tempFloat[i], outblk);
							copyGeometry(inFloat[i], outblk);
							InternalBuffer = outblk;
							
							// Reference the output array
							workDataFloat[i] = (float[]) workFloat[i].Data;
							
							// Request data from the source.    
							inFloat[i] = (DataBlkFloat) src.getInternCompData(inFloat[i], i);
							dataFloat[i] = inFloat[i].DataFloat;
							
							// The nitty-gritty.
							
							for (int row = 0; row < outblk.h; ++row)
							{
								leftedgeIn = inFloat[i].offset + row * inFloat[i].scanw;
								rightedgeIn = leftedgeIn + inFloat[i].w;
								leftedgeOut = outblk.offset + row * outblk.scanw;
								rightedgeOut = leftedgeOut + outblk.w;
								
								for (kOut = leftedgeOut, kIn = leftedgeIn; kIn < rightedgeIn; ++kIn, ++kOut)
								{
									float tmpFloat = dataFloat[i][kIn] / (1 << fixedPtBits) + shiftVal;
									workDataFloat[i][kOut] = ((tmpFloat < 0)?0:((tmpFloat > maxVal)?maxVal:tmpFloat));
								}
							}
							break;
						
						
						case DataBlk.TYPE_SHORT: 
						case DataBlk.TYPE_BYTE: 
						default: 
							// Unsupported output type. 
							throw new System.ArgumentException("Invalid source " + "datablock type");
						}
				}
				
				switch (type)
				{
					
					// Int and Float data only
					case DataBlk.TYPE_INT: 
						
						if (ncomps == 1)
						{
							((MonochromeTransformTosRGB) xform).apply(workInt[c], tempInt[c]);
						}
						else
						{
							// ncomps == 3
							((MatrixBasedTransformTosRGB) xform).apply(workInt, tempInt);
						}
						
						outblk.progressive = inInt[c].progressive;
						outblk.Data = tempInt[c].Data;
						break;
					
					
					case DataBlk.TYPE_FLOAT: 
						
						if (ncomps == 1)
						{
							((MonochromeTransformTosRGB) xform).apply(workFloat[c], tempFloat[c]);
						}
						else
						{
							// ncomps == 3
							((MatrixBasedTransformTosRGB) xform).apply(workFloat, tempFloat);
						}
						
						outblk.progressive = inFloat[c].progressive;
						outblk.Data = tempFloat[c].Data;
						break;
					
					
					case DataBlk.TYPE_SHORT: 
					case DataBlk.TYPE_BYTE: 
					default: 
						// Unsupported output type. 
						throw new System.ArgumentException("invalid source datablock" + " type");
					}
				
				// Initialize the output block geometry and set the profiled
				// data into the output block.
				outblk.offset = 0;
				outblk.scanw = outblk.w;
			}
			catch (MatrixBasedTransformException e)
			{
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
				FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.ERROR, "matrix transform problem:\n" + e.Message);
				if (pl.getParameter("debug").Equals("on"))
				{
					SupportClass.WriteStackTrace(e, Console.Error);
				}
				else
				{
					FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.ERROR, "Use '-debug' option for more details");
				}
				return null;
			}
			catch (MonochromeTransformException e)
			{
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
				FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.ERROR, "monochrome transform problem:\n" + e.Message);
				if (pl.getParameter("debug").Equals("on"))
				{
					SupportClass.WriteStackTrace(e, Console.Error);
				}
				else
				{
					FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.ERROR, "Use '-debug' option for more details");
				}
				return null;
			}
			
			return outblk;
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
		public override DataBlk getInternCompData(DataBlk out_Renamed, int c)
		{
			return getCompData(out_Renamed, c);
		}
		
		/// <summary>Return a suitable String representation of the class instance. </summary>
		public override System.String ToString()
		{
			System.Text.StringBuilder rep = new System.Text.StringBuilder("[ICCProfiler:");
			System.Text.StringBuilder body = new System.Text.StringBuilder();
			if (icc != null)
			{
				body.Append(eol).Append(CSJ2K.Color.ColorSpace.indent("  ", icc.ToString()));
			}
			if (xform != null)
			{
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Object.toString' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
				body.Append(eol).Append(CSJ2K.Color.ColorSpace.indent("  ", xform.ToString()));
			}
			rep.Append(CSJ2K.Color.ColorSpace.indent("  ", body));
			return rep.Append("]").ToString();
		}
		
		/* end class ICCProfiler */
		static ICCProfiler()
		{
			GRAY = RestrictedICCProfile.GRAY;
			RED = RestrictedICCProfile.RED;
			GREEN = RestrictedICCProfile.GREEN;
			BLUE = RestrictedICCProfile.BLUE;
		}
	}
}