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

namespace OpenMetaverse
{
    public class DoubleDictionary<TKey1, TKey2, TValue>
    {
        Dictionary<TKey1, TValue> Dictionary1;
        Dictionary<TKey2, TValue> Dictionary2;
        object syncObject = new object();

        public DoubleDictionary()
        {
            Dictionary1 = new Dictionary<TKey1,TValue>();
            Dictionary2 = new Dictionary<TKey2,TValue>();
        }

        public DoubleDictionary(int capacity)
        {
            Dictionary1 = new Dictionary<TKey1, TValue>(capacity);
            Dictionary2 = new Dictionary<TKey2, TValue>(capacity);
        }

        public void Add(TKey1 key1, TKey2 key2, TValue value)
        {
            lock (syncObject)
            {
                if (Dictionary1.ContainsKey(key1))
                {
                    if (!Dictionary2.ContainsKey(key2))
                        throw new ArgumentException("key1 exists in the dictionary but not key2");
                }
                else if (Dictionary2.ContainsKey(key2))
                {
                    if (!Dictionary1.ContainsKey(key1))
                        throw new ArgumentException("key2 exists in the dictionary but not key1");
                }

                Dictionary1[key1] = value;
                Dictionary2[key2] = value;
            }
        }

        public bool Remove(TKey1 key1, TKey2 key2)
        {
            lock (syncObject)
            {
                Dictionary1.Remove(key1);
                return Dictionary2.Remove(key2);
            }
        }

        public bool Remove(TKey1 key1)
        {
            // This is an O(n) operation!
            lock (syncObject)
            {
                TValue value;
                if (Dictionary1.TryGetValue(key1, out value))
                {
                    foreach (KeyValuePair<TKey2, TValue> kvp in Dictionary2)
                    {
                        if (kvp.Value.Equals(value))
                        {
                            Dictionary1.Remove(key1);
                            Dictionary2.Remove(kvp.Key);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public bool Remove(TKey2 key2)
        {
            // This is an O(n) operation!
            lock (syncObject)
            {
                TValue value;
                if (Dictionary2.TryGetValue(key2, out value))
                {
                    foreach (KeyValuePair<TKey1, TValue> kvp in Dictionary1)
                    {
                        if (kvp.Value.Equals(value))
                        {
                            Dictionary2.Remove(key2);
                            Dictionary1.Remove(kvp.Key);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public void Clear()
        {
            lock (syncObject)
            {
                Dictionary1.Clear();
                Dictionary2.Clear();
            }
        }

        public int Count
        {
            get { return Dictionary1.Count; }
        }

        public bool ContainsKey(TKey1 key)
        {
            return Dictionary1.ContainsKey(key);
        }

        public bool ContainsKey(TKey2 key)
        {
            return Dictionary2.ContainsKey(key);
        }

        public bool TryGetValue(TKey1 key, out TValue value)
        {
            return Dictionary1.TryGetValue(key, out value);
        }

        public bool TryGetValue(TKey2 key, out TValue value)
        {
            return Dictionary2.TryGetValue(key, out value);
        }

        public void ForEach(Action<TValue> action)
        {
            lock (syncObject)
            {
                foreach (TValue value in Dictionary1.Values)
                    action(value);
            }
        }

        public TValue FindValue(Predicate<TValue> predicate)
        {
            lock (syncObject)
            {
                foreach (TValue value in Dictionary1.Values)
                {
                    if (predicate(value))
                        return value;
                }
            }

            return default(TValue);
        }

        public TValue this[TKey1 key1]
        {
            get { return Dictionary1[key1]; }
        }

        public TValue this[TKey2 key2]
        {
            get { return Dictionary2[key2]; }
        }
    }
}
