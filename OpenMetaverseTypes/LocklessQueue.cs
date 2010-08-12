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
using System.Threading;

namespace OpenMetaverse
{
    /// <summary>
    /// A thread-safe lockless queue that supports multiple readers and 
    /// multiple writers
    /// </summary>
    public sealed class LocklessQueue<T>
    {
        /// <summary>
        /// Provides a node container for data in a singly linked list
        /// </summary>
        private sealed class SingleLinkNode
        {
            /// <summary>Pointer to the next node in list</summary>
            public SingleLinkNode Next;
            /// <summary>The data contained by the node</summary>
            public T Item;

            /// <summary>
            /// Constructor
            /// </summary>
            public SingleLinkNode() { }

            /// <summary>
            /// Constructor
            /// </summary>
            public SingleLinkNode(T item)
            {
                this.Item = item;
            }
        }

        /// <summary>Queue head</summary>
        SingleLinkNode head;
        /// <summary>Queue tail</summary>
        SingleLinkNode tail;
        /// <summary>Queue item count</summary>
        int count;

        /// <summary>Gets the current number of items in the queue. Since this
        /// is a lockless collection this value should be treated as a close
        /// estimate</summary>
        public int Count { get { return count; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public LocklessQueue()
        {
            count = 0;
            head = tail = new SingleLinkNode();
        }

        /// <summary>
        /// Enqueue an item
        /// </summary>
        /// <param name="item">Item to enqeue</param>
        public void Enqueue(T item)
        {
            SingleLinkNode newNode = new SingleLinkNode { Item = item };

            while (true)
            {
                SingleLinkNode oldTail = tail;
                SingleLinkNode oldTailNext = oldTail.Next;

                if (tail == oldTail)
                {
                    if (oldTailNext != null)
                    {
                        CAS(ref tail, oldTail, oldTailNext);
                    }
                    else if (CAS(ref tail.Next, null, newNode))
                    {
                        CAS(ref tail, oldTail, newNode);
                        Interlocked.Increment(ref count);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Try to dequeue an item
        /// </summary>
        /// <param name="item">Dequeued item if the dequeue was successful</param>
        /// <returns>True if an item was successfully deqeued, otherwise false</returns>
        public bool TryDequeue(out T item)
        {
            while (true)
            {
                SingleLinkNode oldHead = head;
                SingleLinkNode oldHeadNext = oldHead.Next;

                if (oldHead == head)
                {
                    if (oldHeadNext == null)
                    {
                        item = default(T);
                        count = 0;
                        return false;
                    }
                    if (CAS(ref head, oldHead, oldHeadNext))
                    {
                        item = oldHeadNext.Item;
                        Interlocked.Decrement(ref count);
                        return true;
                    }
                }
            }
        }

        private static bool CAS(ref SingleLinkNode location, SingleLinkNode comparand, SingleLinkNode newValue)
        {
            return
                (object)comparand ==
                (object)Interlocked.CompareExchange<SingleLinkNode>(ref location, newValue, comparand);
        }
    }
}