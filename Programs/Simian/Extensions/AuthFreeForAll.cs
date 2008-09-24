using System;
using OpenMetaverse;

namespace Simian.Extensions
{
    public class AuthFreeForAll : ISimianExtension, IAuthenticationProvider
    {
        Simian server;

        public AuthFreeForAll(Simian server)
        {
            this.server = server;
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public UUID Authenticate(string firstName, string lastName, string password)
        {
            string fullName = String.Format("{0} {1}", firstName, lastName);

            Agent agent;
            if (!server.Accounts.TryGetAccount(fullName, out agent))
            {
                // Account doesn't exist, create it now
                agent = new Agent();
                agent.AccessLevel = "M";
                agent.AgentID = UUID.Random();
                agent.Balance = 1000;
                agent.CreationTime = Utils.DateTimeToUnixTime(DateTime.Now);
                agent.CurrentLookAt = Vector3.Zero;
                agent.CurrentPosition = new Vector3(128f, 128f, 25f);
                agent.CurrentRegionHandle = Helpers.UIntsToLong(Simian.REGION_X, Simian.REGION_Y);
                agent.FirstName = firstName;
                agent.GodLevel = 0;
                agent.HomeLookAt = agent.CurrentLookAt;
                agent.HomePosition = agent.CurrentPosition;
                agent.HomeRegionHandle = agent.CurrentRegionHandle;
                agent.InventoryLibraryOwner = UUID.Random(); // FIXME:
                agent.InventoryLibraryRoot = UUID.Random(); // FIXME:
                agent.InventoryRoot = UUID.Random(); // FIXME:
                agent.LastName = lastName;
                agent.PasswordHash = password;
                // FIXME: Give new avatars a default appearance
                //agent.VisualParams;
                //agent.Texture;

                server.Accounts.AddAccount(agent);

                Logger.Log("Created new account for " + fullName, Helpers.LogLevel.Info);
            }

            if (password == agent.PasswordHash)
                return agent.AgentID;
            else
                return UUID.Zero;
        }
    }
}
