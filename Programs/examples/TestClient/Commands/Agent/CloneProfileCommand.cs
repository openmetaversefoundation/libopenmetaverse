using System;
using System.Collections.Generic;
using System.Threading;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace OpenMetaverse.TestClient
{
    public class CloneProfileCommand : Command
    {
        Avatar.AvatarProperties Properties;
        Avatar.Interests Interests;
        List<UUID> Groups = new List<UUID>();
        bool ReceivedProperties = false;
        bool ReceivedInterests = false;
        bool ReceivedGroups = false;
        ManualResetEvent ReceivedProfileEvent = new ManualResetEvent(false);

        public CloneProfileCommand(TestClient testClient)
        {
            testClient.Avatars.OnAvatarInterests += new AvatarManager.AvatarInterestsCallback(Avatars_OnAvatarInterests);
            testClient.Avatars.OnAvatarProperties += new AvatarManager.AvatarPropertiesCallback(Avatars_OnAvatarProperties);
            testClient.Avatars.OnAvatarGroups += new AvatarManager.AvatarGroupsCallback(Avatars_OnAvatarGroups);
            testClient.Groups.GroupJoinedReply += new EventHandler<GroupOperationEventArgs>(Groups_OnGroupJoined);
            
            testClient.Avatars.OnAvatarPicks += new AvatarManager.AvatarPicksCallback(Avatars_OnAvatarPicks);
            testClient.Avatars.OnPickInfo += new AvatarManager.PickInfoCallback(Avatars_OnPickInfo);

            Name = "cloneprofile";
            Description = "Clones another avatars profile as closely as possible. WARNING: This command will " +
                "destroy your existing profile! Usage: cloneprofile [targetuuid]";
            Category = CommandCategory.Other;
        }        

        public override string Execute(string[] args, UUID fromAgentID)
        {
            if (args.Length != 1)
                return Description;

            UUID targetID;
            ReceivedProperties = false;
            ReceivedInterests = false;
            ReceivedGroups = false;

            try
            {
                targetID = new UUID(args[0]);
            }
            catch (Exception)
            {
                return Description;
            }

            // Request all of the packets that make up an avatar profile
            Client.Avatars.RequestAvatarProperties(targetID);

            //Request all of the avatars pics
            Client.Avatars.RequestAvatarPicks(Client.Self.AgentID);
            Client.Avatars.RequestAvatarPicks(targetID);

            // Wait for all the packets to arrive
            ReceivedProfileEvent.Reset();
            ReceivedProfileEvent.WaitOne(5000, false);

            // Check if everything showed up
            if (!ReceivedInterests || !ReceivedProperties || !ReceivedGroups)
                return "Failed to retrieve a complete profile for that UUID";

            // Synchronize our profile
            Client.Self.UpdateInterests(Interests);
            Client.Self.UpdateProfile(Properties);

            // TODO: Leave all the groups we're currently a member of? This could
            // break TestClient connectivity that might be relying on group authentication

            // Attempt to join all the groups
            foreach (UUID groupID in Groups)
            {
                Client.Groups.RequestJoinGroup(groupID);
            }

            return "Synchronized our profile to the profile of " + targetID.ToString();
        }

        void Avatars_OnAvatarPicks(UUID avatarid, Dictionary<UUID, string> picks)
        {
            foreach (KeyValuePair<UUID, string> kvp in picks)
            {
                if (avatarid == Client.Self.AgentID)
                {
                    Client.Self.PickDelete(kvp.Key);
                }
                else
                {
                    Client.Avatars.RequestPickInfo(avatarid, kvp.Key);
                }
            }
        }

        void Avatars_OnPickInfo(UUID pickid, ProfilePick pick)
        {
            Client.Self.PickInfoUpdate(pickid, pick.TopPick, pick.ParcelID, pick.Name, pick.PosGlobal, pick.SnapshotID, pick.Desc);
        }

        void Avatars_OnAvatarProperties(UUID avatarID, Avatar.AvatarProperties properties)
        {
            lock (ReceivedProfileEvent)
            {
                Properties = properties;
                ReceivedProperties = true;

                if (ReceivedInterests && ReceivedProperties && ReceivedGroups)
                    ReceivedProfileEvent.Set();
            }
        }

        void Avatars_OnAvatarInterests(UUID avatarID, Avatar.Interests interests)
        {
            lock (ReceivedProfileEvent)
            {
                Interests = interests;
                ReceivedInterests = true;

                if (ReceivedInterests && ReceivedProperties && ReceivedGroups)
                    ReceivedProfileEvent.Set();
            }
        }

        void Avatars_OnAvatarGroups(UUID avatarID, List<AvatarGroup> groups)
        {
            lock (ReceivedProfileEvent)
            {
                foreach (AvatarGroup group in groups)
                {
                    Groups.Add(group.GroupID);
                }

                ReceivedGroups = true;

                if (ReceivedInterests && ReceivedProperties && ReceivedGroups)
                    ReceivedProfileEvent.Set();
            }
        }

        void Groups_OnGroupJoined(object sender, GroupOperationEventArgs e)
        {
            Console.WriteLine(Client.ToString() + (e.Success ? " joined " : " failed to join ") +
                e.GroupID.ToString());

            if (e.Success)
            {
                Console.WriteLine(Client.ToString() + " setting " + e.GroupID.ToString() +
                    " as the active group");
                Client.Groups.ActivateGroup(e.GroupID);
            }
        }
    }
}
