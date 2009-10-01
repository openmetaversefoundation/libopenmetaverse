/// <summary>**************************************************************************
/// 
/// $Id: LookUpTable32LinearSRGBtoSRGB.java,v 1.1 2002/07/25 14:56:47 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>
using System;
using ICCCurveType = CSJ2K.Icc.Tags.ICCCurveType;
namespace CSJ2K.Icc.Lut
{
	
	/// <summary> A Linear 32 bit SRGB to SRGB lut
	/// 
	/// </summary>
	/// <version> 	1.0
	/// </version>
	/// <author> 	Bruce A. Kern
	/// </author>
	public class LookUpTable32LinearSRGBtoSRGB:LookUpTable32
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
		public static LookUpTable32LinearSRGBtoSRGB createInstance(int inMax, int outMax, double shadowCutoff, double shadowSlope, double scaleAfterExp, double exponent, double reduceAfterExp)
		{
			return new LookUpTable32LinearSRGBtoSRGB(inMax, outMax, shadowCutoff, shadowSlope, scaleAfterExp, exponent, reduceAfterExp);
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
		protected internal LookUpTable32LinearSRGBtoSRGB(int inMax, int outMax, double shadowCutoff, double shadowSlope, double scaleAfterExp, double exponent, double reduceAfterExp):base(inMax + 1, outMax)
		{
			
			int i = - 1;
			// Normalization factor for i.
			double normalize = 1.0 / (double) inMax;
			
			// Generate the final linear-sRGB to non-linear sRGB LUT    
			
			// calculate where shadow portion of lut ends.
			//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
			int cutOff = (int) System.Math.Floor(shadowCutoff * inMax);
			
			// Scale to account for output
			shadowSlope *= outMax;
			
			// Our output needs to be centered on zero so we shift it down.
			int shift = (outMax + 1) / 2;
			
			for (i = 0; i <= cutOff; i++)
			{
				//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
				lut[i] = (int) (System.Math.Floor(shadowSlope * (i * normalize) + 0.5) - shift);
			}
			
			// Scale values for output.
			scaleAfterExp *= outMax;
			reduceAfterExp *= outMax;
			
			// Now calculate the rest
			for (; i <= inMax; i++)
			{
				//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
				lut[i] = (int) (System.Math.Floor(scaleAfterExp * System.Math.Pow(i * normalize, exponent) - reduceAfterExp + 0.5) - shift);
			}
		}
		
		public override System.String ToString()
		{
			System.Text.StringBuilder rep = new System.Text.StringBuilder("[LookUpTable32LinearSRGBtoSRGB:");
			return rep.Append("]").ToString();
		}
		
		/* end class LookUpTable32LinearSRGBtoSRGB */
	}
}