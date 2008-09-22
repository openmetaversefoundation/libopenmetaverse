using System;
using OpenMetaverse;

namespace Simian
{
    public interface IParcelProvider
    {
        void SendParcelOverlay(Agent agent);
    }
}
