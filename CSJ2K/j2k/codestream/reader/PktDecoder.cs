/// <summary> CVS identifier:
/// 
/// $Id: PktDecoder.java,v 1.46 2002/07/19 12:35:14 grosbois Exp $
/// 
/// Class:                   PktDecoder
/// 
/// Description:             Reads packets heads and keeps location of
/// code-blocks' codewords
/// 
/// 
/// 
/// COPYRIGHT:
/// 
/// This software module was originally developed by Raphaël Grosbois and
/// Diego Santa Cruz (Swiss Federal Institute of Technology-EPFL); Joel
/// Askelöf (Ericsson Radio Systems AB); and Bertrand Berthelot, David
/// Bouchard, Félix Henry, Gerard Mozelle and Patrice Onno (Canon Research
/// Centre France S.A) in the course of development of the JPEG2000
/// standard as specified by ISO/IEC 15444 (JPEG 2000 Standard). This
/// software module is an implementation of a part of the JPEG 2000
/// Standard. Swiss Federal Institute of Technology-EPFL, Ericsson Radio
/// Systems AB and Canon Research Centre France S.A (collectively JJ2000
/// Partners) agree not to assert against ISO/IEC and users of the JPEG
/// 2000 Standard (Users) any of their rights under the copyright, not
/// including other intellectual property rights, for this software module
/// with respect to the usage by ISO/IEC and Users of this software module
/// or modifications thereof for use in hardware or software products
/// claiming conformance to the JPEG 2000 Standard. Those intending to use
/// this software module in hardware or software products are advised that
/// their use may infringe existing patents. The original developers of
/// this software module, JJ2000 Partners and ISO/IEC assume no liability
/// for use of this software module or modifications thereof. No license
/// or right to this software module is granted for non JPEG 2000 Standard
/// conforming products. JJ2000 Partners have full right to use this
/// software module for his/her own purpose, assign or donate this
/// software module to any third party and to inhibit third parties from
/// using this software module for non JPEG 2000 Standard conforming
/// products. This copyright notice must be included in all copies or
/// derivative works of this software module.
/// 
/// Copyright (c) 1999/2000 JJ2000 Partners.
/// 
/// </summary>
using System;
using CSJ2K.j2k.wavelet.synthesis;
using CSJ2K.j2k.codestream;
using CSJ2K.j2k.entropy;
using CSJ2K.j2k.wavelet;
using CSJ2K.j2k.decoder;
using CSJ2K.j2k.image;
using CSJ2K.j2k.util;
using CSJ2K.j2k.io;
namespace CSJ2K.j2k.codestream.reader
{
	
	/// <summary> This class is used to read packet's head and body. All the members must be
	/// re-initialized at the beginning of each tile thanks to the restart()
	/// method.
	/// 
	/// </summary>
	public class PktDecoder
	{
		
		/// <summary>Reference to the codestream reader agent </summary>
		private BitstreamReaderAgent src;
		
		/// <summary>Flag indicating whether packed packet header was used for this tile </summary>
		private bool pph = false;
		
		/// <summary>The packed packet header if it was used </summary>
		private System.IO.MemoryStream pphbais;
		
		/// <summary>Reference to decoder specifications </summary>
		private DecoderSpecs decSpec;
		
		/// <summary>Reference to the HeaderDecoder </summary>
		private HeaderDecoder hd;
		
		/// <summary>Initial value of the state variable associated with code-block
		/// length. 
		/// </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'INIT_LBLOCK '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		private int INIT_LBLOCK = 3;
		
		/// <summary>The wrapper to read bits for the packet heads </summary>
		private PktHeaderBitReader bin;
		
		/// <summary>Reference to the stream where to read from </summary>
		private RandomAccessIO ehs;
		
		/// <summary> Maximum number of precincts :
		/// 
		/// <ul>
		/// <li> 1st dim: component index.</li>
		/// <li> 2nd dim: resolution level index.</li>
		/// </ul>
		/// 
		/// </summary>
		private Coord[][] numPrec;
		
		/// <summary>Index of the current tile </summary>
		private int tIdx;
		
		/// <summary> Array containing the coordinates, width, height, indexes, ... of the
		/// precincts in the current tile:
		/// 
		/// <ul>
		/// <li> 1st dim: component index.</li>
		/// <li> 2nd dim: resolution level index.</li>
		/// <li> 3rd dim: precinct index.</li>
		/// </ul>
		/// 
		/// </summary>
		private PrecInfo[][][] ppinfo;
		
		/// <summary> Lblock value used to read code size information in each packet head:
		/// 
		/// <ul>
		/// <li> 1st dim: component index.</li>
		/// <li> 2nd dim: resolution level index.</li>
		/// <li> 3rd dim: subband index.</li>
		/// <li> 4th/5th dim: code-block index (vert. and horiz.).</li>
		/// </ul> 
		/// 
		/// </summary>
		private int[][][][][] lblock;
		
		/// <summary> Tag tree used to read inclusion informations in packet's head:
		/// 
		/// <ul>   
		/// <li> 1st dim: component index.</li>
		/// <li> 2nd dim: resolution level index.</li>
		/// <li> 3rd dim: precinct index.</li> 
		/// <li> 4th dim: subband index.</li>
		/// 
		/// </summary>
		private TagTreeDecoder[][][][] ttIncl;
		
		/// <summary> Tag tree used to read bit-depth information in packet's head:
		/// 
		/// <ul>
		/// <li> 1st dim: component index.</li>
		/// <li> 2nd dim: resolution level index.</li>
		/// <li> 3rd dim: precinct index.</li>
		/// <li> 4th dim: subband index.</li>
		/// </ul>
		/// 
		/// </summary>
		private TagTreeDecoder[][][][] ttMaxBP;
		
		/// <summary>Number of layers in t he current tile </summary>
		private int nl = 0;
		
		/// <summary>The number of components </summary>
		private int nc;
		
		/// <summary>Whether or not SOP marker segment are used </summary>
		private bool sopUsed = false;
		
		/// <summary>Whether or not EPH marker are used </summary>
		private bool ephUsed = false;
		
		/// <summary>Index of the current packet in the tile. Used with SOP marker segment
		/// 
		/// </summary>
		private int pktIdx;
		
		/// <summary>List of code-blocks found in last read packet head (one list
		/// per subband) 
		/// </summary>
		private System.Collections.ArrayList[] cblks;
		
		/// <summary>Number of codeblocks encountered. used for ncb quit condition</summary>
		private int ncb;
		
		/// <summary>Maximum number of codeblocks to read before ncb quit condition is
		/// reached 
		/// </summary>
		private int maxCB;
		
		/// <summary>Flag indicating whether ncb quit condition has been reached </summary>
		private bool ncbQuit;
		
		/// <summary>The tile in which the ncb quit condition was reached </summary>
		private int tQuit;
		
		/// <summary>The component in which the ncb quit condition was reached </summary>
		private int cQuit;
		
		/// <summary>The subband in which the ncb quit condition was reached </summary>
		private int sQuit;
		
		/// <summary>The resolution in which the ncb quit condition was reached </summary>
		private int rQuit;
		
		/// <summary>The x position of the last code block before ncb quit reached </summary>
		private int xQuit;
		
		/// <summary>The y position of the last code block before ncb quit reached  </summary>
		private int yQuit;
		
		/// <summary>True if truncation mode is used. False if it is parsing mode </summary>
		private bool isTruncMode;
		
