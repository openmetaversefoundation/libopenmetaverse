/*
 * Copyright (c) 2006, Second Life Reverse Engineering Team
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without 
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the Second Life Reverse Engineering Team nor the names 
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

using libsecondlife;

namespace libsecondlife.AssetSystem
{
	/// <summary>
	/// Summary description for Asset.
	/// </summary>
	public class Asset
	{
/*
		public const sbyte ASSET_TYPE_NOTECARD = 7;
        public const sbyte ASSET_TYPE_IMAGE    = 0;
        public const sbyte ASSET_TYPE_WEARABLE_BODY = 13;
        public const sbyte ASSET_TYPE_WEARABLE_CLOTHING = 5;
        public const sbyte ASSET_TYPE_SCRIPT = 10;
*/
        public enum AssetType : sbyte
        {
            /// <summary>Unknown asset type</summary>
            Unknown = -1,

            /// <summary>Texture asset, stores in JPEG2000 J2C stream format</summary>
            Texture = 0,

            /// <summary>Sound asset</summary>
            Sound = 1,

            /// <summary>Calling card for another avatar</summary>
            CallingCard = 2,

            /// <summary>Link to a location in world</summary>
            Landmark = 3,

            /// <summary>Legacy script asset, you should never see one of these</summary>
            [Obsolete]
            Script = 4,

            /// <summary>Collection of textures and parameters that can be 
            /// worn by an avatar</summary>
            Clothing = 5,

            /// <summary>Primitive that can contain textures, sounds, 
            /// scripts and more</summary>
            Object = 6,

            /// <summary>Notecard asset</summary>
            Notecard = 7,

            /// <summary>Holds a collection of inventory items</summary>
            Folder = 8,

            /// <summary>Root inventory folder</summary>
            RootFolder = 9,

            /// <summary>Linden scripting language script</summary>
            LSLText = 10,

            /// <summary>LSO bytecode for a script</summary>
            LSLBytecode = 11,

            /// <summary>Uncompressed TGA texture</summary>
            TextureTGA = 12,

            /// <summary>Collection of textures and shape parameters that can
            /// be worn</summary>
            Bodypart = 13,

            /// <summary>Trash folder</summary>
            TrashFolder = 14,

            /// <summary>Snapshot folder</summary>
            SnapshotFolder = 15,

            /// <summary>Lost and found folder</summary>
            LostAndFoundFolder = 16,

            /// <summary>Uncompressed sound</summary>
            SoundWAV = 17,

            /// <summary>Uncompressed TGA non-square image, not to be used as a
            /// texture</summary>
            ImageTGA = 18,

            /// <summary>Compressed JPEG non-square image, not to be used as a
            /// texture</summary>
            ImageJPEG = 19,

            /// <summary>Animation</summary>
            Animation = 20,

            /// <summary>Sequence of animations, sounds, chat, and pauses</summary>
            Gesture = 21,

            /// <summary>Simstate file</summary>
            Simstate = 22
        }

		public LLUUID AssetID;

		public sbyte Type;
		public bool Tempfile;


		internal byte[] _AssetData;
        public byte[] AssetData
        {
            get
            {
                return _AssetData;
            }
        }


        /// <summary>
        /// </summary>
        /// <param name="assetID"></param>
        /// <param name="type"></param>
        /// <param name="tempfile"></param>
        /// <param name="assetData"></param>
        public Asset(LLUUID assetID, sbyte type, bool tempfile, byte[] assetData)
		{
			AssetID		= assetID;
			Type		= (sbyte)type;
			Tempfile	= tempfile;
			_AssetData	= assetData;
		}

        /// <summary>
        /// </summary>
        /// <param name="assetID"></param>
        /// <param name="type"></param>
        /// <param name="assetData"></param>
        public Asset(LLUUID assetID, sbyte type, byte[] assetData)
		{
			AssetID		= assetID;
			Type		= (sbyte)type;
			Tempfile	= false;
			_AssetData	= assetData;
		}

        /// <summary>
        /// Return this asset's data as a pretty printable string.
        /// </summary>
		public string AssetDataToString()
		{
            return Helpers.FieldToUTF8String((byte[])_AssetData);
		}

        public virtual void SetAssetData(byte[] data)
        {
            _AssetData = data;
        }
	}
}
