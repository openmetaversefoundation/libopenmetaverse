using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using OpenMetaverse;

namespace OpenMetaverse.TestClient
{
    public class ParcelInfoCommand : Command
    {
        private AutoResetEvent ParcelsDownloaded = new AutoResetEvent(false);

        public ParcelInfoCommand(TestClient testClient)
        {
            Name = "parcelinfo";
            Description = "Prints out info about all the parcels in this simulator";
            Category = CommandCategory.Parcel;

            testClient.Network.Disconnected += Network_OnDisconnected;
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            StringBuilder sb = new StringBuilder();
            string result;
            EventHandler<SimParcelsDownloadedEventArgs> del = delegate(object sender, SimParcelsDownloadedEventArgs e)
            {
                ParcelsDownloaded.Set();
            };
            

            ParcelsDownloaded.Reset();
            Client.Parcels.SimParcelsDownloaded += del;
            Client.Parcels.RequestAllSimParcels(Client.Network.CurrentSim);

            if (Client.Network.CurrentSim.IsParcelMapFull())
                ParcelsDownloaded.Set();

            if (ParcelsDownloaded.WaitOne(30000, false) && Client.Network.Connected)
            {
                sb.AppendFormat("Downloaded {0} Parcels in {1} " + System.Environment.NewLine, 
                    Client.Network.CurrentSim.Parcels.Count, Client.Network.CurrentSim.Name);

                Client.Network.CurrentSim.Parcels.ForEach(delegate(Parcel parcel)
                {
                    sb.AppendFormat("Parcel[{0}]: Name: \"{1}\", Description: \"{2}\" ACLBlacklist Count: {3}, ACLWhiteList Count: {5} Traffic: {4}" + System.Environment.NewLine,
                        parcel.LocalID, parcel.Name, parcel.Desc, parcel.AccessBlackList.Count, parcel.Dwell, parcel.AccessWhiteList.Count);
                });

                result = sb.ToString();
            }
            else
                result = "Failed to retrieve information on all the simulator parcels";

            Client.Parcels.SimParcelsDownloaded -= del;
            return result;
        }

        void Network_OnDisconnected(object sender, DisconnectedEventArgs e)
        {
            ParcelsDownloaded.Set();
        }
    }
}
