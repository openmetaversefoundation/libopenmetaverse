using System;
using System.Collections.Generic;
using System.Text;


namespace libsecondlife.TestClient
{
	public class Program
	{
        	static void Main(string[] args)
        	{
			if(args.Length < 3 || args.Length == 4)
			{
				Console.WriteLine("Usage: TestClient.ext firstname lastname password [master name]");
			}
			TestClient testTool = new TestClient(args[0], args[1], args[2]);
			if(args.Length > 4) testTool.Master = args[3] + " " + args[4];
			testTool.Run();
		}
        }
}
