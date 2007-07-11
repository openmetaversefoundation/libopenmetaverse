using System;

namespace OpenJPEGNet
{
    /// <summary>
    /// Capability to load TGAs to Bitmap 
    /// </summary>
    public class LoadTGAClass
    {
        struct tgaColorMap
        {
            public ushort FirstEntryIndex;
            public ushort Length;
            public byte EntrySize;

            public void Read(System.IO.BinaryReader br)
            {
                FirstEntryIndex = br.ReadUInt16();
                Length = br.ReadUInt16();
                EntrySize = br.ReadByte();
            }
        }

        struct tgaImageSpec
        {
            public ushort XOrigin;
            public ushort YOrigin;
            public ushort Width;
            public ushort Height;
            public byte PixelDepth;
            public byte Descriptor;

            public void Read(System.IO.BinaryReader br)
            {
                XOrigin = br.ReadUInt16();
                YOrigin = br.ReadUInt16();
                Width = br.ReadUInt16();
                Height = br.ReadUInt16();
                PixelDepth = br.ReadByte();
                Descriptor = br.ReadByte();
            }

            public byte AlphaBits
            {
                get
                {
                    return (byte)(Descriptor & 0xF);
                }
                set
                {
                    Descriptor = (byte)((Descriptor & ~0xF) | (value & 0xF));
                }
            }

            public bool BottomUp
            {
                get
                {
                    return (Descriptor & 0x20) == 0x20;
                }
                set
                {
                    Descriptor = (byte)((Descriptor & ~0x20) | (value ? 0x20 : 0));
                }
            }
        }

        struct tgaHeader
        {
            public byte IdLength;
            public byte ColorMapType;
            public byte ImageType;

            public tgaColorMap ColorMap;
            public tgaImageSpec ImageSpec;

            public void Read(System.IO.BinaryReader br)
            {
                this.IdLength = br.ReadByte();
                this.ColorMapType = br.ReadByte();
                this.ImageType = br.ReadByte();
                this.ColorMap = new tgaColorMap();
                this.ImageSpec = new tgaImageSpec();
                this.ColorMap.Read(br);
                this.ImageSpec.Read(br);
            }

            public bool RleEncoded
            {
                get
                {
                    return ImageType >= 9;
                }
            }
        }

        struct tgaCD
        {
            public uint RMask, GMask, BMask, AMask;
            public byte RShift, GShift, BShift, AShift;
            public uint FinalOr;
            public bool NeedNoConvert;
        }

        static uint UnpackColor(
            uint sourceColor, ref tgaCD cd)
        {
            if (cd.RMask == 0xFF && cd.GMask == 0xFF && cd.BMask == 0xFF)
            {
                // Special case to deal with 8-bit TGA files that we treat as alpha masks
                return sourceColor << 24;
            }
            else
            {
                uint rpermute = (sourceColor << cd.RShift) | (sourceColor >> (32 - cd.RShift));
                uint gpermute = (sourceColor << cd.GShift) | (sourceColor >> (32 - cd.GShift));
                uint bpermute = (sourceColor << cd.BShift) | (sourceColor >> (32 - cd.BShift));
                uint apermute = (sourceColor << cd.AShift) | (sourceColor >> (32 - cd.AShift));
                uint result =
                    (rpermute & cd.RMask) | (gpermute & cd.GMask)
                    | (bpermute & cd.BMask) | (apermute & cd.AMask) | cd.FinalOr;

                return result;
            }
        }

        static unsafe void decodeLine(
            System.Drawing.Imaging.BitmapData b,
            int line,
            int byp,
            byte[] data,
            ref tgaCD cd)
        {
            if (cd.NeedNoConvert)
            {
                // fast copy
                uint* linep = (uint*)((byte*)b.Scan0.ToPointer() + line * b.Stride);
                fixed (byte* ptr = data)
                {
                    uint* sptr = (uint*)ptr;
                    for (int i = 0; i < b.Width; ++i)
                    {
                        linep[i] = sptr[i];
                    }
                }
            }
            else
            {
                byte* linep = (byte*)b.Scan0.ToPointer() + line * b.Stride;

                uint* up = (uint*)linep;

                int rdi = 0;

                fixed (byte* ptr = data)
                {
                    for (int i = 0; i < b.Width; ++i)
                    {
                        uint x = 0;
                        for (int j = 0; j < byp; ++j)
                        {
                            x |= ((uint)ptr[rdi]) << (j << 3);
                            ++rdi;
                        }
                        up[i] = UnpackColor(x, ref cd);
                    }
                }
            }
        }

