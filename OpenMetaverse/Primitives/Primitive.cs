/*
 * Copyright (c) 2006-2016, openmetaverse.co
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without 
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.co nor the names 
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

            /// <summary>
            /// Calculdates hash code for prim construction data
            /// </summary>
            /// <returns>The has</returns>
            public override int GetHashCode()
            {
                return profileCurve.GetHashCode()
                    ^ PathCurve.GetHashCode()
                    ^ PathEnd.GetHashCode()
                    ^ PathRadiusOffset.GetHashCode()
                    ^ PathSkew.GetHashCode()
                    ^ PathScaleX.GetHashCode()
                    ^ PathScaleY.GetHashCode()
                    ^ PathShearX.GetHashCode()
                    ^ PathShearY.GetHashCode()
                    ^ PathTaperX.GetHashCode()
                    ^ PathTaperY.GetHashCode()
                    ^ PathBegin.GetHashCode()
                    ^ PathTwist.GetHashCode()
                    ^ PathTwistBegin.GetHashCode()
                    ^ PathRevolutions.GetHashCode()
                    ^ ProfileBegin.GetHashCode()
                    ^ ProfileEnd.GetHashCode()
                    ^ ProfileHollow.GetHashCode()
                    ^ Material.GetHashCode()
                    ^ State.GetHashCode()
                    ^ PCode.GetHashCode();
            }
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
        /// Information on the light properties of a primitive as texture map
        /// </summary>
        public class LightImage
        {
            /// <summary></summary>
            public UUID LightTexture;
            /// <summary></summary>
            public Vector3 Params;

            /// <summary>
            /// Default constructor
            /// </summary>
            public LightImage()
            {
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="data"></param>
            /// <param name="pos"></param>
            public LightImage(byte[] data, int pos)
            {
                if (data.Length - pos >= 28)
                {
                    LightTexture = new UUID(data, pos);
                    Params = new Vector3(data, pos + 16);
                }
                else
                {
                    LightTexture = UUID.Zero;
                    Params = Vector3.Zero;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public byte[] GetBytes()
            {
                byte[] data = new byte[28];

                // Alpha channel in color is intensity
                LightTexture.ToBytes(data, 0);
                Params.ToBytes(data, 16);

                return data;
            }

            public OSD GetOSD()
            {
                OSDMap map = new OSDMap();

                map["texture"] = OSD.FromUUID(LightTexture);
                map["params"] = OSD.FromVector3(Params);

                return map;
            }

            public static LightImage FromOSD(OSD osd)
            {
                LightImage light = new LightImage();

                if (osd.Type == OSDType.Map)
                {
                    OSDMap map = (OSDMap)osd;

                    light.LightTexture = map["texture"].AsUUID();
                    light.Params = map["params"].AsVector3();
                }

                return light;
            }

            public override int GetHashCode()
            {
                return LightTexture.GetHashCode() ^ Params.GetHashCode();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return String.Format("LightTexture: {0} Params; {1]", LightTexture, Params);
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
                data[16] = type;

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

        /// <summary>
        /// Describes physics attributes of the prim
        /// </summary>
        public class PhysicsProperties
        {
            /// <summary>Primitive's local ID</summary>
            public uint LocalID;
            /// <summary>Density (1000 for normal density)</summary>
            public float Density;
            /// <summary>Friction</summary>
            public float Friction;
            /// <summary>Gravity multiplier (1 for normal gravity) </summary>
            public float GravityMultiplier;
            /// <summary>Type of physics representation of this primitive in the simulator</summary>
            public PhysicsShapeType PhysicsShapeType;
            /// <summary>Restitution</summary>
            public float Restitution;

            /// <summary>
            /// Creates PhysicsProperties from OSD
            /// </summary>
            /// <param name="osd">OSDMap with incoming data</param>
            /// <returns>Deserialized PhysicsProperties object</returns>
            public static PhysicsProperties FromOSD(OSD osd)
            {
                PhysicsProperties ret = new PhysicsProperties();

                if (osd is OSDMap)
                {
                    OSDMap map = (OSDMap)osd;
                    ret.LocalID = map["LocalID"];
                    ret.Density = map["Density"];
                    ret.Friction = map["Friction"];
                    ret.GravityMultiplier = map["GravityMultiplier"];
                    ret.Restitution = map["Restitution"];
                    ret.PhysicsShapeType = (PhysicsShapeType)map["PhysicsShapeType"].AsInteger();
                }

                return ret;
            }

            /// <summary>
            /// Serializes PhysicsProperties to OSD
            /// </summary>
            /// <returns>OSDMap with serialized PhysicsProperties data</returns>
            public OSD GetOSD()
            {
                OSDMap map = new OSDMap(6);
                map["LocalID"] = LocalID;
                map["Density"] = Density;
                map["Friction"] = Friction;
                map["GravityMultiplier"] = GravityMultiplier;
                map["Restitution"] = Restitution;
                map["PhysicsShapeType"] = (int)PhysicsShapeType;
                return map;
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
        public LightImage LightMap;
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
        /// <summary>Objects physics engine propertis</summary>
        public PhysicsProperties PhysicsProps;
        /// <summary>Extra data about primitive</summary>
        public object Tag;
        /// <summary>Indicates if prim is attached to an avatar</summary>
        public bool IsAttachment;
        /// <summary>Number of clients referencing this prim</summary>
        public int ActiveClients = 0;

        #endregion Public Members

        #region Properties

        /// <summary>Uses basic heuristics to estimate the primitive shape</summary>
        public PrimType Type
        {
            get
            {
                if (Sculpt != null && Sculpt.Type != SculptType.None && Sculpt.SculptTexture != UUID.Zero)
                {
                    if (Sculpt.Type == SculptType.Mesh)
                        return PrimType.Mesh;
                    else
                        return PrimType.Sculpt;
                }

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
                ScratchPad = Utils.EmptyBytes;
            Position = prim.Position;
            Scale = prim.Scale;
            Rotation = prim.Rotation;
            Velocity = prim.Velocity;
            AngularVelocity = prim.AngularVelocity;
            Acceleration = prim.Acceleration;
            CollisionPlane = prim.CollisionPlane;
            Flexible = prim.Flexible;
            Light = prim.Light;
            LightMap = prim.LightMap;
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

        public virtual OSD GetOSD()
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

            OSDMap prim = new OSDMap(20);
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
            prim["pcode"] = OSD.FromInteger((int)PrimData.PCode);
            prim["material"] = OSD.FromInteger((int)PrimData.Material);
            prim["shadows"] = OSD.FromBoolean(((Flags & PrimFlags.CastShadows) != 0));
            prim["state"] = OSD.FromInteger(PrimData.State);

            prim["id"] = OSD.FromUUID(ID);
            prim["localid"] = OSD.FromUInteger(LocalID);
            prim["parentid"] = OSD.FromUInteger(ParentID);

            prim["volume"] = volume;

            if (Textures != null)
                prim["textures"] = Textures.GetOSD();

            if ((TextureAnim.Flags & TextureAnimMode.ANIM_ON) != 0)
                prim["texture_anim"] = TextureAnim.GetOSD();

            if (Light != null)
                prim["light"] = Light.GetOSD();

            if (LightMap != null)
                prim["light_image"] = LightMap.GetOSD();

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
            data.Material = (Material)map["material"].AsInteger();
            data.PCode = (PCode)map["pcode"].AsInteger();
            data.State = (byte)map["state"].AsInteger();

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

            prim.ID = map["id"].AsUUID();
            prim.LocalID = map["localid"].AsUInteger();
            prim.ParentID = map["parentid"].AsUInteger();
            prim.Position = ((OSDArray)map["position"]).AsVector3();
            prim.Rotation = ((OSDArray)map["rotation"]).AsQuaternion();
            prim.Scale = ((OSDArray)map["scale"]).AsVector3();
            
            if (map["flex"])
                prim.Flexible = FlexibleData.FromOSD(map["flex"]);
            
            if (map["light"])
                prim.Light = LightData.FromOSD(map["light"]);

            if (map["light_image"])
                prim.LightMap = LightImage.FromOSD(map["light_image"]);

            if (map["sculpt"])
                prim.Sculpt = SculptData.FromOSD(map["sculpt"]);

            prim.Textures = TextureEntry.FromOSD(map["textures"]);
            
            if (map["texture_anim"])
                prim.TextureAnim = TextureAnimation.FromOSD(map["texture_anim"]);

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
                else if (type == ExtraParamType.LightImage)
                    LightMap = new LightImage(data, i);
                else if (type == ExtraParamType.Sculpt || type == ExtraParamType.Mesh)
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
            byte[] lightmap = null;
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
            if (LightMap != null)
            {
                lightmap = LightMap.GetBytes();
                size += lightmap.Length + 6;
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
            if (lightmap != null)
            {
                Buffer.BlockCopy(Utils.UInt16ToBytes((ushort)ExtraParamType.LightImage), 0, buffer, pos, 2);
                pos += 2;

                Buffer.BlockCopy(Utils.UIntToBytes((uint)lightmap.Length), 0, buffer, pos, 4);
                pos += 4;

                Buffer.BlockCopy(lightmap, 0, buffer, pos, lightmap.Length);
                pos += lightmap.Length;
            }
            if (sculpt != null)
            {
                if (Sculpt.Type == SculptType.Mesh)
                {
                    Buffer.BlockCopy(Utils.UInt16ToBytes((ushort)ExtraParamType.Mesh), 0, buffer, pos, 2);
                }
                else
                {
                    Buffer.BlockCopy(Utils.UInt16ToBytes((ushort)ExtraParamType.Sculpt), 0, buffer, pos, 2);
                }
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
