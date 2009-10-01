/// <summary>**************************************************************************
/// 
/// $Id: MonochromeTransformException.java,v 1.1 2002/07/25 14:56:49 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>
using System;
namespace CSJ2K.Icc.Lut
{
	
	/// <summary> Exception thrown by MonochromeTransformTosRGB.
	/// 
	/// </summary>
	/// <seealso cref="jj2000.j2k.icc.lut.MonochromeTransformTosRGB">
	/// </seealso>
	/// <version> 	1.0
	/// </version>
	/// <author> 	Bruce A. Kern
	/// </author>
	
	[Serializable]
	public class MonochromeTransformException:System.Exception
	{
		
		/// <summary> Contruct with message</summary>
		/// <param name="msg">returned by getMessage()
		/// </param>
		internal MonochromeTransformException(System.String msg):base(msg)
		{
		}
		
		/// <summary> Empty constructor</summary>
		internal MonochromeTransformException()
		{
		}
		
		/* end class MonochromeTransformException */
	}
}