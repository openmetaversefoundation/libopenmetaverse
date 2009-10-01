/// <summary>**************************************************************************
/// 
/// $Id: ICCCurveTypeReverse.java,v 1.1 2002/07/25 14:56:36 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>
using System;
using ICCProfile = CSJ2K.Icc.ICCProfile;
namespace CSJ2K.Icc.Tags
{
	
	/// <summary> The ICCCurveReverse tag
	/// 
	/// </summary>
	/// <version> 	1.0
	/// </version>
	/// <author> 	Bruce A. Kern
	/// </author>
	public class ICCCurveTypeReverse:ICCTag
	{
		
		//UPGRADE_NOTE: Final was removed from the declaration of 'eol '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		private static readonly System.String eol = System.Environment.NewLine;
		/// <summary>Tag fields </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'type '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		new public int type;
		/// <summary>Tag fields </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'reserved '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		public int reserved;
		/// <summary>Tag fields </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'nEntries '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		public int nEntries;
		/// <summary>Tag fields </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'entry '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		public int[] entry_Renamed_Field;
		
		
		/// <summary>Return the string rep of this tag. </summary>
		public override System.String ToString()
		{
			System.Text.StringBuilder rep = new System.Text.StringBuilder("[").Append(base.ToString()).Append(eol);
			rep.Append("num entries = " + System.Convert.ToString(nEntries) + eol);
			rep.Append("data length = " + System.Convert.ToString(entry_Renamed_Field.Length) + eol);
			for (int i = 0; i < nEntries; ++i)
				rep.Append(ICCProfile.toHexString(entry_Renamed_Field[i]) + eol);
			return rep.Append("]").ToString();
		}
		
		/// <summary>Normalization utility </summary>
		public static double CurveToDouble(int entry)
		{
			return ICCCurveType.CurveToDouble(entry);
		}
		
		/// <summary>Normalization utility </summary>
		public static short DoubleToCurve(int entry)
		{
			return ICCCurveType.DoubleToCurve(entry);
		}
		
		/// <summary>Normalization utility </summary>
		public static double CurveGammaToDouble(int entry)
		{
			return ICCCurveType.CurveGammaToDouble(entry);
		}
		
		
		/// <summary> Construct this tag from its constituant parts</summary>
		/// <param name="signature">tag id
		/// </param>
		/// <param name="data">array of bytes
		/// </param>
		/// <param name="offset">to data in the data array
		/// </param>
		/// <param name="length">of data in the data array
		/// </param>
		protected internal ICCCurveTypeReverse(int signature, byte[] data, int offset, int length):base(signature, data, offset, offset + 2 * ICCProfile.int_size)
		{
            type = ICCProfile.getInt(data, offset);
            reserved = ICCProfile.getInt(data, offset + ICCProfile.int_size);
            nEntries = ICCProfile.getInt(data, offset + 2 * ICCProfile.int_size);
			entry_Renamed_Field = new int[nEntries];
			for (int i = 0; i < nEntries; ++i)
			// Reverse the storage order.
                entry_Renamed_Field[nEntries - 1 + i] = ICCProfile.getShort(data, offset + 3 * ICCProfile.int_size + i * ICCProfile.short_size) & 0xFFFF;
		}
		
		/// <summary>Accessor for curve entry at index. </summary>
		public int entry(int i)
		{
			return entry_Renamed_Field[i];
		}
		
		/* end class ICCCurveTypeReverse */
	}
}