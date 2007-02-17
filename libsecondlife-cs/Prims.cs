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
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace libsecondlife
{
    public partial class Primitive : LLObject
    {
        /// <summary>
        /// Extra parameters for primitives, these flags are for features that have
        /// been added after the original ObjectFlags that has all eight bits 
        /// reserved already
        /// </summary>
        public enum ExtraParamType : ushort
        {
            /// <summary>Whether this object has flexible parameters</summary>
            [XmlEnum("Flexible")]
            Flexible = 0x10,
            /// <summary>Whether this object has light parameters</summary>
            [XmlEnum("Light")]
            Light = 0x20
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


        #region Subclasses

        /// <summary>
        /// Information on the flexible properties of a primitive
        /// </summary>
        [Serializable]
        public class FlexibleData
        {
            /// <summary></summary>
            [XmlAttribute("softness"), DefaultValue(0)]
            public int Softness;
            /// <summary></summary>
            [XmlAttribute("gravity"), DefaultValue(0)]
            public float Gravity;
            /// <summary></summary>
            [XmlAttribute("drag"), DefaultValue(0)]
            public float Drag;
            /// <summary></summary>
            [XmlAttribute("wind"), DefaultValue(0)]
            public float Wind;
            /// <summary></summary>
            [XmlAttribute("tension"), DefaultValue(0)]
            public float Tension;
            /// <summary></summary>
            public LLVector3 Force = LLVector3.Zero;

            /// <summary>
            /// 
            /// </summary>
            public FlexibleData()
            {
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="data"></param>
            /// <param name="pos"></param>
            public FlexibleData(byte[] data, int pos)
            {
                FromBytes(data, pos);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public byte[] GetBytes()
            {
                byte[] data = new byte[16];
                int i = 0;

                data[i] = (byte)((Softness & 2) << 6);
                data[i + 1] = (byte)((Softness & 1) << 7);

                data[i++] |= (byte)((byte)(Tension * 10.0f) & 0x7F);
                data[i++] |= (byte)((byte)(Drag * 10.0f) & 0x7F);
                data[i++] = (byte)((Gravity + 10.0f) * 10.0f);
                data[i++] = (byte)(Wind * 10.0f);

                Force.GetBytes().CopyTo(data, i);

                return data;
            }

            private void FromBytes(byte[] data, int pos)
            {
                int i = pos;

                Softness = ((data[i] & 0x80) >> 6) | ((data[i + 1] & 0x80) >> 7);

                Tension = (data[i++] & 0x7F) / 10.0f;
                Drag = (data[i++] & 0x7F) / 10.0f;
                Gravity = (data[i++] / 10.0f) - 10.0f;
                Wind = data[i++] / 10.0f;
                Force = new LLVector3(data, i);
            }
        }

        /// <summary>
        /// Information on the light properties of a primitive
        /// </summary>
        [Serializable]
        public class LightData
        {
            /// <summary></summary>
            [XmlAttribute("red"), DefaultValue(0)]
            public byte R;
            /// <summary></summary>
            [XmlAttribute("green"), DefaultValue(0)]
            public byte G;
            /// <summary></summary>
            [XmlAttribute("blue"), DefaultValue(0)]
            public byte B;
            /// <summary></summary>
            [XmlAttribute("intensity"), DefaultValue(0)]
            public float Intensity;
            /// <summary></summary>
            [XmlAttribute("radius"), DefaultValue(0)]
            public float Radius;
            /// <summary></summary>
            [XmlAttribute("falloff"), DefaultValue(0)]
            public float Falloff;

            /// <summary>
            /// 
            /// </summary>
            public LightData()
            {
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="data"></param>
            /// <param name="pos"></param>
            public LightData(byte[] data, int pos)
            {
                FromBytes(data, pos);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public byte[] GetBytes()
            {
                byte[] data = new byte[16];
                int i = 0;

                data[i++] = R;
                data[i++] = G;
                data[i++] = B;
                data[i++] = (byte)(Intensity * 255.0f);

                BitConverter.GetBytes(Radius).CopyTo(data, i);
                BitConverter.GetBytes(Falloff).CopyTo(data, i + 8);

                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(data, i, 4);
                    Array.Reverse(data, i + 8, 4);
                }

                return data;
            }

            private void FromBytes(byte[] data, int pos)
            {
                int i = pos;

                R = data[i++];
                G = data[i++];
                B = data[i++];
                Intensity = data[i++] / 255.0f;

                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(data, i, 4);
                    Array.Reverse(data, i + 8, 4);
                }

                Radius = BitConverter.ToSingle(data, i);
                Falloff = BitConverter.ToSingle(data, i + 8);
            }
        }

        #endregion Subclasses


        #region Public Members

        /// <summary></summary>
        public TextureAnimation TextureAnim = new TextureAnimation();
        /// <summary></summary>
        public FlexibleData Flexible = new FlexibleData();
        /// <summary></summary>
        public LightData Light = new LightData();
        /// <summary></summary>
        public ParticleSystem ParticleSys = new ParticleSystem();
        /// <summary></summary>
        public ObjectManager.ClickAction ClickAction;
        /// <summary></summary>
        public LLUUID Sound = LLUUID.Zero;
        /// <summary>Identifies the owner of the audio or particle system</summary>
        public LLUUID OwnerID = LLUUID.Zero;
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
            string output = "";

            output += "ID: " + ID + ", ";
            output += "GroupID: " + GroupID + ", ";
            output += "ParentID: " + ParentID + ", ";
            output += "LocalID: " + LocalID + ", ";
            output += "Flags: " + Flags + ", ";
            output += "State: " + data.State + ", ";
            output += "PCode: " + data.PCode + ", ";
            output += "Material: " + data.Material + ", ";

            return output;
        }

        public void ToXml(XmlWriter xmlWriter)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Primitive));
            serializer.Serialize(xmlWriter, this);
        }

        public static Primitive FromXml(XmlReader xmlReader)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Primitive));
            return (Primitive)serializer.Deserialize(xmlReader);
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
