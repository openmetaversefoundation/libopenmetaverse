using System;
using OpenMetaverse;

namespace Simian
{
    public interface IAuthenticationProvider
    {
        UUID Authenticate(string firstName, string lastName, string password);
    }
}
