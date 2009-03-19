using System;
using OpenMetaverse;

namespace Simian
{
    public class Email
    {
        public DateTime Time;
        public string Sender;
        public string Subject;
        public string Message;
        public int NumLeft;
    }

    public delegate void InstantMessageCallback(object sender, UUID fromID, string fromName, UUID toID, InstantMessageDialog dialog,
        bool fromGroup, UUID sessionID, bool offline, Vector3 position, uint parentEstateID, UUID regionID, DateTime timestamp,
        string message, byte[] extraData);

    public interface IMessagingProvider
    {
        event InstantMessageCallback OnInstantMessage;

        void SendInstantMessage(object sender, UUID fromID, string fromName, UUID toID, InstantMessageDialog dialog, bool fromGroup,
            UUID sessionID, bool offline, Vector3 position, uint parentEstateID, UUID regionID, DateTime timestamp, string message,
            byte[] extraData);

        void SendEmail(object sender, UUID fromID, string address, string subject, string message);
        bool GetNextEmail(UUID toID, string address, string subject, out Email email);
    }
}
