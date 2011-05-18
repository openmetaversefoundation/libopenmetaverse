/// <summary>**************************************************************************
/// 
/// $Id: ColorSpace.java,v 1.2 2002/07/25 16:31:11 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>
using System;
using FileFormatBoxes = CSJ2K.j2k.fileformat.FileFormatBoxes;
using ParameterList = CSJ2K.j2k.util.ParameterList;
using HeaderDecoder = CSJ2K.j2k.codestream.reader.HeaderDecoder;
using RandomAccessIO = CSJ2K.j2k.io.RandomAccessIO;
using ICCProfile = CSJ2K.Icc.ICCProfile;
using PaletteBox = CSJ2K.Color.Boxes.PaletteBox;
using ComponentMappingBox = CSJ2K.Color.Boxes.ComponentMappingBox;
using ColorSpecificationBox = CSJ2K.Color.Boxes.ColorSpecificationBox;
using ChannelDefinitionBox = CSJ2K.Color.Boxes.ChannelDefinitionBox;
using ImageHeaderBox = CSJ2K.Color.Boxes.ImageHeaderBox;
using JP2Box = CSJ2K.Color.Boxes.JP2Box;
namespace CSJ2K.Color
{
	
	/// <summary> This class analyzes the image to provide colorspace
	/// information for the decoding chain.  It does this by
	/// examining the box structure of the JP2 image.
	/// It also provides access to the parameter list information,
	/// which is stored as a public final field.
	/// 
	/// </summary>
	/// <seealso cref="jj2000.j2k.icc.ICCProfile">
	/// </seealso>
	/// <version> 	1.0
	/// </version>
	/// <author> 	Bruce A. Kern
	/// </author>
	public class ColorSpace
	{
		/// <summary> Retrieve the ICC profile from the images as
		/// a byte array.
		/// </summary>
		/// <returns> the ICC Profile as a byte [].
		/// </returns>
		virtual public byte[] ICCProfile
		{
			get
			{
				return csbox.ICCProfile;
			}
			
		}
		/// <summary>Return the colorspace method (Profiled, enumerated, or palettized). </summary>
		virtual public MethodEnum Method
		{
			get
			{
				return csbox.Method;
			}
			
		}
		/// <summary>Return number of channels in the palette. </summary>
		virtual public PaletteBox PaletteBox
		{
			get
			{
				return pbox;
			}
			
		}
		/// <summary>Return number of channels in the palette. </summary>
		virtual public int PaletteChannels
		{
			get
			{
				return pbox == null?0:pbox.NumColumns;
			}
			
		}
		/// <summary>Is palettized predicate. </summary>
		virtual public bool Palettized
		{
			get
			{
				return pbox != null;
			}
			
		}
		//UPGRADE_NOTE: Final was removed from the declaration of 'eol '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		public static readonly System.String eol = System.Environment.NewLine;
		
		// Renamed for convenience:
		internal const int GRAY = 0;
		internal const int RED = 1;
		internal const int GREEN = 2;
		internal const int BLUE = 3;
		
		/// <summary>Parameter Specs </summary>
		public ParameterList pl;
		
		/// <summary>Parameter Specs </summary>
		public HeaderDecoder hd;
		
		/* Image box structure as pertains to colorspacees. */
		private PaletteBox pbox = null;
		private ComponentMappingBox cmbox = null;
		private ColorSpecificationBox csbox = null;
		private ChannelDefinitionBox cdbox = null;
		private ImageHeaderBox ihbox = null;
		
		/// <summary>Input image </summary>
		private RandomAccessIO in_Renamed = null;
		
		/// <summary>Indent a String that contains newlines. </summary>
		public static System.String indent(System.String ident, System.Text.StringBuilder instr)
		{
			return indent(ident, instr.ToString());
		}
		
		/// <summary>Indent a String that contains newlines. </summary>
		public static System.String indent(System.String ident, System.String instr)
		{
			System.Text.StringBuilder tgt = new System.Text.StringBuilder(instr);
			char eolChar = eol[0];
			int i = tgt.Length;
			while (--i > 0)
			{
				if (tgt[i] == eolChar)
					tgt.Insert(i + 1, ident);
			}
			return ident + tgt.ToString();
		}
		
