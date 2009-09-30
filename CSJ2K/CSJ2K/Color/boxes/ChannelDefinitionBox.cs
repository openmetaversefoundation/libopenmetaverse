/// <summary>**************************************************************************
/// 
/// $Id: ChannelDefinitionBox.java,v 1.1 2002/07/25 14:50:46 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>
using System;
using ColorSpaceException = CSJ2K.Color.ColorSpaceException;
using ICCProfile = CSJ2K.Icc.ICCProfile;
using ParameterList = CSJ2K.j2k.util.ParameterList;
using RandomAccessIO = CSJ2K.j2k.io.RandomAccessIO;
namespace CSJ2K.Color.Boxes
{
	
	/// <summary> This class maps the components in the codestream
	/// to channels in the image.  It models the Component
	/// Mapping box in the JP2 header.
	/// 
	/// </summary>
	/// <version> 	1.0
	/// </version>
	/// <author> 	Bruce A. Kern
	/// </author>
	public sealed class ChannelDefinitionBox:JP2Box
	{
		public int NDefs
		{
			/* Return the number of channel definitions. */
			
			get
			{
				return ndefs;
			}
			
		}
		
		private int ndefs;
		private System.Collections.Hashtable definitions = System.Collections.Hashtable.Synchronized(new System.Collections.Hashtable());
		
		/// <summary> Construct a ChannelDefinitionBox from an input image.</summary>
		/// <param name="in">RandomAccessIO jp2 image
		/// </param>
		/// <param name="boxStart">offset to the start of the box in the image
		/// </param>
		/// <exception cref="IOException,">ColorSpaceException 
		/// </exception>
		public ChannelDefinitionBox(RandomAccessIO in_Renamed, int boxStart):base(in_Renamed, boxStart)
		{
			readBox();
		}
		
		/// <summary>Analyze the box content. </summary>
		private void  readBox()
		{
			
			byte[] bfr = new byte[8];
			
			in_Renamed.seek(dataStart);
			in_Renamed.readFully(bfr, 0, 2);
            ndefs = ICCProfile.getShort(bfr, 0) & 0x0000ffff;
			
			int offset = dataStart + 2;
			in_Renamed.seek(offset);
			for (int i = 0; i < ndefs; ++i)
			{
				in_Renamed.readFully(bfr, 0, 6);
                int channel = ICCProfile.getShort(bfr, 0);
				int[] channel_def = new int[3];
				channel_def[0] = getCn(bfr);
				channel_def[1] = getTyp(bfr);
				channel_def[2] = getAsoc(bfr);
				definitions[(System.Int32) channel_def[0]] = channel_def;
			}
		}
		
		/* Return the channel association. */
		public int getCn(int asoc)
		{
			System.Collections.IEnumerator keys = definitions.Keys.GetEnumerator();
			//UPGRADE_TODO: Method 'java.util.Enumeration.hasMoreElements' was converted to 'System.Collections.IEnumerator.MoveNext' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilEnumerationhasMoreElements'"
			while (keys.MoveNext())
			{
				//UPGRADE_TODO: Method 'java.util.Enumeration.nextElement' was converted to 'System.Collections.IEnumerator.Current' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilEnumerationnextElement'"
				int[] bfr = (int[]) definitions[keys.Current];
				if (asoc == getAsoc(bfr))
					return getCn(bfr);
			}
			return asoc;
		}
		
		/* Return the channel type. */
		public int getTyp(int channel)
		{
			int[] bfr = (int[]) definitions[(System.Int32) channel];
			return getTyp(bfr);
		}
		
		/* Return the associated channel of the association. */
		public int getAsoc(int channel)
		{
			int[] bfr = (int[]) definitions[(System.Int32) channel];
			return getAsoc(bfr);
		}
		
		
		/// <summary>Return a suitable String representation of the class instance. </summary>
		public override System.String ToString()
		{
			System.Text.StringBuilder rep = new System.Text.StringBuilder("[ChannelDefinitionBox ").Append(eol).Append("  ");
			rep.Append("ndefs= ").Append(System.Convert.ToString(ndefs));
			
			System.Collections.IEnumerator keys = definitions.Keys.GetEnumerator();
			//UPGRADE_TODO: Method 'java.util.Enumeration.hasMoreElements' was converted to 'System.Collections.IEnumerator.MoveNext' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilEnumerationhasMoreElements'"
			while (keys.MoveNext())
			{
				//UPGRADE_TODO: Method 'java.util.Enumeration.nextElement' was converted to 'System.Collections.IEnumerator.Current' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilEnumerationnextElement'"
				int[] bfr = (int[]) definitions[keys.Current];
				rep.Append(eol).Append("  ").Append("Cn= ").Append(System.Convert.ToString(getCn(bfr))).Append(", ").Append("Typ= ").Append(System.Convert.ToString(getTyp(bfr))).Append(", ").Append("Asoc= ").Append(System.Convert.ToString(getAsoc(bfr)));
			}
			
			rep.Append("]");
			return rep.ToString();
		}
		
		/// <summary>Return the channel from the record.</summary>
		private int getCn(byte[] bfr)
		{
            return ICCProfile.getShort(bfr, 0);
		}
		
		/// <summary>Return the channel type from the record.</summary>
		private int getTyp(byte[] bfr)
		{
            return ICCProfile.getShort(bfr, 2);
		}
		
		/// <summary>Return the associated channel from the record.</summary>
		private int getAsoc(byte[] bfr)
		{
            return ICCProfile.getShort(bfr, 4);
		}
		
		private int getCn(int[] bfr)
		{
			return bfr[0];
		}
		
		private int getTyp(int[] bfr)
		{
			return bfr[1];
		}
		
		private int getAsoc(int[] bfr)
		{
			return bfr[2];
		}
		
		/* end class ChannelDefinitionBox */
		static ChannelDefinitionBox()
		{
			{
				type = 0x63646566;
			}
		}
	}
}