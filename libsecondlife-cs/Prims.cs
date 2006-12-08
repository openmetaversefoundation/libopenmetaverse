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
    /// <summary>
    /// Primary parameters for primitives such as Physics Enabled or Phantom
    /// </summary>
    [Flags]
    public enum ObjectFlags
    {
        /// <summary>None of the primary flags are enabled</summary>
        None = 0,
        /// <summary>Whether physics are enabled for this object</summary>
        Physics = 1 << 0,
        /// <summary></summary>
        CreateSelected = 1 << 1,
        Unknown1 = 1 << 2,
        Unknown2 = 1 << 3,
        Unknown3 = 1 << 4,
        Unknown4 = 1 << 5,
        Script = 1 << 6,
        /// <summary>Whether this object contains an active touch script</summary>
        Touch = 1 << 7,
        Unknown5 = 1 << 8,
        /// <summary>Whether this object can receive payments</summary>
        Money = 1 << 9,
        /// <summary>Whether this object is phantom (no collisions)</summary>
        Phantom = 1 << 10,
        Unknown6 = 1 << 11,
        Unknown7 = 1 << 12,
        Unknown8 = 1 << 13,
        Unknown9 = 1 << 14,
        Unknown10 = 1 << 15,
        Unknown11 = 1 << 16,
        Unknown12 = 1 << 17,
        Unknown13 = 1 << 18,
        Unknown14 = 1 << 19,
        Unknown15 = 1 << 20,
        Unknown16 = 1 << 21,
        Unknown17 = 1 << 22,
        Unknown18 = 1 << 23,
        Unknown19 = 1 << 24,
        Unknown20 = 1 << 25,
        Unknown21 = 1 << 26,
        Unknown22 = 1 << 27,
        Unknown23 = 1 << 28,
        Unknown24 = 1 << 29,
        /// <summary>Whether this object is temporary</summary>
        Temp = 1 << 30,
        Unknown25 = 1 << 31,
        Unknown26 = 1 << 32
    }

    /// <summary>
    /// Extra parameters for primitives, these flags are for features that have
    /// been added after the original ObjectFlags that has all eight bits 
    /// reserved already
    /// </summary>
    public enum ExtraParamType : ushort
    {
        /// <summary>Whether this object has flexible parameters</summary>
        [XmlEnum("Flexible")] Flexible = 0x10,
        /// <summary>Whether this object has light parameters</summary>
        [XmlEnum("Light")] Light = 0x20
    }

    /// <summary>
    /// Sweet delicious prims.
    /// </summary>
    [Serializable]
    public class PrimObject
	{
        /// <summary></summary>
        [XmlAttribute("pathtwistbegin"), DefaultValue(0)] public int PathTwistBegin;
        /// <summary></summary>
        [XmlAttribute("pathend"), DefaultValue(0)] public float PathEnd;
        /// <summary></summary>
        [XmlAttribute("profilebegin"), DefaultValue(0)] public float ProfileBegin;
        /// <summary></summary>
        [XmlAttribute("pathradiusoffset"), DefaultValue(0)] public float PathRadiusOffset;
        /// <summary></summary>
        [XmlAttribute("pathskew"), DefaultValue(0)] public float PathSkew;
        /// <summary></summary>
        [XmlAttribute("profilecurve"), DefaultValue(0)] public uint ProfileCurve;
        /// <summary></summary>
        [XmlAttribute("pathscalex"), DefaultValue(0)] public float PathScaleX;
        /// <summary></summary>
        [XmlAttribute("pathscaley"), DefaultValue(0)] public float PathScaleY;
        /// <summary></summary>
        [XmlAttribute("localid"), DefaultValue(0)] public uint LocalID;
        /// <summary></summary>
        [XmlAttribute("parentid"), DefaultValue(0)] public uint ParentID;
        /// <summary></summary>
        [XmlAttribute("material"), DefaultValue(0)] public uint Material;
        /// <summary></summary>
        [XmlAttribute("name"), DefaultValue("")] public string Name = "";
        /// <summary></summary>
        [XmlAttribute("description"), DefaultValue("")] public string Description = "";
        /// <summary></summary>
        [XmlAttribute("pathshearx"), DefaultValue(0)] public float PathShearX;
        /// <summary></summary>
        [XmlAttribute("pathsheary"), DefaultValue(0)] public float PathShearY;
        /// <summary></summary>
        [XmlAttribute("pathtaperx"), DefaultValue(0)] public float PathTaperX;
        /// <summary></summary>
        [XmlAttribute("pathtapery"), DefaultValue(0)] public float PathTaperY;
        /// <summary></summary>
        [XmlAttribute("profileend"), DefaultValue(0)] public float ProfileEnd;
        /// <summary></summary>
        [XmlAttribute("pathbegin"), DefaultValue(0)] public float PathBegin;
        /// <summary></summary>
        [XmlAttribute("pathcurve"), DefaultValue(0)] public uint PathCurve;
        /// <summary></summary>
        [XmlAttribute("pathtwist"), DefaultValue(0)] public int PathTwist;
        /// <summary></summary>
        [XmlAttribute("profilehollow"), DefaultValue(0)] public uint ProfileHollow;
        /// <summary></summary>
        [XmlAttribute("pathrevolutions"), DefaultValue(0)] public float PathRevolutions;
        /// <summary></summary>
        [XmlAttribute("state"), DefaultValue(0)] public uint State;
        /// <summary></summary>
        [XmlAttribute("text"), DefaultValue("")] public string Text = "";
        /// <summary></summary>
        [XmlAttribute("regionhandle"), DefaultValue(0)] public ulong RegionHandle;
        /// <summary></summary>
        [XmlAttribute("flags"), DefaultValue(ObjectFlags.None)] public ObjectFlags Flags;
        /// <summary></summary>
        [XmlIgnore] public ObjectManager.PCode PCode = ObjectManager.PCode.Prim;
        /// <summary></summary>
        [XmlElement("id")] public LLUUID ID = LLUUID.Zero;
        /// <summary></summary>
        [XmlElement("groupid")] public LLUUID GroupID = LLUUID.Zero;
		/// <summary></summary>
        public LLVector3 Position = LLVector3.Zero;
		/// <summary></summary>
        public LLVector3 Scale = LLVector3.Zero;
        /// <summary></summary>
        public LLQuaternion Rotation = LLQuaternion.Identity;
        /// <summary></summary>
        public TextureEntry Textures = new TextureEntry();
        /// <summary></summary>
        public TextureAnimation TextureAnim = new TextureAnimation();
        /// <summary></summary>
        public PrimFlexibleData Flexible = new PrimFlexibleData();
        /// <summary></summary>
        public PrimLightData Light = new PrimLightData();
        /// <summary></summary>
        public ParticleSystem ParticleSys = new ParticleSystem();

        /// <summary>
        /// Default constructor, zeroes out or sets default prim parameters
        /// </summary>
        public PrimObject()
        {
        }
		
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathScale"></param>
        /// <returns></returns>
		public static byte PathScaleByte(float pathScale)
		{
			// Y = 100 + 100X
            int scale = (int)Math.Round(100.0f * pathScale);
			return (byte)(100 + scale);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathScale"></param>
        /// <returns></returns>
        public static float PathScaleFloat(byte pathScale)
        {
            // Y = -1 + 0.01X
            return (float)Math.Round((double)pathScale * 0.01d - 1.0d, 6);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathShear"></param>
        /// <returns></returns>
		public static byte PathShearByte(float pathShear)
		{
			// Y = 256 + 100X
            int shear = (int)Math.Round(100.0f * pathShear);
			shear += 256;
			return (byte)(shear % 256);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathShear"></param>
        /// <returns></returns>
        public static float PathShearFloat(byte pathShear)
        {
            if (pathShear == 0) return 0.0f;

            if (pathShear > 150)
            {
                // Negative value
                return ((float)pathShear - 256.0f) / 100.0f;
            }
            else
            {
                // Positive value
                return (float)pathShear / 100.0f;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileBegin"></param>
        /// <returns></returns>
		public static byte ProfileBeginByte(float profileBegin)
		{
			// Y = ceil (200X)
			return (byte)Math.Round(200.0f * profileBegin);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileBegin"></param>
        /// <returns></returns>
        public static float ProfileBeginFloat(byte profileBegin)
        {
            // Y = 0.005X
            return (float)Math.Round((double)profileBegin * 0.005d, 6);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileEnd"></param>
        /// <returns></returns>
		public static byte ProfileEndByte(float profileEnd)
		{
			// Y = 200 - 200X
            int end = (int)Math.Round(200.0d * (double)profileEnd);
			return (byte)(200 - end);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileEnd"></param>
        /// <returns></returns>
        public static float ProfileEndFloat(byte profileEnd)
        {
            // Y = 1 - 0.005X
            return (float)Math.Round(1.0d - ((double)profileEnd * 0.005d), 6);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathBegin"></param>
        /// <returns></returns>
		public static byte PathBeginByte(float pathBegin)
		{
			// Y = 100X
			return (byte)Convert.ToInt16(100.0f * pathBegin);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathBegin"></param>
        /// <returns></returns>
        public static float PathBeginFloat(byte pathBegin)
        {
            // Y = X / 100
            return (float)pathBegin / 100.0f;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathEnd"></param>
        /// <returns></returns>
		public static byte PathEndByte(float pathEnd)
		{
			// Y = 100 - 100X
            int end = (int)Math.Round(100.0f * pathEnd);
            return (byte)(100 - end);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathEnd"></param>
        /// <returns></returns>
        public static float PathEndFloat(byte pathEnd)
        {
            // Y = 1 - X / 100
            return 1.0f - (float)pathEnd / 100.0f;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathRadiusOffset"></param>
        /// <returns></returns>
		public static sbyte PathRadiusOffsetByte(float pathRadiusOffset)
		{
			// Y = 256 + 100X
			return (sbyte)PathShearByte(pathRadiusOffset);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathRadiusOffset"></param>
        /// <returns></returns>
        public static float PathRadiusOffsetFloat(sbyte pathRadiusOffset)
        {
            // Y = X / 100
            return (float)pathRadiusOffset / 100.0f;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathRevolutions"></param>
        /// <returns></returns>
		public static byte PathRevolutionsByte(float pathRevolutions)
		{
			// Y = 66.5X - 66
            int revolutions = (int)Math.Round(66.5d * (double)pathRevolutions);
			return (byte)(revolutions - 66);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathRevolutions"></param>
        /// <returns></returns>
        public static float PathRevolutionsFloat(byte pathRevolutions)
        {
            // Y = 1 + 0.015X
            return (float)Math.Round(1.0d + (double)pathRevolutions * 0.015d, 6);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathSkew"></param>
        /// <returns></returns>
		public static sbyte PathSkewByte(float pathSkew)
		{
            return PathTaperByte(pathSkew);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathSkew"></param>
        /// <returns></returns>
        public static float PathSkewFloat(sbyte pathSkew)
        {
            return PathTaperFloat(pathSkew);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathTaper"></param>
        /// <returns></returns>
        public static sbyte PathTaperByte(float pathTaper)
        {
            // Y = 256 + 100X
            return (sbyte)PathShearByte(pathTaper);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathTaper"></param>
        /// <returns></returns>
        public static float PathTaperFloat(sbyte pathTaper)
        {
            return (float)pathTaper / 100.0f;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public int SetExtraParamsFromBytes(byte[] data, int pos)
        {
            int i = pos;
            int totalLength = 1;

            if (data.Length == 0)
                return 0;

            byte extraParamCount = data[i++];

            for (int k = 0; k < extraParamCount; k++)
            {
                ExtraParamType type = (ExtraParamType)(data[i++] + (data[i++] << 8));
                uint paramLength = (uint)(data[i++] + (data[i++] << 8) +
                              (data[i++] << 16) + (data[i++] << 24));
                if (type == ExtraParamType.Flexible)
                {
                    Flexible = new PrimFlexibleData(data, i);
                }
                else if (type == ExtraParamType.Light)
                {
                    Light = new PrimLightData(data, i);
                }
                i += (int)paramLength;
                totalLength += (int)paramLength + 6;
            }

            return totalLength;
        }

        public override string ToString()
        {
            string output = "";

            output += (Name.Length != 0) ? Name : "Unnamed";
            output += ": " + ((Description.Length != 0) ? Description : "No description") + Environment.NewLine;
            output += "ID: " + ID + ", ";
            output += "GroupID: " + GroupID + ", ";
            output += "ParentID: " + ParentID + ", ";
            output += "LocalID: " + LocalID + ", ";
            output += "Flags: " + Flags + ", ";
            output += "State: " + State + ", ";
            output += "PCode: " + PCode + ", ";
            output += "Material: " + Material + ", ";

            return output;
        }

        public void ToXml(XmlWriter xmlWriter)
        {
            //XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            //ns.Add("", "");
            XmlSerializer serializer = new XmlSerializer(typeof(PrimObject));
            serializer.Serialize(xmlWriter, this);
        }

        public static PrimObject FromXml(XmlReader xmlReader)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(PrimObject));
            return (PrimObject)serializer.Deserialize(xmlReader);
        }
	}

    /// <summary>
    /// OMG Flexi
    /// </summary>
    [Serializable]
    public class PrimFlexibleData
    {
        /// <summary></summary>
        [XmlAttribute("softness"), DefaultValue(0)] public int Softness;
        /// <summary></summary>
        [XmlAttribute("gravity"), DefaultValue(0)] public float Gravity;
        /// <summary></summary>
        [XmlAttribute("drag"), DefaultValue(0)] public float Drag;
        /// <summary></summary>
        [XmlAttribute("wind"), DefaultValue(0)] public float Wind;
        /// <summary></summary>
        [XmlAttribute("tension"), DefaultValue(0)] public float Tension;
        /// <summary></summary>
        public LLVector3 Force = LLVector3.Zero;

        /// <summary>
        /// 
        /// </summary>
        public PrimFlexibleData()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="pos"></param>
        public PrimFlexibleData(byte[] data, int pos)
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
    /// Information on the light property associated with a prim
    /// </summary>
    [Serializable]
    public class PrimLightData
    {
        /// <summary></summary>
        [XmlAttribute("red"), DefaultValue(0)] public byte R;
        /// <summary></summary>
        [XmlAttribute("green"), DefaultValue(0)] public byte G;
        /// <summary></summary>
        [XmlAttribute("blue"), DefaultValue(0)] public byte B;
        /// <summary></summary>
        [XmlAttribute("intensity"), DefaultValue(0)] public float Intensity;
        /// <summary></summary>
        [XmlAttribute("radius"), DefaultValue(0)] public float Radius;
        /// <summary></summary>
        [XmlAttribute("falloff"), DefaultValue(0)] public float Falloff;

        /// <summary>
        /// 
        /// </summary>
        public PrimLightData()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="pos"></param>
        public PrimLightData(byte[] data, int pos)
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
}
