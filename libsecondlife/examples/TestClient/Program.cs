using System;
using System.Collections.Generic;
using System.IO;
using CommandLine.Utility;

namespace libsecondlife.TestClient
{
    public class Program
    {
        private static void Usage()
        {
            Console.WriteLine("Usage: " + Environment.NewLine +
                    "TestClient.exe --first firstname --last lastname --pass password --contact \"youremail\" [--startpos \"sim/x/y/z\"] [--master \"master name\"]" + 
                    Environment.NewLine + "TestClient.exe --file filename --contact \"youremail\" [--master \"master name\"]");
        }

        static void Main(string[] args)
        {
            Arguments arguments = new Arguments(args);

            ClientManager manager;
            List<LoginDetails> accounts = new List<LoginDetails>();
            LoginDetails account;
            string master = "";
            string file = "";
			string contact = "";

            if (arguments["master"] != null)
            {
                master = arguments["master"];
            }

			if (arguments["contact"] != null)
			{
				contact = arguments["contact"];
				if (arguments["file"] != null)
				{
					file = arguments["file"];

					// Loading names from a file
					try
					{
						using (StreamReader reader = new StreamReader(file))
						{
							string line;
							int lineNumber = 0;

							while ((line = reader.ReadLine()) != null)
							{
								lineNumber++;
								string[] tokens = line.Trim().Split(new char[] { ' ', ',' });

								if (tokens.Length >= 3)
								{
									account = new LoginDetails();
									account.FirstName = tokens[0];
									account.LastName = tokens[1];
									account.Password = tokens[2];

									accounts.Add(account);

									// Leaving this out until we have per-account masters (if that
									// is desirable). For now the command-line option can 
									// specify the single master that TestClient supports
									
									//if (tokens.Length == 5)
									//{
									//    master = tokens[3] + " " + tokens[4];
									//}
								}
								else
								{
									Console.WriteLine("Invalid data on line " + lineNumber +
										", must be in the format of: FirstName LastName Password");
								}
							}
						}
					}
					catch (Exception e)
					{
						Console.WriteLine("Error reading from " + args[1]);
						Console.WriteLine(e.ToString());
						return;
					}
				}
				else
				{
					if (arguments["first"] != null && arguments["last"] != null && arguments["pass"] != null)
					{
						// Taking a single login off the command-line
						account = new LoginDetails();
						account.FirstName = arguments["first"];
						account.LastName = arguments["last"];
						account.Password = arguments["pass"];

						accounts.Add(account);
					}
				}
				}
                else
                {
                    Usage();
                    return;
                }

			foreach (LoginDetails a in accounts)
				a.Master = master;

            // Login the accounts and run the input loop
			if ( arguments["start"] != null ) {
				manager = new ClientManager(accounts, contact, arguments["start"]);
			} else { 
				manager = new ClientManager(accounts, contact);
			}
			manager.Run();
				
        }
    }
}
