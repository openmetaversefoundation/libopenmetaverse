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
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

namespace libsecondlife
{
    /// <summary>
    /// 
    /// </summary>
    public enum Bumpiness
    {
        /// <summary></summary>
        [XmlEnum("None")]
        None = 0,
        /// <summary></summary>
        [XmlEnum("Brightness")]
        Brightness = 1,
        /// <summary></summary>
        [XmlEnum("Darkness")]
        Darkness = 2,
        /// <summary></summary>
        [XmlEnum("Woodgrain")]
        Woodgrain = 3,
        /// <summary></summary>
        [XmlEnum("Bark")]
        Bark = 4,
        /// <summary></summary>
        [XmlEnum("Bricks")]
        Bricks = 5,
        /// <summary></summary>
        [XmlEnum("Checker")]
        Checker = 6,
        /// <summary></summary>
        [XmlEnum("Concrete")]
        Concrete = 7,
        /// <summary></summary>
        [XmlEnum("Crustytile")]
        Crustytile = 8,
        /// <summary></summary>
        [XmlEnum("Cutstone")]
        Cutstone = 9,
        /// <summary></summary>
        [XmlEnum("Discs")]
        Discs = 10,
        /// <summary></summary>
        [XmlEnum("Gravel")]
        Gravel = 11,
        /// <summary></summary>
        [XmlEnum("Petridish")]
        Petridish = 12,
        /// <summary></summary>
        [XmlEnum("Siding")]
        Siding = 13,
        /// <summary></summary>
        [XmlEnum("Stonetile")]
        Stonetile = 14,
        /// <summary></summary>
        [XmlEnum("Stucco")]
        Stucco = 15,
        /// <summary></summary>
        [XmlEnum("Suction")]
        Suction = 16,
        /// <summary></summary>
        [XmlEnum("Weave")]
        Weave = 17
    }

    /// <summary>
    /// 
    /// </summary>
    public enum Shininess
    {
        /// <summary></summary>
        [XmlEnum("None")]
        None = 0,
        /// <summary></summary>
        [XmlEnum("Low")]
        Low = 0x40,
        /// <summary></summary>
        [XmlEnum("Medium")]
        Medium = 0x80,
        /// <summary></summary>
        [XmlEnum("High")]
        High = 0xC0
    }

