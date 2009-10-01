/// <summary>**************************************************************************
/// 
/// $Id: SYccColorSpaceMapper.java,v 1.1 2002/07/25 14:52:01 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>
using System;
using ParameterList = CSJ2K.j2k.util.ParameterList;
using BlkImgDataSrc = CSJ2K.j2k.image.BlkImgDataSrc;
using DataBlk = CSJ2K.j2k.image.DataBlk;
using DataBlkInt = CSJ2K.j2k.image.DataBlkInt;
using DataBlkFloat = CSJ2K.j2k.image.DataBlkFloat;
using ImgDataAdapter = CSJ2K.j2k.image.ImgDataAdapter;
using FacilityManager = CSJ2K.j2k.util.FacilityManager;
using MsgLogger = CSJ2K.j2k.util.MsgLogger;
namespace CSJ2K.Color
{
	
	
	/// <summary> This decodes maps which are defined in the e-sRGB 
	/// colorspace into the sRGB colorspace.
	/// 
	/// </summary>
	/// <seealso cref="jj2000.j2k.colorspace.ColorSpace">
	/// </seealso>
	/// <version> 	1.0
	/// </version>
	/// <author> 	Jason S. Clary
	/// </author>
	public class EsRgbColorSpaceMapper : ColorSpaceMapper
	{
		
		/// <summary> Factory method for creating instances of this class.</summary>
		/// <param name="src">-- source of image data
		/// </param>
		/// <param name="csMap">-- provides colorspace info
		/// </param>
		/// <returns> SYccColorSpaceMapper instance
		/// </returns>
		public static new BlkImgDataSrc createInstance(BlkImgDataSrc src, ColorSpace csMap)
		{
			return new EsRgbColorSpaceMapper(src, csMap);
		}
		
		/// <summary> Ctor which creates an ICCProfile for the image and initializes
		/// all data objects (input, working, and output).
		/// 
		/// </summary>
		/// <param name="src">-- Source of image data
		/// </param>
		/// <param name="csm">-- provides colorspace info
		/// </param>
		protected internal EsRgbColorSpaceMapper(BlkImgDataSrc src, ColorSpace csMap):base(src, csMap)
		{
			initialize();
		}
		
		/// <summary>General utility used by ctors </summary>
		private void  initialize()
		{
			
			if (ncomps != 1 && ncomps != 3)
			{
				System.String msg = "EsRgbColorSpaceMapper: e-sRGB transformation _not_ applied to " + ncomps + " component image";
				FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.ERROR, msg);
				throw new ColorSpaceException(msg);
			}
		}
		
		
		/// <summary> 
        /// <para>Returns, in the blk argument, a block of image data containing the
		/// specifed rectangular area, in the specified component. The data is
		/// returned, as a copy of the internal data, therefore the returned data
		/// can be modified "in place".</para>
		/// 
		/// <para>The rectangular area to return is specified by the 'ulx', 'uly', 'w'
		/// and 'h' members of the 'blk' argument, relative to the current
		/// tile. These members are not modified by this method. The 'offset' of
		/// the returned data is 0, and the 'scanw' is the same as the block's
		/// width. See the 'DataBlk' class.</para>
		/// 
		/// <para>If the data array in 'blk' is 'null', then a new one is created. If
		/// the data array is not 'null' then it is reused, and it must be large
		/// enough to contain the block's data. Otherwise an 'ArrayStoreException'
		/// or an 'IndexOutOfBoundsException' is thrown by the Java system.</para>
		/// 
		/// <para>The returned data has its 'progressive' attribute set to that of the
		/// input data.</para>
		/// 
		/// </summary>
		/// <param name="blk">Its coordinates and dimensions specify the area to
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
		/// </returns>
		/// <seealso cref="getInternCompData">
		/// 
		/// </seealso>
		public override DataBlk getCompData(DataBlk outblk, int c)
		{
			
			int type = outblk.DataType;
            double colors = Math.Pow(2, src.getNomRangeBits(c));
	        double bitoff=colors*0.375D;
            if (type == DataBlk.TYPE_INT)
            {
                DataBlkInt intblk=(DataBlkInt)src.getInternCompData(outblk, c);

                for (int i = 0; i < intblk.data_array.Length; i++)
                {
                    int tmp = intblk.data_array[i];

                    tmp += (int)(colors / 2);
                    tmp -= (int)bitoff;
                    tmp *= 2;
                    tmp -= (int)(colors / 2);

                    intblk.data_array[i] = tmp;
                }
                outblk = intblk;
            }
            else if (type == DataBlk.TYPE_FLOAT)
            {
                FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.WARNING, "Unsupported e-sRGB DataType (float)");

                DataBlkFloat fltblk = (DataBlkFloat)src.getInternCompData(outblk, c);
                outblk = fltblk;
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
		/// </seealso>
		public override DataBlk getInternCompData(DataBlk out_Renamed, int c)
		{
			return getCompData(out_Renamed, c);
		}
		
		/// <summary>Return a suitable String representation of the class instance. </summary>
		public override System.String ToString()
		{
			int i;
			
			System.Text.StringBuilder rep_nComps = new System.Text.StringBuilder("ncomps= ").Append(System.Convert.ToString(ncomps));
			System.Text.StringBuilder rep_comps = new System.Text.StringBuilder();
			
			for (i = 0; i < ncomps; ++i)
			{
				rep_comps.Append("  ").Append("component[").Append(System.Convert.ToString(i)).Append("] height, width = (").Append(src.getCompImgHeight(i)).Append(", ").Append(src.getCompImgWidth(i)).Append(")").Append(eol);
			}
			
			System.Text.StringBuilder rep = new System.Text.StringBuilder("[EsRGBColorSpaceMapper ");
			rep.Append(rep_nComps).Append(eol);
			rep.Append(rep_comps).Append("  ");
			
			return rep.Append("]").ToString();
		}
	}
}