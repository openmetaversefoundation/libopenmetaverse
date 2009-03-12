using System;
using ExtensionLoader;
using OpenMetaverse;

namespace Simian
{
    public class PermissionsFreeForAll : IExtension<Simian>, IPermissionsProvider
    {
        Simian server;

        public PermissionsFreeForAll()
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

        public Permissions GetDefaultPermissions()
        {
            return Permissions.FullPermissions;
        }

        public PrimFlags GetDefaultObjectFlags()
        {
            return
                PrimFlags.ObjectCopy |
                PrimFlags.ObjectModify |
                PrimFlags.ObjectMove |
                PrimFlags.ObjectOwnerModify |
                PrimFlags.ObjectAnyOwner |
                PrimFlags.ObjectTransfer;
        }
    }
}
