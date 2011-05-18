/// <summary>**************************************************************************
/// 
/// $Id: ICCProfile.java,v 1.1 2002/07/25 14:56:55 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>
using System;
using System.Text;
using ParameterList = CSJ2K.j2k.util.ParameterList;
using DecoderSpecs = CSJ2K.j2k.decoder.DecoderSpecs;
using BitstreamReaderAgent = CSJ2K.j2k.codestream.reader.BitstreamReaderAgent;
using ColorSpace = CSJ2K.Color.ColorSpace;
using ColorSpaceException = CSJ2K.Color.ColorSpaceException;
using ICCProfileHeader = CSJ2K.Icc.Types.ICCProfileHeader;
using ICCTag = CSJ2K.Icc.Tags.ICCTag;
using ICCTagTable = CSJ2K.Icc.Tags.ICCTagTable;
using ICCCurveType = CSJ2K.Icc.Tags.ICCCurveType;
using ICCXYZType = CSJ2K.Icc.Tags.ICCXYZType;
using XYZNumber = CSJ2K.Icc.Types.XYZNumber;
using ICCProfileVersion = CSJ2K.Icc.Types.ICCProfileVersion;
using ICCDateTime = CSJ2K.Icc.Types.ICCDateTime;
using FileFormatBoxes = CSJ2K.j2k.fileformat.FileFormatBoxes;
using RandomAccessIO = CSJ2K.j2k.io.RandomAccessIO;
using FacilityManager = CSJ2K.j2k.util.FacilityManager;
using MsgLogger = CSJ2K.j2k.util.MsgLogger;
namespace CSJ2K.Icc
{
	
	/// <summary>  This class models the ICCProfile file.  This file is a binary file which is divided 
	/// into two parts, an ICCProfileHeader followed by an ICCTagTable. The header is a 
	/// straightforward list of descriptive parameters such as profile size, version, date and various
	/// more esoteric parameters.  The tag table is a structured list of more complexly aggragated data
	/// describing things such as ICC curves, copyright information, descriptive text blocks, etc.
	/// 
	/// Classes exist to model the header and tag table and their various constituent parts the developer
	/// is refered to these for further information on the structure and contents of the header and tag table.
	/// 
	/// </summary>
	/// <seealso cref="jj2000.j2k.icc.types.ICCProfileHeader">
	/// </seealso>
	/// <seealso cref="jj2000.j2k.icc.tags.ICCTagTable">
	/// </seealso>
	/// <version> 	1.0
	/// </version>
	/// <author> 	Bruce A. Kern
	/// </author>
	
	public abstract class ICCProfile
	{
		private int ProfileSize
		{
			get
			{
				return header.dwProfileSize;
			}
			
			set
			{
				header.dwProfileSize = value;
			}
			
		}
		private int CMMTypeSignature
		{
			get
			{
				return header.dwCMMTypeSignature;
			}
			
			set
			{
				header.dwCMMTypeSignature = value;
			}
			
		}
		private int ProfileClass
		{
			get
			{
				return header.dwProfileClass;
			}
			
			set
			{
				header.dwProfileClass = value;
			}
			
		}
		private int ColorSpaceType
		{
			get
			{
				return header.dwColorSpaceType;
			}
			
			set
			{
				header.dwColorSpaceType = value;
			}
			
		}
		private int PCSType
		{
			get
			{
				return header.dwPCSType;
			}
			
			set
			{
				header.dwPCSType = value;
			}
			
		}
		private int ProfileSignature
		{
			get
			{
				return header.dwProfileSignature;
			}
			
			set
			{
				header.dwProfileSignature = value;
			}
			
		}
		private int PlatformSignature
		{
			get
			{
				return header.dwPlatformSignature;
			}
			
			set
			{
				header.dwPlatformSignature = value;
			}
			
		}
		private int CMMFlags
		{
			get
			{
				return header.dwCMMFlags;
			}
			
			set
			{
				header.dwCMMFlags = value;
			}
			
		}
		private int DeviceManufacturer
		{
			get
			{
				return header.dwDeviceManufacturer;
			}
			
			set
			{
				header.dwDeviceManufacturer = value;
			}
			
		}
		private int DeviceModel
		{
			get
			{
				return header.dwDeviceModel;
			}
			
			set
			{
				header.dwDeviceModel = value;
			}
			
		}
		private int DeviceAttributes1
		{
			get
			{
				return header.dwDeviceAttributes1;
			}
			
			set
			{
				header.dwDeviceAttributes1 = value;
			}
			
		}
		private int DeviceAttributesReserved
		{
			get
			{
				return header.dwDeviceAttributesReserved;
			}
			
			set
			{
				header.dwDeviceAttributesReserved = value;
			}
			
		}
		private int RenderingIntent
		{
			get
			{
				return header.dwRenderingIntent;
			}
			
			set
			{
				header.dwRenderingIntent = value;
			}
			
		}
		private int CreatorSig
		{
			get
			{
				return header.dwCreatorSig;
			}
			
			set
			{
				header.dwCreatorSig = value;
			}
			
		}
		private ICCProfileVersion ProfileVersion
		{
			get
			{
				return header.profileVersion;
			}
			
			set
			{
				header.profileVersion = value;
			}
			
		}
		private XYZNumber PCSIlluminant
		{
			set
			{
				header.PCSIlluminant = value;
			}
			
		}
		private ICCDateTime DateTime
		{
			set
			{
				header.dateTime = value;
			}
			
		}
		/// <summary> Access the profile header</summary>
		/// <returns> ICCProfileHeader
		/// </returns>
		virtual public ICCProfileHeader Header
		{
			get
			{
				return header;
			}
			
		}
		/// <summary> Access the profile tag table</summary>
		/// <returns> ICCTagTable
		/// </returns>
		virtual public ICCTagTable TagTable
		{
			get
			{
				return tags;
			}
			
		}
		
