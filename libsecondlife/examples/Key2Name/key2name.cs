using System;
using libsecondlife;

namespace Key2Name
{
    class Program
    {
        static System.Threading.AutoResetEvent NameEvent = new System.Threading.AutoResetEvent(false);

        static void Main(string[] args)
        {
            if (args.Length < 4)
            {
                Console.WriteLine("Usage: Key2Name [loginfirstname] [loginlastname] [password] [key]");
                return;
            }

            SecondLife client = new SecondLife();
            client.Avatars.OnAvatarNames += new AvatarManager.AvatarNamesCallback(Avatars_OnAvatarNames);

            Console.WriteLine("Attempting to connect and login to Second Life.");

            // Login to Second Life
            if (!client.Network.Login(args[0], args[1], args[2], "key2name", "jessemalthus@gmail.com"))
            {
                // Login failed
                Console.WriteLine("Error logging in: " + client.Network.LoginMessage);
                return;
            }

            LLUUID lookup = new LLUUID();
            LLUUID.TryParse(args[3], out lookup);

            Console.WriteLine("Looking up name for " + lookup.ToString());

            client.Avatars.RequestAvatarName(lookup);

            if (!NameEvent.WaitOne(15 * 1000, false))
                Console.WriteLine("Name lookup timed out.");

            Console.WriteLine("Press enter to logout.");
            Console.ReadLine();

            client.Network.Logout();
        }

        static void Avatars_OnAvatarNames(System.Collections.Generic.Dictionary<LLUUID, string> names)
        {
            foreach (string name in names.Values)
                Console.WriteLine("Name: " + name);

            NameEvent.Set();
        }
    }
}
