using System;
using log4net;
using HttpServer;

namespace OpenMetaverse.Http
{
    public class log4netLogWriter : ILogWriter
    {
        /// <summary>
        /// Singleton instance of this class
        /// </summary>
        public static log4netLogWriter Instance = new log4netLogWriter(Logger.Log);

        ILog Log;

        log4netLogWriter(ILog log)
        {
            Log = log;
        }

        public void Write(object source, LogPrio prio, string message)
        {
            switch (prio)
            {
                case LogPrio.Trace:
                    return; // This logging is very noisy
                case LogPrio.Debug:
                    Log.DebugFormat("{0}: {1}", source, message);
                    break;
                case LogPrio.Info:
                    Log.InfoFormat("{0}: {1}", source, message);
                    break;
                case LogPrio.Warning:
                    Log.WarnFormat("{0}: {1}", source, message);
                    break;
                case LogPrio.Error:
                    Log.ErrorFormat("{0}: {1}", source, message);
                    break;
                case LogPrio.Fatal:
                    Log.FatalFormat("{0}: {1}", source, message);
                    break;
            }
        }
    }
}
