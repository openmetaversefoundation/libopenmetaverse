/// <summary>**************************************************************************
/// 
/// $Id: ICCProfileNotFoundException.java,v 1.1 2002/07/25 14:56:55 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>
using System;
namespace CSJ2K.Icc
{
	
	/// <summary> This exception is thrown when an image contains no icc profile.
	/// is incorrect.
	/// 
	/// </summary>
	/// <seealso cref="jj2000.j2k.icc.ICCProfile">
	/// </seealso>
	/// <version> 	1.0
	/// </version>
	/// <author> 	Bruce A. Kern
	/// </author>
	
	[Serializable]
	public class ICCProfileNotFoundException:ICCProfileException
	{
		
		/// <summary> Contruct with message</summary>
		/// <param name="msg">returned by getMessage()
		/// </param>
		internal ICCProfileNotFoundException(System.String msg):base(msg)
		{
		}
		
		
		/// <summary> Empty constructor</summary>
		internal ICCProfileNotFoundException():base("no icc profile in image")
		{
		}
		
		/* end class ICCProfileNotFoundException */
	}
}