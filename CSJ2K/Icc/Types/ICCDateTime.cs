/// <summary>**************************************************************************
/// 
/// $Id: ICCDateTime.java,v 1.1 2002/07/25 14:56:31 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>
using System;
using ICCProfile = CSJ2K.Icc.ICCProfile;
namespace CSJ2K.Icc.Types
{
	
	/// <summary> Date Time format for tags
	/// 
	/// </summary>
	/// <version> 	1.0
	/// </version>
	/// <author> 	Bruce A. Kern
	/// </author>
	public class ICCDateTime
	{
		//UPGRADE_NOTE: Final was removed from the declaration of 'size '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		//UPGRADE_NOTE: The initialization of  'size' was moved to static method 'icc.types.ICCDateTime'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1005'"
		public static readonly int size;
		
		/// <summary>Year datum.   </summary>
		public short wYear;
		/// <summary>Month datum.  </summary>
		// Number of the actual year (i.e. 1994)
		public short wMonth;
		/// <summary>Day datum.    </summary>
		// Number of the month (1-12)
		public short wDay;
		/// <summary>Hour datum.   </summary>
		// Number of the day
		public short wHours;
		/// <summary>Minute datum. </summary>
		// Number of hours (0-23)
		public short wMinutes;
		/// <summary>Second datum. </summary>
		// Number of minutes (0-59)
		public short wSeconds; // Number of seconds (0-59)
		
		/// <summary>Construct an ICCDateTime from parts </summary>
		public ICCDateTime(short year, short month, short day, short hour, short minute, short second)
		{
			wYear = year; wMonth = month; wDay = day;
			wHours = hour; wMinutes = minute; wSeconds = second;
		}
		
		/// <summary>Write an ICCDateTime to a file. </summary>
		//UPGRADE_TODO: Class 'java.io.RandomAccessFile' was converted to 'System.IO.FileStream' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javaioRandomAccessFile'"
		public virtual void  write(System.IO.FileStream raf)
		{
			System.IO.BinaryWriter temp_BinaryWriter;
			temp_BinaryWriter = new System.IO.BinaryWriter(raf);
			temp_BinaryWriter.Write((System.Int16) wYear);
			System.IO.BinaryWriter temp_BinaryWriter2;
			temp_BinaryWriter2 = new System.IO.BinaryWriter(raf);
			temp_BinaryWriter2.Write((System.Int16) wMonth);
			System.IO.BinaryWriter temp_BinaryWriter3;
			temp_BinaryWriter3 = new System.IO.BinaryWriter(raf);
			temp_BinaryWriter3.Write((System.Int16) wDay);
			System.IO.BinaryWriter temp_BinaryWriter4;
			temp_BinaryWriter4 = new System.IO.BinaryWriter(raf);
			temp_BinaryWriter4.Write((System.Int16) wHours);
			System.IO.BinaryWriter temp_BinaryWriter5;
			temp_BinaryWriter5 = new System.IO.BinaryWriter(raf);
			temp_BinaryWriter5.Write((System.Int16) wMinutes);
			System.IO.BinaryWriter temp_BinaryWriter6;
			temp_BinaryWriter6 = new System.IO.BinaryWriter(raf);
			temp_BinaryWriter6.Write((System.Int16) wSeconds);
		}
		
		/// <summary>Return a ICCDateTime representation. </summary>
		public override System.String ToString()
		{
			//System.String rep = "";
			return System.Convert.ToString(wYear) + "/" + System.Convert.ToString(wMonth) + "/" + System.Convert.ToString(wDay) + " " + System.Convert.ToString(wHours) + ":" + System.Convert.ToString(wMinutes) + ":" + System.Convert.ToString(wSeconds);
		}
		
		/* end class ICCDateTime*/
		static ICCDateTime()
		{
			size = 6 * ICCProfile.short_size;
		}
	}
}