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

namespace libsecondlife
{
    public delegate void NewPrimCallback(Simulator simulator, PrimObject prim, U64 regionHandle, ushort timeDilation);
    public delegate void NewAvatarCallback(Simulator simulator, Avatar avatar, U64 regionHandle, ushort timeDilation);
    public delegate void PrimMovedCallback(Simulator simulator, PrimUpdate prim, U64 regionHandle, ushort timeDilation);
    public delegate void AvatarMovedCallback(Simulator simulator, AvatarUpdate avatar, U64 regionHandle, ushort timeDilation);

    /// <summary>
    /// Contains all of the variables sent in an object update packet for a 
    /// prim object. Used to track position and movement of prims.
    /// </summary>
    public struct PrimUpdate
    {
        public uint LocalID;
        public byte State;
        public LLVector3 Position;
        public LLVector3 Velocity;
        public LLVector3 Acceleration;
        public LLQuaternion Rotation;
        public LLVector3 RotationVelocity;
    }

    /// <summary>
    /// Contains all of the variables sent in an object update packet for an 
    /// avatar. Used to track position and movement of avatars.
    /// </summary>
    public struct AvatarUpdate
    {
        public uint LocalID;
        public byte State;
        public LLVector4 CollisionPlane;
        public LLVector3 Position;
        public LLVector3 Velocity;
        public LLVector3 Acceleration;
        public LLQuaternion Rotation;
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
        /// contains a new prim. Depending on the circumstances a client could 
        /// receive two or more of these events for the same object, if you 
        /// or the object left the current sim and returned for example. Client 
        /// applications are responsible for tracking and storing objects.
        /// </summary>
        public event NewPrimCallback OnNewPrim;

        /// <summary>
        /// This event will be raised for every ObjectUpdate block that 
        /// contains a new avatar. Depending on the circumstances a client 
        /// could receive two or more of these events for the same avatar, if 
        /// you or the other avatar left the current sim and returned for 
        /// example. Client applications are responsible for tracking and 
        /// storing objects.
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

        protected SecondLife Client;

        public ObjectManager(SecondLife client)
        {
            Client = client;

            Client.Network.RegisterCallback("ObjectUpdate", new PacketCallback(UpdateHandler));
            Client.Network.RegisterCallback("ImprovedTerseObjectUpdate", new PacketCallback(TerseUpdateHandler));
            Client.Network.RegisterCallback("ObjectUpdateCompressed", new PacketCallback(CompressedUpdateHandler));
            Client.Network.RegisterCallback("ObjectUpdateCached", new PacketCallback(CachedUpdateHandler));
        }

