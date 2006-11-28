using System;
using System.Collections.Generic;
using System.Text;


namespace libsecondlife.TestClient
{
    public class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1 || args.Length > 5)
            {
                Console.WriteLine("Usage: " + Environment.NewLine +
                    "TestClient.exe firstname lastname password [master name]" + Environment.NewLine +
                    "TestClient.exe filename [master name]");
            }

            TestClient tester;
            List<LoginDetails> accounts = new List<LoginDetails>();
            LoginDetails account;

            if (args.Length <= 2)
            {
                // Loading names from a file
                // FIXME:

                Console.WriteLine("FIXME!");
                return;
            }
            else
            {
                // Taking a single login off the command-line
                account = new LoginDetails();
                account.FirstName = args[0];
                account.LastName = args[1];
                account.Password = args[2];

                if (args.Length == 5)
                {
                    account.Master = args[3] + " " + args[4];
                }

                accounts.Add(account);
            }

            tester = new TestClient(accounts);
            tester.Run();
        }
    }
}