		/// <summary> Creates an empty PktDecoder object associated with given decoder
		/// specifications and HeaderDecoder. This object must be initialized
		/// thanks to the restart method before being used.
		/// 
		/// </summary>
		/// <param name="decSpec">The decoder specifications.
		/// 
		/// </param>
		/// <param name="hd">The HeaderDecoder instance.
		/// 
		/// </param>
		/// <param name="ehs">The stream where to read data from.
		/// 
		/// </param>
		/// <param name="src">The bit stream reader agent.
		/// 
		/// </param>
		/// <param name="isTruncMode">Whether or not truncation mode is required.
		/// 
		/// </param>
		/// <param name="maxCB">The maximum number of code-blocks to read before ncbquit
		/// 
		/// 
		/// </param>
		public PktDecoder(DecoderSpecs decSpec, HeaderDecoder hd, RandomAccessIO ehs, BitstreamReaderAgent src, bool isTruncMode, int maxCB)
		{
			this.decSpec = decSpec;
			this.hd = hd;
			this.ehs = ehs;
			this.isTruncMode = isTruncMode;
			bin = new PktHeaderBitReader(ehs);
			this.src = src;
			ncb = 0;
			ncbQuit = false;
			this.maxCB = maxCB;
		}
		
		
		/// <summary> Re-initialize the PktDecoder instance at the beginning of a new tile.
		/// 
		/// </summary>
		/// <param name="nc">The number of components in this tile
		/// 
		/// </param>
		/// <param name="mdl">The maximum number of decomposition level in each component
		/// of this tile
		/// 
		/// </param>
		/// <param name="nl">The number of layers in  this tile
		/// 
		/// </param>
		/// <param name="cbI">The code-blocks array
		/// 
		/// </param>
		/// <param name="pph">Flag indicating whether packed packet headers was used
		/// 
		/// </param>
		/// <param name="pphbais">Stream containing the packed packet headers
		/// 
		/// </param>
		public virtual CBlkInfo[][][][][] restart(int nc, int[] mdl, int nl, CBlkInfo[][][][][] cbI, bool pph, System.IO.MemoryStream pphbais)
		{
			this.nc = nc;
			this.nl = nl;
			this.tIdx = src.TileIdx;
			this.pph = pph;
			this.pphbais = pphbais;
			
			sopUsed = ((System.Boolean) decSpec.sops.getTileDef(tIdx));
			pktIdx = 0;
			ephUsed = ((System.Boolean) decSpec.ephs.getTileDef(tIdx));
			
			cbI = new CBlkInfo[nc][][][][];
			lblock = new int[nc][][][][];
			ttIncl = new TagTreeDecoder[nc][][][];
			ttMaxBP = new TagTreeDecoder[nc][][][];
			numPrec = new Coord[nc][];
			ppinfo = new PrecInfo[nc][][];
			
			// Used to compute the maximum number of precincts for each resolution
			// level
			int tcx0, tcy0, tcx1, tcy1; // Current tile position in the domain of
			// the image component
			int trx0, try0, trx1, try1; // Current tile position in the reduced
			// resolution image domain
			//int xrsiz, yrsiz; // Component sub-sampling factors
			
			SubbandSyn root, sb;
			int mins, maxs;
			Coord nBlk = null;
			int cb0x = src.CbULX;
			int cb0y = src.CbULY;
			
			for (int c = 0; c < nc; c++)
			{
				cbI[c] = new CBlkInfo[mdl[c] + 1][][][];
				lblock[c] = new int[mdl[c] + 1][][][];
				ttIncl[c] = new TagTreeDecoder[mdl[c] + 1][][];
				ttMaxBP[c] = new TagTreeDecoder[mdl[c] + 1][][];
				numPrec[c] = new Coord[mdl[c] + 1];
				ppinfo[c] = new PrecInfo[mdl[c] + 1][];
				
				// Get the tile-component coordinates on the reference grid
				tcx0 = src.getResULX(c, mdl[c]);
				tcy0 = src.getResULY(c, mdl[c]);
				tcx1 = tcx0 + src.getTileCompWidth(tIdx, c, mdl[c]);
				tcy1 = tcy0 + src.getTileCompHeight(tIdx, c, mdl[c]);
				
				for (int r = 0; r <= mdl[c]; r++)
				{
					
					// Tile's coordinates in the reduced resolution image domain
					//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
					trx0 = (int) System.Math.Ceiling(tcx0 / (double) (1 << (mdl[c] - r)));
					//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
					try0 = (int) System.Math.Ceiling(tcy0 / (double) (1 << (mdl[c] - r)));
					//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
					trx1 = (int) System.Math.Ceiling(tcx1 / (double) (1 << (mdl[c] - r)));
					//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
					try1 = (int) System.Math.Ceiling(tcy1 / (double) (1 << (mdl[c] - r)));
					
					// Calculate the maximum number of precincts for each
					// resolution level taking into account tile specific options.
					double twoppx = (double) getPPX(tIdx, c, r);
					double twoppy = (double) getPPY(tIdx, c, r);
					numPrec[c][r] = new Coord();
					if (trx1 > trx0)
					{
						//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
						numPrec[c][r].x = (int) System.Math.Ceiling((trx1 - cb0x) / twoppx) - (int) System.Math.Floor((trx0 - cb0x) / twoppx);
					}
					else
					{
						numPrec[c][r].x = 0;
					}
					if (try1 > try0)
					{
						//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
						numPrec[c][r].y = (int) System.Math.Ceiling((try1 - cb0y) / twoppy) - (int) System.Math.Floor((try0 - cb0y) / twoppy);
					}
					else
					{
						numPrec[c][r].y = 0;
					}
					
					// First and last subbands indexes
					mins = (r == 0)?0:1;
					maxs = (r == 0)?1:4;
					
					int maxPrec = numPrec[c][r].x * numPrec[c][r].y;
					
					ttIncl[c][r] = new TagTreeDecoder[maxPrec][];
					for (int i = 0; i < maxPrec; i++)
					{
						ttIncl[c][r][i] = new TagTreeDecoder[maxs + 1];
					}
					ttMaxBP[c][r] = new TagTreeDecoder[maxPrec][];
					for (int i2 = 0; i2 < maxPrec; i2++)
					{
						ttMaxBP[c][r][i2] = new TagTreeDecoder[maxs + 1];
					}
					cbI[c][r] = new CBlkInfo[maxs + 1][][];
					lblock[c][r] = new int[maxs + 1][][];
					
					ppinfo[c][r] = new PrecInfo[maxPrec];
					fillPrecInfo(c, r, mdl[c]);
					
					root = (SubbandSyn) src.getSynSubbandTree(tIdx, c);
					for (int s = mins; s < maxs; s++)
					{
						sb = (SubbandSyn) root.getSubbandByIdx(r, s);
						nBlk = sb.numCb;
						
						cbI[c][r][s] = new CBlkInfo[nBlk.y][];
						for (int i3 = 0; i3 < nBlk.y; i3++)
						{
							cbI[c][r][s][i3] = new CBlkInfo[nBlk.x];
						}
						lblock[c][r][s] = new int[nBlk.y][];
						for (int i4 = 0; i4 < nBlk.y; i4++)
						{
							lblock[c][r][s][i4] = new int[nBlk.x];
						}
						
						for (int i = nBlk.y - 1; i >= 0; i--)
						{
							ArrayUtil.intArraySet(lblock[c][r][s][i], INIT_LBLOCK);
						}
					} // loop on subbands
				} // End loop on resolution levels
			} // End loop on components
			
			return cbI;
		}
		