        private void UpdateHandler(Packet packet, Simulator simulator)
        {
            U64 regionHandle = null;
            ushort timeDilation = 0;

            foreach (Block block in packet.Blocks())
            {
                Avatar avatar = null;
                PrimObject prim = new PrimObject();

                foreach (Field field in block.Fields)
                {
                    switch (field.Layout.Name)
                    {
                        case "ID":
                            prim.LocalID = (UInt32)field.Data;
                            break;
                        case "State":
                            prim.State = (byte)field.Data;
                            break;
                        case "FullID":
                            prim.ID = (LLUUID)field.Data;
                            break;
                        case "ParentID":
                            // Linked objects?
                            break;
                        case "OwnerID":
                            // Sound-related
                            break;
                        case "Material":
                            prim.Material = (byte)(field.Data);
                            break;
                        case "PathCurve":
                            prim.PathCurve = (byte)field.Data;
                            break;
                        case "ProfileCurve":
                            prim.ProfileCurve = (byte)field.Data;
                            break;
                        case "PathBegin":
                            prim.PathBegin = PrimObject.PathBeginFloat((byte)field.Data);
                            break;
                        case "PathEnd":
                            prim.PathEnd = PrimObject.PathEndFloat((byte)field.Data);
                            break;
                        case "PathScaleX":
                            prim.PathScaleX = PrimObject.PathScaleFloat((byte)field.Data);
                            break;
                        case "PathScaleY":
                            prim.PathScaleY = PrimObject.PathScaleFloat((byte)field.Data);
                            break;
                        case "PathShearX":
                            prim.PathShearX = PrimObject.PathShearFloat((byte)field.Data);
                            break;
                        case "PathShearY":
                            prim.PathShearY = PrimObject.PathShearFloat((byte)field.Data);
                            break;
                        case "PathTwist":
                            prim.PathTwist = PrimObject.PathTwistFloat((sbyte)field.Data);
                            break;
                        case "PathTwistBegin":
                            prim.PathTwistBegin = PrimObject.PathTwistFloat((sbyte)field.Data);
                            break;
                        case "PathRadiusOffset":
                            prim.PathRadiusOffset = PrimObject.PathRadiusOffsetFloat((sbyte)field.Data);
                            break;
                        case "PathTaperX":
                            prim.PathTaperX = PrimObject.PathTaperFloat((byte)(sbyte)field.Data);
                            break;
                        case "PathTaperY":
                            prim.PathTaperY = PrimObject.PathTaperFloat((byte)(sbyte)field.Data);
                            break;
                        case "PathRevolutions":
                            prim.PathRevolutions = PrimObject.PathRevolutionsFloat((byte)field.Data);
                            break;
                        case "PathSkew":
                            prim.PathSkew = PrimObject.PathSkewFloat((byte)(sbyte)field.Data);
                            break;
                        case "ProfileBegin":
                            prim.ProfileBegin = PrimObject.ProfileBeginFloat((byte)field.Data);
                            break;
                        case "ProfileEnd":
                            prim.ProfileEnd = PrimObject.ProfileEndFloat((byte)field.Data);
                            break;
                        case "ProfileHollow":
                            prim.ProfileHollow = (byte)field.Data;
                            break;
                        case "NameValue":
                            Console.WriteLine("[debug] Name: " + Helpers.FieldToString(field.Data));
                            prim.Name = Helpers.FieldToString(field.Data);
                            break;
                        case "Data":
                            // ?
                            break;
                        case "Text":
                            // Hovering text
                            break;
                        case "TextColor":
                            // LLColor4U of the hovering text
                            break;
                        case "MediaURL":
                            // Quicktime stream
                            Client.Log("MediaURL: " + Helpers.FieldToString(field.Data), Helpers.LogLevel.Info);
                            break;
                        case "TextureEntry":
                            // TODO: Multi-texture support
                            byte[] bytes = (byte[])field.Data;
                            prim.Texture = new LLUUID(bytes, 0);
                            break;
                        case "TextureAnim":
                            // Not sure how this works
                            break;
                        case "JointType":
                            // ?
                            break;
                        case "JointPivot":
                            // ?
                            break;
                        case "JointAxisOrAnchor":
                            // ?
                            break;
                        case "PCode":
                            // ?
                            break;
                        case "PSBlock":
                            // Particle system related
                            break;
                        case "ExtraParams":
                            // ?
                            break;
                        case "Scale":
                            prim.Scale = (LLVector3)field.Data;
                            break;
                        case "Flags":
                            // ?
                            break;
                        case "UpdateFlags":
                            // ?
                            break;
                        case "CRC":
                            // We could optionally verify this on the client side
                            break;
                        case "ClickAction":
                            //
                            break;
                        case "Gain":
                            // Sound-related
                            break;
                        case "Sound":
                            // Sound-related
                            break;
                        case "Radius":
                            // Sound-related
                            break;
                        case "ObjectData":
                            byte[] data = (byte[])field.Data;
                            if (data.Length == 60)
                            {
                                prim.Position = new LLVector3(data, 0);
                                prim.Rotation = new LLQuaternion(data, 36);
                                // Calculate the quaternion W value from the given X/Y/Z
                                float xyzsum = 1.0F -
                                    prim.Rotation.X * prim.Rotation.X -
                                    prim.Rotation.Y * prim.Rotation.Y -
                                    prim.Rotation.Z * prim.Rotation.Z;
                                prim.Rotation.S = (xyzsum > 0.0F) ? (float)Math.Sqrt(xyzsum) : 0.0F;
                                // TODO: Parse the rest of the fields
                            }
                            // TODO: Parse ObjectData for avatars
                            break;
                        case "TimeDilation":
                            timeDilation = (ushort)field.Data;
                            break;
                        case "RegionHandle":
                            regionHandle = (U64)field.Data;
                            break;
                        default:
                            Client.Log("ObjectUpdate field not handled: " + field.Layout.Name + " " + 
                                field.Data.GetType().ToString(), Helpers.LogLevel.Info);
                            break;
                    }
                }

                // Parse the NameValue to see if this is actually an avatar
                if (prim.LocalID != 0)
                {
                    if (prim.Name.IndexOf("FirstName") > -1)
                    {
                        avatar = new Avatar();
                        avatar.ID = prim.ID;
                        avatar.LocalID = prim.LocalID;
                        // FIXME: Parse the correct name and group name
                        avatar.Name = prim.Name;
                        avatar.GroupName = prim.Name;
                        avatar.Online = true;
                        avatar.Position = prim.Position;
                        // TODO: Look up the region by regionHandle instead
                        avatar.CurrentRegion = simulator.Region;

                        prim = null;

                        // If an event handler is registered call it
                        if (OnNewAvatar != null)
                        {
                            OnNewAvatar(simulator, avatar, regionHandle, timeDilation);
                        }
                    }
                    else
                    {
                        // If an event handler is registered call it
                        if (OnNewPrim != null)
                        {
                            OnNewPrim(simulator, prim, regionHandle, timeDilation);
                        }
                    }
                }
            }
        }

