/// <summary>**************************************************************************
/// 
/// $Id: MatrixBasedTransformException.java,v 1.1 2002/07/25 14:56:49 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>
using System;
namespace CSJ2K.Icc.Lut
{
	
	/// <summary> Thrown by MatrixBasedTransformTosRGB
	/// 
	/// </summary>
	/// <seealso cref="jj2000.j2k.icc.lut.MatrixBasedTransformTosRGB">
	/// </seealso>
	/// <version> 	1.0
	/// </version>
	/// <author> 	Bruce A. Kern
	/// </author>
	
	[Serializable]
	public class MatrixBasedTransformException:System.Exception
	{
		
		/// <summary> Contruct with message</summary>
		/// <param name="msg">returned by getMessage()
		/// </param>
		internal MatrixBasedTransformException(System.String msg):base(msg)
		{
		}
		
		
		/// <summary> Empty constructor</summary>
		internal MatrixBasedTransformException()
		{
		}
		
		/* end class MatrixBasedTransformException */
	}
}