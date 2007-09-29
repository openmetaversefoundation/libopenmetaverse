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
using System.Collections.Generic;

using libsecondlife;
using libsecondlife.InventorySystem;
using libsecondlife.Packets;

namespace libsecondlife.AssetSystem
{
	/// <summary>
	/// </summary>
    public class AssetPacketHelpers
	{
        private const bool DEBUG_PACKETS = true;

        /// <summary>
        /// Upload a small asset where the entire asset will fit in a single packet (less then 500 bytes)
        /// </summary>
        /// <param name="asset"></param>
        public static Packet AssetUploadRequest(Asset asset, LLUUID TransactionID)
		{
            if (asset._AssetData.Length > 1000)
            {
                throw new Exception("Asset too large to use AssetUploadRequest, use AssetUploadRequestHaderOnly() instead.");
            }

            AssetUploadRequestPacket p = new AssetUploadRequestPacket();
            p.AssetBlock.TransactionID = TransactionID;
            p.AssetBlock.Type          = asset.Type;
            p.AssetBlock.Tempfile      = asset.Tempfile;
            p.AssetBlock.AssetData     = asset._AssetData;
            p.AssetBlock.StoreLocal    = false;

            return p;
		}

        /// <summary>
        /// Send header to SL to let it know that a large asset upload is about to proceed.
        /// </summary>
        /// <param name="asset"></param>
		public static Packet AssetUploadRequestHeaderOnly(Asset asset, LLUUID TransactionID)
		{
            AssetUploadRequestPacket p = new AssetUploadRequestPacket();
            p.AssetBlock.TransactionID = TransactionID;
            p.AssetBlock.Type          = asset.Type;
            p.AssetBlock.Tempfile      = asset.Tempfile;
            p.AssetBlock.AssetData     = new byte[0];
            p.AssetBlock.StoreLocal    = false;

            return p;
        }

	
        /// <summary>
        /// Sends a packet of data to SL
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data">First xferpacket data must include a prefixed S32 for the length of the asset.</param>
        /// <param name="packetNum"></param>
        public static Packet SendXferPacket(ulong id, byte[] data, uint packetNum)
		{
            SendXferPacketPacket p = new SendXferPacketPacket();
            p.DataPacket.Data = data;

            p.XferID.ID = id;
            p.XferID.Packet = (uint)packetNum;

            return p;

		}

        /// <summary>
		/// Request the download of an asset
        /// The params field consists of a number of individual data components:
        /// Params: 1: AgentID
        /// Params: 2: SessionID
        /// Params: 3: OwnerID
        /// Params: 4: TaskID (LLUUID.Zero for assets not contained in an object)
        /// Params: 5: ItemID
        /// Params: 6: AssetID (LLUUIZ.Zero if it is unknown)
        /// Params: 7: Type (32-bit field)
        /// </summary>
        /// <param name="SessionID"></param>
        /// <param name="AgentID"></param>
        /// <param name="TransferID"></param>
        /// <param name="item"></param>
        public static Packet TransferRequest(LLUUID SessionID, LLUUID AgentID, LLUUID TransferID, InventoryItem item)
		{
            if (item.Type != 7 && item.Type != 10 && item.Type != 3)
            {
                Console.WriteLine("Warning: TransferRequest may not work for items other then notecards of type 7 and scripts of type 10");
            }

			byte[] param = new byte[100];
			int pos = 0;

			Buffer.BlockCopy(AgentID.Data, 0, param, pos, 16);
			pos += 16;

			Buffer.BlockCopy(SessionID.Data, 0, param, pos, 16);
			pos += 16;

			Buffer.BlockCopy(item.OwnerID.Data, 0, param, pos, 16);
			pos += 16;

			Buffer.BlockCopy(item.GroupID.Data, 0, param, pos, 16);
			pos += 16;

			Buffer.BlockCopy(item.ItemID.Data, 0, param, pos, 16);
			pos += 16;

			Buffer.BlockCopy(item.AssetID.Data, 0, param, pos, 16);
			pos += 16;

			param[pos] = (byte)item.Type;
			pos += 1;


            TransferRequestPacket p = new TransferRequestPacket();
            p.TransferInfo.TransferID   = TransferID;
            p.TransferInfo.Params       = param;
            p.TransferInfo.ChannelType  = 2;
            p.TransferInfo.SourceType   = 3;
            p.TransferInfo.Priority     = (float)101.0;
            return p;
        }

        /**
         * This doesn't seem to work for all asset types... Last noted not working was Notecards
         **/
        public static Packet TransferRequestDirect(LLUUID SessionID, LLUUID AgentID, LLUUID TransferID, LLUUID AssetID, sbyte Type)
        {
            byte[] param = new byte[20];
            int pos = 0;

            Buffer.BlockCopy(AssetID.Data, 0, param, pos, 16);
            pos += 16;

            param[pos] = (byte)Type;
            pos += 1;


            TransferRequestPacket p = new TransferRequestPacket();
            p.TransferInfo.TransferID = TransferID;
            p.TransferInfo.Params = param;
            p.TransferInfo.ChannelType = 2;
            p.TransferInfo.SourceType = 2;
            p.TransferInfo.Priority = (float)101.0;
            return p;
        }
	}
}
