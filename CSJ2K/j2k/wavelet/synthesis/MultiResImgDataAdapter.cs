/* 
* CVS identifier:
* 
* $Id: MultiResImgDataAdapter.java,v 1.10 2002/07/25 15:11:55 grosbois Exp $
* 
* Class:                   MultiResImgDataAdapter
* 
* Description:             A default implementation of the MultiResImgData
*                          interface that has and MultiResImgData source
*                          and just returns the values of the source.
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
using CSJ2K.j2k.image;
namespace CSJ2K.j2k.wavelet.synthesis
{
	
	/// <summary> This class provides a default implementation for the methods of the
	/// 'MultiResImgData' interface. The default implementation consists just in
	/// returning the value of the source, where the source is another
	/// 'MultiResImgData' object.
	/// 
	/// <p>This abstract class can be used to facilitate the development of other
	/// classes that implement 'MultiResImgData'. For example a dequantizer can
	/// inherit from this class and all the trivial methods do not have to be
	/// reimplemented.</p>
	/// 
	/// <p>If the default implementation of a method provided in this class does
	/// not suit a particular implementation of the 'MultiResImgData' interface,
	/// the method can be overriden to implement the proper behaviour.</p>
	/// 
	/// </summary>
	/// <seealso cref="MultiResImgData">
	/// 
	/// </seealso>
	public abstract class MultiResImgDataAdapter : MultiResImgData
	{
		/// <summary>Returns the nominal tiles width </summary>
		virtual public int NomTileWidth
		{
			get
			{
				return mressrc.NomTileWidth;
			}
			
		}
		/// <summary>Returns the nominal tiles height </summary>
		virtual public int NomTileHeight
		{
			get
			{
				return mressrc.NomTileHeight;
			}
			
		}
		/// <summary> Returns the number of components in the image.
		/// 
		/// <p>This default implementation returns the value of the source.</p>
		/// 
		/// </summary>
		/// <returns> The number of components in the image.
		/// 
		/// </returns>
		virtual public int NumComps
		{
			get
			{
				return mressrc.NumComps;
			}
			
		}
		/// <summary> Returns the index of the current tile, relative to a standard scan-line
		/// order.
		/// 
		/// <p>This default implementation returns the value of the source.</p>
		/// 
		/// </summary>
		/// <returns> The current tile's index (starts at 0).
		/// 
		/// </returns>
		virtual public int TileIdx
		{
			get
			{
				return mressrc.TileIdx;
			}
			
		}
		/// <summary>Returns the horizontal tile partition offset in the reference grid </summary>
		virtual public int TilePartULX
		{
			get
			{
				return mressrc.TilePartULX;
			}
			
		}
		/// <summary>Returns the vertical tile partition offset in the reference grid </summary>
		virtual public int TilePartULY
		{
			get
			{
				return mressrc.TilePartULY;
			}
			
		}
		
		/// <summary>Index of the current tile </summary>
		protected internal int tIdx = 0;
		
		/// <summary>The MultiResImgData source </summary>
		protected internal MultiResImgData mressrc;
		
		/// <summary> Instantiates the MultiResImgDataAdapter object specifying the
		/// MultiResImgData source.
		/// 
		/// </summary>
		/// <param name="src">From where to obrtain the MultiResImgData values.
		/// 
		/// </param>
		protected internal MultiResImgDataAdapter(MultiResImgData src)
		{
			mressrc = src;
		}
		
		/// <summary> Returns the overall width of the current tile in pixels, for the given
		/// resolution level. This is the tile's width without accounting for any
		/// component subsampling.
		/// 
		/// <p>This default implementation returns the value of the source.</p>
		/// 
		/// </summary>
		/// <param name="rl">The resolution level, from 0 to L.
		/// 
		/// </param>
		/// <returns> The total current tile's width in pixels.
		/// 
		/// </returns>
		public virtual int getTileWidth(int rl)
		{
			return mressrc.getTileWidth(rl);
		}
		
		/// <summary> Returns the overall height of the current tile in pixels, for the given
		/// resolution level. This is the tile's height without accounting for any
		/// component subsampling.
		/// 
		/// <p>This default implementation returns the value of the source.</p>
		/// 
		/// </summary>
		/// <param name="rl">The resolution level, from 0 to L.
		/// 
		/// </param>
		/// <returns> The total current tile's height in pixels.
		/// 
		/// </returns>
		public virtual int getTileHeight(int rl)
		{
			return mressrc.getTileHeight(rl);
		}
		
		/// <summary> Returns the overall width of the image in pixels, for the given
		/// resolution level. This is the image's width without accounting for any
		/// component subsampling or tiling.
		/// 
		/// <p>This default implementation returns the value of the source.</p>
		/// 
		/// </summary>
		/// <param name="rl">The resolution level, from 0 to L.
		/// 
		/// </param>
		/// <returns> The total image's width in pixels.
		/// 
		/// </returns>
		public virtual int getImgWidth(int rl)
		{
			return mressrc.getImgWidth(rl);
		}
		
		/// <summary> Returns the overall height of the image in pixels, for the given
		/// resolution level. This is the image's height without accounting for any
		/// component subsampling or tiling.
		/// 
		/// <p>This default implementation returns the value of the source.</p>
		/// 
		/// </summary>
		/// <param name="rl">The resolution level, from 0 to L.
		/// 
		/// </param>
		/// <returns> The total image's height in pixels.
		/// 
		/// </returns>
		public virtual int getImgHeight(int rl)
		{
			return mressrc.getImgHeight(rl);
		}
		
		/// <summary> Returns the component subsampling factor in the horizontal direction,
		/// for the specified component. This is, approximately, the ratio of
		/// dimensions between the reference grid and the component itself, see the
		/// 'ImgData' interface desription for details.
		/// 
		/// <p>This default implementation returns the value of the source.</p>
		/// 
		/// </summary>
		/// <param name="c">The index of the component (between 0 and N-1)
		/// 
		/// </param>
		/// <returns> The horizontal subsampling factor of component 'c'
		/// 
		/// </returns>
		/// <seealso cref="jj2000.j2k.image.ImgData">
		/// 
		/// </seealso>
		public virtual int getCompSubsX(int c)
		{
			return mressrc.getCompSubsX(c);
		}
		
		/// <summary> Returns the component subsampling factor in the vertical direction, for
		/// the specified component. This is, approximately, the ratio of
		/// dimensions between the reference grid and the component itself, see the
		/// 'ImgData' interface desription for details.
		/// 
		/// <p>This default implementation returns the value of the source.</p>
		/// 
		/// </summary>
		/// <param name="c">The index of the component (between 0 and N-1)
		/// 
		/// </param>
		/// <returns> The vertical subsampling factor of component 'c'
		/// 
		/// </returns>
		/// <seealso cref="jj2000.j2k.image.ImgData">
		/// 
		/// </seealso>
		public virtual int getCompSubsY(int c)
		{
			return mressrc.getCompSubsY(c);
		}
		
		/// <summary> Returns the width in pixels of the specified tile-component for the
		/// given resolution level.
		/// 
		/// <p>This default implementation returns the value of the source.</p>
		/// 
		/// </summary>
		/// <param name="t">Tile index.
		/// 
		/// </param>
		/// <param name="c">The index of the component, from 0 to N-1.
		/// 
		/// </param>
		/// <param name="rl">The resolution level, from 0 to L.
		/// 
		/// </param>
		/// <returns> The width in pixels of component <tt>c</tt> in tile <tt>t</tt>
		/// for resolution level <tt>rl</tt>.
		/// 
		/// </returns>
		public virtual int getTileCompWidth(int t, int c, int rl)
		{
			return mressrc.getTileCompWidth(t, c, rl);
		}
		
		/// <summary> Returns the height in pixels of the specified tile-component for the
		/// given resolution level.
		/// 
		/// <p>This default implementation returns the value of the source.</p>
		/// 
		/// </summary>
		/// <param name="t">The tile index.
		/// 
		/// </param>
		/// <param name="c">The index of the component, from 0 to N-1.
		/// 
		/// </param>
		/// <param name="rl">The resolution level, from 0 to L.
		/// 
		/// </param>
		/// <returns> The height in pixels of component <tt>c</tt> in tile
		/// <tt>t</tt>. 
		/// 
		/// </returns>
		public virtual int getTileCompHeight(int t, int c, int rl)
		{
			return mressrc.getTileCompHeight(t, c, rl);
		}
		
		/// <summary> Returns the width in pixels of the specified component in the overall
		/// image, for the given resolution level.
		/// 
		/// <p>This default implementation returns the value of the source.</p>
		/// 
		/// </summary>
		/// <param name="c">The index of the component, from 0 to N-1.
		/// 
		/// </param>
		/// <param name="rl">The resolution level, from 0 to L.
		/// 
		/// </param>
		/// <returns> The width in pixels of component <tt>c</tt> in the overall
		/// image.
		/// 
		/// </returns>
		public virtual int getCompImgWidth(int c, int rl)
		{
			return mressrc.getCompImgWidth(c, rl);
		}
		
		/// <summary> Returns the height in pixels of the specified component in the overall
		/// image, for the given resolution level.
		/// 
		/// <P>This default implementation returns the value of the source.
		/// 
		/// </summary>
		/// <param name="c">The index of the component, from 0 to N-1.
		/// 
		/// </param>
		/// <param name="rl">The resolution level, from 0 to L.
		/// 
		/// </param>
		/// <returns> The height in pixels of component <tt>c</tt> in the overall
		/// image.
		/// 
		/// </returns>
		public virtual int getCompImgHeight(int c, int rl)
		{
			return mressrc.getCompImgHeight(c, rl);
		}
		
		/// <summary> Changes the current tile, given the new indexes. An
		/// IllegalArgumentException is thrown if the indexes do not correspond to
		/// a valid tile.
		/// 
		/// <p>This default implementation just changes the tile in the source.</p>
		/// 
		/// </summary>
		/// <param name="x">The horizontal indexes the tile.
		/// 
		/// </param>
		/// <param name="y">The vertical indexes of the new tile.
		/// 
		/// </param>
		public virtual void  setTile(int x, int y)
		{
			mressrc.setTile(x, y);
			tIdx = TileIdx;
		}
		
		/// <summary> Advances to the next tile, in standard scan-line order (by rows then
		/// columns). An NoNextElementException is thrown if the current tile is
		/// the last one (i.e. there is no next tile).
		/// 
		/// <p>This default implementation just changes the tile in the source.</p>
		/// 
		/// </summary>
		public virtual void  nextTile()
		{
			mressrc.nextTile();
			tIdx = TileIdx;
		}
		
		/// <summary> Returns the indexes of the current tile. These are the horizontal and
		/// vertical indexes of the current tile.
		/// 
		/// <p>This default implementation returns the value of the source.</p>
		/// 
		/// </summary>
		/// <param name="co">If not null this object is used to return the information. If
		/// null a new one is created and returned.
		/// 
		/// </param>
		/// <returns> The current tile's indexes (vertical and horizontal indexes).
		/// 
		/// </returns>
		public virtual Coord getTile(Coord co)
		{
			return mressrc.getTile(co);
		}
		
		/// <summary> Returns the horizontal coordinate of the upper-left corner of the
		/// specified resolution level in the given component of the current tile. 
		/// 
		/// <p>This default implementation returns the value of the source.</p>
		/// 
		/// </summary>
		/// <param name="c">The component index.
		/// 
		/// </param>
		/// <param name="rl">The resolution level index.
		/// 
		/// </param>
		public virtual int getResULX(int c, int rl)
		{
			return mressrc.getResULX(c, rl);
		}
		
		/// <summary> Returns the vertical coordinate of the upper-left corner of the
		/// specified resolution in the given component of the current tile. 
		/// 
		/// <p>This default implementation returns the value of the source.</p>
		/// 
		/// </summary>
		/// <param name="c">The component index.
		/// 
		/// </param>
		/// <param name="rl">The resolution level index.
		/// 
		/// </param>
		public virtual int getResULY(int c, int rl)
		{
			return mressrc.getResULY(c, rl);
		}
		
		/// <summary> Returns the horizontal coordinate of the image origin, the top-left
		/// corner, in the canvas system, on the reference grid at the specified
		/// resolution level.
		/// 
		/// <p>This default implementation returns the value of the source.</p>
		/// 
		/// </summary>
		/// <param name="rl">The resolution level, from 0 to L.
		/// 
		/// </param>
		/// <returns> The horizontal coordinate of the image origin in the canvas
		/// system, on the reference grid.
		/// 
		/// </returns>
		public virtual int getImgULX(int rl)
		{
			return mressrc.getImgULX(rl);
		}
		
		/// <summary> Returns the vertical coordinate of the image origin, the top-left
		/// corner, in the canvas system, on the reference grid at the specified
		/// resolution level.
		/// 
		/// <p>This default implementation returns the value of the source.</p>
		/// 
		/// </summary>
		/// <param name="rl">The resolution level, from 0 to L.
		/// 
		/// </param>
		/// <returns> The vertical coordinate of the image origin in the canvas
		/// system, on the reference grid.
		/// 
		/// </returns>
		public virtual int getImgULY(int rl)
		{
			return mressrc.getImgULY(rl);
		}
		
		/// <summary> Returns the number of tiles in the horizontal and vertical directions.
		/// 
		/// <p>This default implementation returns the value of the source.</p>
		/// 
		/// </summary>
		/// <param name="co">If not null this object is used to return the information. If
		/// null a new one is created and returned.
		/// 
		/// </param>
		/// <returns> The number of tiles in the horizontal (Coord.x) and vertical
		/// (Coord.y) directions.
		/// 
		/// </returns>
		public virtual Coord getNumTiles(Coord co)
		{
			return mressrc.getNumTiles(co);
		}
		
		/// <summary> Returns the total number of tiles in the image.
		/// 
		/// <p>This default implementation returns the value of the source.</p>
		/// 
		/// </summary>
		/// <returns> The total number of tiles in the image.
		/// 
		/// </returns>
		public virtual int getNumTiles()
		{
			return mressrc.getNumTiles();
		}
		public abstract CSJ2K.j2k.wavelet.synthesis.SubbandSyn getSynSubbandTree(int param1, int param2);
	}
}