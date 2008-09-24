using System;
using OpenMetaverse;

namespace Simian
{
    public interface IAccountProvider
    {
        void AddAccount(Agent agent);
        bool RemoveAccount(UUID agentID);
        Agent CreateInstance(UUID agentID);
        bool TryGetAccount(UUID agentID, out Agent agent);
        bool TryGetAccount(string fullName, out Agent agent);
    }
}
