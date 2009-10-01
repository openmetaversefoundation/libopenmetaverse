/*
* CVS identifier:
*
* $Id: FileBitstreamReaderAgent.java,v 1.68 2002/07/19 12:34:38 grosbois Exp $
*
* Class:                   FileBitstreamReaderAgent
*
* Description:             Retrieve code-blocks codewords in the bit stream
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
using CSJ2K.j2k.quantization.dequantizer;
using CSJ2K.j2k.wavelet.synthesis;
using CSJ2K.j2k.entropy.decoder;
using CSJ2K.j2k.codestream;
using CSJ2K.j2k.decoder;
using CSJ2K.j2k.entropy;
using CSJ2K.j2k.image;
using CSJ2K.j2k.util;
using CSJ2K.j2k.io;
using CSJ2K.j2k;
namespace CSJ2K.j2k.codestream.reader
{
	
	/// <summary> This class reads the bit stream (with the help of HeaderDecoder for tile
	/// headers and PktDecoder for packets header and body) and retrives location
	/// of all code-block's codewords.
	/// 
	/// <p>Note: All tile-parts headers are read by the constructor whereas packets
	/// are processed when decoding related tile (when setTile method is
	/// called).</p>
	/// 
	/// <p>In parsing mode, the reader simulates a virtual layer-resolution
	/// progressive bit stream with the same truncation points in each code-block,
	/// whereas in truncation mode, only the first bytes are taken into account (it
	/// behaves like if it is a real truncated codestream).</p>
	/// 
	/// </summary>
	/// <seealso cref="HeaderDecoder">
	/// </seealso>
	/// <seealso cref="PktDecoder">
	/// 
	/// </seealso>
	public class FileBitstreamReaderAgent:BitstreamReaderAgent
	{
		/// <summary>Gets the reference to the CBlkInfo array </summary>
		virtual public CBlkInfo[][][][][] CBlkInfo
		{
			get
			{
				return cbI;
			}
			
		}
		
		/// <summary>Whether or not the last read Psot value was zero. Only the Psot in the
		/// last tile-part in the codestream can have such a value. 
		/// </summary>
		private bool isPsotEqualsZero = true;
		
		/// <summary>Reference to the PktDecoder instance </summary>
		public PktDecoder pktDec;
		
		/// <summary>Reference to the ParameterList instance </summary>
		private ParameterList pl;
		
		/// <summary>The RandomAccessIO where to get data from </summary>
		private RandomAccessIO in_Renamed;
		
		/// <summary>Offset of the first packet in each tile-part in each tile </summary>
		private int[][] firstPackOff;
		
		/// <summary> Returns the number of tile-part found for a given tile
		/// 
		/// </summary>
		/// <param name="t">Tile index
		/// 
		/// 
		/// </param>
		public virtual int getNumTileParts(int t)
		{
			if (firstPackOff == null || firstPackOff[t] == null)
			{
				throw new System.ApplicationException("Tile " + t + " not found in input codestream.");
			}
			return firstPackOff[t].Length;
		}
		
		/// <summary> Number of bytes allocated to each tile. In parsing mode, this number
		/// is related to the tile length in the codestream whereas in truncation
		/// mode all the rate is affected to the first tiles. 
		/// </summary>
		private int[] nBytes;
		
		/// <summary>Whether or not to print information found in codestream </summary>
		private bool printInfo = false;
		
		/// <summary> Backup of the number of bytes allocated to each tile. This array is
		/// used to restore the number of bytes to read in each tile when the
		/// codestream is read several times (for instance when decoding an R,G,B
		/// image to three output files)
		/// 
		/// </summary>
		private int[] baknBytes;
		
		/// <summary>Length of each tile-part (written in Psot) </summary>
		private int[][] tilePartLen;
		
		/// <summary>Total length of each tile </summary>
		private int[] totTileLen;
		
		/// <summary>Total length of tiles' header </summary>
		private int[] totTileHeadLen;
		
		/// <summary>First tile part header length</summary>
		private int firstTilePartHeadLen;
		
		/// <summary>Total length of all tile parts in all tiles </summary>
		private double totAllTileLen;
		
		/// <summary>Length of main header </summary>
		private int mainHeadLen;
		
		/// <summary>Length of main and tile-parts headers </summary>
		private int headLen = 0;
		
		/// <summary>Length of all tile-part headers </summary>
		private int[][] tilePartHeadLen;
		
		/// <summary>Length of each packet head found in the tile </summary>
		private System.Collections.ArrayList pktHL;

        /// <summary>Layer starting positions</summary>
        public System.Collections.Generic.List<int> layerStarts;
		
		/// <summary>True if truncation mode is used. False if parsing mode </summary>
		private bool isTruncMode;
		
		/// <summary>The number of tile-parts that remain to read </summary>
		private int remainingTileParts;
		
		/// <summary>The number of tile-parts read so far for each tile </summary>
		private int[] tilePartsRead;
		
		/// <summary>Thetotal  number of tile-parts read so far </summary>
		private int totTilePartsRead = 0;
		
		/// <summary>The number of found tile-parts in each tile. </summary>
		private int[] tileParts;
		
		/// <summary>The current tile part being used </summary>
		private int curTilePart;
		
		/// <summary>The number of the tile-parts found in the codestream after reading the
		/// tp'th tile-part of tile t 
		/// </summary>
		private int[][] tilePartNum;
		
		/// <summary>Whether or not a EOC marker has been found instead of a SOT </summary>
		private bool isEOCFound = false;
		
		/// <summary>Reference to the HeaderInfo instance (used when reading SOT marker
		/// segments) 
		/// </summary>
		private HeaderInfo hi;
		
		/// <summary>Array containing information for all the code-blocks:
		/// 
		/// <ul>
		/// <li>1st dim: component index.</li>
		/// <li>2nd dim: resolution level index.</li>
		/// <li>3rd dim: subband index.</li>
		/// <li>4th/5th dim: code-block index (vert. and horiz.).</li>
		/// </ul>
		/// </summary>
		private CBlkInfo[][][][][] cbI;
		
		/// <summary>The maximum number of layers to decode for any code-block </summary>
		private int lQuit;
		
		/// <summary>Whether or not to use only first progression order </summary>
		private bool usePOCQuit = false;
		
		/// <summary> Reads all tiles headers and keep offset of their first
		/// packet. Finally it calls the rate allocation method.
		/// 
		/// </summary>
		/// <param name="hd">HeaderDecoder of the codestream.
		/// 
		/// </param>
		/// <param name="ehs">The input stream where to read bit-stream.
		/// 
		/// </param>
		/// <param name="decSpec">The decoder specifications 
		/// 
		/// </param>
		/// <param name="pl">The ParameterList instance created from the
		/// command-line arguments.
		/// 
		/// </param>
		/// <param name="cdstrInfo">Whether or not to print information found in
		/// codestream. 
		/// 
		/// </param>
		/// <seealso cref="allocateRate">
		/// 
		/// </seealso>
		public FileBitstreamReaderAgent(HeaderDecoder hd, RandomAccessIO ehs, DecoderSpecs decSpec, ParameterList pl, bool cdstrInfo, HeaderInfo hi):base(hd, decSpec)
		{
			
			this.pl = pl;
			this.printInfo = cdstrInfo;
			this.hi = hi;
			System.String strInfo = "Codestream elements information in bytes " + "(offset, total length, header length):\n\n";
			
			// Check whether quit conditiosn used
			usePOCQuit = pl.getBooleanParameter("poc_quit");
			
			// Get decoding rate
			bool rateInBytes;
			bool parsing = pl.getBooleanParameter("parsing");
			try
			{
				trate = pl.getFloatParameter("rate");
				if (trate == - 1)
				{
					trate = System.Single.MaxValue;
				}
			}
			catch (System.FormatException)
			{
				throw new System.ApplicationException("Invalid value in 'rate' option: " + pl.getParameter("rate"));
			}
			catch (System.ArgumentException)
			{
				throw new System.ApplicationException("'rate' option is missing");
			}
			
			try
			{
				tnbytes = pl.getIntParameter("nbytes");
			}
			catch (System.FormatException)
			{
				throw new System.ApplicationException("Invalid value in 'nbytes' option: " + pl.getParameter("nbytes"));
			}
			catch (System.ArgumentException)
			{
				throw new System.ApplicationException("'nbytes' option is missing");
			}
			
			// Check that '-rate' and '-nbytes' are not used at the same time
			ParameterList defaults = pl.DefaultParameterList;
			if (tnbytes != defaults.getFloatParameter("nbytes"))
			{
				rateInBytes = true;
			}
			else
			{
				rateInBytes = false;
			}
			
			if (rateInBytes)
			{
				trate = tnbytes * 8f / hd.MaxCompImgWidth / hd.MaxCompImgHeight;
			}
			else
			{
				//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
				tnbytes = (int) (trate * hd.MaxCompImgWidth * hd.MaxCompImgHeight) / 8;
                if (tnbytes < 0) tnbytes = int.MaxValue;
			}
			isTruncMode = !pl.getBooleanParameter("parsing");
			
			// Check if quit conditions are being used
			int ncbQuit;
			try
			{
				ncbQuit = pl.getIntParameter("ncb_quit");
			}
			catch (System.FormatException)
			{
				throw new System.ApplicationException("Invalid value in 'ncb_quit' option: " + pl.getParameter("ncb_quit"));
			}
			catch (System.ArgumentException)
			{
				throw new System.ApplicationException("'ncb_quit' option is missing");
			}
			if (ncbQuit != - 1 && !isTruncMode)
			{
				throw new System.ApplicationException("Cannot use -parsing and -ncb_quit condition at " + "the same time.");
			}
			
			try
			{
				lQuit = pl.getIntParameter("l_quit");
			}
			catch (System.FormatException)
			{
				throw new System.ApplicationException("Invalid value in 'l_quit' option: " + pl.getParameter("l_quit"));
			}
			catch (System.ArgumentException)
			{
				throw new System.ApplicationException("'l_quit' option is missing");
			}
			
			// initializations
			in_Renamed = ehs;
			pktDec = new PktDecoder(decSpec, hd, ehs, this, isTruncMode, ncbQuit);
			
			tileParts = new int[nt];
			totTileLen = new int[nt];
			tilePartLen = new int[nt][];
			tilePartNum = new int[nt][];
			firstPackOff = new int[nt][];
			tilePartsRead = new int[nt];
			totTileHeadLen = new int[nt];
			tilePartHeadLen = new int[nt][];
			nBytes = new int[nt];
			baknBytes = new int[nt];
			hd.nTileParts = new int[nt];
			
            // CONVERSION PROBLEM?
			//this.isTruncMode = isTruncMode;
			int t = 0, pos, tp = 0, tptot = 0;
			
			// Keeps main header's length, takes file format overhead into account
			int cdstreamStart = hd.mainHeadOff; // Codestream offset in the file
			mainHeadLen = in_Renamed.Pos - cdstreamStart;
			headLen = mainHeadLen;
			
			// If ncb and lbody quit conditions are used, headers are not counted
			if (ncbQuit == - 1)
			{
				anbytes = mainHeadLen;
			}
			else
			{
				anbytes = 0;
			}
			
			strInfo += ("Main header length    : " + cdstreamStart + ", " + mainHeadLen + ", " + mainHeadLen + "\n");
			
			// If cannot even read the first tile-part
			if (anbytes > tnbytes)
			{
				throw new System.ApplicationException("Requested bitrate is too small.");
			}
			
			// Read all tile-part headers from all tiles.
			int tilePartStart;
			bool rateReached = false;
			int mdl;
			//int numtp = 0;
			totAllTileLen = 0;
			remainingTileParts = nt; // at least as many tile-parts as tiles
			int maxTP = nt; // If maximum 1 tile part per tile specified
			
			try
			{
				while (remainingTileParts != 0)
				{
					
					tilePartStart = in_Renamed.Pos;
					// Read tile-part header
					try
					{
						t = readTilePartHeader();
						if (isEOCFound)
						{
							// Some tiles are missing but the
							// codestream is OK
							break;
						}
						tp = tilePartsRead[t];
						if (isPsotEqualsZero)
						{
							// Psot may equals zero for the
							// last tile-part: it is assumed that this tile-part
							// contain all data until EOC
							tilePartLen[t][tp] = in_Renamed.length() - 2 - tilePartStart;
						}
					}
					catch (System.IO.EndOfStreamException e)
					{
						firstPackOff[t][tp] = in_Renamed.length();
						throw e;
					}
					
					pos = in_Renamed.Pos;
					
					// In truncation mode, if target decoding rate is reached in
					// tile-part header, skips the tile-part and stop reading
					// unless the ncb and lbody quit condition is in use
					if (isTruncMode && ncbQuit == - 1)
					{
						if ((pos - cdstreamStart) > tnbytes)
						{
							firstPackOff[t][tp] = in_Renamed.length();
							rateReached = true;
							break;
						}
					}
					
					// Set tile part position and header length
					firstPackOff[t][tp] = pos;
					tilePartHeadLen[t][tp] = (pos - tilePartStart);
					
					strInfo += ("Tile-part " + tp + " of tile " + t + " : " + tilePartStart + ", " + tilePartLen[t][tp] + ", " + tilePartHeadLen[t][tp] + "\n");
					
					// Update length counters
					totTileLen[t] += tilePartLen[t][tp];
					totTileHeadLen[t] += tilePartHeadLen[t][tp];
					totAllTileLen += tilePartLen[t][tp];
					if (isTruncMode)
					{
						if (anbytes + tilePartLen[t][tp] > tnbytes)
						{
							anbytes += tilePartHeadLen[t][tp];
							headLen += tilePartHeadLen[t][tp];
							rateReached = true;
							nBytes[t] += (tnbytes - anbytes);
							break;
						}
						else
						{
							anbytes += tilePartHeadLen[t][tp];
							headLen += tilePartHeadLen[t][tp];
							nBytes[t] += (tilePartLen[t][tp] - tilePartHeadLen[t][tp]);
						}
					}
					else
					{
						if (anbytes + tilePartHeadLen[t][tp] > tnbytes)
						{
							break;
						}
						else
						{
							anbytes += tilePartHeadLen[t][tp];
							headLen += tilePartHeadLen[t][tp];
						}
					}
					
					// If this is first tile-part, remember header length
					if (tptot == 0)
						firstTilePartHeadLen = tilePartHeadLen[t][tp];
					
					// Go to the beginning of next tile part
					tilePartsRead[t]++;
					in_Renamed.seek(tilePartStart + tilePartLen[t][tp]);
					remainingTileParts--;
					maxTP--;
					tptot++;
					
					// If Psot of the current tile-part was equal to zero, it is
					// assumed that it contains all data until the EOC marker
					if (isPsotEqualsZero)
					{
						if (remainingTileParts != 0)
						{
							FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.WARNING, "Some tile-parts have not " + "been found. The codestream may be corrupted.");
						}
						break;
					}
				}
			}
			catch (System.IO.EndOfStreamException)
			{
				if (printInfo)
				{
					FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.INFO, strInfo);
				}
				FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.WARNING, "Codestream truncated in tile " + t);
				
				// Set specified rate to end of file if valid
				int fileLen = in_Renamed.length();
				if (fileLen < tnbytes)
				{
					tnbytes = fileLen;
					trate = tnbytes * 8f / hd.MaxCompImgWidth / hd.MaxCompImgHeight;
				}
				
				// Bit-rate allocation
				if (!isTruncMode)
				{
					allocateRate();
				}
				
				// Update 'res' value once all tile-part headers are read
				if (pl.getParameter("res") == null)
				{
					targetRes = decSpec.dls.Min;
				}
				else
				{
					try
					{
						targetRes = pl.getIntParameter("res");
						if (targetRes < 0)
						{
							throw new System.ArgumentException("Specified negative " + "resolution level " + "index: " + targetRes);
						}
					}
					catch (System.FormatException)
					{
						throw new System.ArgumentException("Invalid resolution level " + "index ('-res' option) " + pl.getParameter("res"));
					}
				}
				
				// Verify reduction in resolution level
				mdl = decSpec.dls.Min;
				if (targetRes > mdl)
				{
					FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.WARNING, "Specified resolution level (" + targetRes + ") is larger" + " than the maximum value. Setting it to " + mdl + " (maximum value)");
					targetRes = mdl;
				}
				
				// Backup nBytes
				for (int tIdx = 0; tIdx < nt; tIdx++)
				{
					baknBytes[tIdx] = nBytes[tIdx];
				}
				
				return ;
			}
			remainingTileParts = 0;
			
			// Update 'res' value once all tile-part headers are read
			if (pl.getParameter("res") == null)
			{
				targetRes = decSpec.dls.Min;
			}
			else
			{
				try
				{
					targetRes = pl.getIntParameter("res");
					if (targetRes < 0)
					{
						throw new System.ArgumentException("Specified negative " + "resolution level index: " + targetRes);
					}
				}
				catch (System.FormatException)
				{
					throw new System.ArgumentException("Invalid resolution level " + "index ('-res' option) " + pl.getParameter("res"));
				}
			}
			
			// Verify reduction in resolution level
			mdl = decSpec.dls.Min;
			if (targetRes > mdl)
			{
				FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.WARNING, "Specified resolution level (" + targetRes + ") is larger" + " than the maximum possible. Setting it to " + mdl + " (maximum possible)");
				targetRes = mdl;
			}
			
			if (printInfo)
			{
				FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.INFO, strInfo);
			}
			
			// Check presence of EOC marker is decoding rate not reached or if
			// this marker has not been found yet
			if (!isEOCFound && !isPsotEqualsZero)
			{
				try
				{
					if (!rateReached && !isPsotEqualsZero && in_Renamed.readShort() != CSJ2K.j2k.codestream.Markers.EOC)
					{
						FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.WARNING, "EOC marker not found. " + "Codestream is corrupted.");
					}
				}
				catch (System.IO.EndOfStreamException)
				{
					FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.WARNING, "EOC marker is missing");
				}
			}
			
			// Bit-rate allocation
			if (!isTruncMode)
			{
				allocateRate();
			}
			else
			{
				// Take EOC into account if rate is not reached
				if (in_Renamed.Pos >= tnbytes)
					anbytes += 2;
			}
			
			// Backup nBytes
			for (int tIdx = 0; tIdx < nt; tIdx++)
			{
				baknBytes[tIdx] = nBytes[tIdx];
				if (printInfo)
				{
					FacilityManager.getMsgLogger().println("" + hi.toStringTileHeader(tIdx, tilePartLen[tIdx].Length), 2, 2);
				}
			}
		}
		
		/// <summary> Allocates output bit-rate for each tile in parsing mode: The allocator
		/// simulates the truncation of a virtual layer-resolution progressive
		/// codestream.
		/// 
		/// </summary>
		private void  allocateRate()
		{
			int stopOff = tnbytes;
			
			// In parsing mode, the bitrate is allocated related to each tile's
			// length in the bit stream
			
			// EOC marker's length 
			anbytes += 2;
			
			// If there are too few bytes to read the tile part headers throw an
			// error
			if (anbytes > stopOff)
			{
				throw new System.ApplicationException("Requested bitrate is too small for parsing");
			}
			
			// Calculate bitrate for each tile
			int rem = stopOff - anbytes;
			int totnByte = rem;
			for (int t = nt - 1; t > 0; t--)
			{
				//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
				rem -= (nBytes[t] = (int) (totnByte * (totTileLen[t] / totAllTileLen)));
			}
			nBytes[0] = rem;
		}
		
		/// <summary> Reads SOT marker segment of the tile-part header and calls related
		/// methods of the HeaderDecoder to read other markers segments. The
		/// tile-part header is entirely read when a SOD marker is encountered.
		/// 
		/// </summary>
		/// <returns> The tile number of the tile part that was read
		/// 
		/// </returns>
		private int readTilePartHeader()
		{
			HeaderInfo.SOT ms = hi.NewSOT;
			
			// SOT marker
			short marker = in_Renamed.readShort();
			if (marker != CSJ2K.j2k.codestream.Markers.SOT)
			{
				if (marker == CSJ2K.j2k.codestream.Markers.EOC)
				{
					isEOCFound = true;
					return - 1;
				}
				else
				{
					throw new CorruptedCodestreamException("SOT tag not found " + "in tile-part start");
				}
			}
			isEOCFound = false;
			
			// Lsot (shall equals 10)
			int lsot = in_Renamed.readUnsignedShort();
			ms.lsot = lsot;
			if (lsot != 10)
				throw new CorruptedCodestreamException("Wrong length for " + "SOT marker segment: " + lsot);
			
			// Isot
			int tile = in_Renamed.readUnsignedShort();
			ms.isot = tile;
			if (tile > 65534)
			{
				throw new CorruptedCodestreamException("Tile index too high in " + "tile-part.");
			}
			
			// Psot
			int psot = in_Renamed.readInt();
			ms.psot = psot;
			isPsotEqualsZero = (psot != 0)?false:true;
			if (psot < 0)
			{
				throw new NotImplementedError("Tile length larger " + "than maximum supported");
			}
			// TPsot
			int tilePart = in_Renamed.read();
			ms.tpsot = tilePart;
			if (tilePart != tilePartsRead[tile] || tilePart < 0 || tilePart > 254)
			{
				throw new CorruptedCodestreamException("Out of order tile-part");
			}
			// TNsot
			int nrOfTileParts = in_Renamed.read();
			ms.tnsot = nrOfTileParts;
			hi.sotValue["t" + tile + "_tp" + tilePart] = ms;
			if (nrOfTileParts == 0)
			{
				// The number of tile-part is not specified in
				// this tile-part header.
				
				// Assumes that there will be another tile-part in the codestream
				// that will indicate the number of tile-parts for this tile)
				int nExtraTp;
				if (tileParts[tile] == 0 || tileParts[tile] == tilePartLen.Length)
				{
					// Then there are two tile-parts (one is the current and the
					// other will indicate the number of tile-part for this tile)
					nExtraTp = 2;
					remainingTileParts += 1;
				}
				else
				{
					// There is already one scheduled extra tile-part. In this
					// case just add place for the current one
					nExtraTp = 1;
				}
				
				tileParts[tile] += nExtraTp;
				nrOfTileParts = tileParts[tile];
				FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.WARNING, "Header of tile-part " + tilePart + " of tile " + tile + ", does not indicate the total" + " number of tile-parts. Assuming that there are " + nrOfTileParts + " tile-parts for this tile.");
				
				// Increase and re-copy tilePartLen array
				int[] tmpA = tilePartLen[tile];
				tilePartLen[tile] = new int[nrOfTileParts];
				for (int i = 0; i < nrOfTileParts - nExtraTp; i++)
				{
					tilePartLen[tile][i] = tmpA[i];
				}
				
				// Increase and re-copy tilePartNum array
				tmpA = tilePartNum[tile];
				tilePartNum[tile] = new int[nrOfTileParts];
				for (int i = 0; i < nrOfTileParts - nExtraTp; i++)
				{
					tilePartNum[tile][i] = tmpA[i];
				}
				
				// Increase and re-copy firsPackOff array
				tmpA = firstPackOff[tile];
				firstPackOff[tile] = new int[nrOfTileParts];
				for (int i = 0; i < nrOfTileParts - nExtraTp; i++)
				{
					firstPackOff[tile][i] = tmpA[i];
				}
				
				// Increase and re-copy tilePartHeadLen array
				tmpA = tilePartHeadLen[tile];
				tilePartHeadLen[tile] = new int[nrOfTileParts];
				for (int i = 0; i < nrOfTileParts - nExtraTp; i++)
				{
					tilePartHeadLen[tile][i] = tmpA[i];
				}
			}
			else
			{
				// The number of tile-parts is specified in the tile-part
				// header
				
				// Check if it is consistant with what was found in previous
				// tile-part headers
				
				if (tileParts[tile] == 0)
				{
					// First tile-part: OK
					remainingTileParts += nrOfTileParts - 1;
					tileParts[tile] = nrOfTileParts;
					tilePartLen[tile] = new int[nrOfTileParts];
					tilePartNum[tile] = new int[nrOfTileParts];
					firstPackOff[tile] = new int[nrOfTileParts];
					tilePartHeadLen[tile] = new int[nrOfTileParts];
				}
				else if (tileParts[tile] > nrOfTileParts)
				{
					// Already found more tile-parts than signaled here
					throw new CorruptedCodestreamException("Invalid number " + "of tile-parts in" + " tile " + tile + ": " + nrOfTileParts);
				}
				else
				{
					// Signaled number of tile-part fits with number of
					// previously found tile-parts
					remainingTileParts += nrOfTileParts - tileParts[tile];
					
					if (tileParts[tile] != nrOfTileParts)
					{
						
						// Increase and re-copy tilePartLen array
						int[] tmpA = tilePartLen[tile];
						tilePartLen[tile] = new int[nrOfTileParts];
						for (int i = 0; i < tileParts[tile] - 1; i++)
						{
							tilePartLen[tile][i] = tmpA[i];
						}
						
						// Increase and re-copy tilePartNum array                
						tmpA = tilePartNum[tile];
						tilePartNum[tile] = new int[nrOfTileParts];
						for (int i = 0; i < tileParts[tile] - 1; i++)
						{
							tilePartNum[tile][i] = tmpA[i];
						}
						
						// Increase and re-copy firstPackOff array
						tmpA = firstPackOff[tile];
						firstPackOff[tile] = new int[nrOfTileParts];
						for (int i = 0; i < tileParts[tile] - 1; i++)
						{
							firstPackOff[tile][i] = tmpA[i];
						}
						
						// Increase and re-copy tilePartHeadLen array
						tmpA = tilePartHeadLen[tile];
						tilePartHeadLen[tile] = new int[nrOfTileParts];
						for (int i = 0; i < tileParts[tile] - 1; i++)
						{
							tilePartHeadLen[tile][i] = tmpA[i];
						}
					}
				}
			}
			
			// Other markers
			hd.resetHeaderMarkers();
			hd.nTileParts[tile] = nrOfTileParts;
			// Decode and store the tile-part header (i.e. until a SOD marker is
			// found)
			do 
			{
				hd.extractTilePartMarkSeg(in_Renamed.readShort(), in_Renamed, tile, tilePart);
			}
			while ((hd.NumFoundMarkSeg & CSJ2K.j2k.codestream.reader.HeaderDecoder.SOD_FOUND) == 0);
			
			// Read each marker segment previously found
			hd.readFoundTilePartMarkSeg(tile, tilePart);
			
			tilePartLen[tile][tilePart] = psot;
			
			tilePartNum[tile][tilePart] = totTilePartsRead;
			totTilePartsRead++;
			
			// Add to list of which tile each successive tile-part belongs.
			// This list is needed if there are PPM markers used
			hd.TileOfTileParts = tile;
			
			return tile;
		}
		
		/// <summary> Reads packets of the current tile according to the
		/// layer-resolution-component-position progressiveness.
		/// 
		/// </summary>
		/// <param name="lys">Index of the first layer for each component and resolution.
		/// 
		/// </param>
		/// <param name="lye">Index of the last layer.
		/// 
		/// </param>
		/// <param name="ress">Index of the first resolution level.
		/// 
		/// </param>
		/// <param name="rese">Index of the last resolution level.
		/// 
		/// </param>
		/// <param name="comps">Index of the first component.
		/// 
		/// </param>
		/// <param name="compe">Index of the last component.
		/// 
		/// </param>
		/// <returns> True if rate has been reached.
		/// 
		/// </returns>
		private bool readLyResCompPos(int[][] lys, int lye, int ress, int rese, int comps, int compe)
		{
			
			int minlys = 10000;
			for (int c = comps; c < compe; c++)
			{
				//loop on components
				// Check if this component exists
				if (c >= mdl.Length)
					continue;
				
				for (int r = ress; r < rese; r++)
				{
					//loop on resolution levels
					if (lys[c] != null && r < lys[c].Length && lys[c][r] < minlys)
					{
						minlys = lys[c][r];
					}
				}
			}
			
			int t = TileIdx;
			int start;
			bool status = false;
			int lastByte = firstPackOff[t][curTilePart] + tilePartLen[t][curTilePart] - 1 - tilePartHeadLen[t][curTilePart];
			int numLayers = ((System.Int32) decSpec.nls.getTileDef(t));
			int nPrec = 1;
			int hlen, plen;
			System.String strInfo = "Tile " + TileIdx + " (tile-part:" + curTilePart + "): offset, length, header length\n"; ;
			bool pph = false;
			if (((System.Boolean) decSpec.pphs.getTileDef(t)))
			{
				pph = true;
			}
			for (int l = minlys; l < lye; l++)
			{
                // store the layer starting position
                layerStarts.Add(in_Renamed.Pos);

				// loop on layers
				for (int r = ress; r < rese; r++)
				{
					// loop on resolution levels
					for (int c = comps; c < compe; c++)
					{
						// loop on components
						// Checks if component exists
						if (c >= mdl.Length)
							continue;
						// Checks if resolution level exists
						if (r >= lys[c].Length)
							continue;
						if (r > mdl[c])
							continue;
						// Checks if layer exists
						if (l < lys[c][r] || l >= numLayers)
							continue;
						
						nPrec = pktDec.getNumPrecinct(c, r);
						for (int p = 0; p < nPrec; p++)
						{
							// loop on precincts
							start = in_Renamed.Pos;
							
							// If packed packet headers are used, there is no need
							// to check that there are bytes enough to read header
							if (pph)
							{
								pktDec.readPktHead(l, r, c, p, cbI[c][r], nBytes);
							}
							
							// If we are about to read outside of tile-part,
							// skip to next tile-part
							if (start > lastByte && curTilePart < firstPackOff[t].Length - 1)
							{
								curTilePart++;
								in_Renamed.seek(firstPackOff[t][curTilePart]);
								lastByte = in_Renamed.Pos + tilePartLen[t][curTilePart] - 1 - tilePartHeadLen[t][curTilePart];
							}
							
							// Read SOP marker segment if necessary
							status = pktDec.readSOPMarker(nBytes, p, c, r);
							
							if (status)
							{
								if (printInfo)
								{
									FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.INFO, strInfo);
								}
								return true;
							}
							
							if (!pph)
							{
								status = pktDec.readPktHead(l, r, c, p, cbI[c][r], nBytes);
							}
							
							if (status)
							{
								if (printInfo)
								{
									FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.INFO, strInfo);
								}
								return true;
							}
							
							// Store packet's head length
							hlen = in_Renamed.Pos - start;
							pktHL.Add((System.Int32) hlen);
							
							// Reads packet's body
							status = pktDec.readPktBody(l, r, c, p, cbI[c][r], nBytes);
							plen = in_Renamed.Pos - start;
							strInfo += (" Pkt l=" + l + ",r=" + r + ",c=" + c + ",p=" + p + ": " + start + ", " + plen + ", " + hlen + "\n");
							
							if (status)
							{
								if (printInfo)
								{
									FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.INFO, strInfo);
								}
								return true;
							}
						} // end loop on precincts
					} // end loop on components
				} // end loop on resolution levels
			} // end loop on layers
			
			if (printInfo)
			{
				FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.INFO, strInfo);
			}
			return false; // Decoding rate was not reached
		}
		
		/// <summary> Reads packets of the current tile according to the
		/// resolution-layer-component-position progressiveness.
		/// 
		/// </summary>
		/// <param name="lys">Index of the first layer for each component and resolution.
		/// 
		/// </param>
		/// <param name="lye">Index of the last layer.
		/// 
		/// </param>
		/// <param name="ress">Index of the first resolution level.
		/// 
		/// </param>
		/// <param name="rese">Index of the last resolution level.
		/// 
		/// </param>
		/// <param name="comps">Index of the first component.
		/// 
		/// </param>
		/// <param name="compe">Index of the last component.
		/// 
		/// </param>
		/// <returns> True if rate has been reached.
		/// 
		/// </returns>
		private bool readResLyCompPos(int[][] lys, int lye, int ress, int rese, int comps, int compe)
		{
			
			int t = TileIdx; // Current tile index
			bool status = false; // True if decoding rate is reached when
			int lastByte = firstPackOff[t][curTilePart] + tilePartLen[t][curTilePart] - 1 - tilePartHeadLen[t][curTilePart];
			int minlys = 10000;
			for (int c = comps; c < compe; c++)
			{
				//loop on components
				// Check if this component exists
				if (c >= mdl.Length)
					continue;
				
				for (int r = ress; r < rese; r++)
				{
					//loop on resolution levels
					if (r > mdl[c])
						continue;
					if (lys[c] != null && r < lys[c].Length && lys[c][r] < minlys)
					{
						minlys = lys[c][r];
					}
				}
			}
			
			System.String strInfo = "Tile " + TileIdx + " (tile-part:" + curTilePart + "): offset, length, header length\n"; ;
			int numLayers = ((System.Int32) decSpec.nls.getTileDef(t));
			bool pph = false;
			if (((System.Boolean) decSpec.pphs.getTileDef(t)))
			{
				pph = true;
			}
			int nPrec = 1;
			int start;
			int hlen, plen;
			for (int r = ress; r < rese; r++)
			{
				// loop on resolution levels
				for (int l = minlys; l < lye; l++)
				{
					// loop on layers
					for (int c = comps; c < compe; c++)
					{
						// loop on components
						// Checks if component exists
						if (c >= mdl.Length)
							continue;
						// Checks if resolution level exists
						if (r > mdl[c])
							continue;
						if (r >= lys[c].Length)
							continue;
						// Checks if layer exists
						if (l < lys[c][r] || l >= numLayers)
							continue;
						
						nPrec = pktDec.getNumPrecinct(c, r);
						
						for (int p = 0; p < nPrec; p++)
						{
							// loop on precincts
							start = in_Renamed.Pos;
							
							// If packed packet headers are used, there is no need
							// to check that there are bytes enough to read header
							if (pph)
							{
								pktDec.readPktHead(l, r, c, p, cbI[c][r], nBytes);
							}
							
							// If we are about to read outside of tile-part,
							// skip to next tile-part
							if (start > lastByte && curTilePart < firstPackOff[t].Length - 1)
							{
								curTilePart++;
								in_Renamed.seek(firstPackOff[t][curTilePart]);
								lastByte = in_Renamed.Pos + tilePartLen[t][curTilePart] - 1 - tilePartHeadLen[t][curTilePart];
							}
							
							// Read SOP marker segment if necessary
							status = pktDec.readSOPMarker(nBytes, p, c, r);
							
							if (status)
							{
								if (printInfo)
								{
									FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.INFO, strInfo);
								}
								return true;
							}
							
							if (!pph)
							{
								status = pktDec.readPktHead(l, r, c, p, cbI[c][r], nBytes);
							}
							
							if (status)
							{
								if (printInfo)
								{
									FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.INFO, strInfo);
								}
								// Output rate of EOF reached
								return true;
							}
							
							// Store packet's head length
							hlen = in_Renamed.Pos - start;
							pktHL.Add((System.Int32) hlen);
							
							// Reads packet's body
							status = pktDec.readPktBody(l, r, c, p, cbI[c][r], nBytes);
							plen = in_Renamed.Pos - start;
							strInfo += (" Pkt l=" + l + ",r=" + r + ",c=" + c + ",p=" + p + ": " + start + ", " + plen + ", " + hlen + "\n");
							
							if (status)
							{
								if (printInfo)
								{
									FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.INFO, strInfo);
								}
								// Output rate or EOF reached
								return true;
							}
						} // end loop on precincts
					} // end loop on components
				} // end loop on layers
			} // end loop on resolution levels
			
			if (printInfo)
			{
				FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.INFO, strInfo);
			}
			return false; // Decoding rate was not reached
		}
		
		/// <summary> Reads packets of the current tile according to the
		/// resolution-position-component-layer progressiveness.
		/// 
		/// </summary>
		/// <param name="lys">Index of the first layer for each component and resolution.
		/// 
		/// </param>
		/// <param name="lye">Index of the last layer.
		/// 
		/// </param>
		/// <param name="ress">Index of the first resolution level.
		/// 
		/// </param>
		/// <param name="rese">Index of the last resolution level.
		/// 
		/// </param>
		/// <param name="comps">Index of the first component.
		/// 
		/// </param>
		/// <param name="compe">Index of the last component.
		/// 
		/// </param>
		/// <returns> True if rate has been reached.
		/// 
		/// </returns>
		private bool readResPosCompLy(int[][] lys, int lye, int ress, int rese, int comps, int compe)
		{
			// Computes current tile offset in the reference grid
			
			Coord nTiles = getNumTiles(null);
			Coord tileI = getTile(null);
			int x0siz = hd.ImgULX;
			int y0siz = hd.ImgULY;
			int xsiz = x0siz + hd.ImgWidth;
			int ysiz = y0siz + hd.ImgHeight;
			int xt0siz = TilePartULX;
			int yt0siz = TilePartULY;
			int xtsiz = NomTileWidth;
			int ytsiz = NomTileHeight;
			int tx0 = (tileI.x == 0)?x0siz:xt0siz + tileI.x * xtsiz;
			int ty0 = (tileI.y == 0)?y0siz:yt0siz + tileI.y * ytsiz;
			int tx1 = (tileI.x != nTiles.x - 1)?xt0siz + (tileI.x + 1) * xtsiz:xsiz;
			int ty1 = (tileI.y != nTiles.y - 1)?yt0siz + (tileI.y + 1) * ytsiz:ysiz;
			
			// Get precinct information (number,distance between two consecutive
			// precincts in the reference grid) in each component and resolution
			// level
			int t = TileIdx; // Current tile index
			PrecInfo prec; // temporary variable
			int p; // Current precinct index
			int gcd_x = 0; // Horiz. distance between 2 precincts in the ref. grid
			int gcd_y = 0; // Vert. distance between 2 precincts in the ref. grid
			int nPrec = 0; // Total number of found precincts
			int[][] nextPrec = new int[compe][]; // Next precinct index in each
			// component and resolution level
			int minlys = 100000; // minimum layer start index of each component
			int minx = tx1; // Horiz. offset of the second precinct in the
			// reference grid
			int miny = ty1; // Vert. offset of the second precinct in the
			// reference grid. 
			int maxx = tx0; // Max. horiz. offset of precincts in the ref. grid
			int maxy = ty0; // Max. vert. offset of precincts in the ref. grid
			//Coord numPrec;
			for (int c = comps; c < compe; c++)
			{
				// components
				for (int r = ress; r < rese; r++)
				{
					// resolution levels
					if (c >= mdl.Length)
						continue;
					if (r > mdl[c])
						continue;
					nextPrec[c] = new int[mdl[c] + 1];
					if (lys[c] != null && r < lys[c].Length && lys[c][r] < minlys)
					{
						minlys = lys[c][r];
					}
					p = pktDec.getNumPrecinct(c, r) - 1;
					for (; p >= 0; p--)
					{
						prec = pktDec.getPrecInfo(c, r, p);
						if (prec.rgulx != tx0)
						{
							if (prec.rgulx < minx)
								minx = prec.rgulx;
							if (prec.rgulx > maxx)
								maxx = prec.rgulx;
						}
						if (prec.rguly != ty0)
						{
							if (prec.rguly < miny)
								miny = prec.rguly;
							if (prec.rguly > maxy)
								maxy = prec.rguly;
						}
						
						if (nPrec == 0)
						{
							gcd_x = prec.rgw;
							gcd_y = prec.rgh;
						}
						else
						{
							gcd_x = MathUtil.gcd(gcd_x, prec.rgw);
							gcd_y = MathUtil.gcd(gcd_y, prec.rgh);
						}
						nPrec++;
					} // precincts
				} // resolution levels
			} // components
			
			if (nPrec == 0)
			{
				throw new System.ApplicationException("Image cannot have no precinct");
			}
			
			int pyend = (maxy - miny) / gcd_y + 1;
			int pxend = (maxx - minx) / gcd_x + 1;
			int x, y;
			int hlen, plen;
			int start;
			bool status = false;
			int lastByte = firstPackOff[t][curTilePart] + tilePartLen[t][curTilePart] - 1 - tilePartHeadLen[t][curTilePart];
			int numLayers = ((System.Int32) decSpec.nls.getTileDef(t));
			System.String strInfo = "Tile " + TileIdx + " (tile-part:" + curTilePart + "): offset, length, header length\n"; ;
			bool pph = false;
			if (((System.Boolean) decSpec.pphs.getTileDef(t)))
			{
				pph = true;
			}
			for (int r = ress; r < rese; r++)
			{
				// loop on resolution levels
				y = ty0;
				x = tx0;
				for (int py = 0; py <= pyend; py++)
				{
					// Vertical precincts
					for (int px = 0; px <= pxend; px++)
					{
						// Horiz. precincts
						for (int c = comps; c < compe; c++)
						{
							// Components
							if (c >= mdl.Length)
								continue;
							if (r > mdl[c])
								continue;
							if (nextPrec[c][r] >= pktDec.getNumPrecinct(c, r))
							{
								continue;
							}
							prec = pktDec.getPrecInfo(c, r, nextPrec[c][r]);
							if ((prec.rgulx != x) || (prec.rguly != y))
							{
								continue;
							}
							for (int l = minlys; l < lye; l++)
							{
								// layers
								if (r >= lys[c].Length)
									continue;
								if (l < lys[c][r] || l >= numLayers)
									continue;
								
								start = in_Renamed.Pos;
								
								// If packed packet headers are used, there is no
								// need to check that there are bytes enough to
								// read header
								if (pph)
								{
									pktDec.readPktHead(l, r, c, nextPrec[c][r], cbI[c][r], nBytes);
								}
								
								// If we are about to read outside of tile-part,
								// skip to next tile-part
								if (start > lastByte && curTilePart < firstPackOff[t].Length - 1)
								{
									curTilePart++;
									in_Renamed.seek(firstPackOff[t][curTilePart]);
									lastByte = in_Renamed.Pos + tilePartLen[t][curTilePart] - 1 - tilePartHeadLen[t][curTilePart];
								}
								
								// Read SOP marker segment if necessary
								status = pktDec.readSOPMarker(nBytes, nextPrec[c][r], c, r);
								
								if (status)
								{
									if (printInfo)
									{
										FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.INFO, strInfo);
									}
									return true;
								}
								
								if (!pph)
								{
									status = pktDec.readPktHead(l, r, c, nextPrec[c][r], cbI[c][r], nBytes);
								}
								
								if (status)
								{
									if (printInfo)
									{
										FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.INFO, strInfo);
									}
									return true;
								}
								
								// Store packet's head length
								hlen = in_Renamed.Pos - start;
								pktHL.Add((System.Int32) hlen);
								
								// Reads packet's body
								status = pktDec.readPktBody(l, r, c, nextPrec[c][r], cbI[c][r], nBytes);
								plen = in_Renamed.Pos - start;
								strInfo += (" Pkt l=" + l + ",r=" + r + ",c=" + c + ",p=" + nextPrec[c][r] + ": " + start + ", " + plen + ", " + hlen + "\n");
								
								if (status)
								{
									if (printInfo)
									{
										FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.INFO, strInfo);
									}
									return true;
								}
							} // layers
							nextPrec[c][r]++;
						} // Components
						if (px != pxend)
						{
							x = minx + px * gcd_x;
						}
						else
						{
							x = tx0;
						}
					} // Horizontal precincts
					if (py != pyend)
					{
						y = miny + py * gcd_y;
					}
					else
					{
						y = ty0;
					}
				} // Vertical precincts
			} // end loop on resolution levels
			
			if (printInfo)
			{
				FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.INFO, strInfo);
			}
			return false; // Decoding rate was not reached
		}
		
		/// <summary> Reads packets of the current tile according to the
		/// position-component-resolution-layer progressiveness.
		/// 
		/// </summary>
		/// <param name="lys">Index of the first layer for each component and resolution.
		/// 
		/// </param>
		/// <param name="lye">Index of the last layer.
		/// 
		/// </param>
		/// <param name="ress">Index of the first resolution level.
		/// 
		/// </param>
		/// <param name="rese">Index of the last resolution level.
		/// 
		/// </param>
		/// <param name="comps">Index of the first component.
		/// 
		/// </param>
		/// <param name="compe">Index of the last component.
		/// 
		/// </param>
		/// <returns> True if rate has been reached.
		/// 
		/// </returns>
		private bool readPosCompResLy(int[][] lys, int lye, int ress, int rese, int comps, int compe)
		{
			Coord nTiles = getNumTiles(null);
			Coord tileI = getTile(null);
			int x0siz = hd.ImgULX;
			int y0siz = hd.ImgULY;
			int xsiz = x0siz + hd.ImgWidth;
			int ysiz = y0siz + hd.ImgHeight;
			int xt0siz = TilePartULX;
			int yt0siz = TilePartULY;
			int xtsiz = NomTileWidth;
			int ytsiz = NomTileHeight;
			int tx0 = (tileI.x == 0)?x0siz:xt0siz + tileI.x * xtsiz;
			int ty0 = (tileI.y == 0)?y0siz:yt0siz + tileI.y * ytsiz;
			int tx1 = (tileI.x != nTiles.x - 1)?xt0siz + (tileI.x + 1) * xtsiz:xsiz;
			int ty1 = (tileI.y != nTiles.y - 1)?yt0siz + (tileI.y + 1) * ytsiz:ysiz;
			
			// Get precinct information (number,distance between two consecutive
			// precincts in the reference grid) in each component and resolution
			// level
			int t = TileIdx; // Current tile index
			PrecInfo prec; // temporary variable
			int p; // Current precinct index
			int gcd_x = 0; // Horiz. distance between 2 precincts in the ref. grid
			int gcd_y = 0; // Vert. distance between 2 precincts in the ref. grid
			int nPrec = 0; // Total number of found precincts
			int[][] nextPrec = new int[compe][]; // Next precinct index in each
			// component and resolution level
			int minlys = 100000; // minimum layer start index of each component
			int minx = tx1; // Horiz. offset of the second precinct in the
			// reference grid
			int miny = ty1; // Vert. offset of the second precinct in the
			// reference grid. 
			int maxx = tx0; // Max. horiz. offset of precincts in the ref. grid
			int maxy = ty0; // Max. vert. offset of precincts in the ref. grid
			//Coord numPrec;
			for (int c = comps; c < compe; c++)
			{
				// components
				for (int r = ress; r < rese; r++)
				{
					// resolution levels
					if (c >= mdl.Length)
						continue;
					if (r > mdl[c])
						continue;
					nextPrec[c] = new int[mdl[c] + 1];
					if (lys[c] != null && r < lys[c].Length && lys[c][r] < minlys)
					{
						minlys = lys[c][r];
					}
					p = pktDec.getNumPrecinct(c, r) - 1;
					for (; p >= 0; p--)
					{
						prec = pktDec.getPrecInfo(c, r, p);
						if (prec.rgulx != tx0)
						{
							if (prec.rgulx < minx)
								minx = prec.rgulx;
							if (prec.rgulx > maxx)
								maxx = prec.rgulx;
						}
						if (prec.rguly != ty0)
						{
							if (prec.rguly < miny)
								miny = prec.rguly;
							if (prec.rguly > maxy)
								maxy = prec.rguly;
						}
						
						if (nPrec == 0)
						{
							gcd_x = prec.rgw;
							gcd_y = prec.rgh;
						}
						else
						{
							gcd_x = MathUtil.gcd(gcd_x, prec.rgw);
							gcd_y = MathUtil.gcd(gcd_y, prec.rgh);
						}
						nPrec++;
					} // precincts
				} // resolution levels
			} // components
			
			if (nPrec == 0)
			{
				throw new System.ApplicationException("Image cannot have no precinct");
			}
			
			int pyend = (maxy - miny) / gcd_y + 1;
			int pxend = (maxx - minx) / gcd_x + 1;
			int hlen, plen;
			int start;
			bool status = false;
			int lastByte = firstPackOff[t][curTilePart] + tilePartLen[t][curTilePart] - 1 - tilePartHeadLen[t][curTilePart];
			int numLayers = ((System.Int32) decSpec.nls.getTileDef(t));
			System.String strInfo = "Tile " + TileIdx + " (tile-part:" + curTilePart + "): offset, length, header length\n"; ;
			bool pph = false;
			if (((System.Boolean) decSpec.pphs.getTileDef(t)))
			{
				pph = true;
			}
			
			int y = ty0;
			int x = tx0;
			for (int py = 0; py <= pyend; py++)
			{
				// Vertical precincts
				for (int px = 0; px <= pxend; px++)
				{
					// Horiz. precincts
					for (int c = comps; c < compe; c++)
					{
						// Components
						if (c >= mdl.Length)
							continue;
						for (int r = ress; r < rese; r++)
						{
							// Resolution levels
							if (r > mdl[c])
								continue;
							if (nextPrec[c][r] >= pktDec.getNumPrecinct(c, r))
							{
								continue;
							}
							prec = pktDec.getPrecInfo(c, r, nextPrec[c][r]);
							if ((prec.rgulx != x) || (prec.rguly != y))
							{
								continue;
							}
							for (int l = minlys; l < lye; l++)
							{
								// Layers
								if (r >= lys[c].Length)
									continue;
								if (l < lys[c][r] || l >= numLayers)
									continue;
								
								start = in_Renamed.Pos;
								
								// If packed packet headers are used, there is no
								// need to check that there are bytes enough to
								// read header
								if (pph)
								{
									pktDec.readPktHead(l, r, c, nextPrec[c][r], cbI[c][r], nBytes);
								}
								
								// If we are about to read outside of tile-part,
								// skip to next tile-part
								if (start > lastByte && curTilePart < firstPackOff[t].Length - 1)
								{
									curTilePart++;
									in_Renamed.seek(firstPackOff[t][curTilePart]);
									lastByte = in_Renamed.Pos + tilePartLen[t][curTilePart] - 1 - tilePartHeadLen[t][curTilePart];
								}
								
								// Read SOP marker segment if necessary
								status = pktDec.readSOPMarker(nBytes, nextPrec[c][r], c, r);
								
								if (status)
								{
									if (printInfo)
									{
										FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.INFO, strInfo);
									}
									return true;
								}
								
								if (!pph)
								{
									status = pktDec.readPktHead(l, r, c, nextPrec[c][r], cbI[c][r], nBytes);
								}
								
								if (status)
								{
									if (printInfo)
									{
										FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.INFO, strInfo);
									}
									return true;
								}
								
								// Store packet's head length
								hlen = in_Renamed.Pos - start;
								pktHL.Add((System.Int32) hlen);
								
								// Reads packet's body
								status = pktDec.readPktBody(l, r, c, nextPrec[c][r], cbI[c][r], nBytes);
								plen = in_Renamed.Pos - start;
								strInfo += (" Pkt l=" + l + ",r=" + r + ",c=" + c + ",p=" + nextPrec[c][r] + ": " + start + ", " + plen + ", " + hlen + "\n");
								
								if (status)
								{
									if (printInfo)
									{
										FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.INFO, strInfo);
									}
									return true;
								}
							} // layers
							nextPrec[c][r]++;
						} // Resolution levels
					} // Components
					if (px != pxend)
					{
						x = minx + px * gcd_x;
					}
					else
					{
						x = tx0;
					}
				} // Horizontal precincts
				if (py != pyend)
				{
					y = miny + py * gcd_y;
				}
				else
				{
					y = ty0;
				}
			} // Vertical precincts        
			
			if (printInfo)
			{
				FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.INFO, strInfo);
			}
			return false; // Decoding rate was not reached
		}
		
		/// <summary> Reads packets of the current tile according to the
		/// component-position-resolution-layer progressiveness.
		/// 
		/// </summary>
		/// <param name="lys">Index of the first layer for each component and resolution.
		/// 
		/// </param>
		/// <param name="lye">Index of the last layer.
		/// 
		/// </param>
		/// <param name="ress">Index of the first resolution level.
		/// 
		/// </param>
		/// <param name="rese">Index of the last resolution level.
		/// 
		/// </param>
		/// <param name="comps">Index of the first component.
		/// 
		/// </param>
		/// <param name="compe">Index of the last component.
		/// 
		/// </param>
		/// <returns> True if rate has been reached.
		/// 
		/// </returns>
		private bool readCompPosResLy(int[][] lys, int lye, int ress, int rese, int comps, int compe)
		{
			Coord nTiles = getNumTiles(null);
			Coord tileI = getTile(null);
			int x0siz = hd.ImgULX;
			int y0siz = hd.ImgULY;
			int xsiz = x0siz + hd.ImgWidth;
			int ysiz = y0siz + hd.ImgHeight;
			int xt0siz = TilePartULX;
			int yt0siz = TilePartULY;
			int xtsiz = NomTileWidth;
			int ytsiz = NomTileHeight;
			int tx0 = (tileI.x == 0)?x0siz:xt0siz + tileI.x * xtsiz;
			int ty0 = (tileI.y == 0)?y0siz:yt0siz + tileI.y * ytsiz;
			int tx1 = (tileI.x != nTiles.x - 1)?xt0siz + (tileI.x + 1) * xtsiz:xsiz;
			int ty1 = (tileI.y != nTiles.y - 1)?yt0siz + (tileI.y + 1) * ytsiz:ysiz;
			
			// Get precinct information (number,distance between two consecutive
			// precincts in the reference grid) in each component and resolution
			// level
			int t = TileIdx; // Current tile index
			PrecInfo prec; // temporary variable
			int p; // Current precinct index
			int gcd_x = 0; // Horiz. distance between 2 precincts in the ref. grid
			int gcd_y = 0; // Vert. distance between 2 precincts in the ref. grid
			int nPrec = 0; // Total number of found precincts
			int[][] nextPrec = new int[compe][]; // Next precinct index in each
			// component and resolution level
			int minlys = 100000; // minimum layer start index of each component
			int minx = tx1; // Horiz. offset of the second precinct in the
			// reference grid
			int miny = ty1; // Vert. offset of the second precinct in the
			// reference grid. 
			int maxx = tx0; // Max. horiz. offset of precincts in the ref. grid
			int maxy = ty0; // Max. vert. offset of precincts in the ref. grid
			//Coord numPrec;
			for (int c = comps; c < compe; c++)
			{
				// components
				for (int r = ress; r < rese; r++)
				{
					// resolution levels
					if (c >= mdl.Length)
						continue;
					if (r > mdl[c])
						continue;
					nextPrec[c] = new int[mdl[c] + 1];
					if (lys[c] != null && r < lys[c].Length && lys[c][r] < minlys)
					{
						minlys = lys[c][r];
					}
					p = pktDec.getNumPrecinct(c, r) - 1;
					for (; p >= 0; p--)
					{
						prec = pktDec.getPrecInfo(c, r, p);
						if (prec.rgulx != tx0)
						{
							if (prec.rgulx < minx)
								minx = prec.rgulx;
							if (prec.rgulx > maxx)
								maxx = prec.rgulx;
						}
						if (prec.rguly != ty0)
						{
							if (prec.rguly < miny)
								miny = prec.rguly;
							if (prec.rguly > maxy)
								maxy = prec.rguly;
						}
						
						if (nPrec == 0)
						{
							gcd_x = prec.rgw;
							gcd_y = prec.rgh;
						}
						else
						{
							gcd_x = MathUtil.gcd(gcd_x, prec.rgw);
							gcd_y = MathUtil.gcd(gcd_y, prec.rgh);
						}
						nPrec++;
					} // precincts
				} // resolution levels
			} // components
			
			if (nPrec == 0)
			{
				throw new System.ApplicationException("Image cannot have no precinct");
			}
			
			int pyend = (maxy - miny) / gcd_y + 1;
			int pxend = (maxx - minx) / gcd_x + 1;
			int hlen, plen;
			int start;
			bool status = false;
			int lastByte = firstPackOff[t][curTilePart] + tilePartLen[t][curTilePart] - 1 - tilePartHeadLen[t][curTilePart];
			int numLayers = ((System.Int32) decSpec.nls.getTileDef(t));
			System.String strInfo = "Tile " + TileIdx + " (tile-part:" + curTilePart + "): offset, length, header length\n"; ;
			bool pph = false;
			if (((System.Boolean) decSpec.pphs.getTileDef(t)))
			{
				pph = true;
			}
			
			int x, y;
			for (int c = comps; c < compe; c++)
			{
				// components
				if (c >= mdl.Length)
					continue;
				y = ty0;
				x = tx0;
				for (int py = 0; py <= pyend; py++)
				{
					// Vertical precincts
					for (int px = 0; px <= pxend; px++)
					{
						// Horiz. precincts
						for (int r = ress; r < rese; r++)
						{
							// Resolution levels
							if (r > mdl[c])
								continue;
							if (nextPrec[c][r] >= pktDec.getNumPrecinct(c, r))
							{
								continue;
							}
							prec = pktDec.getPrecInfo(c, r, nextPrec[c][r]);
							if ((prec.rgulx != x) || (prec.rguly != y))
							{
								continue;
							}
							
							for (int l = minlys; l < lye; l++)
							{
								// Layers
								if (r >= lys[c].Length)
									continue;
								if (l < lys[c][r])
									continue;
								
								start = in_Renamed.Pos;
								
								// If packed packet headers are used, there is no
								// need to check that there are bytes enough to
								// read header
								if (pph)
								{
									pktDec.readPktHead(l, r, c, nextPrec[c][r], cbI[c][r], nBytes);
								}
								
								// If we are about to read outside of tile-part,
								// skip to next tile-part
								if (start > lastByte && curTilePart < firstPackOff[t].Length - 1)
								{
									curTilePart++;
									in_Renamed.seek(firstPackOff[t][curTilePart]);
									lastByte = in_Renamed.Pos + tilePartLen[t][curTilePart] - 1 - tilePartHeadLen[t][curTilePart];
								}
								
								// Read SOP marker segment if necessary
								status = pktDec.readSOPMarker(nBytes, nextPrec[c][r], c, r);
								
								if (status)
								{
									if (printInfo)
									{
										FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.INFO, strInfo);
									}
									return true;
								}
								
								if (!pph)
								{
									status = pktDec.readPktHead(l, r, c, nextPrec[c][r], cbI[c][r], nBytes);
								}
								
								if (status)
								{
									if (printInfo)
									{
										FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.INFO, strInfo);
									}
									return true;
								}
								
								// Store packet's head length
								hlen = in_Renamed.Pos - start;
								pktHL.Add((System.Int32) hlen);
								
								// Reads packet's body
								status = pktDec.readPktBody(l, r, c, nextPrec[c][r], cbI[c][r], nBytes);
								plen = in_Renamed.Pos - start;
								strInfo += (" Pkt l=" + l + ",r=" + r + ",c=" + c + ",p=" + nextPrec[c][r] + ": " + start + ", " + plen + ", " + hlen + "\n");
								
								if (status)
								{
									if (printInfo)
									{
										FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.INFO, strInfo);
									}
									return true;
								}
							} // layers                        
							nextPrec[c][r]++;
						} // Resolution levels
						if (px != pxend)
						{
							x = minx + px * gcd_x;
						}
						else
						{
							x = tx0;
						}
					} // Horizontal precincts
					if (py != pyend)
					{
						y = miny + py * gcd_y;
					}
					else
					{
						y = ty0;
					}
				} // Vertical precincts
			} // components
			
			if (printInfo)
			{
				FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.INFO, strInfo);
			}
			return false; // Decoding rate was not reached
		}
		
		/// <summary> Finish initialization of members for specified tile, reads packets head
		/// of each tile and keeps location of each code-block's codewords. The
		/// last 2 tasks are done by calling specific methods of PktDecoder.
		/// 
		/// <p>Then, if a parsing output rate is defined, it keeps information of
		/// first layers only. This operation simulates a creation of a
		/// layer-resolution-component progressive bit-stream which will be next
		/// truncated and decoded.</p>
		/// 
		/// </summary>
		/// <param name="t">Tile index
		/// 
		/// </param>
		/// <seealso cref="PktDecoder">
		/// 
		/// </seealso>
		private void  readTilePkts(int t)
		{
			pktHL = System.Collections.ArrayList.Synchronized(new System.Collections.ArrayList(10));
            layerStarts = new System.Collections.Generic.List<int>(5);
			
			// Number of layers
			int nl = ((System.Int32) decSpec.nls.getTileDef(t));
			
			// If packed packet headers was used, get the packet headers for this
			// tile
			if (((System.Boolean) decSpec.pphs.getTileDef(t)))
			{
				// Gets packed headers as separate input stream
				System.IO.MemoryStream pphbais = hd.getPackedPktHead(t);
				
				// Restarts PktDecoder instance
				cbI = pktDec.restart(this.nc, mdl, nl, cbI, true, pphbais);
			}
			else
			{
				// Restarts PktDecoder instance
				cbI = pktDec.restart(this.nc, mdl, nl, cbI, false, null);
			}
			
			// Reads packets of the tile according to the progression order
			int[][] pocSpec = ((int[][]) decSpec.pcs.getTileDef(t));
			int nChg = (pocSpec == null)?1:pocSpec.Length;
			
			// Create an array containing information about changes (progression
			// order type, layers index start, layer index end, resolution level
			// start, resolution level end, component index start, component index
			// end). There is one row per progresion order
			int[][] change = new int[nChg][];
			for (int i = 0; i < nChg; i++)
			{
				change[i] = new int[6];
			}
			int idx = 0; // Index of the current progression order
			
			change[0][1] = 0; // layer start
			
			if (pocSpec == null)
			{
				change[idx][0] = ((System.Int32) decSpec.pos.getTileDef(t));
				// Progression type found in COx marker segments
				change[idx][1] = nl; // Layer index end
				change[idx][2] = 0; // resolution level start
				change[idx][3] = decSpec.dls.getMaxInTile(t) + 1; // res. level end
				change[idx][4] = 0; // Component index start
				change[idx][5] = this.nc; // Component index end
			}
			else
			{
				for (idx = 0; idx < nChg; idx++)
				{
					change[idx][0] = pocSpec[idx][5];
					change[idx][1] = pocSpec[idx][2]; // layer end
					change[idx][2] = pocSpec[idx][0]; // res. lev. start
					change[idx][3] = pocSpec[idx][3]; // res. lev. end
					change[idx][4] = pocSpec[idx][1]; // Comp. index start
					change[idx][5] = pocSpec[idx][4]; // Comp. index end
				}
			}
			
			// Seeks to the first packet of the first tile-part
			try
			{
				// If in truncation mode, the first tile-part may be beyond the
				// target decoding rate. In this case, the offset of the first
				// packet is not defined.
				if (isTruncMode && firstPackOff == null || firstPackOff[t] == null)
				{
					return ;
				}
				in_Renamed.seek(firstPackOff[t][0]);
			}
			catch (System.IO.EndOfStreamException)
			{
				FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.WARNING, "Codestream truncated in tile " + t);
				return ;
			}
			
			curTilePart = 0;
			
			// Start and end indexes for layers, resolution levels and components.
			int lye, ress, rese, comps, compe;
			bool status = false;
			int nb = nBytes[t];
			int[][] lys = new int[this.nc][];
			for (int c = 0; c < this.nc; c++)
			{
				lys[c] = new int[((System.Int32) decSpec.dls.getTileCompVal(t, c)) + 1];
			}
			
			try
			{
				for (int chg = 0; chg < nChg; chg++)
				{
					
					lye = change[chg][1];
					ress = change[chg][2];
					rese = change[chg][3];
					comps = change[chg][4];
					compe = change[chg][5];
					
					switch (change[chg][0])
					{
						
						case CSJ2K.j2k.codestream.ProgressionType.LY_RES_COMP_POS_PROG: 
							status = readLyResCompPos(lys, lye, ress, rese, comps, compe);
							break;
						
						case CSJ2K.j2k.codestream.ProgressionType.RES_LY_COMP_POS_PROG: 
							status = readResLyCompPos(lys, lye, ress, rese, comps, compe);
							break;
						
						case CSJ2K.j2k.codestream.ProgressionType.RES_POS_COMP_LY_PROG: 
							status = readResPosCompLy(lys, lye, ress, rese, comps, compe);
							break;
						
						case CSJ2K.j2k.codestream.ProgressionType.POS_COMP_RES_LY_PROG: 
							status = readPosCompResLy(lys, lye, ress, rese, comps, compe);
							break;
						
						case CSJ2K.j2k.codestream.ProgressionType.COMP_POS_RES_LY_PROG: 
							status = readCompPosResLy(lys, lye, ress, rese, comps, compe);
							break;
						
						default: 
							throw new System.ArgumentException("Not recognized " + "progression type");
						
					}
					
					// Update next first layer index 
					for (int c = comps; c < compe; c++)
					{
						if (c >= lys.Length)
							continue;
						for (int r = ress; r < rese; r++)
						{
							if (r >= lys[c].Length)
								continue;
							lys[c][r] = lye;
						}
					}
					
					if (status || usePOCQuit)
					{
						break;
					}
				}
			}
			catch (System.IO.EndOfStreamException e)
			{
				// Should never happen. Truncated codestream are normally found by
				// the class constructor
				throw e;
			}
			
			// In truncation mode, update the number of read bytes
			if (isTruncMode)
			{
				anbytes += nb - nBytes[t];
				
				// If truncation rate is reached
				if (status)
				{
					nBytes[t] = 0;
				}
			}
			else if (nBytes[t] < (totTileLen[t] - totTileHeadLen[t]))
			{
				// In parsing mode, if there is not enough rate to entirely read the
				// tile. Then, parses the bit stream so as to create a virtual
				// layer-resolution-component progressive bit stream that will be
				// truncated and decoded afterwards.
				CBlkInfo cb;
				
				// Systematicaly reject all remaining code-blocks if one
				// code-block, at least, is refused.
				bool reject;
				// Stop reading any data from the bit stream
				bool stopCount = false;
				// Length of each packet's head (in an array)
				int[] pktHeadLen = new int[pktHL.Count];
				for (int i = pktHL.Count - 1; i >= 0; i--)
				{
					pktHeadLen[i] = ((System.Int32) pktHL[i]);
				}
				
				// Parse each code-block, layer per layer until nBytes[t] is
				// reached
				reject = false;
				for (int l = 0; l < nl; l++)
				{
					// layers
					if (cbI == null)
						continue;
					int nc = cbI.Length;
					
					int mres = 0;
					for (int c = 0; c < nc; c++)
					{
						if (cbI[c] != null && cbI[c].Length > mres)
							mres = cbI[c].Length;
					}
					for (int r = 0; r < mres; r++)
					{
						// resolutions
						
						int msub = 0;
						for (int c = 0; c < nc; c++)
						{
							if (cbI[c] != null && cbI[c][r] != null && cbI[c][r].Length > msub)
								msub = cbI[c][r].Length;
						}
						for (int s = 0; s < msub; s++)
						{
							// subbands
							// Only LL subband resolution level 0
							if (r == 0 && s != 0)
							{
								continue;
							}
							else if (r != 0 && s == 0)
							{
								// No LL subband in resolution level > 0
								continue;
							}
							
							int mnby = 0;
							for (int c = 0; c < nc; c++)
							{
								if (cbI[c] != null && cbI[c][r] != null && cbI[c][r][s] != null && cbI[c][r][s].Length > mnby)
									mnby = cbI[c][r][s].Length;
							}
							for (int m = 0; m < mnby; m++)
							{
								
								int mnbx = 0;
								for (int c = 0; c < nc; c++)
								{
									if (cbI[c] != null && cbI[c][r] != null && cbI[c][r][s] != null && cbI[c][r][s][m] != null && cbI[c][r][s][m].Length > mnbx)
										mnbx = cbI[c][r][s][m].Length;
								}
								for (int n = 0; n < mnbx; n++)
								{
									
									for (int c = 0; c < nc; c++)
									{
										
										if (cbI[c] == null || cbI[c][r] == null || cbI[c][r][s] == null || cbI[c][r][s][m] == null || cbI[c][r][s][m][n] == null)
										{
											continue;
										}
										cb = cbI[c][r][s][m][n];
										
										// If no code-block has been refused until
										// now
										if (!reject)
										{
											// Rate is to low to allow reading of
											// packet's head
											if (nBytes[t] < pktHeadLen[cb.pktIdx[l]])
											{
												// Stop parsing
												stopCount = true;
												// Reject all next
												// code-blocks
												reject = true;
											}
											else
											{
												// Rate is enough to read packet's
												// head 
												if (!stopCount)
												{
													//If parsing was not stopped
													//Takes into account packet's
													//head length
													nBytes[t] -= pktHeadLen[cb.pktIdx[l]];
													anbytes += pktHeadLen[cb.pktIdx[l]];
													// Set packet's head length to
													// 0, so that it won't be
													// taken into account next
													// time
													pktHeadLen[cb.pktIdx[l]] = 0;
												}
											}
										}
										// Code-block has no data in this layer
										if (cb.len[l] == 0)
										{
											continue;
										}
										
										// Accepts code-block if length is enough,
										// if this code-block was not refused in a
										// previous layer and if no code-block was
										// refused in current component
										if (cb.len[l] < nBytes[t] && !reject)
										{
											nBytes[t] -= cb.len[l];
											anbytes += cb.len[l];
										}
										else
										{
											// Refuses code-block
											// Forgets code-block's data
											cb.len[l] = cb.off[l] = cb.ntp[l] = 0;
											// Refuses all other code-block in
											// current and next component
											reject = true;
										}
									} // End loop on components
								} // End loop on horiz. code-blocks
							} // End loop on vert. code-blocks
						} // End loop on subbands
					} // End loop on resolutions
				} // End loop on layers
			}
			else
			{
				// No parsing for this tile, adds tile's body to the total
				// number of read bytes.
				anbytes += totTileLen[t] - totTileHeadLen[t];
				if (t < getNumTiles() - 1)
				{
					nBytes[t + 1] += nBytes[t] - (totTileLen[t] - totTileHeadLen[t]);
				}
			}
		}
		
		
		
		/// <summary> Changes the current tile, given the new indexes. An
		/// IllegalArgumentException is thrown if the indexes do not correspond to
		/// a valid tile.
		/// 
		/// </summary>
		/// <param name="x">The horizontal indexes the tile.
		/// 
		/// </param>
		/// <param name="y">The vertical indexes of the new tile.
		/// 
		/// </param>
		public override void  setTile(int x, int y)
		{
			
			int i; // counter
			// Check validity of tile indexes
			if (x < 0 || y < 0 || x >= ntX || y >= ntY)
			{
				throw new System.ArgumentException();
			}
			int t = (y * ntX + x);
			
			// Reset number of read bytes if needed
			if (t == 0)
			{
				anbytes = headLen;
				if (!isTruncMode)
				{
					anbytes += 2;
				}
				// Restore values of nBytes
				for (int tIdx = 0; tIdx < nt; tIdx++)
				{
					nBytes[tIdx] = baknBytes[tIdx];
				}
			}
			
			// Set the new current tile
			ctX = x;
			ctY = y;
			// Calculate tile relative points
			int ctox = (x == 0)?ax:px + x * ntW;
			int ctoy = (y == 0)?ay:py + y * ntH;
			for (i = nc - 1; i >= 0; i--)
			{
				culx[i] = (ctox + hd.getCompSubsX(i) - 1) / hd.getCompSubsX(i);
				culy[i] = (ctoy + hd.getCompSubsY(i) - 1) / hd.getCompSubsY(i);
				offX[i] = (px + x * ntW + hd.getCompSubsX(i) - 1) / hd.getCompSubsX(i);
				offY[i] = (py + y * ntH + hd.getCompSubsY(i) - 1) / hd.getCompSubsY(i);
			}
			
			// Initialize subband tree and number of resolution levels
			subbTrees = new SubbandSyn[nc];
			mdl = new int[nc];
			derived = new bool[nc];
			params_Renamed = new StdDequantizerParams[nc];
			gb = new int[nc];
			
			for (int c = 0; c < nc; c++)
			{
				derived[c] = decSpec.qts.isDerived(t, c);
				params_Renamed[c] = (StdDequantizerParams) decSpec.qsss.getTileCompVal(t, c);
				gb[c] = ((System.Int32) decSpec.gbs.getTileCompVal(t, c));
				mdl[c] = ((System.Int32) decSpec.dls.getTileCompVal(t, c));
				
				subbTrees[c] = new SubbandSyn(getTileCompWidth(t, c, mdl[c]), getTileCompHeight(t, c, mdl[c]), getResULX(c, mdl[c]), getResULY(c, mdl[c]), mdl[c], decSpec.wfs.getHFilters(t, c), decSpec.wfs.getVFilters(t, c));
				initSubbandsFields(c, subbTrees[c]);
			}
			
			// Read tile's packets
			try
			{
				readTilePkts(t);
			}
			catch (System.IO.IOException e)
			{
				SupportClass.WriteStackTrace(e, Console.Error);
				throw new System.ApplicationException("IO Error when reading tile " + x + " x " + y);
			}
		}
		
		/// <summary> Advances to the next tile, in standard scan-line order (by rows then
		/// columns). A NoNextElementException is thrown if the current tile is the
		/// last one (i.e. there is no next tile).
		/// 
		/// </summary>
		public override void  nextTile()
		{
			if (ctX == ntX - 1 && ctY == ntY - 1)
			{
				// Already at last tile
				throw new NoNextElementException();
			}
			else if (ctX < ntX - 1)
			{
				// If not at end of current tile line
				setTile(ctX + 1, ctY);
			}
			else
			{
				// Go to first tile at next line
				setTile(0, ctY + 1);
			}
		}
		
		/// <summary> Returns the specified coded code-block, for the specified component, in
		/// the current tile. The first layer to return is indicated by 'fl'. The
		/// number of layers that is returned depends on 'nl' and the amount of
		/// available data.
		/// 
		/// <p>The argument 'fl' is to be used by subsequent calls to this method
		/// for the same code-block. In this way supplemental data can be retrieved
		/// at a later time. The fact that data from more than one layer can be
		/// returned means that several packets from the same code-block, of the
		/// same component, and the same tile, have been concatenated.</p>
		/// 
		/// <p>The returned compressed code-block can have its progressive
		/// attribute set. If this attribute is set it means that more data can be
		/// obtained by subsequent calls to this method (subject to transmission
		/// delays, etc). If the progressive attribute is not set it means that the
		/// returned data is all the data that can be obtained for the specified
		/// code-block.</p>
		/// 
		/// <p>The compressed code-block is uniquely specified by the current tile,
		/// the component (identified by 'c'), the subband (indentified by 'sb')
		/// and the code-block vertical and horizontal indexes 'n' and 'm'.</p>
		/// 
		/// <p>The 'ulx' and 'uly' members of the returned 'DecLyrdCBlk' object
		/// contain the coordinates of the top-left corner of the block, with
		/// respect to the tile, not the subband.</p>
		/// 
		/// </summary>
		/// <param name="c">The index of the component, from 0 to N-1.
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
		/// <param name="sb">The subband in whic the requested code-block is.
		/// 
		/// </param>
		/// <param name="fl">The first layer to return.
		/// 
		/// </param>
		/// <param name="nl">The number of layers to return, if negative all available
		/// layers are returned, starting at 'fl'.
		/// 
		/// </param>
		/// <param name="ccb">If not null this object is used to return the compressed
		/// code-block. If null a new object is created and returned. If the data
		/// array in ccb is not null then it can be reused to return the compressed
		/// data.
		/// 
		/// </param>
		/// <returns> The compressed code-block, with a certain number of layers
		/// determined by the available data and 'nl'.
		/// 
		/// </returns>
		public override DecLyrdCBlk getCodeBlock(int c, int m, int n, SubbandSyn sb, int fl, int nl, DecLyrdCBlk ccb)
		{
			
			int t = TileIdx;
			CBlkInfo rcb; // requested code-block
			int r = sb.resLvl; // Resolution level
			int s = sb.sbandIdx; // Subband index
			int tpidx;
			int passtype;
			
			// Number of layers
			int numLayers = ((System.Int32) decSpec.nls.getTileDef(t));
			int options = ((System.Int32) decSpec.ecopts.getTileCompVal(t, c));
			if (nl < 0)
			{
				nl = numLayers - fl + 1;
			}
			
			// If the l quit condition is used, Make sure that no layer 
			// after lquit is returned
			if (lQuit != - 1 && fl + nl > lQuit)
			{
				nl = lQuit - fl;
			}
			
			// Check validity of resquested resolution level (according to the
			// "-res" option).
			int maxdl = getSynSubbandTree(t, c).resLvl;
			if (r > targetRes + maxdl - decSpec.dls.Min)
			{
				throw new System.ApplicationException("JJ2000 error: requesting a code-block " + "disallowed by the '-res' option.");
			}
			
			// Check validity of all the arguments
			try
			{
				rcb = cbI[c][r][s][m][n];
				
				if (fl < 1 || fl > numLayers || fl + nl - 1 > numLayers)
				{
					throw new System.ArgumentException();
				}
			}
			catch (System.IndexOutOfRangeException)
			{
				throw new System.ArgumentException("Code-block (t:" + t + ", c:" + c + ", r:" + r + ", s:" + s + ", " + m + "x" + (+ n) + ") not found in codestream");
			}
			catch (System.NullReferenceException)
			{
				throw new System.ArgumentException("Code-block (t:" + t + ", c:" + c + ", r:" + r + ", s:" + s + ", " + m + "x" + n + ") not found in bit stream");
			}
			
			// Create DecLyrdCBlk object if necessary
			if (ccb == null)
			{
				ccb = new DecLyrdCBlk();
			}
			ccb.m = m;
			ccb.n = n;
			ccb.nl = 0;
			ccb.dl = 0;
			ccb.nTrunc = 0;
			
			if (rcb == null)
			{
				// This code-block was skipped when reading. Returns no data
				ccb.skipMSBP = 0;
				ccb.prog = false;
				ccb.w = ccb.h = ccb.ulx = ccb.uly = 0;
				return ccb;
			}
			
			// ccb initialization
			ccb.skipMSBP = rcb.msbSkipped;
			ccb.ulx = rcb.ulx;
			ccb.uly = rcb.uly;
			ccb.w = rcb.w;
			ccb.h = rcb.h;
			ccb.ftpIdx = 0;
			
			// Search for index of first truncation point (first layer where
			// length of data is not zero)
			int l = 0;
			while ((l < rcb.len.Length) && (rcb.len[l] == 0))
			{
				ccb.ftpIdx += rcb.ntp[l];
				l++;
			}
			
			// Calculate total length, number of included layer and number of
			// truncation points
			for (l = fl - 1; l < fl + nl - 1; l++)
			{
				ccb.nl++;
				ccb.dl += rcb.len[l];
				ccb.nTrunc += rcb.ntp[l];
			}
			
			// Calculate number of terminated segments
			int nts;
			if ((options & CSJ2K.j2k.entropy.StdEntropyCoderOptions.OPT_TERM_PASS) != 0)
			{
				// Regular termination in use One segment per pass
				// (i.e. truncation point)
				nts = ccb.nTrunc - ccb.ftpIdx;
			}
			else if ((options & CSJ2K.j2k.entropy.StdEntropyCoderOptions.OPT_BYPASS) != 0)
			{
				// Selective arithmetic coding bypass mode in use, but no regular
				// termination: 1 segment upto the end of the last pass of the 4th
				// most significant bit-plane, and, in each following bit-plane,
				// one segment upto the end of the 2nd pass and one upto the end
				// of the 3rd pass.
				
				if (ccb.nTrunc <= CSJ2K.j2k.entropy.StdEntropyCoderOptions.FIRST_BYPASS_PASS_IDX)
				{
					nts = 1;
				}
				else
				{
					nts = 1;
					// Adds one for each terminated pass
					for (tpidx = ccb.ftpIdx; tpidx < ccb.nTrunc; tpidx++)
					{
						if (tpidx >= CSJ2K.j2k.entropy.StdEntropyCoderOptions.FIRST_BYPASS_PASS_IDX - 1)
						{
							passtype = (tpidx + CSJ2K.j2k.entropy.StdEntropyCoderOptions.NUM_EMPTY_PASSES_IN_MS_BP) % CSJ2K.j2k.entropy.StdEntropyCoderOptions.NUM_PASSES;
							if (passtype == 1 || passtype == 2)
							{
								// lazy pass just before MQ pass or MQ pass just
								// before lazy pass => terminated
								nts++;
							}
						}
					}
				}
			}
			else
			{
				// Nothing special in use, just one terminated segment
				nts = 1;
			}
			
			// ccb.data creation
			if (ccb.data == null || ccb.data.Length < ccb.dl)
			{
				ccb.data = new byte[ccb.dl];
			}
			
			// ccb.tsLengths creation
			if (nts > 1 && (ccb.tsLengths == null || ccb.tsLengths.Length < nts))
			{
				ccb.tsLengths = new int[nts];
			}
			else if (nts > 1 && (options & (CSJ2K.j2k.entropy.StdEntropyCoderOptions.OPT_BYPASS | CSJ2K.j2k.entropy.StdEntropyCoderOptions.OPT_TERM_PASS)) == CSJ2K.j2k.entropy.StdEntropyCoderOptions.OPT_BYPASS)
			{
				ArrayUtil.intArraySet(ccb.tsLengths, 0);
			}
			
			// Fill ccb with compressed data
			int dataIdx = - 1;
			tpidx = ccb.ftpIdx;
			int ctp = ccb.ftpIdx; // Cumulative number of truncation
			// point for the current layer layer
			int tsidx = 0;
			int j;
			
			for (l = fl - 1; l < fl + nl - 1; l++)
			{
				ctp += rcb.ntp[l];
				// No data in this layer
				if (rcb.len[l] == 0)
					continue;
				
				// Read data
				// NOTE: we should never get an EOFException here since all
				// data is checked to be within the file.
				try
				{
					in_Renamed.seek(rcb.off[l]);
					in_Renamed.readFully(ccb.data, dataIdx + 1, rcb.len[l]);
					dataIdx += rcb.len[l];
				}
				catch (System.IO.IOException e)
				{
					JJ2KExceptionHandler.handleException(e);
				}
				
				// Get the terminated segment lengths, if any
				if (nts == 1)
					continue;
				if ((options & CSJ2K.j2k.entropy.StdEntropyCoderOptions.OPT_TERM_PASS) != 0)
				{
					// Regular termination => each pass is terminated
					for (j = 0; tpidx < ctp; j++, tpidx++)
					{
						if (rcb.segLen[l] != null)
						{
							ccb.tsLengths[tsidx++] = rcb.segLen[l][j];
						}
						else
						{
							// Only one terminated segment in packet
							ccb.tsLengths[tsidx++] = rcb.len[l];
						}
					}
				}
				else
				{
					// Lazy coding without regular termination
					for (j = 0; tpidx < ctp; tpidx++)
					{
						if (tpidx >= CSJ2K.j2k.entropy.StdEntropyCoderOptions.FIRST_BYPASS_PASS_IDX - 1)
						{
							passtype = (tpidx + CSJ2K.j2k.entropy.StdEntropyCoderOptions.NUM_EMPTY_PASSES_IN_MS_BP) % CSJ2K.j2k.entropy.StdEntropyCoderOptions.NUM_PASSES;
							if (passtype != 0)
							{
								// lazy pass just before MQ pass or MQ
								// pass just before lazy pass =>
								// terminated
								if (rcb.segLen[l] != null)
								{
									ccb.tsLengths[tsidx++] += rcb.segLen[l][j++];
									rcb.len[l] -= rcb.segLen[l][j - 1];
								}
								else
								{
									// Only one terminated segment in packet
									ccb.tsLengths[tsidx++] += rcb.len[l];
									rcb.len[l] = 0;
								}
							}
						}
					}
					
					// Last length in packet always in (either terminated segment
					// or contribution to terminated segment)
					if (rcb.segLen[l] != null && j < rcb.segLen[l].Length)
					{
						ccb.tsLengths[tsidx] += rcb.segLen[l][j];
						rcb.len[l] -= rcb.segLen[l][j];
					}
					else
					{
						// Only one terminated segment in packet
						if (tsidx < nts)
						{
							ccb.tsLengths[tsidx] += rcb.len[l];
							rcb.len[l] = 0;
						}
					}
				}
			}
			if (nts == 1 && ccb.tsLengths != null)
			{
				ccb.tsLengths[0] = ccb.dl;
			}
			
			// Set the progressive flag
			int lastlayer = fl + nl - 1;
			if (lastlayer < numLayers - 1)
			{
				for (l = lastlayer + 1; l < numLayers; l++)
				{
					// It remains data for this code-block in the bit stream
					if (rcb.len[l] != 0)
					{
						ccb.prog = true;
					}
				}
			}
			
			return ccb;
		}
	}
}