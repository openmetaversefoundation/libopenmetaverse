using OpenMetaverse;
using OpenMetaverse.Packets;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Simian.Extensions
{
    public class Movement : ISimianExtension
    {
        Simian Server;
        Timer UpdateTimer;
        const float SQRT_TWO = 1.41421356f;

        public Movement(Simian server)
        {
            Server = server;
        }

        public void Start()
        {
            Server.UDPServer.RegisterPacketCallback(PacketType.AgentUpdate, new UDPServer.PacketCallback(AgentUpdateHandler));
            if (UpdateTimer != null) UpdateTimer = null;
            UpdateTimer = new Timer(new TimerCallback(UpdateTimer_Elapsed));
            UpdateTimer.Change(100, 100);
        }

        public void Stop()
        {
            UpdateTimer = null;
        }

        void UpdateTimer_Elapsed(object sender)
        {
            lock (Server.Agents)
            {
                foreach (Agent agent in Server.Agents.Values)
                {
                    agent.Avatar.Velocity.X = 0f;

                    Vector3 fwd = Vector3.Transform(Vector3.UnitX, Matrix4.CreateFromQuaternion(agent.Avatar.Rotation));
                    Vector3 left = Vector3.Transform(Vector3.UnitY, Matrix4.CreateFromQuaternion(agent.Avatar.Rotation));

                    bool heldForward = (agent.ControlFlags & AgentManager.ControlFlags.AGENT_CONTROL_AT_POS) == AgentManager.ControlFlags.AGENT_CONTROL_AT_POS;
                    bool heldBack = (agent.ControlFlags & AgentManager.ControlFlags.AGENT_CONTROL_AT_NEG) == AgentManager.ControlFlags.AGENT_CONTROL_AT_NEG;
                    bool heldLeft = (agent.ControlFlags & AgentManager.ControlFlags.AGENT_CONTROL_LEFT_POS) == AgentManager.ControlFlags.AGENT_CONTROL_LEFT_POS;
                    bool heldRight = (agent.ControlFlags & AgentManager.ControlFlags.AGENT_CONTROL_LEFT_NEG) == AgentManager.ControlFlags.AGENT_CONTROL_LEFT_NEG;

                    float speed = 0.5f;
                    if ((heldForward || heldBack) && (heldLeft || heldRight))
                        speed *= SQRT_TWO;

                    if (heldForward)
                    {
                        agent.Avatar.Position.X += fwd.X * speed;
                        agent.Avatar.Position.Y += fwd.Y * speed;
                        agent.Avatar.Velocity.X += fwd.X * speed;
                        agent.Avatar.Velocity.Y += fwd.Y * speed;
                    }
                    if (heldBack)
                    {
                        agent.Avatar.Position.X -= fwd.X * speed;
                        agent.Avatar.Position.Y -= fwd.Y * speed;
                        agent.Avatar.Velocity.X -= fwd.X * speed;
                        agent.Avatar.Velocity.Y -= fwd.Y * speed;
                    }
                    if (heldLeft)
                    {
                        agent.Avatar.Position.X += left.X * speed;
                        agent.Avatar.Position.Y += left.Y * speed;
                        agent.Avatar.Velocity.X += left.X * speed;
                        agent.Avatar.Velocity.Y += left.Y * speed;
                    }
                    if (heldRight)
                    {
                        agent.Avatar.Position.X -= left.X * speed;
                        agent.Avatar.Position.Y -= left.Y * speed;
                        agent.Avatar.Velocity.X -= left.X * speed;
                        agent.Avatar.Velocity.Y -= left.Y * speed;
                    }
                }
            }
        }

        void AgentUpdateHandler(Packet packet, Agent agent)
        {
            AgentUpdatePacket update = (AgentUpdatePacket)packet;

            agent.Avatar.Rotation = update.AgentData.BodyRotation;
            agent.ControlFlags = (AgentManager.ControlFlags)update.AgentData.ControlFlags;
        }

    }
}
