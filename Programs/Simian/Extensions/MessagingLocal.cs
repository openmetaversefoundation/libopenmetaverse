using System;
using System.Collections.Generic;
using ExtensionLoader;
using OpenMetaverse;

namespace Simian
{
    // FIXME: Implement this class
    class MessagingLocal : IExtension<Simian>, IMessagingProvider
    {
        Simian server;

        public event InstantMessageCallback OnInstantMessage;

        public MessagingLocal()
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

        public void SendInstantMessage(object sender, UUID fromID, string fromName, UUID toID, InstantMessageDialog dialog, bool fromGroup,
            UUID sessionID, bool offline, Vector3 position, uint parentEstateID, UUID regionID, DateTime timestamp, string message,
            byte[] extraData)
        {
            if (OnInstantMessage != null)
            {
                OnInstantMessage(sender, fromID, fromName, toID, dialog, fromGroup, sessionID, offline, position, parentEstateID,
                    regionID, timestamp, message, extraData);
            }
        }

        public void SendEmail(object sender, UUID fromID, string address, string subject, string message)
        {
        }

        public bool GetNextEmail(UUID toID, string address, string subject, out Email email)
        {
            email = null;
            return false;
        }
    }
}
