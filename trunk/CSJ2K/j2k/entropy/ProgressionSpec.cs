/*
* CVS identifier:
*
* $Id: ProgressionSpec.java,v 1.19 2001/05/02 14:08:42 grosbois Exp $
*
* Class:                   ProgressionSpec
*
* Description:             Specification of the progression(s) type(s) and
*                          changes of progression.
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
	
	/// <summary> This class extends ModuleSpec class for progression type(s) and progression
	/// order changes holding purposes.
	/// 
	/// <p>It stores  the progression type(s) used in the  codestream. There can be
	/// several progression  type(s) if  progression order  changes are  used (POC
	/// markers).</p>
	/// 
	/// </summary>
	public class ProgressionSpec:ModuleSpec
	{
		
		/// <summary> Creates a new ProgressionSpec object for the specified number of tiles
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
		/// component specific or both. The ProgressionSpec class should only be
		/// used only with the type ModuleSpec.SPEC_TYPE_TILE.
		/// 
		/// </param>
		public ProgressionSpec(int nt, int nc, byte type):base(nt, nc, type)
		{
			if (type != ModuleSpec.SPEC_TYPE_TILE)
			{
				throw new System.ApplicationException("Illegal use of class ProgressionSpec !");
			}
		}
		
		/// <summary> Creates a new ProgressionSpec object for the specified number of tiles,
		/// components and the ParameterList instance.
		/// 
		/// </summary>
		/// <param name="nt">The number of tiles
		/// 
		/// </param>
		/// <param name="nc">The number of components
		/// 
		/// </param>
		/// <param name="nl">The number of layer
		/// 
		/// </param>
		/// <param name="dls">The number of decomposition levels specifications
		/// 
		/// </param>
		/// <param name="type">the type of the specification module. The ProgressionSpec
		/// class should only be used only with the type ModuleSpec.SPEC_TYPE_TILE.
		/// 
		/// </param>
		/// <param name="pl">The ParameterList instance
		/// 
		/// </param>
		public ProgressionSpec(int nt, int nc, int nl, IntegerSpec dls, byte type, ParameterList pl):base(nt, nc, type)
		{
			
			System.String param = pl.getParameter("Aptype");
			Progression[] prog;
			int mode = - 1;
			
			if (param == null)
			{
				// No parameter specified
				if (pl.getParameter("Rroi") == null)
				{
					mode = checkProgMode("res");
				}
				else
				{
					mode = checkProgMode("layer");
				}
				
				if (mode == - 1)
				{
					System.String errMsg = "Unknown progression type : '" + param + "'";
					throw new System.ArgumentException(errMsg);
				}
				prog = new Progression[1];
				prog[0] = new Progression(mode, 0, nc, 0, dls.Max + 1, nl);
				setDefault(prog);
				return ;
			}
			
			SupportClass.Tokenizer stk = new SupportClass.Tokenizer(param);
			byte curSpecType = SPEC_DEF; // Specification type of the
			// current parameter
			bool[] tileSpec = null; // Tiles concerned by the specification
			System.String word = null; // current word
			System.String errMsg2 = null; // Error message
			bool needInteger = false; // True if an integer value is expected
			int intType = 0; // Type of read integer value (0=index of first
			// resolution level, 1= index of first component, 2=index of first  
			// layer not included, 3= index of first resolution level not
			// included, 4= index of  first component not included
			System.Collections.ArrayList progression = System.Collections.ArrayList.Synchronized(new System.Collections.ArrayList(10));
			int tmp = 0;
			Progression curProg = null;
			
			while (stk.HasMoreTokens())
			{
				word = stk.NextToken();
				
				switch (word[0])
				{
					
					case 't': 
						// If progression were previously found, store them
						if (progression.Count > 0)
						{
							// Ensure that all information has been taken
							curProg.ce = nc;
							curProg.lye = nl;
							curProg.re = dls.Max + 1;
							prog = new Progression[progression.Count];
							progression.CopyTo(prog);
							if (curSpecType == SPEC_DEF)
							{
								setDefault(prog);
							}
							else if (curSpecType == SPEC_TILE_DEF)
							{
								for (int i = tileSpec.Length - 1; i >= 0; i--)
									if (tileSpec[i])
									{
										setTileDef(i, prog);
									}
							}
						}
						progression.Clear();
						intType = - 1;
						needInteger = false;
						
						// Tiles specification
						tileSpec = parseIdx(word, nTiles);
						curSpecType = SPEC_TILE_DEF;
						break;
					
					default: 
						// Here, words is either a Integer (progression bound index)
						// or a String (progression order type). This is determined by
						// the value of needInteger.
						if (needInteger)
						{
							// Progression bound info
							try
							{
								tmp = (System.Int32.Parse(word));
							}
							catch (System.FormatException)
							{
								// Progression has missing parameters
								throw new System.ArgumentException("Progression " + "order" + " specification " + "has missing " + "parameters: " + param);
							}
							
							switch (intType)
							{
								
								case 0:  // cs
									if (tmp < 0 || tmp > (dls.Max + 1))
										throw new System.ArgumentException("Invalid res_start " + "in '-Aptype'" + " option: " + tmp);
									curProg.rs = tmp; break;
								
								case 1:  // rs
									if (tmp < 0 || tmp > nc)
									{
										throw new System.ArgumentException("Invalid comp_start " + "in '-Aptype' " + "option: " + tmp);
									}
									curProg.cs = tmp; break;
								
								case 2:  // lye
									if (tmp < 0)
										throw new System.ArgumentException("Invalid layer_end " + "in '-Aptype'" + " option: " + tmp);
									if (tmp > nl)
									{
										tmp = nl;
									}
									curProg.lye = tmp; break;
								
								case 3:  // ce
									if (tmp < 0)
										throw new System.ArgumentException("Invalid res_end " + "in '-Aptype'" + " option: " + tmp);
									if (tmp > (dls.Max + 1))
									{
										tmp = dls.Max + 1;
									}
									curProg.re = tmp; break;
								
								case 4:  // re
									if (tmp < 0)
										throw new System.ArgumentException("Invalid comp_end " + "in '-Aptype'" + " option: " + tmp);
									if (tmp > nc)
									{
										tmp = nc;
									}
									curProg.ce = tmp; break;
								}
							
							if (intType < 4)
							{
								intType++;
								needInteger = true;
								break;
							}
							else if (intType == 4)
							{
								intType = 0;
								needInteger = false;
								break;
							}
							else
							{
								throw new System.ApplicationException("Error in usage of 'Aptype' " + "option: " + param);
							}
						}
						
						if (!needInteger)
						{
							// Progression type info
							mode = checkProgMode(word);
							if (mode == - 1)
							{
								errMsg2 = "Unknown progression type : '" + word + "'";
								throw new System.ArgumentException(errMsg2);
							}
							needInteger = true;
							intType = 0;
							if (progression.Count == 0)
							{
								curProg = new Progression(mode, 0, nc, 0, dls.Max + 1, nl);
							}
							else
							{
								curProg = new Progression(mode, 0, nc, 0, dls.Max + 1, nl);
							}
							progression.Add(curProg);
						}
						break;
					
				} // switch
			} // while 
			
			if (progression.Count == 0)
			{
				// No progression defined
				if (pl.getParameter("Rroi") == null)
				{
					mode = checkProgMode("res");
				}
				else
				{
					mode = checkProgMode("layer");
				}
				if (mode == - 1)
				{
					errMsg2 = "Unknown progression type : '" + param + "'";
					throw new System.ArgumentException(errMsg2);
				}
				prog = new Progression[1];
				prog[0] = new Progression(mode, 0, nc, 0, dls.Max + 1, nl);
				setDefault(prog);
				return ;
			}
			
			// Ensure that all information has been taken
			curProg.ce = nc;
			curProg.lye = nl;
			curProg.re = dls.Max + 1;
			
			// Store found progression
			prog = new Progression[progression.Count];
			progression.CopyTo(prog);
			
			if (curSpecType == SPEC_DEF)
			{
				setDefault(prog);
			}
			else if (curSpecType == SPEC_TILE_DEF)
			{
				for (int i = tileSpec.Length - 1; i >= 0; i--)
					if (tileSpec[i])
					{
						setTileDef(i, prog);
					}
			}
			
			// Check that default value has been specified
			if (getDefault() == null)
			{
				int ndefspec = 0;
				for (int t = nt - 1; t >= 0; t--)
				{
					for (int c = nc - 1; c >= 0; c--)
					{
						if (specValType[t][c] == SPEC_DEF)
						{
							ndefspec++;
						}
					}
				}
				
				// If some tile-component have received no specification, they
				// receive the default progressiveness.
				if (ndefspec != 0)
				{
					if (pl.getParameter("Rroi") == null)
					{
						mode = checkProgMode("res");
					}
					else
					{
						mode = checkProgMode("layer");
					}
					if (mode == - 1)
					{
						errMsg2 = "Unknown progression type : '" + param + "'";
						throw new System.ArgumentException(errMsg2);
					}
					prog = new Progression[1];
					prog[0] = new Progression(mode, 0, nc, 0, dls.Max + 1, nl);
					setDefault(prog);
				}
				else
				{
					// All tile-component have been specified, takes the first
					// tile-component value as default.
					setDefault(getTileCompVal(0, 0));
					switch (specValType[0][0])
					{
						
						case SPEC_TILE_DEF: 
							for (int c = nc - 1; c >= 0; c--)
							{
								if (specValType[0][c] == SPEC_TILE_DEF)
									specValType[0][c] = SPEC_DEF;
							}
							tileDef[0] = null;
							break;
						
						case SPEC_COMP_DEF: 
							for (int t = nt - 1; t >= 0; t--)
							{
								if (specValType[t][0] == SPEC_COMP_DEF)
									specValType[t][0] = SPEC_DEF;
							}
							compDef[0] = null;
							break;
						
						case SPEC_TILE_COMP: 
							specValType[0][0] = SPEC_DEF;
							tileCompVal["t0c0"] = null;
							break;
						}
				}
			}
		}
		
		/// <summary> Check if the progression mode exists and if so, return its integer
		/// value. It returns -1 otherwise.
		/// 
		/// </summary>
		/// <param name="mode">The progression mode stored in a string
		/// 
		/// </param>
		/// <returns> The integer value of the progression mode or -1 if the
		/// progression mode does not exist.
		/// 
		/// </returns>
		/// <seealso cref="ProgressionType">
		/// 
		/// </seealso>
		private int checkProgMode(System.String mode)
		{
			if (mode.Equals("res"))
			{
				return CSJ2K.j2k.codestream.ProgressionType.RES_LY_COMP_POS_PROG;
			}
			else if (mode.Equals("layer"))
			{
				return CSJ2K.j2k.codestream.ProgressionType.LY_RES_COMP_POS_PROG;
			}
			else if (mode.Equals("pos-comp"))
			{
				return CSJ2K.j2k.codestream.ProgressionType.POS_COMP_RES_LY_PROG;
			}
			else if (mode.Equals("comp-pos"))
			{
				return CSJ2K.j2k.codestream.ProgressionType.COMP_POS_RES_LY_PROG;
			}
			else if (mode.Equals("res-pos"))
			{
				return CSJ2K.j2k.codestream.ProgressionType.RES_POS_COMP_LY_PROG;
			}
			else
			{
				// No corresponding progression mode, we return -1.
				return - 1;
			}
		}
	}
}