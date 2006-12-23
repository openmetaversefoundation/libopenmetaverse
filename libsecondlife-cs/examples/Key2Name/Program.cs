using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Utilities;

namespace Key2Name
{
    class Program
    {
        static void Main(string[] args)
        {
            SecondLife client = new SecondLife();
            if (args.Length < 4)
            {
                Console.WriteLine("Usage: Key2Name [loginfirstname] [loginlastname] [password] [key]");
                return;
            }
            Console.WriteLine("Attempting to connect and login to Second Life.");

            // Setup Login to Second Life
            Dictionary<string, object> loginParams = client.Network.DefaultLoginValues(args[0],
                args[1], args[2], "00:00:00:00:00:00", "last", "Win", "0", "key2name",
                "jessemalthus@gmail.com");
            Dictionary<string, object> loginReply = new Dictionary<string, object>();
            if (!client.Network.Login(loginParams))
            {
                // Login failed
                Console.WriteLine("Error logging in: " + client.Network.LoginError);
                return;
            }
            AvatarTracker avatarTracker = new AvatarTracker(client);
            LLUUID lookup = new LLUUID(args[3]);
            Console.WriteLine("Looking up name for " + lookup.ToStringHyphenated());
            string name = avatarTracker.GetAvatarName(lookup);
            Console.WriteLine("Name: " + name + ". Press enter to logout.");
            Console.ReadLine();
            client.Network.Logout();
        }
    }
}
