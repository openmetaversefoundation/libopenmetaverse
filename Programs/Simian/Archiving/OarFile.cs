using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;
using OpenMetaverse;

namespace Simian
{
    public class Linkset
    {
        public SimulationObject Parent;
        public List<SimulationObject> Children = new List<SimulationObject>();
    }

    public class OarFile
    {
        enum ProfileShape : byte
        {
            Circle = 0,
            Square = 1,
            IsometricTriangle = 2,
            EquilateralTriangle = 3,
            RightTriangle = 4,
            HalfCircle = 5
        }

        public static void PackageArchive(string directoryName, string filename)
        {
            const string ARCHIVE_XML = "<?xml version=\"1.0\" encoding=\"utf-16\"?>\n<archive major_version=\"0\" minor_version=\"1\" />";

            TarArchiveWriter archive = new TarArchiveWriter(new GZipStream(new FileStream(filename, FileMode.Create), CompressionMode.Compress));

            // Create the archive.xml file
            archive.WriteFile("archive.xml", ARCHIVE_XML);

            // Add the assets
            string[] files = Directory.GetFiles(directoryName + "/assets");
            foreach (string file in files)
                archive.WriteFile("assets/" + Path.GetFileName(file), File.ReadAllBytes(file));

            // Add the objects
            files = Directory.GetFiles(directoryName + "/objects");
            foreach (string file in files)
                archive.WriteFile("objects/" + Path.GetFileName(file), File.ReadAllBytes(file));

            // Add the terrain(s)
            files = Directory.GetFiles(directoryName + "/terrains");
            foreach (string file in files)
                archive.WriteFile("terrains/" + Path.GetFileName(file), File.ReadAllBytes(file));

            archive.Close();
        }

        public static void SavePrims(Simian server, string path)
        {
            // Delete all of the old linkset files
            try
            {
                Directory.Delete(path, true);
                Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                Logger.Log("Failed saving prims: " + ex.Message, Helpers.LogLevel.Error);
                return;
            }

            // Copy the double dictionary to a temporary list for iterating
            List<SimulationObject> primList = new List<SimulationObject>();
            server.Scene.ForEachObject(delegate(SimulationObject prim)
            {
                if (!(prim.Prim is Avatar))
                    primList.Add(prim);
            });

            foreach (SimulationObject p in primList)
            {
                if (p.Prim.ParentID == 0)
                {
                    Linkset linkset = new Linkset();
                    linkset.Parent = p;

                    server.Scene.ForEachObject(delegate(SimulationObject q)
                    {
                        if (q.Prim.ParentID == p.Prim.LocalID)
                            linkset.Children.Add(q);
                    });

                    SaveLinkset(linkset, path + "/Primitive_" + linkset.Parent.Prim.ID.ToString() + ".xml");
                }
            }
        }

