using System;
using System.Collections.Generic;
using System.Text;

namespace libsecondlife.TestTool
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: TestTool.exe firstname lastname password [Master Name]");
                return;
            }

			string masterName = String.Empty;
			for (int ct = 3; ct < args.Length;ct++)
				masterName = masterName + args[ct] + " ";

			TestTool testTool = new TestTool(args[0], args[1], args[2], masterName.TrimEnd());
			testTool.Run();
        }
    }
}
