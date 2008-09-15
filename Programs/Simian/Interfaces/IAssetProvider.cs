using System;
using OpenMetaverse;

namespace Simian
{
    public interface IAssetProvider
    {
        void StoreAsset(Asset asset);
        bool TryGetAsset(UUID id, out Asset asset);
    }
}
