using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

using System.Xml;
using System.Xml.Serialization;

using IA_SimpleInventory;
using libsecondlife;
using libsecondlife.InventorySystem;
using libsecondlife.AssetSystem;

using libsecondlife.Packets;

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
    class iManager
    {
        private char[] cmdSeperators = { ' ' };
        private string curDirectory = "/";

        private SecondLife _Client;
        private AppearanceManager aManager;

        private ManualResetEvent ConnectedSignal = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: Inventory [first] [last] [password]");
                return;
            }

            iManager it = new iManager();
            if (it.Connect(args[0], args[1], args[2]))
            {
                if (it.ConnectedSignal.WaitOne(TimeSpan.FromMinutes(1), false))
                {
                    it.doStuff();
                    it.Disconnect();
                }
            }
        }

        public iManager()
        {
            try
            {
                _Client = new SecondLife();
                _Client.Network.OnConnected += new NetworkManager.ConnectedCallback(Network_OnConnected);
                _Client.Self.OnTeleport += new TeleportCallback(Self_OnTeleport);
            }
            catch (Exception e)
            {
                // Error initializing the client
                Console.WriteLine();
                Console.WriteLine(e.ToString());
            }

        }

        private void doStuff()
        {
            System.Threading.Thread.Sleep(1000);

            // Download directory tree
            _Client.Inventory.getRootFolder().BeginDownloadContents(true, false);

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

                    case "setlook":
                        setlook();
                        break;

                    case "saveav":
                        saveavatar(curCmdLine);
                        break;

                    case "savew":
                        savewearables(curCmdLine);
                        break;

                    case "loadw":
                        loadwearables(curCmdLine);
                        break;

                    case "outfit":
                        outfit(curCmdLine);
                        break;

                    default:
                        Console.WriteLine("Unknown command '" + curCmdLine[0] + "'.");
                        Console.WriteLine("Type HELP for a list of available commands.");
                        break;
                }

            } while (shutdown == false);
            
        }

        void Self_OnTeleport(Simulator currentSim, string message, TeleportStatus status)
        {
            Console.WriteLine("Teleport Completed");
            StandUpStraight();
        }

        private void help()
        {
            Console.WriteLine("Currently available commands are: ");
            Console.WriteLine("LS          - List contents of the current directory. Options: /itemid /assetid /type");
            Console.WriteLine("CD          - Change directory.");
            Console.WriteLine("MKDIR       - Make a new directory.");
            Console.WriteLine("RMDIR       - Remove directory.");
            Console.WriteLine("GETASSET    - Fetch an asset from SL.");
            Console.WriteLine("REGIONINFO  - Display Grid Region Info.");
            Console.WriteLine("TELEPORT    - Teleport to a new sim.");
            Console.WriteLine("NOTECARD    - Create a new notecard.");
            Console.WriteLine("XML         - Display an item as xml.");
            Console.WriteLine("GETLOOK     - Send an AgentSetAppearance based on your current weables.");
            Console.WriteLine("SAVEAV      - Serialize your current wearables and the info from them.");
            Console.WriteLine("SAVEW       - Serialize your current wearables.");
            Console.WriteLine("LOADW       - Load a previously serialized wearables.");
            Console.WriteLine("QUIT        - Exit the Inventory Manager.");
        }

        private void outfit(string[] cmdLine)
        {
            if (cmdLine.Length < 2 )
            {
                Console.WriteLine("Usage: outfit [folder]");
                return;
            }

            // Resolve outfit folder
            string targetDir = "";

            if (cmdLine[1].StartsWith("/"))
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

            InventoryFolder iFolder = _Client.Inventory.getFolder(targetDir);

            if (iFolder == null)
            {
                Console.WriteLine("Could not find directory: " + targetDir);
                return;
            }
            if (aManager == null)
            {
                aManager = new AppearanceManager(_Client);
            }

            aManager.WearOutfit(iFolder);

        }

        private void savewearables(string[] cmdLine)
        {
            if (aManager == null)
            {
                aManager = new AppearanceManager(_Client);
            }

            // Get Wearable Data
            AgentWearablesUpdatePacket.WearableDataBlock[] wdbs = aManager.GetWearables();
            List<AgentWearablesUpdatePacket.WearableDataBlock> WearablesList = new List<AgentWearablesUpdatePacket.WearableDataBlock>();
            foreach (AgentWearablesUpdatePacket.WearableDataBlock wdb in wdbs)
            {
                WearablesList.Add(wdb);
            }
            
            // Serialize to XML
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings xmlws = new XmlWriterSettings();
            xmlws.Indent = true;
            XmlWriter xmlw = XmlWriter.Create(sb, xmlws);
            XmlSerializer serializer = new XmlSerializer(typeof(List<AgentWearablesUpdatePacket.WearableDataBlock>));
            serializer.Serialize(xmlw, WearablesList);

            // Output
            if (cmdLine.Length >= 2)
            {
                Console.WriteLine("Writing wearable data to : " + cmdLine[1]);

                File.WriteAllText(cmdLine[1], sb.ToString());

                Console.WriteLine("Done...");
            }
            else
            {
                Console.WriteLine(sb.ToString());
            }
        }

        private void loadwearables(string[] cmdLine)
        {
            if (cmdLine.Length < 2)
            {
                Console.WriteLine("You must specify the file to load the wearables from.");
                Console.WriteLine("Usage: loadw [file.xml]");
                return;
            }

            Console.WriteLine("Reading Wearable data from: " + cmdLine[1]);

            try
            {
                XmlReader xmlr = XmlReader.Create(File.OpenText(cmdLine[1]));

                XmlSerializer serializer = new XmlSerializer(typeof(List<AgentWearablesUpdatePacket.WearableDataBlock>));
                List<AgentWearablesUpdatePacket.WearableDataBlock> WearablesList = (List<AgentWearablesUpdatePacket.WearableDataBlock>)serializer.Deserialize(xmlr);

                foreach (AgentWearablesUpdatePacket.WearableDataBlock wdb in WearablesList)
                {
                    Console.WriteLine(wdb.AssetID);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("An error has occured...");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }

        }

        private void saveavatar(string[] cmdLine)
        {
            if (aManager == null)
            {
                aManager = new AppearanceManager(_Client);
            }

            AgentWearablesUpdatePacket.WearableDataBlock[] wdbs = aManager.GetWearables();
            aManager.GetAvatarAppearanceInfoFromWearableAssets();

            // Get the list of wearables
            
            List<AgentWearablesUpdatePacket.WearableDataBlock> WearablesList = new List<AgentWearablesUpdatePacket.WearableDataBlock>();
            foreach (AgentWearablesUpdatePacket.WearableDataBlock wdb in wdbs)
            {
                WearablesList.Add(wdb);
            }

            XmlWriterSettings xmlws = new XmlWriterSettings();
            xmlws.OmitXmlDeclaration = true;
            xmlws.Indent = true;
            xmlws.CloseOutput = false;

            StringBuilder Save = new StringBuilder();
            Save.Append("<avatar_appearance>");

            // Wearables
            StringBuilder sb = new StringBuilder();
            XmlWriter xmlw   = XmlWriter.Create(sb, xmlws);
            XmlSerializer serializer = new XmlSerializer(typeof(List<AgentWearablesUpdatePacket.WearableDataBlock>));
            serializer.Serialize(xmlw, WearablesList);

            Save.AppendLine(sb.ToString());

            // Parameters
            sb = new StringBuilder();
            xmlw = XmlWriter.Create(sb, xmlws);
            serializer = new XmlSerializer(typeof(SerializableDictionary<uint, float>));
            serializer.Serialize(xmlw, aManager.AgentAppearanceParams);

            Save.AppendLine(sb.ToString());

            // Parameters
            sb = new StringBuilder();
            xmlw = XmlWriter.Create(sb, xmlws);
            serializer = new XmlSerializer(typeof(TextureEntry));
            serializer.Serialize(xmlw, aManager.AgentTextureEntry);

            Save.AppendLine(sb.ToString());

            // Finish off save data
            Save.Append("</avatar_appearance>");

            Console.WriteLine(Save.ToString());

            //aManager.SendAgentSetAppearance

        }

        private void StandUpStraight()
        {
            AgentUpdatePacket p = new AgentUpdatePacket();
            p.AgentData.Far = 96.0f;
            p.AgentData.CameraAtAxis = new LLVector3(0, 0, 0);
            p.AgentData.CameraCenter = new LLVector3(0, 0, 0);
            p.AgentData.CameraLeftAxis = new LLVector3(0, 0, 0);
            p.AgentData.CameraUpAxis = new LLVector3(0, 0, 0);
            p.AgentData.HeadRotation = new LLQuaternion(0, 0, 0, 1); ;
            p.AgentData.BodyRotation = new LLQuaternion(0, 0, 0, 1); ;
            p.AgentData.AgentID = _Client.Network.AgentID;
            p.AgentData.SessionID = _Client.Network.SessionID;
            p.AgentData.ControlFlags = (uint)Avatar.AgentUpdateFlags.AGENT_CONTROL_STAND_UP;
            _Client.Network.SendPacket(p);
        }

        private void setlook()
        {
            if (aManager == null)
            {
                aManager = new AppearanceManager(_Client);
            }

            AgentWearablesUpdatePacket.WearableDataBlock[] wdbs = aManager.GetWearables();

            foreach (AgentWearablesUpdatePacket.WearableDataBlock wdb in wdbs)
            {
                Console.WriteLine(wdb.WearableType + " : " + wdb.ItemID);
            }

            // aManager.SendAgentSetAppearance();

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

            InventoryFolder iFolder = _Client.Inventory.getFolder(curDirectory);

            InventoryBase itemOfInterest = null;

            iFolder.BeginDownloadContents(false).RequestComplete.WaitOne(15000,false);
            foreach (InventoryBase ib in iFolder.GetContents())
            {
                if (ib is InventoryFolder)
                {
                    InventoryFolder folder = (InventoryFolder)ib;
                    if (folder.Name.Equals(cmdLine[1]) || folder.FolderID.Equals(uuid))
                    {
                        // Refresh the folder tree for this folder before outputing it.
                        folder.BeginDownloadContents(true).RequestComplete.WaitOne(30000, false);
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

            InventoryFolder iFolder = _Client.Inventory.getFolder(curDirectory);
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

            if (cmdLine[1].ToLower() == _Client.Network.CurrentSim.Region.Name.ToLower())
            {
                Console.WriteLine("TODO: Add the ability to teleport somewhere in the local region. " +
                    "Exiting for now, please specify a region other than the current one");
            }
            else
            {
                if (_Client.Grid.Regions.Count == 0)
                {
                    Console.WriteLine("Caching estate sims...");
                    _Client.Grid.AddEstateSims();
                    System.Threading.Thread.Sleep(3000);
                }


                _Client.Self.Teleport(cmdLine[1], new LLVector3(float.Parse(cmdLine[2]), float.Parse(cmdLine[3]), float.Parse(cmdLine[4])));
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

            GridRegion gr = _Client.Grid.GetGridRegion(regionName);
            Console.WriteLine(gr);
        }

        

        private void getasset(string[] cmdLine)
        {
            if (cmdLine.Length < 2)
            {
                Console.WriteLine("Usage for arbitrary asset: getasset [type] [UUID]");
                Console.WriteLine("Usage for asset of item (this works for notecards) : getasset [itemname | UUID]");
                Console.WriteLine("Example: getasset 13 c2ca25c1fb242e41650a54901bc2d21c");
                Console.WriteLine("Example: getasset \"New Script\"");
                return;
            }

            if (cmdLine.Length == 3)
            {
                // Arbitrary Asset
                try
                {
                    Asset asset = new Asset(cmdLine[2], sbyte.Parse(cmdLine[1]), null);

                    _Client.Assets.GetInventoryAsset(asset);

                    Console.WriteLine(asset.AssetDataToString());
                }
                catch (Exception)
                {
                    Console.WriteLine("Unable to find an asset with a UUID of: " + cmdLine[2]);
                }
            }
            else
            {
                // Asset for an item in inventory
                InventoryFolder iFolder = _Client.Inventory.getFolder(curDirectory);

                iFolder.BeginDownloadContents(false).RequestComplete.WaitOne(15000, false);
                foreach (InventoryBase ib in iFolder.GetContents())
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

            InventoryFolder iFolder = _Client.Inventory.getFolder(targetDir);
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

            InventoryFolder iFolder = _Client.Inventory.getFolder(curDirectory);

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


            InventoryFolder iFolder = _Client.Inventory.getFolder(targetDir);

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
            bool displayType = false;
            bool displayItemID = false;
            bool displayAssetID = false;

            foreach( string option in cmdLine )
            {
                if( option.ToLower().Equals("/itemid") )
                {
                    displayItemID = true;
                }
                if( option.ToLower().Equals("/assetid") )
                {
                    displayAssetID = true;
                }
                if( option.ToLower().Equals("/type") )
                {
                    displayType = true;
                }
            }

            Console.WriteLine("Contents of: " + curDirectory);
            Console.WriteLine("--------------------------------------------------");
            if (!curDirectory.Equals("/"))
            {
                Console.WriteLine("..");
            }

            InventoryFolder iFolder = _Client.Inventory.getFolder(curDirectory);
            iFolder.BeginDownloadContents(false).RequestComplete.WaitOne(15000, false);
            foreach (InventoryBase ib in iFolder.GetContents())
            {
                if (ib is InventoryFolder)
                {
                    InventoryFolder folder = (InventoryFolder)ib;
                    StringBuilder output = new StringBuilder("[DIR] ");

                    if( displayItemID )
                    {
                        output.Append("<itemid:" + folder.FolderID.ToStringHyphenated() + "> ");
                    }
                    if( displayType )
                    {
                        output.Append("<type:" + folder.GetDisplayType() + "> ");
                    }

                    output.Append(folder.Name);

                    Console.WriteLine( output );
                }
                else
                {
                    InventoryItem item = (InventoryItem)ib;
                    StringBuilder output = new StringBuilder();

                    if (displayItemID)
                    {
                        output.Append("<itemid:" + item.ItemID.ToStringHyphenated() + "> ");
                    }
                    if (displayAssetID)
                    {
                        output.Append("<assetid:" + item.AssetID.ToStringHyphenated() + "> ");
                    }
                    if (displayType)
                    {
                        output.Append("<type:" + item.GetDisplayType() + "> ");
                    }

                    output.Append(item.Name);

                    Console.WriteLine(output);
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

        void Network_OnConnected(object sender)
        {
            ConnectedSignal.Set();
        }

        protected bool Connect(string FirstName, string LastName, string Password)
        {
            Console.WriteLine("Attempting to connect and login to SecondLife.");

            // Setup Login to Second Life
            Dictionary<string, object> loginReply = new Dictionary<string, object>();

            // Login
            if (!_Client.Network.Login(FirstName, LastName, Password, "createnotecard", "static.sprocket@gmail.com"))
            {
                // Login failed
                Console.WriteLine("Error logging in: " + _Client.Network.LoginError);
                return false;
            }

            // Login was successful
            Console.WriteLine("Login was successful.");
            Console.WriteLine("AgentID:   " + _Client.Network.AgentID);
            Console.WriteLine("SessionID: " + _Client.Network.SessionID);

            return true;
        }

        protected void Disconnect()
        {
            // Logout of Second Life
            Console.WriteLine("Request logout");
            _Client.Network.Logout();
        }

    }
}
