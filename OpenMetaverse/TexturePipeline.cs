/*
 * Copyright (c) 2008, openmetaverse.org
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without 
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.org nor the names 
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
using System.Threading;
using OpenMetaverse.Packets;

namespace OpenMetaverse
{
    public enum TextureRequestState
    {
        Pending,
        Started,
        Progress,
        Finished,
        Timeout,
        Aborted,
        NotFound
    }

    public delegate void TextureDownloadCallback(TextureRequestState state, ImageDownload imageData, AssetTexture assetTexture);
    /// <summary>
    /// Texture request download handler, allows a configurable number of download slots
    /// </summary>
    public class TexturePipeline
    {
        class TaskInfo
        {
            public TextureRequestState State;
            public UUID RequestID;
            public int RequestNbr;
            public ImageType Type;
            public TextureDownloadCallback Callback;
            public bool ReportProgress;

            public ImageDownload Transfer;

            public object syncItem;

            /// <summary>
            /// Constructor
            /// </summary>
            public TaskInfo()
            {
                // create a locking object so callbacks can
                // lock individual entries in the dictionary
                syncItem = new object();
            }
        }



        private Dictionary<UUID, TaskInfo> _Transfers;

        private GridClient client;
        /// <summary>Maximum concurrent texture requests</summary>
        private readonly int maxTextureRequests;

        private AutoResetEvent[] resetEvents;
        private int[] threadpoolSlots;
        private Thread downloadMaster;
        bool running;

        private static object lockerObject = new object();

        private System.Timers.Timer RefreshDownloadsTimer = new System.Timers.Timer(500.0);

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="client">Reference to <code>SecondLife</code> client</param>
        /// <param name="maxRequests">Maximum number of concurrent texture requests</param>
        public TexturePipeline(GridClient client)
        {
            running = true;
            this.client = client;
            maxTextureRequests = client.Settings.MAX_CONCURRENT_TEXTURE_DOWNLOADS;

            resetEvents = new AutoResetEvent[maxTextureRequests];
            threadpoolSlots = new int[maxTextureRequests];

            _Transfers = new Dictionary<UUID, TaskInfo>();
            // Pre-configure autoreset events/download slots
            for (int i = 0; i < maxTextureRequests; i++)
            {
                resetEvents[i] = new AutoResetEvent(false);
                threadpoolSlots[i] = -1;
            }

            client.Network.RegisterCallback(PacketType.ImageData, ImageDataHandler);
            client.Network.RegisterCallback(PacketType.ImagePacket, ImagePacketHandler);
            client.Network.RegisterCallback(PacketType.ImageNotInDatabase, ImageNotInDatabaseHandler);

            // Fire up the texture download thread
            downloadMaster = new Thread(new ThreadStart(DownloadThread));
            downloadMaster.Name = "TexturePipeline";
            downloadMaster.IsBackground = true;
            downloadMaster.Start();

            // HACK: Re-request stale pending image downloads
            RefreshDownloadsTimer.Elapsed += RefreshDownloadsTimer_Elapsed;
            RefreshDownloadsTimer.Start();

        }

        /// <summary>
        /// 
        /// </summary>
        public void Shutdown()
        {
            RefreshDownloadsTimer.Stop();

            client.Network.UnregisterCallback(PacketType.ImageNotInDatabase, ImageNotInDatabaseHandler);
            client.Network.UnregisterCallback(PacketType.ImageData, ImageDataHandler);
            client.Network.UnregisterCallback(PacketType.ImagePacket, ImagePacketHandler);
            
            lock (_Transfers)
                _Transfers.Clear();

            for (int i = 0; i < resetEvents.Length; i++)
                if (resetEvents[i] != null)
                    resetEvents[i].Set();

            running = false;
        }

        private void RefreshDownloadsTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (_Transfers)
            {
                foreach (TaskInfo transfer in _Transfers.Values)
                {
                    if (transfer.State == TextureRequestState.Progress)
                    {
                        ImageDownload download = transfer.Transfer;

                        uint packet = 0;

                        if (download.PacketsSeen != null && download.PacketsSeen.Count > 0)
                        {
                            lock (download.PacketsSeen)
                            {
                                bool first = true;
                                foreach (KeyValuePair<ushort, ushort> packetSeen in download.PacketsSeen)
                                {
                                    if (first)
                                    {
                                        // Initially set this to the earliest packet received in the transfer
                                        packet = packetSeen.Value;
                                        first = false;
                                    }
                                    else
                                    {
                                        ++packet;

                                        // If there is a missing packet in the list, break and request the download
                                        // resume here
                                        if (packetSeen.Value != packet)
                                        {
                                            --packet;
                                            break;
                                        }
                                    }
                                }

                                ++packet;
                            }
                        }

                        if (download.TimeSinceLastPacket > 25000)
                        {
                            if (download.DiscardLevel > 0)
                            {
                                --download.DiscardLevel;
                            }
                            download.TimeSinceLastPacket = 0;
                            RequestImage(download.ID, download.ImageType, download.Priority, download.DiscardLevel, packet);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Request a texture asset from the grid. Texture data is sent to the callback
        /// as it is recieved from the simulator for clients that wish to do progressive
        /// texture rendering.
        /// </summary>
        /// <param name="textureID"></param>
        /// <param name="imageType"></param>
        /// <param name="callback"></param>
        /// <param name="progressive"></param>
        public void RequestTexture(UUID textureID, ImageType imageType, TextureDownloadCallback callback, bool progressive)
        {
            if (textureID == UUID.Zero)
                return;

            if (callback != null)
            {
                if (client.Assets.Cache.HasImage(textureID))
                {
                    ImageDownload image = new ImageDownload();
                    image.ID = textureID;
                    image.AssetData = client.Assets.Cache.GetCachedImageBytes(textureID);
                    image.Size = image.AssetData.Length;
                    image.Transferred = image.AssetData.Length;
                    image.ImageType = imageType;
                    image.AssetType = AssetType.Texture;
                    image.Success = true;

                    AssetTexture asset = new AssetTexture(image.ID, image.AssetData);

                    callback(TextureRequestState.Finished, image, asset);
                }
                else
                {
                    lock (_Transfers)
                    {
                        if (!_Transfers.ContainsKey(textureID))
                        {
                            TaskInfo request = new TaskInfo();
                            request.State = TextureRequestState.Pending;
                            request.RequestID = textureID;
                            request.ReportProgress = progressive;
                            request.RequestNbr = -1;
                            request.Type = imageType;
                            request.Callback = callback;
                            //request.Transfer = new ImageDownload();
                            //request.Transfer.ID = textureID;
                            Console.WriteLine("Add Download Request: {0}", textureID);
                            _Transfers.Add(textureID, request);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="imageID">The image to download</param>
        /// <param name="type">Type of the image to download, either a baked
        /// avatar texture or a normal texture</param>
        /// <param name="priority">Priority level of the download. Default is
        /// <c>1,013,000.0f</c></param>
        /// <param name="discardLevel">Number of quality layers to discard.
        /// This controls the end marker of the data sent</param>
        /// <param name="packetNum">Packet number to start the download at.
        /// This controls the start marker of the data sent</param>
        /// <remarks>Sending a priority of 0 and a discardlevel of -1 aborts
        /// download</remarks>
        private void RequestImage(UUID imageID, ImageType type, float priority, int discardLevel, uint packetNum)
        {

            // Priority == 0 && DiscardLevel == -1 means cancel the transfer
            if (priority.Equals(0) && discardLevel.Equals(-1))
            {
                AbortDownload(imageID);
            }
            else
            {
                if(_Transfers.ContainsKey(imageID) && _Transfers[imageID].Transfer != null)
                {
                    // Already downloading, just updating the priority
                    Transfer transfer = _Transfers[imageID].Transfer;
                    float percentComplete = ((float)transfer.Transferred / (float)transfer.Size) * 100f;
                    if (Single.IsNaN(percentComplete))
                        percentComplete = 0f;

                    //Logger.DebugLog(String.Format("Updating priority on image transfer {0}, {1}% complete",
                    //    imageID, Math.Round(percentComplete, 2)));
                } 
                else
                {
                    ImageDownload transfer = new ImageDownload();
                    transfer.ID = imageID;
                    transfer.Simulator = client.Network.CurrentSim;
                    transfer.ImageType = type;
                    transfer.DiscardLevel = discardLevel;
                    transfer.Priority = priority;

                    lock (_Transfers) _Transfers[transfer.ID].Transfer = transfer;                    
                } 

                // Build and send the request packet
                RequestImagePacket request = new RequestImagePacket();
                request.AgentData.AgentID = client.Self.AgentID;
                request.AgentData.SessionID = client.Self.SessionID;
                request.RequestImage = new RequestImagePacket.RequestImageBlock[1];
                request.RequestImage[0] = new RequestImagePacket.RequestImageBlock();
                request.RequestImage[0].DiscardLevel = (sbyte)discardLevel;
                request.RequestImage[0].DownloadPriority = priority;
                request.RequestImage[0].Packet = packetNum;
                request.RequestImage[0].Image = imageID;
                request.RequestImage[0].Type = (byte)type;

                client.Network.SendPacket(request, client.Network.CurrentSim);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="textureID"></param>
        public void AbortDownload(UUID textureID)
        {
            lock (_Transfers)
            {
                if (_Transfers.ContainsKey(textureID))
                {
                    TaskInfo task = _Transfers[textureID];

                    // this means we've actually got the request assigned to the threadpool
                    if (task.State == TextureRequestState.Progress)
                    {
                        RequestImagePacket request = new RequestImagePacket();
                        request.AgentData.AgentID = client.Self.AgentID;
                        request.AgentData.SessionID = client.Self.SessionID;
                        request.RequestImage = new RequestImagePacket.RequestImageBlock[1];
                        request.RequestImage[0] = new RequestImagePacket.RequestImageBlock();
                        request.RequestImage[0].DiscardLevel = -1;
                        request.RequestImage[0].DownloadPriority = 0;
                        request.RequestImage[0].Packet = 0;
                        request.RequestImage[0].Image = textureID;
                        request.RequestImage[0].Type = (byte)task.Type;
                        client.Network.SendPacket(request);

                        task.Transfer.Success = false;
                        task.Transfer.ID = textureID;

                        task.Callback(TextureRequestState.Aborted, task.Transfer, null);

                        resetEvents[task.RequestNbr].Set();

                        _Transfers.Remove(textureID);
                    }
                    else
                    {
                        _Transfers.Remove(textureID);
                        task.Transfer.Success = false;
                        task.Transfer.ID = textureID;
                        task.Callback(TextureRequestState.Aborted, task.Transfer, null);
                    }
                }
            }
        }

        /// <summary>
        /// Master Download Thread, Queues up downloads in the threadpool
        /// </summary>
        private void DownloadThread()
        {
            int slot;

            while (running)
            {
                // find free slots
                int pending = 0;
                int active = 0;
                lock (_Transfers)
                {

                    foreach (UUID request in _Transfers.Keys)
                    {
                        if (_Transfers[request].State == TextureRequestState.Pending)
                            pending++;

                        if (_Transfers[request].State == TextureRequestState.Progress)
                            active++;
                    }

                    Console.WriteLine("Pending: {0} Active {1}", pending, active);

                    if (pending > 0 && active <= maxTextureRequests)
                    {
                        slot = -1;
                        // find available slot for reset event
                        lock (lockerObject)
                        {
                            for (int i = 0; i < threadpoolSlots.Length; i++)
                            {
                                if (threadpoolSlots[i] == -1)
                                {
                                    // found a free slot
                                    threadpoolSlots[i] = 1;
                                    slot = i;
                                    break;
                                }
                            }
                            for (int i = 0; i < threadpoolSlots.Length; i++)
                                Console.WriteLine("Slot {0} = {1}", i, threadpoolSlots[i]);
                            Console.WriteLine("Slot {0}", slot);
                        }

                        if (slot != -1)
                        {
                            TaskInfo task = null;

                            if (pending > 0)
                            {
                                foreach (UUID request in _Transfers.Keys)
                                {
                                    if (_Transfers[request].State == TextureRequestState.Pending)
                                    {
                                        _Transfers[request].State = TextureRequestState.Started;
                                        task = _Transfers[request];
                                        break;
                                    }
                                }
                            }

                            if (task != null)
                            {
                                task.RequestNbr = slot;
                                task.Transfer = new ImageDownload();
                                task.Transfer.ID = task.RequestID;

                                Logger.DebugLog(String.Format("Sending Worker thread new download request {0}", slot));
                                ThreadPool.QueueUserWorkItem(TextureRequestDoWork, task);
                                continue;
                            }
                        }
                    }
                } // release the lock on the _Transfers dictionary

                // Queue was empty or all download slots are inuse, let's give up some CPU time
                Thread.Sleep(500);
            }

            Logger.Log("Texture pipeline shutting down", Helpers.LogLevel.Info);
        }


        private void TextureRequestDoWork(Object threadContext)
        {
            TaskInfo ti = (TaskInfo) threadContext;
            ti.State = TextureRequestState.Progress;

            Logger.DebugLog(String.Format("Worker {0} Requesting {1}", ti.RequestNbr, ti.RequestID));

            int inuse = 0;
            int freeslots = 0;
            lock (lockerObject)
            {
                for (int i = 0; i < threadpoolSlots.Length; i++)
                {
                    if (threadpoolSlots[i] >= 0)
                        inuse++;
                    else
                        freeslots++;
                }
            }

            Console.WriteLine("Slots used={0}, free={1} of total={2} (overall {3})", inuse, freeslots, threadpoolSlots.Length, _Transfers.Count);

            resetEvents[ti.RequestNbr].Reset();
            RequestImage(ti.RequestID, ti.Type, 1013000.0f, 0, 0);

            // don't release this worker slot until texture is downloaded or timeout occurs
            if (!resetEvents[ti.RequestNbr].WaitOne(45 * 1000, false))
            {
                // Timed out
                Logger.Log("Worker " + ti.RequestNbr + " Timeout waiting for Texture " + ti.RequestID + " to Download Got " + ti.Transfer.Transferred + " of " + ti.Transfer.Size, Helpers.LogLevel.Warning);

                ti.Transfer.Success = false;
                ti.Callback(TextureRequestState.Timeout, ti.Transfer, null);

                lock (_Transfers)
                    _Transfers.Remove(ti.RequestID);
            }

            // free up this download slot
            lock(lockerObject)
                threadpoolSlots[ti.RequestNbr] = -1;
        }

        #region Raw Packet Handlers

        private void ImageNotInDatabaseHandler(Packet packet, Simulator simulator)
        {
            ImageNotInDatabasePacket p = (ImageNotInDatabasePacket)packet;
            // remove from queue and fire the callback
            lock (_Transfers)
            {
                if (_Transfers.ContainsKey(p.ImageID.ID))
                {
                    // cancel acive request
                    TaskInfo task = _Transfers[p.ImageID.ID];
                    if (task.State == TextureRequestState.Progress)
                    {
                        resetEvents[task.RequestNbr].Set();
                    }


                    // fire callback
                    task.Transfer.NotFound = true;
                    task.Transfer.Success = false;
                    task.Transfer.ID = task.RequestID;

                    task.Callback(TextureRequestState.NotFound, task.Transfer, null);

                    resetEvents[task.RequestNbr].Set();

                    _Transfers.Remove(p.ImageID.ID);

                }
            }
        }

        /// <summary>
        /// Handles the remaining Image data that did not fit in the initial ImageData packet
        /// </summary>
        private void ImagePacketHandler(Packet packet, Simulator simulator)
        {
            ImagePacketPacket image = (ImagePacketPacket)packet;
            //ImageDownload transfer = null;
            TaskInfo task = null;

            lock (_Transfers)
            {
                if (_Transfers.ContainsKey(image.ImageID.ID))
                {
                    task = _Transfers[image.ImageID.ID];

                    if (task.Transfer.Size == 0)
                    {
                        // We haven't received the header yet, block until it's received or times out
                        task.Transfer.HeaderReceivedEvent.WaitOne(1000 * 5, false);

                        if (task.Transfer.Size == 0)
                        {
                            Logger.Log("Timed out while waiting for the image header to download for " +
                                task.Transfer.ID.ToString(), Helpers.LogLevel.Warning, client);

                            task.Transfer.Success = false;
                            _Transfers.Remove(task.Transfer.ID);
                            resetEvents[task.RequestNbr].Set(); // free up request slot
                            task.Callback(TextureRequestState.Timeout, task.Transfer, null);
                            return;
                        }
                    }

                    // The header is downloaded, we can insert this data in to the proper position
                    // Only insert if we haven't seen this packet before
                    lock (task.Transfer.PacketsSeen)
                    {
                        if (!task.Transfer.PacketsSeen.ContainsKey(image.ImageID.Packet))
                        {
                            task.Transfer.PacketsSeen[image.ImageID.Packet] = image.ImageID.Packet;
                            Buffer.BlockCopy(image.ImageData.Data, 0, task.Transfer.AssetData,
                                task.Transfer.InitialDataSize + (1000 * (image.ImageID.Packet - 1)),
                                image.ImageData.Data.Length);
                            task.Transfer.Transferred += image.ImageData.Data.Length;
                        }
                    }

                    task.Transfer.TimeSinceLastPacket = 0;
                    resetEvents[task.RequestNbr].Reset();
                }
            }

            if (task != null)
            {
                if (task.Transfer.Transferred >= task.Transfer.Size)
                {

                    task.Transfer.Success = true;
                    _Transfers.Remove(task.Transfer.ID);
                    resetEvents[task.RequestNbr].Set(); // free up request slot
                    client.Assets.Cache.SaveImageToCache(task.RequestID, task.Transfer.AssetData);
                    AssetTexture asset = new AssetTexture(task.RequestID, task.Transfer.AssetData);
                    task.Callback(TextureRequestState.Finished, task.Transfer, asset);
                }
                else
                {
                    if (task.ReportProgress)
                        task.Callback(TextureRequestState.Progress, task.Transfer, null);
                }
            }
        }

        private void ImageDataHandler(Packet packet, Simulator simulator)
        {
            ImageDataPacket data = (ImageDataPacket)packet;
            TaskInfo task = null;

            //Logger.DebugLog(String.Format("ImageData: Size={0}, Packets={1}", data.ImageID.Size, data.ImageID.Packets));

            lock (_Transfers)
            {
                if (_Transfers.ContainsKey(data.ImageID.ID))
                {
                    task = _Transfers[data.ImageID.ID];
                    //Monitor.Enter(task.syncItem);
                }


                if (task != null)
                {
                    // reset the timeout interval since we got data
                    resetEvents[task.RequestNbr].Reset();
                    if (task.Transfer.Size == 0)
                    {
                        task.Transfer.Codec = (ImageCodec)data.ImageID.Codec;
                        task.Transfer.PacketCount = data.ImageID.Packets;
                        task.Transfer.Size = (int)data.ImageID.Size;
                        task.Transfer.AssetData = new byte[task.Transfer.Size];
                        task.Transfer.AssetType = AssetType.Texture;
                        task.Transfer.PacketsSeen = new SortedList<ushort, ushort>();
                        Buffer.BlockCopy(data.ImageData.Data, 0, task.Transfer.AssetData, 0, data.ImageData.Data.Length);
                        task.Transfer.InitialDataSize = data.ImageData.Data.Length;
                        task.Transfer.Transferred += data.ImageData.Data.Length;
                    }

                    task.Transfer.HeaderReceivedEvent.Set();

                    if (task.Transfer.Transferred >= task.Transfer.Size)
                    {
                        task.Transfer.Success = true;
                        _Transfers.Remove(task.RequestID);
                        resetEvents[task.RequestNbr].Set();

                        client.Assets.Cache.SaveImageToCache(task.RequestID, task.Transfer.AssetData);
                        AssetTexture asset = new AssetTexture(task.RequestID, task.Transfer.AssetData);
                        task.Callback(TextureRequestState.Finished, task.Transfer, asset);
                    }
                    else
                    {
                        if (task.ReportProgress)
                        {
                            task.Callback(TextureRequestState.Progress, task.Transfer, null);
                        }
                    }

                    //Monitor.Exit(task.syncItem);
                }
            }
        }

        #endregion


        internal void RequestTexture(UUID image, ImageType imageType, float p, int p_4, int p_5, TextureDownloadCallback textureDownloadCallback)
        {
            throw new NotImplementedException();
        }
    }
}
