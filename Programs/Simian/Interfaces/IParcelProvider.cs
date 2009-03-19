using System;
using OpenMetaverse;

namespace Simian
{
    public interface IParcelProvider
    {
        void SendParcelOverlay(Agent agent);
        void UpdateParcel(Parcel parcel);
        int GetParcelID(int x, int y);
        bool TryGetParcel(int parcelID, out Parcel parcel);
    }
}
