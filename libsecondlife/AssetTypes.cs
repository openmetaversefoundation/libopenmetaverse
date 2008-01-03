using System;
using System.Text;
using System.Collections.Generic;

namespace libsecondlife
{
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
        public abstract void Decode();
    }

    public class AssetNotecard : Asset
    {
        public override AssetType AssetType { get { return AssetType.Notecard; } }

        public string Text = null;

        public AssetNotecard() { }
        
        public AssetNotecard(byte[] assetData) : base(assetData) { }
        
        public AssetNotecard(string text)
        {
            Text = text;
        }

        public override void Encode()
        {
            AssetData = Helpers.StringToField(Text);
        }
        
        public override void Decode()
        {
            Text = Helpers.FieldToUTF8String(AssetData);
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

        public override void Decode()
        {
            Source = Helpers.FieldToUTF8String(AssetData);
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
        public override void Decode() { Bytecode = AssetData; }
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
            AssetData = OpenJPEGNet.OpenJPEG.Encode(Image);
        }
        
        public override void Decode()
        {
            Image = OpenJPEGNet.OpenJPEG.Decode(AssetData);
        }
    }

    public class AssetPrim : Asset
    {
        public override AssetType AssetType { get { return AssetType.Primitive; } }

        public AssetPrim() { }

        public override void Encode() { }
        public override void Decode() { }
    }

    public class AssetSound : Asset
    {
        public override AssetType AssetType { get { return AssetType.Sound; } }

        public AssetSound() { }

        // TODO: Sometime we could add OGG encoding/decoding?
        public override void Encode() { }
        public override void Decode() { }
    }

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

        public override void Decode()
        {
            int version = -1;
            int n = -1;
            string data = Helpers.FieldToUTF8String(AssetData);

            n = data.IndexOf('\n');
            version = Int32.Parse(data.Substring(19, n - 18));
            data = data.Remove(0, n);

            if (version != 22)
                throw new Exception("Wearable asset has unrecognized version " + version);

            n = data.IndexOf('\n');
            Name = data.Substring(0, n);
            data = data.Remove(0, n);

            n = data.IndexOf('\n');
            Description = data.Substring(0, n);
            data = data.Remove(0, n);

            // Split in to an upper and lower half
            string[] parts = data.Split(new string[] { "parameters" }, StringSplitOptions.None);
            parts[1] = "parameters" + parts[1];

            Permissions = new Permissions();

            // Parse the upper half
            string[] lines = parts[0].Split('\n');
            foreach (string thisline in lines)
            {
                string line = thisline.Trim();
                string[] fields = line.Split('\t');

                if (fields.Length == 2)
                {
                    if (fields[0] == "creator_mask")
                    {
                        // Deprecated, apply this as the base mask
                        Permissions.BaseMask = (PermissionMask)UInt32.Parse(fields[1], System.Globalization.NumberStyles.HexNumber);
                    }
                    else if (fields[0] == "base_mask")
                    {
                        Permissions.BaseMask = (PermissionMask)UInt32.Parse(fields[1], System.Globalization.NumberStyles.HexNumber);
                    }
                    else if (fields[0] == "owner_mask")
                    {
                        Permissions.OwnerMask = (PermissionMask)UInt32.Parse(fields[1], System.Globalization.NumberStyles.HexNumber);
                    }
                    else if (fields[0] == "group_mask")
                    {
                        Permissions.GroupMask = (PermissionMask)UInt32.Parse(fields[1], System.Globalization.NumberStyles.HexNumber);
                    }
                    else if (fields[0] == "everyone_mask")
                    {
                        Permissions.EveryoneMask = (PermissionMask)UInt32.Parse(fields[1], System.Globalization.NumberStyles.HexNumber);
                    }
                    else if (fields[0] == "next_owner_mask")
                    {
                        Permissions.NextOwnerMask = (PermissionMask)UInt32.Parse(fields[1], System.Globalization.NumberStyles.HexNumber);
                    }
                    else if (fields[0] == "creator_id")
                    {
                        Creator = new LLUUID(fields[1]);
                    }
                    else if (fields[0] == "owner_id")
                    {
                        Owner = new LLUUID(fields[1]);
                    }
                    else if (fields[0] == "last_owner_id")
                    {
                        LastOwner = new LLUUID(fields[1]);
                    }
                    else if (fields[0] == "group_id")
                    {
                        Group = new LLUUID(fields[1]);
                    }
                    else if (fields[0] == "group_owned")
                    {
                        GroupOwned = (Int32.Parse(fields[1]) != 0);
                    }
                    else if (fields[0] == "sale_type")
                    {
                        ForSale = InventoryManager.StringToSaleType(fields[1]);
                    }
                    else if (fields[0] == "sale_price")
                    {
                        SalePrice = Int32.Parse(fields[1]);
                    }
                }
                else if (line.StartsWith("type "))
                {
                    WearableType = (WearableType)Int32.Parse(line.Substring(5));
                    break;
                }
            }

            // Break up the lower half in to parameters and textures
            string[] lowerparts = parts[1].Split(new string[] { "textures" }, StringSplitOptions.None);
            lowerparts[1] = "textures" + lowerparts[1];

            // Parse the parameters
            lines = lowerparts[0].Split('\n');
            foreach (string line in lines)
            {
                string[] fields = line.Split(' ');

                // Use exception handling to deal with all the lines we aren't interested in
                try
                {
                    int id = Int32.Parse(fields[0]);
                    if (fields[1] == ",")
                    {
                        fields[1] = "0";
                    }
                    else
                    {
                        fields[1] = fields[1].Replace(',', '.');
                    }
                    float weight = Single.Parse(fields[1], System.Globalization.NumberStyles.Float,
                        Helpers.EnUsCulture.NumberFormat);

                    Params[id] = weight;
                }
                catch (Exception)
                {
                }
            }

            // Parse the textures
            lines = lowerparts[1].Split('\n');
            foreach (string line in lines)
            {
                string[] fields = line.Split(' ');

                // Use exception handling to deal with all the lines we aren't interested in
                try
                {
                    AppearanceManager.TextureIndex id = (AppearanceManager.TextureIndex)Int32.Parse(fields[0]);
                    LLUUID texture = new LLUUID(fields[1]);

                    Textures[id] = texture;
                }
                catch (Exception)
                {
                }
            }
        }

        public override void Encode()
        {
            StringBuilder data = new StringBuilder("LLWearable version 22\n");
            data.Append(Name); data.Append("\n\n");
            data.Append("\tpermissions 0\n\t{\n");
            data.Append("\t\tbase_mask\t"); data.Append(Helpers.UIntToHexString((uint)Permissions.BaseMask)); data.Append("\n");
            data.Append("\t\towner_mask\t"); data.Append(Helpers.UIntToHexString((uint)Permissions.OwnerMask)); data.Append("\n");
            data.Append("\t\tgroup_mask\t"); data.Append(Helpers.UIntToHexString((uint)Permissions.GroupMask)); data.Append("\n");
            data.Append("\t\teveryone_mask\t"); data.Append(Helpers.UIntToHexString((uint)Permissions.EveryoneMask)); data.Append("\n");
            data.Append("\t\tnext_owner_mask\t"); data.Append(Helpers.UIntToHexString((uint)Permissions.NextOwnerMask)); data.Append("\n");
            data.Append("\t\tcreator_id\t"); data.Append(Creator.ToStringHyphenated()); data.Append("\n");
            data.Append("\t\towner_id\t"); data.Append(Owner.ToStringHyphenated()); data.Append("\n");
            data.Append("\t\tlast_owner_id\t"); data.Append(LastOwner.ToStringHyphenated()); data.Append("\n");
            data.Append("\t\tgroup_id\t"); data.Append(Group.ToStringHyphenated()); data.Append("\n");
            if (GroupOwned) data.Append("\t\tgroup_owned\t1\n");
            data.Append("\t}\n");
            data.Append("\tsale_info\t0\n");
            data.Append("\t{\n");
            data.Append("\t\tsale_type\t"); data.Append(InventoryManager.SaleTypeToString(ForSale)); data.Append("\n");
            data.Append("\t\tsale_price\t"); data.Append(SalePrice); data.Append("\n");
            data.Append("\t}\n");
            data.Append("type "); data.Append((int)WearableType); data.Append("\n");

            data.Append("parameters "); data.Append(Params.Count); data.Append("\n");
            foreach (KeyValuePair<int, float> param in Params)
            {
                data.Append(param.Key); data.Append(" "); data.Append(Helpers.FloatToTerseString(param.Value)); data.Append("\n");
            }

            data.Append("textures "); data.Append(Textures.Count); data.Append("\n");
            foreach (KeyValuePair<AppearanceManager.TextureIndex, LLUUID> texture in Textures)
            {
                data.Append(texture.Key); data.Append(" "); data.Append(texture.Value.ToStringHyphenated()); data.Append("\n");
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
