using System;
using System.Collections;

using libsecondlife;
using libsecondlife.InventorySystem;
//using libsecondlife.AssetSystem;

namespace libsecondlife.AssetSystem.PacketHelpers
{
	/// <summary>
	/// </summary>
	public class AssetPackets
	{
		/*
			Low 00408 - AssetUploadRequest - Untrusted - Zerocoded
			1299 AssetBlock (01)
				0394 UUID (LLUUID / 1)
				0630 Type (S8 / 1)
				0766 Tempfile (BOOL / 1)
				1355 AssetData (Variable / 2)
				
	
			----- AssetUploadRequest -----
			AssetBlock
			UUID: fba4594511897d91ab5145f430c0b37d
			Type: 7
			Tempfile: False
			AssetData: 4c 69 6e 64 65 6e 20 74 65 78 74 20 76 65 72 73 Linden text vers
			AssetData: 69 6f 6e 20 31 0a 7b 0a 4c 4c 45 6d 62 65 64 64 ion 1.{.LLEmbedd
			AssetData: 65 64 49 74 65 6d 73 20 76 65 72 73 69 6f 6e 20 edItems version 
			AssetData: 31 0a 7b 0a 63 6f 75 6e 74 20 30 0a 7d 0a 54 65 1.{.count 0.}.Te
			AssetData: 78 74 20 6c 65 6e 67 74 68 20 32 31 0a 45 64 69 xt length 21.Edi
			AssetData: 74 69 6e 67 20 74 68 69 73 20 6e 6f 74 65 63 61 ting this noteca
			AssetData: 72 64 7d 0a 00                                  rd}..

	
		*/
		public static Packet AssetUploadRequest(ProtocolManager protocol, Asset asset)
		{
			Hashtable blocks = new Hashtable();
			Hashtable fields = new Hashtable();

			fields["UUID"]		= asset.AssetID;
			fields["Type"]		= asset.Type;
			fields["Tempfile"]	= asset.Tempfile;
			fields["AssetData"]	= asset.AssetData;

			blocks[fields]		= "AssetBlock";

			return PacketBuilder.BuildPacket("AssetUploadRequest", protocol, blocks, Helpers.MSG_RELIABLE | Helpers.MSG_ZEROCODED);
		}
		public static Packet AssetUploadRequestHeaderOnly(ProtocolManager protocol, Asset asset)
		{
			Hashtable blocks = new Hashtable();
			Hashtable fields = new Hashtable();

			fields["UUID"]		= asset.AssetID;
			fields["Type"]		= asset.Type;
			fields["Tempfile"]	= asset.Tempfile;
			blocks[fields]		= "AssetBlock";

			return PacketBuilder.BuildPacket("AssetUploadRequest", protocol, blocks, Helpers.MSG_RELIABLE | Helpers.MSG_ZEROCODED);
		}

	
		/*
		 * First xferpacket includes a prefixed S32 for the length of the asset.
		 * 
			High 00020 - SendXferPacket - Untrusted - Zerocoded
				0234 DataPacket (01)
					0527 Data (Variable / 2)
				0804 XferID (01)
					0030 ID (U64 / 1)
					0785 Packet (U32 / 1)
			High 00021 - ConfirmXferPacket - Untrusted - Zerocoded
				0804 XferID (01)
					0030 ID (U64 / 1)
					0785 Packet (U32 / 1)
		
			----- SendXferPacket -----
			DataPacket
				Data: cb 16 00 00 4c 69 6e 64 65 6e 20 74 65 78 74 20 ....Linden text 
				<snip>
				Data: 20 63 6f 6e 74 61 63 74 20 79 6f 75              contact you
			XferID
				ID: 12874913228238730530
				Packet: 0				
		 */

		public static Packet SendXferPacket(ProtocolManager protocol, U64 id, byte[] data, uint packetNum)
		{
			Hashtable blocks = new Hashtable();
			Hashtable fields;

			fields = new Hashtable();
			fields["Data"]				= data;
			blocks[fields]				= "DataPacket";

			fields = new Hashtable();
			fields["ID"]				= id;
			fields["Packet"]			= (uint)packetNum;
			blocks[fields]				= "XferID";

			return PacketBuilder.BuildPacket("SendXferPacket", protocol, blocks, Helpers.MSG_RELIABLE | Helpers.MSG_ZEROCODED);
		}

		/*
			----- TransferRequest -----
			TransferInfo
				TransferID: cc75b93dd4d8c25f6cf56c22f1b3b26c
				Params: 25 47 26 83 cb 32 45 16 90 4a 6c d0 ec ab f1 28 %G&..2E..Jl....(
				Params: ea ba cb 97 42 49 49 db 9f a6 37 f5 ab 56 61 7a ....BII...7..Vaz
				Params: 25 47 26 83 cb 32 45 16 90 4a 6c d0 ec ab f1 28 %G&..2E..Jl....(
				Params: 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ................
				Params: 58 26 f4 2e 9b 57 3f b1 68 f6 1b c6 0f e1 11 18 X&...W?.h.......
				Params: 6e c6 d5 9b 4b d2 53 47 24 26 73 13 5f d3 3b 94 n...K.SG$&s._.;.
				Params: 07 00 00 00                                     ....
				ChannelType: 2
				SourceType: 3
				Priority: 101.000000

			Low 00193 - TransferRequest - Untrusted - Unencoded
				0770 TransferInfo (01)
					0072 TransferID (LLUUID / 1)
					0867 Params (Variable / 2)
					0912 ChannelType (S32 / 1)
					1264 SourceType (S32 / 1)
					1370 Priority (F32 / 1)
					
			Params: 1: OwnerID / *AgentID
			Params: 2: SessionID
			Params: 3: *OwnerID / AgentID
			Params: 4: Unknown (Maybe Group ID)
			Params: 5: ItemID
			Params: 6: AssetID
			Params: 7: Type
			Params: 8: Last Three Bytes Unknown
		*/

		public static Packet TransferRequest(ProtocolManager protocol, LLUUID SessionID, LLUUID AgentID, LLUUID TransferID, InventoryItem item)
		{
			Hashtable blocks = new Hashtable();
			Hashtable fields = new Hashtable();

			byte[] param = new byte[100];
			int pos = 0;

			Array.Copy(AgentID.Data, 0, param, pos, 16);
			pos += 16;

			Array.Copy(SessionID.Data, 0, param, pos, 16);
			pos += 16;

			Array.Copy(item.OwnerID.Data, 0, param, pos, 16);
			pos += 16;

			Array.Copy(item.GroupID.Data, 0, param, pos, 16);
			pos += 16;

			Array.Copy(item.ItemID.Data, 0, param, pos, 16);
			pos += 16;

			Array.Copy(item.AssetID.Data, 0, param, pos, 16);
			pos += 16;

			param[pos] = (byte)item.Type;
			pos += 1;

			fields["TransferID"]	= TransferID;
			fields["Params"]		= param;
			fields["ChannelType"]	= 2;
			fields["SourceType"]	= 3;
			fields["Priority"]		= (float)101.0;

			blocks[fields]		= "TransferInfo";

			return PacketBuilder.BuildPacket("TransferRequest", protocol, blocks, Helpers.MSG_RELIABLE | Helpers.MSG_ZEROCODED);
		}

	}
}
