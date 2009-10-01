/// <summary>**************************************************************************
/// 
/// $Id: ICCXYZTypeReverse.java,v 1.1 2002/07/25 14:56:38 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>
using System;
using ICCProfile = CSJ2K.Icc.ICCProfile;
using XYZNumber = CSJ2K.Icc.Types.XYZNumber;
namespace CSJ2K.Icc.Tags
{
	
	/// <summary> A tag containing a triplet.
	/// 
	/// </summary>
	/// <seealso cref="jj2000.j2k.icc.tags.ICCXYZType">
	/// </seealso>
	/// <seealso cref="jj2000.j2k.icc.types.XYZNumber">
	/// </seealso>
	/// <version> 	1.0
	/// </version>
	/// <author> 	Bruce A. Kern
	/// </author>
	public class ICCXYZTypeReverse:ICCXYZType
	{
		
		/// <summary>x component </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'x '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		new public long x;
		/// <summary>y component </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'y '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		new public long y;
		/// <summary>z component </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'z '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		new public long z;
		
		/// <summary> Construct this tag from its constituant parts</summary>
		/// <param name="signature">tag id
		/// </param>
		/// <param name="data">array of bytes
		/// </param>
		/// <param name="offset">to data in the data array
		/// </param>
		/// <param name="length">of data in the data array
		/// </param>
		protected internal ICCXYZTypeReverse(int signature, byte[] data, int offset, int length):base(signature, data, offset, length)
		{
            z = ICCProfile.getInt(data, offset + 2 * ICCProfile.int_size);
            y = ICCProfile.getInt(data, offset + 3 * ICCProfile.int_size);
            x = ICCProfile.getInt(data, offset + 4 * ICCProfile.int_size);
		}
		
		
		/// <summary>Return the string rep of this tag. </summary>
		public override System.String ToString()
		{
			return "[" + base.ToString() + "(" + x + ", " + y + ", " + z + ")]";
		}
		
		/* end class ICCXYZTypeReverse */
	}
}