        static void decodeRle(
            System.Drawing.Imaging.BitmapData b,
            int byp, tgaCD cd, System.IO.BinaryReader br, bool bottomUp)
        {
            try
            {
                int w = b.Width;
                // make buffer larger, so in case of emergency I can decode 
                // over line ends.
                byte[] linebuffer = new byte[(w + 128) * byp];
                int maxindex = w * byp;
                int index = 0;

                for (int j = 0; j < b.Height; ++j)
                {
                    while (index < maxindex)
                    {
                        byte blocktype = br.ReadByte();

                        int bytestoread;
                        int bytestocopy;

                        if (blocktype >= 0x80)
                        {
                            bytestoread = byp;
                            bytestocopy = byp * (blocktype - 0x80);
                        }
                        else
                        {
                            bytestoread = byp * (blocktype + 1);
                            bytestocopy = 0;
                        }

                        //if (index + bytestoread > maxindex)
                        //	throw new System.ArgumentException ("Corrupt TGA");

                        br.Read(linebuffer, index, bytestoread);
                        index += bytestoread;

                        for (int i = 0; i != bytestocopy; ++i)
                        {
                            linebuffer[index + i] = linebuffer[index + i - bytestoread];
                        }
                        index += bytestocopy;
                    }
                    if (!bottomUp)
                        decodeLine(b, b.Height - j - 1, byp, linebuffer, ref cd);
                    else
                        decodeLine(b, j, byp, linebuffer, ref cd);

                    if (index > maxindex)
                    {
                        Array.Copy(linebuffer, maxindex, linebuffer, 0, index - maxindex);
                        index -= maxindex;
                    }
                    else
                        index = 0;

                }
            }
            catch (System.IO.EndOfStreamException)
            {
            }
        }

        static void decodePlain(
            System.Drawing.Imaging.BitmapData b,
            int byp, tgaCD cd, System.IO.BinaryReader br, bool bottomUp)
        {
            int w = b.Width;
            byte[] linebuffer = new byte[w * byp];

            for (int j = 0; j < b.Height; ++j)
            {
                br.Read(linebuffer, 0, w * byp);

                if (!bottomUp)
                    decodeLine(b, b.Height - j - 1, byp, linebuffer, ref cd);
                else
                    decodeLine(b, j, byp, linebuffer, ref cd);
            }
        }

        static void decodeStandard8(
            System.Drawing.Imaging.BitmapData b,
            tgaHeader hdr,
            System.IO.BinaryReader br)
        {
            tgaCD cd = new tgaCD();
            cd.RMask = 0x000000ff;
            cd.GMask = 0x000000ff;
            cd.BMask = 0x000000ff;
            cd.AMask = 0x000000ff;
            cd.RShift = 0;
            cd.GShift = 0;
            cd.BShift = 0;
            cd.AShift = 0;
            cd.FinalOr = 0x00000000;
            if (hdr.RleEncoded)
                decodeRle(b, 1, cd, br, hdr.ImageSpec.BottomUp);
            else
                decodePlain(b, 1, cd, br, hdr.ImageSpec.BottomUp);
        }

        static void decodeSpecial16(
            System.Drawing.Imaging.BitmapData b, tgaHeader hdr, System.IO.BinaryReader br)
        {
            // i must convert the input stream to a sequence of uint values
            // which I then unpack.
            tgaCD cd = new tgaCD();
            cd.RMask = 0x00f00000;
            cd.GMask = 0x0000f000;
            cd.BMask = 0x000000f0;
            cd.AMask = 0xf0000000;
            cd.RShift = 12;
            cd.GShift = 8;
            cd.BShift = 4;
            cd.AShift = 16;
            cd.FinalOr = 0;

            if (hdr.RleEncoded)
                decodeRle(b, 2, cd, br, hdr.ImageSpec.BottomUp);
            else
                decodePlain(b, 2, cd, br, hdr.ImageSpec.BottomUp);
        }

