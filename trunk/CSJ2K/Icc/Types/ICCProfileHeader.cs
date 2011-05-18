/// <summary>**************************************************************************
/// 
/// $Id: ICCProfileHeader.java,v 1.1 2002/07/25 14:56:31 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>
using System;
using ICCProfile = CSJ2K.Icc.ICCProfile;
namespace CSJ2K.Icc.Types
{
	
	
	/// <summary> An ICC profile contains a 128-byte header followed by a variable
	/// number of tags contained in a tag table. This class models the header
	/// portion of the profile.  Most fields in the header are ints.  Some, such
	/// as data and version are aggregations of ints. This class provides an api to
	/// those fields as well as the definition of standard constants which are used 
	/// in the header.
	/// 
	/// </summary>
	/// <seealso cref="jj2000.j2k.icc.ICCProfile">
	/// </seealso>
	/// <version> 	1.0
	/// </version>
	/// <author> 	Bruce A. Kern
	/// </author>
	
	public class ICCProfileHeader
	{
		//UPGRADE_NOTE: Final was removed from the declaration of 'eol '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		private static readonly System.String eol = System.Environment.NewLine;
		
		/// <summary>ICCProfile header byte array. </summary>
		//private byte[] header = null;
		

		/* Define the set of standard signature and type values. Only
		* those codes required for Restricted ICC use are defined here.
		*/
		/// <summary>Profile header signature </summary>
        private static int kdwProfileSignature = ICCProfile.getInt(System.Text.ASCIIEncoding.ASCII.GetBytes("acsp"), 0);
		
		/// <summary>Profile header signature </summary>
		public static int kdwProfileSigReverse = ICCProfile.getInt(System.Text.ASCIIEncoding.ASCII.GetBytes("psca"),0);

		private const System.String kdwInputProfile = "scnr";
		private const System.String kdwDisplayProfile = "mntr";
		private const System.String kdwRGBData = "RGB ";
		private const System.String kdwGrayData = "GRAY";
		private const System.String kdwXYZData = "XYZ ";
		private const System.String kdwGrayTRCTag = "kTRC";
		private const System.String kdwRedColorantTag = "rXYZ";
		private const System.String kdwGreenColorantTag = "gXYZ";
		private const System.String kdwBlueColorantTag = "bXYZ";
		private const System.String kdwRedTRCTag = "rTRC";
		private const System.String kdwGreenTRCTag = "gTRC";
		private const System.String kdwBlueTRCTag = "bTRC";
		
		/* Offsets into ICCProfile header byte array. */
		
        private static int offProfileSize = 0;
        private static int offCMMTypeSignature         = offProfileSize + ICCProfile.int_size;
        private static int offProfileVersion           = offCMMTypeSignature + ICCProfile.int_size;	   
        private static int offProfileClass             = offProfileVersion + ICCProfileVersion.size;
        private static int offColorSpaceType           = offProfileClass + ICCProfile.int_size;  
        private static int offPCSType                  = offColorSpaceType + ICCProfile.int_size;		   
        private static int offDateTime                 = offPCSType + ICCProfile.int_size;		   
        private static int offProfileSignature         = offDateTime + ICCDateTime.size;
        private static int offPlatformSignature        = offProfileSignature + ICCProfile.int_size;
        private static int offCMMFlags                 = offPlatformSignature + ICCProfile.int_size; 
        private static int offDeviceManufacturer       = offCMMFlags + ICCProfile.int_size;
        private static int offDeviceModel              = offDeviceManufacturer + ICCProfile.int_size;		
        private static int offDeviceAttributes1        = offDeviceModel + ICCProfile.int_size;
        private static int offDeviceAttributesReserved = offDeviceAttributes1 + ICCProfile.int_size;
        private static int offRenderingIntent          = offDeviceAttributesReserved + ICCProfile.int_size;
        private static int offPCSIlluminant            = offRenderingIntent + ICCProfile.int_size;
        private static int offCreatorSig               = offPCSIlluminant + XYZNumber.size;
        private static int offReserved                 = offCreatorSig + ICCProfile.int_size;
        /// <summary>Size of the header </summary>
        public static int size = offReserved + 44 * ICCProfile.byte_size;
		
