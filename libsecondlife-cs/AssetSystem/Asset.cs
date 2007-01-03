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
		public const sbyte ASSET_TYPE_NOTECARD = 7;
        public const sbyte ASSET_TYPE_IMAGE    = 0;
        public const sbyte ASSET_TYPE_WEARABLE_BODY = 13;
        public const sbyte ASSET_TYPE_WEARABLE_CLOTHING = 5;
        public const sbyte ASSET_TYPE_SCRIPT = 10;

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
            return Helpers.FieldToString((byte[])_AssetData);
		}

        public virtual void SetAssetData(byte[] data)
        {
            _AssetData = data;
        }
	}
}