		/// <summary> Retrives precincts and code-blocks coordinates in the given resolution,
		/// level and component. Finishes TagTreeEncoder initialization as well.
		/// 
		/// </summary>
		/// <param name="c">Component index.
		/// 
		/// </param>
		/// <param name="r">Resolution level index.
		/// 
		/// </param>
		/// <param name="mdl">Number of decomposition level in component <tt>c</tt>.
		/// 
		/// </param>
		private void  fillPrecInfo(int c, int r, int mdl)
		{
			if (ppinfo[c][r].Length == 0)
				return ; // No precinct in this
			// resolution level
			
			Coord tileI = src.getTile(null);
			Coord nTiles = src.getNumTiles(null);
			
			int xsiz, ysiz, x0siz, y0siz;
			int xt0siz, yt0siz;
			int xtsiz, ytsiz;
			
			xt0siz = src.TilePartULX;
			yt0siz = src.TilePartULY;
			xtsiz = src.NomTileWidth;
			ytsiz = src.NomTileHeight;
			x0siz = hd.ImgULX;
			y0siz = hd.ImgULY;
			xsiz = hd.ImgWidth;
			ysiz = hd.ImgHeight;
			
			int tx0 = (tileI.x == 0)?x0siz:xt0siz + tileI.x * xtsiz;
			int ty0 = (tileI.y == 0)?y0siz:yt0siz + tileI.y * ytsiz;
			int tx1 = (tileI.x != nTiles.x - 1)?xt0siz + (tileI.x + 1) * xtsiz:xsiz;
			int ty1 = (tileI.y != nTiles.y - 1)?yt0siz + (tileI.y + 1) * ytsiz:ysiz;
			
			int xrsiz = hd.getCompSubsX(c);
			int yrsiz = hd.getCompSubsY(c);
			
			int tcx0 = src.getResULX(c, mdl);
			int tcy0 = src.getResULY(c, mdl);
			int tcx1 = tcx0 + src.getTileCompWidth(tIdx, c, mdl);
			int tcy1 = tcy0 + src.getTileCompHeight(tIdx, c, mdl);
			
			int ndl = mdl - r;
			//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
			int trx0 = (int) System.Math.Ceiling(tcx0 / (double) (1 << ndl));
			//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
			int try0 = (int) System.Math.Ceiling(tcy0 / (double) (1 << ndl));
			//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
			int trx1 = (int) System.Math.Ceiling(tcx1 / (double) (1 << ndl));
			//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
			int try1 = (int) System.Math.Ceiling(tcy1 / (double) (1 << ndl));
			
			int cb0x = src.CbULX;
			int cb0y = src.CbULY;
			
			double twoppx = (double) getPPX(tIdx, c, r);
			double twoppy = (double) getPPY(tIdx, c, r);
			//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
			int twoppx2 = (int) (twoppx / 2);
			//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
			int twoppy2 = (int) (twoppy / 2);
			
			// Precincts are located at (cb0x+i*twoppx,cb0y+j*twoppy)
			// Valid precincts are those which intersect with the current
			// resolution level
			int maxPrec = ppinfo[c][r].Length;
			int nPrec = 0;
			
			//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
			int istart = (int) System.Math.Floor((try0 - cb0y) / twoppy);
			//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
			int iend = (int) System.Math.Floor((try1 - 1 - cb0y) / twoppy);
			//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
			int jstart = (int) System.Math.Floor((trx0 - cb0x) / twoppx);
			//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
			int jend = (int) System.Math.Floor((trx1 - 1 - cb0x) / twoppx);
			
			int acb0x, acb0y;
			
			SubbandSyn root = src.getSynSubbandTree(tIdx, c);
			SubbandSyn sb = null;
			
			int p0x, p0y, p1x, p1y; // Precinct projection in subband
			int s0x, s0y, s1x, s1y; // Active subband portion
			int cw, ch;
			int kstart, kend, lstart, lend, k0, l0;
			int prg_ulx, prg_uly;
			//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
			int prg_w = (int) twoppx << ndl;
			//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
			int prg_h = (int) twoppy << ndl;
			int tmp1, tmp2;
			
			CBlkCoordInfo cb;
			
			for (int i = istart; i <= iend; i++)
			{
				// Vertical precincts
				for (int j = jstart; j <= jend; j++, nPrec++)
				{
					// Horizontal precincts
					//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
					if (j == jstart && (trx0 - cb0x) % (xrsiz * ((int) twoppx)) != 0)
					{
						prg_ulx = tx0;
					}
					else
					{
						//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
						prg_ulx = cb0x + j * xrsiz * ((int) twoppx << ndl);
					}
					//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
					if (i == istart && (try0 - cb0y) % (yrsiz * ((int) twoppy)) != 0)
					{
						prg_uly = ty0;
					}
					else
					{
						//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
						prg_uly = cb0y + i * yrsiz * ((int) twoppy << ndl);
					}
					
					//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
					ppinfo[c][r][nPrec] = new PrecInfo(r, (int) (cb0x + j * twoppx), (int) (cb0y + i * twoppy), (int) twoppx, (int) twoppy, prg_ulx, prg_uly, prg_w, prg_h);
					
					if (r == 0)
					{
						// LL subband
						acb0x = cb0x;
						acb0y = cb0y;
						
						//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
						p0x = acb0x + j * (int) twoppx;
						//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
						p1x = p0x + (int) twoppx;
						//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
						p0y = acb0y + i * (int) twoppy;
						//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
						p1y = p0y + (int) twoppy;
						
						sb = (SubbandSyn) root.getSubbandByIdx(0, 0);
						s0x = (p0x < sb.ulcx)?sb.ulcx:p0x;
						s1x = (p1x > sb.ulcx + sb.w)?sb.ulcx + sb.w:p1x;
						s0y = (p0y < sb.ulcy)?sb.ulcy:p0y;
						s1y = (p1y > sb.ulcy + sb.h)?sb.ulcy + sb.h:p1y;
						
						// Code-blocks are located at (acb0x+k*cw,acb0y+l*ch)
						cw = sb.nomCBlkW;
						ch = sb.nomCBlkH;
						//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
						k0 = (int) System.Math.Floor((sb.ulcy - acb0y) / (double) ch);
						//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
						kstart = (int) System.Math.Floor((s0y - acb0y) / (double) ch);
						//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
						kend = (int) System.Math.Floor((s1y - 1 - acb0y) / (double) ch);
						//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
						l0 = (int) System.Math.Floor((sb.ulcx - acb0x) / (double) cw);
						//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
						lstart = (int) System.Math.Floor((s0x - acb0x) / (double) cw);
						//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
						lend = (int) System.Math.Floor((s1x - 1 - acb0x) / (double) cw);
						
						if (s1x - s0x <= 0 || s1y - s0y <= 0)
						{
							ppinfo[c][r][nPrec].nblk[0] = 0;
							ttIncl[c][r][nPrec][0] = new TagTreeDecoder(0, 0);
							ttMaxBP[c][r][nPrec][0] = new TagTreeDecoder(0, 0);
						}
						else
						{
							ttIncl[c][r][nPrec][0] = new TagTreeDecoder(kend - kstart + 1, lend - lstart + 1);
							ttMaxBP[c][r][nPrec][0] = new TagTreeDecoder(kend - kstart + 1, lend - lstart + 1);
							CBlkCoordInfo[][] tmpArray = new CBlkCoordInfo[kend - kstart + 1][];
							for (int i2 = 0; i2 < kend - kstart + 1; i2++)
							{
								tmpArray[i2] = new CBlkCoordInfo[lend - lstart + 1];
							}
							ppinfo[c][r][nPrec].cblk[0] = tmpArray;
							ppinfo[c][r][nPrec].nblk[0] = (kend - kstart + 1) * (lend - lstart + 1);
							
							for (int k = kstart; k <= kend; k++)
							{
								// Vertical cblks
								for (int l = lstart; l <= lend; l++)
								{
									// Horiz. cblks
									cb = new CBlkCoordInfo(k - k0, l - l0);
									if (l == l0)
									{
										cb.ulx = sb.ulx;
									}
									else
									{
										cb.ulx = sb.ulx + l * cw - (sb.ulcx - acb0x);
									}
									if (k == k0)
									{
										cb.uly = sb.uly;
									}
									else
									{
										cb.uly = sb.uly + k * ch - (sb.ulcy - acb0y);
									}
									tmp1 = acb0x + l * cw;
									tmp1 = (tmp1 > sb.ulcx)?tmp1:sb.ulcx;
									tmp2 = acb0x + (l + 1) * cw;
									tmp2 = (tmp2 > sb.ulcx + sb.w)?sb.ulcx + sb.w:tmp2;
									cb.w = tmp2 - tmp1;
									tmp1 = acb0y + k * ch;
									tmp1 = (tmp1 > sb.ulcy)?tmp1:sb.ulcy;
									tmp2 = acb0y + (k + 1) * ch;
									tmp2 = (tmp2 > sb.ulcy + sb.h)?sb.ulcy + sb.h:tmp2;
									cb.h = tmp2 - tmp1;
									ppinfo[c][r][nPrec].cblk[0][k - kstart][l - lstart] = cb;
								} // Horizontal code-blocks
							} // Vertical code-blocks
						}
					}
					else
					{
						// HL, LH and HH subbands
						// HL subband
						acb0x = 0;
						acb0y = cb0y;
						
						p0x = acb0x + j * twoppx2;
						p1x = p0x + twoppx2;
						p0y = acb0y + i * twoppy2;
						p1y = p0y + twoppy2;
						
						sb = (SubbandSyn) root.getSubbandByIdx(r, 1);
						s0x = (p0x < sb.ulcx)?sb.ulcx:p0x;
						s1x = (p1x > sb.ulcx + sb.w)?sb.ulcx + sb.w:p1x;
						s0y = (p0y < sb.ulcy)?sb.ulcy:p0y;
						s1y = (p1y > sb.ulcy + sb.h)?sb.ulcy + sb.h:p1y;
						
						// Code-blocks are located at (acb0x+k*cw,acb0y+l*ch)
						cw = sb.nomCBlkW;
						ch = sb.nomCBlkH;
						//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
						k0 = (int) System.Math.Floor((sb.ulcy - acb0y) / (double) ch);
						//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
						kstart = (int) System.Math.Floor((s0y - acb0y) / (double) ch);
						//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
						kend = (int) System.Math.Floor((s1y - 1 - acb0y) / (double) ch);
						//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
						l0 = (int) System.Math.Floor((sb.ulcx - acb0x) / (double) cw);
						//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
						lstart = (int) System.Math.Floor((s0x - acb0x) / (double) cw);
						//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
						lend = (int) System.Math.Floor((s1x - 1 - acb0x) / (double) cw);
						
						if (s1x - s0x <= 0 || s1y - s0y <= 0)
						{
							ppinfo[c][r][nPrec].nblk[1] = 0;
							ttIncl[c][r][nPrec][1] = new TagTreeDecoder(0, 0);
							ttMaxBP[c][r][nPrec][1] = new TagTreeDecoder(0, 0);
						}
						else
						{
							ttIncl[c][r][nPrec][1] = new TagTreeDecoder(kend - kstart + 1, lend - lstart + 1);
							ttMaxBP[c][r][nPrec][1] = new TagTreeDecoder(kend - kstart + 1, lend - lstart + 1);
							CBlkCoordInfo[][] tmpArray2 = new CBlkCoordInfo[kend - kstart + 1][];
							for (int i3 = 0; i3 < kend - kstart + 1; i3++)
							{
								tmpArray2[i3] = new CBlkCoordInfo[lend - lstart + 1];
							}
							ppinfo[c][r][nPrec].cblk[1] = tmpArray2;
							ppinfo[c][r][nPrec].nblk[1] = (kend - kstart + 1) * (lend - lstart + 1);
							
							for (int k = kstart; k <= kend; k++)
							{
								// Vertical cblks
								for (int l = lstart; l <= lend; l++)
								{
									// Horiz. cblks
									cb = new CBlkCoordInfo(k - k0, l - l0);
									if (l == l0)
									{
										cb.ulx = sb.ulx;
									}
									else
									{
										cb.ulx = sb.ulx + l * cw - (sb.ulcx - acb0x);
									}
									if (k == k0)
									{
										cb.uly = sb.uly;
									}
									else
									{
										cb.uly = sb.uly + k * ch - (sb.ulcy - acb0y);
									}
									tmp1 = acb0x + l * cw;
									tmp1 = (tmp1 > sb.ulcx)?tmp1:sb.ulcx;
									tmp2 = acb0x + (l + 1) * cw;
									tmp2 = (tmp2 > sb.ulcx + sb.w)?sb.ulcx + sb.w:tmp2;
									cb.w = tmp2 - tmp1;
									tmp1 = acb0y + k * ch;
									tmp1 = (tmp1 > sb.ulcy)?tmp1:sb.ulcy;
									tmp2 = acb0y + (k + 1) * ch;
									tmp2 = (tmp2 > sb.ulcy + sb.h)?sb.ulcy + sb.h:tmp2;
									cb.h = tmp2 - tmp1;
									ppinfo[c][r][nPrec].cblk[1][k - kstart][l - lstart] = cb;
								} // Horizontal code-blocks
							} // Vertical code-blocks
						}
						
						// LH subband
						acb0x = cb0x;
						acb0y = 0;
						
						p0x = acb0x + j * twoppx2;
						p1x = p0x + twoppx2;
						p0y = acb0y + i * twoppy2;
						p1y = p0y + twoppy2;
						
						sb = (SubbandSyn) root.getSubbandByIdx(r, 2);
						s0x = (p0x < sb.ulcx)?sb.ulcx:p0x;
						s1x = (p1x > sb.ulcx + sb.w)?sb.ulcx + sb.w:p1x;
						s0y = (p0y < sb.ulcy)?sb.ulcy:p0y;
						s1y = (p1y > sb.ulcy + sb.h)?sb.ulcy + sb.h:p1y;
						
						// Code-blocks are located at (acb0x+k*cw,acb0y+l*ch)
						cw = sb.nomCBlkW;
						ch = sb.nomCBlkH;
						//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
						k0 = (int) System.Math.Floor((sb.ulcy - acb0y) / (double) ch);
						//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
						kstart = (int) System.Math.Floor((s0y - acb0y) / (double) ch);
						//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
						kend = (int) System.Math.Floor((s1y - 1 - acb0y) / (double) ch);
						//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
						l0 = (int) System.Math.Floor((sb.ulcx - acb0x) / (double) cw);
						//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
						lstart = (int) System.Math.Floor((s0x - acb0x) / (double) cw);
						//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
						lend = (int) System.Math.Floor((s1x - 1 - acb0x) / (double) cw);
						
						if (s1x - s0x <= 0 || s1y - s0y <= 0)
						{
							ppinfo[c][r][nPrec].nblk[2] = 0;
							ttIncl[c][r][nPrec][2] = new TagTreeDecoder(0, 0);
							ttMaxBP[c][r][nPrec][2] = new TagTreeDecoder(0, 0);
						}
						else
						{
							ttIncl[c][r][nPrec][2] = new TagTreeDecoder(kend - kstart + 1, lend - lstart + 1);
							ttMaxBP[c][r][nPrec][2] = new TagTreeDecoder(kend - kstart + 1, lend - lstart + 1);
							CBlkCoordInfo[][] tmpArray3 = new CBlkCoordInfo[kend - kstart + 1][];
							for (int i4 = 0; i4 < kend - kstart + 1; i4++)
							{
								tmpArray3[i4] = new CBlkCoordInfo[lend - lstart + 1];
							}
							ppinfo[c][r][nPrec].cblk[2] = tmpArray3;
							ppinfo[c][r][nPrec].nblk[2] = (kend - kstart + 1) * (lend - lstart + 1);
							
							for (int k = kstart; k <= kend; k++)
							{
								// Vertical cblks
								for (int l = lstart; l <= lend; l++)
								{
									// Horiz cblks
									cb = new CBlkCoordInfo(k - k0, l - l0);
									if (l == l0)
									{
										cb.ulx = sb.ulx;
									}
									else
									{
										cb.ulx = sb.ulx + l * cw - (sb.ulcx - acb0x);
									}
									if (k == k0)
									{
										cb.uly = sb.uly;
									}
									else
									{
										cb.uly = sb.uly + k * ch - (sb.ulcy - acb0y);
									}
									tmp1 = acb0x + l * cw;
									tmp1 = (tmp1 > sb.ulcx)?tmp1:sb.ulcx;
									tmp2 = acb0x + (l + 1) * cw;
									tmp2 = (tmp2 > sb.ulcx + sb.w)?sb.ulcx + sb.w:tmp2;
									cb.w = tmp2 - tmp1;
									tmp1 = acb0y + k * ch;
									tmp1 = (tmp1 > sb.ulcy)?tmp1:sb.ulcy;
									tmp2 = acb0y + (k + 1) * ch;
									tmp2 = (tmp2 > sb.ulcy + sb.h)?sb.ulcy + sb.h:tmp2;
									cb.h = tmp2 - tmp1;
									ppinfo[c][r][nPrec].cblk[2][k - kstart][l - lstart] = cb;
								} // Horizontal code-blocks
							} // Vertical code-blocks
						}
						
						// HH subband
						acb0x = 0;
						acb0y = 0;
						
						p0x = acb0x + j * twoppx2;
						p1x = p0x + twoppx2;
						p0y = acb0y + i * twoppy2;
						p1y = p0y + twoppy2;
						
						sb = (SubbandSyn) root.getSubbandByIdx(r, 3);
						s0x = (p0x < sb.ulcx)?sb.ulcx:p0x;
						s1x = (p1x > sb.ulcx + sb.w)?sb.ulcx + sb.w:p1x;
						s0y = (p0y < sb.ulcy)?sb.ulcy:p0y;
						s1y = (p1y > sb.ulcy + sb.h)?sb.ulcy + sb.h:p1y;
						
						// Code-blocks are located at (acb0x+k*cw,acb0y+l*ch)
						cw = sb.nomCBlkW;
						ch = sb.nomCBlkH;
						//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
						k0 = (int) System.Math.Floor((sb.ulcy - acb0y) / (double) ch);
						//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
						kstart = (int) System.Math.Floor((s0y - acb0y) / (double) ch);
						//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
						kend = (int) System.Math.Floor((s1y - 1 - acb0y) / (double) ch);
						//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
						l0 = (int) System.Math.Floor((sb.ulcx - acb0x) / (double) cw);
						//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
						lstart = (int) System.Math.Floor((s0x - acb0x) / (double) cw);
						//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
						lend = (int) System.Math.Floor((s1x - 1 - acb0x) / (double) cw);
						
						if (s1x - s0x <= 0 || s1y - s0y <= 0)
						{
							ppinfo[c][r][nPrec].nblk[3] = 0;
							ttIncl[c][r][nPrec][3] = new TagTreeDecoder(0, 0);
							ttMaxBP[c][r][nPrec][3] = new TagTreeDecoder(0, 0);
						}
						else
						{
							ttIncl[c][r][nPrec][3] = new TagTreeDecoder(kend - kstart + 1, lend - lstart + 1);
							ttMaxBP[c][r][nPrec][3] = new TagTreeDecoder(kend - kstart + 1, lend - lstart + 1);
							CBlkCoordInfo[][] tmpArray4 = new CBlkCoordInfo[kend - kstart + 1][];
							for (int i5 = 0; i5 < kend - kstart + 1; i5++)
							{
								tmpArray4[i5] = new CBlkCoordInfo[lend - lstart + 1];
							}
							ppinfo[c][r][nPrec].cblk[3] = tmpArray4;
							ppinfo[c][r][nPrec].nblk[3] = (kend - kstart + 1) * (lend - lstart + 1);
							
							for (int k = kstart; k <= kend; k++)
							{
								// Vertical cblks
								for (int l = lstart; l <= lend; l++)
								{
									// Horiz cblks
									cb = new CBlkCoordInfo(k - k0, l - l0);
									if (l == l0)
									{
										cb.ulx = sb.ulx;
									}
									else
									{
										cb.ulx = sb.ulx + l * cw - (sb.ulcx - acb0x);
									}
									if (k == k0)
									{
										cb.uly = sb.uly;
									}
									else
									{
										cb.uly = sb.uly + k * ch - (sb.ulcy - acb0y);
									}
									tmp1 = acb0x + l * cw;
									tmp1 = (tmp1 > sb.ulcx)?tmp1:sb.ulcx;
									tmp2 = acb0x + (l + 1) * cw;
									tmp2 = (tmp2 > sb.ulcx + sb.w)?sb.ulcx + sb.w:tmp2;
									cb.w = tmp2 - tmp1;
									tmp1 = acb0y + k * ch;
									tmp1 = (tmp1 > sb.ulcy)?tmp1:sb.ulcy;
									tmp2 = acb0y + (k + 1) * ch;
									tmp2 = (tmp2 > sb.ulcy + sb.h)?sb.ulcy + sb.h:tmp2;
									cb.h = tmp2 - tmp1;
									ppinfo[c][r][nPrec].cblk[3][k - kstart][l - lstart] = cb;
								} // Horizontal code-blocks
							} // Vertical code-blocks
						}
					}
				} // Horizontal precincts
			} // Vertical precincts
		}
		
