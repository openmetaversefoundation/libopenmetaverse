using System;
using System.Collections.Generic;
using System.Threading;
using libsecondlife;
using libsecondlife.Packets;
using System.Text;

namespace libsecondlife.TestClient
{
    public class FriendsCommand : Command
    {        
        ManualResetEvent GetCurrentGroupsEvent = new ManualResetEvent(false);

        public FriendsCommand(TestClient testClient)
        {
            Name = "friends";
            Description = "List avatar friends. Usage: friends";
        }
        public override string Execute(string[] args, LLUUID fromAgentID)
        {
            List<FriendInfo> friends = Client.Friends.FriendsList();
            if (friends.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (FriendInfo friend in friends)
                {
                    sb.AppendLine(friend.Name);
                }
                return sb.ToString();
            }
            else
            {
                return "No friends";
            }
        }
    }
}
