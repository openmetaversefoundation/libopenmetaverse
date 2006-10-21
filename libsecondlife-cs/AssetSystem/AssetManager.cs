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
using System.Collections;

using libsecondlife;

using libsecondlife.InventorySystem;

using libsecondlife.Packets;

namespace libsecondlife.AssetSystem
{
	/// <summary>
	/// Summary description for AssetManager.
	/// </summary>
	public class AssetManager
	{
        private const bool DEBUG_PACKETS = true;


		public const int SINK_FEE_IMAGE = 1;

		private SecondLife slClient;

        private TransferRequest curUploadRequest = null;
		private Hashtable htDownloadRequests = new Hashtable();

		private class TransferRequest
		{
			public bool Completed;
			public bool Status;
			public string StatusMsg;

			public int Size;
			public int Received;
			public uint LastPacketTime;
            public uint LastPacketNumSent;
			public byte[] AssetData;

            public LLUUID TransactionID;
            public LLUUID AssetID;
		}

        /// <summary>
        /// </summary>
        /// <param name="client"></param>
        internal AssetManager(SecondLife client)
		{
			slClient = client;

			// Used to upload small assets, or as an initial start packet for large transfers
			PacketCallback AssetUploadCompleteCallback = new PacketCallback(AssetUploadCompleteCallbackHandler);
            slClient.Network.RegisterCallback(PacketType.AssetUploadComplete, AssetUploadCompleteCallback);

			// Transfer Packets for downloading large assets		
			PacketCallback TransferInfoCallback = new PacketCallback(TransferInfoCallbackHandler);
            slClient.Network.RegisterCallback(PacketType.TransferInfo, TransferInfoCallback);

			PacketCallback TransferPacketCallback = new PacketCallback(TransferPacketCallbackHandler);
            slClient.Network.RegisterCallback(PacketType.TransferPacket, TransferPacketCallback);

			// XFer packets for uploading large assets
			PacketCallback ConfirmXferPacketCallback = new PacketCallback(ConfirmXferPacketCallbackHandler);
            slClient.Network.RegisterCallback(PacketType.ConfirmXferPacket, ConfirmXferPacketCallback);
			
			PacketCallback RequestXferCallback = new PacketCallback(RequestXferCallbackHandler);
            slClient.Network.RegisterCallback(PacketType.RequestXfer, RequestXferCallback);
			
		}


        /// <summary>
        /// Handle the appropriate sink fee assoiacted with an asset upload
        /// </summary>
        /// <param name="sinkType"></param>
        public void SinkFee(int sinkType)
		{
			switch( sinkType )
			{
				case SINK_FEE_IMAGE:
					slClient.Avatar.GiveMoney( new LLUUID(), 10, "Image Upload" );
					break;
				default:
					throw new Exception("AssetManager: Unknown sinktype (" + sinkType + ")");
			}
		}

        /// <summary>
        /// Upload an asset to Second Life
        /// </summary>
        /// <param name="asset">The asset to be uploaded</param>
        /// <returns>The Asset ID of the completed upload</returns>
        public LLUUID UploadAsset(Asset asset)
		{
            if (curUploadRequest != null)
            {
                throw new Exception("An existing asset upload is currently in-progress.");
            }

			Packet packet;
            curUploadRequest = new TransferRequest();
            curUploadRequest.Completed = false;
			curUploadRequest.TransactionID = LLUUID.GenerateUUID();

			if( asset.AssetData.Length > 500 )
			{
                packet = AssetPacketHelpers.AssetUploadRequestHeaderOnly(asset, curUploadRequest.TransactionID);
				slClient.Network.SendPacket(packet);
                if (DEBUG_PACKETS) { Console.WriteLine(packet); }
                curUploadRequest.AssetData = asset.AssetData;
			} 
			else 
			{
                packet = AssetPacketHelpers.AssetUploadRequest(asset, curUploadRequest.TransactionID);
				slClient.Network.SendPacket(packet);
                if (DEBUG_PACKETS) { Console.WriteLine(packet); }
            }

			while( curUploadRequest.Completed == false )
			{
				slClient.Tick();
			}

            if (curUploadRequest.Status == false)
			{
                throw new Exception(curUploadRequest.StatusMsg);
			} else {
				if( asset.Type == Asset.ASSET_TYPE_IMAGE )
				{
					SinkFee( SINK_FEE_IMAGE );
				}

                asset.AssetID = curUploadRequest.AssetID;

                return asset.AssetID;
			}
		}

        /// <summary>
        /// Get the Asset data for an item
        /// </summary>
        /// <param name="item"></param>
		public void GetInventoryAsset( InventoryItem item )
		{
			LLUUID TransferID = LLUUID.GenerateUUID();

			TransferRequest tr = new TransferRequest();
			tr.Completed  = false;
			tr.Size		  = int.MaxValue; // Number of bytes expected
			tr.Received   = 0; // Number of bytes received
			tr.LastPacketTime = Helpers.GetUnixTime(); // last time we recevied a packet for this request

			htDownloadRequests[TransferID] = tr;

			Packet packet = AssetPacketHelpers.TransferRequest(slClient.Network.SessionID, slClient.Network.AgentID, TransferID, item );
			slClient.Network.SendPacket(packet);
            if (DEBUG_PACKETS) { Console.WriteLine(packet); }

			while( tr.Completed == false )
			{
				slClient.Tick();
			}

			item.SetAssetData( tr.AssetData );
		}

