/* 
* CVS identifier:
* 
* $Id: ThreadPool.java,v 1.9 2002/05/22 15:00:55 grosbois Exp $
* 
* Class:                   ThreadPool
* 
* Description:             A pool of threads
* 
* 
* 
* COPYRIGHT:
* 
* This software module was originally developed by Raphaël Grosbois and
* Diego Santa Cruz (Swiss Federal Institute of Technology-EPFL); Joel
* Askelöf (Ericsson Radio Systems AB); and Bertrand Berthelot, David
* Bouchard, Félix Henry, Gerard Mozelle and Patrice Onno (Canon Research
* Centre France S.A) in the course of development of the JPEG2000
* standard as specified by ISO/IEC 15444 (JPEG 2000 Standard). This
* software module is an implementation of a part of the JPEG 2000
* Standard. Swiss Federal Institute of Technology-EPFL, Ericsson Radio
* Systems AB and Canon Research Centre France S.A (collectively JJ2000
* Partners) agree not to assert against ISO/IEC and users of the JPEG
* 2000 Standard (Users) any of their rights under the copyright, not
* including other intellectual property rights, for this software module
* with respect to the usage by ISO/IEC and Users of this software module
* or modifications thereof for use in hardware or software products
* claiming conformance to the JPEG 2000 Standard. Those intending to use
* this software module in hardware or software products are advised that
* their use may infringe existing patents. The original developers of
* this software module, JJ2000 Partners and ISO/IEC assume no liability
* for use of this software module or modifications thereof. No license
* or right to this software module is granted for non JPEG 2000 Standard
* conforming products. JJ2000 Partners have full right to use this
* software module for his/her own purpose, assign or donate this
* software module to any third party and to inhibit third parties from
* using this software module for non JPEG 2000 Standard conforming
* products. This copyright notice must be included in all copies or
* derivative works of this software module.
* 
* Copyright (c) 1999/2000 JJ2000 Partners.
* */
using System;
namespace CSJ2K.j2k.util
{
	
	/// <summary> This class implements a thread pool. The thread pool contains a set of
	/// threads which can be given work to do.
	/// 
	/// <P>If the Java Virtual Machine (JVM) uses native threads, then the
	/// different threads will be able to execute in different processors in
	/// parallel on multiprocessors machines. However, under some JVMs and
	/// operating systems using native threads is not sufficient to allow the JVM
	/// access to multiple processors. This is the case when native threads are
	/// implemented using POSIX threads on lightweight processes
	/// (i.e. PTHREAD_SCOPE_PROCESS sopce scheduling), which is the case on most
	/// UNIX operating systems. In order to do provide access to multiple
	/// processors it is necessary to set the concurrency level to the number of
	/// processors or slightly higher. This can be achieved by setting the Java
	/// system property with the name defined by CONCURRENCY_PROP_NAME to some
	/// non-negative number. This will make use of the 'NativeServices' class and
	/// supporting native libraries. See 'NativeServices' for details. See
	/// 'CONCURRENCY_PROP_NAME' for the name of the property.
	/// 
	/// <P>Initially the thread pool contains a user specified number of idle
	/// threads. Idle threads can be given a target which is run. While running the
	/// target the thread temporarily leaves the idle list. When the target
	/// finishes, it joins the idle list again, waiting for a new target. When a
	/// target is finished a thread can be notified on a particular object that is
	/// given as a lock.
	/// 
	/// <P>Jobs can be submitted using Runnable interfaces, using the 'runTarget()'
	/// methods. When the job is submitted, an idle thread will be obtained, the
	/// 'run()' method of the 'Runnable' interface will be executed and when it
	/// completes the thread will be returned to the idle list. In general the
	/// 'run()' method should complete in a rather short time, so that the threds
	/// of the pool are not starved.
	/// 
	/// <P>If using the non-asynchronous calls to 'runTarget()', it is important
	/// that any target's 'run()' method, or any method called from it, does not
	/// use non-asynchronous calls to 'runTarget()' on the same thread pool where
	/// it was started. Otherwise this could create a dead-lock when there are not
	/// enough idle threads.
	/// 
	/// <P>The pool also has a global error and runtime exception condition (one
	/// for 'Error' and one for 'RuntimeException'). If a target's 'run()' method
	/// throws an 'Error' or 'RuntimeException' the corresponding exception
	/// condition is set and the exception object saved. In any subsequent call to
	/// 'checkTargetErrors()' the saved exception object is thrown. Likewise, if a
	/// target's 'run()' method throws any other subclass of 'Throwable' a new
	/// 'RuntimeException' is created and saved. It will be thrown on a subsequent
	/// call to 'checkTargetErrors()'. If more than one exception occurs between
	/// calls to 'checkTargetErrors()' only the last one is saved. Any 'Error'
	/// condition has precedence on all 'RuntimeException' conditions. The threads
	/// in the pool are unaffected by any exceptions thrown by targets.
	/// 
	/// <P>The only exception to the above is the 'ThreadDeath' exception. If a
	/// target's 'run()' method throws the 'ThreadDeath' exception a warning
	/// message is printed and the exception is propagated, which will terminate
	/// the thread in which it occurs. This could lead to instabilities of the
	/// pool. The 'ThreadDeath' exception should never be thrown by the program. It 
	/// is thrown by the Java(TM) Virtual Machine when Thread.stop() is
	/// called. This method is deprecated and should never be called.
	/// 
	/// <P>All the threads in the pool are "daemon" threads and will automatically
	/// terminate when no daemon threads are running.
	/// 
	/// </summary>
	/// <seealso cref="NativeServices">
	/// 
	/// </seealso>
	/// <seealso cref="CONCURRENCY_PROP_NAME">
	/// 
	/// </seealso>
	/// <seealso cref="Runnable">
	/// 
	/// </seealso>
	/// <seealso cref="Thread">
	/// 
	/// </seealso>
	/// <seealso cref="Error">
	/// 
	/// </seealso>
	/// <seealso cref="RuntimeException">
	/// 
	/// 
	/// </seealso>
	public class ThreadPool
	{
		/// <summary> Returns the size of the pool. That is the number of threads in this
		/// pool (idle + busy).
		/// 
		/// </summary>
		/// <returns> The pool's size.
		/// 
		/// 
		/// </returns>
		virtual public int Size
		{
			get
			{
				return idle.Length;
			}
			
		}
		
