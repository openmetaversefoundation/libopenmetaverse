using System;
using System.Collections.Generic;
using System.Collections;
using System.Net;
using OpenMetaverse;
using OpenMetaverse.Packets;
using NUnit.Framework;

namespace OpenMetaverse.Tests
{
    public class ExposedInventory : Inventory
    {
        public Dictionary<UUID, InventoryBase> GetItems()
        {
            return Items;
        }

        public void InjectUpdate(ItemData data)
        {
            base.manager_OnItemUpdate(data);
        }

        public void InjectUpdate(FolderData data)
        {
            base.manager_OnFolderUpdate(data);
        }

        public ExposedInventory(InventoryManager manager, InventorySkeleton skeleton)
            : base(manager, skeleton) { }
    }

    [TestFixture]
    public class LocalInventoryTests
    {
        public NetworkManager Network;
        public InventorySkeleton Skeleton;
        public InventoryManager Manager;
        public ExposedInventory Inventory;

        public UUID RootUUID;
        public UUID OwnerUUID;
        public GridClient Client;

        public LocalInventoryTests()
        {
            Client = new GridClient();
            Console.WriteLine("Logging in...");
            //string startLoc = NetworkManager.StartLocation("Hooper", 179, 18, 32);
            Client.Network.Login("Testing", "Anvil", "testinganvil", "Unit Test Framework", "last",
                "contact@libsecondlife.org");
        }

        ~LocalInventoryTests()
        {
            if (Client.Network.Connected)
                Client.Network.Logout();
        }

        [SetUp]
        public void Init()
        {
            Network = Client.Network;
            Manager = Client.Inventory;
            Skeleton = Manager.InventorySkeleton;
            Inventory = new ExposedInventory(Manager, Skeleton);
            OwnerUUID = Client.Self.AgentID;
            RootUUID = Skeleton.RootUUID;
            Assert.IsNotNull(Inventory.RootFolder, "Root folder is not managed.");
            Assert.AreEqual(Skeleton.Folders.Length, Inventory.GetItems().Count, "Inventory skeleton partially managed.");
            Assert.AreEqual(Skeleton.RootUUID, Inventory.RootFolder.UUID, "Skeleton root does not match inventory root.");
        }

        [Test]
        public void StructAbstractions()
        {
            FolderData newFolder = ValidFolder(RootUUID);
            ItemData newItem = ValidItem(RootUUID);

            InventoryBase item = Inventory.Manage(newItem);
            InventoryBase folder = Inventory.Manage(newFolder);

            // ItemData abstractions:
            Assert.AreEqual(item.UUID, (item as InventoryItem).Data.UUID, "ItemData UUID abstraction failed.");
            Assert.AreEqual(item.ParentUUID, (item as InventoryItem).Data.ParentUUID, "ItemData ParentUUID abstraction failed.");
            Assert.AreEqual(item.Name, (item as InventoryItem).Data.Name, "ItemData Name abstraction failed.");
            Assert.AreEqual(item.OwnerUUID, (item as InventoryItem).Data.OwnerID, "ItemData OwnerID abstraction failed.");

            // FolderData abstractions:
            Assert.AreEqual(folder.UUID, (folder as InventoryFolder).Data.UUID, "FolderData UUID abstraction failed.");
            Assert.AreEqual(folder.ParentUUID, (folder as InventoryFolder).Data.ParentUUID, "FolderData ParentUUID abstraction failed.");
            Assert.AreEqual(folder.Name, (folder as InventoryFolder).Data.Name, "FolderData Name abstraction failed.");
            Assert.AreEqual(folder.OwnerUUID, (folder as InventoryFolder).Data.OwnerID, "FolderData OwnerID abstraction failed.");
        }

