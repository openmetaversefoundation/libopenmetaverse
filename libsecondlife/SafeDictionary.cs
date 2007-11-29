/*
 * Copyright (c) 2007, Second Life Reverse Engineering Team
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the Second Life Reverse Engineering Team nor the names
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

namespace libsecondlife
{
    public class SafeDictionary<TKey, TValue>
    {
        internal Dictionary<TKey, TValue> Dictionary;

        public int Count { get { return Dictionary.Count; } }

        public SafeDictionary()
        {
            Dictionary = new Dictionary<TKey, TValue>();
        }

        public SafeDictionary(IDictionary<TKey, TValue> dictionary)
        {
            Dictionary = new Dictionary<TKey, TValue>(dictionary);
        }

        public SafeDictionary(int capacity)
        {
            Dictionary = new Dictionary<TKey, TValue>(capacity);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return Dictionary.TryGetValue(key, out value);
        }

        public TValue Find(Predicate<TValue> match)
        {
            lock (Dictionary)
            {
                foreach (TValue value in Dictionary.Values)
                {
                    if (match(value))
                        return value;
                }
            }
            return default(TValue);
        }

        public List<TValue> FindAll(Predicate<TValue> match)
        {
            List<TValue> found = new List<TValue>();
            lock (Dictionary)
            {
                foreach (KeyValuePair<TKey, TValue> kvp in Dictionary)
                {
                    if (match(kvp.Value))
                        found.Add(kvp.Value);
                }
            }
            return found;
        }

        public void ForEach(Action<TValue> action)
        {
            lock (Dictionary)
            {
                foreach (TValue value in Dictionary.Values)
                {
                    action(value);
                }
            }
        }
    }
}
