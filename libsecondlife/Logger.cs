/*
 * Copyright (c) 2007-2008, Second Life Reverse Engineering Team
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
                log4net.Appender.ConsoleAppender appender = new log4net.Appender.ConsoleAppender();
                appender.Layout = new log4net.Layout.PatternLayout("%timestamp [%thread] %-5level - %message%newline");
                BasicConfigurator.Configure(appender);

                if(Settings.LOG_LEVEL != Helpers.LogLevel.None)
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
                    if (Settings.LOG_LEVEL == Helpers.LogLevel.Debug)
                        LogInstance.Debug(message, exception);
                    break;
                case Helpers.LogLevel.Info:
                    if (Settings.LOG_LEVEL == Helpers.LogLevel.Debug
                        || Settings.LOG_LEVEL == Helpers.LogLevel.Info)
                        LogInstance.Info(message, exception);
                    break;
                case Helpers.LogLevel.Warning:
                    if (Settings.LOG_LEVEL == Helpers.LogLevel.Debug
                        || Settings.LOG_LEVEL == Helpers.LogLevel.Info
                        || Settings.LOG_LEVEL == Helpers.LogLevel.Warning)
                        LogInstance.Warn(message, exception);
                    break;
                case Helpers.LogLevel.Error:
                    if (Settings.LOG_LEVEL == Helpers.LogLevel.Debug
                        || Settings.LOG_LEVEL == Helpers.LogLevel.Info
                        || Settings.LOG_LEVEL == Helpers.LogLevel.Warning
                        || Settings.LOG_LEVEL == Helpers.LogLevel.Error)
                        LogInstance.Error(message, exception);
                    break;
                default:
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
            if (Settings.LOG_LEVEL == Helpers.LogLevel.Debug)
            {
                if (client != null && client.Settings.LOG_NAMES)
                    message = String.Format("<{0}>: {1}", client.Self.Name, message);

                if (OnLogMessage != null)
                    OnLogMessage(message, Helpers.LogLevel.Debug);

                LogInstance.Debug(message);
            }
        }
    }
}