        static void decodeStandard16(
            System.Drawing.Imaging.BitmapData b,
            tgaHeader hdr,
            System.IO.BinaryReader br)
        {
            // i must convert the input stream to a sequence of uint values
            // which I then unpack.
            tgaCD cd = new tgaCD();
            cd.RMask = 0x00f80000;	// from 0xF800
            cd.GMask = 0x0000fc00;	// from 0x07E0
            cd.BMask = 0x000000f8;  // from 0x001F
            cd.AMask = 0x00000000;
            cd.RShift = 8;
            cd.GShift = 5;
            cd.BShift = 3;
            cd.AShift = 0;
            cd.FinalOr = 0xff000000;

            if (hdr.RleEncoded)
                decodeRle(b, 2, cd, br, hdr.ImageSpec.BottomUp);
            else
                decodePlain(b, 2, cd, br, hdr.ImageSpec.BottomUp);
        }


        static void decodeSpecial24(System.Drawing.Imaging.BitmapData b,
            tgaHeader hdr, System.IO.BinaryReader br)
        {
            // i must convert the input stream to a sequence of uint values
            // which I then unpack.
            tgaCD cd = new tgaCD();
            cd.RMask = 0x00f80000;
            cd.GMask = 0x0000fc00;
            cd.BMask = 0x000000f8;
            cd.AMask = 0xff000000;
            cd.RShift = 8;
            cd.GShift = 5;
            cd.BShift = 3;
            cd.AShift = 8;
            cd.FinalOr = 0;

            if (hdr.RleEncoded)
                decodeRle(b, 3, cd, br, hdr.ImageSpec.BottomUp);
            else
                decodePlain(b, 3, cd, br, hdr.ImageSpec.BottomUp);
        }

        static void decodeStandard24(System.Drawing.Imaging.BitmapData b,
            tgaHeader hdr, System.IO.BinaryReader br)
        {
            // i must convert the input stream to a sequence of uint values
            // which I then unpack.
            tgaCD cd = new tgaCD();
            cd.RMask = 0x00ff0000;
            cd.GMask = 0x0000ff00;
            cd.BMask = 0x000000ff;
            cd.AMask = 0x00000000;
            cd.RShift = 0;
            cd.GShift = 0;
            cd.BShift = 0;
            cd.AShift = 0;
            cd.FinalOr = 0xff000000;

            if (hdr.RleEncoded)
                decodeRle(b, 3, cd, br, hdr.ImageSpec.BottomUp);
            else
                decodePlain(b, 3, cd, br, hdr.ImageSpec.BottomUp);
        }

        static void decodeStandard32(System.Drawing.Imaging.BitmapData b,
            tgaHeader hdr, System.IO.BinaryReader br)
        {
            // i must convert the input stream to a sequence of uint values
            // which I then unpack.
            tgaCD cd = new tgaCD();
            cd.RMask = 0x00ff0000;
            cd.GMask = 0x0000ff00;
            cd.BMask = 0x000000ff;
            cd.AMask = 0xff000000;
            cd.RShift = 0;
            cd.GShift = 0;
            cd.BShift = 0;
            cd.AShift = 0;
            cd.FinalOr = 0x00000000;
            cd.NeedNoConvert = true;

            if (hdr.RleEncoded)
                decodeRle(b, 4, cd, br, hdr.ImageSpec.BottomUp);
            else
                decodePlain(b, 4, cd, br, hdr.ImageSpec.BottomUp);
        }


        public static System.Drawing.Size GetTGASize(string filename)
        {
            System.IO.FileStream f = System.IO.File.OpenRead(filename);

            System.IO.BinaryReader br = new System.IO.BinaryReader(f);

            tgaHeader header = new tgaHeader();
            header.Read(br);
            br.Close();

            return new System.Drawing.Size(header.ImageSpec.Width, header.ImageSpec.Height);

        }

