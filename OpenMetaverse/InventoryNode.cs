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
using System.Runtime.Serialization;

namespace OpenMetaverse
{
    [Serializable()]
    public class InventoryNode : ISerializable
    {
        private InventoryBase data;
        private InventoryNode parent;
        private UUID parentID; //used for de-seralization 
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
        public UUID ParentID
        {
            get { return parentID; }
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

        /// <summary>
        /// De-serialization constructor for the InventoryNode Class
        /// </summary>
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
        /// Serialization handler for the InventoryNode Class
        /// </summary>
        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            if(parent!=null)
                info.AddValue("Parent", parent.Data.UUID, typeof(UUID)); //We need to track the parent UUID for de-serialization
            else
                info.AddValue("Parent", UUID.Zero, typeof(UUID));

            info.AddValue("Type", data.GetType(), typeof(Type));
            
            if(data is InventoryAnimation)
                ((InventoryAnimation)data).GetObjectData(info, ctxt);
            if(data is InventoryAttachment)
                ((InventoryAttachment)data).GetObjectData(info, ctxt);
            if(data is InventoryCallingCard)
                ((InventoryCallingCard)data).GetObjectData(info, ctxt);
            if(data is InventoryFolder)
                ((InventoryFolder)data).GetObjectData(info, ctxt);
            if(data is InventoryGesture)
                ((InventoryGesture)data).GetObjectData(info, ctxt);
            if(data is InventoryLandmark)
                ((InventoryLandmark)data).GetObjectData(info, ctxt);
            if(data is InventoryLSL)
                ((InventoryLSL)data).GetObjectData(info, ctxt);
            if (data is InventoryNotecard)
                ((InventoryNotecard)data).GetObjectData(info, ctxt);
            if(data is InventoryObject)
                ((InventoryObject)data).GetObjectData(info, ctxt);
            if(data is InventorySnapshot)
                ((InventorySnapshot)data).GetObjectData(info, ctxt);
            if(data is InventorySound)
                ((InventorySound)data).GetObjectData(info, ctxt);
            if (data is InventoryTexture)
                ((InventoryTexture)data).GetObjectData(info, ctxt);
            if(data is InventoryWearable)
                ((InventoryWearable)data).GetObjectData(info, ctxt);

            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public InventoryNode(SerializationInfo info, StreamingContext ctxt)
        {
            parentID = (UUID)info.GetValue("Parent", typeof(UUID));
            Type type = (Type)info.GetValue("Type", typeof(Type));
           
            if (type == typeof(InventoryAnimation))
                data = new InventoryAnimation(info, ctxt);

            if (type == typeof(InventoryAttachment))
                data = new InventoryAttachment(info, ctxt);

            if (type == typeof(InventoryCallingCard))
                data = new InventoryCallingCard(info, ctxt);

            if (type == typeof(InventoryFolder))
                data = new InventoryFolder(info, ctxt);

            if (type == typeof(InventoryGesture))
                data = new InventoryGesture(info, ctxt);

            if (type == typeof(InventoryLandmark))
                data = new InventoryLandmark(info, ctxt);

            if (type == typeof(InventoryLSL))
                data = new InventoryLSL(info, ctxt);

            if (type == typeof(InventoryNotecard))
                data = new InventoryNotecard(info, ctxt);

            if (type == typeof(InventoryObject))
                data = new InventoryObject(info, ctxt);

            if (type == typeof(InventorySnapshot))
                data = new InventorySnapshot(info, ctxt);

            if (type == typeof(InventorySound))
                data = new InventorySound(info, ctxt);

            if (type == typeof(InventoryTexture))
                data = new InventoryTexture(info, ctxt);

            if (type == typeof(InventoryWearable))
                data = new InventoryWearable(info, ctxt);
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
