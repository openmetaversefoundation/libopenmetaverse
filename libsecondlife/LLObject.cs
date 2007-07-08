/*
 * Copyright (c) 2007, Second Life Reverse Engineering Team
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
using System.Xml;
using System.Xml.Serialization;
using libsecondlife.Packets;

namespace libsecondlife
{
    /// <summary>
    /// Base class for primitives and avatars
    /// </summary>
    [Serializable]
    public abstract partial class LLObject
    {
        #region Enumerations

        /// <summary>
        /// Primary parameters for primitives such as Physics Enabled or Phantom
        /// </summary>
        [Flags]
        public enum ObjectFlags : uint
        {
            /// <summary>None of the primary flags are enabled</summary>
            None = 0,
            /// <summary>Whether physics are enabled for this object</summary>
            Physics = 0x00000001,
            /// <summary></summary>
            CreateSelected = 0x00000002,
            /// <summary></summary>
            ObjectModify = 0x00000004,
            /// <summary></summary>
            ObjectCopy = 0x00000008,
            /// <summary></summary>
            ObjectAnyOwner = 0x00000010,
            /// <summary></summary>
            ObjectYouOwner = 0x00000020,
            /// <summary></summary>
            Scripted = 0x00000040,
            /// <summary>Whether this object contains an active touch script</summary>
            Touch = 0x00000080,
            /// <summary></summary>
            ObjectMove = 0x00000100,
            /// <summary>Whether this object can receive payments</summary>
            Money = 0x00000200,
            /// <summary>Whether this object is phantom (no collisions)</summary>
            Phantom = 0x00000400,
            /// <summary></summary>
            InventoryEmpty = 0x00000800,
            /// <summary></summary>
            JointHinge = 0x00001000,
            /// <summary></summary>
            JointP2P = 0x00002000,
            /// <summary></summary>
            JointLP2P = 0x00004000,
            /// <summary>Deprecated</summary>
            JointWheel = 0x00008000,
            /// <summary></summary>
            AllowInventoryDrop = 0x00010000,
            /// <summary></summary>
            ObjectTransfer = 0x00020000,
            /// <summary></summary>
            ObjectGroupOwned = 0x00040000,
            /// <summary>Deprecated</summary>
            ObjectYouOfficer = 0x00080000,
            /// <summary></summary>
            CameraDecoupled = 0x00100000,
            /// <summary></summary>
            AnimSource = 0x00200000,
            /// <summary></summary>
            CameraSource = 0x00400000,
            /// <summary></summary>
            CastShadows = 0x00800000,
            /// <summary></summary>
            ObjectOwnerModify = 0x10000000,
            /// <summary></summary>
            TemporaryOnRez = 0x20000000,
            /// <summary></summary>
            Temporary = 0x40000000,
            /// <summary></summary>
            ZlibCompressed = 0x80000000
        }

        /// <summary>
        /// Material type for a primitive
        /// </summary>
        public enum MaterialType : byte
        {
            /// <summary></summary>
            Stone = 0,
            /// <summary></summary>
            Metal,
            /// <summary></summary>
            Glass,
            /// <summary></summary>
            Wood,
            /// <summary></summary>
            Flesh,
            /// <summary></summary>
            Plastic,
            /// <summary></summary>
            Rubber,
            /// <summary></summary>
            Light
        }

        #endregion Enumerations


        #region Structs

        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public struct ObjectData
        {
            /// <summary></summary>
            public int PathTwistBegin;
            /// <summary></summary>
            public float PathEnd;
            /// <summary></summary>
            public float ProfileBegin;
            /// <summary></summary>
            public float PathRadiusOffset;
            /// <summary></summary>
            public float PathSkew;
            /// <summary></summary>
            public uint ProfileCurve;
            /// <summary></summary>
            public float PathScaleX;
            /// <summary></summary>
            public float PathScaleY;
            /// <summary></summary>
            public MaterialType Material;
            /// <summary></summary>
            public float PathShearX;
            /// <summary></summary>
            public float PathShearY;
            /// <summary></summary>
            public float PathTaperX;
            /// <summary></summary>
            public float PathTaperY;
            /// <summary></summary>
            public float ProfileEnd;
            /// <summary></summary>
            public float PathBegin;
            /// <summary></summary>
            public uint PathCurve;
            /// <summary></summary>
            public int PathTwist;
            /// <summary></summary>
            public float ProfileHollow;
            /// <summary></summary>
            public float PathRevolutions;
            /// <summary></summary>
            public uint State;
            /// <summary></summary>
            public ObjectManager.PCode PCode;
        }

        /// <summary>
        /// 
        /// </summary>
        public struct ObjectProperties
        {
            /// <summary></summary>
            public LLUUID ObjectID;
            /// <summary></summary>
            public LLUUID CreatorID;
            /// <summary></summary>
            public LLUUID OwnerID;
            /// <summary></summary>
            public LLUUID GroupID;
            /// <summary></summary>
            public ulong CreationDate;
            /// <summary></summary>
            public uint BaseMask;
            /// <summary></summary>
            public uint OwnerMask;
            /// <summary></summary>
            public uint GroupMask;
            /// <summary></summary>
            public uint EveryoneMask;
            /// <summary></summary>
            public uint NextOwnerMask;
            /// <summary></summary>
            public int OwnershipCost;
            /// <summary></summary>
            public byte SaleType;
            /// <summary></summary>
            public int SalePrice;
            /// <summary></summary>
            public byte AggregatePerms;
            /// <summary></summary>
            public byte AggregatePermTextures;
            /// <summary></summary>
            public byte AggregatePermTexturesOwner;
            /// <summary></summary>
            public uint Category;
            /// <summary></summary>
            public short InventorySerial;
            /// <summary></summary>
            public LLUUID ItemID;
            /// <summary></summary>
            public LLUUID FolderID;
            /// <summary></summary>
            public LLUUID FromTaskID;
            /// <summary></summary>
            public LLUUID LastOwnerID;
            /// <summary></summary>
            public string Name;
            /// <summary></summary>
            public string Description;
            /// <summary></summary>
            public string TouchName;
            /// <summary></summary>
            public string SitName;
            /// <summary></summary>
            public LLUUID[] TextureIDs;
        }

        /// <summary>
        /// 
        /// </summary>
        public struct ObjectPropertiesFamily
        {
            /// <summary>
            /// 
            /// </summary>
            public enum RequestFlagsType
            {
                /// <summary></summary>
                None = 0,
                /// <summary></summary>
                BugReportRequest = 1,
                /// <summary></summary>
                ComplaintReportRequest = 2
            }

            /// <summary></summary>
            public RequestFlagsType RequestFlags;
            /// <summary></summary>
            public LLUUID ObjectID;
            /// <summary></summary>
            public LLUUID OwnerID;
            /// <summary></summary>
            public LLUUID GroupID;
            /// <summary></summary>
            public uint BaseMask;
            /// <summary></summary>
            public uint OwnerMask;
            /// <summary></summary>
            public uint GroupMask;
            /// <summary></summary>
            public uint EveryoneMask;
            /// <summary></summary>
            public uint NextOwnerMask;
            /// <summary></summary>
            public int OwnershipCost;
            /// <summary></summary>
            public byte SaleType;
            /// <summary></summary>
            public int SalePrice;
            /// <summary></summary>
            public uint Category;
            /// <summary></summary>
            public LLUUID LastOwnerID;
            /// <summary></summary>
            public string Name;
            /// <summary></summary>
            public string Description;
        }

        #endregion Structs


        #region Public Members

        /// <summary></summary>
        public LLUUID ID;
        /// <summary></summary>
        public LLUUID GroupID;
        /// <summary></summary>
        public uint LocalID;
        /// <summary></summary>
        public uint ParentID;
        /// <summary></summary>
        public ulong RegionHandle;
        /// <summary></summary>
        public ObjectFlags Flags;
        /// <summary>Unknown</summary>
        public byte[] GenericData;
        /// <summary></summary>
        public LLVector3 Position;
        /// <summary></summary>
        public LLVector3 Scale;
        /// <summary></summary>
        public LLQuaternion Rotation;
        /// <summary></summary>
        public LLVector3 Velocity;
        /// <summary></summary>
        public LLVector3 AngularVelocity;
        /// <summary></summary>
        public LLVector3 Acceleration;
        /// <summary></summary>
        public LLVector4 CollisionPlane;
        /// <summary></summary>
        public TextureEntry Textures;
        /// <summary></summary>
        public ObjectProperties Properties;
        /// <summary></summary>
        [XmlIgnore]
        public ObjectPropertiesFamily PropertiesFamily;
        /// <summary></summary>
        public NameValue[] NameValues;
        /// <summary></summary>
        public ObjectData Data;

        #endregion Public Members


        //internal DateTime lastInterpolation;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            LLObject llobj = obj as LLObject;
            if (llobj == null)
                return false;
            return ID.Equals(llobj.ID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        #region Static Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathScale"></param>
        /// <returns></returns>
        public static byte PathScaleByte(float pathScale)
        {
            // Y = 100 + 100X
            int scale = (int)Math.Round(100.0f * pathScale);
            return (byte)(100 + scale);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathScale"></param>
        /// <returns></returns>
        public static float PathScaleFloat(byte pathScale)
        {
            // Y = -1 + 0.01X
            return (float)Math.Round((double)pathScale * 0.01d - 1.0d, 6);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathShear"></param>
        /// <returns></returns>
        public static byte PathShearByte(float pathShear)
        {
            // Y = 256 + 100X
            int shear = (int)Math.Round(100.0f * pathShear);
            shear += 256;
            return (byte)(shear % 256);
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
        public static ushort ProfileBeginUInt16(float profileBegin)
        {
            return Convert.ToUInt16(profileBegin / 0.00002f);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileBegin"></param>
        /// <returns></returns>
        public static float ProfileBeginFloat(ushort profileBegin)
        {
            return (float)Math.Round((double)profileBegin * 0.00002d, 6);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileEnd"></param>
        /// <returns></returns>
        public static ushort ProfileEndUInt16(float profileEnd)
        {
            return (ushort)(50000 - Convert.ToInt32(profileEnd / 0.00002f));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileEnd"></param>
        /// <returns></returns>
        public static float ProfileEndFloat(ushort profileEnd)
        {
            float temp = (float)Math.Round(profileEnd * 0.00002f, 6);
            if (temp > 1.0f) temp = 1.0f;
            return 1.0f - temp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileHollow"></param>
        /// <returns></returns>
        public static ushort ProfileHollowUInt16(float profileHollow)
        {
            return Convert.ToUInt16(profileHollow / 0.002f);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileHollow"></param>
        /// <returns></returns>
        public static float ProfileHollowFloat(ushort profileHollow)
        {
            float temp = profileHollow * 0.002f;
            if (temp > 100.0f) temp = 0.0f;
            return temp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathBegin"></param>
        /// <returns></returns>
        public static ushort PathBeginUInt16(float pathBegin)
        {
            return Convert.ToUInt16(pathBegin / 0.00002f);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathBegin"></param>
        /// <returns></returns>
        public static float PathBeginFloat(ushort pathBegin)
        {
            return (float)pathBegin * 0.00002f;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathEnd"></param>
        /// <returns></returns>
        public static ushort PathEndUInt16(float pathEnd)
        {
            return (ushort)(50000 - Convert.ToInt32(pathEnd / 0.00002f));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathEnd"></param>
        /// <returns></returns>
        public static float PathEndFloat(ushort pathEnd)
        {
            return (float)(50000 - pathEnd) * 0.00002f;
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
            // Y = 66.5X - 66
            int revolutions = (int)Math.Round(66.5d * (double)pathRevolutions);
            return (byte)(revolutions - 66);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathRevolutions"></param>
        /// <returns></returns>
        public static float PathRevolutionsFloat(byte pathRevolutions)
        {
            // Y = 1 + 0.015X
            return (float)Math.Round(1.0d + (double)pathRevolutions * 0.015d, 6);
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
        public static float PathSkewFloat(sbyte pathSkew)
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
        public static float PathTaperFloat(sbyte pathTaper)
        {
            return (float)pathTaper / 100.0f;
        }

        #endregion Static Methods
    }
}
