using System;
using OpenMetaverse;

namespace Simian
{
    public interface IAccountProvider
    {
        void AddAccount(AgentInfo agent);
        bool RemoveAccount(UUID agentID);
        AgentInfo CreateInstance(UUID agentID);
        bool TryGetAccount(UUID agentID, out AgentInfo agent);
        bool TryGetAccount(string fullName, out AgentInfo agent);
    }
}
