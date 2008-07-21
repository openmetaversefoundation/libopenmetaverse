/*
 * Copyright (c) 2007, Second Life Reverse Engineering Team
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

namespace libsecondlife.Imaging
{
    public class ManagedImage
    {
        [Flags]
        public enum ImageChannels
        {
            Gray = 1,
            Color = 2,
            Alpha = 4,
            Bump = 8
        };

        public enum ImageResizeAlgorithm
        {
            NearestNeighbor
        }

        /// <summary>
        /// Image width
        /// </summary>
        public int Width;

        /// <summary>
        /// Image height
        /// </summary>
        public int Height;
        
        /// <summary>
        /// Image channel flags
        /// </summary>
        public ImageChannels Channels;

        /// <summary>
        /// Red channel data
        /// </summary>
        public byte[] Red;
        
        /// <summary>
        /// Green channel data
        /// </summary>
        public byte[] Green;
        
        /// <summary>
        /// Blue channel data
        /// </summary>
        public byte[] Blue;

        /// <summary>
        /// Alpha channel data
        /// </summary>
        public byte[] Alpha;
        
        /// <summary>
        /// Bump channel data
        /// </summary>
        public byte[] Bump;

        /// <summary>
        /// Create a new blank image
        /// </summary>
        /// <param name="width">width</param>
        /// <param name="height">height</param>
        /// <param name="channels">channel flags</param>
        public ManagedImage(int width, int height, ImageChannels channels)
        {
            Width = width;
            Height = height;
            Channels = channels;

            int n = width * height;

            if ((channels & ImageChannels.Gray) != 0)
            {
                Red = new byte[n];
            }
            else if ((channels & ImageChannels.Color) != 0)
            {
                Red = new byte[n];
                Green = new byte[n];
                Blue = new byte[n];
            }

            if ((channels & ImageChannels.Alpha) != 0)
                Alpha = new byte[n];

            if ((channels & ImageChannels.Bump) != 0)
                Bump = new byte[n];
        }

#if !NO_UNSAFE
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        public ManagedImage(System.Drawing.Bitmap bitmap)
        {
            Width = bitmap.Width;
            Height = bitmap.Height;

            int pixelCount = Width * Height;

            if (bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb)
            {
                Channels = ImageChannels.Alpha & ImageChannels.Color;
                Red = new byte[pixelCount];
                Green = new byte[pixelCount];
                Blue = new byte[pixelCount];
                Alpha = new byte[pixelCount];

                System.Drawing.Imaging.BitmapData bd = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, Width, Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                unsafe
                {
                    byte* pixel = (byte*)bd.Scan0;

                    for (int i = 0; i < pixelCount; i++)
                    {
                        // GDI+ gives us BGRA and we need to turn that in to RGBA
                        Blue[i] = *(pixel++);
                        Green[i] = *(pixel++);
                        Red[i] = *(pixel++);
                        Alpha[i] = *(pixel++);
                    }
                }

                bitmap.UnlockBits(bd);
            }
            else if (bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format16bppGrayScale)
            {
                Channels = ImageChannels.Gray;
                Red = new byte[pixelCount];

                throw new NotImplementedException("16bpp grayscale image support is incomplete");
            }
            else if (bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb)
            {
                Channels = ImageChannels.Color;
                Red = new byte[pixelCount];
                Green = new byte[pixelCount];
                Blue = new byte[pixelCount];

                System.Drawing.Imaging.BitmapData bd = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, Width, Height),
                        System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                unsafe
                {
                    byte* pixel = (byte*)bd.Scan0;

                    for (int i = 0; i < pixelCount; i++)
                    {
                        // GDI+ gives us BGR and we need to turn that in to RGB
                        Blue[i] = *(pixel++);
                        Green[i] = *(pixel++);
                        Red[i] = *(pixel++);
                    }
                }

                bitmap.UnlockBits(bd);
            }
            else
            {
                throw new NotSupportedException("Unrecognized pixel format: " + bitmap.PixelFormat.ToString());
            }
        }
#endif

        /// <summary>
        /// Convert the channels in the image. Channels are created or destroyed as required.
        /// </summary>
        /// <param name="channels">new channel flags</param>
        public void ConvertChannels(ImageChannels channels)
        {
            if (Channels == channels)
                return;

            int n = Width * Height;
            ImageChannels add = Channels ^ channels & channels;
            ImageChannels del = Channels ^ channels & Channels;

            if ((add & ImageChannels.Color) != 0)
            {
                Red = new byte[n];
                Green = new byte[n];
                Blue = new byte[n];
            }
            else if ((del & ImageChannels.Color) != 0)
            {
                Red = null;
                Green = null;
                Blue = null;
            }

            if ((add & ImageChannels.Alpha) != 0)
            {
                Alpha = new byte[n];
                FillArray(Alpha, 255);
            }
            else if ((del & ImageChannels.Alpha) != 0)
                Alpha = null;

            if ((add & ImageChannels.Bump) != 0)
                Bump = new byte[n];
            else if ((del & ImageChannels.Bump) != 0)
                Bump = null;

            Channels = channels;
        }

        /// <summary>
        /// Resize or stretch the image using nearest neighbor (ugly) resampling
        /// </summary>
        /// <param name="width">new width</param>
        /// <param name="height">new height</param>
        public void ResizeNearestNeighbor(int width, int height)
        {
            if (width == Width && height == Height)
                return;

            byte[]
                red = null, 
                green = null, 
                blue = null, 
                alpha = null, 
                bump = null;
            int n = width * height;
            int di = 0, si;

            if (Red != null) red = new byte[n];
            if (Green != null) green = new byte[n];
            if (Blue != null) blue = new byte[n];
            if (Alpha != null) alpha = new byte[n];
            if (Bump != null) bump = new byte[n];
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    si = (y * Height / height) * Width + (x * Width / width);
                    if (Red != null) red[di] = Red[si];
                    if (Green != null) green[di] = Green[si];
                    if (Blue != null) blue[di] = Blue[si];
                    if (Alpha != null) alpha[di] = Alpha[si];
                    if (Bump != null) bump[di] = Bump[si];
                    di++;
                }
            }

            Width = width;
            Height = height;
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = alpha;
            Bump = bump;
        }

        public byte[] ExportRaw()
        {
            int n = Width * Height;
            int di = 0;
            byte[] raw = new byte[n * 4];

            if ((Channels & ImageChannels.Alpha) != 0)
            {
                if ((Channels & ImageChannels.Color) != 0)
                {
                    // RGBA
                    for (int i = 0; i < n; i++)
                    {
                        raw[di++] = Red[i];
                        raw[di++] = Green[i];
                        raw[di++] = Blue[i];
                        raw[di++] = Alpha[i];
                    }
                }
                else
                {
                    // Alpha only
                    for (int i = 0; i < n; i++)
                    {
                        raw[di++] = Alpha[i];
                        raw[di++] = Alpha[i];
                        raw[di++] = Alpha[i];
                        raw[di++] = Byte.MaxValue;
                    }
                }
            }
            else
            {
                // RGB
                for (int i = 0; i < n; i++)
                {
                    raw[di++] = Red[i];
                    raw[di++] = Green[i];
                    raw[di++] = Blue[i];
                    raw[di++] = Byte.MaxValue;
                }
            }

            return raw;
        }

        public byte[] ExportTGA()
        {
            byte[] tga = new byte[Width * Height * ((Channels & ImageChannels.Alpha) == 0 ? 3 : 4) + 32];
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
            tga[di++] = (byte)(Width & 0xFF); // width - low byte
            tga[di++] = (byte)(Width >> 8); // width - hi byte
            tga[di++] = (byte)(Height & 0xFF); // height - low byte
            tga[di++] = (byte)(Height >> 8); // height - hi byte
            tga[di++] = (byte)((Channels & ImageChannels.Alpha) == 0 ? 24 : 32); // 24/32 bits per pixel
            tga[di++] = (byte)((Channels & ImageChannels.Alpha) == 0 ? 32 : 40); // image descriptor byte

            int n = Width * Height;

            if ((Channels & ImageChannels.Alpha) != 0)
            {
                if ((Channels & ImageChannels.Color) != 0)
                {
                    // RGBA
                    for (int i = 0; i < n; i++)
                    {
                        tga[di++] = Blue[i];
                        tga[di++] = Green[i];
                        tga[di++] = Red[i];
                        tga[di++] = Alpha[i];
                    }
                }
                else
                {
                    // Alpha only
                    for (int i = 0; i < n; i++)
                    {
                        tga[di++] = Alpha[i];
                        tga[di++] = Alpha[i];
                        tga[di++] = Alpha[i];
                        tga[di++] = Byte.MaxValue;
                    }
                }
            }
            else
            {
                // RGB
                for (int i = 0; i < n; i++)
                {
                    tga[di++] = Blue[i];
                    tga[di++] = Green[i];
                    tga[di++] = Red[i];
                }
            }

            return tga;
        }

        private static void FillArray(byte[] array, byte value)
        {
            if (array != null)
            {
                for (int i = 0; i < array.Length; i++)
                    array[i] = value;
            }
        }

        public void Clear()
        {
            FillArray(Red, 0);
            FillArray(Green, 0);
            FillArray(Blue, 0);
            FillArray(Alpha, 0);
            FillArray(Bump, 0);
        }

        public ManagedImage Clone()
        {
            ManagedImage image = new ManagedImage(Width, Height, Channels);
            if (Red != null) image.Red = (byte[])Red.Clone();
            if (Green != null) image.Green = (byte[])Green.Clone();
            if (Blue != null) image.Blue = (byte[])Blue.Clone();
            if (Alpha != null) image.Alpha = (byte[])Alpha.Clone();
            if (Bump != null) image.Bump = (byte[])Bump.Clone();
            return image;
        }
    }
}
