using System;
using System.Text;
using System.Collections.Generic;

namespace libsecondlife
{
    public enum WearableType : byte
    {
        Shape = 0,
        Skin,
        Hair,
        Eyes,
        Shirt,
        Pants,
        Shoes,
        Socks,
        Jacket,
        Gloves,
        Undershirt,
        Underpants,
        Skirt,
        Invalid = 255
    };

    public abstract class Asset
    {
        public byte[] AssetData;

        private LLUUID _AssetID;
        public LLUUID AssetID
        {
            get { return _AssetID; }
            internal set { _AssetID = value; }
        }

        public abstract AssetType AssetType
        {
            get;
        }

        public Asset() { }

        public Asset(byte[] assetData)
        {
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

    public class AssetNotecard : Asset
    {
        public override AssetType AssetType { get { return AssetType.Notecard; } }

        public string Text = null;

        public AssetNotecard() { }
        
        public AssetNotecard(byte[] assetData) : base(assetData) 
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
            AssetData = Helpers.StringToField(temp);
        }
        
        public override bool Decode()
        {
            Text = Helpers.FieldToUTF8String(AssetData);
            return true;
        }
    }

    public class AssetScriptText : Asset
    {
        public override AssetType AssetType { get { return AssetType.LSLText; } }

        public string Source;

        public AssetScriptText() { }
        
        public AssetScriptText(byte[] assetData) : base(assetData) { }
        
        public AssetScriptText(string source)
        {
            Source = source;
        }

        public override void Encode()
        {
            AssetData = Helpers.StringToField(Source);
        }

        public override bool Decode()
        {
            Source = Helpers.FieldToUTF8String(AssetData);
            return true;
        }
    }

    public class AssetScriptBinary : Asset
    {
        public override AssetType AssetType { get { return AssetType.LSLBytecode; } }

        public byte[] Bytecode;

        public AssetScriptBinary() { }
        
        public AssetScriptBinary(byte[] assetData) : base (assetData)
        {
            Bytecode = assetData;
        }

        public override void Encode() { AssetData = Bytecode; }
        public override bool Decode() { Bytecode = AssetData; return true; }
    }

    public class AssetTexture : Asset
    {
        public override AssetType AssetType { get { return AssetType.Texture; } }

        public Image Image;
        
        public AssetTexture() { }

        public AssetTexture(byte[] assetData) : base(assetData) { }
        
        public AssetTexture(Image image)
        {
            Image = image;
        }

        public override void Encode()
        {
#if PocketPC
            throw new Exception("OpenJPEG encoding is not supported on the PocketPC");
#else
            AssetData = OpenJPEGNet.OpenJPEG.Encode(Image);
#endif
        }
        
        public override bool Decode()
        {
#if PocketPC
            throw new Exception("OpenJPEG decoding is not supported on the PocketPC");
#else
            Image = OpenJPEGNet.OpenJPEG.Decode(AssetData);
            return true;
#endif
        }
    }

    public class AssetPrim : Asset
    {
        public override AssetType AssetType { get { return AssetType.Primitive; } }

        public AssetPrim() { }

        public override void Encode() { }
        public override bool Decode() { return false; }
    }

    public class AssetSound : Asset
    {
        public override AssetType AssetType { get { return AssetType.Sound; } }

        public AssetSound() { }

        // TODO: Sometime we could add OGG encoding/decoding?
        public override void Encode() { }
        public override bool Decode() { return false; }
    }

    public abstract class AssetWearable : Asset
    {
        public string Name = String.Empty;
        public string Description = String.Empty;
        public WearableType WearableType = WearableType.Shape;
        public SaleType ForSale;
        public int SalePrice;
        public LLUUID Creator;
        public LLUUID Owner;
        public LLUUID LastOwner;
        public LLUUID Group;
        public bool GroupOwned;
        public Permissions Permissions;
        public Dictionary<int, float> Params = new Dictionary<int, float>();
        public Dictionary<AppearanceManager.TextureIndex, LLUUID> Textures = new Dictionary<AppearanceManager.TextureIndex, LLUUID>();

        public AssetWearable() { }

        public AssetWearable(byte[] assetData) : base(assetData) { }

        public AssetWearable(string source)
        {
            AssetData = Helpers.StringToField(source);
        }

        public override bool Decode()
        {
            int version = -1;
            Permissions = new Permissions();
            string data = Helpers.FieldToUTF8String(AssetData);

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
                                {
                                    fields[1] = "0";
                                }

                                else
                                {
                                    fields[1] = fields[1].Replace(',', '.');
                                }
                                float weight = float.Parse(fields[1], System.Globalization.NumberStyles.Float,
                                    Helpers.EnUsCulture.NumberFormat);

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
                                LLUUID texture = new LLUUID(fields[1]);

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
                                Creator = new LLUUID(fields[1]);
                                break;
                            case "owner_id":
                                Owner = new LLUUID(fields[1]);
                                break;
                            case "last_owner_id":
                                LastOwner = new LLUUID(fields[1]);
                                break;
                            case "group_id":
                                Group = new LLUUID(fields[1]);
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
            data.Append("\t\tbase_mask\t"); data.Append(Helpers.UIntToHexString((uint)Permissions.BaseMask)); data.Append(NL);
            data.Append("\t\towner_mask\t"); data.Append(Helpers.UIntToHexString((uint)Permissions.OwnerMask)); data.Append(NL);
            data.Append("\t\tgroup_mask\t"); data.Append(Helpers.UIntToHexString((uint)Permissions.GroupMask)); data.Append(NL);
            data.Append("\t\teveryone_mask\t"); data.Append(Helpers.UIntToHexString((uint)Permissions.EveryoneMask)); data.Append(NL);
            data.Append("\t\tnext_owner_mask\t"); data.Append(Helpers.UIntToHexString((uint)Permissions.NextOwnerMask)); data.Append(NL);
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
            foreach (KeyValuePair<AppearanceManager.TextureIndex, LLUUID> texture in Textures)
            {
                data.Append(texture.Key); data.Append(" "); data.Append(texture.Value.ToString()); data.Append(NL);
            }

            AssetData = Helpers.StringToField(data.ToString());
        }
    }

    public class AssetClothing : AssetWearable
    {
        public override AssetType AssetType { get { return AssetType.Clothing; } }

        public AssetClothing() { }
        public AssetClothing(byte[] assetData) : base(assetData) { }
        public AssetClothing(string source) : base(source) { }
    }

    public class AssetBodypart : AssetWearable
    {
        public override AssetType AssetType { get { return AssetType.Bodypart; } }

        public AssetBodypart() { }
        public AssetBodypart(byte[] assetData) : base(assetData) { }
        public AssetBodypart(string source) : base(source) { }
    }
}
