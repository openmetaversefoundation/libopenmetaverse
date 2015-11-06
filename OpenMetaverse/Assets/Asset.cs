/*
 * Copyright (c) 2006-2016, openmetaverse.co
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.co nor the names
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
using OpenMetaverse;

namespace OpenMetaverse.Assets
{
    /// <summary>
    /// Base class for all Asset types
    /// </summary>
    public abstract class Asset
    {
        /// <summary>A byte array containing the raw asset data</summary>
        public byte[] AssetData;
        /// <summary>True if the asset it only stored on the server temporarily</summary>
        public bool Temporary;
        /// <summary>A unique ID</summary>
        private UUID _AssetID;
        /// <summary>The assets unique ID</summary>
        public UUID AssetID
        {
            get { return _AssetID; }
            internal set { _AssetID = value; }
        }

        /// <summary>
        /// The "type" of asset, Notecard, Animation, etc
        /// </summary>
        public abstract AssetType AssetType
        {
            get;
        }

        /// <summary>
        /// Construct a new Asset object
        /// </summary>
        public Asset() { }

        /// <summary>
        /// Construct a new Asset object
        /// </summary>
        /// <param name="assetID">A unique <see cref="UUID"/> specific to this asset</param>
        /// <param name="assetData">A byte array containing the raw asset data</param>
        public Asset(UUID assetID, byte[] assetData)
        {
            _AssetID = assetID;
            AssetData = assetData;
        }

        /// <summary>
        /// Regenerates the <code>AssetData</code> byte array from the properties 
        /// of the derived class.
        /// </summary>
        public abstract void Encode();

        /// <summary>
        /// Decodes the AssetData, placing it in appropriate properties of the derived
        /// class.
        /// </summary>
        /// <returns>True if the asset decoding succeeded, otherwise false</returns>
        public abstract bool Decode();
    }
}
