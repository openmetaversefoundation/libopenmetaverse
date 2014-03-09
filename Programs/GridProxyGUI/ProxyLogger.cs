using System;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Config;
using log4net.Layout;

namespace GridProxyGUI
{
    public class ProxyLogger : AppenderSkeleton
    {
        public delegate void Log(object sender, LogEventArgs e);
        public static event Log OnLogLine;

        public static void Init()
        {
            var appender = new ProxyLogger();
            appender.Layout = new PatternLayout("%timestamp %-5level %message%newline");
            // appender.AddFilter(new log4net.Filter.LoggerMatchFilter() { LoggerToMatch = "OpenMetaverse" });
            BasicConfigurator.Configure(appender);
        }

        protected override void Append(LoggingEvent le)
        {
            if (OnLogLine != null && le.Level != Level.Debug)
            {
                OnLogLine(this, new LogEventArgs(string.Format("{0} [{1}] {2}\n", le.TimeStamp, le.Level, le.MessageObject)));
            }
        }
    }

    public class LogEventArgs : EventArgs
    {
        public string Message { get; set; }

        public LogEventArgs(string msg)
        {
            this.Message = msg;
        }
    }
}

