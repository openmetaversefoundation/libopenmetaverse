/*
* CVS identifier:
*
* $Id: PrecinctSizeSpec.java,v 1.18 2001/09/14 09:26:58 grosbois Exp $
*
* Class:                   PrecinctSizeSpec
*
* Description:             Specification of the precinct sizes
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
using CSJ2K.j2k.codestream;
using CSJ2K.j2k.wavelet;
using CSJ2K.j2k.image;
using CSJ2K.j2k.util;
using CSJ2K.j2k;
namespace CSJ2K.j2k.entropy
{
	
	/// <summary> This class extends ModuleSpec class for precinct partition sizes holding
	/// purposes.
	/// 
	/// <p>It stores the size a of precinct when precinct partition is used or not.
	/// If precinct partition is used, we can have several packets for a given
	/// resolution level whereas there is only one packet per resolution level if
	/// no precinct partition is used.
	/// 
	/// </summary>
	public class PrecinctSizeSpec:ModuleSpec
	{
		
		/// <summary>Name of the option </summary>
		private const System.String optName = "Cpp";
		
		/// <summary>Reference to wavelet number of decomposition levels for each
		/// tile-component.  
		/// </summary>
		private IntegerSpec dls;
		
		/// <summary> Creates a new PrecinctSizeSpec object for the specified number of tiles
		/// and components.
		/// 
		/// </summary>
		/// <param name="nt">The number of tiles
		/// 
		/// </param>
		/// <param name="nc">The number of components
		/// 
		/// </param>
		/// <param name="type">the type of the specification module i.e. tile specific,
		/// component specific or both.
		/// 
		/// </param>
		/// <param name="dls">Reference to the number of decomposition levels
		/// specification
		/// 
		/// </param>
		public PrecinctSizeSpec(int nt, int nc, byte type, IntegerSpec dls):base(nt, nc, type)
		{
			this.dls = dls;
		}
		
		/// <summary> Creates a new PrecinctSizeSpec object for the specified number of tiles
		/// and components and the ParameterList instance.
		/// 
		/// </summary>
		/// <param name="nt">The number of tiles
		/// 
		/// </param>
		/// <param name="nc">The number of components
		/// 
		/// </param>
		/// <param name="type">the type of the specification module i.e. tile specific,
		/// component specific or both.
		/// 
		/// </param>
		/// <param name="imgsrc">The image source (used to get the image size)
		/// 
		/// </param>
		/// <param name="pl">The ParameterList instance
		/// 
		/// </param>
		public PrecinctSizeSpec(int nt, int nc, byte type, BlkImgDataSrc imgsrc, IntegerSpec dls, ParameterList pl):base(nt, nc, type)
		{
			
			this.dls = dls;
			
			// The precinct sizes are stored in a 2 elements vector array, the
			// first element containing a vector for the precincts width for each
			// resolution level and the second element containing a vector for the
			// precincts height for each resolution level. The precincts sizes are
			// specified from the highest resolution level to the lowest one
			// (i.e. 0).  If there are less elements than the number of
			// decomposition levels, the last element is used for all remaining
			// resolution levels (i.e. if the precincts sizes are specified only
			// for resolutions levels 5, 4 and 3, then the precincts size for
			// resolution levels 2, 1 and 0 will be the same as the size used for
			// resolution level 3).
			
			// Boolean used to know if we were previously reading a precinct's 
			// size or if we were reading something else.
			bool wasReadingPrecinctSize = false;
			
			System.String param = pl.getParameter(optName);
			
			// Set precinct sizes to default i.e. 2^15 =
			// Markers.PRECINCT_PARTITION_DEF_SIZE
			System.Collections.ArrayList[] tmpv = new System.Collections.ArrayList[2];
			tmpv[0] = System.Collections.ArrayList.Synchronized(new System.Collections.ArrayList(10)); // ppx
			tmpv[0].Add((System.Int32) CSJ2K.j2k.codestream.Markers.PRECINCT_PARTITION_DEF_SIZE);
			tmpv[1] = System.Collections.ArrayList.Synchronized(new System.Collections.ArrayList(10)); // ppy
			tmpv[1].Add((System.Int32) CSJ2K.j2k.codestream.Markers.PRECINCT_PARTITION_DEF_SIZE);
			setDefault(tmpv);
			
			if (param == null)
			{
				// No precinct size specified in the command line so we do not try 
				// to parse it.
				return ;
			}
			
			// Precinct partition is used : parse arguments
			SupportClass.Tokenizer stk = new SupportClass.Tokenizer(param);
			byte curSpecType = SPEC_DEF; // Specification type of the
			// current parameter
			bool[] tileSpec = null; // Tiles concerned by the specification
			bool[] compSpec = null; // Components concerned by the specification
            int ci, ti; //i, xIdx removed
			
			bool endOfParamList = false;
			System.String word = null; // current word
			System.Int32 w, h;
			System.String errMsg = null;
			
			while ((stk.HasMoreTokens() || wasReadingPrecinctSize) && !endOfParamList)
			{
				
				System.Collections.ArrayList[] v = new System.Collections.ArrayList[2]; // v[0] : ppx, v[1] : ppy
				
				// We do not read the next token if we were reading a precinct's
				// size argument as we have already read the next token into word.
				if (!wasReadingPrecinctSize)
				{
					word = stk.NextToken();
				}
				
				wasReadingPrecinctSize = false;
				
				switch (word[0])
				{
					
					
					case 't':  // Tiles specification
						tileSpec = parseIdx(word, nTiles);
						if (curSpecType == SPEC_COMP_DEF)
						{
							curSpecType = SPEC_TILE_COMP;
						}
						else
						{
							curSpecType = SPEC_TILE_DEF;
						}
						break;
					
					
					case 'c':  // Components specification
						compSpec = parseIdx(word, nComp);
						if (curSpecType == SPEC_TILE_DEF)
						{
							curSpecType = SPEC_TILE_COMP;
						}
						else
						{
							curSpecType = SPEC_COMP_DEF;
						}
						break;
					
					
					default: 
						if (!System.Char.IsDigit(word[0]))
						{
							errMsg = "Bad construction for parameter: " + word;
							throw new System.ArgumentException(errMsg);
						}
						
						// Initialises Vector objects
						v[0] = System.Collections.ArrayList.Synchronized(new System.Collections.ArrayList(10)); // ppx
						v[1] = System.Collections.ArrayList.Synchronized(new System.Collections.ArrayList(10)); // ppy
						
						while (true)
						{
							
							// Now get the precinct dimensions
							try
							{
								// Get precinct width
								w = System.Int32.Parse(word);
								
								// Get next word in argument list
								try
								{
									word = stk.NextToken();
								}
								catch (System.ArgumentOutOfRangeException)
								{
									errMsg = "'" + optName + "' option : could not " + "parse the precinct's width";
									throw new System.ArgumentException(errMsg);
								}
								// Get precinct height
								h = System.Int32.Parse(word);
								if (w != (1 << MathUtil.log2(w)) || h != (1 << MathUtil.log2(h)))
								{
									errMsg = "Precinct dimensions must be powers of 2";
									throw new System.ArgumentException(errMsg);
								}
							}
							catch (System.FormatException)
							{
								errMsg = "'" + optName + "' option : the argument '" + word + "' could not be parsed.";
								throw new System.ArgumentException(errMsg);
							}
							// Store packet's dimensions in Vector arrays
							v[0].Add(w);
							v[1].Add(h);
							
							// Try to get the next token
							if (stk.HasMoreTokens())
							{
								word = stk.NextToken();
								if (!System.Char.IsDigit(word[0]))
								{
									// The next token does not start with a digit so
									// it is not a precinct's size argument. We set
									// the wasReadingPrecinctSize booleen such that we
									// know that we don't have to read another token
									// and check for the end of the parameters list.
									wasReadingPrecinctSize = true;
									
									if (curSpecType == SPEC_DEF)
									{
										setDefault(v);
									}
									else if (curSpecType == SPEC_TILE_DEF)
									{
										for (ti = tileSpec.Length - 1; ti >= 0; ti--)
										{
											if (tileSpec[ti])
											{
												setTileDef(ti, v);
											}
										}
									}
									else if (curSpecType == SPEC_COMP_DEF)
									{
										for (ci = compSpec.Length - 1; ci >= 0; ci--)
										{
											if (compSpec[ci])
											{
												setCompDef(ci, v);
											}
										}
									}
									else
									{
										for (ti = tileSpec.Length - 1; ti >= 0; ti--)
										{
											for (ci = compSpec.Length - 1; ci >= 0; ci--)
											{
												if (tileSpec[ti] && compSpec[ci])
												{
													setTileCompVal(ti, ci, v);
												}
											}
										}
									}
									// Re-initialize
									curSpecType = SPEC_DEF;
									tileSpec = null;
									compSpec = null;
									
									// Go back to 'normal' parsing
									break;
								}
								else
								{
									// Next token starts with a digit so read it
								}
							}
							else
							{
								// We have reached the end of the parameters list so
								// we store the last precinct's sizes and we stop
								if (curSpecType == SPEC_DEF)
								{
									setDefault(v);
								}
								else if (curSpecType == SPEC_TILE_DEF)
								{
									for (ti = tileSpec.Length - 1; ti >= 0; ti--)
									{
										if (tileSpec[ti])
										{
											setTileDef(ti, v);
										}
									}
								}
								else if (curSpecType == SPEC_COMP_DEF)
								{
									for (ci = compSpec.Length - 1; ci >= 0; ci--)
									{
										if (compSpec[ci])
										{
											setCompDef(ci, v);
										}
									}
								}
								else
								{
									for (ti = tileSpec.Length - 1; ti >= 0; ti--)
									{
										for (ci = compSpec.Length - 1; ci >= 0; ci--)
										{
											if (tileSpec[ti] && compSpec[ci])
											{
												setTileCompVal(ti, ci, v);
											}
										}
									}
								}
								endOfParamList = true;
								break;
							}
						} // while (true)
						break;
					
				} // switch
			} // while
		}
		
		/// <summary> Returns the precinct partition width in component 'n' and tile 't' at
		/// resolution level 'rl'. If the tile index is equal to -1 or if the
		/// component index is equal to -1 it means that those should not be taken
		/// into account.
		/// 
		/// </summary>
		/// <param name="t">The tile index, in raster scan order. Specify -1 if it is not
		/// a specific tile.
		/// 
		/// </param>
		/// <param name="c">The component index. Specify -1 if it is not a specific
		/// component.
		/// 
		/// </param>
		/// <param name="rl">The resolution level
		/// 
		/// </param>
		/// <returns> The precinct partition width in component 'c' and tile 't' at
		/// resolution level 'rl'.
		/// 
		/// </returns>
		public virtual int getPPX(int t, int c, int rl)
		{
			int mrl, idx;
			System.Collections.ArrayList[] v = null;
			bool tileSpecified = (t != - 1?true:false);
			bool compSpecified = (c != - 1?true:false);
			
			// Get the maximum number of decomposition levels and the object
			// (Vector array) containing the precinct dimensions (width and
			// height) for the specified (or not) tile/component
			if (tileSpecified && compSpecified)
			{
				mrl = ((System.Int32) dls.getTileCompVal(t, c));
				v = (System.Collections.ArrayList[]) getTileCompVal(t, c);
			}
			else if (tileSpecified && !compSpecified)
			{
				mrl = ((System.Int32) dls.getTileDef(t));
				v = (System.Collections.ArrayList[]) getTileDef(t);
			}
			else if (!tileSpecified && compSpecified)
			{
				mrl = ((System.Int32) dls.getCompDef(c));
				v = (System.Collections.ArrayList[]) getCompDef(c);
			}
			else
			{
				mrl = ((System.Int32) dls.getDefault());
				v = (System.Collections.ArrayList[]) getDefault();
			}
			idx = mrl - rl;
			if (v[0].Count > idx)
			{
				return ((System.Int32) v[0][idx]);
			}
			else
			{
				return ((System.Int32) v[0][v[0].Count - 1]);
			}
		}
		
		/// <summary> Returns the precinct partition height in component 'n' and tile 't' at
		/// resolution level 'rl'. If the tile index is equal to -1 or if the
		/// component index is equal to -1 it means that those should not be taken
		/// into account.
		/// 
		/// </summary>
		/// <param name="t">The tile index, in raster scan order. Specify -1 if it is not
		/// a specific tile.
		/// 
		/// </param>
		/// <param name="c">The component index. Specify -1 if it is not a specific
		/// component.
		/// 
		/// </param>
		/// <param name="rl">The resolution level.
		/// 
		/// </param>
		/// <returns> The precinct partition width in component 'n' and tile 't' at
		/// resolution level 'rl'.
		/// 
		/// </returns>
		public virtual int getPPY(int t, int c, int rl)
		{
			int mrl, idx;
			System.Collections.ArrayList[] v = null;
			bool tileSpecified = (t != - 1?true:false);
			bool compSpecified = (c != - 1?true:false);
			
			// Get the maximum number of decomposition levels and the object
			// (Vector array) containing the precinct dimensions (width and
			// height) for the specified (or not) tile/component
			if (tileSpecified && compSpecified)
			{
				mrl = ((System.Int32) dls.getTileCompVal(t, c));
				v = (System.Collections.ArrayList[]) getTileCompVal(t, c);
			}
			else if (tileSpecified && !compSpecified)
			{
				mrl = ((System.Int32) dls.getTileDef(t));
				v = (System.Collections.ArrayList[]) getTileDef(t);
			}
			else if (!tileSpecified && compSpecified)
			{
				mrl = ((System.Int32) dls.getCompDef(c));
				v = (System.Collections.ArrayList[]) getCompDef(c);
			}
			else
			{
				mrl = ((System.Int32) dls.getDefault());
				v = (System.Collections.ArrayList[]) getDefault();
			}
			idx = mrl - rl;
			if (v[1].Count > idx)
			{
				return ((System.Int32) v[1][idx]);
			}
			else
			{
				return ((System.Int32) v[1][v[1].Count - 1]);
			}
		}
	}
}