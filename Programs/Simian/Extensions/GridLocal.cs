using System;
using ExtensionLoader;
using OpenMetaverse;

namespace Simian
{
    public class GridLocal : IExtension<Simian>, IGridProvider
    {
        Simian server;

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

        public bool TryRegisterGridSpace(RegionInfo region, out UUID regionID)
        {
            regionID = UUID.Zero;
            return false;
        }

        public bool TryRegisterAnyGridSpace(RegionInfo region, bool isolated, out UUID regionID)
        {
            regionID = UUID.Zero;
            return false;
        }

        public bool UnregisterGridSpace(RegionInfo region)
        {
            return false;
        }

        public void RegionUpdate(RegionInfo region)
        {
        }

        public void RegionHeartbeat(RegionInfo region)
        {
        }

        public bool TryGetRegion(UUID regionID, out RegionInfo region)
        {
            region = null;
            return false;
        }

        public bool TryGetRegion(uint x, uint y, out RegionInfo region)
        {
            region = null;
            return false;
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
