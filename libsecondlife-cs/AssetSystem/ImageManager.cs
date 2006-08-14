using System;
using System.Collections;

using libsecondlife;

using libsecondlife.InventorySystem;

using libsecondlife.Packets;

namespace libsecondlife.AssetSystem
{
	/// <summary>
	/// Summary description for AssetManager.
	/// </summary>
	public class ImageManager
	{
		private SecondLife slClient;

		private Hashtable htDownloadRequests = new Hashtable();

		private class TransferRequest
		{
			public bool Completed;
			public bool Status;
			public string StatusMsg;

			public uint Size;
			public uint Received;
			public int LastPacket;
			public byte[] AssetData;

			public TransferRequest()
			{
				Completed = false;

				Status		= false;
				StatusMsg	= "";

				AssetData	= null;
			}
		}

		public ImageManager( SecondLife client )
		{
			slClient = client;

			// Used to upload small assets, or as an initial start packet for large transfers
			PacketCallback ImageDataCallback = new PacketCallback(ImageDataCallbackHandler);
			slClient.Network.RegisterCallback("ImageData", ImageDataCallback);

			// Transfer Packets for downloading large assets		
			PacketCallback ImagePacketCallback = new PacketCallback(ImagePacketCallbackHandler);
			slClient.Network.RegisterCallback("ImagePacket", ImagePacketCallback);

		}

		public byte[] RequestImage( LLUUID ImageID )
		{
			TransferRequest tr = new TransferRequest();
			tr.Completed  = false;
			tr.Size		  = int.MaxValue; // Number of bytes expected
			tr.Received   = 0; // Number of bytes received
			tr.LastPacket = getUnixtime(); // last time we recevied a packet for this request

			htDownloadRequests[ImageID] = tr;

			Packet packet = ImagePackets.RequestImage(slClient.Protocol, ImageID );
			slClient.Network.SendPacket(packet);

			while( tr.Completed == false )
			{
				slClient.Tick();
			}

			if( tr.Status == true )
			{
				return tr.AssetData;
			} 
			else 
			{
				throw new Exception( "RequestImage: " + tr.StatusMsg );
			}

		}


		/*
			High 00010 - ImageData - Trusted - Zerocoded
				0233 ImageID (01)
					0030 ID (LLUUID / 1)
					0085 Packets (U16 / 1)
					0584 Size (U32 / 1)
					1203 Codec (U8 / 1)
				1334 ImageData (01)
					0527 Data (Variable / 2)
					
			---- ImageData ----
				-- ImageID --
					ID: 8955674724cb43ed920b47caed15465f
					Packets: 25344
					Size: 98282
					Codec: 2
				-- ImageData --
					Data: FF 4F FF 51 00 2F 00 00 00 00 02 00 00 00 02 00 .O.Q./..........		
					*/

		public void ImageDataCallbackHandler(Packet packet, Simulator simulator)
		{
//			Console.WriteLine( packet );

			LLUUID ImageID = null;
			ushort Packets = 0;
			uint   Size    = 0;
			byte[] Data    = null;

			ArrayList blocks = packet.Blocks();
			foreach (Block block in blocks)
			{
				if( block.Layout.Name.Equals("ImageID") )
				{
					foreach (Field field in block.Fields )
					{
						switch( field.Layout.Name )
						{
							case "ID":
								ImageID = (LLUUID)field.Data;
								break;
							case "Packets":
								Packets = (ushort)field.Data;
								break;
							case "Size":
								Size	= (uint)field.Data;
								break;

							case "Codec":
								// Not used
								break;
						}
					}
				}
				if( block.Layout.Name.Equals("ImageData") )
				{
					foreach (Field field in block.Fields )
					{
						switch( field.Layout.Name )
						{
							case "Data":
								Data = (byte[])field.Data;
								break;
						}
					}
				}
			}

			TransferRequest tr = (TransferRequest)htDownloadRequests[ImageID];
			if( tr == null )
			{
				return;
			}

			tr.Size = Size;
			tr.AssetData = new byte[tr.Size];

			Array.Copy(Data, 0, tr.AssetData, tr.Received, Data.Length);
			tr.Received += (uint)Data.Length;

			// If we've gotten all the data, mark it completed.
			if( tr.Received >= tr.Size )
			{
				tr.Completed = true;
				tr.Status	 = true;
			}

			
		}

		/*
			High 00011 - ImagePacket - Trusted - Zerocoded
				0233 ImageID (01)
					0030 ID (LLUUID / 1)
					0785 Packet (U16 / 1)
				1334 ImageData (01)
					0527 Data (Variable / 2)
					
			---- ImagePacket ----
				-- ImageID --
					ID: f252794e1b0fbe2f0f10020a437a9e40
					Packet: 256
			-- ImageData --
				Data: 80 80 F9 B7 A8 5E 6A 5E 34 1C E1 8E 25 C5 6B 18 .....^j^4...%.k.
		
		 */

		public void ImagePacketCallbackHandler(Packet packet, Simulator simulator)
		{
			LLUUID ImageID = null;
			byte[] Data    = null;

			ArrayList blocks = packet.Blocks();
			foreach (Block block in blocks)
			{
				if( block.Layout.Name.Equals("ImageID") )
				{
					foreach (Field field in block.Fields )
					{
						switch( field.Layout.Name )
						{
							case "ID":
								ImageID = (LLUUID)field.Data;
								break;
							case "Packet":
								// Not Used
								break;
						}
					}
				}
				if( block.Layout.Name.Equals("ImageData") )
				{
					foreach (Field field in block.Fields )
					{
						switch( field.Layout.Name )
						{
							case "Data":
								Data = (byte[])field.Data;
								break;
						}
					}
				}
			}

			TransferRequest tr = (TransferRequest)htDownloadRequests[ImageID];
			if( tr == null )
			{
				return;
			}

			Array.Copy(Data, 0, tr.AssetData, tr.Received, Data.Length);
			tr.Received += (uint)Data.Length;

			// If we've gotten all the data, mark it completed.
			if( tr.Received >= tr.Size )
			{
				tr.Completed = true;
				tr.Status	 = true;
			}		
		}


		public static int getUnixtime()
		{
			TimeSpan ts = (DateTime.UtcNow - new DateTime(1970,1,1,0,0,0));
			return (int)ts.TotalSeconds;
		}
	}
}
