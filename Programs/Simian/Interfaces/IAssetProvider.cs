using System;
using OpenMetaverse;

namespace Simian
{
    public interface IAssetProvider
    {
        void StoreAsset(Asset asset);
        void StoreTexture(AssetTexture texture);
        bool TryGetAsset(UUID id, out Asset asset);
    }
}
