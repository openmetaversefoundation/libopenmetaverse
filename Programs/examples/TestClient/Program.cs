using System;
using System.Collections.Generic;
using System.IO;
using CommandLine.Utility;

namespace OpenMetaverse.TestClient
{
    public class CommandLineArgumentsException : Exception
    {
    }

    public class Program
    {
        private static void Usage()
        {
            Console.WriteLine("Usage: " + Environment.NewLine +
                    "TestClient.exe --first firstname --last lastname --pass password [--loginuri=\"uri\"] [--startpos \"sim/x/y/z\"] [--master \"master name\"] [--masterkey \"master uuid\"] [--gettextures] [--scriptfile \"filename\"]");
        }

        static void Main(string[] args)
        {
            Arguments arguments = new Arguments(args);

            ClientManager manager;
            List<LoginDetails> accounts = new List<LoginDetails>();
            LoginDetails account;
            bool groupCommands = false;
            string masterName = String.Empty;
            UUID masterKey = UUID.Zero;
            string file = String.Empty;
            string loginuri = String.Empty;
            bool getTextures = false;
            string scriptFile = String.Empty;

            if (arguments["groupcommands"] != null)
                groupCommands = true;

            if (arguments["masterkey"] != null)
                masterKey = UUID.Parse(arguments["masterkey"]);

            if (arguments["master"] != null)
                masterName = arguments["master"];

            if (arguments["loginuri"] != null)
                loginuri = arguments["loginuri"];

            if (arguments["gettextures"] != null)
                getTextures = true;

            if (arguments["scriptfile"] != null)
            {
                scriptFile = arguments["scriptfile"];
                if (!File.Exists(scriptFile))
                {
                    Console.WriteLine("File {0} Does not exist", scriptFile);
                    return;
                }
            }

            if (arguments["file"] != null)
            {
                file = arguments["file"];

                if (!File.Exists(file))
                {
                    Console.WriteLine("File {0} Does not exist", file);
                    return;
                }

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

                                if (tokens.Length >= 4) // Optional starting position
                                {
                                    char sep = '/';
                                    string[] startbits = tokens[3].Split(sep);
                                    account.StartLocation = NetworkManager.StartLocation(startbits[0], Int32.Parse(startbits[1]),
                                        Int32.Parse(startbits[2]), Int32.Parse(startbits[3]));
                                }

                                accounts.Add(account);
                            }
                            else
                            {
                                Logger.Log("Invalid data on line " + lineNumber +
                                    ", must be in the format of: FirstName LastName Password [Sim/StartX/StartY/StartZ]",
                                    Helpers.LogLevel.Warning);
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
            else if (arguments["first"] != null && arguments["last"] != null && arguments["pass"] != null)
            {
                // Taking a single login off the command-line
                account = new LoginDetails();
                account.FirstName = arguments["first"];
                account.LastName = arguments["last"];
                account.Password = arguments["pass"];

                accounts.Add(account);
            }
            else if (arguments["help"] != null)
            {
                Usage();
                return;
            }

            foreach (LoginDetails a in accounts)
            {
                a.GroupCommands = groupCommands;
                a.MasterName = masterName;
                a.MasterKey = masterKey;
                a.URI = loginuri;

                if (arguments["startpos"] != null)
                {
                    char sep = '/';
                    string[] startbits = arguments["startpos"].Split(sep);
                    a.StartLocation = NetworkManager.StartLocation(startbits[0], Int32.Parse(startbits[1]),
                            Int32.Parse(startbits[2]), Int32.Parse(startbits[3]));
                }
            }

            // Login the accounts and run the input loop
            manager = new ClientManager(accounts, getTextures);

            if (!String.IsNullOrEmpty(scriptFile))
                manager.DoCommandAll("script " + scriptFile, UUID.Zero);

            // Then Run the ClientManager normally
            manager.Run();
        }
    }
}
