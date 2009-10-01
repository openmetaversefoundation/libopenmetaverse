/// <summary>**************************************************************************
/// 
/// $Id: LookUpTable.java,v 1.1 2002/07/25 14:56:49 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>
using System;
using ICCCurveType = CSJ2K.Icc.Tags.ICCCurveType;
namespace CSJ2K.Icc.Lut
{
	
	
	/// <summary> Toplevel class for a lut.  All lookup tables must
	/// extend this class.
	/// 
	/// </summary>
	/// <version> 	1.0
	/// </version>
	/// <author> 	Bruce A. Kern
	/// </author>
	public abstract class LookUpTable
	{
		
		/// <summary>End of line string.             </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'eol '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		protected internal static readonly System.String eol = System.Environment.NewLine;
		/// <summary>The curve data                  </summary>
		protected internal ICCCurveType curve = null;
		/// <summary>Number of values in created lut </summary>
		protected internal int dwNumInput = 0;
		
		
		/// <summary> For subclass usage.</summary>
		/// <param name="curve">The curve data  
		/// </param>
		/// <param name="dwNumInput">Number of values in created lut
		/// </param>
		protected internal LookUpTable(ICCCurveType curve, int dwNumInput)
		{
			this.curve = curve;
			this.dwNumInput = dwNumInput;
		}
		
		/* end class LookUpTable */
	}
}