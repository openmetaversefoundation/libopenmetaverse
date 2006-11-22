using System;
using System.Collections.Generic;
using System.Text;

using CommandLine;

namespace libsecondlife.TestTool
{
    public class Program
    {
		[Argument(ArgumentType.Required, HelpText="First name of the SL account to log in with.")]
		public string FirstName;

		[Argument(ArgumentType.Required, HelpText="Last name of the SL account to log in with.")]
		public string LastName;

		[Argument(ArgumentType.Required, HelpText="Password of the SL account to log in with.")]
		public string Password;

		[Argument(ArgumentType.AtMostOnce, HelpText="Full account name to recieve IM commands from.")]
		public string MasterName;

        static void Main(string[] args)
        {
			Program program = new Program();
			CommandLine.Parser.ParseArgumentsWithUsage(args, program);

			TestTool testTool = new TestTool(program.FirstName, program.LastName, program.Password);
			testTool.Master = program.MasterName;			
			testTool.Run();
        }
    }
}
