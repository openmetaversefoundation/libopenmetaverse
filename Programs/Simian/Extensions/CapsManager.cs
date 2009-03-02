using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using ExtensionLoader;
using HttpServer;
using OpenMetaverse;
using OpenMetaverse.Http;
using OpenMetaverse.StructuredData;

namespace Simian.Extensions
{
    public class CapsManager : IExtension<Simian>, ICapabilitiesProvider
    {
        Simian server;
        CapsServer capsServer;

        public CapsManager()
        {
        }

        public void Start(Simian server)
        {
            this.server = server;
            capsServer = new CapsServer(server.HttpServer, @"^/caps/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$");
            capsServer.Start();
        }

        public void Stop()
        {
            if (capsServer != null)
                capsServer.Stop();
        }

        public Uri CreateCapability(CapsRequestCallback localHandler, bool clientCertRequired, object state)
        {
            UUID capID = capsServer.CreateCapability(localHandler, clientCertRequired, state);
            return new Uri(
                (server.SSL ? "https://" : "http://") +
                server.HostName +
                (server.HttpPort == 80 ? String.Empty : ":" + server.HttpPort) +
                "/caps/" + capID.ToString());
        }

        public Uri CreateCapability(Uri remoteHandler, bool clientCertRequired)
        {
            UUID capID = capsServer.CreateCapability(remoteHandler, clientCertRequired);
            return new Uri(
                (server.SSL ? "https://" : "http://") +
                server.HostName +
                (server.HttpPort == 80 ? String.Empty : ":" + server.HttpPort) +
                "/caps/" + capID.ToString());
        }

        public bool RemoveCapability(Uri cap)
        {
            string path = cap.PathAndQuery.TrimEnd('/');
            UUID capID;

            // Parse the capability UUID out of the URI
            if (UUID.TryParse(path.Substring(path.Length - 36), out capID))
                return capsServer.RemoveCapability(capID);
            else
                return false;
        }
    }
}
