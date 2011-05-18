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

using System.Collections.Generic;
using OpenMetaverse;

namespace OpenMetaverse.Assets
{
    /// <summary>
    /// Constants for the archiving module
    /// </summary>
    public class ArchiveConstants
    {
        /// <summary>
        /// The location of the archive control file
        /// </summary>
        public static readonly string CONTROL_FILE_PATH = "archive.xml";

        /// <summary>
        /// Path for the assets held in an archive
        /// </summary>
        public static readonly string ASSETS_PATH = "assets/";

        /// <summary>
        /// Path for the prims file
        /// </summary>
        public static readonly string OBJECTS_PATH = "objects/";

        /// <summary>
        /// Path for terrains.  Technically these may be assets, but I think it's quite nice to split them out.
        /// </summary>
        public static readonly string TERRAINS_PATH = "terrains/";

        /// <summary>
        /// Path for region settings.
        /// </summary>
        public static readonly string SETTINGS_PATH = "settings/";

        /// <summary>
        /// The character the separates the uuid from extension information in an archived asset filename
        /// </summary>
        public static readonly string ASSET_EXTENSION_SEPARATOR = "_";

        /// <summary>
        /// Extensions used for asset types in the archive
        /// </summary>
        public static readonly IDictionary<AssetType, string> ASSET_TYPE_TO_EXTENSION = new Dictionary<AssetType, string>();
        public static readonly IDictionary<string, AssetType> EXTENSION_TO_ASSET_TYPE = new Dictionary<string, AssetType>();

