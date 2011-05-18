/// <summary>**************************************************************************
/// 
/// $Id: MatrixBasedRestrictedProfile.java,v 1.1 2002/07/25 14:56:56 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>
using System;
using ICCCurveType = CSJ2K.Icc.Tags.ICCCurveType;
using ICCXYZType = CSJ2K.Icc.Tags.ICCXYZType;
namespace CSJ2K.Icc
{
	
	/// <summary> This class is a 3 component RestrictedICCProfile
	/// 
	/// </summary>
	/// <version> 	1.0
	/// </version>
	/// <author> 	Bruce A Kern
	/// </author>
	public class MatrixBasedRestrictedProfile:RestrictedICCProfile
	{
		/// <summary> Get the type of RestrictedICCProfile for this object</summary>
		/// <returns> kThreeCompInput
		/// </returns>
		override public int Type
		{
			get
			{
				return kThreeCompInput;
			}
			
		}
		
		/// <summary> Factory method which returns a 3 component RestrictedICCProfile</summary>
		/// <param name="rcurve">Red TRC curve
		/// </param>
		/// <param name="gcurve">Green TRC curve
		/// </param>
		/// <param name="bcurve">Blue TRC curve
		/// </param>
		/// <param name="rcolorant">Red colorant
		/// </param>
		/// <param name="gcolorant">Green colorant
		/// </param>
		/// <param name="bcolorant">Blue colorant
		/// </param>
		/// <returns> the RestrictedICCProfile
		/// </returns>
		public static new RestrictedICCProfile createInstance(ICCCurveType rcurve, ICCCurveType gcurve, ICCCurveType bcurve, ICCXYZType rcolorant, ICCXYZType gcolorant, ICCXYZType bcolorant)
		{
			return new MatrixBasedRestrictedProfile(rcurve, gcurve, bcurve, rcolorant, gcolorant, bcolorant);
		}
		
		/// <summary> Construct a 3 component RestrictedICCProfile</summary>
		/// <param name="rcurve">Red TRC curve
		/// </param>
		/// <param name="gcurve">Green TRC curve
		/// </param>
		/// <param name="bcurve">Blue TRC curve
		/// </param>
		/// <param name="rcolorant">Red colorant
		/// </param>
		/// <param name="gcolorant">Green colorant
		/// </param>
		/// <param name="bcolorant">Blue colorant
		/// </param>
		protected internal MatrixBasedRestrictedProfile(ICCCurveType rcurve, ICCCurveType gcurve, ICCCurveType bcurve, ICCXYZType rcolorant, ICCXYZType gcolorant, ICCXYZType bcolorant):base(rcurve, gcurve, bcurve, rcolorant, gcolorant, bcolorant)
		{
		}
		
		/// <returns> String representation of a MatrixBasedRestrictedProfile
		/// </returns>
		public override System.String ToString()
		{
			System.Text.StringBuilder rep = new System.Text.StringBuilder("[Matrix-Based Input Restricted ICC profile").Append(eol);
			
			rep.Append("trc[RED]:").Append(eol).Append(trc[RED]).Append(eol);
			rep.Append("trc[RED]:").Append(eol).Append(trc[GREEN]).Append(eol);
			rep.Append("trc[RED]:").Append(eol).Append(trc[BLUE]).Append(eol);
			
			rep.Append("Red colorant:  ").Append(colorant[RED]).Append(eol);
			rep.Append("Red colorant:  ").Append(colorant[GREEN]).Append(eol);
			rep.Append("Red colorant:  ").Append(colorant[BLUE]).Append(eol);
			
			return rep.Append("]").ToString();
		}
		
		/* end class MatrixBasedRestrictedProfile */
	}
}