using System;
using System.Collections.Generic;
using System.Threading;

using libsecondlife;
using libsecondlife.AssetSystem;
using libsecondlife.Packets;



namespace libsecondlife.AssetSystem
{
    public class AppearanceManager
    {
        private SecondLife Client;
        private AssetManager AManager;

        private uint SerialNum = 1;

        private ManualResetEvent AgentWearablesSignal = null;
        private AgentWearablesUpdatePacket.WearableDataBlock[] AgentWearablesData;

        public AppearanceManager(SecondLife client)
        {
            Client = client;
            AManager = AssetManager.GetAssetManager(client);

            RegisterCallbacks();

        }

        private void RegisterCallbacks()
        {
            Client.Network.RegisterCallback(libsecondlife.Packets.PacketType.AgentWearablesUpdate, new NetworkManager.PacketCallback(AgentWearablesUpdateCallbackHandler));
        }

        public AgentWearablesUpdatePacket.WearableDataBlock[] GetWearables()
        {
            AgentWearablesSignal = new ManualResetEvent(false);

            AgentWearablesRequestPacket p = new AgentWearablesRequestPacket();
            p.AgentData.AgentID = Client.Network.AgentID;
            p.AgentData.SessionID = Client.Network.SessionID;
            Client.Network.SendPacket(p);

            AgentWearablesSignal.WaitOne();

            return AgentWearablesData;
        }

        public void SendAgentSetAppearance()
        {

            TextureEntry textures = new TextureEntry("C228D1CF4B5D4BA884F4899A0796AA97"); // if this isn't valid, blame JH ;-)

            // Face 17 - Under Pants
            textures.CreateFace(17).TextureID = "b0bac26505cc7076202ba2a2e05fd172"; //Default Men's briefs

            // Face 16 - Under Shirt
            textures.CreateFace(16).TextureID = "d283de852dc3dc07dc452e1bfd4cf193"; //Default Men's tank

            // Face 11 - Eyes
            textures.CreateFace(11).TextureID = "3abd329a78478984ac1cb95f4ef35fbe"; //Default Eye

            // Face 10 - 
            textures.CreateFace(10).TextureID = "c24403bc4569361852b31917ad733035"; //Default 

            // Face 9 - 
            textures.CreateFace(9).TextureID = "a88225377555cf975720aa128e47f934"; //Default 

            // Face 8 - 
            textures.CreateFace(8).TextureID = "472ccc472a4e2556d082f7bb708bcca7"; //Default 



            Dictionary<uint, float> AgentAppearance = new Dictionary<uint, float>();

            foreach (AgentWearablesUpdatePacket.WearableDataBlock wdb in GetWearables())
            {
                if (wdb.ItemID == LLUUID.Zero)
                {
                    continue;
                }

                sbyte Type = 13;

                switch (wdb.WearableType)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                        Type = 13;
                        break;
                    default:
                        Type = 5;
                        break;
                }

                Asset asset = new Asset(wdb.AssetID, Type, null);

                AManager.GetInventoryAsset(asset);
                if (asset.AssetData.Length == 0)
                {
                    Console.WriteLine("Retrieval failed");
                }

                try
                {
                    BodyPart bp = new BodyPart(asset.AssetData);

                    foreach (KeyValuePair<uint, LLUUID> texture in bp.textures)
                    {
                        Console.WriteLine(texture.Key + " : " + texture.Value);
                        textures.CreateFace(texture.Key).TextureID = texture.Value;
                    }

                    foreach (KeyValuePair<uint, float> kvp in bp.parameters)
                    {
                        AgentAppearance[kvp.Key] = bp.parameters[kvp.Key];
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("ItemID: " + wdb.ItemID);
                    Console.WriteLine("WearableType : " + wdb.WearableType);
                    Console.WriteLine("Retrieving as type: " + asset.Type);

                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                    Console.WriteLine(asset.AssetDataToString());
                }
            }

            
            Dictionary<uint, byte> VisualParams = new Dictionary<uint, byte>();


            float maxVal = 0;
            float minVal = 0;
            uint packetIdx = 0;
            float range = 0;
            float percentage = 0;
            byte packetVal = 0;

            foreach (KeyValuePair<uint, float> kvp in AgentAppearance)
            {
                packetIdx = AppearanceManager.GetAgentSetAppearanceIndex(kvp.Key) - 1; //TODO/FIXME: this should be zero indexed, not 1 based.
                maxVal = BodyShapeParams.GetValueMax(kvp.Key);
                minVal = BodyShapeParams.GetValueMin(kvp.Key);

                range = maxVal - minVal;

                percentage = (kvp.Value - minVal) / range;

                packetVal = (byte)(percentage * (byte)255);

                VisualParams[packetIdx] = packetVal;
            }


            AgentSetAppearancePacket p = new AgentSetAppearancePacket();
            p.AgentData.AgentID = Client.Network.AgentID;
            p.AgentData.SessionID = Client.Network.SessionID;
            p.AgentData.SerialNum = ++SerialNum;
            p.AgentData.Size = new LLVector3(0.45f, 0.6f, 1.35187f);
            p.ObjectData.TextureEntry = textures.ToBytes();

            p.VisualParam = new AgentSetAppearancePacket.VisualParamBlock[218];
            for (uint i = 0; i < 218; i++)
            {
                p.VisualParam[i] = new AgentSetAppearancePacket.VisualParamBlock();

                if (VisualParams.ContainsKey(i))
                {
                    p.VisualParam[i].ParamValue = VisualParams[i];
                }
                else
                {
                    uint paramid = GetParamID(i + 1);
                    float defaultValue = BodyShapeParams.GetValueDefault(paramid);

                    maxVal = BodyShapeParams.GetValueMax(paramid);
                    minVal = BodyShapeParams.GetValueMin(paramid);

                    range = maxVal - minVal;

                    percentage = (defaultValue - minVal) / range;

                    packetVal = (byte)(percentage * (byte)255);

                    // Console.WriteLine("Warning Visual Param not defined, IDX: " + (i+1));
                    // Console.WriteLine("PID: " + paramid + " / Default: " + defaultValue);
                    p.VisualParam[i].ParamValue = packetVal;
                }
            }

            Client.Network.SendPacket(p);

            Console.WriteLine(p);
        }



