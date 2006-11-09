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

namespace libsecondlife
{
    /// <summary>
    /// 
    /// </summary>
	public class PrimObject
	{
        /// <summary></summary>
		public int PathTwistBegin = 0;
        /// <summary></summary>
		public float PathEnd = 0;
        /// <summary></summary>
		public float ProfileBegin = 0;
        /// <summary></summary>
		public float PathRadiusOffset = 0;
        /// <summary></summary>
		public float PathSkew = 0;
        /// <summary></summary>
        public LLVector3 Position = LLVector3.Zero;
        /// <summary></summary>
		public uint ProfileCurve = 0;
        /// <summary></summary>
		public float PathScaleX = 0;
        /// <summary></summary>
		public float PathScaleY = 0;
        /// <summary></summary>
        public LLUUID ID = LLUUID.Zero;
        /// <summary></summary>
		public uint LocalID = 0;
        /// <summary></summary>
        public uint ParentID = 0;
        /// <summary></summary>
        public LLUUID GroupID = LLUUID.Zero;
        /// <summary></summary>
		public uint Material = 0;
        /// <summary></summary>
		public string Name = "";
        /// <summary></summary>
		public string Description;
        /// <summary></summary>
		public float PathShearX = 0;
        /// <summary></summary>
		public float PathShearY = 0;
        /// <summary></summary>
		public float PathTaperX = 0;
        /// <summary></summary>
		public float PathTaperY = 0;
        /// <summary></summary>
		public float ProfileEnd = 0;
        /// <summary></summary>
		public float PathBegin = 0;
        /// <summary></summary>
		public uint PathCurve = 0;
        /// <summary></summary>
        public LLVector3 Scale = LLVector3.Zero;
        /// <summary></summary>
		public int PathTwist = 0;
        /// <summary></summary>
        public ObjectManager.PCode PCode;
        /// <summary></summary>
        public TextureEntry Textures;
        /// <summary></summary>
        public TextureAnimation TextureAnim;
        /// <summary></summary>
		public uint ProfileHollow = 0;
        /// <summary></summary>
		public float PathRevolutions = 0;
        /// <summary></summary>
        public LLQuaternion Rotation = LLQuaternion.Identity;
        /// <summary></summary>
		public uint State;
        /// <summary></summary>
        public string Text;
        /// <summary></summary>
        public PrimFlexibleData Flexible;
        /// <summary></summary>
        public PrimLightData Light;
        /// <summary></summary>
        public ParticleSystem ParticleSys;

        private SecondLife Client;

        /// <summary>
        /// 
        /// </summary>
        public PrimObject(SecondLife client)
        {
            Client = client;
            PCode = ObjectManager.PCode.Prim;
            Textures = new TextureEntry();
        }
		
        /// <summary>
        /// 
        /// </summary>
        /// <param name="texture"></param>
		public PrimObject(SecondLife client, LLUUID texture)
		{
            Client = client;
            PCode = ObjectManager.PCode.Prim;
            Textures = new TextureEntry();
            Textures.DefaultTexture.TextureID = texture;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathScale"></param>
        /// <returns></returns>
		public static byte PathScaleByte(float pathScale)
		{
			// Y = 100 + 100X
			return (byte)(100 + Convert.ToInt16(100.0f * pathScale));
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathScale"></param>
        /// <returns></returns>
        public static float PathScaleFloat(byte pathScale)
        {
            // Y = -1 + 0.01X
            return (float)Math.Round((double)pathScale * 0.01d - 1.0d, 4);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathTwist"></param>
        /// <returns></returns>
		public static sbyte PathTwistByte(float pathTwist)
		{
			// Y = 256 + ceil (X / 1.8)
			ushort temp = Convert.ToUInt16(256 + Math.Ceiling(pathTwist / 1.8f));
			return (sbyte)(temp % 256);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathShear"></param>
        /// <returns></returns>
		public static byte PathShearByte(float pathShear)
		{
			// Y = 256 + 100X
			ushort temp = Convert.ToUInt16(100.0f * pathShear);
			temp += 256;
			return (byte)(temp % 256);
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
			return (byte)Convert.ToInt16(200.0f * profileBegin);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileBegin"></param>
        /// <returns></returns>
        public static float ProfileBeginFloat(byte profileBegin)
        {
            // Y = 0.005X
            return (float)Math.Round((double)profileBegin * 0.005d, 4);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileEnd"></param>
        /// <returns></returns>
		public static byte ProfileEndByte(float profileEnd)
		{
			// Y = 200 - ceil (200X)
			return (byte)(200 - (200.0f * profileEnd));
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileEnd"></param>
        /// <returns></returns>
        public static float ProfileEndFloat(byte profileEnd)
        {
            // Y = 1 - 0.005X
            return (float)Math.Round(1.0d - (double)profileEnd * 0.005d);
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
			return (byte)(100 - Convert.ToInt16(100.0f * pathEnd));
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathEnd"></param>
        /// <returns></returns>
        public static float PathEndFloat(byte pathEnd)
        {
            // Y = 1 - X / 100
            return (float)Math.Round(1.0d - (double)pathEnd / 100.0d);
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
			// Y = ceil (66X) - 66
			return (byte)(Convert.ToInt16(Math.Ceiling(66.0f * pathRevolutions)) - 66);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathRevolutions"></param>
        /// <returns></returns>
        public static float PathRevolutionsFloat(byte pathRevolutions)
        {
            // Y = 1 + 0.015X
            return (float)Math.Round(1.0d + (double)pathRevolutions * 0.015d);
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
        public static float PathSkewFloat(byte pathSkew)
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
        public static float PathTaperFloat(byte pathTaper)
        {
            if (pathTaper > 100)
            {
                return (float)Math.Round((double)(256 - pathTaper) * 0.01d);
            }
            else
            {
                return (float)Math.Round((double)pathTaper * 0.01d);
            }
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
	}

    /// <summary>
    /// 
    /// </summary>
    public enum ExtraParamType : ushort
    {
        Flexible = 0x10,
        Light = 0x20
    }

    /// <summary>
    /// 
    /// </summary>
    public class PrimFlexibleData
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
        public PrimFlexibleData(byte[] data, int pos)
        {
            FromBytes(data, pos);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
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
    /// 
    /// </summary>
    public class PrimLightData
    {
        /// <summary></summary>
        public byte R, G, B;
        /// <summary></summary>
        public float Intensity;
        /// <summary></summary>
        public float Radius;
        /// <summary></summary>
        public float Falloff;

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
        public byte[] ToBytes()
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