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
using System.Threading;
using System.Collections.Generic;

namespace OpenMetaverse
{
    #region TimedCacheKey Class

    class TimedCacheKey<TKey> : IComparable<TKey>
    {
        private DateTime expirationDate;
        private bool slidingExpiration;
        private TimeSpan slidingExpirationWindowSize;
        private TKey key;

        public DateTime ExpirationDate { get { return expirationDate; } }
        public TKey Key { get { return key; } }
        public bool SlidingExpiration { get { return slidingExpiration; } }
        public TimeSpan SlidingExpirationWindowSize { get { return slidingExpirationWindowSize; } }

        public TimedCacheKey(TKey key, DateTime expirationDate)
        {
            this.key = key;
            this.slidingExpiration = false;
            this.expirationDate = expirationDate;
        }

        public TimedCacheKey(TKey key, TimeSpan slidingExpirationWindowSize)
        {
            this.key = key;
            this.slidingExpiration = true;
            this.slidingExpirationWindowSize = slidingExpirationWindowSize;
            Accessed();
        }

        public void Accessed()
        {
            if (slidingExpiration)
                expirationDate = DateTime.Now.Add(slidingExpirationWindowSize);
        }

        public int CompareTo(TKey other)
        {
            return key.GetHashCode().CompareTo(other.GetHashCode());
        }
    }

    #endregion

    public sealed class ExpiringCache<TKey, TValue>
    {
        #region Private fields

        /// <summary>For thread safety</summary>
        ReaderWriterLock readWriteLock = new ReaderWriterLock();
        const double CACHE_PURGE_HZ = 1.0;
        const int MAX_LOCK_WAIT = 5000; // milliseconds

        Dictionary<TimedCacheKey<TKey>, TValue> timedStorage = new Dictionary<TimedCacheKey<TKey>, TValue>();
        Dictionary<TKey, TimedCacheKey<TKey>> timedStorageIndex = new Dictionary<TKey, TimedCacheKey<TKey>>();
        private System.Timers.Timer timer = new System.Timers.Timer(TimeSpan.FromSeconds(CACHE_PURGE_HZ).TotalMilliseconds);
        object isPurging = new object();

        #endregion

        #region Constructor

        public ExpiringCache()
        {
            timer.Elapsed += PurgeCache;
            timer.Start();
        }

        #endregion

        #region Public methods

        public bool Add(TKey key, TValue value, DateTime expiration)
        {
            // Synchronise access to storage structures. A read lock may
            // already be acquired before this method is called.
            bool LockUpgraded = readWriteLock.IsReaderLockHeld;
            LockCookie lc = new LockCookie();
            if (LockUpgraded)
            {
                lc = readWriteLock.UpgradeToWriterLock(MAX_LOCK_WAIT);
            }
            else
            {
                readWriteLock.AcquireWriterLock(MAX_LOCK_WAIT);
            }
            try
            {
                // This is the actual adding of the key
                if (timedStorageIndex.ContainsKey(key))
                {
                    return false;
                }
                else
                {
                    TimedCacheKey<TKey> internalKey = new TimedCacheKey<TKey>(key, expiration);
                    timedStorage.Add(internalKey, value);
                    timedStorageIndex.Add(key, internalKey);
                    return true;
                }
            }
            finally
            {
                // Restore lock state
                if (LockUpgraded)
                {
                    readWriteLock.DowngradeFromWriterLock(ref lc);
                }
                else
                {
                    readWriteLock.ReleaseWriterLock();
                }
            }
        }

        public bool Add(TKey key, TValue value, TimeSpan slidingExpiration)
        {
            // Synchronise access to storage structures. A read lock may
            // already be acquired before this method is called.
            bool LockUpgraded = readWriteLock.IsReaderLockHeld;
            LockCookie lc = new LockCookie();
            if (LockUpgraded)
            {
                lc = readWriteLock.UpgradeToWriterLock(MAX_LOCK_WAIT);
            }
            else
            {
                readWriteLock.AcquireWriterLock(MAX_LOCK_WAIT);
            }
            try
            {
                // This is the actual adding of the key
                if (timedStorageIndex.ContainsKey(key))
                {
                    return false;
                }
                else
                {
                    TimedCacheKey<TKey> internalKey = new TimedCacheKey<TKey>(key, slidingExpiration);
                    timedStorage.Add(internalKey, value);
                    timedStorageIndex.Add(key, internalKey);
                    return true;
                }
            }
            finally
            {
                // Restore lock state
                if (LockUpgraded)
                {
                    readWriteLock.DowngradeFromWriterLock(ref lc);
                }
                else
                {
                    readWriteLock.ReleaseWriterLock();
                }
            }
        }

