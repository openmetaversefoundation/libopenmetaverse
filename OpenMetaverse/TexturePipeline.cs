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

namespace OpenMetaverse
{
    /// <summary>
    /// Texture request download handler, allows a configurable number of download slots
    /// </summary>
    public class TexturePipeline
    {
        class TaskInfo
        {
            public UUID RequestID;
            public int RequestNbr;
            public ImageType Type;

            public TaskInfo(UUID reqID, int reqNbr, ImageType type)
            {
                RequestID = reqID;
                RequestNbr = reqNbr;
                Type = type;
            }
        }

        public delegate void DownloadFinishedCallback(UUID id, bool success);
        public delegate void DownloadProgressCallback(UUID image, int recieved, int total);

        /// <summary>Fired when a texture download completes</summary>
        public event DownloadFinishedCallback OnDownloadFinished;
        /// <summary>Fired when some texture data is received</summary>
        public event DownloadProgressCallback OnDownloadProgress;

        /// <summary>For keeping track of active threads available/downloading
        /// textures</summary>
        public int[] ThreadpoolSlots
        {
            get { lock (threadpoolSlots) { return threadpoolSlots; } }
            set { lock (threadpoolSlots) { threadpoolSlots = value; } }
        }

        GridClient client;
        /// <summary>Maximum concurrent texture requests</summary>
        int maxTextureRequests;
        /// <summary>Queue for image requests that have not been sent out yet</summary>
        Queue<KeyValuePair<UUID, ImageType>> requestQueue;
        /// <summary>Current texture downloads</summary>
        Dictionary<UUID, int> currentRequests;
        /// <summary>Storage for completed texture downloads</summary>
        Dictionary<UUID, ImageDownload> completedDownloads;
        AutoResetEvent[] resetEvents;
        int[] threadpoolSlots;
        Thread downloadMaster;
        bool running;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="client">Reference to <code>SecondLife</code> client</param>
        /// <param name="maxRequests">Maximum number of concurrent texture requests</param>
        public TexturePipeline(GridClient client, int maxRequests)
        {
            running = true;
            this.client = client;
            maxTextureRequests = maxRequests;

            requestQueue = new Queue<KeyValuePair<UUID, ImageType>>();
            currentRequests = new Dictionary<UUID, int>(maxTextureRequests);
            completedDownloads = new Dictionary<UUID, ImageDownload>();
            resetEvents = new AutoResetEvent[maxTextureRequests];
            threadpoolSlots = new int[maxTextureRequests];

            // Pre-configure autoreset events/download slots
            for (int i = 0; i < maxTextureRequests; i++)
            {
                resetEvents[i] = new AutoResetEvent(false);
                threadpoolSlots[i] = -1;
            }

            client.Assets.OnImageReceived += Assets_OnImageReceived;
            client.Assets.OnImageReceiveProgress += Assets_OnImageReceiveProgress;

            // Fire up the texture download thread
            downloadMaster = new Thread(new ThreadStart(DownloadThread));
            downloadMaster.Start();
        }

        public void Shutdown()
        {
            client.Assets.OnImageReceived -= Assets_OnImageReceived;
            client.Assets.OnImageReceiveProgress -= Assets_OnImageReceiveProgress;

            requestQueue.Clear();

            for (int i = 0; i < resetEvents.Length; i++)
                if (resetEvents[i] != null)
                    resetEvents[i].Set();

            running = false;
        }

        /// <summary>
        /// Request a texture be downloaded, once downloaded OnImageRenderReady event will be fired
        /// containing texture key which can be used to retrieve texture with GetTextureToRender method
        /// </summary>
        /// <param name="textureID">Texture to request</param>
        /// <param name="type">Type of the requested texture</param>
        public void RequestTexture(UUID textureID, ImageType type)
        {
            if (client.Assets.Cache.HasImage(textureID))
            {
                // Add to rendering dictionary
                lock (completedDownloads)
                {
                    if (!completedDownloads.ContainsKey(textureID))
                    {
                        completedDownloads.Add(textureID, client.Assets.Cache.GetCachedImage(textureID));

                        // Let any subscribers know about it
                        if (OnDownloadFinished != null)
                            OnDownloadFinished(textureID, true);
                    }
                    else
                    {
                        // This image has already been served up, ignore this request
                    }
                }
            }
            else
            {
                lock (requestQueue)
                {
                    // Make sure the request isn't already queued up
                    foreach (KeyValuePair<UUID, ImageType> kvp in requestQueue)
                        if (kvp.Key == textureID)
                            return;

                    // Make sure we aren't already downloading the texture
                    if (!currentRequests.ContainsKey(textureID))
                    {
                        requestQueue.Enqueue(new KeyValuePair<UUID, ImageType>(textureID, type));
                    }
                }
            }
        }

