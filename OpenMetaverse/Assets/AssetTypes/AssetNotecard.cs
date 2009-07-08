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
using System.Text.RegularExpressions;
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

        /// <summary>A text string containing main text of the notecard</summary>
        public string BodyText = null;

        /// <summary>List of <see cref="OpenMetaverse.InventoryItem"/>s embedded on the notecard</summary>
        public List<InventoryItem> EmbeddedItems = null;

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
        /// <param name="text">A text string containing the main body text of the notecard</param>
        public AssetNotecard(string text)
        {
            BodyText = text;
            Encode();
        }

        /// <summary>
        /// Encode the raw contents of a string with the specific Linden Text properties
        /// </summary>
        public override void Encode()
        {
            string body = BodyText ?? String.Empty;

            StringBuilder output = new StringBuilder();
            output.AppendLine("Linden text version 2");
            output.AppendLine("{");
            output.AppendLine("LLEmbeddedItems version 1");
            output.AppendLine("{");

            if (EmbeddedItems != null)
            {
                output.AppendLine("count " + EmbeddedItems.Count);
                output.AppendLine("{");

                for (int i = 0; i < EmbeddedItems.Count; i++)
                {
                    InventoryItem item = EmbeddedItems[i];

                    output.AppendLine("\tinv_item\t" + i);
                    output.AppendLine("\t{");

                    output.AppendLine("\t\titem_id\t" + item.UUID);
                    output.AppendLine("\t\tparent_id\t" + item.ParentUUID);

                    output.AppendLine("\tpermissions\t0");
                    output.AppendLine("\t{");
                    output.AppendLine("\t\tbase_mask\t" + ((uint)item.Permissions.BaseMask).ToString("x"));
                    output.AppendLine("\t\towner_mask\t" + ((uint)item.Permissions.OwnerMask).ToString("x"));
                    output.AppendLine("\t\tgroup_mask\t" + ((uint)item.Permissions.GroupMask).ToString("x"));
                    output.AppendLine("\t\teveryone_mask\t" + ((uint)item.Permissions.EveryoneMask).ToString("x"));
                    output.AppendLine("\t\tnext_owner_mask\t" + ((uint)item.Permissions.NextOwnerMask).ToString("x"));
                    output.AppendLine("\t\tcreator_id\t" + item.CreatorID);
                    output.AppendLine("\t\towner_id\t" + item.OwnerID);
                    output.AppendLine("\t\tlast_owner_id\t" + UUID.Zero);
                    output.AppendLine("\t\tgroup_id\t" + item.GroupID);
                    output.AppendLine("\t}");

                    output.AppendLine("\t\tasset_id\t" + item.AssetUUID);
                    output.AppendLine("\t\ttype\t" + Utils.AssetTypeToString(item.AssetType));
                    output.AppendLine("\t\tinv_type\t" + Utils.InventoryTypeToString(item.InventoryType));
                    output.AppendLine("\t\tflags\t" + item.Flags.ToString().PadLeft(8, '0'));

                    output.AppendLine("\tsale_info\t0");
                    output.AppendLine("\t{");
                    output.AppendLine("\t\tsale_type\t" + Utils.SaleTypeToString(item.SaleType));
                    output.AppendLine("\t\tsale_price\t" + item.SalePrice);
                    output.AppendLine("\t}");

                    output.AppendLine("\t\tname\t" + item.Name.Replace('|', '_') + "|");
                    output.AppendLine("\t\tdesc\t" + item.Description.Replace('|', '_') + "|");
                    output.AppendLine("\t\tcreation_date\t" + Utils.DateTimeToUnixTime(item.CreationDate));

                    output.AppendLine("\t}");
                }

                output.AppendLine("}");
            }
            else
            {
                output.AppendLine("count 0");
            }

            output.AppendLine("}");
            output.AppendLine("Text length " + body.Length);
            output.Append(body + "}");

            AssetData = Utils.StringToBytes(output.ToString());
        }

        /// <summary>
        /// Decode the raw asset data including the Linden Text properties
        /// </summary>
        /// <returns>true if the AssetData was successfully decoded to a string</returns>
        public override bool Decode()
        {
            string data = Utils.BytesToString(AssetData);
            EmbeddedItems = new List<InventoryItem>();
            BodyText = string.Empty;

            try
            {
                string[] lines = data.Split('\n');
                int i = 0;
                Match m;

                // Version
                if (!(m = Regex.Match(lines[i++], @"Linden text version\s+(\d+)")).Success)
                    throw new Exception("could not determine version");
                int notecardVersion = int.Parse(m.Groups[1].Value);
                if (notecardVersion < 1 || notecardVersion > 2)
                    throw new Exception("unsuported version");
                if (!(m = Regex.Match(lines[i++], @"\s*{$")).Success)
                    throw new Exception("wrong format");

                // Embedded items header
                if (!(m = Regex.Match(lines[i++], @"LLEmbeddedItems version\s+(\d+)")).Success)
                    throw new Exception("could not determine embedded items version version");
                if (m.Groups[1].Value != "1")
                    throw new Exception("unsuported embedded item version");
                if (!(m = Regex.Match(lines[i++], @"\s*{$")).Success)
                    throw new Exception("wrong format");

                // Item count
                if (!(m = Regex.Match(lines[i++], @"count\s+(\d+)")).Success)
                    throw new Exception("wrong format");
                int count = int.Parse(m.Groups[1].Value);

                // Decode individual items
                for (int n = 0; n < count; n++)
                {
                    if (!(m = Regex.Match(lines[i++], @"\s*{$")).Success)
                        throw new Exception("wrong format");

                    // Index
                    if (!(m = Regex.Match(lines[i++], @"ext char index\s+(\d+)")).Success)
                        throw new Exception("missing ext char index");
                    int index = int.Parse(m.Groups[1].Value);

                    // Inventory item
                    if (!(m = Regex.Match(lines[i++], @"inv_item\s+0")).Success)
                        throw new Exception("missing inv item");

                    // Item itself
                    InventoryItem embedded = new InventoryItem(UUID.Zero);
                    while (true)
                    {
                        if (!(m = Regex.Match(lines[i++], @"([^\s]+)(\s+)?(.*)?")).Success)
                            throw new Exception("wrong format");
                        string key = m.Groups[1].Value;
                        string val = m.Groups[3].Value;
                        if (key == "{")
                            continue;
                        if (key == "}")
                            break;
                        else if (key == "permissions")
                        {
                            uint baseMask = 0;
                            uint ownerMask = 0;
                            uint groupMask = 0;
                            uint everyoneMask = 0;
                            uint nextOwnerMask = 0;

                            while (true)
                            {
                                if (!(m = Regex.Match(lines[i++], @"([^\s]+)(\s+)?([^\s]+)?")).Success)
                                    throw new Exception("wrong format");
                                string pkey = m.Groups[1].Value;
                                string pval = m.Groups[3].Value;

                                if (pkey == "{")
                                    continue;
                                if (pkey == "}")
                                    break;
                                else if (pkey == "creator_id")
                                {
                                    embedded.CreatorID = new UUID(pval);
                                }
                                else if (pkey == "owner_id")
                                {
                                    embedded.OwnerID = new UUID(pval);
                                }
                                else if (pkey == "group_id")
                                {
                                    embedded.GroupID = new UUID(pval);
                                }
                                else if (pkey == "base_mask")
                                {
                                    baseMask = uint.Parse(pval, System.Globalization.NumberStyles.AllowHexSpecifier);
                                }
                                else if (pkey == "owner_mask")
                                {
                                    ownerMask = uint.Parse(pval, System.Globalization.NumberStyles.AllowHexSpecifier);
                                }
                                else if (pkey == "group_mask")
                                {
                                    groupMask = uint.Parse(pval, System.Globalization.NumberStyles.AllowHexSpecifier);
                                }
                                else if (pkey == "everyone_mask")
                                {
                                    everyoneMask = uint.Parse(pval, System.Globalization.NumberStyles.AllowHexSpecifier);
                                }
                                else if (pkey == "next_owner_mask")
                                {
                                    nextOwnerMask = uint.Parse(pval, System.Globalization.NumberStyles.AllowHexSpecifier);
                                }
                            }
                            embedded.Permissions = new Permissions(baseMask, everyoneMask, groupMask, nextOwnerMask, ownerMask);
                        }
                        else if (key == "sale_info")
                        {
                            while (true)
                            {
                                if (!(m = Regex.Match(lines[i++], @"([^\s]+)(\s+)?([^\s]+)?")).Success)
                                    throw new Exception("wrong format");
                                string pkey = m.Groups[1].Value;
                                string pval = m.Groups[3].Value;

                                if (pkey == "{")
                                    continue;
                                if (pkey == "}")
                                    break;
                                else if (pkey == "sale_price")
                                {
                                    embedded.SalePrice = int.Parse(pval);
                                }
                                else if (pkey == "sale_type")
                                {
                                    embedded.SaleType = Utils.StringToSaleType(pval);
                                }
                            }
                        }
                        else if (key == "item_id")
                        {
                            embedded.UUID = new UUID(val);
                        }
                        else if (key == "parent_id")
                        {
                            embedded.ParentUUID = new UUID(val);
                        }
                        else if (key == "asset_id")
                        {
                            embedded.AssetUUID = new UUID(val);
                        }
                        else if (key == "type")
                        {
                            embedded.AssetType = Utils.StringToAssetType(val);
                        }
                        else if (key == "inv_type")
                        {
                            embedded.InventoryType = Utils.StringToInventoryType(val);
                        }
                        else if (key == "flags")
                        {
                            embedded.Flags = uint.Parse(val, System.Globalization.NumberStyles.AllowHexSpecifier);
                        }
                        else if (key == "name")
                        {
                            embedded.Name = val.Remove(val.LastIndexOf("|"));
                        }
                        else if (key == "desc")
                        {
                            embedded.Description = val.Remove(val.LastIndexOf("|"));
                        }
                        else if (key == "creation_date")
                        {
                            embedded.CreationDate = Utils.UnixTimeToDateTime(int.Parse(val));
                        }
                    }
                    EmbeddedItems.Add(embedded);

                    if (!(m = Regex.Match(lines[i++], @"\s*}$")).Success)
                        throw new Exception("wrong format");

                }

                // Text size
                if (!(m = Regex.Match(lines[i++], @"\s*}$")).Success)
                    throw new Exception("wrong format");
                if (!(m = Regex.Match(lines[i++], @"Text length\s+(\d+)")).Success)
                    throw new Exception("could not determine text length");

                // Read the rest of the notecard
                while (i < lines.Length)
                {
                    BodyText += lines[i++] + "\n";
                }
                BodyText = BodyText.Remove(BodyText.LastIndexOf("}"));
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log("Decoding notecard asset failed: " + ex.Message, Helpers.LogLevel.Error);
                return false;
            }
        }
    }
}