		//UPGRADE_NOTE: Final was removed from the declaration of 'eol '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		private static readonly System.String eol = System.Environment.NewLine;
		
		/// <summary>Gray index. </summary>
		// Renamed for convenience:
		public const int GRAY = 0;
		/// <summary>RGB index.  </summary>
		public const int RED = 0;
		/// <summary>RGB index.  </summary>
		public const int GREEN = 1;
		/// <summary>RGB index.  </summary>
		public const int BLUE = 2;
		
		/// <summary>Size of native type </summary>
		public const int boolean_size = 1;
		/// <summary>Size of native type </summary>
		public const int byte_size = 1;
		/// <summary>Size of native type </summary>
		public const int char_size = 2;
		/// <summary>Size of native type </summary>
		public const int short_size = 2;
		/// <summary>Size of native type </summary>
		public const int int_size = 4;
		/// <summary>Size of native type </summary>
		public const int float_size = 4;
		/// <summary>Size of native type </summary>
		public const int long_size = 8;
		/// <summary>Size of native type </summary>
		public const int double_size = 8;
		
		/* Bit twiddling constant for integral types. */ public const int BITS_PER_BYTE = 8;
		/* Bit twiddling constant for integral types. */ public const int BITS_PER_SHORT = 16;
		/* Bit twiddling constant for integral types. */ public const int BITS_PER_INT = 32;
		/* Bit twiddling constant for integral types. */ public const int BITS_PER_LONG = 64;
		/* Bit twiddling constant for integral types. */ public const int BYTES_PER_SHORT = 2;
		/* Bit twiddling constant for integral types. */ public const int BYTES_PER_INT = 4;
		/* Bit twiddling constant for integral types. */ public const int BYTES_PER_LONG = 8;
		
		/* JP2 Box structure analysis help */
		
		[Serializable]
		private class BoxType:System.Collections.Hashtable
		{
			
			private static System.Collections.Hashtable map = System.Collections.Hashtable.Synchronized(new System.Collections.Hashtable());
			
			public static void  put(int type, System.String desc)
			{
				map[(System.Int32) type] = desc;
			}
			
			public static System.String get_Renamed(int type)
			{
				return (System.String) map[(System.Int32) type];
			}
			