        [Test]
        public void RejectUnowned()
        {
            FolderData badFolder = ValidFolder(RootUUID);
            badFolder.OwnerID = UUID.Random();

            ItemData badItem = ValidItem(RootUUID);
            badItem.OwnerID = UUID.Random();

            // Test folder:
            Assert.IsNull(Inventory.Manage(badFolder), "Inventory managed folder it doesn't own.");
            Assert.IsNull(Inventory[badFolder.UUID], "Inventory stored folder it doesn't own.");

            Inventory.InjectUpdate(badFolder);
            Assert.IsNull(Inventory[badFolder.UUID], "Inventory stored and managed folder it doesn't own from update.");

            // Test item:
            Assert.IsNull(Inventory.Manage(badItem), "Inventory managed item it doesn't own.");
            Assert.IsNull(Inventory[badItem.UUID], "Inventory stored item it doesn't own.");

            Inventory.InjectUpdate(badItem);
            Assert.IsNull(Inventory[badFolder.UUID], "Inventory stored and managed item it doesn't own from update.");
        }

        [Test]
        public void ExplicitManagment()
        {
            // Parent arrives before item. 
            {
                FolderData newFolder = ValidFolder(RootUUID);

                ItemData newItem = ValidItem(newFolder.UUID);

                // Test folder, explicit manage:

                InventoryBase folder = Inventory.Manage(newFolder);
                Assert.IsNotNull(folder, "Folder is not managed.");
                Assert.IsInstanceOfType(typeof(InventoryFolder), folder, "Managed FolderData did not result in InventoryFolder wrapper.");
                Assert.IsNotNull(Inventory[newFolder.UUID], "Folder not stored.");
                Assert.IsEmpty((ICollection)((folder as InventoryFolder).Contents), "Folder not empty.");
                Assert.AreSame(Inventory.RootFolder, folder.Parent, "Folder parent is not the root folder.");

                // Test item, explicit manage:

                InventoryBase item = Inventory.Manage(newItem);
                Assert.IsNotNull(item, "Item is not managed.");
                Assert.IsInstanceOfType(typeof(InventoryItem), item, "Managed ItemData did not result in InventoryItem wrapper.");
                Assert.IsNotNull(Inventory[newItem.UUID], "Item not stored.");
                Assert.IsNotNull(item.Parent, "Item's parent reference not updated.");
                CollectionAssert.Contains(folder as InventoryFolder, item, "Folder not updated with managed item.");
                Assert.AreEqual(1, (folder as InventoryFolder).Contents.Count, "Folder contains more then 1 item.");
            }

            // Opposite scenerio, item arrives before parent.
            {
                FolderData newFolder = ValidFolder(RootUUID);

                ItemData newItem = ValidItem(newFolder.UUID);

                InventoryItem item = Inventory.Manage(newItem) as InventoryItem;
                Assert.IsNotNull(item, "Parentless item not managed.");
                Assert.IsNotNull(Inventory[newItem.UUID], "Parentless item not stored.");
                Assert.IsNull(Inventory[newFolder.UUID], "Parent stored before FolderData for it obtained.");
                Assert.IsNull(item.Parent, "Parent referenced before FolderData for it obtained.");

                InventoryFolder folder = Inventory.Manage(newFolder) as InventoryFolder;
                CollectionAssert.Contains(folder, item, "Early item not added to late parent folder.");
            }
        }

        [Test]
        public void UpdateManagment()
        {
            FolderData newFolder = ValidFolder(RootUUID);

            ItemData newItem = ValidItem(newFolder.UUID);
            newItem.Name = "Foo";

            // Reject updates from unmanaged items:
            Inventory.InjectUpdate(newItem);
            Assert.IsNull(Inventory[newItem.UUID], "Update for unmanaged item stored.");

            // Test new folder from update, child of managed folder:
            Inventory.InjectUpdate(newFolder);
            InventoryFolder folder = Inventory[newFolder.UUID] as InventoryFolder;
            Assert.IsNotNull(folder, "New child of managed folder not stored.");
            CollectionAssert.Contains(Inventory.RootFolder, folder, "Managed folder does not contain new child folder.");

            // Test new item from update:
            Inventory.InjectUpdate(newItem);
            InventoryItem item = Inventory[newItem.UUID] as InventoryItem;
            Assert.IsNotNull(item, "New child of new folder not stored.");
            CollectionAssert.Contains(folder, item, "New folder does not have reference to new item.");

            // Test wrapper data struct update:
            Assert.AreEqual(item.Name, "Foo", "New folder ItemData wrap failue.");
            newItem.Name = "Bar";
            Inventory.InjectUpdate(newItem);
            Assert.AreEqual(item.Name, "Bar", "Item wrapper update failure.");

            Assert.AreEqual(0, folder.Data.DescendentCount, "New folder DescendentCount set from non-update.");
            newFolder.DescendentCount = 1;
            Inventory.InjectUpdate(newFolder);
            Assert.AreEqual(1, folder.Data.DescendentCount, "Folder wrapper update failure.");

            // Test parent change update:
            newItem.ParentUUID = RootUUID;
            Inventory.InjectUpdate(newItem);
            Assert.AreSame(item.Parent, Inventory.RootFolder, "New parent reference not set.");
            CollectionAssert.DoesNotContain(folder, item, "Removal from old parent failed.");
            CollectionAssert.Contains(Inventory.RootFolder, item, "Add to new parent failed.");

        }