        /// <summary>
        /// retrieve texture information from dictionary
        /// </summary>
        /// <param name="textureID">Texture ID</param>
        /// <returns>ImageDownload object</returns>
        public ImageDownload GetTextureToRender(UUID textureID)
        {
            ImageDownload renderable = new ImageDownload();
            lock (completedDownloads)
            {
                if (completedDownloads.ContainsKey(textureID))
                {
                    renderable = completedDownloads[textureID];
                }
                else
                {
                    Logger.Log("Requested texture data for texture that does not exist in dictionary", Helpers.LogLevel.Warning);
                }
                return renderable;
            }
        }

        /// <summary>
        /// Remove no longer necessary texture from dictionary
        /// </summary>
        /// <param name="textureID"></param>
        public void RemoveFromPipeline(UUID textureID)
        {
            lock (completedDownloads)
            {
                if (completedDownloads.ContainsKey(textureID))
                    completedDownloads.Remove(textureID);
            }
        }

        /// <summary>
        /// Master Download Thread, Queues up downloads in the threadpool
        /// </summary>
        private void DownloadThread()
        {
            int reqNbr;

            while (running)
            {
                if (requestQueue.Count > 0)
                {
                    reqNbr = -1;
                    // find available slot for reset event
                    for (int i = 0; i < threadpoolSlots.Length; i++)
                    {
                        if (threadpoolSlots[i] == -1)
                        {
                            threadpoolSlots[i] = 1;
                            reqNbr = i;
                            break;
                        }
                    }

                    if (reqNbr != -1)
                    {
                        KeyValuePair<UUID, ImageType> request;
                        lock (requestQueue)
                            request = requestQueue.Dequeue();

                        Logger.DebugLog(String.Format("Sending Worker thread new download request {0}", reqNbr));
                        ThreadPool.QueueUserWorkItem(new WaitCallback(TextureRequestDoWork), new TaskInfo(request.Key, reqNbr, request.Value));

                        continue;
                    }
                }

                // Queue was empty, let's give up some CPU time
                Thread.Sleep(500);
            }

            Logger.Log("Texture pipeline shutting down", Helpers.LogLevel.Info);
        }

        private void TextureRequestDoWork(Object threadContext)
        {
            TaskInfo ti = (TaskInfo)threadContext;

            lock (currentRequests)
            {
                if (currentRequests.ContainsKey(ti.RequestID))
                {
                    threadpoolSlots[ti.RequestNbr] = -1;
                    return;
                }
                else
                {
                    currentRequests.Add(ti.RequestID, ti.RequestNbr);
                }
            }

            Logger.DebugLog(String.Format("Worker {0} Requesting {1}", ti.RequestNbr, ti.RequestID));

            resetEvents[ti.RequestNbr].Reset();
            client.Assets.RequestImage(ti.RequestID, ti.Type);

            // don't release this worker slot until texture is downloaded or timeout occurs
            if (!resetEvents[ti.RequestNbr].WaitOne(30 * 1000, false))
            {
                // Timed out
                Logger.Log("Worker " + ti.RequestNbr + " Timeout waiting for Texture " + ti.RequestID + " to Download", Helpers.LogLevel.Warning);

                lock (currentRequests)
                    currentRequests.Remove(ti.RequestID);

                if (OnDownloadFinished != null)
                    OnDownloadFinished(ti.RequestID, false);
            }

            // free up this download slot
            threadpoolSlots[ti.RequestNbr] = -1;
        }

        private void Assets_OnImageReceived(ImageDownload image, AssetTexture asset)
        {
            int requestNbr;
            bool found;

            lock (currentRequests)
                found = currentRequests.TryGetValue(image.ID, out requestNbr);

            if (asset != null && found)
            {
                Logger.DebugLog(String.Format("Worker {0} Downloaded texture {1}", requestNbr, image.ID));

                // Free up this slot in the ThreadPool
                lock (currentRequests)
                    currentRequests.Remove(image.ID);

                resetEvents[requestNbr].Set();

                if (image.Success)
                {
                    // Add to the completed texture dictionary
                    lock (completedDownloads)
                        completedDownloads[image.ID] = image;
                }
                else
                {
                    Logger.Log(String.Format("Download of texture {0} failed. NotFound={1}", image.ID, image.NotFound),
                        Helpers.LogLevel.Warning);
                }

                // Let any subscribers know about it
                if (OnDownloadFinished != null)
                    OnDownloadFinished(image.ID, image.Success);
            }
        }

        private void Assets_OnImageReceiveProgress(UUID image, int lastPacket, int recieved, int total)
        {
            if (OnDownloadProgress != null && currentRequests.ContainsKey(image))
                OnDownloadProgress(image, recieved, total);
        }
    }
}
