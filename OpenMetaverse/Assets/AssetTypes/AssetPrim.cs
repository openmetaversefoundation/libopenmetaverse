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
using System.Collections.Generic;
using System.IO;
using System.Xml;
using OpenMetaverse;
using OpenMetaverse.StructuredData;

namespace OpenMetaverse.Assets
{
    /// <summary>
    /// Represents a primitive asset
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
            // FIXME:
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool Decode()
        {
            // FIXME:
            return false;
        }

        public string EncodeXml()
        {
            TextWriter textWriter = new StringWriter();
            XmlTextWriter xmlWriter = new XmlTextWriter(textWriter);
            OarFile.SOGToXml2(xmlWriter, this);
            xmlWriter.Flush();
            return textWriter.ToString();
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
                                    Logger.Log("Found unexpected prim XML element " + reader.Name, Helpers.LogLevel.Warning);
                                    reader.Read();
                                }
                                break;
                            case XmlNodeType.EndElement:
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
            obj.CreatorIdentity = ReadUUID(reader, "CreatorID").ToString();
            //warning CS0219: The variable `folderID' is assigned but its value is never used
            //UUID folderID = ReadUUID(reader, "FolderID");
            obj.Inventory.Serial = reader.ReadElementContentAsInt("InventorySerial", String.Empty);
            
            // FIXME: Parse TaskInventory
            obj.Inventory.Items = new PrimObject.InventoryBlock.ItemBlock[0];
            reader.ReadInnerXml();

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
            
            reader.ReadInnerXml(); // RegionHandle

            obj.RemoteScriptAccessPIN = reader.ReadElementContentAsInt("ScriptAccessPin", String.Empty);

            Vector3 groupPosition = ReadVector(reader, "GroupPosition");
            Vector3 offsetPosition = ReadVector(reader, "OffsetPosition");
            obj.Rotation = ReadQuaternion(reader, "RotationOffset");
            obj.Velocity = ReadVector(reader, "Velocity");
            //warning CS0219: The variable `rotationalVelocity' is assigned but its value is never used
            //Vector3 rotationalVelocity = ReadVector(reader, "RotationalVelocity");
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
            Primitive.TextureEntry te = new Primitive.TextureEntry(teData, 0, teData.Length);
            obj.Faces = FromTextureEntry(te);

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

            reader.ReadInnerXml(); // FlexiTension
            reader.ReadInnerXml(); // FlexiDrag
            reader.ReadInnerXml(); // FlexiGravity
            reader.ReadInnerXml(); // FlexiWind
            reader.ReadInnerXml(); // FlexiForceX
            reader.ReadInnerXml(); // FlexiForceY
            reader.ReadInnerXml(); // FlexiForceZ
            reader.ReadInnerXml(); // LightColorR
            reader.ReadInnerXml(); // LightColorG
            reader.ReadInnerXml(); // LightColorB
            reader.ReadInnerXml(); // LightColorA
            reader.ReadInnerXml(); // LightRadius
            reader.ReadInnerXml(); // LightCutoff
            reader.ReadInnerXml(); // LightFalloff
            reader.ReadInnerXml(); // LightIntensity
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
            //obj.Category = reader.ReadElementContentAsInt("Category", String.Empty);
            //warning CS0219: The variable `category' is assigned but its value is never used
            //int category = reader.ReadElementContentAsInt("Category", String.Empty);
            obj.SalePrice = reader.ReadElementContentAsInt("SalePrice", String.Empty);
            obj.SaleType = reader.ReadElementContentAsInt("ObjectSaleType", String.Empty);
            //warning CS0219: The variable `ownershipCost' is assigned but its value is never used
            //int ownershipCost = reader.ReadElementContentAsInt("OwnershipCost", String.Empty);
            obj.GroupIdentity = ReadUUID(reader, "GroupID").ToString();
            obj.OwnerIdentity = ReadUUID(reader, "OwnerID").ToString();
            obj.LastOwnerIdentity = ReadUUID(reader, "LastOwnerID").ToString();
            obj.PermsBase = (uint)reader.ReadElementContentAsInt("BaseMask", String.Empty);
            obj.PermsOwner = (uint)reader.ReadElementContentAsInt("OwnerMask", String.Empty);
            obj.PermsGroup = (uint)reader.ReadElementContentAsInt("GroupMask", String.Empty);
            obj.PermsEveryone = (uint)reader.ReadElementContentAsInt("EveryoneMask", String.Empty);
            obj.PermsNextOwner = (uint)reader.ReadElementContentAsInt("NextOwnerMask", String.Empty);

            reader.ReadInnerXml(); // Flags
            reader.ReadInnerXml(); // CollisionSound
            reader.ReadInnerXml(); // CollisionSoundVolume

            reader.ReadEndElement();

            if (obj.ParentID == 0)
                obj.Position = groupPosition;
            else
                obj.Position = offsetPosition;

