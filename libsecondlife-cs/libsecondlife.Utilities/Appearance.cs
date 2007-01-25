/*
 * Copyright (c) 2007, Second Life Reverse Engineering Team
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the Second Life Reverse Engineering Team nor the names
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
using System.Threading;
using libsecondlife;
using libsecondlife.Utilities.Assets;
using libsecondlife.Packets;

namespace libsecondlife.Utilities.Appearance
{
    public enum TextureIndex
    {
        HeadBodypaint = 0,
        UpperShirt,
        LowerPants,
        EyesIris,
        Hair,
        UpperBodypaint,
        LowerBodypaint,
        LowerShoes,
        HeadBaked,
        UpperBaked,
        LowerBaked,
        EyesBaked,
        LowerSocks,
        UpperJacket,
        LowerJacket,
        UpperUndershirt,
        LowerUnderpants,
        Skirt,
        SkirtBaked
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

    public enum ForSale
    {
        Not = 0,
        Original = 1,
        Copy = 2,
        Contents = 3
    }


    public class Wearable
    {
        public string Name = String.Empty;
        public string Description = String.Empty;
        public WearableType Type = WearableType.Shape;
        public ForSale ForSale = ForSale.Not;
        public int SalePrice = 0;
        public LLUUID Creator = LLUUID.Zero;
        public LLUUID Owner = LLUUID.Zero;
        public LLUUID LastOwner = LLUUID.Zero;
        public LLUUID Group = LLUUID.Zero;
        public bool GroupOwned = false;
        public Helpers.PermissionType BasePermissions;
        public Helpers.PermissionType EveryonePermissions;
        public Helpers.PermissionType OwnerPermissions;
        public Helpers.PermissionType NextOwnerPermissions;
        public Helpers.PermissionType GroupPermissions;
        public Dictionary<int, float> Params = new Dictionary<int, float>();
        public Dictionary<int, LLUUID> Textures = new Dictionary<int, LLUUID>();


        private SecondLife Client;
        private string[] ForSaleNames = new string[]
        {
            "not",
            "orig",
            "copy",
            "cntn"
        };


        public Wearable(SecondLife client)
        {
            Client = client;
        }

        public bool ImportAsset(string data)
        {
            int version = -1;
            int n = -1;

            try
            {
                n = data.IndexOf('\n');
                version = Int32.Parse(data.Substring(19, n + 1));
                data = data.Remove(0, n);

                if (version != 22)
                {
                    Client.Log("Wearable asset has unrecognized version " + version, Helpers.LogLevel.Warning);
                    return false;
                }

                
                n = data.IndexOf('\n');
                Name = data.Substring(0, n);
                data = data.Remove(0, n);

                n = data.IndexOf('\n');
                Description = data.Substring(0, n);
                data = data.Remove(0, n);

                // Split in to an upper and lower half
                string[] parts = data.Split(new string[] { "parameters" }, StringSplitOptions.None);
                parts[1] = "parameters" + parts[1];

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
                            BasePermissions = (Helpers.PermissionType)UInt32.Parse(fields[1], System.Globalization.NumberStyles.HexNumber);
                        }
                        else if (fields[0] == "base_mask")
                        {
                            BasePermissions = (Helpers.PermissionType)UInt32.Parse(fields[1], System.Globalization.NumberStyles.HexNumber);
                        }
                        else if (fields[0] == "owner_mask")
                        {
                            OwnerPermissions = (Helpers.PermissionType)UInt32.Parse(fields[1], System.Globalization.NumberStyles.HexNumber);
                        }
                        else if (fields[0] == "group_mask")
                        {
                            GroupPermissions = (Helpers.PermissionType)UInt32.Parse(fields[1], System.Globalization.NumberStyles.HexNumber);
                        }
                        else if (fields[0] == "everyone_mask")
                        {
                            EveryonePermissions = (Helpers.PermissionType)UInt32.Parse(fields[1], System.Globalization.NumberStyles.HexNumber);
                        }
                        else if (fields[0] == "next_owner_mask")
                        {
                            NextOwnerPermissions = (Helpers.PermissionType)UInt32.Parse(fields[1], System.Globalization.NumberStyles.HexNumber);
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
                            for (int i = 0; i < ForSaleNames.Length; i++)
                            {
                                if (fields[1] == ForSaleNames[i])
                                {
                                    ForSale = (ForSale)i;
                                    break;
                                }
                            }
                        }
                        else if (fields[0] == "sale_price")
                        {
                            SalePrice = Int32.Parse(fields[1]);
                        }
                        else if (fields[0] == "perm_mask")
                        {
                            Client.Log("Wearable asset has deprecated perm_mask field, ignoring", Helpers.LogLevel.Warning);
                        }
                    }
                    else if (line.StartsWith("type "))
                    {
                        Type = (WearableType)Int32.Parse(line.Substring(5));
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
                        float weight = Single.Parse(fields[1]);

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
                        int id = Int32.Parse(fields[0]);
                        LLUUID texture = LLUUID.Parse(fields[1]);

                        Textures[id] = texture;
                    }
                    catch (Exception)
                    {
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Client.Log("Failed to parse wearable asset: " + e.ToString(), Helpers.LogLevel.Warning);
            }

            return false;
        }

        public string ExportAsset()
        {
            string data = "LLWearable version 22\n";
            data += Name + "\n\n";
            data += "\tpermissions 0\n\t{\n";
            data += "\t\tbase_mask\t" + Helpers.UIntToHexString((uint)BasePermissions) + "\n";
            data += "\t\towner_mask\t" + Helpers.UIntToHexString((uint)OwnerPermissions) + "\n";
            data += "\t\tgroup_mask\t" + Helpers.UIntToHexString((uint)GroupPermissions) + "\n";
            data += "\t\teveryone_mask\t" + Helpers.UIntToHexString((uint)EveryonePermissions) + "\n";
            data += "\t\tnext_owner_mask\t" + Helpers.UIntToHexString((uint)NextOwnerPermissions) + "\n";
            data += "\t\tcreator_id\t" + Creator.ToStringHyphenated() + "\n";
            data += "\t\towner_id\t" + Owner.ToStringHyphenated() + "\n";
            data += "\t\tlast_owner_id\t" + LastOwner.ToStringHyphenated() + "\n";
            data += "\t\tgroup_id\t" + Group.ToStringHyphenated() + "\n";
            if (GroupOwned) data += "\t\tgroup_owned\t1\n";
            data += "\t}\n";
            data += "\tsale_info\t0\n";
            data += "\t{\n";
            data += "\t\tsale_type\t" + ForSaleNames[(int)ForSale] + "\n";
            data += "\t\tsale_price\t" + SalePrice + "\n";
            data += "\t}\n";
            data += "type " + (int)Type + "\n";

            data += "parameters " + Params.Count + "\n";
            foreach (KeyValuePair<int, float> param in Params)
                data += param.Key + " " + Helpers.FloatToTerseString(param.Value) + "\n";

            data += "textures " + Textures.Count + "\n";
            foreach (KeyValuePair<int, LLUUID> texture in Textures)
                data += texture.Key + " " + texture.Value.ToStringHyphenated() + "\n";

            return data;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class AppearanceManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="wearables">A mapping of WearableTypes to KeyValuePairs
        /// with Asset ID of the wearable as key and Item ID as value</param>
        public delegate void AgentWearablesCallback(Dictionary<WearableType, KeyValuePair<LLUUID, LLUUID>> wearables);


        /// <summary>
        /// 
        /// </summary>
        public event AgentWearablesCallback OnAgentWearables;

        /// <summary>Total number of wearables for each avatar</summary>
        public const int WEARABLE_COUNT = 13;

        /// <summary>Map of what wearables are included in each bake</summary>
        public static readonly WearableType[][] WEARABLE_BAKE_MAP = new WearableType[][]
        {
            // Head
            new WearableType[] { WearableType.Shape, WearableType.Skin,    WearableType.Hair,    WearableType.Invalid, WearableType.Invalid, WearableType.Invalid,    WearableType.Invalid },
            // Upper body
            new WearableType[] { WearableType.Shape, WearableType.Skin,    WearableType.Shirt,   WearableType.Jacket,  WearableType.Gloves,  WearableType.Undershirt, WearableType.Invalid },
            // Lower body
            new WearableType[] { WearableType.Shape, WearableType.Skin,    WearableType.Pants,   WearableType.Shoes,   WearableType.Socks,   WearableType.Jacket,     WearableType.Underpants },
            // Eyes
            new WearableType[] { WearableType.Eyes,  WearableType.Invalid, WearableType.Invalid, WearableType.Invalid, WearableType.Invalid, WearableType.Invalid,    WearableType.Invalid },
            // Skirt
            new WearableType[] { WearableType.Skin,  WearableType.Invalid, WearableType.Invalid, WearableType.Invalid, WearableType.Invalid, WearableType.Invalid,    WearableType.Invalid }
        };

        /// <summary></summary>
        public static readonly LLUUID[] BAKED_TEXTURE_HASH = new LLUUID[]
        {
            new LLUUID("18ded8d6-bcfc-e415-8539-944c0f5ea7a6"),
	        new LLUUID("338c29e3-3024-4dbb-998d-7c04cf4fa88f"),
	        new LLUUID("91b4a2c7-1b1a-ba16-9a16-1f8f8dcc1c3f"),
	        new LLUUID("b2cf28af-b840-1071-3c6a-78085d8128b5"),
	        new LLUUID("ea800387-ea1a-14e0-56cb-24f2022f969a")
        };

        /// <summary>Default avatar texture, used to detect when a custom
        /// texture is not set for a face</summary>
        public static readonly LLUUID DEFAULT_AVATAR = new LLUUID("c228d1cf-4b5d-4ba8-84f4-899a0796aa97");


        private SecondLife Client;
        private AssetManager Assets;
        private bool DownloadWearables = false;


        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="assets"></param>
        public AppearanceManager(SecondLife client, libsecondlife.Utilities.Assets.AssetManager assets)
        {
            Client = client;
            Assets = assets;

            Client.Network.RegisterCallback(PacketType.AgentWearablesUpdate, new NetworkManager.PacketCallback(AgentWearablesHandler));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool SetCurrentAppearance()
        {
            AssetManager.AssetReceivedCallback callback = new AssetManager.AssetReceivedCallback(Assets_OnAssetReceived);
            Assets.OnAssetReceived += callback;

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="autoDownload"></param>
        public void RequestAgentWearables(bool autoDownload)
        {
            DownloadWearables = autoDownload;

            AgentWearablesRequestPacket request = new AgentWearablesRequestPacket();
            request.AgentData.AgentID = Client.Network.AgentID;
            request.AgentData.SessionID = Client.Network.SessionID;

            Client.Network.SendPacket(request);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wearables">A mapping of WearableType to ItemIDs, must 
        /// have exactly WEARABLE_COUNT entries</param>
        public void SendAgentWearables(Dictionary<WearableType, LLUUID> wearables)
        {
            if (wearables.Count != WEARABLE_COUNT)
            {
                Client.Log("SendAgentWearables(): wearables must contain " + WEARABLE_COUNT + " IDs", 
                    Helpers.LogLevel.Warning);
                return;
            }

            AgentIsNowWearingPacket wearing = new AgentIsNowWearingPacket();

            wearing.AgentData.AgentID = Client.Network.AgentID;
            wearing.AgentData.SessionID = Client.Network.SessionID;
            wearing.WearableData = new AgentIsNowWearingPacket.WearableDataBlock[WEARABLE_COUNT];

            int i = 0;
            foreach (KeyValuePair<WearableType, LLUUID> pair in wearables)
            {
                wearing.WearableData[i] = new AgentIsNowWearingPacket.WearableDataBlock();
                wearing.WearableData[i].WearableType = (byte)pair.Key;
                wearing.WearableData[i].ItemID = pair.Value;

                i++;
            }

            Client.Network.SendPacket(wearing);
        }

        private void AgentWearablesHandler(Packet packet, Simulator simulator)
        {
            lock (OnAgentWearables)
            {
                if (OnAgentWearables != null)
                {
                    Dictionary<WearableType, KeyValuePair<LLUUID, LLUUID>> wearables = new Dictionary<WearableType, KeyValuePair<LLUUID, LLUUID>>();
                    AgentWearablesUpdatePacket update = (AgentWearablesUpdatePacket)packet;

                    foreach (AgentWearablesUpdatePacket.WearableDataBlock block in update.WearableData)
                    {
                        KeyValuePair<LLUUID, LLUUID> ids = new KeyValuePair<LLUUID, LLUUID>(block.AssetID, block.ItemID);
                        WearableType type = (WearableType)block.WearableType;
                        wearables[type] = ids;

                        //FIXME: Convert WearableType to AssetType
                        //if (DownloadWearables)
                        //    Assets.RequestAsset(block.AssetID, (WearableType)block.WearableType, true);
                    }

                    try { OnAgentWearables(wearables); }
                    catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                }
            }
        }

        void Assets_OnAssetReceived(AssetTransfer asset)
        {
            // Check if this is a wearable we were waiting on
        }
    }
}
