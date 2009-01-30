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

/* 
 * 
 * This implementation is based upon the description at
 * 
 * http://wiki.secondlife.com/wiki/LLSD
 * 
 * and (partially) tested against the (supposed) reference implementation at
 * 
 * http://svn.secondlife.com/svn/linden/release/indra/lib/python/indra/base/osd.py
 * 
 */

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace OpenMetaverse.StructuredData
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class OSDParser
    {
        private const int initialBufferSize = 128;
        private const int int32Length = 4;
        private const int doubleLength = 8;

        private static byte[] llsdBinaryHead = Encoding.ASCII.GetBytes("<?llsd/binary?>\n");
        private const byte undefBinaryValue = (byte)'!';
        private const byte trueBinaryValue = (byte)'1';
        private const byte falseBinaryValue = (byte)'0';
        private const byte integerBinaryMarker = (byte)'i';
        private const byte realBinaryMarker = (byte)'r';
        private const byte GuidBinaryMarker = (byte)'u';
        private const byte binaryBinaryMarker = (byte)'b';
        private const byte stringBinaryMarker = (byte)'s';
        private const byte uriBinaryMarker = (byte)'l';
        private const byte dateBinaryMarker = (byte)'d';
        private const byte arrayBeginBinaryMarker = (byte)'[';
        private const byte arrayEndBinaryMarker = (byte)']';
        private const byte mapBeginBinaryMarker = (byte)'{';
        private const byte mapEndBinaryMarker = (byte)'}';
        private const byte keyBinaryMarker = (byte)'k';

        /// <summary>
        /// 
        /// </summary>
        /// <param name="binaryData"></param>
        /// <returns></returns>
        public static OSD DeserializeLLSDBinary(byte[] binaryData)
        {

            MemoryStream stream = new MemoryStream(binaryData);
            OSD osd = DeserializeLLSDBinary(stream);
            stream.Close();
            return osd;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static OSD DeserializeLLSDBinary(Stream stream)
        {
            if (!stream.CanSeek)
                throw new OSDException("Cannot deserialize binary LLSD from unseekable streams");

            SkipWhiteSpace(stream);

            bool result = FindByteArray(stream, llsdBinaryHead);
            if (!result)
                throw new OSDException("Failed to decode binary LLSD");

            return ParseLLSDBinaryElement(stream);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="osd"></param>
        /// <returns></returns>
        public static byte[] SerializeLLSDBinary(OSD osd)
        {
            MemoryStream stream = SerializeLLSDBinaryStream(osd);
            byte[] binaryData = stream.ToArray();
            stream.Close();

            return binaryData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static MemoryStream SerializeLLSDBinaryStream(OSD data)
        {
            MemoryStream stream = new MemoryStream(initialBufferSize);

            stream.Write(llsdBinaryHead, 0, llsdBinaryHead.Length);
            SerializeLLSDBinaryElement(stream, data);
            return stream;
        }

        private static void SerializeLLSDBinaryElement(MemoryStream stream, OSD osd)
        {
            switch (osd.Type)
            {
                case OSDType.Unknown:
                    stream.WriteByte(undefBinaryValue);
                    break;
                case OSDType.Boolean:
                    stream.Write(osd.AsBinary(), 0, 1);
                    break;
                case OSDType.Integer:
                    stream.WriteByte(integerBinaryMarker);
                    stream.Write(osd.AsBinary(), 0, int32Length);
                    break;
                case OSDType.Real:
                    stream.WriteByte(realBinaryMarker);
                    stream.Write(osd.AsBinary(), 0, doubleLength);
                    break;
                case OSDType.Guid:
                    stream.WriteByte(GuidBinaryMarker);
                    stream.Write(osd.AsBinary(), 0, 16);
                    break;
                case OSDType.String:
                    stream.WriteByte(stringBinaryMarker);
                    byte[] rawString = osd.AsBinary();
                    byte[] stringLengthNetEnd = HostToNetworkIntBytes(rawString.Length);
                    stream.Write(stringLengthNetEnd, 0, int32Length);
                    stream.Write(rawString, 0, rawString.Length);
                    break;
                case OSDType.Binary:
                    stream.WriteByte(binaryBinaryMarker);
                    byte[] rawBinary = osd.AsBinary();
                    byte[] binaryLengthNetEnd = HostToNetworkIntBytes(rawBinary.Length);
                    stream.Write(binaryLengthNetEnd, 0, int32Length);
                    stream.Write(rawBinary, 0, rawBinary.Length);
                    break;
                case OSDType.Date:
                    stream.WriteByte(dateBinaryMarker);
                    stream.Write(osd.AsBinary(), 0, doubleLength);
                    break;
                case OSDType.URI:
                    stream.WriteByte(uriBinaryMarker);
                    byte[] rawURI = osd.AsBinary();
                    byte[] uriLengthNetEnd = HostToNetworkIntBytes(rawURI.Length);
                    stream.Write(uriLengthNetEnd, 0, int32Length);
                    stream.Write(rawURI, 0, rawURI.Length);
                    break;
                case OSDType.Array:
                    SerializeLLSDBinaryArray(stream, (OSDArray)osd);
                    break;
                case OSDType.Map:
                    SerializeLLSDBinaryMap(stream, (OSDMap)osd);
                    break;
                default:
                    throw new OSDException("Binary serialization: Not existing element discovered.");

            }
        }

        private static void SerializeLLSDBinaryArray(MemoryStream stream, OSDArray osdArray)
        {
            stream.WriteByte(arrayBeginBinaryMarker);
            byte[] binaryNumElementsHostEnd = HostToNetworkIntBytes(osdArray.Count);
            stream.Write(binaryNumElementsHostEnd, 0, int32Length);

            foreach (OSD osd in osdArray)
            {
                SerializeLLSDBinaryElement(stream, osd);
            }
            stream.WriteByte(arrayEndBinaryMarker);
        }

        private static void SerializeLLSDBinaryMap(MemoryStream stream, OSDMap osdMap)
        {
            stream.WriteByte(mapBeginBinaryMarker);
            byte[] binaryNumElementsNetEnd = HostToNetworkIntBytes(osdMap.Count);
            stream.Write(binaryNumElementsNetEnd, 0, int32Length);

            foreach (KeyValuePair<string, OSD> kvp in osdMap)
            {
                stream.WriteByte(keyBinaryMarker);
                byte[] binaryKey = Encoding.UTF8.GetBytes(kvp.Key);
                byte[] binaryKeyLength = HostToNetworkIntBytes(binaryKey.Length);
                stream.Write(binaryKeyLength, 0, int32Length);
                stream.Write(binaryKey, 0, binaryKey.Length);
                SerializeLLSDBinaryElement(stream, kvp.Value);
            }
            stream.WriteByte(mapEndBinaryMarker);
        }

        private static OSD ParseLLSDBinaryElement(Stream stream)
        {
            SkipWhiteSpace(stream);
            OSD osd;

            int marker = stream.ReadByte();
            if (marker < 0)
                throw new OSDException("Binary LLSD parsing: Unexpected end of stream.");

            switch ((byte)marker)
            {
                case undefBinaryValue:
                    osd = new OSD();
                    break;
                case trueBinaryValue:
                    osd = OSD.FromBoolean(true);
                    break;
                case falseBinaryValue:
                    osd = OSD.FromBoolean(false);
                    break;
                case integerBinaryMarker:
                    int integer = NetworkToHostInt(ConsumeBytes(stream, int32Length));
                    osd = OSD.FromInteger(integer);
                    break;
                case realBinaryMarker:
                    double dbl = NetworkToHostDouble(ConsumeBytes(stream, doubleLength));
                    osd = OSD.FromReal(dbl);
                    break;
                case GuidBinaryMarker:
                    Guid guid = new Guid();
                    guid.FromBytes(ConsumeBytes(stream, 16), 0);
                    osd = OSD.FromGuid(guid);
                    break;
                case binaryBinaryMarker:
                    int binaryLength = NetworkToHostInt(ConsumeBytes(stream, int32Length));
                    osd = OSD.FromBinary(ConsumeBytes(stream, binaryLength));
                    break;
                case stringBinaryMarker:
                    int stringLength = NetworkToHostInt(ConsumeBytes(stream, int32Length));
                    string ss = Encoding.UTF8.GetString(ConsumeBytes(stream, stringLength));
                    osd = OSD.FromString(ss);
                    break;
                case uriBinaryMarker:
                    int uriLength = NetworkToHostInt(ConsumeBytes(stream, int32Length));
                    string sUri = Encoding.UTF8.GetString(ConsumeBytes(stream, uriLength));
                    Uri uri;
                    try
                    {
                        uri = new Uri(sUri, UriKind.RelativeOrAbsolute);
                    }
                    catch
                    {
                        throw new OSDException("Binary LLSD parsing: Invalid Uri format detected.");
                    }
                    osd = OSD.FromUri(uri);
                    break;
                case dateBinaryMarker:
                    double timestamp = NetworkToHostDouble(ConsumeBytes(stream, doubleLength));
                    DateTime dateTime = DateTime.SpecifyKind(Utils.Epoch, DateTimeKind.Utc);
                    dateTime = dateTime.AddSeconds(timestamp);
                    osd = OSD.FromDate(dateTime.ToLocalTime());
                    break;
                case arrayBeginBinaryMarker:
                    osd = ParseLLSDBinaryArray(stream);
                    break;
                case mapBeginBinaryMarker:
                    osd = ParseLLSDBinaryMap(stream);
                    break;
                default:
                    throw new OSDException("Binary LLSD parsing: Unknown type marker.");

            }
            return osd;
        }

        private static OSD ParseLLSDBinaryArray(Stream stream)
        {
            int numElements = NetworkToHostInt(ConsumeBytes(stream, int32Length));
            int crrElement = 0;
            OSDArray osdArray = new OSDArray();
            while (crrElement < numElements)
            {
                osdArray.Add(ParseLLSDBinaryElement(stream));
                crrElement++;
            }

            if (!FindByte(stream, arrayEndBinaryMarker))
                throw new OSDException("Binary LLSD parsing: Missing end marker in array.");

            return (OSD)osdArray;
        }

        private static OSD ParseLLSDBinaryMap(Stream stream)
        {
            int numElements = NetworkToHostInt(ConsumeBytes(stream, int32Length));
            int crrElement = 0;
            OSDMap osdMap = new OSDMap();
            while (crrElement < numElements)
            {
                if (!FindByte(stream, keyBinaryMarker))
                    throw new OSDException("Binary LLSD parsing: Missing key marker in map.");
                int keyLength = NetworkToHostInt(ConsumeBytes(stream, int32Length));
                string key = Encoding.UTF8.GetString(ConsumeBytes(stream, keyLength));
                osdMap[key] = ParseLLSDBinaryElement(stream);
                crrElement++;
            }

            if (!FindByte(stream, mapEndBinaryMarker))
                throw new OSDException("Binary LLSD parsing: Missing end marker in map.");

            return (OSD)osdMap;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        public static void SkipWhiteSpace(Stream stream)
        {
            int bt;

            while (((bt = stream.ReadByte()) > 0) &&
                ((byte)bt == ' ' || (byte)bt == '\t' ||
                  (byte)bt == '\n' || (byte)bt == '\r')
                 )
            {
            }
            stream.Seek(-1, SeekOrigin.Current);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="toFind"></param>
        /// <returns></returns>
        public static bool FindByte(Stream stream, byte toFind)
        {
            int bt = stream.ReadByte();
            if (bt < 0)
                return false;
            if ((byte)bt == toFind)
                return true;
            else
            {
                stream.Seek(-1L, SeekOrigin.Current);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="toFind"></param>
        /// <returns></returns>
        public static bool FindByteArray(Stream stream, byte[] toFind)
        {
            int lastIndexToFind = toFind.Length - 1;
            int crrIndex = 0;
            bool found = true;
            int bt;
            long lastPosition = stream.Position;

            while (found &&
                  ((bt = stream.ReadByte()) > 0) &&
                    (crrIndex <= lastIndexToFind)
                  )
            {
                if (toFind[crrIndex] == (byte)bt)
                {
                    found = true;
                    crrIndex++;
                }
                else
                    found = false;
            }

            if (found && crrIndex > lastIndexToFind)
            {
                stream.Seek(-1L, SeekOrigin.Current);
                return true;
            }
            else
            {
                stream.Position = lastPosition;
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="consumeBytes"></param>
        /// <returns></returns>
        public static byte[] ConsumeBytes(Stream stream, int consumeBytes)
        {
            byte[] bytes = new byte[consumeBytes];
            if (stream.Read(bytes, 0, consumeBytes) < consumeBytes)
                throw new OSDException("Binary LLSD parsing: Unexpected end of stream.");
            return bytes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="binaryNetEnd"></param>
        /// <returns></returns>
        public static int NetworkToHostInt(byte[] binaryNetEnd)
        {
            if (binaryNetEnd == null)
                return -1;

            int intNetEnd = BitConverter.ToInt32(binaryNetEnd, 0);
            int intHostEnd = System.Net.IPAddress.NetworkToHostOrder(intNetEnd);
            return intHostEnd;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="binaryNetEnd"></param>
        /// <returns></returns>
        public static double NetworkToHostDouble(byte[] binaryNetEnd)
        {
            if (binaryNetEnd == null)
                return -1d;
            long longNetEnd = BitConverter.ToInt64(binaryNetEnd, 0);
            long longHostEnd = System.Net.IPAddress.NetworkToHostOrder(longNetEnd);
            byte[] binaryHostEnd = BitConverter.GetBytes(longHostEnd);
            double doubleHostEnd = BitConverter.ToDouble(binaryHostEnd, 0);
            return doubleHostEnd;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="intHostEnd"></param>
        /// <returns></returns>   
        public static byte[] HostToNetworkIntBytes(int intHostEnd)
        {
            int intNetEnd = System.Net.IPAddress.HostToNetworkOrder(intHostEnd);
            byte[] bytesNetEnd = BitConverter.GetBytes(intNetEnd);
            return bytesNetEnd;

        }
    }
}