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

//#define DEBUG_PACKETS

using System;
using System.Collections.Generic;

using libsecondlife;

using libsecondlife.InventorySystem;

using libsecondlife.Packets;
using System.Threading;

namespace libsecondlife.AssetSystem
{
	/// <summary>
	/// Summary description for AssetManager.
	/// </summary>
	public class AssetManager
	{


		public const int SINK_FEE_IMAGE = 1;

		private SecondLife slClient;

        private AssetRequestUpload curUploadRequest = null;
        private Dictionary<LLUUID, AssetRequestDownload> htDownloadRequests = new Dictionary<LLUUID, AssetRequestDownload>();

        /// <summary>
        /// </summary>
        /// <param name="client"></param>
        internal AssetManager(SecondLife client)
		{
			slClient = client;

			// Used to upload small assets, or as an initial start packet for large transfers
            slClient.Network.RegisterCallback(PacketType.AssetUploadComplete, new NetworkManager.PacketCallback(AssetUploadCompleteCallbackHandler));
			// Transfer Packets for downloading large assets
            slClient.Network.RegisterCallback(PacketType.TransferInfo, new NetworkManager.PacketCallback(TransferInfoCallbackHandler));
            slClient.Network.RegisterCallback(PacketType.TransferPacket, new NetworkManager.PacketCallback(TransferPacketCallbackHandler));
			// XFer packets for uploading large assets
            slClient.Network.RegisterCallback(PacketType.ConfirmXferPacket, new NetworkManager.PacketCallback(ConfirmXferPacketCallbackHandler));
            slClient.Network.RegisterCallback(PacketType.RequestXfer, new NetworkManager.PacketCallback(RequestXferCallbackHandler));
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
					slClient.Self.GiveMoney( LLUUID.Zero, 10, "Image Upload" );
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

            try
            {
                curUploadRequest = new AssetRequestUpload(slClient, LLUUID.GenerateUUID(), asset);

                LLUUID assetID = curUploadRequest.DoUpload();
                if (asset.Type == Asset.ASSET_TYPE_IMAGE)
                {
                    //SinkFee(SINK_FEE_IMAGE);
                }
                return assetID;
            }
            finally
            {
                curUploadRequest = null;
            }
		}

        /// <summary>
        /// Get the Asset data for an item
        /// </summary>
        /// <param name="item"></param>
		public void GetInventoryAsset( InventoryItem item )
		{
			LLUUID TransferID = LLUUID.GenerateUUID();

            AssetRequestDownload request = new AssetRequestDownload(TransferID);
            request.Size = int.MaxValue; // Number of bytes expected
            request.Received = 0; // Number of bytes received
            request.UpdateLastPacketTime(); // last time we recevied a packet for this request

            htDownloadRequests[TransferID] = request;

			Packet packet = AssetPacketHelpers.TransferRequest(slClient.Network.SessionID, slClient.Network.AgentID, TransferID, item );
			slClient.Network.SendPacket(packet);

            #if DEBUG_PACKETS
                Console.WriteLine(packet);
            #endif

            request.Completed.WaitOne();

            item.SetAssetData(request.AssetData);
		}

		public void GetInventoryAsset( Asset asset )
		{
			LLUUID TransferID = LLUUID.GenerateUUID();

            AssetRequestDownload request = new AssetRequestDownload(TransferID);
            request.Size = int.MaxValue; // Number of bytes expected
            request.Received = 0; // Number of bytes received
            request.UpdateLastPacketTime(); // last time we recevied a packet for this request

            htDownloadRequests[TransferID] = request;

			Packet packet = AssetPacketHelpers.TransferRequest4BodyShape(slClient.Network.SessionID, slClient.Network.AgentID, TransferID, asset );
			slClient.Network.SendPacket(packet);

            #if DEBUG_PACKETS
                Console.WriteLine(packet);
            #endif

            request.Completed.WaitOne();

            asset.AssetData = request.AssetData;
            
		}


        private void AssetUploadCompleteCallbackHandler(Packet packet, Simulator simulator)
		{
            #if DEBUG_PACKETS
                Console.WriteLine(packet);
            #endif

            Packets.AssetUploadCompletePacket reply = (AssetUploadCompletePacket)packet;

            curUploadRequest.UploadComplete(reply.AssetBlock.UUID, reply.AssetBlock.Success);
		}

        private void RequestXferCallbackHandler(Packet packet, Simulator simulator)
		{
            #if DEBUG_PACKETS
                Console.WriteLine(packet);
            #endif

            RequestXferPacket reply = (RequestXferPacket)packet;

            ulong XferID   = reply.XferID.ID;
			// LLUUID AssetID = reply.XferID.VFileID; //Not used...

            curUploadRequest.RequestXfer(XferID);
        }

        private void ConfirmXferPacketCallbackHandler(Packet packet, Simulator simulator)
        {
            #if DEBUG_PACKETS
                Console.WriteLine(packet);
            #endif

            ConfirmXferPacketPacket reply = (ConfirmXferPacketPacket)packet;

            curUploadRequest.ConfirmXferPacket(reply.XferID.ID, reply.XferID.Packet);
        }


        private void TransferInfoCallbackHandler(Packet packet, Simulator simulator)
        {
            #if DEBUG_PACKETS
                Console.WriteLine(packet);
            #endif

            TransferInfoPacket reply = (TransferInfoPacket)packet;

            LLUUID TransferID = reply.TransferInfo.TransferID;
            int Size = reply.TransferInfo.Size;
            int Status = reply.TransferInfo.Status;

            // Lookup the request for this packet
            AssetRequestDownload request = htDownloadRequests[TransferID];
            if (request == null)
            {
                return;
            }

            // Mark it as either not found or update the request information
            if (Status == -2)
            {
                request.Status = false;
                request.StatusMsg = "Asset Status -2 :: Likely Status Not Found";

                request.Size = 0;
                request.AssetData = new byte[0];
                request.Completed.Set();
            }
            else
            {
                request.Size = Size;
                request.AssetData = new byte[Size];
            }
        }

        private void TransferPacketCallbackHandler(Packet packet, Simulator simulator)
        {
            #if DEBUG_PACKETS
                Console.WriteLine(packet);
            #endif

            TransferPacketPacket reply = (TransferPacketPacket)packet;

            LLUUID TransferID = reply.TransferData.TransferID;
            byte[] Data = reply.TransferData.Data;


            // Append data to data received.
            AssetRequestDownload request = htDownloadRequests[TransferID];
            if (request == null)
            {
                return;
            }

            lock (request)
            {
                Array.Copy(Data, 0, request.AssetData, request.Received, Data.Length);
                request.Received += Data.Length;

                // If we've gotten all the data, mark it completed.
                if (request.Received >= request.Size)
                {
                    Console.WriteLine("Download Complete");
                    request.Completed.Set();
                }
            }
        }
	}
}
