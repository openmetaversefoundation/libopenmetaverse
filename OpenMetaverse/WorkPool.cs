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

#define SMARTHREADPOOL_REF

using System;
using System.IO;
using System.Reflection;

#if SMARTHREADPOOL_REF
using Amib.Threading;
#else
using System.Threading;
#endif

namespace OpenMetaverse
{

// Use statically referenced SmartThreadPool.dll
#if SMARTHREADPOOL_REF
    public static class WorkPool
    {
        internal static SmartThreadPool Pool = null;

        public static bool Init(bool useSmartThredPool)
        {
            if (Pool == null)
            {
                STPStartInfo param = new STPStartInfo();
                param.MinWorkerThreads = 2;
                param.MaxWorkerThreads = 50;
                param.ThreadPoolName = "LibOpenMetaverse Main ThreadPool";
                param.AreThreadsBackground = true;

                Pool = new SmartThreadPool(param);
            }
            return true;
        }

        public static void Shutdown()
        {
            if (Pool != null)
            {
                Pool.Shutdown();
                Pool = null;
            }
        }

        public static void QueueUserWorkItem(System.Threading.WaitCallback callback)
        {
            if (Pool != null)
            {
                Pool.QueueWorkItem(state => { callback.Invoke(state); return null; });
            }
            else
            {
                System.Threading.ThreadPool.QueueUserWorkItem(state => callback.Invoke(state));
            }
        }

        public static void QueueUserWorkItem(System.Threading.WaitCallback callback, object state)
        {
            if (Pool != null)
            {
                Pool.QueueWorkItem(sync => { callback.Invoke(sync); return null; }, state);
            }
            else
            {
                System.Threading.ThreadPool.QueueUserWorkItem(sync => callback.Invoke(sync), state);
            }
        }
    }

#else

    // Try to load SmartThreadPool.dll during initialization
    // Fallback to System.Threading.ThreadPool if that fails
    public static class WorkPoolDynamic
    {
        internal static object Pool = null;

        private static Type SmartThreadPoolType;
        private static Type WorkItemCallbackType;
        private static MethodInfo QueueWorkItemFunc, QueueWorkItemFunc2;
        private static MethodInfo ShutdownFunc;
        private static Func<System.Threading.WaitCallback, object, object> Invoker;

        public static bool Init()
        {
            try
            {
                string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                Assembly assembly = Assembly.LoadFile(Path.Combine(dir, "SmartThreadPool.dll"));
                Type STPStartInfo = assembly.GetType("Amib.Threading.STPStartInfo");
                SmartThreadPoolType = assembly.GetType("Amib.Threading.SmartThreadPool");
                WorkItemCallbackType = assembly.GetType("Amib.Threading.WorkItemCallback");
                var param = Activator.CreateInstance(STPStartInfo);
                STPStartInfo.GetProperty("MinWorkerThreads").SetValue(param, 2, null);
                STPStartInfo.GetProperty("MaxWorkerThreads").SetValue(param, 50, null);
                STPStartInfo.GetProperty("ThreadPoolName").SetValue(param, "LibOpenMetaverse Main ThreadPool", null);
                STPStartInfo.GetProperty("AreThreadsBackground").SetValue(param, true, null);
                STPStartInfo.GetProperty("MinWorkerThreads").SetValue(param, 2, null);
                Pool = Activator.CreateInstance(SmartThreadPoolType, new object[] { param });
                QueueWorkItemFunc = SmartThreadPoolType.GetMethod("QueueWorkItem", new Type[] { WorkItemCallbackType });
                QueueWorkItemFunc2 = SmartThreadPoolType.GetMethod("QueueWorkItem", new Type[] { WorkItemCallbackType, typeof(object) });
                ShutdownFunc = SmartThreadPoolType.GetMethod("Shutdown", new Type[] { });

                Invoker = (inv, state) =>
                {
                    inv.Invoke(state);
                    return null;
                };

                return true;
            }
            catch
            {
                Pool = null;
                return false;
            }
        }

        public static void Shutdown()
        {
            if (Pool != null)
            {
                ShutdownFunc.Invoke(Pool, null);
                Pool = null;
            }
        }


        public static void QueueUserWorkItem(System.Threading.WaitCallback callback)
        {
            if (Pool != null)
            {
                QueueWorkItemFunc.Invoke(Pool, new object[] { Delegate.CreateDelegate(WorkItemCallbackType, callback, Invoker.Method) });
            }
            else
            {
                System.Threading.ThreadPool.QueueUserWorkItem(state => callback.Invoke(state));
            }
        }

        public static void QueueUserWorkItem(System.Threading.WaitCallback callback, object state)
        {
            if (Pool != null)
            {
                QueueWorkItemFunc2.Invoke(Pool, new object[] { Delegate.CreateDelegate(WorkItemCallbackType, callback, Invoker.Method), state });
            }
            else
            {
                System.Threading.ThreadPool.QueueUserWorkItem(sync => callback.Invoke(sync), state);
            }
        }
    }


    public static class WorkPool
    {
        private static bool UseSmartThreadPool = false;

        public static bool Init(bool useSmartThredPool)
        {
            if (useSmartThredPool)
            {
                if (WorkPoolDynamic.Init())
                {
                    UseSmartThreadPool = true;
                    return true;
                }
                return false;
            }
            return true;
        }

        public static void Shutdown()
        {
            if (UseSmartThreadPool)
            {
                WorkPoolDynamic.Shutdown();
                UseSmartThreadPool = false;
            }
        }

        public static void QueueUserWorkItem(System.Threading.WaitCallback callback)
        {
            if (UseSmartThreadPool)
            {
                WorkPoolDynamic.QueueUserWorkItem(sync => callback.Invoke(sync));
            }
            else
            {
                ThreadPool.QueueUserWorkItem(sync => callback.Invoke(sync));
            }
        }

        public static void QueueUserWorkItem(System.Threading.WaitCallback callback, object state)
        {
            if (UseSmartThreadPool)
            {
                WorkPoolDynamic.QueueUserWorkItem(sync => callback.Invoke(sync), state);
            }
            else
            {
                ThreadPool.QueueUserWorkItem(sync => callback.Invoke(sync), state);
            }
        }
    }
#endif
}
