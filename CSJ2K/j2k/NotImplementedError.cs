/*
* CVS Identifier:
*
* $Id: NotImplementedError.java,v 1.10 2000/09/05 09:22:13 grosbois Exp $
*
* Class:               NotImplementedError
*
* Description:         Exception that is thrown whenever a non-implemented
*                      method is called.
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
* 
* 
* 
*/
using System;
namespace CSJ2K.j2k
{
	
	/// <summary> This exception is thrown whenever a feature or functionality that
	/// has not been implemented is calle.
	/// 
	/// <P>Its purpose it is to ease the development and testing process. A
	/// class that partially implements its functionality should throw a
	/// <tt>NotImplementedError</tt> when a method that has not yet
	/// been implemented is called.
	/// 
	/// <P>This class is made a subclass of <tt>Error</tt> since it should
	/// never be caught by an application. There is no need to declare this
	/// exception in the <tt>throws</tt> clause of a method.
	/// 
	/// </summary>
	/// <seealso cref="Error">
	/// </seealso>
	[Serializable]
	public class NotImplementedError:System.ApplicationException
	{
		
		/// <summary> Constructs a new <tt>NotImplementedError</tt> exception with
		/// the default detail message. The message is:
		/// 
		/// <P><I>The called method has not been implemented yet. Sorry!</I>
		/// 
		/// 
		/// </summary>
		public NotImplementedError():base("The called method has not been implemented yet. Sorry!")
		{
		}
		
		/// <summary> Constructs a new <tt>NotImplementedError</tt> exception with
		/// the specified detail message <tt>m</tt>.
		/// 
		/// </summary>
		/// <param name="m">The detail message to use
		/// 
		/// 
		/// </param>
		public NotImplementedError(System.String m):base(m)
		{
		}
	}
}