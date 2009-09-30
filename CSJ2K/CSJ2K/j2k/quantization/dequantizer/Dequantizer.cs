/*
* CVS identifier:
*
* $Id: Dequantizer.java,v 1.37 2001/10/29 20:07:28 qtxjoas Exp $
*
* Class:                   Dequantizer
*
* Description:             The abstract class for all dequantizers.
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
using CSJ2K.j2k.image.invcomptransf;
using CSJ2K.j2k.wavelet.synthesis;
using CSJ2K.j2k.entropy.decoder;
using CSJ2K.j2k.codestream;
using CSJ2K.j2k.entropy;
using CSJ2K.j2k.decoder;
using CSJ2K.j2k.wavelet;
using CSJ2K.j2k.image;
using CSJ2K.j2k.io;
using CSJ2K.j2k;
namespace CSJ2K.j2k.quantization.dequantizer
{
	
	/// <summary> This is the abstract class from which all dequantizers must inherit. This
	/// class has the concept of a current tile and all operations are performed on
	/// the current tile.
	/// 
	/// <p>This class provides default implemenations for most of the methods
	/// (wherever it makes sense), under the assumption that the image and
	/// component dimensions, and the tiles, are not modifed by the dequantizer. If
	/// that is not the case for a particular implementation then the methods
	/// should be overriden.</p>
	/// 
	/// <p>Sign magnitude representation is used (instead of two's complement) for
	/// the input data. The most significant bit is used for the sign (0 if
	/// positive, 1 if negative). Then the magnitude of the quantized coefficient
	/// is stored in the next most significat bits. The most significant magnitude
	/// bit corresponds to the most significant bit-plane and so on.</p>
	/// 
	/// <p>The output data is either in floating-point, or in fixed-point two's
	/// complement. In case of floating-point data the the value returned by
	/// getFixedPoint() must be 0. If the case of fixed-point data the number of
	/// fractional bits must be defined at the constructor of the implementing
	/// class and all operations must be performed accordingly. Each component may
	/// have a different number of fractional bits.</p>
	/// 
	/// </summary>
	public abstract class Dequantizer:MultiResImgDataAdapter, CBlkWTDataSrcDec
	{
		/// <summary> Returns the horizontal code-block partition origin. Allowable values
		/// are 0 and 1, nothing else.
		/// 
		/// </summary>
		virtual public int CbULX
		{
			get
			{
				return src.CbULX;
			}
			
		}
		/// <summary> Returns the vertical code-block partition origin. Allowable values are
		/// 0 and 1, nothing else.
		/// 
		/// </summary>
		virtual public int CbULY
		{
			get
			{
				return src.CbULY;
			}
			
		}
		/// <summary> Returns the parameters that are used in this class and
		/// implementing classes. It returns a 2D String array. Each of the
		/// 1D arrays is for a different option, and they have 3
		/// elements. The first element is the option name, the second one
		/// is the synopsis and the third one is a long description of what
		/// the parameter is. The synopsis or description may be 'null', in
		/// which case it is assumed that there is no synopsis or
		/// description of the option, respectively. Null may be returned
		/// if no options are supported.
		/// 
		/// </summary>
		/// <returns> the options name, their synopsis and their explanation, 
		/// or null if no options are supported.
		/// 
		/// </returns>
		public static System.String[][] ParameterInfo
		{
			get
			{
				return pinfo;
			}
			
		}
		
		/// <summary>The prefix for dequantizer options: 'Q' </summary>
		public const char OPT_PREFIX = 'Q';
		
		/// <summary>The list of parameters that is accepted by the bit stream
		/// readers. They start with 'Q' 
		/// </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'pinfo'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		private static readonly System.String[][] pinfo = null;
		
		/// <summary>The entropy decoder from where to get the quantized data (the
		/// source). 
		/// </summary>
		protected internal CBlkQuantDataSrcDec src;
		
		/// <summary>The "range bits" for each transformed component </summary>
		protected internal int[] rb = null;
		
		/// <summary>The "range bits" for each un-transformed component </summary>
		protected internal int[] utrb = null;
		
		/// <summary>The inverse component transformation specifications </summary>
		private CompTransfSpec cts;
		
		/// <summary>Reference to the wavelet filter specifications </summary>
		private SynWTFilterSpec wfs;
		
		/// <summary> Initializes the source of compressed data.
		/// 
		/// </summary>
		/// <param name="src">From where to obtain the quantized data.
		/// 
		/// </param>
		/// <param name="rb">The number of "range bits" for each component (must be the
		/// "range bits" of the un-transformed components. For a definition of
		/// "range bits" see the getNomRangeBits() method.
		/// 
		/// </param>
		/// <seealso cref="getNomRangeBits">
		/// 
		/// </seealso>
		public Dequantizer(CBlkQuantDataSrcDec src, int[] utrb, DecoderSpecs decSpec):base(src)
		{
			if (utrb.Length != src.NumComps)
			{
				throw new System.ArgumentException();
			}
			this.src = src;
			this.utrb = utrb;
			this.cts = decSpec.cts;
			this.wfs = decSpec.wfs;
		}
		
		/// <summary> Returns the number of bits, referred to as the "range bits",
		/// corresponding to the nominal range of the data in the specified
		/// component.
		/// 
		/// <p>The returned value corresponds to the nominal dynamic range of the
		/// reconstructed image data, not of the wavelet coefficients
		/// themselves. This is because different subbands have different gains and
		/// thus different nominal ranges. To have an idea of the nominal range in
		/// each subband the subband analysis gain value from the subband tree
		/// structure, returned by the getSynSubbandTree() method, can be used. See
		/// the Subband class for more details.</p>
		/// 
		/// <p>If this number is <i>b</b> then for unsigned data the nominal range
		/// is between 0 and 2^b-1, and for signed data it is between -2^(b-1) and
		/// 2^(b-1)-1.</p>
		/// 
		/// </summary>
		/// <param name="c">The index of the component
		/// 
		/// </param>
		/// <returns> The number of bits corresponding to the nominal range of the
		/// data.
		/// 
		/// </returns>
		/// <seealso cref="Subband">
		/// 
		/// </seealso>
		public virtual int getNomRangeBits(int c)
		{
			return rb[c];
		}
		
		/// <summary> Returns the subband tree, for the specified tile-component. This method
		/// returns the root element of the subband tree structure, see Subband and
		/// SubbandSyn. The tree comprises all the available resolution levels.
		/// 
		/// <P>The number of magnitude bits ('magBits' member variable) for each
		/// subband may have not been not initialized (it depends on the actual
		/// dequantizer and its implementation). However, they are not necessary
		/// for the subsequent steps in the decoder chain.
		/// 
		/// </summary>
		/// <param name="t">The index of the tile, from 0 to T-1.
		/// 
		/// </param>
		/// <param name="c">The index of the component, from 0 to C-1.
		/// 
		/// </param>
		/// <returns> The root of the tree structure.
		/// 
		/// </returns>
		public override SubbandSyn getSynSubbandTree(int t, int c)
		{
			return src.getSynSubbandTree(t, c);
		}
		
		/// <summary> Changes the current tile, given the new indexes. An
		/// IllegalArgumentException is thrown if the indexes do not
		/// correspond to a valid tile.
		/// 
		/// <P>This default implementation changes the tile in the source
		/// and re-initializes properly component transformation variables..
		/// 
		/// </summary>
		/// <param name="x">The horizontal index of the tile.
		/// 
		/// </param>
		/// <param name="y">The vertical index of the new tile.
		/// 
		/// </param>
		public override void  setTile(int x, int y)
		{
			src.setTile(x, y);
			tIdx = TileIdx; // index of the current tile
			
			// initializations
			int cttype = 0;
			if (((System.Int32) cts.getTileDef(tIdx)) == InvCompTransf.NONE)
				cttype = InvCompTransf.NONE;
			else
			{
				int nc = src.NumComps > 3?3:src.NumComps;
				int rev = 0;
				for (int c = 0; c < nc; c++)
					rev += (wfs.isReversible(tIdx, c)?1:0);
				if (rev == 3)
				{
					// All WT are reversible
					cttype = InvCompTransf.INV_RCT;
				}
				else if (rev == 0)
				{
					// All WT irreversible
					cttype = InvCompTransf.INV_ICT;
				}
				else
				{
					// Error
					throw new System.ArgumentException("Wavelet transformation " + "and " + "component transformation" + " not coherent in tile" + tIdx);
				}
			}
			
			switch (cttype)
			{
				
				case InvCompTransf.NONE: 
					rb = utrb;
					break;
				
				case InvCompTransf.INV_RCT: 
					rb = InvCompTransf.calcMixedBitDepths(utrb, InvCompTransf.INV_RCT, null);
					break;
				
				case InvCompTransf.INV_ICT: 
					rb = InvCompTransf.calcMixedBitDepths(utrb, InvCompTransf.INV_ICT, null);
					break;
				
				default: 
					throw new System.ArgumentException("Non JPEG 2000 part I " + "component" + " transformation for tile: " + tIdx);
				
			}
		}
		
		/// <summary> Advances to the next tile, in standard scan-line order (by rows then
		/// columns). An NoNextElementException is thrown if the current tile is
		/// the last one (i.e. there is no next tile).
		/// 
		/// <P>This default implementation just advances to the next tile in the
		/// source and re-initializes properly component transformation variables.
		/// 
		/// </summary>
		public override void  nextTile()
		{
			src.nextTile();
			tIdx = TileIdx; // index of the current tile
			
			// initializations
			int cttype = ((System.Int32) cts.getTileDef(tIdx));
			switch (cttype)
			{
				
				case InvCompTransf.NONE: 
					rb = utrb;
					break;
				
				case InvCompTransf.INV_RCT: 
					rb = InvCompTransf.calcMixedBitDepths(utrb, InvCompTransf.INV_RCT, null);
					break;
				
				case InvCompTransf.INV_ICT: 
					rb = InvCompTransf.calcMixedBitDepths(utrb, InvCompTransf.INV_ICT, null);
					break;
				
				default: 
					throw new System.ArgumentException("Non JPEG 2000 part I " + "component" + " transformation for tile: " + tIdx);
				
			}
		}
		public abstract CSJ2K.j2k.image.DataBlk getCodeBlock(int param1, int param2, int param3, CSJ2K.j2k.wavelet.synthesis.SubbandSyn param4, CSJ2K.j2k.image.DataBlk param5);
		public abstract int getFixedPoint(int param1);
		public abstract CSJ2K.j2k.image.DataBlk getInternCodeBlock(int param1, int param2, int param3, CSJ2K.j2k.wavelet.synthesis.SubbandSyn param4, CSJ2K.j2k.image.DataBlk param5);
	}
}