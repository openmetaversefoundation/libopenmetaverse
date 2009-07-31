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
using System.Collections.Generic;
using System.Threading;

namespace OpenMetaverse
{
    /// <summary>
    /// Provides helper methods for parallelizing loops
    /// </summary>
    public static class Parallel
    {
        private static readonly int processorCount = System.Environment.ProcessorCount;

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel
        /// </summary>
        /// <param name="fromInclusive">The loop will be started at this index</param>
        /// <param name="toExclusive">The loop will be terminated before this index is reached</param>
        /// <param name="body">Method body to run for each iteration of the loop</param>
        public static void For(int fromInclusive, int toExclusive, Action<int> body)
        {
            For(processorCount, fromInclusive, toExclusive, body);
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel
        /// </summary>
        /// <param name="threadCount">The number of concurrent execution threads to run</param>
        /// <param name="fromInclusive">The loop will be started at this index</param>
        /// <param name="toExclusive">The loop will be terminated before this index is reached</param>
        /// <param name="body">Method body to run for each iteration of the loop</param>
        public static void For(int threadCount, int fromInclusive, int toExclusive, Action<int> body)
        {
            AutoResetEvent[] threadFinishEvent = new AutoResetEvent[threadCount];
            --fromInclusive;

            for (int i = 0; i < threadCount; i++)
            {
                threadFinishEvent[i] = new AutoResetEvent(false);

                ThreadPool.QueueUserWorkItem(
                    delegate(object o)
                    {
                        int threadIndex = (int)o;

                        while (true)
                        {
                            int currentIndex = Interlocked.Increment(ref fromInclusive);

                            if (currentIndex >= toExclusive)
                                break;

                            body(currentIndex);
                        }

                        threadFinishEvent[threadIndex].Set();
                    }, i
                );
            }

            for (int i = 0; i < threadCount; i++)
                threadFinishEvent[i].WaitOne();
        }

        /// <summary>
        /// Executes a foreach loop in which iterations may run in parallel
        /// </summary>
        /// <typeparam name="T">Object type that the collection wraps</typeparam>
        /// <param name="enumerable">An enumerable collection to iterate over</param>
        /// <param name="body">Method body to run for each object in the collection</param>
        public static void ForEach<T>(IEnumerable<T> enumerable, Action<T> body)
        {
            ForEach<T>(processorCount, enumerable, body);
        }

        /// <summary>
        /// Executes a foreach loop in which iterations may run in parallel
        /// </summary>
        /// <typeparam name="T">Object type that the collection wraps</typeparam>
        /// <param name="threadCount">The number of concurrent execution threads to run</param>
        /// <param name="enumerable">An enumerable collection to iterate over</param>
        /// <param name="body">Method body to run for each object in the collection</param>
        public static void ForEach<T>(int threadCount, IEnumerable<T> enumerable, Action<T> body)
        {
            AutoResetEvent[] threadFinishEvent = new AutoResetEvent[threadCount];
            IEnumerator<T> enumerator = enumerable.GetEnumerator();
            object syncRoot = new Object();

            for (int i = 0; i < threadCount; i++)
            {
                threadFinishEvent[i] = new AutoResetEvent(false);

                ThreadPool.QueueUserWorkItem(
                    delegate(object o)
                    {
                        int threadIndex = (int)o;

                        while (true)
                        {
                            T entry;

                            lock (syncRoot)
                            {
                                if (!enumerator.MoveNext())
                                    break;
                                entry = (T)enumerator.Current; // Explicit typecast for Mono's sake
                            }

                            body(entry);
                        }

                        threadFinishEvent[threadIndex].Set();
                    }, i
                );
            }

            for (int i = 0; i < threadCount; i++)
                threadFinishEvent[i].WaitOne();
        }

        /// <summary>
        /// Executes a series of tasks in parallel
        /// </summary>
        /// <param name="actions">A series of method bodies to execute</param>
        public static void Invoke(params Action[] actions)
        {
            Invoke(processorCount, actions);
        }

        /// <summary>
        /// Executes a series of tasks in parallel
        /// </summary>
        /// <param name="threadCount">The number of concurrent execution threads to run</param>
        /// <param name="actions">A series of method bodies to execute</param>
        public static void Invoke(int threadCount, params Action[] actions)
        {
            AutoResetEvent[] threadFinishEvent = new AutoResetEvent[threadCount];
            int index = -1;

            for (int i = 0; i < threadCount; i++)
            {
                threadFinishEvent[i] = new AutoResetEvent(false);

                ThreadPool.QueueUserWorkItem(
                    delegate(object o)
                    {
                        int threadIndex = (int)o;

                        while (true)
                        {
                            int currentIndex = Interlocked.Increment(ref index);

                            if (currentIndex >= actions.Length)
                                break;

                            actions[currentIndex]();
                        }

                        threadFinishEvent[threadIndex].Set();
                    }, i
                );
            }

            for (int i = 0; i < threadCount; i++)
                threadFinishEvent[i].WaitOne();
        }
    }
}
