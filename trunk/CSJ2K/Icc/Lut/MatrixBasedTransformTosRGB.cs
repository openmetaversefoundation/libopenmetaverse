/// <summary>**************************************************************************
/// 
/// $Id: MatrixBasedTransformTosRGB.java,v 1.1 2002/07/25 14:56:49 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>
using System;
using ColorSpace = CSJ2K.Color.ColorSpace;
using ICCProfile = CSJ2K.Icc.ICCProfile;
using RestrictedICCProfile = CSJ2K.Icc.RestrictedICCProfile;
using ICCXYZType = CSJ2K.Icc.Tags.ICCXYZType;
using DataBlkInt = CSJ2K.j2k.image.DataBlkInt;
using DataBlkFloat = CSJ2K.j2k.image.DataBlkFloat;
namespace CSJ2K.Icc.Lut
{
	
	/// <summary> Transform for applying ICCProfiling to an input DataBlk
	/// 
	/// </summary>
	/// <seealso cref="jj2000.j2k.image.DataBlkInt">
	/// </seealso>
	/// <seealso cref="jj2000.j2k.image.DataBlkFloat">
	/// </seealso>
	/// <version> 	1.0
	/// </version>
	/// <author> 	Bruce A. Kern
	/// </author>
	public class MatrixBasedTransformTosRGB
	{
		
		//UPGRADE_NOTE: Final was removed from the declaration of 'eol '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		private static readonly System.String eol = System.Environment.NewLine;
		
		// Start of contant definitions:
		
		//UPGRADE_NOTE: Final was removed from the declaration of 'RED '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		//UPGRADE_NOTE: The initialization of  'RED' was moved to static method 'icc.lut.MatrixBasedTransformTosRGB'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1005'"
		//UPGRADE_NOTE: Final was removed from the declaration of 'GREEN '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		//UPGRADE_NOTE: The initialization of  'GREEN' was moved to static method 'icc.lut.MatrixBasedTransformTosRGB'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1005'"
		//UPGRADE_NOTE: Final was removed from the declaration of 'BLUE '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		//UPGRADE_NOTE: The initialization of  'BLUE' was moved to static method 'icc.lut.MatrixBasedTransformTosRGB'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1005'"
		private static readonly int RED;
		private static readonly int GREEN;
		private static readonly int BLUE;
		
		private const double SRGB00 = 3.1337;
		private const double SRGB01 = - 1.6173;
		private const double SRGB02 = - 0.4907;
		private const double SRGB10 = - 0.9785;
		private const double SRGB11 = 1.9162;
		private const double SRGB12 = 0.0334;
		private const double SRGB20 = 0.0720;
		private const double SRGB21 = - 0.2290;
		private const double SRGB22 = 1.4056;
		
		// Define constants representing the indices into the matrix array
		private const int M00 = 0;
		private const int M01 = 1;
		private const int M02 = 2;
		private const int M10 = 3;
		private const int M11 = 4;
		private const int M12 = 5;
		private const int M20 = 6;
		private const int M21 = 7;
		private const int M22 = 8;
		
		private const double ksRGBExponent = (1.0 / 2.4);
		private const double ksRGBScaleAfterExp = 1.055;
		private const double ksRGBReduceAfterExp = 0.055;
		private const double ksRGBShadowCutoff = 0.0031308;
		private const double ksRGBShadowSlope = 12.92;
		
		// End of contant definitions:
		
		//UPGRADE_NOTE: Final was removed from the declaration of 'matrix '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		private double[] matrix; // Matrix coefficients 
		
		private LookUpTableFP[] fLut = new LookUpTableFP[3];
		private LookUpTable32LinearSRGBtoSRGB lut; // Linear sRGB to sRGB LUT
		
		//UPGRADE_NOTE: Final was removed from the declaration of 'dwMaxValue '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		private int[] dwMaxValue;
		//UPGRADE_NOTE: Final was removed from the declaration of 'dwShiftValue '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		private int[] dwShiftValue;
		
		//private int dwMaxCols = 0; // Maximum number of columns that can be processed
		//private int dwMaxRows = 0; // Maximum number of rows that can be processed
		
		private float[][] fBuf = null; // Intermediate output of the first LUT operation.
		
