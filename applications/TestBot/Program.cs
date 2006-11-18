using System;
using System.Collections.Generic;
using System.Text;

namespace TestBot
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: TestBot.exe firstname lastname password [Master Name]");
                return;
            }

			string masterName = String.Empty;
			for (int ct = 3; ct < args.Length;ct++)
				masterName = masterName + args[ct] + " ";

			TestBot bot = new TestBot(args[0], args[1], args[2], masterName.TrimEnd());
			bot.Run();
        }
    }
}
