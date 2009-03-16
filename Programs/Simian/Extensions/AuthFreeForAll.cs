using System;
using ExtensionLoader;
using OpenMetaverse;

namespace Simian
{
    public class AuthFreeForAll : IExtension<Simian>, IAuthenticationProvider
    {
        Simian server;

        public AuthFreeForAll()
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

        public UUID Authenticate(string firstName, string lastName, string password)
        {
            AgentInfo agentInfo;
            ISceneProvider scene = server.Grid.GetDefaultLocalScene();
            string fullName = String.Format("{0} {1}", firstName, lastName);

            if (!server.Accounts.TryGetAccount(fullName, out agentInfo))
            {
                // Account doesn't exist, create it now
                agentInfo = new AgentInfo();
                agentInfo.AccessLevel = "M";
                agentInfo.ID = UUID.Random();
                agentInfo.Balance = 1000;
                agentInfo.CreationTime = Utils.DateTimeToUnixTime(DateTime.Now);
                agentInfo.FirstName = firstName;
                agentInfo.GodLevel = 0;
                agentInfo.HomeLookAt = scene.DefaultLookAt;
                agentInfo.HomePosition = scene.DefaultPosition;
                agentInfo.HomeRegionHandle = scene.RegionHandle;
                agentInfo.LastName = lastName;
                agentInfo.PasswordHash = password;

                // Create a very basic inventory skeleton
                UUID rootFolder = UUID.Random();
                server.Inventory.CreateRootFolder(agentInfo.ID, rootFolder, "Inventory", agentInfo.ID);
                UUID libraryRootFolder = UUID.Random();
                server.Inventory.CreateRootFolder(agentInfo.ID, libraryRootFolder, "Library", agentInfo.ID);

                agentInfo.InventoryRoot = rootFolder;
                agentInfo.InventoryLibraryOwner = agentInfo.ID;
                agentInfo.InventoryLibraryRoot = libraryRootFolder;

                // Create some inventory items for appearance
                UUID clothingFolder = UUID.Random();
                server.Inventory.CreateFolder(agentInfo.ID, clothingFolder, "Clothing", AssetType.Clothing,
                    agentInfo.InventoryRoot, agentInfo.ID);
                UUID defaultOutfitFolder = UUID.Random();
                server.Inventory.CreateFolder(agentInfo.ID, defaultOutfitFolder, "Default Outfit", AssetType.Unknown,
                    clothingFolder, agentInfo.ID);

                UUID hairAsset = new UUID("dc675529-7ba5-4976-b91d-dcb9e5e36188");
                UUID hairItem = server.Inventory.CreateItem(agentInfo.ID, "Default Hair", "Default Hair",
                    InventoryType.Wearable, AssetType.Bodypart, hairAsset, defaultOutfitFolder,
                    PermissionMask.All, PermissionMask.All, agentInfo.ID, agentInfo.ID, UUID.Random(), 0).ID;
                UUID pantsAsset = new UUID("3e8ee2d6-4f21-4a55-832d-77daa505edff");
                UUID pantsItem = server.Inventory.CreateItem(agentInfo.ID, "Default Pants", "Default Pants",
                    InventoryType.Wearable, AssetType.Clothing, pantsAsset, defaultOutfitFolder,
                    PermissionMask.All, PermissionMask.All, agentInfo.ID, agentInfo.ID, UUID.Random(), 0).ID;
                UUID shapeAsset = new UUID("530a2614-052e-49a2-af0e-534bb3c05af0");
                UUID shapeItem = server.Inventory.CreateItem(agentInfo.ID, "Default Shape", "Default Shape",
                    InventoryType.Wearable, AssetType.Clothing, shapeAsset, defaultOutfitFolder,
                    PermissionMask.All, PermissionMask.All, agentInfo.ID, agentInfo.ID, UUID.Random(), 0).ID;
                UUID shirtAsset = new UUID("6a714f37-fe53-4230-b46f-8db384465981");
                UUID shirtItem = server.Inventory.CreateItem(agentInfo.ID, "Default Shirt", "Default Shirt",
                    InventoryType.Wearable, AssetType.Clothing, shirtAsset, defaultOutfitFolder,
                    PermissionMask.All, PermissionMask.All, agentInfo.ID, agentInfo.ID, UUID.Random(), 0).ID;
                UUID skinAsset = new UUID("5f787f25-f761-4a35-9764-6418ee4774c4");
                UUID skinItem = server.Inventory.CreateItem(agentInfo.ID, "Default Skin", "Default Skin",
                    InventoryType.Wearable, AssetType.Clothing, skinAsset, defaultOutfitFolder,
                    PermissionMask.All, PermissionMask.All, agentInfo.ID, agentInfo.ID, UUID.Random(), 0).ID;
                UUID eyesAsset = new UUID("78d20332-9b07-44a2-bf74-3b368605f4b5");
                UUID eyesItem = server.Inventory.CreateItem(agentInfo.ID, "Default Eyes", "Default Eyes",
                    InventoryType.Wearable, AssetType.Bodypart, eyesAsset, defaultOutfitFolder,
                    PermissionMask.All, PermissionMask.All, agentInfo.ID, agentInfo.ID, UUID.Random(), 0).ID;

                agentInfo.HairItem = hairItem;
                agentInfo.PantsItem = pantsItem;
                agentInfo.ShapeItem = shapeItem;
                agentInfo.ShirtItem = shirtItem;
                agentInfo.SkinItem = skinItem;
                agentInfo.EyesItem = eyesItem;

                server.Accounts.AddAccount(agentInfo);

                Logger.Log("Created new account for " + fullName, Helpers.LogLevel.Info);
            }

            if (password == agentInfo.PasswordHash)
            {
                Logger.Log("Authenticated account for " + fullName, Helpers.LogLevel.Info);
                return agentInfo.ID;
            }
            else
            {
                Logger.Log("Authentication failed for " + fullName, Helpers.LogLevel.Warning);
                return UUID.Zero;
            }
        }
    }
}
