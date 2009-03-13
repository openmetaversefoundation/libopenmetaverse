using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using ExtensionLoader;
using OpenMetaverse;

namespace Simian
{
    public class GridLocal : IExtension<Simian>, IGridProvider
    {
        Simian server;
        DoubleDictionary<ulong, UUID, RegionInfo> grid = new DoubleDictionary<ulong, UUID, RegionInfo>();

        public GridLocal()
        {
        }

        public bool Start(Simian server)
        {
            this.server = server;
            return true;
        }

        public void Stop()
        {
        }

        public bool TryRegisterGridSpace(RegionInfo regionInfo, X509Certificate2 regionCert, out UUID regionID)
        {
            // No need to check the certificate since the requests are all local

            // Check the coordinates
            if (!grid.ContainsKey(regionInfo.Handle))
            {
                regionID = UUID.Random();
                grid.Add(regionInfo.Handle, regionID, regionInfo);
                return true;
            }
            else
            {
                regionID = UUID.Zero;
                return false;
            }
        }

        public bool TryRegisterAnyGridSpace(RegionInfo region, X509Certificate2 regionCert, bool isolated, out UUID regionID)
        {
            regionID = UUID.Zero;
            return false;
        }

        public bool UnregisterGridSpace(UUID regionID, X509Certificate2 regionCert)
        {
            return grid.Remove(regionID);
        }

        public void RegionUpdate(UUID regionID, X509Certificate2 regionCert)
        {
        }

        public void RegionHeartbeat(UUID regionID, X509Certificate2 regionCert)
        {
        }

        public bool TryGetRegion(UUID regionID, X509Certificate2 regionCert, out RegionInfo region)
        {
            return grid.TryGetValue(regionID, out region);
        }

        public bool TryGetRegion(uint regionX, uint regionY, X509Certificate2 regionCert, out RegionInfo region)
        {
            ulong handle = Utils.UIntsToLong(256 * regionX, 256 * regionY);
            return grid.TryGetValue(handle, out region);
        }

        public ISceneProvider GetDefaultLocalScene()
        {
            if (server.Scenes.Count > 0)
                return server.Scenes[0];
            else
                return null;
        }
    }
}
