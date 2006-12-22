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
    class AssetRequestUpload
    {
        public ManualResetEvent Completed = new ManualResetEvent(false);
		public bool Status;
		public string StatusMsg;

        public Asset MyAsset;

        public LLUUID TransactionID;
        public ulong XferID;

        SecondLife slClient;

        public int resendCount;
        public uint CurrentPacket;
        private uint _LastPacketTime;
        public uint LastPacketTime
        {
            get { return _LastPacketTime; }
        }

        public uint SecondsSinceLastPacket
        {
            get { return Helpers.GetUnixTime() - _LastPacketTime; }
        }

        private int _NumPackets; 
        public int NumPackets { get { return _NumPackets; } }

        public AssetRequestUpload(SecondLife slClient, LLUUID TransID, Asset asset)
        {
            this.slClient = slClient;
            TransactionID = TransID;
            UpdateLastPacketTime();

            MyAsset = asset;

            CurrentPacket = 0;
            resendCount = 0;
            _NumPackets = asset._AssetData.Length / 1000;
            if (_NumPackets < 1)
            {
                _NumPackets = 1;
            }
        }

        internal LLUUID DoUpload()
        {
            this.SendFirstPacket();

            while (this.Completed.WaitOne(1000, true) == false && this.resendCount < 20) // only resend 20 times
            {
                if (this.SecondsSinceLastPacket > 2)
                {
                    slClient.Log("Resending Packet (more than 2 seconds since last confirm)", Helpers.LogLevel.Info);
                    this.SendCurrentPacket();
                    resendCount++;
                }
            }


            if (this.Status == false)
            {
                throw new Exception(this.StatusMsg);
            }
            else
            {
                return this.TransactionID;
            }
        }

        public void UpdateLastPacketTime()
        {
            _LastPacketTime = Helpers.GetUnixTime();
        }


        internal void SendFirstPacket()
        {
            Packet packet;

            if (this.MyAsset._AssetData.Length > 1000)
            {
                packet = AssetPacketHelpers.AssetUploadRequestHeaderOnly(this.MyAsset, this.TransactionID);
            }
            else
            {
                packet = AssetPacketHelpers.AssetUploadRequest(this.MyAsset, this.TransactionID);
            }

            slClient.Network.SendPacket(packet);

            #if DEBUG_PACKETS
                slClient.DebugLog(packet);
            #endif
            #if DEBUG_HEADERS
            slClient.DebugLog(packet.Header.ToString());
            #endif
        }

        internal void RequestXfer(ulong XferID)
        {
            this.XferID = XferID; 
            // Setup to send the first packet
            SendCurrentPacket();
        }

        internal void ConfirmXferPacket(ulong XferID, uint PacketNumConfirmed)
        {
            // TODO should check that this is the same transfer?
            this.UpdateLastPacketTime();

            if (PacketNumConfirmed == CurrentPacket)
            {
                // Increment Packet #
                this.CurrentPacket++;
                this.resendCount = 0;
                SendCurrentPacket();
            }
            else
            {
                throw new Exception("Something is wrong with uploading assets, a confirmation came in for a packet we didn't send.");
            }
        }

        private void SendCurrentPacket()
        {
            Packet uploadPacket;

            // technically we don't need this lock, because no state is updated here!
            // lock (this) 
            {
                // THREADING: snapshot this num so we use a consistent value throughout
                uint packetNum = CurrentPacket; 
                if (packetNum == 0)
                {
                    if (MyAsset._AssetData.Length <= 1000)
                        throw new Exception("Should not use xfer for small assets");
                    int dataSize = 1000;

                    byte[] packetData = new byte[dataSize + 4]; // Extra space is for leading data length bytes

                    // Prefix the first Xfer packet with the data length
                    // FIXME: Apply endianness patch
                    Array.Copy(BitConverter.GetBytes((int)MyAsset._AssetData.Length), 0, packetData, 0, 4);
                    Array.Copy(MyAsset._AssetData, 0, packetData, 4, dataSize);

                    uploadPacket = AssetPacketHelpers.SendXferPacket(XferID, packetData, packetNum);
                }
                else if (packetNum < this.NumPackets)
                {
                    byte[] packetData = new byte[1000];
                    Array.Copy(this.MyAsset._AssetData, packetNum * 1000, packetData, 0, 1000);

                    uploadPacket = AssetPacketHelpers.SendXferPacket(this.XferID, packetData, packetNum);
                }
                else
                {
                    // The last packet has to be handled slightly differently
                    int lastLen = this.MyAsset._AssetData.Length - (this.NumPackets * 1000);
                    byte[] packetData = new byte[lastLen];
                    Array.Copy(this.MyAsset._AssetData, this.NumPackets * 1000, packetData, 0, lastLen);

                    uint lastPacket = (uint)int.MaxValue + (uint)this.NumPackets + (uint)1;
                    uploadPacket = AssetPacketHelpers.SendXferPacket(this.XferID, packetData, lastPacket);
                }
            }

            slClient.Network.SendPacket(uploadPacket);

            #if DEBUG_PACKETS
                slClient.DebugLog(uploadPacket);
            #endif
            #if DEBUG_HEADERS
                slClient.DebugLog(uploadPacket.Header.ToString());
            #endif
        }

        internal void UploadComplete(LLUUID assetID, bool success)
        {
            MyAsset.AssetID = assetID;
            this.Status = success;
            UpdateLastPacketTime();

            if (Status)
                StatusMsg = "Success";
            else
                StatusMsg = "Server returned failed";

            slClient.Log("Upload complete", Helpers.LogLevel.Info);
            Completed.Set();
        }
    }
}
