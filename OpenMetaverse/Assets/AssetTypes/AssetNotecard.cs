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
    /// Represents a string of characters encoded with specific formatting properties
    /// </summary>
    public class AssetNotecard : Asset
    {
        /// <summary>Override the base classes AssetType</summary>
        public override AssetType AssetType { get { return AssetType.Notecard; } }

        /// <summary>A text string containing the raw contents of the notecard</summary>
        public string Text = null;

        /// <summary>Construct an Asset of type Notecard</summary>
        public AssetNotecard() { }

        /// <summary>
        /// Construct an Asset object of type Notecard
        /// </summary>
        /// <param name="assetID">A unique <see cref="UUID"/> specific to this asset</param>
        /// <param name="assetData">A byte array containing the raw asset data</param>
        public AssetNotecard(UUID assetID, byte[] assetData)
            : base(assetID, assetData)
        {
            Decode();
        }

        /// <summary>
        /// Construct an Asset object of type Notecard
        /// </summary>
        /// <param name="text">A text string containing the raw contents of the notecard</param>
        public AssetNotecard(string text)
        {
            Text = text;
            Encode();
        }

        /// <summary>
        /// Encode the raw contents of a string with the specific Linden Text properties
        /// </summary>
        public override void Encode()
        {
            string temp = "Linden text version 2\n{\nLLEmbeddedItems version 1\n{\ncount 0\n}\nText length ";
            temp += Text.Length + "\n";
            temp += Text;
            temp += "}";
            AssetData = Utils.StringToBytes(temp);
        }

        /// <summary>
        /// Decode the raw asset data including the Linden Text properties
        /// </summary>
        /// <returns>true if the AssetData was successfully decoded to a string</returns>
        public override bool Decode()
        {
            Text = Utils.BytesToString(AssetData);
            return true;
        }
    }
}
