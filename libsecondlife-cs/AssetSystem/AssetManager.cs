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
    public enum AssetPermission : uint
    {
        All = 0x7FFFFFFF,
        Copy = 0x00008000,
        Modify = 0x00004000,
        Transfer = 0x00002000,
        Move = 0x00080000
    }

    public class AssetPermissionException : Exception 
    {
        private InventoryItem _Item;
        public InventoryItem Item 
        {
            get { return _Item; }
        }
        private SecondLife _Client;
        public SecondLife Client
        {
            get { return _Client; }
        }

        public AssetPermissionException(InventoryItem item, SecondLife client, string message)
            : base (message)
        {
            _Item = item;
            _Client = client;
        }
    }

	/// <summary>
	/// Summary description for AssetManager.
	/// </summary>
	public class AssetManager
	{
		private SecondLife slClient;

        private AssetRequestUpload curUploadRequest = null;
        private Dictionary<LLUUID, AssetRequestDownload> htDownloadRequests = new Dictionary<LLUUID, AssetRequestDownload>();

        /// <summary>
        /// </summary>
        /// <param name="client"></param>
        internal AssetManager(SecondLife client)
		{
			slClient = client;

            // Need to know when we're Connected/Disconnected to clear state
            slClient.Network.OnDisconnected += new NetworkManager.DisconnectCallback(Network_OnDisconnected);
            slClient.Network.OnConnected += new NetworkManager.ConnectedCallback(Network_OnConnected);

			// Used to upload small assets, or as an initial start packet for large transfers
            slClient.Network.RegisterCallback(PacketType.AssetUploadComplete, new NetworkManager.PacketCallback(AssetUploadCompleteCallbackHandler));
			// Transfer Packets for downloading large assets
            slClient.Network.RegisterCallback(PacketType.TransferInfo, new NetworkManager.PacketCallback(TransferInfoCallbackHandler));
            slClient.Network.RegisterCallback(PacketType.TransferPacket, new NetworkManager.PacketCallback(TransferPacketCallbackHandler));
			// XFer packets for uploading large assets
            slClient.Network.RegisterCallback(PacketType.ConfirmXferPacket, new NetworkManager.PacketCallback(ConfirmXferPacketCallbackHandler));
            slClient.Network.RegisterCallback(PacketType.RequestXfer, new NetworkManager.PacketCallback(RequestXferCallbackHandler));
		}

        void Network_OnConnected(object sender)
        {
            ClearState();
        }

        void Network_OnDisconnected(NetworkManager.DisconnectType reason, string message)
        {
            ClearState();
        }

        private void ClearState()
        {
            htDownloadRequests.Clear();
            curUploadRequest = null;
        }

        /// <summary>
        /// Handle the appropriate sink fee associated with an asset upload
        /// </summary>
        public void SinkFee()
		{
            slClient.Self.GiveMoney(LLUUID.Zero, slClient.Settings.UPLOAD_COST, "Image Upload");
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
                curUploadRequest = new AssetRequestUpload(slClient, LLUUID.Random(), asset);

                LLUUID assetID = curUploadRequest.DoUpload();
                if (asset.Type == Asset.ASSET_TYPE_IMAGE)
                {
                    SinkFee();
                }
                return assetID;
            }
            finally
            {
                curUploadRequest = null;
            }
		}

        /// <summary>
        /// Get the Asset data for an item, must be used when requesting a Notecard
        /// </summary>
        /// <param name="item"></param>
		public void GetInventoryAsset( InventoryItem item )
		{
            if ( (item.OwnerMask & (uint)AssetPermission.Copy) == 0 )
                throw new AssetPermissionException(item, slClient, "Asset data refused, Copy permission needed.");
            if ( (item.OwnerMask & (uint)AssetPermission.Modify) == 0 && (item.Type == 10) )
                throw new AssetPermissionException(item, slClient, "Asset data refused, Modify permission needed for scripts.");

			LLUUID TransferID = LLUUID.Random();

            AssetRequestDownload request = new AssetRequestDownload(TransferID);
            request.Size = int.MaxValue; // Number of bytes expected
            request.Received = 0; // Number of bytes received
            request.UpdateLastPacketTime(); // last time we recevied a packet for this request

            htDownloadRequests[TransferID] = request;

            // prep packet based on asset type
            Packet packet;
            switch (item.Type)
            {
                case 5:  //Shirt
                case 13: //Bodyshape
                    packet = AssetPacketHelpers.TransferRequestDirect(slClient.Network.SessionID, slClient.Network.AgentID, TransferID, item.AssetID, item.Type);
                    break;
                default:
			        packet = AssetPacketHelpers.TransferRequest(slClient.Network.SessionID, slClient.Network.AgentID, TransferID, item );
                    break;
            }

            // Send packet
            slClient.Network.SendPacket(packet);

            #if DEBUG_PACKETS
                slClient.DebugLog(packet);
            #endif

            request.Completed.WaitOne();

            item.SetAssetData(request.AssetData);
		}

        /// <summary>
        /// Get Asset data, works with BodyShapes (type 13) but does not work with Notecards(type 7)
        /// </summary>
        /// <param name="asset"></param>
        public void GetInventoryAsset(Asset asset)
		{
			LLUUID TransferID = LLUUID.Random();

            AssetRequestDownload request = new AssetRequestDownload(TransferID);
            request.Size = int.MaxValue; // Number of bytes expected
            request.Received = 0; // Number of bytes received
            request.UpdateLastPacketTime(); // last time we recevied a packet for this request

            htDownloadRequests[TransferID] = request;

            Packet packet = AssetPacketHelpers.TransferRequestDirect(slClient.Network.SessionID, slClient.Network.AgentID, TransferID, asset.AssetID, asset.Type);
			slClient.Network.SendPacket(packet);

            #if DEBUG_PACKETS
                slClient.DebugLog(packet);
            #endif

            request.Completed.WaitOne();

            asset.SetAssetData(request.AssetData);
            
		}


        private void AssetUploadCompleteCallbackHandler(Packet packet, Simulator simulator)
		{
            #if DEBUG_PACKETS
                slClient.DebugLog(packet);
            #endif

            Packets.AssetUploadCompletePacket reply = (AssetUploadCompletePacket)packet;

            curUploadRequest.UploadComplete(reply.AssetBlock.UUID, reply.AssetBlock.Success);
		}

        private void RequestXferCallbackHandler(Packet packet, Simulator simulator)
		{
            #if DEBUG_PACKETS
                slClient.DebugLog(packet);
            #endif

            RequestXferPacket reply = (RequestXferPacket)packet;

            ulong XferID   = reply.XferID.ID;
			// LLUUID AssetID = reply.XferID.VFileID; //Not used...

            curUploadRequest.RequestXfer(XferID);
        }

        private void ConfirmXferPacketCallbackHandler(Packet packet, Simulator simulator)
        {
            #if DEBUG_PACKETS
                slClient.DebugLog(packet);
            #endif

            ConfirmXferPacketPacket reply = (ConfirmXferPacketPacket)packet;

            curUploadRequest.ConfirmXferPacket(reply.XferID.ID, reply.XferID.Packet);
        }


        private void TransferInfoCallbackHandler(Packet packet, Simulator simulator)
        {
            #if DEBUG_PACKETS
                slClient.DebugLog(packet);
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
                slClient.DebugLog(packet);
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

            // Add data to data dictionary.
            request.AssetDataReceived[reply.TransferData.Packet] = Data;
            request.Received += Data.Length;



            // If we've gotten all the data, mark it completed.
            if (request.Received >= request.Size)
            {
                int curPos = 0;
                foreach (KeyValuePair<int,byte[]> kvp in request.AssetDataReceived)
                {
                    Array.Copy(kvp.Value, 0, request.AssetData, curPos, kvp.Value.Length);
                    curPos += kvp.Value.Length;
                }

                request.Completed.Set();
            }
        }
	}
}