		/// <summary>The name of the property that sets the concurrency level:
		/// jj2000.j2k.util.ThreadPool.concurrency 
		/// </summary>
		public const System.String CONCURRENCY_PROP_NAME = "jj2000.j2k.util.ThreadPool.concurrency";
		
		/// <summary>The array of idle threads and the lock for the manipulation of the
		/// idle thread list. 
		/// </summary>
		private ThreadPoolThread[] idle;
		
		/// <summary>The number of idle threads </summary>
		private int nidle;
		
		/// <summary>The name of the pool </summary>
		private System.String poolName;
		
		/// <summary>The priority for the pool </summary>
		private int poolPriority;
		
		/// <summary>The last error thrown by a target. Null if none </summary>
		// NOTE: needs to be volatile, so that only one copy exits in memory
		private volatile System.ApplicationException targetE;
		
		/// <summary>The last runtime exception thrown by a target. Null if none </summary>
		// NOTE: needs to be volatile, so that only one copy exits in memory
		private volatile System.SystemException targetRE;
		
		//UPGRADE_NOTE: Field 'EnclosingInstance' was added to class 'ThreadPoolThread' to access its enclosing instance. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1019'"
		/// <summary> The threads that are managed by the pool.
		/// 
		/// </summary>
		internal class ThreadPoolThread:SupportClass.ThreadClass
		{
			private void  InitBlock(ThreadPool enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private ThreadPool enclosingInstance;
			public ThreadPool Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			private IThreadRunnable target;
			private System.Object lock_Renamed;
			private bool doNotifyAll;
			
			/// <summary> Creates a ThreadPoolThread object, setting its name according to
			/// the given 'idx', daemon type and the priority to the one of the
			/// pool.
			/// 
			/// </summary>
			/// <param name="idx">The index of this thread in the pool
			/// 
			/// </param>
			/// <param name="name">The name of the thread
			/// 
			/// </param>
			public ThreadPoolThread(ThreadPool enclosingInstance, int idx, System.String name):base(name)
			{
				InitBlock(enclosingInstance);
				IsBackground = true;
				//UPGRADE_TODO: The differences in the type  of parameters for method 'java.lang.Thread.setPriority'  may cause compilation errors.  "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1092'"
				Priority = (System.Threading.ThreadPriority) Enclosing_Instance.poolPriority;
			}
			
			/// <summary> The method that is run by the thread. This method first joins the
			/// idle state in the pool and then enters an infinite loop. In this
			/// loop it waits until a target to run exists and runs it. Once the
			/// target's run() method is done it re-joins the idle state and
			/// notifies the waiting lock object, if one exists.
			/// 
			/// <P>An interrupt on this thread has no effect other than forcing a
			/// check on the target. Normally the target is checked every time the
			/// thread is woken up by notify, no interrupts should be done.
			/// 
			/// <P>Any exception thrown by the target's 'run()' method is catched
			/// and this thread is not affected, except for 'ThreadDeath'. If a
			/// 'ThreadDeath' exception is catched a warning message is printed by
			/// the 'FacilityManager' and the exception is propagated up. For
			/// exceptions which are subclasses of 'Error' or 'RuntimeException'
			/// the corresponding error condition is set and this thread is not
			/// affected. For any other exceptions a new 'RuntimeException' is
			/// created and the error condition is set, this thread is not affected.
			/// 
			/// </summary>
			override public void  Run()
			{
				// Join the idle threads list
				Enclosing_Instance.putInIdleList(this);
				// Permanently lock the object while running so that target can
				// not be changed until we are waiting again. While waiting for a
				// target the lock is released.
				lock (this)
				{
					while (true)
					{
						// Wait until we get a target
						while (target == null)
						{
							try
							{
								System.Threading.Monitor.Wait(this);
							}
							catch (System.Threading.ThreadInterruptedException e)
							{
							}
						}
						// Run the target and catch all possible errors
						try
						{
							target.Run();
						}
						//UPGRADE_NOTE: Exception 'java.lang.ThreadDeath' was converted to 'System.ApplicationException' which has different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1100'"
						catch (System.Threading.ThreadAbortException td)
						{
							// We have been instructed to abruptly terminate
							// the thread, which should never be done. This can
							// cause another thread, or the system, to lock.
							FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.WARNING, "Thread.stop() called on a ThreadPool " + "thread or ThreadDeath thrown. This is " + "deprecated. Lock-up might occur.");
							throw td;
						}
						catch (System.ApplicationException e)
						{
							Enclosing_Instance.targetE = e;
						}
						catch (System.SystemException re)
						{
							Enclosing_Instance.targetRE = re;
						}
						//UPGRADE_NOTE: Exception 'java.lang.Throwable' was converted to 'System.Exception' which has different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1100'"
						catch (System.Exception e)
						{
							// A totally unexpected error has occurred
							// (Thread.stop(Throwable) has been used, which should 
							// never be.
							Enclosing_Instance.targetRE = new System.SystemException("Unchecked exception " + "thrown by target's " + "run() method in pool " + Enclosing_Instance.poolName + ".");
						}
						// Join idle threads
						Enclosing_Instance.putInIdleList(this);
						// Release the target and notify lock (i.e. wakeup)
						target = null;
						if (lock_Renamed != null)
						{
							lock (lock_Renamed)
							{
								if (doNotifyAll)
								{
									System.Threading.Monitor.PulseAll(lock_Renamed);
								}
								else
								{
									System.Threading.Monitor.Pulse(lock_Renamed);
								}
							}
						}
					}
				}
			}
			