			public static System.String colorSpecMethod(int meth)
			{
				switch (meth)
				{
					
					case 2:  return "Restricted ICC Profile";
					
					case 1:  return "Enumerated Color Space";
					
					default:  return "Undefined Color Spec Method";
					
				}
			}
			static BoxType()
			{
				{
					put(CSJ2K.j2k.fileformat.FileFormatBoxes.BITS_PER_COMPONENT_BOX, "BITS_PER_COMPONENT_BOX");
					put(CSJ2K.j2k.fileformat.FileFormatBoxes.CAPTURE_RESOLUTION_BOX, "CAPTURE_RESOLUTION_BOX");
					put(CSJ2K.j2k.fileformat.FileFormatBoxes.CHANNEL_DEFINITION_BOX, "CHANNEL_DEFINITION_BOX");
					put(CSJ2K.j2k.fileformat.FileFormatBoxes.COLOUR_SPECIFICATION_BOX, "COLOUR_SPECIFICATION_BOX");
					put(CSJ2K.j2k.fileformat.FileFormatBoxes.COMPONENT_MAPPING_BOX, "COMPONENT_MAPPING_BOX");
					put(CSJ2K.j2k.fileformat.FileFormatBoxes.CONTIGUOUS_CODESTREAM_BOX, "CONTIGUOUS_CODESTREAM_BOX");
					put(CSJ2K.j2k.fileformat.FileFormatBoxes.DEFAULT_DISPLAY_RESOLUTION_BOX, "DEFAULT_DISPLAY_RESOLUTION_BOX");
					put(CSJ2K.j2k.fileformat.FileFormatBoxes.FILE_TYPE_BOX, "FILE_TYPE_BOX");
					put(CSJ2K.j2k.fileformat.FileFormatBoxes.IMAGE_HEADER_BOX, "IMAGE_HEADER_BOX");
					put(CSJ2K.j2k.fileformat.FileFormatBoxes.INTELLECTUAL_PROPERTY_BOX, "INTELLECTUAL_PROPERTY_BOX");
					put(CSJ2K.j2k.fileformat.FileFormatBoxes.JP2_HEADER_BOX, "JP2_HEADER_BOX");
					put(CSJ2K.j2k.fileformat.FileFormatBoxes.JP2_SIGNATURE_BOX, "JP2_SIGNATURE_BOX");
					put(CSJ2K.j2k.fileformat.FileFormatBoxes.PALETTE_BOX, "PALETTE_BOX");
					put(CSJ2K.j2k.fileformat.FileFormatBoxes.RESOLUTION_BOX, "RESOLUTION_BOX");
					put(CSJ2K.j2k.fileformat.FileFormatBoxes.URL_BOX, "URL_BOX");
					put(CSJ2K.j2k.fileformat.FileFormatBoxes.UUID_BOX, "UUID_BOX");
					put(CSJ2K.j2k.fileformat.FileFormatBoxes.UUID_INFO_BOX, "UUID_INFO_BOX");
					put(CSJ2K.j2k.fileformat.FileFormatBoxes.UUID_LIST_BOX, "UUID_LIST_BOX");
					put(CSJ2K.j2k.fileformat.FileFormatBoxes.XML_BOX, "XML_BOX");
				}
			}
		}
		
		
		/// <summary> Creates an int from a 4 character String</summary>
		/// <param name="fourChar">string representation of an integer
		/// </param>
		/// <returns> the integer which is denoted by the input String.
		/// </returns>
		public static int getIntFromString(System.String fourChar)
		{
			byte[] bytes = SupportClass.ToByteArray(fourChar);
			return getInt(bytes, 0);
		}
		/// <summary> Create an XYZNumber from byte [] input</summary>
		/// <param name="data">array containing the XYZNumber representation
		/// </param>
		/// <param name="offset">start of the rep in the array
		/// </param>
		/// <returns> the created XYZNumber
		/// </returns>
		public static XYZNumber getXYZNumber(byte[] data, int offset)
		{
			int x, y, z;
            x = ICCProfile.getInt(data, offset);
            y = ICCProfile.getInt(data, offset + int_size);
            z = ICCProfile.getInt(data, offset + 2 * int_size);
			return new XYZNumber(x, y, z);
		}
		
		/// <summary> Create an ICCProfileVersion from byte [] input</summary>
		/// <param name="data">array containing the ICCProfileVersion representation
		/// </param>
		/// <param name="offset">start of the rep in the array
		/// </param>
		/// <returns>  the created ICCProfileVersion
		/// </returns>
		public static ICCProfileVersion getICCProfileVersion(byte[] data, int offset)
		{
			byte major = data[offset];
			byte minor = data[offset + byte_size];
			byte resv1 = data[offset + 2 * byte_size];
			byte resv2 = data[offset + 3 * byte_size];
			return new ICCProfileVersion(major, minor, resv1, resv2);
		}
		