		/// <summary> String representation of class</summary>
		/// <returns> suitable representation for class 
		/// </returns>
		public override System.String ToString()
		{
			int i, j;
			
			System.Text.StringBuilder rep = new System.Text.StringBuilder("[MatrixBasedTransformTosRGB: ");
			
			System.Text.StringBuilder body = new System.Text.StringBuilder("  ");
			body.Append(eol).Append("ksRGBExponent= ").Append(System.Convert.ToString(ksRGBExponent));
			body.Append(eol).Append("ksRGBScaleAfterExp= ").Append(System.Convert.ToString(ksRGBScaleAfterExp));
			body.Append(eol).Append("ksRGBReduceAfterExp= ").Append(System.Convert.ToString(ksRGBReduceAfterExp));
			
			
			body.Append(eol).Append("dwMaxValues= ").Append(System.Convert.ToString(dwMaxValue[0])).Append(", ").Append(System.Convert.ToString(dwMaxValue[1])).Append(", ").Append(System.Convert.ToString(dwMaxValue[2]));
			
			body.Append(eol).Append("dwShiftValues= ").Append(System.Convert.ToString(dwShiftValue[0])).Append(", ").Append(System.Convert.ToString(dwShiftValue[1])).Append(", ").Append(System.Convert.ToString(dwShiftValue[2]));
			
			//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Object.toString' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
			body.Append(eol).Append(eol).Append("fLut= ").Append(eol).Append(ColorSpace.indent("  ", "fLut[RED]=  " + fLut[0].ToString())).Append(eol).Append(ColorSpace.indent("  ", "fLut[GRN]=  " + fLut[1].ToString())).Append(eol).Append(ColorSpace.indent("  ", "fLut[BLU]=  " + fLut[2].ToString()));
			
			// Print the matrix
			body.Append(eol).Append(eol).Append("[matrix ");
			for (i = 0; i < 3; ++i)
			{
				body.Append(eol).Append("  ");
				for (j = 0; j < 3; ++j)
				{
					body.Append(matrix[3 * i + j] + "   ");
				}
			}
			body.Append("]");
			
			
			// Print the LinearSRGBtoSRGB lut.
			body.Append(eol).Append(eol).Append(lut.ToString());
			
			rep.Append(ColorSpace.indent("  ", body)).Append("]");
			return rep.Append("]").ToString();
		}
		
		
		/// <summary> Construct a 3 component transform based on an input RestricedICCProfile
		/// This transform will pass the input throught a floating point lut (LookUpTableFP),
		/// apply a matrix to the output and finally pass the intermediate buffer through
		/// a 8-bit lut (LookUpTable8).  This operation will be designated (LFP*M*L8) * Data
		/// The operators (LFP*M*L8) are constructed here.  Although the data for
		/// only one component is returned, the transformation must be done for all
		/// components, because the matrix application involves a linear combination of
		/// component input to produce the output.
		/// </summary>
		/// <param name="ricc">input profile
		/// </param>
		/// <param name="dwMaxValue">clipping value for output.
		/// </param>
		/// <param name="dwMaxCols">number of columns to transform
		/// </param>
		/// <param name="dwMaxRows">number of rows to transform
		/// </param>
		public MatrixBasedTransformTosRGB(RestrictedICCProfile ricc, int[] dwMaxValue, int[] dwShiftValue)
		{
			
			// Assure the proper type profile for this xform.
			if (ricc.Type != RestrictedICCProfile.kThreeCompInput)
				throw new System.ArgumentException("MatrixBasedTransformTosRGB: wrong type ICCProfile supplied");
			
			int c; // component index.
			this.dwMaxValue = dwMaxValue;
			this.dwShiftValue = dwShiftValue;
			
			// Create the LUTFP from the input profile.
			for (c = 0; c < 3; ++c)
			{
				fLut[c] = LookUpTableFP.createInstance(ricc.trc[c], dwMaxValue[c] + 1);
			}
			
			// Create the Input linear to PCS matrix
			matrix = createMatrix(ricc, dwMaxValue); // Create and matrix from the ICC profile.
			
			// Create the final LUT32
			lut = LookUpTable32LinearSRGBtoSRGB.createInstance(dwMaxValue[0], dwMaxValue[0], ksRGBShadowCutoff, ksRGBShadowSlope, ksRGBScaleAfterExp, ksRGBExponent, ksRGBReduceAfterExp);
		}
		
		
		private double[] createMatrix(RestrictedICCProfile ricc, int[] maxValues)
		{
			
			// Coefficients from the input linear to PCS matrix
			double dfPCS00 = ICCXYZType.XYZToDouble(ricc.colorant[RED].x);
			double dfPCS01 = ICCXYZType.XYZToDouble(ricc.colorant[GREEN].x);
			double dfPCS02 = ICCXYZType.XYZToDouble(ricc.colorant[BLUE].x);
			double dfPCS10 = ICCXYZType.XYZToDouble(ricc.colorant[RED].y);
			double dfPCS11 = ICCXYZType.XYZToDouble(ricc.colorant[GREEN].y);
			double dfPCS12 = ICCXYZType.XYZToDouble(ricc.colorant[BLUE].y);
			double dfPCS20 = ICCXYZType.XYZToDouble(ricc.colorant[RED].z);
			double dfPCS21 = ICCXYZType.XYZToDouble(ricc.colorant[GREEN].z);
			double dfPCS22 = ICCXYZType.XYZToDouble(ricc.colorant[BLUE].z);
			
			double[] matrix = new double[9];
			matrix[M00] = maxValues[0] * (SRGB00 * dfPCS00 + SRGB01 * dfPCS10 + SRGB02 * dfPCS20);
			matrix[M01] = maxValues[0] * (SRGB00 * dfPCS01 + SRGB01 * dfPCS11 + SRGB02 * dfPCS21);
			matrix[M02] = maxValues[0] * (SRGB00 * dfPCS02 + SRGB01 * dfPCS12 + SRGB02 * dfPCS22);
			matrix[M10] = maxValues[1] * (SRGB10 * dfPCS00 + SRGB11 * dfPCS10 + SRGB12 * dfPCS20);
			matrix[M11] = maxValues[1] * (SRGB10 * dfPCS01 + SRGB11 * dfPCS11 + SRGB12 * dfPCS21);
			matrix[M12] = maxValues[1] * (SRGB10 * dfPCS02 + SRGB11 * dfPCS12 + SRGB12 * dfPCS22);
			matrix[M20] = maxValues[2] * (SRGB20 * dfPCS00 + SRGB21 * dfPCS10 + SRGB22 * dfPCS20);
			matrix[M21] = maxValues[2] * (SRGB20 * dfPCS01 + SRGB21 * dfPCS11 + SRGB22 * dfPCS21);
			matrix[M22] = maxValues[2] * (SRGB20 * dfPCS02 + SRGB21 * dfPCS12 + SRGB22 * dfPCS22);
			
			return matrix;
		}
		
		
		/// <summary> Performs the transform.  Pass the input throught the LookUpTableFP, apply the
		/// matrix to the output and finally pass the intermediate buffer through the
		/// LookUpTable8.  This operation is designated (LFP*M*L8) * Data are already 
		/// constructed.  Although the data for only one component is returned, the
		/// transformation must be done for all components, because the matrix application
		/// involves a linear combination of component input to produce the output.
		/// </summary>
		/// <param name="ncols">number of columns in the input
		/// </param>
		/// <param name="nrows">number of rows in the input
		/// </param>
		/// <param name="inb">input data block
		/// </param>
		/// <param name="outb">output data block
		/// </param>
		/// <exception cref="MatrixBasedTransformException">
		/// </exception>
		public virtual void  apply(DataBlkInt[] inb, DataBlkInt[] outb)
		{
			int[][] in_Renamed = new int[3][], out_Renamed = new int[3][]; // data references.
			
			int nrows = inb[0].h, ncols = inb[0].w;
			
			if ((fBuf == null) || (fBuf[0].Length < ncols * nrows))
			{
				float[][] tmpArray = new float[3][];
				for (int i = 0; i < 3; i++)
				{
					tmpArray[i] = new float[ncols * nrows];
				}
				fBuf = tmpArray;
			}
			
			// for each component (rgb)
			for (int c = 0; c < 3; ++c)
			{
				
				// Reference the input and output samples.
				in_Renamed[c] = (int[]) inb[c].Data;
				out_Renamed[c] = (int[]) outb[c].Data;
				
				// Assure a properly sized output buffer.
				if (out_Renamed[c] == null || out_Renamed[c].Length < in_Renamed[c].Length)
				{
					out_Renamed[c] = new int[in_Renamed[c].Length];
					outb[c].Data = out_Renamed[c];
				}
				
				// The first thing to do is to process the input into a standard form
				// and through the first input LUT, producing floating point output values
				standardizeMatrixLineThroughLut(inb[c], fBuf[c], dwMaxValue[c], fLut[c]);
			}
			
			// For each row and column
			float[] ra = fBuf[RED];
			float[] ga = fBuf[GREEN];
			float[] ba = fBuf[BLUE];
			
			int[] ro = out_Renamed[RED];
			int[] go = out_Renamed[GREEN];
			int[] bo = out_Renamed[BLUE];
			int[] lut32 = lut.lut;
			
			double r, g, b;
			int val, index = 0;
			for (int y = 0; y < inb[0].h; ++y)
			{
				int end = index + inb[0].w;
				while (index < end)
				{
					// Calculate the rgb pixel indices for this row / column
					r = ra[index];
					g = ga[index];
					b = ba[index];
					
					// Apply the matrix to the intermediate floating point data in order to index the 
					// final LUT.
					//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
					val = (int) (matrix[M00] * r + matrix[M01] * g + matrix[M02] * b + 0.5);
					// Clip the calculated value if necessary..
					if (val < 0)
						ro[index] = lut32[0];
					else if (val >= lut32.Length)
						ro[index] = lut32[lut32.Length - 1];
					else
						ro[index] = lut32[val];
					
					//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
					val = (int) (matrix[M10] * r + matrix[M11] * g + matrix[M12] * b + 0.5);
					// Clip the calculated value if necessary..
					if (val < 0)
						go[index] = lut32[0];
					else if (val >= lut32.Length)
						go[index] = lut32[lut32.Length - 1];
					else
						go[index] = lut32[val];
					
					//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
					val = (int) (matrix[M20] * r + matrix[M21] * g + matrix[M22] * b + 0.5);
					// Clip the calculated value if necessary..
					if (val < 0)
						bo[index] = lut32[0];
					else if (val >= lut32.Length)
						bo[index] = lut32[lut32.Length - 1];
					else
						bo[index] = lut32[val];
					
					index++;
				}
			}
		}
		
