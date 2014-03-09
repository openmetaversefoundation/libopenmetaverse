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
using System.Collections.Generic;
using System.IO;
using System.Xml;
using OpenMetaverse;
using OpenMetaverse.StructuredData;

namespace OpenMetaverse.Assets
{
    /// <summary>
    /// A linkset asset, containing a parent primitive and zero or more children
    /// </summary>
    public class AssetPrim : Asset
    {
        /// <summary>
        /// Only used internally for XML serialization/deserialization
        /// </summary>
        internal enum ProfileShape : byte
        {
            Circle = 0,
            Square = 1,
            IsometricTriangle = 2,
            EquilateralTriangle = 3,
            RightTriangle = 4,
            HalfCircle = 5
        }

        public PrimObject Parent;
        public List<PrimObject> Children;

        /// <summary>Override the base classes AssetType</summary>
        public override AssetType AssetType { get { return AssetType.Object; } }

        /// <summary>Initializes a new instance of an AssetPrim object</summary>
        public AssetPrim() { }

        /// <summary>
        /// Initializes a new instance of an AssetPrim object
        /// </summary>
        /// <param name="assetID">A unique <see cref="UUID"/> specific to this asset</param>
        /// <param name="assetData">A byte array containing the raw asset data</param>
        public AssetPrim(UUID assetID, byte[] assetData) : base(assetID, assetData) { }

        public AssetPrim(string xmlData)
        {
            DecodeXml(xmlData);
        }

        public AssetPrim(PrimObject parent, List<PrimObject> children)
        {
            Parent = parent;
            if (children != null)
                Children = children;
            else
                Children = new List<PrimObject>(0);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Encode()
        {
            AssetData = System.Text.Encoding.UTF8.GetBytes(EncodeXml());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool Decode()
        {
            if (AssetData != null && AssetData.Length > 0)
            {
                try
                {
                    string xmlData = System.Text.Encoding.UTF8.GetString(AssetData);
                    DecodeXml(xmlData);
                    return true;
                }
                catch { }
            }

            return false;
        }

        public string EncodeXml()
        {
            TextWriter textWriter = new StringWriter();
            using (XmlTextWriter xmlWriter = new XmlTextWriter(textWriter))
            {
                OarFile.SOGToXml2(xmlWriter, this);
                xmlWriter.Flush();
                return textWriter.ToString();
            }
        }

        public bool DecodeXml(string xmlData)
        {
            using (XmlTextReader reader = new XmlTextReader(new StringReader(xmlData)))
            {
                reader.Read();
                reader.ReadStartElement("SceneObjectGroup");
                Parent = LoadPrim(reader);

                if (Parent != null)
                {
                    if (this.AssetID == UUID.Zero)
                        this.AssetID = Parent.ID;

                    List<PrimObject> children = new List<PrimObject>();

                    reader.Read();

                    while (!reader.EOF)
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                if (reader.Name == "SceneObjectPart")
                                {
                                    PrimObject child = LoadPrim(reader);
                                    if (child != null)
                                        children.Add(child);
                                }
                                else
                                {
                                    //Logger.Log("Found unexpected prim XML element " + reader.Name, Helpers.LogLevel.Debug);
                                    reader.Read();
                                }
                                break;
                            case XmlNodeType.EndElement:
                            default:
                                reader.Read();
                                break;
                        }
                    }

                    Children = children;
                    return true;
                }
                else
                {
                    Logger.Log("Failed to load root linkset prim", Helpers.LogLevel.Error);
                    return false;
                }
            }
        }

