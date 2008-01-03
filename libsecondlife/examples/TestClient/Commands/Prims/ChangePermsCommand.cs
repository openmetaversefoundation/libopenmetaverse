using System;
using System.Collections.Generic;
using System.Threading;

namespace libsecondlife.TestClient
{
    public class ChangePermsCommand : Command
    {
        AutoResetEvent GotPermissionsEvent = new AutoResetEvent(false);
        LLUUID SelectedObject = LLUUID.Zero;
        Dictionary<LLUUID, Primitive> Objects = new Dictionary<LLUUID, Primitive>();
        PermissionMask Perms = PermissionMask.None;
        bool PermsSent = false;
        int PermCount = 0;

        public ChangePermsCommand(TestClient testClient)
        {
            testClient.Objects.OnObjectProperties += new ObjectManager.ObjectPropertiesCallback(Objects_OnObjectProperties);

            Name = "changeperms";
            Description = "Recursively changes all of the permissions for child and task inventory objects. Usage prim-uuid [copy] [mod] [xfer]";
        }

        public override string Execute(string[] args, LLUUID fromAgentID)
        {
            LLUUID rootID;
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

            if (!LLUUID.TryParse(args[0], out rootID))
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

            Client.DebugLog("Using PermissionMask: " + Perms.ToString());

            // Find the requested prim
            rootPrim = Client.Network.CurrentSim.ObjectsPrimitives.Find(delegate(Primitive prim) { return prim.ID == rootID; });
            if (rootPrim == null)
                return "Cannot find requested prim " + rootID.ToString();
            else
                Client.DebugLog("Found requested prim " + rootPrim.ID.ToString());

            if (rootPrim.ParentID != 0)
            {
                // This is not actually a root prim, find the root
                if (!Client.Network.CurrentSim.ObjectsPrimitives.TryGetValue(rootPrim.ParentID, out rootPrim))
                    return "Cannot find root prim for requested object";
                else
                    Client.DebugLog("Set root prim to " + rootPrim.ID.ToString());
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
                if ((prim.Flags & LLObject.ObjectFlags.InventoryEmpty) == 0)
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

        void Objects_OnObjectProperties(Simulator simulator, LLObject.ObjectProperties properties)
        {
            if (PermsSent)
            {
                if (Objects.ContainsKey(properties.ObjectID))
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
