
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using OpenMetaverse;

/*
 * [14:09]	<jhurliman>	the onnewprim function will add missing texture uuids to the download queue, 
 * and a separate thread will pull entries off that queue. 
 * if they exist in the cache it will add that texture to a dictionary that the rendering loop accesses, 
 * otherwise it will start the download. 
 * the ondownloaded function will put the new texture in the same dictionary
 * 
 * 
 * Easy Start:
 * subscribe to OnImageRenderReady event
 * send request with RequestTexture()
 * 
 * when OnImageRenderReady fires:
 * request image data with GetTextureToRender() using key returned in OnImageRenderReady event
 * (optionally) use RemoveFromPipeline() with key to cleanup dictionary
 */
namespace PrimWorkshop
{
    class TaskInfo
    {
        public UUID RequestID;
        public int RequestNbr;


        public TaskInfo(UUID reqID, int reqNbr)
        {
            RequestID = reqID;
            RequestNbr = reqNbr;
        }
    }

    /// <summary>
    /// Texture request download handler, allows a configurable number of download slots
    /// </summary>
    public class TexturePipeline
    {
        private static GridClient Client;

        // queue for requested images
        private Queue<UUID> RequestQueue;

        // list of current requests in process
        private Dictionary<UUID, int> CurrentRequests;

        private static AutoResetEvent[] resetEvents;

        private static int[] threadpoolSlots;

        /// <summary>
        /// For keeping track of active threads available/downloading textures
        /// </summary>
        public static int[] ThreadpoolSlots
        {
            get { lock (threadpoolSlots) { return threadpoolSlots; }}
            set { lock (threadpoolSlots) { threadpoolSlots = value; } }
        }

        // storage for images ready to render
        private Dictionary<UUID, ImageDownload> RenderReady;

        // maximum allowed concurrent requests at once
        const int MAX_TEXTURE_REQUESTS = 10;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="success"></param>
        public delegate void DownloadFinishedCallback(UUID id, bool success);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="received"></param>
        /// <param name="total"></param>
        public delegate void DownloadProgressCallback(UUID image, int recieved, int total);

        /// <summary>Fired when a texture download completes</summary>
        public event DownloadFinishedCallback OnDownloadFinished;
        /// <summary></summary>
        public event DownloadProgressCallback OnDownloadProgress;

        private Thread downloadMaster;
        private bool Running;

        private AssetManager.ImageReceivedCallback DownloadCallback;
        private AssetManager.ImageReceiveProgressCallback DownloadProgCallback;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="client">Reference to <code>SecondLife</code> client</param>
        public TexturePipeline(GridClient client)
        {
            Running = true;

            RequestQueue = new Queue<UUID>();
            CurrentRequests = new Dictionary<UUID, int>(MAX_TEXTURE_REQUESTS);

            RenderReady = new Dictionary<UUID, ImageDownload>();

            resetEvents = new AutoResetEvent[MAX_TEXTURE_REQUESTS];
            threadpoolSlots = new int[MAX_TEXTURE_REQUESTS];

            // pre-configure autoreset events/download slots
            for (int i = 0; i < MAX_TEXTURE_REQUESTS; i++)
            {
                resetEvents[i] = new AutoResetEvent(false);
                threadpoolSlots[i] = -1;
            }

            Client = client;

            DownloadCallback = new AssetManager.ImageReceivedCallback(Assets_OnImageReceived);
            DownloadProgCallback = new AssetManager.ImageReceiveProgressCallback(Assets_OnImageReceiveProgress);
            Client.Assets.OnImageReceived += DownloadCallback;
            Client.Assets.OnImageReceiveProgress += DownloadProgCallback;

            // Fire up the texture download thread
            downloadMaster = new Thread(new ThreadStart(DownloadThread));
            downloadMaster.Start();
        }

        public void Shutdown()
        {
            Client.Assets.OnImageReceived -= DownloadCallback;
            Client.Assets.OnImageReceiveProgress -= DownloadProgCallback;

            RequestQueue.Clear();

            for (int i = 0; i < resetEvents.Length; i++)
                if (resetEvents[i] != null)
                    resetEvents[i].Set();

            Running = false;
        }