        static ArchiveConstants()
        {
            ASSET_TYPE_TO_EXTENSION[AssetType.Animation] = ASSET_EXTENSION_SEPARATOR + "animation.bvh";
            ASSET_TYPE_TO_EXTENSION[AssetType.Bodypart] = ASSET_EXTENSION_SEPARATOR + "bodypart.txt";
            ASSET_TYPE_TO_EXTENSION[AssetType.CallingCard] = ASSET_EXTENSION_SEPARATOR + "callingcard.txt";
            ASSET_TYPE_TO_EXTENSION[AssetType.Clothing] = ASSET_EXTENSION_SEPARATOR + "clothing.txt";
            ASSET_TYPE_TO_EXTENSION[AssetType.Folder] = ASSET_EXTENSION_SEPARATOR + "folder.txt";   // Not sure if we'll ever see this
            ASSET_TYPE_TO_EXTENSION[AssetType.Gesture] = ASSET_EXTENSION_SEPARATOR + "gesture.txt";
            ASSET_TYPE_TO_EXTENSION[AssetType.ImageJPEG] = ASSET_EXTENSION_SEPARATOR + "image.jpg";
            ASSET_TYPE_TO_EXTENSION[AssetType.ImageTGA] = ASSET_EXTENSION_SEPARATOR + "image.tga";
            ASSET_TYPE_TO_EXTENSION[AssetType.Landmark] = ASSET_EXTENSION_SEPARATOR + "landmark.txt";
            ASSET_TYPE_TO_EXTENSION[AssetType.LostAndFoundFolder] = ASSET_EXTENSION_SEPARATOR + "lostandfoundfolder.txt";   // Not sure if we'll ever see this
            ASSET_TYPE_TO_EXTENSION[AssetType.LSLBytecode] = ASSET_EXTENSION_SEPARATOR + "bytecode.lso";
            ASSET_TYPE_TO_EXTENSION[AssetType.LSLText] = ASSET_EXTENSION_SEPARATOR + "script.lsl";
            ASSET_TYPE_TO_EXTENSION[AssetType.Notecard] = ASSET_EXTENSION_SEPARATOR + "notecard.txt";
            ASSET_TYPE_TO_EXTENSION[AssetType.Object] = ASSET_EXTENSION_SEPARATOR + "object.xml";
            ASSET_TYPE_TO_EXTENSION[AssetType.RootFolder] = ASSET_EXTENSION_SEPARATOR + "rootfolder.txt";   // Not sure if we'll ever see this
            ASSET_TYPE_TO_EXTENSION[AssetType.Simstate] = ASSET_EXTENSION_SEPARATOR + "simstate.bin";   // Not sure if we'll ever see this
            ASSET_TYPE_TO_EXTENSION[AssetType.SnapshotFolder] = ASSET_EXTENSION_SEPARATOR + "snapshotfolder.txt";   // Not sure if we'll ever see this
            ASSET_TYPE_TO_EXTENSION[AssetType.Sound] = ASSET_EXTENSION_SEPARATOR + "sound.ogg";
            ASSET_TYPE_TO_EXTENSION[AssetType.SoundWAV] = ASSET_EXTENSION_SEPARATOR + "sound.wav";
            ASSET_TYPE_TO_EXTENSION[AssetType.Texture] = ASSET_EXTENSION_SEPARATOR + "texture.jp2";
            ASSET_TYPE_TO_EXTENSION[AssetType.TextureTGA] = ASSET_EXTENSION_SEPARATOR + "texture.tga";
            ASSET_TYPE_TO_EXTENSION[AssetType.TrashFolder] = ASSET_EXTENSION_SEPARATOR + "trashfolder.txt";   // Not sure if we'll ever see this

            EXTENSION_TO_ASSET_TYPE[ASSET_EXTENSION_SEPARATOR + "animation.bvh"] = AssetType.Animation;
            EXTENSION_TO_ASSET_TYPE[ASSET_EXTENSION_SEPARATOR + "bodypart.txt"] = AssetType.Bodypart;
            EXTENSION_TO_ASSET_TYPE[ASSET_EXTENSION_SEPARATOR + "callingcard.txt"] = AssetType.CallingCard;
            EXTENSION_TO_ASSET_TYPE[ASSET_EXTENSION_SEPARATOR + "clothing.txt"] = AssetType.Clothing;
            EXTENSION_TO_ASSET_TYPE[ASSET_EXTENSION_SEPARATOR + "folder.txt"] = AssetType.Folder;
            EXTENSION_TO_ASSET_TYPE[ASSET_EXTENSION_SEPARATOR + "gesture.txt"] = AssetType.Gesture;
            EXTENSION_TO_ASSET_TYPE[ASSET_EXTENSION_SEPARATOR + "image.jpg"] = AssetType.ImageJPEG;
            EXTENSION_TO_ASSET_TYPE[ASSET_EXTENSION_SEPARATOR + "image.tga"] = AssetType.ImageTGA;
            EXTENSION_TO_ASSET_TYPE[ASSET_EXTENSION_SEPARATOR + "landmark.txt"] = AssetType.Landmark;
            EXTENSION_TO_ASSET_TYPE[ASSET_EXTENSION_SEPARATOR + "lostandfoundfolder.txt"] = AssetType.LostAndFoundFolder;
            EXTENSION_TO_ASSET_TYPE[ASSET_EXTENSION_SEPARATOR + "bytecode.lso"] = AssetType.LSLBytecode;
            EXTENSION_TO_ASSET_TYPE[ASSET_EXTENSION_SEPARATOR + "script.lsl"] = AssetType.LSLText;
            EXTENSION_TO_ASSET_TYPE[ASSET_EXTENSION_SEPARATOR + "notecard.txt"] = AssetType.Notecard;
            EXTENSION_TO_ASSET_TYPE[ASSET_EXTENSION_SEPARATOR + "object.xml"] = AssetType.Object;
            EXTENSION_TO_ASSET_TYPE[ASSET_EXTENSION_SEPARATOR + "rootfolder.txt"] = AssetType.RootFolder;
            EXTENSION_TO_ASSET_TYPE[ASSET_EXTENSION_SEPARATOR + "simstate.bin"] = AssetType.Simstate;
            EXTENSION_TO_ASSET_TYPE[ASSET_EXTENSION_SEPARATOR + "snapshotfolder.txt"] = AssetType.SnapshotFolder;
            EXTENSION_TO_ASSET_TYPE[ASSET_EXTENSION_SEPARATOR + "sound.ogg"] = AssetType.Sound;
            EXTENSION_TO_ASSET_TYPE[ASSET_EXTENSION_SEPARATOR + "sound.wav"] = AssetType.SoundWAV;
            EXTENSION_TO_ASSET_TYPE[ASSET_EXTENSION_SEPARATOR + "texture.jp2"] = AssetType.Texture;
            EXTENSION_TO_ASSET_TYPE[ASSET_EXTENSION_SEPARATOR + "texture.tga"] = AssetType.TextureTGA;
            EXTENSION_TO_ASSET_TYPE[ASSET_EXTENSION_SEPARATOR + "trashfolder.txt"] = AssetType.TrashFolder;
        }
    }
}
