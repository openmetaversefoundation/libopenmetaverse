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

using System;
using OpenMetaverse;

namespace OpenMetaverse.Assets
{
    /// <summary>
    /// Represents an LSL Text object containing a string of UTF encoded characters
    /// </summary>
    public class AssetScriptText : Asset
    {
        /// <summary>Override the base classes AssetType</summary>
        public override AssetType AssetType { get { return AssetType.LSLText; } }

        /// <summary>A string of characters represting the script contents</summary>
        public string Source;

        /// <summary>Initializes a new AssetScriptText object</summary>
        public AssetScriptText() { }

        /// <summary>
        /// Initializes a new AssetScriptText object with parameters
        /// </summary>
        /// <param name="assetID">A unique <see cref="UUID"/> specific to this asset</param>
        /// <param name="assetData">A byte array containing the raw asset data</param>
        public AssetScriptText(UUID assetID, byte[] assetData) : base(assetID, assetData) { }

        /// <summary>
        /// Initializes a new AssetScriptText object with parameters
        /// </summary>
        /// <param name="source">A string containing the scripts contents</param>
        public AssetScriptText(string source)
        {
            Source = source;
        }

        /// <summary>
        /// Encode a string containing the scripts contents into byte encoded AssetData
        /// </summary>
        public override void Encode()
        {
            AssetData = Utils.StringToBytes(Source);
        }

        /// <summary>
        /// Decode a byte array containing the scripts contents into a string
        /// </summary>
        /// <returns>true if decoding is successful</returns>
        public override bool Decode()
        {
            Source = Utils.BytesToString(AssetData);
            return true;
        }
    }
}