        /// <summary>
        /// Request a texture be downloaded, once downloaded OnImageRenderReady event will be fired
        /// containing texture key which can be used to retrieve texture with GetTextureToRender method
        /// </summary>
        /// <param name="textureID">id of Texture to request</param>
        public void RequestTexture(UUID textureID)
        {
            if (Client.Assets.Cache.HasImage(textureID))
            {
                // Add to rendering dictionary
                lock (RenderReady)
                {
                    if (!RenderReady.ContainsKey(textureID))
                    {
                        RenderReady.Add(textureID, Client.Assets.Cache.GetCachedImage(textureID));

                        // Let any subscribers know about it
                        if (OnDownloadFinished != null)
                        {
                            OnDownloadFinished(textureID, true);
                        }
                    }
                    else
                    {
                        // This image has already been served up, ignore this request
                    }
                }
            }
            else
            {
                lock (RequestQueue)
                {
                    // Make sure we aren't already downloading the texture
                    if (!RequestQueue.Contains(textureID) && !CurrentRequests.ContainsKey(textureID))
                    {
                        RequestQueue.Enqueue(textureID);
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
            lock (RenderReady)
            {
                if (RenderReady.ContainsKey(textureID))
                {
                    renderable = RenderReady[textureID];
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
            lock (RenderReady)
            {
                if (RenderReady.ContainsKey(textureID))
                    RenderReady.Remove(textureID);
            }
        }

        /// <summary>
        /// Master Download Thread, Queues up downloads in the threadpool
        /// </summary>
        private void DownloadThread()
        {
            int reqNbr;

            while (Running)
            {
                if (RequestQueue.Count > 0)
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
                        UUID requestID;
                        lock (RequestQueue)
                            requestID = RequestQueue.Dequeue();

                        Logger.DebugLog(String.Format("Sending Worker thread new download request {0}", reqNbr));
                        ThreadPool.QueueUserWorkItem(new WaitCallback(textureRequestDoWork), new TaskInfo(requestID, reqNbr));

                        continue;
                    }
                }

                // Queue was empty, let's give up some CPU time
                Thread.Sleep(500);
            }
        }

        void textureRequestDoWork(Object threadContext)
        {
            TaskInfo ti = (TaskInfo)threadContext;

            lock (CurrentRequests)
            {
                if (CurrentRequests.ContainsKey(ti.RequestID))
                {
                    threadpoolSlots[ti.RequestNbr] = -1;
                    return;
                }
                else
                {
                    CurrentRequests.Add(ti.RequestID, ti.RequestNbr);
                }
            }

            Logger.DebugLog(String.Format("Worker {0} Requesting {1}", ti.RequestNbr, ti.RequestID));

            resetEvents[ti.RequestNbr].Reset();
            Client.Assets.RequestImage(ti.RequestID, ImageType.Normal);

            // don't release this worker slot until texture is downloaded or timeout occurs
            if (!resetEvents[ti.RequestNbr].WaitOne(30 * 1000, false))
            {
                // Timed out
                Logger.Log("Worker " + ti.RequestNbr + " Timeout waiting for Texture " + ti.RequestID + " to Download", Helpers.LogLevel.Warning);

                lock (CurrentRequests)
                    CurrentRequests.Remove(ti.RequestID);
            }

            // free up this download slot
            threadpoolSlots[ti.RequestNbr] = -1;
        }

        private void Assets_OnImageReceived(ImageDownload image, AssetTexture asset)
        {
            // Free up this slot in the ThreadPool
            lock (CurrentRequests)
            {
                int requestNbr;
                if (asset != null && CurrentRequests.TryGetValue(image.ID, out requestNbr))
                {
                    Logger.DebugLog(String.Format("Worker {0} Downloaded texture {1}", requestNbr, image.ID));
                    resetEvents[requestNbr].Set();
                    CurrentRequests.Remove(image.ID);
                }
            }

            if (image.Success)
            {
                lock (RenderReady)
                {
                    if (!RenderReady.ContainsKey(image.ID))
                    {
                        // Add to rendering dictionary
                        RenderReady.Add(image.ID, image);
                    }
                }
            }
            else
            {
                Console.WriteLine(String.Format("Download of texture {0} failed. NotFound={1}", image.ID, image.NotFound));
            }

            // Let any subscribers know about it
            if (OnDownloadFinished != null)
            {
                OnDownloadFinished(image.ID, image.Success);
            }
        }

        private void Assets_OnImageReceiveProgress(UUID image, int lastPacket, int recieved, int total)
        {
            if (OnDownloadProgress != null)
            {
                OnDownloadProgress(image, recieved, total);
            }
        }
    }
}
