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
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;

namespace OpenMetaverse.Assets
{
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
        public Dictionary<AvatarTextureIndex, UUID> Textures = new Dictionary<AvatarTextureIndex, UUID>();

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

            try
            {
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

                                    AvatarTextureIndex id = (AvatarTextureIndex)Int32.Parse(fields[0]);
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
            }
            catch (Exception ex)
            {
                Logger.Log("Failed decoding wearable asset " + this.AssetID + ": " + ex.Message,
                    Helpers.LogLevel.Warning);
                return false;
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
            foreach (KeyValuePair<AvatarTextureIndex, UUID> texture in Textures)
            {
                data.Append((byte)texture.Key); data.Append(" "); data.Append(texture.Value.ToString()); data.Append(NL);
            }

            AssetData = Utils.StringToBytes(data.ToString());
        }
    }
}
