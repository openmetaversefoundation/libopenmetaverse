using System;
using System.Collections.Generic;
using System.Text;

namespace libsecondlife.Voice
{
    public partial class VoiceGateway
    {
        /// <summary>
        /// This is used to login a specific user account(s). It may only be called after
        /// Connector initialization has completed successfully
        /// </summary>
        /// <param name="ConnectorHandle">Handle returned from successful Connector ‘create’ request</param>
        /// <param name="AccountName">User's account name</param>
        /// <param name="AccountPassword">User's account password</param>
        /// <param name="AudioSessionAnswerMode">Values may be “AutoAnswer” or “VerifyAnswer”</param>
        /// <param name="AccountURI">""</param>
        /// <param name="ParticipantPropertyFrequency">This is an integer that specifies how often
        /// the daemon will send participant property events while in a channel. If this is not set
        /// the default will be “on state change”, which means that the events will be sent when
        /// the participant starts talking, stops talking, is muted, is unmuted.
        /// The valid values are:
        /// 0 – Never
        /// 5 – 10 times per second
        /// 10 – 5 times per second
        /// 50 – 1 time per second
        /// 100 – on participant state change (this is the default)</param>
        /// <param name="EnableBuddiesAndPresence">false</param>
        /// <returns></returns>
        public int AccountLogin(string ConnectorHandle, string AccountName, string AccountPassword, string AudioSessionAnswerMode, string AccountURI, int ParticipantPropertyFrequency, bool EnableBuddiesAndPresence)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(VoiceGateway.MakeXML("ConnectorHandle", ConnectorHandle));
            sb.Append(VoiceGateway.MakeXML("AccountName", AccountName));
            sb.Append(VoiceGateway.MakeXML("AccountPassword", AccountPassword));
            sb.Append(VoiceGateway.MakeXML("AudioSessionAnswerMode", AudioSessionAnswerMode));
            sb.Append(VoiceGateway.MakeXML("AccountURI", AccountURI));
            sb.Append(VoiceGateway.MakeXML("ParticipantPropertyFrequency", ParticipantPropertyFrequency.ToString()));
            sb.Append(VoiceGateway.MakeXML("EnableBuddiesAndPresence", EnableBuddiesAndPresence ? "true" : "false"));
            return Request("Account.Login.1", sb.ToString());
        }

        /// <summary>
        /// This is used to logout a user session. It should only be called with a valid AccountHandle.
        /// </summary>
        /// <param name="AccountHandle">Handle returned from successful Connector ‘login’ request</param>
        /// <returns></returns>
        public int AccountLogout(string AccountHandle)
        {
            string RequestXML = VoiceGateway.MakeXML("AccountHandle", AccountHandle);
            return Request("Account.Logout.1", RequestXML);
        }
    }
}