		/// <summary> Performs the transform.  Pass the input throught the LookUpTableFP, apply the
		/// matrix to the output and finally pass the intermediate buffer through the
		/// LookUpTable8.  This operation is designated (LFP*M*L8) * Data are already 
		/// constructed.  Although the data for only one component is returned, the
		/// transformation must be done for all components, because the matrix application
		/// involves a linear combination of component input to produce the output.
		/// </summary>
		/// <param name="ncols">number of columns in the input
		/// </param>
		/// <param name="nrows">number of rows in the input
		/// </param>
		/// <param name="inb">input data block
		/// </param>
		/// <param name="outb">output data block
		/// </param>
		/// <exception cref="MatrixBasedTransformException">
		/// </exception>
		public virtual void  apply(DataBlkFloat[] inb, DataBlkFloat[] outb)
		{
			
			float[][] in_Renamed = new float[3][], out_Renamed = new float[3][]; // data references.
			
			int nrows = inb[0].h, ncols = inb[0].w;
			
			if ((fBuf == null) || (fBuf[0].Length < ncols * nrows))
			{
				float[][] tmpArray = new float[3][];
				for (int i = 0; i < 3; i++)
				{
					tmpArray[i] = new float[ncols * nrows];
				}
				fBuf = tmpArray;
			}
			
			// for each component (rgb)
			for (int c = 0; c < 3; ++c)
			{
				
				// Reference the input and output pixels.
				in_Renamed[c] = (float[]) inb[c].Data;
				out_Renamed[c] = (float[]) outb[c].Data;
				
				// Assure a properly sized output buffer.
				if (out_Renamed[c] == null || out_Renamed[c].Length < in_Renamed[c].Length)
				{
					out_Renamed[c] = new float[in_Renamed[c].Length];
					outb[c].Data = out_Renamed[c];
				}
				
				// The first thing to do is to process the input into a standard form
				// and through the first input LUT, producing floating point output values
				standardizeMatrixLineThroughLut(inb[c], fBuf[c], dwMaxValue[c], fLut[c]);
			}
			
			int[] lut32 = lut.lut;
			
			// For each row and column 
			int index = 0, val;
			for (int y = 0; y < inb[0].h; ++y)
			{
				int end = index + inb[0].w;
				while (index < end)
				{
					// Calculate the rgb pixel indices for this row / column
					
					// Apply the matrix to the intermediate floating point data inorder to index the 
					// final LUT.
					//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
					val = (int) (matrix[M00] * fBuf[RED][index] + matrix[M01] * fBuf[GREEN][index] + matrix[M02] * fBuf[BLUE][index] + 0.5);
					// Clip the calculated value if necessary..
					if (val < 0)
						out_Renamed[0][index] = lut32[0];
					else if (val >= lut32.Length)
						out_Renamed[0][index] = lut32[lut32.Length - 1];
					else
						out_Renamed[0][index] = lut32[val];
					
					//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
					val = (int) (matrix[M10] * fBuf[RED][index] + matrix[M11] * fBuf[GREEN][index] + matrix[M12] * fBuf[BLUE][index] + 0.5);
					// Clip the calculated value if necessary..
					if (val < 0)
						out_Renamed[1][index] = lut32[0];
					else if (val >= lut32.Length)
						out_Renamed[1][index] = lut32[lut32.Length - 1];
					else
						out_Renamed[1][index] = lut32[val];
					
					//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
					val = (int) (matrix[M20] * fBuf[RED][index] + matrix[M21] * fBuf[GREEN][index] + matrix[M22] * fBuf[BLUE][index] + 0.5);
					// Clip the calculated value if necessary..
					if (val < 0)
						out_Renamed[2][index] = lut32[0];
					else if (val >= lut32.Length)
						out_Renamed[2][index] = lut32[lut32.Length - 1];
					else
						out_Renamed[2][index] = lut32[val];
					
					index++;
				}
			}
		}
		
