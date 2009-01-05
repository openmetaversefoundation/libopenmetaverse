using System;
using log4net;
using HttpServer;

namespace OpenMetaverse.Http
{
    public class log4netLogWriter : ILogWriter
    {
        ILog Log;

        public log4netLogWriter(ILog log)
        {
            Log = log;
        }

        public void Write(object source, LogPrio prio, string message)
        {
            switch (prio)
            {
                case LogPrio.Trace:
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
