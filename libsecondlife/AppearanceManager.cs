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
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using libsecondlife;
using libsecondlife.Packets;
using libsecondlife.InventorySystem;
using libsecondlife.Baking;

namespace libsecondlife
{
    /// <summary>
    /// 
    /// </summary>
    public class Wearable
    {
        /// <summary>
        /// 
        /// </summary>
        public enum WearableType : byte
        {
            /// <summary></summary>
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
        /// 
        /// </summary>
        public enum ForSale
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
        public Permissions Permissions;
        public Dictionary<int, float> Params = new Dictionary<int, float>();
        public Dictionary<AppearanceManager.TextureIndex, LLUUID> Textures = new Dictionary<AppearanceManager.TextureIndex, LLUUID>();


        private SecondLife Client;
        private string[] ForSaleNames = new string[]
        {
            "not",
            "orig",
            "copy",
            "cntn"
        };


        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="client">Reference to the SecondLife client</param>
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
            data.Append("\t\tsale_type\t"); data.Append(ForSaleNames[(int)Sale]); data.Append("\n");
            data.Append("\t\tsale_price\t"); data.Append(SalePrice); data.Append("\n");
            data.Append("\t}\n");
            data.Append("type "); data.Append((int)Type); data.Append("\n");

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

            return data.ToString();
        }

        /// <summary>
        /// Create a new Wearable from an AssetWearable
        /// </summary>
        /// <param name="client">SecondLife client</param>
        /// <param name="aw">AssetWearable to convert</param>
        /// <returns></returns>
        
