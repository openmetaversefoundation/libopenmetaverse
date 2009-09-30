/// <summary>**************************************************************************
/// 
/// $Id: ICCProfileException.java,v 1.2 2002/08/08 14:08:13 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>
using System;
namespace CSJ2K.Icc
{
	
	/// <summary> This exception is thrown when the content of a profile
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
	public class ICCProfileException:System.Exception
	{
		
		/// <summary>  Contruct with message</summary>
		/// <param name="msg">returned by getMessage()
		/// </param>
		public ICCProfileException(System.String msg):base(msg)
		{
		}
		
		
		/// <summary> Empty constructor</summary>
		public ICCProfileException()
		{
		}
	}
}