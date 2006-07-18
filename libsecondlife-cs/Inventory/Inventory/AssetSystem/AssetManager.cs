using System;
using System.Collections;

using libsecondlife;

using libsecondlife.InventorySystem;

using libsecondlife.AssetSystem.PacketHelpers;

namespace libsecondlife.AssetSystem
{
	/// <summary>
	/// Summary description for AssetManager.
	/// </summary>
	public class AssetManager
	{
		private SecondLife slClient;

		private Hashtable htUploadRequests = new Hashtable();
		private Hashtable htDownloadRequests = new Hashtable();

		private class TransferRequest
		{
			public bool Completed;
			public bool Status;
			public string StatusMsg;

			public int Size;
			public int Received;
			public int LastPacket;
			public byte[] AssetData;
		}

		public AssetManager( SecondLife client )
		{
			slClient = client;

			// Used to upload small assets, or as an initial start packet for large transfers
			PacketCallback AssetUploadCompleteCallback = new PacketCallback(AssetUploadCompleteCallbackHandler);
			slClient.Network.RegisterCallback("AssetUploadComplete", AssetUploadCompleteCallback);

			// Transfer Packets for downloading large assets		
			PacketCallback TransferInfoCallback = new PacketCallback(TransferInfoCallbackHandler);
			slClient.Network.RegisterCallback("TransferInfo", TransferInfoCallback);

			PacketCallback TransferPacketCallback = new PacketCallback(TransferPacketCallbackHandler);
			slClient.Network.RegisterCallback("TransferPacket", TransferPacketCallback);

			// XFer packets for uploading large assets
			PacketCallback ConfirmXferPacketCallback = new PacketCallback(ConfirmXferPacketCallbackHandler);
			slClient.Network.RegisterCallback("ConfirmXferPacket", ConfirmXferPacketCallback);
			
			PacketCallback RequestXferCallback = new PacketCallback(RequestXferCallbackHandler);
			slClient.Network.RegisterCallback("RequestXfer", RequestXferCallback);
			
		}

		
		public void UploadAsset( Asset asset )
		{
			Packet packet;
			TransferRequest tr = new TransferRequest();
			tr.Completed = false;
			htUploadRequests[asset.AssetID] = tr;

			if( asset.AssetData.Length > 500 )
			{
				packet = AssetPackets.AssetUploadRequestHeaderOnly(slClient.Protocol, asset);
				slClient.Network.SendPacket(packet);

				tr.AssetData = asset.AssetData;
			} 
			else 
			{
				packet = AssetPackets.AssetUploadRequest(slClient.Protocol, asset);
				slClient.Network.SendPacket(packet);
			}

			while( tr.Completed == false )
			{
				slClient.Tick();
			}

			if( tr.Status == false )
			{
				throw new Exception( tr.StatusMsg );
			}
		}

		public void GetInventoryAsset( InventoryItem item )
		{
			LLUUID TransferID = LLUUID.GenerateUUID();

			TransferRequest tr = new TransferRequest();
			tr.Completed  = false;
			tr.Size		  = int.MaxValue; // Number of bytes expected
			tr.Received   = 0; // Number of bytes received
			tr.LastPacket = getUnixtime(); // last time we recevied a packet for this request

			htDownloadRequests[TransferID] = tr;


			Packet packet = AssetPackets.TransferRequest(slClient.Protocol, slClient.Network.SessionID, slClient.Network.AgentID, TransferID, item );
			slClient.Network.SendPacket(packet);

			while( tr.Completed == false )
			{
				slClient.Tick();
			}

			item.SetAssetData( tr.AssetData );
		}


		public void AssetUploadCompleteCallbackHandler(Packet packet, Circuit circuit)
		{
			ArrayList blocks = packet.Blocks();
			
			LLUUID AssetID = "";
			bool   Success = false;

			foreach (Block block in blocks)
			{
				if( block.Layout.Name.Equals("AssetBlock") )
				{
					foreach (Field field in block.Fields )
					{
						switch( field.Layout.Name )
						{
							case "Success":
								Success = bool.Parse(field.Data.ToString());
								break;
							case "UUID":
								AssetID = new LLUUID(field.Data.ToString());
								break;
						}
					}

				}
			}

			TransferRequest tr = (TransferRequest)htUploadRequests[AssetID];
			if( Success )
			{
				tr.Completed = true;
				tr.Status    = true;
				tr.StatusMsg = "Success";
			} 
			else 
			{
				tr.Completed = true;
				tr.Status    = false;
				tr.StatusMsg = "Server returned failed";
			}
		}	

