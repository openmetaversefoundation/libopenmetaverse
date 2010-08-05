/// <summary>**************************************************************************
/// 
/// $Id: ICCTextDescriptionType.java,v 1.1 2002/07/25 14:56:37 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>
using System;
using ICCProfile = CSJ2K.Icc.ICCProfile;
namespace CSJ2K.Icc.Tags
{
	
	/// <summary> A text based ICC tag
	/// 
	/// </summary>
	/// <version> 	1.0
	/// </version>
	/// <author> 	Bruce A. Kern
	/// </author>
	public class ICCTextDescriptionType:ICCTag
	{
		
		/// <summary>Tag fields </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'type '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		new public int type;
		/// <summary>Tag fields </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'reserved '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		public int reserved;
		/// <summary>Tag fields </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'size '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		public int size;
		/// <summary>Tag fields </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'ascii '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		public byte[] ascii;
		
		/// <summary> Construct this tag from its constituant parts</summary>
		/// <param name="signature">tag id
		/// </param>
		/// <param name="data">array of bytes
		/// </param>
		/// <param name="offset">to data in the data array
		/// </param>
		/// <param name="length">of data in the data array
		/// </param>
		protected internal ICCTextDescriptionType(int signature, byte[] data, int offset, int length):base(signature, data, offset, length)
		{

            type = ICCProfile.getInt(data, offset);
			offset += ICCProfile.int_size;

            reserved = ICCProfile.getInt(data, offset);
			offset += ICCProfile.int_size;

            size = ICCProfile.getInt(data, offset);
			offset += ICCProfile.int_size;
			
			ascii = new byte[size - 1];
            Buffer.BlockCopy(data, offset, ascii, 0, size - 1);
		}
		
		/// <summary>Return the string rep of this tag. </summary>
		public override System.String ToString()
		{
			return "[" + base.ToString() + " \"" + System.Text.ASCIIEncoding.ASCII.GetString(ascii) + "\"]";
		}
		
		/* end class ICCTextDescriptionType */
	}
}