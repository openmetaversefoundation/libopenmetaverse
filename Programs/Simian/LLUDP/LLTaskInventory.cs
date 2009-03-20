using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace Simian
{
    public class LLTaskInventory : IExtension<ISceneProvider>
    {
        static readonly UUID DEFAULT_SCRIPT = new UUID("a7f70b8e-b2ee-46bb-85c0-5d973137cd47");

        ISceneProvider scene;

        public LLTaskInventory()
        {
        }

        public bool Start(ISceneProvider scene)
        {
            this.scene = scene;

            scene.UDP.RegisterPacketCallback(PacketType.RequestTaskInventory, RequestTaskInventoryHandler);
            scene.UDP.RegisterPacketCallback(PacketType.UpdateTaskInventory, UpdateTaskInventoryHandler);
            scene.UDP.RegisterPacketCallback(PacketType.RezScript, RezScriptHandler);
            scene.UDP.RegisterPacketCallback(PacketType.RemoveTaskInventory, RemoveTaskInventoryHandler);
            scene.UDP.RegisterPacketCallback(PacketType.MoveTaskInventory, MoveTaskInventoryHandler);
            return true;
        }

        public void Stop()
        {
        }

        void RequestTaskInventoryHandler(Packet packet, Agent agent)
        {
            RequestTaskInventoryPacket request = (RequestTaskInventoryPacket)packet;

            // Try to find this object in the scene
            SimulationObject obj;
            if (scene.TryGetObject(request.InventoryData.LocalID, out obj))
            {
                ReplyTaskInventoryPacket reply = new ReplyTaskInventoryPacket();
                reply.InventoryData.Filename = Utils.StringToBytes(obj.Inventory.GetInventoryFilename());
                reply.InventoryData.Serial = obj.Inventory.InventorySerial;
                reply.InventoryData.TaskID = obj.Prim.ID;

                scene.UDP.SendPacket(agent.ID, reply, PacketCategory.Transaction);
            }
        }

        void UpdateTaskInventoryHandler(Packet packet, Agent agent)
        {
            UpdateTaskInventoryPacket update = (UpdateTaskInventoryPacket)packet;

            InventoryTaskItem item;
            SimulationObject targetObj;
            InventoryObject invObj;

            if (update.UpdateData.Key != 0)
            {
                Logger.Log("Got an UpdateTaskInventory packet with a Key of " + update.UpdateData.Key,
                    Helpers.LogLevel.Warning);
                return;
            }

            if (scene.TryGetObject(update.UpdateData.LocalID, out targetObj))
            {
                if (update.InventoryData.ItemID != UUID.Zero)
                {
                    if (targetObj.Inventory.TryGetItem(update.InventoryData.ItemID, out item))
                    {
                        // Updating an existing item in the task inventory
                        Logger.Log("[TODO] Implement updating task inventory items", Helpers.LogLevel.Error);
                    }
                    else
                    {
                        Logger.Log("UpdateTaskInventory for unknown inventory item " + update.InventoryData.ItemID, Helpers.LogLevel.Warning);
                    }
                }
                else if (scene.Server.Inventory.TryGetInventory(agent.ID, update.InventoryData.ItemID, out invObj))
                {
                    // Create a new item in the task inventory
                    if (invObj is InventoryItem)
                    {
                        InventoryItem fromItem = (InventoryItem)invObj;

                        item = new InventoryTaskItem();
                        //item.ID will be assigned in AddOrUpdateItem
                        item.AssetID = fromItem.AssetID;
                        item.AssetType = fromItem.AssetType;
                        item.CreationDate = fromItem.CreationDate;
                        item.CreatorID = fromItem.CreatorID;
                        item.Description = fromItem.Description;
                        item.Flags = fromItem.Flags;
                        item.GrantedPermissions = 0;
                        item.GroupID = fromItem.GroupID;
                        item.GroupOwned = fromItem.GroupOwned;
                        item.InventoryType = fromItem.InventoryType;
                        item.Name = fromItem.Name;
                        item.OwnerID = agent.ID;
                        item.ParentID = update.InventoryData.FolderID;
                        item.Parent = null; // TODO: Try to find a parent folder in task inventory?
                        item.ParentObjectID = targetObj.Prim.ID;
                        item.PermissionGranter = UUID.Zero;
                        item.Permissions = fromItem.Permissions.GetNextPermissions();
                        item.SalePrice = fromItem.SalePrice;
                        item.SaleType = fromItem.SaleType;

                        bool allowDrop = (targetObj.Prim.Flags & PrimFlags.AllowInventoryDrop) != 0;

                        targetObj.Inventory.AddOrUpdateItem(item, false, allowDrop);
                        Logger.Log("Created new task inventory item: " + item.Name, Helpers.LogLevel.Info);

                        // Send an ObjectPropertiesReply to inform the client that inventory has changed
                        ObjectPropertiesPacket props = new ObjectPropertiesPacket();
                        props.ObjectData = new ObjectPropertiesPacket.ObjectDataBlock[1];
                        props.ObjectData[0] = SimulationObject.BuildPropertiesBlock(targetObj.Prim);
                        scene.UDP.SendPacket(agent.ID, props, PacketCategory.Transaction);
                    }
                    else
                    {
                        Logger.Log("[TODO] Handle dropping folders in task inventory", Helpers.LogLevel.Warning);
                    }
                }
                else
                {
                    Logger.Log("Got an UpdateTaskInventory packet referencing an unknown inventory item", Helpers.LogLevel.Warning);
                }
            }
            else
            {
                Logger.Log("Got an UpdateTaskInventory packet referencing an unknown object", Helpers.LogLevel.Warning);
            }
        }

        void RezScriptHandler(Packet packet, Agent agent)
        {
            RezScriptPacket rez = (RezScriptPacket)packet;

            InventoryTaskItem scriptItem;
            SimulationObject targetObj;
            InventoryObject invObj;

            if (scene.TryGetObject(rez.UpdateBlock.ObjectLocalID, out targetObj))
            {
                if (rez.InventoryBlock.ItemID != UUID.Zero)
                {
                    if (scene.Server.Inventory.TryGetInventory(agent.ID, rez.InventoryBlock.ItemID, out invObj))
                    {
                        // Rezzing a script from agent inventory
                        Asset defaultScript;
                        if (scene.Server.Assets.TryGetAsset(DEFAULT_SCRIPT, out defaultScript))
                        {
                            Logger.Log("[TODO] RezScript from agent inventory", Helpers.LogLevel.Error);
                        }
                    }
                    else
                    {
                        Logger.Log("RezScript for unknown inventory item " + rez.InventoryBlock.ItemID, Helpers.LogLevel.Warning);
                    }
                }
                else
                {
                    // Rezzing a new script
                    scriptItem = new InventoryTaskItem();
                    scriptItem.AssetID = DEFAULT_SCRIPT;
                    scriptItem.AssetType = AssetType.LSLText;
                    scriptItem.CreationDate = DateTime.Now;
                    scriptItem.CreatorID = agent.ID;
                    scriptItem.Description = String.Empty;
                    scriptItem.Flags = 0;
                    scriptItem.GrantedPermissions = 0;
                    scriptItem.GroupID = UUID.Zero;
                    scriptItem.GroupOwned = false;
                    scriptItem.ID = UUID.Random();
                    scriptItem.InventoryType = InventoryType.LSL;
                    scriptItem.Name = "New script";
                    scriptItem.OwnerID = agent.ID;
                    scriptItem.ParentID = rez.InventoryBlock.FolderID;
                    scriptItem.Parent = null; // TODO: Try to find a parent folder in task inventory?
                    scriptItem.ParentObjectID = targetObj.Prim.ID;
                    scriptItem.PermissionGranter = UUID.Zero;
                    scriptItem.Permissions = scene.Server.Permissions.GetDefaultPermissions();
                    scriptItem.SalePrice = 10;
                    scriptItem.SaleType = SaleType.Not;

                    targetObj.Inventory.AddOrUpdateItem(scriptItem, false, false);
                    Logger.Log("Created new task inventory script: " + scriptItem.Name, Helpers.LogLevel.Info);

                    // Send an ObjectPropertiesReply to inform the client that inventory has changed
                    ObjectPropertiesPacket props = new ObjectPropertiesPacket();
                    props.ObjectData = new ObjectPropertiesPacket.ObjectDataBlock[1];
                    props.ObjectData[0] = SimulationObject.BuildPropertiesBlock(targetObj.Prim);
                    scene.UDP.SendPacket(agent.ID, props, PacketCategory.Transaction);

                    // Mark this object as scripted
                    targetObj.Prim.Flags |= PrimFlags.Scripted;
                    scene.ObjectAddOrUpdate(this, targetObj, targetObj.Prim.OwnerID, PrimFlags.None, UpdateFlags.PrimFlags);

                    // Run the script
                    scene.ScriptEngine.RezScript(scriptItem.ID, scriptItem.AssetID, targetObj, 0);
                }
            }
            else
            {
                Logger.Log("Got a RezScript packet referencing an unknown object", Helpers.LogLevel.Warning);
            }
        }

        void RemoveTaskInventoryHandler(Packet packet, Agent agent)
        {
        }

        void MoveTaskInventoryHandler(Packet packet, Agent agent)
        {
        }
    }
}