			/// <summary> Assigns a target to this thread, with an optional notify lock and a
			/// notify mode. The another target is currently running the method
			/// will block until it terminates. After setting the new target the
			/// runner thread will be wakenup and execytion will start.
			/// 
			/// </summary>
			/// <param name="target">The runnable object containing the 'run()' method to
			/// run.
			/// 
			/// </param>
			/// <param name="lock">An object on which notify will be called once the
			/// target's run method has finished. A thread to be notified should be 
			/// waiting on that object. If null no thread is notified.
			/// 
			/// </param>
			/// <param name="notifyAll">If true 'notifyAll()', instead of 'notify()', will
			/// be called on tghe lock.
			/// 
			/// </param>
			//UPGRADE_NOTE: Synchronized keyword was removed from method 'setTarget'. Lock expression was added. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1027'"
			internal virtual void  setTarget(IThreadRunnable target, System.Object lock_Renamed, bool notifyAll)
			{
				lock (this)
				{
					// Set the target
					this.target = target;
					this.lock_Renamed = lock_Renamed;
					doNotifyAll = notifyAll;
					// Wakeup the thread
					System.Threading.Monitor.Pulse(this);
				}
			}
		}
		
		/// <summary> Creates a new thread pool of the given size, thread priority and pool
		/// name.
		/// 
		/// <P>If the Java system property of the name defined by
		/// 'CONCURRENCY_PROP_NAME' is set, then an attempt will be made to load
		/// the library that supports concurrency setting (see
		/// 'NativeServices'). If that succeds the concurrency level will be set to 
		/// the specified value. Otherwise a warning is printed.
		/// 
		/// </summary>
		/// <param name="size">The size of the pool (number of threads to create in the
		/// pool).
		/// 
		/// </param>
		/// <param name="priority">The priority to give to the threads in the pool. If
		/// less than 'Thread.MIN_PRIORITY' it will be the same as the priority of
		/// the calling thread.
		/// 
		/// </param>
		/// <param name="name">The name of the pool. If null a default generic name is
		/// chosen.
		/// 
		/// </param>
		/// <seealso cref="NativeServices">
		/// 
		/// </seealso>
		/// <seealso cref="CONCURRENCY_PROP_NAME">
		/// 
		/// </seealso>
		public ThreadPool(int size, int priority, System.String name)
		{
			int i;
			ThreadPoolThread t;
			//System.String prop;
			//int clevel;
			
			// Initialize variables checking for special cases
			if (size <= 0)
			{
				throw new System.ArgumentException("Pool must be of positive size");
			}
			if (priority < (int) System.Threading.ThreadPriority.Lowest)
			{
				poolPriority = (System.Int32) SupportClass.ThreadClass.Current().Priority;
			}
			else
			{
				poolPriority = (priority < (int) System.Threading.ThreadPriority.Highest)?priority:(int) System.Threading.ThreadPriority.Highest;
			}
			if (name == null)
			{
				poolName = "Anonymous ThreadPool";
			}
			else
			{
				poolName = name;
			}
					
			// Allocate internal variables
			idle = new ThreadPoolThread[size];
			nidle = 0;
			
			// Create and start the threads
			for (i = 0; i < size; i++)
			{
				t = new ThreadPoolThread(this, i, poolName + "-" + i);
				t.Start();
			}
		}
		