		/// <summary> Gets the number of precincts in a given component and resolution level.
		/// 
		/// </summary>
		/// <param name="c">Component index
		/// 
		/// </param>
		/// <param name="r">Resolution index
		/// 
		/// </param>
		public virtual int getNumPrecinct(int c, int r)
		{
			return numPrec[c][r].x * numPrec[c][r].y;
		}
		
		/// <summary> Read specified packet head and found length of each code-block's piece
		/// of codewords as well as number of skipped most significant bit-planes.
		/// 
		/// </summary>
		/// <param name="l">layer index
		/// 
		/// </param>
		/// <param name="r">Resolution level index
		/// 
		/// </param>
		/// <param name="c">Component index
		/// 
		/// </param>
		/// <param name="p">Precinct index
		/// 
		/// </param>
		/// <param name="cbI">CBlkInfo array of relevant component and resolution
		/// level.
		/// 
		/// </param>
		/// <param name="nb">The number of bytes to read in each tile before reaching
		/// output rate (used by truncation mode)
		/// 
		/// </param>
		/// <returns> True if specified output rate or EOF is reached.
		/// 
		/// </returns>
		public virtual bool readPktHead(int l, int r, int c, int p, CBlkInfo[][][] cbI, int[] nb)
		{
			
			CBlkInfo ccb;
			int nSeg; // number of segment to read
			int cbLen; // Length of cblk's code-words
			int ltp; // last truncation point index
			int passtype; // coding pass type
			TagTreeDecoder tdIncl, tdBD;
			int tmp, tmp2, totnewtp, lblockCur, tpidx;
			int sumtotnewtp = 0;
			Coord cbc;
			int startPktHead = ehs.Pos;
			if (startPktHead >= ehs.length())
			{
				// EOF reached at the beginning of this packet head
				return true;
			}
			int tIdx = src.TileIdx;
			PktHeaderBitReader bin;
			int mend, nend;
			int b;
			SubbandSyn sb;
			SubbandSyn root = src.getSynSubbandTree(tIdx, c);
			
			// If packed packet headers was used, use separate stream for reading
			// of packet headers
			if (pph)
			{
				bin = new PktHeaderBitReader(pphbais);
			}
			else
			{
				bin = this.bin;
			}
			
			int mins = (r == 0)?0:1;
			int maxs = (r == 0)?1:4;
			
			bool precFound = false;
			for (int s = mins; s < maxs; s++)
			{
				if (p < ppinfo[c][r].Length)
				{
					precFound = true;
				}
			}
			if (!precFound)
			{
				return false;
			}
			
			PrecInfo prec = ppinfo[c][r][p];
			
			// Synchronize for bit reading
			bin.sync();
			
			// If packet is empty there is no info in it (i.e. no code-blocks)
			if (bin.readBit() == 0)
			{
				// No code-block is included
				cblks = new System.Collections.ArrayList[maxs + 1];
				for (int s = mins; s < maxs; s++)
				{
					cblks[s] = System.Collections.ArrayList.Synchronized(new System.Collections.ArrayList(10));
				}
				pktIdx++;
				
				// If truncation mode, checks if output rate is reached
				// unless ncb quit condition is used in which case headers
				// are not counted
				if (isTruncMode && maxCB == - 1)
				{
					tmp = ehs.Pos - startPktHead;
					if (tmp > nb[tIdx])
					{
						nb[tIdx] = 0;
						return true;
					}
					else
					{
						nb[tIdx] -= tmp;
					}
				}
				
				// Read EPH marker if needed
				if (ephUsed)
				{
					readEPHMarker(bin);
				}
				return false;
			}
			
			// Packet is not empty => decode info
			// Loop on each subband in this resolution level
			if (cblks == null || cblks.Length < maxs + 1)
			{
				cblks = new System.Collections.ArrayList[maxs + 1];
			}
			
			for (int s = mins; s < maxs; s++)
			{
				if (cblks[s] == null)
				{
					cblks[s] = System.Collections.ArrayList.Synchronized(new System.Collections.ArrayList(10));
				}
				else
				{
					cblks[s].Clear();
				}
				sb = (SubbandSyn) root.getSubbandByIdx(r, s);
				// No code-block in this precinct
				if (prec.nblk[s] == 0)
				{
					// Go to next subband
					continue;
				}
				
				tdIncl = ttIncl[c][r][p][s];
				tdBD = ttMaxBP[c][r][p][s];
				
				mend = (prec.cblk[s] == null)?0:prec.cblk[s].Length;
				for (int m = 0; m < mend; m++)
				{
					// Vertical code-blocks
					nend = (prec.cblk[s][m] == null)?0:prec.cblk[s][m].Length;
					for (int n = 0; n < nend; n++)
					{
						// Horizontal code-blocks
						cbc = prec.cblk[s][m][n].idx;
						b = cbc.x + cbc.y * sb.numCb.x;
						
						ccb = cbI[s][cbc.y][cbc.x];
						
						try
						{
							// If code-block not included in previous layer(s)
							if (ccb == null || ccb.ctp == 0)
							{
								if (ccb == null)
								{
									ccb = cbI[s][cbc.y][cbc.x] = new CBlkInfo(prec.cblk[s][m][n].ulx, prec.cblk[s][m][n].uly, prec.cblk[s][m][n].w, prec.cblk[s][m][n].h, nl);
								}
								ccb.pktIdx[l] = pktIdx;
								
								// Read inclusion using tag-tree
								tmp = tdIncl.update(m, n, l + 1, bin);
								if (tmp > l)
								{
									// Not included
									continue;
								}
								
								// Read bitdepth using tag-tree
								tmp = 1; // initialization
								for (tmp2 = 1; tmp >= tmp2; tmp2++)
								{
									tmp = tdBD.update(m, n, tmp2, bin);
								}
								ccb.msbSkipped = tmp2 - 2;
								
								// New code-block => at least one truncation point
								totnewtp = 1;
								ccb.addNTP(l, 0);
								
								// Check whether ncb quit condition is reached
								ncb++;
								
								if (maxCB != - 1 && !ncbQuit && ncb == maxCB)
								{
									// ncb quit contidion reached
									ncbQuit = true;
									tQuit = tIdx;
									cQuit = c;
									sQuit = s;
									rQuit = r;
									xQuit = cbc.x;
									yQuit = cbc.y;
								}
							}
							else
							{
								// If code-block already included in one of
								// the previous layers.
								
								ccb.pktIdx[l] = pktIdx;
								
								// If not inclused
								if (bin.readBit() != 1)
								{
									continue;
								}
								
								// At least 1 more truncation point than
								// prev. packet
								totnewtp = 1;
							}
							
							// Read new truncation points
							if (bin.readBit() == 1)
							{
								// if bit is 1
								totnewtp++;
								
								// if next bit is 0 do nothing
								if (bin.readBit() == 1)
								{
									//if is 1
									totnewtp++;
									
									tmp = bin.readBits(2);
									totnewtp += tmp;
									
									// If next 2 bits are not 11 do nothing
									if (tmp == 0x3)
									{
										//if 11
										tmp = bin.readBits(5);
										totnewtp += tmp;
										
										// If next 5 bits are not 11111 do nothing
										if (tmp == 0x1F)
										{
											//if 11111
											totnewtp += bin.readBits(7);
										}
									}
								}
							}
							ccb.addNTP(l, totnewtp);
							sumtotnewtp += totnewtp;
							cblks[s].Add(prec.cblk[s][m][n]);
							
							// Code-block length
							
							// -- Compute the number of bit to read to obtain
							// code-block length.  
							// numBits = betaLamda + log2(totnewtp);
							
							// The length is signalled for each segment in
							// addition to the final one. The total length is the
							// sum of all segment lengths.
							
							// If regular termination in use, then there is one
							// segment per truncation point present. Otherwise, if
							// selective arithmetic bypass coding mode is present,
							// then there is one termination per bypass/MQ and
							// MQ/bypass transition. Otherwise the only
							// termination is at the end of the code-block.
							int options = ((System.Int32) decSpec.ecopts.getTileCompVal(tIdx, c));
							
							if ((options & CSJ2K.j2k.entropy.StdEntropyCoderOptions.OPT_TERM_PASS) != 0)
							{
								// Regular termination in use, one segment per new
								// pass (i.e. truncation point)
								nSeg = totnewtp;
							}
							else if ((options & CSJ2K.j2k.entropy.StdEntropyCoderOptions.OPT_BYPASS) != 0)
							{
								// Selective arithmetic coding bypass coding mode
								// in use, but no regular termination 1 segment up
								// to the end of the last pass of the 4th most
								// significant bit-plane, and, in each following
								// bit-plane, one segment upto the end of the 2nd
								// pass and one upto the end of the 3rd pass.
								
								if (ccb.ctp <= CSJ2K.j2k.entropy.StdEntropyCoderOptions.FIRST_BYPASS_PASS_IDX)
								{
									nSeg = 1;
								}
								else
								{
									nSeg = 1; // One at least for last pass
									// And one for each other terminated pass
									for (tpidx = ccb.ctp - totnewtp; tpidx < ccb.ctp - 1; tpidx++)
									{
										if (tpidx >= CSJ2K.j2k.entropy.StdEntropyCoderOptions.FIRST_BYPASS_PASS_IDX - 1)
										{
											passtype = (tpidx + CSJ2K.j2k.entropy.StdEntropyCoderOptions.NUM_EMPTY_PASSES_IN_MS_BP) % CSJ2K.j2k.entropy.StdEntropyCoderOptions.NUM_PASSES;
											if (passtype == 1 || passtype == 2)
											{
												// bypass coding just before MQ
												// pass or MQ pass just before
												// bypass coding => terminated
												nSeg++;
											}
										}
									}
								}
							}
							else
							{
								// Nothing special in use, just one segment
								nSeg = 1;
							}
							
							// Reads lblock increment (common to all segments)
							while (bin.readBit() != 0)
							{
								lblock[c][r][s][cbc.y][cbc.x]++;
							}
							
							if (nSeg == 1)
							{
								// Only one segment in packet
								cbLen = bin.readBits(lblock[c][r][s][cbc.y][cbc.x] + MathUtil.log2(totnewtp));
							}
							else
							{
								// We must read one length per segment
								ccb.segLen[l] = new int[nSeg];
								cbLen = 0;
								int j;
								if ((options & CSJ2K.j2k.entropy.StdEntropyCoderOptions.OPT_TERM_PASS) != 0)
								{
									// Regular termination: each pass is terminated
									for (tpidx = ccb.ctp - totnewtp, j = 0; tpidx < ccb.ctp; tpidx++, j++)
									{
										
										lblockCur = lblock[c][r][s][cbc.y][cbc.x];
										
										tmp = bin.readBits(lblockCur);
										ccb.segLen[l][j] = tmp;
										cbLen += tmp;
									}
								}
								else
								{
									// Bypass coding: only some passes are
									// terminated
									ltp = ccb.ctp - totnewtp - 1;
									for (tpidx = ccb.ctp - totnewtp, j = 0; tpidx < ccb.ctp - 1; tpidx++)
									{
										if (tpidx >= CSJ2K.j2k.entropy.StdEntropyCoderOptions.FIRST_BYPASS_PASS_IDX - 1)
										{
											passtype = (tpidx + CSJ2K.j2k.entropy.StdEntropyCoderOptions.NUM_EMPTY_PASSES_IN_MS_BP) % CSJ2K.j2k.entropy.StdEntropyCoderOptions.NUM_PASSES;
											if (passtype == 0)
												continue;
											
											lblockCur = lblock[c][r][s][cbc.y][cbc.x];
											tmp = bin.readBits(lblockCur + MathUtil.log2(tpidx - ltp));
											ccb.segLen[l][j] = tmp;
											cbLen += tmp;
											ltp = tpidx;
											j++;
										}
									}
									// Last pass has always the length sent
									lblockCur = lblock[c][r][s][cbc.y][cbc.x];
									tmp = bin.readBits(lblockCur + MathUtil.log2(tpidx - ltp));
									cbLen += tmp;
									ccb.segLen[l][j] = tmp;
								}
							}
							ccb.len[l] = cbLen;
							
							// If truncation mode, checks if output rate is reached
							// unless ncb and lbody quit contitions used.
							if (isTruncMode && maxCB == - 1)
							{
								tmp = ehs.Pos - startPktHead;
								if (tmp > nb[tIdx])
								{
									nb[tIdx] = 0;
									// Remove found information in this code-block
									if (l == 0)
									{
										cbI[s][cbc.y][cbc.x] = null;
									}
									else
									{
										ccb.off[l] = ccb.len[l] = 0;
										ccb.ctp -= ccb.ntp[l];
										ccb.ntp[l] = 0;
										ccb.pktIdx[l] = - 1;
									}
									return true;
								}
							}
						}
						catch (System.IO.EndOfStreamException e)
						{
							// Remove found information in this code-block
							if (l == 0)
							{
								cbI[s][cbc.y][cbc.x] = null;
							}
							else
							{
								ccb.off[l] = ccb.len[l] = 0;
								ccb.ctp -= ccb.ntp[l];
								ccb.ntp[l] = 0;
								ccb.pktIdx[l] = - 1;
							}
							//                         throw new EOFException();
							return true;
						}
					} // End loop on horizontal code-blocks
				} // End loop on vertical code-blocks
			} // End loop on subbands
			
			// Read EPH marker if needed
			if (ephUsed)
			{
				readEPHMarker(bin);
			}
			
			pktIdx++;
			
			// If truncation mode, checks if output rate is reached
			if (isTruncMode && maxCB == - 1)
			{
				tmp = ehs.Pos - startPktHead;
				if (tmp > nb[tIdx])
				{
					nb[tIdx] = 0;
					return true;
				}
				else
				{
					nb[tIdx] -= tmp;
				}
			}
			return false;
		}
		
