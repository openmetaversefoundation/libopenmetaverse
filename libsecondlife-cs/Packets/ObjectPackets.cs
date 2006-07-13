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
			fields["ProfileCurve"] = (byte)prim.ProfileCurve;
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
			fields["State"] = (byte)0; // TODO: ???
			fields["PathTwist"] = (sbyte)prim.PathTwist;
			fields["TextureEntry"] = textureEntry;
			fields["ProfileHollow"] = (byte)prim.ProfileHollow;
			fields["PathRevolutions"] = (byte)prim.PathRevolutions;
			fields["Rotation"] = prim.Rotation;
			fields["RayTargetID"] = targetID;
			blocks[fields] = "ObjectData";
			
			return PacketBuilder.BuildPacket("ObjectAdd", protocol, blocks, Helpers.MSG_RELIABLE);
		}
		
		//		public static Packet ObjectAddSimple(ProtocolManager protocol, PrimObject objectData, LLUUID senderID, 
		//			LLVector3 position, LLVector3 rayStart)
		//		{
		//			LLUUID woodTexture = new LLUUID("8955674724cb43ed920b47caed15465f");
		//			LLUUID rayTargetID = new LLUUID("0f5d10f1f0a38634e893b70e00000000");
		//			int length = 6 + 60 + 2 + objectData.NameValue.Length + 1 + 36 + 2 + 40 + 29;
		//			Packet packet = new Packet("ObjectAdd", protocol, length);
		//			int pos = 6;
		//
		//			// InventoryData appears 0 times
		//			packet.Data[pos] = 0;
		//			pos++;
		//
		//			// InventoryFile.Filename is of 1 length
		//			packet.Data[pos] = 1;
		//			pos++;
		//
		//			// InventoryFile.Filename is just a null terminator
		//			packet.Data[pos] = 0;
		//			pos++;
		//
		//			// U32 AddFlags = 2
		//			uint addFlags = 2;
		//			Array.Copy(BitConverter.GetBytes(addFlags), 0, packet.Data, pos, 4);
		//			pos += 4;
		//
		//			packet.Data[pos] = (byte)objectData.PathTwistBegin;
		//			pos++;
		//
		//			packet.Data[pos] = objectData.PathEnd;
		//			pos++;
		//
		//			packet.Data[pos] = objectData.ProfileBegin;
		//			pos++;
		//
		//			packet.Data[pos] = (byte)objectData.PathRadiusOffset;
		//			pos++;
		//
		//			packet.Data[pos] = (byte)objectData.PathSkew;
		//			pos++;
		//
		//			// SenderID
		//			Array.Copy(senderID.Data, 0, packet.Data, pos, 16);
		//			pos += 16;
		//
		//			// RayStart
		//			Array.Copy(rayStart.GetBytes(), 0, packet.Data, pos, 12);
		//			pos += 12;
		//
		//			packet.Data[pos] = objectData.ProfileCurve;
		//			pos++;
		//
		//			packet.Data[pos] = objectData.PathScaleX;
		//			pos++;
		//
		//			packet.Data[pos] = objectData.PathScaleY;
		//			pos++;
		//
		//			// Set GroupID to zero
		//			pos += 16;
		//
		//			packet.Data[pos] = objectData.Material;
		//			pos++;
		//
		//			if (objectData.NameValue.Length != 0)
		//			{
		//				// NameValue, begins with two bytes describing the size
		//				Array.Copy(BitConverter.GetBytes((ushort)(objectData.NameValue.Length + 1)), 0, packet.Data, pos, 2);
		//				pos += 2;
		//				System.Text.Encoding.UTF8.GetBytes(objectData.NameValue, 0, objectData.NameValue.Length, packet.Data, pos);
		//				// Jump an extra spot for the null terminator
		//				pos += objectData.NameValue.Length + 1;
		//			}
		//			else
		//			{
		//				// Set the two size bytes to zero and increment
		//				pos += 2;
		//			}
		//
		//			packet.Data[pos] = objectData.PathShearX;
		//			pos++;
		//
		//			packet.Data[pos] = objectData.PathShearY;
		//			pos++;
		//
		//			packet.Data[pos] = (byte)objectData.PathTaperX;
		//			pos++;
		//
		//			packet.Data[pos] = (byte)objectData.PathTaperY;
		//			pos++;
		//
		//			// RayEndIsIntersection
		//			packet.Data[pos] = 0;
		//			pos++;
		//
		//			// RayEnd is the position to place the object
		//			Array.Copy(position.GetBytes(), 0, packet.Data, pos, 12);
		//			pos += 12;
		//
		//			packet.Data[pos] = objectData.ProfileEnd;
		//			pos++;
		//
		//			packet.Data[pos] = objectData.PathBegin;
		//			pos++;
		//
		//			// BypassRaycast is 0
		//			packet.Data[pos] = 0;
		//			pos++;
		//
		//			// PCode? set to 9
		//			packet.Data[pos] = 9;
		//			pos++;
		//
		//			packet.Data[pos] = objectData.PathCurve;
		//			pos++;
		//
		//			// Scale
		//			Array.Copy(objectData.Scale.GetBytes(), 0, packet.Data, pos, 12);
		//			pos += 12;
		//
		//			// State? is 0
		//			packet.Data[pos] = 0;
		//			pos++;
		//
		//			packet.Data[pos] = (byte)objectData.PathTwist;
		//			pos++;
		//
		//			// TextureEntry, starts with two bytes describing the size
		//			Array.Copy(BitConverter.GetBytes((ushort)40), 0, packet.Data, pos, 2);
		//			pos += 2;
		//			Array.Copy(woodTexture.Data, 0, packet.Data, pos, 16);
		//			pos += 16;
		//			// Fill in the rest of TextureEntry
		//			pos += 19;
		//			packet.Data[pos] = 0xe0;
		//			pos += 5;
		//
		//			packet.Data[pos] = objectData.ProfileHollow;
		//			pos++;
		//
		//			packet.Data[pos] = objectData.PathRevolutions;
		//			pos++;
		//
		//			// Rotation
		//			Array.Copy(objectData.Rotation.GetBytes(), 0, packet.Data, pos, 16);
		//			pos += 16;
		//
		//			// RayTargetID
		//			Array.Copy(rayTargetID.Data, 0, packet.Data, pos, 12);
		//			pos += 12;
		//			
		//			// Set the packet flags
		//			packet.Data[0] = /*Helpers.MSG_ZEROCODED +*/ Helpers.MSG_RELIABLE;
		//
		//			return packet;
		//		}
	}
}
