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
using System.Collections;

namespace libsecondlife.Packets
{
	public class Object
	{
		public static Packet ObjectAdd(ProtocolManager protocol, LLUUID senderID, LLUUID targetID, 
		                               LLVector3 rayStart, LLVector3 rayEnd, PrimObject prim, 
		                               byte[] textureEntry)
		{
			Hashtable fields = new Hashtable();
			Hashtable blocks = new Hashtable();
			
			fields["AddFlags"] = (uint)2; // TODO: ???
			fields["PathTwistBegin"] = (sbyte)prim.PathTwistBegin;
			fields["PathEnd"] = (byte)prim.PathEnd;
			fields["ProfileBegin"] = (byte)prim.ProfileBegin;
			fields["PathRadiusOffset"] = (sbyte)prim.PathRadiusOffset;
			fields["PathSkew"] = (sbyte)prim.PathSkew;
			fields["SenderID"] = senderID;
			fields["RayStart"] = rayStart;
			fields["ProfileCurve"] = (prim.ProfileCurve > 5) ? (byte)5 : (byte)prim.ProfileCurve;
			fields["PathScaleX"] = (byte)prim.PathScaleX;
			fields["PathScaleY"] = (byte)prim.PathScaleY;
			fields["GroupID"] = prim.GroupID;
			fields["Material"] = (byte)prim.Material;
			fields["NameValue"] = prim.Name;
			fields["PathShearX"] = (byte)prim.PathShearX;
			fields["PathShearY"] = (byte)prim.PathShearY;
			fields["PathTaperX"] = (sbyte)prim.PathTaperX;
			fields["PathTaperY"] = (sbyte)prim.PathTaperY;
			fields["RayEndIsIntersection"] = (byte)0;
			fields["RayEnd"] = rayEnd;
			fields["ProfileEnd"] = (byte)prim.ProfileEnd;
			fields["PathBegin"] = (byte)prim.PathBegin;
			fields["BypassRaycast"] = (byte)1;
			fields["PCode"] = (byte)9; // TODO: ???
			fields["PathCurve"] = (byte)prim.PathCurve;
			fields["Scale"] = prim.Scale;
			fields["State"] = (byte)prim.State;
			fields["PathTwist"] = (sbyte)prim.PathTwist;
			fields["TextureEntry"] = textureEntry;
			fields["ProfileHollow"] = (byte)prim.ProfileHollow;
			fields["PathRevolutions"] = (byte)prim.PathRevolutions;
			fields["Rotation"] = prim.Rotation;
			fields["RayTargetID"] = targetID;
			blocks[fields] = "ObjectData";
			
			return PacketBuilder.BuildPacket("ObjectAdd", protocol, blocks, Helpers.MSG_RELIABLE);
		}
	}
}
