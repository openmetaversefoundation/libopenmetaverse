using System;
using OpenMetaverse;

namespace Simian
{
    public interface IAssetProvider
    {
        void StoreAsset(Asset asset);
        bool TryGetAsset(Guid id, out Asset asset);
    }
}
