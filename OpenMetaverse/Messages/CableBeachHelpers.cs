using System;

namespace OpenMetaverse.Messages.CableBeach
{
    public static class CableBeachUtils
    {
        #region SL / file extension / content-type conversions

        public static string SLAssetTypeToContentType(int assetType)
        {
            switch (assetType)
            {
                case 0:
                    return "image/jp2";
                case 1:
                    return "application/ogg";
                case 2:
                    return "application/x-metaverse-callingcard";
                case 3:
                    return "application/x-metaverse-landmark";
                case 5:
                    return "application/x-metaverse-clothing";
                case 6:
                    return "application/x-metaverse-primitive";
                case 7:
                    return "application/x-metaverse-notecard";
                case 8:
                    return "application/x-metaverse-folder";
                case 10:
                    return "application/x-metaverse-lsl";
                case 11:
                    return "application/x-metaverse-lso";
                case 12:
                    return "image/tga";
                case 13:
                    return "application/x-metaverse-bodypart";
                case 17:
                    return "audio/x-wav";
                case 19:
                    return "image/jpeg";
                case 20:
                    return "application/x-metaverse-animation";
                case 21:
                    return "application/x-metaverse-gesture";
                case 22:
                    return "application/x-metaverse-simstate";
                default:
                    return "application/octet-stream";
            }
        }

        public static int ContentTypeToSLAssetType(string contentType)
        {
            switch (contentType)
            {
                case "image/jp2":
                    return 0;
                case "application/ogg":
                    return 1;
                case "application/x-metaverse-callingcard":
                    return 2;
                case "application/x-metaverse-landmark":
                    return 3;
                case "application/x-metaverse-clothing":
                    return 5;
                case "application/x-metaverse-primitive":
                    return 6;
                case "application/x-metaverse-notecard":
                    return 7;
                case "application/x-metaverse-lsl":
                    return 10;
                case "application/x-metaverse-lso":
                    return 11;
                case "image/tga":
                    return 12;
                case "application/x-metaverse-bodypart":
                    return 13;
                case "audio/x-wav":
                    return 17;
                case "image/jpeg":
                    return 19;
                case "application/x-metaverse-animation":
                    return 20;
                case "application/x-metaverse-gesture":
                    return 21;
                case "application/x-metaverse-simstate":
                    return 22;
                default:
                    return -1;
            }
        }

        public static string ContentTypeToExtension(string contentType)
        {
            switch (contentType)
            {
                case "image/jp2":
                    return "texture";
                case "application/ogg":
                    return "ogg";
                case "application/x-metaverse-callingcard":
                    return "callingcard";
                case "application/x-metaverse-landmark":
                    return "landmark";
                case "application/x-metaverse-clothing":
                    return "clothing";
                case "application/x-metaverse-primitive":
                    return "primitive";
                case "application/x-metaverse-notecard":
                    return "notecard";
                case "application/x-metaverse-lsl":
                    return "lsl";
                case "application/x-metaverse-lso":
                    return "lso";
                case "image/tga":
                    return "tga";
                case "application/x-metaverse-bodypart":
                    return "bodypart";
                case "audio/x-wav":
                    return "wav";
                case "image/jpeg":
                    return "jpg";
                case "application/x-metaverse-animation":
                    return "animation";
                case "application/x-metaverse-gesture":
                    return "gesture";
                case "application/x-metaverse-simstate":
                    return "simstate";
                default:
                    return "bin";
            }
        }

        public static string ExtensionToContentType(string extension)
        {
            switch (extension)
            {
                case "texture":
                case "jp2":
                case "j2c":
                    return "image/jp2";
                case "sound":
                case "ogg":
                    return "application/ogg";
                case "callingcard":
                    return "application/x-metaverse-callingcard";
                case "landmark":
                    return "application/x-metaverse-landmark";
                case "clothing":
                    return "application/x-metaverse-clothing";
                case "primitive":
                    return "application/x-metaverse-primitive";
                case "notecard":
                    return "application/x-metaverse-notecard";
                case "lsl":
                    return "application/x-metaverse-lsl";
                case "lso":
                    return "application/x-metaverse-lso";
                case "tga":
                    return "image/tga";
                case "bodypart":
                    return "application/x-metaverse-bodypart";
                case "wav":
                    return "audio/x-wav";
                case "jpg":
                case "jpeg":
                    return "image/jpeg";
                case "animation":
                    return "application/x-metaverse-animation";
                case "gesture":
                    return "application/x-metaverse-gesture";
                case "simstate":
                    return "application/x-metaverse-simstate";
                case "txt":
                    return "text/plain";
                case "xml":
                    return "application/xml";
                default:
                    return "application/octet-stream";
            }
        }

        #endregion SL / file extension / content-type conversions
    }
}