        public bool AddOrUpdate(TKey key, TValue value, DateTime expiration)
        {
            readWriteLock.AcquireReaderLock(MAX_LOCK_WAIT);
            try
            {
                if (Contains(key))
                {
                    Update(key, value, expiration);
                    return false;
                }
                else
                {
                    Add(key, value, expiration);
                    return true;
                }
            }
            finally { readWriteLock.ReleaseReaderLock(); }
        }

        public bool AddOrUpdate(TKey key, TValue value, TimeSpan slidingExpiration)
        {
            readWriteLock.AcquireReaderLock(MAX_LOCK_WAIT);
            try
            {
                if (Contains(key))
                {
                    Update(key, value, slidingExpiration);
                    return false;
                }
                else
                {
                    Add(key, value, slidingExpiration);
                    return true;
                }
            }
            finally { readWriteLock.ReleaseReaderLock(); }
        }

        public void Clear()
        {
            readWriteLock.AcquireWriterLock(MAX_LOCK_WAIT);
            try
            {
                timedStorage.Clear();
                timedStorageIndex.Clear();
            }
            finally { readWriteLock.ReleaseWriterLock(); }
        }

        public bool Contains(TKey key)
        {
            readWriteLock.AcquireReaderLock(MAX_LOCK_WAIT);
            try
            {
                return timedStorageIndex.ContainsKey(key);
            }
            finally { readWriteLock.ReleaseReaderLock(); }
        }

        public int Count
        {
            get
            {
                return timedStorage.Count;
            }
        }

        public object this[TKey key]
        {
            get
            {
                TValue o;
                readWriteLock.AcquireWriterLock(MAX_LOCK_WAIT);
                try
                {
                    if (timedStorageIndex.ContainsKey(key))
                    {
                        TimedCacheKey<TKey> tkey = timedStorageIndex[key];
                        o = timedStorage[tkey];
                        timedStorage.Remove(tkey);
                        tkey.Accessed();
                        timedStorage.Add(tkey, o);
                        return o;
                    }
                    else
                    {
                        throw new ArgumentException("Key not found in the cache");
                    }
                }
                finally { readWriteLock.ReleaseWriterLock(); }
            }
        }

        public TValue this[TKey key, DateTime expiration]
        {
            set
            {
                AddOrUpdate(key, value, expiration);
            }
        }

        public TValue this[TKey key, TimeSpan slidingExpiration]
        {
            set
            {
                AddOrUpdate(key, value, slidingExpiration);
            }
        }

