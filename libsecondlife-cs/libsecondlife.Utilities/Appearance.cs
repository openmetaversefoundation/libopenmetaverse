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
    public class Wearable
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

        public enum ForSale
        {
            Not = 0,
            Original = 1,
            Copy = 2,
            Contents = 3
        }


        public string Name = String.Empty;
        public string Description = String.Empty;
        public WearableType Type = WearableType.Shape;
        public ForSale Sale = ForSale.Not;
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

        public static AssetType WearableTypeToAssetType(WearableType type)
        {
            switch (type)
            {
                case WearableType.Shape:
                case WearableType.Skin:
                case WearableType.Hair:
                case WearableType.Eyes:
                    return AssetType.Bodypart;
                case WearableType.Shirt:
                case WearableType.Pants:
                case WearableType.Shoes:
                case WearableType.Socks:
                case WearableType.Jacket:
                case WearableType.Gloves:
                case WearableType.Undershirt:
                case WearableType.Underpants:
                    return AssetType.Clothing;
                default:
                    return AssetType.Unknown;
            }
        }

        public bool ImportAsset(string data)
        {
            int version = -1;
            int n = -1;

            try
            {
                n = data.IndexOf('\n');
                version = Int32.Parse(data.Substring(19, n - 18));
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
                                    Sale = (ForSale)i;
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
            data += "\t\tsale_type\t" + ForSaleNames[(int)Sale] + "\n";
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
    public struct WearableData
    {
        public Wearable Wearable;
        public LLUUID AssetID;
        public LLUUID ItemID;
    }


    /// <summary>
    /// 
    /// </summary>
    public class AppearanceManager
    {
        /// <summary>
        /// 
        /// </summary>
        public enum TextureIndex
        {
            Unknown = -1,
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

        public enum BakeType
        {
            Head = 0,
            UpperBody = 1,
            LowerBody = 2,
            Eyes = 3,
            Skirt = 4
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="wearables">A mapping of WearableTypes to KeyValuePairs
        /// with Asset ID of the wearable as key and Item ID as value</param>
        public delegate void AgentWearablesCallback(Dictionary<Wearable.WearableType, KeyValuePair<LLUUID, LLUUID>> wearables);


        /// <summary></summary>
        public event AgentWearablesCallback OnAgentWearables;

        /// <summary>Total number of wearables for each avatar</summary>
        public const int WEARABLE_COUNT = 13;
        /// <summary></summary>
        public const int BAKED_TEXTURE_COUNT = 5;
        /// <summary></summary>
        public const int WEARABLES_PER_LAYER = 7;
        /// <summary></summary>
        public const int AVATAR_TEXTURE_COUNT = 20;
        /// <summary>Map of what wearables are included in each bake</summary>
        public static readonly Wearable.WearableType[][] WEARABLE_BAKE_MAP = new Wearable.WearableType[][]
        {
            new Wearable.WearableType[] { Wearable.WearableType.Shape, Wearable.WearableType.Skin,    Wearable.WearableType.Hair,    Wearable.WearableType.Invalid, Wearable.WearableType.Invalid, Wearable.WearableType.Invalid,    Wearable.WearableType.Invalid    },
            new Wearable.WearableType[] { Wearable.WearableType.Shape, Wearable.WearableType.Skin,    Wearable.WearableType.Shirt,   Wearable.WearableType.Jacket,  Wearable.WearableType.Gloves,  Wearable.WearableType.Undershirt, Wearable.WearableType.Invalid    },
            new Wearable.WearableType[] { Wearable.WearableType.Shape, Wearable.WearableType.Skin,    Wearable.WearableType.Pants,   Wearable.WearableType.Shoes,   Wearable.WearableType.Socks,   Wearable.WearableType.Jacket,     Wearable.WearableType.Underpants },
            new Wearable.WearableType[] { Wearable.WearableType.Eyes,  Wearable.WearableType.Invalid, Wearable.WearableType.Invalid, Wearable.WearableType.Invalid, Wearable.WearableType.Invalid, Wearable.WearableType.Invalid,    Wearable.WearableType.Invalid    },
            new Wearable.WearableType[] { Wearable.WearableType.Skin,  Wearable.WearableType.Invalid, Wearable.WearableType.Invalid, Wearable.WearableType.Invalid, Wearable.WearableType.Invalid, Wearable.WearableType.Invalid,    Wearable.WearableType.Invalid    }
        };
        /// <summary>Secret values to finalize the cache check hashes for each
        /// bake</summary>
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
        public static readonly LLUUID DEFAULT_AVATAR_TEXTURE = new LLUUID("c228d1cf-4b5d-4ba8-84f4-899a0796aa97");


        private SecondLife Client;
        private AssetManager Assets;
        private Dictionary<Wearable.WearableType, WearableData> Wearables = new Dictionary<Wearable.WearableType, WearableData>();
        // As wearable assets are downloaded and decoded, the textures are added to this list
        private Dictionary<int, LLUUID> AgentTextures = new Dictionary<int, LLUUID>();
        // Wearable assets are downloaded one at a time, a new request is pulled off the queue
        // and started when the previous one completes
        private Queue<KeyValuePair<LLUUID, AssetType>> DownloadQueue = new Queue<KeyValuePair<LLUUID, AssetType>>();
        // Whether the handler for our current wearable list should automatically start downloading the assets
        private bool DownloadWearables = false;
        private int CacheCheckSerialNum = 0;
        private uint SetAppearanceSerialNum = 0;
        private ManualResetEvent WearablesDownloadedEvent = new ManualResetEvent(false);
        private ManualResetEvent CachedResponseEvent = new ManualResetEvent(false);
        // FIXME: Create a class-level appearance thread so multiple threads can't be launched

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
            Client.Network.RegisterCallback(PacketType.AgentCachedTextureResponse, new NetworkManager.PacketCallback(AgentCachedTextureResponseHandler));
        }

        /// <summary>
        /// If the appearance thread is running it is terminated here
        /// </summary>
        ~AppearanceManager()
        {
            WearablesDownloadedEvent.Set();
            CachedResponseEvent.Set();
        }

        /// <summary>
        /// Returns the assetID for a given WearableType 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public LLUUID GetWearableAsset(Wearable.WearableType type)
        {
            if (Wearables.ContainsKey(type))
                return Wearables[type].AssetID;
            else
                return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void SetPreviousAppearance()
        {
            // Clear out any previous data
            // TODO: Figure out why deformed avatars are produced the second time around if this
            // isn't done
            DownloadWearables = false;
            lock (Wearables) Wearables.Clear();
            lock (AgentTextures) AgentTextures.Clear();
            lock (DownloadQueue) DownloadQueue.Clear();

            Thread appearanceThread = new Thread(new ThreadStart(StartSetPreviousAppearance));
            appearanceThread.Start();
        }

        /// <summary>
        /// Build hashes out of the texture assetIDs for each baking layer to
        /// ask the simulator whether it has cached copies of each baked layer
        /// </summary>
        public void RequestCachedBakes()
        {
            List<KeyValuePair<int, LLUUID>> hashes = new List<KeyValuePair<int,LLUUID>>();

            AgentCachedTexturePacket cache = new AgentCachedTexturePacket();
            cache.AgentData.AgentID = Client.Network.AgentID;
            cache.AgentData.SessionID = Client.Network.SessionID;
            cache.AgentData.SerialNum = CacheCheckSerialNum;

            // Build hashes for each of the bake layers from the individual components
            for (int bakedIndex = 0; bakedIndex < BAKED_TEXTURE_COUNT; bakedIndex++)
            {
                // Don't do a cache request for a skirt bake if we're not wearing a skirt
                if (bakedIndex == (int)BakeType.Skirt && 
                    (!Wearables.ContainsKey(Wearable.WearableType.Skirt) || Wearables[Wearable.WearableType.Skirt].AssetID == LLUUID.Zero))
                    continue;

                LLUUID hash = new LLUUID();

                for (int wearableIndex = 0; wearableIndex < WEARABLES_PER_LAYER; wearableIndex++)
                {
                    Wearable.WearableType type = WEARABLE_BAKE_MAP[bakedIndex][wearableIndex];
                    LLUUID assetID = GetWearableAsset(type);

                    // Build a hash of all the texture asset IDs in this baking layer
                    if (assetID != null) hash ^= assetID;
                }

                if (hash != LLUUID.Zero)
                {
                    // Hash with our secret value for this baked layer
                    hash ^= BAKED_TEXTURE_HASH[bakedIndex];

                    // Add this to the list of hashes to send out
                    hashes.Add(new KeyValuePair<int, LLUUID>(bakedIndex, hash));
                }
            }

            // Only send the packet out if there's something to check
            if (hashes.Count > 0)
            {
                cache.WearableData = new AgentCachedTexturePacket.WearableDataBlock[hashes.Count];

                for (int i = 0; i < hashes.Count; i++)
                {
                    cache.WearableData[i] = new AgentCachedTexturePacket.WearableDataBlock();
                    cache.WearableData[i].TextureIndex = (byte)hashes[i].Key;
                    cache.WearableData[i].ID = hashes[i].Value;

                    Client.DebugLog("Checking cache for index " + cache.WearableData[i].TextureIndex +
                        ", ID: " + cache.WearableData[i].ID);
                }

                // Increment our serial number for this packet
                CacheCheckSerialNum++;

                // Send it out
                Client.Network.SendPacket(cache);
            }
        }

        /// <summary>
        /// Ask the server what textures our avatar is currently wearing
        /// </summary>
        public void RequestAgentWearables()
        {
            AgentWearablesRequestPacket request = new AgentWearablesRequestPacket();
            request.AgentData.AgentID = Client.Network.AgentID;
            request.AgentData.SessionID = Client.Network.SessionID;

            Client.Network.SendPacket(request);
        }

        private void StartSetPreviousAppearance()
        {
            WearablesDownloadedEvent.Reset();
            CachedResponseEvent.Reset();
            DownloadWearables = true;

            // Register an asset download callback to get wearable data
            AssetManager.AssetReceivedCallback callback = new AssetManager.AssetReceivedCallback(Assets_OnAssetReceived);
            Assets.OnAssetReceived += callback;

            // Ask the server what we are currently wearing
            RequestAgentWearables();

            WearablesDownloadedEvent.WaitOne();

            // Unregister the asset download callback
            Assets.OnAssetReceived -= callback;

            Client.DebugLog("WearablesDownloadEvent completed");

            // Now that we know what the avatar is wearing, we can check if anything needs to be rebaked
            RequestCachedBakes();

            // Send a list of what we are currently wearing
            SendAgentWearables();

            CachedResponseEvent.WaitOne();

            Client.DebugLog("CachedResponseEvent completed");

            // Send all of the visual params and textures for our agent
            SendAgentSetAppearance();
        }

        private void SendAgentSetAppearance()
        {
            AgentSetAppearancePacket set = new AgentSetAppearancePacket();
            set.AgentData.AgentID = Client.Network.AgentID;
            set.AgentData.SessionID = Client.Network.SessionID;
            set.AgentData.SerialNum = SetAppearanceSerialNum++;
            set.VisualParam = new AgentSetAppearancePacket.VisualParamBlock[VisualParams.Params.Length];

            lock (Wearables)
            {
                // Only for debugging output
                int count = 0;

                // Build the visual param array
                for (int i = 0; i < VisualParams.Params.Length; i++)
                {
                    bool found = false;
                    set.VisualParam[i] = new AgentSetAppearancePacket.VisualParamBlock();

                    // Try and find this value in our collection of downloaded wearables
                    foreach (WearableData data in Wearables.Values)
                    {
                        if (data.Wearable.Params.ContainsKey(i))
                        {
                            set.VisualParam[i].ParamValue = Helpers.FloatToByte(data.Wearable.Params[i], 
                                VisualParams.Params[i].MinValue, VisualParams.Params[i].MaxValue);
                            found = true;
                            count++;
                            break;
                        }
                    }

                    // Use a default value if we don't have one set for it
                    if (!found)
                    {
                        set.VisualParam[i].ParamValue = Helpers.FloatToByte(VisualParams.Params[i].DefaultValue,
                            VisualParams.Params[i].MinValue, VisualParams.Params[i].MaxValue);
                    }
                }

                Client.DebugLog("Sending " + count + " VisualParams");

                // Build the texture entry for our agent
                LLObject.TextureEntry te = new LLObject.TextureEntry(DEFAULT_AVATAR_TEXTURE);

                // Put our AgentTextures dictionary in to TextureEntry
                lock (AgentTextures)
                {
                    foreach (KeyValuePair<int, LLUUID> texture in AgentTextures)
                    {
                        LLObject.TextureEntryFace face = te.CreateFace((uint)texture.Key);
                        face.TextureID = texture.Value;
                    }
                }

                foreach (WearableData data in Wearables.Values)
                {
                    foreach (KeyValuePair<int, LLUUID> texture in data.Wearable.Textures)
                    {
                        LLObject.TextureEntryFace face = te.CreateFace((uint)texture.Key);
                        face.TextureID = texture.Value;

                        Client.DebugLog("Setting texture " + ((TextureIndex)texture.Key).ToString() + " to " +
                            texture.Value.ToStringHyphenated());
                    }
                }

                // Set the packet TextureEntry
                set.ObjectData.TextureEntry = te.ToBytes();
            }

            // This is a bit hackish, but a whole lot better than implementing the actual algorithm
            float height = Helpers.ByteToFloat(set.VisualParam[25].ParamValue, VisualParams.Params[25].MinValue,
                VisualParams.Params[25].MaxValue);
            set.AgentData.Size = new LLVector3(0.45f, 0.6f, 1.50856f + ((height / 255.0f) * (2.025506f - 1.50856f)));

            // TODO: Account for not having all the textures baked yet
            set.WearableData = new AgentSetAppearancePacket.WearableDataBlock[BAKED_TEXTURE_COUNT];

            // Build hashes for each of the bake layers from the individual components
            for (int bakedIndex = 0; bakedIndex < BAKED_TEXTURE_COUNT; bakedIndex++)
            {
                LLUUID hash = new LLUUID();

                for (int wearableIndex = 0; wearableIndex < WEARABLES_PER_LAYER; wearableIndex++)
                {
                    Wearable.WearableType type = WEARABLE_BAKE_MAP[bakedIndex][wearableIndex];
                    LLUUID assetID = GetWearableAsset(type);

                    // Build a hash of all the texture asset IDs in this baking layer
                    if (assetID != null) hash ^= assetID;
                }

                if (hash != LLUUID.Zero)
                {
                    // Hash with our secret value for this baked layer
                    hash ^= BAKED_TEXTURE_HASH[bakedIndex];
                }

                // Tell the server what cached texture assetID to use for each bake layer
                set.WearableData[bakedIndex] = new AgentSetAppearancePacket.WearableDataBlock();
                set.WearableData[bakedIndex].TextureIndex = (byte)bakedIndex;
                set.WearableData[bakedIndex].CacheID = hash;
            }

            // Finally, send the packet
            Client.Network.SendPacket(set);
        }

        private void SendAgentWearables()
        {
            Client.DebugLog("Wearables contains " + Wearables.Count + " entries");

            AgentIsNowWearingPacket wearing = new AgentIsNowWearingPacket();
            wearing.AgentData.AgentID = Client.Network.AgentID;
            wearing.AgentData.SessionID = Client.Network.SessionID;
            wearing.WearableData = new AgentIsNowWearingPacket.WearableDataBlock[WEARABLE_COUNT];

            for (int i = 0; i < WEARABLE_COUNT; i++)
            {
                Wearable.WearableType type = (Wearable.WearableType)i;
                wearing.WearableData[i] = new AgentIsNowWearingPacket.WearableDataBlock();
                wearing.WearableData[i].WearableType = (byte)i;

                if (Wearables.ContainsKey(type))
                    wearing.WearableData[i].ItemID = Wearables[type].ItemID;
                else
                    wearing.WearableData[i].ItemID = LLUUID.Zero;
            }

            Client.Network.SendPacket(wearing);
        }

        private int BakedIndexToAgentTextureIndex(int index)
        {
            switch ((BakeType)index)
            {
                case BakeType.Head:
                    return 8;
                case BakeType.UpperBody:
                    return 9;
                case BakeType.LowerBody:
                    return 10;
                case BakeType.Eyes:
                    return 11;
                case BakeType.Skirt:
                    return 19;
                default:
                    return (int)TextureIndex.Unknown;
            }
        }

        private void AgentWearablesHandler(Packet packet, Simulator simulator)
        {
            // Lock to prevent a race condition with multiple AgentWearables packets
            lock (WearablesDownloadedEvent)
            {
                AgentWearablesUpdatePacket update = (AgentWearablesUpdatePacket)packet;

                // Reset the Wearables collection
                lock (Wearables) Wearables.Clear();

                for (int i = 0; i < update.WearableData.Length; i++)
                {
                    if (update.WearableData[i].AssetID != LLUUID.Zero)
                    {
                        Wearable.WearableType type = (Wearable.WearableType)update.WearableData[i].WearableType;
                        WearableData data = new WearableData();
                        data.AssetID = update.WearableData[i].AssetID;
                        data.ItemID = update.WearableData[i].ItemID;
                        data.Wearable = new Wearable(Client);
                        data.Wearable.Type = type;

                        // Add this wearable to our collection
                        lock (Wearables) Wearables[type] = data;

                        // Convert WearableType to AssetType
                        AssetType assetType = Wearable.WearableTypeToAssetType(type);

                        Client.DebugLog("Downloading wearable " + type.ToString() + ": " +
                            data.AssetID.ToStringHyphenated());

                        // Add this wearable asset to the download queue
                        if (DownloadWearables)
                        {
                            KeyValuePair<LLUUID, AssetType> download = 
                                new KeyValuePair<LLUUID,AssetType>(data.AssetID, assetType);
                            DownloadQueue.Enqueue(download);
                        }
                    }
                }

                if (DownloadQueue.Count > 0)
                {
                    KeyValuePair<LLUUID, AssetType> download = DownloadQueue.Dequeue();
                    Assets.RequestAsset(download.Key, download.Value, true);
                }

                // Don't download wearables twice in a row
                DownloadWearables = false;
            }

            if (OnAgentWearables != null)
            {
                // Refactor our internal Wearables dictionary in to something for the callback
                Dictionary<Wearable.WearableType, KeyValuePair<LLUUID, LLUUID>> wearables =
                    new Dictionary<Wearable.WearableType, KeyValuePair<LLUUID, LLUUID>>();

                lock (Wearables)
                {
                    foreach (KeyValuePair<Wearable.WearableType, WearableData> data in Wearables)
                        wearables.Add(data.Key, new KeyValuePair<LLUUID,LLUUID>(data.Value.AssetID, data.Value.ItemID));
                }

                try { OnAgentWearables(wearables); }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            }
        }

        private void AgentCachedTextureResponseHandler(Packet packet, Simulator simulator)
        {
            AgentCachedTextureResponsePacket response = (AgentCachedTextureResponsePacket)packet;

            lock (AgentTextures)
            {
                foreach (AgentCachedTextureResponsePacket.WearableDataBlock block in response.WearableData)
                {
                    // For each missing element we need to bake our own texture
                    Client.DebugLog("Cache response, index: " + block.TextureIndex + ", ID: " +
                        block.TextureID.ToStringHyphenated());

                    // FIXME: Use this
                    Helpers.FieldToUTF8String(block.HostName);

                    // Convert the baked index to an AgentTexture index
                    if (block.TextureID != LLUUID.Zero)
                    {
                        int index = BakedIndexToAgentTextureIndex(block.TextureIndex);
                        AgentTextures[index] = block.TextureID;
                    }
                    else
                    {
                        // FIXME: We need to download all of the images for this layer and bake them
                    }
                }
            }

            CachedResponseEvent.Set();
        }

        private void Assets_OnAssetReceived(AssetDownload asset)
        {
            lock (Wearables)
            {
                // Check if this is a wearable we were waiting on
                foreach (WearableData data in Wearables.Values)
                {
                    if (data.AssetID == asset.AssetID)
                    {
                        // Make sure the download succeeded
                        if (asset.Success)
                        {
                            // Convert the downloaded asset to a string
                            string wearableData = Helpers.FieldToUTF8String(asset.AssetData);

                            // Attempt to parse the wearable data
                            if (data.Wearable.ImportAsset(wearableData))
                            {
                                lock (AgentTextures)
                                {
                                    foreach (KeyValuePair<int, LLUUID> texture in data.Wearable.Textures)
                                        AgentTextures[texture.Key] = texture.Value;
                                }

                                Client.DebugLog("Imported wearable asset " + data.Wearable.Type.ToString());
                            }
                            else
                            {
                                Client.Log("Failed to decode wearable asset " + asset.AssetID.ToStringHyphenated(),
                                    Helpers.LogLevel.Warning);
                            }
                        }
                        else
                        {
                            Client.Log("Wearable " + data.Wearable.Type.ToString() + "(" +
                                asset.AssetID.ToStringHyphenated() + ") failed to download, " + asset.Status.ToString(),
                                Helpers.LogLevel.Warning);
                        }

                        break;
                    }
                }
            }

            if (DownloadQueue.Count > 0)
            {
                // Dowload the next wearable in line
                KeyValuePair<LLUUID, AssetType> download = DownloadQueue.Dequeue();
                Assets.RequestAsset(download.Key, download.Value, true);
            }
            else
            {
                // Everything is downloaded
                WearablesDownloadedEvent.Set();
            }
        }
    }
}
