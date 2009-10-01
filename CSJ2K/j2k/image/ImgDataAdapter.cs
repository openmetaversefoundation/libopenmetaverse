/* 
* CVS identifier:
* 
* $Id: ImgDataAdapter.java,v 1.8 2001/09/14 09:17:23 grosbois Exp $
* 
* Class:                   ImgDataAdapter
* 
* Description:             A default implementation of the ImgData
*                          interface that has an ImgData source and just
*                          returns the values of the source.
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
namespace CSJ2K.j2k.image
{
	
	/// <summary> This class provides a default implementation of the methods in the
	/// 'ImgData' interface. The default implementation is just to return the value
	/// of the source, where the source is another 'ImgData' object.
	/// 
	/// <p>This abstract class can be used to facilitate the development of other
	/// classes that implement 'ImgData'. For example a YCbCr color transform can
	/// inherit from this class and all the trivial methods do not have to be
	/// re-implemented.</p>
	/// 
	/// <p>If the default implementation of a method provided in this class does
	/// not suit a particular implementation of the 'ImgData' interface, the method
	/// can be overridden to implement the proper behavior.</p>
	/// 
	/// </summary>
	/// <seealso cref="ImgData">
	/// 
	/// </seealso>
	public abstract class ImgDataAdapter : ImgData
	{
		/// <summary> Returns the overall width of the current tile in pixels. This is the
		/// tile's width without accounting for any component subsampling. This is
		/// also referred as the reference grid width in the current tile.
		/// 
		/// <p>This default implementation returns the value of the source.</p>
		/// 
		/// </summary>
		/// <returns> The total current tile's width in pixels.
		/// 
		/// </returns>
		virtual public int TileWidth
		{
			get
			{
				return imgdatasrc.TileWidth;
			}
			
		}
		/// <summary> Returns the overall height of the current tile in pixels. This is the
		/// tile's height without accounting for any component subsampling. This is
		/// also referred as the reference grid height in the current tile.
		/// 
		/// <p>This default implementation returns the value of the source.</p>
		/// 
		/// </summary>
		/// <returns> The total current tile's height in pixels.
		/// 
		/// </returns>
		virtual public int TileHeight
		{
			get
			{
				return imgdatasrc.TileHeight;
			}
			
		}
		/// <summary>Returns the nominal tiles width </summary>
		virtual public int NomTileWidth
		{
			get
			{
				return imgdatasrc.NomTileWidth;
			}
			
		}
		/// <summary>Returns the nominal tiles height </summary>
		virtual public int NomTileHeight
		{
			get
			{
				return imgdatasrc.NomTileHeight;
			}
			
		}
		/// <summary> Returns the overall width of the image in pixels. This is the image's
		/// width without accounting for any component subsampling or tiling.
		/// 
		/// <p>This default implementation returns the value of the source.</p>
		/// 
		/// </summary>
		/// <returns> The total image's width in pixels.
		/// 
		/// </returns>
		virtual public int ImgWidth
		{
			get
			{
				return imgdatasrc.ImgWidth;
			}
			
		}
		/// <summary> Returns the overall height of the image in pixels. This is the image's
		/// height without accounting for any component subsampling or tiling.
		/// 
		/// <p>This default implementation returns the value of the source.</p>
		/// 
		/// </summary>
		/// <returns> The total image's height in pixels.
		/// 
		/// </returns>
		virtual public int ImgHeight
		{
			get
			{
				return imgdatasrc.ImgHeight;
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
				return imgdatasrc.NumComps;
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
				return imgdatasrc.TileIdx;
			}
			
		}
		/// <summary>Returns the horizontal tile partition offset in the reference grid </summary>
		virtual public int TilePartULX
		{
			get
			{
				return imgdatasrc.TilePartULX;
			}
			
		}
		/// <summary>Returns the vertical tile offset in the reference grid </summary>
		virtual public int TilePartULY
		{
			get
			{
				return imgdatasrc.TilePartULY;
			}
			
		}
		/// <summary> Returns the horizontal coordinate of the image origin, the top-left
		/// corner, in the canvas system, on the reference grid.
		/// 
		/// <p>This default implementation returns the value of the source.</p>
		/// 
		/// </summary>
		/// <returns> The horizontal coordinate of the image origin in the canvas
		/// system, on the reference grid.
		/// 
		/// </returns>
		virtual public int ImgULX
		{
			get
			{
				return imgdatasrc.ImgULX;
			}
			
		}
		/// <summary> Returns the vertical coordinate of the image origin, the top-left
		/// corner, in the canvas system, on the reference grid.
		/// 
		/// <p>This default implementation returns the value of the source.</p>
		/// 
		/// </summary>
		/// <returns> The vertical coordinate of the image origin in the canvas
		/// system, on the reference grid.
		/// 
		/// </returns>
		virtual public int ImgULY
		{
			get
			{
				return imgdatasrc.ImgULY;
			}
			
		}
		
		/// <summary>Index of the current tile </summary>
		protected internal int tIdx = 0;
		
		/// <summary>The ImgData source </summary>
		protected internal ImgData imgdatasrc;
		
		/// <summary> Instantiates the ImgDataAdapter object specifying the ImgData source.
		/// 
		/// </summary>
		/// <param name="src">From where to obtain all the ImgData values.
		/// 
		/// </param>
		protected internal ImgDataAdapter(ImgData src)
		{
			imgdatasrc = src;
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
		/// <seealso cref="ImgData">
		/// 
		/// </seealso>
		public virtual int getCompSubsX(int c)
		{
			return imgdatasrc.getCompSubsX(c);
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
		/// <seealso cref="ImgData">
		/// 
		/// </seealso>
		public virtual int getCompSubsY(int c)
		{
			return imgdatasrc.getCompSubsY(c);
		}
		
		/// <summary> Returns the width in pixels of the specified tile-component
		/// tile.
		/// 
		/// <p>This default implementation returns the value of the source.</p>
		/// 
		/// </summary>
		/// <param name="t">Tile index
		/// 
		/// </param>
		/// <param name="c">The index of the component, from 0 to N-1.
		/// 
		/// </param>
		/// <returns> The width in pixels of component <tt>c</tt> in tile<tt>t</tt>.
		/// 
		/// </returns>
		public virtual int getTileCompWidth(int t, int c)
		{
			return imgdatasrc.getTileCompWidth(t, c);
		}
		
		/// <summary> Returns the height in pixels of the specified tile-component.
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
		/// <returns> The height in pixels of component <tt>c</tt> in tile
		/// <tt>t</tt>.
		/// 
		/// </returns>
		public virtual int getTileCompHeight(int t, int c)
		{
			return imgdatasrc.getTileCompHeight(t, c);
		}
		
		/// <summary> Returns the width in pixels of the specified component in the overall
		/// image.
		/// 
		/// <p>This default implementation returns the value of the source.</p>
		/// 
		/// </summary>
		/// <param name="c">The index of the component, from 0 to N-1.
		/// 
		/// </param>
		/// <returns> The width in pixels of component <tt>c</tt> in the overall
		/// image.
		/// 
		/// </returns>
		public virtual int getCompImgWidth(int c)
		{
			return imgdatasrc.getCompImgWidth(c);
		}
		
		/// <summary> Returns the height in pixels of the specified component in the overall
		/// image.
		/// 
		/// <p>This default implementation returns the value of the source.</p>
		/// 
		/// </summary>
		/// <param name="c">The index of the component, from 0 to N-1.
		/// 
		/// </param>
		/// <returns> The height in pixels of component <tt>c</tt> in the overall
		/// image.
		/// 
		/// </returns>
		public virtual int getCompImgHeight(int c)
		{
			return imgdatasrc.getCompImgHeight(c);
		}
		
		/// <summary> Returns the number of bits, referred to as the "range bits",
		/// corresponding to the nominal range of the image data in the specified
		/// component. If this number is <i>n</b> then for unsigned data the
		/// nominal range is between 0 and 2^b-1, and for signed data it is between
		/// -2^(b-1) and 2^(b-1)-1. In the case of transformed data which is not in
		/// the image domain (e.g., wavelet coefficients), this method returns the
		/// "range bits" of the image data that generated the coefficients.
		/// 
		/// <p>This default implementation returns the value of the source.</p>
		/// 
		/// </summary>
		/// <param name="c">The index of the component.
		/// 
		/// </param>
		/// <returns> The number of bits corresponding to the nominal range of the
		/// image data (in the image domain).
		/// 
		/// </returns>
		public virtual int getNomRangeBits(int c)
		{
			return imgdatasrc.getNomRangeBits(c);
		}
		
		/// <summary> Changes the current tile, given the new indexes. An
		/// IllegalArgumentException is thrown if the indexes do not correspond to
		/// a valid tile.
		/// 
		/// <p>This default implementation just changes the tile in the source.</p>
		/// 
		/// </summary>
		/// <param name="x">The horizontal index of the tile.
		/// 
		/// </param>
		/// <param name="y">The vertical index of the new tile.
		/// 
		/// </param>
		public virtual void  setTile(int x, int y)
		{
			imgdatasrc.setTile(x, y);
			tIdx = TileIdx;
		}
		
		/// <summary> Advances to the next tile, in standard scan-line order (by rows then
		/// columns). An NoNextElementException is thrown if the current tile is
		/// the last one (i.e. there is no next tile).
		/// 
		/// <p>This default implementation just advances to the next tile in the
		/// source.</p>
		/// 
		/// </summary>
		public virtual void  nextTile()
		{
			imgdatasrc.nextTile();
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
			return imgdatasrc.getTile(co);
		}
		
		/// <summary> Returns the horizontal coordinate of the upper-left corner of the
		/// specified component in the current tile.
		/// 
		/// <p>This default implementation returns the value of the source.</p>
		/// 
		/// </summary>
		/// <param name="c">The component index.
		/// 
		/// </param>
		public virtual int getCompULX(int c)
		{
			return imgdatasrc.getCompULX(c);
		}
		
		/// <summary> Returns the vertical coordinate of the upper-left corner of the
		/// specified component in the current tile.
		/// 
		/// <p>This default implementation returns the value of the source.</p>
		/// 
		/// </summary>
		/// <param name="c">The component index.
		/// 
		/// </param>
		public virtual int getCompULY(int c)
		{
			return imgdatasrc.getCompULY(c);
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
			return imgdatasrc.getNumTiles(co);
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
			return imgdatasrc.getNumTiles();
		}
	}
}