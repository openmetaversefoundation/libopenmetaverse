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
    /// Each inventory AssetType will have its own set of flags, these are the known flags for AssetType=Object
    /// </summary>
    [Flags]
    public enum ObjectType : uint
    {
        None = 0,
        /// <summary>
        /// A Landmark that has not been previously visited shows up as a dark red pushpin, one that has been
        /// visited shows up as a light red pushpin
        /// </summary>
        VisitedLandmark = 1,
        /// <summary>If set, indicates rezzed object will have more restrictive permissions masks;
        /// Which masks will be affected are below</summary>
        RestrictNextOwner = 0x100,
        /// <summary>If set, and <c>RestrictNextOwner</c> bit is set indicates BaseMask will be overwritten on Rez</summary>
        OverwriteBase = 0x010000,
        /// <summary>If set, and <c>RestrictNextOwner</c> bit is set indicates OwnerMask will be overwritten on Rez</summary>
        OverwriteOwner = 0x020000,
        /// <summary>If set, and <c>RestrictNextOwner</c> bit is set indicates GroupMask will be overwritten on Rez</summary>
        OverwriteGroup = 0x040000,
        /// <summary>If set, and <c>RestrictNextOwner</c> bit is set indicates EveryoneMask will be overwritten on Rez</summary>
        OverwriteEveryone = 0x080000,
        /// <summary>If set, and <c>RestrictNextOwner</c> bit is set indicates NextOwnerMask will be overwritten on Rez</summary>
        OverwriteNextOwner = 0x100000,
        /// <summary>If set, indicates item is multiple items coalesced into a single item</summary>
        MultipleObjects = 0x200000
    }

    public abstract class Asset
    {
        public byte[] AssetData;
        public bool Temporary;

        private UUID _AssetID;
        public UUID AssetID
        {
            get { return _AssetID; }
            internal set { _AssetID = value; }
        }

        public abstract AssetType AssetType
        {
            get;
        }

        public Asset() { }

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

    public class AssetAnimation : Asset
    {
        public override AssetType AssetType { get { return AssetType.Animation; } }

        public AssetAnimation() { }

        public AssetAnimation(UUID assetID, byte[] assetData)
            : base(assetID, assetData)
        {
            AssetData = assetData;
        }

        public override void Encode() { }
        public override bool Decode() { return true; }
    }

    public class AssetNotecard : Asset
    {
        public override AssetType AssetType { get { return AssetType.Notecard; } }

        public string Text = null;

        public AssetNotecard() { }
        
        public AssetNotecard(UUID assetID, byte[] assetData) : base(assetID, assetData) 
        {
            Decode();
        }
        
        public AssetNotecard(string text)
        {
            Text = text;
            Encode();
        }

        public override void Encode()
        {

            string temp = "Linden text version 2\n{\nLLEmbeddedItems version 1\n{\ncount 0\n}\nText length ";
            temp += Text.Length + "\n";
            temp += Text;
            temp += "}";
            AssetData = Utils.StringToBytes(temp);
        }
        
        public override bool Decode()
        {
            Text = Utils.BytesToString(AssetData);
            return true;
        }
    }

    public class AssetScriptText : Asset
    {
        public override AssetType AssetType { get { return AssetType.LSLText; } }

        public string Source;

        public AssetScriptText() { }

        public AssetScriptText(UUID assetID, byte[] assetData) : base(assetID, assetData) { }
        
        public AssetScriptText(string source)
        {
            Source = source;
        }

        public override void Encode()
        {
            AssetData = Utils.StringToBytes(Source);
        }

        public override bool Decode()
        {
            Source = Utils.BytesToString(AssetData);
            return true;
        }
    }

    public class AssetScriptBinary : Asset
    {
        public override AssetType AssetType { get { return AssetType.LSLBytecode; } }

        public AssetScriptBinary() { }

        public AssetScriptBinary(UUID assetID, byte[] assetData)
            : base(assetID, assetData)
        {
            AssetData = assetData;
        }

        public override void Encode() { }
        public override bool Decode() { return true; }
    }

    public class AssetSound : Asset
    {
        public override AssetType AssetType { get { return AssetType.Sound; } }

        public AssetSound() { }

        public AssetSound(UUID assetID, byte[] assetData)
            : base(assetID, assetData)
        {
            AssetData = assetData;
        }

        public override void Encode() { }
        public override bool Decode() { return true; }
    }

    public class AssetTexture : Asset
    {
        public override AssetType AssetType { get { return AssetType.Texture; } }

        public ManagedImage Image;
        public OpenJPEG.J2KLayerInfo[] LayerInfo;
        public int Components;
        
        public AssetTexture() { }

        public AssetTexture(UUID assetID, byte[] assetData) : base(assetID, assetData) { }
        
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

    public class AssetPrim : Asset
    {
        public override AssetType AssetType { get { return AssetType.Object; } }

        public AssetPrim() { }

        public override void Encode() { }
        public override bool Decode() { return true; }
    }

    public abstract class AssetWearable : Asset
    {
        public string Name = String.Empty;
        public string Description = String.Empty;
        public WearableType WearableType = WearableType.Shape;
        public SaleType ForSale;
        public int SalePrice;
        public UUID Creator;
        public UUID Owner;
        public UUID LastOwner;
        public UUID Group;
        public bool GroupOwned;
        public Permissions Permissions;
        public Dictionary<int, float> Params = new Dictionary<int, float>();
        public Dictionary<AppearanceManager.TextureIndex, UUID> Textures = new Dictionary<AppearanceManager.TextureIndex, UUID>();

        public AssetWearable() { }

        public AssetWearable(UUID assetID, byte[] assetData) : base(assetID, assetData) { }

        public AssetWearable(string source)
        {
            AssetData = Utils.StringToBytes(source);
        }

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
                                ForSale = InventoryManager.StringToSaleType(fields[1]);
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
            data.Append("\t\tsale_type\t"); data.Append(InventoryManager.SaleTypeToString(ForSale)); data.Append(NL);
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

    public class AssetClothing : AssetWearable
    {
        public override AssetType AssetType { get { return AssetType.Clothing; } }

        public AssetClothing() { }
        public AssetClothing(UUID assetID, byte[] assetData) : base(assetID, assetData) { }
        public AssetClothing(string source) : base(source) { }
    }

    public class AssetBodypart : AssetWearable
    {
        public override AssetType AssetType { get { return AssetType.Bodypart; } }

        public AssetBodypart() { }
        public AssetBodypart(UUID assetID, byte[] assetData) : base(assetID, assetData) { }
        public AssetBodypart(string source) : base(source) { }
    }
}
