/*
 * Copyright (c) 2007-2008, openmetaverse.org
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.org nor the names
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

namespace OpenMetaverse.Imaging
{
#if !NO_UNSAFE
    /// <summary>
    /// A Wrapper around openjpeg to encode and decode images to and from byte arrays
    /// </summary>
    public class OpenJPEG
    {
        /// <summary>TGA Header size</summary>
        public const int TGA_HEADER_SIZE = 32;

        /// <summary>
        /// Defines the beginning and ending file positions of a layer in an
        /// LRCP-progression JPEG2000 file
        /// </summary>
        public struct J2KLayerInfo
        {
            public int Start;
            public int End;

            public int Size { get { return End - Start; } }

            public override string ToString()
            {
                return String.Format("Start: {0} End: {1} Size: {2}", Start, End, Size);
            }
        }

        /// <summary>
        /// This structure is used to marshal both encoded and decoded images.
        /// MUST MATCH THE STRUCT IN dotnet.h!
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct MarshalledImage
        {
            public IntPtr encoded;             // encoded image data
            public int length;                 // encoded image length
            public int dummy;                  // padding for 64-bit alignment

            public IntPtr decoded;             // decoded image, contiguous components

            public int width;                  // width of decoded image
            public int height;                 // height of decoded image
            public int layers;                 // layer count
            public int resolutions;            // resolution count
            public int components;             // component count
            public int packet_count;           // packet count
            public IntPtr packets;             // pointer to the packets array
        }

        /// <summary>
        /// Information about a single packet in a JPEG2000 stream
        /// </summary>
        private struct MarshalledPacket
        {
            /// <summary>Packet start position</summary>
            public int start_pos;
            /// <summary>Packet header end position</summary>
            public int end_ph_pos;
            /// <summary>Packet end position</summary>
            public int end_pos;

            public override string ToString()
            {
                return String.Format("start_pos: {0} end_ph_pos: {1} end_pos: {2}",
                    start_pos, end_ph_pos, end_pos);
            }
        }

        // allocate encoded buffer based on length field
        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("openjpeg-dotnet.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool DotNetAllocEncoded(ref MarshalledImage image);

        // allocate decoded buffer based on width and height fields
        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("openjpeg-dotnet.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool DotNetAllocDecoded(ref MarshalledImage image);
        
        // free buffers
        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("openjpeg-dotnet.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool DotNetFree(ref MarshalledImage image);
        
        // encode raw to jpeg2000
        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("openjpeg-dotnet.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool DotNetEncode(ref MarshalledImage image, bool lossless);

        // decode jpeg2000 to raw
        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("openjpeg-dotnet.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool DotNetDecode(ref MarshalledImage image);

        // decode jpeg2000 to raw, get jpeg2000 file info
        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("openjpeg-dotnet.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool DotNetDecodeWithInfo(ref MarshalledImage image);
        
        /// <summary>
        /// Encode a <seealso cref="ManagedImage"/> object into a byte array
        /// </summary>
        /// <param name="image">The <seealso cref="ManagedImage"/> object to encode</param>
        /// <param name="lossless">true to enable lossless conversion, only useful for small images ie: sculptmaps</param>
        /// <returns>A byte array containing the encoded Image object</returns>
        public static byte[] Encode(ManagedImage image, bool lossless)
        {
            if (
                (image.Channels & ManagedImage.ImageChannels.Color) == 0 ||
                ((image.Channels & ManagedImage.ImageChannels.Bump) != 0 && (image.Channels & ManagedImage.ImageChannels.Alpha) == 0))
                throw new ArgumentException("JPEG2000 encoding is not supported for this channel combination");
            
            MarshalledImage marshalled = new MarshalledImage();

            // allocate and copy to input buffer
            marshalled.width = image.Width;
            marshalled.height = image.Height;
            marshalled.components = 3;
            if ((image.Channels & ManagedImage.ImageChannels.Alpha) != 0) marshalled.components++;
            if ((image.Channels & ManagedImage.ImageChannels.Bump) != 0) marshalled.components++;

            if (!DotNetAllocDecoded(ref marshalled))
                throw new Exception("LibslAllocDecoded failed");

            int n = image.Width * image.Height;

            if ((image.Channels & ManagedImage.ImageChannels.Color) != 0)
            {
                Marshal.Copy(image.Red, 0, marshalled.decoded, n);
                Marshal.Copy(image.Green, 0, (IntPtr)(marshalled.decoded.ToInt64() + n), n);
                Marshal.Copy(image.Blue, 0, (IntPtr)(marshalled.decoded.ToInt64() + n * 2), n);
            }

            if ((image.Channels & ManagedImage.ImageChannels.Alpha) != 0) Marshal.Copy(image.Alpha, 0, (IntPtr)(marshalled.decoded.ToInt64() + n * 3), n);
            if ((image.Channels & ManagedImage.ImageChannels.Bump) != 0) Marshal.Copy(image.Bump, 0, (IntPtr)(marshalled.decoded.ToInt64() + n * 4), n);

            // codec will allocate output buffer
            if (!DotNetEncode(ref marshalled, lossless))
                throw new Exception("LibslEncode failed");

            // copy output buffer
            byte[] encoded = new byte[marshalled.length];
            Marshal.Copy(marshalled.encoded, encoded, 0, marshalled.length);

            // free buffers
            DotNetFree(ref marshalled);

            return encoded;
        }

        /// <summary>
        /// Encode a <seealso cref="ManagedImage"/> object into a byte array
        /// </summary>
        /// <param name="image">The <seealso cref="ManagedImage"/> object to encode</param>
        /// <returns>a byte array of the encoded image</returns>
        public static byte[] Encode(ManagedImage image)
        {
            return Encode(image, false);
        }

        /// <summary>
        /// Decode JPEG2000 data to an <seealso cref="System.Drawing.Image"/> and
        /// <seealso cref="ManagedImage"/>
        /// </summary>
        /// <param name="encoded">JPEG2000 encoded data</param>
        /// <param name="managedImage">ManagedImage object to decode to</param>
        /// <param name="image">Image object to decode to</param>
        /// <returns>True if the decode succeeds, otherwise false</returns>
        public static bool DecodeToImage(byte[] encoded, out ManagedImage managedImage, out Image image)
        {
            managedImage = null;
            image = null;

            if (DecodeToImage(encoded, out managedImage))
            {
                try
                {
                    image = LoadTGAClass.LoadTGA(new MemoryStream(managedImage.ExportTGA()));
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Log("Failed to export and load TGA data from decoded image", Helpers.LogLevel.Error, ex);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="encoded"></param>
        /// <param name="managedImage"></param>
        /// <returns></returns>
        public static bool DecodeToImage(byte[] encoded, out ManagedImage managedImage)
        {
            MarshalledImage marshalled = new MarshalledImage();

            // Allocate and copy to input buffer
            marshalled.length = encoded.Length;
            DotNetAllocEncoded(ref marshalled);
            Marshal.Copy(encoded, 0, marshalled.encoded, encoded.Length);

            // Codec will allocate output buffer
            DotNetDecode(ref marshalled);

            int n = marshalled.width * marshalled.height;

            switch (marshalled.components)
            {
                case 1: // Grayscale
                    managedImage = new ManagedImage(marshalled.width, marshalled.height,
                        ManagedImage.ImageChannels.Color);
                    Marshal.Copy(marshalled.decoded, managedImage.Red, 0, n);
                    Buffer.BlockCopy(managedImage.Red, 0, managedImage.Green, 0, n);
                    Buffer.BlockCopy(managedImage.Red, 0, managedImage.Blue, 0, n);
                    break;

                case 2: // Grayscale + alpha
                    managedImage = new ManagedImage(marshalled.width, marshalled.height,
                        ManagedImage.ImageChannels.Color | ManagedImage.ImageChannels.Alpha);
                    Marshal.Copy(marshalled.decoded, managedImage.Red, 0, n);
                    Buffer.BlockCopy(managedImage.Red, 0, managedImage.Green, 0, n);
                    Buffer.BlockCopy(managedImage.Red, 0, managedImage.Blue, 0, n);
                    Marshal.Copy((IntPtr)(marshalled.decoded.ToInt64() + (long)n), managedImage.Alpha, 0, n);
                    break;

                case 3: // RGB
                    managedImage = new ManagedImage(marshalled.width, marshalled.height,
                        ManagedImage.ImageChannels.Color);
                    Marshal.Copy(marshalled.decoded, managedImage.Red, 0, n);
                    Marshal.Copy((IntPtr)(marshalled.decoded.ToInt64() + (long)n), managedImage.Green, 0, n);
                    Marshal.Copy((IntPtr)(marshalled.decoded.ToInt64() + (long)(n * 2)), managedImage.Blue, 0, n);
                    break;

                case 4: // RGBA
                    managedImage = new ManagedImage(marshalled.width, marshalled.height,
                        ManagedImage.ImageChannels.Color | ManagedImage.ImageChannels.Alpha);
                    Marshal.Copy(marshalled.decoded, managedImage.Red, 0, n);
                    Marshal.Copy((IntPtr)(marshalled.decoded.ToInt64() + (long)n), managedImage.Green, 0, n);
                    Marshal.Copy((IntPtr)(marshalled.decoded.ToInt64() + (long)(n * 2)), managedImage.Blue, 0, n);
                    Marshal.Copy((IntPtr)(marshalled.decoded.ToInt64() + (long)(n * 3)), managedImage.Alpha, 0, n);
                    break;

                case 5: // RGBBA
                    managedImage = new ManagedImage(marshalled.width, marshalled.height,
                        ManagedImage.ImageChannels.Color | ManagedImage.ImageChannels.Alpha | ManagedImage.ImageChannels.Bump);
                    Marshal.Copy(marshalled.decoded, managedImage.Red, 0, n);
                    Marshal.Copy((IntPtr)(marshalled.decoded.ToInt64() + (long)n), managedImage.Green, 0, n);
                    Marshal.Copy((IntPtr)(marshalled.decoded.ToInt64() + (long)(n * 2)), managedImage.Blue, 0, n);
                    // Bump comes before alpha in 5 channel encode
                    Marshal.Copy((IntPtr)(marshalled.decoded.ToInt64() + (long)(n * 3)), managedImage.Bump, 0, n);
                    Marshal.Copy((IntPtr)(marshalled.decoded.ToInt64() + (long)(n * 4)), managedImage.Alpha, 0, n);
                    break;

                default:
                    Logger.Log("Decoded image with unhandled number of components: " + marshalled.components,
                        Helpers.LogLevel.Error);
                    DotNetFree(ref marshalled);
                    managedImage = null;
                    return false;
            }

            DotNetFree(ref marshalled);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="encoded"></param>
        /// <param name="layerInfo"></param>
        /// <param name="components"></param>
        /// <returns></returns>
        public static bool DecodeLayerBoundaries(byte[] encoded, out J2KLayerInfo[] layerInfo, out int components)
        {
            bool success = false;
            layerInfo = null;
            components = 0;
            MarshalledImage marshalled = new MarshalledImage();

            // Allocate and copy to input buffer
            marshalled.length = encoded.Length;
            DotNetAllocEncoded(ref marshalled);
            Marshal.Copy(encoded, 0, marshalled.encoded, encoded.Length);

            // Run the decode
            if (DotNetDecodeWithInfo(ref marshalled))
            {
                components = marshalled.components;

                // Sanity check
                if (marshalled.layers * marshalled.resolutions * marshalled.components == marshalled.packet_count)
                {
                    // Manually marshal the array of opj_packet_info structs
                    MarshalledPacket[] packets = new MarshalledPacket[marshalled.packet_count];
                    int offset = 0;

                    for (int i = 0; i < marshalled.packet_count; i++)
                    {
                        MarshalledPacket packet;
                        packet.start_pos = Marshal.ReadInt32(marshalled.packets, offset);
                        offset += 4;
                        packet.end_ph_pos = Marshal.ReadInt32(marshalled.packets, offset);
                        offset += 4;
                        packet.end_pos = Marshal.ReadInt32(marshalled.packets, offset);
                        offset += 4;
                        // Skip the distortion field. WARNING: It looks like there is alignment
                        // padding in here as well, this needs to be tested on different platforms
                        offset += 12;

                        packets[i] = packet;
                    }

                    layerInfo = new J2KLayerInfo[marshalled.layers];

                    for (int i = 0; i < marshalled.layers; i++)
                    {
                        int packetsPerLayer = marshalled.packet_count / marshalled.layers;
                        MarshalledPacket startPacket = packets[packetsPerLayer * i];
                        MarshalledPacket endPacket = packets[(packetsPerLayer * (i + 1)) - 1];
                        layerInfo[i].Start = startPacket.start_pos;
                        layerInfo[i].End = endPacket.end_pos;
                    }

                    success = true;
                }
                else
                {
                    Logger.Log(String.Format(
                        "Packet count mismatch in JPEG2000 stream. layers={0} resolutions={1} components={2} packets={3}",
                        marshalled.layers, marshalled.resolutions, marshalled.components, marshalled.packet_count),
                        Helpers.LogLevel.Warning);
                }
            }

            DotNetFree(ref marshalled);
            return success;
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
            ManagedImage decoded;

            int bitmapWidth = bitmap.Width;
            int bitmapHeight = bitmap.Height;
            int pixelCount = bitmapWidth * bitmapHeight;
            int i;

            if ((bitmap.PixelFormat & PixelFormat.Alpha) != 0 || (bitmap.PixelFormat & PixelFormat.PAlpha) != 0)
            {
                // Four layers, RGBA
                decoded = new ManagedImage(bitmapWidth, bitmapHeight,
                    ManagedImage.ImageChannels.Color | ManagedImage.ImageChannels.Alpha);
                bd = bitmap.LockBits(new Rectangle(0, 0, bitmapWidth, bitmapHeight),
                    ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
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
                // One layer
                decoded = new ManagedImage(bitmapWidth, bitmapHeight,
                    ManagedImage.ImageChannels.Color);
                bd = bitmap.LockBits(new Rectangle(0, 0, bitmapWidth, bitmapHeight),
                    ImageLockMode.ReadOnly, PixelFormat.Format16bppGrayScale);
                byte* pixel = (byte*)bd.Scan0;

                for (i = 0; i < pixelCount; i++)
                {
                    // Normalize 16-bit data down to 8-bit
                    ushort origVal = (byte)(*(pixel) + (*(pixel + 1) << 8));
                    byte val = (byte)(((double)origVal / (double)UInt32.MaxValue) * (double)Byte.MaxValue);

                    decoded.Red[i] = val;
                    decoded.Green[i] = val;
                    decoded.Blue[i] = val;
                    pixel += 2;
                }
            }
            else
            {
                // Three layers, RGB
                decoded = new ManagedImage(bitmapWidth, bitmapHeight,
                    ManagedImage.ImageChannels.Color);
                bd = bitmap.LockBits(new Rectangle(0, 0, bitmapWidth, bitmapHeight),
                    ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
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
