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
					slClient.Self.GiveMoney( new LLUUID(), 10, "Image Upload" );
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
                curUploadRequest = new AssetRequestUpload(LLUUID.GenerateUUID(), asset);
                SendFirstPacket();


                while (curUploadRequest.Completed.WaitOne(1250, true) == false)
                {
                    Console.WriteLine("WaitOne() timeout while uploading");
                    if (curUploadRequest.SecondsSinceLastPacket > 2)
                    {
                        Console.WriteLine("Resending Packet (more then 2 seconds since last confirm)");
                        if (curUploadRequest.LastPacketNumSent != 0)
                        {
                            SendCurrentPacket();
                        }
                        else
                        {
                            Console.WriteLine("Want to resend, but the packet send function isn't good for resending the first packet");
                        }
                    }
                }


                if (curUploadRequest.Status == false)
                {
                    throw new Exception(curUploadRequest.StatusMsg);
                }
                else
                {
                    if (asset.Type == Asset.ASSET_TYPE_IMAGE)
                    {
                        SinkFee(SINK_FEE_IMAGE);
                    }

                    asset.AssetID = curUploadRequest.TransactionID;

                    return asset.AssetID;
                }
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

        private void AssetUploadCompleteCallbackHandler(Packet packet, Simulator simulator)
		{
            #if DEBUG_PACKETS
                Console.WriteLine(packet);
            #endif

            Packets.AssetUploadCompletePacket reply = (AssetUploadCompletePacket)packet;

            curUploadRequest.AssetID = reply.AssetBlock.UUID;
            curUploadRequest.Status = reply.AssetBlock.Success;
            curUploadRequest.UpdateLastPacketTime();

            if (curUploadRequest.Status)
            {
                curUploadRequest.StatusMsg = "Success";
            }
            else
            {
                curUploadRequest.StatusMsg = "Server returned failed";
            }

            Console.WriteLine("Upload Complete");
            curUploadRequest.Completed.Set();
		}

        private void RequestXferCallbackHandler(Packet packet, Simulator simulator)
		{
            #if DEBUG_PACKETS
                Console.WriteLine(packet);
            #endif

            RequestXferPacket reply = (RequestXferPacket)packet;

            ulong XferID   = reply.XferID.ID;
			// LLUUID AssetID = reply.XferID.VFileID; //Not used...

            // Setup to send the first packet
            curUploadRequest.LastPacketNumSent = 0;

            int dataSize = curUploadRequest.MyAsset.AssetData.Length;
            if (dataSize > 1000)
            {
                dataSize = 1000;
            }

            byte[] packetData = new byte[dataSize + 4]; // Extra space is for leading data length bytes

            // Prefix the first Xfer packet with the data length
            // FIXME: Apply endianness patch
            Array.Copy(BitConverter.GetBytes((int)curUploadRequest.MyAsset.AssetData.Length), 0, packetData, 0, 4);
            Array.Copy(curUploadRequest.MyAsset.AssetData, 0, packetData, 4, dataSize);

            packet = AssetPacketHelpers.SendXferPacket(XferID, packetData, 0);
			slClient.Network.SendPacket(packet);

            #if DEBUG_PACKETS
                Console.WriteLine(packet);
            #endif
        }

        private void ConfirmXferPacketCallbackHandler(Packet packet, Simulator simulator)
        {
            #if DEBUG_PACKETS
                Console.WriteLine(packet);
            #endif

            ConfirmXferPacketPacket reply = (ConfirmXferPacketPacket)packet;

            uint PacketNumConfirmed = reply.XferID.Packet;
            curUploadRequest.UpdateLastPacketTime();
            curUploadRequest.XferID = reply.XferID.ID;

            if (PacketNumConfirmed == curUploadRequest.LastPacketNumSent)
            {
                SendNextPacket();
            } else {
                throw new Exception("Something is wrong with uploading assets, a confirmation came in for a packet we didn't send.");
            }
        }

        private void SendFirstPacket()
        {
            Packet packet;

            lock( curUploadRequest )
            {
                if (curUploadRequest.MyAsset.AssetData.Length > 1000)
                {
                    packet = AssetPacketHelpers.AssetUploadRequestHeaderOnly(curUploadRequest.MyAsset, curUploadRequest.TransactionID);
                }
                else
                {
                    packet = AssetPacketHelpers.AssetUploadRequest(curUploadRequest.MyAsset, curUploadRequest.TransactionID);
                }
            }

            slClient.Network.SendPacket(packet);
            #if DEBUG_PACKETS
                Console.WriteLine(packet);
            #endif

        }

        private void SendNextPacket()
        {
            lock (curUploadRequest)
            {    
                // Increment Packet #
                curUploadRequest.LastPacketNumSent += 1;
            }

            SendCurrentPacket();
        }

        private void SendCurrentPacket()
        {
//            Console.WriteLine("Sending " + tr.LastPacketNumSent + " / " + tr.NumPackets);

            Packet uploadPacket;

            lock(curUploadRequest)
            {
                if (curUploadRequest.LastPacketNumSent < curUploadRequest.NumPackets)
                {
                    byte[] packetData = new byte[1000];
                    Array.Copy(curUploadRequest.MyAsset.AssetData, curUploadRequest.LastPacketNumSent * 1000, packetData, 0, 1000);

                    uploadPacket = AssetPacketHelpers.SendXferPacket(curUploadRequest.XferID, packetData, curUploadRequest.LastPacketNumSent);
                }
                else
                {
                    // The last packet has to be handled slightly differently
                    int lastLen = curUploadRequest.MyAsset.AssetData.Length - (curUploadRequest.NumPackets * 1000);
                    byte[] packetData = new byte[lastLen];
                    Array.Copy(curUploadRequest.MyAsset.AssetData, curUploadRequest.NumPackets * 1000, packetData, 0, lastLen);

                    uint lastPacket = (uint)int.MaxValue + (uint)curUploadRequest.NumPackets + (uint)1;
                    uploadPacket = AssetPacketHelpers.SendXferPacket(curUploadRequest.XferID, packetData, lastPacket);
                }
            }

            slClient.Network.SendPacket(uploadPacket);

            #if DEBUG_PACKETS
                Console.WriteLine(uploadPacket);
            #endif
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
