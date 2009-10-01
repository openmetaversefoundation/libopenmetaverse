using System;
using CSJ2K.Icc;
using CSJ2K.Icc.Types;

namespace CSJ2K.Icc.Tags
{
    public class ICCViewType : ICCTag
    {
        new public int type;
        public int reserved;
        public XYZNumber CIEilluminant;
        public XYZNumber CIEsurround;
        public int illuminant;

        /// <summary> Construct this tag from its constituant parts</summary>
        /// <param name="signature">tag id</param>
        /// <param name="data">array of bytes</param>
        /// <param name="offset">to data in the data array</param>
        /// <param name="length">of data in the data array</param>
        protected internal ICCViewType(int signature, byte[] data, int offset, int length)
            : base(signature, data, offset, offset + 2 * ICCProfile.int_size)
        {
            type = ICCProfile.getInt(data, offset);
            reserved = ICCProfile.getInt(data, offset + ICCProfile.int_size);
            CIEilluminant = ICCProfile.getXYZNumber(data, offset + ICCProfile.int_size);
            CIEsurround = ICCProfile.getXYZNumber(data, offset + (ICCProfile.int_size*3));
            illuminant = ICCProfile.getInt(data, offset + (ICCProfile.int_size * 3));
        }
    }
}