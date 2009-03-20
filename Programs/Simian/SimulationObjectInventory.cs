using System;
using System.Text;
using System.Collections.Generic;
using OpenMetaverse;

namespace Simian
{
    public class TaskInventoryStringBuilder
    {
        StringBuilder builder = new StringBuilder();

        public TaskInventoryStringBuilder(UUID folderID, UUID parentID)
        {
            builder.Append("\tinv_object\t0\n\t{\n");
            AddNameValueLine("obj_id", folderID.ToString());
            AddNameValueLine("parent_id", parentID.ToString());
            AddNameValueLine("type", "category");
            AddNameValueLine("name", "Contents|");
            AddSectionEnd();
        }

        public void AddItemStart()
        {
            builder.Append("\tinv_item\t0\n");
            AddSectionStart();
        }

        public void AddPermissionsStart()
        {
            builder.Append("\tpermissions 0\n");
            AddSectionStart();
        }

        public void AddSaleStart()
        {
            builder.Append("\tsale_info\t0\n");
            AddSectionStart();
        }

        protected void AddSectionStart()
        {
            builder.Append("\t{\n");
        }

        public void AddSectionEnd()
        {
            builder.Append("\t}\n");
        }

        public void AddLine(string addLine)
        {
            builder.Append(addLine);
        }

        public void AddNameValueLine(string name, string value)
        {
            builder.Append("\t\t");
            builder.Append(name);
            builder.Append("\t");
            builder.Append(value);
            builder.Append("\n");
        }

        public override string ToString()
        {
            return builder.ToString();
        }
    }

    public class SimulationObjectInventory
    {
        SimulationObject hostObject;
        string inventoryFilename;
        short inventoryFilenameSerial;
        DoubleDictionary<UUID, string, InventoryTaskItem> items;

        public short InventorySerial
        {
            get { return hostObject.Prim.Properties.InventorySerial; }
            set { hostObject.Prim.Properties.InventorySerial = value; }
        }

        public SimulationObjectInventory(SimulationObject hostObject)
        {
            this.hostObject = hostObject;
        }

        public void ChangeInventoryOwner(UUID newOwnerID)
        {
            LazyInitialize();
            throw new NotImplementedException();
        }

        public void ChangeInventoryGroup(UUID newGroupID)
        {
            LazyInitialize();
            throw new NotImplementedException();
        }

        public void StartScripts(int startParam, bool triggerOnRezEvent)
        {
            throw new NotImplementedException();
        }

        public void StopScripts()
        {
            throw new NotImplementedException();
        }

        public void StartScript(InventoryTaskItem script, int startParam, bool triggerOnRezEvent)
        {
            LazyInitialize();
            throw new NotImplementedException();
        }

        public void StopScript(UUID itemID)
        {
            LazyInitialize();
            throw new NotImplementedException();
        }

        public void AddOrUpdateItem(InventoryTaskItem item, bool replace, bool allowedDrop)
        {
            LazyInitialize();

            item.ParentObjectID = hostObject.Prim.ID;

            if (replace)
            {
                InventoryTaskItem oldItem;
                if (items.TryGetValue(item.Name, out oldItem))
                {
                    item.ID = oldItem.ID;
                    items.Remove(item.ID, item.Name);
                }
            }
            else
            {
                item.Name = NextAvailableFilename(item.Name);
            }

            if (item.ID == UUID.Zero)
                item.ID = UUID.Random();

            items.Add(item.ID, item.Name, item);

            UpdateTaskInventoryAsset();

            // Post a script event
            Changed change = allowedDrop ? Changed.ALLOWED_DROP : Changed.INVENTORY;
            hostObject.Scene.ScriptEngine.PostObjectEvent(hostObject.Prim.ID, new EventParams(
                "changed", new object[] { new ScriptTypes.LSL_Integer((uint)change) }, new DetectParams[0]));
        }

        public InventoryType RemoveItem(UUID itemID)
        {
            LazyInitialize();
            InventoryTaskItem item;
            if (items.TryGetValue(itemID, out item))
            {
                items.Remove(itemID, item.Name);

                UpdateTaskInventoryAsset();

                // Post a script event
                hostObject.Scene.ScriptEngine.PostObjectEvent(hostObject.Prim.ID, new EventParams(
                    "changed", new object[] { new ScriptTypes.LSL_Integer((uint)Changed.INVENTORY) }, new DetectParams[0]));

                // FIXME: Check if this prim still classifies as "scripted"

                return item.InventoryType;
            }
            else
            {
                return InventoryType.Unknown;
            }
        }

        public bool TryGetItem(UUID itemID, out InventoryTaskItem item)
        {
            LazyInitialize();
            return items.TryGetValue(itemID, out item);
        }

