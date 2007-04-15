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
using System.IO;
using System.Threading;

using libsecondlife;
using libsecondlife.Packets;

using libsecondlife.InventorySystem;

namespace libsecondlife.AssetSystem
{
    public delegate void ImageRetrievedCallback(LLUUID id, byte[] data, bool cached, string statusmsg); //this delegate is called when an image completed.

    /// <summary>
    /// Manages the uploading and downloading of Images from SecondLife
    /// </summary>
    public class ImageManager
    {

        private SecondLife slClient;

        public enum CacheTypes { None, Memory, Disk };
        private CacheTypes CacheType;
        private string CacheDirectory = "ImageCache";
        private Dictionary<LLUUID, Byte[]> CacheTable = new Dictionary<LLUUID, byte[]>();
        private List<LLUUID> CachedDiskIndex = new List<LLUUID>();

        private ImagePacketHelpers ImagePacketHelper;

        private Dictionary<LLUUID, TransferRequest> htDownloadRequests = new Dictionary<LLUUID, TransferRequest>();

        public ImageRetrievedCallback OnImageRetrieved;

        private class TransferRequest
        {
            public ManualResetEvent ReceivedHeaderPacket = new ManualResetEvent(false);
            public ManualResetEvent Completed = new ManualResetEvent(false);

            public bool Status;
            public string StatusMsg;

            public uint Size;
            public uint Received;
            public uint TimeOfLastPacket;
            public byte[] AssetData;

            public int BaseDataReceived;

