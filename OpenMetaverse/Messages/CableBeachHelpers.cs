using System;
using OpenMetaverse;

namespace OpenMetaverse.Messages.CableBeach
{
    public static class CableBeachUtils
    {
        // The following section is based on the table at https://wiki.secondlife.com/wiki/Asset_System

        #region SL / file extension / content-type conversions

        public static string SLAssetTypeToContentType(int assetType)
        {
            switch (assetType)
            {
                case 0:
                    return "image/x-j2c";
                case 1:
                    return "application/ogg";
                case 2:
                    return "application/vnd.ll.callingcard";
                case 3:
                    return "application/vnd.ll.landmark";
                case 5:
                    return "application/vnd.ll.clothing";
                case 6:
                    return "application/vnd.ll.primitive";
                case 7:
                    return "application/vnd.ll.notecard";
                case 10:
                    return "application/vnd.ll.lsltext";
                case 11:
                    return "application/vnd.ll.lslbyte";
                case 13:
                    return "application/vnd.ll.bodypart";
                case 20:
                    return "application/vnd.ll.animation";
                case 21:
                    return "application/vnd.ll.gesture";
                default:
                    return "application/octet-stream";
            }
        }

        public static int ContentTypeToSLAssetType(string contentType)
        {
            switch (contentType)
            {
                case "image/x-j2c":
                    return 0;
                case "application/ogg":
                    return 1;
                case "application/vnd.ll.callingcard":
                    return 2;
                case "application/vnd.ll.landmark":
                    return 3;
                case "application/vnd.ll.clothing":
                    return 5;
                case "application/vnd.ll.primitive":
                    return 6;
                case "application/vnd.ll.notecard":
                    return 7;
                case "application/vnd.ll.lsltext":
                    return 10;
                case "application/vnd.ll.lslbyte":
                    return 11;
                case "application/vnd.ll.bodypart":
                    return 13;
                case "application/vnd.ll.animation":
                    return 20;
                case "application/vnd.ll.gesture":
                    return 21;
                default:
                    return -1;
            }
        }

        public static int ContentTypeToSLInvType(string contentType)
        {
            switch (contentType)
            {
                case "image/x-j2c":
                    return (int)InventoryType.Texture;
                case "application/ogg":
                    return (int)InventoryType.Sound;
                case "application/vnd.ll.callingcard":
                    return (int)InventoryType.CallingCard;
                case "application/vnd.ll.landmark":
                    return (int)InventoryType.Landmark;
                case "application/vnd.ll.clothing":
                case "application/vnd.ll.bodypart":
                    return (int)InventoryType.Wearable;
                case "application/vnd.ll.primitive":
                    return (int)InventoryType.Object;
                case "application/vnd.ll.notecard":
                    return (int)InventoryType.Notecard;
                case "application/vnd.ll.lsltext":
                case "application/vnd.ll.lslbyte":
                    return (int)InventoryType.LSL;
                case "application/vnd.ll.animation":
                    return (int)InventoryType.Animation;
                case "application/vnd.ll.gesture":
                    return (int)InventoryType.Gesture;
                default:
                    return (int)InventoryType.Unknown;
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
                case "application/vnd.ll.lsltext":
                    return "lsltext";
                case "application/vnd.ll.lslbyte":
                    return "lslbyte";
                case "application/vnd.ll.bodypart":
                    return "bodypart";
                case "application/vnd.ll.animation":
                    return "animatn";
                case "application/vnd.ll.gesture":
                    return "gesture";
                default:
                    return "binary";
            }
        }

        public static string ExtensionToContentType(string extension)
        {
            switch (extension)
            {
                case "texture":
                case "jp2":
                case "j2c":
                    return "image/x-j2c";
                case "sound":
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
                case "lsl":
                    return "application/vnd.ll.lsltext";
                case "lso":
                    return "application/vnd.ll.lslbyte";
                case "bodypart":
                    return "application/vnd.ll.bodypart";
                case "animatn":
                    return "application/vnd.ll.animation";
                case "gesture":
                    return "application/vnd.ll.gesture";
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
