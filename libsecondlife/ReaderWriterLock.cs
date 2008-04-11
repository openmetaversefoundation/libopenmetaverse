/*
 * Copyright (c) 2007-2008, the libsecondlife development team
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
using System.Diagnostics;
using System.Threading;

#if PocketPC

// FIXME: This class was very likely broken when converting things to Auto/Manual ResetEvents

namespace libsecondlife
{
    /// <summary>
    /// A reader-writer lock implementation that is intended to be simple, yet very
    /// efficient.  In particular only 1 interlocked operation is taken for any lock 
    /// operation (we use spin locks to achieve this).  The spin lock is never held
    /// for more than a few instructions (in particular, we never call event APIs
    /// or in fact any non-trivial API while holding the spin lock).   
    /// 
    /// Currently this ReaderWriterLock does not support recurision, however it is 
    /// not hard to add 
    /// </summary>
    public class ReaderWriterLock
    {
        // Lock specifiation for myLock:  This lock protects exactly the local fields associted
        // instance of MyReaderWriterLock.  It does NOT protect the memory associted with the
        // the events that hang off this lock (eg writeEvent, readEvent upgradeEvent).
        int myLock;

        // Who owns the lock owners > 0 => readers
        // owners = -1 means there is one writer.  Owners must be >= -1.  
        int owners;

        // These variables allow use to avoid Setting events (which is expensive) if we don't have to. 
        uint numWriteWaiters;        // maximum number of threads that can be doing a WaitOne on the writeEvent 
        uint numReadWaiters;         // maximum number of threads that can be doing a WaitOne on the readEvent
        uint numUpgradeWaiters;      // maximum number of threads that can be doing a WaitOne on the upgradeEvent (at most 1). 

        // conditions we wait on. 
        WaitHandle writeEvent;    // threads waiting to aquire a write lock go here.
        WaitHandle readEvent;     // threads waiting to aquire a read lock go here (will be released in bulk)
        WaitHandle upgradeEvent;  // thread waiting to upgrade a read lock to a write lock go here (at most one)

        public ReaderWriterLock()
        {
            // All state can start out zeroed. 
        }

        public void AcquireReaderLock(int millisecondsTimeout)
        {
            EnterMyLock();
            for (; ; )
            {
                // We can enter a read lock if there are only read-locks have been given out
                // and a writer is not trying to get in.  
                if (owners >= 0 && numWriteWaiters == 0)
                {
                    // Good case, there is no contention, we are basically done
                    owners++;       // Indicate we have another reader
                    break;
                }

                // Drat, we need to wait.  Mark that we have waiters and wait.  
                if (readEvent == null)      // Create the needed event 
                {
                    LazyCreateEvent(ref readEvent, false);
                    continue;   // since we left the lock, start over. 
                }

                WaitOnEvent(readEvent, ref numReadWaiters, millisecondsTimeout);
            }
            ExitMyLock();
        }

        public void AcquireWriterLock(int millisecondsTimeout)
        {
            EnterMyLock();
            for (; ; )
            {
                if (owners == 0)
                {
                    // Good case, there is no contention, we are basically done
                    owners = -1;    // indicate we have a writer.
                    break;
                }

                // Drat, we need to wait.  Mark that we have waiters and wait.
                if (writeEvent == null)     // create the needed event.
                {
                    LazyCreateEvent(ref writeEvent, true);
                    continue;   // since we left the lock, start over. 
                }

                WaitOnEvent(writeEvent, ref numWriteWaiters, millisecondsTimeout);
            }
            ExitMyLock();
        }

        public void UpgradeToWriterLock(int millisecondsTimeout)
        {
            EnterMyLock();
            for (; ; )
            {
                Debug.Assert(owners > 0, "Upgrading when no reader lock held");
                if (owners == 1)
                {
                    // Good case, there is no contention, we are basically done
                    owners = -1;    // inidicate we have a writer. 
                    break;
                }

                // Drat, we need to wait.  Mark that we have waiters and wait. 
                if (upgradeEvent == null)   // Create the needed event
                {
                    LazyCreateEvent(ref upgradeEvent, false);
                    continue;   // since we left the lock, start over. 
                }

                if (numUpgradeWaiters > 0)
                {
                    ExitMyLock();
                    throw new ApplicationException("UpgradeToWriterLock already in process.  Deadlock!");
                }

                WaitOnEvent(upgradeEvent, ref numUpgradeWaiters, millisecondsTimeout);
            }
            ExitMyLock();
        }

        public void ReleaseReaderLock()
        {
            EnterMyLock();
            Debug.Assert(owners > 0, "ReleasingReaderLock: releasing lock and no read lock taken");
            --owners;
            ExitAndWakeUpAppropriateWaiters();
        }

        public void ReleaseWriterLock()
        {
            EnterMyLock();
            Debug.Assert(owners == -1, "Calling ReleaseWriterLock when no write lock is held");
            Debug.Assert(numUpgradeWaiters > 0);
            owners++;
            ExitAndWakeUpAppropriateWaiters();
        }

        public void DowngradeToReaderLock()
        {
            EnterMyLock();
            Debug.Assert(owners == -1, "Downgrading when no writer lock held");
            owners = 1;
            ExitAndWakeUpAppropriateWaiters();
        }

        /// <summary>
        /// A routine for lazily creating a event outside the lock (so if errors
        /// happen they are outside the lock and that we don't do much work
        /// while holding a spin lock).  If all goes well, reenter the lock and
        /// set 'waitEvent' 
        /// </summary>
        private void LazyCreateEvent(ref WaitHandle waitEvent, bool makeAutoResetEvent)
        {
            Debug.Assert(MyLockHeld);
            Debug.Assert(waitEvent == null);

            ExitMyLock();
            WaitHandle newEvent;
            if (makeAutoResetEvent)
                newEvent = new AutoResetEvent(false);
            else
                newEvent = new ManualResetEvent(false);
            EnterMyLock();
            if (waitEvent == null)          // maybe someone snuck in. 
                waitEvent = newEvent;
        }

        /// <summary>
        /// Waits on 'waitEvent' with a timeout of 'millisceondsTimeout.  
        /// Before the wait 'numWaiters' is incremented and is restored before leaving this routine.
        /// </summary>
        private void WaitOnEvent(WaitHandle waitEvent, ref uint numWaiters, int millisecondsTimeout)
        {
            Debug.Assert(MyLockHeld);

            if (waitEvent is AutoResetEvent)
                ((AutoResetEvent)waitEvent).Reset();
            else
                ((ManualResetEvent)waitEvent).Reset();

            numWaiters++;

            bool waitSuccessful = false;
            ExitMyLock();      // Do the wait outside of any lock 
            try
            {
                if (!waitEvent.WaitOne(millisecondsTimeout, false))
                    throw new ApplicationException("ReaderWriterLock timeout expired");
                waitSuccessful = true;
            }
            finally
            {
                EnterMyLock();
                --numWaiters;
                if (!waitSuccessful)        // We are going to throw for some reason.  Exit myLock. 
                    ExitMyLock();
            }
        }

        /// <summary>
        /// Determines the appropriate events to set, leaves the locks, and sets the events. 
        /// </summary>
        private void ExitAndWakeUpAppropriateWaiters()
        {
            Debug.Assert(MyLockHeld);

            if (owners == 0 && numWriteWaiters > 0)
            {
                ExitMyLock();      // Exit before signaling to improve efficiency (wakee will need the lock)

                // release one writer
                if (writeEvent is AutoResetEvent)
                    ((AutoResetEvent)writeEvent).Set();
                else
                    ((ManualResetEvent)writeEvent).Set();
            }
            else if (owners == 1 && numUpgradeWaiters != 0)
            {
                ExitMyLock();          // Exit before signaling to improve efficiency (wakee will need the lock)

                // release all upgraders (however there can be at most one).
                // two threads upgrading is a guarenteed deadlock, so we throw in that case
                if (upgradeEvent is AutoResetEvent)
                    ((AutoResetEvent)upgradeEvent).Set();
                else
                    ((ManualResetEvent)upgradeEvent).Set();
            }
            else if (owners >= 0 && numReadWaiters != 0)
            {
                ExitMyLock();    // Exit before signaling to improve efficiency (wakee will need the lock)
                // release all readers
                if (readEvent is AutoResetEvent)
                    ((AutoResetEvent)readEvent).Set();
                else
                    ((ManualResetEvent)readEvent).Set();
            }
            else
                ExitMyLock();
        }

        private void EnterMyLock()
        {
            if (Interlocked.CompareExchange(ref myLock, 1, 0) != 0)
                EnterMyLockSpin();
        }

        private void EnterMyLockSpin()
        {
            for (int i = 0; ; i++)
            {
                Thread.Sleep(0);        // Give up my quantum.  

                if (Interlocked.CompareExchange(ref myLock, 1, 0) == 0)
                    return;
            }
        }
        private void ExitMyLock()
        {
            Debug.Assert(myLock != 0, "Exiting spin lock that is not held");
            myLock = 0;
        }

        private bool MyLockHeld { get { return myLock != 0; } }

    };
}

#endif