		/// <summary> Create an ICCDateTime from byte [] input</summary>
		/// <param name="data">array containing the ICCProfileVersion representation
		/// </param>
		/// <param name="offset">start of the rep in the array
		/// </param>
		/// <returns> the created ICCProfileVersion
		/// </returns>
		public static ICCDateTime getICCDateTime(byte[] data, int offset)
		{
            short wYear = ICCProfile.getShort(data, offset); // Number of the actual year (i.e. 1994)
            short wMonth = ICCProfile.getShort(data, offset + ICCProfile.short_size); // Number of the month (1-12)
            short wDay = ICCProfile.getShort(data, offset + 2 * ICCProfile.short_size); // Number of the day
            short wHours = ICCProfile.getShort(data, offset + 3 * ICCProfile.short_size); // Number of hours (0-23)
            short wMinutes = ICCProfile.getShort(data, offset + 4 * ICCProfile.short_size); // Number of minutes (0-59)
            short wSeconds = ICCProfile.getShort(data, offset + 5 * ICCProfile.short_size); // Number of seconds (0-59)
			return new ICCDateTime(wYear, wMonth, wDay, wHours, wMinutes, wSeconds);
		}
		
		
		/// <summary> Create a String from a byte []. Optionally swap adjacent byte
		/// pairs.  Intended to be used to create integer String representations
		/// allowing for endian translations.
		/// </summary>
		/// <param name="bfr">data array
		/// </param>
		/// <param name="offset">start of data in array
		/// </param>
		/// <param name="length">length of data in array
		/// </param>
		/// <param name="swap">swap adjacent bytes?
		/// </param>
		/// <returns> String rep of data
		/// </returns>
		public static System.String getString(byte[] bfr, int offset, int length, bool swap)
		{
			
			byte[] result = new byte[length];
			int incr = swap?- 1:1;
			int start = swap?offset + length - 1:offset;
			for (int i = 0, j = start; i < length; ++i)
			{
				result[i] = bfr[j];
				j += incr;
			}
			return new System.String(SupportClass.ToCharArray(result));
		}
		
		/// <summary> Create a short from a two byte [], with optional byte swapping.</summary>
		/// <param name="bfr">data array
		/// </param>
		/// <param name="off">start of data in array
		/// </param>
		/// <param name="swap">swap bytes?
		/// </param>
		/// <returns> native type from representation.
		/// </returns>
		public static short getShort(byte[] bfr, int off, bool swap)
		{
			
			int tmp0 = bfr[off] & 0xff; // Clear the sign extended bits in the int.
			int tmp1 = bfr[off + 1] & 0xff;
			
			
			return (short) (swap?(tmp1 << BITS_PER_BYTE | tmp0):(tmp0 << BITS_PER_BYTE | tmp1));
		}
		
		/// <summary> Create a short from a two byte [].</summary>
		/// <param name="bfr">data array
		/// </param>
		/// <param name="off">start of data in array
		/// </param>
		/// <returns> native type from representation.
		/// </returns>
		public static short getShort(byte[] bfr, int off)
		{
			int tmp0 = bfr[off] & 0xff; // Clear the sign extended bits in the int.
			int tmp1 = bfr[off + 1] & 0xff;
			return (short) (tmp0 << BITS_PER_BYTE | tmp1);
		}
		
		/// <summary> Separate bytes in an int into a byte array lsb to msb order.</summary>
		/// <param name="d">integer to separate
		/// </param>
		/// <returns> byte [] containing separated int.
		/// </returns>
		public static byte[] setInt(int d)
		{
			return setInt(d, new byte[BYTES_PER_INT]);
		}
		
		/// <summary> Separate bytes in an int into a byte array lsb to msb order.
		/// Return the result in the provided array
		/// </summary>
		/// <param name="d">integer to separate
		/// </param>
		/// <param name="b">return output here.
		/// </param>
		/// <returns> reference to output.
		/// </returns>
		public static byte[] setInt(int d, byte[] b)
		{
			if (b == null)
				b = new byte[BYTES_PER_INT];
			for (int i = 0; i < BYTES_PER_INT; ++i)
			{
				b[i] = (byte) (d & 0x0ff);
				d = d >> BITS_PER_BYTE;
			}
			return b;
		}
		
