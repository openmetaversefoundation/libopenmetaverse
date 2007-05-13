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


		protected SecondLife slClient;

        protected AssetRequestUpload curUploadRequest = null;
        protected Dictionary<LLUUID, AssetRequestDownload> htDownloadRequests = new Dictionary<LLUUID, AssetRequestDownload>();

        /// <summary>
        /// Time to wait for next packet, during an asset download.
        /// </summary>
        public readonly static int DefaultTimeout = 10000;

        /// <summary>
        /// Event singaling an asset transfer request has completed.
        /// </summary>
        /// <param name="request"></param>
        public delegate void On_TransferRequestCompleted(AssetRequest request);
        public event On_TransferRequestCompleted TransferRequestCompletedEvent;

        /// <summary>
        /// </summary>
        /// <param name="client"></param>
        public AssetManager(SecondLife client)
		{
			slClient = client;

            // Need to know when we're Connected/Disconnected to clear state
            slClient.Network.OnDisconnected += new NetworkManager.DisconnectedCallback(Network_OnDisconnected);
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

        #region State Handling
        private void Network_OnConnected(object sender)
        {
            ClearState();
        }

        private void Network_OnDisconnected(NetworkManager.DisconnectType reason, string message)
        {
            ClearState();
        }

        protected void ClearState()
        {
            htDownloadRequests.Clear();
            curUploadRequest = null;
        }
        #endregion

        #region Asset Uploading 

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
                if (asset.Type == (sbyte)Asset.AssetType.Texture)
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
        /// Handle the appropriate sink fee associated with an asset upload
        /// </summary>
        protected void SinkFee()
        {
            slClient.Self.GiveMoney(LLUUID.Zero, slClient.Settings.UPLOAD_COST, "Image Upload");
        }

        #endregion

        #region Download Assets

        /// <summary>
        /// Get the Asset data for an item, must be used when requesting a Notecard
        /// </summary>
        /// <remarks>It is the responsibility of the calling party to retrieve the asset data from the request object when it is compelte.</remarks>
        /// <param name="item"></param>
        public AssetRequestDownload RequestInventoryAsset(InventoryItem item)
		{
            if (!(item is InventoryWearable))
            {
                if ((item.OwnerMask & (uint)AssetPermission.Copy) == 0)
                    throw new AssetPermissionException(item, slClient, "Asset data refused, Copy permission needed.");
                if ((item.OwnerMask & (uint)AssetPermission.Modify) == 0 && (item.Type == 10))
                    throw new AssetPermissionException(item, slClient, "Asset data refused, Modify permission needed for scripts.");
            }

			LLUUID TransferID = LLUUID.Random();

            AssetRequestDownload request = new AssetRequestDownload(slClient.Assets, TransferID, item.AssetID);
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
                slClient.Log(packet.ToString(), Helpers.LogLevel.Info);
            #endif

            return request;
        }

        /// <summary>
        /// Get Asset data, works with BodyShapes (type 13) but does not work with Notecards(type 7)
        /// </summary>
        public AssetRequestDownload RequestInventoryAsset(LLUUID AssetID, sbyte Type)
		{
			LLUUID TransferID = LLUUID.Random();

            AssetRequestDownload request = new AssetRequestDownload(slClient.Assets, TransferID, AssetID);
            request.UpdateLastPacketTime(); // last time we recevied a packet for this request

            htDownloadRequests[TransferID] = request;

            Packet packet = AssetPacketHelpers.TransferRequestDirect(slClient.Network.SessionID, slClient.Network.AgentID, TransferID, AssetID, Type);
			slClient.Network.SendPacket(packet);

            #if DEBUG_PACKETS
            slClient.Log(packet.ToString(), Helpers.LogLevel.Info);
            #endif

            return request;

        }

        #endregion

        #region Event Generation
        internal void FireTransferRequestCompletedEvent(AssetRequest request)
        {
            if (TransferRequestCompletedEvent != null)
            {
                TransferRequestCompletedEvent(request);
            }
        }
        #endregion

        #region Deprecated Methods
        /// <summary>
        /// Get the Asset data for an item, must be used when requesting a Notecard
        /// </summary>
        /// <param name="item"></param>
        [Obsolete("Use RequestInventoryAsset instead.", false)]
        public void GetInventoryAsset(InventoryItem item)
        {
            RequestInventoryAsset(item).Wait(-1);
        }

        /// <summary>
        /// Get Asset data, works with BodyShapes (type 13) but does not work with Notecards(type 7)
        /// </summary>
        /// <param name="asset"></param>
        [Obsolete("Use RequestInventoryAsset instead.", false)]
        public void GetInventoryAsset(Asset asset)
        {
            AssetRequestDownload request = RequestInventoryAsset(asset.AssetID, asset.Type);
            request.Wait(-1);
            asset.SetAssetData(request.GetAssetData());
        }
        #endregion

        #region Callback Handlers (Uploading)

        private void AssetUploadCompleteCallbackHandler(Packet packet, Simulator simulator)
		{
            #if DEBUG_PACKETS
                slClient.Log(packet.ToString(), Helpers.LogLevel.Info);
            #endif

            if (curUploadRequest == null) return;

            Packets.AssetUploadCompletePacket reply = (AssetUploadCompletePacket)packet;

            if (reply.AssetBlock.Success)
            {
                curUploadRequest.UploadComplete(reply.AssetBlock.UUID, AssetRequest.RequestStatus.Success);
            }
            else
            {
                curUploadRequest.UploadComplete(reply.AssetBlock.UUID, AssetRequest.RequestStatus.Failure);
            }

            if (TransferRequestCompletedEvent != null)
            {
                try { TransferRequestCompletedEvent(curUploadRequest); }
                catch (Exception e) { slClient.Log(e.ToString(), Helpers.LogLevel.Error); }
            }
		}

        private void RequestXferCallbackHandler(Packet packet, Simulator simulator)
		{
            #if DEBUG_PACKETS
                slClient.Log(packet.ToString(), Helpers.LogLevel.Info);
            #endif

            RequestXferPacket reply = (RequestXferPacket)packet;

            ulong XferID = reply.XferID.ID;
			// LLUUID AssetID = reply.XferID.VFileID; //Not used...

            if (curUploadRequest != null) curUploadRequest.RequestXfer(XferID);
        }

        private void ConfirmXferPacketCallbackHandler(Packet packet, Simulator simulator)
        {
            #if DEBUG_PACKETS
                slClient.Log(packet.ToString(), Helpers.LogLevel.Info);
            #endif

            ConfirmXferPacketPacket reply = (ConfirmXferPacketPacket)packet;

            if (curUploadRequest != null) curUploadRequest.ConfirmXferPacket(reply.XferID.ID, reply.XferID.Packet);
        }

        #endregion

        #region Callback Handlers (Downloading)

        // Download stuff
        private void TransferInfoCallbackHandler(Packet packet, Simulator simulator)
        {
            #if DEBUG_PACKETS
                slClient.Log(packet.ToString(), Helpers.LogLevel.Info);
            #endif

            TransferInfoPacket reply = (TransferInfoPacket)packet;

            LLUUID TransferID = reply.TransferInfo.TransferID;
            int Size = reply.TransferInfo.Size;
            int Status = reply.TransferInfo.Status;

            //TODO: AssetID should be pulled out of the TransferInfo, if available

            // Lookup the request for this packet
            if (!htDownloadRequests.ContainsKey(TransferID))
            {
                //slClient.Log("Received unexpected TransferInfo packet." + Environment.NewLine + packet.ToString(), 
                //    Helpers.LogLevel.Warning);
                return;
            }
            AssetRequestDownload request = htDownloadRequests[TransferID];

            // Mark it as either not found or update the request information
            if (Status == -2)
            {
                request.SetExpectedSize(Size);
                request.Fail("Asset Status -2 :: Likely Status Not Found");
            }
            else
            {
                request.SetExpectedSize(Size);
            }
        }

        private void TransferPacketCallbackHandler(Packet packet, Simulator simulator)
        {
            #if DEBUG_PACKETS
                slClient.Log(packet.ToString(), Helpers.LogLevel.Info);
            #endif

            TransferPacketPacket reply = (TransferPacketPacket)packet;

            LLUUID TransferID = reply.TransferData.TransferID;
            byte[] Data = reply.TransferData.Data;


            // Lookup the request for this packet
            if (!htDownloadRequests.ContainsKey(TransferID))
            {
                //slClient.Log("Received unexpected TransferPacket packet." + Environment.NewLine + packet.ToString(), 
                //    Helpers.LogLevel.Warning);
                return;
            }
            AssetRequestDownload request = htDownloadRequests[TransferID];

            // Append data to data received.
            request.AddDownloadedData(reply.TransferData.Packet, Data);
        }

        #endregion
    }
}