    /// <summary>
    /// 
    /// </summary>
    public enum Mapping
    {
        /// <summary></summary>
        [XmlEnum("Default")]
        Default = 0,
        /// <summary></summary>
        [XmlEnum("Planar")]
        Planar = 2
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class TextureEntry
    {
        /// <summary></summary>
        public TextureEntryFace DefaultTexture = null;
        /// <summary></summary>
        public SerializableDictionary<uint, TextureEntryFace> FaceTextures = 
            new SerializableDictionary<uint,TextureEntryFace>();

        /// <summary>
        /// 
        /// </summary>
        public TextureEntry()
        {
        }

        public TextureEntry(LLUUID textureID)
        {
            DefaultTexture = new TextureEntryFace(null);
            DefaultTexture.TextureID = textureID;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="pos"></param>
        public TextureEntry(byte[] data, int pos, int length)
        {
            FromBytes(data, pos, length);
        }

        public TextureEntryFace GetFace(uint index)
        {
            if (FaceTextures.ContainsKey(index))
                return FaceTextures[index];
            else
                return DefaultTexture;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public TextureEntryFace SetFace(uint index)
        {
            if (!FaceTextures.ContainsKey(index))
                FaceTextures[index] = new TextureEntryFace(this.DefaultTexture);

            return FaceTextures[index];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            if (DefaultTexture == null)
            {
                return new byte[0];
            }

            MemoryStream memStream = new MemoryStream();
            BinaryWriter binWriter = new BinaryWriter(memStream);

            Dictionary<LLUUID, uint> TextureIDs = new Dictionary<LLUUID,uint>();
            Dictionary<uint, uint> RGBAs = new Dictionary<uint, uint>();
            Dictionary<short, uint> RepeatUs = new Dictionary<short, uint>();
            Dictionary<short, uint> RepeatVs = new Dictionary<short, uint>();
            Dictionary<short, uint> OffsetUs = new Dictionary<short, uint>();
            Dictionary<short, uint> OffsetVs = new Dictionary<short, uint>();
            Dictionary<short, uint> Rotations = new Dictionary<short, uint>();
            Dictionary<byte, uint> Flag1s = new Dictionary<byte, uint>();
            Dictionary<byte, uint> Flag2s = new Dictionary<byte, uint>();
            foreach (KeyValuePair<uint,TextureEntryFace> face in FaceTextures)
            {
                if (face.Value.TextureID != DefaultTexture.TextureID)
                {
                    if (TextureIDs.ContainsKey(face.Value.TextureID))
                        TextureIDs[face.Value.TextureID] |= (uint)(1 << (int)face.Key);
                    else
                        TextureIDs[face.Value.TextureID] = (uint)(1 << (int)face.Key);
                }

                if (face.Value.RGBA != DefaultTexture.RGBA)
                {
                    if (RGBAs.ContainsKey(face.Value.RGBA))
                        RGBAs[face.Value.RGBA] |= (uint)(1 << (int)face.Key);
                    else
                        RGBAs[face.Value.RGBA] = (uint)(1 << (int)face.Key);
                }

                short value;
                short defaultValue;

                value = RepeatShort(face.Value.RepeatU);
                defaultValue = RepeatShort(DefaultTexture.RepeatU);
                if (value != defaultValue)
                {
                    if (RepeatUs.ContainsKey(value))
                        RepeatUs[value] |= (uint)(1 << (int)face.Key);
                    else
                        RepeatUs[value] = (uint)(1 << (int)face.Key);
                }

                value = RepeatShort(face.Value.RepeatV);
                defaultValue = RepeatShort(DefaultTexture.RepeatV);
                if (value != defaultValue)
                {
                    if (RepeatVs.ContainsKey(value))
                        RepeatVs[value] |= (uint)(1 << (int)face.Key);
                    else
                        RepeatVs[value] = (uint)(1 << (int)face.Key);
                }

                value = OffsetShort(face.Value.OffsetU);
                defaultValue = OffsetShort(DefaultTexture.OffsetU);
                if (value != defaultValue)
                {
                    if (OffsetUs.ContainsKey(value))
                        OffsetUs[value] |= (uint)(1 << (int)face.Key);
                    else
                        OffsetUs[value] = (uint)(1 << (int)face.Key);
                }

                value = OffsetShort(face.Value.OffsetV);
                defaultValue = OffsetShort(DefaultTexture.OffsetV);
                if (value != defaultValue)
                {
                    if (OffsetVs.ContainsKey(value))
                        OffsetVs[value] |= (uint)(1 << (int)face.Key);
                    else
                        OffsetVs[value] = (uint)(1 << (int)face.Key);
                }

                value = RotationShort(face.Value.Rotation);
                defaultValue = RotationShort(DefaultTexture.Rotation);
                if (value != defaultValue)
                {
                    if (Rotations.ContainsKey(value))
                        Rotations[value] |= (uint)(1 << (int)face.Key);
                    else
                        Rotations[value] = (uint)(1 << (int)face.Key);
                }

                if (face.Value.Flags1 != DefaultTexture.Flags1)
                {
                    if (Flag1s.ContainsKey(face.Value.Flags1))
                        Flag1s[face.Value.Flags1] |= (uint)(1 << (int)face.Key);
                    else
                        Flag1s[face.Value.Flags1] = (uint)(1 << (int)face.Key);
                }

                if (face.Value.Flags2 != DefaultTexture.Flags2)
                {
                    if (Flag2s.ContainsKey(face.Value.Flags2))
                        Flag2s[face.Value.Flags2] |= (uint)(1 << (int)face.Key);
                    else
                        Flag2s[face.Value.Flags2] = (uint)(1 << (int)face.Key);
                }
            }

            binWriter.Write(DefaultTexture.TextureID.Data);
            foreach (KeyValuePair<LLUUID, uint> kv in TextureIDs)
            {
                binWriter.Write(GetFaceBitfieldBytes(kv.Value));
                binWriter.Write(kv.Key.Data);
            }

            binWriter.Write((byte)0);
            binWriter.Write(DefaultTexture.RGBA);
            foreach (KeyValuePair<uint, uint> kv in RGBAs)
            {
                binWriter.Write(GetFaceBitfieldBytes(kv.Value));
                binWriter.Write(kv.Key);
            }

            binWriter.Write((byte)0);
            binWriter.Write(RepeatShort(DefaultTexture.RepeatU));
            foreach (KeyValuePair<short, uint> kv in RepeatUs)
            {
                binWriter.Write(GetFaceBitfieldBytes(kv.Value));
                binWriter.Write(kv.Key);
            }

            binWriter.Write((byte)0);
            binWriter.Write(RepeatShort(DefaultTexture.RepeatV));
            foreach (KeyValuePair<short, uint> kv in RepeatVs)
            {
                binWriter.Write(GetFaceBitfieldBytes(kv.Value));
                binWriter.Write(kv.Key);
            }

            binWriter.Write((byte)0);
            binWriter.Write(OffsetShort(DefaultTexture.OffsetU));
            foreach (KeyValuePair<short, uint> kv in OffsetUs)
            {
                binWriter.Write(GetFaceBitfieldBytes(kv.Value));
                binWriter.Write(kv.Key);
            }

            binWriter.Write((byte)0);
            binWriter.Write(OffsetShort(DefaultTexture.OffsetV));
            foreach (KeyValuePair<short, uint> kv in OffsetVs)
            {
                binWriter.Write(GetFaceBitfieldBytes(kv.Value));
                binWriter.Write(kv.Key);
            }

            binWriter.Write((byte)0);
            binWriter.Write(RotationShort(DefaultTexture.Rotation));
            foreach (KeyValuePair<short, uint> kv in Rotations)
            {
                binWriter.Write(GetFaceBitfieldBytes(kv.Value));
                binWriter.Write(kv.Key);
            }

            binWriter.Write((byte)0);
            binWriter.Write(DefaultTexture.Flags1);
            foreach (KeyValuePair<byte, uint> kv in Flag1s)
            {
                binWriter.Write(GetFaceBitfieldBytes(kv.Value));
                binWriter.Write(kv.Key);
            }

            binWriter.Write((byte)0);
            binWriter.Write(DefaultTexture.Flags2);
            foreach (KeyValuePair<byte, uint> kv in Flag2s)
            {
                binWriter.Write(GetFaceBitfieldBytes(kv.Value));
                binWriter.Write(kv.Key);
            }

            return memStream.GetBuffer();
        }

        private byte[] GetFaceBitfieldBytes(uint bitfield)
        {
            int byteLength = 0;
            uint tmpBitfield = bitfield;
            while (tmpBitfield != 0)
            {
                tmpBitfield >>= 7;
                byteLength++;
            }

            if (byteLength == 0)
                return new byte[1] { 0 };

            byte[] bytes = new byte[byteLength];
            for (int i = 0; i < byteLength; i++)
            {
                bytes[i] = (byte)((bitfield >> (7 * (byteLength - i - 1))) & 0x7F);
                if (i < byteLength - 1)
                    bytes[i] |= 0x80;
            }
            return bytes;
        }

        private bool ReadFaceBitfield(byte[] data, ref int pos, ref uint faceBits, ref uint bitfieldSize)
        {
            faceBits = 0;
            bitfieldSize = 0;

            if (pos >= data.Length)
                return false;

            byte b = 0;
            do
            {
                b = data[pos];
                faceBits = (faceBits << 7) | (uint)(b & 0x7F);
                bitfieldSize += 7;
                pos++;
            }
            while ((b & 0x80) != 0);

            return (faceBits != 0);
        }

        private float DequantizeSigned(byte[] byteArray, int pos, float upper)
        {
            short value = (short)(byteArray[pos] | (byteArray[pos + 1] << 8));
            float QV = (float)value;
            float QF = upper / 32767.0f;
            return (float)(QV * QF);
        }

        private short QuantizeSigned(float f, float upper)
        {
            float QF = 32767.0F / upper;
            return (short)(f * QF);
        }

        private short RepeatShort(float value)
        {
            return QuantizeSigned(value - 1.0f, 101.0f);
        }

        private short OffsetShort(float value)
        {
            return QuantizeSigned(value, 1.0f);
        }

        private short RotationShort(float value)
        {
            return QuantizeSigned(value, 359.995f);
        }

        private float RepeatFloat(byte[] data, int pos)
        {
            return DequantizeSigned(data, pos, 101.0f) + 1.0f;
        }

        private float OffsetFloat(byte[] data, int pos)
        {
            return DequantizeSigned(data, pos, 1.0f);
        }

        private float RotationFloat(byte[] data, int pos)
        {
            return DequantizeSigned(data, pos, 359.995f);
        }

        private void FromBytes(byte[] data, int pos, int length)
        {
            FaceTextures = new SerializableDictionary<uint, TextureEntryFace>();
            DefaultTexture = new TextureEntryFace(null);

            if (length <= 0) 
                return;  // No TextureEntry to process

            uint BitfieldSize = 0;
            uint faceBits = 0;
            int i = pos;

            //Read TextureID ---------------------------------------
            DefaultTexture.TextureID = new LLUUID(data, i);
            i += 16;

            while (ReadFaceBitfield(data, ref i, ref faceBits, ref BitfieldSize))
            {
                LLUUID tmpUUID = new LLUUID(data, i);
                i += 16;

                for (uint face = 0, bit = 1; face < BitfieldSize; face++, bit <<= 1)
                    if ((faceBits & bit) != 0)
                        SetFace(face).TextureID = tmpUUID;
            }
            //Read RGBA --------------------------------------------
            DefaultTexture.RGBA = (uint)(data[i] + (data[i + 1] << 8) + (data[i + 2] << 16) + (data[i + 3] << 24));
            i += 4;

            while (ReadFaceBitfield(data, ref i, ref faceBits, ref BitfieldSize))
            {
                uint tmpUint = (uint)(data[i] + (data[i + 1] << 8) + (data[i + 2] << 16) + (data[i + 3] << 24));
                i += 4;

                for (uint face = 0, bit = 1; face < BitfieldSize; face++, bit <<= 1)
                    if ((faceBits & bit) != 0)
                        SetFace(face).RGBA = tmpUint;
            }
            //Read RepeatU -----------------------------------------
            DefaultTexture.RepeatU = RepeatFloat(data, i);
            i += 2;

            while (ReadFaceBitfield(data, ref i, ref faceBits, ref BitfieldSize))
            {
                float tmpFloat = RepeatFloat(data, i);
                i += 2;

                for (uint face = 0, bit = 1; face < BitfieldSize; face++, bit <<= 1)
                    if ((faceBits & bit) != 0)
                        SetFace(face).RepeatU = tmpFloat;
            }
            //Read RepeatV -----------------------------------------
            DefaultTexture.RepeatV = RepeatFloat(data, i);
            i += 2;

            while (ReadFaceBitfield(data, ref i, ref faceBits, ref BitfieldSize))
            {
                float tmpFloat = RepeatFloat(data, i);
                i += 2;

                for (uint face = 0, bit = 1; face < BitfieldSize; face++, bit <<= 1)
                    if ((faceBits & bit) != 0)
                        SetFace(face).RepeatV = tmpFloat;
            }
            //Read OffsetU -----------------------------------------
            DefaultTexture.OffsetU = OffsetFloat(data, i);
            i += 2;

            while (ReadFaceBitfield(data, ref i, ref faceBits, ref BitfieldSize))
            {
                float tmpFloat = OffsetFloat(data, i);
                i += 2;

                for (uint face = 0, bit = 1; face < BitfieldSize; face++, bit <<= 1)
                    if ((faceBits & bit) != 0)
                        SetFace(face).OffsetU = tmpFloat;
            }
            //Read OffsetV -----------------------------------------
            DefaultTexture.OffsetV = OffsetFloat(data, i);
            i += 2;

            while (ReadFaceBitfield(data, ref i, ref faceBits, ref BitfieldSize))
            {
                float tmpFloat = OffsetFloat(data, i);
                i += 2;

                for (uint face = 0, bit = 1; face < BitfieldSize; face++, bit <<= 1)
                    if ((faceBits & bit) != 0)
                        SetFace(face).OffsetV = tmpFloat;
            }
            //Read Rotation ----------------------------------------
            DefaultTexture.Rotation = RotationFloat(data, i);
            i += 2;

            while (ReadFaceBitfield(data, ref i, ref faceBits, ref BitfieldSize))
            {
                float tmpFloat = RotationFloat(data, i);
                i += 2;

                for (uint face = 0, bit = 1; face < BitfieldSize; face++, bit <<= 1)
                    if ((faceBits & bit) != 0)
                        SetFace(face).Rotation = tmpFloat;
            }
            //Read Flags1 ------------------------------------------
            DefaultTexture.Flags1 = data[i];
            i++;

            while (ReadFaceBitfield(data, ref i, ref faceBits, ref BitfieldSize))
            {
                byte tmpByte = data[i];
                i++;

                for (uint face = 0, bit = 1; face < BitfieldSize; face++, bit <<= 1)
                    if ((faceBits & bit) != 0)
                        SetFace(face).Flags1 = tmpByte;
            }
            //Read Flags2 ------------------------------------------
            DefaultTexture.Flags2 = data[i];
            i++;

            while (i - pos < length && ReadFaceBitfield(data, ref i, ref faceBits, ref BitfieldSize))
            {
                byte tmpByte = data[i];
                i++;

                for (uint face = 0, bit = 1; face < BitfieldSize; face++, bit <<= 1)
                    if ((faceBits & bit) != 0)
                        SetFace(face).Flags2 = tmpByte;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class TextureEntryFace
    {
        [XmlAttribute] protected uint rgba;
        [XmlAttribute] protected float repeatU;
        [XmlAttribute] protected float repeatV;
        [XmlAttribute] protected float offsetU;
        [XmlAttribute] protected float offsetV;
        [XmlAttribute] protected float rotation;
        [XmlAttribute] protected byte flags1;
        [XmlAttribute] protected byte flags2;
        protected LLUUID textureID;
        protected TextureAttributes hasAttribute;
        protected TextureEntryFace DefaultTexture;

        [Flags, Serializable]
        public enum TextureAttributes : uint
        {
            None      = 0,
            TextureID = 1 << 0,
            RGBA      = 1 << 1,
            RepeatU   = 1 << 2,
            RepeatV   = 1 << 3,
            OffsetU   = 1 << 4,
            OffsetV   = 1 << 5,
            Rotation  = 1 << 6,
            Flags1    = 1 << 7,
            Flags2    = 1 << 8,
            All = 0xFFFFFFFF
        }

        /// <summary>
        /// 
        /// </summary>
        public TextureEntryFace()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="defaultTexture"></param>
        public TextureEntryFace(TextureEntryFace defaultTexture)
        {
            DefaultTexture = defaultTexture;
            if (DefaultTexture == null)
                hasAttribute = TextureAttributes.All;
            else
                hasAttribute = TextureAttributes.None;
        }

        /// <summary>
        /// 
        /// </summary>
        public LLUUID TextureID
        {
            get
            {
                if ((hasAttribute & TextureAttributes.TextureID) != 0)
                    return textureID;
                else
                    return DefaultTexture.textureID;
            }
            set
            {
                textureID = value;
                hasAttribute |= TextureAttributes.TextureID;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public uint RGBA
        {
            get
            {
                if ((hasAttribute & TextureAttributes.RGBA) != 0)
                    return rgba;
                else
                    return DefaultTexture.rgba;
            }
            set
            {
                rgba = value;
                hasAttribute |= TextureAttributes.RGBA;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public float RepeatU
        {
            get
            {
                if ((hasAttribute & TextureAttributes.RepeatU) != 0)
                    return repeatU;
                else
                    return DefaultTexture.repeatU;
            }
            set
            {
                repeatU = value;
                hasAttribute |= TextureAttributes.RepeatU;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public float RepeatV
        {
            get
            {
                if ((hasAttribute & TextureAttributes.RepeatV) != 0)
                    return repeatV;
                else
                    return DefaultTexture.repeatV;
            }
            set
            {
                repeatV = value;
                hasAttribute |= TextureAttributes.RepeatV;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public float OffsetU
        {
            get
            {
                if ((hasAttribute & TextureAttributes.OffsetU) != 0)
                    return offsetU;
                else
                    return DefaultTexture.offsetU;
            }
            set
            {
                offsetU = value;
                hasAttribute |= TextureAttributes.OffsetU;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public float OffsetV
        {
            get
            {
                if ((hasAttribute & TextureAttributes.OffsetV) != 0)
                    return offsetV;
                else
                    return DefaultTexture.offsetV;
            }
            set
            {
                offsetV = value;
                hasAttribute |= TextureAttributes.OffsetV;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public float Rotation
        {
            get
            {
                if ((hasAttribute & TextureAttributes.Rotation) != 0)
                    return rotation;
                else
                    return DefaultTexture.rotation;
            }
            set
            {
                rotation = value;
                hasAttribute |= TextureAttributes.Rotation;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public byte Flags1
        {
            get
            {
                if ((hasAttribute & TextureAttributes.Flags1) != 0)
                    return flags1;
                else
                    return DefaultTexture.flags1;
            }
            set
            {
                flags1 = value;
                hasAttribute |= TextureAttributes.Flags1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public byte Flags2
        {
            get
            {
                if ((hasAttribute & TextureAttributes.Flags2) != 0)
                    return flags2;
                else
                    return DefaultTexture.flags2;
            }
            set
            {
                flags2 = value;
                hasAttribute |= TextureAttributes.Flags2;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class TextureAnimation
    {
        /// <summary></summary>
        [XmlAttribute] public uint Flags;
        /// <summary></summary>
        [XmlAttribute] public uint Face;
        /// <summary></summary>
        [XmlAttribute] public uint SizeX;
        /// <summary></summary>
        [XmlAttribute] public uint SizeY;
        /// <summary></summary>
        [XmlAttribute] public float Start;
        /// <summary></summary>
        [XmlAttribute] public float Length;
        /// <summary></summary>
        [XmlAttribute] public float Rate;

        /// <summary>
        /// Default constructor
        /// </summary>
        public TextureAnimation()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="pos"></param>
        public TextureAnimation(byte[] data, int pos)
        {
            FromBytes(data, pos);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            byte[] bytes = new byte[0];
            // FIXME: Finish TextureAnimation GetBytes() function
            return bytes;
        }

        private void FromBytes(byte[] data, int pos)
        {
            int i = pos;

            if (data.Length == 0)
                return;

            Flags = (uint)data[i++];
            Face = (uint)data[i++];
            SizeX = (uint)data[i++];
            SizeY = (uint)data[i++];

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(data, i, 4);
                Array.Reverse(data, i + 4, 4);
                Array.Reverse(data, i + 8, 4);
            }

            Start = BitConverter.ToSingle(data, i);
            Length = BitConverter.ToSingle(data, i + 4);
            Rate = BitConverter.ToSingle(data, i + 8);
        }
    }

    /// <summary>
    /// A serializable dictionary of TextureEntryFace objects, indexed by 
    /// the prim face they are mapped to
    /// </summary>
    [Serializable]
    public class Faces : System.Collections.DictionaryBase, System.Xml.Serialization.IXmlSerializable
    {
        private const string NS = "http://www.libsecondlife.org/";

        /// <summary>
        /// Default constructor
        /// </summary>
        public Faces()
        {
        }

        public virtual TextureEntryFace this[uint key]
        {
            get
            {
                return (TextureEntryFace)this.Dictionary[key];
            }
            set
            {
                this.Dictionary[key] = value;
            }
        }

        public virtual void Add(uint key, TextureEntryFace value)
        {
            this.Dictionary.Add(key, value);
        }

        public virtual bool Contains(uint key)
        {
            return this.Dictionary.Contains(key);
        }

        public virtual bool ContainsKey(uint key)
        {
            return this.Dictionary.Contains(key);
        }

        public virtual bool ContainsValue(TextureEntryFace value)
        {
            foreach (TextureEntryFace item in this.Dictionary.Values)
            {
                if (item == value)
                    return true;
            }
            return false;
        }

        public virtual void Remove(uint key)
        {
            this.Dictionary.Remove(key);
        }

        public virtual System.Collections.ICollection Keys
        {
            get
            {
                return this.Dictionary.Keys;
            }
        }

        public virtual System.Collections.ICollection Values
        {
            get
            {
                return this.Dictionary.Values;
            }
        }

        void System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter w)
        {
            System.Xml.Serialization.XmlSerializer keySer =
                new System.Xml.Serialization.XmlSerializer(typeof(uint));
            System.Xml.Serialization.XmlSerializer valueSer =
                new System.Xml.Serialization.XmlSerializer(typeof(TextureEntryFace));
            w.WriteStartElement("dictionary", NS);
            foreach (object key in Dictionary.Keys)
            {
                w.WriteStartElement("item", NS);

                w.WriteStartElement("key", NS);
                keySer.Serialize(w, key);
                w.WriteEndElement();

                w.WriteStartElement("value", NS);
                object value = Dictionary[key];
                valueSer.Serialize(w, value);
                w.WriteEndElement();

                w.WriteEndElement();
            }
            w.WriteEndElement();
        }

        void System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader r)
        {
            System.Xml.Serialization.XmlSerializer keySer =
                new System.Xml.Serialization.XmlSerializer(typeof(string));
            System.Xml.Serialization.XmlSerializer valueSer =
                new System.Xml.Serialization.XmlSerializer(typeof(TextureEntryFace));

            r.Read();
            r.ReadStartElement("dictionary", NS);
            while (r.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                r.ReadStartElement("item", NS);

                r.ReadStartElement("key", NS);
                object key = keySer.Deserialize(r);
                r.ReadEndElement();

                r.ReadStartElement("value", NS);
                object value = valueSer.Deserialize(r);
                r.ReadEndElement();

                Dictionary.Add(key, value);

                r.ReadEndElement();
                r.MoveToContent();
            }
            r.ReadEndElement();
        }

        System.Xml.Schema.XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema()
        {
            return null;
        }
    }
}
