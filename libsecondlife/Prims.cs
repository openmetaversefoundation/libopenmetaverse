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

namespace libsecondlife
{
    public partial class Primitive : LLObject
    {
        #region Enums

        /// <summary>
        /// Extra parameters for primitives, these flags are for features that have
        /// been added after the original ObjectFlags that has all eight bits 
        /// reserved already
        /// </summary>
        [Flags]
        public enum ExtraParamType : ushort
        {
            /// <summary>Whether this object has flexible parameters</summary>
            Flexible = 0x10,
            /// <summary>Whether this object has light parameters</summary>
            Light = 0x20,
            /// <summary>Whether this object is a sculpted prim</summary>
            Sculpt = 0x30
        }

        /// <summary>
        /// 
        /// </summary>
        public enum JointType : byte
        {
            /// <summary></summary>
            Invalid = 0,
            /// <summary></summary>
            Hinge = 1,
            /// <summary></summary>
            Point = 2,
            /// <summary></summary>
            [Obsolete]
            LPoint = 3,
            /// <summary></summary>
            [Obsolete]
            Wheel = 4
        }

        /// <summary>
        /// 
        /// </summary>
        public enum SculptType : byte
        {
            /// <summary></summary>
            None = 0,
            /// <summary></summary>
            Sphere = 1,
            /// <summary></summary>
            Torus = 2,
            /// <summary></summary>
            Plane = 3,
            /// <summary></summary>
            Cylinder = 4
        }

        /// <summary>
        /// 
        /// </summary>
        public enum FaceType : ushort
        {
            /// <summary></summary>
            PathBegin = 0x1 << 0,
            /// <summary></summary>
            PathEnd = 0x1 << 1,
            /// <summary></summary>
            InnerSide = 0x1 << 2,
            /// <summary></summary>
            ProfileBegin = 0x1 << 3,
            /// <summary></summary>
            ProfileEnd = 0x1 << 4,
            /// <summary></summary>
            OuterSide0 = 0x1 << 5,
            /// <summary></summary>
            OuterSide1 = 0x1 << 6,
            /// <summary></summary>
            OuterSide2 = 0x1 << 7,
            /// <summary></summary>
            OuterSide3 = 0x1 << 8
        }

        #endregion Enums

        #region Subclasses

        /// <summary>
        /// Controls the texture animation of a particular prim
        /// </summary>
        [Serializable]
        public struct TextureAnimation
        {
            /// <summary></summary>
            public uint Flags;
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
                    Flags = (uint)data[pos++];
                    Face = (uint)data[pos++];
                    SizeX = (uint)data[pos++];
                    SizeY = (uint)data[pos++];

                    Start = Helpers.BytesToFloat(data, pos);
                    Length = Helpers.BytesToFloat(data, pos + 4);
                    Rate = Helpers.BytesToFloat(data, pos + 8);
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

                Helpers.FloatToBytes(Start).CopyTo(data, pos);
                Helpers.FloatToBytes(Length).CopyTo(data, pos + 4);
                Helpers.FloatToBytes(Rate).CopyTo(data, pos + 4);

                return data;
            }
        }

