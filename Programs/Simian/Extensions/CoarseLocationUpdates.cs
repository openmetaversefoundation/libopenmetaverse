using OpenMetaverse;
using OpenMetaverse.Packets;
using ExtensionLoader;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Simian.Extensions
{
    public class CoarseLocationUpdates : IExtension<Simian>
    {
        Simian server;
        Timer CoarseLocationTimer;

        public CoarseLocationUpdates()
        {
        }

        public void Start(Simian server)
        {
            this.server = server;

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
            lock (server.Agents)
            {
                foreach (Agent recipient in server.Agents.Values)
                {
                    int i = 0;

                    CoarseLocationUpdatePacket update = new CoarseLocationUpdatePacket();
                    update.Index.Prey = -1;
                    update.Index.You = 0;

                    update.AgentData = new CoarseLocationUpdatePacket.AgentDataBlock[server.Agents.Count];
                    update.Location = new CoarseLocationUpdatePacket.LocationBlock[server.Agents.Count];

                    // Fill in this avatar
                    update.AgentData[0] = new CoarseLocationUpdatePacket.AgentDataBlock();
                    update.AgentData[0].AgentID = recipient.AgentID;
                    update.Location[0] = new CoarseLocationUpdatePacket.LocationBlock();
                    update.Location[0].X = (byte)((int)recipient.Avatar.Position.X);
                    update.Location[0].Y = (byte)((int)recipient.Avatar.Position.Y);
                    update.Location[0].Z = (byte)((int)recipient.Avatar.Position.Z / 4);
                    ++i;
                    
                    foreach (Agent agent in server.Agents.Values)
                    {
                        if (agent != recipient)
                        {
                            update.AgentData[i] = new CoarseLocationUpdatePacket.AgentDataBlock();
                            update.AgentData[i].AgentID = agent.AgentID;
                            update.Location[i] = new CoarseLocationUpdatePacket.LocationBlock();
                            update.Location[i].X = (byte)((int)agent.Avatar.Position.X);
                            update.Location[i].Y = (byte)((int)agent.Avatar.Position.Y);
                            update.Location[i].Z = (byte)((int)agent.Avatar.Position.Z / 4);
                            ++i;
                        }
                    }

                    server.UDP.SendPacket(recipient.AgentID, update, PacketCategory.State);
                }
            }
        }
    }
}
