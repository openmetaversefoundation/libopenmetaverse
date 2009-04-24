/*
 * Copyright (c) 2006-2008, openmetaverse.org
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
using System.Collections.Generic;
using System.Text;
using OpenMetaverse.Packets;
using System.Reflection;

namespace OpenMetaverse
{
    /// <summary>
    /// Static helper functions and global variables
    /// </summary>
    public static class Helpers
    {
        /// <summary>This header flag signals that ACKs are appended to the packet</summary>
        public const byte MSG_APPENDED_ACKS = 0x10;
        /// <summary>This header flag signals that this packet has been sent before</summary>
        public const byte MSG_RESENT = 0x20;
        /// <summary>This header flags signals that an ACK is expected for this packet</summary>
        public const byte MSG_RELIABLE = 0x40;
        /// <summary>This header flag signals that the message is compressed using zerocoding</summary>
        public const byte MSG_ZEROCODED = 0x80;

        /// <summary>
        /// Passed to Logger.Log() to identify the severity of a log entry
        /// </summary>
        public enum LogLevel
        {
            /// <summary>No logging information will be output</summary>
            None,
            /// <summary>Non-noisy useful information, may be helpful in 
            /// debugging a problem</summary>
            Info,
            /// <summary>A non-critical error occurred. A warning will not 
            /// prevent the rest of the library from operating as usual, 
            /// although it may be indicative of an underlying issue</summary>
            Warning,
            /// <summary>A critical error has occurred. Generally this will 
            /// be followed by the network layer shutting down, although the 
            /// stability of the library after an error is uncertain</summary>
            Error,
            /// <summary>Used for internal testing, this logging level can 
            /// generate very noisy (long and/or repetitive) messages. Don't
            /// pass this to the Log() function, use DebugLog() instead.
            /// </summary>
            Debug
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static short TEOffsetShort(float offset)
        {
            offset = Utils.Clamp(offset, -1.0f, 1.0f);
            offset *= 32767.0f;
            return (short)Math.Round(offset);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static float TEOffsetFloat(byte[] bytes, int pos)
        {
            float offset = (float)BitConverter.ToInt16(bytes, pos);
            return offset / 32767.0f;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public static short TERotationShort(float rotation)
        {
            const double TWO_PI = Math.PI * 2.0d;
            double remainder = Math.IEEERemainder(rotation, TWO_PI);
            return (short)Math.Round((remainder / TWO_PI) * 32767.0d);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static float TERotationFloat(byte[] bytes, int pos)
        {
            const float TWO_PI = (float)(Math.PI * 2.0d);
            return (float)((bytes[pos] | (bytes[pos + 1] << 8)) / 32767.0f) * TWO_PI;
        }

        public static byte TEGlowByte(float glow)
        {
            return (byte)(glow * 255.0f);
        }

        public static float TEGlowFloat(byte[] bytes, int pos)
        {
            return (float)bytes[pos] / 255.0f;
        }

        /// <summary>
        /// Given an X/Y location in absolute (grid-relative) terms, a region
        /// handle is returned along with the local X/Y location in that region
        /// </summary>
        /// <param name="globalX">The absolute X location, a number such as 
        /// 255360.35</param>
        /// <param name="globalY">The absolute Y location, a number such as
        /// 255360.35</param>
        /// <param name="localX">The sim-local X position of the global X
        /// position, a value from 0.0 to 256.0</param>
        /// <param name="localY">The sim-local Y position of the global Y
        /// position, a value from 0.0 to 256.0</param>
        /// <returns>A 64-bit region handle that can be used to teleport to</returns>
        public static ulong GlobalPosToRegionHandle(float globalX, float globalY, out float localX, out float localY)
        {
            uint x = ((uint)globalX / 256) * 256;
            uint y = ((uint)globalY / 256) * 256;
            localX = globalX - (float)x;
            localY = globalY - (float)y;
            return Utils.UIntsToLong(x, y);
        }

        /// <summary>
        /// Converts a floating point number to a terse string format used for
        /// transmitting numbers in wearable asset files
        /// </summary>
        /// <param name="val">Floating point number to convert to a string</param>
        /// <returns>A terse string representation of the input number</returns>
        public static string FloatToTerseString(float val)
        {
            string s = string.Format("{0:.00}", val);

            if (val == 0)
                return ".00";

            // Trim trailing zeroes
            while (s[s.Length - 1] == '0')
                s = s.Remove(s.Length - 1, 1);

            // Remove superfluous decimal places after the trim
            if (s[s.Length - 1] == '.')
                s = s.Remove(s.Length - 1, 1);
            // Remove leading zeroes after a negative sign
            else if (s[0] == '-' && s[1] == '0')
                s = s.Remove(1, 1);
            // Remove leading zeroes in positive numbers
            else if (s[0] == '0')
                s = s.Remove(0, 1);

            return s;
        }

        /// <summary>
        /// Convert a variable length field (byte array) to a string, with a
        /// field name prepended to each line of the output
        /// </summary>
        /// <remarks>If the byte array has unprintable characters in it, a 
        /// hex dump will be written instead</remarks>
        /// <param name="output">The StringBuilder object to write to</param>
        /// <param name="bytes">The byte array to convert to a string</param>
        /// <param name="fieldName">A field name to prepend to each line of output</param>
        internal static void FieldToString(StringBuilder output, byte[] bytes, string fieldName)
        {
            // Check for a common case
            if (bytes.Length == 0) return;

            bool printable = true;

            for (int i = 0; i < bytes.Length; ++i)
            {
                // Check if there are any unprintable characters in the array
                if ((bytes[i] < 0x20 || bytes[i] > 0x7E) && bytes[i] != 0x09
                    && bytes[i] != 0x0D && bytes[i] != 0x0A && bytes[i] != 0x00)
                {
                    printable = false;
                    break;
                }
            }

            if (printable)
            {
                if (fieldName.Length > 0)
                {
                    output.Append(fieldName);
                    output.Append(": ");
                }

                if (bytes[bytes.Length - 1] == 0x00)
                    output.Append(UTF8Encoding.UTF8.GetString(bytes, 0, bytes.Length - 1));
                else
                    output.Append(UTF8Encoding.UTF8.GetString(bytes, 0, bytes.Length));
            }
            else
            {
                for (int i = 0; i < bytes.Length; i += 16)
                {
                    if (i != 0)
                        output.Append('\n');
                    if (fieldName.Length > 0)
                    {
                        output.Append(fieldName);
                        output.Append(": ");
                    }

                    for (int j = 0; j < 16; j++)
                    {
                        if ((i + j) < bytes.Length)
                            output.Append(String.Format("{0:X2} ", bytes[i + j]));
                        else
                            output.Append("   ");
                    }
                }
            }
        }

        /// <summary>
        /// Decode a zerocoded byte array, used to decompress packets marked
        /// with the zerocoded flag
        /// </summary>
        /// <remarks>Any time a zero is encountered, the next byte is a count 
        /// of how many zeroes to expand. One zero is encoded with 0x00 0x01, 
        /// two zeroes is 0x00 0x02, three zeroes is 0x00 0x03, etc. The 
        /// first four bytes are copied directly to the output buffer.
        /// </remarks>
        /// <param name="src">The byte array to decode</param>
        /// <param name="srclen">The length of the byte array to decode. This 
        /// would be the length of the packet up to (but not including) any
        /// appended ACKs</param>
        /// <param name="dest">The output byte array to decode to</param>
        /// <returns>The length of the output buffer</returns>
        public static int ZeroDecode(byte[] src, int srclen, byte[] dest)
        {
            if (srclen > src.Length)
                throw new ArgumentException("srclen cannot be greater than src.Length");

            uint zerolen = 0;
            int bodylen = 0;
            uint i = 0;

            try
            {
                Buffer.BlockCopy(src, 0, dest, 0, 6);
                zerolen = 6;
                bodylen = srclen;

                for (i = zerolen; i < bodylen; i++)
                {
                    if (src[i] == 0x00)
                    {
                        for (byte j = 0; j < src[i + 1]; j++)
                        {
                            dest[zerolen++] = 0x00;
                        }

                        i++;
                    }
                    else
                    {
                        dest[zerolen++] = src[i];
                    }
                }

                // Copy appended ACKs
                for (; i < srclen; i++)
                {
                    dest[zerolen++] = src[i];
                }

                return (int)zerolen;
            }
            catch (Exception)
            {
                Logger.Log(String.Format("Zerodecoding error: i={0}, srclen={1}, bodylen={2}, zerolen={3}\n{4}",
                    i, srclen, bodylen, zerolen, Utils.BytesToHexString(src, srclen, null)), LogLevel.Error);
            }

            return 0;
        }

        /// <summary>
        /// Encode a byte array with zerocoding. Used to compress packets marked
        /// with the zerocoded flag. Any zeroes in the array are compressed down
        /// to a single zero byte followed by a count of how many zeroes to expand
        /// out. A single zero becomes 0x00 0x01, two zeroes becomes 0x00 0x02,
        /// three zeroes becomes 0x00 0x03, etc. The first four bytes are copied
        /// directly to the output buffer.
        /// </summary>
        /// <param name="src">The byte array to encode</param>
        /// <param name="srclen">The length of the byte array to encode</param>
        /// <param name="dest">The output byte array to encode to</param>
        /// <returns>The length of the output buffer</returns>
        public static int ZeroEncode(byte[] src, int srclen, byte[] dest)
        {
            uint zerolen = 0;
            byte zerocount = 0;

            Buffer.BlockCopy(src, 0, dest, 0, 6);
            zerolen += 6;

            int bodylen;
            if ((src[0] & MSG_APPENDED_ACKS) == 0)
            {
                bodylen = srclen;
            }
            else
            {
                bodylen = srclen - src[srclen - 1] * 4 - 1;
            }

            uint i;
            for (i = zerolen; i < bodylen; i++)
            {
                if (src[i] == 0x00)
                {
                    zerocount++;

                    if (zerocount == 0)
                    {
                        dest[zerolen++] = 0x00;
                        dest[zerolen++] = 0xff;
                        zerocount++;
                    }
                }
                else
                {
                    if (zerocount != 0)
                    {
                        dest[zerolen++] = 0x00;
                        dest[zerolen++] = (byte)zerocount;
                        zerocount = 0;
                    }

                    dest[zerolen++] = src[i];
                }
            }

            if (zerocount != 0)
            {
                dest[zerolen++] = 0x00;
                dest[zerolen++] = (byte)zerocount;
            }

            // copy appended ACKs
            for (; i < srclen; i++)
            {
                dest[zerolen++] = src[i];
            }

            return (int)zerolen;
        }

        /// <summary>
        /// Calculates the CRC (cyclic redundancy check) needed to upload inventory.
        /// </summary>
        /// <param name="creationDate">Creation date</param>
        /// <param name="saleType">Sale type</param>
        /// <param name="invType">Inventory type</param>
        /// <param name="type">Type</param>
        /// <param name="assetID">Asset ID</param>
        /// <param name="groupID">Group ID</param>
        /// <param name="salePrice">Sale price</param>
        /// <param name="ownerID">Owner ID</param>
        /// <param name="creatorID">Creator ID</param>
        /// <param name="itemID">Item ID</param>
        /// <param name="folderID">Folder ID</param>
        /// <param name="everyoneMask">Everyone mask (permissions)</param>
        /// <param name="flags">Flags</param>
        /// <param name="nextOwnerMask">Next owner mask (permissions)</param>
        /// <param name="groupMask">Group mask (permissions)</param>
        /// <param name="ownerMask">Owner mask (permisions)</param>
        /// <returns>The calculated CRC</returns>
        public static uint InventoryCRC(int creationDate, byte saleType, sbyte invType, sbyte type,
            UUID assetID, UUID groupID, int salePrice, UUID ownerID, UUID creatorID,
            UUID itemID, UUID folderID, uint everyoneMask, uint flags, uint nextOwnerMask,
            uint groupMask, uint ownerMask)
        {
            uint CRC = 0;

            // IDs
            CRC += assetID.CRC(); // AssetID
            CRC += folderID.CRC(); // FolderID
            CRC += itemID.CRC(); // ItemID

            // Permission stuff
            CRC += creatorID.CRC(); // CreatorID
            CRC += ownerID.CRC(); // OwnerID
            CRC += groupID.CRC(); // GroupID

            // CRC += another 4 words which always seem to be zero -- unclear if this is a UUID or what
            CRC += ownerMask;
            CRC += nextOwnerMask;
            CRC += everyoneMask;
            CRC += groupMask;

            // The rest of the CRC fields
            CRC += flags; // Flags
            CRC += (uint)invType; // InvType
            CRC += (uint)type; // Type 
            CRC += (uint)creationDate; // CreationDate
            CRC += (uint)salePrice;    // SalePrice
            CRC += (uint)((uint)saleType * 0x07073096); // SaleType

            return CRC;
        }

        /// <summary>
        /// Attempts to load a file embedded in the assembly
        /// </summary>
        /// <param name="resourceName">The filename of the resource to load</param>
        /// <returns>A Stream for the requested file, or null if the resource
        /// was not successfully loaded</returns>
        public static System.IO.Stream GetResourceStream(string resourceName)
        {
            return GetResourceStream(resourceName, Settings.RESOURCE_DIR);
        }
        
        /// <summary>
        /// Attempts to load a file either embedded in the assembly or found in
        /// a given search path
        /// </summary>
        /// <param name="resourceName">The filename of the resource to load</param>
        /// <param name="searchPath">An optional path that will be searched if
        /// the asset is not found embedded in the assembly</param>
        /// <returns>A Stream for the requested file, or null if the resource
        /// was not successfully loaded</returns>
        public static System.IO.Stream GetResourceStream(string resourceName, string searchPath)
        {
            try
            {
                System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
                System.IO.Stream s = a.GetManifestResourceStream("OpenMetaverse.Resources." + resourceName);
                if (s != null) return s;
            }
            catch (Exception)
            {
                // Failed to load the resource from the assembly itself
            }

            try
            {
                return new System.IO.FileStream(
                    System.IO.Path.Combine(System.IO.Path.Combine(System.Environment.CurrentDirectory, searchPath), resourceName),
                    System.IO.FileMode.Open);
            }
            catch (Exception)
            {
                // Failed to load the resource from the given path
            }

            return null;
        }

        /// <summary>
        /// Converts a list of primitives to an object that can be serialized
        /// with the LLSD system
        /// </summary>
        /// <param name="prims">Primitives to convert to a serializable object</param>
        /// <returns>An object that can be serialized with LLSD</returns>
        public static StructuredData.OSD PrimListToOSD(List<Primitive> prims)
        {
            StructuredData.OSDMap map = new OpenMetaverse.StructuredData.OSDMap(prims.Count);

            for (int i = 0; i < prims.Count; i++)
                map.Add(prims[i].LocalID.ToString(), prims[i].GetOSD());

            return map;
        }

        /// <summary>
        /// Deserializes OSD in to a list of primitives
        /// </summary>
        /// <param name="osd">Structure holding the serialized primitive list,
        /// must be of the SDMap type</param>
        /// <returns>A list of deserialized primitives</returns>
        public static List<Primitive> OSDToPrimList(StructuredData.OSD osd)
        {
            if (osd.Type != StructuredData.OSDType.Map)
                throw new ArgumentException("LLSD must be in the Map structure");

            StructuredData.OSDMap map = (StructuredData.OSDMap)osd;
            List<Primitive> prims = new List<Primitive>(map.Count);

            foreach (KeyValuePair<string, StructuredData.OSD> kvp in map)
            {
                Primitive prim = Primitive.FromOSD(kvp.Value);
                prim.LocalID = UInt32.Parse(kvp.Key);
                prims.Add(prim);
            }

            return prims;
        }

        public static AttachmentPoint StateToAttachmentPoint(uint state)
        {
            const uint ATTACHMENT_MASK = 0xF0;
            uint fixedState = (((byte)state & ATTACHMENT_MASK) >> 4) | (((byte)state & ~ATTACHMENT_MASK) << 4);
            return (AttachmentPoint)fixedState;
        }

        public static List<int> SplitBlocks(PacketBlock[] blocks, int packetOverhead)
        {
            List<int> splitPoints = new List<int>();
            int size = 0;

            if (blocks != null && blocks.Length > 0)
            {
                splitPoints.Add(0);

                for (int i = 0; i < blocks.Length; i++)
                {
                    size += blocks[i].Length;

                    // If the next block will put this packet over the limit, add a split point
                    if (i < blocks.Length - 1 &&
                        size + blocks[i + 1].Length + packetOverhead >= Settings.MAX_PACKET_SIZE)
                    {
                        splitPoints.Add(i + 1);
                        size = 0;
                    }
                }
            }

            return splitPoints;
        }

        /// <summary>
        /// Parse a packet into human readable formatted key/value pairs
        /// </summary>
        /// <param name="packet">the Packet to parse</param>
        /// <returns>A string containing the packet block name, and key/value pairs of the data fields</returns>
        public static string PacketToString(Packet packet)
        {
            StringBuilder result = new StringBuilder();

            foreach(FieldInfo packetField in packet.GetType().GetFields())
            {
                object packetDataObject = packetField.GetValue(packet);

                result.AppendFormat("-- {0} --" + System.Environment.NewLine, packetField.Name);
                result.AppendFormat("-- {0} --" + System.Environment.NewLine, packet.Type);
                foreach(FieldInfo packetValueField in packetField.GetValue(packet).GetType().GetFields())
                {
                    result.AppendFormat("{0}: {1}" + System.Environment.NewLine, 
                        packetValueField.Name, packetValueField.GetValue(packetDataObject));
                }

                // handle blocks that are arrays
                if (packetDataObject.GetType().IsArray)
                {
                    foreach (object nestedArrayRecord in packetDataObject as Array)
                    {
                        foreach (FieldInfo packetArrayField in nestedArrayRecord.GetType().GetFields())
                        {
                            result.AppendFormat("{0} {1}" + System.Environment.NewLine, 
                                packetArrayField.Name, packetArrayField.GetValue(nestedArrayRecord));
                        }
                    }
                }
                else
                {
                    // handle non array data blocks
                    foreach (PropertyInfo packetPropertyField in packetField.GetValue(packet).GetType().GetProperties())
                    {
                        // Handle fields named "Data" specifically, this is generally binary data, we'll display it as hex values
                        if (packetPropertyField.PropertyType.Equals(typeof(System.Byte[])) 
                            && packetPropertyField.Name.Equals("Data"))
                        {
                            result.AppendFormat("{0}: {1}" + System.Environment.NewLine,
                                packetPropertyField.Name, 
                                Utils.BytesToHexString((byte[])packetPropertyField.GetValue(packetDataObject, null), packetPropertyField.Name));
                        }
                        // decode bytes into strings
                        else if (packetPropertyField.PropertyType.Equals(typeof(System.Byte[])))
                        {
                            result.AppendFormat("{0}: {1}" + System.Environment.NewLine, 
                                packetPropertyField.Name, 
                                Utils.BytesToString((byte[])packetPropertyField.GetValue(packetDataObject, null)));
                        }
                        else
                        {
                            result.AppendFormat("{0}: {1}" + System.Environment.NewLine, 
                                packetPropertyField.Name, packetPropertyField.GetValue(packetDataObject, null));
                        }
                    }
                }
            }
            return result.ToString();
        }
    }
}