        public bool Remove(TKey key)
        {
            readWriteLock.AcquireWriterLock(MAX_LOCK_WAIT);
            try
            {
                if (timedStorageIndex.ContainsKey(key))
                {
                    timedStorage.Remove(timedStorageIndex[key]);
                    timedStorageIndex.Remove(key);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            finally { readWriteLock.ReleaseWriterLock(); }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            TValue o;

            readWriteLock.AcquireWriterLock(MAX_LOCK_WAIT);
            try
            {
                if (timedStorageIndex.ContainsKey(key))
                {
                    TimedCacheKey<TKey> tkey = timedStorageIndex[key];
                    o = timedStorage[tkey];
                    timedStorage.Remove(tkey);
                    tkey.Accessed();
                    timedStorage.Add(tkey, o);
                    value = o;
                    return true;
                }
                else
                {
                    value = default(TValue);
                    return false;
                }
            }
            finally { readWriteLock.ReleaseReaderLock(); }
        }

        /// <summary>
        /// Enumerates over all of the stored values without updating access times
        /// </summary>
        /// <param name="action">Action to perform on all of the elements</param>
        public void ForEach(Action<TValue> action)
        {
            readWriteLock.AcquireReaderLock(MAX_LOCK_WAIT);
            try
            {
                foreach (TValue value in timedStorage.Values)
                    action(value);
            }
            finally { readWriteLock.ReleaseReaderLock(); }
        }

        public bool Update(TKey key, TValue value)
        {
            // Synchronise access to storage structures. A read lock may
            // already be acquired before this method is called.
            LockCookie lc = new LockCookie();
            bool lockUpgrade = readWriteLock.IsReaderLockHeld;

            if (lockUpgrade)
                lc = readWriteLock.UpgradeToWriterLock(MAX_LOCK_WAIT);
            else
                readWriteLock.AcquireWriterLock(MAX_LOCK_WAIT);

            try
            {
                if (timedStorageIndex.ContainsKey(key))
                {
                    timedStorage.Remove(timedStorageIndex[key]);
                    timedStorageIndex[key].Accessed();
                    timedStorage.Add(timedStorageIndex[key], value);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            finally
            {
                // Restore lock state
                if (lockUpgrade)
                    readWriteLock.DowngradeFromWriterLock(ref lc);
                else
                    readWriteLock.ReleaseWriterLock();
            }
        }

        public bool Update(TKey key, TValue value, DateTime expiration)
        {
            // Synchronise access to storage structures. A read lock may
            // already be acquired before this method is called.
            LockCookie lc = new LockCookie();
            bool lockUpgrade = readWriteLock.IsReaderLockHeld;
            
            if (lockUpgrade)
                lc = readWriteLock.UpgradeToWriterLock(MAX_LOCK_WAIT);
            else
                readWriteLock.AcquireWriterLock(MAX_LOCK_WAIT);

            try
            {
                if (timedStorageIndex.ContainsKey(key))
                {
                    timedStorage.Remove(timedStorageIndex[key]);
                    timedStorageIndex.Remove(key);
                }
                else
                {
                    return false;
                }

                TimedCacheKey<TKey> internalKey = new TimedCacheKey<TKey>(key, expiration);
                timedStorage.Add(internalKey, value);
                timedStorageIndex.Add(key, internalKey);
                return true;
            }
            finally
            {
                // Restore lock state
                if (lockUpgrade)
                    readWriteLock.DowngradeFromWriterLock(ref lc);
                else
                    readWriteLock.ReleaseWriterLock();
            }
        }

        public bool Update(TKey key, TValue value, TimeSpan slidingExpiration)
        {
            // Synchronise access to storage structures. A read lock may
            // already be acquired before this method is called.
            LockCookie lc = new LockCookie();
            bool lockUpgrade = readWriteLock.IsReaderLockHeld;
            
            if (lockUpgrade)
                lc = readWriteLock.UpgradeToWriterLock(MAX_LOCK_WAIT);
            else
                readWriteLock.AcquireWriterLock(MAX_LOCK_WAIT);

            try
            {
                if (timedStorageIndex.ContainsKey(key))
                {
                    timedStorage.Remove(timedStorageIndex[key]);
                    timedStorageIndex.Remove(key);
                }
                else
                {
                    return false;
                }

                TimedCacheKey<TKey> internalKey = new TimedCacheKey<TKey>(key, slidingExpiration);
                timedStorage.Add(internalKey, value);
                timedStorageIndex.Add(key, internalKey);
                return true;
            }
            finally
            {
                // Restore lock state
                if (lockUpgrade)
                    readWriteLock.DowngradeFromWriterLock(ref lc);
                else
                    readWriteLock.ReleaseWriterLock();
            }
        }

        public void CopyTo(Array array, int startIndex)
        {
            // Error checking
            if (array == null) { throw new ArgumentNullException("array"); }

            if (startIndex < 0) { throw new ArgumentOutOfRangeException("startIndex", "startIndex must be >= 0."); }

            if (array.Rank > 1) { throw new ArgumentException("array must be of Rank 1 (one-dimensional)", "array"); }
            if (startIndex >= array.Length) { throw new ArgumentException("startIndex must be less than the length of the array.", "startIndex"); }
            if (Count > array.Length - startIndex) { throw new ArgumentException("There is not enough space from startIndex to the end of the array to accomodate all items in the cache."); }

            // Copy the data to the array (in a thread-safe manner)
            readWriteLock.AcquireReaderLock(MAX_LOCK_WAIT);
            try
            {
                foreach (object o in timedStorage)
                {
                    array.SetValue(o, startIndex);
                    startIndex++;
                }
            }
            finally { readWriteLock.ReleaseReaderLock(); }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Purges expired objects from the cache. Called automatically by the purge timer.
        /// </summary>
        private void PurgeCache(object sender, System.Timers.ElapsedEventArgs e)
        {
            // Note: This implementation runs with low priority. If the cache lock
            // is heavily contended (many threads) the purge will take a long time
            // to obtain the lock it needs and may never be run.
            Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

            // Only let one thread purge at once - a buildup could cause a crash
            // This could cause the purge to be delayed while there are lots of read/write ops 
            // happening on the cache
            if (!Monitor.TryEnter(isPurging))
                return;

            try
            {
                readWriteLock.AcquireWriterLock(MAX_LOCK_WAIT);
                try
                {
                    List<object> expiredItems = new List<object>();

                    foreach (TimedCacheKey<TKey> timedKey in timedStorage.Keys)
                    {
                        if (timedKey.ExpirationDate < e.SignalTime)
                        {
                            // Mark the object for purge
                            expiredItems.Add(timedKey.Key);
                        }
                        else
                        {
                            break;
                        }
                    }

                    foreach (TKey key in expiredItems)
                    {
                        TimedCacheKey<TKey> timedKey = timedStorageIndex[key];
                        timedStorageIndex.Remove(timedKey.Key);
                        timedStorage.Remove(timedKey);
                    }
                }
                catch (ApplicationException)
                {
                    // Unable to obtain write lock to the timed cache storage object
                }
                finally
                {
                    readWriteLock.ReleaseWriterLock();
                }
            }
            finally { Monitor.Exit(isPurging); }
        }

        #endregion
    }
}
