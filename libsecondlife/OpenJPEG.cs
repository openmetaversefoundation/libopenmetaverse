/*
 * Copyright (c) 2007-2008, the libsecondlife development team
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the Second Life Reverse Engineering Team nor the names
 *   of its contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using libsecondlife;

namespace OpenJPEGNet
{
#if !NO_UNSAFE
    /// <summary>
    /// A Wrapper around openjpeg to encode and decode images to and from byte arrays
    /// </summary>
    public class OpenJPEG
    {
        // This structure is used to marshal both encoded and decoded images
        // MUST MATCH THE STRUCT IN libsl.h!
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct MarshalledImage
        {
            public IntPtr encoded;             // encoded image data
            public int length;                 // encoded image length
            public int dummy; // padding for 64-bit alignment

            public IntPtr decoded;             // decoded image, contiguous components
            public int width;                  // width of decoded image
            public int height;                 // height of decoded image
            public int components;             // component count
        }

        // allocate encoded buffer based on length field
        [DllImport("openjpeg-libsl.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool LibslAllocEncoded(ref MarshalledImage image);

        // allocate decoded buffer based on width and height fields
        [DllImport("openjpeg-libsl.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool LibslAllocDecoded(ref MarshalledImage image);
        
        // free buffers
        [DllImport("openjpeg-libsl.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool LibslFree(ref MarshalledImage image);
        
        // encode raw to jpeg2000
        [DllImport("openjpeg-libsl.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool LibslEncode(ref MarshalledImage image, bool lossless);

        // decode jpeg2000 to raw
        [DllImport("openjpeg-libsl.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool LibslDecode(ref MarshalledImage image);
        
        /// <summary>
        /// Encode a <seealso cref="libsecondlife.Image"/> object into a byte array
        /// </summary>
        /// <param name="image">The <seealso cref="libsecondlife.Image"/> object to encode</param>
        /// <param name="lossless">true to enable lossless conversion, only useful for small images ie: sculptmaps</param>
        /// <returns>A byte array containing the encoded Image object</returns>
        public static byte[] Encode(libsecondlife.Image image, bool lossless)
        {
            if (
                (image.Channels & ImageChannels.Color) == 0 ||
                ((image.Channels & ImageChannels.Bump) != 0 && (image.Channels & ImageChannels.Alpha) == 0))
                throw new ArgumentException("JPEG2000 encoding is not supported for this channel combination");
            
            MarshalledImage marshalled = new MarshalledImage();

            // allocate and copy to input buffer
            marshalled.width = image.Width;
            marshalled.height = image.Height;
            marshalled.components = 3;
            if ((image.Channels & ImageChannels.Alpha) != 0) marshalled.components++;
            if ((image.Channels & ImageChannels.Bump) != 0) marshalled.components++;

            if (!LibslAllocDecoded(ref marshalled))
                throw new Exception("LibslAllocDecoded failed");

            int n = image.Width * image.Height;

            if ((image.Channels & ImageChannels.Color) != 0)
            {
                Marshal.Copy(image.Red, 0, marshalled.decoded, n);
                Marshal.Copy(image.Green, 0, (IntPtr)(marshalled.decoded.ToInt64() + n), n);
                Marshal.Copy(image.Blue, 0, (IntPtr)(marshalled.decoded.ToInt64() + n * 2), n);
            }

            if ((image.Channels & ImageChannels.Alpha) != 0) Marshal.Copy(image.Alpha, 0, (IntPtr)(marshalled.decoded.ToInt64() + n * 3), n);
            if ((image.Channels & ImageChannels.Bump) != 0) Marshal.Copy(image.Bump, 0, (IntPtr)(marshalled.decoded.ToInt64() + n * 4), n);

            // codec will allocate output buffer
            if (!LibslEncode(ref marshalled, lossless))
                throw new Exception("LibslEncode failed");

            // copy output buffer
            byte[] encoded = new byte[marshalled.length];
            Marshal.Copy(marshalled.encoded, encoded, 0, marshalled.length);

            // free buffers
            LibslFree(ref marshalled);

            return encoded;
        }

        /// <summary>
        /// Encode a <seealso cref="libsecondlife.Image"/> object into a byte array
        /// </summary>
        /// <param name="image">The <seealso cref="libsecondlife.Image"/> object to encode</param>
        /// <returns>a byte array of the encoded image</returns>
        public static byte[] Encode(libsecondlife.Image image)
        {
            return Encode(image, false);
        }

        /// <summary>
        /// Decode a <seealso cref="libsecondlife.Image"/> object from a byte array
        /// </summary>
        /// <param name="encoded">The encoded byte array to decode</param>
        /// <returns>A <seealso cref="libsecondlife.image"/> object</returns>
        public static libsecondlife.Image Decode(byte[] encoded)
        {
            MarshalledImage marshalled = new MarshalledImage();

            // allocate and copy to input buffer
            marshalled.length = encoded.Length;
            LibslAllocEncoded(ref marshalled);
            Marshal.Copy(encoded, 0, marshalled.encoded, encoded.Length);

            // codec will allocate output buffer
            LibslDecode(ref marshalled);

            libsecondlife.Image image;
            int n = marshalled.width * marshalled.height;

            switch (marshalled.components)
            {
                case 1: // grayscale
                    image = new libsecondlife.Image(marshalled.width, marshalled.height, ImageChannels.Color);
                    Marshal.Copy(marshalled.decoded, image.Red, 0, n);
                    Array.Copy(image.Red, image.Green, n);
                    Array.Copy(image.Red, image.Blue, n);
                    break;

                case 2: // grayscale + alpha
                    image = new libsecondlife.Image(marshalled.width, marshalled.height, ImageChannels.Color | ImageChannels.Alpha);
                    Marshal.Copy(marshalled.decoded, image.Red, 0, n);
                    Array.Copy(image.Red, image.Green, n);
                    Array.Copy(image.Red, image.Blue, n);
                    Marshal.Copy((IntPtr)(marshalled.decoded.ToInt64() + n), image.Alpha, 0, n);
                    break;

                case 3: // RGB
                    image = new libsecondlife.Image(marshalled.width, marshalled.height, ImageChannels.Color);
                    Marshal.Copy(marshalled.decoded, image.Red, 0, n);
                    Marshal.Copy((IntPtr)(marshalled.decoded.ToInt64() + n), image.Green, 0, n);
                    Marshal.Copy((IntPtr)(marshalled.decoded.ToInt64() + n * 2), image.Blue, 0, n);
                    break;

                case 4: // RGBA
                    image = new libsecondlife.Image(marshalled.width, marshalled.height, ImageChannels.Color | ImageChannels.Alpha);
                    Marshal.Copy(marshalled.decoded, image.Red, 0, n);
                    Marshal.Copy((IntPtr)(marshalled.decoded.ToInt64() + n), image.Green, 0, n);
                    Marshal.Copy((IntPtr)(marshalled.decoded.ToInt64() + n * 2), image.Blue, 0, n);
                    Marshal.Copy((IntPtr)(marshalled.decoded.ToInt64() + n * 3), image.Alpha, 0, n);
                    break;

                case 5: // RGBBA
                    image = new libsecondlife.Image(marshalled.width, marshalled.height, ImageChannels.Color | ImageChannels.Alpha | ImageChannels.Bump);
                    Marshal.Copy(marshalled.decoded, image.Red, 0, n);
                    Marshal.Copy((IntPtr)(marshalled.decoded.ToInt64() + n), image.Green, 0, n);
                    Marshal.Copy((IntPtr)(marshalled.decoded.ToInt64() + n * 2), image.Blue, 0, n);
                    Marshal.Copy((IntPtr)(marshalled.decoded.ToInt64() + n * 3), image.Bump, 0, n); // bump comes before alpha in 5 channel encode
                    Marshal.Copy((IntPtr)(marshalled.decoded.ToInt64() + n * 4), image.Alpha, 0, n);
                    break;

                default:
                    throw new Exception("Decoded image with unhandled number of components (" + marshalled.components + ")");
            }

            // free buffers
            LibslFree(ref marshalled);

            return image;
        }

        /// <summary>
        /// TGA Header size
        /// </summary>
        public const int TGA_HEADER_SIZE = 32;
        
        /// <summary>
        /// Decode an encoded <seealso cref="libsecondlife.Image"/> byte array to a TGA byte array
        /// </summary>
        /// <param name="encoded">The encoded image</param>
        /// <param name="image">A <seealso cref="libsecondlife.Image"/> object</param>
        /// <returns>A TGA decoded byte array containing the encoded image</returns>
        public static byte[] DecodeToTGA(byte[] encoded)
        {
            return Decode(encoded).ExportTGA();
        }

        /// <summary>
        /// Decode an encoded image byte array to a <seealso cref="System.Drawing.Image"/> which can be used
        /// directly in Windows Forms or by the System.Drawing.Image class
        /// </summary>
        /// <param name="encoded">A encoded byte array containing the source image to decode</param>
        /// <param name="image">A <seealso cref="libsecondlife.Image"/> object</param>
        /// <returns>A <seealso cref="System.Drawing.Image"/> object</returns>
        public static System.Drawing.Image DecodeToImage(byte[] encoded)
        {
            return LoadTGAClass.LoadTGA(new MemoryStream(DecodeToTGA(encoded)));
        }

        /// <summary>
        /// Encode a <seealso cref="System.Drawing.Bitmap"/> object into a byte array
        /// </summary>
        /// <param name="bitmap">The source <seealso cref="System.Drawing.Bitmap"/> object to encode</param>
        /// <param name="lossless">true to enable lossless decoding</param>
        /// <returns>A byte array containing the source Bitmap object</returns>
        public unsafe static byte[] EncodeFromImage(Bitmap bitmap, bool lossless)
        {
            BitmapData bd;
            libsecondlife.Image decoded;

            int bitmapWidth = bitmap.Width;
            int bitmapHeight = bitmap.Height;
            int pixelCount = bitmapWidth * bitmapHeight;
            int i;

            if ((bitmap.PixelFormat & PixelFormat.Alpha) != 0 || (bitmap.PixelFormat & PixelFormat.PAlpha) != 0)
            {
                // four layers, RGBA
                decoded = new libsecondlife.Image(bitmapWidth, bitmapHeight, ImageChannels.Color | ImageChannels.Alpha);
                bd = bitmap.LockBits(new Rectangle(0, 0, bitmapWidth, bitmapHeight), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                byte* pixel = (byte*)bd.Scan0;

                for (i = 0; i < pixelCount; i++)
                {
                    // GDI+ gives us BGRA and we need to turn that in to RGBA
                    decoded.Blue[i] = *(pixel++);
                    decoded.Green[i] = *(pixel++);
                    decoded.Red[i] = *(pixel++);
                    decoded.Alpha[i] = *(pixel++);
                }
            }
            else if (bitmap.PixelFormat == PixelFormat.Format16bppGrayScale)
            {
                // one layer
                decoded = new libsecondlife.Image(bitmapWidth, bitmapHeight, ImageChannels.Color);
                bd = bitmap.LockBits(new Rectangle(0, 0, bitmapWidth, bitmapHeight), ImageLockMode.ReadOnly, PixelFormat.Format16bppGrayScale);
                byte* pixel = (byte*)bd.Scan0;

                for (i = 0; i < pixelCount; i++)
                {
                    // turn 16 bit data in to 8 bit data (TODO: Does this work?)
                    decoded.Red[i] = *(pixel);
                    decoded.Green[i] = *(pixel);
                    decoded.Blue[i] = *(pixel);
                    pixel += 2;
                }
            }
            else
            {
                // three layers, RGB
                decoded = new libsecondlife.Image(bitmapWidth, bitmapHeight, ImageChannels.Color);
                bd = bitmap.LockBits(new Rectangle(0, 0, bitmapWidth, bitmapHeight), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                byte* pixel = (byte*)bd.Scan0;

                for (i = 0; i < pixelCount; i++)
                {
                    decoded.Blue[i] = *(pixel++);
                    decoded.Green[i] = *(pixel++);
                    decoded.Red[i] = *(pixel++);
                }
            }

            bitmap.UnlockBits(bd);
            byte[] encoded = Encode(decoded, lossless);
            return encoded;
        }
    }

#endif
}
