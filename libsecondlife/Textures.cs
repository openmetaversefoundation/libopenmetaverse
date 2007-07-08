/*
 * Copyright (c) 2006-2007, Second Life Reverse Engineering Team
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
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using System.IO;

namespace libsecondlife
{
    public abstract partial class LLObject
    {
        /// <summary>
        /// Represents all of the texturable faces for an object
        /// </summary>
        /// <remarks>Objects in Second Life have infinite faces, with each face
        /// using the properties of the default face unless set otherwise. So if
        /// you have a TextureEntry with a default texture uuid of X, and face 72
        /// has a texture UUID of Y, every face would be textured with X except for
        /// face 72 that uses Y. In practice however, primitives utilize a maximum
        /// of nine faces and avatars utilize</remarks>
        [Serializable]
        public class TextureEntry
        {
            /// <summary></summary>
            public TextureEntryFace DefaultTexture;
            /// <summary></summary>
            public SerializableDictionary<uint, TextureEntryFace> FaceTextures;

            /// <summary>
            /// Default constructor, DefaultTexture will be null
            /// </summary>
            public TextureEntry()
            {
                DefaultTexture = null;
                FaceTextures = new SerializableDictionary<uint, TextureEntryFace>();
            }

            /// <summary>
            /// Constructor that takes a default texture UUID
            /// </summary>
            /// <param name="defaultTextureID">Texture UUID to use as the default texture</param>
            public TextureEntry(LLUUID defaultTextureID)
            {
                DefaultTexture = new TextureEntryFace(null);
                DefaultTexture.TextureID = defaultTextureID;
                FaceTextures = new SerializableDictionary<uint, TextureEntryFace>();
            }

            /// <summary>
            /// Constructor that creates the TextureEntry class from a byte array
            /// </summary>
            /// <param name="data">Byte array containing the TextureEntry field</param>
            /// <param name="pos">Starting position of the TextureEntry field in 
            /// the byte array</param>
            /// <param name="length">Length of the TextureEntry field, in bytes</param>
            public TextureEntry(byte[] data, int pos, int length)
            {
                FromBytes(data, pos, length);
            }

            /// <summary>
            /// Returns the TextureEntryFace that is applied to the specified 
            /// index. If custom attributes are not set for this face that 
            /// would be the default texture for this TextureEntry. Do not 
            /// modify the returned TextureEntryFace, it will have undefined 
            /// results. Use CreateFace() to get a TextureEntryFace that is 
            /// safe for writing
            /// </summary>
            /// <param name="index">The index number of the face to retrieve</param>
            /// <returns>A TextureEntryFace containing all the properties for that
            /// face, suitable for read-only operations</returns>
            public TextureEntryFace GetFace(uint index)
            {
                if (FaceTextures.ContainsKey(index))
                    return FaceTextures[index];
                else
                    return DefaultTexture;
            }

            /// <summary>
            /// Check whether a custom face is defined for a particular index
            /// </summary>
            /// <param name="index">The index to check whether a custom face is
            /// defined for</param>
            /// <returns>True if this face has it's own TextureEntryFace, otherwise
            /// false</returns>
            public bool FaceExists(uint index)
            {
                return FaceTextures.ContainsKey(index);
            }

            /// <summary>
            /// This will either create a new face if a custom face for the given
            /// index is not defined, or return the custom face for that index if
            /// it already exists
            /// </summary>
            /// <param name="index">The index number of the face to create or 
            /// retrieve</param>
            /// <returns>A TextureEntryFace containing all the properties for that
            /// face</returns>
            public TextureEntryFace CreateFace(uint index)
            {
                if (!FaceTextures.ContainsKey(index))
                    FaceTextures[index] = new TextureEntryFace(this.DefaultTexture);

                return FaceTextures[index];
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public byte[] ToBytes()
            {
                if (DefaultTexture == null)
                {
                    return new byte[0];
                }

                MemoryStream memStream = new MemoryStream();
                BinaryWriter binWriter = new BinaryWriter(memStream);

                Dictionary<LLUUID, uint> TextureIDs = new Dictionary<LLUUID, uint>();
                Dictionary<uint, uint> RGBAs = new Dictionary<uint, uint>();
                Dictionary<float, uint> RepeatUs = new Dictionary<float, uint>();
                Dictionary<float, uint> RepeatVs = new Dictionary<float, uint>();
                Dictionary<short, uint> OffsetUs = new Dictionary<short, uint>();
                Dictionary<short, uint> OffsetVs = new Dictionary<short, uint>();
                Dictionary<short, uint> Rotations = new Dictionary<short, uint>();
                Dictionary<byte, uint> Flag1s = new Dictionary<byte, uint>();
                Dictionary<byte, uint> Flag2s = new Dictionary<byte, uint>();
                foreach (KeyValuePair<uint, TextureEntryFace> face in FaceTextures)
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

                    float fvalue = face.Value.RepeatU;
                    float fdefaultValue = DefaultTexture.RepeatU;

                    if (fvalue != fdefaultValue)
                    {
                        if (RepeatUs.ContainsKey(fvalue))
                            RepeatUs[fvalue] |= (uint)(1 << (int)face.Key);
                        else
                            RepeatUs[fvalue] = (uint)(1 << (int)face.Key);
                    }

                    fvalue = face.Value.RepeatV;
                    fdefaultValue = DefaultTexture.RepeatV;

                    if (fvalue != fdefaultValue)
                    {
                        if (RepeatVs.ContainsKey(fvalue))
                            RepeatVs[fvalue] |= (uint)(1 << (int)face.Key);
                        else
                            RepeatVs[fvalue] = (uint)(1 << (int)face.Key);
                    }

                    short value = Helpers.TEOffsetShort(face.Value.OffsetU);
                    short defaultValue = Helpers.TEOffsetShort(DefaultTexture.OffsetU);

                    if (value != defaultValue)
                    {
                        if (OffsetUs.ContainsKey(value))
                            OffsetUs[value] |= (uint)(1 << (int)face.Key);
                        else
                            OffsetUs[value] = (uint)(1 << (int)face.Key);
                    }

                    value = Helpers.TEOffsetShort(face.Value.OffsetV);
                    defaultValue = Helpers.TEOffsetShort(DefaultTexture.OffsetV);
                    if (value != defaultValue)
                    {
                        if (OffsetVs.ContainsKey(value))
                            OffsetVs[value] |= (uint)(1 << (int)face.Key);
                        else
                            OffsetVs[value] = (uint)(1 << (int)face.Key);
                    }

                    value = Helpers.TERotationShort(face.Value.Rotation);
                    defaultValue = Helpers.TERotationShort(DefaultTexture.Rotation);
                    if (value != defaultValue)
                    {
                        if (Rotations.ContainsKey(value))
                            Rotations[value] |= (uint)(1 << (int)face.Key);
                        else
                            Rotations[value] = (uint)(1 << (int)face.Key);
                    }

                    if (face.Value.material != DefaultTexture.material)
                    {
                        if (Flag1s.ContainsKey(face.Value.material))
                            Flag1s[face.Value.material] |= (uint)(1 << (int)face.Key);
                        else
                            Flag1s[face.Value.material] = (uint)(1 << (int)face.Key);
                    }

                    if (face.Value.media != DefaultTexture.media)
                    {
                        if (Flag2s.ContainsKey(face.Value.media))
                            Flag2s[face.Value.media] |= (uint)(1 << (int)face.Key);
                        else
                            Flag2s[face.Value.media] = (uint)(1 << (int)face.Key);
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
                binWriter.Write(DefaultTexture.RepeatU);
                foreach (KeyValuePair<float, uint> kv in RepeatUs)
                {
                    binWriter.Write(GetFaceBitfieldBytes(kv.Value));
                    binWriter.Write(kv.Key);
                }

                binWriter.Write((byte)0);
                binWriter.Write(DefaultTexture.RepeatV);
                foreach (KeyValuePair<float, uint> kv in RepeatVs)
                {
                    binWriter.Write(GetFaceBitfieldBytes(kv.Value));
                    binWriter.Write(kv.Key);
                }

                binWriter.Write((byte)0);
                binWriter.Write(Helpers.TEOffsetShort(DefaultTexture.OffsetU));
                foreach (KeyValuePair<short, uint> kv in OffsetUs)
                {
                    binWriter.Write(GetFaceBitfieldBytes(kv.Value));
                    binWriter.Write(kv.Key);
                }

                binWriter.Write((byte)0);
                binWriter.Write(Helpers.TEOffsetShort(DefaultTexture.OffsetV));
                foreach (KeyValuePair<short, uint> kv in OffsetVs)
                {
                    binWriter.Write(GetFaceBitfieldBytes(kv.Value));
                    binWriter.Write(kv.Key);
                }

                binWriter.Write((byte)0);
                binWriter.Write(Helpers.TERotationShort(DefaultTexture.Rotation));
                foreach (KeyValuePair<short, uint> kv in Rotations)
                {
                    binWriter.Write(GetFaceBitfieldBytes(kv.Value));
                    binWriter.Write(kv.Key);
                }

                binWriter.Write((byte)0);
                binWriter.Write(DefaultTexture.material);
                foreach (KeyValuePair<byte, uint> kv in Flag1s)
                {
                    binWriter.Write(GetFaceBitfieldBytes(kv.Value));
                    binWriter.Write(kv.Key);
                }

                binWriter.Write((byte)0);
                binWriter.Write(DefaultTexture.media);
                foreach (KeyValuePair<byte, uint> kv in Flag2s)
                {
                    binWriter.Write(GetFaceBitfieldBytes(kv.Value));
                    binWriter.Write(kv.Key);
                }

                return memStream.ToArray();
            }

            public override string ToString()
            {
                string output = String.Empty;

                output += "Default Face: " + DefaultTexture.ToString() + Environment.NewLine;

                foreach (KeyValuePair<uint, TextureEntryFace> face in FaceTextures)
                {
                    output += "Face " + face.Key + ": " + face.Value.ToString() + Environment.NewLine;
                }

                return output;
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
                            CreateFace(face).TextureID = tmpUUID;
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
                            CreateFace(face).RGBA = tmpUint;
                }
                //Read RepeatU -----------------------------------------
                DefaultTexture.RepeatU = Helpers.BytesToFloat(data, i);
                i += 4;

                while (ReadFaceBitfield(data, ref i, ref faceBits, ref BitfieldSize))
                {
                    float tmpFloat = Helpers.BytesToFloat(data, i);
                    i += 4;

                    for (uint face = 0, bit = 1; face < BitfieldSize; face++, bit <<= 1)
                        if ((faceBits & bit) != 0)
                            CreateFace(face).RepeatU = tmpFloat;
                }
                //Read RepeatV -----------------------------------------
                DefaultTexture.RepeatV = Helpers.BytesToFloat(data, i);
                i += 4;

                while (ReadFaceBitfield(data, ref i, ref faceBits, ref BitfieldSize))
                {
                    float tmpFloat = Helpers.BytesToFloat(data, i);
                    i += 4;

                    for (uint face = 0, bit = 1; face < BitfieldSize; face++, bit <<= 1)
                        if ((faceBits & bit) != 0)
                            CreateFace(face).RepeatV = tmpFloat;
                }
                //Read OffsetU -----------------------------------------
                DefaultTexture.OffsetU = Helpers.TEOffsetFloat(data, i);
                i += 2;

                while (ReadFaceBitfield(data, ref i, ref faceBits, ref BitfieldSize))
                {
                    float tmpFloat = Helpers.TEOffsetFloat(data, i);
                    i += 2;

                    for (uint face = 0, bit = 1; face < BitfieldSize; face++, bit <<= 1)
                        if ((faceBits & bit) != 0)
                            CreateFace(face).OffsetU = tmpFloat;
                }
                //Read OffsetV -----------------------------------------
                DefaultTexture.OffsetV = Helpers.TEOffsetFloat(data, i);
                i += 2;

                while (ReadFaceBitfield(data, ref i, ref faceBits, ref BitfieldSize))
                {
                    float tmpFloat = Helpers.TEOffsetFloat(data, i);
                    i += 2;

                    for (uint face = 0, bit = 1; face < BitfieldSize; face++, bit <<= 1)
                        if ((faceBits & bit) != 0)
                            CreateFace(face).OffsetV = tmpFloat;
                }
                //Read Rotation ----------------------------------------
                DefaultTexture.Rotation = Helpers.TERotationFloat(data, i);
                i += 2;

                while (ReadFaceBitfield(data, ref i, ref faceBits, ref BitfieldSize))
                {
                    float tmpFloat = Helpers.TERotationFloat(data, i);
                    i += 2;

                    for (uint face = 0, bit = 1; face < BitfieldSize; face++, bit <<= 1)
                        if ((faceBits & bit) != 0)
                            CreateFace(face).Rotation = tmpFloat;
                }
                //Read Material Flags ------------------------------------------
                DefaultTexture.material = data[i];
                i++;

                while (ReadFaceBitfield(data, ref i, ref faceBits, ref BitfieldSize))
                {
                    byte tmpByte = data[i];
                    i++;

                    for (uint face = 0, bit = 1; face < BitfieldSize; face++, bit <<= 1)
                        if ((faceBits & bit) != 0)
                            CreateFace(face).material = tmpByte;
                }
                //Read Media Flags ------------------------------------------
                DefaultTexture.media = data[i];
                i++;

                while (i - pos < length && ReadFaceBitfield(data, ref i, ref faceBits, ref BitfieldSize))
                {
                    byte tmpByte = data[i];
                    i++;

                    for (uint face = 0, bit = 1; face < BitfieldSize; face++, bit <<= 1)
                        if ((faceBits & bit) != 0)
                            CreateFace(face).media = tmpByte;
                }
            }
        }

        /// <summary>
        /// A single textured face. Don't instantiate this class yourself, use the
        /// methods in TextureEntry
        /// </summary>
        [Serializable]
        public class TextureEntryFace
        {
            // +----------+ S = Shiny
            // | SSFBBBBB | F = Fullbright
            // | 76543210 | B = Bumpmap
            // +----------+
            private const byte BUMP_MASK = 0x1F;
            private const byte FULLBRIGHT_MASK = 0x20;
            private const byte SHINY_MASK = 0xC0;
            // +----------+ M = Media Flags (web page)
            // | .....TTM | T = Texture Mapping
            // | 76543210 | . = Unused
            // +----------+
            private const byte MEDIA_MASK = 0x01;
            private const byte TEX_MAP_MASK = 0x06;

            private uint rgba;
            private float repeatU = 1.0f;
            private float repeatV = 1.0f;
            private float offsetU;
            private float offsetV;
            private float rotation;
            private TextureAttributes hasAttribute;
            private LLUUID textureID;
            private TextureEntryFace DefaultTexture = null;

            internal byte material;
            internal byte media;

            #region Properties

            /// <summary></summary>
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

            /// <summary></summary>
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

            /// <summary></summary>
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

            /// <summary></summary>
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

            /// <summary></summary>
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

            /// <summary></summary>
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

            /// <summary></summary>
            public Bumpiness Bump
            {
                get
                {
                    if ((hasAttribute & TextureAttributes.Material) != 0)
                        return (Bumpiness)(material & BUMP_MASK);
                    else
                        return DefaultTexture.Bump;
                }
                set
                {
                    // Clear out the old material value
                    material &= 0xE0;
                    // Put the new bump value in the material byte
                    material |= (byte)value;
                    hasAttribute |= TextureAttributes.Material;
                }
            }

            public Shininess Shiny
            {
                get
                {
                    if ((hasAttribute & TextureAttributes.Material) != 0)
                        return (Shininess)(material & SHINY_MASK);
                    else
                        return DefaultTexture.Shiny;
                }
                set
                {
                    // Clear out the old shiny value
                    material &= 0x3F;
                    // Put the new shiny value in the material byte
                    material |= (byte)value;
                    hasAttribute |= TextureAttributes.Material;
                }
            }

            public bool Fullbright
            {
                get
                {
                    if ((hasAttribute & TextureAttributes.Material) != 0)
                        return (material & FULLBRIGHT_MASK) != 0;
                    else
                        return DefaultTexture.Fullbright;
                }
                set
                {
                    // Clear out the old fullbright value
                    material &= 0xDF;
                    if (value)
                    {
                        material |= 0x20;
                        hasAttribute |= TextureAttributes.Material;
                    }
                }
            }

            /// <summary>In the future this will specify whether a webpage is
            /// attached to this face</summary>
            public bool MediaFlags
            {
                get
                {
                    if ((hasAttribute & TextureAttributes.Media) != 0)
                        return (media & MEDIA_MASK) != 0;
                    else
                        return DefaultTexture.MediaFlags;
                }
                set
                {
                    // Clear out the old mediaflags value
                    media &= 0xFE;
                    if (value)
                    {
                        media |= 0x01;
                        hasAttribute |= TextureAttributes.Media;
                    }
                }
            }

            public Mapping TexMapType
            {
                get
                {
                    if ((hasAttribute & TextureAttributes.Media) != 0)
                        return (Mapping)(media & TEX_MAP_MASK);
                    else
                        return DefaultTexture.TexMapType;
                }
                set
                {
                    // Clear out the old texmap value
                    media &= 0xF9;
                    // Put the new texmap value in the media byte
                    media |= (byte)value;
                    hasAttribute |= TextureAttributes.Media;
                }
            }

            /// <summary></summary>
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

            #endregion Properties

            /// <summary>
            /// Contains the definition for individual faces
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
            /// <returns></returns>
            public override string ToString()
            {
                return String.Format("RGBA: {0} RepeatU: {1} RepeatV: {2} OffsetU: {3} OffsetV: {4} Rotation: {5} " +
                    "TextureAttributes: {6} Material: {7} Media: {8} ID: {9}", rgba, repeatU, repeatV, offsetU, 
                    offsetV, rotation, hasAttribute.ToString(), material, media, textureID.ToStringHyphenated());
            }
        }
    }
}