        private void AgentWearablesUpdateCallbackHandler(Packet packet, Simulator simulator)
        {
            AgentWearablesUpdatePacket wearablesPacket = (AgentWearablesUpdatePacket)packet;

            AgentWearablesData = wearablesPacket.WearableData;
            AgentWearablesSignal.Set();
        }

        public static uint GetAgentSetAppearanceIndex(uint AgentSetAppearanceIdx)
        {
            switch (AgentSetAppearanceIdx)
            {
                case 1: return 1;
                case 2: return 2;
                case 4: return 3;
                case 5: return 4;
                case 6: return 5;
                case 7: return 6;
                case 8: return 7;
                case 10: return 8;
                case 11: return 9;
                case 12: return 10;
                case 13: return 11;
                case 14: return 12;
                case 15: return 13;
                case 16: return 14;
                case 17: return 15;
                case 18: return 16;
                case 19: return 17;
                case 20: return 18;
                case 21: return 19;
                case 22: return 20;
                case 23: return 21;
                case 24: return 22;
                case 25: return 23;
                case 27: return 24;
                case 31: return 25;
                case 33: return 26;
                case 34: return 27;
                case 35: return 28;
                case 36: return 29;
                case 37: return 30;
                case 38: return 31;
                case 80: return 32;
                case 93: return 33;
                case 98: return 34;
                case 99: return 35;
                case 105: return 36;
                case 108: return 37;
                case 110: return 38;
                case 111: return 39;
                case 112: return 40;
                case 113: return 41;
                case 114: return 42;
                case 115: return 43;
                case 116: return 44;
                case 117: return 45;
                case 119: return 46;
                case 130: return 47;
                case 131: return 48;
                case 132: return 49;
                case 133: return 50;
                case 134: return 51;
                case 135: return 52;
                case 136: return 53;
                case 137: return 54;
                case 140: return 55;
                case 141: return 56;
                case 142: return 57;
                case 143: return 58;
                case 150: return 59;
                case 155: return 60;
                case 157: return 61;
                case 162: return 62;
                case 163: return 63;
                case 165: return 64;
                case 166: return 65;
                case 167: return 66;
                case 168: return 67;
                case 169: return 68;
                case 177: return 69;
                case 181: return 70;
                case 182: return 71;
                case 183: return 72;
                case 184: return 73;
                case 185: return 74;
                case 192: return 75;
                case 193: return 76;
                case 196: return 77;
                case 198: return 78;
                case 503: return 79;
                case 505: return 80;
                case 506: return 81;
                case 507: return 82;
                case 508: return 83;
                case 513: return 84;
                case 514: return 85;
                case 515: return 86;
                case 517: return 87;
                case 518: return 88;
                case 603: return 89;
                case 604: return 90;
                case 605: return 91;
                case 606: return 92;
                case 607: return 93;
                case 608: return 94;
                case 609: return 95;
                case 616: return 96;
                case 617: return 97;
                case 619: return 98;
                case 624: return 99;
                case 625: return 100;
                case 629: return 101;
                case 637: return 102;
                case 638: return 103;
                case 646: return 104;
                case 647: return 105;
                case 649: return 106;
                case 650: return 107;
                case 652: return 108;
                case 653: return 109;
                case 654: return 110;
                case 656: return 111;
                case 659: return 112;
                case 662: return 113;
                case 663: return 114;
                case 664: return 115;
                case 665: return 116;
                case 674: return 117;
                case 675: return 118;
                case 676: return 119;
                case 678: return 120;
                case 682: return 121;
                case 683: return 122;
                case 684: return 123;
                case 685: return 124;
                case 690: return 125;
                case 692: return 126;
                case 693: return 127;
                case 700: return 128;
                case 701: return 129;
                case 702: return 130;
                case 703: return 131;
                case 704: return 132;
                case 705: return 133;
                case 706: return 134;
                case 707: return 135;
                case 708: return 136;
                case 709: return 137;
                case 710: return 138;
                case 711: return 139;
                case 712: return 140;
                case 713: return 141;
                case 714: return 142;
                case 715: return 143;
                case 750: return 144;
                case 752: return 145;
                case 753: return 146;
                case 754: return 147;
                case 755: return 148;
                case 756: return 149;
                case 757: return 150;
                case 758: return 151;
                case 759: return 152;
                case 760: return 153;
                case 762: return 154;
                case 763: return 155;
                case 764: return 156;
                case 765: return 157;
                case 769: return 158;
                case 773: return 159;
                case 775: return 160;
                case 779: return 161;
                case 780: return 162;
                case 781: return 163;
                case 785: return 164;
                case 789: return 165;
                case 795: return 166;
                case 796: return 167;
                case 799: return 168;
                case 800: return 169;
                case 801: return 170;
                case 802: return 171;
                case 803: return 172;
                case 804: return 173;
                case 805: return 174;
                case 806: return 175;
                case 807: return 176;
                case 808: return 177;
                case 812: return 178;
                case 813: return 179;
                case 814: return 180;
                case 815: return 181;
                case 816: return 182;
                case 817: return 183;
                case 818: return 184;
                case 819: return 185;
                case 820: return 186;
                case 821: return 187;
                case 822: return 188;
                case 823: return 189;
                case 824: return 190;
                case 825: return 191;
                case 826: return 192;
                case 827: return 193;
                case 828: return 194;
                case 829: return 195;
                case 830: return 196;
                case 834: return 197;
                case 835: return 198;
                case 836: return 199;
                case 840: return 200;
                case 841: return 201;
                case 842: return 202;
                case 844: return 203;
                case 848: return 204;
                case 858: return 205;
                case 859: return 206;
                case 860: return 207;
                case 861: return 208;
                case 862: return 209;
                case 863: return 210;
                case 868: return 211;
                case 869: return 212;
                case 877: return 213;
                case 879: return 214;
                case 880: return 215;
                case 921: return 216;
                case 922: return 217;
                case 923: return 218;

                default:
                    throw new Exception("We don't have a mapping for AgentSetAppearanceIdx " + AgentSetAppearanceIdx + " yet...");
            }
        }

