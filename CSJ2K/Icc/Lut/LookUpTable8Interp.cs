/// <summary>**************************************************************************
/// 
/// $Id: LookUpTable8Interp.java,v 1.1 2002/07/25 14:56:48 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>
using System;
using ICCCurveType = CSJ2K.Icc.Tags.ICCCurveType;
namespace CSJ2K.Icc.Lut
{
	
	
	/// <summary> An interpolated 8 bit lut
	/// 
	/// </summary>
	/// <version> 	1.0
	/// </version>
	/// <author> 	Bruce A.Kern
	/// </author>
	public class LookUpTable8Interp:LookUpTable8
	{
		
		/// <summary> Construct the lut from the curve data</summary>
		/// <oaram>   curve the data </oaram>
		/// <oaram>   dwNumInput the lut size </oaram>
		/// <oaram>   dwMaxOutput the lut max value </oaram>
		public LookUpTable8Interp(ICCCurveType curve, int dwNumInput, byte dwMaxOutput):base(curve, dwNumInput, dwMaxOutput)
		{
			
			int dwLowIndex, dwHighIndex; // Indices of interpolation points
			double dfLowIndex, dfHighIndex; // FP indices of interpolation points
			double dfTargetIndex; // Target index into interpolation table
			double dfRatio; // Ratio of LUT input points to curve values
			double dfLow, dfHigh; // Interpolation values
			double dfOut; // Output LUT value
			
			dfRatio = (double) (curve.count - 1) / (double) (dwNumInput - 1);
			
			for (int i = 0; i < dwNumInput; i++)
			{
				dfTargetIndex = (double) i * dfRatio;
				dfLowIndex = System.Math.Floor(dfTargetIndex);
				//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
				dwLowIndex = (int) dfLowIndex;
				dfHighIndex = System.Math.Ceiling(dfTargetIndex);
				//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
				dwHighIndex = (int) dfHighIndex;
				
				if (dwLowIndex == dwHighIndex)
					dfOut = ICCCurveType.CurveToDouble(curve.entry(dwLowIndex));
				else
				{
					dfLow = ICCCurveType.CurveToDouble(curve.entry(dwLowIndex));
					dfHigh = ICCCurveType.CurveToDouble(curve.entry(dwHighIndex));
					dfOut = dfLow + (dfHigh - dfLow) * (dfTargetIndex - dfLowIndex);
				}
				
				//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
				lut[i] = (byte) System.Math.Floor(dfOut * dwMaxOutput + 0.5);
			}
		}
		
		/* end class LookUpTable8Interp */
	}
}