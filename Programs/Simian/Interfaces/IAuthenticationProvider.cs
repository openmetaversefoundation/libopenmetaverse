using System;
using OpenMetaverse;

namespace Simian
{
    public interface IAuthenticationProvider
    {
        Guid Authenticate(string firstName, string lastName, string password);
    }
}