		/*
			---- TransferInfo ----
				-- TransferInfo --
					TransferID: 5eb06365fb85d94c853a15352da57574
					Size: 88
					ChannelType: 2
					TargetType: 0
					Status: 0
					
			Low 00194 - TransferInfo - Untrusted - Unencoded
				0770 TransferInfo (01)
					0072 TransferID (LLUUID / 1)
					0584 Size (S32 / 1)
					0912 ChannelType (S32 / 1)
					1049 TargetType (S32 / 1)
					1062 Status (S32 / 1)
		
		*/
		public void TransferInfoCallbackHandler(Packet packet, Circuit circuit)
		{
			ArrayList blocks = packet.Blocks();
			
			LLUUID TransferID = "";
			int Size = 0;
			int Status = 0;


			foreach (Block block in blocks)
			{
				if( block.Layout.Name.Equals("TransferInfo") )
				{
					foreach (Field field in block.Fields )
					{
						switch( field.Layout.Name )
						{
							case "Size":
								Size = (int)field.Data;
								break;
							case "TransferID":
								TransferID = (LLUUID)field.Data;
								break;
							case "Status":
								Status = (int)field.Data;
								break;
						}
					}

				}
			}

			TransferRequest tr = (TransferRequest)htDownloadRequests[TransferID];

			if( Status == -2 )
			{
				tr.Completed = true;
				tr.Status    = false;
				tr.StatusMsg = "Asset Status -2 :: Likely Status Not Found";

				tr.Size = 1;
				tr.AssetData = new byte[1];

			} 
			else 
			{
				tr.Size = Size;
				tr.AssetData = new byte[Size];
			}
		}

		/*
			----- TransferPacket -----
			TransferData
				TransferID: cc75b93dd4d8c25f6cf56c22f1b3b26c
				Data: 4c 69 6e 64 65 6e 20 74 65 78 74 20 76 65 72 73 Linden text vers
				Data: 69 6f 6e 20 31 0a 7b 0a 4c 4c 45 6d 62 65 64 64 ion 1.{.LLEmbedd
				Data: 65 64 49 74 65 6d 73 20 76 65 72 73 69 6f 6e 20 edItems version 
				Data: 31 0a 7b 0a 63 6f 75 6e 74 20 30 0a 7d 0a 54 65 1.{.count 0.}.Te
				Data: 78 74 20 6c 65 6e 67 74 68 20 39 32 0a 0a 45 61 xt length 92..Ea
				Data: 72 6c 79 20 41 6c 65 72 74 20 57 65 62 73 69 74 rly Alert Websit
				Data: 65 0a 68 74 74 70 3a 2f 2f 65 61 72 6c 79 61 6c e.http://earlyal
				Data: 65 72 74 2e 66 75 6c 6c 63 6f 6c 6c 2e 65 64 75 ert.fullcoll.edu
				Data: 0a 46 75 6c 6c 65 72 74 6f 6e 20 43 6f 6c 6c 65 .Fullerton Colle
				Data: 67 65 0a 43 6f 75 6e 73 65 6c 69 6e 67 20 44 65 ge.Counseling De
				Data: 70 61 72 74 6d 65 6e 74 0a 7d 0a 00             partment.}..
				Packet: 0
				ChannelType: 2
				Status: 1

		 */
		public void TransferPacketCallbackHandler(Packet packet, Circuit circuit)
		{
			LLUUID TransferID = "";
			byte[] Data = null;

			ArrayList blocks = packet.Blocks();
			foreach (Block block in blocks)
			{
				if( block.Layout.Name.Equals("TransferData") )
				{
					foreach (Field field in block.Fields )
					{
						switch( field.Layout.Name )
						{
							case "TransferID":
								TransferID = new LLUUID(field.Data.ToString());
								break;
							case "Data":
								Data = (byte[])field.Data;
								break;
						}
					}

				}
			}

			// Append data to data received.
			TransferRequest tr = (TransferRequest)htDownloadRequests[TransferID];
			Array.Copy(Data, 0, tr.AssetData, tr.Received, Data.Length);
			tr.Received += Data.Length;

			// If we've gotten all the data, mark it completed.
			if( tr.Received >= tr.Size )
			{
				tr.Completed = true;
			}
			
		}

