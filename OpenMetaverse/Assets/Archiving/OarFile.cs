﻿/*
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
using System.IO.Compression;
using System.Text;
using System.Xml;
using System.Threading;
using OpenMetaverse;

namespace OpenMetaverse.Assets
{
    public static class OarFile
    {
        public delegate void AssetLoadedCallback(Asset asset, long bytesRead, long totalBytes);
        public delegate void TerrainLoadedCallback(float[,] terrain, long bytesRead, long totalBytes);
        public delegate void SceneObjectLoadedCallback(AssetPrim linkset, long bytesRead, long totalBytes);
        public delegate void SettingsLoadedCallback(string regionName, RegionSettings settings);

        #region Archive Loading

        public static void UnpackageArchive(string filename, AssetLoadedCallback assetCallback, TerrainLoadedCallback terrainCallback,
            SceneObjectLoadedCallback objectCallback, SettingsLoadedCallback settingsCallback)
        {
            int successfulAssetRestores = 0;
            int failedAssetRestores = 0;

            try
            {
                using (FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    using (GZipStream loadStream = new GZipStream(fileStream, CompressionMode.Decompress))
                    {
                        TarArchiveReader archive = new TarArchiveReader(loadStream);

                        string filePath;
                        byte[] data;
                        TarArchiveReader.TarEntryType entryType;

                        while ((data = archive.ReadEntry(out filePath, out entryType)) != null)
                        {
                            if (filePath.StartsWith(ArchiveConstants.OBJECTS_PATH))
                            {
                                // Deserialize the XML bytes
                                if (objectCallback != null)
                                    LoadObjects(data, objectCallback, fileStream.Position, fileStream.Length);
                            }
                            else if (filePath.StartsWith(ArchiveConstants.ASSETS_PATH))
                            {
                                if (assetCallback != null)
                                {
                                    if (LoadAsset(filePath, data, assetCallback, fileStream.Position, fileStream.Length))
                                        successfulAssetRestores++;
                                    else
                                        failedAssetRestores++;
                                }
                            }
                            else if (filePath.StartsWith(ArchiveConstants.TERRAINS_PATH))
                            {
                                if (terrainCallback != null)
                                    LoadTerrain(filePath, data, terrainCallback, fileStream.Position, fileStream.Length);
                            }
                            else if (filePath.StartsWith(ArchiveConstants.SETTINGS_PATH))
                            {
                                if (settingsCallback != null)
                                    LoadRegionSettings(filePath, data, settingsCallback);
                            }
                        }

                        archive.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log("[OarFile] Error loading OAR file: " + e.Message, Helpers.LogLevel.Error);
                return;
            }

            if (failedAssetRestores > 0)
                Logger.Log(String.Format("[OarFile]: Failed to load {0} assets", failedAssetRestores), Helpers.LogLevel.Warning);
        }

        private static bool LoadAsset(string assetPath, byte[] data, AssetLoadedCallback assetCallback, long bytesRead, long totalBytes)
        {
            // Right now we're nastily obtaining the UUID from the filename
            string filename = assetPath.Remove(0, ArchiveConstants.ASSETS_PATH.Length);
            int i = filename.LastIndexOf(ArchiveConstants.ASSET_EXTENSION_SEPARATOR);

            if (i == -1)
            {
                Logger.Log(String.Format(
                    "[OarFile]: Could not find extension information in asset path {0} since it's missing the separator {1}.  Skipping",
                    assetPath, ArchiveConstants.ASSET_EXTENSION_SEPARATOR), Helpers.LogLevel.Warning);
                return false;
            }

            string extension = filename.Substring(i);
            UUID uuid;
            UUID.TryParse(filename.Remove(filename.Length - extension.Length), out uuid);

            if (ArchiveConstants.EXTENSION_TO_ASSET_TYPE.ContainsKey(extension))
            {
                AssetType assetType = ArchiveConstants.EXTENSION_TO_ASSET_TYPE[extension];
                Asset asset = null;

                switch (assetType)
                {
                    case AssetType.Animation:
                        asset = new AssetAnimation(uuid, data);
                        break;
                    case AssetType.Bodypart:
                        asset = new AssetBodypart(uuid, data);
                        break;
                    case AssetType.Clothing:
                        asset = new AssetClothing(uuid, data);
                        break;
                    case AssetType.Gesture:
                        asset = new AssetGesture(uuid, data);
                        break;
                    case AssetType.Landmark:
                        asset = new AssetLandmark(uuid, data);
                        break;
                    case AssetType.LSLBytecode:
                        asset = new AssetScriptBinary(uuid, data);
                        break;
                    case AssetType.LSLText:
                        asset = new AssetScriptText(uuid, data);
                        break;
                    case AssetType.Notecard:
                        asset = new AssetNotecard(uuid, data);
                        break;
                    case AssetType.Object:
                        asset = new AssetPrim(uuid, data);
                        break;
                    case AssetType.Sound:
                        asset = new AssetSound(uuid, data);
                        break;
                    case AssetType.Texture:
                        asset = new AssetTexture(uuid, data);
                        break;
                    default:
                        Logger.Log("[OarFile] Unhandled asset type " + assetType, Helpers.LogLevel.Error);
                        break;
                }

                if (asset != null)
                {
                    assetCallback(asset, bytesRead, totalBytes);
                    return true;
                }
            }

            Logger.Log("[OarFile] Failed to load asset", Helpers.LogLevel.Warning);
            return false;
        }

        private static bool LoadRegionSettings(string filePath, byte[] data, SettingsLoadedCallback settingsCallback)
        {
            RegionSettings settings = null;
            bool loaded = false;

            try
            {
                using (MemoryStream stream = new MemoryStream(data))
                    settings = RegionSettings.FromStream(stream);
                loaded = true;
            }
            catch (Exception ex)
            {
                Logger.Log("[OarFile] Failed to parse region settings file " + filePath + ": " + ex.Message, Helpers.LogLevel.Warning);
            }

            // Parse the region name out of the filename
            string regionName = Path.GetFileNameWithoutExtension(filePath);

            if (loaded)
                settingsCallback(regionName, settings);

            return loaded;
        }

        private static bool LoadTerrain(string filePath, byte[] data, TerrainLoadedCallback terrainCallback, long bytesRead, long totalBytes)
        {
            float[,] terrain = new float[256, 256];
            bool loaded = false;

            switch (Path.GetExtension(filePath))
            {
                case ".r32":
                case ".f32":
                    // RAW32
                    if (data.Length == 256 * 256 * 4)
                    {
                        int pos = 0;
                        for (int y = 0; y < 256; y++)
                        {
                            for (int x = 0; x < 256; x++)
                            {
                                terrain[y, x] = Utils.Clamp(Utils.BytesToFloat(data, pos), 0.0f, 255.0f);
                                pos += 4;
                            }
                        }

                        loaded = true;
                    }
                    else
                    {
                        Logger.Log("[OarFile] RAW32 terrain file " + filePath + " has the wrong number of bytes: " + data.Length,
                            Helpers.LogLevel.Warning);
                    }
                    break;
                case ".ter":
                    // Terragen
                case ".raw":
                    // LLRAW
                case ".jpg":
                case ".jpeg":
                    // JPG
                case ".bmp":
                    // BMP
                case ".png":
                    // PNG
                case ".gif":
                    // GIF
                case ".tif":
                case ".tiff":
                    // TIFF
                default:
                    Logger.Log("[OarFile] Unrecognized terrain format in " + filePath, Helpers.LogLevel.Warning);
                    break;
            }

            if (loaded)
                terrainCallback(terrain, bytesRead, totalBytes);

            return loaded;
        }

        public static void LoadObjects(byte[] objectData, SceneObjectLoadedCallback objectCallback, long bytesRead, long totalBytes)
        {
            XmlDocument doc = new XmlDocument();

            using (XmlTextReader reader = new XmlTextReader(new MemoryStream(objectData)))
            {
                reader.WhitespaceHandling = WhitespaceHandling.None;
                doc.Load(reader);
            }

            XmlNode rootNode = doc.FirstChild;

            if (rootNode.LocalName.Equals("scene"))
            {
                foreach (XmlNode node in rootNode.ChildNodes)
                {
                    AssetPrim linkset = new AssetPrim(node.OuterXml);
                    if (linkset != null)
                        objectCallback(linkset, bytesRead, totalBytes);
                }
            }
            else
            {
                AssetPrim linkset = new AssetPrim(rootNode.OuterXml);
                if (linkset != null)
                    objectCallback(linkset, bytesRead, totalBytes);
            }
        }

        #endregion Archive Loading

        #region Archive Saving

        public static void PackageArchive(string directoryName, string filename)
        {
            const string ARCHIVE_XML = "<?xml version=\"1.0\" encoding=\"utf-16\"?>\n<archive major_version=\"0\" minor_version=\"1\" />";

            TarArchiveWriter archive = new TarArchiveWriter(new GZipStream(new FileStream(filename, FileMode.Create), CompressionMode.Compress));

            // Create the archive.xml file
            archive.WriteFile("archive.xml", ARCHIVE_XML);

            // Add the assets
            string[] files = Directory.GetFiles(directoryName + "/" + ArchiveConstants.ASSETS_PATH);
            foreach (string file in files)
                archive.WriteFile(ArchiveConstants.ASSETS_PATH + Path.GetFileName(file), File.ReadAllBytes(file));

            // Add the objects
            files = Directory.GetFiles(directoryName + "/" + ArchiveConstants.OBJECTS_PATH);
            foreach (string file in files)
                archive.WriteFile(ArchiveConstants.OBJECTS_PATH + Path.GetFileName(file), File.ReadAllBytes(file));

            // Add the terrain(s)
            files = Directory.GetFiles(directoryName + "/" + ArchiveConstants.TERRAINS_PATH);
            foreach (string file in files)
                archive.WriteFile(ArchiveConstants.TERRAINS_PATH + Path.GetFileName(file), File.ReadAllBytes(file));

            // Add the parcels(s)
            files = Directory.GetFiles(directoryName + "/" + ArchiveConstants.LANDDATA_PATH);
            foreach (string file in files)
                archive.WriteFile(ArchiveConstants.LANDDATA_PATH + Path.GetFileName(file), File.ReadAllBytes(file));

            // Add the setting(s)
            files = Directory.GetFiles(directoryName + "/" + ArchiveConstants.SETTINGS_PATH);
            foreach (string file in files)
                archive.WriteFile(ArchiveConstants.SETTINGS_PATH + Path.GetFileName(file), File.ReadAllBytes(file));

            archive.Close();
        }

        public static void SaveTerrain(Simulator sim, string terrainPath)
        {
            if (Directory.Exists(terrainPath))
                Directory.Delete(terrainPath, true);
            Thread.Sleep(100);
            Directory.CreateDirectory(terrainPath);
            Thread.Sleep(100);
            FileInfo file = new FileInfo(Path.Combine(terrainPath, sim.Name + ".r32"));
            FileStream s = file.Open(FileMode.Create, FileAccess.Write);
            SaveTerrainStream(s, sim);

            s.Close();
        }

        private static void SaveTerrainStream(Stream s, Simulator sim)
        {
            BinaryWriter bs = new BinaryWriter(s);
            
            int y;
            for (y = 0; y < 256; y++)
            {
                int x;
                for (x = 0; x < 256; x++)
                {
                    float height;
                    sim.TerrainHeightAtPoint(x, y, out height);
                    bs.Write(height);
                }
            }

            bs.Close();
        }

        public static void SaveParcels(Simulator sim, string parcelPath)
        {
            if (Directory.Exists(parcelPath))
                Directory.Delete(parcelPath, true);
            Thread.Sleep(100);
            Directory.CreateDirectory(parcelPath);
            Thread.Sleep(100);
            sim.Parcels.ForEach((Parcel parcel) =>
                {
                    UUID globalID = UUID.Random();
                    SerializeParcel(parcel, globalID, Path.Combine(parcelPath, globalID + ".xml"));
                });
        }

        private static void SerializeParcel(Parcel parcel, UUID globalID, string filename)
        {
            StringWriter sw = new StringWriter();
            XmlTextWriter xtw = new XmlTextWriter(sw) { Formatting = Formatting.Indented };

            xtw.WriteStartDocument();
            xtw.WriteStartElement("LandData");

            xtw.WriteElementString("Area", Convert.ToString(parcel.Area));
            xtw.WriteElementString("AuctionID", Convert.ToString(parcel.AuctionID));
            xtw.WriteElementString("AuthBuyerID", parcel.AuthBuyerID.ToString());
            xtw.WriteElementString("Category", Convert.ToString((sbyte)parcel.Category));
            TimeSpan t = parcel.ClaimDate.ToUniversalTime() - Utils.Epoch;
            xtw.WriteElementString("ClaimDate", Convert.ToString((int)t.TotalSeconds));
            xtw.WriteElementString("ClaimPrice", Convert.ToString(parcel.ClaimPrice));
            xtw.WriteElementString("GlobalID", globalID.ToString());
            xtw.WriteElementString("GroupID", parcel.GroupID.ToString());
            xtw.WriteElementString("IsGroupOwned", Convert.ToString(parcel.IsGroupOwned));
            xtw.WriteElementString("Bitmap", Convert.ToBase64String(parcel.Bitmap));
            xtw.WriteElementString("Description", parcel.Desc);
            xtw.WriteElementString("Flags", Convert.ToString((uint)parcel.Flags));
            xtw.WriteElementString("LandingType", Convert.ToString((byte)parcel.Landing));
            xtw.WriteElementString("Name", parcel.Name);
            xtw.WriteElementString("Status", Convert.ToString((sbyte)parcel.Status));
            xtw.WriteElementString("LocalID", parcel.LocalID.ToString());
            xtw.WriteElementString("MediaAutoScale", Convert.ToString(parcel.Media.MediaAutoScale ? 1 : 0));
            xtw.WriteElementString("MediaID", parcel.Media.MediaID.ToString());
            xtw.WriteElementString("MediaURL", parcel.Media.MediaURL);
            xtw.WriteElementString("MusicURL", parcel.MusicURL);
            xtw.WriteElementString("OwnerID", parcel.OwnerID.ToString());

            xtw.WriteStartElement("ParcelAccessList");
            foreach (ParcelManager.ParcelAccessEntry pal in parcel.AccessBlackList)
            {
                xtw.WriteStartElement("ParcelAccessEntry");
                xtw.WriteElementString("AgentID", pal.AgentID.ToString());
                xtw.WriteElementString("Time", pal.Time.ToString("s"));
                xtw.WriteElementString("AccessList", Convert.ToString((uint)pal.Flags));
                xtw.WriteEndElement();
            }
            foreach (ParcelManager.ParcelAccessEntry pal in parcel.AccessWhiteList)
            {
                xtw.WriteStartElement("ParcelAccessEntry");
                xtw.WriteElementString("AgentID", pal.AgentID.ToString());
                xtw.WriteElementString("Time", pal.Time.ToString("s"));
                xtw.WriteElementString("AccessList", Convert.ToString((uint)pal.Flags));
                xtw.WriteEndElement();
            }
            xtw.WriteEndElement();

            xtw.WriteElementString("PassHours", Convert.ToString(parcel.PassHours));
            xtw.WriteElementString("PassPrice", Convert.ToString(parcel.PassPrice));
            xtw.WriteElementString("SalePrice", Convert.ToString(parcel.SalePrice));
            xtw.WriteElementString("SnapshotID", parcel.SnapshotID.ToString());
            xtw.WriteElementString("UserLocation", parcel.UserLocation.ToString());
            xtw.WriteElementString("UserLookAt", parcel.UserLookAt.ToString());
            xtw.WriteElementString("Dwell", "0");
            xtw.WriteElementString("OtherCleanTime", Convert.ToString(parcel.OtherCleanTime));

            xtw.WriteEndElement();

            xtw.Close();
            sw.Close();
            File.WriteAllText(filename, sw.ToString());
        }

        public static void SaveRegionSettings(Simulator sim, string settingsPath)
        {
            if (Directory.Exists(settingsPath))
                Directory.Delete(settingsPath, true);
            Thread.Sleep(100);
            Directory.CreateDirectory(settingsPath);
            Thread.Sleep(100);

            RegionSettings settings = new RegionSettings();
            //settings.AgentLimit;
            settings.AllowDamage = (sim.Flags & RegionFlags.AllowDamage) == RegionFlags.AllowDamage;
            //settings.AllowLandJoinDivide;
            settings.AllowLandResell = (sim.Flags & RegionFlags.BlockLandResell) != RegionFlags.BlockLandResell;
            settings.BlockFly = (sim.Flags & RegionFlags.NoFly) == RegionFlags.NoFly;
            settings.BlockLandShowInSearch = (sim.Flags & RegionFlags.BlockParcelSearch) == RegionFlags.BlockParcelSearch;
            settings.BlockTerraform = (sim.Flags & RegionFlags.BlockTerraform) == RegionFlags.BlockTerraform;
            settings.DisableCollisions = (sim.Flags & RegionFlags.SkipCollisions) == RegionFlags.SkipCollisions;
            settings.DisablePhysics = (sim.Flags & RegionFlags.SkipPhysics) == RegionFlags.SkipPhysics;
            settings.DisableScripts = (sim.Flags & RegionFlags.SkipScripts) == RegionFlags.SkipScripts;
            settings.FixedSun = (sim.Flags & RegionFlags.SunFixed) == RegionFlags.SunFixed;
            settings.MaturityRating = (int)(sim.Access & SimAccess.Mature & SimAccess.Adult & SimAccess.PG);
            //settings.ObjectBonus;
            settings.RestrictPushing = (sim.Flags & RegionFlags.RestrictPushObject) == RegionFlags.RestrictPushObject;
            settings.TerrainDetail0 = sim.TerrainDetail0;
            settings.TerrainDetail1 = sim.TerrainDetail1;
            settings.TerrainDetail2 = sim.TerrainDetail2;
            settings.TerrainDetail3 = sim.TerrainDetail3;
            settings.TerrainHeightRange00 = sim.TerrainHeightRange00;
            settings.TerrainHeightRange01 = sim.TerrainHeightRange01;
            settings.TerrainHeightRange10 = sim.TerrainHeightRange10;
            settings.TerrainHeightRange11 = sim.TerrainHeightRange11;
            settings.TerrainStartHeight00 = sim.TerrainStartHeight00;
            settings.TerrainStartHeight01 = sim.TerrainStartHeight01;
            settings.TerrainStartHeight10 = sim.TerrainStartHeight10;
            settings.TerrainStartHeight11 = sim.TerrainStartHeight11;
            //settings.UseEstateSun;
            settings.WaterHeight = sim.WaterHeight;

            settings.ToXML(Path.Combine(settingsPath, sim.Name + ".xml"));
        }

        public static void SavePrims(AssetManager manager, IList<AssetPrim> prims, string primsPath, string assetsPath)
        {
            Dictionary<UUID, UUID> textureList = new Dictionary<UUID, UUID>();

            // Delete all of the old linkset files
            try { Directory.Delete(primsPath, true); }
            catch (Exception) { }

            Thread.Sleep(100);
            // Create a new folder for the linkset files
            try { Directory.CreateDirectory(primsPath); }
            catch (Exception ex)
            {
                Logger.Log("Failed saving prims: " + ex.Message, Helpers.LogLevel.Error);
                return;
            }
            Thread.Sleep(100);
            try
            {
                foreach (AssetPrim assetPrim in prims)
                {
                    SavePrim(assetPrim, Path.Combine(primsPath, "Primitive_" + assetPrim.Parent.ID + ".xml"));

                    CollectTextures(assetPrim.Parent, textureList);
                    if (assetPrim.Children != null)
                    {
                        foreach (PrimObject child in assetPrim.Children)
                            CollectTextures(child, textureList);
                    }
                }

                SaveAssets(manager, AssetType.Texture, new List<UUID>(textureList.Keys), assetsPath);
            }
            catch
            {
            }
        }

        static void CollectTextures(PrimObject prim, Dictionary<UUID, UUID> textureList)
        {
            if (prim.Textures != null)
            {
                // Add all of the textures on this prim to the save list
                if (prim.Textures.DefaultTexture != null)
                    textureList[prim.Textures.DefaultTexture.TextureID] = prim.Textures.DefaultTexture.TextureID;

                if (prim.Textures.FaceTextures != null)
                {
                    for (int i = 0; i < prim.Textures.FaceTextures.Length; i++)
                    {
                        Primitive.TextureEntryFace face = prim.Textures.FaceTextures[i];
                        if (face != null)
                            textureList[face.TextureID] = face.TextureID;
                    }
                }
                if(prim.Sculpt != null && prim.Sculpt.Texture != UUID.Zero)
                    textureList[prim.Sculpt.Texture] = prim.Sculpt.Texture;
            }
        }

        public static void ClearAssetFolder(string assetsPath)
        {
            // Delete the assets folder
            try { Directory.Delete(assetsPath, true); }
            catch (Exception) { }
            Thread.Sleep(100);

            // Create a new assets folder
            try { Directory.CreateDirectory(assetsPath); }
            catch (Exception ex)
            {
                Logger.Log("Failed saving assets: " + ex.Message, Helpers.LogLevel.Error);
                return;
            }
            Thread.Sleep(100);
        }

        public static void SaveAssets(AssetManager assetManager, AssetType assetType, IList<UUID> assets, string assetsPath)
        {
            int count = 0;

            List<UUID> remainingTextures = new List<UUID>(assets);
            AutoResetEvent AllPropertiesReceived = new AutoResetEvent(false);
            for (int i = 0; i < assets.Count; i++)
            {
                UUID texture = assets[i];
                if(assetType == AssetType.Texture)
                {
                    assetManager.RequestImage(texture, (state, assetTexture) =>
                    {
                        string extension = string.Empty;

                        if (assetTexture == null)
                        {
                            Console.WriteLine("Missing asset " + texture);
                            return;
                        }

                        if (ArchiveConstants.ASSET_TYPE_TO_EXTENSION.ContainsKey(assetType))
                            extension = ArchiveConstants.ASSET_TYPE_TO_EXTENSION[assetType];

                        File.WriteAllBytes(Path.Combine(assetsPath, texture.ToString() + extension), assetTexture.AssetData);
                        remainingTextures.Remove(assetTexture.AssetID);
                        if (remainingTextures.Count == 0)
                            AllPropertiesReceived.Set();
                        ++count;
                    });
                }
                else
                {
                    assetManager.RequestAsset(texture, assetType, false, (transfer, asset) =>
                    {
                        string extension = string.Empty;

                        if (asset == null)
                        {
                            Console.WriteLine("Missing asset " + texture);
                            return;
                        }

                        if (ArchiveConstants.ASSET_TYPE_TO_EXTENSION.ContainsKey(assetType))
                            extension = ArchiveConstants.ASSET_TYPE_TO_EXTENSION[assetType];

                        File.WriteAllBytes(Path.Combine(assetsPath, texture.ToString() + extension), asset.AssetData);
                        remainingTextures.Remove(asset.AssetID);
                        if (remainingTextures.Count == 0)
                            AllPropertiesReceived.Set();
                        ++count;
                    });
                }

                Thread.Sleep(200);
                if (i % 5 == 0)
                    Thread.Sleep(250);
            }
            AllPropertiesReceived.WaitOne(5000 + 350 * assets.Count);

            Logger.Log("Copied " + count + " textures to the asset archive folder", Helpers.LogLevel.Info);
        }

        public static void SaveSimAssets(AssetManager assetManager, AssetType assetType, UUID assetID, UUID itemID, UUID primID, string assetsPath)
        {
            int count = 0;

            AutoResetEvent AllPropertiesReceived = new AutoResetEvent(false);
            assetManager.RequestAsset(assetID, itemID, primID, assetType, false, SourceType.SimInventoryItem, UUID.Random(), (transfer, asset) =>
            {
                string extension = string.Empty;

                if (ArchiveConstants.ASSET_TYPE_TO_EXTENSION.ContainsKey(assetType))
                    extension = ArchiveConstants.ASSET_TYPE_TO_EXTENSION[assetType];

                if (asset == null)
                {
                    AllPropertiesReceived.Set();
                    return;
                }
                File.WriteAllBytes(Path.Combine(assetsPath, assetID.ToString() + extension), asset.AssetData);
                ++count;
                AllPropertiesReceived.Set();
            });
            AllPropertiesReceived.WaitOne(5000);

            Logger.Log("Copied " + count + " textures to the asset archive folder", Helpers.LogLevel.Info);
        }

        static void SavePrim(AssetPrim prim, string filename)
        {
            try
            {
                using (StreamWriter stream = new StreamWriter(filename))
                {
                    XmlTextWriter writer = new XmlTextWriter(stream);
                    writer.Formatting = Formatting.Indented;
                    writer.Indentation = 4;
                    writer.IndentChar = ' ';
                    SOGToXml2(writer, prim);
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Failed saving linkset: " + ex.Message, Helpers.LogLevel.Error);
            }
        }

        public static void SOGToXml2(XmlTextWriter writer, AssetPrim prim)
        {
            writer.WriteStartElement(String.Empty, "SceneObjectGroup", String.Empty);
            SOPToXml(writer, prim.Parent, null);
            writer.WriteStartElement(String.Empty, "OtherParts", String.Empty);

            foreach (PrimObject child in prim.Children)
                SOPToXml(writer, child, prim.Parent);

            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        static void SOPToXml(XmlTextWriter writer, PrimObject prim, PrimObject parent)
        {
            writer.WriteStartElement("SceneObjectPart");
            writer.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
            writer.WriteAttributeString("xmlns:xsd", "http://www.w3.org/2001/XMLSchema");

            WriteUUID(writer, "CreatorID", prim.CreatorID);
            WriteUUID(writer, "FolderID", prim.FolderID);
            writer.WriteElementString("InventorySerial", (prim.Inventory != null) ? prim.Inventory.Serial.ToString() : "0");
            
            // FIXME: Task inventory
            writer.WriteStartElement("TaskInventory");
            if (prim.Inventory != null)
            {
                foreach (PrimObject.InventoryBlock.ItemBlock item in prim.Inventory.Items)
                {
                    writer.WriteStartElement("", "TaskInventoryItem", "");

                    WriteUUID(writer, "AssetID", item.AssetID);
                    writer.WriteElementString("BasePermissions", item.PermsBase.ToString());
                    writer.WriteElementString("CreationDate", (item.CreationDate.ToUniversalTime() - Utils.Epoch).TotalSeconds.ToString());
                    WriteUUID(writer, "CreatorID", item.CreatorID);
                    writer.WriteElementString("Description", item.Description);
                    writer.WriteElementString("EveryonePermissions", item.PermsEveryone.ToString());
                    writer.WriteElementString("Flags", item.Flags.ToString());
                    WriteUUID(writer, "GroupID", item.GroupID);
                    writer.WriteElementString("GroupPermissions", item.PermsGroup.ToString());
                    writer.WriteElementString("InvType", ((int)item.InvType).ToString());
                    WriteUUID(writer, "ItemID", item.ID);
                    WriteUUID(writer, "OldItemID", UUID.Zero);
                    WriteUUID(writer, "LastOwnerID", item.LastOwnerID);
                    writer.WriteElementString("Name", item.Name);
                    writer.WriteElementString("NextPermissions", item.PermsNextOwner.ToString());
                    WriteUUID(writer, "OwnerID", item.OwnerID);
                    writer.WriteElementString("CurrentPermissions", item.PermsOwner.ToString());
                    WriteUUID(writer, "ParentID", prim.ID);
                    WriteUUID(writer, "ParentPartID", prim.ID);
                    WriteUUID(writer, "PermsGranter", item.PermsGranterID);
                    writer.WriteElementString("PermsMask", "0");
                    writer.WriteElementString("Type", ((int)item.Type).ToString());
                    writer.WriteElementString("OwnerChanged", "false");

                    writer.WriteEndElement();
                }
            }
            writer.WriteEndElement();

            PrimFlags flags = PrimFlags.None;
            if (prim.UsePhysics) flags |= PrimFlags.Physics;
            if (prim.Phantom) flags |= PrimFlags.Phantom;
            if (prim.DieAtEdge) flags |= PrimFlags.DieAtEdge;
            if (prim.ReturnAtEdge) flags |= PrimFlags.ReturnAtEdge;
            if (prim.Temporary) flags |= PrimFlags.Temporary;
            if (prim.Sandbox) flags |= PrimFlags.Sandbox;
            writer.WriteElementString("ObjectFlags", ((int)flags).ToString());

            WriteUUID(writer, "UUID", prim.ID);
            writer.WriteElementString("LocalId", prim.LocalID.ToString());
            writer.WriteElementString("Name", prim.Name);
            writer.WriteElementString("Material", ((int)prim.Material).ToString());
            writer.WriteElementString("RegionHandle", prim.RegionHandle.ToString());
            writer.WriteElementString("ScriptAccessPin", prim.RemoteScriptAccessPIN.ToString());

            Vector3 groupPosition;
            if (parent == null)
                groupPosition = prim.Position;
            else
                groupPosition = parent.Position;

            WriteVector(writer, "GroupPosition", groupPosition);
            if (prim.ParentID == 0)
                WriteVector(writer, "OffsetPosition", Vector3.Zero);
            else
                WriteVector(writer, "OffsetPosition", prim.Position);
            WriteQuaternion(writer, "RotationOffset", prim.Rotation);
            WriteVector(writer, "Velocity", prim.Velocity);
            WriteVector(writer, "RotationalVelocity", Vector3.Zero);
            WriteVector(writer, "AngularVelocity", prim.AngularVelocity);
            WriteVector(writer, "Acceleration", prim.Acceleration);
            writer.WriteElementString("Description", prim.Description);
            writer.WriteStartElement("Color");
                writer.WriteElementString("R", prim.TextColor.R.ToString(Utils.EnUsCulture));
                writer.WriteElementString("G", prim.TextColor.G.ToString(Utils.EnUsCulture));
                writer.WriteElementString("B", prim.TextColor.B.ToString(Utils.EnUsCulture));
                writer.WriteElementString("A", prim.TextColor.G.ToString(Utils.EnUsCulture));
            writer.WriteEndElement();
            writer.WriteElementString("Text", prim.Text);
            writer.WriteElementString("SitName", prim.SitName);
            writer.WriteElementString("TouchName", prim.TouchName);

            writer.WriteElementString("LinkNum", prim.LinkNumber.ToString());
            writer.WriteElementString("ClickAction", prim.ClickAction.ToString());
            writer.WriteStartElement("Shape");

            writer.WriteElementString("PathBegin", Primitive.PackBeginCut(prim.Shape.PathBegin).ToString());
            writer.WriteElementString("PathCurve", prim.Shape.PathCurve.ToString());
            writer.WriteElementString("PathEnd", Primitive.PackEndCut(prim.Shape.PathEnd).ToString());
            writer.WriteElementString("PathRadiusOffset", Primitive.PackPathTwist(prim.Shape.PathRadiusOffset).ToString());
            writer.WriteElementString("PathRevolutions", Primitive.PackPathRevolutions(prim.Shape.PathRevolutions).ToString());
            writer.WriteElementString("PathScaleX", Primitive.PackPathScale(prim.Shape.PathScaleX).ToString());
            writer.WriteElementString("PathScaleY", Primitive.PackPathScale(prim.Shape.PathScaleY).ToString());
            writer.WriteElementString("PathShearX", ((byte)Primitive.PackPathShear(prim.Shape.PathShearX)).ToString());
            writer.WriteElementString("PathShearY", ((byte)Primitive.PackPathShear(prim.Shape.PathShearY)).ToString());
            writer.WriteElementString("PathSkew", Primitive.PackPathTwist(prim.Shape.PathSkew).ToString());
            writer.WriteElementString("PathTaperX", Primitive.PackPathTaper(prim.Shape.PathTaperX).ToString());
            writer.WriteElementString("PathTaperY", Primitive.PackPathTaper(prim.Shape.PathTaperY).ToString());
            writer.WriteElementString("PathTwist", Primitive.PackPathTwist(prim.Shape.PathTwist).ToString());
            writer.WriteElementString("PathTwistBegin", Primitive.PackPathTwist(prim.Shape.PathTwistBegin).ToString());
            writer.WriteElementString("PCode", prim.PCode.ToString());
            writer.WriteElementString("ProfileBegin", Primitive.PackBeginCut(prim.Shape.ProfileBegin).ToString());
            writer.WriteElementString("ProfileEnd", Primitive.PackEndCut(prim.Shape.ProfileEnd).ToString());
            writer.WriteElementString("ProfileHollow", Primitive.PackProfileHollow(prim.Shape.ProfileHollow).ToString());
            WriteVector(writer, "Scale", prim.Scale);
            writer.WriteElementString("State", prim.State.ToString());

            AssetPrim.ProfileShape shape = (AssetPrim.ProfileShape)(prim.Shape.ProfileCurve & 0x0F);
            HoleType hole = (HoleType)(prim.Shape.ProfileCurve & 0xF0);
            writer.WriteElementString("ProfileShape", shape.ToString());
            writer.WriteElementString("HollowShape", hole.ToString());
            writer.WriteElementString("ProfileCurve", prim.Shape.ProfileCurve.ToString());

            writer.WriteStartElement("TextureEntry");

            byte[] te;
            if (prim.Textures != null)
                te = prim.Textures.GetBytes();
            else
                te = Utils.EmptyBytes;

            writer.WriteBase64(te, 0, te.Length);
            writer.WriteEndElement();

            // FIXME: ExtraParams
            writer.WriteStartElement("ExtraParams"); writer.WriteEndElement();

            writer.WriteEndElement();

            WriteVector(writer, "Scale", prim.Scale);
            writer.WriteElementString("UpdateFlag", "0");
            WriteVector(writer, "SitTargetOrientation", Vector3.UnitZ); // TODO: Is this really a vector and not a quaternion?
            WriteVector(writer, "SitTargetPosition", prim.SitOffset);
            WriteVector(writer, "SitTargetPositionLL", prim.SitOffset);
            WriteQuaternion(writer, "SitTargetOrientationLL", prim.SitRotation);
            writer.WriteElementString("ParentID", prim.ParentID.ToString());
            writer.WriteElementString("CreationDate", ((int)Utils.DateTimeToUnixTime(prim.CreationDate)).ToString());
            writer.WriteElementString("Category", "0");
            writer.WriteElementString("SalePrice", prim.SalePrice.ToString());
            writer.WriteElementString("ObjectSaleType", ((int)prim.SaleType).ToString());
            writer.WriteElementString("OwnershipCost", "0");
            WriteUUID(writer, "GroupID", prim.GroupID);
            WriteUUID(writer, "OwnerID", prim.OwnerID);
            WriteUUID(writer, "LastOwnerID", prim.LastOwnerID);
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
            writer.WriteElementString("X", vec.X.ToString(Utils.EnUsCulture));
            writer.WriteElementString("Y", vec.Y.ToString(Utils.EnUsCulture));
            writer.WriteElementString("Z", vec.Z.ToString(Utils.EnUsCulture));
            writer.WriteEndElement();
        }

        static void WriteQuaternion(XmlTextWriter writer, string name, Quaternion quat)
        {
            writer.WriteStartElement(name);
            writer.WriteElementString("X", quat.X.ToString(Utils.EnUsCulture));
            writer.WriteElementString("Y", quat.Y.ToString(Utils.EnUsCulture));
            writer.WriteElementString("Z", quat.Z.ToString(Utils.EnUsCulture));
            writer.WriteElementString("W", quat.W.ToString(Utils.EnUsCulture));
            writer.WriteEndElement();
        }

        #endregion Archive Saving
    }
}
