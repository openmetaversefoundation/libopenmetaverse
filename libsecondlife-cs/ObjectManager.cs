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
using System.Collections.Generic;
using libsecondlife.Packets;

namespace libsecondlife
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="simulator"></param>
    /// <param name="prim"></param>
    /// <param name="regionHandle"></param>
    /// <param name="timeDilation"></param>
    public delegate void NewPrimCallback(Simulator simulator, PrimObject prim, ulong regionHandle, ushort timeDilation);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="simulator"></param>
    /// <param name="avatar"></param>
    /// <param name="regionHandle"></param>
    /// <param name="timeDilation"></param>
    public delegate void NewAvatarCallback(Simulator simulator, Avatar avatar, ulong regionHandle, ushort timeDilation);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="simulator"></param>
    /// <param name="prim"></param>
    /// <param name="regionHandle"></param>
    /// <param name="timeDilation"></param>
    public delegate void PrimMovedCallback(Simulator simulator, PrimUpdate prim, ulong regionHandle, ushort timeDilation);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="simulator"></param>
    /// <param name="avatar"></param>
    /// <param name="regionHandle"></param>
    /// <param name="timeDilation"></param>
    public delegate void AvatarMovedCallback(Simulator simulator, AvatarUpdate avatar, ulong regionHandle, ushort timeDilation);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="simulator"></param>
    /// <param name="objectID"></param>
    public delegate void KillObjectCallback(Simulator simulator, uint objectID);

    public enum PCode
    {
        Prim = 9
    }

    /// <summary>
    /// Contains all of the variables sent in an object update packet for a 
    /// prim object. Used to track position and movement of prims.
    /// </summary>
    public struct PrimUpdate
    {
        /// <summary></summary>
        public uint LocalID;
        /// <summary></summary>
        public byte State;
        /// <summary></summary>
        public LLVector3 Position;
        /// <summary></summary>
        public LLVector3 Velocity;
        /// <summary></summary>
        public LLVector3 Acceleration;
        /// <summary></summary>
        public LLQuaternion Rotation;
        /// <summary></summary>
        public LLVector3 RotationVelocity;
    }

    /// <summary>
    /// Contains all of the variables sent in an object update packet for an 
    /// avatar. Used to track position and movement of avatars.
    /// </summary>
    public struct AvatarUpdate
    {
        /// <summary></summary>
        public uint LocalID;
        /// <summary></summary>
        public byte State;
        /// <summary></summary>
        public LLVector4 CollisionPlane;
        /// <summary></summary>
        public LLVector3 Position;
        /// <summary></summary>
        public LLVector3 Velocity;
        /// <summary></summary>
        public LLVector3 Acceleration;
        /// <summary></summary>
        public LLQuaternion Rotation;
        /// <summary></summary>
        public LLVector3 RotationVelocity;
    }

	/// <summary>
	/// Handles all network traffic related to prims and avatar positions and 
    /// movement.
	/// </summary>
	public class ObjectManager
    {
        /// <summary>
        /// This event will be raised for every ObjectUpdate block that 
        /// contains a new prim.
        /// <remarks>Depending on the circumstances a client could 
        /// receive two or more of these events for the same object, if you 
        /// or the object left the current sim and returned for example. Client
        /// applications are responsible for tracking and storing objects.
        /// </remarks>
        /// </summary>
        public event NewPrimCallback OnNewPrim;
        /// <summary>
        /// This event will be raised for every ObjectUpdate block that 
        /// contains a new avatar.
        /// <remarks>Depending on the circumstances a client 
        /// could receive two or more of these events for the same avatar, if 
        /// you or the other avatar left the current sim and returned for 
        /// example. Client applications are responsible for tracking and 
        /// storing objects.</remarks>
        /// </summary>
        public event NewAvatarCallback OnNewAvatar;
        /// <summary>
        /// This event will be raised when a prim movement packet is received, 
        /// containing the updated position, rotation, and movement-related 
        /// vectors.
        /// </summary>
        public event PrimMovedCallback OnPrimMoved;
        /// <summary>
        /// This event will be raised when an avatar movement packet is 
        /// received, containing the updated position, rotation, and 
        /// movement-related vectors.
        /// </summary>
        public event AvatarMovedCallback OnAvatarMoved;
        /// <summary>
        /// This event will be raised when an object is removed from a 
        /// simulator.
        /// </summary>
        public event KillObjectCallback OnObjectKilled;

        private SecondLife Client;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        public ObjectManager(SecondLife client)
        {
            Client = client;

            Client.Network.RegisterCallback(PacketType.ObjectUpdate, new PacketCallback(UpdateHandler));
            Client.Network.RegisterCallback(PacketType.ImprovedTerseObjectUpdate, new PacketCallback(TerseUpdateHandler));
            Client.Network.RegisterCallback(PacketType.ObjectUpdateCompressed, new PacketCallback(CompressedUpdateHandler));
            Client.Network.RegisterCallback(PacketType.ObjectUpdateCached, new PacketCallback(CachedUpdateHandler));
            Client.Network.RegisterCallback(PacketType.KillObject, new PacketCallback(KillObjectHandler));
        }

        public void RequestObject(Simulator simulator, uint localID)
        {
            RequestMultipleObjectsPacket request = new RequestMultipleObjectsPacket();
            request.AgentData.AgentID = Client.Network.AgentID;
            request.AgentData.SessionID = Client.Network.SessionID;
            request.ObjectData = new RequestMultipleObjectsPacket.ObjectDataBlock[1];
            request.ObjectData[0] = new RequestMultipleObjectsPacket.ObjectDataBlock();
            request.ObjectData[0].ID = localID;
            request.ObjectData[0].CacheMissType = 0;

            Client.Network.SendPacket(request, simulator);
        }

        public void RequestObjects(Simulator simulator, List<uint> localIDs)
        {
            int i = 0;

            RequestMultipleObjectsPacket request = new RequestMultipleObjectsPacket();
            request.AgentData.AgentID = Client.Network.AgentID;
            request.AgentData.SessionID = Client.Network.SessionID;
            request.ObjectData = new RequestMultipleObjectsPacket.ObjectDataBlock[localIDs.Count];

            foreach (uint localID in localIDs)
            {
                request.ObjectData[i] = new RequestMultipleObjectsPacket.ObjectDataBlock();
                request.ObjectData[i].ID = localID;
                request.ObjectData[i].CacheMissType = 0;

                i++;
            }

            Client.Network.SendPacket(request, simulator);
        }

        private void ParseAvName(string name, ref string firstName, ref string lastName, ref string groupName)
        {
            string[] lines = name.Split('\n');

            foreach (string line in lines)
            {
                if (line.Substring(0, 19) == "Title STRING RW SV ")
                {
                    groupName = line.Substring(19);
                }
                else if (line.Substring(0, 23) == "FirstName STRING RW SV ")
                {
                    firstName = line.Substring(23);
                }
                else if (line.Substring(0, 22) == "LastName STRING RW SV ")
                {
                    lastName = line.Substring(22);
                }
                else
                {
                    Client.Log("Unhandled line in an avatar name: " + line, Helpers.LogLevel.Warning);
                }
            }
        }

        private void UpdateHandler(Packet packet, Simulator simulator)
        {
            ObjectUpdatePacket update = (ObjectUpdatePacket)packet;

            foreach (ObjectUpdatePacket.ObjectDataBlock block in update.ObjectData)
            {
                if (block.ObjectData.Length == 60)
                {
                    if (block.PCode == (byte)PCode.Prim)
                    {
                        // New prim spotted
                        PrimObject prim = new PrimObject(Client);

                        prim.Position = new LLVector3(block.ObjectData, 0);
                        prim.Rotation = new LLQuaternion(block.ObjectData, 36, true);

                        // TODO: Parse the rest of the ObjectData byte array fields

                        prim.LocalID = block.ID;
                        prim.State = block.State;
                        prim.ID = block.FullID;
                        prim.ParentID = block.ParentID;
                        //block.OwnerID Sound-related
                        prim.Material = block.Material;
                        prim.PathCurve = block.PathCurve;
                        prim.ProfileCurve = block.ProfileCurve;
                        prim.PathBegin = PrimObject.PathBeginFloat(block.PathBegin);
                        prim.PathEnd = PrimObject.PathEndFloat(block.PathEnd);
                        prim.PathScaleX = PrimObject.PathScaleFloat(block.PathScaleX);
                        prim.PathScaleY = PrimObject.PathScaleFloat(block.PathScaleY);
                        prim.PathShearX = PrimObject.PathShearFloat(block.PathShearX);
                        prim.PathShearY = PrimObject.PathShearFloat(block.PathShearY);
                        prim.PathTwist = block.PathTwist; //PrimObject.PathTwistFloat(block.PathTwist);
                        prim.PathTwistBegin = block.PathTwistBegin; //PrimObject.PathTwistFloat(block.PathTwistBegin);
                        prim.PathRadiusOffset = PrimObject.PathRadiusOffsetFloat(block.PathRadiusOffset);
                        prim.PathTaperX = PrimObject.PathTaperFloat((byte)block.PathTaperX);
                        prim.PathTaperY = PrimObject.PathTaperFloat((byte)block.PathTaperY);
                        prim.PathRevolutions = PrimObject.PathRevolutionsFloat(block.PathRevolutions);
                        prim.PathSkew = PrimObject.PathSkewFloat((byte)block.PathSkew);
                        prim.ProfileBegin = PrimObject.ProfileBeginFloat(block.ProfileBegin);
                        prim.ProfileEnd = PrimObject.ProfileEndFloat(block.ProfileEnd);
                        prim.ProfileHollow = block.ProfileHollow;
                        prim.Name = Helpers.FieldToString(block.NameValue);
                        //block.Data ?
                        //block.Text Hovering text
                        //block.TextColor LLColor4U of the hovering text
                        //block.MediaURL Quicktime stream
                        prim.Textures = new TextureEntry(block.TextureEntry, 0, block.TextureEntry.Length);
                        prim.TextureAnim = new TextureAnimation(block.TextureAnim, 0);
                        //block.JointType ?
                        //block.JointPivot ?
                        //block.JointAxisOrAnchor ?
                        prim.ParticleSys = new ParticleSystem(block.PSBlock, 0);
                        prim.SetExtraParamsFromBytes(block.ExtraParams, 0);
                        prim.Scale = block.Scale;
                        //block.Flags ?
                        //block.UpdateFlags ?
                        //block.ClickAction ?
                        //block.Gain Sound-related
                        //block.Sound Sound-related
                        //block.Radius Sound-related

                        if (OnNewPrim != null)
                        {
                            OnNewPrim(simulator, prim, update.RegionData.RegionHandle, update.RegionData.TimeDilation);
                        }
                    }
                    else
                    {
                        // FIXME: This is probably a tree/bush/grasspatch, which need their own
                        // separate callback IMO -jph
                        Client.Log("Got an ObjectUpdate block with an unhandled PCode " + 
                            block.PCode, Helpers.LogLevel.Debug);
                    }
                }
                else if (block.ObjectData.Length == 76)
                {
                    Avatar avatar = new Avatar();

                    string FirstName = "";
                    string LastName = "";
                    string GroupName = "";

                    //avatar.CollisionPlane = new LLQuaternion(block.ObjectData, 0);
                    avatar.Position = new LLVector3(block.ObjectData, 16);
                    avatar.Rotation = new LLQuaternion(block.ObjectData, 52, true);

                    // TODO: Parse the rest of the ObjectData byte array fields

                    ParseAvName(Helpers.FieldToString(block.NameValue), ref FirstName, ref LastName, ref GroupName);

                    avatar.ID = block.FullID;
                    avatar.LocalID = block.ID;
                    avatar.Name = FirstName + " " + LastName;
                    avatar.GroupName = GroupName;
                    avatar.Online = true;
                    avatar.CurrentRegion = simulator.Region;

                    avatar.Textures = new TextureEntry(block.TextureEntry, 0, block.TextureEntry.Length);

                    if (FirstName == Client.Self.FirstName && LastName == Client.Self.LastName)
                    {
                        // Update our avatar
                        Client.Self.LocalID = avatar.LocalID;
                        Client.Self.Position = avatar.Position;
                        Client.Self.Rotation = avatar.Rotation;
                    }
                    else
                    {
                        Client.Avatars.AddAvatar(avatar);

                        if (OnNewAvatar != null)
                        {
                            OnNewAvatar(simulator, avatar, update.RegionData.RegionHandle, update.RegionData.TimeDilation);
                        }
                    }
                }
                else
                {
                    // Unknown
                    Client.Log("Unhandled ObjectData.ObjectData length:\n" + block.ObjectData.Length, 
                        Helpers.LogLevel.Warning);
                }
            }
        }

        private void TerseUpdateHandler(Packet packet, Simulator simulator)
        {
            float x, y, z, w;
            uint localid;
            LLVector4 CollisionPlane = null;
            LLVector3 Position;
            LLVector3 Velocity;
            LLVector3 Acceleration;
            LLQuaternion Rotation;
            LLVector3 RotationVelocity;

            ImprovedTerseObjectUpdatePacket update = (ImprovedTerseObjectUpdatePacket)packet;

            foreach (ImprovedTerseObjectUpdatePacket.ObjectDataBlock block in update.ObjectData)
            {
                int i = 0;
                bool avatar;

                localid = (uint)(block.Data[i++] + (block.Data[i++] << 8) + 
                    (block.Data[i++] << 16) + (block.Data[i++] << 24));

                byte state = block.Data[i++];

                avatar = Convert.ToBoolean(block.Data[i++]);

                if (avatar)
                {
                    CollisionPlane = new LLVector4(block.Data, i);
                    i += 16;
                }

                // Position
                Position = new LLVector3(block.Data, i);
                i += 12;
                // Velocity
                x = Dequantize(block.Data, i, -128.0F, 128.0F);
                i += 2;
                y = Dequantize(block.Data, i, -128.0F, 128.0F);
                i += 2;
                z = Dequantize(block.Data, i, -128.0F, 128.0F);
                i += 2;
                Velocity = new LLVector3(x, y, z);
                // Acceleration
                x = Dequantize(block.Data, i, -64.0F, 64.0F);
                i += 2;
                y = Dequantize(block.Data, i, -64.0F, 64.0F);
                i += 2;
                z = Dequantize(block.Data, i, -64.0F, 64.0F);
                i += 2;
                Acceleration = new LLVector3(x, y, z);
                // Rotation
                x = Dequantize(block.Data, i, -1.0F, 1.0F);
                i += 2;
                y = Dequantize(block.Data, i, -1.0F, 1.0F);
                i += 2;
                z = Dequantize(block.Data, i, -1.0F, 1.0F);
                i += 2;
                w = Dequantize(block.Data, i, -1.0F, 1.0F);
                i += 2;
                Rotation = new LLQuaternion(x, y, z, w);
                // Rotation velocity
                x = Dequantize(block.Data, i, -64.0F, 64.0F);
                i += 2;
                y = Dequantize(block.Data, i, -64.0F, 64.0F);
                i += 2;
                z = Dequantize(block.Data, i, -64.0F, 64.0F);
                i += 2;
                RotationVelocity = new LLVector3(x, y, z);

                if (avatar)
                {
                    if (localid == Client.Self.LocalID)
                    {
                        Client.Self.Position = Position;
                        Client.Self.Rotation = Rotation;
                    }

                    AvatarUpdate avupdate = new AvatarUpdate();
                    avupdate.LocalID = localid;
                    avupdate.State = state;
                    avupdate.Position = Position;
                    avupdate.CollisionPlane = CollisionPlane;
                    avupdate.Velocity = Velocity;
                    avupdate.Acceleration = Acceleration;
                    avupdate.Rotation = Rotation;
                    avupdate.RotationVelocity = RotationVelocity;

                    if (OnAvatarMoved != null)
                    {
                        OnAvatarMoved(simulator, avupdate, update.RegionData.RegionHandle, update.RegionData.TimeDilation);
                    }
                }
                else
                {
                    // TODO: Is there an easy way to distinguish prims from trees in this packet,
                    // or would the client have to do it's own lookup to determine whether it's a
                    // prim or a tree? If the latter, we should rename this update to something 
                    // less prim specific

                    PrimUpdate primupdate = new PrimUpdate();
                    primupdate.LocalID = localid;
                    primupdate.State = state;
                    primupdate.Position = Position;
                    primupdate.Velocity = Velocity;
                    primupdate.Acceleration = Acceleration;
                    primupdate.Rotation = Rotation;
                    primupdate.RotationVelocity = RotationVelocity;

                    if (OnPrimMoved != null)
                    {
                        OnPrimMoved(simulator, primupdate, update.RegionData.RegionHandle, update.RegionData.TimeDilation);
                    }
                }
            }
        }

        private void CompressedUpdateHandler(Packet packet, Simulator simulator)
        {
            ObjectUpdateCompressedPacket update = (ObjectUpdateCompressedPacket)packet;
            PrimObject prim;

            foreach (ObjectUpdateCompressedPacket.ObjectDataBlock block in update.ObjectData)
            {
                int i = 0;
                prim = new PrimObject(Client);

                try
                {
                    prim.ID = new LLUUID(block.Data, 0);
                    i += 16;
                    prim.LocalID = (uint)(block.Data[i++] + (block.Data[i++] << 8) +
                        (block.Data[i++] << 16) + (block.Data[i++] << 24));

                    byte pcode = block.Data[i++];

                    if (pcode != (byte)PCode.Prim)
                    {
                        Client.Log("Got an ObjectUpdateCompressed block with unhandled PCode " +
                            pcode, Helpers.LogLevel.Debug);
                        continue;
                    }

                    prim.State = (uint)block.Data[i++];
                    i += 4; //CRC
                    prim.Material = (uint)block.Data[i++];
                    i++; //ClickAction

                    prim.Scale = new LLVector3(block.Data, i);
                    i += 12;
                    prim.Position = new LLVector3(block.Data, i);
                    i += 12;
                    prim.Rotation = new LLQuaternion(block.Data, i, true);
                    i += 12;

                    uint flags = (uint)(block.Data[i++] + (block.Data[i++] << 8) +
                        (block.Data[i++] << 16) + (block.Data[i++] << 24));

                    if ((flags & 0x02) != 0)
                    {
                        //Unknown 2 bytes
                        i += 2;

                        if (OnNewPrim != null)
                        {
                            OnNewPrim(simulator, prim, update.RegionData.RegionHandle, update.RegionData.TimeDilation);
                        }
                        continue;
                    }

                    if ((flags & 0x20) != 0)
                    {
                        prim.ParentID = (uint)(block.Data[i++] + (block.Data[i++] << 8) +
                        (block.Data[i++] << 16) + (block.Data[i++] << 24));
                    }
                    else
                    {
                        prim.ParentID = 0;
                    }

                    //Unknown field
                    if ((flags & 0x80) != 0)
                    {
                        i += 12;
                    }

                    if ((flags & 0x08) != 0)
                    {
                        prim.ParticleSys = new ParticleSystem(block.Data, i);
                        i += 86;
                    }

                    if ((flags & 0x04) != 0)
                    {
                        string text = "";
                        while (block.Data[i] != 0)
                        {
                            text += (char)block.Data[i];
                            i++;
                        }
                        prim.Text = text;
                        i++;

                        //Unknown field, possibly text color.
                        i += 4;
                    }

                    i += prim.SetExtraParamsFromBytes(block.Data, i);

                    //Indicates that this is an attachment?
                    if ((flags & 0x100) != 0)
                    {
                        //A string
                        //Example: "AttachItemID STRING RW SV fa9a5ab8-1bad-b449-9873-cf5b803e664e"
                        while (block.Data[i] != 0)
                        {
                            i++;
                        }
                        i++;
                    }

                    prim.PathCurve = (uint)block.Data[i++];
                    prim.PathBegin = PrimObject.PathBeginFloat(block.Data[i++]);
                    prim.PathEnd = PrimObject.PathEndFloat(block.Data[i++]);
                    prim.PathScaleX = PrimObject.PathScaleFloat(block.Data[i++]);
                    prim.PathScaleY = PrimObject.PathScaleFloat(block.Data[i++]);
                    prim.PathShearX = PrimObject.PathShearFloat(block.Data[i++]);
                    prim.PathShearY = PrimObject.PathShearFloat(block.Data[i++]);
                    prim.PathTwist = (int)block.Data[i++];
                    prim.PathTwistBegin = (int)block.Data[i++];
                    prim.PathRadiusOffset = PrimObject.PathRadiusOffsetFloat((sbyte)block.Data[i++]);
                    prim.PathTaperX = PrimObject.PathTaperFloat(block.Data[i++]);
                    prim.PathTaperY = PrimObject.PathTaperFloat(block.Data[i++]);
                    prim.PathRevolutions = PrimObject.PathRevolutionsFloat(block.Data[i++]);
                    prim.PathSkew = PrimObject.PathSkewFloat(block.Data[i++]);

                    prim.ProfileCurve = (uint)block.Data[i++];
                    prim.ProfileBegin = PrimObject.ProfileBeginFloat(block.Data[i++]);
                    prim.ProfileEnd = PrimObject.ProfileEndFloat(block.Data[i++]);
                    prim.ProfileHollow = (uint)block.Data[i++];

                    int textureEntryLength = (int)(block.Data[i++] + (block.Data[i++] << 8) +
                        (block.Data[i++] << 16) + (block.Data[i++] << 24));

                    prim.Textures = new TextureEntry(block.Data, i, textureEntryLength);

                    i += textureEntryLength;

                    if (i < block.Data.Length)
                    {
                        int textureAnimLength = (int)(block.Data[i++] + (block.Data[i++] << 8) +
                            (block.Data[i++] << 16) + (block.Data[i++] << 24));

                        prim.TextureAnim = new TextureAnimation(block.Data, i);
                    }
                }
                catch (System.IndexOutOfRangeException e)
                {
                    Client.Log("Had a problem decoding an ObjectUpdateCompressed packet: " + 
                        e.ToString(), Helpers.LogLevel.Warning);
                    Client.Log(block.ToString(), Helpers.LogLevel.Warning);
                }


                if (OnNewPrim != null)
                {
                    OnNewPrim(simulator, prim, update.RegionData.RegionHandle, update.RegionData.TimeDilation);
                }
            }
        }

        private void CachedUpdateHandler(Packet packet, Simulator simulator)
        {
            List<uint> ids = new List<uint>();
            ObjectUpdateCachedPacket update = (ObjectUpdateCachedPacket)packet;

            // Assume clients aren't caching objects for now, so request updates for all of these objects
            foreach (ObjectUpdateCachedPacket.ObjectDataBlock block in update.ObjectData)
            {
                ids.Add(block.ID);
            }

            RequestObjects(simulator, ids);
        }

        private void KillObjectHandler(Packet packet, Simulator simulator)
        {
            if (OnObjectKilled != null)
            {
                foreach (KillObjectPacket.ObjectDataBlock block in ((KillObjectPacket)packet).ObjectData)
                {
                    OnObjectKilled(simulator, block.ID);
                }
            }
        }

        /// <summary>
        /// Takes a quantized 16-bit value from a byte array and its range and returns 
        /// a float representation of the continuous value. For example, a value of 
        /// 32767 and a range of -128.0 to 128.0 would return 0.0. The endian conversion 
        /// from the 16-bit little endian to the native platform will also be handled.
        /// </summary>
        /// <param name="byteArray">The byte array containing the short value</param>
        /// <param name="pos">The beginning position of the short (quantized) value</param>
        /// <param name="lower">The lower quantization range</param>
        /// <param name="upper">The upper quantization range</param>
        /// <returns>A 32-bit floating point representation of the dequantized value</returns>
        private float Dequantize(byte[] byteArray, int pos, float lower, float upper)
        {
            ushort value = (ushort)(byteArray[pos] + (byteArray[pos + 1] << 8));
            float QV = (float)value;
            float range = upper - lower;
            float QF = range / 65536.0F;
            return (float)((QV * QF - (0.5F * range)) + QF);
        }
    }
}
