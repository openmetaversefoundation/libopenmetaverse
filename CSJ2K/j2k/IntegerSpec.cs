/* 
* CVS identifier:
* 
* $Id: IntegerSpec.java,v 1.14 2001/09/20 12:31:08 grosbois Exp $
* 
* Class:                   IntegerSpec
* 
* Description:             Holds specs corresponding to an Integer
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
namespace CSJ2K.j2k
{
	
	/// <summary> This class extends ModuleSpec and is responsible of Integer specifications
	/// for each tile-component.
	/// 
	/// </summary>
	/// <seealso cref="ModuleSpec">
	/// 
	/// </seealso>
	public class IntegerSpec:ModuleSpec
	{
		/// <summary> Gets the maximum value of all tile-components.
		/// 
		/// </summary>
		/// <returns> The maximum value
		/// 
		/// </returns>
		virtual public int Max
		{
			get
			{
				int max = ((System.Int32) def);
				int tmp;
				
				for (int t = 0; t < nTiles; t++)
				{
					for (int c = 0; c < nComp; c++)
					{
						tmp = ((System.Int32) getSpec(t, c));
						if (max < tmp)
							max = tmp;
					}
				}
				
				return max;
			}
			
		}
		/// <summary> Get the minimum value of all tile-components.
		/// 
		/// </summary>
		/// <returns> The minimum value
		/// 
		/// </returns>
		virtual public int Min
		{
			get
			{
				int min = ((System.Int32) def);
				int tmp;
				
				for (int t = 0; t < nTiles; t++)
				{
					for (int c = 0; c < nComp; c++)
					{
						tmp = ((System.Int32) getSpec(t, c));
						if (min > tmp)
							min = tmp;
					}
				}
				
				return min;
			}
			
		}
		
		
		/// <summary>The largest value of type int </summary>
		protected internal static int MAX_INT = System.Int32.MaxValue;
		
		/// <summary> Constructs a new 'IntegerSpec' for the specified number of tiles and
		/// components and with allowed type of specifications. This constructor is
		/// normally called at decoder side.
		/// 
		/// </summary>
		/// <param name="nt">The number of tiles
		/// 
		/// </param>
		/// <param name="nc">The number of components
		/// 
		/// </param>
		/// <param name="type">The type of allowed specifications
		/// 
		/// </param>
		public IntegerSpec(int nt, int nc, byte type):base(nt, nc, type)
		{
		}
		
		/// <summary> Constructs a new 'IntegerSpec' for the specified number of tiles and
		/// components, the allowed specifications type and the ParameterList
		/// instance. This constructor is normally called at encoder side and parse
		/// arguments of specified option.
		/// 
		/// </summary>
		/// <param name="nt">The number of tiles
		/// 
		/// </param>
		/// <param name="nc">The number of components
		/// 
		/// </param>
		/// <param name="type">The allowed specifications type
		/// 
		/// </param>
		/// <param name="pl">The ParameterList instance
		/// 
		/// </param>
		/// <param name="optName">The name of the option to process
		/// 
		/// </param>
		public IntegerSpec(int nt, int nc, byte type, ParameterList pl, System.String optName):base(nt, nc, type)
		{
			
			System.Int32 value_Renamed;
			System.String param = pl.getParameter(optName);
			
			if (param == null)
			{
				// No parameter specified
				param = pl.DefaultParameterList.getParameter(optName);
				try
				{
					setDefault((System.Object) System.Int32.Parse(param));
				}
				catch (System.FormatException e)
				{
					throw new System.ArgumentException("Non recognized value" + " for option -" + optName + ": " + param);
				}
				return ;
			}
			
			// Parse argument
			SupportClass.Tokenizer stk = new SupportClass.Tokenizer(param);
			System.String word; // current word
			byte curSpecType = SPEC_DEF; // Specification type of the
			// current parameter
			bool[] tileSpec = null; // Tiles concerned by the specification
			bool[] compSpec = null; // Components concerned by the specification
			
			while (stk.HasMoreTokens())
			{
				word = stk.NextToken();
				
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
						try
						{
							value_Renamed = System.Int32.Parse(word);
						}
						catch (System.FormatException e)
						{
							throw new System.ArgumentException("Non recognized value" + " for option -" + optName + ": " + word);
						}
						
						if (curSpecType == SPEC_DEF)
						{
							setDefault((System.Object) value_Renamed);
						}
						else if (curSpecType == SPEC_TILE_DEF)
						{
							for (int i = tileSpec.Length - 1; i >= 0; i--)
								if (tileSpec[i])
								{
									setTileDef(i, (System.Object) value_Renamed);
								}
						}
						else if (curSpecType == SPEC_COMP_DEF)
						{
							for (int i = compSpec.Length - 1; i >= 0; i--)
								if (compSpec[i])
								{
									setCompDef(i, (System.Object) value_Renamed);
								}
						}
						else
						{
							for (int i = tileSpec.Length - 1; i >= 0; i--)
							{
								for (int j = compSpec.Length - 1; j >= 0; j--)
								{
									if (tileSpec[i] && compSpec[j])
									{
										setTileCompVal(i, j, (System.Object) value_Renamed);
									}
								}
							}
						}
						
						// Re-initialize
						curSpecType = SPEC_DEF;
						tileSpec = null;
						compSpec = null;
						break;
					
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
				
				// If some tile-component have received no specification, it takes
				// the default value defined in ParameterList
				if (ndefspec != 0)
				{
					param = pl.DefaultParameterList.getParameter(optName);
					try
					{
						setDefault((System.Object) System.Int32.Parse(param));
					}
					catch (System.FormatException e)
					{
						throw new System.ArgumentException("Non recognized value" + " for option -" + optName + ": " + param);
					}
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
		
		/// <summary> Gets the maximum value of each tile for specified component
		/// 
		/// </summary>
		/// <param name="c">The component index
		/// 
		/// </param>
		/// <returns> The maximum value
		/// 
		/// </returns>
		public virtual int getMaxInComp(int c)
		{
			int max = 0;
			int tmp;
			
			for (int t = 0; t < nTiles; t++)
			{
				tmp = ((System.Int32) getSpec(t, c));
				if (max < tmp)
					max = tmp;
			}
			
			return max;
		}
		
		/// <summary> Gets the minimum value of all tiles for the specified component.
		/// 
		/// </summary>
		/// <param name="c">The component index
		/// 
		/// </param>
		/// <returns> The minimum value
		/// 
		/// </returns>
		public virtual int getMinInComp(int c)
		{
			int min = MAX_INT; // Big value
			int tmp;
			
			for (int t = 0; t < nTiles; t++)
			{
				tmp = ((System.Int32) getSpec(t, c));
				if (min > tmp)
					min = tmp;
			}
			
			return min;
		}
		
		/// <summary> Gets the maximum value of all components in the specified tile.
		/// 
		/// </summary>
		/// <param name="t">The tile index
		/// 
		/// </param>
		/// <returns> The maximum value
		/// 
		/// </returns>
		public virtual int getMaxInTile(int t)
		{
			int max = 0;
			int tmp;
			
			for (int c = 0; c < nComp; c++)
			{
				tmp = ((System.Int32) getSpec(t, c));
				if (max < tmp)
					max = tmp;
			}
			
			return max;
		}
		
		/// <summary> Gets the minimum value of each component in specified tile
		/// 
		/// </summary>
		/// <param name="t">The tile index
		/// 
		/// </param>
		/// <returns> The minimum value
		/// 
		/// </returns>
		public virtual int getMinInTile(int t)
		{
			int min = MAX_INT; // Big value
			int tmp;
			
			for (int c = 0; c < nComp; c++)
			{
				tmp = ((System.Int32) getSpec(t, c));
				if (min > tmp)
					min = tmp;
			}
			
			return min;
		}
	}
}