		/// <summary> Separate bytes in a long into a byte array lsb to msb order.</summary>
		/// <param name="d">long to separate
		/// </param>
		/// <returns> byte [] containing separated int.
		/// </returns>
		public static byte[] setLong(long d)
		{
			return setLong(d, new byte[BYTES_PER_INT]);
		}
		
		/// <summary> Separate bytes in a long into a byte array lsb to msb order.
		/// Return the result in the provided array
		/// </summary>
		/// <param name="d">long to separate
		/// </param>
		/// <param name="b">return output here.
		/// </param>
		/// <returns> reference to output.
		/// </returns>
		public static byte[] setLong(long d, byte[] b)
		{
			if (b == null)
				b = new byte[BYTES_PER_LONG];
			for (int i = 0; i < BYTES_PER_LONG; ++i)
			{
				b[i] = (byte) (d & 0x0ff);
				d = d >> BITS_PER_BYTE;
			}
			return b;
		}
		
		
		/// <summary> Create an int from a byte [4], with optional byte swapping.</summary>
		/// <param name="bfr">data array
		/// </param>
		/// <param name="off">start of data in array
		/// </param>
		/// <param name="swap">swap bytes?
		/// </param>
		/// <returns> native type from representation.
		/// </returns>
		public static int getInt(byte[] bfr, int off, bool swap)
		{
			
			int tmp0 = getShort(bfr, off, swap) & 0xffff; // Clear the sign extended bits in the int.
			int tmp1 = getShort(bfr, off + 2, swap) & 0xffff;
			
			return (int) (swap?(tmp1 << BITS_PER_SHORT | tmp0):(tmp0 << BITS_PER_SHORT | tmp1));
		}
		
		/// <summary> Create an int from a byte [4].</summary>
		/// <param name="bfr">data array
		/// </param>
		/// <param name="off">start of data in array
		/// </param>
		/// <returns> native type from representation.
		/// </returns>
		public static int getInt(byte[] bfr, int off)
		{
			
			int tmp0 = getShort(bfr, off) & 0xffff; // Clear the sign extended bits in the int.
			int tmp1 = getShort(bfr, off + 2) & 0xffff;
			
			return (int) (tmp0 << BITS_PER_SHORT | tmp1);
		}
		
		/// <summary> Create an long from a byte [8].</summary>
		/// <param name="bfr">data array
		/// </param>
		/// <param name="off">start of data in array
		/// </param>
		/// <returns> native type from representation.
		/// </returns>
		public static long getLong(byte[] bfr, int off)
		{
			
			long tmp0 = getInt(bfr, off) & unchecked((int) 0xffffffff); // Clear the sign extended bits in the int.
			long tmp1 = getInt(bfr, off + 4) & unchecked((int) 0xffffffff);
			
			return (long) (tmp0 << BITS_PER_INT | tmp1);
		}
		
		
		/// <summary>signature    </summary>
		// Define the set of standard signature and type values
		// Because of the endian issues and byte swapping, the profile codes must
		// be stored in memory and be addressed by address. As such, only those
		// codes required for Restricted ICC use are defined here
		
		//UPGRADE_NOTE: Final was removed from the declaration of 'kdwProfileSignature '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		public static readonly int kdwProfileSignature;
		/// <summary>signature    </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'kdwProfileSigReverse '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		public static readonly int kdwProfileSigReverse;
		/// <summary>profile type </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'kdwInputProfile '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		public static readonly int kdwInputProfile;
		/// <summary>tag type     </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'kdwDisplayProfile '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		public static readonly int kdwDisplayProfile;
		/// <summary>tag type     </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'kdwRGBData '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		public static readonly int kdwRGBData;
		/// <summary>tag type     </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'kdwGrayData '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		public static readonly int kdwGrayData;
		/// <summary>tag type     </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'kdwXYZData '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		public static readonly int kdwXYZData;
		/// <summary>input type   </summary>
		public const int kMonochromeInput = 0;
		/// <summary>input type   </summary>
		public const int kThreeCompInput = 1;
	
