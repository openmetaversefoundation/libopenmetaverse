/// <summary>**************************************************************************
/// 
/// $Id: ICCTagTable.java,v 1.1 2002/07/25 14:56:37 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>
using System;
using ColorSpace = CSJ2K.Color.ColorSpace;
using ICCProfile = CSJ2K.Icc.ICCProfile;
using ICCProfileHeader = CSJ2K.Icc.Types.ICCProfileHeader;
namespace CSJ2K.Icc.Tags
{
	
	/// <summary> This class models an ICCTagTable as a HashTable which maps 
	/// ICCTag signatures (as Integers) to ICCTags.
	/// 
	/// On disk the tag table exists as a byte array conventionally aggragted into a
	/// structured sequence of types (bytes, shorts, ints, and floats.  The first four bytes
	/// are the integer count of tags in the table.  This is followed by an array of triplets,
	/// one for each tag. The triplets each contain three integers, which are the tag signature,
	/// the offset of the tag in the byte array and the length of the tag in bytes.
	/// The tag data follows.  Each tag consists of an integer (4 bytes) tag type, a reserved integer
	/// and the tag data, which varies depending on the tag.
	/// 
	/// </summary>
	/// <seealso cref="jj2000.j2k.icc.tags.ICCTag">
	/// </seealso>
	/// <version> 	1.0
	/// </version>
	/// <author> 	Bruce A. Kern
	/// </author>
	[Serializable]
	public class ICCTagTable:System.Collections.Hashtable
	{
		//UPGRADE_NOTE: Final was removed from the declaration of 'eol '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		private static readonly System.String eol = System.Environment.NewLine;
		//UPGRADE_NOTE: Final was removed from the declaration of 'offTagCount '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		//UPGRADE_NOTE: The initialization of  'offTagCount' was moved to static method 'icc.tags.ICCTagTable'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1005'"
		private static readonly int offTagCount;
		//UPGRADE_NOTE: Final was removed from the declaration of 'offTags '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		//UPGRADE_NOTE: The initialization of  'offTags' was moved to static method 'icc.tags.ICCTagTable'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1005'"
		private static readonly int offTags;
		
		//UPGRADE_NOTE: Final was removed from the declaration of 'trios '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		private System.Collections.ArrayList trios = System.Collections.ArrayList.Synchronized(new System.Collections.ArrayList(10));
		
		private int tagCount;
		
		
		private class Triplet
		{
			/// <summary>Tag identifier              </summary>
			internal int signature;
			/// <summary>absolute offset of tag data </summary>
			internal int offset;
			/// <summary>length of tag data          </summary>
			internal int count;
			/// <summary>size of an entry            </summary>
			//UPGRADE_NOTE: Final was removed from the declaration of 'size '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
			//UPGRADE_NOTE: The initialization of  'size' was moved to static method 'icc.tags.ICCTagTable.Triplet'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1005'"
			public static readonly int size;
			
			
			internal Triplet(int signature, int offset, int count)
			{
				this.signature = signature;
				this.offset = offset;
				this.count = count;
			}
			static Triplet()
			{
				size = 3 * ICCProfile.int_size;
			}
		}
		
