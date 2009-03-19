using System;
using OpenMetaverse;

namespace Simian
{
    public interface IAvatarProvider
    {
        bool SetDefaultAnimation(Agent agent, UUID animID);
        bool AddAnimation(Agent agent, UUID animID);
        bool RemoveAnimation(Agent agent, UUID animID);
        bool ClearAnimations(Agent agent);
        void SendAnimations(Agent agent);

        void SendAlert(Agent agent, string message);
    }
}
