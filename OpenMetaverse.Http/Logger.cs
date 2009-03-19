/*
 * Copyright (c) 2008, openmetaverse.org
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
using log4net;
using log4net.Config;

[assembly: log4net.Config.XmlConfigurator(ConfigFileExtension = "log4net")]

namespace OpenMetaverse.Http
{
    /// <summary>
    /// Singleton logging class for the entire library
    /// </summary>
    internal static class Logger
    {
        /// <summary>log4net logging engine</summary>
        public static ILog Log;

        static Logger()
        {
            Log = LogManager.GetLogger(System.Reflection.Assembly.GetExecutingAssembly().FullName);

            // If error level reporting isn't enabled we assume no logger is configured and initialize a default
            // ConsoleAppender
            if (!Log.Logger.IsEnabledFor(log4net.Core.Level.Error))
            {
                log4net.Appender.ConsoleAppender appender = new log4net.Appender.ConsoleAppender();
                appender.Layout = new log4net.Layout.PatternLayout("%timestamp [%thread] %-5level - %message%newline");
                BasicConfigurator.Configure(appender);

                Log.Info("No log configuration found, defaulting to console logging");
            }
        }
    }
}
