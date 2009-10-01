/*
* CVS identifier:
*
* $Id: HeaderEncoder.java,v 1.43 2001/10/12 09:02:14 grosbois Exp $
*
* Class:                   HeaderEncoder
*
* Description:             Write codestream headers.
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
using CSJ2K.j2k.quantization.quantizer;
using CSJ2K.j2k.wavelet.analysis;
using CSJ2K.j2k.entropy.encoder;
using CSJ2K.j2k.quantization;
using CSJ2K.j2k.image.input;
using CSJ2K.j2k.roi.encoder;
using CSJ2K.j2k.codestream;
using CSJ2K.j2k.wavelet;
using CSJ2K.j2k.encoder;
using CSJ2K.j2k.entropy;
using CSJ2K.j2k.image;
using CSJ2K.j2k.util;
using CSJ2K.j2k.io;
using CSJ2K.j2k;
namespace CSJ2K.j2k.codestream.writer
{
	
	/// <summary> This class writes almost of the markers and marker segments in main header
	/// and in tile-part headers. It is created by the run() method of the Encoder
	/// instance.
	/// 
	/// <p>A marker segment includes a marker and eventually marker segment
	/// parameters. It is designed by the three letter code of the marker
	/// associated with the marker segment. JPEG 2000 part I defines 6 types of
	/// markers:
	/// <ul> 
	/// <li>Delimiting : SOC,SOT,SOD,EOC (written in FileCodestreamWriter).</li>
	/// <li>Fixed information: SIZ.</li> 
	/// <li>Functional: COD,COC,RGN,QCD,QCC,POC.</li>
	/// <li> In bit-stream: SOP,EPH.</li>
	/// <li> Pointer: TLM,PLM,PLT,PPM,PPT.</li> 
	/// <li> Informational: CRG,COM.</li>
	/// </ul></p>
	/// 
	/// <p>Main Header is written when Encoder instance calls encodeMainHeader
	/// whereas tile-part headers are written when the EBCOTRateAllocator instance
	/// calls encodeTilePartHeader.</p>
	/// 
	/// </summary>
	/// <seealso cref="Encoder">
	/// </seealso>
	/// <seealso cref="Markers">
	/// </seealso>
	/// <seealso cref="EBCOTRateAllocator">
	/// 
	/// </seealso>
	public class HeaderEncoder
	{
		/// <summary> Returns the parameters that are used in this class and implementing
		/// classes. It returns a 2D String array. Each of the 1D arrays is for a
		/// different option, and they have 3 elements. The first element is the
		/// option name, the second one is the synopsis, the third one is a long
		/// description of what the parameter is and the fourth is its default
		/// value. The synopsis or description may be 'null', in which case it is
		/// assumed that there is no synopsis or description of the option,
		/// respectively. Null may be returned if no options are supported.
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
		/// <summary> Returns the byte-buffer used to store the codestream header.
		/// 
		/// </summary>
		/// <returns> A byte array countaining codestream header
		/// 
		/// </returns>
		virtual protected internal byte[] Buffer
		{
			get
			{
				return baos.ToArray();
			}
			
		}
		/// <summary> Returns the length of the header.
		/// 
		/// </summary>
		/// <returns> The length of the header in bytes
		/// 
		/// </returns>
		virtual public int Length
		{
			get
			{
				return (int)hbuf.BaseStream.Length;
			}
			
		}
		/// <summary> Returns the number of bytes used in the codestream header's buffer.
		/// 
		/// </summary>
		/// <returns> Header length in buffer (without any header overhead)
		/// 
		/// </returns>
		virtual protected internal int BufferLength
		{
			get
			{
				return (int)baos.Length;
			}
			
		}
		
		/// <summary>The prefix for the header encoder options: 'H' </summary>
		public const char OPT_PREFIX = 'H';
		
		/// <summary>The list of parameters that are accepted for the header encoder
		/// module. Options for this modules start with 'H'. 
		/// </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'pinfo'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		private static readonly System.String[][] pinfo = new System.String[][]{new System.String[]{"Hjj2000_COM", null, "Writes or not the JJ2000 COM marker in the " + "codestream", "off"}, new System.String[]{"HCOM", "<Comment 1>[#<Comment 2>[#<Comment3...>]]", "Adds COM marker segments in the codestream. Comments must be " + "separated with '#' and are written into distinct maker segments.", null}};
		
		/// <summary>Nominal range bit of the component defining default values in QCD for
		/// main header 
		/// </summary>
		private int defimgn;
		
		/// <summary>Nominal range bit of the component defining default values in QCD for
		/// tile headers 
		/// </summary>
		private int deftilenr;
		
		/// <summary>The number of components in the image </summary>
		private int nComp;
		
		/// <summary>Whether or not to write the JJ2000 COM marker segment </summary>
		private bool enJJ2KMarkSeg = true;
		
		/// <summary>Other COM marker segments specified in the command line </summary>
		private System.String otherCOMMarkSeg = null;
		
		/// <summary>The ByteArrayOutputStream to store header data. This handler is kept
		/// in order to use methods not accessible from a general
		/// DataOutputStream. For the other methods, it's better to use variable
		/// hbuf.
		/// 
		/// </summary>
		/// <seealso cref="hbuf">
		/// </seealso>
		protected internal System.IO.MemoryStream baos;
		
		/// <summary>The DataOutputStream to store header data. This kind of object is
		/// useful to write short, int, .... It's constructor takes baos as
		/// parameter.
		/// 
		/// </summary>
		/// <seealso cref="baos">
		/// 
		/// </seealso>
		//UPGRADE_TODO: Class 'java.io.DataOutputStream' was converted to 'System.IO.BinaryWriter' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javaioDataOutputStream'"
		protected internal System.IO.BinaryWriter hbuf;
		
		/// <summary>The image data reader. Source of original data info </summary>
		protected internal ImgData origSrc;
		
		/// <summary>An array specifying, for each component,if the data was signed or not
		/// 
		/// </summary>
		protected internal bool[] isOrigSig;
		
		/// <summary>Reference to the rate allocator </summary>
		protected internal PostCompRateAllocator ralloc;
		
		/// <summary>Reference to the DWT module </summary>
		protected internal ForwardWT dwt;
		
		/// <summary>Reference to the tiler module </summary>
		protected internal Tiler tiler;
		
		/// <summary>Reference to the ROI module </summary>
		protected internal ROIScaler roiSc;
		
		/// <summary>The encoder specifications </summary>
		protected internal EncoderSpecs encSpec;
		
		/// <summary> Initializes the header writer with the references to the coding chain.
		/// 
		/// </summary>
		/// <param name="origsrc">The original image data (before any component mixing,
		/// tiling, etc.)
		/// 
		/// </param>
		/// <param name="isorigsig">An array specifying for each component if it was
		/// originally signed or not.
		/// 
		/// </param>
		/// <param name="dwt">The discrete wavelet transform module.
		/// 
		/// </param>
		/// <param name="tiler">The tiler module.
		/// 
		/// </param>
		/// <param name="encSpec">The encoder specifications
		/// 
		/// </param>
		/// <param name="roiSc">The ROI scaler module.
		/// 
		/// </param>
		/// <param name="ralloc">The post compression rate allocator.
		/// 
		/// </param>
		/// <param name="pl">ParameterList instance.
		/// 
		/// </param>
		public HeaderEncoder(ImgData origsrc, bool[] isorigsig, ForwardWT dwt, Tiler tiler, EncoderSpecs encSpec, ROIScaler roiSc, PostCompRateAllocator ralloc, ParameterList pl)
		{
			pl.checkList(OPT_PREFIX, CSJ2K.j2k.util.ParameterList.toNameArray(pinfo));
			if (origsrc.NumComps != isorigsig.Length)
			{
				throw new System.ArgumentException();
			}
			this.origSrc = origsrc;
			this.isOrigSig = isorigsig;
			this.dwt = dwt;
			this.tiler = tiler;
			this.encSpec = encSpec;
			this.roiSc = roiSc;
			this.ralloc = ralloc;
			
			baos = new System.IO.MemoryStream();
			//UPGRADE_TODO: Class 'java.io.DataOutputStream' was converted to 'System.IO.BinaryWriter' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javaioDataOutputStream'"
            hbuf = new CSJ2K.Util.EndianBinaryWriter(baos, true);
			nComp = origsrc.NumComps;
			enJJ2KMarkSeg = pl.getBooleanParameter("Hjj2000_COM");
			otherCOMMarkSeg = pl.getParameter("HCOM");
		}
		
		/// <summary> Resets the contents of this HeaderEncoder to its initial state. It
		/// erases all the data in the header buffer and reactualizes the
		/// headerLength field of the bit stream writer.
		/// 
		/// </summary>
		public virtual void  reset()
		{
			//UPGRADE_ISSUE: Method 'java.io.ByteArrayOutputStream.reset' was not converted. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1000_javaioByteArrayOutputStreamreset'"
			// CONVERSION PROBLEM?
            //baos.reset();
            baos.SetLength(0);
			//UPGRADE_TODO: Class 'java.io.DataOutputStream' was converted to 'System.IO.BinaryWriter' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javaioDataOutputStream'"
            hbuf = new CSJ2K.Util.EndianBinaryWriter(baos, true); //new System.IO.BinaryWriter(baos);
		}
		
		/// <summary> Writes the header to the specified BinaryDataOutput.
		/// 
		/// </summary>
		/// <param name="out">Where to write the header.
		/// 
		/// </param>
		public virtual void  writeTo(BinaryDataOutput out_Renamed)
		{
			int i, len;
			byte[] buf;
			
			buf = Buffer;
			len = Length;
			
			for (i = 0; i < len; i++)
			{
				out_Renamed.writeByte(buf[i]);
			}
		}
		
		/// <summary> Writes the header to the specified OutputStream.
		/// 
		/// </summary>
		/// <param name="out">Where to write the header.
		/// 
		/// </param>
		public virtual void  writeTo(System.IO.Stream out_Renamed)
		{
			out_Renamed.Write(Buffer, 0, BufferLength);
		}
		
		/// <summary> Start Of Codestream marker (SOC) signalling the beginning of a
		/// codestream.
		/// 
		/// </summary>
		private void  writeSOC()
		{
			hbuf.Write((System.Int16) CSJ2K.j2k.codestream.Markers.SOC);
		}
		
		/// <summary> Writes SIZ marker segment of the codestream header. It is a fixed
		/// information marker segment containing informations about image and tile
		/// sizes. It is required in the main header immediately after SOC marker
		/// segment.
		/// 
		/// </summary>
		private void  writeSIZ()
		{
			int tmp;
			
			// SIZ marker
			hbuf.Write((System.Int16) CSJ2K.j2k.codestream.Markers.SIZ);
			
			// Lsiz (Marker length) corresponding to
			// Lsiz(2 bytes)+Rsiz(2)+Xsiz(4)+Ysiz(4)+XOsiz(4)+YOsiz(4)+
			// XTsiz(4)+YTsiz(4)+XTOsiz(4)+YTOsiz(4)+Csiz(2)+
			// (Ssiz(1)+XRsiz(1)+YRsiz(1))*nComp
			// markSegLen = 38 + 3*nComp;
			int markSegLen = 38 + 3 * nComp;
			hbuf.Write((System.Int16) markSegLen);
			
			// Rsiz (codestream capabilities)
			hbuf.Write((System.Int16) 0); // JPEG 2000 - Part I
			
			// Xsiz (original image width)
			hbuf.Write(tiler.ImgWidth + tiler.ImgULX);
			
			// Ysiz (original image height)
			hbuf.Write(tiler.ImgHeight + tiler.ImgULY);
			
			// XOsiz (horizontal offset from the origin of the reference
			// grid to the left side of the image area)
			hbuf.Write(tiler.ImgULX);
			
			// YOsiz (vertical offset from the origin of the reference
			// grid to the top side of the image area)
			hbuf.Write(tiler.ImgULY);
			
			// XTsiz (nominal tile width)
			hbuf.Write(tiler.NomTileWidth);
			
			// YTsiz (nominal tile height)
			hbuf.Write(tiler.NomTileHeight);
			
			Coord torig = tiler.getTilingOrigin(null);
			// XTOsiz (Horizontal offset from the origin of the reference
			// grid to the left side of the first tile)
			hbuf.Write(torig.x);
			
			// YTOsiz (Vertical offset from the origin of the reference
			// grid to the top side of the first tile)
			hbuf.Write(torig.y);
			
			// Csiz (number of components)
			hbuf.Write((System.Int16) nComp);
			
			// Bit-depth and downsampling factors.
			for (int c = 0; c < nComp; c++)
			{
				// Loop on each component
				
				// Ssiz bit-depth before mixing
				tmp = origSrc.getNomRangeBits(c) - 1;
				
				tmp |= ((isOrigSig[c]?1:0) << CSJ2K.j2k.codestream.Markers.SSIZ_DEPTH_BITS);
				hbuf.Write((System.Byte) tmp);
				
				// XRsiz (component sub-sampling value x-wise)
				hbuf.Write((System.Byte) tiler.getCompSubsX(c));
				
				// YRsiz (component sub-sampling value y-wise)
				hbuf.Write((System.Byte) tiler.getCompSubsY(c));
			} // End loop on each component
		}
		
		/// <summary> Writes COD marker segment. COD is a functional marker segment
		/// containing the code style default (coding style, decomposition,
		/// layering) used for compressing all the components in an image.
		/// 
		/// <p>The values can be overriden for an individual component by a COC
		/// marker in either the main or the tile header.</p>
		/// 
		/// </summary>
		/// <param name="mh">Flag indicating whether this marker belongs to the main
		/// header
		/// 
		/// </param>
		/// <param name="tileIdx">Tile index if the marker belongs to a tile-part header
		/// 
		/// </param>
		/// <seealso cref="writeCOC">
		/// 
		/// </seealso>
		protected internal virtual void  writeCOD(bool mh, int tileIdx)
		{
			AnWTFilter[][] filt;
			bool precinctPartitionUsed;
			int tmp;
			int mrl = 0, a = 0;
			int ppx = 0, ppy = 0;
			Progression[] prog;
			
			if (mh)
			{
				mrl = ((System.Int32) encSpec.dls.getDefault());
				// get default precinct size 
				ppx = encSpec.pss.getPPX(- 1, - 1, mrl);
				ppy = encSpec.pss.getPPY(- 1, - 1, mrl);
				prog = (Progression[]) (encSpec.pocs.getDefault());
			}
			else
			{
				mrl = ((System.Int32) encSpec.dls.getTileDef(tileIdx));
				// get precinct size for specified tile
				ppx = encSpec.pss.getPPX(tileIdx, - 1, mrl);
				ppy = encSpec.pss.getPPY(tileIdx, - 1, mrl);
				prog = (Progression[]) (encSpec.pocs.getTileDef(tileIdx));
			}
			
			if (ppx != CSJ2K.j2k.codestream.Markers.PRECINCT_PARTITION_DEF_SIZE || ppy != CSJ2K.j2k.codestream.Markers.PRECINCT_PARTITION_DEF_SIZE)
			{
				precinctPartitionUsed = true;
			}
			else
			{
				precinctPartitionUsed = false;
			}
			
			if (precinctPartitionUsed)
			{
				// If precinct partition is used we add one byte per resolution
				// level i.e. mrl+1 (+1 for resolution 0).
				a = mrl + 1;
			}
			
			// Write COD marker
			hbuf.Write((System.Int16) CSJ2K.j2k.codestream.Markers.COD);
			
			// Lcod (marker segment length (in bytes)) Basic : Lcod(2
			// bytes)+Scod(1)+SGcod(4)+SPcod(5+a)  where:
			// a=0 if no precinct partition is used
			// a=mrl+1 if precinct partition used
			int markSegLen = 12 + a;
			hbuf.Write((System.Int16) markSegLen);
			
			// Scod (coding style parameter)
			tmp = 0;
			if (precinctPartitionUsed)
			{
				tmp = CSJ2K.j2k.codestream.Markers.SCOX_PRECINCT_PARTITION;
			}
			
			// Are SOP markers used ?
			if (mh)
			{
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Object.toString' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
				if (((System.String) encSpec.sops.getDefault().ToString()).ToUpper().Equals("on".ToUpper()))
				{
					tmp |= CSJ2K.j2k.codestream.Markers.SCOX_USE_SOP;
				}
			}
			else
			{
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Object.toString' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
				if (((System.String) encSpec.sops.getTileDef(tileIdx).ToString()).ToUpper().Equals("on".ToUpper()))
				{
					tmp |= CSJ2K.j2k.codestream.Markers.SCOX_USE_SOP;
				}
			}
			
			// Are EPH markers used ?
			if (mh)
			{
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Object.toString' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
				if (((System.String) encSpec.ephs.getDefault().ToString()).ToUpper().Equals("on".ToUpper()))
				{
					tmp |= CSJ2K.j2k.codestream.Markers.SCOX_USE_EPH;
				}
			}
			else
			{
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Object.toString' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
				if (((System.String) encSpec.ephs.getTileDef(tileIdx).ToString()).ToUpper().Equals("on".ToUpper()))
				{
					tmp |= CSJ2K.j2k.codestream.Markers.SCOX_USE_EPH;
				}
			}
			if (dwt.CbULX != 0)
				tmp |= CSJ2K.j2k.codestream.Markers.SCOX_HOR_CB_PART;
			if (dwt.CbULY != 0)
				tmp |= CSJ2K.j2k.codestream.Markers.SCOX_VER_CB_PART;
			hbuf.Write((System.Byte) tmp);
			
			// SGcod
			// Progression order
			hbuf.Write((System.Byte) prog[0].type);
			
			// Number of layers
			hbuf.Write((System.Int16) ralloc.NumLayers);
			
			// Multiple component transform
			// CSsiz (Color transform)
			System.String str = null;
			if (mh)
			{
				str = ((System.String) encSpec.cts.getDefault());
			}
			else
			{
				str = ((System.String) encSpec.cts.getTileDef(tileIdx));
			}
			
			if (str.Equals("none"))
			{
				hbuf.Write((System.Byte) 0);
			}
			else
			{
				hbuf.Write((System.Byte) 1);
			}
			
			// SPcod
			// Number of decomposition levels
			hbuf.Write((System.Byte) mrl);
			
			// Code-block width and height
			if (mh)
			{
				// main header, get default values
				tmp = encSpec.cblks.getCBlkWidth(ModuleSpec.SPEC_DEF, - 1, - 1);
				hbuf.Write((System.Byte) (MathUtil.log2(tmp) - 2));
				tmp = encSpec.cblks.getCBlkHeight(ModuleSpec.SPEC_DEF, - 1, - 1);
				hbuf.Write((System.Byte) (MathUtil.log2(tmp) - 2));
			}
			else
			{
				// tile header, get tile default values
				tmp = encSpec.cblks.getCBlkWidth(ModuleSpec.SPEC_TILE_DEF, tileIdx, - 1);
				hbuf.Write((System.Byte) (MathUtil.log2(tmp) - 2));
				tmp = encSpec.cblks.getCBlkHeight(ModuleSpec.SPEC_TILE_DEF, tileIdx, - 1);
				hbuf.Write((System.Byte) (MathUtil.log2(tmp) - 2));
			}
			
			// Style of the code-block coding passes
			tmp = 0;
			if (mh)
			{
				// Main header
				// Selective arithmetic coding bypass ?
				if (((System.String) encSpec.bms.getDefault()).Equals("on"))
				{
					tmp |= CSJ2K.j2k.entropy.StdEntropyCoderOptions.OPT_BYPASS;
				}
				// MQ reset after each coding pass ?
				if (((System.String) encSpec.mqrs.getDefault()).Equals("on"))
				{
					tmp |= CSJ2K.j2k.entropy.StdEntropyCoderOptions.OPT_RESET_MQ;
				}
				// MQ termination after each arithmetically coded coding pass ?
				if (((System.String) encSpec.rts.getDefault()).Equals("on"))
				{
					tmp |= CSJ2K.j2k.entropy.StdEntropyCoderOptions.OPT_TERM_PASS;
				}
				// Vertically stripe-causal context mode ?
				if (((System.String) encSpec.css.getDefault()).Equals("on"))
				{
					tmp |= CSJ2K.j2k.entropy.StdEntropyCoderOptions.OPT_VERT_STR_CAUSAL;
				}
				// Predictable termination ?
				if (((System.String) encSpec.tts.getDefault()).Equals("predict"))
				{
					tmp |= CSJ2K.j2k.entropy.StdEntropyCoderOptions.OPT_PRED_TERM;
				}
				// Error resilience segmentation symbol insertion ?
				if (((System.String) encSpec.sss.getDefault()).Equals("on"))
				{
					tmp |= CSJ2K.j2k.entropy.StdEntropyCoderOptions.OPT_SEG_SYMBOLS;
				}
			}
			else
			{
				// Tile header
				// Selective arithmetic coding bypass ?
				if (((System.String) encSpec.bms.getTileDef(tileIdx)).Equals("on"))
				{
					tmp |= CSJ2K.j2k.entropy.StdEntropyCoderOptions.OPT_BYPASS;
				}
				// MQ reset after each coding pass ?
				if (((System.String) encSpec.mqrs.getTileDef(tileIdx)).Equals("on"))
				{
					tmp |= CSJ2K.j2k.entropy.StdEntropyCoderOptions.OPT_RESET_MQ;
				}
				// MQ termination after each arithmetically coded coding pass ?
				if (((System.String) encSpec.rts.getTileDef(tileIdx)).Equals("on"))
				{
					tmp |= CSJ2K.j2k.entropy.StdEntropyCoderOptions.OPT_TERM_PASS;
				}
				// Vertically stripe-causal context mode ?
				if (((System.String) encSpec.css.getTileDef(tileIdx)).Equals("on"))
				{
					tmp |= CSJ2K.j2k.entropy.StdEntropyCoderOptions.OPT_VERT_STR_CAUSAL;
				}
				// Predictable termination ?
				if (((System.String) encSpec.tts.getTileDef(tileIdx)).Equals("predict"))
				{
					tmp |= CSJ2K.j2k.entropy.StdEntropyCoderOptions.OPT_PRED_TERM;
				}
				// Error resilience segmentation symbol insertion ?
				if (((System.String) encSpec.sss.getTileDef(tileIdx)).Equals("on"))
				{
					tmp |= CSJ2K.j2k.entropy.StdEntropyCoderOptions.OPT_SEG_SYMBOLS;
				}
			}
			hbuf.Write((System.Byte) tmp);
			
			// Wavelet transform
			// Wavelet Filter
			if (mh)
			{
				filt = ((AnWTFilter[][]) encSpec.wfs.getDefault());
				hbuf.Write((System.Byte) filt[0][0].FilterType);
			}
			else
			{
				filt = ((AnWTFilter[][]) encSpec.wfs.getTileDef(tileIdx));
				hbuf.Write((System.Byte) filt[0][0].FilterType);
			}
			
			// Precinct partition
			if (precinctPartitionUsed)
			{
				// Write the precinct size for each resolution level + 1
				// (resolution 0) if precinct partition is used.
				System.Collections.ArrayList[] v = null;
				if (mh)
				{
					v = (System.Collections.ArrayList[]) encSpec.pss.getDefault();
				}
				else
				{
					v = (System.Collections.ArrayList[]) encSpec.pss.getTileDef(tileIdx);
				}
				for (int r = mrl; r >= 0; r--)
				{
					if (r >= v[1].Count)
					{
						tmp = ((System.Int32) v[1][v[1].Count - 1]);
					}
					else
					{
						tmp = ((System.Int32) v[1][r]);
					}
					int yExp = (MathUtil.log2(tmp) << 4) & 0x00F0;
					
					if (r >= v[0].Count)
					{
						tmp = ((System.Int32) v[0][v[0].Count - 1]);
					}
					else
					{
						tmp = ((System.Int32) v[0][r]);
					}
					int xExp = MathUtil.log2(tmp) & 0x000F;
					hbuf.Write((System.Byte) (yExp | xExp));
				}
			}
		}
		
		/// <summary> Writes COC marker segment . It is a functional marker containing the
		/// coding style for one component (coding style, decomposition, layering).
		/// 
		/// <p>Its values overrides any value previously set in COD in the main
		/// header or in the tile header.</p>
		/// 
		/// </summary>
		/// <param name="mh">Flag indicating whether the main header is to be written. 
		/// 
		/// </param>
		/// <param name="tileIdx">Tile index.
		/// 
		/// </param>
		/// <param name="compIdx">index of the component which need use of the COC marker
		/// segment.
		/// 
		/// </param>
		/// <seealso cref="writeCOD">
		/// 
		/// </seealso>
		protected internal virtual void  writeCOC(bool mh, int tileIdx, int compIdx)
		{
			AnWTFilter[][] filt;
			bool precinctPartitionUsed;
			int tmp;
			int mrl = 0, a = 0;
			int ppx = 0, ppy = 0;
			Progression[] prog;
			
			if (mh)
			{
				mrl = ((System.Int32) encSpec.dls.getCompDef(compIdx));
				// Get precinct size for specified component
				ppx = encSpec.pss.getPPX(- 1, compIdx, mrl);
				ppy = encSpec.pss.getPPY(- 1, compIdx, mrl);
				prog = (Progression[]) (encSpec.pocs.getCompDef(compIdx));
			}
			else
			{
				mrl = ((System.Int32) encSpec.dls.getTileCompVal(tileIdx, compIdx));
				// Get precinct size for specified component/tile
				ppx = encSpec.pss.getPPX(tileIdx, compIdx, mrl);
				ppy = encSpec.pss.getPPY(tileIdx, compIdx, mrl);
				prog = (Progression[]) (encSpec.pocs.getTileCompVal(tileIdx, compIdx));
			}
			
			if (ppx != CSJ2K.j2k.codestream.Markers.PRECINCT_PARTITION_DEF_SIZE || ppy != CSJ2K.j2k.codestream.Markers.PRECINCT_PARTITION_DEF_SIZE)
			{
				precinctPartitionUsed = true;
			}
			else
			{
				precinctPartitionUsed = false;
			}
			if (precinctPartitionUsed)
			{
				// If precinct partition is used we add one byte per resolution 
				// level  i.e. mrl+1 (+1 for resolution 0).
				a = mrl + 1;
			}
			
			// COC marker
			hbuf.Write((System.Int16) CSJ2K.j2k.codestream.Markers.COC);
			
			// Lcoc (marker segment length (in bytes))
			// Basic: Lcoc(2 bytes)+Scoc(1)+ Ccoc(1 or 2)+SPcod(5+a)
			int markSegLen = 8 + ((nComp < 257)?1:2) + a;
			
			// Rounded to the nearest even value greater or equals
			hbuf.Write((System.Int16) markSegLen);
			
			// Ccoc
			if (nComp < 257)
			{
				hbuf.Write((System.Byte) compIdx);
			}
			else
			{
				hbuf.Write((System.Int16) compIdx);
			}
			
			// Scod (coding style parameter)
			tmp = 0;
			if (precinctPartitionUsed)
			{
				tmp = CSJ2K.j2k.codestream.Markers.SCOX_PRECINCT_PARTITION;
			}
			hbuf.Write((System.Byte) tmp);
			
			
			// SPcoc
			
			// Number of decomposition levels
			hbuf.Write((System.Byte) mrl);
			
			// Code-block width and height
			if (mh)
			{
				// main header, get component default values
				tmp = encSpec.cblks.getCBlkWidth(ModuleSpec.SPEC_COMP_DEF, - 1, compIdx);
				hbuf.Write((System.Byte) (MathUtil.log2(tmp) - 2));
				tmp = encSpec.cblks.getCBlkHeight(ModuleSpec.SPEC_COMP_DEF, - 1, compIdx);
				hbuf.Write((System.Byte) (MathUtil.log2(tmp) - 2));
			}
			else
			{
				// tile header, get tile component values
				tmp = encSpec.cblks.getCBlkWidth(ModuleSpec.SPEC_TILE_COMP, tileIdx, compIdx);
				hbuf.Write((System.Byte) (MathUtil.log2(tmp) - 2));
				tmp = encSpec.cblks.getCBlkHeight(ModuleSpec.SPEC_TILE_COMP, tileIdx, compIdx);
				hbuf.Write((System.Byte) (MathUtil.log2(tmp) - 2));
			}
			
			// Entropy coding mode options
			tmp = 0;
			if (mh)
			{
				// Main header
				// Lazy coding mode ?
				if (((System.String) encSpec.bms.getCompDef(compIdx)).Equals("on"))
				{
					tmp |= CSJ2K.j2k.entropy.StdEntropyCoderOptions.OPT_BYPASS;
				}
				// MQ reset after each coding pass ?
				if (((System.String) encSpec.mqrs.getCompDef(compIdx)).ToUpper().Equals("on".ToUpper()))
				{
					tmp |= CSJ2K.j2k.entropy.StdEntropyCoderOptions.OPT_RESET_MQ;
				}
				// MQ termination after each arithmetically coded coding pass ?
				if (((System.String) encSpec.rts.getCompDef(compIdx)).Equals("on"))
				{
					tmp |= CSJ2K.j2k.entropy.StdEntropyCoderOptions.OPT_TERM_PASS;
				}
				// Vertically stripe-causal context mode ?
				if (((System.String) encSpec.css.getCompDef(compIdx)).Equals("on"))
				{
					tmp |= CSJ2K.j2k.entropy.StdEntropyCoderOptions.OPT_VERT_STR_CAUSAL;
				}
				// Predictable termination ?
				if (((System.String) encSpec.tts.getCompDef(compIdx)).Equals("predict"))
				{
					tmp |= CSJ2K.j2k.entropy.StdEntropyCoderOptions.OPT_PRED_TERM;
				}
				// Error resilience segmentation symbol insertion ?
				if (((System.String) encSpec.sss.getCompDef(compIdx)).Equals("on"))
				{
					tmp |= CSJ2K.j2k.entropy.StdEntropyCoderOptions.OPT_SEG_SYMBOLS;
				}
			}
			else
			{
				// Tile Header
				if (((System.String) encSpec.bms.getTileCompVal(tileIdx, compIdx)).Equals("on"))
				{
					tmp |= CSJ2K.j2k.entropy.StdEntropyCoderOptions.OPT_BYPASS;
				}
				// MQ reset after each coding pass ?
				if (((System.String) encSpec.mqrs.getTileCompVal(tileIdx, compIdx)).Equals("on"))
				{
					tmp |= CSJ2K.j2k.entropy.StdEntropyCoderOptions.OPT_RESET_MQ;
				}
				// MQ termination after each arithmetically coded coding pass ?
				if (((System.String) encSpec.rts.getTileCompVal(tileIdx, compIdx)).Equals("on"))
				{
					tmp |= CSJ2K.j2k.entropy.StdEntropyCoderOptions.OPT_TERM_PASS;
				}
				// Vertically stripe-causal context mode ?
				if (((System.String) encSpec.css.getTileCompVal(tileIdx, compIdx)).Equals("on"))
				{
					tmp |= CSJ2K.j2k.entropy.StdEntropyCoderOptions.OPT_VERT_STR_CAUSAL;
				}
				// Predictable termination ?
				if (((System.String) encSpec.tts.getTileCompVal(tileIdx, compIdx)).Equals("predict"))
				{
					tmp |= CSJ2K.j2k.entropy.StdEntropyCoderOptions.OPT_PRED_TERM;
				}
				// Error resilience segmentation symbol insertion ?
				if (((System.String) encSpec.sss.getTileCompVal(tileIdx, compIdx)).Equals("on"))
				{
					tmp |= CSJ2K.j2k.entropy.StdEntropyCoderOptions.OPT_SEG_SYMBOLS;
				}
			}
			hbuf.Write((System.Byte) tmp);
			
			// Wavelet transform
			// Wavelet Filter
			if (mh)
			{
				filt = ((AnWTFilter[][]) encSpec.wfs.getCompDef(compIdx));
				hbuf.Write((System.Byte) filt[0][0].FilterType);
			}
			else
			{
				filt = ((AnWTFilter[][]) encSpec.wfs.getTileCompVal(tileIdx, compIdx));
				hbuf.Write((System.Byte) filt[0][0].FilterType);
			}
			
			// Precinct partition
			if (precinctPartitionUsed)
			{
				// Write the precinct size for each resolution level + 1
				// (resolution 0) if precinct partition is used.
				System.Collections.ArrayList[] v = null;
				if (mh)
				{
					v = (System.Collections.ArrayList[]) encSpec.pss.getCompDef(compIdx);
				}
				else
				{
					v = (System.Collections.ArrayList[]) encSpec.pss.getTileCompVal(tileIdx, compIdx);
				}
				for (int r = mrl; r >= 0; r--)
				{
					if (r >= v[1].Count)
					{
						tmp = ((System.Int32) v[1][v[1].Count - 1]);
					}
					else
					{
						tmp = ((System.Int32) v[1][r]);
					}
					int yExp = (MathUtil.log2(tmp) << 4) & 0x00F0;
					
					if (r >= v[0].Count)
					{
						tmp = ((System.Int32) v[0][v[0].Count - 1]);
					}
					else
					{
						tmp = ((System.Int32) v[0][r]);
					}
					int xExp = MathUtil.log2(tmp) & 0x000F;
					hbuf.Write((System.Byte) (yExp | xExp));
				}
			}
		}
		
		/// <summary> Writes QCD marker segment in main header. QCD is a functional marker
		/// segment countaining the quantization default used for compressing all
		/// the components in an image. The values can be overriden for an
		/// individual component by a QCC marker in either the main or the tile
		/// header.
		/// 
		/// </summary>
		protected internal virtual void  writeMainQCD()
		{
			int mrl;
			int qstyle;
			
			float step;
			
			System.String qType = (System.String) encSpec.qts.getDefault();
			//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Float.floatValue' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
			float baseStep = (float) ((System.Single) encSpec.qsss.getDefault());
			int gb = ((System.Int32) encSpec.gbs.getDefault());
			
			bool isDerived = qType.Equals("derived");
			bool isReversible = qType.Equals("reversible");
			
			mrl = ((System.Int32) encSpec.dls.getDefault());
			
			int nt = dwt.getNumTiles();
			int nc = dwt.NumComps;
			int tmpI;
			int[] tcIdx = new int[2];
			System.String tmpStr;
			bool notFound = true;
			for (int t = 0; t < nt && notFound; t++)
			{
				for (int c = 0; c < nc && notFound; c++)
				{
					tmpI = ((System.Int32) encSpec.dls.getTileCompVal(t, c));
					tmpStr = ((System.String) encSpec.qts.getTileCompVal(t, c));
					if (tmpI == mrl && tmpStr.Equals(qType))
					{
						tcIdx[0] = t; tcIdx[1] = c;
						notFound = false;
					}
				}
			}
			if (notFound)
			{
				throw new System.ApplicationException("Default representative for quantization type " + " and number of decomposition levels not found " + " in main QCD marker segment. " + "You have found a JJ2000 bug.");
			}
			SubbandAn sb, csb, sbRoot = dwt.getAnSubbandTree(tcIdx[0], tcIdx[1]);
			defimgn = dwt.getNomRangeBits(tcIdx[1]);
			
			int nqcd; // Number of quantization step-size to transmit
			
			// Get the quantization style
			qstyle = (isReversible)?CSJ2K.j2k.codestream.Markers.SQCX_NO_QUANTIZATION:((isDerived)?CSJ2K.j2k.codestream.Markers.SQCX_SCALAR_DERIVED:CSJ2K.j2k.codestream.Markers.SQCX_SCALAR_EXPOUNDED);
			
			// QCD marker
			hbuf.Write((System.Int16) CSJ2K.j2k.codestream.Markers.QCD);
			
			// Compute the number of steps to send
			switch (qstyle)
			{
				
				case CSJ2K.j2k.codestream.Markers.SQCX_SCALAR_DERIVED: 
					nqcd = 1; // Just the LL value
					break;
				
				case CSJ2K.j2k.codestream.Markers.SQCX_NO_QUANTIZATION: 
				case CSJ2K.j2k.codestream.Markers.SQCX_SCALAR_EXPOUNDED: 
					// One value per subband
					nqcd = 0;
					
					sb = sbRoot;
					
					// Get the subband at first resolution level
					sb = (SubbandAn) sb.getSubbandByIdx(0, 0);
					
					// Count total number of subbands
					for (int j = 0; j <= mrl; j++)
					{
						csb = sb;
						while (csb != null)
						{
							nqcd++;
							csb = (SubbandAn) csb.nextSubband();
						}
						// Go up one resolution level
						sb = (SubbandAn) sb.NextResLevel;
					}
					break;
				
				default: 
					throw new System.ApplicationException("Internal JJ2000 error");
				
			}
			
			// Lqcd (marker segment length (in bytes))
			// Lqcd(2 bytes)+Sqcd(1)+ SPqcd (2*Nqcd)
			int markSegLen = 3 + ((isReversible)?nqcd:2 * nqcd);
			
			// Rounded to the nearest even value greater or equals
			hbuf.Write((System.Int16) markSegLen);
			
			// Sqcd
			hbuf.Write((System.Byte) (qstyle + (gb << CSJ2K.j2k.codestream.Markers.SQCX_GB_SHIFT)));
			
			// SPqcd
			switch (qstyle)
			{
				
				case CSJ2K.j2k.codestream.Markers.SQCX_NO_QUANTIZATION: 
					sb = sbRoot;
					sb = (SubbandAn) sb.getSubbandByIdx(0, 0);
					
					// Output one exponent per subband
					for (int j = 0; j <= mrl; j++)
					{
						csb = sb;
						while (csb != null)
						{
							int tmp = (defimgn + csb.anGainExp);
							hbuf.Write((System.Byte) (tmp << CSJ2K.j2k.codestream.Markers.SQCX_EXP_SHIFT));
							
							csb = (SubbandAn) csb.nextSubband();
							// Go up one resolution level
						}
						sb = (SubbandAn) sb.NextResLevel;
					}
					break;
				
				case CSJ2K.j2k.codestream.Markers.SQCX_SCALAR_DERIVED: 
					sb = sbRoot;
					sb = (SubbandAn) sb.getSubbandByIdx(0, 0);
					
					// Calculate subband step (normalized to unit
					// dynamic range)
					step = baseStep / (1 << sb.level);
					
					// Write exponent-mantissa, 16 bits
					hbuf.Write((System.Int16) StdQuantizer.convertToExpMantissa(step));
					break;
				
				case CSJ2K.j2k.codestream.Markers.SQCX_SCALAR_EXPOUNDED: 
					sb = sbRoot;
					sb = (SubbandAn) sb.getSubbandByIdx(0, 0);
					
					// Output one step per subband
					for (int j = 0; j <= mrl; j++)
					{
						csb = sb;
						while (csb != null)
						{
							// Calculate subband step (normalized to unit
							// dynamic range)
							step = baseStep / (csb.l2Norm * (1 << csb.anGainExp));
							
							// Write exponent-mantissa, 16 bits
							hbuf.Write((System.Int16) StdQuantizer.convertToExpMantissa(step));
							
							csb = (SubbandAn) csb.nextSubband();
						}
						// Go up one resolution level
						sb = (SubbandAn) sb.NextResLevel;
					}
					break;
				
				default: 
					throw new System.ApplicationException("Internal JJ2000 error");
				
			}
		}
		
		/// <summary> Writes QCC marker segment in main header. It is a functional marker
		/// segment countaining the quantization used for compressing the specified
		/// component in an image. The values override for the specified component
		/// what was defined by a QCC marker in either the main or the tile header.
		/// 
		/// </summary>
		/// <param name="compIdx">Index of the component which needs QCC marker segment.
		/// 
		/// </param>
		protected internal virtual void  writeMainQCC(int compIdx)
		{
			
			int mrl;
			int qstyle;
			int tIdx = 0;
			float step;
			
			SubbandAn sb, sb2;
			SubbandAn sbRoot;
			
			int imgnr = dwt.getNomRangeBits(compIdx);
			System.String qType = (System.String) encSpec.qts.getCompDef(compIdx);
			//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Float.floatValue' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
			float baseStep = (float) ((System.Single) encSpec.qsss.getCompDef(compIdx));
			int gb = ((System.Int32) encSpec.gbs.getCompDef(compIdx));
			
			bool isReversible = qType.Equals("reversible");
			bool isDerived = qType.Equals("derived");
			
			mrl = ((System.Int32) encSpec.dls.getCompDef(compIdx));
			
			int nt = dwt.getNumTiles();
			int nc = dwt.NumComps;
			int tmpI;
			System.String tmpStr;
			bool notFound = true;
			for (int t = 0; t < nt && notFound; t++)
			{
				for (int c = 0; c < nc && notFound; c++)
				{
					tmpI = ((System.Int32) encSpec.dls.getTileCompVal(t, c));
					tmpStr = ((System.String) encSpec.qts.getTileCompVal(t, c));
					if (tmpI == mrl && tmpStr.Equals(qType))
					{
						tIdx = t;
						notFound = false;
					}
				}
			}
			if (notFound)
			{
				throw new System.ApplicationException("Default representative for quantization type " + " and number of decomposition levels not found " + " in main QCC (c=" + compIdx + ") marker segment. " + "You have found a JJ2000 bug.");
			}
			sbRoot = dwt.getAnSubbandTree(tIdx, compIdx);
			
			int nqcc; // Number of quantization step-size to transmit
			
			// Get the quantization style
			if (isReversible)
			{
				qstyle = CSJ2K.j2k.codestream.Markers.SQCX_NO_QUANTIZATION;
			}
			else if (isDerived)
			{
				qstyle = CSJ2K.j2k.codestream.Markers.SQCX_SCALAR_DERIVED;
			}
			else
			{
				qstyle = CSJ2K.j2k.codestream.Markers.SQCX_SCALAR_EXPOUNDED;
			}
			
			// QCC marker
			hbuf.Write((System.Int16) CSJ2K.j2k.codestream.Markers.QCC);
			
			// Compute the number of steps to send
			switch (qstyle)
			{
				
				case CSJ2K.j2k.codestream.Markers.SQCX_SCALAR_DERIVED: 
					nqcc = 1; // Just the LL value
					break;
				
				case CSJ2K.j2k.codestream.Markers.SQCX_NO_QUANTIZATION: 
				case CSJ2K.j2k.codestream.Markers.SQCX_SCALAR_EXPOUNDED: 
					// One value per subband
					nqcc = 0;
					
					sb = sbRoot;
					mrl = sb.resLvl;
					
					// Get the subband at first resolution level
					sb = (SubbandAn) sb.getSubbandByIdx(0, 0);
					
					// Find root element for LL subband
					while (sb.resLvl != 0)
					{
						sb = sb.subb_LL;
					}
					
					// Count total number of subbands
					for (int j = 0; j <= mrl; j++)
					{
						sb2 = sb;
						while (sb2 != null)
						{
							nqcc++;
							sb2 = (SubbandAn) sb2.nextSubband();
						}
						// Go up one resolution level
						sb = (SubbandAn) sb.NextResLevel;
					}
					break;
				
				default: 
					throw new System.ApplicationException("Internal JJ2000 error");
				
			}
			
			// Lqcc (marker segment length (in bytes))
			// Lqcc(2 bytes)+Cqcc(1 or 2)+Sqcc(1)+ SPqcc (2*Nqcc)
			int markSegLen = 3 + ((nComp < 257)?1:2) + ((isReversible)?nqcc:2 * nqcc);
			hbuf.Write((System.Int16) markSegLen);
			
			// Cqcc
			if (nComp < 257)
			{
				hbuf.Write((System.Byte) compIdx);
			}
			else
			{
				hbuf.Write((System.Int16) compIdx);
			}
			
			// Sqcc (quantization style)
			hbuf.Write((System.Byte) (qstyle + (gb << CSJ2K.j2k.codestream.Markers.SQCX_GB_SHIFT)));
			
			// SPqcc
			switch (qstyle)
			{
				
				case CSJ2K.j2k.codestream.Markers.SQCX_NO_QUANTIZATION: 
					// Get resolution level 0 subband
					sb = sbRoot;
					sb = (SubbandAn) sb.getSubbandByIdx(0, 0);
					
					// Output one exponent per subband
					for (int j = 0; j <= mrl; j++)
					{
						sb2 = sb;
						while (sb2 != null)
						{
							int tmp = (imgnr + sb2.anGainExp);
							hbuf.Write((System.Byte) (tmp << CSJ2K.j2k.codestream.Markers.SQCX_EXP_SHIFT));
							
							sb2 = (SubbandAn) sb2.nextSubband();
						}
						// Go up one resolution level
						sb = (SubbandAn) sb.NextResLevel;
					}
					break;
				
				case CSJ2K.j2k.codestream.Markers.SQCX_SCALAR_DERIVED: 
					// Get resolution level 0 subband
					sb = sbRoot;
					sb = (SubbandAn) sb.getSubbandByIdx(0, 0);
					
					// Calculate subband step (normalized to unit
					// dynamic range)
					step = baseStep / (1 << sb.level);
					
					// Write exponent-mantissa, 16 bits
					hbuf.Write((System.Int16) StdQuantizer.convertToExpMantissa(step));
					break;
				
				case CSJ2K.j2k.codestream.Markers.SQCX_SCALAR_EXPOUNDED: 
					// Get resolution level 0 subband
					sb = sbRoot;
					mrl = sb.resLvl;
					
					sb = (SubbandAn) sb.getSubbandByIdx(0, 0);
					
					for (int j = 0; j <= mrl; j++)
					{
						sb2 = sb;
						while (sb2 != null)
						{
							// Calculate subband step (normalized to unit
							// dynamic range)
							step = baseStep / (sb2.l2Norm * (1 << sb2.anGainExp));
							
							// Write exponent-mantissa, 16 bits
							hbuf.Write((System.Int16) StdQuantizer.convertToExpMantissa(step));
							sb2 = (SubbandAn) sb2.nextSubband();
						}
						// Go up one resolution level
						sb = (SubbandAn) sb.NextResLevel;
					}
					break;
				
				default: 
					throw new System.ApplicationException("Internal JJ2000 error");
				
			}
		}
		
		/// <summary> Writes QCD marker segment in tile header. QCD is a functional marker
		/// segment countaining the quantization default used for compressing all
		/// the components in an image. The values can be overriden for an
		/// individual component by a QCC marker in either the main or the tile
		/// header.
		/// 
		/// </summary>
		/// <param name="tIdx">Tile index
		/// 
		/// </param>
		protected internal virtual void  writeTileQCD(int tIdx)
		{
			int mrl;
			int qstyle;
			
			float step;
			SubbandAn sb, csb, sbRoot;
			
			System.String qType = (System.String) encSpec.qts.getTileDef(tIdx);
			//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Float.floatValue' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
			float baseStep = (float) ((System.Single) encSpec.qsss.getTileDef(tIdx));
			mrl = ((System.Int32) encSpec.dls.getTileDef(tIdx));
			
			int nc = dwt.NumComps;
			int tmpI;
			System.String tmpStr;
			bool notFound = true;
			int compIdx = 0;
			for (int c = 0; c < nc && notFound; c++)
			{
				tmpI = ((System.Int32) encSpec.dls.getTileCompVal(tIdx, c));
				tmpStr = ((System.String) encSpec.qts.getTileCompVal(tIdx, c));
				if (tmpI == mrl && tmpStr.Equals(qType))
				{
					compIdx = c;
					notFound = false;
				}
			}
			if (notFound)
			{
				throw new System.ApplicationException("Default representative for quantization type " + " and number of decomposition levels not found " + " in tile QCD (t=" + tIdx + ") marker segment. " + "You have found a JJ2000 bug.");
			}
			
			sbRoot = dwt.getAnSubbandTree(tIdx, compIdx);
			deftilenr = dwt.getNomRangeBits(compIdx);
			int gb = ((System.Int32) encSpec.gbs.getTileDef(tIdx));
			
			bool isDerived = qType.Equals("derived");
			bool isReversible = qType.Equals("reversible");
			
			int nqcd; // Number of quantization step-size to transmit
			
			// Get the quantization style
			qstyle = (isReversible)?CSJ2K.j2k.codestream.Markers.SQCX_NO_QUANTIZATION:((isDerived)?CSJ2K.j2k.codestream.Markers.SQCX_SCALAR_DERIVED:CSJ2K.j2k.codestream.Markers.SQCX_SCALAR_EXPOUNDED);
			
			// QCD marker
			hbuf.Write((System.Int16) CSJ2K.j2k.codestream.Markers.QCD);
			
			// Compute the number of steps to send
			switch (qstyle)
			{
				
				case CSJ2K.j2k.codestream.Markers.SQCX_SCALAR_DERIVED: 
					nqcd = 1; // Just the LL value
					break;
				
				case CSJ2K.j2k.codestream.Markers.SQCX_NO_QUANTIZATION: 
				case CSJ2K.j2k.codestream.Markers.SQCX_SCALAR_EXPOUNDED: 
					// One value per subband
					nqcd = 0;
					
					sb = sbRoot;
					
					// Get the subband at first resolution level
					sb = (SubbandAn) sb.getSubbandByIdx(0, 0);
					
					// Count total number of subbands
					for (int j = 0; j <= mrl; j++)
					{
						csb = sb;
						while (csb != null)
						{
							nqcd++;
							csb = (SubbandAn) csb.nextSubband();
						}
						// Go up one resolution level
						sb = (SubbandAn) sb.NextResLevel;
					}
					break;
				
				default: 
					throw new System.ApplicationException("Internal JJ2000 error");
				
			}
			
			// Lqcd (marker segment length (in bytes))
			// Lqcd(2 bytes)+Sqcd(1)+ SPqcd (2*Nqcd)
			int markSegLen = 3 + ((isReversible)?nqcd:2 * nqcd);
			
			// Rounded to the nearest even value greater or equals
			hbuf.Write((System.Int16) markSegLen);
			
			// Sqcd
			hbuf.Write((System.Byte) (qstyle + (gb << CSJ2K.j2k.codestream.Markers.SQCX_GB_SHIFT)));
			
			// SPqcd
			switch (qstyle)
			{
				
				case CSJ2K.j2k.codestream.Markers.SQCX_NO_QUANTIZATION: 
					sb = sbRoot;
					sb = (SubbandAn) sb.getSubbandByIdx(0, 0);
					
					// Output one exponent per subband
					for (int j = 0; j <= mrl; j++)
					{
						csb = sb;
						while (csb != null)
						{
							int tmp = (deftilenr + csb.anGainExp);
							hbuf.Write((System.Byte) (tmp << CSJ2K.j2k.codestream.Markers.SQCX_EXP_SHIFT));
							
							csb = (SubbandAn) csb.nextSubband();
							// Go up one resolution level
						}
						sb = (SubbandAn) sb.NextResLevel;
					}
					break;
				
				case CSJ2K.j2k.codestream.Markers.SQCX_SCALAR_DERIVED: 
					sb = sbRoot;
					sb = (SubbandAn) sb.getSubbandByIdx(0, 0);
					
					// Calculate subband step (normalized to unit
					// dynamic range)
					step = baseStep / (1 << sb.level);
					
					// Write exponent-mantissa, 16 bits
					hbuf.Write((System.Int16) StdQuantizer.convertToExpMantissa(step));
					break;
				
				case CSJ2K.j2k.codestream.Markers.SQCX_SCALAR_EXPOUNDED: 
					sb = sbRoot;
					sb = (SubbandAn) sb.getSubbandByIdx(0, 0);
					
					// Output one step per subband
					for (int j = 0; j <= mrl; j++)
					{
						csb = sb;
						while (csb != null)
						{
							// Calculate subband step (normalized to unit
							// dynamic range)
							step = baseStep / (csb.l2Norm * (1 << csb.anGainExp));
							
							// Write exponent-mantissa, 16 bits
							hbuf.Write((System.Int16) StdQuantizer.convertToExpMantissa(step));
							
							csb = (SubbandAn) csb.nextSubband();
						}
						// Go up one resolution level
						sb = (SubbandAn) sb.NextResLevel;
					}
					break;
				
				default: 
					throw new System.ApplicationException("Internal JJ2000 error");
				
			}
		}
		
		/// <summary> Writes QCC marker segment in tile header. It is a functional marker
		/// segment countaining the quantization used for compressing the specified
		/// component in an image. The values override for the specified component
		/// what was defined by a QCC marker in either the main or the tile header.
		/// 
		/// </summary>
		/// <param name="t">Tile index
		/// 
		/// </param>
		/// <param name="compIdx">Index of the component which needs QCC marker segment.
		/// 
		/// </param>
		protected internal virtual void  writeTileQCC(int t, int compIdx)
		{
			
			int mrl;
			int qstyle;
			float step;
			
			SubbandAn sb, sb2;
			int nqcc; // Number of quantization step-size to transmit
			
			SubbandAn sbRoot = dwt.getAnSubbandTree(t, compIdx);
			int imgnr = dwt.getNomRangeBits(compIdx);
			System.String qType = (System.String) encSpec.qts.getTileCompVal(t, compIdx);
			//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Float.floatValue' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
			float baseStep = (float) ((System.Single) encSpec.qsss.getTileCompVal(t, compIdx));
			int gb = ((System.Int32) encSpec.gbs.getTileCompVal(t, compIdx));
			
			bool isReversible = qType.Equals("reversible");
			bool isDerived = qType.Equals("derived");
			
			mrl = ((System.Int32) encSpec.dls.getTileCompVal(t, compIdx));
			
			// Get the quantization style
			if (isReversible)
			{
				qstyle = CSJ2K.j2k.codestream.Markers.SQCX_NO_QUANTIZATION;
			}
			else if (isDerived)
			{
				qstyle = CSJ2K.j2k.codestream.Markers.SQCX_SCALAR_DERIVED;
			}
			else
			{
				qstyle = CSJ2K.j2k.codestream.Markers.SQCX_SCALAR_EXPOUNDED;
			}
			
			// QCC marker
			hbuf.Write((System.Int16) CSJ2K.j2k.codestream.Markers.QCC);
			
			// Compute the number of steps to send
			switch (qstyle)
			{
				
				case CSJ2K.j2k.codestream.Markers.SQCX_SCALAR_DERIVED: 
					nqcc = 1; // Just the LL value
					break;
				
				case CSJ2K.j2k.codestream.Markers.SQCX_NO_QUANTIZATION: 
				case CSJ2K.j2k.codestream.Markers.SQCX_SCALAR_EXPOUNDED: 
					// One value per subband
					nqcc = 0;
					
					sb = sbRoot;
					mrl = sb.resLvl;
					
					// Get the subband at first resolution level
					sb = (SubbandAn) sb.getSubbandByIdx(0, 0);
					
					// Find root element for LL subband
					while (sb.resLvl != 0)
					{
						sb = sb.subb_LL;
					}
					
					// Count total number of subbands
					for (int j = 0; j <= mrl; j++)
					{
						sb2 = sb;
						while (sb2 != null)
						{
							nqcc++;
							sb2 = (SubbandAn) sb2.nextSubband();
						}
						// Go up one resolution level
						sb = (SubbandAn) sb.NextResLevel;
					}
					break;
				
				default: 
					throw new System.ApplicationException("Internal JJ2000 error");
				
			}
			
			// Lqcc (marker segment length (in bytes))
			// Lqcc(2 bytes)+Cqcc(1 or 2)+Sqcc(1)+ SPqcc (2*Nqcc)
			int markSegLen = 3 + ((nComp < 257)?1:2) + ((isReversible)?nqcc:2 * nqcc);
			hbuf.Write((System.Int16) markSegLen);
			
			// Cqcc
			if (nComp < 257)
			{
				hbuf.Write((System.Byte) compIdx);
			}
			else
			{
				hbuf.Write((System.Int16) compIdx);
			}
			
			// Sqcc (quantization style)
			hbuf.Write((System.Byte) (qstyle + (gb << CSJ2K.j2k.codestream.Markers.SQCX_GB_SHIFT)));
			
			// SPqcc
			switch (qstyle)
			{
				
				case CSJ2K.j2k.codestream.Markers.SQCX_NO_QUANTIZATION: 
					// Get resolution level 0 subband
					sb = sbRoot;
					sb = (SubbandAn) sb.getSubbandByIdx(0, 0);
					
					// Output one exponent per subband
					for (int j = 0; j <= mrl; j++)
					{
						sb2 = sb;
						while (sb2 != null)
						{
							int tmp = (imgnr + sb2.anGainExp);
							hbuf.Write((System.Byte) (tmp << CSJ2K.j2k.codestream.Markers.SQCX_EXP_SHIFT));
							
							sb2 = (SubbandAn) sb2.nextSubband();
						}
						// Go up one resolution level
						sb = (SubbandAn) sb.NextResLevel;
					}
					break;
				
				case CSJ2K.j2k.codestream.Markers.SQCX_SCALAR_DERIVED: 
					// Get resolution level 0 subband
					sb = sbRoot;
					sb = (SubbandAn) sb.getSubbandByIdx(0, 0);
					
					// Calculate subband step (normalized to unit
					// dynamic range)
					step = baseStep / (1 << sb.level);
					
					// Write exponent-mantissa, 16 bits
					hbuf.Write((System.Int16) StdQuantizer.convertToExpMantissa(step));
					break;
				
				case CSJ2K.j2k.codestream.Markers.SQCX_SCALAR_EXPOUNDED: 
					// Get resolution level 0 subband
					sb = sbRoot;
					mrl = sb.resLvl;
					
					sb = (SubbandAn) sb.getSubbandByIdx(0, 0);
					
					for (int j = 0; j <= mrl; j++)
					{
						sb2 = sb;
						while (sb2 != null)
						{
							// Calculate subband step (normalized to unit
							// dynamic range)
							step = baseStep / (sb2.l2Norm * (1 << sb2.anGainExp));
							
							// Write exponent-mantissa, 16 bits
							hbuf.Write((System.Int16) StdQuantizer.convertToExpMantissa(step));
							sb2 = (SubbandAn) sb2.nextSubband();
						}
						// Go up one resolution level
						sb = (SubbandAn) sb.NextResLevel;
					}
					break;
				
				default: 
					throw new System.ApplicationException("Internal JJ2000 error");
				
			}
		}
		
		/// <summary> Writes POC marker segment. POC is a functional marker segment
		/// containing the bounds and progression order for any progression order
		/// other than default in the codestream.
		/// 
		/// </summary>
		/// <param name="mh">Flag indicating whether the main header is to be written 
		/// 
		/// </param>
		/// <param name="tileIdx">Tile index
		/// 
		/// </param>
		protected internal virtual void  writePOC(bool mh, int tileIdx)
		{
			int markSegLen = 0; // Segment marker length
			int lenCompField; // Holds the size of any component field as
			// this size depends on the number of 
			//components
			Progression[] prog = null; // Holds the progression(s)
			int npoc; // Number of progression order changes
			
			// Get the progression order changes, their number and checks
			// if it is ok
			if (mh)
			{
				prog = (Progression[]) (encSpec.pocs.getDefault());
			}
			else
			{
				prog = (Progression[]) (encSpec.pocs.getTileDef(tileIdx));
			}
			
			// Calculate the length of a component field (depends on the number of 
			// components)
			lenCompField = (nComp < 257?1:2);
			
			// POC marker
			hbuf.Write((System.Int16) CSJ2K.j2k.codestream.Markers.POC);
			
			// Lpoc (marker segment length (in bytes))
			// Basic: Lpoc(2 bytes) + npoc * [ RSpoc(1) + CSpoc(1 or 2) + 
			// LYEpoc(2) + REpoc(1) + CEpoc(1 or 2) + Ppoc(1) ]
			npoc = prog.Length;
			markSegLen = 2 + npoc * (1 + lenCompField + 2 + 1 + lenCompField + 1);
			hbuf.Write((System.Int16) markSegLen);
			
			// Write each progression order change 
			for (int i = 0; i < npoc; i++)
			{
				// RSpoc(i)
				hbuf.Write((System.Byte) prog[i].rs);
				// CSpoc(i)
				if (lenCompField == 2)
				{
					hbuf.Write((System.Int16) prog[i].cs);
				}
				else
				{
					hbuf.Write((System.Byte) prog[i].cs);
				}
				// LYEpoc(i)
				hbuf.Write((System.Int16) prog[i].lye);
				// REpoc(i)
				hbuf.Write((System.Byte) prog[i].re);
				// CEpoc(i)
				if (lenCompField == 2)
				{
					hbuf.Write((System.Int16) prog[i].ce);
				}
				else
				{
					hbuf.Write((System.Byte) prog[i].ce);
				}
				// Ppoc(i)
				hbuf.Write((System.Byte) prog[i].type);
			}
		}
		
		
		/// <summary> Write main header. JJ2000 main header corresponds to the following
		/// sequence of marker segments:
		/// 
		/// <ol>
		/// <li>SOC</li>
		/// <li>SIZ</li>
		/// <li>COD</li>
		/// <li>COC (if needed)</li>
		/// <li>QCD</li>
		/// <li>QCC (if needed)</li>
		/// <li>POC (if needed)</li>
		/// </ol>
		/// 
		/// </summary>
		public virtual void  encodeMainHeader()
		{
			int i;
			
			
			// +---------------------------------+
			// |    SOC marker segment           |
			// +---------------------------------+
			writeSOC();
			
			// +---------------------------------+
			// |    Image and tile SIZe (SIZ)    |
			// +---------------------------------+
			writeSIZ();
			
			// +-------------------------------+
			// |   COding style Default (COD)  |
			// +-------------------------------+
			bool isEresUsed = ((System.String) encSpec.tts.getDefault()).Equals("predict");
			writeCOD(true, 0);
			
			// +---------------------------------+
			// |   COding style Component (COC)  |
			// +---------------------------------+
			for (i = 0; i < nComp; i++)
			{
				bool isEresUsedinComp = ((System.String) encSpec.tts.getCompDef(i)).Equals("predict");
				if (encSpec.wfs.isCompSpecified(i) || encSpec.dls.isCompSpecified(i) || encSpec.bms.isCompSpecified(i) || encSpec.mqrs.isCompSpecified(i) || encSpec.rts.isCompSpecified(i) || encSpec.sss.isCompSpecified(i) || encSpec.css.isCompSpecified(i) || encSpec.pss.isCompSpecified(i) || encSpec.cblks.isCompSpecified(i) || (isEresUsed != isEresUsedinComp))
				// Some component non-default stuff => need COC
					writeCOC(true, 0, i);
			}
			
			// +-------------------------------+
			// |   Quantization Default (QCD)  |
			// +-------------------------------+
			writeMainQCD();
			
			// +-------------------------------+
			// | Quantization Component (QCC)  |
			// +-------------------------------+
			// Write needed QCC markers
			for (i = 0; i < nComp; i++)
			{
				if (dwt.getNomRangeBits(i) != defimgn || encSpec.qts.isCompSpecified(i) || encSpec.qsss.isCompSpecified(i) || encSpec.dls.isCompSpecified(i) || encSpec.gbs.isCompSpecified(i))
				{
					writeMainQCC(i);
				}
			}
			
			// +--------------------------+
			// |    POC maker segment     |
			// +--------------------------+
			Progression[] prog = (Progression[]) (encSpec.pocs.getDefault());
			if (prog.Length > 1)
				writePOC(true, 0);
			
			// +---------------------------+
			// |      Comments (COM)       |
			// +---------------------------+
			writeCOM();
		}
		
		/// <summary> Write COM marker segment(s) to the codestream.
		/// 
		/// <p>This marker is currently written in main header and indicates the
		/// JJ2000 encoder's version that has created the codestream.</p>
		/// 
		/// </summary>
		private void  writeCOM()
		{
			// JJ2000 COM marker segment
			if (enJJ2KMarkSeg)
			{
				System.String str = "Created by: CSJ2K version " + JJ2KInfo.version;
				int markSegLen; // the marker segment length
				
				// COM marker
				hbuf.Write((System.Int16) CSJ2K.j2k.codestream.Markers.COM);
				
				// Calculate length: Lcom(2) + Rcom (2) + string's length;
				markSegLen = 2 + 2 + str.Length;
				hbuf.Write((System.Int16) markSegLen);
				
				// Rcom 
				hbuf.Write((System.Int16) 1); // General use (IS 8859-15:1999(Latin) values)
				
				byte[] chars = System.Text.ASCIIEncoding.ASCII.GetBytes(str);
				for (int i = 0; i < chars.Length; i++)
				{
					hbuf.Write((byte) chars[i]);
				}
			}
			// other COM marker segments
			if (otherCOMMarkSeg != null)
			{
				SupportClass.Tokenizer stk = new SupportClass.Tokenizer(otherCOMMarkSeg, "#");
				while (stk.HasMoreTokens())
				{
					System.String str = stk.NextToken();
					int markSegLen; // the marker segment length
					
					// COM marker
					hbuf.Write((System.Int16) CSJ2K.j2k.codestream.Markers.COM);
					
					// Calculate length: Lcom(2) + Rcom (2) + string's length;
					markSegLen = 2 + 2 + str.Length;
					hbuf.Write((System.Int16) markSegLen);
					
					// Rcom 
					hbuf.Write((System.Int16) 1); // General use (IS 8859-15:1999(Latin)
					// values)
					
					byte[] chars = System.Text.ASCIIEncoding.ASCII.GetBytes(str);
					for (int i = 0; i < chars.Length; i++)
					{
						hbuf.Write((byte) chars[i]);
					}
				}
			}
		}
		
		/// <summary> Writes the RGN marker segment in the tile header. It describes the
		/// scaling value in each tile component
		/// 
		/// <p>May be used in tile or main header. If used in main header, it
		/// refers to a ROI of the whole image, regardless of tiling. When used in
		/// tile header, only the particular tile is affected.</p>
		/// 
		/// </summary>
		/// <param name="tIdx">The tile index 
		/// 
		/// </param>
		/// <exception cref="IOException">If an I/O error occurs while reading from the
		/// encoder header stream
		/// 
		/// </exception>
		private void  writeRGN(int tIdx)
		{
			int i;
			int markSegLen; // the marker length
			
			// Write one RGN marker per component 
			for (i = 0; i < nComp; i++)
			{
				// RGN marker
				hbuf.Write((System.Int16) CSJ2K.j2k.codestream.Markers.RGN);
				
				// Calculate length (Lrgn)
				// Basic: Lrgn (2) + Srgn (1) + SPrgn + one byte 
				// or two for component number
				markSegLen = 4 + ((nComp < 257)?1:2);
				hbuf.Write((System.Int16) markSegLen);
				
				// Write component (Crgn)
				if (nComp < 257)
				{
					hbuf.Write((System.Byte) i);
				}
				else
				{
					hbuf.Write((System.Int16) i);
				}
				
				// Write type of ROI (Srgn) 
				hbuf.Write((System.Byte) CSJ2K.j2k.codestream.Markers.SRGN_IMPLICIT);
				
				// Write ROI info (SPrgn)
				hbuf.Write((System.Byte) ((System.Int32) (encSpec.rois.getTileCompVal(tIdx, i))));
			}
		}
		/// <summary> Writes tile-part header. JJ2000 tile-part header corresponds to the
		/// following sequence of marker segments:
		/// 
		/// <ol> 
		/// <li>SOT</li> 
		/// <li>COD (if needed)</li> 
		/// <li>COC (if needed)</li> 
		/// <li>QCD (if needed)</li> 
		/// <li>QCC (if needed)</li> 
		/// <li>RGN (if needed)</li> 
		/// <li>POC (if needed)</li>
		/// <li>SOD</li> 
		/// </ol>
		/// 
		/// </summary>
		/// <param name="length">The length of the current tile-part.
		/// 
		/// </param>
		/// <param name="tileIdx">Index of the tile to write
		/// 
		/// </param>
		public virtual void  encodeTilePartHeader(int tileLength, int tileIdx)
		{
			
			int tmp;
			Coord numTiles = ralloc.getNumTiles(null);
			ralloc.setTile(tileIdx % numTiles.x, tileIdx / numTiles.x);
			
			// +--------------------------+
			// |    SOT maker segment     |
			// +--------------------------+
			// SOT marker
			hbuf.Write((System.Byte) SupportClass.URShift(CSJ2K.j2k.codestream.Markers.SOT, 8));
			hbuf.Write((System.Byte) (CSJ2K.j2k.codestream.Markers.SOT & 0x00FF));
			
			// Lsot (10 bytes)
			hbuf.Write((System.Byte) 0);
			hbuf.Write((System.Byte) 10);
			
			// Isot
			if (tileIdx > 65534)
			{
				throw new System.ArgumentException("Trying to write a tile-part " + "header whose tile index is " + "too high");
			}
			hbuf.Write((System.Byte) (tileIdx >> 8));
			hbuf.Write((System.Byte) tileIdx);
			
			// Psot
			tmp = tileLength;
			hbuf.Write((System.Byte) (tmp >> 24));
			hbuf.Write((System.Byte) (tmp >> 16));
			hbuf.Write((System.Byte) (tmp >> 8));
			hbuf.Write((System.Byte) tmp);
			
			// TPsot
			hbuf.Write((System.Byte) 0); // Only one tile-part currently supported !
			
			// TNsot
			hbuf.Write((System.Byte) 1); // Only one tile-part currently supported !
			
			// +--------------------------+
			// |    COD maker segment     |
			// +--------------------------+
			bool isEresUsed = ((System.String) encSpec.tts.getDefault()).Equals("predict");
			bool isEresUsedInTile = ((System.String) encSpec.tts.getTileDef(tileIdx)).Equals("predict");
			bool tileCODwritten = false;
			if (encSpec.wfs.isTileSpecified(tileIdx) || encSpec.cts.isTileSpecified(tileIdx) || encSpec.dls.isTileSpecified(tileIdx) || encSpec.bms.isTileSpecified(tileIdx) || encSpec.mqrs.isTileSpecified(tileIdx) || encSpec.rts.isTileSpecified(tileIdx) || encSpec.css.isTileSpecified(tileIdx) || encSpec.pss.isTileSpecified(tileIdx) || encSpec.sops.isTileSpecified(tileIdx) || encSpec.sss.isTileSpecified(tileIdx) || encSpec.pocs.isTileSpecified(tileIdx) || encSpec.ephs.isTileSpecified(tileIdx) || encSpec.cblks.isTileSpecified(tileIdx) || (isEresUsed != isEresUsedInTile))
			{
				writeCOD(false, tileIdx);
				tileCODwritten = true;
			}
			
			// +--------------------------+
			// |    COC maker segment     |
			// +--------------------------+
			for (int c = 0; c < nComp; c++)
			{
				bool isEresUsedInTileComp = ((System.String) encSpec.tts.getTileCompVal(tileIdx, c)).Equals("predict");
				
				if (encSpec.wfs.isTileCompSpecified(tileIdx, c) || encSpec.dls.isTileCompSpecified(tileIdx, c) || encSpec.bms.isTileCompSpecified(tileIdx, c) || encSpec.mqrs.isTileCompSpecified(tileIdx, c) || encSpec.rts.isTileCompSpecified(tileIdx, c) || encSpec.css.isTileCompSpecified(tileIdx, c) || encSpec.pss.isTileCompSpecified(tileIdx, c) || encSpec.sss.isTileCompSpecified(tileIdx, c) || encSpec.cblks.isTileCompSpecified(tileIdx, c) || (isEresUsedInTileComp != isEresUsed))
				{
					writeCOC(false, tileIdx, c);
				}
				else if (tileCODwritten)
				{
					if (encSpec.wfs.isCompSpecified(c) || encSpec.dls.isCompSpecified(c) || encSpec.bms.isCompSpecified(c) || encSpec.mqrs.isCompSpecified(c) || encSpec.rts.isCompSpecified(c) || encSpec.sss.isCompSpecified(c) || encSpec.css.isCompSpecified(c) || encSpec.pss.isCompSpecified(c) || encSpec.cblks.isCompSpecified(c) || (encSpec.tts.isCompSpecified(c) && ((System.String) encSpec.tts.getCompDef(c)).Equals("predict")))
					{
						writeCOC(false, tileIdx, c);
					}
				}
			}
			
			// +--------------------------+
			// |    QCD maker segment     |
			// +--------------------------+
			bool tileQCDwritten = false;
			if (encSpec.qts.isTileSpecified(tileIdx) || encSpec.qsss.isTileSpecified(tileIdx) || encSpec.dls.isTileSpecified(tileIdx) || encSpec.gbs.isTileSpecified(tileIdx))
			{
				writeTileQCD(tileIdx);
				tileQCDwritten = true;
			}
			else
			{
				deftilenr = defimgn;
			}
			
			// +--------------------------+
			// |    QCC maker segment     |
			// +--------------------------+
			for (int c = 0; c < nComp; c++)
			{
				if (dwt.getNomRangeBits(c) != deftilenr || encSpec.qts.isTileCompSpecified(tileIdx, c) || encSpec.qsss.isTileCompSpecified(tileIdx, c) || encSpec.dls.isTileCompSpecified(tileIdx, c) || encSpec.gbs.isTileCompSpecified(tileIdx, c))
				{
					writeTileQCC(tileIdx, c);
				}
				else if (tileQCDwritten)
				{
					if (encSpec.qts.isCompSpecified(c) || encSpec.qsss.isCompSpecified(c) || encSpec.dls.isCompSpecified(c) || encSpec.gbs.isCompSpecified(c))
					{
						writeTileQCC(tileIdx, c);
					}
				}
			}
			
			// +--------------------------+
			// |    RGN maker segment     |
			// +--------------------------+
			if (roiSc.useRoi() && (!roiSc.BlockAligned))
				writeRGN(tileIdx);
			
			// +--------------------------+
			// |    POC maker segment     |
			// +--------------------------+
			Progression[] prog;
			if (encSpec.pocs.isTileSpecified(tileIdx))
			{
				prog = (Progression[]) (encSpec.pocs.getTileDef(tileIdx));
				if (prog.Length > 1)
					writePOC(false, tileIdx);
			}
			
			// +--------------------------+
			// |         SOD maker        |
			// +--------------------------+
			hbuf.Write((System.Byte) SupportClass.URShift(CSJ2K.j2k.codestream.Markers.SOD, 8));
			hbuf.Write((System.Byte) (CSJ2K.j2k.codestream.Markers.SOD & 0x00FF));
		}
	}
}