		/// <summary>Header field </summary>
		/* Header fields mapped to primitive types. */
		public int dwProfileSize;
		/// <summary>Header field </summary>
		// Size of the entire profile in bytes	
		public int dwCMMTypeSignature;
		/// <summary>Header field </summary>
		// The preferred CMM for this profile
		public int dwProfileClass;
		/// <summary>Header field </summary>
		// Profile/Device class signature
		public int dwColorSpaceType;
		/// <summary>Header field </summary>
		// Colorspace signature
		public int dwPCSType;
		/// <summary>Header field </summary>
		// PCS type signature
		public int dwProfileSignature;
		/// <summary>Header field </summary>
		// Must be 'acsp' (0x61637370)
		public int dwPlatformSignature;
		/// <summary>Header field </summary>
		// Primary platform for which this profile was created
		public int dwCMMFlags;
		/// <summary>Header field </summary>
		// Flags to indicate various hints for the CMM
		public int dwDeviceManufacturer;
		/// <summary>Header field </summary>
		// Signature of device manufacturer
		public int dwDeviceModel;
		/// <summary>Header field </summary>
		// Signature of device model
		public int dwDeviceAttributes1;
		/// <summary>Header field </summary>
		// Attributes of the device
		public int dwDeviceAttributesReserved;
		/// <summary>Header field </summary>
		public int dwRenderingIntent;
		/// <summary>Header field </summary>
		// Desired rendering intent for this profile
		public int dwCreatorSig;
		/// <summary>Header field </summary>
		// Profile creator signature
		
		public byte[] reserved = new byte[44]; // 
		
		/// <summary>Header field </summary>
		/* Header fields mapped to ggregate types. */
		public ICCProfileVersion profileVersion;
		/// <summary>Header field </summary>
		// Version of the profile format on which
		public ICCDateTime dateTime;
		/// <summary>Header field </summary>
		// Date and time of profile creation// this profile is based
		public XYZNumber PCSIlluminant; // Illuminant used for this profile
		
		
		/// <summary>Construct and empty header </summary>
		public ICCProfileHeader()
		{
		}
		
		/// <summary> Construct a header from a complete ICCProfile</summary>
		/// <param name="byte">[] -- holds ICCProfile contents
		/// </param>
		public ICCProfileHeader(byte[] data)
		{

            dwProfileSize = ICCProfile.getInt(data, offProfileSize);
            dwCMMTypeSignature = ICCProfile.getInt(data, offCMMTypeSignature);
            dwProfileClass = ICCProfile.getInt(data, offProfileClass);
            dwColorSpaceType = ICCProfile.getInt(data, offColorSpaceType);
            dwPCSType = ICCProfile.getInt(data, offPCSType);
            dwProfileSignature = ICCProfile.getInt(data, offProfileSignature);
            dwPlatformSignature = ICCProfile.getInt(data, offPlatformSignature);
            dwCMMFlags = ICCProfile.getInt(data, offCMMFlags);
            dwDeviceManufacturer = ICCProfile.getInt(data, offDeviceManufacturer);
            dwDeviceModel = ICCProfile.getInt(data, offDeviceModel);
            dwDeviceAttributes1 = ICCProfile.getInt(data, offDeviceAttributesReserved);
            dwDeviceAttributesReserved = ICCProfile.getInt(data, offDeviceAttributesReserved);
            dwRenderingIntent = ICCProfile.getInt(data, offRenderingIntent);
            dwCreatorSig = ICCProfile.getInt(data, offCreatorSig);
			profileVersion = ICCProfile.getICCProfileVersion(data, offProfileVersion);
			dateTime = ICCProfile.getICCDateTime(data, offDateTime);
			PCSIlluminant = ICCProfile.getXYZNumber(data, offPCSIlluminant);
			
			for (int i = 0; i < reserved.Length; ++i)
				reserved[i] = data[offReserved + i];
		}
		
