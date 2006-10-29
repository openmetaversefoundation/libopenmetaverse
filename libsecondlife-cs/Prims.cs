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
		public LLVector3 Position = new LLVector3();
        /// <summary></summary>
		public uint ProfileCurve = 0;
        /// <summary></summary>
		public float PathScaleX = 0;
        /// <summary></summary>
		public float PathScaleY = 0;
        /// <summary></summary>
		public LLUUID ID = new LLUUID();
        /// <summary></summary>
		public uint LocalID = 0;
        /// <summary></summary>
        public uint ParentID = 0;
        /// <summary></summary>
		public LLUUID GroupID = new LLUUID();
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
		public LLVector3 Scale = new LLVector3();
        /// <summary></summary>
		public int PathTwist = 0;
        /// <summary></summary>
        public TextureEntry Textures;
        /// <summary></summary>
		public uint ProfileHollow = 0;
        /// <summary></summary>
		public float PathRevolutions = 0;
        /// <summary></summary>
		public LLQuaternion Rotation = new LLQuaternion();
        /// <summary></summary>
		public uint State;
        /// <summary></summary>
        public string Text;

        private SecondLife Client;

        /// <summary>
        /// 
        /// </summary>
        public PrimObject(SecondLife client)
        {
            Client = client;
            Textures = new TextureEntry(Client);
        }
		
        /// <summary>
        /// 
        /// </summary>
        /// <param name="texture"></param>
		public PrimObject(SecondLife client, LLUUID texture)
		{
            Client = client;
            Textures = new TextureEntry(Client);
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
			return (byte)(100 + Convert.ToInt16(100.0F * pathScale));
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathScale"></param>
        /// <returns></returns>
        public static float PathScaleFloat(byte pathScale)
        {
            // Y = -1 + 0.01X
            return (float)pathScale * 0.01F - 1.0F;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathTwist"></param>
        /// <returns></returns>
		public static byte PathTwistByte(float pathTwist)
		{
			// Y = 256 + ceil (X / 1.8)
			ushort temp = Convert.ToUInt16(256 + Math.Ceiling(pathTwist / 1.8F));
			return (byte)(temp % 256);
		}

        /*/// <summary>
        /// 
        /// </summary>
        /// <param name="pathTwist"></param>
        /// <returns></returns>
        public static float PathTwistFloat(sbyte pathTwist)
        {
            // Y = 0.5556X
            return (float)pathTwist * 0.5556F;
        }*/

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathShear"></param>
        /// <returns></returns>
		public static byte PathShearByte(float pathShear)
		{
			// Y = 256 + 100X
			ushort temp = Convert.ToUInt16(100.0F * pathShear);
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
            // Y = (X - 256) / 100
            if (pathShear > 150)
            {
                return ((float)pathShear - 256.0F) / 100.0F;
            }
            else
            {
                return (float)pathShear / 100.0F;
            }        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileBegin"></param>
        /// <returns></returns>
		public static byte ProfileBeginByte(float profileBegin)
		{
			// Y = ceil (200X)
			return (byte)Convert.ToInt16(200.0F * profileBegin);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileBegin"></param>
        /// <returns></returns>
        public static float ProfileBeginFloat(byte profileBegin)
        {
            // Y = 0.005X
            return (float)profileBegin * 0.005F;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileEnd"></param>
        /// <returns></returns>
		public static byte ProfileEndByte(float profileEnd)
		{
			// Y = 200 - ceil (200X)
			return (byte)(200 - (200.0F * profileEnd));
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileEnd"></param>
        /// <returns></returns>
        public static float ProfileEndFloat(byte profileEnd)
        {
            // Y = 1 - 0.005X
            return 1.0F - (float)profileEnd * 0.005F;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathBegin"></param>
        /// <returns></returns>
		public static byte PathBeginByte(float pathBegin)
		{
			// Y = 100X
			return (byte)Convert.ToInt16(100.0F * pathBegin);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathBegin"></param>
        /// <returns></returns>
        public static float PathBeginFloat(byte pathBegin)
        {
            // Y = X / 100
            return (float)pathBegin / 100.0F;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathEnd"></param>
        /// <returns></returns>
		public static byte PathEndByte(float pathEnd)
		{
			// Y = 100 - 100X
			return (byte)(100 - Convert.ToInt16(100.0F * pathEnd));
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathEnd"></param>
        /// <returns></returns>
        public static float PathEndFloat(byte pathEnd)
        {
            // Y = 1 - X / 100
            return 1.0F - (float)pathEnd / 100;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathRadiusOffset"></param>
        /// <returns></returns>
		public static byte PathRadiusOffsetByte(float pathRadiusOffset)
		{
			// Y = 256 + 100X
			return PathShearByte(pathRadiusOffset);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathRadiusOffset"></param>
        /// <returns></returns>
        public static float PathRadiusOffsetFloat(sbyte pathRadiusOffset)
        {
            // Y = X / 100
            return (float)pathRadiusOffset / 100.0F;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathRevolutions"></param>
        /// <returns></returns>
		public static byte PathRevolutionsByte(float pathRevolutions)
		{
			// Y = ceil (66X) - 66
			return (byte)(Convert.ToInt16(Math.Ceiling(66.0F * pathRevolutions)) - 66);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathRevolutions"></param>
        /// <returns></returns>
        public static float PathRevolutionsFloat(byte pathRevolutions)
        {
            // Y = 1 + 0.015X
            return 1.0F + (float)pathRevolutions * 0.015F;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathSkew"></param>
        /// <returns></returns>
		public static byte PathSkewByte(float pathSkew)
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
		public static byte PathTaperByte(float pathTaper)
		{
			// Y = 256 + 100X
			return PathShearByte(pathTaper);
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
                return (float)(256 - pathTaper) * 0.01F;
            }
            else
            {
                return (float)pathTaper * 0.01F;
            }
        }
	}
}
