/*
 * Copyright (c) 2007-2008, openmetaverse.org
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
using System.Text;
using System.Collections.Generic;
using OpenMetaverse.Imaging;

namespace OpenMetaverse
{
    /// <summary>
    /// 
    /// </summary>
    [Flags]
    public enum WearableType : byte
    {
        /// <summary>A shape</summary>
        Shape = 0,
        /// <summary></summary>
        Skin,
        /// <summary></summary>
        Hair,
        /// <summary></summary>
        Eyes,
        /// <summary></summary>
        Shirt,
        /// <summary></summary>
        Pants,
        /// <summary></summary>
        Shoes,
        /// <summary></summary>
        Socks,
        /// <summary></summary>
        Jacket,
        /// <summary></summary>
        Gloves,
        /// <summary></summary>
        Undershirt,
        /// <summary></summary>
        Underpants,
        /// <summary></summary>
        Skirt,
        /// <summary></summary>
        Invalid = 255
    };

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

    /// <summary>
    /// Represents an Animation
    /// </summary>
    public class AssetAnimation : Asset
    {
        /// <summary>Override the base classes AssetType</summary>
        public override AssetType AssetType { get { return AssetType.Animation; } }

        /// <summary>Default Constructor</summary>
        public AssetAnimation() { }

        /// <summary>
        /// Construct an Asset object of type Animation
        /// </summary>
        /// <param name="assetID">A unique <see cref="UUID"/> specific to this asset</param>
        /// <param name="assetData">A byte array containing the raw asset data</param>
        public AssetAnimation(UUID assetID, byte[] assetData)
            : base(assetID, assetData)
        {
            AssetData = assetData;
        }

        public override void Encode() { }
        public override bool Decode() { return true; }
    }

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
        public AssetNotecard(UUID assetID, byte[] assetData) : base(assetID, assetData) 
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

    /// <summary>
    /// Represents an AssetScriptBinary object containing the 
    /// LSO compiled bytecode of an LSL script
    /// </summary>
    public class AssetScriptBinary : Asset
    {
        /// <summary>Override the base classes AssetType</summary>
        public override AssetType AssetType { get { return AssetType.LSLBytecode; } }

        /// <summary>Initializes a new instance of an AssetScriptBinary object</summary>
        public AssetScriptBinary() { }

        /// <summary>Initializes a new instance of an AssetScriptBinary object with parameters</summary>
        /// <param name="assetID">A unique <see cref="UUID"/> specific to this asset</param>
        /// <param name="assetData">A byte array containing the raw asset data</param>
        public AssetScriptBinary(UUID assetID, byte[] assetData)
            : base(assetID, assetData)
        {
            AssetData = assetData;
        }

        /// <summary>
        /// TODO: Encodes a scripts contents into a LSO Bytecode file
        /// </summary>
        public override void Encode() { }

        /// <summary>
        /// TODO: Decode LSO Bytecode into a string
        /// </summary>
        /// <returns>true</returns>
        public override bool Decode() { return true; }
    }

    /// <summary>
    /// Represents a Sound Asset
    /// </summary>
    public class AssetSound : Asset
    {
        /// <summary>Override the base classes AssetType</summary>
        public override AssetType AssetType { get { return AssetType.Sound; } }

        /// <summary>Initializes a new instance of an AssetSound object</summary>
        public AssetSound() { }

        /// <summary>Initializes a new instance of an AssetSound object with parameters</summary>
        /// <param name="assetID">A unique <see cref="UUID"/> specific to this asset</param>
        /// <param name="assetData">A byte array containing the raw asset data</param>
        public AssetSound(UUID assetID, byte[] assetData)
            : base(assetID, assetData)
        {
            AssetData = assetData;
        }

        /// <summary>
        /// TODO: Encodes a sound file
        /// </summary>
        public override void Encode() { }

        /// <summary>
        /// TODO: Decode a sound file
        /// </summary>
        /// <returns>true</returns>
        public override bool Decode() { return true; }
    }

    /// <summary>
    /// Represents a texture
    /// </summary>
    public class AssetTexture : Asset
    {
        /// <summary>Override the base classes AssetType</summary>
        public override AssetType AssetType { get { return AssetType.Texture; } }

        /// <summary>A <seealso cref="ManagedImage"/> object containing image data</summary>
        public ManagedImage Image;

        /// <summary></summary>
        public OpenJPEG.J2KLayerInfo[] LayerInfo;

        /// <summary></summary>
        public int Components;

        /// <summary>Initializes a new instance of an AssetTexture object</summary>
        public AssetTexture() { }

        /// <summary>
        /// Initializes a new instance of an AssetTexture object
        /// </summary>
        /// <param name="assetID">A unique <see cref="UUID"/> specific to this asset</param>
        /// <param name="assetData">A byte array containing the raw asset data</param>
        public AssetTexture(UUID assetID, byte[] assetData) : base(assetID, assetData) { }
        
        /// <summary>
        /// Initializes a new instance of an AssetTexture object
        /// </summary>
        /// <param name="image">A <seealso cref="ManagedImage"/> object containing texture data</param>
        public AssetTexture(ManagedImage image)
        {
            Image = image;
            Components = 0;
            if ((Image.Channels & ManagedImage.ImageChannels.Color) != 0)
                Components += 3;
            if ((Image.Channels & ManagedImage.ImageChannels.Gray) != 0)
                ++Components;
            if ((Image.Channels & ManagedImage.ImageChannels.Bump) != 0)
                ++Components;
            if ((Image.Channels & ManagedImage.ImageChannels.Alpha) != 0)
                ++Components;
        }

        /// <summary>
        /// Populates the <seealso cref="AssetData"/> byte array with a JPEG2000
        /// encoded image created from the data in <seealso cref="Image"/>
        /// </summary>
        public override void Encode()
        {
            AssetData = OpenJPEG.Encode(Image);
        }
        
        /// <summary>
        /// Decodes the JPEG2000 data in <code>AssetData</code> to the
        /// <seealso cref="ManagedImage"/> object <seealso cref="Image"/>
        /// </summary>
        /// <returns>True if the decoding was successful, otherwise false</returns>
        public override bool Decode()
        {
            Components = 0;

            if (OpenJPEG.DecodeToImage(AssetData, out Image))
            {
                if ((Image.Channels & ManagedImage.ImageChannels.Color) != 0)
                    Components += 3;
                if ((Image.Channels & ManagedImage.ImageChannels.Gray) != 0)
                    ++Components;
                if ((Image.Channels & ManagedImage.ImageChannels.Bump) != 0)
                    ++Components;
                if ((Image.Channels & ManagedImage.ImageChannels.Alpha) != 0)
                    ++Components;

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Decodes the begin and end byte positions for each quality layer in
        /// the image
        /// </summary>
        /// <returns></returns>
        public bool DecodeLayerBoundaries()
        {
            return OpenJPEG.DecodeLayerBoundaries(AssetData, out LayerInfo, out Components);
        }
    }

    /// <summary>
    /// Represents a primitive asset
    /// </summary>
    public class AssetPrim : Asset
    {

        /// <summary>Override the base classes AssetType</summary>
        public override AssetType AssetType { get { return AssetType.Object; } }

        /// <summary>Initializes a new instance of an AssetPrim object</summary>
        public AssetPrim() { }


        /// <summary>
        /// TODO: 
        /// </summary>
        public override void Encode() { }

        /// <summary>
        /// TODO: 
        /// </summary>
        /// <returns>true</returns>
        public override bool Decode() { return true; }
    }

    /// <summary>
    /// Represents a Wearable Asset, Clothing, Hair, Skin, Etc
    /// </summary>
    public abstract class AssetWearable : Asset
    {
        /// <summary>A string containing the name of the asset</summary>
        public string Name = String.Empty;
        /// <summary>A string containing a short description of the asset</summary>
        public string Description = String.Empty;
        /// <summary>The Assets WearableType</summary>
        public WearableType WearableType = WearableType.Shape;
        /// <summary>The For-Sale status of the object</summary>
        public SaleType ForSale;
        /// <summary>An Integer representing the purchase price of the asset</summary>
        public int SalePrice;
        /// <summary>The <seealso cref="UUID"/> of the assets creator</summary>
        public UUID Creator;
        /// <summary>The <seealso cref="UUID"/> of the assets current owner</summary>
        public UUID Owner;
        /// <summary>The <seealso cref="UUID"/> of the assets prior owner</summary>
        public UUID LastOwner;
        /// <summary>The <seealso cref="UUID"/> of the Group this asset is set to</summary>
        public UUID Group;
        /// <summary>True if the asset is owned by a <seealso cref="Group"/></summary>
        public bool GroupOwned;
        /// <summary>The Permissions mask of the asset</summary>
        public Permissions Permissions;
        /// <summary>A Dictionary containing Key/Value pairs of the objects parameters</summary>
        public Dictionary<int, float> Params = new Dictionary<int, float>();
        /// <summary>A Dictionary containing Key/Value pairs where the Key is the textures Index and the Value is the Textures <seealso cref="UUID"/></summary>
        public Dictionary<AppearanceManager.TextureIndex, UUID> Textures = new Dictionary<AppearanceManager.TextureIndex, UUID>();

        /// <summary>Initializes a new instance of an AssetWearable object</summary>
        public AssetWearable() { }

        /// <summary>Initializes a new instance of an AssetWearable object with parameters</summary>
        /// <param name="assetID">A unique <see cref="UUID"/> specific to this asset</param>
        /// <param name="assetData">A byte array containing the raw asset data</param>
        public AssetWearable(UUID assetID, byte[] assetData) : base(assetID, assetData) { }

        /// <summary>Initializes a new instance of an AssetWearable object with parameters</summary>
        /// <param name="source">A string containing the asset parameters</param>
        public AssetWearable(string source)
        {
            AssetData = Utils.StringToBytes(source);
        }

        /// <summary>
        /// Decode an assets byte encoded data to a string
        /// </summary>
        /// <returns>true if the asset data was decoded successfully</returns>
        public override bool Decode()
        {
            int version = -1;
            Permissions = new Permissions();
            string data = Utils.BytesToString(AssetData);

            data = data.Replace("\r", String.Empty);
            string[] lines = data.Split('\n');
            for (int stri = 0; stri < lines.Length; stri++)
            {
                if (stri == 0)
                {
                    string versionstring = lines[stri];
                    version = Int32.Parse(versionstring.Split(' ')[2]);
                    if (version != 22 && version != 18)
                        return false;
                }
                else if (stri == 1)
                {
                    Name = lines[stri];
                }
                else if (stri == 2)
                {
                    Description = lines[stri];
                }
                else
                {
                    string line = lines[stri].Trim();
                    string[] fields = line.Split('\t');

                    if (fields.Length == 1)
                    {
                        fields = line.Split(' ');
                        if (fields[0] == "parameters")
                        {
                            int count = Int32.Parse(fields[1]) + stri;
                            for (; stri < count; )
                            {
                                stri++;
                                line = lines[stri].Trim();
                                fields = line.Split(' ');

                                int id = Int32.Parse(fields[0]);
                                if (fields[1] == ",")
                                    fields[1] = "0";
                                else
                                    fields[1] = fields[1].Replace(',', '.');

                                float weight = float.Parse(fields[1], System.Globalization.NumberStyles.Float,
                                    Utils.EnUsCulture.NumberFormat);

                                Params[id] = weight;
                            }
                        }
                        else if (fields[0] == "textures")
                        {
                            int count = Int32.Parse(fields[1]) + stri;
                            for (; stri < count; )
                            {
                                stri++;
                                line = lines[stri].Trim();
                                fields = line.Split(' ');

                                AppearanceManager.TextureIndex id = (AppearanceManager.TextureIndex)Int32.Parse(fields[0]);
                                UUID texture = new UUID(fields[1]);

                                Textures[id] = texture;
                            }
                        }
                        else if (fields[0] == "type")
                        {
                            WearableType = (WearableType)Int32.Parse(fields[1]);
                        }

                    }
                    else if (fields.Length == 2)
                    {
                        switch (fields[0])
                        {
                            case "creator_mask":
                                // Deprecated, apply this as the base mask
                                Permissions.BaseMask = (PermissionMask)UInt32.Parse(fields[1], System.Globalization.NumberStyles.HexNumber);
                                break;
                            case "base_mask":
                                Permissions.BaseMask = (PermissionMask)UInt32.Parse(fields[1], System.Globalization.NumberStyles.HexNumber);
                                break;
                            case "owner_mask":
                                Permissions.OwnerMask = (PermissionMask)UInt32.Parse(fields[1], System.Globalization.NumberStyles.HexNumber);
                                break;
                            case "group_mask":
                                Permissions.GroupMask = (PermissionMask)UInt32.Parse(fields[1], System.Globalization.NumberStyles.HexNumber);
                                break;
                            case "everyone_mask":
                                Permissions.EveryoneMask = (PermissionMask)UInt32.Parse(fields[1], System.Globalization.NumberStyles.HexNumber);
                                break;
                            case "next_owner_mask":
                                Permissions.NextOwnerMask = (PermissionMask)UInt32.Parse(fields[1], System.Globalization.NumberStyles.HexNumber);
                                break;
                            case "creator_id":
                                Creator = new UUID(fields[1]);
                                break;
                            case "owner_id":
                                Owner = new UUID(fields[1]);
                                break;
                            case "last_owner_id":
                                LastOwner = new UUID(fields[1]);
                                break;
                            case "group_id":
                                Group = new UUID(fields[1]);
                                break;
                            case "group_owned":
                                GroupOwned = (Int32.Parse(fields[1]) != 0);
                                break;
                            case "sale_type":
                                ForSale = Utils.StringToSaleType(fields[1]);
                                break;
                            case "sale_price":
                                SalePrice = Int32.Parse(fields[1]);
                                break;
                            case "sale_info":
                                // Container for sale_type and sale_price, ignore
                                break;
                            default:
                                return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Encode the assets string represantion into a format consumable by the asset server
        /// </summary>
        public override void Encode()
        {
            const string NL = "\n";

            StringBuilder data = new StringBuilder("LLWearable version 22\n");
            data.Append(Name); data.Append(NL); data.Append(NL);
            data.Append("\tpermissions 0\n\t{\n");
            data.Append("\t\tbase_mask\t"); data.Append(Utils.UIntToHexString((uint)Permissions.BaseMask)); data.Append(NL);
            data.Append("\t\towner_mask\t"); data.Append(Utils.UIntToHexString((uint)Permissions.OwnerMask)); data.Append(NL);
            data.Append("\t\tgroup_mask\t"); data.Append(Utils.UIntToHexString((uint)Permissions.GroupMask)); data.Append(NL);
            data.Append("\t\teveryone_mask\t"); data.Append(Utils.UIntToHexString((uint)Permissions.EveryoneMask)); data.Append(NL);
            data.Append("\t\tnext_owner_mask\t"); data.Append(Utils.UIntToHexString((uint)Permissions.NextOwnerMask)); data.Append(NL);
            data.Append("\t\tcreator_id\t"); data.Append(Creator.ToString()); data.Append(NL);
            data.Append("\t\towner_id\t"); data.Append(Owner.ToString()); data.Append(NL);
            data.Append("\t\tlast_owner_id\t"); data.Append(LastOwner.ToString()); data.Append(NL);
            data.Append("\t\tgroup_id\t"); data.Append(Group.ToString()); data.Append(NL);
            if (GroupOwned) data.Append("\t\tgroup_owned\t1\n");
            data.Append("\t}\n");
            data.Append("\tsale_info\t0\n");
            data.Append("\t{\n");
            data.Append("\t\tsale_type\t"); data.Append(Utils.SaleTypeToString(ForSale)); data.Append(NL);
            data.Append("\t\tsale_price\t"); data.Append(SalePrice); data.Append(NL);
            data.Append("\t}\n");
            data.Append("type "); data.Append((int)WearableType); data.Append(NL);

            data.Append("parameters "); data.Append(Params.Count); data.Append(NL);
            foreach (KeyValuePair<int, float> param in Params)
            {
                data.Append(param.Key); data.Append(" "); data.Append(Helpers.FloatToTerseString(param.Value)); data.Append(NL);
            }

            data.Append("textures "); data.Append(Textures.Count); data.Append(NL);
            foreach (KeyValuePair<AppearanceManager.TextureIndex, UUID> texture in Textures)
            {
                data.Append(texture.Key); data.Append(" "); data.Append(texture.Value.ToString()); data.Append(NL);
            }

            AssetData = Utils.StringToBytes(data.ToString());
        }
    }


    /// <summary>
    /// Represents an <seealso cref="AssetWearable"/> that can be worn on an avatar
    /// such as a Shirt, Pants, etc.
    /// </summary>
    public class AssetClothing : AssetWearable
    {
        /// <summary>Override the base classes AssetType</summary>
        public override AssetType AssetType { get { return AssetType.Clothing; } }

        /// <summary>Initializes a new instance of an AssetScriptBinary object</summary>
        public AssetClothing() { }

        /// <summary>Initializes a new instance of an AssetScriptBinary object with parameters</summary>
        /// <param name="assetID">A unique <see cref="UUID"/> specific to this asset</param>
        /// <param name="assetData">A byte array containing the raw asset data</param>
        public AssetClothing(UUID assetID, byte[] assetData) : base(assetID, assetData) { }

        /// <summary>Initializes a new instance of an AssetScriptBinary object with parameters</summary>
        /// <param name="source">A string containing the Clothings data</param>
        public AssetClothing(string source) : base(source) { }
    }

    /// <summary>
    /// Represents an <seealso cref="AssetWearable"/> that represents an avatars body ie: Hair, Etc.
    /// </summary>
    public class AssetBodypart : AssetWearable
    {
        /// <summary>Override the base classes AssetType</summary>
        public override AssetType AssetType { get { return AssetType.Bodypart; } }

        /// <summary>Initializes a new instance of an AssetBodyPart object</summary>
        public AssetBodypart() { }

        /// <summary>Initializes a new instance of an AssetBodyPart object with parameters</summary>
        /// <param name="assetID">A unique <see cref="UUID"/> specific to this asset</param>
        /// <param name="assetData">A byte array containing the raw asset data</param>
        public AssetBodypart(UUID assetID, byte[] assetData) : base(assetID, assetData) { }

        
        /// <summary>Initializes a new instance of an AssetBodyPart object with parameters</summary>
        /// <param name="source">A string representing the values of the Bodypart</param>
        public AssetBodypart(string source) : base(source) { }
    }
}
