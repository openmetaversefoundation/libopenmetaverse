using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using ExtensionLoader;
using OpenMetaverse;

namespace Simian
{
    public class GridLocal : IExtension<Simian>, IGridProvider
    {
        public event RegionUpdateCallback OnRegionUpdate;

        Simian server;
        DoubleDictionary<ulong, UUID, RegionInfo> grid = new DoubleDictionary<ulong, UUID, RegionInfo>();
        object syncRoot = new object();

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

            lock (syncRoot)
            {
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
        }

        public bool TryRegisterAnyGridSpace(RegionInfo regionInfo, X509Certificate2 regionCert, bool isolated, out UUID regionID)
        {
            regionID = UUID.Zero;
            regionInfo.Online = false;
            return false;
        }

        public bool UnregisterGridSpace(UUID regionID, X509Certificate2 regionCert)
        {
            lock (syncRoot)
            {
                RegionInfo regionInfo;
                if (grid.TryGetValue(regionID, out regionInfo))
                {
                    regionInfo.Online = false;
                    grid.Remove(regionID);

                    if (OnRegionUpdate != null)
                    {
                        OnRegionUpdate(regionInfo);
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void RegionUpdate(RegionInfo regionInfo, X509Certificate2 regionCert)
        {
            lock (syncRoot)
            {
                RegionInfo oldRegionInfo;
                if (grid.TryGetValue(regionInfo.ID, out oldRegionInfo))
                {
                    // TODO: Handle requests to move the region
                    //oldRegionInfo.Handle

                    oldRegionInfo.HttpServer = regionInfo.HttpServer;
                    oldRegionInfo.IPAndPort = regionInfo.IPAndPort;
                    oldRegionInfo.MapTextureID = regionInfo.MapTextureID;
                    oldRegionInfo.Name = regionInfo.Name;
                    oldRegionInfo.Owner = regionInfo.Owner;
                    oldRegionInfo.Online = regionInfo.Online;
                    oldRegionInfo.EnableClientCap = regionInfo.EnableClientCap;

                    if (OnRegionUpdate != null)
                    {
                        OnRegionUpdate(oldRegionInfo);
                    }
                }
            }
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

        public IList<RegionInfo> GetRegionsInArea(int minX, int minY, int maxX, int maxY)
        {
            lock (syncRoot)
            {
                IList<RegionInfo> regions = grid.FindAll(
                    delegate(RegionInfo region)
                    {
                        uint x, y;
                        Utils.LongToUInts(region.Handle, out x, out y);
                        x /= 256;
                        y /= 256;

                        return (x >= minX && y >= minY && x <= maxX && y <= maxY);
                    }
                );

                return regions;
            }
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
