using OpenMetaverse;
using OpenMetaverse.Packets;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Simian.Extensions
{
    public class CoarseLocationUpdates : ISimianExtension
    {
        Simian Server;
        Timer CoarseLocationTimer;

        public CoarseLocationUpdates(Simian server)
        {
            Server = server;
        }

        public void Start()
        {
            if (CoarseLocationTimer != null) CoarseLocationTimer = null;
            CoarseLocationTimer = new Timer(new TimerCallback(CoarseLocationTimer_Elapsed));
            CoarseLocationTimer.Change(1000, 1000);
        }

        public void Stop()
        {
            CoarseLocationTimer = null;
        }

        void CoarseLocationTimer_Elapsed(object sender)
        {
            lock (Server.Agents)
            {
                List<Vector3> avatarPositions = new List<Vector3>();

                CoarseLocationUpdatePacket update = new CoarseLocationUpdatePacket();
                update.AgentData = new CoarseLocationUpdatePacket.AgentDataBlock[Server.Agents.Count];
                update.Location = new CoarseLocationUpdatePacket.LocationBlock[Server.Agents.Count];

                short i = 0;
                foreach (Agent agent in Server.Agents.Values)
                {
                    update.AgentData[i] = new CoarseLocationUpdatePacket.AgentDataBlock();
                    update.AgentData[i].AgentID = agent.AgentID;
                    update.Location[i] = new CoarseLocationUpdatePacket.LocationBlock();
                    update.Location[i].X = (byte)((int)agent.Avatar.Position.X);
                    update.Location[i].Y = (byte)((int)agent.Avatar.Position.Y);
                    update.Location[i].Z = (byte)((int)agent.Avatar.Position.Z / 4);
                    update.Index.Prey = -1;
                    i++;
                }

                i = 0;
                foreach (Agent recipient in Server.Agents.Values)
                {
                    update.Index.You = i;
                    recipient.SendPacket(update);
                    i++;
                }

            }
        }
    }
}