        static void SaveLinkset(Linkset linkset, string filename)
        {
            try
            {
                using (StreamWriter stream = new StreamWriter(filename))
                {
                    XmlTextWriter writer = new XmlTextWriter(stream);
                    SOGToXml2(writer, linkset);
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Failed saving linkset: " + ex.Message, Helpers.LogLevel.Error);
            }
        }

        static void SOGToXml2(XmlTextWriter writer, Linkset linkset)
        {
            writer.WriteStartElement(String.Empty, "SceneObjectGroup", String.Empty);
            SOPToXml(writer, linkset.Parent, null);
            writer.WriteStartElement(String.Empty, "OtherParts", String.Empty);

            foreach (SimulationObject child in linkset.Children)
                SOPToXml(writer, child, linkset.Parent);

            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        static void SOPToXml(XmlTextWriter writer, SimulationObject prim, SimulationObject parent)
        {
            writer.WriteStartElement("SceneObjectPart");
            writer.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
            writer.WriteAttributeString("xmlns:xsd", "http://www.w3.org/2001/XMLSchema");

            WriteUUID(writer, "CreatorID", prim.Prim.Properties.CreatorID);
            WriteUUID(writer, "FolderID", prim.Prim.Properties.FolderID);
            writer.WriteElementString("InventorySerial", prim.Prim.Properties.InventorySerial.ToString());
            writer.WriteStartElement("TaskInventory"); writer.WriteEndElement();
            writer.WriteElementString("ObjectFlags", ((int)prim.Prim.Flags).ToString());
            WriteUUID(writer, "UUID", prim.Prim.ID);
            writer.WriteElementString("LocalId", prim.Prim.LocalID.ToString());
            writer.WriteElementString("Name", prim.Prim.Properties.Name);
            writer.WriteElementString("Material", ((int)prim.Prim.PrimData.Material).ToString());
            writer.WriteElementString("RegionHandle", prim.Prim.RegionHandle.ToString());
            writer.WriteElementString("ScriptAccessPin", "0");

            Vector3 groupPosition;
            if (parent == null)
                groupPosition = prim.Prim.Position;
            else
                groupPosition = parent.Prim.Position;

            WriteVector(writer, "GroupPosition", groupPosition);
            WriteVector(writer, "OffsetPosition", groupPosition - prim.Prim.Position);
            WriteQuaternion(writer, "RotationOffset", prim.Prim.Rotation);
            WriteVector(writer, "Velocity", Vector3.Zero);
            WriteVector(writer, "RotationalVelocity", Vector3.Zero);
            WriteVector(writer, "AngularVelocity", prim.Prim.AngularVelocity);
            WriteVector(writer, "Acceleration", Vector3.Zero);
            writer.WriteElementString("Description", prim.Prim.Properties.Description);
            writer.WriteStartElement("Color");
            writer.WriteElementString("R", prim.Prim.TextColor.R.ToString());
            writer.WriteElementString("G", prim.Prim.TextColor.G.ToString());
            writer.WriteElementString("B", prim.Prim.TextColor.B.ToString());
            writer.WriteElementString("A", prim.Prim.TextColor.G.ToString());
            writer.WriteEndElement();
            writer.WriteElementString("Text", prim.Prim.Text);
            writer.WriteElementString("SitName", prim.Prim.Properties.SitName);
            writer.WriteElementString("TouchName", prim.Prim.Properties.TouchName);

            writer.WriteElementString("LinkNum", prim.LinkNumber.ToString());
            writer.WriteElementString("ClickAction", ((int)prim.Prim.ClickAction).ToString());
            writer.WriteStartElement("Shape");

            writer.WriteElementString("PathBegin", Primitive.PackBeginCut(prim.Prim.PrimData.PathBegin).ToString());
            writer.WriteElementString("PathCurve", ((byte)prim.Prim.PrimData.PathCurve).ToString());
            writer.WriteElementString("PathEnd", Primitive.PackEndCut(prim.Prim.PrimData.PathEnd).ToString());
            writer.WriteElementString("PathRadiusOffset", Primitive.PackPathTwist(prim.Prim.PrimData.PathRadiusOffset).ToString());
            writer.WriteElementString("PathRevolutions", Primitive.PackPathRevolutions(prim.Prim.PrimData.PathRevolutions).ToString());
            writer.WriteElementString("PathScaleX", Primitive.PackPathScale(prim.Prim.PrimData.PathScaleX).ToString());
            writer.WriteElementString("PathScaleY", Primitive.PackPathScale(prim.Prim.PrimData.PathScaleY).ToString());
            writer.WriteElementString("PathShearX", ((byte)Primitive.PackPathShear(prim.Prim.PrimData.PathShearX)).ToString());
            writer.WriteElementString("PathShearY", ((byte)Primitive.PackPathShear(prim.Prim.PrimData.PathShearY)).ToString());
            writer.WriteElementString("PathSkew", Primitive.PackPathTwist(prim.Prim.PrimData.PathSkew).ToString());
            writer.WriteElementString("PathTaperX", Primitive.PackPathTaper(prim.Prim.PrimData.PathTaperX).ToString());
            writer.WriteElementString("PathTaperY", Primitive.PackPathTaper(prim.Prim.PrimData.PathTaperY).ToString());
            writer.WriteElementString("PathTwist", Primitive.PackPathTwist(prim.Prim.PrimData.PathTwist).ToString());
            writer.WriteElementString("PathTwistBegin", Primitive.PackPathTwist(prim.Prim.PrimData.PathTwistBegin).ToString());
            writer.WriteElementString("PCode", ((byte)prim.Prim.PrimData.PCode).ToString());
            writer.WriteElementString("ProfileBegin", Primitive.PackBeginCut(prim.Prim.PrimData.ProfileBegin).ToString());
            writer.WriteElementString("ProfileEnd", Primitive.PackEndCut(prim.Prim.PrimData.ProfileEnd).ToString());
            writer.WriteElementString("ProfileHollow", Primitive.PackProfileHollow(prim.Prim.PrimData.ProfileHollow).ToString());
            WriteVector(writer, "Scale", prim.Prim.Scale);
            writer.WriteElementString("State", prim.Prim.PrimData.State.ToString());

            ProfileShape shape = (ProfileShape)prim.Prim.PrimData.ProfileCurve;
            writer.WriteElementString("ProfileShape", shape.ToString());
            writer.WriteElementString("HollowShape", prim.Prim.PrimData.ProfileHole.ToString());
            writer.WriteElementString("ProfileCurve", prim.Prim.PrimData.profileCurve.ToString());

            writer.WriteStartElement("TextureEntry");

            byte[] te;
            if (prim.Prim.Textures != null)
                te = prim.Prim.Textures.GetBytes();
            else
                te = Utils.EmptyBytes;

            writer.WriteBase64(te, 0, te.Length);
            writer.WriteEndElement();

            // FIXME: ExtraParams
            writer.WriteStartElement("ExtraParams"); writer.WriteEndElement();

            writer.WriteEndElement();

            WriteVector(writer, "Scale", prim.Prim.Scale);
            writer.WriteElementString("UpdateFlag", "0");
            WriteVector(writer, "SitTargetOrientation", Vector3.UnitZ); // TODO: Is this really a vector and not a quaternion?
            WriteVector(writer, "SitTargetPosition", prim.SitPosition);
            WriteVector(writer, "SitTargetPositionLL", prim.SitPosition);
            WriteQuaternion(writer, "SitTargetOrientationLL", prim.SitRotation);
            writer.WriteElementString("ParentID", prim.Prim.ParentID.ToString());
            writer.WriteElementString("CreationDate", ((int)Utils.DateTimeToUnixTime(prim.Prim.Properties.CreationDate)).ToString());
            writer.WriteElementString("Category", ((int)prim.Prim.Properties.Category).ToString());
            writer.WriteElementString("SalePrice", prim.Prim.Properties.SalePrice.ToString());
            writer.WriteElementString("ObjectSaleType", ((int)prim.Prim.Properties.SaleType).ToString());
            writer.WriteElementString("OwnershipCost", prim.Prim.Properties.OwnershipCost.ToString());
            WriteUUID(writer, "GroupID", prim.Prim.GroupID);
            WriteUUID(writer, "OwnerID", prim.Prim.OwnerID);
            WriteUUID(writer, "LastOwnerID", prim.Prim.Properties.LastOwnerID);
            writer.WriteElementString("BaseMask", ((uint)PermissionMask.All).ToString());
            writer.WriteElementString("OwnerMask", ((uint)PermissionMask.All).ToString());
            writer.WriteElementString("GroupMask", ((uint)PermissionMask.All).ToString());
            writer.WriteElementString("EveryoneMask", ((uint)PermissionMask.All).ToString());
            writer.WriteElementString("NextOwnerMask", ((uint)PermissionMask.All).ToString());
            writer.WriteElementString("Flags", "None");
            WriteUUID(writer, "SitTargetAvatar", UUID.Zero);

            writer.WriteEndElement();
        }

        static void WriteUUID(XmlTextWriter writer, string name, UUID id)
        {
            writer.WriteStartElement(name);
            writer.WriteElementString("UUID", id.ToString());
            writer.WriteEndElement();
        }

        static void WriteVector(XmlTextWriter writer, string name, Vector3 vec)
        {
            writer.WriteStartElement(name);
            writer.WriteElementString("X", vec.X.ToString());
            writer.WriteElementString("Y", vec.Y.ToString());
            writer.WriteElementString("Z", vec.Z.ToString());
            writer.WriteEndElement();
        }

        static void WriteQuaternion(XmlTextWriter writer, string name, Quaternion quat)
        {
            writer.WriteStartElement(name);
            writer.WriteElementString("X", quat.X.ToString());
            writer.WriteElementString("Y", quat.Y.ToString());
            writer.WriteElementString("Z", quat.Z.ToString());
            writer.WriteElementString("W", quat.W.ToString());
            writer.WriteEndElement();
        }
    }
}
