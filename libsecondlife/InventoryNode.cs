using System;
using System.Collections.Generic;
using System.Text;

namespace libsecondlife
{
    public class InventoryNode
    {
        private InventoryBase data;
        private InventoryNode parent;
        private InventoryNodeDictionary nodes;

        /// <summary></summary>
        public InventoryBase Data
        {
            get { return data; }
            set { data = value; }
        }

        /// <summary></summary>
        public InventoryNode Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        /// <summary></summary>
        public InventoryNodeDictionary Nodes
        {
            get
            {
                if (nodes == null)
                    nodes = new InventoryNodeDictionary(this);

                return nodes;
            }
            set { nodes = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public InventoryNode()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public InventoryNode(InventoryBase data)
        {
            this.data = data;
        }

        public InventoryNode(InventoryBase data, InventoryNode parent)
        {
            this.data = data;
            this.parent = parent;

            if (parent != null)
            {
                // Add this node to the collection of parent nodes
                lock (parent.Nodes.SyncRoot) parent.Nodes.Add(data.UUID, this);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this.Data == null) return "[Empty Node]";
            return this.Data.ToString();
        }
    }
}