		/// <summary>tag signature </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'kdwGrayTRCTag '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		public static readonly int kdwGrayTRCTag;
		/// <summary>tag signature </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'kdwRedColorantTag '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		public static readonly int kdwRedColorantTag;
		/// <summary>tag signature </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'kdwGreenColorantTag '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		public static readonly int kdwGreenColorantTag;
		/// <summary>tag signature </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'kdwBlueColorantTag '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		public static readonly int kdwBlueColorantTag;
		/// <summary>tag signature </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'kdwRedTRCTag '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		public static readonly int kdwRedTRCTag;
		/// <summary>tag signature </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'kdwGreenTRCTag '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		public static readonly int kdwGreenTRCTag;
		/// <summary>tag signature </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'kdwBlueTRCTag '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		public static readonly int kdwBlueTRCTag;
		/// <summary>tag signature </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'kdwCopyrightTag '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		public static readonly int kdwCopyrightTag;
		/// <summary>tag signature </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'kdwMediaWhiteTag '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		public static readonly int kdwMediaWhiteTag;
		/// <summary>tag signature </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'kdwProfileDescTag '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		public static readonly int kdwProfileDescTag;
		
		
		private ICCProfileHeader header = null;
		private ICCTagTable tags = null;
		private byte[] profile = null;
		
		//private byte[] data = null;
		private ParameterList pl = null;
		
		private ICCProfile()
		{
			throw new ICCProfileException("illegal to invoke empty constructor");
		}
		
		/// <summary> ParameterList constructor </summary>
		/// <param name="csb">provides colorspace information
		/// </param>
		protected internal ICCProfile(ColorSpace csm)
		{
			this.pl = csm.pl;
			profile = csm.ICCProfile;
			initProfile(profile);
		}
		
		/// <summary> Read the header and tags into memory and verify
		/// that the correct type of profile is being used. for encoding.
		/// </summary>
		/// <param name="data">ICCProfile
		/// </param>
		/// <exception cref="ICCProfileInvalidException">for bad signature and class and bad type
		/// </exception>
		private void  initProfile(byte[] data)
		{
			header = new ICCProfileHeader(data);
			tags = ICCTagTable.createInstance(data);
			
			
			// Verify that the data pointed to by icc is indeed a valid profile    
			// and that it is possibly of one of the Restricted ICC types. The simplest way to check    
			// this is to verify that the profile signature is correct, that it is an input profile,    
			// and that the PCS used is XYX.    
			
			// However, a common error in profiles will be to create Monitor profiles rather    
			// than input profiles. If this is the only error found, it's still useful to let this  
			// go through with an error written to stderr.  
			
			if (ProfileClass == kdwDisplayProfile)
			{
				System.String message = "NOTE!! Technically, this profile is a Display profile, not an" + " Input Profile, and thus is not a valid Restricted ICC profile." + " However, it is quite possible that this profile is usable as" + " a Restricted ICC profile, so this code will ignore this state" + " and proceed with processing.";
				
				FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.WARNING, message);
			}
			
			if ((ProfileSignature != kdwProfileSignature) || ((ProfileClass != kdwInputProfile) && (ProfileClass != kdwDisplayProfile)) || (PCSType != kdwXYZData))
			{
				throw new ICCProfileInvalidException();
			}
		}
		
		
		/// <summary>Provide a suitable string representation for the class </summary>
		public override System.String ToString()
		{
			System.Text.StringBuilder rep = new System.Text.StringBuilder("[ICCProfile:");
			System.Text.StringBuilder body = new System.Text.StringBuilder();
			body.Append(eol).Append(header);
			body.Append(eol).Append(eol).Append(tags);
			rep.Append(ColorSpace.indent("  ", body));
			return rep.Append("]").ToString();
		}
		
		
		/// <summary> Create a two character hex representation of a byte</summary>
		/// <param name="i">byte to represent
		/// </param>
		/// <returns> representation
		/// </returns>
		public static System.String toHexString(byte i)
		{
			System.String rep = (i >= 0 && i < 16?"0":"") + System.Convert.ToString((int) i, 16);
			if (rep.Length > 2)
				rep = rep.Substring(rep.Length - 2);
			return rep;
		}
		
