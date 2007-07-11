using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace OpenJPEGNet
{
    public class OpenJPEG
    {
        // This structure is used to pass images back and forth for both encoding and decoding
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct LibslImage
        {
            public IntPtr encoded;     // encoded image data
            public int length;         // encoded image length

            public IntPtr decoded;     // decoded image data (8 bits per component, RGBA order)
            public int width;          // width of decoded image
            public int height;         // height of decoded image
            public int components;     // number of decoded components
        }

        // allocate encoded buffer based on length field
        [DllImport("openjpeg-libsl.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool LibslAllocEncoded(ref LibslImage image);

        // allocate decoded buffer based on width and height fields
        [DllImport("openjpeg-libsl.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool LibslAllocDecoded(ref LibslImage image);
        
        // free buffers
        [DllImport("openjpeg-libsl.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool LibslFree(ref LibslImage image);
        
        // encode raw to jpeg2000
        [DllImport("openjpeg-libsl.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool LibslEncode(ref LibslImage image, bool lossless);

        // decode jpeg2000 to raw
        [DllImport("openjpeg-libsl.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool LibslDecode(ref LibslImage image);
        
        // encode 
        public static byte[] Encode(byte[] decoded, int width, int height, int components, bool lossless)
        {
            if (decoded.Length != width * height * components)
                throw new ArgumentException("Length of decoded buffer does not match parameters");
            
            LibslImage image = new LibslImage();

            // allocate and copy to input buffer
            image.width = width;
            image.height = height;
            image.components = components;
            LibslAllocDecoded(ref image);
            Marshal.Copy(decoded, 0, image.decoded, width * height * components);

            // codec will allocate output buffer
            LibslEncode(ref image, lossless);

            // copy output buffer
            byte[] encoded = new byte[image.length];
            Marshal.Copy(image.encoded, encoded, 0, image.length);

            // free buffers
            LibslFree(ref image);

            return encoded;
        }

        public static byte[] Encode(byte[] decoded, int width, int height, int components)
        {
            return Encode(decoded, width, height, components, false);
        }

        public static byte[] Decode(byte[] encoded, out int width, out int height, out int components)
        {
            LibslImage image = new LibslImage();

            // allocate and copy to input buffer
            image.length = encoded.Length;
            LibslAllocEncoded(ref image);
            Marshal.Copy(encoded, 0, image.encoded, encoded.Length);

            // codec will allocate output buffer
            LibslDecode(ref image);

            // copy output buffer
            byte[] decoded = new byte[image.width * image.height * image.components];
            Marshal.Copy(image.decoded, decoded, 0, image.width * image.height * image.components);

            // copy image dimensions
            width = image.width;
            height = image.height;
            components = image.components;

            // free buffers
            LibslFree(ref image);

            return decoded;
        }

        public const int TGA_HEADER_SIZE = 32;
        
        public static byte[] DecodeToTGA(byte[] encoded)
        {
            int width, height, components;
            byte[] decoded = Decode(encoded, out width, out height, out components);

            byte[] tga = new byte[width * height * 4 + TGA_HEADER_SIZE];
            int di = 0;
            tga[di++] = 0; // idlength
            tga[di++] = 0; // colormaptype = 0: no colormap
            tga[di++] = 2; // image type = 2: uncompressed RGB
            tga[di++] = 0; // color map spec is five zeroes for no color map
            tga[di++] = 0; // color map spec is five zeroes for no color map
            tga[di++] = 0; // color map spec is five zeroes for no color map
            tga[di++] = 0; // color map spec is five zeroes for no color map
            tga[di++] = 0; // color map spec is five zeroes for no color map
            tga[di++] = 0; // x origin = two bytes
            tga[di++] = 0; // x origin = two bytes
            tga[di++] = 0; // y origin = two bytes
            tga[di++] = 0; // y origin = two bytes
            tga[di++] = (byte)(width & 0xFF); // width - low byte
            tga[di++] = (byte)(width >> 8); // width - hi byte
            tga[di++] = (byte)(height & 0xFF); // height - low byte
            tga[di++] = (byte)(height >> 8); // height - hi byte
            tga[di++] = 32; // 32 bits per pixel
            tga[di++] = 40; // image descriptor byte

            int si = 0;
            
            switch (components)
            {
                case 5:
                    for (int i = 0; i < (width * height); i++)
                    {
                        tga[di++] = decoded[si + 2]; // blue
                        tga[di++] = decoded[si + 1]; // green
                        tga[di++] = decoded[si + 0]; // red
                        tga[di++] = decoded[si + 4]; // alpha
                        si += 5;
                    }
                    break;
                case 4:
                    for (int i = 0; i < (width * height); i++)
                    {
                        tga[di++] = decoded[si + 2]; // blue
                        tga[di++] = decoded[si + 1]; // green
                        tga[di++] = decoded[si + 0]; // red
                        tga[di++] = decoded[si + 3]; // alpha
                        si += 4;
                    }
                    break;
                case 3:
                    for (int i = 0; i < (width * height); i++)
                    {
                        tga[di++] = decoded[si + 2]; // blue
                        tga[di++] = decoded[si + 1]; // green
                        tga[di++] = decoded[si + 0]; // red
                        tga[di++] = 0xFF; // alpha
                        si += 3;
                    }
                    break;
                case 2:
                    for (int i = 0; i < (width * height); i++)
                    {
                        tga[di++] = decoded[si + 0]; // blue
                        tga[di++] = decoded[si + 0]; // green
                        tga[di++] = decoded[si + 0]; // red
                        tga[di++] = decoded[si + 1]; // alpha
                        si += 2;
                    }
                    break;
                case 1:
                    for (int i = 0; i < (width * height); i++)
                    {
                        tga[di++] = decoded[si]; // blue
                        tga[di++] = decoded[si]; // green
                        tga[di++] = decoded[si]; // red
                        tga[di++] = 0xFF; // alpha
                        si++;
                    }
                    break;

                default:
                    throw new Exception("Invalid number of components: " + components);
            }

            return tga;
        }

        public static Image DecodeToImage(byte[] encoded)
        {
            return LoadTGAClass.LoadTGA(new MemoryStream(DecodeToTGA(encoded)));
        }

        public unsafe static byte[] EncodeFromImage(Bitmap bitmap, bool lossless)
        {
            int numcomps;
            BitmapData bd;
            byte[] decoded;
            int i = 0;

            int bitmapWidth = bitmap.Width;
            int bitmapHeight = bitmap.Height;

            if ((bitmap.PixelFormat & PixelFormat.Alpha) != 0 || (bitmap.PixelFormat & PixelFormat.PAlpha) != 0)
            {
                // four layers, RGBA
                numcomps = 4;
                decoded = new byte[bitmapWidth * bitmapHeight * numcomps];
                bd = bitmap.LockBits(new Rectangle(0, 0, bitmapWidth, bitmapHeight), ImageLockMode.ReadOnly,
                    PixelFormat.Format32bppArgb);

                for (int y = 0; y < bitmapHeight; y++)
                {
                    for (int x = 0; x < bitmapWidth; x++)
                    {
                        byte* pixel = (byte*)bd.Scan0;
                        pixel += (y * bitmapWidth + x) * numcomps;

                        // GDI+ gives us BGRA and we need to turn that in to RGBA
                        decoded[i++] = *(pixel + 2);
                        decoded[i++] = *(pixel + 1);
                        decoded[i++] = *(pixel);
                        decoded[i++] = *(pixel + 3);
                        pixel += 4;
                    }
                }
            }
            else if (bitmap.PixelFormat == PixelFormat.Format16bppGrayScale)
            {
                // one layer
                numcomps = 1;
                decoded = new byte[bitmapWidth * bitmapHeight * numcomps];
                bd = bitmap.LockBits(new Rectangle(0, 0, bitmapWidth, bitmapHeight), ImageLockMode.ReadOnly,
                    PixelFormat.Format16bppGrayScale);

                for (int y = 0; y < bitmapHeight; y++)
                {
                    for (int x = 0; x < bitmapWidth; x++)
                    {
                        byte* pixel = (byte*)bd.Scan0;
                        pixel += (y * bitmapWidth + x) * numcomps;

                        // turn 16 bit data in to 8 bit data (TODO: Does this work?)
                        decoded[i++] = *(pixel);
                        pixel += 2;
                    }
                }
            }
            else
            {
                // three layers, RGB
                numcomps = 3;
                decoded = new byte[bitmapWidth * bitmapHeight * numcomps];
                bd = bitmap.LockBits(new Rectangle(0, 0, bitmapWidth, bitmapHeight), ImageLockMode.ReadOnly,
                    PixelFormat.Format24bppRgb);

                for (int y = 0; y < bitmapHeight; y++)
                {
                    for (int x = 0; x < bitmapWidth; x++)
                    {
                        byte* pixel = (byte*)bd.Scan0;
                        pixel += (y * bitmapWidth + x) * numcomps;

                        decoded[i++] = *(pixel + 2);
                        decoded[i++] = *(pixel + 1);
                        decoded[i++] = *(pixel + 0);
                        pixel += 3;
                    }
                }
            }

            bitmap.UnlockBits(bd);
            byte[] encoded = Encode(decoded, bitmap.Width, bitmap.Height, numcomps, lossless);
            return encoded;
        }

        public static byte[] ConvertComponents(byte[] source, int width, int height, int sourceComponents, int destComponents)
        {
            if (sourceComponents == destComponents)
                return source;
            
            int x, y, si = 0, di = 0;
            byte r, g, b, alpha, bump;
            byte[] dest = new byte[width*height*destComponents];

            for (y = 0; y < height; y++)
            {
                for (x = 0; x < width; x++)
                {
                    si = (y * width + x) * sourceComponents;
                    di = (y * width + x) * destComponents;

                    switch (sourceComponents)
                    {
                        case 1:
                            r = source[si];
                            g = source[si];
                            b = source[si];
                            bump = 0;
                            alpha = 255;
                            break;

                        case 3:
                            r = source[si];
                            g = source[si + 1];
                            b = source[si + 2];
                            bump = 0;
                            alpha = 255;
                            break;

                        case 4:
                            r = source[si];
                            g = source[si + 1];
                            b = source[si + 2];
                            bump = 0;
                            alpha = source[si + 3];
                            break;

                        case 5:
                            r = source[si];
                            g = source[si + 1];
                            b = source[si + 2];
                            bump = source[si + 3];
                            alpha = source[si + 4];
                            break;

                        default:
                            throw new ArgumentException("Invalid number of source components " + sourceComponents);
                    }

                    switch (destComponents)
                    {
                        case 1:
                            dest[di] = (byte)(((int)r + g + b) / 3);
                            break;

                        case 3:
                            dest[di] = r;
                            dest[di + 1] = g;
                            dest[di + 2] = b;
                            break;

                        case 4:
                            dest[di] = r;
                            dest[di + 1] = g;
                            dest[di + 2] = b;
                            dest[di + 3] = alpha;
                            break;

                        case 5:
                            dest[di] = r;
                            dest[di + 1] = g;
                            dest[di + 2] = b;
                            dest[di + 3] = bump;
                            dest[di + 4] = alpha;
                            break;

                        default:
                            throw new ArgumentException("Invalid number of dest components " + destComponents);
                    }
                }
            }

            return dest;
        }
    }
}
