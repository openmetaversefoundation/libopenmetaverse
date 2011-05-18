/*
* CVS identifier:
*
* $Id: Tiler.java,v 1.34 2001/09/14 09:16:09 grosbois Exp $
*
* Class:                   Tiler
*
* Description:             An object to create TiledImgData from
*                          ImgData
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
namespace CSJ2K.j2k.image
{
	
	/// <summary> This class places an image in the canvas coordinate system, tiles it, if so
	/// specified, and performs the coordinate conversions transparently. The
	/// source must be a 'BlkImgDataSrc' which is not tiled and has a the image
	/// origin at the canvas origin (i.e. it is not "canvased"), or an exception is
	/// thrown by the constructor. A tiled and "canvased" output is given through
	/// the 'BlkImgDataSrc' interface. See the 'ImgData' interface for a
	/// description of the canvas and tiling.
	/// 
	/// <p>All tiles produced are rectangular, non-overlapping and their union
	/// covers all the image. However, the tiling may not be uniform, depending on
	/// the nominal tile size, tiling origin, component subsampling and other
	/// factors. Therefore it might not be assumed that all tiles are of the same
	/// width and height.</p>
	/// 
	/// <p>The nominal dimension of the tiles is the maximal one, in the reference
	/// grid. All the components of the image have the same number of tiles.</p>
	/// 
	/// </summary>
	/// <seealso cref="ImgData">
	/// </seealso>
	/// <seealso cref="BlkImgDataSrc">
	/// 
	/// </seealso>
	public class Tiler:ImgDataAdapter, BlkImgDataSrc
	{
		/// <summary> Returns the overall width of the current tile in pixels. This is the
		/// tile's width without accounting for any component subsampling.
		/// 
		/// </summary>
		/// <returns> The total current tile width in pixels.
		/// 
		/// </returns>
		override public int TileWidth
		{
			get
			{
				return tileW;
			}
			
		}
		/// <summary> Returns the overall height of the current tile in pixels. This is the
		/// tile's width without accounting for any component subsampling.
		/// 
		/// </summary>
		/// <returns> The total current tile height in pixels.
		/// 
		/// </returns>
		override public int TileHeight
		{
			get
			{
				return tileH;
			}
			
		}
		/// <summary> Returns the index of the current tile, relative to a standard scan-line
		/// order.
		/// 
		/// </summary>
		/// <returns> The current tile's index (starts at 0).
		/// 
		/// </returns>
		override public int TileIdx
		{
			get
			{
				return ty * ntX + tx;
			}
			
		}
		/// <summary>Returns the horizontal tile partition offset in the reference grid </summary>
		override public int TilePartULX
		{
			get
			{
				return xt0siz;
			}
			
		}
		/// <summary>Returns the vertical tile partition offset in the reference grid </summary>
		override public int TilePartULY
		{
			get
			{
				return yt0siz;
			}
			
		}
		/// <summary> Returns the horizontal coordinate of the image origin, the top-left
		/// corner, in the canvas system, on the reference grid.
		/// 
		/// </summary>
		/// <returns> The horizontal coordinate of the image origin in the canvas
		/// system, on the reference grid.
		/// 
		/// </returns>
		override public int ImgULX
		{
			get
			{
				return x0siz;
			}
			
		}
		/// <summary> Returns the vertical coordinate of the image origin, the top-left
		/// corner, in the canvas system, on the reference grid.
		/// 
		/// </summary>
		/// <returns> The vertical coordinate of the image origin in the canvas
		/// system, on the reference grid.
		/// 
		/// </returns>
		override public int ImgULY
		{
			get
			{
				return y0siz;
			}
			
		}
		/// <summary> Returns the nominal width of the tiles in the reference grid.
		/// 
		/// </summary>
		/// <returns> The nominal tile width, in the reference grid.
		/// 
		/// </returns>
		override public int NomTileWidth
		{
			get
			{
				return xtsiz;
			}
			
		}
		/// <summary> Returns the nominal width of the tiles in the reference grid.
		/// 
		/// </summary>
		/// <returns> The nominal tile width, in the reference grid.
		/// 
		/// </returns>
		override public int NomTileHeight
		{
			get
			{
				return ytsiz;
			}
			
		}
		
		/// <summary>The source of image data </summary>
		private BlkImgDataSrc src = null;
		
		/// <summary>Horizontal coordinate of the upper left hand reference grid point.</summary>
		private int x0siz;
		
		/// <summary>Vertical coordinate of the upper left hand reference grid point.</summary>
		private int y0siz;
		
		/// <summary>The horizontal coordinate of the tiling origin in the canvas system,
		/// on the reference grid. 
		/// </summary>
		private int xt0siz;
		
		/// <summary>The vertical coordinate of the tiling origin in the canvas system, on
		/// the reference grid. 
		/// </summary>
		private int yt0siz;
		
		/// <summary>The nominal width of the tiles, on the reference grid. If 0 then there 
		/// is no tiling in that direction. 
		/// </summary>
		private int xtsiz;
		
		/// <summary>The nominal height of the tiles, on the reference grid. If 0 then
		/// there is no tiling in that direction. 
		/// </summary>
		private int ytsiz;
		
		/// <summary>The number of tiles in the horizontal direction. </summary>
		private int ntX;
		
		/// <summary>The number of tiles in the vertical direction. </summary>
		private int ntY;
		
		/// <summary>The component width in the current active tile, for each component </summary>
		private int[] compW = null;
		
		/// <summary>The component height in the current active tile, for each component </summary>
		private int[] compH = null;
		
		/// <summary>The horizontal coordinates of the upper-left corner of the components
		/// in the current tile 
		/// </summary>
		private int[] tcx0 = null;
		
		/// <summary>The vertical coordinates of the upper-left corner of the components in
		/// the current tile. 
		/// </summary>
		private int[] tcy0 = null;
		
		/// <summary>The horizontal index of the current tile </summary>
		private int tx;
		
		/// <summary>The vertical index of the current tile </summary>
		private int ty;
		
		/// <summary>The width of the current tile, on the reference grid. </summary>
		private int tileW;
		
		/// <summary>The height of the current tile, on the reference grid. </summary>
		private int tileH;
		
		/// <summary> Constructs a new tiler with the specified 'BlkImgDataSrc' source,
		/// image origin, tiling origin and nominal tile size.
		/// 
		/// </summary>
		/// <param name="src">The 'BlkImgDataSrc' source from where to get the image
		/// data. It must not be tiled and the image origin must be at '(0,0)' on
		/// its canvas.
		/// 
		/// </param>
		/// <param name="ax">The horizontal coordinate of the image origin in the canvas
		/// system, on the reference grid (i.e. the image's top-left corner in the
		/// reference grid).
		/// 
		/// </param>
		/// <param name="ay">The vertical coordinate of the image origin in the canvas
		/// system, on the reference grid (i.e. the image's top-left corner in the
		/// reference grid).
		/// 
		/// </param>
		/// <param name="px">The horizontal tiling origin, in the canvas system, on the
		/// reference grid. It must satisfy 'px<=ax'.
		/// 
		/// </param>
		/// <param name="py">The vertical tiling origin, in the canvas system, on the
		/// reference grid. It must satisfy 'py<=ay'.
		/// 
		/// </param>
		/// <param name="nw">The nominal tile width, on the reference grid. If 0 then
		/// there is no tiling in that direction.
		/// 
		/// </param>
		/// <param name="nh">The nominal tile height, on the reference grid. If 0 then
		/// there is no tiling in that direction.
		/// 
		/// </param>
		/// <exception cref="IllegalArgumentException">If src is tiled or "canvased", or
		/// if the arguments do not satisfy the specified constraints.
		/// 
		/// </exception>
		public Tiler(BlkImgDataSrc src, int ax, int ay, int px, int py, int nw, int nh):base(src)
		{
			
			// Initialize
			this.src = src;
			this.x0siz = ax;
			this.y0siz = ay;
			this.xt0siz = px;
			this.yt0siz = py;
			this.xtsiz = nw;
			this.ytsiz = nh;
			
			// Verify that input is not tiled
			if (src.getNumTiles() != 1)
			{
				throw new System.ArgumentException("Source is tiled");
			}
			// Verify that source is not "canvased"
			if (src.ImgULX != 0 || src.ImgULY != 0)
			{
				throw new System.ArgumentException("Source is \"canvased\"");
			}
			// Verify that arguments satisfy trivial requirements
			if (x0siz < 0 || y0siz < 0 || xt0siz < 0 || yt0siz < 0 || xtsiz < 0 || ytsiz < 0 || xt0siz > x0siz || yt0siz > y0siz)
			{
				throw new System.ArgumentException("Invalid image origin, " + "tiling origin or nominal " + "tile size");
			}
			
			// If no tiling has been specified, creates a unique tile with maximum
			// dimension.
			if (xtsiz == 0)
				xtsiz = x0siz + src.ImgWidth - xt0siz;
			if (ytsiz == 0)
				ytsiz = y0siz + src.ImgHeight - yt0siz;
			
			// Automatically adjusts xt0siz,yt0siz so that tile (0,0) always
			// overlaps with the image.
			if (x0siz - xt0siz >= xtsiz)
			{
				xt0siz += ((x0siz - xt0siz) / xtsiz) * xtsiz;
			}
			if (y0siz - yt0siz >= ytsiz)
			{
				yt0siz += ((y0siz - yt0siz) / ytsiz) * ytsiz;
			}
			if (x0siz - xt0siz >= xtsiz || y0siz - yt0siz >= ytsiz)
			{
				FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.INFO, "Automatically adjusted tiling " + "origin to equivalent one (" + xt0siz + "," + yt0siz + ") so that " + "first tile overlaps the image");
			}
			
			// Calculate the number of tiles
			//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
			ntX = (int) System.Math.Ceiling((x0siz + src.ImgWidth) / (double) xtsiz);
			//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
			ntY = (int) System.Math.Ceiling((y0siz + src.ImgHeight) / (double) ytsiz);
		}
		
		/// <summary> Returns the width in pixels of the specified tile-component.
		/// 
		/// </summary>
		/// <param name="t">Tile index
		/// 
		/// </param>
		/// <param name="c">The index of the component, from 0 to N-1.
		/// 
		/// </param>
		/// <returns> The width of specified tile-component.
		/// 
		/// </returns>
		public override int getTileCompWidth(int t, int c)
		{
			if (t != TileIdx)
			{
				throw new System.ApplicationException("Asking the width of a tile-component which is " + "not in the current tile (call setTile() or " + "nextTile() methods before).");
			}
			return compW[c];
		}
		
		/// <summary> Returns the height in pixels of the specified tile-component.
		/// 
		/// </summary>
		/// <param name="t">The tile index.
		/// 
		/// </param>
		/// <param name="c">The index of the component, from 0 to N-1.
		/// 
		/// </param>
		/// <returns> The height of specified tile-component.
		/// 
		/// </returns>
		public override int getTileCompHeight(int t, int c)
		{
			if (t != TileIdx)
			{
				throw new System.ApplicationException("Asking the width of a tile-component which is " + "not in the current tile (call setTile() or " + "nextTile() methods before).");
			}
			return compH[c];
		}
		
		/// <summary> Returns the position of the fixed point in the specified
		/// component. This is the position of the least significant integral
		/// (i.e. non-fractional) bit, which is equivalent to the number of
		/// fractional bits. For instance, for fixed-point values with 2 fractional
		/// bits, 2 is returned. For floating-point data this value does not apply
		/// and 0 should be returned. Position 0 is the position of the least
		/// significant bit in the data.
		/// 
		/// </summary>
		/// <param name="c">The index of the component.
		/// 
		/// </param>
		/// <returns> The position of the fixed-point, which is the same as the
		/// number of fractional bits. For floating-point data 0 is returned.
		/// 
		/// </returns>
		public virtual int getFixedPoint(int c)
		{
			return src.getFixedPoint(c);
		}
		
		/// <summary> Returns, in the blk argument, a block of image data containing the
		/// specifed rectangular area, in the specified component. The data is
		/// returned, as a reference to the internal data, if any, instead of as a
		/// copy, therefore the returned data should not be modified.
		/// 
		/// <p>The rectangular area to return is specified by the 'ulx', 'uly', 'w'
		/// and 'h' members of the 'blk' argument, relative to the current
		/// tile. These members are not modified by this method. The 'offset' and
		/// 'scanw' of the returned data can be arbitrary. See the 'DataBlk'
		/// class.</p>
		/// 
		/// <p>This method, in general, is more efficient than the 'getCompData()'
		/// method since it may not copy the data. However if the array of returned
		/// data is to be modified by the caller then the other method is probably
		/// preferable.</p>
		/// 
		/// <p>If the data array in <tt>blk</tt> is <tt>null</tt>, then a new one
		/// is created if necessary. The implementation of this interface may
		/// choose to return the same array or a new one, depending on what is more
		/// efficient. Therefore, the data array in <tt>blk</tt> prior to the
		/// method call should not be considered to contain the returned data, a
		/// new array may have been created. Instead, get the array from
		/// <tt>blk</tt> after the method has returned.</p>
		/// 
		/// <p>The returned data may have its 'progressive' attribute set. In this
		/// case the returned data is only an approximation of the "final"
		/// data.</p>
		/// 
		/// </summary>
		/// <param name="blk">Its coordinates and dimensions specify the area to return,
		/// relative to the current tile. Some fields in this object are modified
		/// to return the data.
		/// 
		/// </param>
		/// <param name="c">The index of the component from which to get the data.
		/// 
		/// </param>
		/// <returns> The requested DataBlk
		/// 
		/// </returns>
		/// <seealso cref="getCompData">
		/// 
		/// </seealso>
		public DataBlk getInternCompData(DataBlk blk, int c)
		{
			// Check that block is inside tile
			if (blk.ulx < 0 || blk.uly < 0 || blk.w > compW[c] || blk.h > compH[c])
			{
				throw new System.ArgumentException("Block is outside the tile");
			}
			// Translate to the sources coordinates
			//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
			int incx = (int) System.Math.Ceiling(x0siz / (double) src.getCompSubsX(c));
			//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
			int incy = (int) System.Math.Ceiling(y0siz / (double) src.getCompSubsY(c));
			blk.ulx -= incx;
			blk.uly -= incy;
			blk = src.getInternCompData(blk, c);
			// Translate back to the tiled coordinates
			blk.ulx += incx;
			blk.uly += incy;
			return blk;
		}
		
		/// <summary> Returns, in the blk argument, a block of image data containing the
		/// specifed rectangular area, in the specified component. The data is
		/// returned, as a copy of the internal data, therefore the returned data
		/// can be modified "in place".
		/// 
		/// <p>The rectangular area to return is specified by the 'ulx', 'uly', 'w'
		/// and 'h' members of the 'blk' argument, relative to the current
		/// tile. These members are not modified by this method. The 'offset' of
		/// the returned data is 0, and the 'scanw' is the same as the block's
		/// width. See the 'DataBlk' class.</p>
		/// 
		/// <p>This method, in general, is less efficient than the
		/// 'getInternCompData()' method since, in general, it copies the
		/// data. However if the array of returned data is to be modified by the
		/// caller then this method is preferable.</p>
		/// 
		/// <p>If the data array in 'blk' is 'null', then a new one is created. If
		/// the data array is not 'null' then it is reused, and it must be large
		/// enough to contain the block's data. Otherwise an 'ArrayStoreException'
		/// or an 'IndexOutOfBoundsException' is thrown by the Java system.</p>
		/// 
		/// <p>The returned data may have its 'progressive' attribute set. In this
		/// case the returned data is only an approximation of the "final"
		/// data.</p>
		/// 
		/// </summary>
		/// <param name="blk">Its coordinates and dimensions specify the area to return,
		/// relative to the current tile. If it contains a non-null data array,
		/// then it must be large enough. If it contains a null data array a new
		/// one is created. Some fields in this object are modified to return the
		/// data.
		/// 
		/// </param>
		/// <param name="c">The index of the component from which to get the data.
		/// 
		/// </param>
		/// <returns> The requested DataBlk
		/// 
		/// </returns>
		/// <seealso cref="getInternCompData">
		/// 
		/// </seealso>
		public DataBlk getCompData(DataBlk blk, int c)
		{
			// Check that block is inside tile
			if (blk.ulx < 0 || blk.uly < 0 || blk.w > compW[c] || blk.h > compH[c])
			{
				throw new System.ArgumentException("Block is outside the tile");
			}
			// Translate to the source's coordinates
			//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
			int incx = (int) System.Math.Ceiling(x0siz / (double) src.getCompSubsX(c));
			//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
			int incy = (int) System.Math.Ceiling(y0siz / (double) src.getCompSubsY(c));
			blk.ulx -= incx;
			blk.uly -= incy;
			blk = src.getCompData(blk, c);
			// Translate back to the tiled coordinates
			blk.ulx += incx;
			blk.uly += incy;
			return blk;
		}
		
		/// <summary> Changes the current tile, given the new tile indexes. An
		/// IllegalArgumentException is thrown if the coordinates do not correspond
		/// to a valid tile.
		/// 
		/// </summary>
		/// <param name="x">The horizontal index of the tile.
		/// 
		/// </param>
		/// <param name="y">The vertical index of the new tile.
		/// 
		/// </param>
		public override void  setTile(int x, int y)
		{
			// Check tile indexes
			if (x < 0 || y < 0 || x >= ntX || y >= ntY)
			{
				throw new System.ArgumentException("Tile's indexes out of bounds");
			}
			
			// Set new current tile
			tx = x;
			ty = y;
			// Calculate tile origins
			int tx0 = (x != 0)?xt0siz + x * xtsiz:x0siz;
			int ty0 = (y != 0)?yt0siz + y * ytsiz:y0siz;
			int tx1 = (x != ntX - 1)?(xt0siz + (x + 1) * xtsiz):(x0siz + src.ImgWidth);
			int ty1 = (y != ntY - 1)?(yt0siz + (y + 1) * ytsiz):(y0siz + src.ImgHeight);
			// Set general variables
			tileW = tx1 - tx0;
			tileH = ty1 - ty0;
			// Set component specific variables
			int nc = src.NumComps;
			if (compW == null)
				compW = new int[nc];
			if (compH == null)
				compH = new int[nc];
			if (tcx0 == null)
				tcx0 = new int[nc];
			if (tcy0 == null)
				tcy0 = new int[nc];
			for (int i = 0; i < nc; i++)
			{
				//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
				tcx0[i] = (int) System.Math.Ceiling(tx0 / (double) src.getCompSubsX(i));
				//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
				tcy0[i] = (int) System.Math.Ceiling(ty0 / (double) src.getCompSubsY(i));
				//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
				compW[i] = (int) System.Math.Ceiling(tx1 / (double) src.getCompSubsX(i)) - tcx0[i];
				//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
				compH[i] = (int) System.Math.Ceiling(ty1 / (double) src.getCompSubsY(i)) - tcy0[i];
			}
		}
		
		/// <summary> Advances to the next tile, in standard scan-line order (by rows then
		/// columns). An NoNextElementException is thrown if the current tile is
		/// the last one (i.e. there is no next tile).
		/// 
		/// </summary>
		public override void  nextTile()
		{
			if (tx == ntX - 1 && ty == ntY - 1)
			{
				// Already at last tile
				throw new NoNextElementException();
			}
			else if (tx < ntX - 1)
			{
				// If not at end of current tile line
				setTile(tx + 1, ty);
			}
			else
			{
				// First tile at next line
				setTile(0, ty + 1);
			}
		}
		
		/// <summary> Returns the horizontal and vertical indexes of the current tile.
		/// 
		/// </summary>
		/// <param name="co">If not null this object is used to return the
		/// information. If null a new one is created and returned.
		/// 
		/// </param>
		/// <returns> The current tile's horizontal and vertical indexes..
		/// 
		/// </returns>
		public override Coord getTile(Coord co)
		{
			if (co != null)
			{
				co.x = tx;
				co.y = ty;
				return co;
			}
			else
			{
				return new Coord(tx, ty);
			}
		}
		
		/// <summary> Returns the horizontal coordinate of the upper-left corner of the
		/// specified component in the current tile.
		/// 
		/// </summary>
		/// <param name="c">The component index.
		/// 
		/// </param>
		public override int getCompULX(int c)
		{
			return tcx0[c];
		}
		
		/// <summary> Returns the vertical coordinate of the upper-left corner of the
		/// specified component in the current tile.
		/// 
		/// </summary>
		/// <param name="c">The component index.
		/// 
		/// </param>
		public override int getCompULY(int c)
		{
			return tcy0[c];
		}
		
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
		public override Coord getNumTiles(Coord co)
		{
			if (co != null)
			{
				co.x = ntX;
				co.y = ntY;
				return co;
			}
			else
			{
				return new Coord(ntX, ntY);
			}
		}
		
		/// <summary> Returns the total number of tiles in the image.
		/// 
		/// </summary>
		/// <returns> The total number of tiles in the image.
		/// 
		/// </returns>
		public override int getNumTiles()
		{
			return ntX * ntY;
		}
		
		/// <summary> Returns the tiling origin, referred to as '(xt0siz,yt0siz)' in the
		/// codestream header (SIZ marker segment).
		/// 
		/// </summary>
		/// <param name="co">If not null this object is used to return the information. If
		/// null a new one is created and returned.
		/// 
		/// </param>
		/// <returns> The coordinate of the tiling origin, in the canvas system, on
		/// the reference grid.
		/// 
		/// </returns>
		/// <seealso cref="ImgData">
		/// 
		/// </seealso>
		public Coord getTilingOrigin(Coord co)
		{
			if (co != null)
			{
				co.x = xt0siz;
				co.y = yt0siz;
				return co;
			}
			else
			{
				return new Coord(xt0siz, yt0siz);
			}
		}
		
		/// <summary> Returns a String object representing Tiler's informations
		/// 
		/// </summary>
		/// <returns> Tiler's infos in a string
		/// 
		/// </returns>
		public override System.String ToString()
		{
			//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Object.toString' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
			return "Tiler: source= " + src + "\n" + getNumTiles() + " tile(s), nominal width=" + xtsiz + ", nominal height=" + ytsiz;
		}
	}
}