        public static PrimObject LoadPrim(XmlTextReader reader)
        {
            PrimObject obj = new PrimObject();
            obj.Shape = new PrimObject.ShapeBlock();
            obj.Inventory = new PrimObject.InventoryBlock();

            reader.ReadStartElement("SceneObjectPart");

            if (reader.Name == "AllowedDrop")
                obj.AllowedDrop = reader.ReadElementContentAsBoolean("AllowedDrop", String.Empty);
            else
                obj.AllowedDrop = true;

            obj.CreatorID = ReadUUID(reader, "CreatorID");
            obj.FolderID = ReadUUID(reader, "FolderID");
            obj.Inventory.Serial = reader.ReadElementContentAsInt("InventorySerial", String.Empty);

            #region Task Inventory

            List<PrimObject.InventoryBlock.ItemBlock> invItems = new List<PrimObject.InventoryBlock.ItemBlock>();

            reader.ReadStartElement("TaskInventory", String.Empty);
            while (reader.Name == "TaskInventoryItem")
            {
                PrimObject.InventoryBlock.ItemBlock item = new PrimObject.InventoryBlock.ItemBlock();
                reader.ReadStartElement("TaskInventoryItem", String.Empty);

                item.AssetID = ReadUUID(reader, "AssetID");
                item.PermsBase = (uint)reader.ReadElementContentAsInt("BasePermissions", String.Empty);
                item.CreationDate = Utils.UnixTimeToDateTime((uint)reader.ReadElementContentAsInt("CreationDate", String.Empty));
                item.CreatorID = ReadUUID(reader, "CreatorID");
                item.Description = reader.ReadElementContentAsString("Description", String.Empty);
                item.PermsEveryone = (uint)reader.ReadElementContentAsInt("EveryonePermissions", String.Empty);
                item.Flags = reader.ReadElementContentAsInt("Flags", String.Empty);
                item.GroupID = ReadUUID(reader, "GroupID");
                item.PermsGroup = (uint)reader.ReadElementContentAsInt("GroupPermissions", String.Empty);
                item.InvType = (InventoryType)reader.ReadElementContentAsInt("InvType", String.Empty);
                item.ID = ReadUUID(reader, "ItemID");
                UUID oldItemID = ReadUUID(reader, "OldItemID"); // TODO: Is this useful?
                item.LastOwnerID = ReadUUID(reader, "LastOwnerID");
                item.Name = reader.ReadElementContentAsString("Name", String.Empty);
                item.PermsNextOwner = (uint)reader.ReadElementContentAsInt("NextPermissions", String.Empty);
                item.OwnerID = ReadUUID(reader, "OwnerID");
                item.PermsOwner = (uint)reader.ReadElementContentAsInt("CurrentPermissions", String.Empty);
                UUID parentID = ReadUUID(reader, "ParentID");
                UUID parentPartID = ReadUUID(reader, "ParentPartID");
                item.PermsGranterID = ReadUUID(reader, "PermsGranter");
                item.PermsBase = (uint)reader.ReadElementContentAsInt("PermsMask", String.Empty);
                item.Type = (AssetType)reader.ReadElementContentAsInt("Type", String.Empty);

                reader.ReadEndElement();
                invItems.Add(item);
            }
            if (reader.NodeType == XmlNodeType.EndElement)
                reader.ReadEndElement();

            obj.Inventory.Items = invItems.ToArray();

            #endregion Task Inventory

            PrimFlags flags = (PrimFlags)reader.ReadElementContentAsInt("ObjectFlags", String.Empty);
            obj.UsePhysics = (flags & PrimFlags.Physics) != 0;
            obj.Phantom = (flags & PrimFlags.Phantom) != 0;
            obj.DieAtEdge = (flags & PrimFlags.DieAtEdge) != 0;
            obj.ReturnAtEdge = (flags & PrimFlags.ReturnAtEdge) != 0;
            obj.Temporary = (flags & PrimFlags.Temporary) != 0;
            obj.Sandbox = (flags & PrimFlags.Sandbox) != 0;

            obj.ID = ReadUUID(reader, "UUID");
            obj.LocalID = (uint)reader.ReadElementContentAsLong("LocalId", String.Empty);
            obj.Name = reader.ReadElementString("Name");
            obj.Material = reader.ReadElementContentAsInt("Material", String.Empty);

            if (reader.Name == "PassTouches")
                obj.PassTouches = reader.ReadElementContentAsBoolean("PassTouches", String.Empty);
            else
                obj.PassTouches = false;

            obj.RegionHandle = (ulong)reader.ReadElementContentAsLong("RegionHandle", String.Empty);
            obj.RemoteScriptAccessPIN = reader.ReadElementContentAsInt("ScriptAccessPin", String.Empty);
            
            if (reader.Name == "PlaySoundSlavePrims")
                reader.ReadInnerXml();
            if (reader.Name == "LoopSoundSlavePrims")
                reader.ReadInnerXml();

            Vector3 groupPosition = ReadVector(reader, "GroupPosition");
            Vector3 offsetPosition = ReadVector(reader, "OffsetPosition");
            obj.Rotation = ReadQuaternion(reader, "RotationOffset");
            obj.Velocity = ReadVector(reader, "Velocity");
            if (reader.Name == "RotationalVelocity")
                ReadVector(reader, "RotationalVelocity");
            obj.AngularVelocity = ReadVector(reader, "AngularVelocity");
            obj.Acceleration = ReadVector(reader, "Acceleration");
            obj.Description = reader.ReadElementString("Description");
            reader.ReadStartElement("Color");
            if (reader.Name == "R")
            {
                obj.TextColor.R = reader.ReadElementContentAsFloat("R", String.Empty);
                obj.TextColor.G = reader.ReadElementContentAsFloat("G", String.Empty);
                obj.TextColor.B = reader.ReadElementContentAsFloat("B", String.Empty);
                obj.TextColor.A = reader.ReadElementContentAsFloat("A", String.Empty);
                reader.ReadEndElement();
            }
            obj.Text = reader.ReadElementString("Text", String.Empty);
            obj.SitName = reader.ReadElementString("SitName", String.Empty);
            obj.TouchName = reader.ReadElementString("TouchName", String.Empty);

            obj.LinkNumber = reader.ReadElementContentAsInt("LinkNum", String.Empty);
            obj.ClickAction = reader.ReadElementContentAsInt("ClickAction", String.Empty);
            
            reader.ReadStartElement("Shape");
            obj.Shape.ProfileCurve = reader.ReadElementContentAsInt("ProfileCurve", String.Empty);

            byte[] teData = Convert.FromBase64String(reader.ReadElementString("TextureEntry"));
            obj.Textures = new Primitive.TextureEntry(teData, 0, teData.Length);

            reader.ReadInnerXml(); // ExtraParams

            obj.Shape.PathBegin = Primitive.UnpackBeginCut((ushort)reader.ReadElementContentAsInt("PathBegin", String.Empty));
            obj.Shape.PathCurve = reader.ReadElementContentAsInt("PathCurve", String.Empty);
            obj.Shape.PathEnd = Primitive.UnpackEndCut((ushort)reader.ReadElementContentAsInt("PathEnd", String.Empty));
            obj.Shape.PathRadiusOffset = Primitive.UnpackPathTwist((sbyte)reader.ReadElementContentAsInt("PathRadiusOffset", String.Empty));
            obj.Shape.PathRevolutions = Primitive.UnpackPathRevolutions((byte)reader.ReadElementContentAsInt("PathRevolutions", String.Empty));
            obj.Shape.PathScaleX = Primitive.UnpackPathScale((byte)reader.ReadElementContentAsInt("PathScaleX", String.Empty));
            obj.Shape.PathScaleY = Primitive.UnpackPathScale((byte)reader.ReadElementContentAsInt("PathScaleY", String.Empty));
            obj.Shape.PathShearX = Primitive.UnpackPathShear((sbyte)reader.ReadElementContentAsInt("PathShearX", String.Empty));
            obj.Shape.PathShearY = Primitive.UnpackPathShear((sbyte)reader.ReadElementContentAsInt("PathShearY", String.Empty));
            obj.Shape.PathSkew = Primitive.UnpackPathTwist((sbyte)reader.ReadElementContentAsInt("PathSkew", String.Empty));
            obj.Shape.PathTaperX = Primitive.UnpackPathTaper((sbyte)reader.ReadElementContentAsInt("PathTaperX", String.Empty));
            obj.Shape.PathTaperY = Primitive.UnpackPathShear((sbyte)reader.ReadElementContentAsInt("PathTaperY", String.Empty));
            obj.Shape.PathTwist = Primitive.UnpackPathTwist((sbyte)reader.ReadElementContentAsInt("PathTwist", String.Empty));
            obj.Shape.PathTwistBegin = Primitive.UnpackPathTwist((sbyte)reader.ReadElementContentAsInt("PathTwistBegin", String.Empty));
            obj.PCode = reader.ReadElementContentAsInt("PCode", String.Empty);
            obj.Shape.ProfileBegin = Primitive.UnpackBeginCut((ushort)reader.ReadElementContentAsInt("ProfileBegin", String.Empty));
            obj.Shape.ProfileEnd = Primitive.UnpackEndCut((ushort)reader.ReadElementContentAsInt("ProfileEnd", String.Empty));
            obj.Shape.ProfileHollow = Primitive.UnpackProfileHollow((ushort)reader.ReadElementContentAsInt("ProfileHollow", String.Empty));
            obj.Scale = ReadVector(reader, "Scale");
            obj.State = (byte)reader.ReadElementContentAsInt("State", String.Empty);

            ProfileShape profileShape = (ProfileShape)Enum.Parse(typeof(ProfileShape), reader.ReadElementString("ProfileShape"));
            HoleType holeType = (HoleType)Enum.Parse(typeof(HoleType), reader.ReadElementString("HollowShape"));
            obj.Shape.ProfileCurve = (int)profileShape | (int)holeType;

            UUID sculptTexture = ReadUUID(reader, "SculptTexture");
            SculptType sculptType = (SculptType)reader.ReadElementContentAsInt("SculptType", String.Empty);
            if (sculptTexture != UUID.Zero)
            {
                obj.Sculpt = new PrimObject.SculptBlock();
                obj.Sculpt.Texture = sculptTexture;
                obj.Sculpt.Type = (int)sculptType;
            }

            PrimObject.FlexibleBlock flexible = new PrimObject.FlexibleBlock();
            PrimObject.LightBlock light = new PrimObject.LightBlock();

            reader.ReadInnerXml(); // SculptData

            flexible.Softness = reader.ReadElementContentAsInt("FlexiSoftness", String.Empty);
            flexible.Tension = reader.ReadElementContentAsFloat("FlexiTension", String.Empty);
            flexible.Drag = reader.ReadElementContentAsFloat("FlexiDrag", String.Empty);
            flexible.Gravity = reader.ReadElementContentAsFloat("FlexiGravity", String.Empty);
            flexible.Wind = reader.ReadElementContentAsFloat("FlexiWind", String.Empty);
            flexible.Force.X = reader.ReadElementContentAsFloat("FlexiForceX", String.Empty);
            flexible.Force.Y = reader.ReadElementContentAsFloat("FlexiForceY", String.Empty);
            flexible.Force.Z = reader.ReadElementContentAsFloat("FlexiForceZ", String.Empty);

            light.Color.R = reader.ReadElementContentAsFloat("LightColorR", String.Empty);
            light.Color.G = reader.ReadElementContentAsFloat("LightColorG", String.Empty);
            light.Color.B = reader.ReadElementContentAsFloat("LightColorB", String.Empty);
            light.Color.A = reader.ReadElementContentAsFloat("LightColorA", String.Empty);
            light.Radius = reader.ReadElementContentAsFloat("LightRadius", String.Empty);
            light.Cutoff = reader.ReadElementContentAsFloat("LightCutoff", String.Empty);
            light.Falloff = reader.ReadElementContentAsFloat("LightFalloff", String.Empty);
            light.Intensity = reader.ReadElementContentAsFloat("LightIntensity", String.Empty);

            bool hasFlexi = reader.ReadElementContentAsBoolean("FlexiEntry", String.Empty);
            bool hasLight = reader.ReadElementContentAsBoolean("LightEntry", String.Empty);
            reader.ReadInnerXml(); // SculptEntry

            if (hasFlexi)
                obj.Flexible = flexible;
            if (hasLight)
                obj.Light = light;

            reader.ReadEndElement();

            obj.Scale = ReadVector(reader, "Scale"); // Yes, again
            reader.ReadInnerXml(); // UpdateFlag

            reader.ReadInnerXml(); // SitTargetOrientation
            reader.ReadInnerXml(); // SitTargetPosition
            obj.SitOffset = ReadVector(reader, "SitTargetPositionLL");
            obj.SitRotation = ReadQuaternion(reader, "SitTargetOrientationLL");
            obj.ParentID = (uint)reader.ReadElementContentAsLong("ParentID", String.Empty);
            obj.CreationDate = Utils.UnixTimeToDateTime(reader.ReadElementContentAsInt("CreationDate", String.Empty));
            int category = reader.ReadElementContentAsInt("Category", String.Empty);
            obj.SalePrice = reader.ReadElementContentAsInt("SalePrice", String.Empty);
            obj.SaleType = reader.ReadElementContentAsInt("ObjectSaleType", String.Empty);
            int ownershipCost = reader.ReadElementContentAsInt("OwnershipCost", String.Empty);
            obj.GroupID = ReadUUID(reader, "GroupID");
            obj.OwnerID = ReadUUID(reader, "OwnerID");
            obj.LastOwnerID = ReadUUID(reader, "LastOwnerID");
            obj.PermsBase = (uint)reader.ReadElementContentAsInt("BaseMask", String.Empty);
            obj.PermsOwner = (uint)reader.ReadElementContentAsInt("OwnerMask", String.Empty);
            obj.PermsGroup = (uint)reader.ReadElementContentAsInt("GroupMask", String.Empty);
            obj.PermsEveryone = (uint)reader.ReadElementContentAsInt("EveryoneMask", String.Empty);
            obj.PermsNextOwner = (uint)reader.ReadElementContentAsInt("NextOwnerMask", String.Empty);

            reader.ReadInnerXml(); // Flags

            obj.CollisionSound = ReadUUID(reader, "CollisionSound");
            obj.CollisionSoundVolume = reader.ReadElementContentAsFloat("CollisionSoundVolume", String.Empty);

            reader.ReadEndElement();

            if (obj.ParentID == 0)
                obj.Position = groupPosition;
            else
                obj.Position = offsetPosition;

            return obj;
        }

