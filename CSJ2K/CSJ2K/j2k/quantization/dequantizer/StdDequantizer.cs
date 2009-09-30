/*
* CVS identifier:
*
* $Id: StdDequantizer.java,v 1.15 2002/07/19 12:50:23 grosbois Exp $
*
* Class:                   StdDequantizer
*
* Description:             Scalar deadzone dequantizer that returns integers
*                          or floats.
*                          This is a merger of the ScalarDZDeqInt and
*                          ScalarDZDeqFloat classes by Joel Askelof and Diego
*                          Santa Cruz.
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
using CSJ2K.j2k.wavelet.synthesis;
using CSJ2K.j2k.entropy.decoder;
using CSJ2K.j2k.quantization;
using CSJ2K.j2k.codestream;
using CSJ2K.j2k.entropy;
using CSJ2K.j2k.decoder;
using CSJ2K.j2k.image;
using CSJ2K.j2k.io;
namespace CSJ2K.j2k.quantization.dequantizer
{
	
	/// <summary> This class implements a scalar dequantizer with deadzone. The output can be
	/// either integer ('int') or floating-point ('float') data. The dequantization
	/// step sizes and other parameters are taken from a StdDequantizerParams
	/// class, which inherits from DequantizerParams.
	/// 
	/// <p>Sign magnitude representation is used (instead of two's complement) for
	/// the input data. The most significant bit is used for the sign (0 if
	/// positive, 1 if negative). Then the magnitude of the quantized coefficient
	/// is stored in the next most significat bits. The most significant magnitude
	/// bit corresponds to the most significant bit-plane and so on.</p>
	/// 
	/// <p>When reversible quantization is used, this class only converts between
	/// the sign-magnitude representation and the integer (or eventually
	/// fixed-point) output, since there is no true quantization.</p>
	/// 
	/// <p>The output data is fixed-point two's complement for 'int' output and
	/// floating-point for 'float' output. The type of output and the number number
	/// of fractional bits for 'int' output are defined at the constructor. Each
	/// component may have a different number of fractional bits.</p>
	/// 
	/// <p>The reconstruction levels used by the dequantizer are exactly what is
	/// received from the entropy decoder. It is assumed that the entropy decoder
	/// always returns codewords that are midways in the decoded intervals. In this
	/// way the dequantized values will always lie midways in the quantization
	/// intervals.</p>
	/// 
	/// </summary>
	public class StdDequantizer:Dequantizer
	{
		
		/// <summary>The quantizer type spec </summary>
		private QuantTypeSpec qts;
		
		/// <summary>The quantizer step sizes  spec </summary>
		private QuantStepSizeSpec qsss;
		
		/// <summary>The number of guard bits spec </summary>
		private GuardBitsSpec gbs;
		
		/// <summary>The decoding parameters of the dequantizer </summary>
		//private StdDequantizerParams params_Renamed;
		
		/// <summary>The 'DataBlkInt' object used to request data, used when output data is
		/// not int 
		/// </summary>
		private DataBlkInt inblk;
		
		/// <summary>Type of the current output data </summary>
		private int outdtype;
		
		/// <summary> Initializes the source of compressed data. And sets the number of range
		/// bits and fraction bits and receives the parameters for the dequantizer.
		/// 
		/// </summary>
		/// <param name="src">From where to obtain the quantized data.
		/// 
		/// </param>
		/// <param name="rb">The number of "range bits" (bitdepth) for each component
		/// (must be the "range bits" of the un-transformed components). For a
		/// definition of "range bits" see the getNomRangeBits() method.
		/// 
		/// </param>
		/// <param name="qts">The quantizer type spec
		/// 
		/// </param>
		/// <param name="qsss">The dequantizer step sizes spec
		/// 
		/// </param>
		/// <seealso cref="Dequantizer.getNomRangeBits">
		/// 
		/// </seealso>
		/// <exception cref="IllegalArgumentException">Thrown if 'outdt' is neither
		/// TYPE_FLOAT nor TYPE_INT, or if 'param' specify reversible quantization
		/// and 'outdt' is not TYPE_INT or 'fp' has non-zero values, or if 'outdt'
		/// is TYPE_FLOAT and 'fp' has non-zero values.
		/// 
		/// </exception>
		public StdDequantizer(CBlkQuantDataSrcDec src, int[] utrb, DecoderSpecs decSpec):base(src, utrb, decSpec)
		{
			
			if (utrb.Length != src.NumComps)
			{
				throw new System.ArgumentException("Invalid rb argument");
			}
			this.qsss = decSpec.qsss;
			this.qts = decSpec.qts;
			this.gbs = decSpec.gbs;
		}
		
		/// <summary> Returns the position of the fixed point in the output data for the
		/// specified component. This is the position of the least significant
		/// integral (i.e. non-fractional) bit, which is equivalent to the number
		/// of fractional bits. For instance, for fixed-point values with 2
		/// fractional bits, 2 is returned. For floating-point data this value does
		/// not apply and 0 should be returned. Position 0 is the position of the
		/// least significant bit in the data. If the output data is 'float' then 0
		/// is always returned.
		/// 
		/// <p><u>Note:</u> Fractional bits are no more supported by JJ2000.</p>
		/// 
		/// </summary>
		/// <param name="c">The index of the component.
		/// 
		/// </param>
		/// <returns> The position of the fixed-point, which is the same as the
		/// number of fractional bits. For floating-point data 0 is returned.
		/// 
		/// </returns>
		public override int getFixedPoint(int c)
		{
			return 0;
		}
		
		/// <summary> Returns the specified code-block in the current tile for the specified
		/// component, as a copy (see below).
		/// 
		/// <p>The returned code-block may be progressive, which is indicated by
		/// the 'progressive' variable of the returned 'DataBlk' object. If a
		/// code-block is progressive it means that in a later request to this
		/// method for the same code-block it is possible to retrieve data which is
		/// a better approximation, since meanwhile more data to decode for the
		/// code-block could have been received. If the code-block is not
		/// progressive then later calls to this method for the same code-block
		/// will return the exact same data values.</p>
		/// 
		/// <p>The data returned by this method is always a copy of the internal
		/// data of this object, if any, and it can be modified "in place" without
		/// any problems after being returned. The 'offset' of the returned data is 
		/// 0, and the 'scanw' is the same as the code-block width. See the
		/// 'DataBlk' class.</p>
		/// 
		/// </summary>
		/// <param name="c">The component for which to return the next code-block.
		/// 
		/// </param>
		/// <param name="m">The vertical index of the code-block to return, in the
		/// specified subband.
		/// 
		/// </param>
		/// <param name="n">The horizontal index of the code-block to return, in the
		/// specified subband.
		/// 
		/// </param>
		/// <param name="sb">The subband in which the code-block to return is.
		/// 
		/// </param>
		/// <param name="cblk">If non-null this object will be used to return the new
		/// code-block. If null a new one will be allocated and returned. If the
		/// "data" array of the object is non-null it will be reused, if possible,
		/// to return the data.
		/// 
		/// </param>
		/// <returns> The next code-block in the current tile for component 'n', or
		/// null if all code-blocks for the current tile have been returned.
		/// 
		/// </returns>
		/// <seealso cref="DataBlk">
		/// 
		/// </seealso>
		public override DataBlk getCodeBlock(int c, int m, int n, SubbandSyn sb, DataBlk cblk)
		{
			return getInternCodeBlock(c, m, n, sb, cblk);
		}
		
		/// <summary> Returns the specified code-block in the current tile for the specified
		/// component (as a reference or copy).
		/// 
		/// <p>The returned code-block may be progressive, which is indicated by
		/// the 'progressive' variable of the returned 'DataBlk'
		/// object. If a code-block is progressive it means that in a later request
		/// to this method for the same code-block it is possible to retrieve data
		/// which is a better approximation, since meanwhile more data to decode
		/// for the code-block could have been received. If the code-block is not
		/// progressive then later calls to this method for the same code-block
		/// will return the exact same data values.</p>
		/// 
		/// <p>The data returned by this method can be the data in the internal
		/// buffer of this object, if any, and thus can not be modified by the
		/// caller. The 'offset' and 'scanw' of the returned data can be
		/// arbitrary. See the 'DataBlk' class.</p>
		/// 
		/// </summary>
		/// <param name="c">The component for which to return the next code-block.
		/// 
		/// </param>
		/// <param name="m">The vertical index of the code-block to return, in the
		/// specified subband.
		/// 
		/// </param>
		/// <param name="n">The horizontal index of the code-block to return, in the
		/// specified subband.
		/// 
		/// </param>
		/// <param name="sb">The subband in which the code-block to return is.
		/// 
		/// </param>
		/// <param name="cblk">If non-null this object will be used to return the new
		/// code-block. If null a new one will be allocated and returned. If the
		/// "data" array of the object is non-null it will be reused, if possible,
		/// to return the data.
		/// 
		/// </param>
		/// <returns> The next code-block in the current tile for component 'n', or
		/// null if all code-blocks for the current tile have been returned.
		/// 
		/// </returns>
		/// <seealso cref="DataBlk">
		/// 
		/// </seealso>
		public override DataBlk getInternCodeBlock(int c, int m, int n, SubbandSyn sb, DataBlk cblk)
		{
			// This method is declared final since getNextCodeBlock() relies on
			// the actual implementation of this method.
			int j, jmin, k;
			int temp;
			float step;
			int shiftBits;
			int magBits;
			int[] outiarr, inarr;
			float[] outfarr;
			int w, h;
			bool reversible = qts.isReversible(tIdx, c);
			bool derived = qts.isDerived(tIdx, c);
			StdDequantizerParams params_Renamed = (StdDequantizerParams) qsss.getTileCompVal(tIdx, c);
			int G = ((System.Int32) gbs.getTileCompVal(tIdx, c));
			
			outdtype = cblk.DataType;
			
			if (reversible && outdtype != DataBlk.TYPE_INT)
			{
				throw new System.ArgumentException("Reversible quantizations " + "must use int data");
			}
			
			// To get compiler happy
			outiarr = null;
			outfarr = null;
			inarr = null;
			
			// Get source data and initialize output DataBlk object.
			switch (outdtype)
			{
				
				case DataBlk.TYPE_INT: 
					// With int data we can use the same DataBlk object to get the
					// data from the source and return the dequantized data, and we
					// can also work "in place" (i.e. same buffer).
					cblk = src.getCodeBlock(c, m, n, sb, cblk);
					// Input and output arrays are the same
					outiarr = (int[]) cblk.Data;
					break;
				
				case DataBlk.TYPE_FLOAT: 
					// With float data we must use a different DataBlk objects to get
					// the data from the source and to return the dequantized data.
					inblk = (DataBlkInt) src.getInternCodeBlock(c, m, n, sb, inblk);
					inarr = inblk.DataInt;
					if (cblk == null)
					{
						cblk = new DataBlkFloat();
					}
					// Copy the attributes of the CodeBlock object
					cblk.ulx = inblk.ulx;
					cblk.uly = inblk.uly;
					cblk.w = inblk.w;
					cblk.h = inblk.h;
					cblk.offset = 0;
					cblk.scanw = cblk.w;
					cblk.progressive = inblk.progressive;
					// Get output data array and check its size
					outfarr = (float[]) cblk.Data;
					if (outfarr == null || outfarr.Length < cblk.w * cblk.h)
					{
						outfarr = new float[cblk.w * cblk.h];
						cblk.Data = outfarr;
					}
					break;
				}
			
			magBits = sb.magbits;
			
			// Calculate quantization step and number of magnitude bits
			// depending on reversibility and derivedness and perform
			// inverse quantization
			if (reversible)
			{
				shiftBits = 31 - magBits;
				// For int data Inverse quantization happens "in-place". The input
				// array has an offset of 0 and scan width equal to the code-block
				// width.
				for (j = outiarr.Length - 1; j >= 0; j--)
				{
					temp = outiarr[j]; // input array is same as output one
					outiarr[j] = (temp >= 0)?(temp >> shiftBits):- ((temp & 0x7FFFFFFF) >> shiftBits);
				}
			}
			else
			{
				// Not reversible 
				if (derived)
				{
					// Max resolution level
					int mrl = src.getSynSubbandTree(TileIdx, c).resLvl;
					step = params_Renamed.nStep[0][0] * (1L << (rb[c] + sb.anGainExp + mrl - sb.level));
				}
				else
				{
					step = params_Renamed.nStep[sb.resLvl][sb.sbandIdx] * (1L << (rb[c] + sb.anGainExp));
				}
				shiftBits = 31 - magBits;
				
				// Adjust step to the number of shiftBits
				step /= (1 << shiftBits);
				
				switch (outdtype)
				{
					
					case DataBlk.TYPE_INT: 
						// For int data Inverse quantization happens "in-place". The
						// input array has an offset of 0 and scan width equal to the
						// code-block width.
						for (j = outiarr.Length - 1; j >= 0; j--)
						{
							temp = outiarr[j]; // input array is same as output one
							//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
							outiarr[j] = (int) (((float) ((temp >= 0)?temp:- (temp & 0x7FFFFFFF))) * step);
						}
						break;
					
					case DataBlk.TYPE_FLOAT: 
						// For float data the inverse quantization can not happen
						// "in-place".
						w = cblk.w;
						h = cblk.h;
						for (j = w * h - 1, k = inblk.offset + (h - 1) * inblk.scanw + w - 1, jmin = w * (h - 1); j >= 0; jmin -= w)
						{
							for (; j >= jmin; k--, j--)
							{
								temp = inarr[k];
								//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
								outfarr[j] = ((float) ((temp >= 0)?temp:- (temp & 0x7FFFFFFF))) * step;
							}
							// Jump to beggining of previous line in input
							k -= (inblk.scanw - w);
						}
						break;
					}
			}
			// Return the output code-block
			return cblk;
		}
	}
}