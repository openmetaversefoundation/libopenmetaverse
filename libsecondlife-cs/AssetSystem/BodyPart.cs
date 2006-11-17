using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.AssetSystem
{
    public class BodyPart
    {
        public string name = "";
        public uint type = 0;

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



        public static BodyPart ByteDataToBodyPart(byte[] data)
        {
            BodyPart bp = new BodyPart(); 

            byte state = 0;
            const byte parameters_block = 4;
            const byte textures_block = 6;
            
            Exception Corrupted = new Exception("Corrupted Body Part data");
            string whole_enchilada = System.Text.Encoding.ASCII.GetString(data);
            
            //this seperates the whole enchilada into two, the header and the body.
            string[] seperated_enchilada = whole_enchilada.Split(new string[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (seperated_enchilada.Length != 2) throw Corrupted;

            //this parses out the name out of the header
            string[] header = seperated_enchilada[0].Split('\n');
            if (header.Length < 2) throw Corrupted;
            bp.name = header[1];

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
                               default: break;
                           }
                       }
                   }
               }
               else if (state == parameters_block)
               {
                   string[] split_up = block.Split(' ');
                   if (split_up.Length != 2) throw Corrupted;
                   
                   if (bp.parameters.ContainsKey(uint.Parse(split_up[0]))) bp.parameters.Remove(uint.Parse(split_up[0]));
                   bp.parameters.Add(uint.Parse(split_up[0]), float.Parse(split_up[1]));
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

        public BodyPart()
        {
            //empty BodyPart constructor
        }
    }
}