        static UUID ReadUUID(XmlTextReader reader, string name)
        {
            UUID id;
            string idStr;

            reader.ReadStartElement(name);

            if (reader.Name == "Guid")
                idStr = reader.ReadElementString("Guid");
            else // UUID
                idStr = reader.ReadElementString("UUID");

            UUID.TryParse(idStr, out id);
            reader.ReadEndElement();

            return id;
        }

        static Vector3 ReadVector(XmlTextReader reader, string name)
        {
            Vector3 vec;

            reader.ReadStartElement(name);
            vec.X = reader.ReadElementContentAsFloat("X", String.Empty);
            vec.Y = reader.ReadElementContentAsFloat("Y", String.Empty);
            vec.Z = reader.ReadElementContentAsFloat("Z", String.Empty);
            reader.ReadEndElement();

            return vec;
        }

        static Quaternion ReadQuaternion(XmlTextReader reader, string name)
        {
            Quaternion quat;

            reader.ReadStartElement(name);
            quat.X = reader.ReadElementContentAsFloat("X", String.Empty);
            quat.Y = reader.ReadElementContentAsFloat("Y", String.Empty);
            quat.Z = reader.ReadElementContentAsFloat("Z", String.Empty);
            quat.W = reader.ReadElementContentAsFloat("W", String.Empty);
            reader.ReadEndElement();

            return quat;
        }
    }

    /// <summary>
    /// The deserialized form of a single primitive in a linkset asset
    /// </summary>
    public class PrimObject
    {
        public class FlexibleBlock
        {
            public int Softness;
            public float Gravity;
            public float Drag;
            public float Wind;
            public float Tension;
            public Vector3 Force;

            public OSDMap Serialize()
            {
                OSDMap map = new OSDMap();
                map["softness"] = OSD.FromInteger(Softness);
                map["gravity"] = OSD.FromReal(Gravity);
                map["drag"] = OSD.FromReal(Drag);
                map["wind"] = OSD.FromReal(Wind);
                map["tension"] = OSD.FromReal(Tension);
                map["force"] = OSD.FromVector3(Force);
                return map;
            }

            public void Deserialize(OSDMap map)
            {
                Softness = map["softness"].AsInteger();
                Gravity = (float)map["gravity"].AsReal();
                Drag = (float)map["drag"].AsReal();
                Wind = (float)map["wind"].AsReal();
                Tension = (float)map["tension"].AsReal();
                Force = map["force"].AsVector3();
            }
        }

        public class LightBlock
        {
            public Color4 Color;
            public float Intensity;
            public float Radius;
            public float Falloff;
            public float Cutoff;

            public OSDMap Serialize()
            {
                OSDMap map = new OSDMap();
                map["color"] = OSD.FromColor4(Color);
                map["intensity"] = OSD.FromReal(Intensity);
                map["radius"] = OSD.FromReal(Radius);
                map["falloff"] = OSD.FromReal(Falloff);
                map["cutoff"] = OSD.FromReal(Cutoff);
                return map;
            }

            public void Deserialize(OSDMap map)
            {
                Color = map["color"].AsColor4();
                Intensity = (float)map["intensity"].AsReal();
                Radius = (float)map["radius"].AsReal();
                Falloff = (float)map["falloff"].AsReal();
                Cutoff = (float)map["cutoff"].AsReal();
            }
        }

        public class SculptBlock
        {
            public UUID Texture;
            public int Type;

            public OSDMap Serialize()
            {
                OSDMap map = new OSDMap();
                map["texture"] = OSD.FromUUID(Texture);
                map["type"] = OSD.FromInteger(Type);
                return map;
            }

            public void Deserialize(OSDMap map)
            {
                Texture = map["texture"].AsUUID();
                Type = map["type"].AsInteger();
            }
        }

