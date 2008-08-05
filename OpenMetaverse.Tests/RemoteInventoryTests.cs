using System;
using System.Collections.Generic;
using System.Net;
using OpenMetaverse;
using OpenMetaverse.Packets;
using System.Threading;
using NUnit.Framework;



namespace OpenMetaverse.Tests
{
    [TestFixture]
    public class RemoteInventoryTests
    {
        public GridClient Client;
        public Inventory Inventory;
        public RemoteInventoryTests()
        {
            Client = new GridClient();
            Console.WriteLine("Logging in...");
            //string startLoc = NetworkManager.StartLocation("Hooper", 179, 18, 32);
            Client.Network.Login("Testing", "Anvil", "testinganvil", "Unit Test Framework", "last",
                "contact@libsecondlife.org");
        }

        [SetUp]
        public void Init()
        {
            Assert.IsTrue(Client.Network.Connected, "Client is not connected to the grid");

            Inventory = new Inventory(Client.Inventory, Client.Inventory.InventorySkeleton);
            Assert.IsNotNull(Inventory.RootFolder, "Root folder is null.");
            Assert.AreNotEqual(UUID.Zero, Inventory.RootUUID, "Root UUID is zero.");
        }

        ~RemoteInventoryTests()
        {
            if (Client.Network.Connected)
                Client.Network.Logout();
        }

        [Test]
        public void Rename()
        {
            InventoryFolder objects = Inventory[Client.Inventory.FindFolderForType(AssetType.Object)] as InventoryFolder;
            Assert.IsTrue(objects.DownloadContents(TimeSpan.FromSeconds(30)), "Initial contents request failure.");
            List<InventoryBase> contents = objects.Contents;
            CollectionAssert.IsNotEmpty(contents, "Objects folder does not contain any items");

            InventoryBase randomthing = contents[0];
            UUID uuid = randomthing.UUID;
            string oldName = randomthing.Name;
            Random rand = new Random();
            string newName = rand.Next().ToString();
            Console.WriteLine("Renaming {0} to {1}", oldName, newName);
            randomthing.Rename(newName);

            Assert.AreEqual(randomthing.Name, newName, "Local name update failed.");

            // Redownload the contents:
            Assert.IsTrue(objects.DownloadContents(TimeSpan.FromSeconds(30)), "Post-update contents request failure.");
            Assert.AreEqual(newName, Inventory[uuid].Name, "Remote update failed.");
        }

        [Test]
        public void Copy()
        {
            InventoryFolder notecards = Inventory[Client.Inventory.FindFolderForType(AssetType.Notecard)] as InventoryFolder;
            Assert.IsTrue(notecards.DownloadContents(TimeSpan.FromSeconds(30)), "Initial contents request failure");
            List<InventoryBase> contents = notecards.Contents;

            Console.WriteLine("Notecards folder contents:");
            InventoryItem note = null;
            foreach (InventoryBase ib in notecards)
            {
                if (ib is InventoryItem)
                {
                    if (note == null || note.Data.InventoryType != InventoryType.Notecard)
                    {
                        note = ib as InventoryItem;
                    }
                }
                Console.WriteLine(ib.Name);
            }
            Console.WriteLine("Chosen note: {0}", note.Name);

            InventoryItem copy = note.Copy(Inventory.RootFolder, TimeSpan.FromSeconds(30));
            Assert.IsNotNull(copy, "Copied ItemData not wrapped.");

            Console.WriteLine("Copied item:");
            Console.WriteLine(copy.Data.ToString());

            Console.WriteLine("Original item:");
            Console.WriteLine(note.Data.ToString());

            ItemData theoretical = note.Data;
            // All other properties should be identical.
            theoretical.ParentUUID = Inventory.RootUUID;
            theoretical.CreationDate = copy.Data.CreationDate;
            theoretical.UUID = copy.UUID;

            Assert.IsNotNull(Inventory[copy.UUID], "Copy not managed.");
            Assert.IsTrue(copy.Data == theoretical, "Copied item is not identical to original.");
        }
    }
}
