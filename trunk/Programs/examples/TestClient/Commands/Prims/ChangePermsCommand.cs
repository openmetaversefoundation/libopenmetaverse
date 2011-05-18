using System;
using System.Collections.Generic;
using System.Threading;

namespace OpenMetaverse.TestClient
{
    public class ChangePermsCommand : Command
    {
        AutoResetEvent GotPermissionsEvent = new AutoResetEvent(false);
        Dictionary<UUID, Primitive> Objects = new Dictionary<UUID, Primitive>();
        PermissionMask Perms = PermissionMask.None;
        private bool PermsSent;
        private int PermCount;

        public ChangePermsCommand(TestClient testClient)
        {            
            testClient.Objects.ObjectProperties += new EventHandler<ObjectPropertiesEventArgs>(Objects_OnObjectProperties);

            Name = "changeperms";
            Description = "Recursively changes all of the permissions for child and task inventory objects. Usage prim-uuid [copy] [mod] [xfer]";
            Category = CommandCategory.Objects;
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            UUID rootID;
            Primitive rootPrim;
            List<Primitive> childPrims;
            List<uint> localIDs = new List<uint>();

            // Reset class-wide variables
            PermsSent = false;
            Objects.Clear();
            Perms = PermissionMask.None;
            PermCount = 0;

            if (args.Length < 1 || args.Length > 4)
                return "Usage prim-uuid [copy] [mod] [xfer]";

            if (!UUID.TryParse(args[0], out rootID))
                return "Usage prim-uuid [copy] [mod] [xfer]";

            for (int i = 1; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "copy":
                        Perms |= PermissionMask.Copy;
                        break;
                    case "mod":
                        Perms |= PermissionMask.Modify;
                        break;
                    case "xfer":
                        Perms |= PermissionMask.Transfer;
                        break;
                    default:
                        return "Usage prim-uuid [copy] [mod] [xfer]";
                }
            }

            Logger.DebugLog("Using PermissionMask: " + Perms.ToString(), Client);

            // Find the requested prim
            rootPrim = Client.Network.CurrentSim.ObjectsPrimitives.Find(delegate(Primitive prim) { return prim.ID == rootID; });
            if (rootPrim == null)
                return "Cannot find requested prim " + rootID.ToString();
            else
                Logger.DebugLog("Found requested prim " + rootPrim.ID.ToString(), Client);

            if (rootPrim.ParentID != 0)
            {
                // This is not actually a root prim, find the root
                if (!Client.Network.CurrentSim.ObjectsPrimitives.TryGetValue(rootPrim.ParentID, out rootPrim))
                    return "Cannot find root prim for requested object";
                else
                    Logger.DebugLog("Set root prim to " + rootPrim.ID.ToString(), Client);
            }

            // Find all of the child objects linked to this root
            childPrims = Client.Network.CurrentSim.ObjectsPrimitives.FindAll(delegate(Primitive prim) { return prim.ParentID == rootPrim.LocalID; });

            // Build a dictionary of primitives for referencing later
            Objects[rootPrim.ID] = rootPrim;
            for (int i = 0; i < childPrims.Count; i++)
                Objects[childPrims[i].ID] = childPrims[i];

            // Build a list of all the localIDs to set permissions for
            localIDs.Add(rootPrim.LocalID);
            for (int i = 0; i < childPrims.Count; i++)
                localIDs.Add(childPrims[i].LocalID);

            // Go through each of the three main permissions and enable or disable them
            #region Set Linkset Permissions

            PermCount = 0;
            if ((Perms & PermissionMask.Modify) == PermissionMask.Modify)
                Client.Objects.SetPermissions(Client.Network.CurrentSim, localIDs, PermissionWho.NextOwner, PermissionMask.Modify, true);
            else
                Client.Objects.SetPermissions(Client.Network.CurrentSim, localIDs, PermissionWho.NextOwner, PermissionMask.Modify, false);
            PermsSent = true;

            if (!GotPermissionsEvent.WaitOne(1000 * 30, false))
                return "Failed to set the modify bit, permissions in an unknown state";

            PermCount = 0;
            if ((Perms & PermissionMask.Copy) == PermissionMask.Copy)
                Client.Objects.SetPermissions(Client.Network.CurrentSim, localIDs, PermissionWho.NextOwner, PermissionMask.Copy, true);
            else
                Client.Objects.SetPermissions(Client.Network.CurrentSim, localIDs, PermissionWho.NextOwner, PermissionMask.Copy, false);
            PermsSent = true;

            if (!GotPermissionsEvent.WaitOne(1000 * 30, false))
                return "Failed to set the copy bit, permissions in an unknown state";

            PermCount = 0;
            if ((Perms & PermissionMask.Transfer) == PermissionMask.Transfer)
                Client.Objects.SetPermissions(Client.Network.CurrentSim, localIDs, PermissionWho.NextOwner, PermissionMask.Transfer, true);
            else
                Client.Objects.SetPermissions(Client.Network.CurrentSim, localIDs, PermissionWho.NextOwner, PermissionMask.Transfer, false);
            PermsSent = true;

            if (!GotPermissionsEvent.WaitOne(1000 * 30, false))
                return "Failed to set the transfer bit, permissions in an unknown state";

            #endregion Set Linkset Permissions

            // Check each prim for task inventory and set permissions on the task inventory
            int taskItems = 0;
            foreach (Primitive prim in Objects.Values)
            {
                if ((prim.Flags & PrimFlags.InventoryEmpty) == 0)
                {
                    List<InventoryBase> items = Client.Inventory.GetTaskInventory(prim.ID, prim.LocalID, 1000 * 30);

                    if (items != null)
                    {
                        for (int i = 0; i < items.Count; i++)
                        {
                            if (!(items[i] is InventoryFolder))
                            {
                                InventoryItem item = (InventoryItem)items[i];
                                item.Permissions.NextOwnerMask = Perms;

                                Client.Inventory.UpdateTaskInventory(prim.LocalID, item);
                                ++taskItems;
                            }
                        }
                    }
                }
            }

            return "Set permissions to " + Perms.ToString() + " on " + localIDs.Count + " objects and " + taskItems + " inventory items";
        }

        void Objects_OnObjectProperties(object sender, ObjectPropertiesEventArgs e)
        {
            if (PermsSent)
            {
                if (Objects.ContainsKey(e.Properties.ObjectID))
                {
                    // FIXME: Confirm the current operation against properties.Permissions.NextOwnerMask

                    ++PermCount;
                    if (PermCount >= Objects.Count)
                        GotPermissionsEvent.Set();
                }
            }
        }
    }
}