		/// <summary> Runs the run method of the specified target in an idle thread of this
		/// pool. When the target's run method completes, the thread waiting on the
		/// lock object is notified, if any. If there is currently no idle thread
		/// the method will block until a thread of the pool becomes idle or the
		/// calling thread is interrupted.
		/// 
		/// <P>This method is the same as <tt>runTarget(t,l,true,false)</tt>.
		/// 
		/// </summary>
		/// <param name="t">The target. The 'run()' method of this object will be run in
		/// an idle thread of the pool.
		/// 
		/// </param>
		/// <param name="l">The lock object. A thread waiting on the lock of the 'l'
		/// object will be notified, through the 'notify()' call, when the target's 
		/// run method completes. If null no thread is notified.
		/// 
		/// </param>
		/// <returns> True if the target was submitted to some thread. False if no
		/// idle thread could be found and the target was not submitted for
		/// execution.
		/// 
		/// 
		/// </returns>
		public virtual bool runTarget(IThreadRunnable t, System.Object l)
		{
			return runTarget(t, l, false, false);
		}
		
		/// <summary> Runs the run method of the specified target in an idle thread of this
		/// pool. When the target's run method completes, the thread waiting on the
		/// lock object is notified, if any. If there is currently no idle thread
		/// and the asynchronous mode is not used the method will block until a
		/// thread of the pool becomes idle or the calling thread is
		/// interrupted. If the asynchronous mode is used then the method will not
		/// block and will return false.
		/// 
		/// <P>This method is the same as <tt>runTarget(t,l,async,false)</tt>.
		/// 
		/// </summary>
		/// <param name="t">The target. The 'run()' method of this object will be run in
		/// an idle thread of the pool.
		/// 
		/// </param>
		/// <param name="l">The lock object. A thread waiting on the lock of the 'l'
		/// object will be notified, through the 'notify()' call, when the target's 
		/// run method completes. If null no thread is notified.
		/// 
		/// </param>
		/// <param name="async">If true the asynchronous mode will be used.
		/// 
		/// </param>
		/// <returns> True if the target was submitted to some thread. False if no
		/// idle thread could be found and the target was not submitted for
		/// execution.
		/// 
		/// 
		/// </returns>
		public virtual bool runTarget(IThreadRunnable t, System.Object l, bool async)
		{
			return runTarget(t, l, async, false);
		}
		
		/// <summary> Runs the run method of the specified target in an idle thread of this
		/// pool. When the target's run method completes, the thread waiting on the
		/// lock object is notified, if any. If there is currently no idle thread
		/// and the asynchronous mode is not used the method will block until a
		/// thread of the pool becomes idle or the calling thread is
		/// interrupted. If the asynchronous mode is used then the method will not
		/// block and will return false.
		/// 
		/// </summary>
		/// <param name="t">The target. The 'run()' method of this object will be run in
		/// an idle thread of the pool.
		/// 
		/// </param>
		/// <param name="l">The lock object. A thread waiting on the lock of the 'l'
		/// object will be notified, through the 'notify()' call, when the target's 
		/// run method completes. If null no thread is notified.
		/// 
		/// </param>
		/// <param name="async">If true the asynchronous mode will be used.
		/// 
		/// </param>
		/// <param name="notifyAll">If true, threads waiting on the lock of the 'l' object
		/// will be notified trough the 'notifyAll()' instead of the normal
		/// 'notify()' call. This is not normally needed.
		/// 
		/// </param>
		/// <returns> True if the target was submitted to some thread. False if no
		/// idle thread could be found and the target was not submitted for
		/// execution.
		/// 
		/// 
		/// </returns>
		public virtual bool runTarget(IThreadRunnable t, System.Object l, bool async, bool notifyAll)
		{
			ThreadPoolThread runner; // The thread to run the target
			
			// Get a thread to run
			runner = getIdle(async);
			// If no runner return failure
			if (runner == null)
				return false;
			// Set the runner
			runner.setTarget(t, l, notifyAll);
			return true;
		}
		