        /// <summary>
        /// Information on the flexible properties of a primitive
        /// </summary>
        [Serializable]
        public struct FlexibleData
        {
            /// <summary></summary>
            public int Softness;
            /// <summary></summary>
            public float Gravity;
            /// <summary></summary>
            public float Drag;
            /// <summary></summary>
            public float Wind;
            /// <summary></summary>
            public float Tension;
            /// <summary></summary>
            public LLVector3 Force;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="data"></param>
            /// <param name="pos"></param>
            public FlexibleData(byte[] data, int pos)
            {
                if (data.Length >= 5)
                {
                    Softness = ((data[pos] & 0x80) >> 6) | ((data[pos + 1] & 0x80) >> 7);

                    Tension = (float)(data[pos++] & 0x7F) / 10.0f;
                    Drag = (float)(data[pos++] & 0x7F) / 10.0f;
                    Gravity = (float)(data[pos++] / 10.0f) - 10.0f;
                    Wind = (float)data[pos++] / 10.0f;
                    Force = new LLVector3(data, pos);
                }
                else
                {
                    Softness = 0;

                    Tension = 0.0f;
                    Drag = 0.0f;
                    Gravity = 0.0f;
                    Wind = 0.0f;
                    Force = LLVector3.Zero;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public byte[] GetBytes()
            {
                byte[] data = new byte[16];
                int i = 0;

                // Softness is packed in the upper bits of tension and drag
                data[i] = (byte)((Softness & 2) << 6);
                data[i + 1] = (byte)((Softness & 1) << 7);

                data[i++] |= (byte)((byte)(Tension * 10.01f) & 0x7F);
                data[i++] |= (byte)((byte)(Drag * 10.01f) & 0x7F);
                data[i++] = (byte)((Gravity + 10.0f) * 10.01f);
                data[i++] = (byte)(Wind * 10.01f);

                Force.GetBytes().CopyTo(data, i);

                return data;
            }
        }

        /// <summary>
        /// Information on the light properties of a primitive
        /// </summary>
        [Serializable]
        public struct LightData
        {
            /// <summary></summary>
            public LLColor Color;
            /// <summary></summary>
            public float Intensity;
            /// <summary></summary>
            public float Radius;
            /// <summary></summary>
            public float Cutoff;
            /// <summary></summary>
            public float Falloff;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="data"></param>
            /// <param name="pos"></param>
            public LightData(byte[] data, int pos)
            {
                if (data.Length - pos >= 16)
                {
                    Color = new LLColor(data, pos, false);
                    Radius = Helpers.BytesToFloat(data, pos + 4);
                    Cutoff = Helpers.BytesToFloat(data, pos + 8);
                    Falloff = Helpers.BytesToFloat(data, pos + 12);

                    // Alpha in color is actually intensity
                    Intensity = Color.A;
                    Color.A = 1f;
                }
                else
                {
                    Color = LLColor.Black;
                    Radius = 0f;
                    Cutoff = 0f;
                    Falloff = 0f;
                    Intensity = 0f;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public byte[] GetBytes()
            {
                byte[] data = new byte[16];

                // Alpha channel in color is intensity
                LLColor tmpColor = Color;
                tmpColor.A = Intensity;
                tmpColor.GetBytes().CopyTo(data, 0);
                Helpers.FloatToBytes(Radius).CopyTo(data, 4);
                Helpers.FloatToBytes(Cutoff).CopyTo(data, 8);
                Helpers.FloatToBytes(Falloff).CopyTo(data, 12);

                return data;
            }
            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return String.Format("Color: {0} Intensity: {1} Radius: {2} Cutoff: {3} Falloff: {4}",
                    Color, Intensity, Radius, Cutoff, Falloff);
            }
        }

        /// <summary>
        /// Information on the sculpt properties of a sculpted primitive
        /// </summary>
        [Serializable]
        public struct SculptData
        {
            public LLUUID SculptTexture;
            public SculptType Type;

            public SculptData(byte[] data, int pos)
            {
                if (data.Length >= 17)
                {
                    SculptTexture = new LLUUID(data, pos);
                    Type = (SculptType)data[pos + 16];
                }
                else
                {
                    SculptTexture = LLUUID.Zero;
                    Type = SculptType.None;
                }
            }

            public byte[] GetBytes()
            {
                byte[] data = new byte[17];

                SculptTexture.GetBytes().CopyTo(data, 0);
                data[16] = (byte)Type;

                return data;
            }
        }

        #endregion Subclasses

        #region Public Members

        /// <summary></summary>
        public TextureAnimation TextureAnim;
        /// <summary></summary>
        public FlexibleData Flexible;
        /// <summary></summary>
        public LightData Light;
        /// <summary></summary>
        public SculptData Sculpt;
        /// <summary></summary>
        public ParticleSystem ParticleSys;
        /// <summary></summary>
        public ObjectManager.ClickAction ClickAction;
        /// <summary></summary>
        public LLUUID Sound;
        /// <summary>Identifies the owner of the audio or particle system</summary>
        public LLUUID OwnerID;
        /// <summary></summary>
        public byte SoundFlags;
        /// <summary></summary>
        public float SoundGain;
        /// <summary></summary>
        public float SoundRadius;
        /// <summary></summary>
        public string Text;
        /// <summary></summary>
        public LLColor TextColor;
        /// <summary></summary>
        public string MediaURL;
        /// <summary></summary>
        public JointType Joint;
        /// <summary></summary>
        public LLVector3 JointPivot;
        /// <summary></summary>
        public LLVector3 JointAxisOrAnchor;

        #endregion Public Members

        /// <summary>
        /// Default constructor
        /// </summary>
        public Primitive()
        {
        }

        public override string ToString()
        {
            return String.Format("ID: {0}, GroupID: {1}, ParentID: {2}, LocalID: {3}, Flags: {4}, " +
                "State: {5}, PCode: {6}, Material: {7}", ID, GroupID, ParentID, LocalID, Flags, Data.State,
                Data.PCode, Data.Material);
        }

        public Dictionary<string, object> ToLLSD()
        {
            Dictionary<string, object> path = new Dictionary<string, object>(14);
            path["begin"] = Data.PathBegin;
            path["curve"] = Data.PathCurve;
            path["end"] = Data.PathEnd;
            path["radius_offset"] = Data.PathRadiusOffset;
            path["revolutions"] = Data.PathRevolutions;
            path["scale_x"] = Data.PathScaleX;
            path["scale_y"] = Data.PathScaleY;
            path["shear_x"] = Data.PathShearX;
            path["shear_y"] = Data.PathShearY;
            path["skew"] = Data.PathSkew;
            path["taper_x"] = Data.PathTaperX;
            path["taper_y"] = Data.PathTaperY;
            path["twist"] = Data.PathTwist;
            path["twist_begin"] = Data.PathTwistBegin;

            Dictionary<string, object> profile = new Dictionary<string, object>(4);
            profile["begin"] = Data.ProfileBegin;
            profile["curve"] = Data.ProfileCurve;
            profile["end"] = Data.ProfileEnd;
            profile["hollow"] = Data.ProfileHollow;

            Dictionary<string, object> volume = new Dictionary<string, object>(2);
            volume["path"] = path;
            volume["profile"] = profile;

            Dictionary<string, object> prim = new Dictionary<string, object>(9);
            prim["phantom"] = ((Flags & ObjectFlags.Phantom) != 0);
            prim["physical"] = ((Flags & ObjectFlags.Physics) != 0);
            prim["position"] = Position.ToLLSD();
            prim["rotation"] = Rotation.ToLLSD();
            prim["scale"] = Scale.ToLLSD();
            prim["shadows"] = ((Flags & ObjectFlags.CastShadows) != 0);
            prim["textures"] = Textures.ToLLSD();
            prim["volume"] = volume;
            if (ParentID != 0)
                prim["parentid"] = ParentID;

            return prim;
        }

        internal int SetExtraParamsFromBytes(byte[] data, int pos)
        {
            int i = pos;
            int totalLength = 1;

            if (data.Length == 0 || pos >= data.Length)
                return 0;

            try
            {
                byte extraParamCount = data[i++];

                for (int k = 0; k < extraParamCount; k++)
                {
                    ExtraParamType type = (ExtraParamType)Helpers.BytesToUInt16(data, i);
                    i += 2;

                    uint paramLength = Helpers.BytesToUIntBig(data, i);
                    i += 4;

                    if (type == ExtraParamType.Flexible)
                        Flexible = new FlexibleData(data, i);
                    else if (type == ExtraParamType.Light)
                        Light = new LightData(data, i);
                    else if (type == ExtraParamType.Sculpt)
                        Sculpt = new SculptData(data, i);

                    i += (int)paramLength;
                    totalLength += (int)paramLength + 6;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return totalLength;
        }
    }
}
