/*
* CVS identifier:
*
* $Id: CBlkRateDistStats.java,v 1.11 2001/09/14 09:23:51 grosbois Exp $
*
* Class:                   CBlkRateDistStats
*
* Description:             The coded (compressed) code-block with
*                          rate-distortion statistics.
*
*
*
* COPYRIGHT:
* 
* This software module was originally developed by Raphaël Grosbois and
* Diego Santa Cruz (Swiss Federal Institute of Technology-EPFL); Joel
* Askelöf (Ericsson Radio Systems AB); and Bertrand Berthelot, David
* Bouchard, Félix Henry, Gerard Mozelle and Patrice Onno (Canon Research
* Centre France S.A) in the course of development of the JPEG2000
* standard as specified by ISO/IEC 15444 (JPEG 2000 Standard). This
* software module is an implementation of a part of the JPEG 2000
* Standard. Swiss Federal Institute of Technology-EPFL, Ericsson Radio
* Systems AB and Canon Research Centre France S.A (collectively JJ2000
* Partners) agree not to assert against ISO/IEC and users of the JPEG
* 2000 Standard (Users) any of their rights under the copyright, not
* including other intellectual property rights, for this software module
* with respect to the usage by ISO/IEC and Users of this software module
* or modifications thereof for use in hardware or software products
* claiming conformance to the JPEG 2000 Standard. Those intending to use
* this software module in hardware or software products are advised that
* their use may infringe existing patents. The original developers of
* this software module, JJ2000 Partners and ISO/IEC assume no liability
* for use of this software module or modifications thereof. No license
* or right to this software module is granted for non JPEG 2000 Standard
* conforming products. JJ2000 Partners have full right to use this
* software module for his/her own purpose, assign or donate this
* software module to any third party and to inhibit third parties from
* using this software module for non JPEG 2000 Standard conforming
* products. This copyright notice must be included in all copies or
* derivative works of this software module.
* 
* Copyright (c) 1999/2000 JJ2000 Partners.
* */
using System;
using CSJ2K.j2k.wavelet.analysis;
using CSJ2K.j2k.entropy;
namespace CSJ2K.j2k.entropy.encoder
{
	
	/// <summary> This class stores coded (compressed) code-blocks with their associated
	/// rate-distortion statistics. This object should always contain all the
	/// compressed data of the code-block. It is applicable to the encoder engine
	/// only. Some data of the coded-block is stored in the super class, see
	/// CodedCBlk.
	/// 
	/// <p>The rate-distortion statistics (i.e. R-D slope) is stored for valid
	/// points only. The set of valid points is determined by the entropy coder
	/// engine itself. Normally they are selected so as to lye in a convex hull,
	/// which can be achived by using the 'selectConvexHull' method of this class,
	/// but some other strategies might be employed.</p>
	/// 
	/// <p>The rate (in bytes) for each truncation point (valid or not) is stored
	/// in the 'truncRates' array. The rate of a truncation point is the total
	/// number of bytes in 'data' (see super class) that have to be decoded to
	/// reach the truncation point.</p>
	/// 
	/// <p>The slope (reduction of distortion divided by the increase in rate) at
	/// each of the valid truncation points is stored in 'truncSlopes'.</p>
	/// 
	/// <p>The index of each valid truncation point is stored in 'truncIdxs'. The
	/// index should be interpreted in the following way: a valid truncation point
	/// at position 'n' has the index 'truncIdxs[n]', the rate
	/// 'truncRates[truncIdxs[n]]' and the slope 'truncSlopes[n]'. The arrays
	/// 'truncIdxs' and 'truncRates' have at least 'nVldTrunc' elements. The
	/// 'truncRates' array has at least 'nTotTrunc' elements.</p>
	/// 
	/// <p>In addition the 'isTermPass' array contains a flag for each truncation
	/// point (valid and non-valid ones) that tells if the pass is terminated or
	/// not. If this variable is null then it means that no pass is terminated,
	/// except the last one which always is.</p>
	/// 
	/// <p>The compressed data is stored in the 'data' member variable of the super
	/// class.</p>
	/// 
	/// </summary>
	/// <seealso cref="CodedCBlk">
	/// 
	/// </seealso>
	public class CBlkRateDistStats:CodedCBlk
	{
		
		/// <summary>The subband to which the code-block belongs </summary>
		public SubbandAn sb;
		
		/// <summary>The total number of truncation points </summary>
		public int nTotTrunc;
		
		/// <summary>The number of valid truncation points </summary>
		public int nVldTrunc;
		
		/// <summary>The rate (in bytes) for each truncation point (valid and non-valid
		/// ones) 
		/// </summary>
		public int[] truncRates;
		
		/// <summary>The distortion for each truncation point (valid and non-valid ones) </summary>
		public double[] truncDists;
		
		/// <summary>The negative of the rate-distortion slope for each valid truncation
		/// point 
		/// </summary>
		public float[] truncSlopes;
		