		/// <summary> Reads specificied packet body in order to find offset of each
		/// code-block's piece of codeword. This use the list of found code-blocks
		/// in previous red packet head.
		/// 
		/// </summary>
		/// <param name="l">layer index
		/// 
		/// </param>
		/// <param name="r">Resolution level index
		/// 
		/// </param>
		/// <param name="c">Component index
		/// 
		/// </param>
		/// <param name="p">Precinct index
		/// 
		/// </param>
		/// <param name="cbI">CBlkInfo array of relevant component and resolution
		/// level.
		/// 
		/// </param>
		/// <param name="nb">The remainding number of bytes to read from the bit stream in
		/// each tile before reaching the decoding rate (in truncation mode)
		/// 
		/// </param>
		/// <returns> True if decoding rate is reached 
		/// 
		/// </returns>
		public virtual bool readPktBody(int l, int r, int c, int p, CBlkInfo[][][] cbI, int[] nb)
		{
			int curOff = ehs.Pos;
			//Coord curCB;
			CBlkInfo ccb;
			bool stopRead = false;
			int tIdx = src.TileIdx;
			Coord cbc;
			
			bool precFound = false;
			int mins = (r == 0)?0:1;
			int maxs = (r == 0)?1:4;
			for (int s = mins; s < maxs; s++)
			{
				if (p < ppinfo[c][r].Length)
				{
					precFound = true;
				}
			}
			if (!precFound)
			{
				return false;
			}
			
			for (int s = mins; s < maxs; s++)
			{
				for (int numCB = 0; numCB < cblks[s].Count; numCB++)
				{
					cbc = ((CBlkCoordInfo) cblks[s][numCB]).idx;
					ccb = cbI[s][cbc.y][cbc.x];
					ccb.off[l] = curOff;
					curOff += ccb.len[l];
					try
					{
						ehs.seek(curOff);
					}
					catch (System.IO.EndOfStreamException e)
					{
						if (l == 0)
						{
							cbI[s][cbc.y][cbc.x] = null;
						}
						else
						{
							ccb.off[l] = ccb.len[l] = 0;
							ccb.ctp -= ccb.ntp[l];
							ccb.ntp[l] = 0;
							ccb.pktIdx[l] = - 1;
						}
						throw new System.IO.EndOfStreamException();
					}
					
					// If truncation mode
					if (isTruncMode)
					{
						if (stopRead || ccb.len[l] > nb[tIdx])
						{
							// Remove found information in this code-block
							if (l == 0)
							{
								cbI[s][cbc.y][cbc.x] = null;
							}
							else
							{
								ccb.off[l] = ccb.len[l] = 0;
								ccb.ctp -= ccb.ntp[l];
								ccb.ntp[l] = 0;
								ccb.pktIdx[l] = - 1;
							}
							stopRead = true;
						}
						if (!stopRead)
						{
							nb[tIdx] -= ccb.len[l];
						}
					}
					// If ncb quit condition reached
					if (ncbQuit && r == rQuit && s == sQuit && cbc.x == xQuit && cbc.y == yQuit && tIdx == tQuit && c == cQuit)
					{
						cbI[s][cbc.y][cbc.x] = null;
						stopRead = true;
					}
				} // Loop on code-blocks
			} // End loop on subbands
			
			// Seek to the end of the packet
			ehs.seek(curOff);
			
			if (stopRead)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		
		
		/// <summary> Returns the precinct partition width for the specified component,
		/// resolution level and tile.
		/// 
		/// </summary>
		/// <param name="t">the tile index
		/// 
		/// </param>
		/// <param name="c">The index of the component (between 0 and C-1)
		/// 
		/// </param>
		/// <param name="r">The resolution level, from 0 to L.
		/// 
		/// </param>
		/// <returns> the precinct partition width for the specified component,
		/// resolution level and tile.
		/// 
		/// </returns>
		public int getPPX(int t, int c, int r)
		{
			return decSpec.pss.getPPX(t, c, r);
		}
		
		/// <summary> Returns the precinct partition height for the specified component,
		/// resolution level and tile.
		/// 
		/// </summary>
		/// <param name="t">the tile index
		/// 
		/// </param>
		/// <param name="c">The index of the component (between 0 and C-1)
		/// 
		/// </param>
		/// <param name="rl">The resolution level, from 0 to L.
		/// 
		/// </param>
		/// <returns> the precinct partition height in the specified component, for
		/// the specified resolution level, for the current tile.
		/// 
		/// </returns>
		public int getPPY(int t, int c, int rl)
		{
			return decSpec.pss.getPPY(t, c, rl);
		}
		
		/// <summary> Try to read a SOP marker and check that its sequence number if not out
		/// of sequence. If so, an error is thrown.
		/// 
		/// </summary>
		/// <param name="nBytes">The number of bytes left to read from each tile
		/// 
		/// </param>
		/// <param name="p">Precinct index
		/// 
		/// </param>
		/// <param name="r">Resolution level index
		/// 
		/// </param>
		/// <param name="c">Component index
		/// 
		/// </param>
		public virtual bool readSOPMarker(int[] nBytes, int p, int c, int r)
		{
			int val;
			byte[] sopArray = new byte[6];
			int tIdx = src.TileIdx;
			int mins = (r == 0)?0:1;
			int maxs = (r == 0)?1:4;
			bool precFound = false;
			for (int s = mins; s < maxs; s++)
			{
				if (p < ppinfo[c][r].Length)
				{
					precFound = true;
				}
			}
			if (!precFound)
			{
				return false;
			}
			
			// If SOP markers are not used, return
			if (!sopUsed)
			{
				return false;
			}
			
			// Check if SOP is used for this packet
			int pos = ehs.Pos;
			if ((short) ((ehs.read() << 8) | ehs.read()) != CSJ2K.j2k.codestream.Markers.SOP)
			{
				ehs.seek(pos);
				return false;
			}
			ehs.seek(pos);
			
			// If length of SOP marker greater than remaining bytes to read for
			// this tile return true
			if (nBytes[tIdx] < 6)
			{
				return true;
			}
			nBytes[tIdx] -= 6;
			
			// Read marker into array 'sopArray'
			ehs.readFully(sopArray, 0, CSJ2K.j2k.codestream.Markers.SOP_LENGTH);
			
			// Check if this is the correct marker
			val = sopArray[0];
			val <<= 8;
			val |= sopArray[1];
			if (val != CSJ2K.j2k.codestream.Markers.SOP)
			{
				throw new System.ApplicationException("Corrupted Bitstream: Could not parse SOP " + "marker !");
			}
			
			// Check if length is correct
			val = (sopArray[2] & 0xff);
			val <<= 8;
			val |= (sopArray[3] & 0xff);
			if (val != 4)
			{
				throw new System.ApplicationException("Corrupted Bitstream: Corrupted SOP marker !");
			}
			
			// Check if sequence number if ok
			val = (sopArray[4] & 0xff);
			val <<= 8;
			val |= (sopArray[5] & 0xff);
			
			if (!pph && val != pktIdx)
			{
				throw new System.ApplicationException("Corrupted Bitstream: SOP marker out of " + "sequence !");
			}
			if (pph && val != pktIdx - 1)
			{
				// if packed packet headers are used, packet header was read
				// before SOP marker segment
				throw new System.ApplicationException("Corrupted Bitstream: SOP marker out of " + "sequence !");
			}
			return false;
		}
		
		/// <summary> Try to read an EPH marker. If it is not possible then an Error is
		/// thrown.
		/// 
		/// </summary>
		/// <param name="bin">The packet header reader to read the EPH marker from
		/// 
		/// </param>
		public virtual void  readEPHMarker(PktHeaderBitReader bin)
		{
			int val;
			byte[] ephArray = new byte[2];
			
			if (bin.usebais)
			{
				bin.bais.Read(ephArray, 0, CSJ2K.j2k.codestream.Markers.EPH_LENGTH);
			}
			else
			{
				bin.in_Renamed.readFully(ephArray, 0, CSJ2K.j2k.codestream.Markers.EPH_LENGTH);
			}
			
			// Check if this is the correct marker
			val = ephArray[0];
			val <<= 8;
			val |= ephArray[1];
			if (val != CSJ2K.j2k.codestream.Markers.EPH)
			{
				throw new System.ApplicationException("Corrupted Bitstream: Could not parse EPH " + "marker ! ");
			}
		}
		
		/// <summary> Get PrecInfo instance of the specified resolution level, component and
		/// precinct.
		/// 
		/// </summary>
		/// <param name="c">Component index.
		/// 
		/// </param>
		/// <param name="r">Resolution level index.
		/// 
		/// </param>
		/// <param name="p">Precinct index.
		/// 
		/// </param>
		public virtual PrecInfo getPrecInfo(int c, int r, int p)
		{
			return ppinfo[c][r][p];
		}
	}
}