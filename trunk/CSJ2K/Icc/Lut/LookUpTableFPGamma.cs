/// <summary>**************************************************************************
/// 
/// $Id: LookUpTableFPGamma.java,v 1.1 2002/07/25 14:56:48 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>
using System;
using ICCCurveType = CSJ2K.Icc.Tags.ICCCurveType;
namespace CSJ2K.Icc.Lut
{
	
	/// <summary> Class Description
	/// 
	/// </summary>
	/// <version> 	1.0
	/// </version>
	/// <author> 	Bruce A. Kern
	/// </author>
	
	public class LookUpTableFPGamma:LookUpTableFP
	{
		
		internal double dfE = - 1;
		//UPGRADE_NOTE: Final was removed from the declaration of 'eol '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		new private static readonly System.String eol = System.Environment.NewLine;
		
		public LookUpTableFPGamma(ICCCurveType curve, int dwNumInput):base(curve, dwNumInput)
		{
			
			// Gamma exponent for inverse transformation
			dfE = ICCCurveType.CurveGammaToDouble(curve.entry(0));
			for (int i = 0; i < dwNumInput; i++)
			{
				//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
				lut[i] = (float) System.Math.Pow((double) i / (dwNumInput - 1), dfE);
			}
		}
		
		/// <summary> Create an abbreviated string representation of a 16 bit lut.</summary>
		/// <returns> the lut as a String
		/// </returns>
		public override System.String ToString()
		{
			System.Text.StringBuilder rep = new System.Text.StringBuilder("[LookUpTableGamma ");
			//int row, col;
			rep.Append("dfe= " + dfE);
			rep.Append(", nentries= " + lut.Length);
			return rep.Append("]").ToString();
		}
		
		
		/* end class LookUpTableFPGamma */
	}
}