		/// <summary>The indices of the valid truncation points, in increasing order.</summary>
		public int[] truncIdxs;
		
		/// <summary>Array of flags indicating terminated passes (valid or non-valid
		/// truncation points). 
		/// </summary>
		public bool[] isTermPass;
		
		/// <summary>The number of ROI coefficients in the code-block </summary>
		public int nROIcoeff = 0;
		
		/// <summary>Number of ROI coding passes </summary>
		public int nROIcp = 0;
		
		/// <summary> Creates a new CBlkRateDistStats object without allocating any space for
		/// 'truncRates', 'truncSlopes', 'truncDists' and 'truncIdxs' or 'data'.
		/// 
		/// </summary>
		public CBlkRateDistStats()
		{
		}
		
		/// <summary> Creates a new CBlkRateDistStats object and initializes the valid
		/// truncation points, their rates and their slopes, from the 'rates' and
		/// 'dist' arrays. The 'rates', 'dist' and 'termp' arrays must contain the
		/// rate (in bytes), the reduction in distortion (from nothing coded) and
		/// the flag indicating if termination is used, respectively, for each
		/// truncation point.
		/// 
		/// <p>The valid truncation points are selected by taking them as lying on
		/// a convex hull. This is done by calling the method
		/// selectConvexHull().</p>
		/// 
		/// <p>Note that the arrays 'rates' and 'termp' are copied, not referenced,
		/// so they can be modified after a call to this constructor.</p>
		/// 
		/// </summary>
		/// <param name="m">The horizontal index of the code-block, within the subband.
		/// 
		/// </param>
		/// <param name="n">The vertical index of the code-block, within the subband.
		/// 
		/// </param>
		/// <param name="skipMSBP">The number of skipped most significant bit-planes for
		/// this code-block.
		/// 
		/// </param>
		/// <param name="data">The compressed data. This array is referenced by this
		/// object so it should not be modified after.
		/// 
		/// </param>
		/// <param name="rates">The rates (in bytes) for each truncation point in the
		/// compressed data. This array is modified by the method but no reference
		/// is kept to it.
		/// 
		/// </param>
		/// <param name="dists">The reduction in distortion (with respect to no
		/// information coded) for each truncation point. This array is modified by
		/// the method but no reference is kept to it.
		/// 
		/// </param>
		/// <param name="termp">An array of boolean flags indicating, for each pass, if a
		/// pass is terminated or not (true if terminated). If null then it is
		/// assumed that no pass is terminated except the last one which always is.
		/// 
		/// </param>
		/// <param name="np">The number of truncation points contained in 'rates', 'dist'
		/// and 'termp'.
		/// 
		/// </param>
		/// <param name="inclast">If false the convex hull is constructed as for lossy
		/// coding. If true it is constructed as for lossless coding, in which case
		/// it is ensured that all bit-planes are sent (i.e. the last truncation
		/// point is always included).
		/// 
		/// </param>
		public CBlkRateDistStats(int m, int n, int skipMSBP, byte[] data, int[] rates, double[] dists, bool[] termp, int np, bool inclast):base(m, n, skipMSBP, data)
		{
			selectConvexHull(rates, dists, termp, np, inclast);
		}
		
