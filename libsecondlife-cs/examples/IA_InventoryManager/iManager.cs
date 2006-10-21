using System;
using System.Collections.Generic;
using System.Text;

using IA_SimpleInventory;
using libsecondlife;
using libsecondlife.InventorySystem;

namespace IA_InventoryManager
{
    /// <summary>
    /// Todo:
    /// * TYPE or VIEW (for notecards, landmarks, etc)
    /// * RENAME
    /// * DOWNLOAD
    /// * COPY
    /// * MOVE
    /// * GIVE
    /// </summary>
    class iManager : SimpleInventory
    {
        private char[] cmdSeperators = { ' ' };
        private string curDirectory = "/";

        private string TYPE_DIR  = "<DIR>  ";
        private string TYPE_ITEM = "<ITEM> ";


        static new void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: Inventory [first] [last] [password]");
                return;
            }

            iManager it = new iManager();
            it.Connect(args[0], args[1], args[2]);
            it.doStuff();
            it.Disconnect();

            System.Threading.Thread.Sleep(500);

        }

        private new void doStuff()
        {
            System.Threading.Thread.Sleep(1000);

            Console.WriteLine("==================================================================");
            Console.WriteLine("The Inventory Manager program provides a simple shell for working with your Second Life[tm] Avatar's Inventory.");
            Console.WriteLine();
            Console.WriteLine("Type HELP for a list of available commands.");
            Console.WriteLine("-------------------------------------------");

            bool shutdown = false;


            do
            {
                Console.WriteLine();
                Console.Write( curDirectory + "> ");

                string curCmd = Console.ReadLine();
                string[] curCmdLine = curCmd.Split(cmdSeperators);

                switch (curCmdLine[0].ToLower())
                {
                    case "quit":
                    case "exit":
                    case "q":
                        shutdown = true;
                        break;

                    case "help":
                        help();
                        break;

                    case "ls":
                    case "dir":
                        ls( curCmdLine );
                        break;

                    case "cd":
                        cd(curCmdLine);
                        break;

                    case "mkdir":
                        mkdir(curCmdLine);
                        break;

                    case "rmdir":
                        rmdir(curCmdLine);
                        break;

                    default:
                        Console.WriteLine("Unknown command '" + curCmdLine[0] + "'.");
                        Console.WriteLine("Type HELP for a list of available commands.");
                        break;
                }

            } while (shutdown == false);
            
        }

        private void help()
        {
            Console.WriteLine("Currently available commands are: ");
            Console.WriteLine("LS          - List contents of the current directory.");
            Console.WriteLine("CD [dir]    - Change directory.");
            Console.WriteLine("MKDIR [dir] - Make a new directory.");
            Console.WriteLine("RMDIR [dir] - Remove directory.");
            Console.WriteLine("QUIT        - Exit the Inventory Manager.");
        }

        private void rmdir(string[] cmdLine)
        {
            if (cmdLine.Length < 2)
            {
                Console.WriteLine("Usage: rmdir [directory]");
                Console.WriteLine("Example: rmdir SubDir");
                return;
            }

            string targetDir = "";
            if (!curDirectory.Equals("/"))
            {
                targetDir = curDirectory + "/";

            }
            targetDir += combineCmdArg(cmdLine);

            InventoryFolder iFolder = AgentInventory.getFolder(targetDir);
            if (iFolder == null)
            {
                Console.WriteLine("Could not find directory: " + targetDir);
                return;
            }
            else
            {
                iFolder.Delete();
            }
        }

        private void mkdir(string[] cmdLine)
        {
            if (cmdLine.Length < 2)
            {
                Console.WriteLine("Usage: mkdir [directory]");
                Console.WriteLine("Example: mkdir SubDir");
                return;
            }

            string targetDir = combineCmdArg(cmdLine);

            InventoryFolder iFolder = AgentInventory.getFolder(curDirectory);

            InventoryFolder newFolder = iFolder.CreateFolder(targetDir);

            if (newFolder != null)
            {
                Console.WriteLine("Directory created: " + targetDir);
            }
            else
            {
                Console.WriteLine("Error creating directory: " + targetDir);
            }
        }

        private void cd(string[] cmdLine)
        {
            if( cmdLine.Length < 2 )
            {
                Console.WriteLine("Usage: cd [directory]");
                Console.WriteLine("Example: cd /Notecards");
                return;
            }

            string targetDir = "";

            if (cmdLine[1].Equals(".."))
            {
                targetDir = curDirectory.Substring(0, curDirectory.LastIndexOf("/"));
            }
            else if (cmdLine[1].StartsWith("/"))
            {
                targetDir += combineCmdArg(cmdLine);
            }
            else
            {
                if (!curDirectory.Equals("/"))
                {
                    targetDir = curDirectory + "/";

                }

                targetDir += combineCmdArg(cmdLine);
            }
            Console.WriteLine("Changing directory to: " + targetDir );


            InventoryFolder iFolder = AgentInventory.getFolder(targetDir);

            if (iFolder == null)
            {
                Console.WriteLine("Could not find directory: " + targetDir);
                return;
            }
            else
            {
                if (targetDir.StartsWith("/"))
                {
                    curDirectory = targetDir;
                }
                else
                {
                    curDirectory = "/" + targetDir;
                }
            }
        }

        private void ls(string[] cmdLine)
        {
            Console.WriteLine("Contents of: " + curDirectory);
            Console.WriteLine("--------------------------------------------------");
            if (!curDirectory.Equals("/"))
            {
                Console.WriteLine("..");
            }

            InventoryFolder iFolder = AgentInventory.getFolder(curDirectory);
            foreach (InventoryBase ib in iFolder.alContents)
            {
                if (ib is InventoryFolder)
                {
                    InventoryFolder folder = (InventoryFolder)ib;
                    Console.WriteLine(TYPE_DIR + folder.Name);
                }
                else
                {
                    InventoryItem item = (InventoryItem)ib;
                    Console.WriteLine(TYPE_ITEM + item.Name);
                }
            }
        }

        private string combineCmdArg( string[] cmdLine )
        {
            return combineString( cmdLine, 1 );
        }
        private string combineString( string[] parts, int start )
        {
            string rtn = "";
            for (int i = start; i < parts.Length; i++)
            {
                rtn += " " + parts[i];
            }
            return rtn.Trim();
        }
    }
}
