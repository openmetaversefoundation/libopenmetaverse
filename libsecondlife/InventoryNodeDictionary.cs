using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace libsecondlife
{
    public class InventoryNodeDictionary : DictionaryBase
    {
        protected InventoryNode parent;
        protected object syncRoot = new object();

        public InventoryNode Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        public object SyncRoot { get { return syncRoot; } }

        public InventoryNodeDictionary(InventoryNode parentNode)
        {
            parent = parentNode;
        }

        public InventoryNode this[LLUUID key]
        {
            get { return (InventoryNode)this.Dictionary[key]; }
            set
            {
                value.Parent = parent;
                lock (syncRoot) this.Dictionary[key] = value;
            }
        }

        public ICollection Keys { get { return this.Dictionary.Keys; } }
        public ICollection Values { get { return this.Dictionary.Values; } }

        public void Add(LLUUID key, InventoryNode value)
        {
            value.Parent = parent;
            lock (syncRoot) this.Dictionary.Add(key, value); 
        }

        public void Remove(LLUUID key)
        {
            lock (syncRoot) this.Dictionary.Remove(key);
        }

        public bool Contains(LLUUID key)
        {
            return this.Dictionary.Contains(key);
        }
    }
}
