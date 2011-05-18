/*
 * Copyright (c) 2007-2008, openmetaverse.org
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
    public class InventoryNodeDictionary
    {
        protected Dictionary<UUID, InventoryNode> Dictionary = new Dictionary<UUID, InventoryNode>();
        protected InventoryNode parent;
        protected object syncRoot = new object();

        public InventoryNode Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        public object SyncRoot { get { return syncRoot; } }

        public int Count { get { return Dictionary.Count; } }

        public InventoryNodeDictionary(InventoryNode parentNode)
        {
            parent = parentNode;
        }

        public InventoryNode this[UUID key]
        {
            get { return (InventoryNode)this.Dictionary[key]; }
            set
            {
                value.Parent = parent;
                lock (syncRoot) this.Dictionary[key] = value;
            }
        }

        public ICollection<UUID> Keys { get { return this.Dictionary.Keys; } }
        public ICollection<InventoryNode> Values { get { return this.Dictionary.Values; } }

        public void Add(UUID key, InventoryNode value)
        {
            value.Parent = parent;
            lock (syncRoot) this.Dictionary.Add(key, value); 
        }

        public void Remove(UUID key)
        {
            lock (syncRoot) this.Dictionary.Remove(key);
        }

        public bool Contains(UUID key)
        {
            return this.Dictionary.ContainsKey(key);
        }
    }
}
