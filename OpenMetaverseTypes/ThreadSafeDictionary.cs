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

using System;
using System.Collections.Generic;

namespace OpenMetaverse
{
    public class ThreadSafeDictionary<TKey, TValue>
    {
        Dictionary<TKey, TValue> Dictionary;
        object syncObject = new object();

        public ThreadSafeDictionary()
        {
            Dictionary = new Dictionary<TKey, TValue>();
        }

        public ThreadSafeDictionary(int capacity)
        {
            Dictionary = new Dictionary<TKey, TValue>(capacity);
        }

        public void Add(TKey key, TValue value)
        {
            lock (syncObject)
            {
                Dictionary[key] = value;
            }
        }

        public bool Remove(TKey key)
        {
            lock (syncObject)
            {
                return Dictionary.Remove(key);
            }
        }

        public void Clear()
        {
            lock (syncObject)
            {
                Dictionary.Clear();
            }
        }

        public int Count
        {
            get { return Dictionary.Count; }
        }

        public bool ContainsKey(TKey key)
        {
            return Dictionary.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return Dictionary.TryGetValue(key, out value);
        }

        public void ForEach(Action<TValue> action)
        {
            lock (syncObject)
            {
                foreach (TValue value in Dictionary.Values)
                    action(value);
            }
        }

        public void ForEach(Action<KeyValuePair<TKey, TValue>> action)
        {
            lock (syncObject)
            {
                foreach (KeyValuePair<TKey, TValue> entry in Dictionary)
                    action(entry);
            }
        }

        public TValue FindValue(Predicate<TValue> predicate)
        {
            lock (syncObject)
            {
                foreach (TValue value in Dictionary.Values)
                {
                    if (predicate(value))
                        return value;
                }
            }

            return default(TValue);
        }

        public IList<TValue> FindAll(Predicate<TValue> predicate)
        {
            IList<TValue> list = new List<TValue>();

            lock (syncObject)
            {
                foreach (TValue value in Dictionary.Values)
                {
                    if (predicate(value))
                        list.Add(value);
                }
            }

            return list;
        }

        public int RemoveAll(Predicate<TValue> predicate)
        {
            IList<TKey> list = new List<TKey>();

            lock (syncObject)
            {
                foreach (KeyValuePair<TKey, TValue> kvp in Dictionary)
                {
                    if (predicate(kvp.Value))
                        list.Add(kvp.Key);
                }

                for (int i = 0; i < list.Count; i++)
                    Dictionary.Remove(list[i]);
            }

            return list.Count;
        }

        public TValue this[TKey key]
        {
            get { return Dictionary[key]; }
        }
    }
}
