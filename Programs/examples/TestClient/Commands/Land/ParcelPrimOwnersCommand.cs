using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using OpenMetaverse;

namespace OpenMetaverse.TestClient
{
    public class ParcelPrimOwnersCommand : Command
    {
        public ParcelPrimOwnersCommand(TestClient testClient)
        {
            Name = "primowners";
            Description = "Displays a list of prim owners and prim counts on a parcel. Usage: primowners parcelID";
            Category = CommandCategory.Parcel;
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            if (args.Length < 1)
                return "Usage: primowners parcelID (use parcelinfo to get ID)";

            int parcelID;
            Parcel parcel;
            StringBuilder result = new StringBuilder();
            // test argument that is is a valid integer, then verify we have that parcel data stored in the dictionary
            if (Int32.TryParse(args[0], out parcelID) && Client.Network.CurrentSim.Parcels.TryGetValue(parcelID, out parcel))
            {
                AutoResetEvent wait = new AutoResetEvent(false);

                EventHandler<ParcelObjectOwnersReplyEventArgs> callback = delegate(object sender, ParcelObjectOwnersReplyEventArgs e)
                {
                    for (int i = 0; i < e.PrimOwners.Count; i++)
                    {
                        result.AppendFormat("Owner: {0} Count: {1}" + System.Environment.NewLine, e.PrimOwners[i].OwnerID, e.PrimOwners[i].Count);
                        wait.Set();
                    }
                };


                Client.Parcels.ParcelObjectOwnersReply += callback;
                
                Client.Parcels.RequestObjectOwners(Client.Network.CurrentSim, parcelID);
                if (!wait.WaitOne(10000, false))
                {
                    result.AppendLine("Timed out waiting for packet.");
                }
                Client.Parcels.ParcelObjectOwnersReply -= callback;
                
                return result.ToString();
            }
            else
            {
                return String.Format("Unable to find Parcel {0} in Parcels Dictionary, Did you run parcelinfo to populate the dictionary first?", args[0]);
            }
        }        
    }
}