        public class ParticlesBlock
        {
            public int Flags;
            public int Pattern;
            public float MaxAge;
            public float StartAge;
            public float InnerAngle;
            public float OuterAngle;
            public float BurstRate;
            public float BurstRadius;
            public float BurstSpeedMin;
            public float BurstSpeedMax;
            public int BurstParticleCount;
            public Vector3 AngularVelocity;
            public Vector3 Acceleration;
            public UUID TextureID;
            public UUID TargetID;
            public int DataFlags;
            public float ParticleMaxAge;
            public Color4 ParticleStartColor;
            public Color4 ParticleEndColor;
            public Vector2 ParticleStartScale;
            public Vector2 ParticleEndScale;

            public OSDMap Serialize()
            {
                OSDMap map = new OSDMap();
                map["flags"] = OSD.FromInteger(Flags);
                map["pattern"] = OSD.FromInteger(Pattern);
                map["max_age"] = OSD.FromReal(MaxAge);
                map["start_age"] = OSD.FromReal(StartAge);
                map["inner_angle"] = OSD.FromReal(InnerAngle);
                map["outer_angle"] = OSD.FromReal(OuterAngle);
                map["burst_rate"] = OSD.FromReal(BurstRate);
                map["burst_radius"] = OSD.FromReal(BurstRadius);
                map["burst_speed_min"] = OSD.FromReal(BurstSpeedMin);
                map["burst_speed_max"] = OSD.FromReal(BurstSpeedMax);
                map["burst_particle_count"] = OSD.FromInteger(BurstParticleCount);
                map["angular_velocity"] = OSD.FromVector3(AngularVelocity);
                map["acceleration"] = OSD.FromVector3(Acceleration);
                map["texture_id"] = OSD.FromUUID(TextureID);
                map["target_id"] = OSD.FromUUID(TargetID);
                map["data_flags"] = OSD.FromInteger(DataFlags);
                map["particle_max_age"] = OSD.FromReal(ParticleMaxAge);
                map["particle_start_color"] = OSD.FromColor4(ParticleStartColor);
                map["particle_end_color"] = OSD.FromColor4(ParticleEndColor);
                map["particle_start_scale"] = OSD.FromVector2(ParticleStartScale);
                map["particle_end_scale"] = OSD.FromVector2(ParticleEndScale);
                return map;
            }

            public void Deserialize(OSDMap map)
            {
                Flags = map["flags"].AsInteger();
                Pattern = map["pattern"].AsInteger();
                MaxAge = (float)map["max_age"].AsReal();
                StartAge = (float)map["start_age"].AsReal();
                InnerAngle = (float)map["inner_angle"].AsReal();
                OuterAngle = (float)map["outer_angle"].AsReal();
                BurstRate = (float)map["burst_rate"].AsReal();
                BurstRadius = (float)map["burst_radius"].AsReal();
                BurstSpeedMin = (float)map["burst_speed_min"].AsReal();
                BurstSpeedMax = (float)map["burst_speed_max"].AsReal();
                BurstParticleCount = map["burst_particle_count"].AsInteger();
                AngularVelocity = map["angular_velocity"].AsVector3();
                Acceleration = map["acceleration"].AsVector3();
                TextureID = map["texture_id"].AsUUID();
                DataFlags = map["data_flags"].AsInteger();
                ParticleMaxAge = (float)map["particle_max_age"].AsReal();
                ParticleStartColor = map["particle_start_color"].AsColor4();
                ParticleEndColor = map["particle_end_color"].AsColor4();
                ParticleStartScale = map["particle_start_scale"].AsVector2();
                ParticleEndScale = map["particle_end_scale"].AsVector2();
            }
        }

        public class ShapeBlock
        {
            public int PathCurve;
            public float PathBegin;
            public float PathEnd;
            public float PathScaleX;
            public float PathScaleY;
            public float PathShearX;
            public float PathShearY;
            public float PathTwist;
            public float PathTwistBegin;
            public float PathRadiusOffset;
            public float PathTaperX;
            public float PathTaperY;
            public float PathRevolutions;
            public float PathSkew;
            public int ProfileCurve;
            public float ProfileBegin;
            public float ProfileEnd;
            public float ProfileHollow;

            public OSDMap Serialize()
            {
                OSDMap map = new OSDMap();
                map["path_curve"] = OSD.FromInteger(PathCurve);
                map["path_begin"] = OSD.FromReal(PathBegin);
                map["path_end"] = OSD.FromReal(PathEnd);
                map["path_scale_x"] = OSD.FromReal(PathScaleX);
                map["path_scale_y"] = OSD.FromReal(PathScaleY);
                map["path_shear_x"] = OSD.FromReal(PathShearX);
                map["path_shear_y"] = OSD.FromReal(PathShearY);
                map["path_twist"] = OSD.FromReal(PathTwist);
                map["path_twist_begin"] = OSD.FromReal(PathTwistBegin);
                map["path_radius_offset"] = OSD.FromReal(PathRadiusOffset);
                map["path_taper_x"] = OSD.FromReal(PathTaperX);
                map["path_taper_y"] = OSD.FromReal(PathTaperY);
                map["path_revolutions"] = OSD.FromReal(PathRevolutions);
                map["path_skew"] = OSD.FromReal(PathSkew);
                map["profile_curve"] = OSD.FromInteger(ProfileCurve);
                map["profile_begin"] = OSD.FromReal(ProfileBegin);
                map["profile_end"] = OSD.FromReal(ProfileEnd);
                map["profile_hollow"] = OSD.FromReal(ProfileHollow);
                return map;
            }

            public void Deserialize(OSDMap map)
            {
                PathCurve = map["path_curve"].AsInteger();
                PathBegin = (float)map["path_begin"].AsReal();
                PathEnd = (float)map["path_end"].AsReal();
                PathScaleX = (float)map["path_scale_x"].AsReal();
                PathScaleY = (float)map["path_scale_y"].AsReal();
                PathShearX = (float)map["path_shear_x"].AsReal();
                PathShearY = (float)map["path_shear_y"].AsReal();
                PathTwist = (float)map["path_twist"].AsReal();
                PathTwistBegin = (float)map["path_twist_begin"].AsReal();
                PathRadiusOffset = (float)map["path_radius_offset"].AsReal();
                PathTaperX = (float)map["path_taper_x"].AsReal();
                PathTaperY = (float)map["path_taper_y"].AsReal();
                PathRevolutions = (float)map["path_revolutions"].AsReal();
                PathSkew = (float)map["path_skew"].AsReal();
                ProfileCurve = map["profile_curve"].AsInteger();
                ProfileBegin = (float)map["profile_begin"].AsReal();
                ProfileEnd = (float)map["profile_end"].AsReal();
                ProfileHollow = (float)map["profile_hollow"].AsReal();
            }
        }

        public class InventoryBlock
        {
            public class ItemBlock
            {
                public UUID ID;
                public string Name;
                public UUID OwnerID;
                public UUID CreatorID;
                public UUID GroupID;
                public UUID LastOwnerID;
                public UUID PermsGranterID;
                public UUID AssetID;
                public AssetType Type;
                public InventoryType InvType;
                public string Description;
                public uint PermsBase;
                public uint PermsOwner;
                public uint PermsGroup;
                public uint PermsEveryone;
                public uint PermsNextOwner;
                public int Flags;
                public DateTime CreationDate;