        public static uint GetParamID(uint ParamID)
        {
            switch (ParamID)
            {
                case 1: return 1;
                case 2: return 2;
                case 3: return 4;
                case 4: return 5;
                case 5: return 6;
                case 6: return 7;
                case 7: return 8;
                case 8: return 10;
                case 9: return 11;
                case 10: return 12;
                case 11: return 13;
                case 12: return 14;
                case 13: return 15;
                case 14: return 16;
                case 15: return 17;
                case 16: return 18;
                case 17: return 19;
                case 18: return 20;
                case 19: return 21;
                case 20: return 22;
                case 21: return 23;
                case 22: return 24;
                case 23: return 25;
                case 24: return 27;
                case 25: return 31;
                case 26: return 33;
                case 27: return 34;
                case 28: return 35;
                case 29: return 36;
                case 30: return 37;
                case 31: return 38;
                case 32: return 80;
                case 33: return 93;
                case 34: return 98;
                case 35: return 99;
                case 36: return 105;
                case 37: return 108;
                case 38: return 110;
                case 39: return 111;
                case 40: return 112;
                case 41: return 113;
                case 42: return 114;
                case 43: return 115;
                case 44: return 116;
                case 45: return 117;
                case 46: return 119;
                case 47: return 130;
                case 48: return 131;
                case 49: return 132;
                case 50: return 133;
                case 51: return 134;
                case 52: return 135;
                case 53: return 136;
                case 54: return 137;
                case 55: return 140;
                case 56: return 141;
                case 57: return 142;
                case 58: return 143;
                case 59: return 150;
                case 60: return 155;
                case 61: return 157;
                case 62: return 162;
                case 63: return 163;
                case 64: return 165;
                case 65: return 166;
                case 66: return 167;
                case 67: return 168;
                case 68: return 169;
                case 69: return 177;
                case 70: return 181;
                case 71: return 182;
                case 72: return 183;
                case 73: return 184;
                case 74: return 185;
                case 75: return 192;
                case 76: return 193;
                case 77: return 196;
                case 78: return 198;
                case 79: return 503;
                case 80: return 505;
                case 81: return 506;
                case 82: return 507;
                case 83: return 508;
                case 84: return 513;
                case 85: return 514;
                case 86: return 515;
                case 87: return 517;
                case 88: return 518;
                case 89: return 603;
                case 90: return 604;
                case 91: return 605;
                case 92: return 606;
                case 93: return 607;
                case 94: return 608;
                case 95: return 609;
                case 96: return 616;
                case 97: return 617;
                case 98: return 619;
                case 99: return 624;
                case 100: return 625;
                case 101: return 629;
                case 102: return 637;
                case 103: return 638;
                case 104: return 646;
                case 105: return 647;
                case 106: return 649;
                case 107: return 650;
                case 108: return 652;
                case 109: return 653;
                case 110: return 654;
                case 111: return 656;
                case 112: return 659;
                case 113: return 662;
                case 114: return 663;
                case 115: return 664;
                case 116: return 665;
                case 117: return 674;
                case 118: return 675;
                case 119: return 676;
                case 120: return 678;
                case 121: return 682;
                case 122: return 683;
                case 123: return 684;
                case 124: return 685;
                case 125: return 690;
                case 126: return 692;
                case 127: return 693;
                case 128: return 700;
                case 129: return 701;
                case 130: return 702;
                case 131: return 703;
                case 132: return 704;
                case 133: return 705;
                case 134: return 706;
                case 135: return 707;
                case 136: return 708;
                case 137: return 709;
                case 138: return 710;
                case 139: return 711;
                case 140: return 712;
                case 141: return 713;
                case 142: return 714;
                case 143: return 715;
                case 144: return 750;
                case 145: return 752;
                case 146: return 753;
                case 147: return 754;
                case 148: return 755;
                case 149: return 756;
                case 150: return 757;
                case 151: return 758;
                case 152: return 759;
                case 153: return 760;
                case 154: return 762;
                case 155: return 763;
                case 156: return 764;
                case 157: return 765;
                case 158: return 769;
                case 159: return 773;
                case 160: return 775;
                case 161: return 779;
                case 162: return 780;
                case 163: return 781;
                case 164: return 785;
                case 165: return 789;
                case 166: return 795;
                case 167: return 796;
                case 168: return 799;
                case 169: return 800;
                case 170: return 801;
                case 171: return 802;
                case 172: return 803;
                case 173: return 804;
                case 174: return 805;
                case 175: return 806;
                case 176: return 807;
                case 177: return 808;
                case 178: return 812;
                case 179: return 813;
                case 180: return 814;
                case 181: return 815;
                case 182: return 816;
                case 183: return 817;
                case 184: return 818;
                case 185: return 819;
                case 186: return 820;
                case 187: return 821;
                case 188: return 822;
                case 189: return 823;
                case 190: return 824;
                case 191: return 825;
                case 192: return 826;
                case 193: return 827;
                case 194: return 828;
                case 195: return 829;
                case 196: return 830;
                case 197: return 834;
                case 198: return 835;
                case 199: return 836;
                case 200: return 840;
                case 201: return 841;
                case 202: return 842;
                case 203: return 844;
                case 204: return 848;
                case 205: return 858;
                case 206: return 859;
                case 207: return 860;
                case 208: return 861;
                case 209: return 862;
                case 210: return 863;
                case 211: return 868;
                case 212: return 869;
                case 213: return 877;
                case 214: return 879;
                case 215: return 880;
                case 216: return 921;
                case 217: return 922;
                case 218: return 923;

                default:
                    throw new Exception("We don't have a mapping for ParamID " + ParamID + " yet...");
            }
        }
    }
}
