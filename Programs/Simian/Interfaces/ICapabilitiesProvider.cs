using System;
using OpenMetaverse;
using OpenMetaverse.Http;
using OpenMetaverse.StructuredData;
using HttpServer;

namespace Simian
{
    public interface ICapabilitiesProvider
    {
        UUID CreateCapability(HttpRequestCallback localHandler, bool clientCertRequired);
        UUID CreateCapability(Uri remoteHandler, bool clientCertRequired);
        bool RemoveCapability(UUID capID);

        void SendEvent(Agent agent, string name, OSDMap body);
    }
}
