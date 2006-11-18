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
			bot.Run();
        }
    }
}
