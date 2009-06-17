/*
 * Copyright (c) 2006-2008, openmetaverse.org
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
using System.Collections.Generic;
using OpenMetaverse.StructuredData;

namespace OpenMetaverse
{
    #region Enums

    /// <summary>
    /// Identifier code for primitive types
    /// </summary>
    public enum PCode : byte
    {
        /// <summary>None</summary>
        None = 0,
        /// <summary>A Primitive</summary>
        Prim = 9,
        /// <summary>A Avatar</summary>
        Avatar = 47,
        /// <summary>Linden grass</summary>
        Grass = 95,
        /// <summary>Linden tree</summary>
        NewTree = 111,
        /// <summary>A primitive that acts as the source for a particle stream</summary>
        ParticleSystem = 143,
        /// <summary>A Linden tree</summary>
        Tree = 255
    }

    /// <summary>
    /// Primary parameters for primitives such as Physics Enabled or Phantom
    /// </summary>
    [Flags]
    public enum PrimFlags : uint
    {
        /// <summary>Deprecated</summary>
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
        /// <summary>Server flag, will not be sent to clients. Specifies that
        /// the object is destroyed when it touches a simulator edge</summary>
        DieAtEdge = 0x01000000,
        /// <summary>Server flag, will not be sent to clients. Specifies that
        /// the object will be returned to the owner's inventory when it
        /// touches a simulator edge</summary>
        ReturnAtEdge = 0x02000000,
        /// <summary>Server flag, will not be sent to clients.</summary>
        Sandbox = 0x04000000,
        /// <summary>Server flag, will not be sent to client. Specifies that
        /// the object is hovering/flying</summary>
        Flying = 0x08000000,
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
    /// Sound flags for sounds attached to primitives
    /// </summary>
    [Flags]
    public enum SoundFlags : byte
    {
        /// <summary></summary>
        None = 0,
        /// <summary></summary>
        Loop = 0x01,
        /// <summary></summary>
        SyncMaster = 0x02,
        /// <summary></summary>
        SyncSlave = 0x04,
        /// <summary></summary>
        SyncPending = 0x08,
        /// <summary></summary>
        Queue = 0x10,
        /// <summary></summary>
        Stop = 0x20
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
    public enum Material : byte
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

    /// <summary>
    /// Used in a helper function to roughly determine prim shape
    /// </summary>
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

    /// <summary>
    /// Extra parameters for primitives, these flags are for features that have
    /// been added after the original ObjectFlags that has all eight bits 
    /// reserved already
    /// </summary>
    [Flags]
    public enum ExtraParamType : ushort
    {
        /// <summary>Whether this object has flexible parameters</summary>
        Flexible = 0x10,
        /// <summary>Whether this object has light parameters</summary>
        Light = 0x20,
        /// <summary>Whether this object is a sculpted prim</summary>
        Sculpt = 0x30
    }

    /// <summary>
    /// 
    /// </summary>
    public enum JointType : byte
    {
        /// <summary></summary>
        Invalid = 0,
        /// <summary></summary>
        Hinge = 1,
        /// <summary></summary>
        Point = 2,
        // <summary></summary>
        //[Obsolete]
        //LPoint = 3,
        //[Obsolete]
        //Wheel = 4
    }

    /// <summary>
    /// 
    /// </summary>
    public enum SculptType : byte
    {
        /// <summary></summary>
        None = 0,
        /// <summary></summary>
        Sphere = 1,
        /// <summary></summary>
        Torus = 2,
        /// <summary></summary>
        Plane = 3,
        /// <summary></summary>
        Cylinder = 4,
        /// <summary></summary>
        Invert = 64,
        /// <summary></summary>
        Mirror = 128
    }

    /// <summary>
    /// 
    /// </summary>
    public enum FaceType : ushort
    {
        /// <summary></summary>
        PathBegin = 0x1 << 0,
        /// <summary></summary>
        PathEnd = 0x1 << 1,
        /// <summary></summary>
        InnerSide = 0x1 << 2,
        /// <summary></summary>
        ProfileBegin = 0x1 << 3,
        /// <summary></summary>
        ProfileEnd = 0x1 << 4,
        /// <summary></summary>
        OuterSide0 = 0x1 << 5,
        /// <summary></summary>
        OuterSide1 = 0x1 << 6,
        /// <summary></summary>
        OuterSide2 = 0x1 << 7,
        /// <summary></summary>
        OuterSide3 = 0x1 << 8
    }

    /// <summary>
    /// 
    /// </summary>
    public enum ObjectCategory
    {
        /// <summary></summary>
        Invalid = -1,
        /// <summary></summary>
        None = 0,
        /// <summary></summary>
        Owner,
        /// <summary></summary>
        Group,
        /// <summary></summary>
        Other,
        /// <summary></summary>
        Selected,
        /// <summary></summary>
        Temporary
    }

    /// <summary>
    /// Attachment points for objects on avatar bodies
    /// </summary>
    /// <remarks>
    /// Both InventoryObject and InventoryAttachment types can be attached
    ///</remarks>
    public enum AttachmentPoint : byte
    {
        /// <summary>Right hand if object was not previously attached</summary>
        Default = 0,
        /// <summary>Chest</summary>
        Chest = 1,
        /// <summary>Skull</summary>
        Skull,
        /// <summary>Left shoulder</summary>
        LeftShoulder,
        /// <summary>Right shoulder</summary>
        RightShoulder,
        /// <summary>Left hand</summary>
        LeftHand,
        /// <summary>Right hand</summary>
        RightHand,
        /// <summary>Left foot</summary>
        LeftFoot,
        /// <summary>Right foot</summary>
        RightFoot,
        /// <summary>Spine</summary>
        Spine,
        /// <summary>Pelvis</summary>
        Pelvis,
        /// <summary>Mouth</summary>
        Mouth,
        /// <summary>Chin</summary>
        Chin,
        /// <summary>Left ear</summary>
        LeftEar,
        /// <summary>Right ear</summary>
        RightEar,
        /// <summary>Left eyeball</summary>
        LeftEyeball,
        /// <summary>Right eyeball</summary>
        RightEyeball,
        /// <summary>Nose</summary>
        Nose,
        /// <summary>Right upper arm</summary>
        RightUpperArm,
        /// <summary>Right forearm</summary>
        RightForearm,
        /// <summary>Left upper arm</summary>
        LeftUpperArm,
        /// <summary>Left forearm</summary>
        LeftForearm,
        /// <summary>Right hip</summary>
        RightHip,
        /// <summary>Right upper leg</summary>
        RightUpperLeg,
        /// <summary>Right lower leg</summary>
        RightLowerLeg,
        /// <summary>Left hip</summary>
        LeftHip,
        /// <summary>Left upper leg</summary>
        LeftUpperLeg,
        /// <summary>Left lower leg</summary>
        LeftLowerLeg,
        /// <summary>Stomach</summary>
        Stomach,
        /// <summary>Left pectoral</summary>
        LeftPec,
        /// <summary>Right pectoral</summary>
        RightPec,
        /// <summary>HUD Center position 2</summary>
        HUDCenter2,
        /// <summary>HUD Top-right</summary>
        HUDTopRight,
        /// <summary>HUD Top</summary>
        HUDTop,
        /// <summary>HUD Top-left</summary>
        HUDTopLeft,
        /// <summary>HUD Center</summary>
        HUDCenter,
        /// <summary>HUD Bottom-left</summary>
        HUDBottomLeft,
        /// <summary>HUD Bottom</summary>
        HUDBottom,
        /// <summary>HUD Bottom-right</summary>
        HUDBottomRight
    }

    /// <summary>
    /// Tree foliage types
    /// </summary>
    public enum Tree : byte
    {
        /// <summary>Pine1 tree</summary>
        Pine1 = 0,
        /// <summary>Oak tree</summary>
        Oak,
        /// <summary>Tropical Bush1</summary>
        TropicalBush1,
        /// <summary>Palm1 tree</summary>
        Palm1,
        /// <summary>Dogwood tree</summary>
        Dogwood,
        /// <summary>Tropical Bush2</summary>
        TropicalBush2,
        /// <summary>Palm2 tree</summary>
        Palm2,
        /// <summary>Cypress1 tree</summary>
        Cypress1,
        /// <summary>Cypress2 tree</summary>
        Cypress2,
        /// <summary>Pine2 tree</summary>
        Pine2,
        /// <summary>Plumeria</summary>
        Plumeria,
        /// <summary>Winter pinetree1</summary>
        WinterPine1,
        /// <summary>Winter Aspen tree</summary>
        WinterAspen,
        /// <summary>Winter pinetree2</summary>
        WinterPine2,
        /// <summary>Eucalyptus tree</summary>
        Eucalyptus,
        /// <summary>Fern</summary>
        Fern,
        /// <summary>Eelgrass</summary>
        Eelgrass,
        /// <summary>Sea Sword</summary>
        SeaSword,
        /// <summary>Kelp1 plant</summary>
        Kelp1,
        /// <summary>Beach grass</summary>
        BeachGrass1,
        /// <summary>Kelp2 plant</summary>
        Kelp2
    }

    /// <summary>
    /// Grass foliage types
    /// </summary>
    public enum Grass : byte
    {
        /// <summary></summary>
        Grass0 = 0,
        /// <summary></summary>
        Grass1,
        /// <summary></summary>
        Grass2,
        /// <summary></summary>
        Grass3,
        /// <summary></summary>
        Grass4,
        /// <summary></summary>
        Undergrowth1
    }

    /// <summary>
    /// Action associated with clicking on an object
    /// </summary>
    public enum ClickAction : byte
    {
        /// <summary>Touch object</summary>
        Touch = 0,
        /// <summary>Sit on object</summary>
        Sit = 1,
        /// <summary>Purchase object or contents</summary>
        Buy = 2,
        /// <summary>Pay the object</summary>
        Pay = 3,
        /// <summary>Open task inventory</summary>
        OpenTask = 4,
        /// <summary>Play parcel media</summary>
        PlayMedia = 5,
        /// <summary>Open parcel media</summary>
        OpenMedia = 6
    }

    #endregion Enums

    public partial class Primitive : IEquatable<Primitive>
    {
        // Used for packing and unpacking parameters
        protected const float CUT_QUANTA = 0.00002f;
        protected const float SCALE_QUANTA = 0.01f;
        protected const float SHEAR_QUANTA = 0.01f;
        protected const float TAPER_QUANTA = 0.01f;
        protected const float REV_QUANTA = 0.015f;
        protected const float HOLLOW_QUANTA = 0.00002f;

        #region Subclasses

        /// <summary>
        /// Parameters used to construct a visual representation of a primitive
        /// </summary>
        public struct ConstructionData
        {
            private const byte PROFILE_MASK = 0x0F;
            private const byte HOLE_MASK = 0xF0;

            /// <summary></summary>
            public byte profileCurve;
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
            public Material Material;
            /// <summary></summary>
            public byte State;
            /// <summary></summary>
            public PCode PCode;

            #region Properties

            /// <summary>Attachment point to an avatar</summary>
            public AttachmentPoint AttachmentPoint
            {
                get { return (AttachmentPoint)Utils.SwapWords(State); }
                set { State = (byte)Utils.SwapWords((byte)value); }
            }

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
            public Vector2 PathBeginScale
            {
                get
                {
                    Vector2 begin = new Vector2(1f, 1f);
                    if (PathScaleX > 1f)
                        begin.X = 2f - PathScaleX;
                    if (PathScaleY > 1f)
                        begin.Y = 2f - PathScaleY;
                    return begin;
                }
            }

            /// <summary></summary>
            public Vector2 PathEndScale
            {
                get
                {
                    Vector2 end = new Vector2(1f, 1f);
                    if (PathScaleX < 1f)
                        end.X = PathScaleX;
                    if (PathScaleY < 1f)
                        end.Y = PathScaleY;
                    return end;
                }
            }

            #endregion Properties
        }

        /// <summary>
        /// Information on the flexible properties of a primitive
        /// </summary>
        public class FlexibleData
        {
            /// <summary></summary>
            public int Softness;
            /// <summary></summary>
            public float Gravity;
            /// <summary></summary>
            public float Drag;
            /// <summary></summary>
            public float Wind;
            /// <summary></summary>
            public float Tension;
            /// <summary></summary>
            public Vector3 Force;

            /// <summary>
            /// Default constructor
            /// </summary>
            public FlexibleData()
            {
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="data"></param>
            /// <param name="pos"></param>
            public FlexibleData(byte[] data, int pos)
            {
                if (data.Length >= 5)
                {
                    Softness = ((data[pos] & 0x80) >> 6) | ((data[pos + 1] & 0x80) >> 7);

                    Tension = (float)(data[pos++] & 0x7F) / 10.0f;
                    Drag = (float)(data[pos++] & 0x7F) / 10.0f;
                    Gravity = (float)(data[pos++] / 10.0f) - 10.0f;
                    Wind = (float)data[pos++] / 10.0f;
                    Force = new Vector3(data, pos);
                }
                else
                {
                    Softness = 0;

                    Tension = 0.0f;
                    Drag = 0.0f;
                    Gravity = 0.0f;
                    Wind = 0.0f;
                    Force = Vector3.Zero;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public byte[] GetBytes()
            {
                byte[] data = new byte[16];
                int i = 0;

                // Softness is packed in the upper bits of tension and drag
                data[i] = (byte)((Softness & 2) << 6);
                data[i + 1] = (byte)((Softness & 1) << 7);

                data[i++] |= (byte)((byte)(Tension * 10.01f) & 0x7F);
                data[i++] |= (byte)((byte)(Drag * 10.01f) & 0x7F);
                data[i++] = (byte)((Gravity + 10.0f) * 10.01f);
                data[i++] = (byte)(Wind * 10.01f);

                Force.GetBytes().CopyTo(data, i);

                return data;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public OSD GetOSD()
            {
                OSDMap map = new OSDMap();

                map["simulate_lod"] = OSD.FromInteger(Softness);
                map["gravity"] = OSD.FromReal(Gravity);
                map["air_friction"] = OSD.FromReal(Drag);
                map["wind_sensitivity"] = OSD.FromReal(Wind);
                map["tension"] = OSD.FromReal(Tension);
                map["user_force"] = OSD.FromVector3(Force);

                return map;
            }

            public static FlexibleData FromOSD(OSD osd)
            {
                FlexibleData flex = new FlexibleData();

                if (osd.Type == OSDType.Map)
                {
                    OSDMap map = (OSDMap)osd;

                    flex.Softness = map["simulate_lod"].AsInteger();
                    flex.Gravity = (float)map["gravity"].AsReal();
                    flex.Drag = (float)map["air_friction"].AsReal();
                    flex.Wind = (float)map["wind_sensitivity"].AsReal();
                    flex.Tension = (float)map["tension"].AsReal();
                    flex.Force = ((OSDArray)map["user_force"]).AsVector3();
                }

                return flex;
            }

            public override int GetHashCode()
            {
                return
                    Softness.GetHashCode() ^
                    Gravity.GetHashCode() ^
                    Drag.GetHashCode() ^
                    Wind.GetHashCode() ^
                    Tension.GetHashCode() ^
                    Force.GetHashCode();
            }
        }

        /// <summary>
        /// Information on the light properties of a primitive
        /// </summary>
        public class LightData
        {
            /// <summary></summary>
            public Color4 Color;
            /// <summary></summary>
            public float Intensity;
            /// <summary></summary>
            public float Radius;
            /// <summary></summary>
            public float Cutoff;
            /// <summary></summary>
            public float Falloff;

            /// <summary>
            /// Default constructor
            /// </summary>
            public LightData()
            {
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="data"></param>
            /// <param name="pos"></param>
            public LightData(byte[] data, int pos)
            {
                if (data.Length - pos >= 16)
                {
                    Color = new Color4(data, pos, false);
                    Radius = Utils.BytesToFloat(data, pos + 4);
                    Cutoff = Utils.BytesToFloat(data, pos + 8);
                    Falloff = Utils.BytesToFloat(data, pos + 12);

                    // Alpha in color is actually intensity
                    Intensity = Color.A;
                    Color.A = 1f;
                }
                else
                {
                    Color = Color4.Black;
                    Radius = 0f;
                    Cutoff = 0f;
                    Falloff = 0f;
                    Intensity = 0f;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public byte[] GetBytes()
            {
                byte[] data = new byte[16];

                // Alpha channel in color is intensity
                Color4 tmpColor = Color;
                tmpColor.A = Intensity;
                tmpColor.GetBytes().CopyTo(data, 0);
                Utils.FloatToBytes(Radius).CopyTo(data, 4);
                Utils.FloatToBytes(Cutoff).CopyTo(data, 8);
                Utils.FloatToBytes(Falloff).CopyTo(data, 12);

                return data;
            }

            public OSD GetOSD()
            {
                OSDMap map = new OSDMap();

                map["color"] = OSD.FromColor4(Color);
                map["intensity"] = OSD.FromReal(Intensity);
                map["radius"] = OSD.FromReal(Radius);
                map["cutoff"] = OSD.FromReal(Cutoff);
                map["falloff"] = OSD.FromReal(Falloff);

                return map;
            }

            public static LightData FromOSD(OSD osd)
            {
                LightData light = new LightData();

                if (osd.Type == OSDType.Map)
                {
                    OSDMap map = (OSDMap)osd;

                    light.Color = ((OSDArray)map["color"]).AsColor4();
                    light.Intensity = (float)map["intensity"].AsReal();
                    light.Radius = (float)map["radius"].AsReal();
                    light.Cutoff = (float)map["cutoff"].AsReal();
                    light.Falloff = (float)map["falloff"].AsReal();
                }

                return light;
            }

            public override int GetHashCode()
            {
                return
                    Color.GetHashCode() ^
                    Intensity.GetHashCode() ^
                    Radius.GetHashCode() ^
                    Cutoff.GetHashCode() ^
                    Falloff.GetHashCode();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return String.Format("Color: {0} Intensity: {1} Radius: {2} Cutoff: {3} Falloff: {4}",
                    Color, Intensity, Radius, Cutoff, Falloff);
            }
        }

        /// <summary>
        /// Information on the sculpt properties of a sculpted primitive
        /// </summary>
        public class SculptData
        {
            public UUID SculptTexture;
            private byte type;

            public SculptType Type
            {
                get { return (SculptType)(type & 7); }
                set { type = (byte)value; }
            }

            /// <summary>
            /// Render inside out (inverts the normals).
            /// </summary>
            public bool Invert
            {
                get { return ((type & (byte)SculptType.Invert) != 0); }
            }

            /// <summary>
            /// Render an X axis mirror of the sculpty.
            /// </summary>
            public bool Mirror
            {
                get { return ((type & (byte)SculptType.Mirror) != 0); }
            }            

            /// <summary>
            /// Default constructor
            /// </summary>
            public SculptData()
            {
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="data"></param>
            /// <param name="pos"></param>
            public SculptData(byte[] data, int pos)
            {
                if (data.Length >= 17)
                {
                    SculptTexture = new UUID(data, pos);
                    type = data[pos + 16];
                }
                else
                {
                    SculptTexture = UUID.Zero;
                    type = (byte)SculptType.None;
                }
            }

            public byte[] GetBytes()
            {
                byte[] data = new byte[17];

                SculptTexture.GetBytes().CopyTo(data, 0);
                data[16] = (byte)Type;

                return data;
            }

            public OSD GetOSD()
            {
                OSDMap map = new OSDMap();

                map["texture"] = OSD.FromUUID(SculptTexture);
                map["type"] = OSD.FromInteger(type);

                return map;
            }

            public static SculptData FromOSD(OSD osd)
            {
                SculptData sculpt = new SculptData();

                if (osd.Type == OSDType.Map)
                {
                    OSDMap map = (OSDMap)osd;

                    sculpt.SculptTexture = map["texture"].AsUUID();
                    sculpt.type = (byte)map["type"].AsInteger();
                }

                return sculpt;
            }

            public override int GetHashCode()
            {
                return SculptTexture.GetHashCode() ^ type.GetHashCode();
            }
        }

        /// <summary>
        /// Extended properties to describe an object
        /// </summary>
        public class ObjectProperties
        {
            /// <summary></summary>
            public UUID ObjectID;
            /// <summary></summary>
            public UUID CreatorID;
            /// <summary></summary>
            public UUID OwnerID;
            /// <summary></summary>
            public UUID GroupID;
            /// <summary></summary>
            public DateTime CreationDate;
            /// <summary></summary>
            public Permissions Permissions;
            /// <summary></summary>
            public int OwnershipCost;
            /// <summary></summary>
            public SaleType SaleType;
            /// <summary></summary>
            public int SalePrice;
            /// <summary></summary>
            public byte AggregatePerms;
            /// <summary></summary>
            public byte AggregatePermTextures;
            /// <summary></summary>
            public byte AggregatePermTexturesOwner;
            /// <summary></summary>
            public ObjectCategory Category;
            /// <summary></summary>
            public short InventorySerial;
            /// <summary></summary>
            public UUID ItemID;
            /// <summary></summary>
            public UUID FolderID;
            /// <summary></summary>
            public UUID FromTaskID;
            /// <summary></summary>
            public UUID LastOwnerID;
            /// <summary></summary>
            public string Name;
            /// <summary></summary>
            public string Description;
            /// <summary></summary>
            public string TouchName;
            /// <summary></summary>
            public string SitName;
            /// <summary></summary>
            public UUID[] TextureIDs;

            /// <summary>
            /// Default constructor
            /// </summary>
            public ObjectProperties()
            {
                Name = String.Empty;
                Description = String.Empty;
                TouchName = String.Empty;
                SitName = String.Empty;
            }

            /// <summary>
            /// Set the properties that are set in an ObjectPropertiesFamily packet
            /// </summary>
            /// <param name="props"><seealso cref="ObjectProperties"/> that has
            /// been partially filled by an ObjectPropertiesFamily packet</param>
            public void SetFamilyProperties(ObjectProperties props)
            {
                ObjectID = props.ObjectID;
                OwnerID = props.OwnerID;
                GroupID = props.GroupID;
                Permissions = props.Permissions;
                OwnershipCost = props.OwnershipCost;
                SaleType = props.SaleType;
                SalePrice = props.SalePrice;
                Category = props.Category;
                LastOwnerID = props.LastOwnerID;
                Name = props.Name;
                Description = props.Description;
            }

            public byte[] GetTextureIDBytes()
            {
                if (TextureIDs == null || TextureIDs.Length == 0)
                    return Utils.EmptyBytes;

                byte[] bytes = new byte[16 * TextureIDs.Length];
                for (int i = 0; i < TextureIDs.Length; i++)
                    TextureIDs[i].ToBytes(bytes, 16 * i);

                return bytes;
            }
        }

        #endregion Subclasses

        #region Public Members

        /// <summary></summary>
        public UUID ID;
        /// <summary></summary>
        public UUID GroupID;
        /// <summary></summary>
        public uint LocalID;
        /// <summary></summary>
        public uint ParentID;
        /// <summary></summary>
        public ulong RegionHandle;
        /// <summary></summary>
        public PrimFlags Flags;
        /// <summary>Foliage type for this primitive. Only applicable if this
        /// primitive is foliage</summary>
        public Tree TreeSpecies;
        /// <summary>Unknown</summary>
        public byte[] ScratchPad;
        /// <summary></summary>
        public Vector3 Position;
        /// <summary></summary>
        public Vector3 Scale;
        /// <summary></summary>
        public Quaternion Rotation = Quaternion.Identity;
        /// <summary></summary>
        public Vector3 Velocity;
        /// <summary></summary>
        public Vector3 AngularVelocity;
        /// <summary></summary>
        public Vector3 Acceleration;
        /// <summary></summary>
        public Vector4 CollisionPlane;
        /// <summary></summary>
        public FlexibleData Flexible;
        /// <summary></summary>
        public LightData Light;
        /// <summary></summary>
        public SculptData Sculpt;
        /// <summary></summary>
        public ClickAction ClickAction;
        /// <summary></summary>
        public UUID Sound;
        /// <summary>Identifies the owner if audio or a particle system is
        /// active</summary>
        public UUID OwnerID;
        /// <summary></summary>
        public SoundFlags SoundFlags;
        /// <summary></summary>
        public float SoundGain;
        /// <summary></summary>
        public float SoundRadius;
        /// <summary></summary>
        public string Text;
        /// <summary></summary>
        public Color4 TextColor;
        /// <summary></summary>
        public string MediaURL;
        /// <summary></summary>
        public JointType Joint;
        /// <summary></summary>
        public Vector3 JointPivot;
        /// <summary></summary>
        public Vector3 JointAxisOrAnchor;
        /// <summary></summary>
        public NameValue[] NameValues;
        /// <summary></summary>
        public ConstructionData PrimData;
        /// <summary></summary>
        public ObjectProperties Properties;

        #endregion Public Members

        #region Properties

        /// <summary>Uses basic heuristics to estimate the primitive shape</summary>
        public PrimType Type
        {
            get
            {
                if (Sculpt != null && Sculpt.Type != SculptType.None)
                    return PrimType.Sculpt;

                bool linearPath = (PrimData.PathCurve == PathCurve.Line || PrimData.PathCurve == PathCurve.Flexible);
                float scaleY = PrimData.PathScaleY;

                if (linearPath)
                {
                    switch (PrimData.ProfileCurve)
                    {
                        case ProfileCurve.Circle:
                            return PrimType.Cylinder;
                        case ProfileCurve.Square:
                            return PrimType.Box;
                        case ProfileCurve.IsoTriangle:
                        case ProfileCurve.EqualTriangle:
                        case ProfileCurve.RightTriangle:
                            return PrimType.Prism;
                        case ProfileCurve.HalfCircle:
                        default:
                            return PrimType.Unknown;
                    }
                }
                else
                {
                    switch (PrimData.PathCurve)
                    {
                        case PathCurve.Flexible:
                            return PrimType.Unknown;
                        case PathCurve.Circle:
                            switch (PrimData.ProfileCurve)
                            {
                                case ProfileCurve.Circle:
                                    if (scaleY > 0.75f)
                                        return PrimType.Sphere;
                                    else
                                        return PrimType.Torus;
                                case ProfileCurve.HalfCircle:
                                    return PrimType.Sphere;
                                case ProfileCurve.EqualTriangle:
                                    return PrimType.Ring;
                                case ProfileCurve.Square:
                                    if (scaleY <= 0.75f)
                                        return PrimType.Tube;
                                    else
                                        return PrimType.Unknown;
                                default:
                                    return PrimType.Unknown;
                            }
                        case PathCurve.Circle2:
                            if (PrimData.ProfileCurve == ProfileCurve.Circle)
                                return PrimType.Sphere;
                            else
                                return PrimType.Unknown;
                        default:
                            return PrimType.Unknown;
                    }
                }
            }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public Primitive()
        {
            // Default a few null property values to String.Empty
            Text = String.Empty;
            MediaURL = String.Empty;
        }

        public Primitive(Primitive prim)
        {
            ID = prim.ID;
            GroupID = prim.GroupID;
            LocalID = prim.LocalID;
            ParentID = prim.ParentID;
            RegionHandle = prim.RegionHandle;
            Flags = prim.Flags;
            TreeSpecies = prim.TreeSpecies;
            if (prim.ScratchPad != null)
            {
                ScratchPad = new byte[prim.ScratchPad.Length];
                Buffer.BlockCopy(prim.ScratchPad, 0, ScratchPad, 0, ScratchPad.Length);
            }
            else
                ScratchPad = null;
            Position = prim.Position;
            Scale = prim.Scale;
            Rotation = prim.Rotation;
            Velocity = prim.Velocity;
            AngularVelocity = prim.AngularVelocity;
            Acceleration = prim.Acceleration;
            CollisionPlane = prim.CollisionPlane;
            Flexible = prim.Flexible;
            Light = prim.Light;
            Sculpt = prim.Sculpt;
            ClickAction = prim.ClickAction;
            Sound = prim.Sound;
            OwnerID = prim.OwnerID;
            SoundFlags = prim.SoundFlags;
            SoundGain = prim.SoundGain;
            SoundRadius = prim.SoundRadius;
            Text = prim.Text;
            TextColor = prim.TextColor;
            MediaURL = prim.MediaURL;
            Joint = prim.Joint;
            JointPivot = prim.JointPivot;
            JointAxisOrAnchor = prim.JointAxisOrAnchor;
            if (prim.NameValues != null)
            {
                if (NameValues == null || NameValues.Length != prim.NameValues.Length)
                    NameValues = new NameValue[prim.NameValues.Length];
                Array.Copy(prim.NameValues, NameValues, prim.NameValues.Length);
            }
            else
                NameValues = null;
            PrimData = prim.PrimData;
            Properties = prim.Properties;
            // FIXME: Get a real copy constructor for TextureEntry instead of serializing to bytes and back
            if (prim.Textures != null)
            {
                byte[] textureBytes = prim.Textures.GetBytes();
                Textures = new TextureEntry(textureBytes, 0, textureBytes.Length);
            }
            else
            {
                Textures = null;
            }
            TextureAnim = prim.TextureAnim;
            ParticleSys = prim.ParticleSys;
        }

        #endregion Constructors

        #region Public Methods

        public OSD GetOSD()
        {
            OSDMap path = new OSDMap(14);
            path["begin"] = OSD.FromReal(PrimData.PathBegin);
            path["curve"] = OSD.FromInteger((int)PrimData.PathCurve);
            path["end"] = OSD.FromReal(PrimData.PathEnd);
            path["radius_offset"] = OSD.FromReal(PrimData.PathRadiusOffset);
            path["revolutions"] = OSD.FromReal(PrimData.PathRevolutions);
            path["scale_x"] = OSD.FromReal(PrimData.PathScaleX);
            path["scale_y"] = OSD.FromReal(PrimData.PathScaleY);
            path["shear_x"] = OSD.FromReal(PrimData.PathShearX);
            path["shear_y"] = OSD.FromReal(PrimData.PathShearY);
            path["skew"] = OSD.FromReal(PrimData.PathSkew);
            path["taper_x"] = OSD.FromReal(PrimData.PathTaperX);
            path["taper_y"] = OSD.FromReal(PrimData.PathTaperY);
            path["twist"] = OSD.FromReal(PrimData.PathTwist);
            path["twist_begin"] = OSD.FromReal(PrimData.PathTwistBegin);

            OSDMap profile = new OSDMap(4);
            profile["begin"] = OSD.FromReal(PrimData.ProfileBegin);
            profile["curve"] = OSD.FromInteger((int)PrimData.ProfileCurve);
            profile["hole"] = OSD.FromInteger((int)PrimData.ProfileHole);
            profile["end"] = OSD.FromReal(PrimData.ProfileEnd);
            profile["hollow"] = OSD.FromReal(PrimData.ProfileHollow);

            OSDMap volume = new OSDMap(2);
            volume["path"] = path;
            volume["profile"] = profile;

            OSDMap prim = new OSDMap(9);
            if (Properties != null)
            {
                prim["name"] = OSD.FromString(Properties.Name);
                prim["description"] = OSD.FromString(Properties.Description);
            }
            else
            {
                prim["name"] = OSD.FromString("Object");
                prim["description"] = OSD.FromString(String.Empty);
            }
            prim["phantom"] = OSD.FromBoolean(((Flags & PrimFlags.Phantom) != 0));
            prim["physical"] = OSD.FromBoolean(((Flags & PrimFlags.Physics) != 0));
            prim["position"] = OSD.FromVector3(Position);
            prim["rotation"] = OSD.FromQuaternion(Rotation);
            prim["scale"] = OSD.FromVector3(Scale);
            prim["material"] = OSD.FromInteger((int)PrimData.Material);
            prim["shadows"] = OSD.FromBoolean(((Flags & PrimFlags.CastShadows) != 0));
            prim["textures"] = Textures.GetOSD();
            prim["volume"] = volume;
            if (ParentID != 0)
                prim["parentid"] = OSD.FromInteger(ParentID);

            if (Light != null)
                prim["light"] = Light.GetOSD();

            if (Flexible != null)
                prim["flex"] = Flexible.GetOSD();

            if (Sculpt != null)
                prim["sculpt"] = Sculpt.GetOSD();

            return prim;
        }

        public static Primitive FromOSD(OSD osd)
        {
            Primitive prim = new Primitive();
            Primitive.ConstructionData data;

            OSDMap map = (OSDMap)osd;
            OSDMap volume = (OSDMap)map["volume"];
            OSDMap path = (OSDMap)volume["path"];
            OSDMap profile = (OSDMap)volume["profile"];

            #region Path/Profile

            data.profileCurve = (byte)0;
            data.State = 0;
            data.Material = (Material)map["material"].AsInteger();
            data.PCode = PCode.Prim; // TODO: Put this in SD

            data.PathBegin = (float)path["begin"].AsReal();
            data.PathCurve = (PathCurve)path["curve"].AsInteger();
            data.PathEnd = (float)path["end"].AsReal();
            data.PathRadiusOffset = (float)path["radius_offset"].AsReal();
            data.PathRevolutions = (float)path["revolutions"].AsReal();
            data.PathScaleX = (float)path["scale_x"].AsReal();
            data.PathScaleY = (float)path["scale_y"].AsReal();
            data.PathShearX = (float)path["shear_x"].AsReal();
            data.PathShearY = (float)path["shear_y"].AsReal();
            data.PathSkew = (float)path["skew"].AsReal();
            data.PathTaperX = (float)path["taper_x"].AsReal();
            data.PathTaperY = (float)path["taper_y"].AsReal();
            data.PathTwist = (float)path["twist"].AsReal();
            data.PathTwistBegin = (float)path["twist_begin"].AsReal();

            data.ProfileBegin = (float)profile["begin"].AsReal();
            data.ProfileEnd = (float)profile["end"].AsReal();
            data.ProfileHollow = (float)profile["hollow"].AsReal();
            data.ProfileCurve = (ProfileCurve)profile["curve"].AsInteger();
            data.ProfileHole = (HoleType)profile["hole"].AsInteger();

            #endregion Path/Profile

            prim.PrimData = data;

            if (map["phantom"].AsBoolean())
                prim.Flags |= PrimFlags.Phantom;

            if (map["physical"].AsBoolean())
                prim.Flags |= PrimFlags.Physics;

            if (map["shadows"].AsBoolean())
                prim.Flags |= PrimFlags.CastShadows;

            prim.ParentID = (uint)map["parentid"].AsInteger();
            prim.Position = ((OSDArray)map["position"]).AsVector3();
            prim.Rotation = ((OSDArray)map["rotation"]).AsQuaternion();
            prim.Scale = ((OSDArray)map["scale"]).AsVector3();
            prim.Flexible = FlexibleData.FromOSD(map["flex"]);
            prim.Light = LightData.FromOSD(map["light"]);
            prim.Sculpt = SculptData.FromOSD(map["sculpt"]);
            prim.Textures = TextureEntry.FromOSD(map["textures"]);
            prim.Properties = new ObjectProperties();

            if (!string.IsNullOrEmpty(map["name"].AsString()))
            {
                prim.Properties.Name = map["name"].AsString();
            }

            if (!string.IsNullOrEmpty(map["description"].AsString()))
            {
                prim.Properties.Description = map["description"].AsString();
            }

            return prim;
        }

        public int SetExtraParamsFromBytes(byte[] data, int pos)
        {
            int i = pos;
            int totalLength = 1;

            Flexible = null;
            Light = null;
            Sculpt = null;

            if (data.Length == 0 || pos >= data.Length)
                return 0;

            byte extraParamCount = data[i++];

            for (int k = 0; k < extraParamCount; k++)
            {
                ExtraParamType type = (ExtraParamType)Utils.BytesToUInt16(data, i);
                i += 2;

                uint paramLength = Utils.BytesToUInt(data, i);
                i += 4;

                if (type == ExtraParamType.Flexible)
                    Flexible = new FlexibleData(data, i);
                else if (type == ExtraParamType.Light)
                    Light = new LightData(data, i);
                else if (type == ExtraParamType.Sculpt)
                    Sculpt = new SculptData(data, i);

                i += (int)paramLength;
                totalLength += (int)paramLength + 6;
            }

            return totalLength;
        }

        public byte[] GetExtraParamsBytes()
        {
            byte[] flexible = null;
            byte[] light = null;
            byte[] sculpt = null;
            byte[] buffer = null;
            int size = 1;
            int pos = 0;
            byte count = 0;

            if (Flexible != null)
            {
                flexible = Flexible.GetBytes();
                size += flexible.Length + 6;
                ++count;
            }
            if (Light != null)
            {
                light = Light.GetBytes();
                size += light.Length + 6;
                ++count;
            }
            if (Sculpt != null)
            {
                sculpt = Sculpt.GetBytes();
                size += sculpt.Length + 6;
                ++count;
            }

            buffer = new byte[size];
            buffer[0] = count;
            ++pos;

            if (flexible != null)
            {
                Buffer.BlockCopy(Utils.UInt16ToBytes((ushort)ExtraParamType.Flexible), 0, buffer, pos, 2);
                pos += 2;

                Buffer.BlockCopy(Utils.UIntToBytes((uint)flexible.Length), 0, buffer, pos, 4);
                pos += 4;

                Buffer.BlockCopy(flexible, 0, buffer, pos, flexible.Length);
                pos += flexible.Length;
            }
            if (light != null)
            {
                Buffer.BlockCopy(Utils.UInt16ToBytes((ushort)ExtraParamType.Light), 0, buffer, pos, 2);
                pos += 2;

                Buffer.BlockCopy(Utils.UIntToBytes((uint)light.Length), 0, buffer, pos, 4);
                pos += 4;

                Buffer.BlockCopy(light, 0, buffer, pos, light.Length);
                pos += light.Length;
            }
            if (sculpt != null)
            {
                Buffer.BlockCopy(Utils.UInt16ToBytes((ushort)ExtraParamType.Sculpt), 0, buffer, pos, 2);
                pos += 2;

                Buffer.BlockCopy(Utils.UIntToBytes((uint)sculpt.Length), 0, buffer, pos, 4);
                pos += 4;

                Buffer.BlockCopy(sculpt, 0, buffer, pos, sculpt.Length);
                pos += sculpt.Length;
            }

            return buffer;
        }

        #endregion Public Methods

        #region Overrides

        public override bool Equals(object obj)
        {
            return (obj is Primitive) ? this == (Primitive)obj : false;
        }

        public bool Equals(Primitive other)
        {
            return this == other;
        }

        public override string ToString()
        {
            switch (PrimData.PCode)
            {
                case PCode.Prim:
                    return String.Format("{0} ({1})", Type, ID);
                default:
                    return String.Format("{0} ({1})", PrimData.PCode, ID);
            }
        }

        public override int GetHashCode()
        {
            return
                Position.GetHashCode() ^
                Velocity.GetHashCode() ^
                Acceleration.GetHashCode() ^
                Rotation.GetHashCode() ^
                AngularVelocity.GetHashCode() ^
                ClickAction.GetHashCode() ^
                (Flexible != null ? Flexible.GetHashCode() : 0) ^
                (Light != null ? Light.GetHashCode() : 0) ^
                (Sculpt != null ? Sculpt.GetHashCode() : 0) ^
                Flags.GetHashCode() ^
                PrimData.Material.GetHashCode() ^
                MediaURL.GetHashCode() ^
                //TODO: NameValues?
                (Properties != null ? Properties.OwnerID.GetHashCode() : 0) ^
                ParentID.GetHashCode() ^
                PrimData.PathBegin.GetHashCode() ^
                PrimData.PathCurve.GetHashCode() ^
                PrimData.PathEnd.GetHashCode() ^
                PrimData.PathRadiusOffset.GetHashCode() ^
                PrimData.PathRevolutions.GetHashCode() ^
                PrimData.PathScaleX.GetHashCode() ^
                PrimData.PathScaleY.GetHashCode() ^
                PrimData.PathShearX.GetHashCode() ^
                PrimData.PathShearY.GetHashCode() ^
                PrimData.PathSkew.GetHashCode() ^
                PrimData.PathTaperX.GetHashCode() ^
                PrimData.PathTaperY.GetHashCode() ^
                PrimData.PathTwist.GetHashCode() ^
                PrimData.PathTwistBegin.GetHashCode() ^
                PrimData.PCode.GetHashCode() ^
                PrimData.ProfileBegin.GetHashCode() ^
                PrimData.ProfileCurve.GetHashCode() ^
                PrimData.ProfileEnd.GetHashCode() ^
                PrimData.ProfileHollow.GetHashCode() ^
                ParticleSys.GetHashCode() ^
                TextColor.GetHashCode() ^
                TextureAnim.GetHashCode() ^
                (Textures != null ? Textures.GetHashCode() : 0) ^
                SoundRadius.GetHashCode() ^
                Scale.GetHashCode() ^
                Sound.GetHashCode() ^
                PrimData.State.GetHashCode() ^
                Text.GetHashCode() ^
                TreeSpecies.GetHashCode();
        }

        #endregion Overrides

        #region Operators

        public static bool operator ==(Primitive lhs, Primitive rhs)
        {
            if ((Object)lhs == null || (Object)rhs == null)
            {
                return (Object)rhs == (Object)lhs;
            }
            return (lhs.ID == rhs.ID);
        }

        public static bool operator !=(Primitive lhs, Primitive rhs)
        {
            if ((Object)lhs == null || (Object)rhs == null)
            {
                return (Object)rhs != (Object)lhs;
            }
            return !(lhs.ID == rhs.ID);
        }

        #endregion Operators

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
