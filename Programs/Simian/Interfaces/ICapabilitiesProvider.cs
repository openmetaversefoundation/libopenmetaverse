using System;
using OpenMetaverse;
using OpenMetaverse.Http;
using OpenMetaverse.StructuredData;
using HttpServer;

namespace Simian
{
    public interface ICapabilitiesProvider
    {
        Uri CreateCapability(CapsRequestCallback localHandler, bool clientCertRequired, object state);
        Uri CreateCapability(Uri remoteHandler, bool clientCertRequired);
        bool RemoveCapability(Uri cap);
    }
}
