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

namespace OpenMetaverse
{
    /// <summary>
    /// Attribute class that allows extra attributes to be attached to ENUMs
    /// </summary>
    public class EnumInfoAttribute : Attribute
    {
        /// <summary>Text used when presenting ENUM to user</summary>
        public string Text = string.Empty;

        /// <summary>Default initializer</summary>
        public EnumInfoAttribute() { }

        /// <summary>Text used when presenting ENUM to user</summary>
        public EnumInfoAttribute(string text)
        {
            this.Text = text;
        }
    }

    /// <summary>
    /// The different types of grid assets
    /// </summary>
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
        // <summary>Legacy script asset, you should never see one of these</summary>
        //[Obsolete]
        //Script = 4,
        /// <summary>Collection of textures and parameters that can be worn by an avatar</summary>
        Clothing = 5,
        /// <summary>Primitive that can contain textures, sounds, 
        /// scripts and more</summary>
        Object = 6,
        /// <summary>Notecard asset</summary>
        Notecard = 7,
        /// <summary>Holds a collection of inventory items. "Category" in the Linden viewer</summary>
        Folder = 8,
        /// <summary>Linden scripting language script</summary>
        LSLText = 10,
        /// <summary>LSO bytecode for a script</summary>
        LSLBytecode = 11,
        /// <summary>Uncompressed TGA texture</summary>
        TextureTGA = 12,
        /// <summary>Collection of textures and shape parameters that can be worn</summary>
        Bodypart = 13,
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
        Simstate = 22,
        /// <summary>Asset is a link to another inventory item</summary>
        Link = 24,
        /// <summary>Asset is a link to another inventory folder</summary>
        LinkFolder = 25,
        /// <summary>Marketplace Folder. Same as an Category but different display methods.</summary>
        MarketplaceFolder = 26,
        /// <summary>Linden mesh format</summary>
        Mesh = 49,
    }

    /// <summary>
    /// The different types of folder.
    /// </summary>
    public enum FolderType : sbyte
    {
        /// <summary>None folder type</summary>
        None = -1,
        /// <summary>Texture folder type</summary>
        Texture = 0,
        /// <summary>Sound folder type</summary>
        Sound = 1,
        /// <summary>Calling card folder type</summary>
        CallingCard = 2,
        /// <summary>Landmark folder type</summary>
        Landmark = 3,
        /// <summary>Clothing folder type</summary>
        Clothing = 5,
        /// <summary>Object folder type</summary>
        Object = 6,
        /// <summary>Notecard folder type</summary>
        Notecard = 7,
        /// <summary>The root folder type</summary>
        Root = 8,
        /// <summary>LSLText folder</summary>
        LSLText = 10,
        /// <summary>Bodyparts folder</summary>
        BodyPart = 13,
        /// <summary>Trash folder</summary>
        Trash = 14,
        /// <summary>Snapshot folder</summary>
        Snapshot = 15,
        /// <summary>Lost And Found folder</summary>
        LostAndFound = 16,
        /// <summary>Animation folder</summary>
        Animation = 20,
        /// <summary>Gesture folder</summary>
        Gesture = 21,
        /// <summary>Favorites folder</summary>
        Favorites = 23,
        /// <summary>Ensemble beginning range</summary>
        EnsembleStart = 26,
        /// <summary>Ensemble ending range</summary>
        EnsembleEnd= 45,
        /// <summary>Current outfit folder</summary>
        CurrentOutfit = 46,
        /// <summary>Outfit folder</summary>
        Outfit = 47,
        /// <summary>My outfits folder</summary>
        MyOutfits = 48,
        /// <summary>Mesh folder</summary>
        Mesh = 49,
        /// <summary>Marketplace direct delivery inbox ("Received Items")</summary>
        Inbox = 50,
        /// <summary>Marketplace direct delivery outbox</summary>
        Outbox = 51,
        /// <summary>Basic root folder</summary>
        BasicRoot = 52,
        /// <summary>Marketplace listings folder</summary>
        MarketplaceListings = 53,
        /// <summary>Marketplace stock folder</summary>
        MarkplaceStock = 54,
        /// <summary>Hypergrid Suitcase folder</summary>
        Suitcase = 100
    }

    /// <summary>
    /// Inventory Item Types, eg Script, Notecard, Folder, etc
    /// </summary>
    public enum InventoryType : sbyte
    {
        /// <summary>Unknown</summary>
        Unknown = -1,
        /// <summary>Texture</summary>
        Texture = 0,
        /// <summary>Sound</summary>
        Sound = 1,
        /// <summary>Calling Card</summary>
        CallingCard = 2,
        /// <summary>Landmark</summary>
        Landmark = 3,
        /*
        /// <summary>Script</summary>
        //[Obsolete("See LSL")] Script = 4,
        /// <summary>Clothing</summary>
        //[Obsolete("See Wearable")] Clothing = 5,
        /// <summary>Object, both single and coalesced</summary>
         */
        Object = 6,
        /// <summary>Notecard</summary>
        Notecard = 7,
        /// <summary></summary>
        Category = 8,
        /// <summary>Folder</summary>
        Folder = 8,
        /// <summary></summary>
        RootCategory = 9,
        /// <summary>an LSL Script</summary>
        LSL = 10,
        /*
        /// <summary></summary>
        //[Obsolete("See LSL")] LSLBytecode = 11,
        /// <summary></summary>
        //[Obsolete("See Texture")] TextureTGA = 12,
        /// <summary></summary>
        //[Obsolete] Bodypart = 13,
        /// <summary></summary>
        //[Obsolete] Trash = 14,
         */
        /// <summary></summary>
        Snapshot = 15,
        /*
        /// <summary></summary>
        //[Obsolete] LostAndFound = 16,
         */
        /// <summary></summary>
        Attachment = 17,
        /// <summary></summary>
        Wearable = 18,
        /// <summary></summary>
        Animation = 19,
        /// <summary></summary>
        Gesture = 20,

        /// <summary></summary>
        Mesh = 22,
    }

    /// <summary>
    /// Item Sale Status
    /// </summary>
    public enum SaleType : byte
    {
        /// <summary>Not for sale</summary>
        Not = 0,
        /// <summary>The original is for sale</summary>
        Original = 1,
        /// <summary>Copies are for sale</summary>
        Copy = 2,
        /// <summary>The contents of the object are for sale</summary>
        Contents = 3
    }

    /// <summary>
    /// Types of wearable assets
    /// </summary>
    public enum WearableType : byte
    {
        /// <summary>Body shape</summary>
        Shape = 0,
        /// <summary>Skin textures and attributes</summary>
        Skin,
        /// <summary>Hair</summary>
        Hair,
        /// <summary>Eyes</summary>
        Eyes,
        /// <summary>Shirt</summary>
        Shirt,
        /// <summary>Pants</summary>
        Pants,
        /// <summary>Shoes</summary>
        Shoes,
        /// <summary>Socks</summary>
        Socks,
        /// <summary>Jacket</summary>
        Jacket,
        /// <summary>Gloves</summary>
        Gloves,
        /// <summary>Undershirt</summary>
        Undershirt,
        /// <summary>Underpants</summary>
        Underpants,
        /// <summary>Skirt</summary>
        Skirt,
        /// <summary>Alpha mask to hide parts of the avatar</summary>
        Alpha,
        /// <summary>Tattoo</summary>
        Tattoo,
        /// <summary>Physics</summary>
        Physics,
        /// <summary>Invalid wearable asset</summary>
        Invalid = 255
    };
}
