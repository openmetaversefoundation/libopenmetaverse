using System;
using System.Collections.Generic;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.StructuredData;

namespace Simian.Extensions
{
    public class AccountManager : IExtension<Simian>, IAccountProvider, IPersistable
    {
        Simian server;
        DoubleDictionary<string, UUID, Agent> accounts = new DoubleDictionary<string, UUID, Agent>();

        public AccountManager()
        {
        }

        public void Start(Simian server)
        {
            this.server = server;
        }

        public void Stop()
        {
        }

        public void AddAccount(Agent agent)
        {
            accounts.Add(agent.FullName, agent.AgentID, agent);
        }

        public bool RemoveAccount(UUID agentID)
        {
            Agent agent;
            if (accounts.TryGetValue(agentID, out agent))
                return accounts.Remove(agent.FullName, agentID);
            else
                return false;
        }

        public Agent CreateInstance(UUID agentID)
        {
            Agent agent;
            if (accounts.TryGetValue(agentID, out agent))
            {
                // Random session IDs
                agent.SessionID = UUID.Random();
                agent.SecureSessionID = UUID.Random();

                // Avatar flags
                agent.Flags = PrimFlags.Physics | PrimFlags.ObjectModify | PrimFlags.ObjectCopy |
                    PrimFlags.ObjectAnyOwner | PrimFlags.ObjectMove | PrimFlags.InventoryEmpty |
                    PrimFlags.ObjectTransfer | PrimFlags.ObjectOwnerModify | PrimFlags.ObjectYouOwner;

                return agent;
            }
            else
            {
                Logger.Log(String.Format("Agent {0} does not exist in the account store", agentID),
                    Helpers.LogLevel.Error);
                return null;
            }
        }

        public bool TryGetAccount(UUID agentID, out Agent agent)
        {
            return accounts.TryGetValue(agentID, out agent);
        }

        public bool TryGetAccount(string fullName, out Agent agent)
        {
            return accounts.TryGetValue(fullName, out agent);
        }

        #region Persistance

        public LLSD Serialize()
        {
            LLSDArray array = new LLSDArray(accounts.Count);

            accounts.ForEach(delegate(Agent agent)
            {
                array.Add(LLSD.SerializeMembers(agent));
            });

            Logger.Log(String.Format("Serializing the agent store with {0} entries", accounts.Count),
                Helpers.LogLevel.Info);

            return array;
        }

        public void Deserialize(LLSD serialized)
        {
            accounts.Clear();

            LLSDArray array = (LLSDArray)serialized;

            for (int i = 0; i < array.Count; i++)
            {
                Agent agent = new Agent();
                object agentRef = (object)agent;
                LLSD.DeserializeMembers(ref agentRef, (LLSDMap)array[i]);
                agent = (Agent)agentRef;

                accounts.Add(agent.FullName, agent.AgentID, agent);
            }

            Logger.Log(String.Format("Deserialized the agent store with {0} entries", accounts.Count),
                Helpers.LogLevel.Info);
        }

        #endregion Persistance
    }
}
