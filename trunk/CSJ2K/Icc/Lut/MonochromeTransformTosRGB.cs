/// <summary>**************************************************************************
/// 
/// $Id: MonochromeTransformTosRGB.java,v 1.1 2002/07/25 14:56:50 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>
using System;
using ColorSpace = CSJ2K.Color.ColorSpace;
using ICCProfile = CSJ2K.Icc.ICCProfile;
using RestrictedICCProfile = CSJ2K.Icc.RestrictedICCProfile;
using DataBlkInt = CSJ2K.j2k.image.DataBlkInt;
using DataBlkFloat = CSJ2K.j2k.image.DataBlkFloat;
namespace CSJ2K.Icc.Lut
{
	
	/// <summary> 
	/// This class constructs a LookUpTableFP from a RestrictedICCProfile.
	/// The values in this table are used to calculate a second lookup table (simply a short []).  
	/// table.  When this transform is applied to an input DataBlk, an output data block is
	/// constructed by using the input samples as indices into the lookup table, whose values
	/// are used to populate the output DataBlk.
	/// 
	/// </summary>
	/// <seealso cref="jj2000.j2k.icc.RestrictedICCProfile">
	/// </seealso>
	/// <seealso cref="jj2000.j2k.icc.lut.LookUpTableFP">
	/// </seealso>
	/// <version> 	1.0
	/// </version>
	/// <author> 	Bruce A. Kern
	/// </author>
	
	public class MonochromeTransformTosRGB
	{
		
		//UPGRADE_NOTE: Final was removed from the declaration of 'eol '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		private static readonly System.String eol = System.Environment.NewLine;
		
		/// <summary>Transform parameter. </summary>
		public const double ksRGBShadowCutoff = 0.0031308;
		/// <summary>Transform parameter. </summary>
		public const double ksRGBShadowSlope = 12.92;
		/// <summary>Transform parameter. </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'ksRGB8ShadowSlope '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		public static readonly double ksRGB8ShadowSlope = (255 * ksRGBShadowSlope);
		/// <summary>Transform parameter. </summary>
		public const double ksRGBExponent = (1.0 / 2.4);
		/// <summary>Transform parameter. </summary>
		public const double ksRGB8ScaleAfterExp = 269.025;
		/// <summary>Transform parameter. </summary>
		public const double ksRGB8ReduceAfterExp = 14.025;
		
		private short[] lut = null;
		private int dwInputMaxValue = 0;
		private LookUpTableFP fLut = null;
		
		
		/// <summary> String representation of class</summary>
		/// <returns> suitable representation for class 
		/// </returns>
		public override System.String ToString()
		{
			System.Text.StringBuilder rep = new System.Text.StringBuilder("[MonochromeTransformTosRGB ");
			System.Text.StringBuilder body = new System.Text.StringBuilder("  ");
			
			// Print the parameters:
			body.Append(eol).Append("ksRGBShadowSlope= ").Append(System.Convert.ToString(ksRGBShadowSlope));
			body.Append(eol).Append("ksRGBShadowCutoff= ").Append(System.Convert.ToString(ksRGBShadowCutoff));
			body.Append(eol).Append("ksRGBShadowSlope= ").Append(System.Convert.ToString(ksRGBShadowSlope));
			body.Append(eol).Append("ksRGB8ShadowSlope= ").Append(System.Convert.ToString(ksRGB8ShadowSlope));
			body.Append(eol).Append("ksRGBExponent= ").Append(System.Convert.ToString(ksRGBExponent));
			body.Append(eol).Append("ksRGB8ScaleAfterExp= ").Append(System.Convert.ToString(ksRGB8ScaleAfterExp));
			body.Append(eol).Append("ksRGB8ReduceAfterExp= ").Append(System.Convert.ToString((ksRGB8ReduceAfterExp)));
			body.Append(eol).Append("dwInputMaxValue= ").Append(System.Convert.ToString(dwInputMaxValue));
			
			// Print the LinearSRGBtoSRGB lut.
			body.Append(eol).Append("[lut = [short[" + lut.Length + "]]]");
			
			// Print the FP luts.
			//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Object.toString' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
			body.Append(eol).Append("fLut=  " + fLut.ToString());
			
			rep.Append(ColorSpace.indent("  ", body));
			return rep.Append("]").ToString();
		}
		
