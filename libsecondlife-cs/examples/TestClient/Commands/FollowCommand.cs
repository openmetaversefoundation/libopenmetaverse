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
			Description = "Follow another avatar. (usage: follow FirstName LastName)";
		}

        public override string Execute(SecondLife Client, string[] args, LLUUID fromAgentID)
		{
			string target = String.Empty;
			for (int ct = 0; ct < args.Length;ct++)
				target = target + args[ct] + " ";
			target = target.TrimEnd();
			
			if (Follow(target))
				return "Following " + target;
			else
				return "Unable to follow " + target + ".  Client may not be able to see that avatar.";
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
            if (vecDist(followAvatar.Position, Client.Self.Position) > DISTANCE_BUFFER)
            {
                //move toward target
				ulong x = (ulong)(followAvatar.Position.X + (followAvatar.CurrentRegion.GridRegionData.X * 256));
				ulong y = (ulong)(followAvatar.Position.Y + (followAvatar.CurrentRegion.GridRegionData.Y * 256));
                Client.Self.AutoPilotLocal(Convert.ToInt32(followAvatar.Position.X), Convert.ToInt32(followAvatar.Position.Y), followAvatar.Position.Z);
            }
			//else
			//{
			//    //stop at current position
			//    LLVector3 myPos = Client.Self.Position;
			//    Client.Self.AutoPilot((ulong)myPos.X, (ulong)myPos.Y, myPos.Z);
			//}

			base.Think(Client);
		}

		//void SendAgentUpdate(uint ControlID)
		//{
		//    AgentUpdatePacket p = new AgentUpdatePacket();
		//    p.AgentData.Far = 30.0f;
		//    p.AgentData.CameraAtAxis = new LLVector3(0, 0, 0);
		//    p.AgentData.CameraCenter = new LLVector3(0, 0, 0);
		//    p.AgentData.CameraLeftAxis = new LLVector3(0, 0, 0);
		//    p.AgentData.CameraUpAxis = new LLVector3(0, 0, 0);
		//    p.AgentData.HeadRotation = new LLQuaternion(0, 0, 0, 1); ;
		//    p.AgentData.BodyRotation = new LLQuaternion(0, 0, 0, 1); ;
		//    p.AgentData.AgentID = client.Network.AgentID;
		//    p.AgentData.SessionID = client.Network.SessionID;
		//    p.AgentData.ControlFlags = ControlID;
		//    client.Network.SendPacket(p);
		//}

        float vecDist(LLVector3 pointA, LLVector3 pointB)
        {
            float xd = pointB.X - pointA.X;
            float yd = pointB.Y - pointA.Y;
            float zd = pointB.Z - pointA.Z;
            return (float)Math.Sqrt(xd * xd + yd * yd + zd * zd);
        }
    }
}
