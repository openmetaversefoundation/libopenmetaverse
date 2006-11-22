using System;
using System.Collections.Generic;
using System.Text;

namespace TestBot
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Usage: TestBot.exe [firstname] [lastname] [password]");
                return;
            }

            TestBot bot = new TestBot(args[0], args[1], args[2]);
            Console.WriteLine("Type quit to exit");
            string input = "";

            while (bot.running && bot.Client.Network.Connected)
            {
                input = Console.ReadLine();
                bot.DoCommand(input, null, null);
            }

            if (bot.Client.Network.Connected)
            {
                bot.Client.Network.Logout();
            }
        }
    }
}
