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
	public class PrimObject
	{
		public int PathTwistBegin = 0;
		public uint PathEnd = 0;
		public float ProfileBegin = 0;
		public float PathRadiusOffset = 0;
		public float PathSkew = 0;
		public LLVector3 Position = new LLVector3();
		public uint ProfileCurve = 0;
		public float PathScaleX = 0;
		public float PathScaleY = 0;
		public LLUUID UUID = new LLUUID();
		public uint ID = 0;
		public LLUUID GroupID = new LLUUID();
		public uint Material = 0;
		public string Name = "";
		public string Description;
		public float PathShearX = 0;
		public float PathShearY = 0;
		public float PathTaperX = 0;
		public float PathTaperY = 0;
		public float ProfileEnd = 0;
		public uint PathBegin = 0;
		public uint PathCurve = 0;
		public LLVector3 Scale = new LLVector3();
		public int PathTwist = 0;
		public LLUUID Texture = new LLUUID(); // TODO: Add multi-texture support
		public uint ProfileHollow = 0;
		public float PathRevolutions = 0;
		public LLQuaternion Rotation = new LLQuaternion();
		public uint State;
		
		public PrimObject(LLUUID texture)
		{
			Texture = texture;
		}

		public static byte PathScaleByte(float pathScale)
		{
			// Y = 100 + 100X
			return (byte)(100 + Convert.ToInt16(100.0F * pathScale));
		}

		public static byte PathTwistByte(float pathTwist)
		{
			// Y = 256 + ceil (X / 1.8)
			ushort temp = Convert.ToUInt16(256 + Math.Ceiling(pathTwist / 1.8F));
			return (byte)(temp % 256);
		}

		public static byte PathShearByte(float pathShear)
		{
			// Y = 256 + 100X
			ushort temp = Convert.ToUInt16(100.0F * pathShear);
			temp += 256;
			return (byte)(temp % 256);
		}

		public static byte ProfileBeginByte(float profileBegin)
		{
			// Y = ceil (200X)
			return (byte)Convert.ToInt16(200.0F * profileBegin);
		}

		public static byte ProfileEndByte(float profileEnd)
		{
			// Y = 200 - ceil (200X)
			return (byte)(200 - (200.0F * profileEnd));
		}

		public static byte PathBeginByte(float pathBegin)
		{
			// Y = 100X
			return (byte)Convert.ToInt16(100.0F * pathBegin);
		}

		public static byte PathEndByte(float pathEnd)
		{
			// Y = 100 - 100X
			return (byte)(100 - Convert.ToInt16(100.0F * pathEnd));
		}

		public static byte PathRadiusOffsetByte(float pathRadiusOffset)
		{
			// Y = 256 + 100X
			return PathShearByte(pathRadiusOffset);
		}

		public static byte PathRevolutionsByte(float pathRevolutions)
		{
			// Y = ceil (66X) - 66
			return (byte)(Convert.ToInt16(Math.Ceiling(66.0F * pathRevolutions)) - 66);
		}

		public static byte PathSkewByte(float pathSkew)
		{
			// Y = 256 + 100X
			return PathShearByte(pathSkew);
		}

		public static byte PathTaperByte(float pathTaper)
		{
			// Y = 256 + 100X
			return PathShearByte(pathTaper);
		}
	}
}
