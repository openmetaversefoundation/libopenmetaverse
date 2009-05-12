/*
 * Copyright (c) 2009, openmetaverse.org
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

//#define DEBUG_TIMING

using System;
using System.Collections.Generic;
using System.Threading;
using OpenMetaverse.Packets;
using OpenMetaverse.Assets;

namespace OpenMetaverse
{
    /// <summary>
    /// The current status of a texture request as it moves through the pipeline or final result of a texture request. 
    /// </summary>
    public enum TextureRequestState
    {
        /// <summary>The initial state given to a request. Requests in this state
        /// are waiting for an available slot in the pipeline</summary>
        Pending,
        /// <summary>A request that has been added to the pipeline and the request packet
        /// has been sent to the simulator</summary>
        Started,
        /// <summary>A request that has received one or more packets back from the simulator</summary>
        Progress,
        /// <summary>A request that has received all packets back from the simulator</summary>
        Finished,
        /// <summary>A request that has taken longer than <seealso cref="Settings.PIPELINE_REQUEST_TIMEOUT"/>
        /// to download OR the initial packet containing the packet information was never received</summary>
        Timeout,
        /// <summary>The texture request was aborted by request of the agent</summary>
        Aborted,
        /// <summary>The simulator replied to the request that it was not able to find the requested texture</summary>
        NotFound
    }
    /// <summary>
    /// A callback fired to indicate the status or final state of the requested texture. For progressive 
    /// downloads this will fire each time new asset data is returned from the simulator.
    /// </summary>
    /// <param name="state">The <see cref="TextureRequestState"/> indicating either Progress for textures not fully downloaded,
    /// or the final result of the request after it has been processed through the TexturePipeline</param>
    /// <param name="assetTexture">The <see cref="AssetTexture"/> object containing the Assets ID, raw data
    /// and other information. For progressive rendering the <see cref="Asset.AssetData"/> will contain
    /// the data from the beginning of the file. For failed, aborted and timed out requests it will contain
    /// an empty byte array.</param>
    public delegate void TextureDownloadCallback(TextureRequestState state, AssetTexture assetTexture);

    /// <summary>
    /// Texture request download handler, allows a configurable number of download slots which manage multiple
    /// concurrent texture downloads from the <seealso cref="Simulator"/>
    /// </summary>
    /// <remarks>This class makes full use of the internal <seealso cref="TextureCache"/> 
    /// system for full texture downloads.</remarks>
    public class TexturePipeline
    {
#if DEBUG_TIMING // Timing globals
        /// <summary>The combined time it has taken for all textures requested sofar. This includes the amount of time the 
        /// texture spent waiting for a download slot, and the time spent retrieving the actual texture from the Grid</summary>
        public static TimeSpan TotalTime;
        /// <summary>The amount of time the request spent in the <see cref="TextureRequestState.Progress"/> state</summary>
        public static TimeSpan NetworkTime;
        /// <summary>The total number of bytes transferred since the TexturePipeline was started</summary>
        public static int TotalBytes;
#endif
        /// <summary>
        /// A request task containing information and status of a request as it is processed through the <see cref="TexturePipeline"/>
        /// </summary>
        private class TaskInfo
        {
            /// <summary>The current <seealso cref="TextureRequestState"/> which identifies the current status of the request</summary>
            public TextureRequestState State;
            /// <summary>The Unique Request ID, This is also the Asset ID of the texture being requested</summary>
            public UUID RequestID;
            /// <summary>The slot this request is occupying in the threadpoolSlots array</summary>
            public int RequestSlot;
            /// <summary>The ImageType of the request.</summary>
            public ImageType Type;

            /// <summary>The callback to fire when the request is complete, will include 
            /// the <seealso cref="TextureRequestState"/> and the <see cref="AssetTexture"/> 
            /// object containing the result data</summary>
            public List<TextureDownloadCallback> Callbacks;
            /// <summary>If true, indicates the callback will be fired whenever new data is returned from the simulator.
            /// This is used to progressively render textures as portions of the texture are received.</summary>
            public bool ReportProgress;
#if DEBUG_TIMING
            /// <summary>The time the request was added to the the PipeLine</summary>
            public DateTime StartTime;
            /// <summary>The time the request was sent to the simulator</summary>
            public DateTime NetworkTime;
#endif
            /// <summary>An object that maintains the data of an request thats in-process.</summary>
            public ImageDownload Transfer;
        }

        /// <summary>A dictionary containing all pending and in-process transfer requests where the Key is both the RequestID
        /// and also the Asset Texture ID, and the value is an object containing the current state of the request and also
        /// the asset data as it is being re-assembled</summary>
        private readonly Dictionary<UUID, TaskInfo> _Transfers;
        /// <summary>Holds the reference to the <see cref="GridClient"/> client object</summary>
        private readonly GridClient _Client;
        /// <summary>Maximum concurrent texture requests allowed at a time</summary>
        private readonly int maxTextureRequests;
        /// <summary>An array of <see cref="AutoResetEvent"/> objects used to manage worker request threads</summary>
        private readonly AutoResetEvent[] resetEvents;
        /// <summary>An array of worker slots which shows the availablity status of the slot</summary>
        private readonly int[] threadpoolSlots;
        /// <summary>The primary thread which manages the requests.</summary>
        private readonly Thread downloadMaster;
        /// <summary>true if the TexturePipeline is currently running</summary>
        bool _Running;
        /// <summary>A synchronization object used by the primary thread</summary>
        private static object lockerObject = new object();
        /// <summary>A refresh timer used to increase the priority of stalled requests</summary>
        private readonly System.Timers.Timer RefreshDownloadsTimer = 
            new System.Timers.Timer(Settings.PIPELINE_REFRESH_INTERVAL);

        /// <summary>Current number of pending and in-process transfers</summary>
        public int TransferCount { get { return _Transfers.Count; } }

        /// <summary>
        /// Default constructor, Instantiates a new copy of the TexturePipeline class
        /// </summary>
        /// <param name="client">Reference to the instantiated <see cref="GridClient"/> object</param>
        public TexturePipeline(GridClient client)
        {
            _Client = client;

            maxTextureRequests = client.Settings.MAX_CONCURRENT_TEXTURE_DOWNLOADS;

            resetEvents = new AutoResetEvent[maxTextureRequests];
            threadpoolSlots = new int[maxTextureRequests];

            _Transfers = new Dictionary<UUID, TaskInfo>();

            // Pre-configure autoreset events and threadpool slots
            for (int i = 0; i < maxTextureRequests; i++)
            {
                resetEvents[i] = new AutoResetEvent(true);
                threadpoolSlots[i] = -1;
            }

            // Handle client connected and disconnected events
            client.Network.OnConnected += delegate { Startup(); };
            client.Network.OnDisconnected += delegate { Shutdown(); };

            // Instantiate master thread that manages the request pool
            downloadMaster = new Thread(DownloadThread);
            downloadMaster.Name = "TexturePipeline";
            downloadMaster.IsBackground = true;
            
            RefreshDownloadsTimer.Elapsed += RefreshDownloadsTimer_Elapsed;
        }

        /// <summary>
        /// Initialize callbacks required for the TexturePipeline to operate
        /// </summary>
        private void Startup()
        {
            if(_Running)
                return;

            _Client.Network.RegisterCallback(PacketType.ImageData, ImageDataHandler);
            _Client.Network.RegisterCallback(PacketType.ImagePacket, ImagePacketHandler);
            _Client.Network.RegisterCallback(PacketType.ImageNotInDatabase, ImageNotInDatabaseHandler);
            downloadMaster.Start();
            RefreshDownloadsTimer.Start();

            _Running = true;
        }

        /// <summary>
        /// Shutdown the TexturePipeline and cleanup any callbacks or transfers
        /// </summary>
        private void Shutdown()
        {
            if(!_Running)
                return;
#if DEBUG_TIMING
            Logger.Log(String.Format("Combined Execution Time: {0}, Network Execution Time {1}, Network {2}K/sec, Image Size {3}",
                        TotalTime, NetworkTime, Math.Round(TotalBytes / NetworkTime.TotalSeconds / 60, 2), TotalBytes), Helpers.LogLevel.Debug);
#endif
            RefreshDownloadsTimer.Stop();

            _Client.Network.UnregisterCallback(PacketType.ImageNotInDatabase, ImageNotInDatabaseHandler);
            _Client.Network.UnregisterCallback(PacketType.ImageData, ImageDataHandler);
            _Client.Network.UnregisterCallback(PacketType.ImagePacket, ImagePacketHandler);
            
            lock (_Transfers)
                _Transfers.Clear();

            for (int i = 0; i < resetEvents.Length; i++)
                if (resetEvents[i] != null)
                    resetEvents[i].Set();

            _Running = false;
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

                            if (download.TimeSinceLastPacket > 5000)
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
        /// Request a texture asset from the simulator using the <see cref="TexturePipeline"/> system to 
        /// manage the requests and re-assemble the image from the packets received from the simulator
        /// </summary>
        /// <param name="textureID">The <see cref="UUID"/> of the texture asset to download</param>
        /// <param name="imageType">The <see cref="ImageType"/> of the texture asset. 
        /// Use <see cref="ImageType.Normal"/> for most textures, or <see cref="ImageType.Baked"/> for baked layer texture assets</param>
        /// <param name="priority">A float indicating the requested priority for the transfer. Higher priority values tell the simulator
        /// to prioritize the request before lower valued requests. An image already being transferred using the <see cref="TexturePipeline"/> can have
        /// its priority changed by resending the request with the new priority value</param>
        /// <param name="discardLevel">Number of quality layers to discard.
        /// This controls the end marker of the data sent</param>
        /// <param name="packetStart">The packet number to begin the request at. A value of 0 begins the request
        /// from the start of the asset texture</param>
        /// <param name="callback">The <see cref="TextureDownloadCallback"/> callback to fire when the image is retrieved. The callback
        /// will contain the result of the request and the texture asset data</param>
        /// <param name="progressive">If true, the callback will be fired for each chunk of the downloaded image. 
        /// The callback asset parameter will contain all previously received chunks of the texture asset starting 
        /// from the beginning of the request</param>
        public void RequestTexture(UUID textureID, ImageType imageType, float priority, int discardLevel, uint packetStart, TextureDownloadCallback callback, bool progressive)
        {
            if (textureID == UUID.Zero)
                return;

            if (callback != null)
            {
                if (_Client.Assets.Cache.HasImage(textureID))
                {
                    ImageDownload image = new ImageDownload();
                    image.ID = textureID;
                    image.AssetData = _Client.Assets.Cache.GetCachedImageBytes(textureID);
                    image.Size = image.AssetData.Length;
                    image.Transferred = image.AssetData.Length;
                    image.ImageType = imageType;
                    image.AssetType = AssetType.Texture;
                    image.Success = true;
                    
                    callback(TextureRequestState.Finished, new AssetTexture(image.ID, image.AssetData));

                    _Client.Assets.FireImageProgressEvent(image.ID, image.Transferred, image.Size);
                }
                else
                {
                    lock (_Transfers)
                    {
                        if (_Transfers.ContainsKey(textureID))
                        {
                            _Transfers[textureID].Callbacks.Add(callback);
                        } 
                        else 
                        {
                            TaskInfo request = new TaskInfo();
                            request.State = TextureRequestState.Pending;
                            request.RequestID = textureID;
                            request.ReportProgress = progressive;
                            request.RequestSlot = -1;
                            request.Type = imageType;

                            request.Callbacks = new List<TextureDownloadCallback>(); 
                            request.Callbacks.Add(callback);

                            ImageDownload downloadParams = new ImageDownload();
                            downloadParams.ID = textureID;
                            downloadParams.Priority = priority;
                            downloadParams.ImageType = imageType;
                            downloadParams.DiscardLevel = discardLevel;

                            request.Transfer = downloadParams;
#if DEBUG_TIMING
                            request.StartTime = DateTime.UtcNow;
#endif
                            _Transfers.Add(textureID, request);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sends the actual request packet to the simulator
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
                AbortTextureRequest(imageID);
            }
            else
            {

                lock (_Transfers)
                {
                    if (_Transfers.ContainsKey(imageID))
                    {
                        if (_Transfers[imageID].Transfer.Simulator != null)
                        {
                            // Already downloading, just updating the priority
                            TaskInfo task = _Transfers[imageID];
                            
                            float percentComplete = (task.Transfer.Transferred / (float)task.Transfer.Size) * 100f;
                            if (Single.IsNaN(percentComplete))
                                percentComplete = 0f;

                            if (percentComplete > 0)
                                Logger.DebugLog(String.Format("Updating priority on image transfer {0}, {1}% complete",
                                                              imageID, Math.Round(percentComplete, 2)));
                        }
                        else
                        {
                            ImageDownload transfer = _Transfers[imageID].Transfer;
                            transfer.Simulator = _Client.Network.CurrentSim;
                        }

                        // Build and send the request packet
                        RequestImagePacket request = new RequestImagePacket();
                        request.AgentData.AgentID = _Client.Self.AgentID;
                        request.AgentData.SessionID = _Client.Self.SessionID;
                        request.RequestImage = new RequestImagePacket.RequestImageBlock[1];
                        request.RequestImage[0] = new RequestImagePacket.RequestImageBlock();
                        request.RequestImage[0].DiscardLevel = (sbyte)discardLevel;
                        request.RequestImage[0].DownloadPriority = priority;
                        request.RequestImage[0].Packet = packetNum;
                        request.RequestImage[0].Image = imageID;
                        request.RequestImage[0].Type = (byte)type;

                        _Client.Network.SendPacket(request, _Client.Network.CurrentSim);
                    }
                    else
                    {
                        Logger.Log("Received texture download request for a texture that isn't in the download queue: " + imageID, Helpers.LogLevel.Warning);
                    }
                }
            }
        }

        /// <summary>
        /// Cancel a pending or in process texture request
        /// </summary>
        /// <param name="textureID">The texture assets unique ID</param>
        public void AbortTextureRequest(UUID textureID)
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
                        request.AgentData.AgentID = _Client.Self.AgentID;
                        request.AgentData.SessionID = _Client.Self.SessionID;
                        request.RequestImage = new RequestImagePacket.RequestImageBlock[1];
                        request.RequestImage[0] = new RequestImagePacket.RequestImageBlock();
                        request.RequestImage[0].DiscardLevel = -1;
                        request.RequestImage[0].DownloadPriority = 0;
                        request.RequestImage[0].Packet = 0;
                        request.RequestImage[0].Image = textureID;
                        request.RequestImage[0].Type = (byte)task.Type;
                        _Client.Network.SendPacket(request);

                        foreach(TextureDownloadCallback callback in task.Callbacks)
                            callback(TextureRequestState.Aborted, new AssetTexture(textureID, Utils.EmptyBytes));

                        _Client.Assets.FireImageProgressEvent(task.RequestID, task.Transfer.Transferred, task.Transfer.Size);

                        resetEvents[task.RequestSlot].Set();

                        _Transfers.Remove(textureID);
                    }
                    else
                    {
                        _Transfers.Remove(textureID);
                        
                        foreach(TextureDownloadCallback callback in task.Callbacks)
                            callback(TextureRequestState.Aborted, new AssetTexture(textureID, Utils.EmptyBytes));

                        _Client.Assets.FireImageProgressEvent(task.RequestID, task.Transfer.Transferred, task.Transfer.Size);
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

            while (_Running)
            {
                // find free slots
                int pending = 0;
                int active = 0;
                
                TaskInfo nextTask = null;

                lock (_Transfers)
                {
                    foreach (UUID request in _Transfers.Keys)
                    {
                        if (_Transfers[request].State == TextureRequestState.Pending)
                        {
                            nextTask = _Transfers[request];
                            pending++;
                        }

                        if (_Transfers[request].State == TextureRequestState.Progress)
                            active++;
                    }
                }

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
                        }

                        // -1 = slot not available
                        if (slot != -1 && nextTask != null)
                        {
                            
                                nextTask.State = TextureRequestState.Started;
                                nextTask.RequestSlot = slot;
                                nextTask.Transfer = new ImageDownload();
                                nextTask.Transfer.ID = nextTask.RequestID;

                                //Logger.DebugLog(String.Format("Sending Worker thread new download request {0}", slot));
                                ThreadPool.QueueUserWorkItem(TextureRequestDoWork, nextTask);
                                continue;
                        }
                    }

                // Queue was empty or all download slots are inuse, let's give up some CPU time
                Thread.Sleep(500);
            }

            Logger.Log("Texture pipeline shutting down", Helpers.LogLevel.Info);
        }


        /// <summary>
        /// The worker thread that sends the request and handles timeouts
        /// </summary>
        /// <param name="threadContext">A <see cref="TaskInfo"/> object containing the request details</param>
        private void TextureRequestDoWork(Object threadContext)
        {
            TaskInfo task = (TaskInfo) threadContext;
            
            task.State = TextureRequestState.Progress;

#if DEBUG_TIMING
            task.NetworkTime = DateTime.UtcNow;
#endif
            // start the timeout timer
            resetEvents[task.RequestSlot].Reset();
            RequestImage(task.RequestID, task.Type, 1013000.0f, 0, 0);

            // don't release this worker slot until texture is downloaded or timeout occurs
            if (!resetEvents[task.RequestSlot].WaitOne(_Client.Settings.PIPELINE_REQUEST_TIMEOUT, false))
            {
                // Timed out
                Logger.Log("Worker " + task.RequestSlot + " Timeout waiting for Texture " + task.RequestID + " to Download Got " + task.Transfer.Transferred + " of " + task.Transfer.Size, Helpers.LogLevel.Warning);

                foreach(TextureDownloadCallback callback in task.Callbacks)
                    callback(TextureRequestState.Timeout, new AssetTexture(task.RequestID, task.Transfer.AssetData));

                _Client.Assets.FireImageProgressEvent(task.RequestID, task.Transfer.Transferred, task.Transfer.Size);

                lock (_Transfers)
                    _Transfers.Remove(task.RequestID);
            }

            // free up this download slot
            lock(lockerObject)
                threadpoolSlots[task.RequestSlot] = -1;
        }

        #region Raw Packet Handlers

        /// <summary>
        /// Handle responses from the simulator that tell us a texture we have requested is unable to be located
        /// or no longer exists. This will remove the request from the pipeline and free up a slot if one is in use
        /// </summary>
        /// <param name="packet">The <see cref="ImageNotInDatabasePacket"/></param>
        /// <param name="simulator">The <see cref="Simulator"/> sending this packet</param>
        private void ImageNotInDatabaseHandler(Packet packet, Simulator simulator)
        {
            ImageNotInDatabasePacket imageNotFoundData = (ImageNotInDatabasePacket)packet;
            lock (_Transfers)
            {
                if (_Transfers.ContainsKey(imageNotFoundData.ImageID.ID))
                {
                    // cancel acive request and free up the threadpool slot
                    TaskInfo task = _Transfers[imageNotFoundData.ImageID.ID];
                    if (task.State == TextureRequestState.Progress)
                    {
                        resetEvents[task.RequestSlot].Set();
                    }

                    // fire callback to inform the caller 
                    foreach (TextureDownloadCallback callback in task.Callbacks)
                        callback(TextureRequestState.NotFound, new AssetTexture(imageNotFoundData.ImageID.ID, Utils.EmptyBytes));

                    resetEvents[task.RequestSlot].Set();

                    _Transfers.Remove(imageNotFoundData.ImageID.ID);
                } 
                else
                {
                    Logger.Log("Received an ImageNotFound packet for an image we did not request: " + imageNotFoundData.ImageID.ID, Helpers.LogLevel.Warning);
                }
            }
        }

        /// <summary>
        /// Handles the remaining Image data that did not fit in the initial ImageData packet
        /// </summary>
        private void ImagePacketHandler(Packet packet, Simulator simulator)
        {
            ImagePacketPacket image = (ImagePacketPacket)packet;
            TaskInfo task = null;

            lock (_Transfers)
            {
                if (_Transfers.ContainsKey(image.ImageID.ID))
                {
                    task = _Transfers[image.ImageID.ID];
                }


                if (task != null)
                {
                    if (task.Transfer.Size == 0)
                    {
                        // We haven't received the header yet, block until it's received or times out
                        task.Transfer.HeaderReceivedEvent.WaitOne(1000*5, false);

                        if (task.Transfer.Size == 0)
                        {
                            Logger.Log("Timed out while waiting for the image header to download for " +
                                       task.Transfer.ID, Helpers.LogLevel.Warning, _Client);

                            _Transfers.Remove(task.Transfer.ID);
                            resetEvents[task.RequestSlot].Set(); // free up request slot

                            foreach (TextureDownloadCallback callback in task.Callbacks)
                                callback(TextureRequestState.Timeout, new AssetTexture(task.RequestID, task.Transfer.AssetData));

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
                                             task.Transfer.InitialDataSize + (1000*(image.ImageID.Packet - 1)),
                                             image.ImageData.Data.Length);
                            task.Transfer.Transferred += image.ImageData.Data.Length;
                        }
                    }

                    task.Transfer.TimeSinceLastPacket = 0;

                    if (task.Transfer.Transferred >= task.Transfer.Size)
                    {
#if DEBUG_TIMING
                        DateTime stopTime = DateTime.UtcNow;
                        TimeSpan requestDuration = stopTime - task.StartTime;

                        TimeSpan networkDuration = stopTime - task.NetworkTime;

                        TotalTime += requestDuration;
                        NetworkTime += networkDuration;
                        TotalBytes += task.Transfer.Size;

                        Logger.Log(
                            String.Format(
                                "Transfer Complete {0} [{1}] Total Request Time: {2}, Download Time {3}, Network {4}Kb/sec, Image Size {5} bytes",
                                task.RequestID, task.RequestSlot, requestDuration, networkDuration,
                                Math.Round(task.Transfer.Size/networkDuration.TotalSeconds/60, 2), task.Transfer.Size),
                            Helpers.LogLevel.Debug);
#endif

                        task.Transfer.Success = true;
                        _Transfers.Remove(task.Transfer.ID);
                        resetEvents[task.RequestSlot].Set(); // free up request slot
                        _Client.Assets.Cache.SaveImageToCache(task.RequestID, task.Transfer.AssetData);
                        foreach (TextureDownloadCallback callback in task.Callbacks)
                            callback(TextureRequestState.Finished, new AssetTexture(task.RequestID, task.Transfer.AssetData));

                        _Client.Assets.FireImageProgressEvent(task.RequestID, task.Transfer.Transferred, task.Transfer.Size);
                    }
                    else
                    {
                        if (task.ReportProgress)
                        {
                            foreach (TextureDownloadCallback callback in task.Callbacks)
                                callback(TextureRequestState.Progress,
                                         new AssetTexture(task.RequestID, task.Transfer.AssetData));
                        }
                        _Client.Assets.FireImageProgressEvent(task.RequestID, task.Transfer.Transferred,
                                                                  task.Transfer.Size);
                    }
                }
            }
        }

        /// <summary>
        /// Handle the initial ImageDataPacket sent from the simulator
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="simulator"></param>
        private void ImageDataHandler(Packet packet, Simulator simulator)
        {
            ImageDataPacket data = (ImageDataPacket) packet;
            TaskInfo task = null;

            lock (_Transfers)
            {
                if (_Transfers.ContainsKey(data.ImageID.ID))
                {
                    task = _Transfers[data.ImageID.ID];
                }
            

            if (task != null)
            {
                // reset the timeout interval since we got data
                resetEvents[task.RequestSlot].Reset();

                if (task.Transfer.Size == 0)
                {
                    task.Transfer.Codec = (ImageCodec) data.ImageID.Codec;
                    task.Transfer.PacketCount = data.ImageID.Packets;
                    task.Transfer.Size = (int) data.ImageID.Size;
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
#if DEBUG_TIMING
                    DateTime stopTime = DateTime.UtcNow;
                    TimeSpan requestDuration = stopTime - task.StartTime;

                    TimeSpan networkDuration = stopTime - task.NetworkTime;

                    TotalTime += requestDuration;
                    NetworkTime += networkDuration;
                    TotalBytes += task.Transfer.Size;

                    Logger.Log(
                        String.Format(
                            "Transfer Complete {0} [{1}] Total Request Time: {2}, Download Time {3}, Network {4}Kb/sec, Image Size {5} bytes",
                            task.RequestID, task.RequestSlot, requestDuration, networkDuration,
                            Math.Round(task.Transfer.Size/networkDuration.TotalSeconds/60, 2), task.Transfer.Size),
                        Helpers.LogLevel.Debug);
#endif
                    task.Transfer.Success = true;
                    _Transfers.Remove(task.RequestID);
                    resetEvents[task.RequestSlot].Set();

                    _Client.Assets.Cache.SaveImageToCache(task.RequestID, task.Transfer.AssetData);

                    foreach (TextureDownloadCallback callback in task.Callbacks)
                        callback(TextureRequestState.Finished, new AssetTexture(task.RequestID, task.Transfer.AssetData));

                    _Client.Assets.FireImageProgressEvent(task.RequestID, task.Transfer.Transferred, task.Transfer.Size);
                }
                else
                {
                    if (task.ReportProgress)
                    {
                        foreach (TextureDownloadCallback callback in task.Callbacks)
                            callback(TextureRequestState.Progress,
                                      new AssetTexture(task.RequestID, task.Transfer.AssetData));
                    }
                    _Client.Assets.FireImageProgressEvent(task.RequestID, task.Transfer.Transferred,
                                                              task.Transfer.Size);
                }
            }
        }
    }
        #endregion
    }
}