                public OSDMap Serialize()
                {
                    OSDMap map = new OSDMap();
                    map["id"] = OSD.FromUUID(ID);
                    map["name"] = OSD.FromString(Name);
                    map["owner_id"] = OSD.FromUUID(OwnerID);
                    map["creator_id"] = OSD.FromUUID(CreatorID);
                    map["group_id"] = OSD.FromUUID(GroupID);
                    map["last_owner_id"] = OSD.FromUUID(LastOwnerID);
                    map["perms_granter_id"] = OSD.FromUUID(PermsGranterID);
                    map["asset_id"] = OSD.FromUUID(AssetID);
                    map["asset_type"] = OSD.FromInteger((int)Type);
                    map["inv_type"] = OSD.FromInteger((int)InvType);
                    map["description"] = OSD.FromString(Description);
                    map["perms_base"] = OSD.FromInteger(PermsBase);
                    map["perms_owner"] = OSD.FromInteger(PermsOwner);
                    map["perms_group"] = OSD.FromInteger(PermsGroup);
                    map["perms_everyone"] = OSD.FromInteger(PermsEveryone);
                    map["perms_next_owner"] = OSD.FromInteger(PermsNextOwner);
                    map["flags"] = OSD.FromInteger(Flags);
                    map["creation_date"] = OSD.FromDate(CreationDate);
                    return map;
                }

                public void Deserialize(OSDMap map)
                {
                    ID = map["id"].AsUUID();
                    Name = map["name"].AsString();
                    OwnerID = map["owner_id"].AsUUID();
                    CreatorID = map["creator_id"].AsUUID();
                    GroupID = map["group_id"].AsUUID();
                    LastOwnerID = map["last_owner_id"].AsUUID();
                    PermsGranterID = map["perms_granter_id"].AsUUID();
                    AssetID = map["asset_id"].AsUUID();
                    Type = (AssetType)map["asset_type"].AsInteger();
                    InvType = (InventoryType)map["inv_type"].AsInteger();
                    Description = map["description"].AsString();
                    PermsBase = (uint)map["perms_base"].AsInteger();
                    PermsOwner = (uint)map["perms_owner"].AsInteger();
                    PermsGroup = (uint)map["perms_group"].AsInteger();
                    PermsEveryone = (uint)map["perms_everyone"].AsInteger();
                    PermsNextOwner = (uint)map["perms_next_owner"].AsInteger();
                    Flags = map["flags"].AsInteger();
                    CreationDate = map["creation_date"].AsDate();
                }

                public static ItemBlock FromInventoryBase(InventoryItem item)
                {
                    ItemBlock block = new ItemBlock();
                    block.AssetID = item.AssetUUID;
                    block.CreationDate = item.CreationDate;
                    block.CreatorID = item.CreatorID;
                    block.Description = item.Description;
                    block.Flags = (int)item.Flags;
                    block.GroupID = item.GroupID;
                    block.ID = item.UUID;
                    block.InvType = item.InventoryType == InventoryType.Unknown && item.AssetType == AssetType.LSLText ? InventoryType.LSL : item.InventoryType; ;
                    block.LastOwnerID = item.LastOwnerID;
                    block.Name = item.Name;
                    block.OwnerID = item.OwnerID;
                    block.PermsBase = (uint)item.Permissions.BaseMask;
                    block.PermsEveryone = (uint)item.Permissions.EveryoneMask;
                    block.PermsGroup = (uint)item.Permissions.GroupMask;
                    block.PermsNextOwner = (uint)item.Permissions.NextOwnerMask;
                    block.PermsOwner = (uint)item.Permissions.OwnerMask;
                    block.PermsGranterID = UUID.Zero;
                    block.Type = item.AssetType;
                    return block;
                }
            }

            public int Serial;
            public ItemBlock[] Items;

            public OSDMap Serialize()
            {
                OSDMap map = new OSDMap();
                map["serial"] = OSD.FromInteger(Serial);

                if (Items != null)
                {
                    OSDArray array = new OSDArray(Items.Length);
                    for (int i = 0; i < Items.Length; i++)
                        array.Add(Items[i].Serialize());
                    map["items"] = array;
                }

                return map;
            }

            public void Deserialize(OSDMap map)
            {
                Serial = map["serial"].AsInteger();

                if (map.ContainsKey("items"))
                {
                    OSDArray array = (OSDArray)map["items"];
                    Items = new ItemBlock[array.Count];

                    for (int i = 0; i < array.Count; i++)
                    {
                        ItemBlock item = new ItemBlock();
                        item.Deserialize((OSDMap)array[i]);
                        Items[i] = item;
                    }
                }
                else
                {
                    Items = new ItemBlock[0];
                }
            }
        }

        public UUID ID;
        public bool AllowedDrop;
        public Vector3 AttachmentPosition;
        public Quaternion AttachmentRotation;
        public Quaternion BeforeAttachmentRotation;
        public string Name;
        public string Description;
        public uint PermsBase;
        public uint PermsOwner;
        public uint PermsGroup;
        public uint PermsEveryone;
        public uint PermsNextOwner;
        public UUID CreatorID;
        public UUID OwnerID;
        public UUID LastOwnerID;
        public UUID GroupID;
        public UUID FolderID;
        public ulong RegionHandle;
        public int ClickAction;
        public int LastAttachmentPoint;
        public int LinkNumber;
        public uint LocalID;
        public uint ParentID;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Velocity;
        public Vector3 AngularVelocity;
        public Vector3 Acceleration;
        public Vector3 Scale;
        public Vector3 SitOffset;
        public Quaternion SitRotation;
        public Vector3 CameraEyeOffset;
        public Vector3 CameraAtOffset;
        public int State;
        public int PCode;
        public int Material;
        public bool PassTouches;
        public UUID SoundID;
        public float SoundGain;
        public float SoundRadius;
        public int SoundFlags;
        public Color4 TextColor;
        public string Text;
        public string SitName;
        public string TouchName;
        public bool Selected;
        public UUID SelectorID;
        public bool UsePhysics;
        public bool Phantom;
        public int RemoteScriptAccessPIN;
        public bool VolumeDetect;
        public bool DieAtEdge;
        public bool ReturnAtEdge;
        public bool Temporary;
        public bool Sandbox;
        public DateTime CreationDate;
        public DateTime RezDate;
        public int SalePrice;
        public int SaleType;
        public byte[] ScriptState;
        public UUID CollisionSound;
        public float CollisionSoundVolume;
        public FlexibleBlock Flexible;
        public LightBlock Light;
        public SculptBlock Sculpt;
        public ParticlesBlock Particles;
        public ShapeBlock Shape;
        public Primitive.TextureEntry Textures;
        public InventoryBlock Inventory;