		/// <summary> Checks that no error or runtime exception in any target have occurred
		/// so far. If an error or runtime exception has occurred in a target's run 
		/// method they are thrown by this method.
		/// 
		/// </summary>
		/// <exception cref="Error">If an error condition has been thrown by a target
		/// 'run()' method.
		/// 
		/// </exception>
		/// <exception cref="RuntimeException">If a runtime exception has been thrown by a 
		/// target 'run()' method.
		/// 
		/// </exception>
		public virtual void  checkTargetErrors()
		{
			// Check for Error
			if (targetE != null)
				throw targetE;
			// Check for RuntimeException
			if (targetRE != null)
				throw targetRE;
		}
		
		/// <summary> Clears the current target error conditions, if any. Note that a thread
		/// in the pool might have set the error conditions since the last check
		/// and that those error conditions will be lost. Likewise, before
		/// returning from this method another thread might set the error
		/// conditions. There is no guarantee that no error conditions exist when
		/// returning from this method.
		/// 
		/// <P>In order to ensure that no error conditions exist when returning
		/// from this method cooperation from the targets and the thread using this
		/// pool is necessary (i.e. currently no targets running or waiting to
		/// run).
		/// 
		/// </summary>
		public virtual void  clearTargetErrors()
		{
			// Clear the error and runtime exception conditions
			targetE = null;
			targetRE = null;
		}
		
		/// <summary> Puts the thread 't' in the idle list. The thread 't' should be in fact
		/// idle and ready to accept a new target when it joins the idle list.
		/// 
		/// <P> An idle thread that is already in the list should never add itself
		/// to the list before it is removed. For efficiency reasons there is no
		/// check to see if the thread is already in the list of idle threads.
		/// 
		/// <P> If the idle list was empty 'notify()' will be called on the 'idle'
		/// array, to wake up a thread that might be waiting (within the
		/// 'getIdle()' method) on an idle thread to become available.
		/// 
		/// </summary>
		/// <param name="t">The thread to put in the idle list.
		/// 
		/// </param>
		private void  putInIdleList(ThreadPoolThread t)
		{
			// NOTE: if already in idle => catastrophe! (should be OK since //
			// this is private method)
			// Lock the idle array to avoid races with 'getIdle()'
			lock (idle)
			{
				idle[nidle] = t;
				nidle++;
				// If idle array was empty wakeup any waiting threads.
				if (nidle == 1)
					System.Threading.Monitor.Pulse(idle);
			}
		}
		
		/// <summary> Returns and idle thread and removes it from the list of idle
		/// threads. In asynchronous mode it will immediately return an idle
		/// thread, or null if none is available. In non-asynchronous mode it will
		/// block until a thread of the pool becomes idle or the calling thread is
		/// interrupted.
		/// 
		/// <P>If in non-asynchronous mode and there are currently no idle threads
		/// available the calling thread will wait on the 'idle' array lock, until
		/// notified by 'putInIdleList()' that an idle thread might have become
		/// available.
		/// 
		/// </summary>
		/// <param name="async">If true asynchronous mode is used.
		/// 
		/// </param>
		/// <returns> An idle thread of the pool, that has been removed from the idle 
		/// list, or null if none is available.
		/// 
		/// </returns>
		private ThreadPoolThread getIdle(bool async)
		{
			// Lock the idle array to avoid races with 'putInIdleList()'
			lock (idle)
			{
				if (async)
				{
					// In asynchronous mode just return null if no idle thread
					if (nidle == 0)
						return null;
				}
				else
				{
					// In synchronous mode wait until a thread becomes idle
					while (nidle == 0)
					{
						try
						{
							System.Threading.Monitor.Wait(idle);
						}
						catch (System.Threading.ThreadInterruptedException e)
						{
							// If we were interrupted just return null
							return null;
						}
					}
				}
				// Decrease the idle count and return one of the idle threads
				nidle--;
				return idle[nidle];
			}
		}
	}
}