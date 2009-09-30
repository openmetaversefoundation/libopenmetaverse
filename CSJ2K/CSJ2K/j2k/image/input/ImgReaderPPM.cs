/*
*
* Class:                   ImgWriterPPM
*
* Description:             Image writer for unsigned 8 bit data in
*                          PPM files.
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
using CSJ2K.j2k.io;
using CSJ2K.j2k;
namespace CSJ2K.j2k.image.input
{
	
	/// <summary> This class implements the ImgData interface for reading 8 bits unsigned
	/// data from a binary PPM file
	/// 
	/// <P> After being read the coefficients are level shifted by subtracting
	/// 2^(nominal bit range - 1)
	/// 
	/// <P>The transfer type (see ImgData) of this class is TYPE_INT.
	/// 
	/// <P>This class is <i>buffered</i>: the 3 input components(R,G,B) are read
	/// when the first one (R) is asked. The 2 others are stored until they are
	/// needed.
	/// 
	/// <P>NOTE: This class is not thread safe, for reasons of internal buffering.
	/// 
	/// </summary>
	/// <seealso cref="jj2000.j2k.image.ImgData">
	/// 
	/// </seealso>
	public class ImgReaderPPM:ImgReader
	{
		
		/// <summary>DC offset value used when reading image </summary>
		public static int DC_OFFSET = 128;
		
		/// <summary>Where to read the data from </summary>
		//UPGRADE_TODO: Class 'java.io.RandomAccessFile' was converted to 'System.IO.FileStream' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javaioRandomAccessFile'"
		private System.IO.FileStream in_Renamed;
		
		/// <summary>The offset of the raw pixel data in the PPM file </summary>
		private int offset;
		
		/// <summary>The number of bits that determine the nominal dynamic range </summary>
		private int rb;
		
		/// <summary>Buffer for the 3 components of each pixel(in the current block) </summary>
		private int[][] barr = new int[3][];
		
		/// <summary>Data block used only to store coordinates of the buffered blocks </summary>
		private DataBlkInt dbi = new DataBlkInt();
		
		/// <summary>The line buffer. </summary>
		// This makes the class not thread safe (but it is not the only one making
		// it so)
		private byte[] buf;
		
		/// <summary>Temporary DataBlkInt object (needed when encoder uses floating-point
		/// filters). This avoid allocating new DataBlk at each time 
		/// </summary>
		private DataBlkInt intBlk;
		
		/// <summary> Creates a new PPM file reader from the specified file.
		/// 
		/// </summary>
		/// <param name="file">The input file.
		/// 
		/// </param>
		/// <param name="IOException">If an error occurs while opening the file.
		/// 
		/// </param>
		public ImgReaderPPM(System.IO.FileInfo file):this(SupportClass.RandomAccessFileSupport.CreateRandomAccessFile(file, "r"))
		{
		}
		
		/// <summary> Creates a new PPM file reader from the specified file name.
		/// 
		/// </summary>
		/// <param name="fname">The input file name.
		/// 
		/// </param>
		/// <param name="IOException">If an error occurs while opening the file.
		/// 
		/// </param>
		public ImgReaderPPM(System.String fname):this(SupportClass.RandomAccessFileSupport.CreateRandomAccessFile(fname, "r"))
		{
		}
		
		/// <summary> Creates a new PPM file reader from the specified RandomAccessFile
		/// object. The file header is read to acquire the image size.
		/// 
		/// </summary>
		/// <param name="in">From where to read the data
		/// 
		/// </param>
		/// <exception cref="EOFException">if an EOF is read
		/// </exception>
		/// <exception cref="IOException">if an error occurs when opening the file
		/// 
		/// </exception>
		//UPGRADE_TODO: Class 'java.io.RandomAccessFile' was converted to 'System.IO.FileStream' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javaioRandomAccessFile'"
		private ImgReaderPPM(System.IO.FileStream in_Renamed)
		{
			this.in_Renamed = in_Renamed;
			
			confirmFileType();
			skipCommentAndWhiteSpace();
			w = readHeaderInt();
			skipCommentAndWhiteSpace();
			h = readHeaderInt();
			skipCommentAndWhiteSpace();
			/*Read the highest pixel value from header (not used)*/
			readHeaderInt();
			nc = 3;
			rb = 8;
		}
		
		/// <summary> Closes the underlying file from where the image data is being read. No
		/// operations are possible after a call to this method.
		/// 
		/// </summary>
		/// <exception cref="IOException">If an I/O error occurs.
		/// 
		/// </exception>
		public override void  close()
		{
			in_Renamed.Close();
			in_Renamed = null;
			// Free memory
			barr[0] = null;
			barr[1] = null;
			barr[2] = null;
			buf = null;
		}
		
		
		/// <summary> Returns the number of bits corresponding to the nominal range of the
		/// data in the specified component. This is the value rb (range bits) that
		/// was specified in the constructor, which normally is 8 for non bilevel
		/// data, and 1 for bilevel data.
		/// 
		/// <P>If this number is <i>b</b> then the nominal range is between
		/// -2^(b-1) and 2^(b-1)-1, since unsigned data is level shifted to have a
		/// nominal avergae of 0.
		/// 
		/// </summary>
		/// <param name="c">The index of the component.
		/// 
		/// </param>
		/// <returns> The number of bits corresponding to the nominal range of the
		/// data. For floating-point data this value is not applicable and the
		/// return value is undefined.
		/// 
		/// </returns>
		public override int getNomRangeBits(int c)
		{
			// Check component index
			if (c < 0 || c > 2)
				throw new System.ArgumentException();
			
			return rb;
		}
		
		/// <summary> Returns the position of the fixed point in the specified component
		/// (i.e. the number of fractional bits), which is always 0 for this
		/// ImgReader.
		/// 
		/// </summary>
		/// <param name="c">The index of the component.
		/// 
		/// </param>
		/// <returns> The position of the fixed-point (i.e. the number of fractional
		/// bits). Always 0 for this ImgReader.
		/// 
		/// </returns>
		public override int getFixedPoint(int c)
		{
			// Check component index
			if (c < 0 || c > 2)
				throw new System.ArgumentException();
			return 0;
		}
		
		
		/// <summary> Returns, in the blk argument, the block of image data containing the
		/// specifed rectangular area, in the specified component. The data is
		/// returned, as a reference to the internal data, if any, instead of as a
		/// copy, therefore the returned data should not be modified.
		/// 
		/// <P> After being read the coefficients are level shifted by subtracting
		/// 2^(nominal bit range - 1)
		/// 
		/// <P>The rectangular area to return is specified by the 'ulx', 'uly', 'w'
		/// and 'h' members of the 'blk' argument, relative to the current
		/// tile. These members are not modified by this method. The 'offset' and
		/// 'scanw' of the returned data can be arbitrary. See the 'DataBlk' class.
		/// 
		/// <P>If the data array in <tt>blk</tt> is <tt>null</tt>, then a new one
		/// is created if necessary. The implementation of this interface may
		/// choose to return the same array or a new one, depending on what is more
		/// efficient. Therefore, the data array in <tt>blk</tt> prior to the
		/// method call should not be considered to contain the returned data, a
		/// new array may have been created. Instead, get the array from
		/// <tt>blk</tt> after the method has returned.
		/// 
		/// <P>The returned data always has its 'progressive' attribute unset
		/// (i.e. false).
		/// 
		/// <P>When an I/O exception is encountered the JJ2KExceptionHandler is
		/// used. The exception is passed to its handleException method. The action
		/// that is taken depends on the action that has been registered in
		/// JJ2KExceptionHandler. See JJ2KExceptionHandler for details.
		/// 
		/// <P>This method implements buffering for the 3 components: When the
		/// first one is asked, all the 3 components are read and stored until they
		/// are needed.
		/// 
		/// </summary>
		/// <param name="blk">Its coordinates and dimensions specify the area to
		/// return. Some fields in this object are modified to return the data.
		/// 
		/// </param>
		/// <param name="c">The index of the component from which to get the data. Only 0,
		/// 1 and 3 are valid.
		/// 
		/// </param>
		/// <returns> The requested DataBlk
		/// 
		/// </returns>
		/// <seealso cref="getCompData">
		/// 
		/// </seealso>
		/// <seealso cref="JJ2KExceptionHandler">
		/// 
		/// </seealso>
		public override DataBlk getInternCompData(DataBlk blk, int c)
		{
			// Check component index
			if (c < 0 || c > 2)
				throw new System.ArgumentException();
			
			// Check type of block provided as an argument
			if (blk.DataType != DataBlk.TYPE_INT)
			{
				if (intBlk == null)
					intBlk = new DataBlkInt(blk.ulx, blk.uly, blk.w, blk.h);
				else
				{
					intBlk.ulx = blk.ulx;
					intBlk.uly = blk.uly;
					intBlk.w = blk.w;
					intBlk.h = blk.h;
				}
				blk = intBlk;
			}
			
			// If asking a component for the first time for this block, read the 3
			// components
			if ((barr[c] == null) || (dbi.ulx > blk.ulx) || (dbi.uly > blk.uly) || (dbi.ulx + dbi.w < blk.ulx + blk.w) || (dbi.uly + dbi.h < blk.uly + blk.h))
			{
				int k, j, i, mi;
				int[] red, green, blue;
				
				// Reset data arrays if needed
				if (barr[c] == null || barr[c].Length < blk.w * blk.h)
				{
					barr[c] = new int[blk.w * blk.h];
				}
				blk.Data = barr[c];
				
				i = (c + 1) % 3;
				if (barr[i] == null || barr[i].Length < blk.w * blk.h)
				{
					barr[i] = new int[blk.w * blk.h];
				}
				i = (c + 2) % 3;
				if (barr[i] == null || barr[i].Length < blk.w * blk.h)
				{
					barr[i] = new int[blk.w * blk.h];
				}
				
				// set attributes of the DataBlk used for buffering
				dbi.ulx = blk.ulx;
				dbi.uly = blk.uly;
				dbi.w = blk.w;
				dbi.h = blk.h;
				
				// Check line buffer
				if (buf == null || buf.Length < 3 * blk.w)
				{
					buf = new byte[3 * blk.w];
				}
				
				red = barr[0];
				green = barr[1];
				blue = barr[2];
				
				try
				{
					// Read line by line
					mi = blk.uly + blk.h;
					for (i = blk.uly; i < mi; i++)
					{
						// Reposition in input offset takes care of
						// header offset
						in_Renamed.Seek(offset + i * 3 * w + 3 * blk.ulx, System.IO.SeekOrigin.Begin);
						in_Renamed.Read(buf, 0, 3 * blk.w);
						
						for (k = (i - blk.uly) * blk.w + blk.w - 1, j = 3 * blk.w - 1; j >= 0; k--)
						{
							// Read every third sample
							blue[k] = (((byte) buf[j--]) & 0xFF) - DC_OFFSET;
							green[k] = (((byte) buf[j--]) & 0xFF) - DC_OFFSET;
							red[k] = (((byte) buf[j--]) & 0xFF) - DC_OFFSET;
						}
					}
				}
				catch (System.IO.IOException e)
				{
					JJ2KExceptionHandler.handleException(e);
				}
				barr[0] = red;
				barr[1] = green;
				barr[2] = blue;
				
				// Set buffer attributes
				blk.Data = barr[c];
				blk.offset = 0;
				blk.scanw = blk.w;
			}
			else
			{
				//Asking for the 2nd or 3rd block component
				blk.Data = barr[c];
				blk.offset = (blk.ulx - dbi.ulx) * dbi.w + blk.ulx - dbi.ulx;
				blk.scanw = dbi.scanw;
			}
			
			// Turn off the progressive attribute
			blk.progressive = false;
			return blk;
		}
		
		/// <summary> Returns, in the blk argument, a block of image data containing the
		/// specifed rectangular area, in the specified component. The data is
		/// returned, as a copy of the internal data, therefore the returned data
		/// can be modified "in place".
		/// 
		/// <P> After being read the coefficients are level shifted by subtracting
		/// 2^(nominal bit range - 1)
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
		/// <P>The returned data has its 'progressive' attribute unset
		/// (i.e. false).
		/// 
		/// <P>When an I/O exception is encountered the JJ2KExceptionHandler is
		/// used. The exception is passed to its handleException method. The action
		/// that is taken depends on the action that has been registered in
		/// JJ2KExceptionHandler. See JJ2KExceptionHandler for details.
		/// 
		/// </summary>
		/// <param name="blk">Its coordinates and dimensions specify the area to
		/// return. If it contains a non-null data array, then it must have the
		/// correct dimensions. If it contains a null data array a new one is
		/// created. The fields in this object are modified to return the data.
		/// 
		/// </param>
		/// <param name="c">The index of the component from which to get the data. Only
		/// 0,1 and 2 are valid.
		/// 
		/// </param>
		/// <returns> The requested DataBlk
		/// 
		/// </returns>
		/// <seealso cref="getInternCompData">
		/// 
		/// </seealso>
		/// <seealso cref="JJ2KExceptionHandler">
		/// 
		/// </seealso>
		public override DataBlk getCompData(DataBlk blk, int c)
		{
			// NOTE: can not directly call getInterCompData since that returns
			// internally buffered data.
			int ulx, uly, w, h;
			
			// Check type of block provided as an argument
			if (blk.DataType != DataBlk.TYPE_INT)
			{
				DataBlkInt tmp = new DataBlkInt(blk.ulx, blk.uly, blk.w, blk.h);
				blk = tmp;
			}
			
			int[] bakarr = (int[]) blk.Data;
			// Save requested block size
			ulx = blk.ulx;
			uly = blk.uly;
			w = blk.w;
			h = blk.h;
			// Force internal data buffer to be different from external
			blk.Data = null;
			getInternCompData(blk, c);
			// Copy the data
			if (bakarr == null)
			{
				bakarr = new int[w * h];
			}
			if (blk.offset == 0 && blk.scanw == w)
			{
				// Requested and returned block buffer are the same size
                // CONVERSION PROBLEM?
				Array.Copy((System.Array)blk.Data, 0, (System.Array)bakarr, 0, w * h);
			}
			else
			{
				// Requested and returned block are different
				for (int i = h - 1; i >= 0; i--)
				{
					// copy line by line
                    // CONVERSION PROBLEM?
					Array.Copy((System.Array)blk.Data, blk.offset + i * blk.scanw, (System.Array)bakarr, i * w, w);
				}
			}
			blk.Data = bakarr;
			blk.offset = 0;
			blk.scanw = blk.w;
			return blk;
		}
		
		/// <summary> Returns a byte read from the RandomAccessFile. The number of read byted
		/// are counted to keep track of the offset of the pixel data in the PPM
		/// file
		/// 
		/// </summary>
		/// <returns> One byte read from the header of the PPM file.  
		/// 
		/// </returns>
		private byte countedByteRead()
		{
			offset++;
			return (byte) in_Renamed.ReadByte();
		}
		
		/// <summary> Checks that the file begins with 'P6' 
		/// 
		/// </summary>
		private void  confirmFileType()
		{
			byte[] type = new byte[]{80, 54};
			int i;
			byte b;
			
			for (i = 0; i < 2; i++)
			{
				b = countedByteRead();
				if (b != type[i])
				{
					if (i == 1 && b == 51)
					{
						// i.e 'P3'
						throw new System.ArgumentException("JJ2000 does not support" + " ascii-PPM files. Use " + " raw-PPM file instead. ");
					}
					else
					{
						throw new System.ArgumentException("Not a raw-PPM file");
					}
				}
			}
		}
		
		/// <summary> Skips any line in the header starting with '#' and any space, tab, line
		/// feed or carriage return.
		/// 
		/// </summary>
		private void  skipCommentAndWhiteSpace()
		{
			
			bool done = false;
			byte b;
			
			while (!done)
			{
				b = countedByteRead();
				if (b == 35)
				{
					// Comment start
					while (b != 10 && b != 13)
					{
						// While not comment end (end-of-line)
						b = countedByteRead();
					}
				}
				else if (!(b == 9 || b == 10 || b == 13 || b == 32))
				{
					// If not whitespace
					done = true;
				}
			}
			// Put back last valid byte
			offset--;
			in_Renamed.Seek(offset, System.IO.SeekOrigin.Begin);
		}
		
		/// <summary> Returns an int read from the header of the PPM file.
		/// 
		/// </summary>
		/// <returns> One int read from the header of the PPM file.  
		/// 
		/// </returns>
		private int readHeaderInt()
		{
			int res = 0;
			byte b = 0;
			
			b = countedByteRead();
			while (b != 32 && b != 10 && b != 9 && b != 13)
			{
				// While not whitespace
				res = res * 10 + b - 48; // Convert from ASCII to decimal
				b = countedByteRead();
			}
			return res;
		}
		
		/// <summary> Returns true if the data read was originally signed in the specified
		/// component, false if not. This method always returns false since PPM
		/// data is always unsigned.
		/// 
		/// </summary>
		/// <param name="c">The index of the component, from 0 to N-1.
		/// 
		/// </param>
		/// <returns> always false, since PPM data is always unsigned.
		/// 
		/// </returns>
		public override bool isOrigSigned(int c)
		{
			// Check component index
			if (c < 0 || c > 2)
				throw new System.ArgumentException();
			return false;
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
			return "ImgReaderPPM: WxH = " + w + "x" + h + ", Component = 0,1,2" + "\nUnderlying RandomAccessFile:\n" + in_Renamed.ToString();
		}
	}
}