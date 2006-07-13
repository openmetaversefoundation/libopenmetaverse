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
		public uint ProfileBegin = 0;
		public int PathRadiusOffset = 0;
		public int PathSkew = 0;
		public LLVector3 RayStart = new LLVector3();
		public int ProfileCurve = 0;
		public int PathScaleX = 0;
		public int PathScaleY = 0;
		public LLUUID GroupID = new LLUUID();
		public uint Material = 0;
		public string Name = "";
		public string Description;
		public uint PathShearX = 0;
		public uint PathShearY = 0;
		public int PathTaperX = 0;
		public int PathTaperY = 0;
		public uint ProfileEnd = 0;
		public uint PathBegin = 0;
		public uint PathCurve = 0;
		public LLVector3 Scale = new LLVector3();
		public int PathTwist = 0;
		public LLUUID Texture = null; // TODO: Add multi-texture support
		public uint ProfileHollow = 0;
		public uint PathRevolutions = 0;
		public LLQuaternion Rotation = new LLQuaternion();
		
		public PrimObject(LLUUID texture)
		{
			Texture = texture;
		}
	}
}