            public TransferRequest()
            {
                Status = false;
                StatusMsg = "";

                AssetData = null;
                BaseDataReceived = 0;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="client"></param>
        public ImageManager(SecondLife client)
        {
            Init(client, CacheTypes.None, null);
        }

        /// <summary>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="ctype">The type of Cache system to use for images.</param>
        public ImageManager(SecondLife client, CacheTypes ctype)
        {
            Init(client, ctype, null);
        }

        /// <summary>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="ctype">The type of Cache system to use for images.</param>
        /// <param name="directory">The directory to use for disk based caching.</param>
        public ImageManager(SecondLife client, CacheTypes ctype, String directory)
        {
            Init(client, ctype, directory);
        }

        /// <summary>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="ctype">The type of Cache system to use for images.</param>
        /// <param name="directory">The directory to use for disk based caching.</param>
        private void Init(SecondLife client, CacheTypes ctype, string directory)
        {
            slClient = client;

            // Setup Image Caching
            CacheType = ctype;
            if (ctype == CacheTypes.Disk)
            {
                if (directory != null)
                {
                    CacheDirectory = directory;
                }

                try
                {
                    if (!Directory.Exists(CacheDirectory))
                    {
                        Directory.CreateDirectory(CacheDirectory);
                    }
                }
                catch (Exception e)
                {
                    slClient.Log("Disk Cache directory could not be established, defaulting to Memory Cache: " + Environment.NewLine +
                        e.ToString(), Helpers.LogLevel.Warning);

                    CacheType = CacheTypes.Memory;
                }
            }

            // Image Packet Helpers
            ImagePacketHelper = new ImagePacketHelpers(client);

            // Image Callbacks
            slClient.Network.RegisterCallback(PacketType.ImageData, new NetworkManager.PacketCallback(ImageDataCallbackHandler));
            slClient.Network.RegisterCallback(PacketType.ImagePacket, new NetworkManager.PacketCallback(ImagePacketCallbackHandler));
            slClient.Network.RegisterCallback(PacketType.ImageNotInDatabase, new NetworkManager.PacketCallback(ImageNotInDatabaseCallbackHandler));
        }

        private void CacheImage(LLUUID ImageID, byte[] ImageData)
        {
            switch (CacheType)
            {
                case CacheTypes.Memory:
                    CacheTable[ImageID] = ImageData;
                    break;
                case CacheTypes.Disk:
                    String filepath = Path.Combine(CacheDirectory, ImageID.ToStringHyphenated());
                    File.WriteAllBytes(filepath, ImageData);
                    CachedDiskIndex.Add(ImageID);
                    break;
                default:
                    break;
            }
        }

        private byte[] CachedImage(LLUUID ImageID)
        {
            switch (CacheType)
            {
                case CacheTypes.Memory:
                    if (CacheTable.ContainsKey(ImageID))
                    {
                        return CacheTable[ImageID];
                    }
                    else
                    {
                        return null;
                    }
                case CacheTypes.Disk:
                    String filepath = Path.Combine(CacheDirectory, ImageID.ToStringHyphenated());
                    if (File.Exists(filepath))
                    {
                        return File.ReadAllBytes(filepath);
                    }
                    else
                    {
                        return null;
                    }

                default:
                    return null;
            }
        }

        public bool isCachedImage(LLUUID ImageID)
        {
            if (ImageID == null)
            {
                throw new Exception("Don't go calling isCachedImage() with a null...");
            }

            switch (CacheType)
            {
                case CacheTypes.Memory:
                    return CacheTable.ContainsKey(ImageID);
                case CacheTypes.Disk:
                    if (CachedDiskIndex.Contains(ImageID))
                    {
                        return true;
                    }
                    else
                    {
                        String filepath = Path.Combine(CacheDirectory, ImageID.ToStringHyphenated());
                        if (File.Exists(filepath))
                        {
                            CachedDiskIndex.Add(ImageID);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                default:
                    return false;
            }
        }

        public bool isDownloadingImages()
        {
            bool isDownloading = false;
            lock (htDownloadRequests)
            {
                isDownloading = htDownloadRequests.Count > 0 ? true : false;
            }
            return isDownloading;
        }

        /// <summary>
        /// Requests an image from SecondLife and blocks until it's received.
        /// </summary>
        /// <param name="ImageID">The Image's AssetID</param>
        public byte[] RequestImage(LLUUID ImageID)
        {
            byte[] imgData = CachedImage(ImageID);
            if (imgData != null)
            {
                return imgData;
            }

            TransferRequest tr;
            lock (htDownloadRequests)
            {
                if (htDownloadRequests.ContainsKey(ImageID) == false)
                {
                    tr = new TransferRequest();
                    tr.Size = int.MaxValue; // Number of bytes expected
                    tr.Received = 0; // Number of bytes received
                    tr.TimeOfLastPacket = Helpers.GetUnixTime(); // last time we recevied a packet for this request

                    htDownloadRequests[ImageID] = tr;

                    Packet packet = ImagePacketHelper.RequestImage(ImageID);
                    slClient.Network.SendPacket(packet);
                }
                else
                {
                    tr = htDownloadRequests[ImageID];
                }
            }

            // Wait for transfer to complete.
            while( !tr.Completed.WaitOne(10000, false) ) //If it times out, then check, otherwise loop again until WaitOne returns true
            {
                slClient.Log("Warning long running texture download: " + ImageID.ToStringHyphenated(), Helpers.LogLevel.Warning);
                Console.WriteLine("Downloaded : " + tr.Received);
                if( (Helpers.GetUnixTime() - tr.TimeOfLastPacket) > 10 )
                {
                    tr.Status = false;
                    tr.StatusMsg = "Timeout while downloading image.";
                    slClient.Log(tr.StatusMsg, Helpers.LogLevel.Error);
                    tr.Completed.Set();
                }
            }

            if (tr.Status == true)
            {
                return tr.AssetData;
            }
            else
            {
                throw new Exception("RequestImage: " + tr.StatusMsg);
            }
        }

        /// <summary>
        /// Requests an image from SecondLife.
        /// </summary>
        /// <param name="ImageID">The Image's AssetID</param>
        public void RequestImageAsync(LLUUID ImageID)
        {
            if (ImageID == null)
            {
                throw new Exception("WTF!!!  Don't request Image Assets by passing in an ImageID of null");
            }

            byte[] imgData = CachedImage(ImageID);
            if (imgData != null)
            {
                FireImageRetrieved(ImageID, imgData, true);
            }

            lock (htDownloadRequests)
            {
                if (htDownloadRequests.ContainsKey(ImageID) == false)
                {
                    TransferRequest tr = new TransferRequest();
                    tr.Size = int.MaxValue; // Number of bytes expected
                    tr.Received = 0; // Number of bytes received
                    tr.TimeOfLastPacket = Helpers.GetUnixTime(); // last time we recevied a packet for this request

                    htDownloadRequests[ImageID] = tr;

                    Packet packet = ImagePacketHelper.RequestImage(ImageID);
                    slClient.Network.SendPacket(packet);
                }
            }
        }


        /// <summary>
        /// Handles the Image Data packet, which includes the ID, and Size of the image, 
        /// along with the first block of data for the image.  If the image is small enough
        /// there will be no additional packets.
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="simulator"></param>
        public void ImageDataCallbackHandler(Packet packet, Simulator simulator)
        {
#if DEBUG_PACKETS
                slClient.DebugLog(packet);
#endif

            ImageDataPacket reply = (ImageDataPacket)packet;

            LLUUID ImageID = reply.ImageID.ID;
            // unused?		ushort Packets = reply.ImageID.Packets;
            uint Size = reply.ImageID.Size;
            byte[] Data = reply.ImageData.Data;

            // Lookup the request that this packet is for
            TransferRequest tr;
            lock (htDownloadRequests)
            {
                if( htDownloadRequests.ContainsKey(ImageID) )
                {
                    tr = htDownloadRequests[ImageID];
                } else {
                    // Received a packet for an image we didn't request...
                    return;
                }
            }

            // Initialize the request so that it's data buffer is the right size for the image
            tr.Size = Size;
            tr.AssetData = new byte[tr.Size];
            tr.BaseDataReceived = Data.Length;

            // Copy the first block of image data into the request.
            Buffer.BlockCopy(Data, 0, tr.AssetData, (int)tr.Received, Data.Length);
            tr.Received += (uint)Data.Length;

            // Mark that the TransferRequest has received this header packet
            tr.ReceivedHeaderPacket.Set();

            tr.TimeOfLastPacket = Helpers.GetUnixTime(); // last time we recevied a packet for this request

            // If we've gotten all the data, mark it completed.
            if (tr.Received >= tr.Size)
            {
                tr.Status = true;
                tr.Completed.Set();

                // Fire off image downloaded event
                CacheImage(ImageID, tr.AssetData);
                FireImageRetrieved(ImageID, tr.AssetData, false);
            }
        }

        /// <summary>
        /// Handles the remaining Image data that did not fit in the initial ImageData packet
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="simulator"></param>
        public void ImagePacketCallbackHandler(Packet packet, Simulator simulator)
        {
#if DEBUG_PACKETS
                slClient.DebugLog(packet);
#endif

            ImagePacketPacket reply = (ImagePacketPacket)packet;

            LLUUID ImageID = reply.ImageID.ID;

            // Lookup the request for this packet
            TransferRequest tr = null;
            lock (htDownloadRequests)
            {
                if (htDownloadRequests.ContainsKey(ImageID))
                {
                    tr = (TransferRequest)htDownloadRequests[ImageID];
                }
                else
                {
                    // Received a packet that doesn't belong to any requests in our queue, strange...
                    return;
                }
            }


            // TODO: Received data should probably be put into a temporary collection that's indected by ImageID.Packet
            // then once we've received all data packets, it should be re-assembled into a complete array and marked
            // completed.

            // FIXME: Sometimes this gets called before ImageDataCallbackHandler, when that
            // happens tr.AssetData will be null.  Implimenting the above TODO should fix this.

            // Wait until we've received the header packet for this image, which creates the AssetData array
            if (!tr.ReceivedHeaderPacket.WaitOne(15000,false))
            {
                tr.Status = false;
                tr.StatusMsg = "Failed to receive Image Header packet in a timely manor, aborting.";
                slClient.Log(tr.StatusMsg, Helpers.LogLevel.Error);
                tr.Completed.Set();
            }

            // Add this packet's data to the request.
            Buffer.BlockCopy(reply.ImageData.Data, 0, tr.AssetData, tr.BaseDataReceived + (1000 * (reply.ImageID.Packet - 1)), reply.ImageData.Data.Length);
            tr.Received += (uint)reply.ImageData.Data.Length;

            tr.TimeOfLastPacket = Helpers.GetUnixTime(); // last time we recevied a packet for this request

            // If we've gotten all the data, mark it completed.
            if (tr.Received >= tr.Size)
            {
                tr.Status = true;
                tr.Completed.Set();

                // Fire off image downloaded event
                CacheImage(ImageID, tr.AssetData);
                FireImageRetrieved(ImageID, tr.AssetData, false);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void ImageNotInDatabaseCallbackHandler(Packet packet, Simulator simulator)
        {
#if DEBUG_PACKETS
                slClient.DebugLog(packet);
#endif

            ImageNotInDatabasePacket reply = (ImageNotInDatabasePacket)packet;

            LLUUID ImageID = reply.ImageID.ID;

            // Lookup the request for this packet
            TransferRequest tr = null;
            lock (htDownloadRequests)
            {
                if (htDownloadRequests.ContainsKey(ImageID))
                {
                    tr = (TransferRequest)htDownloadRequests[ImageID];
                }
                else
                {
                    // Received a packet that doesn't belong to any requests in our queue, strange...
                    return;
                }
            }

            tr.Status = false;
            tr.StatusMsg = "Image not in database";
            tr.Completed.Set();

            // Fire off image downloaded event
            FireImageRetrieved(ImageID, null, false, tr.StatusMsg);
        }

        protected void FireImageRetrieved(LLUUID ImageID, byte[] ImageData, bool cached)
        {
            FireImageRetrieved(ImageID, ImageData, cached, "");
        }

        protected void FireImageRetrieved(LLUUID ImageID, byte[] ImageData, bool cached, string status)
        {
            if (OnImageRetrieved != null)
            {
                OnImageRetrieved(ImageID, ImageData, cached, status);
            }
        }

    }
}