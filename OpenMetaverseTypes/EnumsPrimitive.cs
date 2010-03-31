/*
 * Copyright (c) 2009, openmetaverse.org
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
        [EnumInfo(Text = "Default")]
        Default = 0,
        /// <summary>Chest</summary>
        [EnumInfo(Text = "Chest")]
        Chest = 1,
        /// <summary>Skull</summary>
        [EnumInfo(Text = "Head")]
        Skull,
        /// <summary>Left shoulder</summary>
        [EnumInfo(Text = "Left Shoulder")]
        LeftShoulder,
        /// <summary>Right shoulder</summary>
        [EnumInfo(Text = "Right Shoulder")]
        RightShoulder,
        /// <summary>Left hand</summary>
        [EnumInfo(Text = "Left Hand")]
        LeftHand,
        /// <summary>Right hand</summary>
        [EnumInfo(Text = "Right Hand")]
        RightHand,
        /// <summary>Left foot</summary>
        [EnumInfo(Text = "Left Foot")]
        LeftFoot,
        /// <summary>Right foot</summary>
        [EnumInfo(Text = "Right Foot")]
        RightFoot,
        /// <summary>Spine</summary>
        [EnumInfo(Text = "Back")]
        Spine,
        /// <summary>Pelvis</summary>
        [EnumInfo(Text = "Pelvis")]
        Pelvis,
        /// <summary>Mouth</summary>
        [EnumInfo(Text = "Mouth")]
        Mouth,
        /// <summary>Chin</summary>
        [EnumInfo(Text = "Chin")]
        Chin,
        /// <summary>Left ear</summary>
        [EnumInfo(Text = "Left Ear")]
        LeftEar,
        /// <summary>Right ear</summary>
        [EnumInfo(Text = "Right Ear")]
        RightEar,
        /// <summary>Left eyeball</summary>
        [EnumInfo(Text = "Left Eye")]
        LeftEyeball,
        /// <summary>Right eyeball</summary>
        [EnumInfo(Text = "Right Eye")]
        RightEyeball,
        /// <summary>Nose</summary>
        [EnumInfo(Text = "Nose")]
        Nose,
        /// <summary>Right upper arm</summary>
        [EnumInfo(Text = "Right Upper Arm")]
        RightUpperArm,
        /// <summary>Right forearm</summary>
        [EnumInfo(Text = "Right Lower Arm")]
        RightForearm,
        /// <summary>Left upper arm</summary>
        [EnumInfo(Text = "Left Upper Arm")]
        LeftUpperArm,
        /// <summary>Left forearm</summary>
        [EnumInfo(Text = "Left Lower Arm")]
        LeftForearm,
        /// <summary>Right hip</summary>
        [EnumInfo(Text = "Right Hip")]
        RightHip,
        /// <summary>Right upper leg</summary>
        [EnumInfo(Text = "Right Upper Leg")]
        RightUpperLeg,
        /// <summary>Right lower leg</summary>
        [EnumInfo(Text = "Right Lower Leg")]
        RightLowerLeg,
        /// <summary>Left hip</summary>
        [EnumInfo(Text = "Left Hip")]
        LeftHip,
        /// <summary>Left upper leg</summary>
        [EnumInfo(Text = "Left Hip")]
        LeftUpperLeg,
        /// <summary>Left lower leg</summary>
        [EnumInfo(Text = "Left Lower Leg")]
        LeftLowerLeg,
        /// <summary>Stomach</summary>
        [EnumInfo(Text = "Belly")]
        Stomach,
        /// <summary>Left pectoral</summary>
        [EnumInfo(Text = "Left Pec")]
        LeftPec,
        /// <summary>Right pectoral</summary>
        [EnumInfo(Text = "Right Pec")]
        RightPec,
        /// <summary>HUD Center position 2</summary>
        [EnumInfo(Text = "HUD Center 2")]
        HUDCenter2,
        /// <summary>HUD Top-right</summary>
        [EnumInfo(Text = "HUD Top Right")]
        HUDTopRight,
        /// <summary>HUD Top</summary>
        [EnumInfo(Text = "HUD Top Center")]
        HUDTop,
        /// <summary>HUD Top-left</summary>
        [EnumInfo(Text = "HUD Top Left")]
        HUDTopLeft,
        /// <summary>HUD Center</summary>
        [EnumInfo(Text = "HUD Center 1")]
        HUDCenter,
        /// <summary>HUD Bottom-left</summary>
        [EnumInfo(Text = "HUD Bottom Left")]
        HUDBottomLeft,
        /// <summary>HUD Bottom</summary>
        [EnumInfo(Text = "HUD Bottom")]
        HUDBottom,
        /// <summary>HUD Bottom-right</summary>
        [EnumInfo(Text = "HUD Bottom Right")]
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
}