		/// <summary> Create a 4 character hex representation of a short</summary>
		/// <param name="i">short to represent
		/// </param>
		/// <returns> representation
		/// </returns>
		public static System.String toHexString(short i)
		{
			System.String rep;
			
			if (i >= 0 && i < 0x10)
				rep = "000" + System.Convert.ToString((int) i, 16);
			else if (i >= 0 && i < 0x100)
				rep = "00" + System.Convert.ToString((int) i, 16);
			else if (i >= 0 && i < 0x1000)
				rep = "0" + System.Convert.ToString((int) i, 16);
			else
				rep = "" + System.Convert.ToString((int) i, 16);
			
			if (rep.Length > 4)
				rep = rep.Substring(rep.Length - 4);
			return rep;
		}
		
		
		/// <summary> Create a 8 character hex representation of a int</summary>
		/// <param name="i">int to represent
		/// </param>
		/// <returns> representation
		/// </returns>
		public static System.String toHexString(int i)
		{
			System.String rep;
			
			if (i >= 0 && i < 0x10)
				rep = "0000000" + System.Convert.ToString((int) i, 16);
			else if (i >= 0 && i < 0x100)
				rep = "000000" + System.Convert.ToString((int) i, 16);
			else if (i >= 0 && i < 0x1000)
				rep = "00000" + System.Convert.ToString((int) i, 16);
			else if (i >= 0 && i < 0x10000)
				rep = "0000" + System.Convert.ToString((int) i, 16);
			else if (i >= 0 && i < 0x100000)
				rep = "000" + System.Convert.ToString((int) i, 16);
			else if (i >= 0 && i < 0x1000000)
				rep = "00" + System.Convert.ToString((int) i, 16);
			else if (i >= 0 && i < 0x10000000)
				rep = "0" + System.Convert.ToString((int) i, 16);
			else
				rep = "" + System.Convert.ToString((int) i, 16);
			
			if (rep.Length > 8)
				rep = rep.Substring(rep.Length - 8);
			return rep;
		}
		
		public static System.String ToString(byte[] data)
		{
			
			int i, row, col, rem, rows, cols;
			
			System.Text.StringBuilder rep = new System.Text.StringBuilder();
			System.Text.StringBuilder rep0 = null;
			System.Text.StringBuilder rep1 = null;
			System.Text.StringBuilder rep2 = null;
			
			cols = 16;
			rows = data.Length / cols;
			rem = data.Length % cols;
			
			byte[] lbytes = new byte[8];
			for (row = 0, i = 0; row < rows; ++row)
			{
				rep1 = new System.Text.StringBuilder();
				rep2 = new System.Text.StringBuilder();
				
				for (i = 0; i < 8; ++i)
					lbytes[i] = 0;
                byte[] tbytes = System.Text.ASCIIEncoding.ASCII.GetBytes(System.Convert.ToString(row * 16, 16));
				for (int t = 0, l = lbytes.Length - tbytes.Length; t < tbytes.Length; ++l, ++t)
					lbytes[l] = tbytes[t];
				
				rep0 = new System.Text.StringBuilder(new System.String(SupportClass.ToCharArray(lbytes)));
				
				for (col = 0; col < cols; ++col)
				{
					byte b = data[i++];
					rep1.Append(toHexString(b)).Append(i % 2 == 0?" ":"");
					if ((System.Char.IsLetter((char) b) || ((char) b).CompareTo('$') == 0 || ((char) b).CompareTo('_') == 0))
						rep2.Append((char) b);
					else
						rep2.Append(".");
				}
				rep.Append(rep0).Append(" :  ").Append(rep1).Append(":  ").Append(rep2).Append(eol);
			}
			
			rep1 = new System.Text.StringBuilder();
			rep2 = new System.Text.StringBuilder();
			
			for (i = 0; i < 8; ++i)
				lbytes[i] = 0;
			byte[] tbytes2 = System.Text.ASCIIEncoding.ASCII.GetBytes(System.Convert.ToString(row * 16, 16));
			for (int t = 0, l = lbytes.Length - tbytes2.Length; t < tbytes2.Length; ++l, ++t)
				lbytes[l] = tbytes2[t];
			
			rep0 = new System.Text.StringBuilder(System.Text.ASCIIEncoding.ASCII.GetString(lbytes));
			
			for (col = 0; col < rem; ++col)
			{
				byte b = data[i++];
				rep1.Append(toHexString(b)).Append(i % 2 == 0?" ":"");
				if ((System.Char.IsLetter((char) b) || ((char) b).CompareTo('$') == 0 || ((char) b).CompareTo('_') == 0))
					rep2.Append((char) b);
				else
					rep2.Append(".");
			}
			for (col = rem; col < 16; ++col)
				rep1.Append("  ").Append(col % 2 == 0?" ":"");
			
			rep.Append(rep0).Append(" :  ").Append(rep1).Append(":  ").Append(rep2).Append(eol);
			
			return rep.ToString();
		}
		