        private void AssetUploadCompleteCallbackHandler(Packet packet, Simulator simulator)
		{
            if (DEBUG_PACKETS) { Console.WriteLine(packet); }
            Packets.AssetUploadCompletePacket reply = (AssetUploadCompletePacket)packet;

            curUploadRequest.AssetID = reply.AssetBlock.UUID;
            bool Success = reply.AssetBlock.Success;

			if( Success )
			{
                curUploadRequest.Completed = true;
                curUploadRequest.Status = true;
                curUploadRequest.StatusMsg = "Success";
			} 
			else 
			{
                curUploadRequest.Completed = true;
                curUploadRequest.Status = false;
                curUploadRequest.StatusMsg = "Server returned failed";
			}
		}

        private void RequestXferCallbackHandler(Packet packet, Simulator simulator)
		{
            if (DEBUG_PACKETS) { Console.WriteLine(packet); }
            RequestXferPacket reply = (RequestXferPacket)packet;

            ulong XferID   = reply.XferID.ID;
			LLUUID AssetID = reply.XferID.VFileID;

            // Setup to send the first packet
            curUploadRequest.LastPacketNumSent = 0;

			byte[] packetData = new byte[1004];

            // Prefix the first Xfer packet with the data length
            // FIXME: Apply endianness patch
            Array.Copy(BitConverter.GetBytes((int)curUploadRequest.AssetData.Length), 0, packetData, 0, 4);
            Array.Copy(curUploadRequest.AssetData, 0, packetData, 4, 1000);

            packet = AssetPacketHelpers.SendXferPacket(XferID, packetData, 0);
			slClient.Network.SendPacket(packet);
            if (DEBUG_PACKETS) { Console.WriteLine(packet); }
        }

        private void ConfirmXferPacketCallbackHandler(Packet packet, Simulator simulator)
        {
            if (DEBUG_PACKETS) { Console.WriteLine(packet); }
            ConfirmXferPacketPacket reply = (ConfirmXferPacketPacket)packet;

            ulong XferID = reply.XferID.ID;
            uint PacketNumConfirmed = reply.XferID.Packet;

            if (PacketNumConfirmed == curUploadRequest.LastPacketNumSent)
            {
                curUploadRequest.LastPacketNumSent += 1;

                uint i = curUploadRequest.LastPacketNumSent;
                int numPackets = curUploadRequest.AssetData.Length / 1000;

                if (i < numPackets)
                {
                    byte[] packetData = new byte[1000];
                    Array.Copy(curUploadRequest.AssetData, i * 1000, packetData, 0, 1000);

                    packet = AssetPacketHelpers.SendXferPacket(XferID, packetData, i);
                    slClient.Network.SendPacket(packet);
                    if (DEBUG_PACKETS) { Console.WriteLine(packet); }
                }
                else
                {
                    // The last packet has to be handled slightly differently
                    int lastLen = curUploadRequest.AssetData.Length - (numPackets * 1000);
                    byte[] packetData = new byte[lastLen];
                    Array.Copy(curUploadRequest.AssetData, numPackets * 1000, packetData, 0, lastLen);

                    uint lastPacket = (uint)int.MaxValue + (uint)numPackets + (uint)1;
                    packet = AssetPacketHelpers.SendXferPacket(XferID, packetData, lastPacket);
                    slClient.Network.SendPacket(packet);
                    if (DEBUG_PACKETS) { Console.WriteLine(packet); }
                }
            } else {
                throw new Exception("Something is wrong with uploading assets, a confirmation came in for a packet we didn't send.");
            }
        }

        private void TransferInfoCallbackHandler(Packet packet, Simulator simulator)
        {
            if (DEBUG_PACKETS) { Console.WriteLine(packet); }
            TransferInfoPacket reply = (TransferInfoPacket)packet;

            LLUUID TransferID = reply.TransferInfo.TransferID;
            int Size = reply.TransferInfo.Size;
            int Status = reply.TransferInfo.Status;

            // Lookup the request for this packet
            TransferRequest tr = (TransferRequest)htDownloadRequests[TransferID];
            if (tr == null)
            {
                return;
            }

            // Mark it as either not found or update the request information
            if (Status == -2)
            {
                tr.Completed = true;
                tr.Status = false;
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

        private void TransferPacketCallbackHandler(Packet packet, Simulator simulator)
        {
            if (DEBUG_PACKETS) { Console.WriteLine(packet); }
            TransferPacketPacket reply = (TransferPacketPacket)packet;

            LLUUID TransferID = reply.TransferData.TransferID;
            byte[] Data = reply.TransferData.Data;


            // Append data to data received.
            TransferRequest tr = (TransferRequest)htDownloadRequests[TransferID];
            if (tr == null)
            {
                return;
            }

            Array.Copy(Data, 0, tr.AssetData, tr.Received, Data.Length);
            tr.Received += Data.Length;

            // If we've gotten all the data, mark it completed.
            if (tr.Received >= tr.Size)
            {
                tr.Completed = true;
            }
        }
	}
}
