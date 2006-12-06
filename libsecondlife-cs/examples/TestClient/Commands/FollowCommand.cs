using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class FollowCommand: Command
    {
		public FollowCommand()
		{
			Name = "follow";
			Description = "Follow another avatar. (usage: follow [FirstName LastName])  If no target is set then will follow master.";
		}

        public override string Execute(SecondLife Client, string[] args, LLUUID fromAgentID)
		{
			string target = String.Empty;
			for (int ct = 0; ct < args.Length; ct++)
				target = target + args[ct] + " ";
			target = target.TrimEnd();

			if (target.Length == 0)
				target = TestClient.Master;

            if (target.Length > 0)
            {
                if (Follow(target))
                    return "Following " + target;
                else
                    return "Unable to follow " + target + ".  Client may not be able to see that avatar.";
            }
            else
            {
                return "No target specified and no master is set. usage: follow [FirstName LastName])";
            }
		}

        const float DISTANCE_BUFFER = 3.0f;
        string followName;
		Avatar followAvatar;

        bool Follow(string name)
        {
            foreach (Avatar av in TestClient.Avatars.Values)
            {
                if (av.Name == name)
				{
		            followName = name;
					followAvatar = av;
					Active = true;
	                return true;
				}
            }
            return false;
        }

		public override void Think(SecondLife Client)
		{
            if (Helpers.VecDist(followAvatar.Position, Client.Self.Position) > DISTANCE_BUFFER)
            {
                //move toward target
				if (followAvatar.CurrentRegion.GridRegionData != null)
				{
					ulong x = (ulong)(followAvatar.Position.X + (followAvatar.CurrentRegion.GridRegionData.X * 256));
					ulong y = (ulong)(followAvatar.Position.Y + (followAvatar.CurrentRegion.GridRegionData.Y * 256));
					Client.Self.AutoPilotLocal(Convert.ToInt32(followAvatar.Position.X), Convert.ToInt32(followAvatar.Position.Y), followAvatar.Position.Z);
				}
            }
			//else
			//{
			//    //stop at current position
			//    LLVector3 myPos = client.Self.Position;
			//    client.Self.AutoPilot((ulong)myPos.x, (ulong)myPos.y, myPos.Z);
			//}

			base.Think(Client);
		}

    }
}
