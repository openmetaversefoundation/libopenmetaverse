using System;
using System.Collections.Generic;
using System.IO;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.Imaging;

namespace Simian.Extensions
{
    public class AssetManager : IExtension<Simian>, IAssetProvider
    {
        public const string UPLOAD_DIR = "uploadedAssets";

        Simian server;
        Dictionary<UUID, Asset> AssetStore = new Dictionary<UUID, Asset>();
        string UploadDir;

        public AssetManager()
        {
        }

        public void Start(Simian server)
        {
            this.server = server;

            UploadDir = Path.Combine(Simian.DATA_DIR, UPLOAD_DIR);

            // Try to create the data directories if they don't already exist
            if (!Directory.Exists(Simian.DATA_DIR))
            {
                try { Directory.CreateDirectory(Simian.DATA_DIR); }
                catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Warning, ex); }
            }
            if (!Directory.Exists(UploadDir))
            {
                try { Directory.CreateDirectory(UploadDir); }
                catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Warning, ex); }
            }

            LoadAssets(Simian.DATA_DIR);
            LoadAssets(UploadDir);
        }

        public void Stop()
        {
        }

        public void StoreAsset(Asset asset)
        {
            if (asset is AssetTexture)
            {
                AssetTexture texture = (AssetTexture)asset;

                if (texture.DecodeLayerBoundaries())
                {
                    lock (AssetStore)
                        AssetStore[asset.AssetID] = texture;
                    if (!asset.Temporary)
                        SaveAsset(texture);
                }
                else
                {
                    Logger.Log(String.Format("Failed to decoded layer boundaries on texture {0}", texture.AssetID),
                        Helpers.LogLevel.Warning);
                }
            }
            else
            {
                if (asset.Decode())
                {
                    lock (AssetStore)
                        AssetStore[asset.AssetID] = asset;
                    if (!asset.Temporary)
                        SaveAsset(asset);
                }
                else
                {
                    Logger.Log(String.Format("Failed to decode {0} asset {1}", asset.AssetType, asset.AssetID),
                        Helpers.LogLevel.Warning);
                }
            }
        }

        public bool TryGetAsset(UUID id, out Asset asset)
        {
            return AssetStore.TryGetValue(id, out asset);
        }

        public byte[] EncodePrimAsset(List<SimulationObject> linkset)
        {
            // FIXME:
            return new byte[0];
        }

        public bool TryDecodePrimAsset(byte[] primAssetData, out List<SimulationObject> linkset)
        {
            // FIXME:
            linkset = null;
            return false;
        }

        void SaveAsset(Asset asset)
        {
            try
            {
                File.WriteAllBytes(Path.Combine(UploadDir, String.Format("{0}.{1}", asset.AssetID,
                    asset.AssetType.ToString().ToLower())), asset.AssetData);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, Helpers.LogLevel.Warning, ex);
            }
        }

        Asset CreateAsset(AssetType type, UUID assetID, byte[] data)
        {
            switch (type)
            {
                case AssetType.Bodypart:
                    return new AssetBodypart(assetID, data);
                case AssetType.Clothing:
                    return new AssetClothing(assetID, data);
                case AssetType.LSLBytecode:
                    return new AssetScriptBinary(assetID, data);
                case AssetType.LSLText:
                    return new AssetScriptText(assetID, data);
                case AssetType.Notecard:
                    return new AssetNotecard(assetID, data);
                case AssetType.Texture:
                    return new AssetTexture(assetID, data);
                case AssetType.Animation:
                    return new AssetAnimation(assetID, data);
                case AssetType.CallingCard:
                case AssetType.Folder:
                case AssetType.Gesture:
                case AssetType.ImageJPEG:
                case AssetType.ImageTGA:
                case AssetType.Landmark:
                case AssetType.LostAndFoundFolder:
                case AssetType.Object:
                case AssetType.RootFolder:
                case AssetType.Simstate:
                case AssetType.SnapshotFolder:
                case AssetType.Sound:
                    return new AssetSound(assetID, data);
                case AssetType.SoundWAV:
                case AssetType.TextureTGA:
                case AssetType.TrashFolder:
                case AssetType.Unknown:
                default:
                    Logger.Log("Asset type " + type.ToString() + " not implemented!", Helpers.LogLevel.Warning);
                    return null;
            }
        }

        void LoadAssets(string path)
        {
            try
            {
                string[] textures = Directory.GetFiles(path, "*.jp2", SearchOption.TopDirectoryOnly);
                string[] clothing = Directory.GetFiles(path, "*.clothing", SearchOption.TopDirectoryOnly);
                string[] bodyparts = Directory.GetFiles(path, "*.bodypart", SearchOption.TopDirectoryOnly);
                string[] sounds = Directory.GetFiles(path, "*.ogg", SearchOption.TopDirectoryOnly);

                for (int i = 0; i < textures.Length; i++)
                {
                    UUID assetID = ParseUUIDFromFilename(textures[i]);
                    Asset asset = new AssetTexture(assetID, File.ReadAllBytes(textures[i]));
                    asset.Temporary = true;
                    StoreAsset(asset);
                }

                for (int i = 0; i < clothing.Length; i++)
                {
                    UUID assetID = ParseUUIDFromFilename(clothing[i]);
                    Asset asset = new AssetClothing(assetID, File.ReadAllBytes(clothing[i]));
                    asset.Temporary = true;
                    StoreAsset(asset);
                }

                for (int i = 0; i < bodyparts.Length; i++)
                {
                    UUID assetID = ParseUUIDFromFilename(bodyparts[i]);
                    Asset asset = new AssetBodypart(assetID, File.ReadAllBytes(bodyparts[i]));
                    asset.Temporary = true;
                    StoreAsset(asset);
                }

                for (int i = 0; i < sounds.Length; i++)
                {
                    UUID assetID = ParseUUIDFromFilename(sounds[i]);
                    Asset asset = new AssetSound(assetID, File.ReadAllBytes(sounds[i]));
                    asset.Temporary = true;
                    StoreAsset(asset);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, Helpers.LogLevel.Warning, ex);
            }
        }

        static UUID ParseUUIDFromFilename(string filename)
        {
            int dot = filename.LastIndexOf('.');

            if (dot > 35)
            {
                // Grab the last 36 characters of the filename
                string uuidString = filename.Substring(dot - 36, 36);
                UUID uuid;
                UUID.TryParse(uuidString, out uuid);
                return uuid;
            }
            else
            {
                return UUID.Zero;
            }
        }
    }
}
