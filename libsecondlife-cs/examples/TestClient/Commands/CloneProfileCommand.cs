using System;
using System.Collections.Generic;
using System.Threading;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class CloneProfileCommand : Command
    {
        Avatar.Properties Properties;
        Avatar.Interests Interests;
        bool ReceivedProperties = false;
        bool ReceivedInterests = false;
        ManualResetEvent ReceivedProfileEvent = new ManualResetEvent(false);

        public CloneProfileCommand(TestClient testClient)
        {
            testClient.Avatars.OnAvatarInterests += new AvatarManager.AvatarInterestsCallback(Avatars_OnAvatarInterests);
            testClient.Avatars.OnAvatarProperties += new AvatarManager.AvatarPropertiesCallback(Avatars_OnAvatarProperties);

            Name = "cloneprofile";
            Description = "Clones another avatars profile as closely as possible. WARNING: This command will " +
                "destroy your existing profile! Usage: cloneprofile [targetuuid]";
        }

        public override string Execute(string[] args, LLUUID fromAgentID)
        {
            if (args.Length != 1)
                return Description;

            LLUUID targetID;
            ReceivedProperties = false;
            ReceivedInterests = false;

            try
            {
                targetID = new LLUUID(args[0]);
            }
            catch (Exception)
            {
                return Description;
            }

            Client.Avatars.RequestAvatarProperties(targetID);

            ReceivedProfileEvent.Reset();
            ReceivedProfileEvent.WaitOne(5000, false);

            if (!ReceivedInterests || !ReceivedProperties)
                return "Failed to retrieve a complete profile for that UUID";

            Client.Self.ProfileInterests = Interests;
            Client.Self.ProfileProperties = Properties;
            Client.Self.SetAvatarInformation();

            return "Synchronized our profile to the profile of " + targetID.ToStringHyphenated();
        }

        void Avatars_OnAvatarProperties(LLUUID avatarID, Avatar.Properties properties)
        {
            lock (ReceivedProfileEvent)
            {
                Properties = properties;
                ReceivedProperties = true;

                if (ReceivedInterests && ReceivedProperties)
                    ReceivedProfileEvent.Set();
            }
        }

        void Avatars_OnAvatarInterests(LLUUID avatarID, Avatar.Interests interests)
        {
            lock (ReceivedProfileEvent)
            {
                Interests = interests;
                ReceivedInterests = true;

                if (ReceivedInterests && ReceivedProperties)
                    ReceivedProfileEvent.Set();
            }
        }
    }
}