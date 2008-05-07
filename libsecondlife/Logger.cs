using System;
using log4net;
using log4net.Config;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace libsecondlife
{
    /// <summary>
    /// Singleton logging class for libsecondlife
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Callback used for client apps to receive log messages from
        /// libsecondlife
        /// </summary>
        /// <param name="message">Data being logged</param>
        /// <param name="level">The severity of the log entry from <seealso cref="Helpers.LogLevel"/></param>
        public delegate void LogCallback(object message, Helpers.LogLevel level);

        /// <summary>Triggered whenever a message is logged. If this is left
        /// null, log messages will go to the console</summary>
        public static event LogCallback OnLogMessage;

        /// <summary>log4net logging engine</summary>
        public static ILog LogInstance;

        /// <summary>
        /// Default constructor
        /// </summary>
        static Logger()
        {
            LogInstance = LogManager.GetLogger("libsecondlife");

            // If error level reporting isn't enabled we assume no logger is configured and initialize a default
            // ConsoleAppender
            if (!LogInstance.Logger.IsEnabledFor(log4net.Core.Level.Error))
            {
                BasicConfigurator.Configure();
                LogInstance.Info("No log configuration found, defaulting to console logging");
            }
        }

        /// <summary>
        /// Send a log message to the logging engine
        /// </summary>
        /// <param name="message">The log message</param>
        /// <param name="level">The severity of the log entry</param>
        public static void Log(object message, Helpers.LogLevel level)
        {
            Log(message, level, null, null);
        }

        /// <summary>
        /// Send a log message to the logging engine
        /// </summary>
        /// <param name="message">The log message</param>
        /// <param name="level">The severity of the log entry</param>
        /// <param name="client">Instance of the client</param>
        public static void Log(object message, Helpers.LogLevel level, SecondLife client)
        {
            Log(message, level, client, null);
        }

        /// <summary>
        /// Send a log message to the logging engine
        /// </summary>
        /// <param name="message">The log message</param>
        /// <param name="level">The severity of the log entry</param>
        /// <param name="exception">Exception that was raised</param>
        public static void Log(object message, Helpers.LogLevel level, Exception exception)
        {
            Log(message, level, null, exception);
        }

        /// <summary>
        /// Send a log message to the logging engine
        /// </summary>
        /// <param name="message">The log message</param>
        /// <param name="level">The severity of the log entry</param>
        /// <param name="client">Instance of the client</param>
        /// <param name="exception">Exception that was raised</param>
        public static void Log(object message, Helpers.LogLevel level, SecondLife client, Exception exception)
        {
            if (client != null && client.Settings.LOG_NAMES)
                message = String.Format("<{0}>: {1}", client.Self.Name, message);

            if (OnLogMessage != null)
                OnLogMessage(message, level);

            switch (level)
            {
                case Helpers.LogLevel.Debug:
                    LogInstance.Debug(message, exception);
                    break;
                case Helpers.LogLevel.Info:
                    LogInstance.Info(message, exception);
                    break;
                case Helpers.LogLevel.Warning:
                    LogInstance.Warn(message, exception);
                    break;
                case Helpers.LogLevel.Error:
                    LogInstance.Error(message, exception);
                    break;
            }
        }

        /// <summary>
        /// If the library is compiled with DEBUG defined, an event will be
        /// fired if an <code>OnLogMessage</code> handler is registered and the
        /// message will be sent to the logging engine
        /// </summary>
        /// <param name="message">The message to log at the DEBUG level to the
        /// current logging engine</param>
        public static void DebugLog(object message)
        {
            DebugLog(message, null);
        }

        /// <summary>
        /// If the library is compiled with DEBUG defined and
        /// <code>SecondLife.Settings.DEBUG</code> is true, an event will be
        /// fired if an <code>OnLogMessage</code> handler is registered and the
        /// message will be sent to the logging engine
        /// </summary>
        /// <param name="message">The message to log at the DEBUG level to the
        /// current logging engine</param>
        /// <param name="client">Instance of the client</param>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void DebugLog(object message, SecondLife client)
        {
            if (client != null && client.Settings.LOG_NAMES)
                message = String.Format("<{0}>: {1}", client.Self.Name, message);

            if (OnLogMessage != null)
                OnLogMessage(message, Helpers.LogLevel.Debug);

            LogInstance.Debug(message);
        }
    }
}