        public bool TryGetItem(string name, out InventoryTaskItem item)
        {
            LazyInitialize();
            return items.TryGetValue(name, out item);
        }

        public string GetInventoryFilename()
        {
            if (InventorySerial > 0)
            {
                if (String.IsNullOrEmpty(inventoryFilename) || inventoryFilenameSerial < InventorySerial)
                    inventoryFilename = "inventory_" + UUID.Random() + ".tmp";

                inventoryFilenameSerial = InventorySerial;

                return inventoryFilename;
            }
            else
            {
                return String.Empty;
            }
        }

        public void ForEachItem(Action<InventoryTaskItem> action)
        {
            LazyInitialize();
            items.ForEach(action);
        }

        public InventoryTaskItem FindItem(Predicate<InventoryTaskItem> match)
        {
            LazyInitialize();
            return items.FindValue(match);
        }

        public IList<InventoryTaskItem> FindAllItems(Predicate<InventoryTaskItem> match)
        {
            LazyInitialize();
            return items.FindAll(match);
        }

        void UpdateTaskInventoryAsset()
        {
            // Remove the previous task inventory asset
            string filename = GetInventoryFilename();
            if (!String.IsNullOrEmpty(filename))
                hostObject.Scene.TaskInventory.RemoveTaskFile(filename);

            // Update the inventory serial number
            ++InventorySerial;

            // Create the new asset
            filename = GetInventoryFilename();
            byte[] assetData = GetTaskInventoryAsset();
            hostObject.Scene.TaskInventory.AddTaskFile(filename, assetData);
        }

        byte[] GetTaskInventoryAsset()
        {
            TaskInventoryStringBuilder invString = new TaskInventoryStringBuilder(hostObject.Prim.ID, UUID.Zero);
            
            items.ForEach(
                delegate(InventoryTaskItem item)
                {
                    invString.AddItemStart();
                    invString.AddNameValueLine("item_id", item.ID.ToString());
                    invString.AddNameValueLine("parent_id", hostObject.Prim.ID.ToString());

                    invString.AddPermissionsStart();

                    invString.AddNameValueLine("base_mask", Utils.UIntToHexString((uint)item.Permissions.BaseMask));
                    invString.AddNameValueLine("owner_mask", Utils.UIntToHexString((uint)item.Permissions.OwnerMask));
                    invString.AddNameValueLine("group_mask", Utils.UIntToHexString((uint)item.Permissions.GroupMask));
                    invString.AddNameValueLine("everyone_mask", Utils.UIntToHexString((uint)item.Permissions.EveryoneMask));
                    invString.AddNameValueLine("next_owner_mask", Utils.UIntToHexString((uint)item.Permissions.NextOwnerMask));

                    invString.AddNameValueLine("creator_id", item.CreatorID.ToString());
                    invString.AddNameValueLine("owner_id", item.OwnerID.ToString());

                    invString.AddNameValueLine("last_owner_id", item.CreatorID.ToString()); // FIXME: Do we need InventoryItem.LastOwnerID?

                    invString.AddNameValueLine("group_id", item.GroupID.ToString());
                    invString.AddSectionEnd();

                    invString.AddNameValueLine("asset_id", item.AssetID.ToString());
                    invString.AddNameValueLine("type", OpenMetaverse.InventoryManager.AssetTypeToString(item.AssetType));
                    invString.AddNameValueLine("inv_type", OpenMetaverse.InventoryManager.InventoryTypeToString(item.InventoryType));
                    invString.AddNameValueLine("flags", Utils.UIntToHexString(item.Flags));

                    invString.AddSaleStart();
                    invString.AddNameValueLine("sale_type", OpenMetaverse.InventoryManager.SaleTypeToString(item.SaleType));
                    invString.AddNameValueLine("sale_price", item.SalePrice.ToString());
                    invString.AddSectionEnd();

                    invString.AddNameValueLine("name", item.Name + "|");
                    invString.AddNameValueLine("desc", item.Description + "|");

                    invString.AddNameValueLine("creation_date", Utils.DateTimeToUnixTime(item.CreationDate).ToString());
                    invString.AddSectionEnd();
                }
            );

            return Utils.StringToBytes(invString.ToString());
        }

        string NextAvailableFilename(string name)
        {
            string tryName = name;
            int suffix = 1;

            while (items.ContainsKey(tryName) && suffix < 256)
                tryName = String.Format("{0} {1}", name, suffix++);

            return tryName;
        }

        void LazyInitialize()
        {
            if (items == null)
                items = new DoubleDictionary<UUID, string, InventoryTaskItem>();
        }
    }
}