        public static System.Drawing.Bitmap LoadTGA(System.IO.Stream source)
        {
            byte[] buffer = new byte[source.Length];
            source.Read(buffer, 0, buffer.Length);

            System.IO.MemoryStream ms = new System.IO.MemoryStream(buffer);

            System.IO.BinaryReader br = new System.IO.BinaryReader(ms);

            tgaHeader header = new tgaHeader();
            header.Read(br);

            if (header.ImageSpec.PixelDepth != 8 &&
                header.ImageSpec.PixelDepth != 16 &&
                header.ImageSpec.PixelDepth != 24 &&
                header.ImageSpec.PixelDepth != 32)
                throw new ArgumentException("Not a supported tga file.");

            if (header.ImageSpec.AlphaBits > 8)
                throw new ArgumentException("Not a supported tga file.");

            if (header.ImageSpec.Width > 4096 ||
                header.ImageSpec.Height > 4096)
                throw new ArgumentException("Image too large.");



            System.Drawing.Bitmap b = new System.Drawing.Bitmap(
                header.ImageSpec.Width, header.ImageSpec.Height);

            System.Drawing.Imaging.BitmapData bd = b.LockBits(new System.Drawing.Rectangle(0, 0, b.Width, b.Height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            switch (header.ImageSpec.PixelDepth)
            {
                case 8:
                    decodeStandard8(bd, header, br);
                    break;
                case 16:
                    if (header.ImageSpec.AlphaBits > 0)
                        decodeSpecial16(bd, header, br);
                    else
                        decodeStandard16(bd, header, br);
                    break;
                case 24:
                    if (header.ImageSpec.AlphaBits > 0)
                        decodeSpecial24(bd, header, br);
                    else
                        decodeStandard24(bd, header, br);
                    break;
                case 32:
                    decodeStandard32(bd, header, br);
                    break;
                default:
                    b.UnlockBits(bd);
                    b.Dispose();
                    return null;
            }
            b.UnlockBits(bd);
            br.Close();
            return b;
        }

        public static unsafe byte[] LoadTGARaw(System.IO.Stream source)
        {
            byte[] buffer = new byte[source.Length];
            source.Read(buffer, 0, buffer.Length);

            System.IO.MemoryStream ms = new System.IO.MemoryStream(buffer);

            System.IO.BinaryReader br = new System.IO.BinaryReader(ms);

            tgaHeader header = new tgaHeader();
            header.Read(br);

            if (header.ImageSpec.PixelDepth != 8 &&
                header.ImageSpec.PixelDepth != 16 &&
                header.ImageSpec.PixelDepth != 24 &&
                header.ImageSpec.PixelDepth != 32)
                throw new ArgumentException("Not a supported tga file.");

            if (header.ImageSpec.AlphaBits > 8)
                throw new ArgumentException("Not a supported tga file.");

            if (header.ImageSpec.Width > 4096 ||
                header.ImageSpec.Height > 4096)
                throw new ArgumentException("Image too large.");

            byte[] decoded = new byte[header.ImageSpec.Width * header.ImageSpec.Height * 4];
            System.Drawing.Imaging.BitmapData bd = new System.Drawing.Imaging.BitmapData();

            fixed (byte* pdecoded = &decoded[0])
            {
                bd.Width = header.ImageSpec.Width;
                bd.Height = header.ImageSpec.Height;
                bd.PixelFormat = System.Drawing.Imaging.PixelFormat.Format32bppPArgb;
                bd.Stride = header.ImageSpec.Width * 4;
                bd.Scan0 = (IntPtr)pdecoded;

                switch (header.ImageSpec.PixelDepth)
                {
                    case 8:
                        decodeStandard8(bd, header, br);
                        break;
                    case 16:
                        if (header.ImageSpec.AlphaBits > 0)
                            decodeSpecial16(bd, header, br);
                        else
                            decodeStandard16(bd, header, br);
                        break;
                    case 24:
                        if (header.ImageSpec.AlphaBits > 0)
                            decodeSpecial24(bd, header, br);
                        else
                            decodeStandard24(bd, header, br);
                        break;
                    case 32:
                        decodeStandard32(bd, header, br);
                        break;
                    default:
                        return null;
                }
            }

            // swap red and blue channels (TGA is BGRA)
            byte tmp;
            for (int i = 0; i < decoded.Length; i += 4)
            {
                tmp = decoded[i];
                decoded[i] = decoded[i + 2];
                decoded[i + 2] = tmp;
            }

            br.Close();
            return decoded;
        }

        public static System.Drawing.Bitmap LoadTGA(string filename)
        {
            try
            {
                using (System.IO.FileStream f = System.IO.File.OpenRead(filename))
                {
                    return LoadTGA(f);
                }
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                return null;	// file not found
            }
            catch (System.IO.FileNotFoundException)
            {
                return null; // file not found
            }
        }
    }
}