        public OSDMap Serialize()
        {
            OSDMap map = new OSDMap();
            map["id"] = OSD.FromUUID(ID);
            map["attachment_position"] = OSD.FromVector3(AttachmentPosition);
            map["attachment_rotation"] = OSD.FromQuaternion(AttachmentRotation);
            map["before_attachment_rotation"] = OSD.FromQuaternion(BeforeAttachmentRotation);
            map["name"] = OSD.FromString(Name);
            map["description"] = OSD.FromString(Description);
            map["perms_base"] = OSD.FromInteger(PermsBase);
            map["perms_owner"] = OSD.FromInteger(PermsOwner);
            map["perms_group"] = OSD.FromInteger(PermsGroup);
            map["perms_everyone"] = OSD.FromInteger(PermsEveryone);
            map["perms_next_owner"] = OSD.FromInteger(PermsNextOwner);
            map["creator_identity"] = OSD.FromUUID(CreatorID);
            map["owner_identity"] = OSD.FromUUID(OwnerID);
            map["last_owner_identity"] = OSD.FromUUID(LastOwnerID);
            map["group_identity"] = OSD.FromUUID(GroupID);
            map["folder_id"] = OSD.FromUUID(FolderID);
            map["region_handle"] = OSD.FromULong(RegionHandle);
            map["click_action"] = OSD.FromInteger(ClickAction);
            map["last_attachment_point"] = OSD.FromInteger(LastAttachmentPoint);
            map["link_number"] = OSD.FromInteger(LinkNumber);
            map["local_id"] = OSD.FromInteger(LocalID);
            map["parent_id"] = OSD.FromInteger(ParentID);
            map["position"] = OSD.FromVector3(Position);
            map["rotation"] = OSD.FromQuaternion(Rotation);
            map["velocity"] = OSD.FromVector3(Velocity);
            map["angular_velocity"] = OSD.FromVector3(AngularVelocity);
            map["acceleration"] = OSD.FromVector3(Acceleration);
            map["scale"] = OSD.FromVector3(Scale);
            map["sit_offset"] = OSD.FromVector3(SitOffset);
            map["sit_rotation"] = OSD.FromQuaternion(SitRotation);
            map["camera_eye_offset"] = OSD.FromVector3(CameraEyeOffset);
            map["camera_at_offset"] = OSD.FromVector3(CameraAtOffset);
            map["state"] = OSD.FromInteger(State);
            map["prim_code"] = OSD.FromInteger(PCode);
            map["material"] = OSD.FromInteger(Material);
            map["pass_touches"] = OSD.FromBoolean(PassTouches);
            map["sound_id"] = OSD.FromUUID(SoundID);
            map["sound_gain"] = OSD.FromReal(SoundGain);
            map["sound_radius"] = OSD.FromReal(SoundRadius);
            map["sound_flags"] = OSD.FromInteger(SoundFlags);
            map["text_color"] = OSD.FromColor4(TextColor);
            map["text"] = OSD.FromString(Text);
            map["sit_name"] = OSD.FromString(SitName);
            map["touch_name"] = OSD.FromString(TouchName);
            map["selected"] = OSD.FromBoolean(Selected);
            map["selector_id"] = OSD.FromUUID(SelectorID);
            map["use_physics"] = OSD.FromBoolean(UsePhysics);
            map["phantom"] = OSD.FromBoolean(Phantom);
            map["remote_script_access_pin"] = OSD.FromInteger(RemoteScriptAccessPIN);
            map["volume_detect"] = OSD.FromBoolean(VolumeDetect);
            map["die_at_edge"] = OSD.FromBoolean(DieAtEdge);
            map["return_at_edge"] = OSD.FromBoolean(ReturnAtEdge);
            map["temporary"] = OSD.FromBoolean(Temporary);
            map["sandbox"] = OSD.FromBoolean(Sandbox);
            map["creation_date"] = OSD.FromDate(CreationDate);
            map["rez_date"] = OSD.FromDate(RezDate);
            map["sale_price"] = OSD.FromInteger(SalePrice);
            map["sale_type"] = OSD.FromInteger(SaleType);

            if (Flexible != null)
                map["flexible"] = Flexible.Serialize();
            if (Light != null)
                map["light"] = Light.Serialize();
            if (Sculpt != null)
                map["sculpt"] = Sculpt.Serialize();
            if (Particles != null)
                map["particles"] = Particles.Serialize();
            if (Shape != null)
                map["shape"] = Shape.Serialize();
            if (Textures != null)
                map["textures"] = Textures.GetOSD();
            if (Inventory != null)
                map["inventory"] = Inventory.Serialize();

            return map;
        }

        public void Deserialize(OSDMap map)
        {
            ID = map["id"].AsUUID();
            AttachmentPosition = map["attachment_position"].AsVector3();
            AttachmentRotation = map["attachment_rotation"].AsQuaternion();
            BeforeAttachmentRotation = map["before_attachment_rotation"].AsQuaternion();
            Name = map["name"].AsString();
            Description = map["description"].AsString();
            PermsBase = (uint)map["perms_base"].AsInteger();
            PermsOwner = (uint)map["perms_owner"].AsInteger();
            PermsGroup = (uint)map["perms_group"].AsInteger();
            PermsEveryone = (uint)map["perms_everyone"].AsInteger();
            PermsNextOwner = (uint)map["perms_next_owner"].AsInteger();
            CreatorID = map["creator_identity"].AsUUID();
            OwnerID = map["owner_identity"].AsUUID();
            LastOwnerID = map["last_owner_identity"].AsUUID();
            GroupID = map["group_identity"].AsUUID();
            FolderID = map["folder_id"].AsUUID();
            RegionHandle = map["region_handle"].AsULong();
            ClickAction = map["click_action"].AsInteger();
            LastAttachmentPoint = map["last_attachment_point"].AsInteger();
            LinkNumber = map["link_number"].AsInteger();
            LocalID = (uint)map["local_id"].AsInteger();
            ParentID = (uint)map["parent_id"].AsInteger();
            Position = map["position"].AsVector3();
            Rotation = map["rotation"].AsQuaternion();
            Velocity = map["velocity"].AsVector3();
            AngularVelocity = map["angular_velocity"].AsVector3();
            Acceleration = map["acceleration"].AsVector3();
            Scale = map["scale"].AsVector3();
            SitOffset = map["sit_offset"].AsVector3();
            SitRotation = map["sit_rotation"].AsQuaternion();
            CameraEyeOffset = map["camera_eye_offset"].AsVector3();
            CameraAtOffset = map["camera_at_offset"].AsVector3();
            State = map["state"].AsInteger();
            PCode = map["prim_code"].AsInteger();
            Material = map["material"].AsInteger();
            PassTouches = map["pass_touches"].AsBoolean();
            SoundID = map["sound_id"].AsUUID();
            SoundGain = (float)map["sound_gain"].AsReal();
            SoundRadius = (float)map["sound_radius"].AsReal();
            SoundFlags = map["sound_flags"].AsInteger();
            TextColor = map["text_color"].AsColor4();
            Text = map["text"].AsString();
            SitName = map["sit_name"].AsString();
            TouchName = map["touch_name"].AsString();
            Selected = map["selected"].AsBoolean();
            SelectorID = map["selector_id"].AsUUID();
            UsePhysics = map["use_physics"].AsBoolean();
            Phantom = map["phantom"].AsBoolean();
            RemoteScriptAccessPIN = map["remote_script_access_pin"].AsInteger();
            VolumeDetect = map["volume_detect"].AsBoolean();
            DieAtEdge = map["die_at_edge"].AsBoolean();
            ReturnAtEdge = map["return_at_edge"].AsBoolean();
            Temporary = map["temporary"].AsBoolean();
            Sandbox = map["sandbox"].AsBoolean();
            CreationDate = map["creation_date"].AsDate();
            RezDate = map["rez_date"].AsDate();
            SalePrice = map["sale_price"].AsInteger();
            SaleType = map["sale_type"].AsInteger();
        }

