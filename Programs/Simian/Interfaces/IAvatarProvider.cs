using System;
using OpenMetaverse;

namespace Simian
{
    public interface IAvatarProvider
    {
        bool SetDefaultAnimation(Agent agent, Guid animID);
        bool AddAnimation(Agent agent, Guid animID);
        bool RemoveAnimation(Agent agent, Guid animID);
        void SendAnimations(Agent agent);
    }
}
