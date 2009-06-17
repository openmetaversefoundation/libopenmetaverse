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
using System.Collections.Generic;
using System.IO;
using OpenMetaverse.StructuredData;

namespace OpenMetaverse
{
    #region Enumerations

    /// <summary>
    /// The type of bump-mapping applied to a face
    /// </summary>
    public enum Bumpiness : byte
    {
        /// <summary></summary>
        None = 0,
        /// <summary></summary>
        Brightness = 1,
        /// <summary></summary>
        Darkness = 2,
        /// <summary></summary>
        Woodgrain = 3,
        /// <summary></summary>
        Bark = 4,
        /// <summary></summary>
        Bricks = 5,
        /// <summary></summary>
        Checker = 6,
        /// <summary></summary>
        Concrete = 7,
        /// <summary></summary>
        Crustytile = 8,
        /// <summary></summary>
        Cutstone = 9,
        /// <summary></summary>
        Discs = 10,
        /// <summary></summary>
        Gravel = 11,
        /// <summary></summary>
        Petridish = 12,
        /// <summary></summary>
        Siding = 13,
        /// <summary></summary>
        Stonetile = 14,
        /// <summary></summary>
        Stucco = 15,
        /// <summary></summary>
        Suction = 16,
        /// <summary></summary>
        Weave = 17
    }

    /// <summary>
    /// The level of shininess applied to a face
    /// </summary>
    public enum Shininess : byte
    {
        /// <summary></summary>
        None = 0,
        /// <summary></summary>
        Low = 0x40,
        /// <summary></summary>
        Medium = 0x80,
        /// <summary></summary>
        High = 0xC0
    }

    /// <summary>
    /// The texture mapping style used for a face
    /// </summary>
    public enum MappingType : byte
    {
        /// <summary></summary>
        Default = 0,
        /// <summary></summary>
        Planar = 2
    }

    /// <summary>
    /// Flags in the TextureEntry block that describe which properties are 
    /// set
    /// </summary>
    [Flags]
    public enum TextureAttributes : uint
    {
        /// <summary></summary>
        None = 0,
        /// <summary></summary>
        TextureID = 1 << 0,
        /// <summary></summary>
        RGBA = 1 << 1,
        /// <summary></summary>
        RepeatU = 1 << 2,
        /// <summary></summary>
        RepeatV = 1 << 3,
        /// <summary></summary>
        OffsetU = 1 << 4,
        /// <summary></summary>
        OffsetV = 1 << 5,
        /// <summary></summary>
        Rotation = 1 << 6,
        /// <summary></summary>
        Material = 1 << 7,
        /// <summary></summary>
        Media = 1 << 8,
        /// <summary></summary>
        Glow = 1 << 9,
        /// <summary></summary>
        All = 0xFFFFFFFF
    }

    #endregion Enumerations

    public partial class Primitive
    {
        #region Enums

        /// <summary>
        /// Texture animation mode
        /// </summary>
        [Flags]
        public enum TextureAnimMode : uint
        {
            /// <summary>Disable texture animation</summary>
            ANIM_OFF = 0x00,
            /// <summary>Enable texture animation</summary>
            ANIM_ON = 0x01,
            /// <summary>Loop when animating textures</summary>
            LOOP = 0x02,
            /// <summary>Animate in reverse direction</summary>
            REVERSE = 0x04,
            /// <summary>Animate forward then reverse</summary>
            PING_PONG = 0x08,
            /// <summary>Slide texture smoothly instead of frame-stepping</summary>
            SMOOTH = 0x10,
            /// <summary>Rotate texture instead of using frames</summary>
            ROTATE = 0x20,
            /// <summary>Scale texture instead of using frames</summary>
            SCALE = 0x40,
        }

        #endregion Enums

        #region Subclasses

        /// <summary>
        /// A single textured face. Don't instantiate this class yourself, use the
        /// methods in TextureEntry
        /// </summary>
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

            private Color4 rgba;
            private float repeatU;
            private float repeatV;
            private float offsetU;
            private float offsetV;
            private float rotation;
            private float glow;
            private byte materialb;
            private byte mediab;
            private TextureAttributes hasAttribute;
            private UUID textureID;
            private TextureEntryFace DefaultTexture;


            #region Properties

            /// <summary></summary>
            internal byte material
            {
                get
                {
                    if ((hasAttribute & TextureAttributes.Material) != 0)
                        return materialb;
                    else
                        return DefaultTexture.material;
                }
                set
                {
                    materialb = value;
                    hasAttribute |= TextureAttributes.Material;
                }
            }
            
