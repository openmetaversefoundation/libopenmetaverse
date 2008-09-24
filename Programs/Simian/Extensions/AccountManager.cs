using System;
using System.Collections.Generic;
using OpenMetaverse;
using OpenMetaverse.StructuredData;

namespace Simian.Extensions
{
    public class AccountManager : ISimianExtension, IAccountProvider, IPersistable
    {
        public string StoreName { get { return "Accounts"; } }

        Simian server;
        DoubleDictionary<string, UUID, Agent> accounts = new DoubleDictionary<string, UUID, Agent>();

        public AccountManager(Simian server)
        {
            this.server = server;
        }

        public void Start()
        {
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

                // HACK: FIXME: These should point to actual inventories, not random UUIDs

                // Agent inventory
                InventoryFolder rootFolder = new InventoryFolder();
                rootFolder.ID = agent.InventoryRoot;
                rootFolder.Name = "Inventory";
                rootFolder.OwnerID = agent.AgentID;
                rootFolder.PreferredType = AssetType.RootFolder;
                rootFolder.Version = 1;
                agent.Inventory[rootFolder.ID] = rootFolder;

                // Default library
                InventoryFolder libRootFolder = new InventoryFolder();
                libRootFolder.ID = agent.InventoryLibraryRoot;
                libRootFolder.Name = "Library";
                libRootFolder.OwnerID = agent.AgentID;
                libRootFolder.PreferredType = AssetType.RootFolder;
                libRootFolder.Version = 1;
                agent.Library[libRootFolder.ID] = libRootFolder;

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