		/// <summary> Write out this ICCProfile header to a RandomAccessFile</summary>
		/// <param name="raf">sink for data
		/// </param>
		/// <exception cref="IOException">
		/// </exception>
		//UPGRADE_TODO: Class 'java.io.RandomAccessFile' was converted to 'System.IO.FileStream' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javaioRandomAccessFile'"
		public virtual void  write(System.IO.FileStream raf)
		{
			
			raf.Seek(offProfileSize, System.IO.SeekOrigin.Begin); raf.WriteByte((System.Byte) dwProfileSize);
			raf.Seek(offCMMTypeSignature, System.IO.SeekOrigin.Begin); raf.WriteByte((System.Byte) dwCMMTypeSignature);
			raf.Seek(offProfileVersion, System.IO.SeekOrigin.Begin); profileVersion.write(raf);
			raf.Seek(offProfileClass, System.IO.SeekOrigin.Begin); raf.WriteByte((System.Byte) dwProfileClass);
			raf.Seek(offColorSpaceType, System.IO.SeekOrigin.Begin); raf.WriteByte((System.Byte) dwColorSpaceType);
			raf.Seek(offPCSType, System.IO.SeekOrigin.Begin); raf.WriteByte((System.Byte) dwPCSType);
			raf.Seek(offDateTime, System.IO.SeekOrigin.Begin); dateTime.write(raf);
			raf.Seek(offProfileSignature, System.IO.SeekOrigin.Begin); raf.WriteByte((System.Byte) dwProfileSignature);
			raf.Seek(offPlatformSignature, System.IO.SeekOrigin.Begin); raf.WriteByte((System.Byte) dwPlatformSignature);
			raf.Seek(offCMMFlags, System.IO.SeekOrigin.Begin); raf.WriteByte((System.Byte) dwCMMFlags);
			raf.Seek(offDeviceManufacturer, System.IO.SeekOrigin.Begin); raf.WriteByte((System.Byte) dwDeviceManufacturer);
			raf.Seek(offDeviceModel, System.IO.SeekOrigin.Begin); raf.WriteByte((System.Byte) dwDeviceModel);
			raf.Seek(offDeviceAttributes1, System.IO.SeekOrigin.Begin); raf.WriteByte((System.Byte) dwDeviceAttributes1);
			raf.Seek(offDeviceAttributesReserved, System.IO.SeekOrigin.Begin); raf.WriteByte((System.Byte) dwDeviceAttributesReserved);
			raf.Seek(offRenderingIntent, System.IO.SeekOrigin.Begin); raf.WriteByte((System.Byte) dwRenderingIntent);
			raf.Seek(offPCSIlluminant, System.IO.SeekOrigin.Begin); PCSIlluminant.write(raf);
			raf.Seek(offCreatorSig, System.IO.SeekOrigin.Begin); raf.WriteByte((System.Byte) dwCreatorSig);
			raf.Seek(offReserved, System.IO.SeekOrigin.Begin);
            raf.Write(reserved, 0, reserved.Length);
            //SupportClass.RandomAccessFileSupport.WriteRandomFile(reserved, raf);
		}
		
		
		/// <summary>String representation of class </summary>
		public override System.String ToString()
		{
			System.Text.StringBuilder rep = new System.Text.StringBuilder("[ICCProfileHeader: ");
			
			rep.Append(eol + "         ProfileSize: " + System.Convert.ToString(dwProfileSize, 16));
			rep.Append(eol + "    CMMTypeSignature: " + System.Convert.ToString(dwCMMTypeSignature, 16));
			rep.Append(eol + "        ProfileClass: " + System.Convert.ToString(dwProfileClass, 16));
			rep.Append(eol + "      ColorSpaceType: " + System.Convert.ToString(dwColorSpaceType, 16));
			rep.Append(eol + "           dwPCSType: " + System.Convert.ToString(dwPCSType, 16));
			rep.Append(eol + "  dwProfileSignature: " + System.Convert.ToString(dwProfileSignature, 16));
			rep.Append(eol + " dwPlatformSignature: " + System.Convert.ToString(dwPlatformSignature, 16));
			rep.Append(eol + "          dwCMMFlags: " + System.Convert.ToString(dwCMMFlags, 16));
			rep.Append(eol + "dwDeviceManufacturer: " + System.Convert.ToString(dwDeviceManufacturer, 16));
			rep.Append(eol + "       dwDeviceModel: " + System.Convert.ToString(dwDeviceModel, 16));
			rep.Append(eol + " dwDeviceAttributes1: " + System.Convert.ToString(dwDeviceAttributes1, 16));
			rep.Append(eol + "   dwRenderingIntent: " + System.Convert.ToString(dwRenderingIntent, 16));
			rep.Append(eol + "        dwCreatorSig: " + System.Convert.ToString(dwCreatorSig, 16));
			rep.Append(eol + "      profileVersion: " + profileVersion);
			rep.Append(eol + "            dateTime: " + dateTime);
			rep.Append(eol + "       PCSIlluminant: " + PCSIlluminant);
			return rep.Append("]").ToString();
		}
		
		/* end class ICCProfileHeader */
	}
}