using System;
using System.Collections.Generic;
using System.Threading;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace Simian
{
    public class LLTaskInventory : IExtension<ISceneProvider>
    {
        ISceneProvider scene;

        public LLTaskInventory()
        {
        }

        public bool Start(ISceneProvider scene)
        {
            this.scene = scene;

            scene.UDP.RegisterPacketCallback(PacketType.RequestTaskInventory, RequestTaskInventoryHandler);
            scene.UDP.RegisterPacketCallback(PacketType.UpdateTaskInventory, UpdateTaskInventoryHandler);
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
            
            if (update.UpdateData.Key == 0)
            {
                if (scene.TryGetObject(update.UpdateData.LocalID, out targetObj))
                {
                    if (targetObj.Inventory.TryGetItem(update.InventoryData.ItemID, out item))
                    {
                        // Updating an existing item in the task inventory
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
                            props.ObjectData[0] = new ObjectPropertiesPacket.ObjectDataBlock();
                            props.ObjectData[0].AggregatePerms = targetObj.Prim.Properties.AggregatePerms;
                            props.ObjectData[0].AggregatePermTextures = targetObj.Prim.Properties.AggregatePermTextures;
                            props.ObjectData[0].AggregatePermTexturesOwner = targetObj.Prim.Properties.AggregatePermTexturesOwner;
                            props.ObjectData[0].BaseMask = (uint)targetObj.Prim.Properties.Permissions.BaseMask;
                            props.ObjectData[0].Category = (uint)targetObj.Prim.Properties.Category;
                            props.ObjectData[0].CreationDate = Utils.DateTimeToUnixTime(targetObj.Prim.Properties.CreationDate);
                            props.ObjectData[0].CreatorID = targetObj.Prim.Properties.CreatorID;
                            props.ObjectData[0].Description = Utils.StringToBytes(targetObj.Prim.Properties.Description);
                            props.ObjectData[0].EveryoneMask = (uint)targetObj.Prim.Properties.Permissions.EveryoneMask;
                            props.ObjectData[0].FolderID = targetObj.Prim.Properties.FolderID;
                            props.ObjectData[0].FromTaskID = targetObj.Prim.Properties.FromTaskID;
                            props.ObjectData[0].GroupID = targetObj.Prim.Properties.GroupID;
                            props.ObjectData[0].GroupMask = (uint)targetObj.Prim.Properties.Permissions.GroupMask;
                            props.ObjectData[0].InventorySerial = targetObj.Prim.Properties.InventorySerial;
                            props.ObjectData[0].ItemID = targetObj.Prim.Properties.ItemID;
                            props.ObjectData[0].LastOwnerID = targetObj.Prim.Properties.LastOwnerID;
                            props.ObjectData[0].Name = Utils.StringToBytes(targetObj.Prim.Properties.Name);
                            props.ObjectData[0].NextOwnerMask = (uint)targetObj.Prim.Properties.Permissions.NextOwnerMask;
                            props.ObjectData[0].ObjectID = targetObj.Prim.ID;
                            props.ObjectData[0].OwnerID = targetObj.Prim.Properties.OwnerID;
                            props.ObjectData[0].OwnerMask = (uint)targetObj.Prim.Properties.Permissions.OwnerMask;
                            props.ObjectData[0].OwnershipCost = targetObj.Prim.Properties.OwnershipCost;
                            props.ObjectData[0].SalePrice = targetObj.Prim.Properties.SalePrice;
                            props.ObjectData[0].SaleType = (byte)targetObj.Prim.Properties.SaleType;
                            props.ObjectData[0].SitName = Utils.StringToBytes(targetObj.Prim.Properties.SitName);
                            props.ObjectData[0].TextureID = targetObj.Prim.Properties.GetTextureIDBytes();
                            props.ObjectData[0].TouchName = Utils.StringToBytes(targetObj.Prim.Properties.TouchName);

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
            else
            {
                Logger.Log("Got an UpdateTaskInventory packet with a Key of " + update.UpdateData.Key, Helpers.LogLevel.Warning);
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