        [Test]
        public void Removal()
        {
            FolderData newFolder = ValidFolder(RootUUID);
            ItemData newItem = ValidItem(newFolder.UUID);

            InventoryFolder folder = Inventory.Manage(newFolder);
            InventoryItem item = Inventory.Manage(newItem);

            folder.Remove();
            CollectionAssert.DoesNotContain(Inventory, folder, "Delete failed to unmanage item.");
            CollectionAssert.DoesNotContain(Inventory, item, "Delete failed to recurse.");
        }

        [Test]
        public void InventoryFromPath()
        {
            // Null path:
            CollectionAssert.Contains(Inventory.InventoryFromPath(null), Inventory.RootFolder, "Null path does not return root");
            CollectionAssert.Contains(Inventory.InventoryFromPath(new string[0]), Inventory.RootFolder, "Empty path does not return root.");

            FolderData folder1 = ValidFolder(RootUUID);
            folder1.Name = "Objects";
            FolderData folder1_1 = ValidFolder(folder1.UUID);
            folder1_1.Name = "Trees";
            FolderData folder1_2 = ValidFolder(folder1.UUID);
            folder1_2.Name = "Gizmos";
            FolderData folder1_3 = ValidFolder(folder1.UUID);
            folder1_3.Name = "Purchases";

            ItemData item1_1 = ValidItem(folder1.UUID);
            item1_1.Name = "Object";
            ItemData item1_2 = ValidItem(folder1.UUID);
            item1_2.Name = "Object";
            ItemData item1_3 = ValidItem(folder1.UUID);
            item1_3.Name = "Tilly";
            ItemData item1_1_1 = ValidItem(folder1_1.UUID);
            item1_1_1.Name = "Pine";
            ItemData item1_1_2 = ValidItem(folder1_1.UUID);
            item1_1_2.Name = "Maple";
            ItemData item1_1_3 = ValidItem(folder1_1.UUID);
            item1_1_3.Name = "Maple";
            ItemData item1_3_1 = ValidItem(folder1_3.UUID);
            item1_3_1.Name = "Death Ray";
            ItemData item1_3_2 = ValidItem(folder1_3.UUID);
            item1_3_2.Name = "Maple";

            // Add all of these to the inventory:
            FolderData[] folders = new FolderData[] { folder1, folder1_1, folder1_2, folder1_3 };
            ItemData[] items = new ItemData[] { item1_1, item1_1_1, item1_1_2, item1_1_3, item1_2, item1_3, item1_3_1, item1_3_2 };

            foreach (FolderData folder in folders)
                Inventory.Manage(folder);
            foreach (ItemData item in items)
                Inventory.Manage(item);

            /* Inventory looks like:
             * Root
             *  folder1
             *      item1_1
             *      item1_2
             *      item1_3
             *      folder1_1
             *          item1_1_1
             *          item1_1_2
             *          item1_1_3
             *      folder1_2
             *      folder1_3
             *          item1_3_1
             *          item1_3_2
             */
            List<InventoryBase> results = null;
            // Test one item path.
            results = Inventory.InventoryFromPath(new string[] { "Objects", "Tilly" });
            CollectionAssert.Contains(results, Inventory[item1_3.UUID], "Path search did not find item at path to it.");
            Assert.AreEqual(results.Count, 1, "Unexpected results returned for single item path search.");

            // Test multiple items in same folder, with same path.
            results = Inventory.InventoryFromPath(new string[] { "Objects", "Object" });
            CollectionAssert.AreEquivalent(results, new InventoryBase[] { Inventory[item1_2.UUID], Inventory[item1_1.UUID] }, "Multiple result, same folders path failed.");

            folder1_1.Name = "Purchases"; // Rename folder1_1 from "Trees" to "Purchases"
            Inventory.InjectUpdate(folder1_1);
            // folder1_1 and folder1_3 now have the same name. 

            // Test items from different folders with the same path.
            results = Inventory.InventoryFromPath(new string[] { "Objects", "Purchases", "Maple" });
            // This should return item1_3_2 "Maple" in folder1_3 and item 1_1_2 "Maple" and item1_1_3 "Maple" in folder1_1.
            CollectionAssert.AreEquivalent(results,
                new InventoryBase[] { Inventory[item1_3_2.UUID], Inventory[item1_1_2.UUID], Inventory[item1_1_3.UUID] },
                "Multiple result, different folders path failed.");
        }

