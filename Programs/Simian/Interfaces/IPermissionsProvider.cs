using System;
using OpenMetaverse;

namespace Simian
{
    public interface IPermissionsProvider
    {
        Permissions GetDefaultPermissions();
        PrimFlags GetDefaultObjectFlags();
    }
}
