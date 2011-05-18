/// <summary> CVS identifier:
/// 
/// $Id: ImgReader.java,v 1.10 2002/07/25 15:07:44 grosbois Exp $
/// 
/// Class:                   ImgReader
/// 
/// Description:             Generic interface for image readers (from
/// file or other resource)
/// 
/// 
/// 
/// COPYRIGHT:
/// 
/// This software module was originally developed by Raphaël Grosbois and
/// Diego Santa Cruz (Swiss Federal Institute of Technology-EPFL); Joel
/// Askelöf (Ericsson Radio Systems AB); and Bertrand Berthelot, David
/// Bouchard, Félix Henry, Gerard Mozelle and Patrice Onno (Canon Research
/// Centre France S.A) in the course of development of the JPEG2000
/// standard as specified by ISO/IEC 15444 (JPEG 2000 Standard). This
/// software module is an implementation of a part of the JPEG 2000
/// Standard. Swiss Federal Institute of Technology-EPFL, Ericsson Radio
/// Systems AB and Canon Research Centre France S.A (collectively JJ2000
/// Partners) agree not to assert against ISO/IEC and users of the JPEG
/// 2000 Standard (Users) any of their rights under the copyright, not
/// including other intellectual property rights, for this software module
/// with respect to the usage by ISO/IEC and Users of this software module
/// or modifications thereof for use in hardware or software products
/// claiming conformance to the JPEG 2000 Standard. Those intending to use
/// this software module in hardware or software products are advised that
/// their use may infringe existing patents. The original developers of
/// this software module, JJ2000 Partners and ISO/IEC assume no liability
/// for use of this software module or modifications thereof. No license
/// or right to this software module is granted for non JPEG 2000 Standard
/// conforming products. JJ2000 Partners have full right to use this
/// software module for his/her own purpose, assign or donate this
/// software module to any third party and to inhibit third parties from
/// using this software module for non JPEG 2000 Standard conforming
/// products. This copyright notice must be included in all copies or
/// derivative works of this software module.
/// 
/// Copyright (c) 1999/2000 JJ2000 Partners.
/// 
/// </summary>
using System;
using CSJ2K.j2k.image;
using CSJ2K.j2k;
namespace CSJ2K.j2k.image.input
{
	
