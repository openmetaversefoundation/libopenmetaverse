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

        public UUID Authenticate(string firstName, string lastName, string password)
        {
            string fullName = String.Format("{0} {1}", firstName, lastName);

            Agent agent;
            if (!server.Accounts.TryGetAccount(fullName, out agent))
            {
                // Account doesn't exist, create it now
                Avatar avatar = new Avatar();
                SimulationObject obj = new SimulationObject(avatar, server);
                agent = new Agent(obj);
                agent.AccessLevel = "M";
                agent.Avatar.Prim.ID = UUID.Random();
                agent.Balance = 1000;
                agent.CreationTime = Utils.DateTimeToUnixTime(DateTime.Now);
                agent.CurrentLookAt = Vector3.UnitZ;
                agent.Avatar.Prim.Position = new Vector3(128f, 128f, 25f);
                agent.CurrentRegionHandle = Utils.UIntsToLong(256 * server.Scene.RegionX, 256 * server.Scene.RegionY);
                agent.FirstName = firstName;
                agent.GodLevel = 0;
                agent.HomeLookAt = agent.CurrentLookAt;
                agent.HomePosition = agent.Avatar.Prim.Position;
                agent.HomeRegionHandle = agent.CurrentRegionHandle;
                agent.LastName = lastName;
                agent.PasswordHash = password;

                // Create a very basic inventory skeleton
                UUID rootFolder = UUID.Random();
                server.Inventory.CreateRootFolder(agent.ID, rootFolder, "Inventory", agent.ID);
                UUID libraryRootFolder = UUID.Random();
                server.Inventory.CreateRootFolder(agent.ID, libraryRootFolder, "Library", agent.ID);

                agent.InventoryRoot = rootFolder;
                agent.InventoryLibraryOwner = agent.ID;
                agent.InventoryLibraryRoot = libraryRootFolder;

                // Create some inventory items for appearance
                UUID clothingFolder = UUID.Random();
                server.Inventory.CreateFolder(agent.ID, clothingFolder, "Clothing", AssetType.Clothing,
                    agent.InventoryRoot, agent.ID);
                UUID defaultOutfitFolder = UUID.Random();
                server.Inventory.CreateFolder(agent.ID, defaultOutfitFolder, "Default Outfit", AssetType.Unknown,
                    clothingFolder, agent.ID);

                UUID hairAsset = new UUID("dc675529-7ba5-4976-b91d-dcb9e5e36188");
                UUID hairItem = server.Inventory.CreateItem(agent.ID, "Default Hair", "Default Hair",
                    InventoryType.Wearable, AssetType.Bodypart, hairAsset, defaultOutfitFolder,
                    PermissionMask.All, PermissionMask.All, agent.ID, agent.ID, UUID.Random(), 0, false);
                UUID pantsAsset = new UUID("3e8ee2d6-4f21-4a55-832d-77daa505edff");
                UUID pantsItem = server.Inventory.CreateItem(agent.ID, "Default Pants", "Default Pants",
                    InventoryType.Wearable, AssetType.Clothing, pantsAsset, defaultOutfitFolder,
                    PermissionMask.All, PermissionMask.All, agent.ID, agent.ID, UUID.Random(), 0, false);
                UUID shapeAsset = new UUID("530a2614-052e-49a2-af0e-534bb3c05af0");
                UUID shapeItem = server.Inventory.CreateItem(agent.ID, "Default Shape", "Default Shape",
                    InventoryType.Wearable, AssetType.Clothing, shapeAsset, defaultOutfitFolder,
                    PermissionMask.All, PermissionMask.All, agent.ID, agent.ID, UUID.Random(), 0, false);
                UUID shirtAsset = new UUID("6a714f37-fe53-4230-b46f-8db384465981");
                UUID shirtItem = server.Inventory.CreateItem(agent.ID, "Default Shirt", "Default Shirt",
                    InventoryType.Wearable, AssetType.Clothing, shirtAsset, defaultOutfitFolder,
                    PermissionMask.All, PermissionMask.All, agent.ID, agent.ID, UUID.Random(), 0, false);
                UUID skinAsset = new UUID("5f787f25-f761-4a35-9764-6418ee4774c4");
                UUID skinItem = server.Inventory.CreateItem(agent.ID, "Default Skin", "Default Skin",
                    InventoryType.Wearable, AssetType.Clothing, skinAsset, defaultOutfitFolder,
                    PermissionMask.All, PermissionMask.All, agent.ID, agent.ID, UUID.Random(), 0, false);
                UUID eyesAsset = new UUID("78d20332-9b07-44a2-bf74-3b368605f4b5");
                UUID eyesItem = server.Inventory.CreateItem(agent.ID, "Default Eyes", "Default Eyes",
                    InventoryType.Wearable, AssetType.Bodypart, eyesAsset, defaultOutfitFolder,
                    PermissionMask.All, PermissionMask.All, agent.ID, agent.ID, UUID.Random(), 0, false);

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
                return agent.ID;
            else
                return UUID.Zero;
        }
    }
}