		/// <summary> Compute the rate-distorsion slopes and selects those that lie in a
		/// convex hull. It will compute the slopes, select the ones that form the
		/// convex hull and initialize the 'truncIdxs' and 'truncSlopes' arrays, as
		/// well as 'nVldTrunc', with the selected truncation points. It will also
		/// initialize 'truncRates' and 'isTermPass' arrays, as well as
		/// 'nTotTrunc', with all the truncation points (selected or not).
		/// 
		/// <p> Note that the arrays 'rates' and 'termp' are copied, not
		/// referenced, so they can be modified after a call to this method.</p>
		/// 
		/// </summary>
		/// <param name="rates">The rates (in bytes) for each truncation point in the
		/// compressed data. This array is modified by the method.
		/// 
		/// </param>
		/// <param name="dists">The reduction in distortion (with respect to no
		/// information coded) for each truncation point. This array is modified by
		/// the method.
		/// 
		/// </param>
		/// <param name="termp">An array of boolean flags indicating, for each pass, if a
		/// pass is terminated or not (true if terminated). If null then it is
		/// assumed that no pass is terminated except the last one which always is.
		/// 
		/// </param>
		/// <param name="n">The number of truncation points contained in 'rates', 'dist'
		/// and 'termp'.
		/// 
		/// </param>
		/// <param name="inclast">If false the convex hull is constructed as for lossy
		/// coding. If true it is constructed as for lossless coding, in which case
		/// it is ensured that all bit-planes are sent (i.e. the last truncation
		/// point is always included).
		/// 
		/// </param>
		public virtual void  selectConvexHull(int[] rates, double[] dists, bool[] termp, int n, bool inclast)
		{
			int first_pnt; // The first point containing some coded data
			int p; // last selected point
			int k; // current point
			int i; // current valid point
			int npnt; // number of selected (i.e. valid) points
			int delta_rate; // Rate difference
			double delta_dist; // Distortion difference
			float k_slope; // R-D slope for the current point
			float p_slope; // R-D slope for the last selected point
			//int ll_rate; // Rate for "lossless" coding (i.e. all coded info)
			
			// Convention: when a negative value is stored in 'rates' it meas an
			// invalid point. The absolute value is always the rate for that point.
			
			// Look for first point with some coded info (rate not 0)
			first_pnt = 0;
			while (first_pnt < n && rates[first_pnt] <= 0)
			{
				first_pnt++;
			}
			
			// Select the valid points
			npnt = n - first_pnt;
			p_slope = 0f; // To keep compiler happy
			//UPGRADE_NOTE: Label 'ploop' was moved. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1014'"
			do 
			{
				p = - 1;
				for (k = first_pnt; k < n; k++)
				{
					if (rates[k] < 0)
					{
						// Already invalidated point
						continue;
					}
					// Calculate decrease in distortion and rate
					if (p >= 0)
					{
						delta_rate = rates[k] - rates[p];
						delta_dist = dists[k] - dists[p];
					}
					else
					{
						// This is with respect to no info coded
						delta_rate = rates[k];
						delta_dist = dists[k];
					}
					// If exactly same distortion don't eliminate if the rates are 
					// equal, otherwise it can lead to infinite slope in lossless
					// coding.
					if (delta_dist < 0f || (delta_dist == 0f && delta_rate > 0))
					{
						// This point increases distortion => invalidate
						rates[k] = - rates[k];
						npnt--;
						continue; // Goto next point
					}
					//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
					k_slope = (float) (delta_dist / delta_rate);
					// Check that there is a decrease in distortion, slope is not
					// infinite (i.e. delta_dist is not 0) and slope is
					// decreasing.
					if (p >= 0 && (delta_rate <= 0 || k_slope >= p_slope))
					{
						// Last point was not good
						rates[p] = - rates[p]; // Remove p from valid points
						npnt--;
						//UPGRADE_NOTE: Labeled continue statement was changed to a goto statement. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1015'"
						goto ploop; // Restart from the first one
					}
					else
					{
						p_slope = k_slope;
						p = k;
					}
				}
				// If we get to last point we are done
				break;
				//UPGRADE_NOTE: Label 'ploop' was moved. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1014'"

ploop: ;
			}
			while (true); // We end the loop with the break statement
			
			// If in lossless mode make sure we don't eliminate any last
			// bit-planes from being sent.
			if (inclast && n > 0 && rates[n - 1] < 0)
			{
				rates[n - 1] = - rates[n - 1];
				// This rate can never be equal to any previous selected rate,
				// given the selection algorithm above, so no problem arises of
				// infinite slopes.
				npnt++;
			}
			
			// Initialize the arrays of this object
			nTotTrunc = n;
			nVldTrunc = npnt;
			truncRates = new int[n];
			truncDists = new double[n];
			truncSlopes = new float[npnt];
			truncIdxs = new int[npnt];
			if (termp != null)
			{
				isTermPass = new bool[n];
				Array.Copy(termp, 0, isTermPass, 0, n);
			}
			else
			{
				isTermPass = null;
			}
			Array.Copy(rates, 0, truncRates, 0, n);
			for (k = first_pnt, p = - 1, i = 0; k < n; k++)
			{
				if (rates[k] > 0)
				{
					// A valid point
					truncDists[k] = dists[k];
					if (p < 0)
					{
						// Only arrives at first valid point
						//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
						truncSlopes[i] = (float) (dists[k] / rates[k]);
					}
					else
					{
						//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
						truncSlopes[i] = (float) ((dists[k] - dists[p]) / (rates[k] - rates[p]));
					}
					truncIdxs[i] = k;
					i++;
					p = k;
				}
				else
				{
					truncDists[k] = - 1;
					truncRates[k] = - truncRates[k];
				}
			}
		}
		
		/// <summary> Returns the contents of the object in a string. This is used for
		/// debugging.
		/// 
		/// </summary>
		/// <returns> A string with the contents of the object
		/// 
		/// </returns>
		public override System.String ToString()
		{
			System.String str = base.ToString() + "\n nVldTrunc=" + nVldTrunc + ", nTotTrunc=" + nTotTrunc + ", num. ROI" + " coeff=" + nROIcoeff + ", num. ROI coding passes=" + nROIcp + ", sb=" + sb.sbandIdx;
			//          str += "\n\ttruncRates:\n";
			//          for(int i=0; i<truncRates.length; i++) {
			//              str += "\t "+i+": "+truncRates[i]+"\n";
			//          }
			return str;
		}
	}
}