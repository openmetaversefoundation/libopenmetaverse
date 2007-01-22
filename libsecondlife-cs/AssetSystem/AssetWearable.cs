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
        private byte _TypeFromAsset = 0;
        public byte TypeFromAsset
        {
            get { return _TypeFromAsset; }
            set
            {
                _TypeFromAsset = value;
                UpdateAssetData();
            }
        }

        private string _Sale_Type = "not";
        public string Sale_Type
        {
            get { return _Sale_Type; }
            set
            {
                _Sale_Type = value;
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

        private Dictionary<uint, float> _Parameters = new Dictionary<uint, float>();
        public Dictionary<uint, float> Parameters
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

            byte state = 0;
            const byte parameters_block = 4;
            const byte textures_block = 6;

            Exception Corrupted = new Exception("Corrupted Body Part data");

            string whole_enchilada = System.Text.Encoding.ASCII.GetString(AssetData);

            //this seperates the whole enchilada into two, the header and the body.
            string[] seperated_enchilada = whole_enchilada.Split(new string[] { "permissions" }, StringSplitOptions.RemoveEmptyEntries);
            if (seperated_enchilada.Length != 2) throw Corrupted;

            //this parses out the name out of the header
            string[] header = seperated_enchilada[0].Split('\n');
            if (header.Length < 2) throw Corrupted;
            this._Name = header[1];

            seperated_enchilada[1] = "permissions" + seperated_enchilada[1];
            string[] body = seperated_enchilada[1].Split('\n');
            foreach (string blk in body)
            {
                string block = blk.Trim();
                if (block == "{" || block == "}") continue; //I hate those things..
                if (block == "") continue;
                //use the markers...
                if (block.StartsWith("parameters "))
                {
                    state = parameters_block;
                    continue;
                }
                else if (block.StartsWith("textures "))
                {
                    state = textures_block;
                    continue;
                }

                if (state == 0)
                {
                    if (block.StartsWith("type "))
                    {
                        this._TypeFromAsset = byte.Parse(block.Substring(5));
                    }
                    else
                    {
                        string[] split_field = block.Split('\t');

                        if (split_field.Length == 2)
                        {
                            switch (split_field[0])
                            {
                                case "base_mask":
                                    this._Permission_Base_Mask = uint.Parse(split_field[1], System.Globalization.NumberStyles.HexNumber);
                                    break;
                                case "owner_mask":
                                    this._Permission_Owner_Mask = uint.Parse(split_field[1], System.Globalization.NumberStyles.HexNumber);
                                    break;
                                case "group_mask":
                                    this._Permission_Group_Mask = uint.Parse(split_field[1], System.Globalization.NumberStyles.HexNumber);
                                    break;
                                case "everyone_mask":
                                    this._Permission_Everyone_Mask = uint.Parse(split_field[1], System.Globalization.NumberStyles.HexNumber);
                                    break;
                                case "next_owner_mask":
                                    this._Permission_Next_Owner_Mask = uint.Parse(split_field[1], System.Globalization.NumberStyles.HexNumber);
                                    break;
                                case "creator_id":
                                    this._Creator_ID = new LLUUID(split_field[1]);
                                    break;
                                case "owner_id":
                                    this._Owner_ID = new LLUUID(split_field[1]);
                                    break;
                                case "last_owner_id":
                                    this._Last_Owner_ID = new LLUUID(split_field[1]);
                                    break;
                                case "group_id":
                                    this._Group_ID = new LLUUID(split_field[1]);
                                    break;
                                case "sale_type":
                                    this._Sale_Type = split_field[1];
                                    break;
                                case "sale_price":
                                    this._Sale_Price = uint.Parse(split_field[1]);
                                    break;
                                default: break;
                            }
                        }
                    }
                }
                else if (state == parameters_block)
                {
                    string[] split_up = block.Split(' ');
                    // if (split_up.Length != 2) throw Corrupted;
                    if (split_up.Length == 2)
                    {
                        if (this._Parameters.ContainsKey(uint.Parse(split_up[0]))) this._Parameters.Remove(uint.Parse(split_up[0]));
                        this._Parameters.Add(uint.Parse(split_up[0]), float.Parse(split_up[1]));
                    }
                }
                else if (state == textures_block)
                {
                    string[] split_up = block.Split(' ');
                    if (split_up.Length != 2) throw Corrupted;

                    if (this._Parameters.ContainsKey(uint.Parse(split_up[0]))) this._Parameters.Remove(uint.Parse(split_up[0]));
                    this._Textures.Add(uint.Parse(split_up[0]), new LLUUID(split_up[1]));
                }

            }
        }

        private void UpdateAssetData()
        {
            string data = "LLWearable version 22\n";
            data += this._Name + "\n\n";
            data += "\tpermissions 0\n\t{\n";
            data += "\t\tbase_mask\t" + intToHex(this._Permission_Base_Mask);
            data += "\n\t\towner_mask\t" + intToHex(this._Permission_Owner_Mask);
            data += "\n\t\tgroup_mask\t" + intToHex(this._Permission_Group_Mask);
            data += "\n\t\teveryone_mask\t" + intToHex(this._Permission_Everyone_Mask);
            data += "\n\t\tnext_owner_mask\t" + intToHex(this._Permission_Next_Owner_Mask);
            data += "\n\t\tcreator_id\t" + this._Creator_ID.ToStringHyphenated();
            data += "\n\t\towner_id\t" + this._Owner_ID.ToStringHyphenated();
            data += "\n\t\tlast_owner_id\t" + this._Last_Owner_ID.ToStringHyphenated();
            data += "\n\t\tgroup_id\t" + this._Group_ID.ToStringHyphenated();
            data += "\n\t}";
            data += "\n\tsale_info\t0";
            data += "\n\t{";
            data += "\n\t\tsale_type\t" + this._Sale_Type;
            data += "\n\t\tsale_price\t" + this._Sale_Price;
            data += "\n\t}";
            data += "\ntype " + this._TypeFromAsset;
            data += "\nparameters " + this._Parameters.Count;
            foreach (KeyValuePair<uint, float> param in this._Parameters)
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

        private static string intToHex(uint i)
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