        public static PrimObject FromPrimitive(Primitive obj)
        {
            PrimObject prim = new PrimObject();
            prim.Acceleration = obj.Acceleration;
            prim.AllowedDrop = (obj.Flags & PrimFlags.AllowInventoryDrop) == PrimFlags.AllowInventoryDrop;
            prim.AngularVelocity = obj.AngularVelocity;
            //prim.AttachmentPosition
            //prim.AttachmentRotation
            //prim.BeforeAttachmentRotation
            //prim.CameraAtOffset
            //prim.CameraEyeOffset
            prim.ClickAction = (int)obj.ClickAction;
            //prim.CollisionSound
            //prim.CollisionSoundVolume;
            prim.CreationDate = obj.Properties.CreationDate;
            prim.CreatorID = obj.Properties.CreatorID;
            prim.Description = obj.Properties.Description;
            prim.DieAtEdge = (obj.Flags & PrimFlags.DieAtEdge) == PrimFlags.AllowInventoryDrop;
            if (obj.Flexible != null)
            {
                prim.Flexible = new FlexibleBlock();
                prim.Flexible.Drag = obj.Flexible.Drag;
                prim.Flexible.Force = obj.Flexible.Force;
                prim.Flexible.Gravity = obj.Flexible.Gravity;
                prim.Flexible.Softness = obj.Flexible.Softness;
                prim.Flexible.Tension = obj.Flexible.Tension;
                prim.Flexible.Wind = obj.Flexible.Wind;
            }
            prim.FolderID = obj.Properties.FolderID;
            prim.GroupID = obj.Properties.GroupID;
            prim.ID = obj.Properties.ObjectID;
            //prim.Inventory;
            //prim.LastAttachmentPoint;
            prim.LastOwnerID = obj.Properties.LastOwnerID;
            if (obj.Light != null)
            {
                prim.Light = new LightBlock();
                prim.Light.Color = obj.Light.Color;
                prim.Light.Cutoff = obj.Light.Cutoff;
                prim.Light.Falloff = obj.Light.Falloff;
                prim.Light.Intensity = obj.Light.Intensity;
                prim.Light.Radius = obj.Light.Radius;
            }

            //prim.LinkNumber;
            prim.LocalID = obj.LocalID;
            prim.Material = (int)obj.PrimData.Material;
            prim.Name = obj.Properties.Name;
            prim.OwnerID = obj.Properties.OwnerID;
            prim.ParentID = obj.ParentID;
            
            prim.Particles = new ParticlesBlock();
            prim.Particles.AngularVelocity = obj.ParticleSys.AngularVelocity;
            prim.Particles.Acceleration = obj.ParticleSys.PartAcceleration;
            prim.Particles.BurstParticleCount = obj.ParticleSys.BurstPartCount;
            prim.Particles.BurstRate = obj.ParticleSys.BurstRadius;
            prim.Particles.BurstRate = obj.ParticleSys.BurstRate;
            prim.Particles.BurstSpeedMax = obj.ParticleSys.BurstSpeedMax;
            prim.Particles.BurstSpeedMin = obj.ParticleSys.BurstSpeedMin;
            prim.Particles.DataFlags = (int)obj.ParticleSys.PartDataFlags;
            prim.Particles.Flags = (int)obj.ParticleSys.PartFlags;
            prim.Particles.InnerAngle = obj.ParticleSys.InnerAngle;
            prim.Particles.MaxAge = obj.ParticleSys.MaxAge;
            prim.Particles.OuterAngle = obj.ParticleSys.OuterAngle;
            prim.Particles.ParticleEndColor = obj.ParticleSys.PartEndColor;
            prim.Particles.ParticleEndScale = new Vector2(obj.ParticleSys.PartEndScaleX, obj.ParticleSys.PartEndScaleY);
            prim.Particles.ParticleMaxAge = obj.ParticleSys.MaxAge;
            prim.Particles.ParticleStartColor = obj.ParticleSys.PartStartColor;
            prim.Particles.ParticleStartScale = new Vector2(obj.ParticleSys.PartStartScaleX, obj.ParticleSys.PartStartScaleY);
            prim.Particles.Pattern = (int)obj.ParticleSys.Pattern;
            prim.Particles.StartAge = obj.ParticleSys.StartAge;
            prim.Particles.TargetID = obj.ParticleSys.Target;
            prim.Particles.TextureID = obj.ParticleSys.Texture;

            //prim.PassTouches;
            prim.PCode = (int)obj.PrimData.PCode;
            prim.PermsBase = (uint)obj.Properties.Permissions.BaseMask;
            prim.PermsEveryone = (uint)obj.Properties.Permissions.EveryoneMask;
            prim.PermsGroup = (uint)obj.Properties.Permissions.GroupMask;
            prim.PermsNextOwner = (uint)obj.Properties.Permissions.NextOwnerMask;
            prim.PermsOwner = (uint)obj.Properties.Permissions.OwnerMask;
            prim.Phantom = (obj.Flags & PrimFlags.Phantom) == PrimFlags.Phantom;
            prim.Position = obj.Position;
            prim.RegionHandle = obj.RegionHandle;
            //prim.RemoteScriptAccessPIN;
            prim.ReturnAtEdge = (obj.Flags & PrimFlags.ReturnAtEdge) == PrimFlags.ReturnAtEdge;
            //prim.RezDate;
            prim.Rotation = obj.Rotation;
            prim.SalePrice = obj.Properties.SalePrice;
            prim.SaleType = (int)obj.Properties.SaleType;
            prim.Sandbox = (obj.Flags & PrimFlags.Sandbox) == PrimFlags.Sandbox;
            prim.Scale = obj.Scale;
            //prim.ScriptState;
            if (obj.Sculpt != null)
            {
                prim.Sculpt = new SculptBlock();
                prim.Sculpt.Texture = obj.Sculpt.SculptTexture;
                prim.Sculpt.Type = (int)obj.Sculpt.Type;
            }
            prim.Shape = new ShapeBlock();
            prim.Shape.PathBegin = obj.PrimData.PathBegin;
            prim.Shape.PathCurve = (int)obj.PrimData.PathCurve;
            prim.Shape.PathEnd = obj.PrimData.PathEnd;
            prim.Shape.PathRadiusOffset = obj.PrimData.PathRadiusOffset;
            prim.Shape.PathRevolutions = obj.PrimData.PathRevolutions;
            prim.Shape.PathScaleX = obj.PrimData.PathScaleX;
            prim.Shape.PathScaleY = obj.PrimData.PathScaleY;
            prim.Shape.PathShearX = obj.PrimData.PathShearX;
            prim.Shape.PathShearY = obj.PrimData.PathShearY;
            prim.Shape.PathSkew = obj.PrimData.PathSkew;
            prim.Shape.PathTaperX = obj.PrimData.PathTaperX;
            prim.Shape.PathTaperY = obj.PrimData.PathTaperY;

            prim.Shape.PathTwist = obj.PrimData.PathTwist;
            prim.Shape.PathTwistBegin = obj.PrimData.PathTwistBegin;
            prim.Shape.ProfileBegin = obj.PrimData.ProfileBegin;
            prim.Shape.ProfileCurve = obj.PrimData.profileCurve;
            prim.Shape.ProfileEnd = obj.PrimData.ProfileEnd;
            prim.Shape.ProfileHollow = obj.PrimData.ProfileHollow;

            prim.SitName = obj.Properties.SitName;
            //prim.SitOffset;
            //prim.SitRotation;
            prim.SoundFlags = (int)obj.SoundFlags;
            prim.SoundGain = obj.SoundGain;
            prim.SoundID = obj.Sound;
            prim.SoundRadius = obj.SoundRadius;
            prim.State = obj.PrimData.State;
            prim.Temporary = (obj.Flags & PrimFlags.Temporary) == PrimFlags.Temporary;
            prim.Text = obj.Text;
            prim.TextColor = obj.TextColor;
            prim.Textures = obj.Textures;
            //prim.TouchName;
            prim.UsePhysics = (obj.Flags & PrimFlags.Physics) == PrimFlags.Physics;
            prim.Velocity = obj.Velocity;

            return prim;
        }

