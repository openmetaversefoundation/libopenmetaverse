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
        Dictionary<UUID, EventQueueServer> eventQueues = new Dictionary<UUID, EventQueueServer>();

        public CapsManager()
        {
        }

        public void Start(Simian server)
        {
            this.server = server;
            capsServer = new CapsServer(server.HttpServer, @"^/caps/");
        }

        public void Stop()
        {
            lock (eventQueues)
            {
                foreach (EventQueueServer eventQueue in eventQueues.Values)
                    eventQueue.Stop();
            }

            capsServer.Stop();
        }

        public UUID CreateCapability(HttpServer.HttpRequestCallback localHandler, bool clientCertRequired)
        {
            return capsServer.CreateCapability(localHandler, clientCertRequired);
        }

        public UUID CreateCapability(Uri remoteHandler, bool clientCertRequired)
        {
            return capsServer.CreateCapability(remoteHandler, clientCertRequired);
        }

        public bool RemoveCapability(UUID capID)
        {
            return capsServer.RemoveCapability(capID);
        }

        public void SendEvent(Agent agent, string name, OSDMap body)
        {
            EventQueueServer eventQueue;
            if (eventQueues.TryGetValue(agent.Avatar.ID, out eventQueue))
            {
                eventQueue.SendEvent(name, body);
            }
            else
            {
                Logger.Log(String.Format("Cannot send the event {0} to agent {1} {2}, no event queue for that avatar",
                    name, agent.FirstName, agent.LastName), Helpers.LogLevel.Warning);
            }
        }
    }
}
