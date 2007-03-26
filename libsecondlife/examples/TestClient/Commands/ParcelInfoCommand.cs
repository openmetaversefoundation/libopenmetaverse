using System;
using System.Collections.Generic;
using System.Threading;
using libsecondlife;
using libsecondlife.Utilities;

namespace libsecondlife.TestClient
{
    public class ParcelInfoCommand : Command
    {
        private ParcelDownloader Parcels;
        private ManualResetEvent ParcelsDownloaded = new ManualResetEvent(false);
        private int ParcelCount = 0;

        public ParcelInfoCommand(TestClient testClient)
		{
			Name = "parcelinfo";
			Description = "Prints out info about all the parcels in this simulator";

            Parcels = new ParcelDownloader(testClient);
            Parcels.OnParcelsDownloaded += new ParcelDownloader.ParcelsDownloadedCallback(Parcels_OnParcelsDownloaded);
		}

        public override string Execute(string[] args, LLUUID fromAgentID)
        {
            Parcels.DownloadSimParcels(Client.Network.CurrentSim);

            ParcelsDownloaded.Reset();
            ParcelsDownloaded.WaitOne(20000, false);

            return "Downloaded information for " + ParcelCount + " parcels in " + Client.Network.CurrentSim.Name;
        }

        void Parcels_OnParcelsDownloaded(Simulator simulator, Dictionary<int, Parcel> Parcels, int[,] map)
        {
            foreach (KeyValuePair<int, Parcel> parcel in Parcels)
            {
                Console.WriteLine("Parcels[{0}]: Name: \"{1}\", Description: \"{2}\"", parcel.Key, parcel.Value.Name,
                    parcel.Value.Desc);
            }

            ParcelCount = Parcels.Count;

            ParcelsDownloaded.Set();
        }
    }
}