        [Test]
        public void Move()
        {
            FolderData newFolder1 = ValidFolder(RootUUID);
            ItemData newItem = ValidItem(newFolder1.UUID);
            FolderData newFolder2 = ValidFolder(RootUUID);


            InventoryFolder folder1 = Inventory.Manage(newFolder1);
            InventoryFolder folder2 = Inventory.Manage(newFolder2);
            InventoryItem item = Inventory.Manage(newItem);

            CollectionAssert.Contains(folder1, item);

            // Move item into folder2.
            item.Move(folder2);
            CollectionAssert.Contains(folder2, item, "Item not added to new parent's contents.");
            Assert.AreSame(item.Parent, folder2, "Item parent reference not updated.");
            Assert.AreEqual(item.ParentUUID, folder2.UUID, "Reference to parent's UUID not updated.");

            // Move folder2 into folder1
            folder2.Move(folder1);
            CollectionAssert.Contains(folder1, folder2, "Folder2 not added to new parent's contents.");
            Assert.AreSame(folder2.Parent, folder1, "Folder parent reference not updated.");
            Assert.AreEqual(folder2.ParentUUID, folder1.UUID, "Reference to parent's UUID not updated");
        }

        [Test]
        public void DataStructParsing()
        {
            ItemData item = new ItemData();
            item.AssetType = AssetType.Object;
            item.AssetUUID = UUID.Random();
            item.CreationDate = DateTime.Today;
            item.CreatorID = UUID.Random();
            item.Description = "Madonna";
            item.Flags = 0xF00;
            item.GroupID = UUID.Random();
            item.InventoryType = InventoryType.Object;
            item.Name = "Jimmy Jimmy";
            item.OwnerID = UUID.Random();
            item.ParentUUID = UUID.Random();
            item.Permissions = new Permissions(0xF00, 0xBA0, 0xCAF, 0xEBA, 0xBE);
            item.SalePrice = 402;
            item.SaleType = SaleType.Copy;
            item.UUID = UUID.Random();

            ItemData reconstituted;
            Assert.IsTrue(ItemData.TryParse(item.ToString(), out reconstituted), "ItemData.ToString output parsing failed.");
            Assert.IsTrue(item == reconstituted, "Reconstituted item is not identical to original.");

            FolderData folder = new FolderData();
            folder.Name = "What?";
            folder.OwnerID = UUID.Random();
            folder.ParentUUID = UUID.Random();
            folder.PreferredType = AssetType.LSLText;
            folder.UUID = UUID.Random();
            folder.Version = 24;
            // folder.DescendentCount = 2; // Not recorded in string form.

            FolderData reconFolder;
            Assert.IsTrue(FolderData.TryParse(folder.ToString(), out reconFolder), "FolderData.ToString output parsing failed.");
            Assert.IsTrue(folder == reconFolder, "Reconstituted folder is not identical to original.");
        }

        private ItemData ValidItem(UUID parent)
        {
            ItemData item = new ItemData(UUID.Random());
            item.ParentUUID = parent;
            item.OwnerID = OwnerUUID;
            return item;
        }

        private FolderData ValidFolder(UUID parent)
        {
            FolderData folder = new FolderData(UUID.Random());
            folder.ParentUUID = parent;
            folder.OwnerID = OwnerUUID;
            return folder;
        }

    }
}