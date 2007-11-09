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
    public class AssetRequest
    {
        public enum RequestStatus { Success, Failure };

        protected AssetManager _AssetManager;

        protected LLUUID _TransactionID;
        public Asset AssetBeingTransferd;
        

        protected ManualResetEvent _Completed = new ManualResetEvent(false);

        protected RequestStatus _Status;
        public RequestStatus Status
        {
            get { return _Status; }
        }

        protected string _StatusMsg = "";
        public string StatusMsg
        {
            get { return _StatusMsg; }
        }

        protected int _Size;
        protected byte[] _AssetData;

        protected uint _LastPacketTime;
        public uint LastPacketTime { get { return _LastPacketTime; } }

        public uint SecondsSinceLastPacket { get { return Helpers.GetUnixTime() - _LastPacketTime; } }

        public AssetRequest(AssetManager Manager, LLUUID TransID, Asset Asset)
        {
            _AssetManager = Manager;
            _TransactionID = TransID;
            AssetBeingTransferd = Asset;

            _Size = int.MaxValue;

            UpdateLastPacketTime();
        }

        public AssetRequest(AssetManager Manager, LLUUID TransID)
        {
            _AssetManager = Manager;
            _TransactionID = TransID;
            AssetBeingTransferd = null;

            _Size = int.MaxValue;

            UpdateLastPacketTime();
        }


        public void UpdateLastPacketTime()
        {
            _LastPacketTime = Helpers.GetUnixTime();
        }

        /// <summary>
        /// Wait for this Request to be completed
        /// </summary>
        /// <param name="timeout">milliseconds to wait for next packet in download, -1 to wait indefinitely</param>
        /// <returns></returns>
        public RequestStatus Wait(int timeout)
        {
            return Wait(-1, timeout);
        }

        /// <summary>
        /// Wait for this Request to be completed.
        /// </summary>
        /// <remarks>Hard Timeout should only be specified if you really can't wait for the download, even though it's still going and may be successful.</remarks>
        /// <param name="hardTimeout">Return after hardTimeout milliseconds even if download is still in progress, -1 to wait indefinitely</param>
        /// <param name="softTimeout">How long to wait, before deciding download is stalled, -1 to wait indefinitely</param>
        /// <returns></returns>
        public RequestStatus Wait(int hardTimeout, int softTimeout)
        {
            UpdateLastPacketTime();

//            uint TimeStarted = LastPacketTime;

            while (!_Completed.WaitOne(900, false))
            {
                if ((hardTimeout != -1) && (SecondsSinceLastPacket > hardTimeout))
                {
                    _StatusMsg += "Timeout Failure - Hard timeout reached (" + SecondsSinceLastPacket + " > " + hardTimeout + ")";
                    return RequestStatus.Failure;
                }
                else
                {
                    if ((softTimeout != -1) && (SecondsSinceLastPacket > softTimeout))
                    {
                        _StatusMsg += "Timeout Failure - Soft Timeout ( " + SecondsSinceLastPacket + " > " + softTimeout + ")";
                        return RequestStatus.Failure;
                    }
                }
            }

            return _Status;
        }

        protected void MarkCompleted(RequestStatus status, string status_msg)
        {
            _StatusMsg += status_msg;
            _Status = status;

            _Completed.Set();

            _AssetManager.FireTransferRequestCompletedEvent(this);
        }

        internal void Fail(string status_msg)
        {
            MarkCompleted(RequestStatus.Failure, status_msg);
        }

    }


    public class AssetRequestDownload : AssetRequest
    {
        protected int _Received;
        protected SortedList<int, byte[]> _AssetDataReceived = new SortedList<int, byte[]>();
        protected LLUUID _AssetID;
        public LLUUID AssetID
        {
            get { return _AssetID; }
        }

        public AssetRequestDownload(AssetManager Manager, LLUUID TransID, LLUUID AssetID)
            : base(Manager, TransID)
        {
            _Received = 0;
            _AssetID = AssetID;
        }

        internal void AddDownloadedData(int packetNum, byte[] data)
        {
            if (!_AssetDataReceived.ContainsKey(packetNum))
            {
                _AssetDataReceived[packetNum] = data;
                _Received += data.Length;
            }

            // If we've gotten all the data, mark it completed.
            if (_Received >= _Size)
            {
                int curPos = 0;
                foreach (KeyValuePair<int, byte[]> kvp in _AssetDataReceived)
                {
                    Buffer.BlockCopy(kvp.Value, 0, _AssetData, curPos, kvp.Value.Length);
                    curPos += kvp.Value.Length;
                }

                MarkCompleted(AssetRequestDownload.RequestStatus.Success, "Download Completed");
            }

        }

        internal void SetExpectedSize(int size)
        {
            _Size = size;
            _AssetData = new byte[_Size];
        }

        /// <summary>
        /// Get the asset data downloaded by this request.
        /// </summary>
        /// <returns></returns>
        public byte[] GetAssetData()
        {
            return _AssetData;
        }
    }
}
