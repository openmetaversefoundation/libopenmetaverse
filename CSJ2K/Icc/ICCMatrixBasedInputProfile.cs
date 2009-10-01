/// <summary>**************************************************************************
/// 
/// $Id: ICCMatrixBasedInputProfile.java,v 1.1 2002/07/25 14:56:54 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>
using System;
using ColorSpace = CSJ2K.Color.ColorSpace;
using ColorSpaceException = CSJ2K.Color.ColorSpaceException;
using RandomAccessIO = CSJ2K.j2k.io.RandomAccessIO;
namespace CSJ2K.Icc
{
	
	/// <summary> This class enables an application to construct an 3 component ICCProfile
	/// 
	/// </summary>
	/// <version> 	1.0
	/// </version>
	/// <author> 	Bruce A. Kern
	/// </author>
	
	public class ICCMatrixBasedInputProfile:ICCProfile
	{
		
		/// <summary> Factory method to create ICCMatrixBasedInputProfile based on a
		/// suppled profile file.
		/// </summary>
		/// <param name="f">contains a disk based ICCProfile.
		/// </param>
		/// <returns> the ICCMatrixBasedInputProfile
		/// </returns>
		/// <exception cref="ICCProfileInvalidException">
		/// </exception>
		/// <exception cref="ColorSpaceException">
		/// </exception>
		public static ICCMatrixBasedInputProfile createInstance(ColorSpace csm)
		{
			return new ICCMatrixBasedInputProfile(csm);
		}
		
		/// <summary> Construct an ICCMatrixBasedInputProfile based on a
		/// suppled profile file.
		/// </summary>
		/// <param name="f">contains a disk based ICCProfile.
		/// </param>
		/// <exception cref="ColorSpaceException">
		/// </exception>
		/// <exception cref="ICCProfileInvalidException">
		/// </exception>
		protected internal ICCMatrixBasedInputProfile(ColorSpace csm):base(csm)
		{
		}
		
		/* end class ICCMatrixBasedInputProfile */
	}
}