using System;
using OpenMetaverse;

namespace OpenMetaverse.Messages.CableBeach
{
    public static class CableBeachUtils
    {
        // The following section is based on the table at https://wiki.secondlife.com/wiki/Asset_System

        #region SL / file extension / content-type conversions

        public static string SLAssetTypeToContentType(AssetType assetType)
        {
            switch (assetType)
            {
                case AssetType.Texture:
                    return "image/x-j2c";
                case AssetType.Sound:
                    return "application/ogg";
                case AssetType.CallingCard:
                    return "application/vnd.ll.callingcard";
                case AssetType.Landmark:
                    return "application/vnd.ll.landmark";
                case AssetType.Clothing:
                    return "application/vnd.ll.clothing";
                case AssetType.Object:
                    return "application/vnd.ll.primitive";
                case AssetType.Notecard:
                    return "application/vnd.ll.notecard";
                case AssetType.Folder:
                    return "application/vnd.ll.folder";
                case AssetType.RootFolder:
                    return "application/vnd.ll.rootfolder";
                case AssetType.LSLText:
                    return "application/vnd.ll.lsltext";
                case AssetType.LSLBytecode:
                    return "application/vnd.ll.lslbyte";
                case AssetType.TextureTGA:
                case AssetType.ImageTGA:
                    return "image/tga";
                case AssetType.Bodypart:
                    return "application/vnd.ll.bodypart";
                case AssetType.TrashFolder:
                    return "application/vnd.ll.trashfolder";
                case AssetType.SnapshotFolder:
                    return "application/vnd.ll.snapshotfolder";
                case AssetType.LostAndFoundFolder:
                    return "application/vnd.ll.lostandfoundfolder";
                case AssetType.SoundWAV:
                    return "audio/x-wav";
                case AssetType.ImageJPEG:
                    return "image/jpeg";
                case AssetType.Animation:
                    return "application/vnd.ll.animation";
                case AssetType.Gesture:
                    return "application/vnd.ll.gesture";
                case AssetType.Simstate:
                case AssetType.Unknown:
                default:
                    return "application/octet-stream";
            }
        }

        public static AssetType ContentTypeToSLAssetType(string contentType)
        {
            switch (contentType)
            {
                case "image/x-j2c":
                    return AssetType.Texture;
                case "application/ogg":
                    return AssetType.Sound;
                case "application/vnd.ll.callingcard":
                    return AssetType.CallingCard;
                case "application/vnd.ll.landmark":
                    return AssetType.Landmark;
                case "application/vnd.ll.clothing":
                    return AssetType.Clothing;
                case "application/vnd.ll.primitive":
                    return AssetType.Object;
                case "application/vnd.ll.notecard":
                    return AssetType.Notecard;
                case "application/vnd.ll.folder":
                    return AssetType.Folder;
                case "application/vnd.ll.rootfolder":
                    return AssetType.RootFolder;
                case "application/vnd.ll.lsltext":
                    return AssetType.LSLText;
                case "application/vnd.ll.lslbyte":
                    return AssetType.LSLBytecode;
                case "image/tga":
                    // Note that AssetType.TextureTGA will be converted to AssetType.ImageTGA
                    return AssetType.ImageTGA;
                case "application/vnd.ll.bodypart":
                    return AssetType.Bodypart;
                case "application/vnd.ll.trashfolder":
                    return AssetType.TrashFolder;
                case "application/vnd.ll.snapshotfolder":
                    return AssetType.SnapshotFolder;
                case "application/vnd.ll.lostandfoundfolder":
                    return AssetType.LostAndFoundFolder;
                case "audio/x-wav":
                    return AssetType.SoundWAV;
                case "image/jpeg":
                    return AssetType.ImageJPEG;
                case "application/vnd.ll.animation":
                    return AssetType.Animation;
                case "application/vnd.ll.gesture":
                    return AssetType.Gesture;
                case "application/octet-stream":
                default:
                    return AssetType.Unknown;
            }
        }

