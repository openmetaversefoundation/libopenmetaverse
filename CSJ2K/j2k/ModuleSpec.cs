/* 
* CVS identifier:
* 
* $Id: ModuleSpec.java,v 1.24 2001/10/26 16:30:11 grosbois Exp $
* 
* Class:                   ModuleSpec
* 
* Description:             Generic class for storing module specs
* 
*                           from WTFilterSpec (Diego Santa Cruz)
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
namespace CSJ2K.j2k
{
	
	/// <summary> This generic class is used to handle values to be used by a module for each
	/// tile and component.  It uses attribute to determine which value to use. It
	/// should be extended by each module needing this feature.
	/// 
	/// This class might be used for values that are only tile specific or
	/// component specific but not both.
	/// 
	/// <p>The attributes to use are defined by a hierarchy. The hierarchy is:
	/// 
	/// <ul>
	/// <li> Tile and component specific attribute</li>
	/// <li> Tile specific default attribute</li>
	/// <li> Component main default attribute</li>
	/// <li> Main default attribute</li>
	/// </ul></p>
	/// 
	/// </summary>
	public class ModuleSpec : System.ICloneable
	{
		virtual public ModuleSpec Copy
		{
			get
			{
				return (ModuleSpec) this.Clone();
			}
			
		}
		
		/// <summary>The identifier for a specification module that applies only to
		/// components 
		/// </summary>
		public const byte SPEC_TYPE_COMP = 0;
		
		/// <summary>The identifier for a specification module that applies only to tiles </summary>
		public const byte SPEC_TYPE_TILE = 1;
		
		/// <summary>The identifier for a specification module that applies both to tiles
		/// and components 
		/// </summary>
		public const byte SPEC_TYPE_TILE_COMP = 2;
		
		/// <summary>The identifier for default specification </summary>
		public const byte SPEC_DEF = 0;
		
		/// <summary>The identifier for "component default" specification </summary>
		public const byte SPEC_COMP_DEF = 1;
		
		/// <summary>The identifier for "tile default" specification </summary>
		public const byte SPEC_TILE_DEF = 2;
		
		/// <summary>The identifier for a "tile-component" specification </summary>
		public const byte SPEC_TILE_COMP = 3;
		
		/// <summary>The type of the specification module </summary>
		protected internal int specType;
		
		/// <summary>The number of tiles </summary>
		protected internal int nTiles = 0;
		
		/// <summary>The number of components </summary>
		protected internal int nComp = 0;
		
		/// <summary>The spec type for each tile-component. The first index is the tile
		/// index, the second is the component index.  
		/// </summary>
		protected internal byte[][] specValType;
		
		/// <summary>Default value for each tile-component </summary>
		protected internal System.Object def = null;
		
		/// <summary>The default value for each component. Null if no component
		/// specific value is defined 
		/// </summary>
		protected internal System.Object[] compDef = null;
		
		/// <summary>The default value for each tile. Null if no tile specific value is
		/// defined 
		/// </summary>
		protected internal System.Object[] tileDef = null;
		
		/// <summary>The specific value for each tile-component. Value of tile 16 component
		/// 3 is accessible through the hash value "t16c3". Null if no
		/// tile-component specific value is defined 
		/// </summary>
		protected internal System.Collections.Hashtable tileCompVal;
		
		public virtual System.Object Clone()
		{
			ModuleSpec ms;
			try
			{
				ms = (ModuleSpec) base.MemberwiseClone();
			}
			//UPGRADE_NOTE: Exception 'java.lang.CloneNotSupportedException' was converted to 'System.Exception' which has different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1100'"
			catch (System.Exception e)
			{
				throw new System.ApplicationException("Error when cloning ModuleSpec instance");
			}
			// Create a copy of the specValType array
			ms.specValType = new byte[nTiles][];
			for (int i = 0; i < nTiles; i++)
			{
				ms.specValType[i] = new byte[nComp];
			}
			for (int t = 0; t < nTiles; t++)
			{
				for (int c = 0; c < nComp; c++)
				{
					ms.specValType[t][c] = specValType[t][c];
				}
			}
			// Create a copy of tileDef
			if (tileDef != null)
			{
				ms.tileDef = new System.Object[nTiles];
				for (int t = 0; t < nTiles; t++)
				{
					ms.tileDef[t] = tileDef[t];
				}
			}
			// Create a copy of tileCompVal
			if (tileCompVal != null)
			{
				ms.tileCompVal = System.Collections.Hashtable.Synchronized(new System.Collections.Hashtable());
				System.String tmpKey;
				System.Object tmpVal;
				//UPGRADE_TODO: Method 'java.util.Enumeration.hasMoreElements' was converted to 'System.Collections.IEnumerator.MoveNext' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilEnumerationhasMoreElements'"
				for (System.Collections.IEnumerator e = tileCompVal.Keys.GetEnumerator(); e.MoveNext(); )
				{
					//UPGRADE_TODO: Method 'java.util.Enumeration.nextElement' was converted to 'System.Collections.IEnumerator.Current' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilEnumerationnextElement'"
					tmpKey = ((System.String) e.Current);
					tmpVal = tileCompVal[tmpKey];
					ms.tileCompVal[tmpKey] = tmpVal;
				}
			}
			return ms;
		}
		
		/// <summary> Rotate the ModuleSpec instance by 90 degrees (this modifies only tile
		/// and tile-component specifications).
		/// 
		/// </summary>
		/// <param name="nT">Number of tiles along horizontal and vertical axis after
		/// rotation. 
		/// 
		/// </param>
		public virtual void  rotate90(Coord anT)
		{
			// Rotate specValType
			byte[][] tmpsvt = new byte[nTiles][];
			int ax, ay;
			Coord bnT = new Coord(anT.y, anT.x);
			for (int by = 0; by < bnT.y; by++)
			{
				for (int bx = 0; bx < bnT.x; bx++)
				{
					ay = bx;
					ax = bnT.y - by - 1;
					tmpsvt[ay * anT.x + ax] = specValType[by * bnT.x + bx];
				}
			}
			specValType = tmpsvt;
			
			// Rotate tileDef
			if (tileDef != null)
			{
				System.Object[] tmptd = new System.Object[nTiles];
				for (int by = 0; by < bnT.y; by++)
				{
					for (int bx = 0; bx < bnT.x; bx++)
					{
						ay = bx;
						ax = bnT.y - by - 1;
						tmptd[ay * anT.x + ax] = tileDef[by * bnT.x + bx];
					}
				}
				tileDef = tmptd;
			}
			
			// Rotate tileCompVal
			if (tileCompVal != null && tileCompVal.Count > 0)
			{
				System.Collections.Hashtable tmptcv = System.Collections.Hashtable.Synchronized(new System.Collections.Hashtable());
				System.String tmpKey;
				System.Object tmpVal;
				int btIdx, atIdx;
				int i1, i2;
				int bx, by;
				//UPGRADE_TODO: Method 'java.util.Enumeration.hasMoreElements' was converted to 'System.Collections.IEnumerator.MoveNext' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilEnumerationhasMoreElements'"
				for (System.Collections.IEnumerator e = tileCompVal.Keys.GetEnumerator(); e.MoveNext(); )
				{
					//UPGRADE_TODO: Method 'java.util.Enumeration.nextElement' was converted to 'System.Collections.IEnumerator.Current' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilEnumerationnextElement'"
					tmpKey = ((System.String) e.Current);
					tmpVal = tileCompVal[tmpKey];
					i1 = tmpKey.IndexOf('t');
					i2 = tmpKey.IndexOf('c');
					btIdx = (System.Int32.Parse(tmpKey.Substring(i1 + 1, (i2) - (i1 + 1))));
					bx = btIdx % bnT.x;
					by = btIdx / bnT.x;
					ay = bx;
					ax = bnT.y - by - 1;
					atIdx = ax + ay * anT.x;
					tmptcv["t" + atIdx + tmpKey.Substring(i2)] = tmpVal;
				}
				tileCompVal = tmptcv;
			}
		}
		
		/// <summary> Constructs a 'ModuleSpec' object, initializing all the components and
		/// tiles to the 'SPEC_DEF' spec val type, for the specified number of
		/// components and tiles.
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
		public ModuleSpec(int nt, int nc, byte type)
		{
			nTiles = nt;
			nComp = nc;
			specValType = new byte[nt][];
			for (int i = 0; i < nt; i++)
			{
				specValType[i] = new byte[nc];
			}
			switch (type)
			{
				
				case SPEC_TYPE_TILE: 
					specType = SPEC_TYPE_TILE;
					break;
				
				case SPEC_TYPE_COMP: 
					specType = SPEC_TYPE_COMP;
					break;
				
				case SPEC_TYPE_TILE_COMP: 
					specType = SPEC_TYPE_TILE_COMP;
					break;
				}
		}
		
		/// <summary> Sets default value for this module 
		/// 
		/// </summary>
		public virtual void  setDefault(System.Object value_Renamed)
		{
			def = value_Renamed;
		}
		
		/// <summary> Gets default value for this module. 
		/// 
		/// </summary>
		/// <returns> The default value (Must be casted before use)
		/// 
		/// </returns>
		public virtual System.Object getDefault()
		{
			return def;
		}
		
		/// <summary> Sets default value for specified component and specValType tag if
		/// allowed by its priority.
		/// 
		/// </summary>
		/// <param name="c">Component index 
		/// 
		/// </param>
		public virtual void  setCompDef(int c, System.Object value_Renamed)
		{
			if (specType == SPEC_TYPE_TILE)
			{
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Object.toString' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
				System.String errMsg = "Option whose value is '" + value_Renamed + "' cannot be " + "specified for components as it is a 'tile only' specific " + "option";
				throw new System.ApplicationException(errMsg);
			}
			if (compDef == null)
			{
				compDef = new System.Object[nComp];
			}
			for (int i = 0; i < nTiles; i++)
			{
				if (specValType[i][c] < SPEC_COMP_DEF)
				{
					specValType[i][c] = SPEC_COMP_DEF;
				}
			}
			compDef[c] = value_Renamed;
		}
		
		/// <summary> Gets default value of the specified component. If no specification have
		/// been entered for this component, returns default value.
		/// 
		/// </summary>
		/// <param name="c">Component index 
		/// 
		/// </param>
		/// <returns> The default value for this component (Must be casted before
		/// use)
		/// 
		/// </returns>
		/// <seealso cref="setCompDef">
		/// 
		/// </seealso>
		public virtual System.Object getCompDef(int c)
		{
			if (specType == SPEC_TYPE_TILE)
			{
				throw new System.ApplicationException("Illegal use of ModuleSpec class");
			}
			if (compDef == null || compDef[c] == null)
			{
				return getDefault();
			}
			else
			{
				return compDef[c];
			}
		}
		
		/// <summary> Sets default value for specified tile and specValType tag if allowed by
		/// its priority.
		/// 
		/// </summary>
		/// <param name="c">Tile index.
		/// 
		/// </param>
		public virtual void  setTileDef(int t, System.Object value_Renamed)
		{
			if (specType == SPEC_TYPE_COMP)
			{
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Object.toString' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
				System.String errMsg = "Option whose value is '" + value_Renamed + "' cannot be " + "specified for tiles as it is a 'component only' specific " + "option";
				throw new System.ApplicationException(errMsg);
			}
			if (tileDef == null)
			{
				tileDef = new System.Object[nTiles];
			}
			for (int i = 0; i < nComp; i++)
			{
				if (specValType[t][i] < SPEC_TILE_DEF)
				{
					specValType[t][i] = SPEC_TILE_DEF;
				}
			}
			tileDef[t] = value_Renamed;
		}
		
		/// <summary> Gets default value of the specified tile. If no specification has been
		/// entered, it returns the default value.
		/// 
		/// </summary>
		/// <param name="t">Tile index 
		/// 
		/// </param>
		/// <returns> The default value for this tile (Must be casted before use)
		/// 
		/// </returns>
		/// <seealso cref="setTileDef">
		/// 
		/// </seealso>
		public virtual System.Object getTileDef(int t)
		{
			if (specType == SPEC_TYPE_COMP)
			{
				throw new System.ApplicationException("Illegal use of ModuleSpec class");
			}
			if (tileDef == null || tileDef[t] == null)
			{
				return getDefault();
			}
			else
			{
				return tileDef[t];
			}
		}
		
		/// <summary> Sets value for specified tile-component.
		/// 
		/// </summary>
		/// <param name="t">Tie index 
		/// 
		/// </param>
		/// <param name="c">Component index 
		/// 
		/// </param>
		public virtual void  setTileCompVal(int t, int c, System.Object value_Renamed)
		{
			if (specType != SPEC_TYPE_TILE_COMP)
			{
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Object.toString' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
				System.String errMsg = "Option whose value is '" + value_Renamed + "' cannot be " + "specified for ";
				switch (specType)
				{
					
					case SPEC_TYPE_TILE: 
						errMsg += "components as it is a 'tile only' specific option";
						break;
					
					case SPEC_TYPE_COMP: 
						errMsg += "tiles as it is a 'component only' specific option";
						break;
					}
				throw new System.ApplicationException(errMsg);
			}
			if (tileCompVal == null)
				tileCompVal = System.Collections.Hashtable.Synchronized(new System.Collections.Hashtable());
			specValType[t][c] = SPEC_TILE_COMP;
			tileCompVal["t" + t + "c" + c] = value_Renamed;
		}
		
		/// <summary> Gets value of specified tile-component. This method calls getSpec but
		/// has a public access.
		/// 
		/// </summary>
		/// <param name="t">Tile index 
		/// 
		/// </param>
		/// <param name="c">Component index 
		/// 
		/// </param>
		/// <returns> The value of this tile-component (Must be casted before use)
		/// 
		/// </returns>
		/// <seealso cref="setTileCompVal">
		/// 
		/// </seealso>
		/// <seealso cref="getSpec">
		/// 
		/// </seealso>
		public virtual System.Object getTileCompVal(int t, int c)
		{
			if (specType != SPEC_TYPE_TILE_COMP)
			{
				throw new System.ApplicationException("Illegal use of ModuleSpec class");
			}
			return getSpec(t, c);
		}
		
		/// <summary> Gets value of specified tile-component without knowing if a specific
		/// tile-component value has been previously entered. It first check if a
		/// tile-component specific value has been entered, then if a tile specific
		/// value exist, then if a component specific value exist. If not the
		/// default value is returned.
		/// 
		/// </summary>
		/// <param name="t">Tile index
		/// 
		/// </param>
		/// <param name="c">Component index
		/// 
		/// </param>
		/// <returns> Value for this tile component.
		/// 
		/// </returns>
		protected internal virtual System.Object getSpec(int t, int c)
		{
			switch (specValType[t][c])
			{
				
				case SPEC_DEF: 
					return getDefault();
				
				case SPEC_COMP_DEF: 
					return getCompDef(c);
				
				case SPEC_TILE_DEF: 
					return getTileDef(t);
				
				case SPEC_TILE_COMP: 
					return tileCompVal["t" + t + "c" + c];
				
				default: 
					throw new System.ArgumentException("Not recognized spec type");
				
			}
		}
		
		/// <summary> Return the spec type of the given tile-component.
		/// 
		/// </summary>
		/// <param name="t">Tile index
		/// 
		/// </param>
		/// <param name="c">Component index
		/// 
		/// </param>
		public virtual byte getSpecValType(int t, int c)
		{
			return specValType[t][c];
		}
		
		/// <summary> Whether or not specifications have been entered for the given
		/// component.
		/// 
		/// </summary>
		/// <param name="c">Index of the component
		/// 
		/// </param>
		/// <returns> True if component specification has been defined
		/// 
		/// </returns>
		public virtual bool isCompSpecified(int c)
		{
			if (compDef == null || compDef[c] == null)
			{
				return false;
			}
			else
			{
				return true;
			}
		}
		
		/// <summary> Whether or not specifications have been entered for the given tile.
		/// 
		/// </summary>
		/// <param name="t">Index of the tile
		/// 
		/// </param>
		/// <returns> True if tile specification has been entered
		/// 
		/// </returns>
		public virtual bool isTileSpecified(int t)
		{
			if (tileDef == null || tileDef[t] == null)
			{
				return false;
			}
			else
			{
				return true;
			}
		}
		
		/// <summary> Whether or not a tile-component specification has been defined
		/// 
		/// </summary>
		/// <param name="t">Tile index
		/// 
		/// </param>
		/// <param name="c">Component index
		/// 
		/// </param>
		/// <returns> True if a tile-component specification has been defined.
		/// 
		/// </returns>
		public virtual bool isTileCompSpecified(int t, int c)
		{
			if (tileCompVal == null || tileCompVal["t" + t + "c" + c] == null)
			{
				return false;
			}
			else
			{
				return true;
			}
		}
		
		/// <summary> This method is responsible of parsing tile indexes set and component
		/// indexes set for an option. Such an argument must follow the following
		/// policy:<br>
		/// 
		/// <tt>t&lt;indexes set&gt;</tt> or <tt>c&lt;indexes set&gt;</tt> where
		/// tile or component indexes are separated by commas or a dashes.
		/// 
		/// <p><u>Example:</u><br>
		/// <li> <tt>t0,3,4</tt> means tiles with indexes 0, 3 and 4.<br>
		/// <li> <tt>t2-4</tt> means tiles with indexes 2,3 and 4.<br>
		/// 
		/// It returns a boolean array skteching which tile or component are
		/// concerned by the next parameters.
		/// 
		/// </summary>
		/// <param name="word">The word to parse.
		/// 
		/// </param>
		/// <param name="maxIdx">Maximum authorized index
		/// 
		/// </param>
		/// <returns> Indexes concerned by this parameter.
		/// 
		/// </returns>
		public static bool[] parseIdx(System.String word, int maxIdx)
		{
			int nChar = word.Length; // Number of characters
			char c = word[0]; // current character
			int idx = - 1; // Current (tile or component) index
			int lastIdx = - 1; // Last (tile or component) index
			bool isDash = false; // Whether or not last separator was a dash
			
			bool[] idxSet = new bool[maxIdx];
			int i = 1; // index of the current character
			
			while (i < nChar)
			{
				c = word[i];
				if (System.Char.IsDigit(c))
				{
					if (idx == - 1)
					{
						idx = 0;
					}
					idx = idx * 10 + (c - '0');
				}
				else
				{
					if (idx == - 1 || (c != ',' && c != '-'))
					{
						throw new System.ArgumentException("Bad construction for " + "parameter: " + word);
					}
					if (idx < 0 || idx >= maxIdx)
					{
						throw new System.ArgumentException("Out of range index " + "in " + "parameter `" + word + "' : " + idx);
					}
					
					// Found a comma
					if (c == ',')
					{
						if (isDash)
						{
							// Previously found a dash, fill idxSet
							for (int j = lastIdx + 1; j < idx; j++)
							{
								idxSet[j] = true;
							}
						}
						isDash = false;
					}
					else
					{
						// Found a dash
						isDash = true;
					}
					
					// Udate idxSet
					idxSet[idx] = true;
					lastIdx = idx;
					idx = - 1;
				}
				i++;
			}
			
			// Process last found index
			if (idx < 0 || idx >= maxIdx)
			{
				throw new System.ArgumentException("Out of range index in " + "parameter `" + word + "' : " + idx);
			}
			if (isDash)
			{
				for (int j = lastIdx + 1; j < idx; j++)
				{
					idxSet[j] = true;
				}
			}
			idxSet[idx] = true;
			
			return idxSet;
		}
	}
}