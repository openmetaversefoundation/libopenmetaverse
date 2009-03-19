using System;
using System.Collections.Generic;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.StructuredData;

namespace Simian
{
    public class AccountManager : IExtension<Simian>, IAccountProvider, IPersistable
    {
        Simian server;
        DoubleDictionary<string, UUID, AgentInfo> accounts = new DoubleDictionary<string, UUID, AgentInfo>();

        public AccountManager()
        {
        }

        public bool Start(Simian server)
        {
            this.server = server;
            return true;
        }

        public void Stop()
        {
        }

        public void AddAccount(AgentInfo agent)
        {
            accounts.Add(agent.FirstName + " " + agent.LastName, agent.ID, agent);
        }

        public bool RemoveAccount(UUID agentID)
        {
            AgentInfo agent;
            if (accounts.TryGetValue(agentID, out agent))
                return accounts.Remove(agent.FirstName + " " + agent.LastName, agentID);
            else
                return false;
        }

        public AgentInfo CreateInstance(UUID agentID)
        {
            AgentInfo agent;
            if (accounts.TryGetValue(agentID, out agent))
            {
                return agent;
            }
            else
            {
                Logger.Log(String.Format("Agent {0} does not exist in the account store", agentID),
                    Helpers.LogLevel.Error);
                return null;
            }
        }

        public bool TryGetAccount(UUID agentID, out AgentInfo agent)
        {
            return accounts.TryGetValue(agentID, out agent);
        }

        public bool TryGetAccount(string fullName, out AgentInfo agent)
        {
            return accounts.TryGetValue(fullName, out agent);
        }

        #region Persistence

        public OSD Serialize()
        {
            OSDArray array = new OSDArray(accounts.Count);

            accounts.ForEach(delegate(AgentInfo agent)
            {
                OSDMap agentMap = OSD.SerializeMembers(agent);
                array.Add(agentMap);
            });

            Logger.Log(String.Format("Serializing the agent store with {0} entries", accounts.Count),
                Helpers.LogLevel.Info);

            return array;
        }

        public void Deserialize(OSD serialized)
        {
            accounts.Clear();

            OSDArray array = (OSDArray)serialized;

            for (int i = 0; i < array.Count; i++)
            {
                OSDMap map = array[i] as OSDMap;

                AgentInfo agentInfo = new AgentInfo();
                object agentRef = (object)agentInfo;
                OSD.DeserializeMembers(ref agentRef, map);
                agentInfo = (AgentInfo)agentRef;

                accounts.Add(agentInfo.FirstName + " " + agentInfo.LastName, agentInfo.ID, agentInfo);
            }

            Logger.Log(String.Format("Deserialized the agent store with {0} entries", accounts.Count),
                Helpers.LogLevel.Info);
        }

        #endregion Persistence
    }
}
