using System;
using libsecondlife;
using libsecondlife.Utilities;

namespace Key2Name
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 4)
            {
                Console.WriteLine("Usage: Key2Name [loginfirstname] [loginlastname] [password] [key]");
                return;
            }

            SecondLife client = new SecondLife();
            Console.WriteLine("Attempting to connect and login to Second Life.");

            // Login to Second Life
            if (!client.Network.Login(args[0], args[1], args[2], "key2name", "jessemalthus@gmail.com"))
            {
                // Login failed
                Console.WriteLine("Error logging in: " + client.Network.LoginMessage);
                return;
            }

            AvatarTracker avatarTracker = new AvatarTracker(client);

            LLUUID lookup = new LLUUID();
            LLUUID.TryParse(args[3], out lookup);

            Console.WriteLine("Looking up name for " + lookup.ToStringHyphenated());

            string name = avatarTracker.GetAvatarName(lookup);

            Console.WriteLine("Name: " + name + Environment.NewLine + "Press enter to logout.");
            Console.ReadLine();

            client.Network.Logout();
        }
    }
}