	/// <summary> This is the generic interface to be implemented by all image file (or other
	/// resource) readers for different image file formats.
	/// 
	/// <p>An ImgReader behaves as an ImgData object. Whenever image data is
	/// requested through the getInternCompData() or getCompData() methods, the
	/// image data will be read (if it is not buffered) and returned. Implementing
	/// classes should not buffer large amounts of data, so as to reduce memory
	/// usage.</p>
	/// 
	/// <p>This class sets the image origin to (0,0). All default implementations
	/// of the methods assume this.</p>
	/// 
	/// <p>This class provides default implementations of many methods. These
	/// default implementations assume that there is no tiling (i.e., the only tile
	/// is the entire image), that the image origin is (0,0) in the canvas system
	/// and that there is no component subsampling (all components are the same
	/// size), but they can be overloaded by the implementating class if need
	/// be.</p>
	/// 
	/// </summary>
	public abstract class ImgReader : BlkImgDataSrc
	{
		/// <summary> Returns the width of the current tile in pixels, assuming there is
		/// no-tiling. Since no-tiling is assumed this is the same as the width of
		/// the image. The value of <tt>w</tt> is returned.
		/// 
		/// </summary>
		/// <returns> The total image width in pixels.
		/// 
		/// </returns>
		virtual public int TileWidth
		{
			get
			{
				return w;
			}
			
		}
		/// <summary> Returns the overall height of the current tile in pixels, assuming
		/// there is no-tiling. Since no-tiling is assumed this is the same as the
		/// width of the image. The value of <tt>h</tt> is returned.
		/// 
		/// </summary>
		/// <returns> The total image height in pixels.  
		/// </returns>
		virtual public int TileHeight
		{
			get
			{
				return h;
			}
			
		}
		/// <summary>Returns the nominal tiles width </summary>
		virtual public int NomTileWidth
		{
			get
			{
				return w;
			}
			
		}
		/// <summary>Returns the nominal tiles height </summary>
		virtual public int NomTileHeight
		{
			get
			{
				return h;
			}
			
		}
		/// <summary> Returns the overall width of the image in pixels. This is the image's
		/// width without accounting for any component subsampling or tiling. The
		/// value of <tt>w</tt> is returned.
		/// 
		/// </summary>
		/// <returns> The total image's width in pixels.
		/// 
		/// </returns>
		virtual public int ImgWidth
		{
			get
			{
				return w;
			}
			
		}
		/// <summary> Returns the overall height of the image in pixels. This is the image's
		/// height without accounting for any component subsampling or tiling. The
		/// value of <tt>h</tt> is returned.
		/// 
		/// </summary>
		/// <returns> The total image's height in pixels.
		/// 
		/// </returns>
		virtual public int ImgHeight
		{
			get
			{
				return h;
			}
			
		}
		/// <summary> Returns the number of components in the image. The value of <tt>nc</tt>
		/// is returned.
		/// 
		/// </summary>
		/// <returns> The number of components in the image.
		/// 
		/// </returns>
		virtual public int NumComps
		{
			get
			{
				return nc;
			}
			
		}
		/// <summary> Returns the index of the current tile, relative to a standard scan-line
		/// order. This default implementations assumes no tiling, so 0 is always
		/// returned.
		/// 
		/// </summary>
		/// <returns> The current tile's index (starts at 0).
		/// 
		/// </returns>
		virtual public int TileIdx
		{
			get
			{
				return 0;
			}
			
		}
		/// <summary>Returns the horizontal tile partition offset in the reference grid </summary>
		virtual public int TilePartULX
		{
			get
			{
				return 0;
			}
			
		}
		/// <summary>Returns the vertical tile partition offset in the reference grid </summary>
		virtual public int TilePartULY
		{
			get
			{
				return 0;
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
		virtual public int ImgULX
		{
			get
			{
				return 0;
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
		virtual public int ImgULY
		{
			get
			{
				return 0;
			}
			
		}
		
		/// <summary>The width of the image </summary>
		protected internal int w;
		
		/// <summary>The height of the image </summary>
		protected internal int h;
		
		/// <summary>The number of components in the image </summary>
		protected internal int nc;
		
		/// <summary> Closes the underlying file or network connection from where the
		/// image data is being read.
		/// 
		/// </summary>
		/// <exception cref="IOException">If an I/O error occurs.
		/// </exception>
		public abstract void  close();
		
		/// <summary> Returns the component subsampling factor in the horizontal direction,
		/// for the specified component. This is, approximately, the ratio of
		/// dimensions between the reference grid and the component itself, see the
		/// 'ImgData' interface desription for details.
		/// 
		/// </summary>
		/// <param name="c">The index of the component (between 0 and C-1)
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
			return 1;
		}
		
		/// <summary> Returns the component subsampling factor in the vertical direction, for
		/// the specified component. This is, approximately, the ratio of
		/// dimensions between the reference grid and the component itself, see the
		/// 'ImgData' interface desription for details.
		/// 
		/// </summary>
		/// <param name="c">The index of the component (between 0 and C-1)
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
			return 1;
		}
		
		/// <summary> Returns the width in pixels of the specified tile-component. This
		/// default implementation assumes no tiling and no component subsampling
		/// (i.e., all components, or components, have the same dimensions in
		/// pixels).
		/// 
		/// </summary>
		/// <param name="t">Tile index
		/// 
		/// </param>
		/// <param name="c">The index of the component, from 0 to C-1.
		/// 
		/// </param>
		/// <returns> The width in pixels of component <tt>c</tt> in tile<tt>t</tt>.
		/// 
		/// </returns>
		public virtual int getTileCompWidth(int t, int c)
		{
			if (t != 0)
			{
				throw new System.ApplicationException("Asking a tile-component width for a tile index" + " greater than 0 whereas there is only one tile");
			}
			return w;
		}
		
		/// <summary> Returns the height in pixels of the specified tile-component. This
		/// default implementation assumes no tiling and no component subsampling
		/// (i.e., all components, or components, have the same dimensions in
		/// pixels).
		/// 
		/// </summary>
		/// <param name="t">The tile index
		/// 
		/// </param>
		/// <param name="c">The index of the component, from 0 to C-1.
		/// 
		/// </param>
		/// <returns> The height in pixels of component <tt>c</tt> in tile
		/// <tt>t</tt>.
		/// 
		/// </returns>
		public virtual int getTileCompHeight(int t, int c)
		{
			if (t != 0)
			{
				throw new System.ApplicationException("Asking a tile-component width for a tile index" + " greater than 0 whereas there is only one tile");
			}
			return h;
		}
		
		/// <summary> Returns the width in pixels of the specified component in the overall
		/// image. This default implementation assumes no component, or component,
		/// subsampling (i.e. all components have the same dimensions in pixels).
		/// 
		/// </summary>
		/// <param name="c">The index of the component, from 0 to C-1.
		/// 
		/// </param>
		/// <returns> The width in pixels of component <tt>c</tt> in the overall
		/// image.
		/// 
		/// </returns>
		public virtual int getCompImgWidth(int c)
		{
			return w;
		}
		
		/// <summary> Returns the height in pixels of the specified component in the overall
		/// image. This default implementation assumes no component, or component,
		/// subsampling (i.e. all components have the same dimensions in pixels).
		/// 
		/// </summary>
		/// <param name="c">The index of the component, from 0 to C-1.
		/// 
		/// </param>
		/// <returns> The height in pixels of component <tt>c</tt> in the overall
		/// image.
		/// 
		/// </returns>
		public virtual int getCompImgHeight(int c)
		{
			return h;
		}
		
		/// <summary> Changes the current tile, given the new coordinates. An
		/// IllegalArgumentException is thrown if the coordinates do not correspond
		/// to a valid tile. This default implementation assumes no tiling so the
		/// only valid arguments are x=0, y=0.
		/// 
		/// </summary>
		/// <param name="x">The horizontal coordinate of the tile.
		/// 
		/// </param>
		/// <param name="y">The vertical coordinate of the new tile.
		/// 
		/// </param>
		public virtual void  setTile(int x, int y)
		{
			if (x != 0 || y != 0)
			{
				throw new System.ArgumentException();
			}
		}
		
		/// <summary> Advances to the next tile, in standard scan-line order (by rows then
		/// columns). A NoNextElementException is thrown if the current tile is the
		/// last one (i.e. there is no next tile). This default implementation
		/// assumes no tiling, so NoNextElementException() is always thrown.
		/// 
		/// </summary>
		public virtual void  nextTile()
		{
			throw new NoNextElementException();
		}
		
		/// <summary> Returns the coordinates of the current tile. This default
		/// implementation assumes no-tiling, so (0,0) is returned.
		/// 
		/// </summary>
		/// <param name="co">If not null this object is used to return the information. If
		/// null a new one is created and returned.
		/// 
		/// </param>
		/// <returns> The current tile's coordinates.
		/// 
		/// </returns>
		public virtual Coord getTile(Coord co)
		{
			if (co != null)
			{
				co.x = 0;
				co.y = 0;
				return co;
			}
			else
			{
				return new Coord(0, 0);
			}
		}
		
		/// <summary> Returns the horizontal coordinate of the upper-left corner of the
		/// specified component in the current tile.
		/// 
		/// </summary>
		/// <param name="c">The component index.
		/// 
		/// </param>
		public virtual int getCompULX(int c)
		{
			return 0;
		}
		
		/// <summary> Returns the vertical coordinate of the upper-left corner of the
		/// specified component in the current tile.
		/// 
		/// </summary>
		/// <param name="c">The component index.
		/// 
		/// </param>
		public virtual int getCompULY(int c)
		{
			return 0;
		}
		
		/// <summary> Returns the number of tiles in the horizontal and vertical
		/// directions. This default implementation assumes no tiling, so (1,1) is
		/// always returned.
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
			if (co != null)
			{
				co.x = 1;
				co.y = 1;
				return co;
			}
			else
			{
				return new Coord(1, 1);
			}
		}
		
		/// <summary> Returns the total number of tiles in the image. This default
		/// implementation assumes no tiling, so 1 is always returned.
		/// 
		/// </summary>
		/// <returns> The total number of tiles in the image.
		/// 
		/// </returns>
		public virtual int getNumTiles()
		{
			return 1;
		}
		
		/// <summary> Returns true if the data read was originally signed in the specified
		/// component, false if not.
		/// 
		/// </summary>
		/// <param name="c">The index of the component, from 0 to C-1.
		/// 
		/// </param>
		/// <returns> true if the data was originally signed, false if not.
		/// 
		/// </returns>
		public abstract bool isOrigSigned(int c);
		public abstract int getFixedPoint(int param1);
		public abstract CSJ2K.j2k.image.DataBlk getInternCompData(CSJ2K.j2k.image.DataBlk param1, int param2);
		public abstract int getNomRangeBits(int param1);
		public abstract CSJ2K.j2k.image.DataBlk getCompData(CSJ2K.j2k.image.DataBlk param1, int param2);
	}
}