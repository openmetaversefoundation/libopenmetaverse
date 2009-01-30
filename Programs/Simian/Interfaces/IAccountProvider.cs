using System;
using OpenMetaverse;

namespace Simian
{
    public interface IAccountProvider
    {
        void AddAccount(Agent agent);
        bool RemoveAccount(Guid agentID);
        Agent CreateInstance(Guid agentID);
        bool TryGetAccount(Guid agentID, out Agent agent);
        bool TryGetAccount(string fullName, out Agent agent);
    }
}
