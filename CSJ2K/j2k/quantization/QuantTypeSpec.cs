/* 
* CVS identifier:
* 
* $Id: QuantTypeSpec.java,v 1.18 2001/10/24 12:05:18 grosbois Exp $
* 
* Class:                   QuantTypeSpec
* 
* Description:             Quantization type specifications
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
using CSJ2K.j2k.util;
using CSJ2K.j2k;
namespace CSJ2K.j2k.quantization
{
	
	/// <summary> This class extends ModuleSpec class in order to hold specifications about
	/// the quantization type to use in each tile-component. Supported quantization
	/// type are:<br>
	/// 
	/// <ul>
	/// <li> Reversible (no quantization)</li>
	/// <li> Derived (the quantization step size is derived from the one of the
	/// LL-subband)</li>
	/// <li> Expounded (the quantization step size of each subband is signalled in
	/// the codestream headers) </li>
	/// </ul>
	/// 
	/// </summary>
	/// <seealso cref="ModuleSpec">
	/// 
	/// </seealso>
	public class QuantTypeSpec:ModuleSpec
	{
		/// <summary> Check the reversibility of the whole image.
		/// 
		/// </summary>
		/// <returns> Whether or not the whole image is reversible
		/// 
		/// </returns>
		virtual public bool FullyReversible
		{
			get
			{
				// The whole image is reversible if default specification is
				// rev and no tile default, component default and
				// tile-component value has been specificied
				if (((System.String) getDefault()).Equals("reversible"))
				{
					for (int t = nTiles - 1; t >= 0; t--)
						for (int c = nComp - 1; c >= 0; c--)
							if (specValType[t][c] != SPEC_DEF)
								return false;
					return true;
				}
				
				return false;
			}
			
		}
		/// <summary> Check the irreversibility of the whole image.
		/// 
		/// </summary>
		/// <returns> Whether or not the whole image is reversible
		/// 
		/// </returns>
		virtual public bool FullyNonReversible
		{
			get
			{
				// The whole image is irreversible no tile-component is reversible
				for (int t = nTiles - 1; t >= 0; t--)
					for (int c = nComp - 1; c >= 0; c--)
						if (((System.String) getSpec(t, c)).Equals("reversible"))
							return false;
				return true;
			}
			
		}
		
		/// <summary> Constructs an empty 'QuantTypeSpec' with the specified number of tiles
		/// and components. This constructor is called by the decoder.
		/// 
		/// </summary>
		/// <param name="nt">Number of tiles
		/// 
		/// </param>
		/// <param name="nc">Number of components
		/// 
		/// </param>
		/// <param name="type">the type of the allowed specifications for this module
		/// i.e. tile specific, component specific or both.
		/// 
		/// </param>
		public QuantTypeSpec(int nt, int nc, byte type):base(nt, nc, type)
		{
		}
		
		
		/// <summary> Constructs a new 'QuantTypeSpec' for the specified number of components
		/// and tiles and the arguments of "-Qtype" option. This constructor is
		/// called by the encoder.
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
		/// <param name="pl">The ParameterList
		/// 
		/// </param>
		public QuantTypeSpec(int nt, int nc, byte type, ParameterList pl):base(nt, nc, type)
		{
			
			System.String param = pl.getParameter("Qtype");
			if (param == null)
			{
				if (pl.getBooleanParameter("lossless"))
				{
					setDefault("reversible");
				}
				else
				{
					setDefault("expounded");
				}
				return ;
			}
			
			// Parse argument
			SupportClass.Tokenizer stk = new SupportClass.Tokenizer(param);
			System.String word; // current word
			byte curSpecValType = SPEC_DEF; // Specification type of the
			// current parameter
			bool[] tileSpec = null; // Tiles concerned by the specification
			bool[] compSpec = null; // Components concerned by the specification
			
			while (stk.HasMoreTokens())
			{
				word = stk.NextToken().ToLower();
				
				switch (word[0])
				{
					
					case 't':  // Tiles specification
						tileSpec = parseIdx(word, nTiles);
						
						if (curSpecValType == SPEC_COMP_DEF)
						{
							curSpecValType = SPEC_TILE_COMP;
						}
						else
						{
							curSpecValType = SPEC_TILE_DEF;
						}
						break;
					
					case 'c':  // Components specification
						compSpec = parseIdx(word, nComp);
						
						if (curSpecValType == SPEC_TILE_DEF)
						{
							curSpecValType = SPEC_TILE_COMP;
						}
						else
						{
							curSpecValType = SPEC_COMP_DEF;
						}
						break;
					
					case 'r': 
					// reversible specification
					case 'd': 
					// derived quantization step size specification
					case 'e':  // expounded quantization step size specification
						if (!word.ToUpper().Equals("reversible".ToUpper()) && !word.ToUpper().Equals("derived".ToUpper()) && !word.ToUpper().Equals("expounded".ToUpper()))
						{
							throw new System.ArgumentException("Unknown parameter " + "for " + "'-Qtype' option: " + word);
						}
						
						if (pl.getBooleanParameter("lossless") && (word.ToUpper().Equals("derived".ToUpper()) || word.ToUpper().Equals("expounded".ToUpper())))
						{
							throw new System.ArgumentException("Cannot use non " + "reversible " + "quantization with " + "'-lossless' option");
						}
						
						if (curSpecValType == SPEC_DEF)
						{
							// Default specification
							setDefault(word);
						}
						else if (curSpecValType == SPEC_TILE_DEF)
						{
							// Tile default specification
							for (int i = tileSpec.Length - 1; i >= 0; i--)
							{
								if (tileSpec[i])
								{
									setTileDef(i, word);
								}
							}
						}
						else if (curSpecValType == SPEC_COMP_DEF)
						{
							// Component default specification 
							for (int i = compSpec.Length - 1; i >= 0; i--)
								if (compSpec[i])
								{
									setCompDef(i, word);
								}
						}
						else
						{
							// Tile-component specification
							for (int i = tileSpec.Length - 1; i >= 0; i--)
							{
								for (int j = compSpec.Length - 1; j >= 0; j--)
								{
									if (tileSpec[i] && compSpec[j])
									{
										setTileCompVal(i, j, word);
									}
								}
							}
						}
						
						// Re-initialize
						curSpecValType = SPEC_DEF;
						tileSpec = null;
						compSpec = null;
						break;
					
					
					default: 
						throw new System.ArgumentException("Unknown parameter for " + "'-Qtype' option: " + word);
					
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
				
				// If some tile-component have received no specification, the
				// quantization type is 'reversible' (if '-lossless' is specified)
				// or 'expounded' (if not). 
				if (ndefspec != 0)
				{
					if (pl.getBooleanParameter("lossless"))
					{
						setDefault("reversible");
					}
					else
					{
						setDefault("expounded");
					}
				}
				else
				{
					// All tile-component have been specified, takes arbitrarily
					// the first tile-component value as default and modifies the
					// specification type of all tile-component sharing this
					// value.
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
		
		/// <summary> Returns true if given tile-component uses derived quantization step
		/// size.
		/// 
		/// </summary>
		/// <param name="t">Tile index
		/// 
		/// </param>
		/// <param name="c">Component index
		/// 
		/// </param>
		/// <returns> True if derived quantization step size
		/// 
		/// </returns>
		public virtual bool isDerived(int t, int c)
		{
			if (((System.String) getTileCompVal(t, c)).Equals("derived"))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		
		/// <summary> Check the reversibility of the given tile-component.
		/// 
		/// </summary>
		/// <param name="t">The index of the tile
		/// 
		/// </param>
		/// <param name="c">The index of the component
		/// 
		/// </param>
		/// <returns> Whether or not the tile-component is reversible
		/// 
		/// </returns>
		public virtual bool isReversible(int t, int c)
		{
			if (((System.String) getTileCompVal(t, c)).Equals("reversible"))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}