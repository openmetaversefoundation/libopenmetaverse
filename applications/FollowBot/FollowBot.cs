using System;
using System.Threading;
using System.Collections.Generic;
using libsecondlife;
using libsecondlife.Packets;

namespace FollowBot
{
    class FollowBot
    {
        SecondLife client;
        Dictionary<uint, Avatar> avatars;
        static bool logout;
        bool flying;
        const float DISTANCE_BUFFER = 3.0f;
        string followName;
        int regionX;
        int regionY;

        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Usage: FollowBot <firstName> <lastName> <password>");
            }
            else
            {
                FollowBot bot = new FollowBot(args[0], args[1], args[2]);
            }
        }

        public FollowBot(string firstName, string lastName, string password)
        {
            client = new SecondLife();
            avatars = new Dictionary<uint, Avatar>();
            client.Objects.OnNewAvatar += new ObjectManager.NewAvatarCallback(OnNewAvatarEvent);
            client.Objects.OnAvatarMoved += new ObjectManager.AvatarMovedCallback(OnAvatarMovedEvent);
            client.Objects.OnObjectKilled += new ObjectManager.KillObjectCallback(OnObjectKilledEvent);
            client.Network.Login(firstName, lastName, password, "FollowBot", "root66@gmail.com");
            if (flying) SendAgentUpdate((uint)Avatar.AgentUpdateFlags.AGENT_CONTROL_FLY);
            else SendAgentUpdate(0);
            while (!logout) ParseCommand(Console.ReadLine());
            client.Network.Logout();
        }

        //EVENTS #####################################################
        void OnNewAvatarEvent(Simulator simulator, Avatar avatar, ulong regionHandle, ushort timeDilation)
        {
            lock (avatars)
            {
                avatars[avatar.LocalID] = avatar;
            }
        }

        void OnAvatarMovedEvent(Simulator simulator, AvatarUpdate avatar, ulong regionHandle, ushort timeDilation)
        {
            Avatar test;
            if (!avatars.TryGetValue(avatar.LocalID, out test)) return;
            regionX = (int)(regionHandle >> 32);
            regionY = (int)(regionHandle & 0xFFFFFFFF);
            lock (avatars)
            {
                string name = avatars[avatar.LocalID].Name;
                if (avatars[avatar.LocalID].Name == followName)
                {
                    avatars[avatar.LocalID].Position = avatar.Position;
                    avatars[avatar.LocalID].Rotation = avatar.Rotation;
                    if (!Follow(name))
                    {
                        if (flying) SendAgentUpdate((uint)Avatar.AgentUpdateFlags.AGENT_CONTROL_FLY);
                        else SendAgentUpdate(0);
                    }
                }
            }
        }

        void OnObjectKilledEvent(Simulator simulator, uint objectID)
        {
            lock (avatars)
            {
                if (avatars.ContainsKey(objectID)) avatars.Remove(objectID);
            }
        }

        //FUNCTIONS ##########################################################
        void ParseCommand(string message)
        {
            if (message.Length == 0) return;
            char[] splitChar = { ' ' };
            string[] msg = message.Split(splitChar);
            string command = msg[0].ToLower();
            string response = "";

            //Store command arguments in "details" variable
            string details = null;
            for (int i = 1; i < msg.Length; i++)
            {
                details += msg[i];
                if (i + 1 < msg.Length) details += " ";
            }

            switch (command)
            {
                case "fly":
                    {
                        flying = true;
                        SendAgentUpdate((uint)Avatar.AgentUpdateFlags.AGENT_CONTROL_FLY);
                        break;
                    }
                case "land":
                    {
                        flying = false;
                        SendAgentUpdate(0);
                        break;
                    }
                case "follow":
                    {
                        followName = details;
                        if (flying) SendAgentUpdate((uint)Avatar.AgentUpdateFlags.AGENT_CONTROL_FLY);
                        else SendAgentUpdate(0);
                        response = "Following " + details;
                        break;
                    }
                case "quit":
                    {
                        logout = true;
                        response = "Logging out";
                        break;
                    }
            }

            if (response != "") Console.WriteLine("* " + response);
        }

        float vecDist(LLVector3 pointA, LLVector3 pointB)
        {
            float xd = pointB.X - pointA.X;
            float yd = pointB.Y - pointA.Y;
            float zd = pointB.Z - pointA.Z;
            return (float)Math.Sqrt(xd * xd + yd * yd + zd * zd);
        }

 
        bool Follow(string name)
        {
            followName = name;
            foreach (Avatar av in avatars.Values)
            {
                if (av.Name != name) continue;
                else if (vecDist(av.Position, client.Self.Position) > DISTANCE_BUFFER)
                {
                    //move toward target
                    if (av.Position.Z > client.Self.Position.Z + DISTANCE_BUFFER) flying = true;
                    else flying = false;
                    ulong x = (ulong)(av.Position.X + regionX);
                    ulong y = (ulong)(av.Position.Y + regionY);
                    client.Self.AutoPilot(x, y, av.Position.Z);
                }
                else
                {
                    //stop at current position
                    //LLVector3 myPos = client.Self.Position;
                    //client.Self.AutoPilot((ulong)myPos.X, (ulong)myPos.Y, myPos.Z);
                }
                Thread.Sleep(200); //Sleep 200ms between updates
                if (flying) SendAgentUpdate((uint)Avatar.AgentUpdateFlags.AGENT_CONTROL_FLY);
                else SendAgentUpdate(0);
                return true;
            }
            return false;
        }

        void SendAgentUpdate(uint ControlID)
        {
            AgentUpdatePacket p = new AgentUpdatePacket();
            p.AgentData.Far = 30.0f;
            p.AgentData.CameraAtAxis = new LLVector3(0, 0, 0);
            p.AgentData.CameraCenter = new LLVector3(0, 0, 0);
            p.AgentData.CameraLeftAxis = new LLVector3(0, 0, 0);
            p.AgentData.CameraUpAxis = new LLVector3(0, 0, 0);
            p.AgentData.HeadRotation = new LLQuaternion(0, 0, 0, 1); ;
            p.AgentData.BodyRotation = new LLQuaternion(0, 0, 0, 1); ;
            p.AgentData.AgentID = client.Network.AgentID;
            p.AgentData.SessionID = client.Network.SessionID;
            p.AgentData.ControlFlags = ControlID;
            client.Network.SendPacket(p);
        }

    }
}
