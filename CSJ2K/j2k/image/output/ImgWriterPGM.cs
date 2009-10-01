/*
* CVS identifier:
*
* $Id: ImgWriterPGM.java,v 1.14 2002/07/19 14:13:38 grosbois Exp $
*
* Class:                   ImgWriterRawPGM
*
* Description:             Image writer for unsigned 8 bit data in
*                          PGM file format.
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
using CSJ2K.j2k.image;
using CSJ2K.j2k.util;
namespace CSJ2K.j2k.image.output
{
	
	/// <summary> This class writes a component from an image in 8 bit unsigned data to a
	/// binary PGM file. The size of the image that is written to the file is the
	/// size of the component from which to get the data, not the size of the
	/// source image (they differ if there is some sub-sampling).
	/// 
	/// <p>Before writing, all coefficients are inversly level shifted and then
	/// "saturated" (they are limited to the nominal dynamic range).<br> <u>Ex:</u>
	/// if the nominal range is 0-255, the following algorithm is applied:<br>
	/// <tt>if coeff<0, output=0<br> if coeff>255, output=255<br> else
	/// output=coeff</tt></p>
	/// 
	/// <p>The write() methods of an object of this class may not be called
	/// concurrently from different threads.</p>
	/// 
	/// <p>NOTE: This class is not thread safe, for reasons of internal
	/// buffering.</p>
	/// 
	/// </summary>
	public class ImgWriterPGM:ImgWriter
	{
		
		/// <summary>Value used to inverse level shift </summary>
		private int levShift;
		
		/// <summary>Where to write the data </summary>
		//UPGRADE_TODO: Class 'java.io.RandomAccessFile' was converted to 'System.IO.FileStream' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javaioRandomAccessFile'"
		private System.IO.FileStream out_Renamed;
		
		/// <summary>The index of the component from where to get the data </summary>
		private int c;
		
		/// <summary>The number of fractional bits in the source data </summary>
		private int fb;
		
		/// <summary>A DataBlk, just used to avoid allocating a new one each time
		/// it is needed 
		/// </summary>
		private DataBlkInt db = new DataBlkInt();
		
		/// <summary>The offset of the raw pixel data in the PGM file </summary>
		private int offset;
		
		/// <summary>The line buffer. </summary>
		// This makes the class not thrad safe
		// (but it is not the only one making it so)
		private byte[] buf;
		
		/// <summary> Creates a new writer to the specified File object, to write data from
		/// the specified component.
		/// 
		/// <p>The size of the image that is written to the file is the size of the
		/// component from which to get the data, specified by b, not the size of
		/// the source image (they differ if there is some sub-sampling).</p>
		/// 
		/// </summary>
		/// <param name="out">The file where to write the data
		/// 
		/// </param>
		/// <param name="imgSrc">The source from where to get the image data to write.
		/// 
		/// </param>
		/// <param name="c">The index of the component from where to get the data.
		/// 
		/// </param>
		public ImgWriterPGM(System.IO.FileInfo out_Renamed, BlkImgDataSrc imgSrc, int c)
		{
			// Check that imgSrc is of the correct type
			// Check that the component index is valid
			if (c < 0 || c >= imgSrc.NumComps)
			{
				throw new System.ArgumentException("Invalid number of components");
			}
			
			// Check that imgSrc is of the correct type
			if (imgSrc.getNomRangeBits(c) > 8)
			{
				FacilityManager.getMsgLogger().println("Warning: Component " + c + " has nominal bitdepth " + imgSrc.getNomRangeBits(c) + ". Pixel values will be " + "down-shifted to fit bitdepth of 8 for PGM file", 8, 8);
			}
			
			// Initialize
			bool tmpBool;
			if (System.IO.File.Exists(out_Renamed.FullName))
				tmpBool = true;
			else
				tmpBool = System.IO.Directory.Exists(out_Renamed.FullName);
			bool tmpBool2;
			if (System.IO.File.Exists(out_Renamed.FullName))
			{
				System.IO.File.Delete(out_Renamed.FullName);
				tmpBool2 = true;
			}
			else if (System.IO.Directory.Exists(out_Renamed.FullName))
			{
				System.IO.Directory.Delete(out_Renamed.FullName);
				tmpBool2 = true;
			}
			else
				tmpBool2 = false;
			if (tmpBool && !tmpBool2)
			{
				throw new System.IO.IOException("Could not reset file");
			}
			this.out_Renamed = SupportClass.RandomAccessFileSupport.CreateRandomAccessFile(out_Renamed, "rw");
			src = imgSrc;
			this.c = c;
			w = imgSrc.ImgWidth;
			h = imgSrc.ImgHeight;
			fb = imgSrc.getFixedPoint(c);
			levShift = 1 << (imgSrc.getNomRangeBits(c) - 1);
			
			writeHeaderInfo();
		}
		
		/// <summary> Creates a new writer to the specified file, to write data from the
		/// specified component.
		/// 
		/// <P>The size of the image that is written to the file is the size of the
		/// component from which to get the data, specified by b, not the size of
		/// the source image (they differ if there is some sub-sampling).
		/// 
		/// </summary>
		/// <param name="fname">The name of the file where to write the data
		/// 
		/// </param>
		/// <param name="imgSrc">The source from where to get the image data to write.
		/// 
		/// </param>
		/// <param name="c">The index of the component from where to get the data.
		/// 
		/// </param>
		public ImgWriterPGM(System.String fname, BlkImgDataSrc imgSrc, int c):this(new System.IO.FileInfo(fname), imgSrc, c)
		{
		}
		
		/// <summary> Closes the underlying file or netwrok connection to where the data is
		/// written. Any call to other methods of the class become illegal after a
		/// call to this one.
		/// 
		/// </summary>
		/// <exception cref="IOException">If an I/O error occurs.
		/// 
		/// </exception>
		public override void  close()
		{
			int i;
			// Finish writing the file, writing 0s at the end if the data at end
			// has not been written.
			if (out_Renamed.Length != w * h + offset)
			{
				// Goto end of file
				out_Renamed.Seek(out_Renamed.Length, System.IO.SeekOrigin.Begin);
				// Fill with 0s
				for (i = offset + w * h - (int) out_Renamed.Length; i > 0; i--)
				{
					out_Renamed.WriteByte((System.Byte) 0);
				}
			}
			out_Renamed.Close();
			src = null;
			out_Renamed = null;
			db = null;
		}
		
		/// <summary> Writes all buffered data to the file or resource.
		/// 
		/// </summary>
		/// <exception cref="IOException">If an I/O error occurs.
		/// 
		/// </exception>
		public override void  flush()
		{
			// No flush needed here since we are using a RandomAccessFile Get rid
			// of line buffer (is this a good choice?)
			buf = null;
		}
		
		/// <summary> Writes the data of the specified area to the file, coordinates are
		/// relative to the current tile of the source. Before writing, the
		/// coefficients are limited to the nominal range.
		/// 
		/// <p>This method may not be called concurrently from different
		/// threads.</p>
		/// 
		/// <p>If the data returned from the BlkImgDataSrc source is progressive,
		/// then it is requested over and over until it is not progressive
		/// anymore.</p>
		/// 
		/// </summary>
		/// <param name="ulx">The horizontal coordinate of the upper-left corner of the
		/// area to write, relative to the current tile.
		/// 
		/// </param>
		/// <param name="uly">The vertical coordinate of the upper-left corner of the area
		/// to write, relative to the current tile.
		/// 
		/// </param>
		/// <param name="width">The width of the area to write.
		/// 
		/// </param>
		/// <param name="height">The height of the area to write.
		/// 
		/// </param>
		/// <exception cref="IOException">If an I/O error occurs.
		/// 
		/// </exception>
		public override void  write(int ulx, int uly, int w, int h)
		{
			int k, i, j;
			int fracbits = fb; // In local variable for faster access
			int tOffx, tOffy; // Active tile offset in the X and Y direction
			
			// Initialize db
			db.ulx = ulx;
			db.uly = uly;
			db.w = w;
			db.h = h;
			// Get the current active tile offset
			//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
			tOffx = src.getCompULX(c) - (int) System.Math.Ceiling(src.ImgULX / (double) src.getCompSubsX(c));
			//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
			tOffy = src.getCompULY(c) - (int) System.Math.Ceiling(src.ImgULY / (double) src.getCompSubsY(c));
			// Check the array size
			if (db.data_array != null && db.data_array.Length < w * h)
			{
				// A new one will be allocated by getInternCompData()
				db.data_array = null;
			}
			// Request the data and make sure it is not
			// progressive
			do 
			{
				db = (DataBlkInt) src.getInternCompData(db, c);
			}
			while (db.progressive);
			
			// variables used during coeff saturation
			int tmp, maxVal = (1 << src.getNomRangeBits(c)) - 1;
			
			// If nominal bitdepth greater than 8, calculate down shift
			int downShift = src.getNomRangeBits(c) - 8;
			if (downShift < 0)
			{
				downShift = 0;
			}
			
			// Check line buffer
			if (buf == null || buf.Length < w)
			{
				buf = new byte[w]; // Expand buffer
			}
			
			// Write line by line
			for (i = 0; i < h; i++)
			{
				// Skip to beggining of line in file
				out_Renamed.Seek(this.offset + this.w * (uly + tOffy + i) + ulx + tOffx, System.IO.SeekOrigin.Begin);
				// Write all bytes in the line
				if (fracbits == 0)
				{
					for (k = db.offset + i * db.scanw + w - 1, j = w - 1; j >= 0; j--, k--)
					{
						tmp = db.data_array[k] + levShift;
						buf[j] = (byte) (((tmp < 0)?0:((tmp > maxVal)?maxVal:tmp)) >> downShift);
					}
				}
				else
				{
					for (k = db.offset + i * db.scanw + w - 1, j = w - 1; j >= 0; j--, k--)
					{
						tmp = (db.data_array[k] >> fracbits) + levShift;
						buf[j] = (byte) (((tmp < 0)?0:((tmp > maxVal)?maxVal:tmp)) >> downShift);
					}
				}
				out_Renamed.Write(buf, 0, w);
			}
		}
		
		/// <summary> Writes the source's current tile to the output. The requests of data
		/// issued to the source BlkImgDataSrc object are done by strips, in order
		/// to reduce memory usage.
		/// 
		/// <p>If the data returned from the BlkImgDataSrc source is progressive,
		/// then it is requested over and over until it is not progressive
		/// anymore.</p>
		/// 
		/// </summary>
		/// <exception cref="IOException">If an I/O error occurs.
		/// 
		/// </exception>
		/// <seealso cref="DataBlk">
		/// 
		/// </seealso>
		public override void  write()
		{
			int i;
			int tIdx = src.TileIdx;
			int tw = src.getTileCompWidth(tIdx, c); // Tile width
			int th = src.getTileCompHeight(tIdx, c); // Tile height
			// Write in strips
			for (i = 0; i < th; i += DEF_STRIP_HEIGHT)
			{
				write(0, i, tw, (th - i < DEF_STRIP_HEIGHT)?th - i:DEF_STRIP_HEIGHT);
			}
		}
		
		/// <summary> Writes the header info of the PGM file :
		/// 
		/// P5
		/// width height
		/// 255
		/// 
		/// </summary>
		/// <exception cref="IOException">If there is an IOException
		/// 
		/// </exception>
		private void  writeHeaderInfo()
		{
			byte[] byteVals;
			int i;
			System.String val;
			
			// write 'P5' to file
			out_Renamed.WriteByte((System.Byte) 'P'); // 'P'
			out_Renamed.WriteByte((System.Byte) '5'); // '5'
			out_Renamed.WriteByte((System.Byte) '\n'); // newline
			offset = 3;
			// Write width in ASCII
			val = System.Convert.ToString(w);
			byteVals = System.Text.ASCIIEncoding.ASCII.GetBytes(val);
			for (i = 0; i < byteVals.Length; i++)
			{
				out_Renamed.WriteByte((byte) byteVals[i]);
				offset++;
			}
			out_Renamed.WriteByte((System.Byte) ' '); // blank
			offset++;
			// Write height in ASCII
			val = System.Convert.ToString(h);
			byteVals = System.Text.ASCIIEncoding.ASCII.GetBytes(val);
			for (i = 0; i < byteVals.Length; i++)
			{
				out_Renamed.WriteByte((byte) byteVals[i]);
				offset++;
			}
			// Write maxval
			out_Renamed.WriteByte((System.Byte) '\n'); // newline
			out_Renamed.WriteByte((System.Byte) '2'); // '2'
			out_Renamed.WriteByte((System.Byte) '5'); // '5'
			out_Renamed.WriteByte((System.Byte) '5'); // '5'
			out_Renamed.WriteByte((System.Byte) '\n'); // newline
			offset += 5;
		}
		
		/// <summary> Returns a string of information about the object, more than 1 line
		/// long. The information string includes information from the underlying
		/// RandomAccessFile (its toString() method is called in turn).
		/// 
		/// </summary>
		/// <returns> A string of information about the object.
		/// 
		/// </returns>
		public override System.String ToString()
		{
			//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Object.toString' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
			return "ImgWriterPGM: WxH = " + w + "x" + h + ", Component=" + c + "\nUnderlying RandomAccessFile:\n" + out_Renamed.ToString();
		}
	}
}