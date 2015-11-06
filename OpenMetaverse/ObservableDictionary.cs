/*
 * Copyright (c) 2006-2016, openmetaverse.co
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.co nor the names
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
using System.Collections;

namespace OpenMetaverse
{

    /// <summary>
    /// 
    /// </summary>
    public enum DictionaryEventAction
    {
        /// <summary>
        /// 
        /// </summary>
        Add,
        /// <summary>
        /// 
        /// </summary>
        Remove,
        /// <summary>
        /// 
        /// </summary>
        Change
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="action"></param>
    /// <param name="entry"></param>
    public delegate void DictionaryChangeCallback(DictionaryEventAction action, DictionaryEntry entry);

    /// <summary>
    /// The ObservableDictionary class is used for storing key/value pairs. It has methods for firing
    /// events to subscribers when items are added, removed, or changed.
    /// </summary>
    /// <typeparam name="TKey">Key <see langword="Tkey"/></typeparam>
    /// <typeparam name="TValue">Value <see langword="TValue"/></typeparam>
    public class ObservableDictionary<TKey, TValue>
    {
        #region Observable implementation
        /// <summary>
        /// A dictionary of callbacks to fire when specified action occurs
        /// </summary>
        private Dictionary<DictionaryEventAction, List<DictionaryChangeCallback>> Delegates;

        /// <summary>
        /// Register a callback to be fired when an action occurs
        /// </summary>
        /// <param name="action">The action</param>
        /// <param name="callback">The callback to fire</param>
        public void AddDelegate(DictionaryEventAction action, DictionaryChangeCallback callback)
        {
            if (Delegates.ContainsKey(action))
            {
                Delegates[action].Add(callback);   
            }
            else
            {
                List<DictionaryChangeCallback> callbacks = new List<DictionaryChangeCallback>(1);
                callbacks.Add(callback);
                Delegates.Add(action, callbacks);
            }
        }

        /// <summary>
        /// Unregister a callback
        /// </summary>
        /// <param name="action">The action</param>
        /// <param name="callback">The callback to fire</param>
        public void RemoveDelegate(DictionaryEventAction action, DictionaryChangeCallback callback)
        {
            if (Delegates.ContainsKey(action))
            {
                if (Delegates[action].Contains(callback))
                    Delegates[action].Remove(callback);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="entry"></param>
        private void FireChangeEvent(DictionaryEventAction action, DictionaryEntry entry)
        {
            
            if(Delegates.ContainsKey(action))
            {
                foreach(DictionaryChangeCallback handler in Delegates[action])
                {
                    handler(action, entry);
                }
            }
        }

        #endregion

        /// <summary>Internal dictionary that this class wraps around. Do not
        /// modify or enumerate the contents of this dictionary without locking</summary>
        private Dictionary<TKey, TValue> Dictionary;

        /// <summary>
        /// Gets the number of Key/Value pairs contained in the <seealso cref="T:ObservableDictionary"/>
        /// </summary>
        public int Count { get { return Dictionary.Count; } }

        /// <summary>
        /// Initializes a new instance of the <seealso cref="T:ObservableDictionary"/> Class 
        /// with the specified key/value, has the default initial capacity.
        /// </summary>
        /// <example>
        /// <code>
        /// // initialize a new ObservableDictionary named testDict with a string as the key and an int as the value.
        /// public ObservableDictionary&lt;string, int&gt; testDict = new ObservableDictionary&lt;string, int&gt;();
        /// </code>
        /// </example>
        public ObservableDictionary()
        {
            Dictionary = new Dictionary<TKey, TValue>();
            Delegates = new Dictionary<DictionaryEventAction, List<DictionaryChangeCallback>>();
        }

        /// <summary>
        /// Initializes a new instance of the <seealso cref="T:OpenMetaverse.ObservableDictionary"/> Class 
        /// with the specified key/value, With its initial capacity specified.
        /// </summary>
        /// <param name="capacity">Initial size of dictionary</param>
        /// <example>
        /// <code>
        /// // initialize a new ObservableDictionary named testDict with a string as the key and an int as the value, 
        /// // initially allocated room for 10 entries.
        /// public ObservableDictionary&lt;string, int&gt; testDict = new ObservableDictionary&lt;string, int&gt;(10);
        /// </code>
        /// </example>
        public ObservableDictionary(int capacity)
        {
            Dictionary = new Dictionary<TKey, TValue>(capacity);
            Delegates = new Dictionary<DictionaryEventAction, List<DictionaryChangeCallback>>();
        }

        /// <summary>
        /// Try to get entry from the <seealso cref="ObservableDictionary"/> with specified key 
        /// </summary>
        /// <param name="key">Key to use for lookup</param>
        /// <param name="value">Value returned</param>
        /// <returns><see langword="true"/> if specified key exists,  <see langword="false"/> if not found</returns>
        /// <example>
        /// <code>
        /// // find your avatar using the Simulator.ObjectsAvatars ObservableDictionary:
        ///    Avatar av;
        ///    if (Client.Network.CurrentSim.ObjectsAvatars.TryGetValue(Client.Self.AgentID, out av))
        ///        Console.WriteLine("Found Avatar {0}", av.Name);
        /// </code>
        /// <seealso cref="Simulator.ObjectsAvatars"/>
        /// </example>
        public bool TryGetValue(TKey key, out TValue value)
        {
                return Dictionary.TryGetValue(key, out value);
        }

        /// <summary>
        /// Finds the specified match.
        /// </summary>
        /// <param name="match">The match.</param>
        /// <returns>Matched value</returns>
        /// <example>
        /// <code>
        /// // use a delegate to find a prim in the ObjectsPrimitives ObservableDictionary
        /// // with the ID 95683496
        /// uint findID = 95683496;
        /// Primitive findPrim = sim.ObjectsPrimitives.Find(
        ///             delegate(Primitive prim) { return prim.ID == findID; });
        /// </code>
        /// </example>
        public TValue Find(Predicate<TValue> match)
        {
                foreach (TValue value in Dictionary.Values)
                {
                    if (match(value))
                        return value;
                }
            return default(TValue);
        }

        /// <summary>Find All items in an <seealso cref="T:ObservableDictionary"/></summary>
        /// <param name="match">return matching items.</param>
        /// <returns>a <seealso cref="T:System.Collections.Generic.List"/> containing found items.</returns>
        /// <example>
        /// Find All prims within 20 meters and store them in a List
        /// <code>
        /// int radius = 20;
        /// List&lt;Primitive&gt; prims = Client.Network.CurrentSim.ObjectsPrimitives.FindAll(
        ///         delegate(Primitive prim) {
        ///             Vector3 pos = prim.Position;
        ///             return ((prim.ParentID == 0) &amp;&amp; (pos != Vector3.Zero) &amp;&amp; (Vector3.Distance(pos, location) &lt; radius));
        ///         }
        ///    ); 
        ///</code>
        ///</example>
        public List<TValue> FindAll(Predicate<TValue> match)
        {
            List<TValue> found = new List<TValue>();
         
                foreach (KeyValuePair<TKey, TValue> kvp in Dictionary)
                {
                    if (match(kvp.Value))
                        found.Add(kvp.Value);
                }
                     return found;
        }

        /// <summary>Find All items in an <seealso cref="T:ObservableDictionary"/></summary>
        /// <param name="match">return matching keys.</param>
        /// <returns>a <seealso cref="T:System.Collections.Generic.List"/> containing found keys.</returns>
        /// <example>
        /// Find All keys which also exist in another dictionary
        /// <code>
        /// List&lt;UUID&gt; matches = myDict.FindAll(
        ///         delegate(UUID id) {
        ///             return myOtherDict.ContainsKey(id);
        ///         }
        ///    ); 
        ///</code>
        ///</example>
        public List<TKey> FindAll(Predicate<TKey> match)
        {
            List<TKey> found = new List<TKey>();
         
                foreach (KeyValuePair<TKey, TValue> kvp in Dictionary)
                {
                    if (match(kvp.Key))
                        found.Add(kvp.Key);
                }
         
            return found;
        }

        /// <summary>Check if Key exists in Dictionary</summary>
        /// <param name="key">Key to check for</param>
        /// <returns><see langword="true"/> if found, <see langword="false"/> otherwise</returns>
        public bool ContainsKey(TKey key)
        {
                return Dictionary.ContainsKey(key);
        }

        /// <summary>Check if Value exists in Dictionary</summary>
        /// <param name="value">Value to check for</param>
        /// <returns><see langword="true"/> if found, <see langword="false"/> otherwise</returns>
        public bool ContainsValue(TValue value)
        {
                return Dictionary.ContainsValue(value);
        }

        /// <summary>
        /// Adds the specified key to the dictionary, dictionary locking is not performed, 
        /// <see cref="SafeAdd"/>
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="value">The value</param>
        public void Add(TKey key, TValue value)
        {
            Dictionary.Add(key, value);
            FireChangeEvent(DictionaryEventAction.Add, new DictionaryEntry(key, value));
        }

        /// <summary>
        /// Removes the specified key, dictionary locking is not performed
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><see langword="true"/> if successful, <see langword="false"/> otherwise</returns>
        public bool Remove(TKey key)
        {
            FireChangeEvent(DictionaryEventAction.Remove, new DictionaryEntry(key, Dictionary[key]));
            return Dictionary.Remove(key);
        }

        /// <summary>
        /// Indexer for the dictionary
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>The value</returns>
        public TValue this[TKey key]
        {
            get { return Dictionary[key]; }
            set { FireChangeEvent(DictionaryEventAction.Add, new DictionaryEntry(key, value));
                Dictionary[key] = value; }
        }

        /// <summary>
        /// Clear the contents of the dictionary
        /// </summary>
        public void Clear()
        {
            foreach (KeyValuePair<TKey, TValue> kvp in Dictionary)
                FireChangeEvent(DictionaryEventAction.Remove, new DictionaryEntry(kvp.Key, kvp.Value));

            Dictionary.Clear();
        }

        /// <summary>
        /// Enumerator for iterating dictionary entries
        /// </summary>
        /// <returns></returns>
        public System.Collections.IEnumerator GetEnumerator()
        {
            return Dictionary.GetEnumerator();
        }
    }
}
