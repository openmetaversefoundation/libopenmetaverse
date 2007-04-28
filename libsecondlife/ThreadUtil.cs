// Written by Peter A. Bromberg, found at
// http://www.eggheadcafe.com/articles/20060727.asp

using System;
using System.Threading;

/// <summary>
/// 
/// </summary>
public class ThreadUtil
{
    /// <summary>
    /// Delegate to wrap another delegate and its arguments
    /// </summary>
    /// <param name="d"></param>
    /// <param name="args"></param>
    delegate void DelegateWrapper(Delegate d, object[] args);

    /// <summary>
    /// An instance of DelegateWrapper which calls InvokeWrappedDelegate,
    /// which in turn calls the DynamicInvoke method of the wrapped
    /// delegate
    /// </summary>
    static DelegateWrapper wrapperInstance = new DelegateWrapper(InvokeWrappedDelegate);

    /// <summary>
    /// Callback used to call EndInvoke on the asynchronously
    /// invoked DelegateWrapper
    /// </summary>
    static AsyncCallback callback = new AsyncCallback(EndWrapperInvoke);

    /// <summary>
    /// Executes the specified delegate with the specified arguments
    /// asynchronously on a thread pool thread
    /// </summary>
    /// <param name="d"></param>
    /// <param name="args"></param>
    public static void FireAndForget(Delegate d, params object[] args)
    {
        // Invoke the wrapper asynchronously, which will then
        // execute the wrapped delegate synchronously (in the
        // thread pool thread)
        if (d != null) wrapperInstance.BeginInvoke(d, args, callback, null);
    }

    /// <summary>
    /// Invokes the wrapped delegate synchronously
    /// </summary>
    /// <param name="d"></param>
    /// <param name="args"></param>
    static void InvokeWrappedDelegate(Delegate d, object[] args)
    {
        d.DynamicInvoke(args);
    }

    /// <summary>
    /// Calls EndInvoke on the wrapper and Close on the resulting WaitHandle
    /// to prevent resource leaks
    /// </summary>
    /// <param name="ar"></param>
    static void EndWrapperInvoke(IAsyncResult ar)
    {
        wrapperInstance.EndInvoke(ar);
        ar.AsyncWaitHandle.Close();
    }
}