        public Primitive ToPrimitive()
        {
            Primitive prim = new Primitive();
            prim.Properties = new Primitive.ObjectProperties();
            
            prim.Acceleration = this.Acceleration;
            prim.AngularVelocity = this.AngularVelocity;
            prim.ClickAction = (ClickAction)this.ClickAction;
            prim.Properties.CreationDate = this.CreationDate;
            prim.Properties.CreatorID = this.CreatorID;
            prim.Properties.Description = this.Description;
            if (this.DieAtEdge) prim.Flags |= PrimFlags.DieAtEdge;
            prim.Properties.FolderID = this.FolderID;
            prim.Properties.GroupID = this.GroupID;
            prim.ID = this.ID;
            prim.Properties.LastOwnerID = this.LastOwnerID;
            prim.LocalID = this.LocalID;
            prim.PrimData.Material = (Material)this.Material;
            prim.Properties.Name = this.Name;
            prim.OwnerID = this.OwnerID;
            prim.ParentID = this.ParentID;
            prim.PrimData.PCode = (PCode)this.PCode;
            prim.Properties.Permissions = new Permissions(this.PermsBase, this.PermsEveryone, this.PermsGroup, this.PermsNextOwner, this.PermsOwner);
            if (this.Phantom) prim.Flags |= PrimFlags.Phantom;
            prim.Position = this.Position;
            if (this.ReturnAtEdge) prim.Flags |= PrimFlags.ReturnAtEdge;
            prim.Rotation = this.Rotation;
            prim.Properties.SalePrice = this.SalePrice;
            prim.Properties.SaleType = (SaleType)this.SaleType;
            if (this.Sandbox) prim.Flags |= PrimFlags.Sandbox;
            prim.Scale = this.Scale;
            prim.SoundFlags = (SoundFlags)this.SoundFlags;
            prim.SoundGain = this.SoundGain;
            prim.Sound = this.SoundID;
            prim.SoundRadius = this.SoundRadius;
            prim.PrimData.State = (byte)this.State;
            if (this.Temporary) prim.Flags |= PrimFlags.Temporary;
            prim.Text = this.Text;
            prim.TextColor = this.TextColor;
            prim.Textures = this.Textures;
            if (this.UsePhysics) prim.Flags |= PrimFlags.Physics;
            prim.Velocity = this.Velocity;

            prim.PrimData.PathBegin = this.Shape.PathBegin;
            prim.PrimData.PathCurve = (PathCurve)this.Shape.PathCurve;
            prim.PrimData.PathEnd = this.Shape.PathEnd;
            prim.PrimData.PathRadiusOffset = this.Shape.PathRadiusOffset;
            prim.PrimData.PathRevolutions = this.Shape.PathRevolutions;
            prim.PrimData.PathScaleX = this.Shape.PathScaleX;
            prim.PrimData.PathScaleY = this.Shape.PathScaleY;
            prim.PrimData.PathShearX = this.Shape.PathShearX;
            prim.PrimData.PathShearY = this.Shape.PathShearY;
            prim.PrimData.PathSkew = this.Shape.PathSkew;
            prim.PrimData.PathTaperX = this.Shape.PathTaperX;
            prim.PrimData.PathTaperY = this.Shape.PathTaperY;
            prim.PrimData.PathTwist = this.Shape.PathTwist;
            prim.PrimData.PathTwistBegin = this.Shape.PathTwistBegin;
            prim.PrimData.ProfileBegin = this.Shape.ProfileBegin;
            prim.PrimData.profileCurve = (byte)this.Shape.ProfileCurve;
            prim.PrimData.ProfileEnd = this.Shape.ProfileEnd;
            prim.PrimData.ProfileHollow = this.Shape.ProfileHollow;

            if (this.Flexible != null)
            {
                prim.Flexible = new Primitive.FlexibleData();
                prim.Flexible.Drag = this.Flexible.Drag;
                prim.Flexible.Force = this.Flexible.Force;
                prim.Flexible.Gravity = this.Flexible.Gravity;
                prim.Flexible.Softness = this.Flexible.Softness;
                prim.Flexible.Tension = this.Flexible.Tension;
                prim.Flexible.Wind = this.Flexible.Wind;
            }

            if (this.Light != null)
            {
                prim.Light = new Primitive.LightData();
                prim.Light.Color = this.Light.Color;
                prim.Light.Cutoff = this.Light.Cutoff;
                prim.Light.Falloff = this.Light.Falloff;
                prim.Light.Intensity = this.Light.Intensity;
                prim.Light.Radius = this.Light.Radius;
            }

            if (this.Particles != null)
            {
                prim.ParticleSys = new Primitive.ParticleSystem();
                prim.ParticleSys.AngularVelocity = this.Particles.AngularVelocity;
                prim.ParticleSys.PartAcceleration = this.Particles.Acceleration;
                prim.ParticleSys.BurstPartCount = (byte)this.Particles.BurstParticleCount;
                prim.ParticleSys.BurstRate = this.Particles.BurstRadius;
                prim.ParticleSys.BurstRate = this.Particles.BurstRate;
                prim.ParticleSys.BurstSpeedMax = this.Particles.BurstSpeedMax;
                prim.ParticleSys.BurstSpeedMin = this.Particles.BurstSpeedMin;
                prim.ParticleSys.PartDataFlags = (Primitive.ParticleSystem.ParticleDataFlags)this.Particles.DataFlags;
                prim.ParticleSys.PartFlags = (uint)this.Particles.Flags;
                prim.ParticleSys.InnerAngle = this.Particles.InnerAngle;
                prim.ParticleSys.MaxAge = this.Particles.MaxAge;
                prim.ParticleSys.OuterAngle = this.Particles.OuterAngle;
                prim.ParticleSys.PartEndColor = this.Particles.ParticleEndColor;
                prim.ParticleSys.PartEndScaleX = this.Particles.ParticleEndScale.X;
                prim.ParticleSys.PartEndScaleY = this.Particles.ParticleEndScale.Y;
                prim.ParticleSys.MaxAge = this.Particles.ParticleMaxAge;
                prim.ParticleSys.PartStartColor = this.Particles.ParticleStartColor;
                prim.ParticleSys.PartStartScaleX = this.Particles.ParticleStartScale.X;
                prim.ParticleSys.PartStartScaleY = this.Particles.ParticleStartScale.Y;
                prim.ParticleSys.Pattern = (Primitive.ParticleSystem.SourcePattern)this.Particles.Pattern;
                prim.ParticleSys.StartAge = this.Particles.StartAge;
                prim.ParticleSys.Target = this.Particles.TargetID;
                prim.ParticleSys.Texture = this.Particles.TextureID;
            }

            if (this.Sculpt != null)
            {
                prim.Sculpt = new Primitive.SculptData();
                prim.Sculpt.SculptTexture = this.Sculpt.Texture;
                prim.Sculpt.Type = (SculptType)this.Sculpt.Type;
            }

            return prim;
        }
    }
}
