using System;
using System.Collections.Generic;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class DebugCommand : Command
    {
        public DebugCommand(TestClient testClient)
        {
            Name = "debug";
            Description = "Turn debug messages on or off. Usage: debug [level] where level is one of None, Debug, Error, Info, Warn";
        }

        public override string Execute(string[] args, LLUUID fromAgentID)
        {
            if (args.Length != 1)
                return "Usage: debug [level] where level is one of None, Debug, Error, Info, Warn";

            if (args[0].ToLower() == "debug")
            {
                Settings.LOG_LEVEL = Helpers.LogLevel.Debug;
                return "Logging is set to Debug";
            }
            else if (args[0].ToLower() == "none")
            {
                Settings.LOG_LEVEL = Helpers.LogLevel.None;
                return "Logging is set to None";
            }
            else if (args[0].ToLower() == "warn")
            {
                Settings.LOG_LEVEL = Helpers.LogLevel.Warning;
                return "Logging is set to level Warning";
            }
            else if (args[0].ToLower() == "info")
            {
                Settings.LOG_LEVEL = Helpers.LogLevel.Info;
                return "Logging is set to level Info";
            }
            else if (args[0].ToLower() == "error")
            {
                Settings.LOG_LEVEL = Helpers.LogLevel.Error;
                return "Logging is set to level Error";
            }
            else
            {
                return "Usage: debug [level] where level is one of None, Debug, Error, Info, Warn";
            }
        }
    }
}
