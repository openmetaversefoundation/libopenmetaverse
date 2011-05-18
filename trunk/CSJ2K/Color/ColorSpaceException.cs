/// <summary>**************************************************************************
/// 
/// $Id: ColorSpaceException.java,v 1.1 2002/07/25 14:52:00 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>
using System;
namespace CSJ2K.Color
{
	
	/// <summary> This exception is thrown when the content of an
	/// image contains an incorrect colorspace box
	/// 
	/// </summary>
	/// <seealso cref="jj2000.j2k.colorspace.ColorSpaceMapper">
	/// </seealso>
	/// <version> 	1.0
	/// </version>
	/// <author> 	Bruce A. Kern
	/// </author>
	
	[Serializable]
	public class ColorSpaceException:System.Exception
	{
		
		/// <summary> Contruct with message</summary>
		/// <param name="msg">returned by getMessage()
		/// </param>
		public ColorSpaceException(System.String msg):base(msg)
		{
		}
		
		
		/// <summary> Empty constructor</summary>
		public ColorSpaceException()
		{
		}
		
		/* end class ColorSpaceException */
	}
}