            return obj;
        }

        public static PrimObject.FaceBlock[] FromTextureEntry(Primitive.TextureEntry te)
        {
            // FIXME:
            return new PrimObject.FaceBlock[0];
        }

        public static Primitive.TextureEntry ToTextureEntry(PrimObject.FaceBlock[] faces)
        {
            // FIXME:
            return new Primitive.TextureEntry(UUID.Zero);
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

        public class FaceBlock
        {
            public int FaceIndex;
            public UUID ImageID;
            public Color4 Color;
            public float ScaleS;
            public float ScaleT;
            public float OffsetS;
            public float OffsetT;
            public float ImageRot;
            public int Bump;
            public bool FullBright;
            public int MediaFlags;

            public OSDMap Serialize()
            {
                OSDMap map = new OSDMap();
                map["face_index"] = OSD.FromInteger(FaceIndex);
                map["image_id"] = OSD.FromUUID(ImageID);
                map["color"] = OSD.FromColor4(Color);
                map["scale_s"] = OSD.FromReal(ScaleS);
                map["scale_t"] = OSD.FromReal(ScaleT);
                map["offset_s"] = OSD.FromReal(OffsetS);
                map["offset_t"] = OSD.FromReal(OffsetT);
                map["image_rot"] = OSD.FromReal(ImageRot);
                map["bump"] = OSD.FromInteger(Bump);
                map["full_bright"] = OSD.FromBoolean(FullBright);
                map["media_flags"] = OSD.FromInteger(MediaFlags);
                return map;
            }

            public void Deserialize(OSDMap map)
            {
                FaceIndex = map["face_index"].AsInteger();
                ImageID = map["image_id"].AsUUID();
                Color = map["color"].AsColor4();
                ScaleS = (float)map["scale_s"].AsReal();
                ScaleT = (float)map["scale_t"].AsReal();
                OffsetS = (float)map["offset_s"].AsReal();
                OffsetT = (float)map["offset_t"].AsReal();
                ImageRot = (float)map["image_rot"].AsReal();
                Bump = map["bump"].AsInteger();
                FullBright = map["full_bright"].AsBoolean();
                MediaFlags = map["media_flags"].AsInteger();
            }
        }

        public class InventoryBlock
        {
            public class ItemBlock
            {
                public UUID ID;
                public string Name;
                public string OwnerIdentity;
                public string CreatorIdentity;
                public string GroupIdentity;
                public UUID AssetID;
                public string ContentType;
                public string Description;
                public uint PermsBase;
                public uint PermsOwner;
                public uint PermsGroup;
                public uint PermsEveryone;
                public uint PermsNextOwner;
                public int SalePrice;
                public int SaleType;
                public int Flags;
                public DateTime CreationDate;

                public OSDMap Serialize()
                {
                    OSDMap map = new OSDMap();
                    map["id"] = OSD.FromUUID(ID);
                    map["name"] = OSD.FromString(Name);
                    map["owner_identity"] = OSD.FromString(OwnerIdentity);
                    map["creator_identity"] = OSD.FromString(CreatorIdentity);
                    map["group_identity"] = OSD.FromString(GroupIdentity);
                    map["asset_id"] = OSD.FromUUID(AssetID);
                    map["content_type"] = OSD.FromString(ContentType);
                    map["description"] = OSD.FromString(Description);
                    map["perms_base"] = OSD.FromInteger(PermsBase);
                    map["perms_owner"] = OSD.FromInteger(PermsOwner);
                    map["perms_group"] = OSD.FromInteger(PermsGroup);
                    map["perms_everyone"] = OSD.FromInteger(PermsEveryone);
                    map["perms_next_owner"] = OSD.FromInteger(PermsNextOwner);
                    map["sale_price"] = OSD.FromInteger(SalePrice);
                    map["sale_type"] = OSD.FromInteger(SaleType);
                    map["flags"] = OSD.FromInteger(Flags);
                    map["creation_date"] = OSD.FromDate(CreationDate);
                    return map;
                }

                public void Deserialize(OSDMap map)
                {
                    ID = map["id"].AsUUID();
                    Name = map["name"].AsString();
                    OwnerIdentity = map["owner_identity"].AsString();
                    CreatorIdentity = map["creator_identity"].AsString();
                    GroupIdentity = map["group_identity"].AsString();
                    AssetID = map["asset_id"].AsUUID();
                    ContentType = map["content_type"].AsString();
                    Description = map["description"].AsString();
                    PermsBase = (uint)map["perms_base"].AsInteger();
                    PermsOwner = (uint)map["perms_owner"].AsInteger();
                    PermsGroup = (uint)map["perms_group"].AsInteger();
                    PermsEveryone = (uint)map["perms_everyone"].AsInteger();
                    PermsNextOwner = (uint)map["perms_next_owner"].AsInteger();
                    SalePrice = map["sale_price"].AsInteger();
                    SaleType = map["sale_type"].AsInteger();
                    Flags = map["flags"].AsInteger();
                    CreationDate = map["creation_date"].AsDate();
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
        public string CreatorIdentity;
        public string OwnerIdentity;
        public string LastOwnerIdentity;
        public string GroupIdentity;
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
        public FlexibleBlock Flexible;
        public LightBlock Light;
        public SculptBlock Sculpt;
        public ParticlesBlock Particles;
        public ShapeBlock Shape;
        public FaceBlock[] Faces;
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
            map["creator_identity"] = OSD.FromString(CreatorIdentity);
            map["owner_identity"] = OSD.FromString(OwnerIdentity);
            map["last_owner_identity"] = OSD.FromString(LastOwnerIdentity);
            map["group_identity"] = OSD.FromString(GroupIdentity);
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
            if (Faces != null)
            {
                OSDArray array = new OSDArray(Faces.Length);
                for (int i = 0; i < Faces.Length; i++)
                    array.Add(Faces[i].Serialize());
                map["faces"] = array;
            }
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
            CreatorIdentity = map["creator_identity"].AsString();
            OwnerIdentity = map["owner_identity"].AsString();
            LastOwnerIdentity = map["last_owner_identity"].AsString();
            GroupIdentity = map["group_identity"].AsString();
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
    }
}
