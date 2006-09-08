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
    /// 
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
    /// 
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
	/// Tracks all the objects (avatars and prims) in a region
	/// </summary>
	public class ObjectManager
    {
        public event NewPrimCallback OnNewPrim;
        public event NewAvatarCallback OnNewAvatar;
        public event PrimMovedCallback OnPrimMoved;
        public event AvatarMovedCallback OnAvatarMoved;

        private SecondLife Client;

        public ObjectManager(SecondLife client)
        {
            Client = client;

            Client.Network.RegisterCallback("ObjectUpdate", new PacketCallback(UpdateHandler));
            Client.Network.RegisterCallback("ImprovedTerseObjectUpdate", new PacketCallback(TerseUpdateHandler));
        }

        private void UpdateHandler(Packet packet, Simulator simulator)
        {
            U64 regionHandle = null;
            ushort timeDilation = 0;

            Avatar avatar = null;
            PrimObject prim = new PrimObject();

            foreach (Block block in packet.Blocks())
            {
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
                            //prim.ParentID = (uint)field.Data;
                            break;
                        case "OwnerID":
                            //prim.OwnerID = (LLUUID)field.Data;
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
                            prim.PathBegin = (byte)field.Data;
                            break;
                        case "PathEnd":
                            prim.PathEnd = (byte)field.Data;
                            break;
                        case "PathScaleX":
                            prim.PathScaleX = (float)((byte)field.Data);
                            break;
                        case "PathScaleY":
                            prim.PathScaleY = (float)((byte)field.Data);
                            break;
                        case "PathShearX":
                            prim.PathShearX = (float)((byte)field.Data);
                            break;
                        case "PathShearY":
                            prim.PathShearY = (float)((byte)field.Data);
                            break;
                        case "PathTwist":
                            prim.PathTwist = (sbyte)field.Data;
                            break;
                        case "PathRadiusOffset":
                            prim.PathRadiusOffset = (float)((sbyte)field.Data);
                            break;
                        case "PathTaperX":
                            prim.PathTaperX = (float)((sbyte)field.Data);
                            break;
                        case "PathTaperY":
                            prim.PathTaperY = (float)((sbyte)field.Data);
                            break;
                        case "PathRevolutions":
                            prim.PathRevolutions = (float)((byte)field.Data);
                            break;
                        case "PathSkew":
                            prim.PathSkew = (float)((sbyte)field.Data);
                            break;
                        case "ProfileBegin":
                            prim.ProfileBegin = (float)((byte)field.Data);
                            break;
                        case "ProfileEnd":
                            prim.ProfileEnd = (float)((byte)field.Data);
                            break;
                        case "ProfileHollow":
                            prim.ProfileHollow = (byte)field.Data;
                            break;
                        case "NameValue":
                            Console.WriteLine("[debug] Name: " + Helpers.FieldToString(field.Data));
                            prim.Name = Helpers.FieldToString(field.Data);
                            break;
                        case "Data":
                            //prim.Sound = (LLUUID)field.Data;
                            break;
                        case "Text":
                            //
                            break;
                        case "TextColor":
                            // LLColor4U
                            break;
                        case "MediaURL":
                            Console.WriteLine("[debug] MediaURL: " + Helpers.FieldToString(field.Data));
                            //
                            break;
                        case "TextureEntry":
                            byte[] bytes = (byte[])field.Data;
                            prim.Texture = new LLUUID(bytes, 0);
                            break;
                        case "TextureAnim":
                            break;
                        case "JointType":
                            //prim.JointType = (byte)field.Data;
                            break;
                        case "JointPivot":
                            //
                            break;
                        case "JointAxisOrAnchor":
                            //
                            break;
                        case "PCode":
                            //prim.PCode = (byte)field.Data;
                            break;
                        case "PSBlock":
                            //
                            break;
                        case "ExtraParams":
                            //
                            break;
                        case "Scale":
                            prim.Scale = (LLVector3)field.Data;
                            break;
                        case "Flags":
                            //prim.Flags = (byte)field.Data;
                            break;
                        case "UpdateFlags":
                            //prim.UpdateFlags = (uint)field.Data;
                            break;
                        case "PathTwistBegin":
                            prim.PathTwistBegin = (sbyte)field.Data;
                            break;
                        case "CRC":
                            //prim.CRC = (uint)field.Data;
                            break;
                        case "ClickAction":
                            //prim.ClickAction = (byte)field.Data;
                            break;
                        case "Gain":
                            //prim.Gain = (Single)field.Data;
                            break;
                        case "Sound":
                            //prim.Sound = (LLUUID)field.Data;
                            break;
                        case "Radius":
                            //prim.Radius = (Single)field.Data;
                            break;
                        case "ObjectData":
                            //
                            break;
                        case "TimeDilation":
                            timeDilation = (ushort)field.Data;
                            break;
                        case "RegionHandle":
                            regionHandle = (U64)field.Data;
                            break;
                        default:
                            Console.WriteLine("Field Not Handled: " + field.Layout.Name + " " + field.Data.GetType().ToString());
                            break;
                    }
                }
            }

            // Parse the NameValue to see if this is actually an avatar
            if (prim.Name.Contains("FirstName"))
            {
                avatar = new Avatar();
                avatar.ID = prim.ID;
                avatar.LocalID = prim.LocalID;
                // FIXME: Parse the correct name and group name
                avatar.Name = prim.Name;
                avatar.GroupName = prim.Name;
                avatar.Online = true;
                avatar.Position = prim.Position;
                // FIXME: Look up the region by regionHandle instead
                avatar.CurrentRegion = simulator.Region;

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

        private void TerseUpdateHandler(Packet packet, Simulator simulator)
        {
            bool avatar = false;
            int i;
            byte[] data;
            uint localid = 0;
            byte state = 0;
            float x, y, z, s;
            LLVector4 CollisionPlane = null;
            LLVector3 Position = null, Velocity = null, Acceleration = null, RotationVelocity = null;
            LLQuaternion Rotation = null;

            // Create an AvatarUpdate or PrimUpdate and fire the callback
            Client.Log("ImprovedTerseObjectUpdate", Helpers.LogLevel.Info);

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
                            //
                            break;
                        case "TimeDilation":
                            //
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

                Client.Log("LocalID: " + localid + ", State: " + state + ", Position: " + Position.ToString() + 
                    ", CollisionPlane: " + CollisionPlane.ToString() + ", Velocity: " + Velocity.ToString() + 
                    ", Acceleration: " + Acceleration.ToString() + ", Rotation: " + Rotation.ToString() + 
                    ", RotationVelocity: " + RotationVelocity.ToString(), Helpers.LogLevel.Info);
            }
            else
            {
                ;
            }

            // If an event handler is registered call it
        }

        /// <summary>
        /// Takes a quantized value and its quantization range and returns a float 
        /// representation of the continuous value. For example, a value of 32767 
        /// and a range of -128.0 to 128.0 would return 0.0. The endian conversion 
        /// from the 16-bit little endian to the native platform will also be handled.
        /// </summary>
        /// <param name="byteArray">The byte array containing the short value</param>
        /// <param name="pos">The beginning position of the short (quantized) value</param>
        /// <param name="lower">The lower quantization range</param>
        /// <param name="upper">The upper quantization range</param>
        /// <returns>A 32-bit floating point representation of the dequantized value</returns>
        public static float Dequantize(byte[] byteArray, int pos, float lower, float upper)
        {
            ushort value = (ushort)(byteArray[pos] + (byteArray[pos + 1] << 8));
            float QV = (float)value;
            float range = upper - lower;
            float QF = range / 65536.0F;
            return (float)((QV * QF - (0.5F * range)) + QF);
        }
    }
}