        public static Wearable FromAssetWearable(SecondLife client, libsecondlife.AssetSystem.AssetWearable aw)
        {
            Wearable w = new Wearable(client);
            w.Creator = aw.Creator_ID;
            w.Description = aw.Description;
            w.Group = aw.Group_ID;
            w.GroupOwned = aw.Group_Owned;
            w.LastOwner = aw.Last_Owner_ID;
            w.Name = aw.Name;
            w.Owner = aw.Owner_ID;
            w.Params = new Dictionary<int, float>(aw.Parameters);
            w.SalePrice = (int)aw.Sale_Price;
            w.Textures = new Dictionary<AppearanceManager.TextureIndex, LLUUID>(aw.Textures.Count);

            foreach (KeyValuePair<uint, LLUUID> i in aw.Textures)
                w.Textures.Add((AppearanceManager.TextureIndex)i.Key, i.Value);

            w.Permissions.BaseMask = (PermissionMask)aw.Permission_Base_Mask;
            w.Permissions.EveryoneMask = (PermissionMask)aw.Permission_Everyone_Mask;
            w.Permissions.GroupMask = (PermissionMask)aw.Permission_Group_Mask;
            w.Permissions.NextOwnerMask = (PermissionMask)aw.Permission_Next_Owner_Mask;
            w.Permissions.OwnerMask = (PermissionMask)aw.Permission_Owner_Mask;

            w.Type = (Wearable.WearableType)aw.AppearanceLayer; // assumes these two enums are identical

            return w;
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

        public static WearableData FromInventoryWearable(SecondLife client, InventorySystem.InventoryWearable iw)
        {
            WearableData wd = new WearableData();
            wd.Wearable = Wearable.FromAssetWearable(client,(libsecondlife.AssetSystem.AssetWearable)iw.Asset);
            wd.AssetID = iw.AssetID;
            wd.ItemID = iw.ItemID;

            return wd;
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
            UpperGloves,
            UpperUndershirt,
            LowerUnderpants,
            Skirt,
            SkirtBaked
        }

        /// <summary>
        /// 
        /// </summary>
        public enum BakeType
        {
            Unknown = -1,
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
        // As wearable assets are downloaded and decoded, the textures are added to this array
        private LLUUID[] AgentTextures = new LLUUID[AVATAR_TEXTURE_COUNT];

        protected struct PendingAssetDownload
        {
            public LLUUID Id;
            public AssetType Type;

            public PendingAssetDownload(LLUUID id, AssetType type)
            {
                Id = id;
                Type = type;
            }
        }
        
        // Wearable assets are downloaded one at a time, a new request is pulled off the queue
        // and started when the previous one completes
        private Queue<PendingAssetDownload> DownloadQueue = new Queue<PendingAssetDownload>();
        // A list of all the images we are currently downloading, prior to baking
        private Dictionary<LLUUID, TextureIndex> ImageDownloads = new Dictionary<LLUUID, TextureIndex>();
        // A list of all the bakes we need to complete
        private Dictionary<BakeType, Baker> PendingBakes = new Dictionary<BakeType, Baker>(BAKED_TEXTURE_COUNT);
        // A list of all the uploads that are in progress
        private Dictionary<LLUUID, TextureIndex> PendingUploads = new Dictionary<LLUUID, TextureIndex>(BAKED_TEXTURE_COUNT);
        // Whether the handler for our current wearable list should automatically start downloading the assets
        private bool DownloadWearables = false;
        private static int CacheCheckSerialNum = 1; //FIXME
        private static uint SetAppearanceSerialNum = 1; //FIXME
        private AutoResetEvent WearablesDownloadedEvent = new AutoResetEvent(false);
        private AutoResetEvent CachedResponseEvent = new AutoResetEvent(false);
        // FIXME: Create a class-level appearance thread so multiple threads can't be launched

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="assets"></param>
        public AppearanceManager(SecondLife client, AssetManager assets)
        {
            Client = client;
            Assets = assets;

            // Initialize AgentTextures to zero UUIDs
            for (int i = 0; i < AgentTextures.Length; i++)
                AgentTextures[i] = LLUUID.Zero;

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
                return LLUUID.Zero;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void SetPreviousAppearance()
        {
            // Clear out any previous data
            DownloadWearables = false;
            lock (Wearables) Wearables.Clear();
            lock (AgentTextures)
            {
                for (int i = 0; i < AgentTextures.Length; i++)
                    AgentTextures[i] = LLUUID.Zero;
            }
            lock (DownloadQueue) DownloadQueue.Clear();

            Thread appearanceThread = new Thread(new ThreadStart(StartSetPreviousAppearance));
            appearanceThread.Start();
        }

        /// <summary>
        /// Add a single wearable to your outfit, replacing if nessesary.
        /// </summary>
        /// <param name="wearable"></param>
        public void Wear(libsecondlife.InventorySystem.InventoryWearable wearable)
        {
            List<libsecondlife.InventorySystem.InventoryWearable> x = new List<libsecondlife.InventorySystem.InventoryWearable>();
            x.Add(wearable);
            Wear(x);
        }

        public void Wear(List<libsecondlife.InventorySystem.InventoryWearable> iws)
        {
            DownloadWearables = false;
            
            lock (Wearables)
            {
                Dictionary<Wearable.WearableType,WearableData> preserve = new Dictionary<Wearable.WearableType,WearableData>(4);

                foreach (KeyValuePair<Wearable.WearableType, WearableData> kvp in Wearables)
                {
                    if (
                        kvp.Key == Wearable.WearableType.Shape ||
                        kvp.Key == Wearable.WearableType.Skin ||
                        kvp.Key == Wearable.WearableType.Eyes ||
                        kvp.Key == Wearable.WearableType.Hair
                        )
                    {
                        preserve.Add(kvp.Key,kvp.Value);
                        Client.DebugLog("Keeping " + kvp.Key.ToString() + " " + kvp.Value.Wearable.Name);
                    }
                }
                
                Wearables = preserve;

                foreach (libsecondlife.InventorySystem.InventoryWearable iw in iws)
                {
                    WearableData wd = WearableData.FromInventoryWearable(Client,iw);
                    Wearables[wd.Wearable.Type] = wd;
                    Client.DebugLog("Found " + iw.Name);
                }
            }

            lock (DownloadQueue) DownloadQueue.Clear();
            lock (ImageDownloads) ImageDownloads.Clear();
            lock (PendingBakes) PendingBakes.Clear();
            lock (PendingUploads) PendingUploads.Clear();

            lock (AgentTextures)
            {
                for (int i = 0; i < AgentTextures.Length; i++)
                    AgentTextures[i] = LLUUID.Zero;
            }

            Thread appearanceThread = new Thread(new ThreadStart(StartWear));
            appearanceThread.Start();
        }

        public void WearOutfit(InventoryFolder folder)
        {
            Thread wearOutfitThread = new Thread(new ParameterizedThreadStart(WearOutfitAsync));
            wearOutfitThread.Start(folder);
        }

        public void WearOutfit(string folder)
        {
            Thread wearOutfitThread = new Thread(new ParameterizedThreadStart(WearOutfitAsync));
            wearOutfitThread.Start(folder);
        }

        public void WearOutfitAsync(object _folder)
        {
            InventoryFolder folder;

            if (_folder is string)
                folder = Client.Inventory.getFolder((string)_folder);
            else
                folder = (InventoryFolder)_folder;
            
            List<InventoryWearable> iws = new List<InventoryWearable>();
            folder.RequestDownloadContents(false, false, true).RequestComplete.WaitOne();

            foreach (InventoryBase ib in folder.GetContents())
            {
                if (ib is InventoryWearable)
                    iws.Add((InventoryWearable)ib);
            }

            Wear(iws);
        }

        /// <summary>
        /// Build hashes out of the texture assetIDs for each baking layer to
        /// ask the simulator whether it has cached copies of each baked texture
        /// </summary>
        public void RequestCachedBakes()
        {
            Client.DebugLog("RequestCachedBakes()");
            
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

        private void StartWear()
        {
            Client.DebugLog("StartWear()");
            
            DownloadWearables = true;

            // Register an asset download callback to get wearable data
            AssetManager.AssetReceivedCallback assetCallback = new AssetManager.AssetReceivedCallback(Assets_OnAssetReceived);
            AssetManager.ImageReceivedCallback imageCallback = new AssetManager.ImageReceivedCallback(Assets_OnImageReceived);
            AssetManager.AssetUploadedCallback uploadCallback = new AssetManager.AssetUploadedCallback(Assets_OnAssetUploaded);
            Assets.OnAssetReceived += assetCallback;
            Assets.OnImageReceived += imageCallback;
            Assets.OnAssetUploaded += uploadCallback;

            // Download assets for what we are wearing and fill in AgentTextures
            DownloadWearableAssets();
            WearablesDownloadedEvent.WaitOne();

            // Unregister the asset download callback
            Assets.OnAssetReceived -= assetCallback;

            string tex = "";

            for (int i = 0; i < AgentTextures.Length; i++)
                if (AgentTextures[i] != LLUUID.Zero)
                    tex += ((TextureIndex)i).ToString() + " = " + AgentTextures[i] + "\n";

            Client.DebugLog("AgentTextures:\n" + tex);

            // Check if anything needs to be rebaked
            RequestCachedBakes();

            // Tell the sim what we are wearing
            SendAgentIsNowWearing();

            // Wait for cached layer check to finish
            CachedResponseEvent.WaitOne();

            // Unregister the image download and asset upload callbacks
            Assets.OnImageReceived -= imageCallback;
            Assets.OnAssetUploaded -= uploadCallback;

            Client.DebugLog("CachedResponseEvent completed");

            // Send all of the visual params and textures for our agent
            SendAgentSetAppearance();
        }
        
        private void StartSetPreviousAppearance()
        {
            DownloadWearables = true;

            // Register an asset download callback to get wearable data
            AssetManager.AssetReceivedCallback assetCallback = new AssetManager.AssetReceivedCallback(Assets_OnAssetReceived);
            AssetManager.ImageReceivedCallback imageCallback = new AssetManager.ImageReceivedCallback(Assets_OnImageReceived);
            AssetManager.AssetUploadedCallback uploadCallback = new AssetManager.AssetUploadedCallback(Assets_OnAssetUploaded);
            Assets.OnAssetReceived += assetCallback;
            Assets.OnImageReceived += imageCallback;
            Assets.OnAssetUploaded += uploadCallback;

            // Ask the server what we are currently wearing
            RequestAgentWearables();

            WearablesDownloadedEvent.WaitOne();

            // Unregister the asset download callback
            Assets.OnAssetReceived -= assetCallback;

            Client.DebugLog("WearablesDownloadEvent completed");

            // Now that we know what the avatar is wearing, we can check if anything needs to be rebaked
            RequestCachedBakes();

            CachedResponseEvent.WaitOne();

            // Send a list of what we are currently wearing
            SendAgentIsNowWearing();

            // Unregister the image download and asset upload callbacks
            Assets.OnImageReceived -= imageCallback;
            Assets.OnAssetUploaded -= uploadCallback;

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
            set.VisualParam = new AgentSetAppearancePacket.VisualParamBlock[VisualParams.Params.Count];

            lock (Wearables)
            {
                // Only for debugging output
                int count = 0, vpIndex = 0;

                // Build the visual param array
                foreach (KeyValuePair<int,VisualParam> kvp in VisualParams.Params)
                {
                    bool found = false;
                    set.VisualParam[vpIndex] = new AgentSetAppearancePacket.VisualParamBlock();
                    VisualParam vp = kvp.Value;

                    // Try and find this value in our collection of downloaded wearables
                    foreach (WearableData data in Wearables.Values)
                    {
                        if (data.Wearable.Params.ContainsKey(vp.ParamID))
                        {
                            set.VisualParam[vpIndex].ParamValue = Helpers.FloatToByte(data.Wearable.Params[vp.ParamID], 
                                vp.MinValue, vp.MaxValue);
                            found = true;
                            count++;
                            break;
                        }
                    }

                    // Use a default value if we don't have one set for it
                    if (!found)
                    {
                        set.VisualParam[vpIndex].ParamValue = Helpers.FloatToByte(vp.DefaultValue,
                            vp.MinValue, vp.MaxValue);
                    }

                    vpIndex++;
                }

                Client.DebugLog("AgentSetAppearance contains " + count + " VisualParams");

                // Build the texture entry for our agent
                LLObject.TextureEntry te = new LLObject.TextureEntry(DEFAULT_AVATAR_TEXTURE);

                // Put our AgentTextures array in to TextureEntry
                lock (AgentTextures)
                {
                    for (uint i = 0; i < AgentTextures.Length; i++)
                    {
                        if (AgentTextures[i] != LLUUID.Zero)
                        {
                            LLObject.TextureEntryFace face = te.CreateFace(i);
                            face.TextureID = AgentTextures[i];
                        }
                    }
                }

                foreach (WearableData data in Wearables.Values)
                {
                    foreach (KeyValuePair<TextureIndex, LLUUID> texture in data.Wearable.Textures)
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

            // FIXME: Our hackish algorithm is making squished avatars. See
            // http://www.libsecondlife.org/wiki/Agent_Size for discussion of the correct algorithm
            float height = Helpers.ByteToFloat(set.VisualParam[33].ParamValue, VisualParams.Params[33].MinValue,
                VisualParams.Params[33].MaxValue);
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
                    if (assetID != LLUUID.Zero) hash ^= assetID;
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

        private void SendAgentIsNowWearing()
        {
            Client.DebugLog("SendAgentIsNowWearing()");

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

        private TextureIndex BakeTypeToAgentTextureIndex(BakeType index)
        {
            switch (index)
            {
                case BakeType.Head:
                    return TextureIndex.HeadBaked;
                case BakeType.UpperBody:
                    return TextureIndex.UpperBaked;
                case BakeType.LowerBody:
                    return TextureIndex.LowerBaked;
                case BakeType.Eyes:
                    return TextureIndex.EyesBaked;
                case BakeType.Skirt:
                    return TextureIndex.SkirtBaked;
                default:
                    return TextureIndex.Unknown;
            }
        }

        private void DownloadWearableAssets()
        {
            Client.DebugLog("DownloadWearableAssets()");
            
            foreach (KeyValuePair<Wearable.WearableType,WearableData> kvp in Wearables)
                DownloadQueue.Enqueue(new PendingAssetDownload(kvp.Value.AssetID,Wearable.WearableTypeToAssetType(kvp.Value.Wearable.Type)));

            if (DownloadQueue.Count > 0)
            {
                PendingAssetDownload download = DownloadQueue.Dequeue();
                Assets.RequestAsset(download.Id, download.Type, true);
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
                            PendingAssetDownload download = new PendingAssetDownload(data.AssetID, assetType);
                            DownloadQueue.Enqueue(download);
                        }
                    }
                }

                if (DownloadQueue.Count > 0)
                {
                    PendingAssetDownload download = DownloadQueue.Dequeue();
                    Assets.RequestAsset(download.Id, download.Type, true);
                }

                // Don't download wearables twice in a row
                DownloadWearables = false;
            }

            CallOnAgentWearables();
        }

        private void CallOnAgentWearables()
        {
            if (OnAgentWearables != null)
            {
                // Refactor our internal Wearables dictionary in to something for the callback
                Dictionary<Wearable.WearableType, KeyValuePair<LLUUID, LLUUID>> wearables =
                    new Dictionary<Wearable.WearableType, KeyValuePair<LLUUID, LLUUID>>();

                lock (Wearables)
                {
                    foreach (KeyValuePair<Wearable.WearableType, WearableData> data in Wearables)
                        wearables.Add(data.Key, new KeyValuePair<LLUUID, LLUUID>(data.Value.AssetID, data.Value.ItemID));
                }

                try { OnAgentWearables(wearables); }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            }
        }

        private void AgentCachedTextureResponseHandler(Packet packet, Simulator simulator)
        {
            Client.DebugLog("AgentCachedTextureResponseHandler()");
            
            AgentCachedTextureResponsePacket response = (AgentCachedTextureResponsePacket)packet;
            Dictionary<int, float> paramValues = new Dictionary<int, float>(VisualParams.Params.Count);

            // Build a dictionary of appearance parameter indices and values from the wearables
            foreach (KeyValuePair<int,VisualParam> kvp in VisualParams.Params)
            {
                bool found = false;
                VisualParam vp = kvp.Value;

                // Try and find this value in our collection of downloaded wearables
                foreach (WearableData data in Wearables.Values)
                {
                    if (data.Wearable.Params.ContainsKey(vp.ParamID))
                    {
                        paramValues.Add(vp.ParamID,data.Wearable.Params[vp.ParamID]);
                        found = true;
                        break;
                    }
                }

                // Use a default value if we don't have one set for it
                if (!found) paramValues.Add(vp.ParamID, vp.DefaultValue);
            }

            lock (AgentTextures)
            {
                foreach (AgentCachedTextureResponsePacket.WearableDataBlock block in response.WearableData)
                {
                    // For each missing element we need to bake our own texture
                    Client.DebugLog("Cache response, index: " + block.TextureIndex + ", ID: " +
                        block.TextureID.ToStringHyphenated());

                    // FIXME: Use this. Right now we treat baked images on other sims as if they were missing
                    string host = Helpers.FieldToUTF8String(block.HostName);
                    if (host.Length > 0) Client.DebugLog("Cached bake exists on foreign host " + host);

                    BakeType bakeType = (BakeType)block.TextureIndex;
                    
                    // Convert the baked index to an AgentTexture index
                    if (block.TextureID != LLUUID.Zero && host.Length == 0)
                    {
                        TextureIndex index = BakeTypeToAgentTextureIndex(bakeType);
                        AgentTextures[(int)index] = block.TextureID;
                    }
                    else
                    {
                        int imageCount = 0;

                        // Download all of the images in this layer
                        switch (bakeType)
                        {
                            case BakeType.Head:
                                lock (ImageDownloads)
                                {
                                    imageCount += AddImageDownload(TextureIndex.HeadBodypaint);
                                    //imageCount += AddImageDownload(TextureIndex.Hair);
                                }
                                break;
                            case BakeType.UpperBody:
                                lock (ImageDownloads)
                                {
                                    imageCount += AddImageDownload(TextureIndex.UpperBodypaint);
                                    imageCount += AddImageDownload(TextureIndex.UpperGloves);
                                    imageCount += AddImageDownload(TextureIndex.UpperUndershirt);
                                    imageCount += AddImageDownload(TextureIndex.UpperShirt);
                                    imageCount += AddImageDownload(TextureIndex.UpperJacket);
                                }
                                break;
                            case BakeType.LowerBody:
                                lock (ImageDownloads)
                                {
                                    imageCount += AddImageDownload(TextureIndex.LowerBodypaint);
                                    imageCount += AddImageDownload(TextureIndex.LowerUnderpants);
                                    imageCount += AddImageDownload(TextureIndex.LowerSocks);
                                    imageCount += AddImageDownload(TextureIndex.LowerShoes);
                                    imageCount += AddImageDownload(TextureIndex.LowerPants);
                                    imageCount += AddImageDownload(TextureIndex.LowerJacket);
                                }
                                break;
                            case BakeType.Eyes:
                                lock (ImageDownloads)
                                {
                                    imageCount += AddImageDownload(TextureIndex.EyesIris);
                                }
                                break;
                            case BakeType.Skirt:
                                if (Wearables.ContainsKey(Wearable.WearableType.Skirt))
                                {
                                    lock (ImageDownloads)
                                    {
                                        imageCount += AddImageDownload(TextureIndex.Skirt);
                                    }
                                }
                                break;
                            default:
                                Client.Log("Unknown BakeType " + block.TextureIndex, Helpers.LogLevel.Warning);
                                break;
                        }

                        if (!PendingBakes.ContainsKey(bakeType))
                        {
                            Client.DebugLog("Initializing " + bakeType.ToString() + " bake with " + imageCount + " textures");

                            if (imageCount == 0)
                            {
                                // if there are no textures to download, we can bake right away and start the upload
                                Baker bake = new Baker(Client, bakeType, 0, paramValues);
                                UploadBake(bake);
                            }
                            else
                            {
                                lock (PendingBakes)
                                    PendingBakes.Add(bakeType, new Baker(Client, bakeType, imageCount, paramValues));
                            }
                        }
                        else if (!PendingBakes.ContainsKey(bakeType))
                        {
                            Client.Log("No cached bake for " + bakeType.ToString() + " and no textures for that " +
                                "layer, this is an unhandled case", Helpers.LogLevel.Error);
                        }
                    }
                }
            }

            if (ImageDownloads.Count == 0)
            {
                // No pending downloads for baking, we're done
                CachedResponseEvent.Set();
            }
            else
            {
                lock (ImageDownloads)
                {
                    foreach (LLUUID image in ImageDownloads.Keys)
                    {
                        // Download all the images we need for baking
                        Assets.RequestImage(image, ImageType.Normal, 1013000.0f, 0);
                    }
                }
            }
        }

        private int AddImageDownload(TextureIndex index)
        {
            LLUUID image = AgentTextures[(int)index];

            if (image != LLUUID.Zero)
            {
                if (!ImageDownloads.ContainsKey(image))
                {
                    Client.DebugLog("Downloading layer " + index.ToString());
                    ImageDownloads.Add(image, index);
                }

                return 1;
            }

            return 0;
        }

        private void Assets_OnAssetReceived(AssetDownload asset)
        {
            Client.DebugLog("Assets_OnAssetReceived()");
            
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
                                Client.DebugLog("Imported wearable asset " + data.Wearable.Type.ToString());
                                Client.DebugLog(wearableData);

                                lock (AgentTextures)
                                {
                                    foreach (KeyValuePair<AppearanceManager.TextureIndex, LLUUID> texture in data.Wearable.Textures)
                                    {
                                        Client.DebugLog("Setting " + texture.Key + " to " + texture.Value);
                                        AgentTextures[(int)texture.Key] = texture.Value;
                                    }
                                }
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
                PendingAssetDownload download = DownloadQueue.Dequeue();
                Assets.RequestAsset(download.Id, download.Type, true);
            }
            else
            {
                // Everything is downloaded
                WearablesDownloadedEvent.Set();
            }
        }

        private void Assets_OnImageReceived(ImageDownload image)
        {
            lock (ImageDownloads)
            {
                if (ImageDownloads.ContainsKey(image.ID))
                {
                    // NOTE: this image may occupy more than one TextureIndex! We must finish this loop
                    for (int at = 0; at < AgentTextures.Length; at++)
                    {
                        if (AgentTextures[at] == image.ID)
                        {
                            TextureIndex index = (TextureIndex)at;
                            Client.DebugLog("Finished downloading texture for " + index.ToString());
                            BakeType type = Baker.BakeTypeFor(index);
                            
                            //BinaryWriter writer = new BinaryWriter(File.Create("wearable_" + index.ToString() + "_" + image.ID.ToString() + ".jp2"));
                            //writer.Write(image.AssetData);
                            //writer.Close();

                            bool baked = false;

                            if (PendingBakes.ContainsKey(type))
                            {
                                if (image.Success)
                                    baked = PendingBakes[type].AddTexture(index, image.AssetData);
                                else
                                {
                                    Client.Log("Texture for " + index.ToString() + " failed to download, " +
                                        "bake will be incomplete", Helpers.LogLevel.Warning);

                                    baked = PendingBakes[type].MissingTexture(index);
                                }
                            }

                            if (baked)
                            {
                                UploadBake(PendingBakes[type]);
                                PendingBakes.Remove(type);
                            }

                            ImageDownloads.Remove(image.ID);

                            if (ImageDownloads.Count == 0 && PendingUploads.Count == 0)
                            {
                                // This is a failsafe catch, as the upload completed callback should normally 
                                // be triggering the event
                                Client.DebugLog("No pending downloads or uploads detected in OnImageReceived");
                                CachedResponseEvent.Set();
                            }
                            else
                            {
                                Client.DebugLog("Pending uploads: " + PendingUploads.Count + ", pending downloads: " +
                                    ImageDownloads.Count);
                            }

                        }
                    }
                }
                else
                    Client.Log("Received an image download callback for an image we did not request " + image.ID.ToStringHyphenated(), Helpers.LogLevel.Warning);
            }
        }

        private void UploadBake(Baker bake)
        {
            // Create a transactionID and assetID for this upload
            LLUUID transactionID = LLUUID.Random();
            LLUUID assetID = transactionID.Combine(Client.Network.SecureSessionID);

            Client.DebugLog("Bake " + bake.BakeType.ToString() + " completed. Uploading asset " + assetID.ToStringHyphenated());

            // Upload the completed layer data
            Assets.RequestUpload(transactionID, AssetType.Texture, bake.EncodedBake, true, true, false);

            // Add it to a pending uploads list
            lock (PendingUploads) PendingUploads.Add(assetID, BakeTypeToAgentTextureIndex(bake.BakeType));
        }

        private void Assets_OnAssetUploaded(AssetUpload upload)
        {
            lock (PendingUploads)
            {
                if (PendingUploads.ContainsKey(upload.AssetID))
                {
                    if (upload.Success)
                    {
                        // Setup the TextureEntry with the new baked upload
                        TextureIndex index = PendingUploads[upload.AssetID];
                        AgentTextures[(int)index] = upload.AssetID;

                        Client.DebugLog("Upload complete, AgentTextures " + index.ToString() + " set to " + 
                            upload.AssetID.ToStringHyphenated());
                    }
                    else
                    {
                        Client.Log("Asset upload " + upload.AssetID.ToStringHyphenated() + " failed", 
                            Helpers.LogLevel.Warning);
                    }

                    PendingUploads.Remove(upload.AssetID);

                    Client.DebugLog("Pending uploads: " + PendingUploads.Count + ", pending downloads: " +
                        ImageDownloads.Count);

                    if (PendingUploads.Count == 0 && ImageDownloads.Count == 0)
                    {
                        Client.DebugLog("All pending image downloads and uploads complete");

                        CachedResponseEvent.Set();
                    }
                }
                else
                {
                    // TEMP
                    Client.DebugLog("Upload " + upload.AssetID.ToStringHyphenated() + " was not found in PendingUploads");
                }
            }
        }
    }
}