		/*
			High 00021 - ConfirmXferPacket - Untrusted - Zerocoded
				0804 XferID (01)
					0030 ID (U64 / 1)
					0785 Packet (U32 / 1)
					
			----- ConfirmXferPacket -----
			XferID
				ID: 4089841211943505063
				Packet: 0

		 */

		public void ConfirmXferPacketCallbackHandler(Packet packet, Circuit circuit)
		{
			U64 XferID = new U64();
			uint PacketNum = 0;

			ArrayList blocks = packet.Blocks();
			foreach (Block block in blocks)
			{
				if( block.Layout.Name.Equals("XferID") )
				{
					foreach (Field field in block.Fields )
					{
						switch( field.Layout.Name )
						{
							case "ID":
								XferID = (U64)field.Data;
								break;
							case "Packet":
								PacketNum = (uint)field.Data;
								break;
						}
					}

				}
			}
		}


		/*
			Low 00197 - RequestXfer - Untrusted - Unencoded
				0804 XferID (01)
					0030 ID (U64 / 1)
					0183 UseBigPackets (BOOL / 1)
					0351 DeleteOnCompletion (BOOL / 1)
					0687 FilePath (U8 / 1)
					0817 Filename (Variable / 1)
					0997 VFileID (LLUUID / 1)
					1333 VFileType (S16 / 1)

			----- RequestXfer -----
			XferID
				ID: 12874913228238730530
				UseBigPackets: False
				DeleteOnCompletion: False
				FilePath: 0
				Filename: 
				VFileID: b16097032e253a9d5220ba07c1a1b28a
				VFileType: 7
		*/
		public void RequestXferCallbackHandler(Packet packet, Circuit circuit)
		{
			U64 XferID = new U64();
			LLUUID AssetID = "";

			ArrayList blocks = packet.Blocks();
			foreach (Block block in blocks)
			{
				if( block.Layout.Name.Equals("XferID") )
				{
					foreach (Field field in block.Fields )
					{
						switch( field.Layout.Name )
						{
							case "ID":
								XferID = (U64)field.Data;
								break;
							case "VFileID":
								AssetID = (LLUUID)field.Data;
								break;
						}
					}

				}
			}


			TransferRequest tr = (TransferRequest)htUploadRequests[AssetID];

			byte[] packetData = new byte[1004];

			// FIXME: Apply endianness patch
			Array.Copy(BitConverter.GetBytes((int)tr.AssetData.Length), 0, packetData, 0, 4);
			Array.Copy(tr.AssetData, 0, packetData, 4, 1000);

			packet = AssetPackets.SendXferPacket(slClient.Protocol, XferID, packetData, 0);
			slClient.Network.SendPacket(packet);


			// TODO: This for loop should be removed and these uploads should take place in
			// a call back handler for ConfirmXferPacket
			int numPackets = tr.AssetData.Length / 1000;
			for( uint i = 1; i<numPackets; i++ )
			{
				packetData = new byte[1000];
				Array.Copy(tr.AssetData, i*1000, packetData, 0, 1000);

				packet = AssetPackets.SendXferPacket(slClient.Protocol, XferID, packetData, i);
				slClient.Network.SendPacket(packet);
			}

			int lastLen = tr.AssetData.Length - (numPackets * 1000);
			packetData = new byte[ lastLen ];
			Array.Copy(tr.AssetData, numPackets * 1000, packetData, 0, lastLen);

			uint lastPacket = (uint)int.MaxValue + (uint)numPackets + (uint)1;
			packet = AssetPackets.SendXferPacket(slClient.Protocol, XferID, packetData, lastPacket);
			slClient.Network.SendPacket(packet);
		}

		public static int getUnixtime()
		{
			TimeSpan ts = (DateTime.UtcNow - new DateTime(1970,1,1,0,0,0));
			return (int)ts.TotalSeconds;
		}
	}
}
