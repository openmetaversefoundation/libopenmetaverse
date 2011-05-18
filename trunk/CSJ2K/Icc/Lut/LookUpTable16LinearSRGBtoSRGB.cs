/// <summary>**************************************************************************
/// 
/// $Id: LookUpTable16LinearSRGBtoSRGB.java,v 1.1 2002/07/25 14:56:47 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>
using System;
using ICCCurveType = CSJ2K.Icc.Tags.ICCCurveType;
namespace CSJ2K.Icc.Lut
{
	
	/// <summary> A Linear 16 bit SRGB to SRGB lut
	/// 
	/// </summary>
	/// <version> 	1.0
	/// </version>
	/// <author> 	Bruce A. Kern
	/// </author>
	public class LookUpTable16LinearSRGBtoSRGB:LookUpTable16
	{
		
		/// <summary> Factory method for creating the lut.</summary>
		/// <param name="wShadowCutoff">size of shadow region
		/// </param>
		/// <param name="dfShadowSlope">shadow region parameter
		/// </param>
		/// <param name="ksRGBLinearMaxValue">size of lut
		/// </param>
		/// <param name="ksRGB8ScaleAfterExp">post shadow region parameter
		/// </param>
		/// <param name="ksRGBExponent">post shadow region parameter
		/// </param>
		/// <param name="ksRGB8ReduceAfterEx">post shadow region parameter
		/// </param>
		/// <returns> the lut
		/// </returns>
		public static LookUpTable16LinearSRGBtoSRGB createInstance(int wShadowCutoff, double dfShadowSlope, int ksRGBLinearMaxValue, double ksRGB8ScaleAfterExp, double ksRGBExponent, double ksRGB8ReduceAfterEx)
		{
			return new LookUpTable16LinearSRGBtoSRGB(wShadowCutoff, dfShadowSlope, ksRGBLinearMaxValue, ksRGB8ScaleAfterExp, ksRGBExponent, ksRGB8ReduceAfterEx);
		}
		
		/// <summary> Construct the lut</summary>
		/// <param name="wShadowCutoff">size of shadow region
		/// </param>
		/// <param name="dfShadowSlope">shadow region parameter
		/// </param>
		/// <param name="ksRGBLinearMaxValue">size of lut
		/// </param>
		/// <param name="ksRGB8ScaleAfterExp">post shadow region parameter
		/// </param>
		/// <param name="ksRGBExponent">post shadow region parameter
		/// </param>
		/// <param name="ksRGB8ReduceAfterExp">post shadow region parameter
		/// </param>
		protected internal LookUpTable16LinearSRGBtoSRGB(int wShadowCutoff, double dfShadowSlope, int ksRGBLinearMaxValue, double ksRGB8ScaleAfterExp, double ksRGBExponent, double ksRGB8ReduceAfterExp):base(ksRGBLinearMaxValue + 1, (short) 0)
		{
			
			int i = - 1;
			//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
			double dfNormalize = 1.0 / (float) ksRGBLinearMaxValue;
			
			// Generate the final linear-sRGB to non-linear sRGB LUT
			for (i = 0; i <= wShadowCutoff; i++)
			{
				//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
				lut[i] = (byte) System.Math.Floor(dfShadowSlope * (double) i + 0.5);
			}
			
			// Now calculate the rest
			for (; i <= ksRGBLinearMaxValue; i++)
			{
				//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
				lut[i] = (byte) System.Math.Floor(ksRGB8ScaleAfterExp * System.Math.Pow((double) i * dfNormalize, ksRGBExponent) - ksRGB8ReduceAfterExp + 0.5);
			}
		}
		
		/* end class LookUpTable16LinearSRGBtoSRGB */
	}
}