		/// <summary> public constructor which takes in the image, parameterlist and the
		/// image header decoder as args.
		/// </summary>
		/// <param name="in">input RandomAccess image file.
		/// </param>
		/// <param name="hd">provides information about the image header.
		/// </param>
		/// <param name="pl">provides parameters from the default and commandline lists. 
		/// </param>
		/// <exception cref="IOException,">ColorSpaceException
		/// </exception>
		public ColorSpace(RandomAccessIO in_Renamed, HeaderDecoder hd, ParameterList pl)
		{
			this.pl = pl;
			this.in_Renamed = in_Renamed;
			this.hd = hd;
			getBoxes();
		}
		
		/// <summary> Retrieve the various boxes from the JP2 file.</summary>
		/// <exception cref="ColorSpaceException,">IOException
		/// </exception>
		protected internal void  getBoxes()
		{
			//byte[] data;
			int type;
			long len = 0;
			int boxStart = 0;
			byte[] boxHeader = new byte[16];
			int i = 0;
			
			// Search the toplevel boxes for the header box
			while (true)
			{
				in_Renamed.seek(boxStart);
				in_Renamed.readFully(boxHeader, 0, 16);
                // CONVERSION PROBLEM?

                len = (long)CSJ2K.Icc.ICCProfile.getInt(boxHeader, 0);
				if (len == 1)
                    len = CSJ2K.Icc.ICCProfile.getLong(boxHeader, 8); // Extended
				// length
                type = CSJ2K.Icc.ICCProfile.getInt(boxHeader, 4);
				
				// Verify the contents of the file so far.
				if (i == 0 && type != CSJ2K.j2k.fileformat.FileFormatBoxes.JP2_SIGNATURE_BOX)
				{
					throw new ColorSpaceException("first box in image not " + "signature");
				}
				else if (i == 1 && type != CSJ2K.j2k.fileformat.FileFormatBoxes.FILE_TYPE_BOX)
				{
					throw new ColorSpaceException("second box in image not file");
				}
				else if (type == CSJ2K.j2k.fileformat.FileFormatBoxes.CONTIGUOUS_CODESTREAM_BOX)
				{
					throw new ColorSpaceException("header box not found in image");
				}
				else if (type == CSJ2K.j2k.fileformat.FileFormatBoxes.JP2_HEADER_BOX)
				{
					break;
				}
				
				// Progress to the next box.
				++i;
				boxStart = (int) (boxStart + len);
			}
			
			// boxStart indexes the start of the JP2_HEADER_BOX,
			// make headerBoxEnd index the end of the box.
			long headerBoxEnd = boxStart + len;
			
			if (len == 1)
				boxStart += 8; // Extended length header
			
			for (boxStart += 8; boxStart < headerBoxEnd; boxStart = (int) (boxStart + len))
			{
				in_Renamed.seek(boxStart);
				in_Renamed.readFully(boxHeader, 0, 16);
                len = (long)CSJ2K.Icc.ICCProfile.getInt(boxHeader, 0);
				if (len == 1)
					throw new ColorSpaceException("Extended length boxes " + "not supported");
                type = (int)CSJ2K.Icc.ICCProfile.getInt(boxHeader, 4);
				
				switch (type)
				{
					
					case CSJ2K.j2k.fileformat.FileFormatBoxes.IMAGE_HEADER_BOX: 
						ihbox = new ImageHeaderBox(in_Renamed, boxStart);
						break;
					
					case CSJ2K.j2k.fileformat.FileFormatBoxes.COLOUR_SPECIFICATION_BOX: 
						csbox = new ColorSpecificationBox(in_Renamed, boxStart);
						break;
					
					case CSJ2K.j2k.fileformat.FileFormatBoxes.CHANNEL_DEFINITION_BOX: 
						cdbox = new ChannelDefinitionBox(in_Renamed, boxStart);
						break;
					
					case CSJ2K.j2k.fileformat.FileFormatBoxes.COMPONENT_MAPPING_BOX: 
						cmbox = new ComponentMappingBox(in_Renamed, boxStart);
						break;
					
					case CSJ2K.j2k.fileformat.FileFormatBoxes.PALETTE_BOX: 
						pbox = new PaletteBox(in_Renamed, boxStart);
						break;
					
					default: 
						break;
					
				}
			}
			
			if (ihbox == null)
				throw new ColorSpaceException("image header box not found");
			
			if ((pbox == null && cmbox != null) || (pbox != null && cmbox == null))
				throw new ColorSpaceException("palette box and component " + "mapping box inconsistency");
		}
		
		
		/// <summary>Return the channel definition of the input component. </summary>
		public virtual int getChannelDefinition(int c)
		{
			if (cdbox == null)
				return c;
			else
				return cdbox.getCn(c + 1);
		}
		
