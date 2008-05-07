/*
 * Copyright (c) 2006-2008, Second Life Reverse Engineering Team
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
using libsecondlife.StructuredData;

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

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public LLSD ToLLSD()
            {
                LLSDMap map = new LLSDMap();

                map["simulate_lod"] = LLSD.FromInteger(Softness);
                map["gravity"] = LLSD.FromReal(Gravity);
                map["air_friction"] = LLSD.FromReal(Drag);
                map["wind_sensitivity"] = LLSD.FromReal(Wind);
                map["tension"] = LLSD.FromReal(Tension);
                map["user_force"] = Force.ToLLSD();

                return map;
            }

            public static FlexibleData FromLLSD(LLSD llsd)
            {
                FlexibleData flex = new FlexibleData();

                if (llsd.Type == LLSDType.Map)
                {
                    LLSDMap map = (LLSDMap)llsd;

                    flex.Softness = map["simulate_lod"].AsInteger();
                    flex.Gravity = (float)map["gravity"].AsReal();
                    flex.Drag = (float)map["air_friction"].AsReal();
                    flex.Wind = (float)map["wind_sensitivity"].AsReal();
                    flex.Tension = (float)map["tension"].AsReal();
                    flex.Force.FromLLSD(map["user_force"]);
                }

                return flex;
            }
        }

        /// <summary>
        /// Information on the light properties of a primitive
        /// </summary>
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

            public LLSD ToLLSD()
            {
                LLSDMap map = new LLSDMap();

                map["color"] = Color.ToLLSD();
                map["intensity"] = LLSD.FromReal(Intensity);
                map["radius"] = LLSD.FromReal(Radius);
                map["cutoff"] = LLSD.FromReal(Cutoff);
                map["falloff"] = LLSD.FromReal(Falloff);

                return map;
            }

            public static LightData FromLLSD(LLSD llsd)
            {
                LightData light = new LightData();

                if (llsd.Type == LLSDType.Map)
                {
                    LLSDMap map = (LLSDMap)llsd;

                    light.Color.FromLLSD(map["color"]);
                    light.Intensity = (float)map["intensity"].AsReal();
                    light.Radius = (float)map["radius"].AsReal();
                    light.Cutoff = (float)map["cutoff"].AsReal();
                    light.Falloff = (float)map["falloff"].AsReal();
                }

                return light;
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

            public LLSD ToLLSD()
            {
                LLSDMap map = new LLSDMap();

                map["texture"] = LLSD.FromUUID(SculptTexture);
                map["type"] = LLSD.FromInteger((int)Type);

                return map;
            }

            public static SculptData FromLLSD(LLSD llsd)
            {
                SculptData sculpt = new SculptData();

                if (llsd.Type == LLSDType.Map)
                {
                    LLSDMap map = (LLSDMap)llsd;

                    sculpt.SculptTexture = map["texture"].AsUUID();
                    sculpt.Type = (SculptType)map["type"].AsInteger();
                }

                return sculpt;
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
        public ClickAction ClickAction;
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

        public LLSD ToLLSD()
        {
            LLSDMap path = new LLSDMap(14);
            path["begin"] = LLSD.FromReal(Data.PathBegin);
            path["curve"] = LLSD.FromInteger((int)Data.PathCurve);
            path["end"] = LLSD.FromReal(Data.PathEnd);
            path["radius_offset"] = LLSD.FromReal(Data.PathRadiusOffset);
            path["revolutions"] = LLSD.FromReal(Data.PathRevolutions);
            path["scale_x"] = LLSD.FromReal(Data.PathScaleX);
            path["scale_y"] = LLSD.FromReal(Data.PathScaleY);
            path["shear_x"] = LLSD.FromReal(Data.PathShearX);
            path["shear_y"] = LLSD.FromReal(Data.PathShearY);
            path["skew"] = LLSD.FromReal(Data.PathSkew);
            path["taper_x"] = LLSD.FromReal(Data.PathTaperX);
            path["taper_y"] = LLSD.FromReal(Data.PathTaperY);
            path["twist"] = LLSD.FromReal(Data.PathTwist);
            path["twist_begin"] = LLSD.FromReal(Data.PathTwistBegin);

            LLSDMap profile = new LLSDMap(4);
            profile["begin"] = LLSD.FromReal(Data.ProfileBegin);
            profile["curve"] = LLSD.FromInteger((int)Data.ProfileCurve);
            profile["hole"] = LLSD.FromInteger((int)Data.ProfileHole);
            profile["end"] = LLSD.FromReal(Data.ProfileEnd);
            profile["hollow"] = LLSD.FromReal(Data.ProfileHollow);

            LLSDMap volume = new LLSDMap(2);
            volume["path"] = path;
            volume["profile"] = profile;

            LLSDMap prim = new LLSDMap(9);
            prim["name"] = LLSD.FromString(Properties.Name);
            prim["description"] = LLSD.FromString(Properties.Description);
            prim["phantom"] = LLSD.FromBoolean(((Flags & ObjectFlags.Phantom) != 0));
            prim["physical"] = LLSD.FromBoolean(((Flags & ObjectFlags.Physics) != 0));
            prim["position"] = Position.ToLLSD();
            prim["rotation"] = Rotation.ToLLSD();
            prim["scale"] = Scale.ToLLSD();
            prim["material"] = LLSD.FromInteger((int)Data.Material);
            prim["shadows"] = LLSD.FromBoolean(((Flags & ObjectFlags.CastShadows) != 0));
            prim["textures"] = Textures.ToLLSD();
            prim["volume"] = volume;
            if (ParentID != 0)
                prim["parentid"] = LLSD.FromInteger(ParentID);

            prim["light"] = Light.ToLLSD();
            prim["flex"] = Flexible.ToLLSD();
            prim["sculpt"] = Sculpt.ToLLSD();

            return prim;
        }

        public static Primitive FromLLSD(LLSD llsd)
        {
            Primitive prim = new Primitive();
            LLObject.ObjectData data = new ObjectData();

            LLSDMap map = (LLSDMap)llsd;
            LLSDMap volume = (LLSDMap)map["volume"];
            LLSDMap path = (LLSDMap)volume["path"];
            LLSDMap profile = (LLSDMap)volume["profile"];

            #region Path/Profile

            data.PathBegin = (float)path["begin"].AsReal();
            data.PathCurve = (PathCurve)path["curve"].AsInteger();
            data.PathEnd = (float)path["end"].AsReal();
            data.PathRadiusOffset = (float)path["radius_offset"].AsReal();
            data.PathRevolutions = (float)path["revolutions"].AsReal();
            data.PathScaleX = (float)path["scale_x"].AsReal();
            data.PathScaleY = (float)path["scale_y"].AsReal();
            data.PathShearX = (float)path["shear_x"].AsReal();
            data.PathShearY = (float)path["shear_y"].AsReal();
            data.PathSkew = (float)path["skew"].AsReal();
            data.PathTaperX = (float)path["taper_x"].AsReal();
            data.PathTaperY = (float)path["taper_y"].AsReal();
            data.PathTwist = (float)path["twist"].AsReal();
            data.PathTwistBegin = (float)path["twist_begin"].AsReal();

            data.ProfileBegin = (float)profile["begin"].AsReal();
            data.ProfileCurve = (ProfileCurve)profile["curve"].AsInteger();
            data.ProfileHole = (HoleType)profile["hole"].AsInteger();
            data.ProfileEnd = (float)profile["end"].AsReal();
            data.ProfileHollow = (float)profile["hollow"].AsReal();

            #endregion Path/Profile

            prim.Data = data;
            
            if (map["phantom"].AsBoolean())
                prim.Flags |= ObjectFlags.Phantom;

            if (map["physical"].AsBoolean())
                prim.Flags |= ObjectFlags.Physics;

            if (map["shadows"].AsBoolean())
                prim.Flags |= ObjectFlags.CastShadows;

            prim.ParentID = (uint)map["parentid"].AsInteger();
            prim.Position.FromLLSD(map["position"]);
            prim.Rotation.FromLLSD(map["rotation"]);
            prim.Scale.FromLLSD(map["scale"]);
            prim.Data.Material = (MaterialType)map["material"].AsInteger();
            prim.Flexible = FlexibleData.FromLLSD(map["flex"]);
            prim.Light = LightData.FromLLSD(map["light"]);
            prim.Sculpt = SculptData.FromLLSD(map["sculpt"]);
            prim.Textures = TextureEntry.FromLLSD(map["textures"]);
            if (!string.IsNullOrEmpty(map["name"].AsString())) {
                prim.Properties.Name = map["name"].AsString();
            }

            if (!string.IsNullOrEmpty(map["description"].AsString())) {
                prim.Properties.Description = map["description"].AsString();
            }

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