		/// <summary> Parse this ICCProfile into a RestrictedICCProfile
		/// which is appropriate to the data in this profile.
		/// Either a MonochromeInputRestrictedProfile or 
		/// MatrixBasedRestrictedProfile is returned
		/// </summary>
		/// <returns> RestrictedICCProfile
		/// </returns>
		/// <exception cref="ICCProfileInvalidException">no curve data
		/// </exception>
		public virtual RestrictedICCProfile parse()
		{
			
			// The next step is to determine which Restricted ICC type is used by this profile.
			// Unfortunately, the only way to do this is to look through the tag table for
			// the tags required by the two types.
			
			// First look for the gray TRC tag. If the profile is indeed an input profile, and this
			// tag exists, then the profile is a Monochrome Input profile
			
			ICCCurveType grayTag = (ICCCurveType) tags[(System.Int32) kdwGrayTRCTag];
			if (grayTag != null)
			{
				return RestrictedICCProfile.createInstance(grayTag);
			}
			
			// If it wasn't a Monochrome Input profile, look for the Red Colorant tag. If that
			// tag is found and the profile is indeed an input profile, then this profile is
			// a Three-Component Matrix-Based Input profile
			
			ICCCurveType rTRCTag = (ICCCurveType) tags[(System.Int32) kdwRedTRCTag];
			
			
			if (rTRCTag != null)
			{
				ICCCurveType gTRCTag = (ICCCurveType) tags[(System.Int32) kdwGreenTRCTag];
				ICCCurveType bTRCTag = (ICCCurveType) tags[(System.Int32) kdwBlueTRCTag];
				ICCXYZType rColorantTag = (ICCXYZType) tags[(System.Int32) kdwRedColorantTag];
				ICCXYZType gColorantTag = (ICCXYZType) tags[(System.Int32) kdwGreenColorantTag];
				ICCXYZType bColorantTag = (ICCXYZType) tags[(System.Int32) kdwBlueColorantTag];
				return RestrictedICCProfile.createInstance(rTRCTag, gTRCTag, bTRCTag, rColorantTag, gColorantTag, bColorantTag);
			}
			
			throw new ICCProfileInvalidException("curve data not found in profile");
		}
		
		/// <summary> Output this ICCProfile to a RandomAccessFile</summary>
		/// <param name="os">output file
		/// </param>
		//UPGRADE_TODO: Class 'java.io.RandomAccessFile' was converted to 'System.IO.FileStream' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javaioRandomAccessFile'"
		public virtual void  write(System.IO.FileStream os)
		{
			Header.write(os);
			TagTable.write(os);
		}
		
		
		/* end class ICCProfile */
        static ICCProfile()
		{
		    kdwProfileSignature = GetTagInt("acsp");
		    kdwProfileSigReverse = GetTagInt("psca");
		    kdwInputProfile = GetTagInt("scnr");
		    kdwDisplayProfile = GetTagInt("mntr");
		    kdwRGBData = GetTagInt("RGB ");
		    kdwGrayData = GetTagInt("GRAY");
		    kdwXYZData = GetTagInt("XYZ ");

		    kdwGrayTRCTag = GetTagInt("kTRC");
            kdwRedColorantTag = GetTagInt("rXYZ");
            kdwGreenColorantTag = GetTagInt("gXYZ");
            kdwBlueColorantTag = GetTagInt("bXYZ");
            kdwRedTRCTag = GetTagInt("rTRC");
            kdwGreenTRCTag = GetTagInt("gTRC");
            kdwBlueTRCTag = GetTagInt("bTRC");
            kdwCopyrightTag = GetTagInt("cprt");
            kdwMediaWhiteTag = GetTagInt("wtpt");
            kdwProfileDescTag = GetTagInt("desc");
        }
        static int GetTagInt(string tag)
        {
            byte[] tagBytes = ASCIIEncoding.ASCII.GetBytes(tag);
            Array.Reverse(tagBytes);
            return BitConverter.ToInt32(tagBytes, 0);
        }
        
	}
}