		/// <summary>Return the colorspace (sYCC, sRGB, sGreyScale). </summary>
		public virtual CSEnum getColorSpace()
		{
			return csbox.ColorSpace;
		}
		
		/// <summary>Return bitdepth of the palette entries. </summary>
		public virtual int getPaletteChannelBits(int c)
		{
			return pbox == null ? 0 : (int)pbox.getBitDepth(c);
		}
		
		/// <summary> Return a palettized sample</summary>
		/// <param name="channel">requested 
		/// </param>
		/// <param name="index">of entry
		/// </param>
		/// <returns> palettized sample
		/// </returns>
		public virtual int getPalettizedSample(int channel, int index)
		{
			return pbox == null?0:pbox.getEntry(channel, index);
		}
		
		/// <summary>Signed output predicate. </summary>
		public virtual bool isOutputSigned(int channel)
		{
			return (pbox != null)?pbox.isSigned(channel):hd.isOriginalSigned(channel);
		}
		
		/// <summary>Return a suitable String representation of the class instance. </summary>
		public override System.String ToString()
		{
			System.Text.StringBuilder rep = new System.Text.StringBuilder("[ColorSpace is ").Append(csbox.MethodString).Append(Palettized?"  and palettized ":" ").Append(Method == MethodEnum.ENUMERATED?csbox.ColorSpaceString:"");
			if (ihbox != null)
			{
				rep.Append(eol).Append(indent("    ", ihbox.ToString()));
			}
			if (cdbox != null)
			{
				rep.Append(eol).Append(indent("    ", cdbox.ToString()));
			}
			if (csbox != null)
			{
				rep.Append(eol).Append(indent("    ", csbox.ToString()));
			}
			if (pbox != null)
			{
				rep.Append(eol).Append(indent("    ", pbox.ToString()));
			}
			if (cmbox != null)
			{
				rep.Append(eol).Append(indent("    ", cmbox.ToString()));
			}
			return rep.Append("]").ToString();
		}
		
		/// <summary> Are profiling diagnostics turned on</summary>
		/// <returns> yes or no
		/// </returns>
		public virtual bool debugging()
		{
			return pl.Get("colorspace_debug") != null && pl.Get("colorspace_debug").ToUpper().Equals("on".ToUpper());
		}


        public enum MethodEnum
        {
            ICC_PROFILED,
            ENUMERATED
        }
        public enum CSEnum
        {
            sRGB,
            GreyScale,
            sYCC,
            esRGB,
            Illegal,
            Unknown
        }
		/* Enumeration Class */
        /*
		/// <summary>method enumeration </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'ICC_PROFILED '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		public const MethodEnum ICC_PROFILED = new MethodEnum("profiled");
		/// <summary>method enumeration </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'ENUMERATED '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		public const MethodEnum ENUMERATED = new MethodEnum("enumerated");
		
		/// <summary>colorspace enumeration </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'sRGB '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		public const CSEnum sRGB = new CSEnum("sRGB");
		/// <summary>colorspace enumeration </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'GreyScale '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		public const CSEnum GreyScale = new CSEnum("GreyScale");
		/// <summary>colorspace enumeration </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'sYCC '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		public const CSEnum sYCC = new CSEnum("sYCC");
		/// <summary>colorspace enumeration </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'Illegal '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		public const CSEnum Illegal = new CSEnum("Illegal");
		/// <summary>colorspace enumeration </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'Unknown '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		public const CSEnum Unknown = new CSEnum("Unknown");
		
		/// <summary> Typesafe enumeration class</summary>
		/// <version> 	1.0
		/// </version>
		/// <author> 	Bruce A Kern
		/// </author>
		public class Enumeration
		{
			//UPGRADE_NOTE: Final was removed from the declaration of 'value '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
			public System.String value_Renamed;
			public Enumeration(System.String value_Renamed)
			{
				this.value_Renamed = value_Renamed;
			}
			public override System.String ToString()
			{
				return value_Renamed;
			}
		}
		
        		
		/// <summary> Method enumeration class</summary>
		/// <version> 	1.0
		/// </version>
		/// <author> 	Bruce A Kern
		/// </author>
		public class MethodEnum:Enumeration
		{
			public MethodEnum(System.String value_Renamed):base(value_Renamed)
			{
			}
		}
		
		/// <summary> Colorspace enumeration class</summary>
		/// <version> 	1.0
		/// </version>
		/// <author> 	Bruce A Kern
		/// </author>
		public class CSEnum:Enumeration
		{
			public CSEnum(System.String value_Renamed):base(value_Renamed)
			{
			}
		}
		*/
		/* end class ColorSpace */
	}
}