        public static InventoryType ContentTypeToSLInvType(string contentType)
        {
            switch (contentType)
            {
                case "image/x-j2c":
                case "image/tga":
                case "image/jpeg":
                    return InventoryType.Texture;
                case "application/ogg":
                case "audio/x-wav":
                    return InventoryType.Sound;
                case "application/vnd.ll.callingcard":
                    return InventoryType.CallingCard;
                case "application/vnd.ll.landmark":
                    return InventoryType.Landmark;
                case "application/vnd.ll.clothing":
                case "application/vnd.ll.bodypart":
                    return InventoryType.Wearable;
                case "application/vnd.ll.primitive":
                    return InventoryType.Object;
                case "application/vnd.ll.notecard":
                    return InventoryType.Notecard;
                case "application/vnd.ll.folder":
                    return InventoryType.Folder;
                case "application/vnd.ll.rootfolder":
                    return InventoryType.RootCategory;
                case "application/vnd.ll.lsltext":
                case "application/vnd.ll.lslbyte":
                    return InventoryType.LSL;
                case "application/vnd.ll.trashfolder":
                case "application/vnd.ll.snapshotfolder":
                case "application/vnd.ll.lostandfoundfolder":
                    return InventoryType.Folder;
                case "application/vnd.ll.animation":
                    return InventoryType.Animation;
                case "application/vnd.ll.gesture":
                    return InventoryType.Gesture;
                case "application/octet-stream":
                default:
                    return InventoryType.Unknown;
            }
        }

        public static string ContentTypeToExtension(string contentType)
        {
            switch (contentType)
            {
                case "image/x-j2c":
                    return "texture";
                case "application/ogg":
                    return "ogg";
                case "application/vnd.ll.callingcard":
                    return "callingcard";
                case "application/vnd.ll.landmark":
                    return "landmark";
                case "application/vnd.ll.clothing":
                    return "clothing";
                case "application/vnd.ll.primitive":
                    return "primitive";
                case "application/vnd.ll.notecard":
                    return "notecard";
                case "application/vnd.ll.folder":
                    return "folder";
                case "application/vnd.ll.rootfolder":
                    return "rootfolder";
                case "application/vnd.ll.lsltext":
                    return "lsltext";
                case "application/vnd.ll.lslbyte":
                    return "lslbyte";
                case "image/tga":
                    return "tga";
                case "application/vnd.ll.bodypart":
                    return "bodypart";
                case "application/vnd.ll.trashfolder":
                    return "trashfolder";
                case "application/vnd.ll.snapshotfolder":
                    return "snapshotfolder";
                case "application/vnd.ll.lostandfoundfolder":
                    return "lostandfoundfolder";
                case "audio/x-wav":
                    return "wav";
                case "image/jpeg":
                    return "jpg";
                case "application/vnd.ll.animation":
                    return "animatn";
                case "application/vnd.ll.gesture":
                    return "gesture";
                case "application/octet-stream":
                default:
                    return "binary";
            }
        }

        public static string ExtensionToContentType(string extension)
        {
            switch (extension)
            {
                case "texture":
                    return "image/x-j2c";
                case "ogg":
                    return "application/ogg";
                case "callingcard":
                    return "application/vnd.ll.callingcard";
                case "landmark":
                    return "application/vnd.ll.landmark";
                case "clothing":
                    return "application/vnd.ll.clothing";
                case "primitive":
                    return "application/vnd.ll.primitive";
                case "notecard":
                    return "application/vnd.ll.notecard";
                case "folder":
                    return "application/vnd.ll.folder";
                case "rootfolder":
                    return "application/vnd.ll.rootfolder";
                case "lsltext":
                    return "application/vnd.ll.lsltext";
                case "lslbyte":
                    return "application/vnd.ll.lslbyte";
                case "tga":
                    return "image/tga";
                case "bodypart":
                    return "application/vnd.ll.bodypart";
                case "trashfolder":
                    return "application/vnd.ll.trashfolder";
                case "snapshotfolder":
                    return "application/vnd.ll.snapshotfolder";
                case "lostandfoundfolder":
                    return "application/vnd.ll.lostandfoundfolder";
                case "wav":
                    return "audio/x-wav";
                case "jpg":
                    return "image/jpeg";
                case "animatn":
                    return "application/vnd.ll.animation";
                case "gesture":
                    return "application/vnd.ll.gesture";
                case "binary":
                default:
                    return "application/octet-stream";
            }
        }

        #endregion SL / file extension / content-type conversions

        public static UUID IdentityToUUID(Uri identity)
        {
            return new UUID((OpenMetaverse.Utils.MD5(System.Text.Encoding.UTF8.GetBytes(identity.ToString()))), 0);
        }
    }
}