		/// <summary> Representation of a tag table</summary>
		/// <returns> String
		/// </returns>
		//UPGRADE_NOTE: The equivalent of method 'java.util.Hashtable.toString' is not an override method. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1143'"
		public override System.String ToString()
		{
			System.Text.StringBuilder rep = new System.Text.StringBuilder("[ICCTagTable containing " + tagCount + " tags:");
			System.Text.StringBuilder body = new System.Text.StringBuilder("  ");
			System.Collections.IEnumerator keys = Keys.GetEnumerator();
			//UPGRADE_TODO: Method 'java.util.Enumeration.hasMoreElements' was converted to 'System.Collections.IEnumerator.MoveNext' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilEnumerationhasMoreElements'"
			while (keys.MoveNext())
			{
				//UPGRADE_TODO: Method 'java.util.Enumeration.nextElement' was converted to 'System.Collections.IEnumerator.Current' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilEnumerationnextElement'"
				System.Int32 key = (System.Int32) keys.Current;
				ICCTag tag = (ICCTag) this[key];
				body.Append(eol).Append(tag.ToString());
			}
			rep.Append(ColorSpace.indent("  ", body));
			return rep.Append("]").ToString();
		}
		
		
		/// <summary> Factory method for creating a tag table from raw input.</summary>
		/// <param name="byte">array of unstructured data representing a tag
		/// </param>
		/// <returns> ICCTagTable
		/// </returns>
		public static ICCTagTable createInstance(byte[] data)
		{
			ICCTagTable tags = new ICCTagTable(data);
			return tags;
		}
		
		
		/// <summary> Ctor used by factory method.</summary>
		/// <param name="byte">raw tag data
		/// </param>
		protected internal ICCTagTable(byte[] data)
		{
            tagCount = ICCProfile.getInt(data, offTagCount);
			
			int offset = offTags;
			for (int i = 0; i < tagCount; ++i)
			{
                int signature = ICCProfile.getInt(data, offset);
                int tagOffset = ICCProfile.getInt(data, offset + ICCProfile.int_size);
                int length = ICCProfile.getInt(data, offset + 2 * ICCProfile.int_size);
				trios.Add(new Triplet(signature, tagOffset, length));
				offset += 3 * ICCProfile.int_size;
			}
			
			
			System.Collections.IEnumerator Enum = trios.GetEnumerator();
			//UPGRADE_TODO: Method 'java.util.Enumeration.hasMoreElements' was converted to 'System.Collections.IEnumerator.MoveNext' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilEnumerationhasMoreElements'"
			while (Enum.MoveNext())
			{
				//UPGRADE_TODO: Method 'java.util.Enumeration.nextElement' was converted to 'System.Collections.IEnumerator.Current' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilEnumerationnextElement'"
				Triplet trio = (Triplet) Enum.Current;
				ICCTag tag = ICCTag.createInstance(trio.signature, data, trio.offset, trio.count);
				System.Object tempObject;
				tempObject = this[(System.Int32) tag.signature];
				this[(System.Int32) tag.signature] = tag;
				System.Object generatedAux2 = tempObject;
			}
		}
		
		
		/// <summary> Output the table to a disk</summary>
		/// <param name="raf">RandomAccessFile which receives the table.
		/// </param>
		/// <exception cref="IOException">
		/// </exception>
		//UPGRADE_TODO: Class 'java.io.RandomAccessFile' was converted to 'System.IO.FileStream' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javaioRandomAccessFile'"
		public virtual void  write(System.IO.FileStream raf)
		{
			
			int ntags = trios.Count;
			
			int countOff = ICCProfileHeader.size;
			int tagOff = countOff + ICCProfile.int_size;
			int dataOff = tagOff + 3 * ntags * ICCProfile.int_size;
			
			raf.Seek(countOff, System.IO.SeekOrigin.Begin);
			System.IO.BinaryWriter temp_BinaryWriter;
			temp_BinaryWriter = new System.IO.BinaryWriter(raf);
			temp_BinaryWriter.Write((System.Int32) ntags);
			
			int currentTagOff = tagOff;
			int currentDataOff = dataOff;
			
			System.Collections.IEnumerator enum_Renamed = trios.GetEnumerator();
			//UPGRADE_TODO: Method 'java.util.Enumeration.hasMoreElements' was converted to 'System.Collections.IEnumerator.MoveNext' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilEnumerationhasMoreElements'"
			while (enum_Renamed.MoveNext())
			{
				//UPGRADE_TODO: Method 'java.util.Enumeration.nextElement' was converted to 'System.Collections.IEnumerator.Current' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilEnumerationnextElement'"
				Triplet trio = (Triplet) enum_Renamed.Current;
				ICCTag tag = (ICCTag) this[(System.Int32) trio.signature];
				
				raf.Seek(currentTagOff, System.IO.SeekOrigin.Begin);
				System.IO.BinaryWriter temp_BinaryWriter2;
				temp_BinaryWriter2 = new System.IO.BinaryWriter(raf);
				temp_BinaryWriter2.Write((System.Int32) tag.signature);
				System.IO.BinaryWriter temp_BinaryWriter3;
				temp_BinaryWriter3 = new System.IO.BinaryWriter(raf);
				temp_BinaryWriter3.Write((System.Int32) currentDataOff);
				System.IO.BinaryWriter temp_BinaryWriter4;
				temp_BinaryWriter4 = new System.IO.BinaryWriter(raf);
				temp_BinaryWriter4.Write((System.Int32) tag.count);
				currentTagOff += 3 * CSJ2K.Icc.Tags.ICCTagTable.Triplet.size;
				
				raf.Seek(currentDataOff, System.IO.SeekOrigin.Begin);
				raf.Write(tag.data, tag.offset, tag.count);
				currentDataOff += tag.count;
			}
		}
		
		/* end class ICCTagTable */
		static ICCTagTable()
		{
			offTagCount = ICCProfileHeader.size;
			offTags = offTagCount + ICCProfile.int_size;
		}
	}
}