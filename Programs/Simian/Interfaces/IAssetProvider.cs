using System;
using System.Collections.Generic;
using OpenMetaverse;

namespace Simian
{
    public interface IAssetProvider
    {
        void StoreAsset(Asset asset);
        bool TryGetAsset(UUID id, out Asset asset);

        byte[] EncodePrimAsset(List<SimulationObject> linkset);
        bool TryDecodePrimAsset(byte[] primAssetData, out List<SimulationObject> linkset);
    }
}