        private void TerseUpdateHandler(Packet packet, Simulator simulator)
        {
            U64 regionHandle = null;
            ushort timeDilation = 0;
            bool avatar = false;
            int i;
            byte[] data;
            uint localid = 0;
            byte state = 0;
            float x, y, z, s;
            LLVector4 CollisionPlane = null;
            LLVector3 Position = null, Velocity = null, Acceleration = null, RotationVelocity = null;
            LLQuaternion Rotation = null;

            foreach (Block block in packet.Blocks())
            {
                foreach (Field field in block.Fields)
                {
                    switch (field.Layout.Name)
                    {
                        case "Data":
                            i = 0;
                            data = (byte[])field.Data;

                            // Region ID
                            localid = (uint)(data[i] + (data[i + 1] << 8) + (data[i + 2] << 16) + (data[i + 3] << 24));
                            i += 4;

                            // Object state
                            state = data[i++];

                            // Avatar boolean
                            avatar = Convert.ToBoolean(data[i]);
                            i++;

                            if (avatar)
                            {
                                CollisionPlane = new LLVector4(data, i);
                                i += 16;
                            }

                            // Position
                            Position = new LLVector3(data, i);
                            i += 12;
                            // Velocity
                            x = Dequantize(data, i, -128.0F, 128.0F);
                            i += 2;
                            y = Dequantize(data, i, -128.0F, 128.0F);
                            i += 2;
                            z = Dequantize(data, i, -128.0F, 128.0F);
                            i += 2;
                            Velocity = new LLVector3(x, y, z);
                            // Acceleration
                            x = Dequantize(data, i, -64.0F, 64.0F);
                            i += 2;
                            y = Dequantize(data, i, -64.0F, 64.0F);
                            i += 2;
                            z = Dequantize(data, i, -64.0F, 64.0F);
                            i += 2;
                            Acceleration = new LLVector3(x, y, z);
                            // Rotation
                            x = Dequantize(data, i, -1.0F, 1.0F);
                            i += 2;
                            y = Dequantize(data, i, -1.0F, 1.0F);
                            i += 2;
                            z = Dequantize(data, i, -1.0F, 1.0F);
                            i += 2;
                            s = Dequantize(data, i, -1.0F, 1.0F);
                            i += 2;
                            Rotation = new LLQuaternion(x, y, z, s);
                            // Rotation velocity
                            x = Dequantize(data, i, -64.0F, 64.0F);
                            i += 2;
                            y = Dequantize(data, i, -64.0F, 64.0F);
                            i += 2;
                            z = Dequantize(data, i, -64.0F, 64.0F);
                            i += 2;
                            RotationVelocity = new LLVector3(x, y, z);

                            break;
                        case "RegionHandle":
                            regionHandle = (U64)field.Data;
                            break;
                        case "TimeDilation":
                            timeDilation = (ushort)field.Data;
                            break;
                        case "TextureEntry":
                            //
                            break;
                    }
                }
            }

            if (avatar)
            {
                AvatarUpdate avupdate = new AvatarUpdate();
                avupdate.LocalID = localid;
                avupdate.State = state;
                avupdate.Position = Position;
                avupdate.CollisionPlane = CollisionPlane;
                avupdate.Velocity = Velocity;
                avupdate.Acceleration = Acceleration;
                avupdate.Rotation = Rotation;
                avupdate.RotationVelocity = RotationVelocity;

                // If an event handler is registered call it
                if (OnAvatarMoved != null)
                {
                    OnAvatarMoved(simulator, avupdate, regionHandle, timeDilation);
                }
            }
            else
            {
                PrimUpdate primupdate = new PrimUpdate();
                primupdate.LocalID = localid;
                primupdate.State = state;
                primupdate.Position = Position;
                primupdate.Velocity = Velocity;
                primupdate.Acceleration = Acceleration;
                primupdate.Rotation = Rotation;
                primupdate.RotationVelocity = RotationVelocity;

                // If an event handler is registered call it
                if (OnPrimMoved != null)
                {
                    OnPrimMoved(simulator, primupdate, regionHandle, timeDilation);
                }
            }
        }

        private void CompressedUpdateHandler(Packet packet, Simulator simulator)
        {
            Client.Log("Received an ObjectUpdateCompressed packet, length=" + packet.Data.Length, Helpers.LogLevel.Info);
        }

        private void CachedUpdateHandler(Packet packet, Simulator simulator)
        {
            Client.Log("Received an ObjectUpdateCached packet, length=" + packet.Data.Length, Helpers.LogLevel.Info);
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
