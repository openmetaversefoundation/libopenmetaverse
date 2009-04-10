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
using System.IO;
using System.Threading;

namespace OpenMetaverse
{
    /// <summary>
    /// Class that handles the local image cache
    /// </summary>
    public class TextureCache
    {
        // User can plug in a routine to compute the texture cache location
        public delegate string ComputeTextureCacheFilenameDelegate(string cacheDir, UUID textureID);

        public ComputeTextureCacheFilenameDelegate ComputeTextureCacheFilename = null;

        private GridClient Client;
        private Thread cleanerThread;
        private System.Timers.Timer cleanerTimer;
        private double pruneInterval = 1000 * 60 * 5;

        /// <summary>
        /// Allows setting weather to periodicale prune the cache if it grows too big
        /// Default is enabled, when caching is enabled
        /// </summary>
        public bool AutoPruneEnabled
        {
            set
            {
                if (!Operational())
                {
                    return;
                }
                else
                {
                    cleanerTimer.Enabled = value;
                }
            }
            get { return cleanerTimer.Enabled; }
        }

        /// <summary>
        /// How long (in ms) between cache checks (default is 5 min.) 
        /// </summary>
        public double AutoPruneInterval
        {
            get { return pruneInterval; }
            set
            {
                pruneInterval = value;
                cleanerTimer.Interval = pruneInterval;
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="client">A reference to the GridClient object</param>
        public TextureCache(GridClient client)
        {
            Client = client;
            cleanerTimer = new System.Timers.Timer(pruneInterval);
            cleanerTimer.Elapsed += new System.Timers.ElapsedEventHandler(cleanerTimer_Elapsed);
            if (Operational())
            {
                cleanerTimer.Enabled = true;
            }
            else
            {
                cleanerTimer.Enabled = false;
            }
        }

        /// <summary>
        /// Return bytes read from the local image cache, null if it does not exist
        /// </summary>
        /// <param name="imageID">UUID of the image we want to get</param>
        /// <returns>Raw bytes of the image, or null on failure</returns>
        public byte[] GetCachedImageBytes(UUID imageID)
        {
            if (!Operational())
            {
                return null;
            }
            try
            {
                Logger.DebugLog("Reading " + FileName(imageID) + " from texture cache.");
                byte[] data = File.ReadAllBytes(FileName(imageID));
                return data;
            }
            catch (Exception ex)
            {
                Logger.Log("Failed reading image from cache (" + ex.Message + ")", Helpers.LogLevel.Warning, Client);
                return null;
            }
        }

        /// <summary>
        /// Returns ImageDownload object of the
        /// image from the local image cache, null if it does not exist
        /// </summary>
        /// <param name="imageID">UUID of the image we want to get</param>
        /// <returns>ImageDownload object containing the image, or null on failure</returns>
        public ImageDownload GetCachedImage(UUID imageID)
        {
            if (!Operational())
                return null;

            byte[] imageData = GetCachedImageBytes(imageID);
            if (imageData == null)
                return null;
            ImageDownload transfer = new ImageDownload();
            transfer.AssetType = AssetType.Texture;
            transfer.ID = imageID;
            transfer.Simulator = Client.Network.CurrentSim;
            transfer.Size = imageData.Length;
            transfer.Success = true;
            transfer.Transferred = imageData.Length;
            transfer.AssetData = imageData;
            return transfer;
        }

        /// <summary>
        /// Constructs a file name of the cached image
        /// </summary>
        /// <param name="imageID">UUID of the image</param>
        /// <returns>String with the file name of the cahced image</returns>
        private string FileName(UUID imageID)
        {
            if (ComputeTextureCacheFilename != null) {
                return ComputeTextureCacheFilename(Client.Settings.TEXTURE_CACHE_DIR, imageID);
            }
            return Client.Settings.TEXTURE_CACHE_DIR + Path.DirectorySeparatorChar + imageID.ToString();
        }

        /// <summary>
        /// Saves an image to the local cache
        /// </summary>
        /// <param name="imageID">UUID of the image</param>
        /// <param name="imageData">Raw bytes the image consists of</param>
        /// <returns>Weather the operation was successfull</returns>
        public bool SaveImageToCache(UUID imageID, byte[] imageData)
        {
            if (!Operational())
            {
                return false;
            }

            try
            {
                Logger.DebugLog("Saving " + FileName(imageID) + " to texture cache.", Client);

                if (!Directory.Exists(Client.Settings.TEXTURE_CACHE_DIR))
                {
                    Directory.CreateDirectory(Client.Settings.TEXTURE_CACHE_DIR);
                }

                File.WriteAllBytes(FileName(imageID), imageData);
            }
            catch (Exception ex)
            {
                Logger.Log("Failed saving image to cache (" + ex.Message + ")", Helpers.LogLevel.Warning, Client);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get the file name of the asset stored with gived UUID
        /// </summary>
        /// <param name="imageID">UUID of the image</param>
        /// <returns>Null if we don't have that UUID cached on disk, file name if found in the cache folder</returns>
        public string ImageFileName(UUID imageID)
        {
            if (!Operational())
            {
                return null;
            }

            string fileName = FileName(imageID);

            if (File.Exists(fileName))
                return fileName;
            else
                return null;
        }

        /// <summary>
        /// Checks if the image exists in the local cache
        /// </summary>
        /// <param name="imageID">UUID of the image</param>
        /// <returns>True is the image is stored in the cache, otherwise false</returns>
        public bool HasImage(UUID imageID)
        {
            if (!Operational())
                return false;
            else
                return File.Exists(FileName(imageID));
        }

        /// <summary>
        /// Wipes out entire cache
        /// </summary>
        public void Clear()
        {
            string cacheDir = Client.Settings.TEXTURE_CACHE_DIR;
            if (!Directory.Exists(cacheDir))
            {
                return;
            }

            DirectoryInfo di = new DirectoryInfo(cacheDir);
            // We save file with UUID as file name, only delete those
            FileInfo[] files = di.GetFiles("????????-????-????-????-????????????", SearchOption.TopDirectoryOnly);

            int num = 0;
            foreach (FileInfo file in files)
            {
                file.Delete();
                ++num;
            }

            Logger.Log("Wiped out " + num + " files from the cache directory.", Helpers.LogLevel.Debug);
        }

        /// <summary>
        /// Brings cache size to the 90% of the max size
        /// </summary>
        public void Prune()
        {
            string cacheDir = Client.Settings.TEXTURE_CACHE_DIR;
            if (!Directory.Exists(cacheDir))
            {
                return;
            }
            DirectoryInfo di = new DirectoryInfo(cacheDir);
            // We save file with UUID as file name, only count those
            FileInfo[] files = di.GetFiles("????????-????-????-????-????????????", SearchOption.TopDirectoryOnly);

            long size = GetFileSize(files);

            if (size > Client.Settings.TEXTURE_CACHE_MAX_SIZE)
            {
                Array.Sort(files, new SortFilesByAccesTimeHelper());
                long targetSize = (long)(Client.Settings.TEXTURE_CACHE_MAX_SIZE * 0.9);
                int num = 0;
                foreach (FileInfo file in files)
                {
                    ++num;
                    size -= file.Length;
                    file.Delete();
                    if (size < targetSize)
                    {
                        break;
                    }
                }
                Logger.Log(num + " files deleted from the cache, cache size now: " + NiceFileSize(size), Helpers.LogLevel.Debug);
            }
            else
            {
                Logger.Log("Cache size is " + NiceFileSize(size) + ", file deletion not needed", Helpers.LogLevel.Debug);
            }

        }

        /// <summary>
        /// Asynchronously brings cache size to the 90% of the max size
        /// </summary>
        public void BeginPrune()
        {
            // Check if the background cache cleaning thread is active first
            if (cleanerThread != null && cleanerThread.IsAlive)
            {
                return;
            }

            lock (this)
            {
                cleanerThread = new Thread(new ThreadStart(this.Prune));
                cleanerThread.IsBackground = true;
                cleanerThread.Start();
            }
        }

        /// <summary>
        /// Adds up file sizes passes in a FileInfo array
        /// </summary>
        long GetFileSize(FileInfo[] files)
        {
            long ret = 0;
            foreach (FileInfo file in files)
            {
                ret += file.Length;
            }
            return ret;
        }

        /// <summary>
        /// Checks whether caching is enabled
        /// </summary>
        private bool Operational()
        {
            return Client.Settings.USE_TEXTURE_CACHE;
        }

        /// <summary>
        /// Periodically prune the cache
        /// </summary>
        private void cleanerTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            BeginPrune();
        }

        /// <summary>
        /// Nicely formats file sizes
        /// </summary>
        /// <param name="byteCount">Byte size we want to output</param>
        /// <returns>String with humanly readable file size</returns>
        private string NiceFileSize(long byteCount)
        {
            string size = "0 Bytes";
            if (byteCount >= 1073741824)
                size = String.Format("{0:##.##}", byteCount / 1073741824) + " GB";
            else if (byteCount >= 1048576)
                size = String.Format("{0:##.##}", byteCount / 1048576) + " MB";
            else if (byteCount >= 1024)
                size = String.Format("{0:##.##}", byteCount / 1024) + " KB";
            else if (byteCount > 0 && byteCount < 1024)
                size = byteCount.ToString() + " Bytes";

            return size;
        }

        /// <summary>
        /// Helper class for sorting files by their last accessed time
        /// </summary>
        private class SortFilesByAccesTimeHelper : IComparer<FileInfo>
        {
            int IComparer<FileInfo>.Compare(FileInfo f1, FileInfo f2)
            {
                if (f1.LastAccessTime > f2.LastAccessTime)
                    return 1;
                if (f1.LastAccessTime < f2.LastAccessTime)
                    return -1;
                else
                    return 0;
            }
        }
    }
}
