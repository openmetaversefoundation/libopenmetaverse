/// <summary>**************************************************************************
/// 
/// $Id: ImageHeaderBox.java,v 1.1 2002/07/25 14:50:47 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>
using System;
using ColorSpaceException = CSJ2K.Color.ColorSpaceException;
using ParameterList = CSJ2K.j2k.util.ParameterList;
using RandomAccessIO = CSJ2K.j2k.io.RandomAccessIO;
using ICCProfile = CSJ2K.Icc.ICCProfile;
namespace CSJ2K.Color.Boxes
{
	
	/// <summary> This class models the Image Header box contained in a JP2
	/// image.  It is a stub class here since for colormapping the
	/// knowlege of the existance of the box in the image is sufficient.
	/// 
	/// </summary>
	/// <version> 	1.0
	/// </version>
	/// <author> 	Bruce A. Kern
	/// </author>
	public sealed class ImageHeaderBox:JP2Box
	{
		
		internal long height;
		internal long width;
		internal int nc;
		internal short bpc;
		internal short c;
		internal bool unk;
		internal bool ipr;
		
		
		/// <summary> Construct an ImageHeaderBox from an input image.</summary>
		/// <param name="in">RandomAccessIO jp2 image
		/// </param>
		/// <param name="boxStart">offset to the start of the box in the image
		/// </param>
		/// <exception cref="IOException,">ColorSpaceException
		/// </exception>
		public ImageHeaderBox(RandomAccessIO in_Renamed, int boxStart):base(in_Renamed, boxStart)
		{
			readBox();
		}
		
		/// <summary>Return a suitable String representation of the class instance. </summary>
		public override System.String ToString()
		{
			System.Text.StringBuilder rep = new System.Text.StringBuilder("[ImageHeaderBox ").Append(eol).Append("  ");
			rep.Append("height= ").Append(System.Convert.ToString(height)).Append(", ");
			rep.Append("width= ").Append(System.Convert.ToString(width)).Append(eol).Append("  ");
			
			rep.Append("nc= ").Append(System.Convert.ToString(nc)).Append(", ");
			rep.Append("bpc= ").Append(System.Convert.ToString(bpc)).Append(", ");
			rep.Append("c= ").Append(System.Convert.ToString(c)).Append(eol).Append("  ");
			
			rep.Append("image colorspace is ").Append(new System.Text.StringBuilder(unk == true?"known":"unknown").ToString());
			rep.Append(", the image ").Append(new System.Text.StringBuilder(ipr == true?"contains ":"does not contain ").ToString()).Append("intellectual property").Append("]");
			
			return rep.ToString();
		}
		
		/// <summary>Analyze the box content. </summary>
		internal void  readBox()
		{
			byte[] bfr = new byte[14];
			in_Renamed.seek(dataStart);
			in_Renamed.readFully(bfr, 0, 14);

            height = ICCProfile.getInt(bfr, 0);
            width = ICCProfile.getInt(bfr, 4);
            nc = ICCProfile.getShort(bfr, 8);
			bpc = (short) (bfr[10] & 0x00ff);
			c = (short) (bfr[11] & 0x00ff);
			unk = bfr[12] == 0?true:false;
			ipr = bfr[13] == 1?true:false;
		}
		
		/* end class ImageHeaderBox */
		static ImageHeaderBox()
		{
			{
				type = 69686472;
			}
		}
	}
}