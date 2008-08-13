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
using System.Text;

namespace OpenMetaverse
{
    public class ReversableDictionary<K, V> : IDictionary<K, V>
    {
        private Dictionary<K, V> dict;
        private Dictionary<V, K> reverseDict;

        public ReversableDictionary()
            : this(10) { }

        public ReversableDictionary(int initialCapacity)
        {
            dict = new Dictionary<K, V>(initialCapacity);
            reverseDict = new Dictionary<V, K>(initialCapacity);
        }

        #region IDictionary<K,V> Members

        public void Add(K key, V value)
        {
            dict.Add(key, value);
            reverseDict.Add(value, key);
        }

        public bool ContainsKey(K key)
        {
            return dict.ContainsKey(key);
        }

        public ICollection<K> Keys
        {
            get { return dict.Keys; }
        }

        public bool Remove(K key)
        {
            V value = dict[key];
            bool success = dict.Remove(key);
            reverseDict.Remove(value);
            return success;
        }

        public bool TryGetValue(K key, out V value)
        {
            return dict.TryGetValue(key, out value);
        }

        public bool TryGetKey(V value, out K key)
        {
            return reverseDict.TryGetValue(value, out key);
        }

        public ICollection<V> Values
        {
            get { return dict.Values; }
        }

        public V this[K key]
        {
            get
            {
                return dict[key];
            }
            set
            {
                dict[key] = value;
            }
        }

        public K this[V val]
        {
            get
            {
                return reverseDict[val];
            }
            set
            {
                reverseDict[val] = value;
            }
        }

        #endregion

        #region ICollection<KeyValuePair<K,V>> Members

        public void Add(KeyValuePair<K, V> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            dict.Clear();
            reverseDict.Clear();
        }

        public bool Contains(KeyValuePair<K, V> item)
        {
            return dict.ContainsKey(item.Key);
        }

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            foreach (KeyValuePair<K, V> kvp in dict)
                array[arrayIndex++] = kvp;
        }

        public int Count
        {
            get { return dict.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<K, V> item)
        {
            return Remove(item.Key);
        }

        #endregion

        #region IEnumerable<KeyValuePair<K,V>> Members

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            foreach (KeyValuePair<K, V> kvp in dict)
            {
                yield return kvp;
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