		/// <summary> Construct the lut from the RestrictedICCProfile.
		/// 
		/// </summary>
		/// <param name="ricc">input RestrictedICCProfile
		/// </param>
		/// <param name="dwInputMaxValue">size of the output lut.
		/// </param>
		/// <param name="dwInputShiftValue">value used to shift samples to positive
		/// </param>
		public MonochromeTransformTosRGB(RestrictedICCProfile ricc, int dwInputMaxValue, int dwInputShiftValue)
		{
			
			if (ricc.Type != RestrictedICCProfile.kMonochromeInput)
				throw new System.ArgumentException("MonochromeTransformTosRGB: wrong type ICCProfile supplied");
			
			this.dwInputMaxValue = dwInputMaxValue;
			lut = new short[dwInputMaxValue + 1];
			fLut = LookUpTableFP.createInstance(ricc.trc[ICCProfile.GRAY], dwInputMaxValue + 1);
			
			// First calculate the value for the shadow region
			int i;
			for (i = 0; ((i <= dwInputMaxValue) && (fLut.lut[i] <= ksRGBShadowCutoff)); i++)
			{
				//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
				lut[i] = (short) (System.Math.Floor(ksRGB8ShadowSlope * (double) fLut.lut[i] + 0.5) - dwInputShiftValue);
			}
			
			// Now calculate the rest   
			for (; i <= dwInputMaxValue; i++)
			{
				//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
				lut[i] = (short) (System.Math.Floor(ksRGB8ScaleAfterExp * System.Math.Pow((double) fLut.lut[i], ksRGBExponent) - ksRGB8ReduceAfterExp + 0.5) - dwInputShiftValue);
			}
		}
		
		/// <summary> Populate the output block by looking up the values in the lut, using the input
		/// as lut indices.
		/// </summary>
		/// <param name="inb">input samples
		/// </param>
		/// <param name="outb">output samples.
		/// </param>
		/// <exception cref="MonochromeTransformException">
		/// </exception>
		public virtual void  apply(DataBlkInt inb, DataBlkInt outb)
		{

            int i, j, o; //  x, y removed
			
			int[] in_Renamed = (int[]) inb.Data;
			int[] out_Renamed = (int[]) outb.Data;
			
			if (out_Renamed == null || out_Renamed.Length < in_Renamed.Length)
			{
				out_Renamed = new int[in_Renamed.Length];
				outb.Data = out_Renamed;
			}
			
			outb.uly = inb.uly;
			outb.ulx = inb.ulx;
			outb.h = inb.h;
			outb.w = inb.w;
			outb.offset = inb.offset;
			outb.scanw = inb.scanw;
			
			o = inb.offset;
			for (i = 0; i < inb.h * inb.w; ++i)
			{
				j = in_Renamed[i];
				if (j < 0)
					j = 0;
				else if (j > dwInputMaxValue)
					j = dwInputMaxValue;
				out_Renamed[i] = lut[j];
			}
		}
		
		/// <summary> Populate the output block by looking up the values in the lut, using the input
		/// as lut indices.
		/// </summary>
		/// <param name="inb">input samples
		/// </param>
		/// <param name="outb">output samples.
		/// </param>
		/// <exception cref="MonochromeTransformException">
		/// </exception>
		public virtual void  apply(DataBlkFloat inb, DataBlkFloat outb)
		{

            int i, j, o; // x, y removed
			
			float[] in_Renamed = (float[]) inb.Data;
			float[] out_Renamed = (float[]) outb.Data;
			
			
			if (out_Renamed == null || out_Renamed.Length < in_Renamed.Length)
			{
				out_Renamed = new float[in_Renamed.Length];
				outb.Data = out_Renamed;
				
				outb.uly = inb.uly;
				outb.ulx = inb.ulx;
				outb.h = inb.h;
				outb.w = inb.w;
				outb.offset = inb.offset;
				outb.scanw = inb.scanw;
			}
			
			o = inb.offset;
			for (i = 0; i < inb.h * inb.w; ++i)
			{
				//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
				j = (int) in_Renamed[i];
				if (j < 0)
					j = 0;
				else if (j > dwInputMaxValue)
					j = dwInputMaxValue;
				out_Renamed[i] = lut[j];
			}
		}
		
		
		/* end class MonochromeTransformTosRGB */
	}
}