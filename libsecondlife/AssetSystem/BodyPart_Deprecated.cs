using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.AssetSystem
{
    class BodyPart_Deprecated
    {
        public string name = "";
        public uint type = 0;

        public string sale_type = "not";
        public uint sale_price = 10;

        public LLUUID creator_id = new LLUUID();
        public LLUUID owner_id = new LLUUID();
        public LLUUID last_owner_id = new LLUUID();
        public LLUUID group_id = new LLUUID();

        public uint base_mask = 0;
        public uint owner_mask = 0;
        public uint group_mask = 0;
        public uint everyone_mask = 0;
        public uint next_owner_mask = 0;

        public Dictionary<uint, float> parameters = new Dictionary<uint, float>();
        public Dictionary<uint, LLUUID> textures = new Dictionary<uint, LLUUID>();
        
        private static string intToHex(uint i)
        {
            return string.Format("{0:x8}", i);
        }

        public static byte[] BodyPartToByteData(BodyPart_Deprecated bp)
        {
            string data = "LLWearable version 22\n";
            data += bp.name + "\n\n";
            data += "\tpermissions 0\n\t{\n";
            data += "\t\tbase_mask\t" + intToHex(bp.base_mask);
            data += "\n\t\towner_mask\t" + intToHex(bp.owner_mask);
            data += "\n\t\tgroup_mask\t" + intToHex(bp.group_mask);
            data += "\n\t\teveryone_mask\t" + intToHex(bp.everyone_mask);
            data += "\n\t\tnext_owner_mask\t" + intToHex(bp.next_owner_mask);
            data += "\n\t\tcreator_id\t" + bp.creator_id.ToStringHyphenated();
            data += "\n\t\towner_id\t" + bp.owner_id.ToStringHyphenated();
            data += "\n\t\tlast_owner_id\t" + bp.last_owner_id.ToStringHyphenated();
            data += "\n\t\tgroup_id\t" + bp.group_id.ToStringHyphenated();
            data += "\n\t}";
            data += "\n\tsale_info\t0";
            data += "\n\t{";
            data += "\n\t\tsale_type\t" + bp.sale_type;
            data += "\n\t\tsale_price\t" + bp.sale_price;
            data += "\n\t}";
            data += "\ntype " + bp.type;
            data += "\nparameters " + bp.parameters.Count;
            foreach (KeyValuePair<uint, float> param in bp.parameters)
            {
                string prm = string.Format("{0:f1}", param.Value);
                if(prm == "-1.0" || prm == "1.0" || prm == "0.0")
                {
                    switch(prm)
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
            data += "\ntextures " + bp.textures.Count;
            foreach (KeyValuePair<uint, LLUUID> texture in bp.textures)
            {
                data += "\n" + texture.Key + " " + texture.Value.ToStringHyphenated();
            }

            return System.Text.Encoding.ASCII.GetBytes(data.ToCharArray());
        }
        /// <summary>
        /// Converts byte[] data from a data transfer into a bodypart class
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static BodyPart_Deprecated ByteDataToBodyPart(byte[] data)
        {
            BodyPart_Deprecated bp = new BodyPart_Deprecated(); 

            byte state = 0;
            const byte parameters_block = 4;
            const byte textures_block = 6;
            
            Exception Corrupted = new Exception("Corrupted Body Part data");
            string whole_enchilada = System.Text.Encoding.ASCII.GetString(data);
            
            //this seperates the whole enchilada into two, the header and the body.
            string[] seperated_enchilada = whole_enchilada.Split(new string[] { "permissions" }, StringSplitOptions.RemoveEmptyEntries);
            if (seperated_enchilada.Length != 2) throw Corrupted;

            //this parses out the name out of the header
            string[] header = seperated_enchilada[0].Split('\n');
            if (header.Length < 2) throw Corrupted;
            bp.name = header[1];

            seperated_enchilada[1] = "permissions" + seperated_enchilada[1];
            string[] body = seperated_enchilada[1].Split('\n');
            foreach(string blk in body)
            {
               string block = blk.Trim();
               if (block == "{" || block == "}") continue; //I hate those things..
               if (block == "") continue;
               //use the markers...
               if(block.StartsWith("parameters "))
               {
                   state = parameters_block; 
                   continue;
               }
               else if(block.StartsWith("textures "))
               {
                   state = textures_block;
                   continue;
               }
               
               if(state == 0)
               {
                   if(block.StartsWith("type "))
                   {
                       bp.type = uint.Parse(block.Substring(5));
                   }
                   else
                   {
                       string[] split_field = block.Split('\t');
                       
                       if(split_field.Length == 2)
                       {
                           switch(split_field[0])
                           {
                               case "base_mask":
                                   bp.base_mask = uint.Parse(split_field[1], System.Globalization.NumberStyles.HexNumber);
                                   break;
                               case "owner_mask":
                                   bp.owner_mask = uint.Parse(split_field[1], System.Globalization.NumberStyles.HexNumber);
                                   break;
                               case "group_mask":
                                   bp.group_mask = uint.Parse(split_field[1], System.Globalization.NumberStyles.HexNumber);
                                   break;
                               case "everyone_mask":
                                   bp.everyone_mask = uint.Parse(split_field[1], System.Globalization.NumberStyles.HexNumber);
                                   break;
                               case "next_owner_mask":
                                   bp.next_owner_mask = uint.Parse(split_field[1], System.Globalization.NumberStyles.HexNumber);
                                   break;
                               case "creator_id":
                                   bp.creator_id = new LLUUID(split_field[1]);
                                   break;
                               case "owner_id":
                                   bp.owner_id = new LLUUID(split_field[1]);
                                   break;
                               case "last_owner_id":
                                   bp.last_owner_id = new LLUUID(split_field[1]);
                                   break;
                               case "group_id":
                                   bp.group_id = new LLUUID(split_field[1]);
                                   break;
                               case "sale_type":
                                   bp.sale_type = split_field[1];
                                   break;
                               case "sale_price":
                                   bp.sale_price = uint.Parse(split_field[1]);
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
                       if (bp.parameters.ContainsKey(uint.Parse(split_up[0]))) bp.parameters.Remove(uint.Parse(split_up[0]));
                       bp.parameters.Add(uint.Parse(split_up[0]), float.Parse(split_up[1]));
                   }
               }
               else if (state == textures_block)
               {
                   string[] split_up = block.Split(' ');
                   if (split_up.Length != 2) throw Corrupted;

                   if (bp.parameters.ContainsKey(uint.Parse(split_up[0]))) bp.parameters.Remove(uint.Parse(split_up[0]));
                   bp.textures.Add(uint.Parse(split_up[0]), new LLUUID(split_up[1]));
               }
               
            }

            return bp;
        }
        public BodyPart_Deprecated() { } //blank construction

        public BodyPart_Deprecated(byte[] data)
        {
            BodyPart_Deprecated bp = BodyPart_Deprecated.ByteDataToBodyPart(data);
            this.base_mask = bp.base_mask;
            this.creator_id = bp.creator_id;
            this.everyone_mask = bp.everyone_mask;
            this.group_id = bp.group_id;
            this.group_mask = bp.group_mask;
            this.last_owner_id = bp.last_owner_id;
            this.name = bp.name;
            this.next_owner_mask = bp.next_owner_mask;
            this.owner_id = bp.owner_id;
            this.owner_mask = bp.owner_mask;
            this.parameters = bp.parameters;
            this.textures = bp.textures;
            this.type = bp.type;
            this.sale_price = bp.sale_price;
            this.sale_type = bp.sale_type;
        }
    }
}
