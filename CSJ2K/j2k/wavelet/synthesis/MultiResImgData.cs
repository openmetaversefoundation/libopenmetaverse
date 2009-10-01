/* 
* CVS identifier:
* 
* $Id: MultiResImgData.java,v 1.11 2002/07/25 15:11:33 grosbois Exp $
* 
* Class:                   MultiResImgData
* 
* Description:             The interface for classes that provide
*                          multi-resolution image data.
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
	
	/// <summary> This interface defines methods to access image attributes (width, height,
	/// number of components, etc.) of multiresolution images, such as those
	/// resulting from an inverse wavelet transform. The image can be tiled or not
	/// (i.e. if the image is not tiled then there is only 1 tile). It should be
	/// implemented by all classes that provide multi-resolution image data, such
	/// as entropy decoders, dequantizers, etc. This interface, however, does not
	/// define methods to transfer image data (i.e. pixel data), that is defined by
	/// other interfaces, such as 'CBlkQuantDataSrcDec'.
	/// 
	/// <p>This interface is very similar to the 'ImgData' one. It differs only by
	/// the fact that it handles multiple resolutions.</p>
	/// 
	/// <p>Resolution levels are counted from 0 to L. Resolution level 0 is the
	/// lower resolution, while L is the maximum resolution level, or full
	/// resolution, which is returned by 'getMaxResLvl()'. Note that there are L+1
	/// resolution levels available.</p>
	/// 
	/// <p>As in the 'ImgData' interface a multi-resolution image lies on top of a
	/// canvas. The canvas coordinates are mapped from the full resolution
	/// reference grid (i.e. resolution level 'L' reference grid) to a resolution
	/// level 'l' reference grid by '(x_l,y_l) =
	/// (ceil(x_l/2^(L-l)),ceil(y_l/2^(L-l)))', where '(x,y)' are the full
	/// resolution reference grid coordinates and '(x_l,y_l)' are the level 'l'
	/// reference grid coordinates.</p>
	/// 
	/// <p>For details on the canvas system and its implications consult the
	/// 'ImgData' interface.</p>
	/// 
	/// <p>Note that tile sizes may not be obtained by simply dividing the tile
	/// size in the reference grid by the subsampling factor.</p>
	/// 
	/// </summary>
	/// <seealso cref="jj2000.j2k.image.ImgData">
	/// 
	/// </seealso>
	/// <seealso cref="jj2000.j2k.quantization.dequantizer.CBlkQuantDataSrcDec">
	/// 
	/// </seealso>
	public interface MultiResImgData
	{
		/// <summary>Returns the nominal tiles width </summary>
		int NomTileWidth
		{
			get;
			
		}
		/// <summary>Returns the nominal tiles height </summary>
		int NomTileHeight
		{
			get;
			
		}
		/// <summary> Returns the number of components in the image.
		/// 
		/// </summary>
		/// <returns> The number of components in the image.
		/// 
		/// </returns>
		int NumComps
		{
			get;
			
		}
		/// <summary> Returns the index of the current tile, relative to a standard scan-line
		/// order.
		/// 
		/// </summary>
		/// <returns> The current tile's index (starts at 0).
		/// 
		/// </returns>
		int TileIdx
		{
			get;
			
		}
		/// <summary>Returns the horizontal tile partition offset in the reference grid </summary>
		int TilePartULX
		{
			get;
			
		}
		/// <summary>Returns the vertical tile partition offset in the reference grid </summary>
		int TilePartULY
		{
			get;
			
		}
		
		/// <summary> Returns the overall width of the current tile in pixels for the given
		/// resolution level. This is the tile's width without accounting for any
		/// component subsampling. The resolution level is indexed from the lowest
		/// number of resolution levels of all components of the current tile.
		/// 
		/// </summary>
		/// <param name="rl">The resolution level, from 0 to L.
		/// 
		/// </param>
		/// <returns> The total current tile's width in pixels.
		/// 
		/// </returns>
		int getTileWidth(int rl);
		
		/// <summary> Returns the overall height of the current tile in pixels, for the given
		/// resolution level. This is the tile's height without accounting for any
		/// component subsampling. The resolution level is indexed from the lowest
		/// number of resolution levels of all components of the current tile.
		/// 
		/// </summary>
		/// <param name="rl">The resolution level, from 0 to L.
		/// 
		/// </param>
		/// <returns> The total current tile's height in pixels.
		/// 
		/// </returns>
		int getTileHeight(int rl);
		
		/// <summary> Returns the overall width of the image in pixels, for the given
		/// resolution level. This is the image's width without accounting for any
		/// component subsampling or tiling. The resolution level is indexed from
		/// the lowest number of resolution levels of all components of the current
		/// tile.
		/// 
		/// </summary>
		/// <param name="rl">The resolution level, from 0 to L.
		/// 
		/// </param>
		/// <returns> The total image's width in pixels.
		/// 
		/// </returns>
		int getImgWidth(int rl);
		
		/// <summary> Returns the overall height of the image in pixels, for the given
		/// resolution level. This is the image's height without accounting for any
		/// component subsampling or tiling. The resolution level is indexed from
		/// the lowest number of resolution levels of all components of the current
		/// tile.
		/// 
		/// </summary>
		/// <param name="rl">The resolution level, from 0 to L.
		/// 
		/// </param>
		/// <returns> The total image's height in pixels.
		/// 
		/// </returns>
		int getImgHeight(int rl);
		
		/// <summary> Returns the component subsampling factor in the horizontal direction,
		/// for the specified component. This is, approximately, the ratio of
		/// dimensions between the reference grid and the component itself, see the
		/// 'ImgData' interface desription for details.
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
		int getCompSubsX(int c);
		
		/// <summary> Returns the component subsampling factor in the vertical direction, for
		/// the specified component. This is, approximately, the ratio of
		/// dimensions between the reference grid and the component itself, see the
		/// 'ImgData' interface desription for details.
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
		int getCompSubsY(int c);
		
		/// <summary> Returns the width in pixels of the specified tile-component for the
		/// given resolution level.
		/// 
		/// </summary>
		/// <param name="t">Tile index
		/// 
		/// </param>
		/// <param name="c">The index of the component, from 0 to N-1.
		/// 
		/// </param>
		/// <param name="rl">The resolution level, from 0 to L.
		/// 
		/// </param>
		/// <returns> The width in pixels of component <tt>c</tt> in tile <tt>t</tt>
		/// for resolution <tt>rl</tt>.
		/// 
		/// </returns>
		int getTileCompWidth(int t, int c, int rl);
		
		/// <summary> Returns the height in pixels of the specified tile-component for the
		/// given resolution level.
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
		int getTileCompHeight(int t, int c, int rl);
		
		/// <summary> Returns the width in pixels of the specified component in the overall
		/// image, for the given resolution level.
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
		int getCompImgWidth(int c, int rl);
		
		/// <summary> Returns the height in pixels of the specified component in the overall
		/// image, for the given resolution level.
		/// 
		/// </summary>
		/// <param name="c">The index of the component, from 0 to N-1.
		/// 
		/// </param>
		/// <param name="rl">The resolution level, from 0 to L.
		/// 
		/// </param>
		/// <returns> The height in pixels of component <tt>n</tt> in the overall
		/// image.
		/// 
		/// </returns>
		int getCompImgHeight(int n, int rl);
		
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
		void  setTile(int x, int y);
		
		/// <summary> Advances to the next tile, in standard scan-line order (by rows then
		/// columns). An NoNextElementException is thrown if the current tile is
		/// the last one (i.e. there is no next tile).
		/// 
		/// </summary>
		void  nextTile();
		
		/// <summary> Returns the indexes of the current tile. These are the horizontal and
		/// vertical indexes of the current tile.
		/// 
		/// </summary>
		/// <param name="co">If not null this object is used to return the information. If
		/// null a new one is created and returned.
		/// 
		/// </param>
		/// <returns> The current tile's indexes (vertical and horizontal indexes).
		/// 
		/// </returns>
		Coord getTile(Coord co);
		
		/// <summary> Returns the horizontal coordinate of the upper-left corner of the
		/// specified resolution in the given component of the current tile.
		/// 
		/// </summary>
		/// <param name="c">The component index.
		/// 
		/// </param>
		/// <param name="rl">The resolution level index.
		/// 
		/// </param>
		int getResULX(int c, int rl);
		
		/// <summary> Returns the vertical coordinate of the upper-left corner of the
		/// specified resolution in the given component of the current tile.
		/// 
		/// </summary>
		/// <param name="c">The component index.
		/// 
		/// </param>
		/// <param name="rl">The resolution level index.
		/// 
		/// </param>
		int getResULY(int c, int rl);
		
		/// <summary> Returns the horizontal coordinate of the image origin, the top-left
		/// corner, in the canvas system, on the reference grid at the specified
		/// resolution level.  The resolution level is indexed from the lowest
		/// number of resolution levels of all components of the current tile.
		/// 
		/// </summary>
		/// <param name="rl">The resolution level, from 0 to L.
		/// 
		/// </param>
		/// <returns> The horizontal coordinate of the image origin in the canvas
		/// system, on the reference grid.
		/// 
		/// </returns>
		int getImgULX(int rl);
		
		/// <summary> Returns the vertical coordinate of the image origin, the top-left
		/// corner, in the canvas system, on the reference grid at the specified
		/// resolution level.  The resolution level is indexed from the lowest
		/// number of resolution levels of all components of the current tile.
		/// 
		/// </summary>
		/// <param name="rl">The resolution level, from 0 to L.
		/// 
		/// </param>
		/// <returns> The vertical coordinate of the image origin in the canvas
		/// system, on the reference grid.
		/// 
		/// </returns>
		int getImgULY(int rl);
		
		/// <summary> Returns the number of tiles in the horizontal and vertical directions.
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
		Coord getNumTiles(Coord co);
		
		/// <summary> Returns the total number of tiles in the image.
		/// 
		/// </summary>
		/// <returns> The total number of tiles in the image.
		/// 
		/// </returns>
		int getNumTiles();
		
		/// <summary> Returns the specified synthesis subband tree 
		/// 
		/// </summary>
		/// <param name="t">Tile index.
		/// 
		/// </param>
		/// <param name="c">Component index.
		/// 
		/// </param>
		SubbandSyn getSynSubbandTree(int t, int c);
	}
}