﻿/*
 * Copyright (c) 2006-2014, openmetaverse.org
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.org nor the names
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

namespace OpenMetaverse
{
    /// <summary>
    /// Reads in a byte array of an Animation Asset created by the SecondLife(tm) client.
    /// </summary>
    public class BinBVHAnimationReader
    {
        /// <summary>
        /// Rotation Keyframe count (used internally)
        /// </summary>
        private int rotationkeys;
        
        /// <summary>
        /// Position Keyframe count (used internally)
        /// </summary>
        private int positionkeys;

        public UInt16 unknown0; // Always 1
        public UInt16 unknown1; // Always 0

        /// <summary>
        /// Animation Priority
        /// </summary>
        public int Priority;

        /// <summary>
        /// The animation length in seconds.
        /// </summary>
        public Single Length;

        /// <summary>
        /// Expression set in the client.  Null if [None] is selected
        /// </summary>
        public string ExpressionName; // "" (null)

        /// <summary>
        /// The time in seconds to start the animation
        /// </summary>
        public Single InPoint;

        /// <summary>
        /// The time in seconds to end the animation
        /// </summary>
        public Single OutPoint;

        /// <summary>
        /// Loop the animation
        /// </summary>
        public bool Loop;

        /// <summary>
        /// Meta data. Ease in Seconds.
        /// </summary>
        public Single EaseInTime;

        /// <summary>
        /// Meta data. Ease out seconds.
        /// </summary>
        public Single EaseOutTime;

        /// <summary>
        /// Meta Data for the Hand Pose
        /// </summary>
        public uint HandPose;

        /// <summary>
        /// Number of joints defined in the animation
        /// </summary>
        public uint JointCount;


        /// <summary>
        /// Contains an array of joints
        /// </summary>
        public binBVHJoint[] joints;

        /// <summary>
        /// Searialize an animation asset into it's joints/keyframes/meta data
        /// </summary>
        /// <param name="animationdata"></param>
        public BinBVHAnimationReader(byte[] animationdata)
        {
            int i = 0;
            if (!BitConverter.IsLittleEndian)
            {
                unknown0 = Utils.BytesToUInt16(EndianSwap(animationdata, i, 2)); i += 2; // Always 1
                unknown1 = Utils.BytesToUInt16(EndianSwap(animationdata, i, 2)); i += 2; // Always 0
                Priority = Utils.BytesToInt(EndianSwap(animationdata, i, 4)); i += 4;
                Length = Utils.BytesToFloat(EndianSwap(animationdata, i, 4), 0); i += 4;
            }
            else
            {
                unknown0 = Utils.BytesToUInt16(animationdata, i); i += 2; // Always 1
                unknown1 = Utils.BytesToUInt16(animationdata, i); i += 2; // Always 0
                Priority = Utils.BytesToInt(animationdata, i); i += 4;
                Length = Utils.BytesToFloat(animationdata, i); i += 4;
            }
            ExpressionName = ReadBytesUntilNull(animationdata, ref i);
            if (!BitConverter.IsLittleEndian)
            {
                InPoint = Utils.BytesToFloat(EndianSwap(animationdata, i, 4), 0); i += 4;
                OutPoint = Utils.BytesToFloat(EndianSwap(animationdata, i, 4), 0); i += 4;
                Loop = (Utils.BytesToInt(EndianSwap(animationdata, i, 4)) != 0); i += 4;
                EaseInTime = Utils.BytesToFloat(EndianSwap(animationdata, i, 4), 0); i += 4;
                EaseOutTime = Utils.BytesToFloat(EndianSwap(animationdata, i, 4), 0); i += 4;
                HandPose = Utils.BytesToUInt(EndianSwap(animationdata, i, 4)); i += 4; // Handpose?

                JointCount = Utils.BytesToUInt(animationdata, i); i += 4; // Get Joint count
            }
            else
            {
                InPoint = Utils.BytesToFloat(animationdata, i); i += 4;
                OutPoint = Utils.BytesToFloat(animationdata, i); i += 4;
                Loop = (Utils.BytesToInt(animationdata, i) != 0); i += 4;
                EaseInTime = Utils.BytesToFloat(animationdata, i); i += 4;
                EaseOutTime = Utils.BytesToFloat(animationdata, i); i += 4;
                HandPose = Utils.BytesToUInt(animationdata, i); i += 4; // Handpose?

                JointCount = Utils.BytesToUInt(animationdata, i); i += 4; // Get Joint count
            }
            joints = new binBVHJoint[JointCount];

            // deserialize the number of joints in the animation.
            // Joints are variable length blocks of binary data consisting of joint data and keyframes
            for (int iter = 0; iter < JointCount; iter++)
            {
                binBVHJoint joint = readJoint(animationdata, ref i);
                joints[iter] = joint;
            }
        }

        private byte[] EndianSwap(byte[] arr, int offset, int len)
        {
            byte[] bendian = new byte[offset + len];
            Buffer.BlockCopy(arr, offset, bendian, 0, len);
            Array.Reverse(bendian);
            return bendian;
        }
        /// <summary>
        /// Variable length strings seem to be null terminated in the animation asset..    but..   
        /// use with caution, home grown.
        /// advances the index.
        /// </summary>
        /// <param name="data">The animation asset byte array</param>
        /// <param name="i">The offset to start reading</param>
        /// <returns>a string</returns>
        public string ReadBytesUntilNull(byte[] data, ref int i)
        {
            char nterm = '\0'; // Null terminator
            int endpos = i;
            int startpos = i;

            // Find the null character
            for (int j = i; j < data.Length; j++)
            {
                char spot = Convert.ToChar(data[j]);
                if (spot == nterm)
                {
                    endpos = j;
                    break;
                }
            }

            // if we got to the end, then it's a zero length string
            if (i == endpos)
            {
                // advance the 1 null character
                i++;
                return string.Empty;
            }
            else
            {
                // We found the end of the string
                // append the bytes from the beginning of the string to the end of the string
                // advance i
                byte[] interm = new byte[endpos - i];
                for (; i < endpos; i++)
                {
                    interm[i - startpos] = data[i];
                }
                i++;  // advance past the null character

                return Utils.BytesToString(interm);
            }
        }

        /// <summary>
        /// Read in a Joint from an animation asset byte array
        /// Variable length Joint fields, yay!
        /// Advances the index
        /// </summary>
        /// <param name="data">animation asset byte array</param>
        /// <param name="i">Byte Offset of the start of the joint</param>
        /// <returns>The Joint data serialized into the binBVHJoint structure</returns>
        public binBVHJoint readJoint(byte[] data, ref int i)
        {

            binBVHJointKey[] positions;
            binBVHJointKey[] rotations;

            binBVHJoint pJoint = new binBVHJoint();

            /*
                109
                84
                111
                114
                114
                111
                0 <--- Null terminator
            */

            pJoint.Name = ReadBytesUntilNull(data, ref i); // Joint name

            /* 
                 2 <- Priority Revisited
                 0
                 0
                 0
            */

            /* 
                5 <-- 5 keyframes
                0
                0
                0
                ... 5 Keyframe data blocks
            */

            /* 
                2 <-- 2 keyframes
                0
                0
                0
                ..  2 Keyframe data blocks
            */
            if (!BitConverter.IsLittleEndian)
            {
                pJoint.Priority = Utils.BytesToInt(EndianSwap(data, i, 4)); i += 4; // Joint Priority override?
                rotationkeys = Utils.BytesToInt(EndianSwap(data, i, 4)); i += 4; // How many rotation keyframes
            }
            else
            {
                pJoint.Priority = Utils.BytesToInt(data, i); i += 4; // Joint Priority override?
                rotationkeys = Utils.BytesToInt(data, i); i += 4; // How many rotation keyframes
            }
            
            // Sanity check how many rotation keys there are
            if (rotationkeys < 0 || rotationkeys > 10000)
            {
                rotationkeys = 0;
            }

            rotations = readKeys(data, ref i, rotationkeys, -1.0f, 1.0f);
            
            if (!BitConverter.IsLittleEndian)
            {
                positionkeys = Utils.BytesToInt(EndianSwap(data, i, 4)); i += 4; // How many position keyframes
            }
            else
            {
                positionkeys = Utils.BytesToInt(data, i); i += 4; // How many position keyframes
            }

            // Sanity check how many positions keys there are
            if (positionkeys < 0 || positionkeys > 10000)
            {
                positionkeys = 0;
            }

            // Read in position keyframes
            positions = readKeys(data, ref i, positionkeys, -0.5f, 1.5f);

            pJoint.rotationkeys = rotations;
            pJoint.positionkeys = positions;

            return pJoint;
        }

        /// <summary>
        /// Read Keyframes of a certain type
        /// advance i
        /// </summary>
        /// <param name="data">Animation Byte array</param>
        /// <param name="i">Offset in the Byte Array.  Will be advanced</param>
        /// <param name="keycount">Number of Keyframes</param>
        /// <param name="min">Scaling Min to pass to the Uint16ToFloat method</param>
        /// <param name="max">Scaling Max to pass to the Uint16ToFloat method</param>
        /// <returns></returns>
        public binBVHJointKey[] readKeys(byte[] data, ref int i, int keycount, float min, float max)
        {
            float x;
            float y;
            float z;

            /*
                17          255  <-- Time Code
                17          255  <-- Time Code
                255         255  <-- X
                127         127  <-- X
                255         255  <-- Y
                127         127  <-- Y
                213         213  <-- Z
                142         142  <---Z

            */

            binBVHJointKey[] m_keys = new binBVHJointKey[keycount];
            for (int j = 0; j < keycount; j++)
            {
                binBVHJointKey pJKey = new binBVHJointKey();
                if (!BitConverter.IsLittleEndian)
                {
                    pJKey.time = Utils.UInt16ToFloat(EndianSwap(data, i, 2), 0, InPoint, OutPoint); i += 2;
                    x = Utils.UInt16ToFloat(EndianSwap(data, i, 2), 0, min, max); i += 2;
                    y = Utils.UInt16ToFloat(EndianSwap(data, i, 2), 0, min, max); i += 2;
                    z = Utils.UInt16ToFloat(EndianSwap(data, i, 2), 0, min, max); i += 2;
                }
                else
                {
                    pJKey.time = Utils.UInt16ToFloat(data, i, InPoint, OutPoint); i += 2;
                    x = Utils.UInt16ToFloat(data, i, min, max); i += 2;
                    y = Utils.UInt16ToFloat(data, i, min, max); i += 2;
                    z = Utils.UInt16ToFloat(data, i, min, max); i += 2;
                }
                pJKey.key_element = new Vector3(x, y, z);
                m_keys[j] = pJKey;
            }
            return m_keys;
        }

        public bool Equals(BinBVHAnimationReader other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.Loop.Equals(Loop) && other.OutPoint == OutPoint && other.InPoint == InPoint && other.Length == Length && other.HandPose == HandPose && other.JointCount == JointCount && Equals(other.joints, joints) && other.EaseInTime == EaseInTime && other.EaseOutTime == EaseOutTime && other.Priority == Priority && other.unknown1 == unknown1 && other.unknown0 == unknown0 && other.positionkeys == positionkeys && other.rotationkeys == rotationkeys;
        }

        /// <summary> 
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>. 
        /// </summary> 
        /// <returns> 
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false. 
        /// </returns> 
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.  
        ///                 </param><exception cref="T:System.NullReferenceException">The <paramref name="obj"/> parameter is null. 
        ///                 </exception><filterpriority>2</filterpriority> 
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(BinBVHAnimationReader)) return false;
            return Equals((BinBVHAnimationReader)obj);
    }

    /// <summary>
        /// Serves as a hash function for a particular type.  
        /// </summary> 
        /// <returns> 
        /// A hash code for the current <see cref="T:System.Object"/>. 
        /// </returns> 
        /// <filterpriority>2</filterpriority> 
        public override int GetHashCode()
        {
            unchecked
            {
                int result = Loop.GetHashCode();
                result = (result * 397) ^ OutPoint.GetHashCode();
                result = (result * 397) ^ InPoint.GetHashCode();
                result = (result * 397) ^ Length.GetHashCode();
                result = (result * 397) ^ HandPose.GetHashCode();
                result = (result * 397) ^ JointCount.GetHashCode();
                result = (result * 397) ^ (joints != null ? joints.GetHashCode() : 0);
                result = (result * 397) ^ EaseInTime.GetHashCode();
                result = (result * 397) ^ EaseOutTime.GetHashCode();
                result = (result * 397) ^ Priority;
                result = (result * 397) ^ unknown1.GetHashCode();
                result = (result * 397) ^ unknown0.GetHashCode();
                result = (result * 397) ^ positionkeys;
                result = (result * 397) ^ rotationkeys;
                return result;
            }
        }

        public static bool Equals(binBVHJoint[] arr1, binBVHJoint[] arr2)
        {
            if (arr1.Length == arr2.Length)
            {
                for (int i = 0; i < arr1.Length; i++)
                    if (!arr1[i].Equals(arr2[i]))
                        return false;
                /* not same*/
                return true;
            }
            return false;
        }

    }


    /// <summary> 
    /// A Joint and it's associated meta data and keyframes
    /// </summary>
    public struct binBVHJoint
    {
        public static bool Equals(binBVHJointKey[] arr1, binBVHJointKey[] arr2)
        {
            if (arr1.Length == arr2.Length)
            {
                for (int i = 0; i < arr1.Length; i++)
                    if (!Equals(arr1[i], arr2[i]))
                        return false;
                /* not same*/
                return true;
            }
            return false;
        }
        public static bool Equals(binBVHJointKey arr1, binBVHJointKey arr2)
        {
            return (arr1.time == arr2.time && arr1.key_element == arr2.key_element);
        }

        public bool Equals(binBVHJoint other)
        {
            return other.Priority == Priority && Equals(other.rotationkeys, rotationkeys) && Equals(other.Name, Name) && Equals(other.positionkeys, positionkeys);
        }

        /// <summary> 
        /// Indicates whether this instance and a specified object are equal. 
        /// </summary> 
        /// <returns> 
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false. 
        /// </returns> 
        /// <param name="obj">Another object to compare to.  
        ///                 </param><filterpriority>2</filterpriority> 
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof(binBVHJoint)) return false;
            return Equals((binBVHJoint)obj);
        }

        /// <summary> 
        /// Returns the hash code for this instance. 
        /// </summary> 
        /// <returns> 
        /// A 32-bit signed integer that is the hash code for this instance. 
        /// </returns> 
        /// <filterpriority>2</filterpriority> 
        public override int GetHashCode()
        {
            unchecked
            {
                int result = Priority;
                result = (result * 397) ^ (rotationkeys != null ? rotationkeys.GetHashCode() : 0);
                result = (result * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                result = (result * 397) ^ (positionkeys != null ? positionkeys.GetHashCode() : 0);
                return result;
            }
        }

        public static bool operator ==(binBVHJoint left, binBVHJoint right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(binBVHJoint left, binBVHJoint right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Name of the Joint.  Matches the avatar_skeleton.xml in client distros
        /// </summary>
        public string Name;

        /// <summary>
        /// Joint Animation Override?   Was the same as the Priority in testing.. 
        /// </summary>
        public int Priority;

        /// <summary>
        /// Array of Rotation Keyframes in order from earliest to latest
        /// </summary>
        public binBVHJointKey[] rotationkeys;

        /// <summary>
        /// Array of Position Keyframes in order from earliest to latest
        /// This seems to only be for the Pelvis?
        /// </summary>
        public binBVHJointKey[] positionkeys;

        /// <summary>
        /// Custom application data that can be attached to a joint
        /// </summary>
        public object Tag;
    }

    /// <summary>
    /// A Joint Keyframe.  This is either a position or a rotation.
    /// </summary>
    public struct binBVHJointKey
    {
        // Time in seconds for this keyframe.
        public float time;

        /// <summary>
        /// Either a Vector3 position or a Vector3 Euler rotation
        /// </summary>
        public Vector3 key_element;
    }

    /// <summary>
    /// Poses set in the animation metadata for the hands.
    /// </summary>
    public enum HandPose : uint
    {
        Spread = 0,
        Relaxed = 1,
        Point_Both = 2,
        Fist = 3,
        Relaxed_Left = 4,
        Point_Left = 5,
        Fist_Left = 6,
        Relaxed_Right = 7,
        Point_Right = 8,
        Fist_Right = 9,
        Salute_Right = 10,
        Typing = 11,
        Peace_Right = 12
    }
}
