/*
* CVS Identifier:
*
* $Id: DataBlkInt.java,v 1.7 2001/10/09 12:51:54 grosbois Exp $
*
* Interface:           DataBlkInt
*
* Description:         A signed int implementation of DataBlk
*
*
*
* COPYRIGHT:
* 
* This software module was originally developed by Raphaël Grosbois and
* Diego Santa Cruz (Swiss Federal Institute of Technology-EPFL); Joel
* Askelöf (Ericsson Radio Systems AB); and Bertrand Berthelot, David
* Bouchard, Félix Henry, Gerard Mozelle and Patrice Onno (Canon Research
* Centre France S.A) in the course of development of the JPEG2000
* standard as specified by ISO/IEC 15444 (JPEG 2000 Standard). This
* software module is an implementation of a part of the JPEG 2000
* Standard. Swiss Federal Institute of Technology-EPFL, Ericsson Radio
* Systems AB and Canon Research Centre France S.A (collectively JJ2000
* Partners) agree not to assert against ISO/IEC and users of the JPEG
* 2000 Standard (Users) any of their rights under the copyright, not
* including other intellectual property rights, for this software module
* with respect to the usage by ISO/IEC and Users of this software module
* or modifications thereof for use in hardware or software products
* claiming conformance to the JPEG 2000 Standard. Those intending to use
* this software module in hardware or software products are advised that
* their use may infringe existing patents. The original developers of
* this software module, JJ2000 Partners and ISO/IEC assume no liability
* for use of this software module or modifications thereof. No license
* or right to this software module is granted for non JPEG 2000 Standard
* conforming products. JJ2000 Partners have full right to use this
* software module for his/her own purpose, assign or donate this
* software module to any third party and to inhibit third parties from
* using this software module for non JPEG 2000 Standard conforming
* products. This copyright notice must be included in all copies or
* derivative works of this software module.
* 
* Copyright (c) 1999/2000 JJ2000 Partners.
* */
using System;
namespace CSJ2K.j2k.image
{
	
	/// <summary> This is an implementation of the <tt>DataBlk</tt> interface for signed 32
	/// bit integral data.
	/// 
	/// <p>The methods in this class are declared final, so that they can be
	/// inlined by inlining compilers.</p>
	/// 
	/// </summary>
	/// <seealso cref="DataBlk">
	/// 
	/// </seealso>
	public class DataBlkInt:DataBlk
	{
		/// <summary> Returns the identifier of this data type, <tt>TYPE_INT</tt>, as defined
		/// in <tt>DataBlk</tt>.
		/// 
		/// </summary>
		/// <returns> The type of data stored. Always <tt>DataBlk.TYPE_INT</tt>
		/// 
		/// </returns>
		/// <seealso cref="DataBlk.TYPE_INT">
		/// 
		/// </seealso>
		override public int DataType
		{
			get
			{
				return TYPE_INT;
			}
			
		}
		//UPGRADE_NOTE: Respective javadoc comments were merged.  It should be changed in order to comply with .NET documentation conventions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1199'"
		/// <summary> Returns the array containing the data, or null if there is no data
		/// array. The returned array is a int array.
		/// 
		/// </summary>
		/// <returns> The array of data (a int[]) or null if there is no data.
		/// 
		/// </returns>
		/// <summary> Sets the data array to the specified one. The provided array must be a
		/// int array, otherwise a ClassCastException is thrown. The size of the
		/// array is not checked for consistency with the block's dimensions.
		/// 
		/// </summary>
		/// <param name="arr">The data array to use. Must be a int array.
		/// 
		/// </param>
		override public System.Object Data
		{
			get
			{
				return data_array;
			}
			
			set
			{
				data_array = (int[]) value;
			}
			
		}
		//UPGRADE_NOTE: Respective javadoc comments were merged.  It should be changed in order to comply with .NET documentation conventions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1199'"
		/// <summary> Returns the array containing the data, or null if there is no data
		/// array.
		/// 
		/// </summary>
		/// <returns> The array of data or null if there is no data.
		/// 
		/// </returns>
		/// <summary> Sets the data array to the specified one. The size of the array is not
		/// checked for consistency with the block's dimensions. This method is
		/// more efficient than setData
		/// 
		/// </summary>
		/// <param name="arr">The data array to use.
		/// 
		/// </param>
		virtual public int[] DataInt
		{
			get
			{
				return data_array;
			}
			
			set
			{
				data_array = value;
			}
			
		}
		/// <summary>The array where the data is stored </summary>
		public int[] data_array;
		
		/// <summary> Creates a DataBlkInt with 0 dimensions and no data array (i.e. data is
		/// null).
		/// 
		/// </summary>
		public DataBlkInt()
		{
		}
		
		/// <summary> Creates a DataBlkInt with the specified dimensions and position. The
		/// data array is initialized to an array of size w*h.
		/// 
		/// </summary>
		/// <param name="ulx">The horizontal coordinate of the upper-left corner of the
		/// block
		/// 
		/// </param>
		/// <param name="uly">The vertical coordinate of the upper-left corner of the
		/// block
		/// 
		/// </param>
		/// <param name="w">The width of the block (in pixels)
		/// 
		/// </param>
		/// <param name="h">The height of the block (in pixels)
		/// 
		/// </param>
		public DataBlkInt(int ulx, int uly, int w, int h)
		{
			this.ulx = ulx;
			this.uly = uly;
			this.w = w;
			this.h = h;
			offset = 0;
			scanw = w;
			data_array = new int[w * h];
		}
		
		/// <summary> Copy constructor. Creates a DataBlkInt which is the copy of the
		/// DataBlkInt given as paramter.
		/// 
		/// </summary>
		/// <param name="DataBlkInt">the object to be copied.
		/// 
		/// </param>
		public DataBlkInt(DataBlkInt src)
		{
			this.ulx = src.ulx;
			this.uly = src.uly;
			this.w = src.w;
			this.h = src.h;
			this.offset = 0;
			this.scanw = this.w;
			this.data_array = new int[this.w * this.h];
			for (int i = 0; i < this.h; i++)
                Buffer.BlockCopy(src.data_array, i * src.scanw, this.data_array, i * this.scanw, this.w);
		}
		
		/// <summary> Returns a string of informations about the DataBlkInt.
		/// 
		/// </summary>
		public override System.String ToString()
		{
			System.String str = base.ToString();
			if (data_array != null)
			{
				str += (",data=" + data_array.Length + " bytes");
			}
			return str;
		}
	}
}