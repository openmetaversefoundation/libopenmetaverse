/*
 * Copyright (c) 2006, Second Life Reverse Engineering Team
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
using System.Globalization;

using libsecondlife;

namespace libsecondlife.AssetSystem
{
	/// <summary>
    /// Asset for wearables such as Socks, Eyes, Gloves, Hair, Pants, Shape, Shirt, Shoes, Skin, Jacket, Skirt, Underpants
	/// </summary>
	public class AssetWearable : Asset
	{
        private string _Name = "";
        public string Name
        {
            get { return _Name; }
            set
            {
                _Name = value;
                UpdateAssetData();
            }
        }
        private string _Description = "";
        public string Description
        {
            get { return _Description; }
            set
            {
                _Description = value;
                UpdateAssetData();
            }
        }

        private AppearanceLayerType _AppearanceLayer = 0;
        public AppearanceLayerType AppearanceLayer
        {
            get { return _AppearanceLayer; }
            set
            {
                _AppearanceLayer = value;
                UpdateAssetData();
            }
        }

        private uint _Sale_Price = 10;
        public uint Sale_Price
        {
            get { return _Sale_Price; }
            set
            {
                _Sale_Price = value;
                UpdateAssetData();
            }
        }

        private LLUUID _Creator_ID = new LLUUID();
        public LLUUID Creator_ID
        {
            get { return _Creator_ID; }
            set
            {
                _Creator_ID = value;
                UpdateAssetData();
            }
        }
        private LLUUID _Owner_ID = new LLUUID();
        public LLUUID Owner_ID
        {
            get { return _Owner_ID; }
            set
            {
                _Owner_ID = value;
                UpdateAssetData();
            }
        }
        private LLUUID _Last_Owner_ID = new LLUUID();
        public LLUUID Last_Owner_ID
        {
            get { return _Last_Owner_ID; }
            set
            {
                _Last_Owner_ID = value;
                UpdateAssetData();
            }
        }

        private LLUUID _Group_ID = new LLUUID();
        public LLUUID Group_ID
        {
            get { return _Group_ID; }
            set
            {
                _Group_ID = value;
                UpdateAssetData();
            }
        }

        private bool _Group_Owned = false;
        public bool Group_Owned
        {
            get { return _Group_Owned; }
            set
            {
                _Group_Owned = value;
                UpdateAssetData();
            }
        }


        private uint _Permission_Base_Mask = 0;
        public uint Permission_Base_Mask
        {
            get { return _Permission_Base_Mask; }
            set
            {
                _Permission_Base_Mask = value;
                UpdateAssetData();
            }
        }

        private uint _Permission_Owner_Mask = 0;
        public uint Permission_Owner_Mask
        {
            get { return _Permission_Owner_Mask; }
            set
            {
                _Permission_Owner_Mask = value;
                UpdateAssetData();
            }
        }

        private uint _Permission_Group_Mask = 0;
        public uint Permission_Group_Mask
        {
            get { return _Permission_Group_Mask; }
            set
            {
                _Permission_Group_Mask = value;
                UpdateAssetData();
            }
        }

        private uint _Permission_Everyone_Mask = 0;
        public uint Permission_Everyone_Mask
        {
            get { return _Permission_Everyone_Mask; }
            set
            {
                _Permission_Everyone_Mask = value;
                UpdateAssetData();
            }
        }

        private uint _Permission_Next_Owner_Mask = 0;
        public uint Permission_Next_Owner_Mask
        {
            get { return _Permission_Next_Owner_Mask; }
            set
            {
                _Permission_Next_Owner_Mask = value;
                UpdateAssetData();
            }
        }

        private Dictionary<int, float> _Parameters = new Dictionary<int, float>();
        public Dictionary<int, float> Parameters
        {
            get { return _Parameters; }
            set
            {
                _Parameters = value;
                UpdateAssetData();
            }
        }

        private Dictionary<uint, LLUUID> _Textures = new Dictionary<uint, LLUUID>();
        public Dictionary<uint, LLUUID> Textures
        {
            get { return _Textures; }
            set
            {
                _Textures = value;
                UpdateAssetData();
            }
        }

        private string[] _ForSaleNames = new string[]
        {
            "not",
            "orig",
            "copy",
            "cntn"
        };

        private enum _ForSale
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

        private _ForSale _Sale = _ForSale.Not;

        public enum AppearanceLayerType : byte
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
        /// </summary>
        /// <param name="assetID"></param>
        /// <param name="assetData"></param>
        public AssetWearable(LLUUID assetID, sbyte assetType, byte[] assetData)
            : base(assetID, assetType, false, assetData)
		{
            UpdateFromAssetData();
		}

        /// <summary>
        /// Converts byte[] data from a data transfer into a bodypart class
        /// </summary>
        /// <returns></returns>
        internal void UpdateFromAssetData()
        {
            if ( AssetData == null || AssetData.Length == 0)
            {
                return;
            }

            string wearableData = Helpers.FieldToUTF8String(this._AssetData);

            int version = -1;
            int n = -1;

            try
            {
                n = wearableData.IndexOf('\n');
                version = Int32.Parse(wearableData.Substring(19, n - 18));
                wearableData = wearableData.Remove(0, n);

                if (version != 22)
                {
                    Console.WriteLine("** WARNING ** : Wearable asset has unrecognized version " + version);
                    return;
                }

                n = wearableData.IndexOf('\n');
                Name = wearableData.Substring(0, n);
                wearableData = wearableData.Remove(0, n);

                n = wearableData.IndexOf('\n');
                Description = wearableData.Substring(0, n);
                wearableData = wearableData.Remove(0, n);

                // Split in to an upper and lower half
                string[] parts = wearableData.Split(new string[] { "parameters" }, StringSplitOptions.None);
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
                            _Permission_Base_Mask = UInt32.Parse(fields[1], System.Globalization.NumberStyles.HexNumber);
                        }
                        else if (fields[0] == "base_mask")
                        {
                            _Permission_Base_Mask = UInt32.Parse(fields[1], System.Globalization.NumberStyles.HexNumber);
                        }
                        else if (fields[0] == "owner_mask")
                        {
                            _Permission_Owner_Mask = UInt32.Parse(fields[1], System.Globalization.NumberStyles.HexNumber);
                        }
                        else if (fields[0] == "group_mask")
                        {
                            _Permission_Group_Mask = UInt32.Parse(fields[1], System.Globalization.NumberStyles.HexNumber);
                        }
                        else if (fields[0] == "everyone_mask")
                        {
                            _Permission_Everyone_Mask = UInt32.Parse(fields[1], System.Globalization.NumberStyles.HexNumber);
                        }
                        else if (fields[0] == "next_owner_mask")
                        {
                            _Permission_Next_Owner_Mask = UInt32.Parse(fields[1], System.Globalization.NumberStyles.HexNumber);
                        }
                        else if (fields[0] == "creator_id")
                        {
                            _Creator_ID = new LLUUID(fields[1]);
                        }
                        else if (fields[0] == "owner_id")
                        {
                            _Owner_ID = new LLUUID(fields[1]);
                        }
                        else if (fields[0] == "last_owner_id")
                        {
                            _Last_Owner_ID = new LLUUID(fields[1]);
                        }
                        else if (fields[0] == "group_id")
                        {
                            _Group_ID = new LLUUID(fields[1]);
                        }
                        else if (fields[0] == "group_owned")
                        {
                            
                            _Group_Owned = (Int32.Parse(fields[1]) != 0);
                        }
                        else if (fields[0] == "sale_type")
                        {
                            for (int i = 0; i < _ForSaleNames.Length; i++)
                            {
                                if (fields[1] == _ForSaleNames[i])
                                {
                                    _Sale = (_ForSale)i;
                                    break;
                                }
                            }
                        }
                        else if (fields[0] == "sale_price")
                        {
                            _Sale_Price = UInt32.Parse(fields[1]);
                        }
                        else if (fields[0] == "perm_mask")
                        {
                            Console.WriteLine("** WARNING ** : Wearable asset has deprecated perm_mask field, ignoring");
                        }
                    }
                    else if (line.StartsWith("type "))
                    {
                        AppearanceLayer = (AppearanceLayerType)Int32.Parse(line.Substring(5));
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

                    int id;
                    if( Int32.TryParse(fields[0], out id) == false )
                    {
                        continue; // Not interested in this line
                    }

                    float weight = 0.0f;
                    Single.TryParse(fields[1], System.Globalization.NumberStyles.Float,
                        Helpers.EnUsCulture.NumberFormat, out weight);
                    _Parameters[id] = weight;
                }

                // Parse the textures
                lines = lowerparts[1].Split('\n');
                foreach (string line in lines)
                {
                    string[] fields = line.Split(' ');

                    uint id;
                    if (UInt32.TryParse(fields[0], out id) == false)
                    {
                        continue; // Not interested in this line
                    }

                    LLUUID texture;
                    
                    if( LLUUID.TryParse(fields[1], out texture) == false )
                    {
                        continue; // if it won't parse, ignore and continue
                    }

                    _Textures[id] = texture;
                }

                return;
            }
            catch (Exception e)
            {
                Console.WriteLine("** WARNING **", "Failed to parse wearable asset: " + e.ToString());
            }

            return;
        }

        private void UpdateAssetData()
        {
            string data = "LLWearable version 22\n";
            data += this._Name + "\n\n";
            data += "\tpermissions 0\n\t{\n";
            data += "\t\tbase_mask\t" + uintToHex(this._Permission_Base_Mask);
            data += "\n\t\towner_mask\t" + uintToHex(this._Permission_Owner_Mask);
            data += "\n\t\tgroup_mask\t" + uintToHex(this._Permission_Group_Mask);
            data += "\n\t\teveryone_mask\t" + uintToHex(this._Permission_Everyone_Mask);
            data += "\n\t\tnext_owner_mask\t" + uintToHex(this._Permission_Next_Owner_Mask);
            data += "\n\t\tcreator_id\t" + this._Creator_ID.ToStringHyphenated();
            data += "\n\t\towner_id\t" + this._Owner_ID.ToStringHyphenated();
            data += "\n\t\tlast_owner_id\t" + this._Last_Owner_ID.ToStringHyphenated();
            data += "\n\t\tgroup_id\t" + this._Group_ID.ToStringHyphenated();
            data += "\n\t}";
            data += "\n\tsale_info\t0";
            data += "\n\t{";
            data += "\n\t\tsale_type\t" + _ForSaleNames[(int)this._Sale];
            data += "\n\t\tsale_price\t" + this._Sale_Price;
            data += "\n\t}";
            data += "\ntype " + this._AppearanceLayer;
            data += "\nparameters " + this._Parameters.Count;
            foreach (KeyValuePair<int, float> param in this._Parameters)
            {
                string prm = string.Format("{0:f1}", param.Value);
                if (prm == "-1.0" || prm == "1.0" || prm == "0.0")
                {
                    switch (prm)
                    {
                        case "-1.0":
                            prm = "-1";
                            break;
                        case "0.0":
                            prm = "0";
                            break;
                        case "1.0":
                            prm = "1";
                            break;
                    }
                }
                data += "\n" + param.Key + " " + prm;
            }
            data += "\ntextures " + this._Textures.Count;
            foreach (KeyValuePair<uint, LLUUID> texture in this._Textures)
            {
                data += "\n" + texture.Key + " " + texture.Value.ToStringHyphenated();
            }

            _AssetData = System.Text.Encoding.ASCII.GetBytes(data.ToCharArray());
        }

        private static string uintToHex(uint i)
        {
            return string.Format("{0:x8}", i);
        }

        public override void SetAssetData(byte[] data)
        {
            _AssetData = data;
            if ( (_AssetData != null) && (_AssetData.Length > 0) )
            {
                UpdateFromAssetData();
            }
        }
	}
}