            /// <summary></summary>
            internal byte media
            {
                get
                {
                    if ((hasAttribute & TextureAttributes.Media) != 0)
                        return mediab;
                    else
                        return DefaultTexture.media;
                }
                set
                {
                    mediab = value;
                    hasAttribute |= TextureAttributes.Media;
                }
            }
            
            /// <summary></summary>
            public Color4 RGBA
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
            public float Glow
            {
                get
                {
                    if ((hasAttribute & TextureAttributes.Glow) != 0)
                        return glow;
                    else
                        return DefaultTexture.glow;
                }
                set
                {
                    glow = value;
                    hasAttribute |= TextureAttributes.Glow;
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

            public MappingType TexMapType
            {
                get
                {
                    if ((hasAttribute & TextureAttributes.Media) != 0)
                        return (MappingType)(media & TEX_MAP_MASK);
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
            public UUID TextureID
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
                rgba = Color4.White;
                repeatU = 1.0f;
                repeatV = 1.0f;

                DefaultTexture = defaultTexture;
                if (DefaultTexture == null)
                    hasAttribute = TextureAttributes.All;
                else
                    hasAttribute = TextureAttributes.None;
            }

            public OSD GetOSD(int faceNumber)
            {
                OSDMap tex = new OSDMap(10);
                if (faceNumber >= 0) tex["face_number"] = OSD.FromInteger(faceNumber);
                tex["colors"] = OSD.FromColor4(RGBA);
                tex["scales"] = OSD.FromReal(RepeatU);
                tex["scalet"] = OSD.FromReal(RepeatV);
                tex["offsets"] = OSD.FromReal(OffsetU);
                tex["offsett"] = OSD.FromReal(OffsetV);
                tex["imagerot"] = OSD.FromReal(Rotation);
                tex["bump"] = OSD.FromInteger((int)Bump);
                tex["shiny"] = OSD.FromInteger((int)Shiny);
                tex["fullbright"] = OSD.FromBoolean(Fullbright);
                tex["media_flags"] = OSD.FromInteger(Convert.ToInt32(MediaFlags));
                tex["mapping"] = OSD.FromInteger((int)TexMapType);
                tex["glow"] = OSD.FromReal(Glow);

                if (TextureID != Primitive.TextureEntry.WHITE_TEXTURE)
                    tex["imageid"] = OSD.FromUUID(TextureID);
                else
                    tex["imageid"] = OSD.FromUUID(UUID.Zero);

                return tex;
            }

            public static TextureEntryFace FromOSD(OSD osd, TextureEntryFace defaultFace, out int faceNumber)
            {
                OSDMap map = (OSDMap)osd;

                TextureEntryFace face = new TextureEntryFace(defaultFace);
                faceNumber = (map.ContainsKey("face_number")) ? map["face_number"].AsInteger() : -1;
                Color4 rgba = face.RGBA;
                rgba = ((OSDArray)map["colors"]).AsColor4();
                face.RGBA = rgba;
                face.RepeatU = (float)map["scales"].AsReal();
                face.RepeatV = (float)map["scalet"].AsReal();
                face.OffsetU = (float)map["offsets"].AsReal();
                face.OffsetV = (float)map["offsett"].AsReal();
                face.Rotation = (float)map["imagerot"].AsReal();
                face.Bump = (Bumpiness)map["bump"].AsInteger();
                face.Shiny = (Shininess)map["shiny"].AsInteger();
                face.Fullbright = map["fullbright"].AsBoolean();
                face.MediaFlags = map["media_flags"].AsBoolean();
                face.TexMapType = (MappingType)map["mapping"].AsInteger();
                face.Glow = (float)map["glow"].AsReal();
                face.TextureID = map["imageid"].AsUUID();

                return face;
            }

            public override int GetHashCode()
            {
                return
                    RGBA.GetHashCode() ^
                    RepeatU.GetHashCode() ^
                    RepeatV.GetHashCode() ^
                    OffsetU.GetHashCode() ^
                    OffsetV.GetHashCode() ^
                    Rotation.GetHashCode() ^
                    Glow.GetHashCode() ^
                    Bump.GetHashCode() ^
                    Shiny.GetHashCode() ^
                    Fullbright.GetHashCode() ^
                    MediaFlags.GetHashCode() ^
                    TexMapType.GetHashCode() ^
                    TextureID.GetHashCode();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return String.Format("Color: {0} RepeatU: {1} RepeatV: {2} OffsetU: {3} OffsetV: {4} " +
                    "Rotation: {5} Bump: {6} Shiny: {7} Fullbright: {8} Mapping: {9} Media: {10} Glow: {11} ID: {12}",
                    RGBA, RepeatU, RepeatV, OffsetU, OffsetV, Rotation, Bump, Shiny, Fullbright, TexMapType,
                    MediaFlags, Glow, TextureID);
            }
        }

        /// <summary>
        /// Represents all of the texturable faces for an object
        /// </summary>
        /// <remarks>Grid objects have infinite faces, with each face
        /// using the properties of the default face unless set otherwise. So if
        /// you have a TextureEntry with a default texture uuid of X, and face 18
        /// has a texture UUID of Y, every face would be textured with X except for
        /// face 18 that uses Y. In practice however, primitives utilize a maximum
        /// of nine faces</remarks>
        public class TextureEntry
        {
            public const int MAX_FACES = 32;
            public static readonly UUID WHITE_TEXTURE = new UUID("5748decc-f629-461c-9a36-a35a221fe21f");

            /// <summary></summary>
            public TextureEntryFace DefaultTexture;
            /// <summary></summary>
            public TextureEntryFace[] FaceTextures = new TextureEntryFace[MAX_FACES];

            /// <summary>
            /// Constructor that takes a default texture UUID
            /// </summary>
            /// <param name="defaultTextureID">Texture UUID to use as the default texture</param>
            public TextureEntry(UUID defaultTextureID)
            {
                DefaultTexture = new TextureEntryFace(null);
                DefaultTexture.TextureID = defaultTextureID;
            }

            /// <summary>
            /// Constructor that takes a <code>TextureEntryFace</code> for the
            /// default face
            /// </summary>
            /// <param name="defaultFace">Face to use as the default face</param>
            public TextureEntry(TextureEntryFace defaultFace)
            {
                DefaultTexture = new TextureEntryFace(null);
                DefaultTexture.Bump = defaultFace.Bump;
                DefaultTexture.Fullbright = defaultFace.Fullbright;
                DefaultTexture.MediaFlags = defaultFace.MediaFlags;
                DefaultTexture.OffsetU = defaultFace.OffsetU;
                DefaultTexture.OffsetV = defaultFace.OffsetV;
                DefaultTexture.RepeatU = defaultFace.RepeatU;
                DefaultTexture.RepeatV = defaultFace.RepeatV;
                DefaultTexture.RGBA = defaultFace.RGBA;
                DefaultTexture.Rotation = defaultFace.Rotation;
                DefaultTexture.Glow = defaultFace.Glow;
                DefaultTexture.Shiny = defaultFace.Shiny;
                DefaultTexture.TexMapType = defaultFace.TexMapType;
                DefaultTexture.TextureID = defaultFace.TextureID;
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
                if (index >= MAX_FACES) throw new Exception(index + " is outside the range of MAX_FACES");

                if (FaceTextures[index] == null)
                    FaceTextures[index] = new TextureEntryFace(this.DefaultTexture);

                return FaceTextures[index];
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            public TextureEntryFace GetFace(uint index)
            {
                if (index >= MAX_FACES) throw new Exception(index + " is outside the range of MAX_FACES");

                if (FaceTextures[index] != null)
                    return FaceTextures[index];
                else
                    return DefaultTexture;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public OSD GetOSD()
            {
                OSDArray array = new OSDArray();

                // Always add default texture
                array.Add(DefaultTexture.GetOSD(-1));

                for (int i = 0; i < MAX_FACES; i++)
                {
                    if (FaceTextures[i] != null)
                        array.Add(FaceTextures[i].GetOSD(i));
                }

                return array;
            }

            public static TextureEntry FromOSD(OSD osd)
            {
                OSDArray array = (OSDArray)osd;
                OSDMap faceSD;

                if (array.Count > 0)
                {
                    int faceNumber;
                    faceSD = (OSDMap)array[0];
                    TextureEntryFace defaultFace = TextureEntryFace.FromOSD(faceSD, null, out faceNumber);
                    TextureEntry te = new TextureEntry(defaultFace);

                    for (int i = 1; i < array.Count; i++)
                    {
                        TextureEntryFace tex = TextureEntryFace.FromOSD(array[i], defaultFace, out faceNumber);
                        if (faceNumber >= 0 && faceNumber < te.FaceTextures.Length)
                            te.FaceTextures[faceNumber] = tex;
                    }

                    return te;
                }
                else
                {
                    throw new ArgumentException("SD contains no elements");
                }
            }

            private void FromBytes(byte[] data, int pos, int length)
            {
                if (length <= 0)
                {
                    // No TextureEntry to process
                    DefaultTexture = null;
                    return;
                }
                else
                {
                    DefaultTexture = new TextureEntryFace(null);
                }

                uint bitfieldSize = 0;
                uint faceBits = 0;
                int i = pos;

                #region Texture
                DefaultTexture.TextureID = new UUID(data, i);
                i += 16;

                while (ReadFaceBitfield(data, ref i, ref faceBits, ref bitfieldSize))
                {
                    UUID tmpUUID = new UUID(data, i);
                    i += 16;

                    for (uint face = 0, bit = 1; face < bitfieldSize; face++, bit <<= 1)
                        if ((faceBits & bit) != 0)
                            CreateFace(face).TextureID = tmpUUID;
                }
                #endregion Texture

                #region Color
                DefaultTexture.RGBA = new Color4(data, i, true);
                i += 4;

                while (ReadFaceBitfield(data, ref i, ref faceBits, ref bitfieldSize))
                {
                    Color4 tmpColor = new Color4(data, i, true);
                    i += 4;

                    for (uint face = 0, bit = 1; face < bitfieldSize; face++, bit <<= 1)
                        if ((faceBits & bit) != 0)
                            CreateFace(face).RGBA = tmpColor;
                }
                #endregion Color

                #region RepeatU
                DefaultTexture.RepeatU = Utils.BytesToFloat(data, i);
                i += 4;

                while (ReadFaceBitfield(data, ref i, ref faceBits, ref bitfieldSize))
                {
                    float tmpFloat = Utils.BytesToFloat(data, i);
                    i += 4;

                    for (uint face = 0, bit = 1; face < bitfieldSize; face++, bit <<= 1)
                        if ((faceBits & bit) != 0)
                            CreateFace(face).RepeatU = tmpFloat;
                }
                #endregion RepeatU

                #region RepeatV
                DefaultTexture.RepeatV = Utils.BytesToFloat(data, i);
                i += 4;

                while (ReadFaceBitfield(data, ref i, ref faceBits, ref bitfieldSize))
                {
                    float tmpFloat = Utils.BytesToFloat(data, i);
                    i += 4;

                    for (uint face = 0, bit = 1; face < bitfieldSize; face++, bit <<= 1)
                        if ((faceBits & bit) != 0)
                            CreateFace(face).RepeatV = tmpFloat;
                }
                #endregion RepeatV

                #region OffsetU
                DefaultTexture.OffsetU = Helpers.TEOffsetFloat(data, i);
                i += 2;

                while (ReadFaceBitfield(data, ref i, ref faceBits, ref bitfieldSize))
                {
                    float tmpFloat = Helpers.TEOffsetFloat(data, i);
                    i += 2;

                    for (uint face = 0, bit = 1; face < bitfieldSize; face++, bit <<= 1)
                        if ((faceBits & bit) != 0)
                            CreateFace(face).OffsetU = tmpFloat;
                }
                #endregion OffsetU

                #region OffsetV
                DefaultTexture.OffsetV = Helpers.TEOffsetFloat(data, i);
                i += 2;

                while (ReadFaceBitfield(data, ref i, ref faceBits, ref bitfieldSize))
                {
                    float tmpFloat = Helpers.TEOffsetFloat(data, i);
                    i += 2;

                    for (uint face = 0, bit = 1; face < bitfieldSize; face++, bit <<= 1)
                        if ((faceBits & bit) != 0)
                            CreateFace(face).OffsetV = tmpFloat;
                }
                #endregion OffsetV

                #region Rotation
                DefaultTexture.Rotation = Helpers.TERotationFloat(data, i);
                i += 2;

                while (ReadFaceBitfield(data, ref i, ref faceBits, ref bitfieldSize))
                {
                    float tmpFloat = Helpers.TERotationFloat(data, i);
                    i += 2;

                    for (uint face = 0, bit = 1; face < bitfieldSize; face++, bit <<= 1)
                        if ((faceBits & bit) != 0)
                            CreateFace(face).Rotation = tmpFloat;
                }
                #endregion Rotation

                #region Material
                DefaultTexture.material = data[i];
                i++;

                while (ReadFaceBitfield(data, ref i, ref faceBits, ref bitfieldSize))
                {
                    byte tmpByte = data[i];
                    i++;

                    for (uint face = 0, bit = 1; face < bitfieldSize; face++, bit <<= 1)
                        if ((faceBits & bit) != 0)
                            CreateFace(face).material = tmpByte;
                }
                #endregion Material

                #region Media
                DefaultTexture.media = data[i];
                i++;

                while (i - pos < length && ReadFaceBitfield(data, ref i, ref faceBits, ref bitfieldSize))
                {
                    byte tmpByte = data[i];
                    i++;

                    for (uint face = 0, bit = 1; face < bitfieldSize; face++, bit <<= 1)
                        if ((faceBits & bit) != 0)
                            CreateFace(face).media = tmpByte;
                }
                #endregion Media

                #region Glow
                DefaultTexture.Glow = Helpers.TEGlowFloat(data, i);
                i++;

                while (ReadFaceBitfield(data, ref i, ref faceBits, ref bitfieldSize))
                {
                    float tmpFloat = Helpers.TEGlowFloat(data, i);
                    i++;

                    for (uint face = 0, bit = 1; face < bitfieldSize; face++, bit <<= 1)
                        if ((faceBits & bit) != 0)
                            CreateFace(face).Glow = tmpFloat;
                }
 	  	        #endregion Glow
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public byte[] GetBytes()
            {
                if (DefaultTexture == null)
                    return Utils.EmptyBytes;

                MemoryStream memStream = new MemoryStream();
                BinaryWriter binWriter = new BinaryWriter(memStream);

                #region Bitfield Setup

                uint[] textures = new uint[FaceTextures.Length];
                InitializeArray(ref textures);
                uint[] rgbas = new uint[FaceTextures.Length];
                InitializeArray(ref rgbas);
                uint[] repeatus = new uint[FaceTextures.Length];
                InitializeArray(ref repeatus);
                uint[] repeatvs = new uint[FaceTextures.Length];
                InitializeArray(ref repeatvs);
                uint[] offsetus = new uint[FaceTextures.Length];
                InitializeArray(ref offsetus);
                uint[] offsetvs = new uint[FaceTextures.Length];
                InitializeArray(ref offsetvs);
                uint[] rotations = new uint[FaceTextures.Length];
                InitializeArray(ref rotations);
                uint[] materials = new uint[FaceTextures.Length];
                InitializeArray(ref materials);
                uint[] medias = new uint[FaceTextures.Length];
                InitializeArray(ref medias);
                uint[] glows = new uint[FaceTextures.Length];
                InitializeArray(ref glows);

                for (int i = 0; i < FaceTextures.Length; i++)
                {
                    if (FaceTextures[i] == null) continue;

                    if (FaceTextures[i].TextureID != DefaultTexture.TextureID)
                    {
                        if (textures[i] == UInt32.MaxValue) textures[i] = 0;
                        textures[i] |= (uint)(1 << i);
                    }
                    if (FaceTextures[i].RGBA != DefaultTexture.RGBA)
                    {
                        if (rgbas[i] == UInt32.MaxValue) rgbas[i] = 0;
                        rgbas[i] |= (uint)(1 << i);
                    }
                    if (FaceTextures[i].RepeatU != DefaultTexture.RepeatU)
                    {
                        if (repeatus[i] == UInt32.MaxValue) repeatus[i] = 0;
                        repeatus[i] |= (uint)(1 << i);
                    }
                    if (FaceTextures[i].RepeatV != DefaultTexture.RepeatV)
                    {
                        if (repeatvs[i] == UInt32.MaxValue) repeatvs[i] = 0;
                        repeatvs[i] |= (uint)(1 << i);
                    }
                    if (Helpers.TEOffsetShort(FaceTextures[i].OffsetU) != Helpers.TEOffsetShort(DefaultTexture.OffsetU))
                    {
                        if (offsetus[i] == UInt32.MaxValue) offsetus[i] = 0;
                        offsetus[i] |= (uint)(1 << i);
                    }
                    if (Helpers.TEOffsetShort(FaceTextures[i].OffsetV) != Helpers.TEOffsetShort(DefaultTexture.OffsetV))
                    {
                        if (offsetvs[i] == UInt32.MaxValue) offsetvs[i] = 0;
                        offsetvs[i] |= (uint)(1 << i);
                    }
                    if (Helpers.TERotationShort(FaceTextures[i].Rotation) != Helpers.TERotationShort(DefaultTexture.Rotation))
                    {
                        if (rotations[i] == UInt32.MaxValue) rotations[i] = 0;
                        rotations[i] |= (uint)(1 << i);
                    }
                    if (FaceTextures[i].material != DefaultTexture.material)
                    {
                        if (materials[i] == UInt32.MaxValue) materials[i] = 0;
                        materials[i] |= (uint)(1 << i);
                    }
                    if (FaceTextures[i].media != DefaultTexture.media)
                    {
                        if (medias[i] == UInt32.MaxValue) medias[i] = 0;
                        medias[i] |= (uint)(1 << i);
                    }
                    if (Helpers.TEGlowByte(FaceTextures[i].Glow) != Helpers.TEGlowByte(DefaultTexture.Glow))
                    {
                        if (glows[i] == UInt32.MaxValue) glows[i] = 0;
                        glows[i] |= (uint)(1 << i);
                    }
                }

                #endregion Bitfield Setup

                #region Texture
                binWriter.Write(DefaultTexture.TextureID.GetBytes());
                for (int i = 0; i < textures.Length; i++)
                {
                    if (textures[i] != UInt32.MaxValue)
                    {
                        binWriter.Write(GetFaceBitfieldBytes(textures[i]));
                        binWriter.Write(FaceTextures[i].TextureID.GetBytes());
                    }
                }
                binWriter.Write((byte)0);
                #endregion Texture

                #region Color
                // Serialize the color bytes inverted to optimize for zerocoding
                binWriter.Write(DefaultTexture.RGBA.GetBytes(true));
                for (int i = 0; i < rgbas.Length; i++)
                {
                    if (rgbas[i] != UInt32.MaxValue)
                    {
                        binWriter.Write(GetFaceBitfieldBytes(rgbas[i]));
                        // Serialize the color bytes inverted to optimize for zerocoding
                        binWriter.Write(FaceTextures[i].RGBA.GetBytes(true));
                    }
                }
                binWriter.Write((byte)0);
                #endregion Color

                #region RepeatU
                binWriter.Write(DefaultTexture.RepeatU);
                for (int i = 0; i < repeatus.Length; i++)
                {
                    if (repeatus[i] != UInt32.MaxValue)
                    {
                        binWriter.Write(GetFaceBitfieldBytes(repeatus[i]));
                        binWriter.Write(FaceTextures[i].RepeatU);
                    }
                }
                binWriter.Write((byte)0);
                #endregion RepeatU

                #region RepeatV
                binWriter.Write(DefaultTexture.RepeatV);
                for (int i = 0; i < repeatvs.Length; i++)
                {
                    if (repeatvs[i] != UInt32.MaxValue)
                    {
                        binWriter.Write(GetFaceBitfieldBytes(repeatvs[i]));
                        binWriter.Write(FaceTextures[i].RepeatV);
                    }
                }
                binWriter.Write((byte)0);
                #endregion RepeatV

                #region OffsetU
                binWriter.Write(Helpers.TEOffsetShort(DefaultTexture.OffsetU));
                for (int i = 0; i < offsetus.Length; i++)
                {
                    if (offsetus[i] != UInt32.MaxValue)
                    {
                        binWriter.Write(GetFaceBitfieldBytes(offsetus[i]));
                        binWriter.Write(Helpers.TEOffsetShort(FaceTextures[i].OffsetU));
                    }
                }
                binWriter.Write((byte)0);
                #endregion OffsetU

                #region OffsetV
                binWriter.Write(Helpers.TEOffsetShort(DefaultTexture.OffsetV));
                for (int i = 0; i < offsetvs.Length; i++)
                {
                    if (offsetvs[i] != UInt32.MaxValue)
                    {
                        binWriter.Write(GetFaceBitfieldBytes(offsetvs[i]));
                        binWriter.Write(Helpers.TEOffsetShort(FaceTextures[i].OffsetV));
                    }
                }
                binWriter.Write((byte)0);
                #endregion OffsetV

                #region Rotation
                binWriter.Write(Helpers.TERotationShort(DefaultTexture.Rotation));
                for (int i = 0; i < rotations.Length; i++)
                {
                    if (rotations[i] != UInt32.MaxValue)
                    {
                        binWriter.Write(GetFaceBitfieldBytes(rotations[i]));
                        binWriter.Write(Helpers.TERotationShort(FaceTextures[i].Rotation));
                    }
                }
                binWriter.Write((byte)0);
                #endregion Rotation

                #region Material
                binWriter.Write(DefaultTexture.material);
                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] != UInt32.MaxValue)
                    {
                        binWriter.Write(GetFaceBitfieldBytes(materials[i]));
                        binWriter.Write(FaceTextures[i].material);
                    }
                }
                binWriter.Write((byte)0);
                #endregion Material

                #region Media
                binWriter.Write(DefaultTexture.media);
                for (int i = 0; i < medias.Length; i++)
                {
                    if (medias[i] != UInt32.MaxValue)
                    {
                        binWriter.Write(GetFaceBitfieldBytes(medias[i]));
                        binWriter.Write(FaceTextures[i].media);
                    }
                }
                binWriter.Write((byte)0);
                #endregion Media

                #region Glow
                binWriter.Write(Helpers.TEGlowByte(DefaultTexture.Glow));
                for (int i = 0; i < glows.Length; i++)
                {
                    if (glows[i] != UInt32.MaxValue)
                    {
                        binWriter.Write(GetFaceBitfieldBytes(glows[i]));
                        binWriter.Write(Helpers.TEGlowByte(FaceTextures[i].Glow));
                    }
                }
                #endregion Glow

                return memStream.ToArray();
            }

            public override int GetHashCode()
            {
                int hashCode = DefaultTexture != null ? DefaultTexture.GetHashCode() : 0;
                for (int i = 0; i < FaceTextures.Length; i++)
                {
                    if (FaceTextures[i] != null)
                        hashCode ^= FaceTextures[i].GetHashCode();
                }
                return hashCode;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                string output = String.Empty;

                output += "Default Face: " + DefaultTexture.ToString() + Environment.NewLine;

                for (int i = 0; i < FaceTextures.Length; i++)
                {
                    if (FaceTextures[i] != null)
                        output += "Face " + i + ": " + FaceTextures[i].ToString() + Environment.NewLine;
                }

                return output;
            }

            #region Helpers

            private void InitializeArray(ref uint[] array)
            {
                for (int i = 0; i < array.Length; i++)
                    array[i] = UInt32.MaxValue;
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

            #endregion Helpers
        }

        /// <summary>
        /// Controls the texture animation of a particular prim
        /// </summary>
        public struct TextureAnimation
        {
            /// <summary></summary>
            public TextureAnimMode Flags;
            /// <summary></summary>
            public uint Face;
            /// <summary></summary>
            public uint SizeX;
            /// <summary></summary>
            public uint SizeY;
            /// <summary></summary>
            public float Start;
            /// <summary></summary>
            public float Length;
            /// <summary></summary>
            public float Rate;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="data"></param>
            /// <param name="pos"></param>
            public TextureAnimation(byte[] data, int pos)
            {
                if (data.Length >= 16)
                {
                    Flags = (TextureAnimMode)((uint)data[pos++]);
                    Face = (uint)data[pos++];
                    SizeX = (uint)data[pos++];
                    SizeY = (uint)data[pos++];

                    Start = Utils.BytesToFloat(data, pos);
                    Length = Utils.BytesToFloat(data, pos + 4);
                    Rate = Utils.BytesToFloat(data, pos + 8);
                }
                else
                {
                    Flags = 0;
                    Face = 0;
                    SizeX = 0;
                    SizeY = 0;

                    Start = 0.0f;
                    Length = 0.0f;
                    Rate = 0.0f;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public byte[] GetBytes()
            {
                byte[] data = new byte[16];
                int pos = 0;

                data[pos++] = (byte)Flags;
                data[pos++] = (byte)Face;
                data[pos++] = (byte)SizeX;
                data[pos++] = (byte)SizeY;

                Utils.FloatToBytes(Start).CopyTo(data, pos);
                Utils.FloatToBytes(Length).CopyTo(data, pos + 4);
                Utils.FloatToBytes(Rate).CopyTo(data, pos + 4);

                return data;
            }
        }

        #endregion Subclasses

        #region Public Members

        /// <summary></summary>
        public TextureEntry Textures;
        /// <summary></summary>
        public TextureAnimation TextureAnim;

        #endregion Public Members
    }
}
