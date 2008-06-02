/*
 * Copyright (c) 2007-2008, Second Life Reverse Engineering Team
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
    /// Base class for primitives and avatars
    /// </summary>
    public abstract partial class LLObject
    {
        // Used for packing and unpacking parameters
        protected const float CUT_QUANTA = 0.00002f;
        protected const float SCALE_QUANTA = 0.01f;
        protected const float SHEAR_QUANTA = 0.01f;
        protected const float TAPER_QUANTA = 0.01f;
        protected const float REV_QUANTA = 0.015f;
        protected const float HOLLOW_QUANTA = 0.00002f;

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

        public enum ProfileCurve : byte
        {
            Circle = 0x00,
            Square = 0x01,
            IsoTriangle = 0x02,
            EqualTriangle = 0x03,
            RightTriangle = 0x04,
            HalfCircle = 0x05
        }

        public enum HoleType : byte
        {
            Same = 0x00,
            Circle = 0x10,
            Square = 0x20,
            Triangle = 0x30
        }

        public enum PathCurve : byte
        {
            Line = 0x10,
            Circle = 0x20,
            Circle2 = 0x30,
            Test = 0x40,
            Flexible = 0x80
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

        public enum PrimType
        {
            Unknown,
            Box,
            Cylinder,
            Prism,
            Sphere,
            Torus,
            Tube,
            Ring,
            Sculpt
        }

        #endregion Enumerations

        #region Structs

        /// <summary>
        /// 
        /// </summary>
        public struct ObjectData
        {
            private const byte PROFILE_MASK = 0x0F;
            private const byte HOLE_MASK = 0xF0;

            /// <summary></summary>
            internal byte profileCurve;

            /// <summary></summary>
            public PathCurve PathCurve;
            /// <summary></summary>
            public float PathEnd;
            /// <summary></summary>
            public float PathRadiusOffset;
            /// <summary></summary>
            public float PathSkew;
            /// <summary></summary>
            public float PathScaleX;
            /// <summary></summary>
            public float PathScaleY;
            /// <summary></summary>
            public float PathShearX;
            /// <summary></summary>
            public float PathShearY;
            /// <summary></summary>
            public float PathTaperX;
            /// <summary></summary>
            public float PathTaperY;
            /// <summary></summary>
            public float PathBegin;
            /// <summary></summary>
            public float PathTwist;
            /// <summary></summary>
            public float PathTwistBegin;
            /// <summary></summary>
            public float PathRevolutions;
            /// <summary></summary>
            public float ProfileBegin;
            /// <summary></summary>
            public float ProfileEnd;
            /// <summary></summary>
            public float ProfileHollow;

            /// <summary></summary>
            public MaterialType Material;
            /// <summary></summary>
            public byte State;
            /// <summary></summary>
            public PCode PCode;

            #region Properties

            /// <summary></summary>
            public ProfileCurve ProfileCurve
            {
                get { return (ProfileCurve)(profileCurve & PROFILE_MASK); }
                set
                {
                    profileCurve &= HOLE_MASK;
                    profileCurve |= (byte)value;
                }
            }

            /// <summary></summary>
            public HoleType ProfileHole
            {
                get { return (HoleType)(profileCurve & HOLE_MASK); }
                set
                {
                    profileCurve &= PROFILE_MASK;
                    profileCurve |= (byte)value;
                }
            }

            /// <summary></summary>
            public LLVector2 PathBeginScale
            {
                get
                {
                    LLVector2 begin = new LLVector2(1f, 1f);
                    if (PathScaleX > 1f)
                        begin.X = 2f - PathScaleX;
                    if (PathScaleY > 1f)
                        begin.Y = 2f - PathScaleY;
                    return begin;
                }
            }

            /// <summary></summary>
            public LLVector2 PathEndScale
            {
                get
                {
                    LLVector2 end = new LLVector2(1f, 1f);
                    if (PathScaleX < 1f)
                        end.X = PathScaleX;
                    if (PathScaleY < 1f)
                        end.Y = PathScaleY;
                    return end;
                }
            }

            public PrimType Type
            {
                get
                {
                    bool linearPath = (PathCurve == LLObject.PathCurve.Line || PathCurve == LLObject.PathCurve.Flexible);
                    float scaleX = PathScaleX;
                    float scaleY = PathScaleY;

                    if (linearPath && ProfileCurve == LLObject.ProfileCurve.Circle)
                        return PrimType.Cylinder;
                    else if (linearPath && ProfileCurve == LLObject.ProfileCurve.Square)
                        return PrimType.Box;
                    else if (linearPath && ProfileCurve == LLObject.ProfileCurve.IsoTriangle)
                        return PrimType.Prism;
                    else if (linearPath && ProfileCurve == LLObject.ProfileCurve.EqualTriangle)
                        return PrimType.Prism;
                    else if (linearPath && ProfileCurve == LLObject.ProfileCurve.RightTriangle)
                        return PrimType.Prism;
                    else if (PathCurve == LLObject.PathCurve.Flexible)
                        return PrimType.Unknown;
                    else if (PathCurve == LLObject.PathCurve.Circle && ProfileCurve == LLObject.ProfileCurve.Circle && scaleY > 0.75f)
                        return PrimType.Sphere;
                    else if (PathCurve == LLObject.PathCurve.Circle && ProfileCurve == LLObject.ProfileCurve.Circle && scaleY <= 0.75f)
                        return PrimType.Torus;
                    else if (PathCurve == LLObject.PathCurve.Circle && ProfileCurve == LLObject.ProfileCurve.HalfCircle)
                        return PrimType.Sphere;
                    else if (PathCurve == LLObject.PathCurve.Circle2 && ProfileCurve == LLObject.ProfileCurve.Circle)
                        return PrimType.Sphere; // Spiral/sphere
                    else if (PathCurve == LLObject.PathCurve.Circle && ProfileCurve == LLObject.ProfileCurve.EqualTriangle)
                        return PrimType.Ring;
                    else if (PathCurve == LLObject.PathCurve.Circle && ProfileCurve == LLObject.ProfileCurve.Square && scaleY <= 0.75f)
                        return PrimType.Tube;
                    else
                        return PrimType.Unknown;
                }
            }

            #endregion Properties

            public override string ToString()
            {
                return Type.ToString();
            }
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
            /// <summary>This is a protocol hack, it will only be set if the
            /// object has a non-null sound so you can mute the owner</summary>
            public LLUUID OwnerID;
            /// <summary></summary>
            public LLUUID GroupID;
            /// <summary></summary>
            public ulong CreationDate;
            /// <summary></summary>
            public Permissions Permissions;
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
            public Permissions Permissions;
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

            //public bool IsOwnedBy(SecondLife client)
            //{
            //    if (GroupID != LLUUID.Zero)
            //    {
            //        // Group owned, iterate through all of this clients groups
            //        // and see if it is a member
            //        //client.Groups.
            //        // FIXME: Current groups should be stored in GroupManager and auto-requested (with a setting to turn off)
            //    }
            //    else
            //    {
            //        // Avatar owned
            //    }
            //}
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
        public ObjectPropertiesFamily PropertiesFamily;
        /// <summary></summary>
        public NameValue[] NameValues;
        /// <summary></summary>
        public ObjectData Data;

        #endregion Public Members

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

        #region Parameter Packing Methods

        public static ushort PackBeginCut(float beginCut)
        {
            return (ushort)Math.Round(beginCut / CUT_QUANTA);
        }

        public static ushort PackEndCut(float endCut)
        {
            return (ushort)(50000 - (ushort)Math.Round(endCut / CUT_QUANTA));
        }

        public static byte PackPathScale(float pathScale)
        {
            return (byte)(200 - (byte)Math.Round(pathScale / SCALE_QUANTA));
        }

        public static sbyte PackPathShear(float pathShear)
        {
            return (sbyte)Math.Round(pathShear / SHEAR_QUANTA);
        }

        /// <summary>
        /// Packs PathTwist, PathTwistBegin, PathRadiusOffset, and PathSkew
        /// parameters in to signed eight bit values
        /// </summary>
        /// <param name="pathTwist">Floating point parameter to pack</param>
        /// <returns>Signed eight bit value containing the packed parameter</returns>
        public static sbyte PackPathTwist(float pathTwist)
        {
            return (sbyte)Math.Round(pathTwist / SCALE_QUANTA);
        }

        public static sbyte PackPathTaper(float pathTaper)
        {
            return (sbyte)Math.Round(pathTaper / TAPER_QUANTA);
        }

        public static byte PackPathRevolutions(float pathRevolutions)
        {
            return (byte)Math.Round((pathRevolutions - 1f) / REV_QUANTA);
        }

        public static ushort PackProfileHollow(float profileHollow)
        {
            return (ushort)Math.Round(profileHollow / HOLLOW_QUANTA);
        }

        #endregion Parameter Packing Methods

        #region Parameter Unpacking Methods

        public static float UnpackBeginCut(ushort beginCut)
        {
            return (float)beginCut * CUT_QUANTA;
        }

        public static float UnpackEndCut(ushort endCut)
        {
            return (float)(50000 - endCut) * CUT_QUANTA;
        }

        public static float UnpackPathScale(byte pathScale)
        {
            return (float)(200 - pathScale) * SCALE_QUANTA;
        }

        public static float UnpackPathShear(sbyte pathShear)
        {
            return (float)pathShear * SHEAR_QUANTA;
        }

        /// <summary>
        /// Unpacks PathTwist, PathTwistBegin, PathRadiusOffset, and PathSkew
        /// parameters from signed eight bit integers to floating point values
        /// </summary>
        /// <param name="pathTwist">Signed eight bit value to unpack</param>
        /// <returns>Unpacked floating point value</returns>
        public static float UnpackPathTwist(sbyte pathTwist)
        {
            return (float)pathTwist * SCALE_QUANTA;
        }

        public static float UnpackPathTaper(sbyte pathTaper)
        {
            return (float)pathTaper * TAPER_QUANTA;
        }

        public static float UnpackPathRevolutions(byte pathRevolutions)
        {
            return (float)pathRevolutions * REV_QUANTA + 1f;
        }

        public static float UnpackProfileHollow(ushort profileHollow)
        {
            return (float)profileHollow * HOLLOW_QUANTA;
        }

        #endregion Parameter Unpacking Methods
    }
}
