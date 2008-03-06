using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using libsecondlife;
using libsecondlife.Utilities;

namespace libsecondlife.TestClient
{
    public class ParcelInfoCommand : Command
    {
        private AutoResetEvent ParcelsDownloaded = new AutoResetEvent(false);
        private int ParcelCount = 0;

        public ParcelInfoCommand(TestClient testClient)
        {
            Name = "parcelinfo";
            Description = "Prints out info about all the parcels in this simulator";

            testClient.Network.OnDisconnected += new NetworkManager.DisconnectedCallback(Network_OnDisconnected);
        }



        public override string Execute(string[] args, LLUUID fromAgentID)
        {
            
            StringBuilder sb = new StringBuilder();
            string result;

            ParcelManager.SimParcelsDownloaded del = delegate(Simulator simulator, InternalDictionary<int, Parcel> simParcels, int[,] parcelMap)
            {
                ParcelCount = simParcels.Count;

                simParcels.ForEach(delegate(Parcel parcel)
                {
                    sb.AppendFormat("Parcels[{0}]: Name: \"{1}\", Description: \"{2}\" ACL Count: {3}" + System.Environment.NewLine,
                        parcel.LocalID, parcel.Name, parcel.Desc, parcel.AccessList.Count);
                });
                ParcelsDownloaded.Set();

            };

            ParcelsDownloaded.Reset();
            Client.Parcels.OnSimParcelsDownloaded += del;
            Client.Parcels.RequestAllSimParcels(Client.Network.CurrentSim);

            if (ParcelsDownloaded.WaitOne(20000, false) && Client.Network.Connected)
                result = sb.ToString();
            else
                result =  "Failed to retrieve information on all the simulator parcels";

            Client.Parcels.OnSimParcelsDownloaded -= del;
            return result;
        }

        void Network_OnDisconnected(NetworkManager.DisconnectType reason, string message)
        {
            ParcelsDownloaded.Set();
        }
    }
}
