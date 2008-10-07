using System;
using ExtensionLoader;
using OpenMetaverse;

namespace Simian.Extensions
{
    public class AuthFreeForAll : IExtension, IAuthenticationProvider
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
                agent.CurrentRegionHandle = Utils.UIntsToLong(Simian.REGION_X, Simian.REGION_Y);
                agent.FirstName = firstName;
                agent.GodLevel = 0;
                agent.HomeLookAt = agent.CurrentLookAt;
                agent.HomePosition = agent.CurrentPosition;
                agent.HomeRegionHandle = agent.CurrentRegionHandle;
                agent.LastName = lastName;
                agent.PasswordHash = password;

                // Create a very basic inventory skeleton
                UUID rootFolder = UUID.Random();
                server.Inventory.CreateRootFolder(agent.AgentID, rootFolder, "Inventory", agent.AgentID);
                UUID libraryRootFolder = UUID.Random();
                server.Inventory.CreateRootFolder(agent.AgentID, libraryRootFolder, "Library", agent.AgentID);

                agent.InventoryRoot = rootFolder;
                agent.InventoryLibraryOwner = agent.AgentID;
                agent.InventoryLibraryRoot = libraryRootFolder;

                // Create some inventory items for appearance
                UUID clothingFolder = UUID.Random();
                server.Inventory.CreateFolder(agent.AgentID, clothingFolder, "Clothing", AssetType.Clothing,
                    agent.InventoryRoot, agent.AgentID);
                UUID defaultOutfitFolder = UUID.Random();
                server.Inventory.CreateFolder(agent.AgentID, defaultOutfitFolder, "Default Outfit", AssetType.Unknown,
                    clothingFolder, agent.AgentID);

                UUID hairAsset = new UUID("dc675529-7ba5-4976-b91d-dcb9e5e36188");
                UUID hairItem = server.Inventory.CreateItem(agent.AgentID, "Default Hair", "Default Hair",
                    InventoryType.Wearable, AssetType.Bodypart, hairAsset, defaultOutfitFolder,
                    PermissionMask.All, PermissionMask.All, agent.AgentID, agent.AgentID, UUID.Random(), 0);
                UUID pantsAsset = new UUID("3e8ee2d6-4f21-4a55-832d-77daa505edff");
                UUID pantsItem = server.Inventory.CreateItem(agent.AgentID, "Default Pants", "Default Pants",
                    InventoryType.Wearable, AssetType.Clothing, pantsAsset, defaultOutfitFolder,
                    PermissionMask.All, PermissionMask.All, agent.AgentID, agent.AgentID, UUID.Random(), 0);
                UUID shapeAsset = new UUID("530a2614-052e-49a2-af0e-534bb3c05af0");
                UUID shapeItem = server.Inventory.CreateItem(agent.AgentID, "Default Shape", "Default Shape",
                    InventoryType.Wearable, AssetType.Clothing, shapeAsset, defaultOutfitFolder,
                    PermissionMask.All, PermissionMask.All, agent.AgentID, agent.AgentID, UUID.Random(), 0);
                UUID shirtAsset = new UUID("6a714f37-fe53-4230-b46f-8db384465981");
                UUID shirtItem = server.Inventory.CreateItem(agent.AgentID, "Default Shirt", "Default Shirt",
                    InventoryType.Wearable, AssetType.Clothing, shirtAsset, defaultOutfitFolder,
                    PermissionMask.All, PermissionMask.All, agent.AgentID, agent.AgentID, UUID.Random(), 0);
                UUID skinAsset = new UUID("5f787f25-f761-4a35-9764-6418ee4774c4");
                UUID skinItem = server.Inventory.CreateItem(agent.AgentID, "Default Skin", "Default Skin",
                    InventoryType.Wearable, AssetType.Clothing, skinAsset, defaultOutfitFolder,
                    PermissionMask.All, PermissionMask.All, agent.AgentID, agent.AgentID, UUID.Random(), 0);

                agent.HairAsset = hairAsset;
                agent.HairItem = hairItem;
                agent.PantsAsset = pantsAsset;
                agent.PantsItem = pantsItem;
                agent.ShapeAsset = shapeAsset;
                agent.ShapeItem = shapeItem;
                agent.ShirtAsset = shirtAsset;
                agent.ShirtItem = shirtItem;
                agent.SkinAsset = skinAsset;
                agent.SkinItem = skinItem;

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
