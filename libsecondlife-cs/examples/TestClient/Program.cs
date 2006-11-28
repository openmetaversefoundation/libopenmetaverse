using System;
using System.Collections.Generic;
using System.IO;

namespace libsecondlife.TestClient
{
    public class Program
    {
        private static void Usage()
        {
            Console.WriteLine("Usage: " + Environment.NewLine +
                    "TestClient.exe firstname lastname password [master name]" + Environment.NewLine +
                    "TestClient.exe --file filename [master name]");
        }

        static void Main(string[] args)
        {
            if (args.Length < 2 || args.Length > 5)
            {
                Usage();
                return;
            }

            TestClient tester;
            List<LoginDetails> accounts = new List<LoginDetails>();
            LoginDetails account;

            if (args[0] == "--file")
            {
                // Loading names from a file
                try
                {
                    using (StreamReader reader = new StreamReader(args[1]))
                    {
                        string line;
                        int lineNumber = 0;

                        while ((line = reader.ReadLine()) != null)
                        {
                            lineNumber++;
                            string[] tokens = line.Trim().Split(new char[] { ' ', ',' });

                            if (tokens.Length == 3)
                            {
                                account = new LoginDetails();
                                account.FirstName = tokens[0];
                                account.LastName = tokens[1];
                                account.Password = tokens[2];

                                if (args.Length == 4)
                                {
                                    account.Master = args[2] + " " + args[3];
                                }

                                accounts.Add(account);
                            }
                            else
                            {
                                Console.WriteLine("Invalid data on line " + lineNumber +
                                    ", must be in the format of: FirstName LastName Password");
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Error reading from " + args[1]);
                    return;
                }
            }
            else if (args.Length == 3 || args.Length == 5)
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
            else
            {
                Usage();
                return;
            }

            // Login the accounts and run the input loop
            tester = new TestClient(accounts);
            tester.Run();
        }
    }
}
