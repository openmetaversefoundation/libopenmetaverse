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
using System.Text;
using System.Threading;
using System.Collections.Generic;
using OpenMetaverse.Packets;

namespace OpenMetaverse.Stats
{
    public enum Type
        {
            Packet,
            Message
        }
    public class UtilizationStatistics
    {
        
        public class Stat
        {
            public Type Type;
            public long TxCount;
            public long RxCount;
            public long TxBytes;
            public long RxBytes;

            public Stat(Type type, long txCount, long rxCount, long txBytes, long rxBytes)
            {
                this.Type = type;
                this.TxCount = txCount;
                this.RxCount = rxCount;
                this.TxBytes = txBytes;
                this.RxBytes = rxBytes;
            }
        }
                
        private Dictionary<string, Stat> m_StatsCollection;

        public UtilizationStatistics()
        {
            m_StatsCollection = new Dictionary<string, Stat>();
        }

        internal void Update(string key, Type Type, long txBytes, long rxBytes)
        {            
            lock (m_StatsCollection)
            {
                if(m_StatsCollection.ContainsKey(key))
                {
                    Stat stat = m_StatsCollection[key];
                    if (rxBytes > 0)
                    {
                        Interlocked.Increment(ref stat.RxCount);
                        Interlocked.Add(ref stat.RxBytes, rxBytes);    
                    }

                    if (txBytes > 0)
                    {
                        Interlocked.Increment(ref stat.TxCount);
                        Interlocked.Add(ref stat.TxBytes, txBytes);
                    }
                                                                           
                } else {
                    Stat stat;
                    if (txBytes > 0)
                        stat = new Stat(Type, 1, 0, txBytes, 0);
                    else
                        stat = new Stat(Type, 0, 1, 0, rxBytes);

                    m_StatsCollection.Add(key, stat);
                }
            }
        }

        public Dictionary<string, Stat> GetStatistics()
        {
            lock(m_StatsCollection)
            {
                return new Dictionary<string, Stat>(m_StatsCollection);
            }
        }
    }
}
