/*
 * Copyright (c) 2006, Second Life Reverse Engineering Team
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
using System.Text;

namespace libsecondlife
{
/// <summary>
/// Static helper functions and global variables
/// </summary>
public class Helpers
{
    /// <summary>The version of libsecondlife (not the SL protocol itself)</summary>
    public readonly static string VERSION = "libsecondlife 0.0.9";
    /// <summary>This header flag signals that ACKs are appended to the packet</summary>
    public const byte MSG_APPENDED_ACKS = 0x10;
    /// <summary>This header flag signals that this packet has been sent before</summary>
    public const byte MSG_RESENT = 0x20;
    /// <summary>This header flags signals that an ACK is expected for this packet</summary>
    public const byte MSG_RELIABLE = 0x40;
    /// <summary>This header flag signals that the message is compressed using zerocoding</summary>
    public const byte MSG_ZEROCODED = 0x80;

    /// <summary>
    /// Passed to SecondLife.Log() to identify the severity of a log entry
    /// </summary>
    public enum LogLevel
    {
        /// <summary>Non-noisy useful information, may be helpful in 
        /// debugging a problem</summary>
        Info,
        /// <summary>A non-critical error occurred. A warning will not 
        /// prevent the rest of libsecondlife from operating as usual, 
        /// although it may be indicative of an underlying issue</summary>
        Warning,
        /// <summary>A critical error has occurred. Generally this will 
        /// be followed by the network layer shutting down, although the 
        /// stability of libsecondlife after an error is uncertain</summary>
        Error,
        /// <summary>Used for internal testing, this logging level can 
        /// generate very noisy (long and/or repetitive) messages. Don't
        /// pass this to the Log() function, use DebugLog() instead.
        /// </summary>
        Debug
    };

    /// <summary>
    /// Packs to 32-bit unsigned integers in to a 64-bit unsigned integer
    /// </summary>
    /// <param name="a">The left-hand (or X) value</param>
    /// <param name="b">The right-hand (or Y) value</param>
    /// <returns>A 64-bit integer containing the two 32-bit input values</returns>
    public static ulong UIntsToLong(uint a, uint b)
    {
        return (ulong)(((ulong)a << 32) + (ulong)b);
    }

    /// <summary>
    /// Unpacks two 32-bit unsigned integers from a 64-bit unsigned integer
    /// </summary>
    /// <param name="a">The 64-bit input integer</param>
    /// <param name="b">The left-hand (or X) output value</param>
    /// <param name="c">The right-hand (or Y) output value</param>
    public static void LongToUInts(ulong a, out uint b, out uint c)
    {
        b = (uint)(a >> 32);
        c = (uint)(a & 0x00000000FFFFFFFF);
    }

    /// <summary>
    /// Convert a variable length field (byte array) to a string.
    /// </summary>
    /// <remarks>If the byte array has unprintable characters in it, a 
    /// hex dump will be put in the string instead</remarks>
    /// <param name="bytes">The byte array to convert to a string</param>
    /// <returns>A UTF8 string, minus the null terminator</returns>
    public static string FieldToString(byte[] bytes)
    {
        return FieldToString(bytes, "");
    }

    /// <summary>
    /// Convert a variable length field (byte array) to a string, with a
    /// field name prepended to each line of the output.
    /// </summary>
    /// <remarks>If the byte array has unprintable characters in it, a 
    /// hex dump will be put in the string instead</remarks>
    /// <param name="bytes">The byte array to convert to a string</param>
    /// <param name="fieldName">A field name to prepend to each line of output</param>
    /// <returns>A UTF8 string, minus the null terminator</returns>
    public static string FieldToString(byte[] bytes, string fieldName)
    {
        string output = "";
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
                output += fieldName + ": ";
            }

            output += System.Text.Encoding.UTF8.GetString(bytes).Replace("\0", "");
        }
        else
        {
            for (int i = 0; i < bytes.Length; i += 16)
            {
                if (i != 0) { output += "\n"; }
                if (fieldName != "") { output += fieldName + ": "; }

                for (int j = 0; j < 16; j++)
                {
                    if ((i + j) < bytes.Length)
                    {
                        string s = String.Format("{0:X} ", bytes[i + j]);
                        if (s.Length == 2)
                        {
                            s = "0" + s;
                        }

                        output += s;
                    }
                    else
                    {
                        output += "   ";
                    }
                }

                for (int j = 0; j < 16 && (i + j) < bytes.Length; j++)
                {
                    if (bytes[i + j] >= 0x20 && bytes[i + j] < 0x7E)
                    {
                        output += (char)bytes[i + j];
                    }
                    else
                    {
                        output += ".";
                    }
                }
            }
        }

        return output;
    }

    /// <summary>
    /// Convert a UTF8 string to a byte array
    /// </summary>
    /// <param name="str">The string to convert to a byte array</param>
    /// <returns>A null-terminated byte array</returns>
    public static byte[] StringToField(string str)
    {
        if (!str.EndsWith("\0")) { str += "\0"; }
        return System.Text.UTF8Encoding.UTF8.GetBytes(str);
    }

    public static uint GetUnixTime()
    {
        return (uint)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
    }

    /// <summary>
    /// Calculates the distance between two vectors
    /// </summary>
    public static float VecDist(LLVector3 pointA, LLVector3 pointB)
    {
        float xd = pointB.X - pointA.X;
        float yd = pointB.Y - pointA.Y;
        float zd = pointB.Z - pointA.Z;
        return (float)Math.Sqrt(xd * xd + yd * yd + zd * zd);
    }

    /// <summary>
    /// Calculates the magnitutde of the supplied vector
    /// </summary>
    public static float VecMag(LLVector3 vector)
    {
        return (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
    }

    /// <summary>
    /// Returns the supplied vector in normalized form
    /// </summary>
    public static LLVector3 VecDist(LLVector3 vector)
    {
        float mag = VecMag(vector);
        return new LLVector3(vector.X/mag,vector.Y/mag,vector.Z/mag);
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
    /// <param name="srclen">The length of the byte array to decode</param>
    /// <param name="dest">The output byte array to decode to</param>
    /// <returns>The length of the output buffer</returns>
    public static int ZeroDecode(byte[] src, int srclen, byte[] dest)
    {
        uint zerolen = 0;

        Array.Copy(src, 0, dest, 0, 4);
        zerolen += 4;

        //int bodylen;
        //if ((src[0] & MSG_APPENDED_ACKS) == 0)
        //{
        //    bodylen = srclen;
        //}
        //else
        //{
        //    bodylen = srclen - src[srclen - 1] * 4 - 1;
        //}
        int bodylen = srclen;

        uint i;
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

        // HACK: Fix truncated zerocoded messages
        for (uint j = zerolen; j < zerolen + 16; j++)
        {
            dest[j] = 0;
        }
        zerolen += 16;

        // copy appended ACKs
        for (; i < srclen; i++)
        {
            dest[zerolen++] = src[i];
        }

        return (int)zerolen;
    }

    /// <summary>
    /// Decode enough of a byte array to get the packet ID.  Data before and
    /// after the packet ID is undefined.
    /// </summary>
    /// <param name="src">The byte array to decode</param>
    /// <param name="dest">The output byte array to encode to</param>
    public static void ZeroDecodeCommand(byte[] src, byte[] dest)
    {
        for (int srcPos = 4, destPos = 4; destPos < 8; ++srcPos)
        {
            if (src[srcPos] == 0x00)
            {
                for (byte j = 0; j < src[srcPos + 1]; ++j)
                {
                    dest[destPos++] = 0x00;
                }

                ++srcPos;
            }
            else
            {
                dest[destPos++] = src[srcPos];
            }
        }
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

        Array.Copy(src, 0, dest, 0, 4);
        zerolen += 4;

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
        LLUUID assetID, LLUUID groupID, int salePrice, LLUUID ownerID, LLUUID creatorID,
        LLUUID itemID, LLUUID folderID, uint everyoneMask, uint flags, uint nextOwnerMask,
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

        // CRC += another 4 words which always seem to be zero -- unclear if this is a LLUUID or what
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

    public static string MD5(string password)
    {
        StringBuilder digest = new StringBuilder();
        System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] hash = md5.ComputeHash(Encoding.ASCII.GetBytes(password));

        // Convert the hash to a hex string
        foreach (byte b in hash)
        {
            digest.AppendFormat("{0:x2}", b);
        }

        return "$1$" + digest.ToString();
    }
}
}