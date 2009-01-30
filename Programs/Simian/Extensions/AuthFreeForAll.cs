using System;
using ExtensionLoader;
using OpenMetaverse;

namespace Simian.Extensions
{
    public class AuthFreeForAll : IExtension<Simian>, IAuthenticationProvider
    {
        Simian server;

        public AuthFreeForAll()
        {
        }

        public void Start(Simian server)
        {
            this.server = server;
        }

        public void Stop()
        {
        }

        public Guid Authenticate(string firstName, string lastName, string password)
        {
            string fullName = String.Format("{0} {1}", firstName, lastName);

            Agent agent;
            if (!server.Accounts.TryGetAccount(fullName, out agent))
            {
                // Account doesn't exist, create it now
                agent = new Agent();
                agent.AccessLevel = "M";
                agent.AgentID = Guid.NewGuid();
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
                Guid rootFolder = Guid.NewGuid();
                server.Inventory.CreateRootFolder(agent.AgentID, rootFolder, "Inventory", agent.AgentID);
                Guid libraryRootFolder = Guid.NewGuid();
                server.Inventory.CreateRootFolder(agent.AgentID, libraryRootFolder, "Library", agent.AgentID);

                agent.InventoryRoot = rootFolder;
                agent.InventoryLibraryOwner = agent.AgentID;
                agent.InventoryLibraryRoot = libraryRootFolder;

                // Create some inventory items for appearance
                Guid clothingFolder = Guid.NewGuid();
                server.Inventory.CreateFolder(agent.AgentID, clothingFolder, "Clothing", AssetType.Clothing,
                    agent.InventoryRoot, agent.AgentID);
                Guid defaultOutfitFolder = Guid.NewGuid();
                server.Inventory.CreateFolder(agent.AgentID, defaultOutfitFolder, "Default Outfit", AssetType.Unknown,
                    clothingFolder, agent.AgentID);

                Guid hairAsset = new Guid("dc675529-7ba5-4976-b91d-dcb9e5e36188");
                Guid hairItem = server.Inventory.CreateItem(agent.AgentID, "Default Hair", "Default Hair",
                    InventoryType.Wearable, AssetType.Bodypart, hairAsset, defaultOutfitFolder,
                    PermissionMask.All, PermissionMask.All, agent.AgentID, agent.AgentID, Guid.NewGuid(), 0, false);
                Guid pantsAsset = new Guid("3e8ee2d6-4f21-4a55-832d-77daa505edff");
                Guid pantsItem = server.Inventory.CreateItem(agent.AgentID, "Default Pants", "Default Pants",
                    InventoryType.Wearable, AssetType.Clothing, pantsAsset, defaultOutfitFolder,
                    PermissionMask.All, PermissionMask.All, agent.AgentID, agent.AgentID, Guid.NewGuid(), 0, false);
                Guid shapeAsset = new Guid("530a2614-052e-49a2-af0e-534bb3c05af0");
                Guid shapeItem = server.Inventory.CreateItem(agent.AgentID, "Default Shape", "Default Shape",
                    InventoryType.Wearable, AssetType.Clothing, shapeAsset, defaultOutfitFolder,
                    PermissionMask.All, PermissionMask.All, agent.AgentID, agent.AgentID, Guid.NewGuid(), 0, false);
                Guid shirtAsset = new Guid("6a714f37-fe53-4230-b46f-8db384465981");
                Guid shirtItem = server.Inventory.CreateItem(agent.AgentID, "Default Shirt", "Default Shirt",
                    InventoryType.Wearable, AssetType.Clothing, shirtAsset, defaultOutfitFolder,
                    PermissionMask.All, PermissionMask.All, agent.AgentID, agent.AgentID, Guid.NewGuid(), 0, false);
                Guid skinAsset = new Guid("5f787f25-f761-4a35-9764-6418ee4774c4");
                Guid skinItem = server.Inventory.CreateItem(agent.AgentID, "Default Skin", "Default Skin",
                    InventoryType.Wearable, AssetType.Clothing, skinAsset, defaultOutfitFolder,
                    PermissionMask.All, PermissionMask.All, agent.AgentID, agent.AgentID, Guid.NewGuid(), 0, false);
                Guid eyesAsset = new Guid("78d20332-9b07-44a2-bf74-3b368605f4b5");
                Guid eyesItem = server.Inventory.CreateItem(agent.AgentID, "Default Eyes", "Default Eyes",
                    InventoryType.Wearable, AssetType.Bodypart, eyesAsset, defaultOutfitFolder,
                    PermissionMask.All, PermissionMask.All, agent.AgentID, agent.AgentID, Guid.NewGuid(), 0, false);

                agent.HairItem = hairItem;
                agent.PantsItem = pantsItem;
                agent.ShapeItem = shapeItem;
                agent.ShirtItem = shirtItem;
                agent.SkinItem = skinItem;
                agent.EyesItem = eyesItem;

                server.Accounts.AddAccount(agent);

                Logger.Log("Created new account for " + fullName, Helpers.LogLevel.Info);
            }

            if (password == agent.PasswordHash)
                return agent.AgentID;
            else
                return Guid.Empty;
        }
    }
}
