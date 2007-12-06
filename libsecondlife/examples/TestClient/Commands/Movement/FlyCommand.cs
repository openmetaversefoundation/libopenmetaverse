using System;
using libsecondlife;

namespace libsecondlife.TestClient
{
    public class FlyCommand : Command
    {
        public FlyCommand(TestClient testClient)
        {
            Name = "fly";
            Description = "Starts or stops flying. Usage: fly [start/stop]";
        }

        public override string Execute(string[] args, LLUUID fromAgentID)
        {
            bool start = true;

            if (args.Length == 1 && args[0].ToLower() == "stop")
                start = false;

            if (start)
            {
                Client.Self.Fly(true);
                return "Started flying";
            }
            else
            {
                Client.Self.Fly(false);
                return "Stopped flying";
            }
        }
    }
}
