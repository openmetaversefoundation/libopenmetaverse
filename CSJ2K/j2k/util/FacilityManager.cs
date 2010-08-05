/*
* CVS identifier:
*
* $Id: FacilityManager.java,v 1.12 2002/05/22 15:00:24 grosbois Exp $
*
* Class:                   MsgLoggerManager
*
* Description:             Manages common facilities across threads
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
namespace CSJ2K.j2k.util
{
	
	/// <summary> This class manages common facilities for multi-threaded
	/// environments, It can register different facilities for each thread,
	/// and also a default one, so that they can be referred by static
	/// methods, while possibly having different ones for different
	/// threads. Also a default facility exists that is used for threads
	/// for which no particular facility has been registerd registered.
	/// 
	/// <p>Currently the only kind of facilities managed is MsgLogger.</p>
	/// 
	/// <P>An example use of this class is if 2 instances of a decoder are running
	/// in different threads and the messages of the 2 instances should be
	/// separated.
	/// 
	/// <P>The default MsgLogger is a StreamMsgLogger that uses System.out as
	/// the 'out' stream and System.err as the 'err' stream, and a line width of
	/// 78. This can be changed using the registerMsgLogger() method.
	/// 
	/// </summary>
	/// <seealso cref="MsgLogger">
	/// </seealso>
	/// <seealso cref="StreamMsgLogger">
	/// 
	/// </seealso>
	public class FacilityManager
	{		
		/// <summary>The loggers associated to different threads </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'loggerList '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		private static readonly System.Collections.Hashtable loggerList = System.Collections.Hashtable.Synchronized(new System.Collections.Hashtable());
		
		/// <summary>The default logger, for threads that have none associated with them </summary>
		private static MsgLogger defMsgLogger = new StreamMsgLogger(System.Console.OpenStandardOutput(), System.Console.OpenStandardError(), 78);
		
		/// <summary>The ProgressWatch instance associated to different threads </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'watchProgList '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		private static readonly System.Collections.Hashtable watchProgList = System.Collections.Hashtable.Synchronized(new System.Collections.Hashtable());
		
		/// <summary>The default ProgressWatch for threads that have none
		/// associated with them. 
		/// </summary>
		private static ProgressWatch defWatchProg = null;
		
		
		internal static void  registerProgressWatch(SupportClass.ThreadClass t, ProgressWatch pw)
		{
			if (pw == null)
			{
				throw new System.NullReferenceException();
			}
			if (t == null)
			{
				defWatchProg = pw;
			}
			else
			{
				watchProgList[t] = pw;
			}
		}
		
		/// <summary> Registers the MsgLogger 'ml' as the logging facility of the
		/// thread 't'. If any other logging facility was registered with
		/// the thread 't' it is overriden by 'ml'. If 't' is null then
		/// 'ml' is taken as the default message logger that is used for
		/// threads that have no MsgLogger registered.
		/// 
		/// </summary>
		/// <param name="t">The thread to associate with 'ml'
		/// 
		/// </param>
		/// <param name="ml">The MsgLogger to associate with therad ml
		/// 
		/// </param>
		internal static void  registerMsgLogger(SupportClass.ThreadClass t, MsgLogger ml)
		{
			if (ml == null)
			{
				throw new System.NullReferenceException();
			}
			if (t == null)
			{
				defMsgLogger = ml;
			}
			else
			{
				loggerList[t] = ml;
			}
		}
		
		/// <summary> Returns the MsgLogger registered with the current thread (the
		/// thread that calls this method). If the current thread has no
		/// registered MsgLogger then the default message logger is
		/// returned.
		/// 
		/// </summary>
		/// <returns> The MsgLogger registerd for the current thread, or the
		/// default one if there is none registered for it.
		/// 
		/// </returns>
		public static MsgLogger getMsgLogger()
		{
			MsgLogger ml = (MsgLogger) loggerList[SupportClass.ThreadClass.Current()];
			return (ml == null)?defMsgLogger:ml;
		}
		
		/// <summary> Returns the MsgLogger registered with the thread 't' (the
		/// thread that calls this method). If the thread 't' has no
		/// registered MsgLogger then the default message logger is
		/// returned.
		/// 
		/// </summary>
		/// <param name="t">The thread for which to return the MsgLogger
		/// 
		/// </param>
		/// <returns> The MsgLogger registerd for the current thread, or the
		/// default one if there is none registered for it.
		/// 
		/// </returns>
		internal static MsgLogger getMsgLogger(SupportClass.ThreadClass t)
		{
			MsgLogger ml = (MsgLogger) loggerList[t];
			return (ml == null)?defMsgLogger:ml;
		}
	}
}