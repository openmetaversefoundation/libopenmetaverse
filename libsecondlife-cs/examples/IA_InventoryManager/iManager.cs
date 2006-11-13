using System;
using System.Collections.Generic;
using System.Text;

using IA_SimpleInventory;
using libsecondlife;
using libsecondlife.InventorySystem;
using libsecondlife.AssetSystem;

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
            Console.WriteLine("The Inventory Manager program provides a simple shell for working with your Second Life[tm] Avatar.");
            Console.WriteLine();
            Console.WriteLine("Type HELP for a list of available commands.");
            Console.WriteLine("-------------------------------------------");

            bool shutdown = false;


            do
            {
                Console.WriteLine();
                Console.Write( curDirectory + "> ");

                string curCmd = Console.ReadLine();
                string[] curCmdLineParts = curCmd.Split(cmdSeperators);
                List<string> merged = new List<string>();

                bool inQuotedString = false;
                string temp = "";
                foreach (string s in curCmdLineParts)
                {
                    if (s.StartsWith("\""))
                    {
                        temp = s.Remove(0,1);
                        inQuotedString = true;
                    }
                    else if (s.EndsWith("\""))
                    {
                        temp += " " + s.Remove(s.LastIndexOf('"'));
                        merged.Add(temp);
                        temp = "";
                        inQuotedString = false;
                    }
                    else
                    {
                        if (inQuotedString)
                        {
                            temp += " " + s;
                        }
                        else
                        {
                            merged.Add(s);
                        }
                    }
                }

                string[] curCmdLine = new string[merged.Count];
                int i = 0;
                foreach( string s in merged )
                {
                    curCmdLine[i++] = s;
                }

                switch (curCmdLine[0].ToLower())
                {
                    case "quit":
                    case "exit":
                    case "bye":
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

                    case "getasset":
                        getasset(curCmdLine);
                        break;

                    case "regioninfo":
                        regioninfo(curCmdLine);
                        break;

                    case "teleport":
                        teleport(curCmdLine);
                        break;

                    case "notecard":
                        notecard(curCmdLine);
                        break;

                    case "xml":
                        xml(curCmdLine);
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
            Console.WriteLine("CD          - Change directory.");
            Console.WriteLine("MKDIR       - Make a new directory.");
            Console.WriteLine("RMDIR       - Remove directory.");
            Console.WriteLine("GETASSET    - Fetch an asset from SL.");
            Console.WriteLine("REGIONINFO  - Display Grid Region Info.");
            Console.WriteLine("TELEPORT    - Teleport to a new sim.");
            Console.WriteLine("NOTECARD    - Create a new notecard.");
            Console.WriteLine("XML         - Display an item as xml");
            Console.WriteLine("QUIT        - Exit the Inventory Manager.");
        }

        private void xml(string[] cmdLine)
        {
            if (cmdLine.Length < 2)
            {
                Console.WriteLine("Usage: XML [itemName | uuid] ([outputAsset true/false])");
                return;
            }

            LLUUID uuid = null;

            try
            {
                uuid = new LLUUID(cmdLine[1]);
            }
            catch (Exception)
            {
            }

            InventoryFolder iFolder = AgentInventory.getFolder(curDirectory);

            InventoryBase itemOfInterest = null;

            foreach (InventoryBase ib in iFolder.alContents)
            {
                if (ib is InventoryFolder)
                {
                    InventoryFolder folder = (InventoryFolder)ib;
                    if (folder.Name.Equals(cmdLine[1]) || folder.FolderID.Equals(uuid))
                    {
                        itemOfInterest = folder;
                        break;
                    }
                }
                else if (ib is InventoryItem)
                {
                    InventoryItem item = (InventoryItem)ib;
                    if (item.Name.Equals(cmdLine[1]) || item.ItemID.Equals(uuid))
                    {
                        itemOfInterest = item;
                        break;
                    }
                }
            }

            if (itemOfInterest == null)
            {
                Console.WriteLine("Could not find: " + cmdLine[1]);
                return;
            }

            if (cmdLine.Length == 3)
            {
                Console.WriteLine(itemOfInterest.toXML(bool.Parse(cmdLine[2])));
            }
            else
            {
                Console.WriteLine(itemOfInterest.toXML(false));
            }

        }


        private void notecard(string[] cmdLine)
        {
            string NoteName = "";
            for( int i = 1; i < cmdLine.Length; i++ )
            {
                NoteName += cmdLine[i] + " ";
            }
            NoteName = NoteName.Trim();

            Console.Write("Description: ");
            string NoteDesc = Console.ReadLine();

            Console.WriteLine("Please enter body, press ESC to end");
            Console.WriteLine("----------------------------------");

            StringBuilder sb = new StringBuilder();
            ConsoleKeyInfo cki;
            do
            {
                cki = Console.ReadKey(true);
                sb.Append(cki.Key.ToString());
            } while (cki.Key != ConsoleKey.Escape);

            InventoryFolder iFolder = AgentInventory.getFolder(curDirectory);
            iFolder.NewNotecard(cmdLine[1], NoteDesc, sb.ToString());

            Console.WriteLine("Notecard '" + NoteName + " 'Created");
        }

        private void teleport(string[] cmdLine)
        {
            if (cmdLine.Length < 5)
            {
                Console.WriteLine("Usage: teleport [sim] [x] [y] [z]");
                Console.WriteLine("Example: teleport hooper 182 40 26");
                return;
            }

            if (cmdLine[1].ToLower() == client.Network.CurrentSim.Region.Name.ToLower())
            {
                Console.WriteLine("TODO: Add the ability to teleport somewhere in the local region. " +
                    "Exiting for now, please specify a region other than the current one");
            }
            else
            {
                if (client.Grid.Regions.Count == 0)
                {
                    Console.WriteLine("Caching estate sims...");
                    client.Grid.AddEstateSims();
                    System.Threading.Thread.Sleep(3000);
                }

                
                client.Self.Teleport(cmdLine[1], new LLVector3( float.Parse(cmdLine[2]), float.Parse(cmdLine[3]), float.Parse(cmdLine[4]) ) );
            }
        }

        private void regioninfo(string[] cmdLine)
        {
            if (cmdLine.Length < 2)
            {
                Console.WriteLine("Usage: regioninfo [name]");
                Console.WriteLine("Example: regioninfo ahern");
                return;
            }

            string regionName = "";
            for( int i = 1; i < cmdLine.Length; i++ )
            {
                regionName += cmdLine[i] + " ";
            }
            regionName = regionName.Trim();

            GridRegion gr = client.Grid.GetGridRegion(regionName);
            Console.WriteLine(gr);
        }

        

        private void getasset(string[] cmdLine)
        {
            if (cmdLine.Length < 2)
            {
                Console.WriteLine("Usage for arbitrary asset: getasset [type] [UUID]");
                Console.WriteLine("Usage for asset of item  : getasset [itemname | UUID]");
                Console.WriteLine("Example: getasset 13 c2ca25c1fb242e41650a54901bc2d21c");
                Console.WriteLine("Example: getasset \"New Script\"");
                return;
            }

            if (cmdLine.Length == 3)
            {
                // Arbitrary Asset
                Asset asset = new Asset(cmdLine[2], sbyte.Parse(cmdLine[1]), null);

                if (asset.Type == 13)
                {
                    AgentInventory.getAssetManager().GetInventoryAsset(asset);
                }
                else
                {
                    Console.WriteLine("Can't currently retrieve assets other then type 13 using this method.");
                    return;
                }

                Console.WriteLine(asset.AssetDataToString());
            }
            else
            {
                // Asset for an item in inventory
                InventoryFolder iFolder = AgentInventory.getFolder(curDirectory);
                foreach (InventoryBase ib in iFolder.alContents)
                {
                    if (ib is InventoryItem)
                    {
                        InventoryItem item = (InventoryItem)ib;
                        if (item.ItemID.Equals(cmdLine[1]) || item.Name.Equals(cmdLine[1]))
                        {
                            Console.WriteLine("Asset for " + item.Name + " [" + item.ItemID + "]");
                            Console.WriteLine(item.Asset.AssetDataToString());
                        }
                    }
                }
            }

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
                    Console.WriteLine(TYPE_DIR + "<" + folder.FolderID.ToStringHyphenated() + "> " + folder.Name);
                }
                else
                {
                    InventoryItem item = (InventoryItem)ib;
                    Console.WriteLine(TYPE_ITEM + "<" + item.ItemID.ToStringHyphenated() + "> " + item.Name);
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
