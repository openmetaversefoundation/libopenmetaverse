﻿/*
 * Copyright (c) 2006-2014, openmetaverse.org
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
    /// Class that handles the local asset cache
    /// </summary>
    public class AssetCache
    {
        // User can plug in a routine to compute the asset cache location
        public delegate string ComputeAssetCacheFilenameDelegate(string cacheDir, UUID assetID);

        public ComputeAssetCacheFilenameDelegate ComputeAssetCacheFilename = null;

        private GridClient Client;
        private Thread cleanerThread;
        private System.Timers.Timer cleanerTimer;
        private double pruneInterval = 1000 * 60 * 5;
        private bool autoPruneEnabled = true;

        /// <summary>
        /// Allows setting weather to periodicale prune the cache if it grows too big
        /// Default is enabled, when caching is enabled
        /// </summary>
        public bool AutoPruneEnabled
        {
            set
            {
                autoPruneEnabled = value;

                if (autoPruneEnabled)
                {
                    SetupTimer();
                }
                else
                {
                    DestroyTimer();
                }
            }
            get { return autoPruneEnabled; }
        }

        /// <summary>
        /// How long (in ms) between cache checks (default is 5 min.) 
        /// </summary>
        public double AutoPruneInterval
        {
            set
            {
                pruneInterval = value;
                SetupTimer();
            }
            get { return pruneInterval; }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="client">A reference to the GridClient object</param>
        public AssetCache(GridClient client)
        {
            Client = client;
            Client.Network.LoginProgress += delegate(object sender, LoginProgressEventArgs e)
            {
                if (e.Status == LoginStatus.Success)
                {
                    SetupTimer();
                }
            };

            Client.Network.Disconnected += delegate(object sender, DisconnectedEventArgs e) { DestroyTimer(); };
        }


        /// <summary>
        /// Disposes cleanup timer
        /// </summary>
        private void DestroyTimer()
        {
            if (cleanerTimer != null)
            {
                cleanerTimer.Dispose();
                cleanerTimer = null;
            }
        }

        /// <summary>
        /// Only create timer when needed
        /// </summary>
        private void SetupTimer()
        {
            if (Operational() && autoPruneEnabled && Client.Network.Connected)
            {
                if (cleanerTimer == null)
                {
                    cleanerTimer = new System.Timers.Timer(pruneInterval);
                    cleanerTimer.Elapsed += new System.Timers.ElapsedEventHandler(cleanerTimer_Elapsed);
                }
                cleanerTimer.Interval = pruneInterval;
                cleanerTimer.Enabled = true;
            }
        }

        /// <summary>
        /// Return bytes read from the local asset cache, null if it does not exist
        /// </summary>
        /// <param name="assetID">UUID of the asset we want to get</param>
        /// <returns>Raw bytes of the asset, or null on failure</returns>
        public byte[] GetCachedAssetBytes(UUID assetID)
        {
            if (!Operational())
            {
                return null;
            }
            try
            {
                byte[] data;

                if (File.Exists(FileName(assetID)))
                {
                    DebugLog("Reading " + FileName(assetID) + " from asset cache.");
                    data = File.ReadAllBytes(FileName(assetID));
                }
                else
                {
                    DebugLog("Reading " + StaticFileName(assetID) + " from static asset cache.");
                    data = File.ReadAllBytes(StaticFileName(assetID));

                }
                return data;
            }
            catch (Exception ex)
            {
                DebugLog("Failed reading asset from cache (" + ex.Message + ")");
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

            byte[] imageData = GetCachedAssetBytes(imageID);
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
        /// Constructs a file name of the cached asset
        /// </summary>
        /// <param name="assetID">UUID of the asset</param>
        /// <returns>String with the file name of the cahced asset</returns>
        private string FileName(UUID assetID)
        {
            if (ComputeAssetCacheFilename != null)
            {
                return ComputeAssetCacheFilename(Client.Settings.ASSET_CACHE_DIR, assetID);
            }
            return Client.Settings.ASSET_CACHE_DIR + Path.DirectorySeparatorChar + assetID.ToString();
        }

        /// <summary>
        /// Constructs a file name of the static cached asset
        /// </summary>
        /// <param name="assetID">UUID of the asset</param>
        /// <returns>String with the file name of the static cached asset</returns>
        private string StaticFileName(UUID assetID)
        {
            return Settings.RESOURCE_DIR + Path.DirectorySeparatorChar + "static_assets" + Path.DirectorySeparatorChar + assetID.ToString();
        }

        /// <summary>
        /// Saves an asset to the local cache
        /// </summary>
        /// <param name="assetID">UUID of the asset</param>
        /// <param name="assetData">Raw bytes the asset consists of</param>
        /// <returns>Weather the operation was successfull</returns>
        public bool SaveAssetToCache(UUID assetID, byte[] assetData)
        {
            if (!Operational())
            {
                return false;
            }

            try
            {
                DebugLog("Saving " + FileName(assetID) + " to asset cache.");

                if (!Directory.Exists(Client.Settings.ASSET_CACHE_DIR))
                {
                    Directory.CreateDirectory(Client.Settings.ASSET_CACHE_DIR);
                }

                File.WriteAllBytes(FileName(assetID), assetData);
            }
            catch (Exception ex)
            {
                Logger.Log("Failed saving asset to cache (" + ex.Message + ")", Helpers.LogLevel.Warning, Client);
                return false;
            }

            return true;
        }

        private void DebugLog(string message)
        {
            if (Client.Settings.LOG_DISKCACHE) Logger.DebugLog(message, Client);
        }

        /// <summary>
        /// Get the file name of the asset stored with gived UUID
        /// </summary>
        /// <param name="assetID">UUID of the asset</param>
        /// <returns>Null if we don't have that UUID cached on disk, file name if found in the cache folder</returns>
        public string AssetFileName(UUID assetID)
        {
            if (!Operational())
            {
                return null;
            }

            string fileName = FileName(assetID);

            if (File.Exists(fileName))
                return fileName;
            else
                return null;
        }

        /// <summary>
        /// Checks if the asset exists in the local cache
        /// </summary>
        /// <param name="assetID">UUID of the asset</param>
        /// <returns>True is the asset is stored in the cache, otherwise false</returns>
        public bool HasAsset(UUID assetID)
        {
            if (!Operational())
                return false;
            else
                if (File.Exists(FileName(assetID)))
                    return true;
                else
                    return File.Exists(StaticFileName(assetID));

        }

        /// <summary>
        /// Wipes out entire cache
        /// </summary>
        public void Clear()
        {
            string cacheDir = Client.Settings.ASSET_CACHE_DIR;
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

            DebugLog("Wiped out " + num + " files from the cache directory.");
        }

        /// <summary>
        /// Brings cache size to the 90% of the max size
        /// </summary>
        public void Prune()
        {
            string cacheDir = Client.Settings.ASSET_CACHE_DIR;
            if (!Directory.Exists(cacheDir))
            {
                return;
            }
            DirectoryInfo di = new DirectoryInfo(cacheDir);
            // We save file with UUID as file name, only count those
            FileInfo[] files = di.GetFiles("????????-????-????-????-????????????", SearchOption.TopDirectoryOnly);

            long size = GetFileSize(files);

            if (size > Client.Settings.ASSET_CACHE_MAX_SIZE)
            {
                Array.Sort(files, new SortFilesByAccesTimeHelper());
                long targetSize = (long)(Client.Settings.ASSET_CACHE_MAX_SIZE * 0.9);
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
                DebugLog(num + " files deleted from the cache, cache size now: " + NiceFileSize(size));
            }
            else
            {
                DebugLog("Cache size is " + NiceFileSize(size) + ", file deletion not needed");
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
            return Client.Settings.USE_ASSET_CACHE;
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
