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
#define DEBUG_HEADERS

using System;
using System.Collections.Generic;

using libsecondlife;

using libsecondlife.InventorySystem;

using libsecondlife.Packets;
using System.Threading;

namespace libsecondlife.AssetSystem
{
    public class AssetRequestUpload : AssetRequest
    {
        protected SecondLife _Client;
        protected readonly int _MaxResendAttempts = 20;

        protected ulong _XferID;

        protected int _ResendCount;
        protected uint _CurrentPacket;

        protected int _NumPackets2Send; 

        public AssetRequestUpload(SecondLife Client, LLUUID TransID, Asset Asset2Upload) : base(Client.Assets, TransID, Asset2Upload)
        {
            if ((AssetBeingTransferd._AssetData == null) || (AssetBeingTransferd._AssetData.Length == 0))
            {
                throw new Exception("Asset data cannot be null.");
            }

            _Client = Client;

            _CurrentPacket = 0;
            _ResendCount = 0;
            _NumPackets2Send = AssetBeingTransferd._AssetData.Length / 1000;
            if (_NumPackets2Send < 1)
            {
                _NumPackets2Send = 1;
            }
        }

        internal LLUUID DoUpload()
        {
            SendFirstPacket();

            while ((_Completed.WaitOne(1000, true) == false) && (_ResendCount < _MaxResendAttempts))
            {
                if (this.SecondsSinceLastPacket > 2)
                {
                    _Client.Log("Resending Packet (more than 2 seconds since last confirm)", Helpers.LogLevel.Info);
                    this.SendCurrentPacket();
                    _ResendCount++;
                }
            }


            if (_Status == RequestStatus.Failure)
            {
                throw new Exception(_StatusMsg);
            }
            else
            {
                return _TransactionID;
            }
        }

        protected void SendFirstPacket()
        {
            Packet packet;

            if (AssetBeingTransferd._AssetData.Length > 1000)
            {
                packet = AssetPacketHelpers.AssetUploadRequestHeaderOnly(AssetBeingTransferd, _TransactionID);
            }
            else
            {
                packet = AssetPacketHelpers.AssetUploadRequest(AssetBeingTransferd, _TransactionID);
            }

            _Client.Network.SendPacket(packet);

            #if DEBUG_PACKETS
                slClient.DebugLog(packet);
            #endif
            #if DEBUG_HEADERS
            _Client.DebugLog(packet.Header.ToString());
            #endif
        }

        internal void RequestXfer(ulong XferID)
        {
            _XferID = XferID; 
            // Setup to send the first packet
            SendCurrentPacket();
        }

        internal void ConfirmXferPacket(ulong XferID, uint PacketNumConfirmed)
        {
            // TODO should check that this is the same transfer?
            this.UpdateLastPacketTime();

            if (PacketNumConfirmed == _CurrentPacket)
            {
                // Increment Packet #
                this._CurrentPacket++;
                this._ResendCount = 0;
                SendCurrentPacket();
            }
            else
            {
                throw new Exception("Something is wrong with uploading assets, a confirmation came in for a packet we didn't send.");
            }
        }

        protected void SendCurrentPacket()
        {
            Packet uploadPacket;

            // THREADING: snapshot this num so we use a consistent value throughout
            uint packetNum = _CurrentPacket; 
            if (packetNum == 0)
            {
                if (AssetBeingTransferd._AssetData.Length <= 1000)
                {
                    throw new Exception("Should not use xfer for small assets");
                }
                int dataSize = 1000;

                byte[] packetData = new byte[dataSize + 4]; // Extra space is for leading data length bytes

                // Prefix the first Xfer packet with the data length
                // FIXME: Apply endianness patch
                Buffer.BlockCopy(BitConverter.GetBytes((int)AssetBeingTransferd._AssetData.Length), 0, packetData, 0, 4);
                Buffer.BlockCopy(AssetBeingTransferd._AssetData, 0, packetData, 4, dataSize);

                uploadPacket = AssetPacketHelpers.SendXferPacket(_XferID, packetData, packetNum);
            }
            else if (packetNum < _NumPackets2Send)
            {
                byte[] packetData = new byte[1000];
                Buffer.BlockCopy(AssetBeingTransferd._AssetData, (int)packetNum * 1000, packetData, 0, 1000);

                uploadPacket = AssetPacketHelpers.SendXferPacket(_XferID, packetData, packetNum);
            }
            else
            {
                // The last packet has to be handled slightly differently
                int lastLen = this.AssetBeingTransferd._AssetData.Length - (_NumPackets2Send * 1000);
                byte[] packetData = new byte[lastLen];
                Buffer.BlockCopy(this.AssetBeingTransferd._AssetData, _NumPackets2Send * 1000, packetData, 0, lastLen);

                uint lastPacket = (uint)int.MaxValue + (uint)_NumPackets2Send + (uint)1;
                uploadPacket = AssetPacketHelpers.SendXferPacket(_XferID, packetData, lastPacket);
            }

            _Client.Network.SendPacket(uploadPacket);

            #if DEBUG_PACKETS
                slClient.DebugLog(uploadPacket);
            #endif
            #if DEBUG_HEADERS
                _Client.DebugLog(uploadPacket.Header.ToString());
            #endif
        }

        internal void UploadComplete(LLUUID assetID, RequestStatus success)
        {
            AssetBeingTransferd.AssetID = assetID;
            UpdateLastPacketTime();
            _Client.Log("Upload complete", Helpers.LogLevel.Info);

            if (_Status == RequestStatus.Success)
            {
                MarkCompleted(success, "Success");
            }
            else
            {
                MarkCompleted(success, "Server returned failed");
            }

        }
    }
}
