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
    /// Represents a Callingcard with AvatarID and Position vector
    /// </summary>
    public class AssetCallingCard : Asset
    {
        /// <summary>Override the base classes AssetType</summary>
        public override AssetType AssetType { get { return AssetType.CallingCard; } }

        /// <summary>UUID of the Callingcard target avatar</summary>
        public UUID AvatarID = UUID.Zero;

        /// <summary>Construct an Asset of type Callingcard</summary>
        public AssetCallingCard() { }

        /// <summary>
        /// Construct an Asset object of type Callingcard
        /// </summary>
        /// <param name="assetID">A unique <see cref="UUID"/> specific to this asset</param>
        /// <param name="assetData">A byte array containing the raw asset data</param>
        public AssetCallingCard(UUID assetID, byte[] assetData)
            : base(assetID, assetData)
        {
            Decode();
        }

        /// <summary>
        /// Constuct an asset of type Callingcard
        /// </summary>
        /// <param name="avatarID">UUID of the target avatar</param>
        public AssetCallingCard(UUID avatarID)
        {
            AvatarID = avatarID;
            Encode();
        }

        /// <summary>
        /// Encode the raw contents of a string with the specific Callingcard format
        /// </summary>
        public override void Encode()
        {
            string temp = "Callingcard version 2\n";
            temp += "avatar_id " + AvatarID + "\n";
            AssetData = Utils.StringToBytes(temp);
        }

        /// <summary>
        /// Decode the raw asset data, populating the AvatarID and Position
        /// </summary>
        /// <returns>true if the AssetData was successfully decoded to a UUID and Vector</returns>
        public override bool Decode()
        {
            String text = Utils.BytesToString(AssetData);
            if (text.ToLower().Contains("callingcard version 2"))
            {
                AvatarID = new UUID(text.Substring(text.IndexOf("avatar_id") + 10, 36));
                return true;
            }
            return false;
        }
    }
}
