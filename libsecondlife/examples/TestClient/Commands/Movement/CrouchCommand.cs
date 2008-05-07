using System;
using libsecondlife;

namespace libsecondlife.TestClient
{
    public class CrouchCommand : Command
    {
        public CrouchCommand(TestClient testClient)
        {
            Name = "crouch";
            Description = "Starts or stops crouching. Usage: crouch [start/stop]";
        }

        public override string Execute(string[] args, LLUUID fromAgentID)
        {
            bool start = true;

            if (args.Length == 1 && args[0].ToLower() == "stop")
                start = false;

            if (start)
            {
                Client.Self.Crouch(true);
                return "Started crouching";
            }
            else
            {
                Client.Self.Crouch(false);
                return "Stopped crouching";
            }
        }
    }
}
