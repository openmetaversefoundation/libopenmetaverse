using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using OpenMetaverse;

namespace OpenMetaverse.TestClient
{
    public class ParcelSelectObjectsCommand : Command
    {
        public ParcelSelectObjectsCommand(TestClient testClient)
        {
            Name = "selectobjects";
            Description = "Displays a list of prim localIDs on a given parcel with a specific owner. Usage: selectobjects parcelID OwnerUUID";
            Category = CommandCategory.Parcel;
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            if (args.Length < 2)
                return "Usage: selectobjects parcelID OwnerUUID (use parcelinfo to get ID, use parcelprimowners to get ownerUUID)";

            int parcelID;
            UUID ownerUUID;

            int counter = 0;
            StringBuilder result = new StringBuilder();
            // test argument that is is a valid integer, then verify we have that parcel data stored in the dictionary
            if (Int32.TryParse(args[0], out parcelID) 
                && UUID.TryParse(args[1], out ownerUUID))
            {
                AutoResetEvent wait = new AutoResetEvent(false);
                EventHandler<ForceSelectObjectsReplyEventArgs> callback = delegate(object sender, ForceSelectObjectsReplyEventArgs e)
                {
                    
                    for (int i = 0; i < e.ObjectIDs.Count; i++)
                    {
                        result.Append(e.ObjectIDs[i].ToString() + " ");
                        counter++;
                    }
                    
                    if (e.ObjectIDs.Count < 251)
                        wait.Set();
                };
                

                Client.Parcels.ForceSelectObjectsReply += callback;
                Client.Parcels.RequestSelectObjects(parcelID, (ObjectReturnType)16, ownerUUID);
                

                Client.Parcels.RequestObjectOwners(Client.Network.CurrentSim, parcelID);
                if (!wait.WaitOne(30000, false))
                {
                    result.AppendLine("Timed out waiting for packet.");
                }
                
                Client.Parcels.ForceSelectObjectsReply -= callback;
                result.AppendLine("Found a total of " + counter + " Objects");
                return result.ToString();
            }
            else
            {
                return String.Format("Unable to find Parcel {0} in Parcels Dictionary, Did you run parcelinfo to populate the dictionary first?", args[0]);
            }
        }
    }
}