		private static void  standardizeMatrixLineThroughLut(DataBlkInt inb, float[] out_Renamed, int dwInputMaxValue, LookUpTableFP lut)
		{
			int wTemp, j = 0;
			int[] in_Renamed = (int[]) inb.Data; // input pixel reference
			float[] lutFP = lut.lut;
			for (int y = inb.uly; y < inb.uly + inb.h; ++y)
			{
				for (int x = inb.ulx; x < inb.ulx + inb.w; ++x)
				{
					int i = inb.offset + (y - inb.uly) * inb.scanw + (x - inb.ulx); // pixel index.
					if (in_Renamed[i] > dwInputMaxValue)
						wTemp = dwInputMaxValue;
					else if (in_Renamed[i] < 0)
						wTemp = 0;
					else
						wTemp = in_Renamed[i];
					out_Renamed[j++] = lutFP[wTemp];
				}
			}
		}
		
		
		private static void  standardizeMatrixLineThroughLut(DataBlkFloat inb, float[] out_Renamed, float dwInputMaxValue, LookUpTableFP lut)
		{
			int j = 0;
			float wTemp;
			float[] in_Renamed = (float[]) inb.Data; // input pixel reference
			float[] lutFP = lut.lut;
			
			for (int y = inb.uly; y < inb.uly + inb.h; ++y)
			{
				for (int x = inb.ulx; x < inb.ulx + inb.w; ++x)
				{
					int i = inb.offset + (y - inb.uly) * inb.scanw + (x - inb.ulx); // pixel index.
					if (in_Renamed[i] > dwInputMaxValue)
						wTemp = dwInputMaxValue;
					else if (in_Renamed[i] < 0)
						wTemp = 0;
					else
						wTemp = in_Renamed[i];
					//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
					out_Renamed[j++] = lutFP[(int) wTemp];
				}
			}
		}
		
		/* end class MatrixBasedTransformTosRGB */
		static MatrixBasedTransformTosRGB()
		{
			RED = ICCProfile.RED;
			GREEN = ICCProfile.GREEN;
			BLUE = ICCProfile.BLUE;
		}
	}
}