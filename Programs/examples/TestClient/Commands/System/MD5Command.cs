using System;
using OpenMetaverse;

namespace OpenMetaverse.TestClient
{
    public class MD5Command : Command
    {
        public MD5Command(TestClient testClient)
        {
            Name = "md5";
            Description = "Creates an MD5 hash from a given password. Usage: md5 [password]";
            Category = CommandCategory.Other;
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            if (args.Length == 1)
                return Utils.MD5(args[0]);
            else
                return "Usage: md5 [password]";
        }
    }
}
