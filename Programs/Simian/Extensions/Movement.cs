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
        long _LastTick;

        const int UPDATE_ITERATION = 100;

        const float WALK_SPEED = 3f;
        const float RUN_SPEED = 6f;
        const float FLY_SPEED = 12f;

        const float SQRT_TWO = 1.41421356f;

        public int LastTick
        {
            get { return (int) Interlocked.Read(ref _LastTick); }
            set { Interlocked.Exchange(ref _LastTick, value); }
        }

        public Movement(Simian server)
        {
            Server = server;
        }

        public void Start()
        {
            Server.UDPServer.RegisterPacketCallback(PacketType.AgentAnimation, new UDPServer.PacketCallback(AgentAnimationHandler));
            Server.UDPServer.RegisterPacketCallback(PacketType.AgentUpdate, new UDPServer.PacketCallback(AgentUpdateHandler));
            Server.UDPServer.RegisterPacketCallback(PacketType.AgentHeightWidth, new UDPServer.PacketCallback(AgentHeightWidthHandler));
            Server.UDPServer.RegisterPacketCallback(PacketType.SetAlwaysRun, new UDPServer.PacketCallback(SetAlwaysRunHandler));
            Server.UDPServer.RegisterPacketCallback(PacketType.ViewerEffect, new UDPServer.PacketCallback(ViewerEffectHandler));

            UpdateTimer = new Timer(new TimerCallback(UpdateTimer_Elapsed));
            LastTick = Environment.TickCount;
            UpdateTimer.Change(UPDATE_ITERATION, UPDATE_ITERATION);
        }

        public void Stop()
        {
            UpdateTimer.Dispose();
        }

        void UpdateTimer_Elapsed(object sender)
        {
            int tick = Environment.TickCount;
            float seconds = (float)((tick  - LastTick) / 1000f);
            LastTick = tick;

            lock (Server.Agents)
            {
                foreach (Agent agent in Server.Agents.Values)
                {
                    agent.Avatar.Velocity.X = 0f;

                    Matrix4 rotMatrix = Matrix4.CreateFromQuaternion(agent.Avatar.Rotation);
                    Vector3 fwd = Vector3.Transform(Vector3.UnitX, rotMatrix);
                    Vector3 left = Vector3.Transform(Vector3.UnitY, rotMatrix);

                    bool heldForward = (agent.ControlFlags & AgentManager.ControlFlags.AGENT_CONTROL_AT_POS) == AgentManager.ControlFlags.AGENT_CONTROL_AT_POS;
                    bool heldBack = (agent.ControlFlags & AgentManager.ControlFlags.AGENT_CONTROL_AT_NEG) == AgentManager.ControlFlags.AGENT_CONTROL_AT_NEG;
                    bool heldLeft = (agent.ControlFlags & AgentManager.ControlFlags.AGENT_CONTROL_LEFT_POS) == AgentManager.ControlFlags.AGENT_CONTROL_LEFT_POS;
                    bool heldRight = (agent.ControlFlags & AgentManager.ControlFlags.AGENT_CONTROL_LEFT_NEG) == AgentManager.ControlFlags.AGENT_CONTROL_LEFT_NEG;
                    bool heldTurnLeft = (agent.ControlFlags & AgentManager.ControlFlags.AGENT_CONTROL_TURN_LEFT) == AgentManager.ControlFlags.AGENT_CONTROL_TURN_LEFT;
                    bool heldTurnRight = (agent.ControlFlags & AgentManager.ControlFlags.AGENT_CONTROL_TURN_RIGHT) == AgentManager.ControlFlags.AGENT_CONTROL_TURN_RIGHT;
                    bool heldUp = (agent.ControlFlags & AgentManager.ControlFlags.AGENT_CONTROL_UP_POS) == AgentManager.ControlFlags.AGENT_CONTROL_UP_POS;
                    bool heldDown = (agent.ControlFlags & AgentManager.ControlFlags.AGENT_CONTROL_UP_NEG) == AgentManager.ControlFlags.AGENT_CONTROL_UP_NEG;
                    bool flying = (agent.ControlFlags & AgentManager.ControlFlags.AGENT_CONTROL_FLY) == AgentManager.ControlFlags.AGENT_CONTROL_FLY;
                    bool mouselook = (agent.ControlFlags & AgentManager.ControlFlags.AGENT_CONTROL_MOUSELOOK) == AgentManager.ControlFlags.AGENT_CONTROL_MOUSELOOK;

                    float speed = seconds * (flying ? FLY_SPEED : agent.Running ? RUN_SPEED : WALK_SPEED);

                    Vector3 move = Vector3.Zero;

                    if (heldForward) { move.X += fwd.X; move.Y += fwd.Y; }
                    if (heldBack) { move.X -= fwd.X; move.Y -= fwd.Y; }
                    if (heldLeft) { move.X += left.X; move.Y += left.Y; }
                    if (heldRight) { move.X -= left.X; move.Y -= left.Y; }

                    float oldFloor = GetLandHeightAt(agent.Avatar.Position);
                    float newFloor = GetLandHeightAt(agent.Avatar.Position + move);
                    float lowerLimit = newFloor + agent.Avatar.Scale.Z / 2;

                    if ((heldForward || heldBack) && (heldLeft || heldRight))
                        speed /= SQRT_TWO;

                    if (!flying && newFloor != oldFloor) speed /= (1 + (SQRT_TWO * Math.Abs(newFloor - oldFloor)));

                    if (flying)
                    {
                        if (heldUp)
                            agent.Avatar.Position.Z += speed;

                        if (heldDown)
                            agent.Avatar.Position.Z -= speed;
                    }
                    else agent.Avatar.Position.Z = lowerLimit;

                    agent.Avatar.Position.X += move.X * speed;
                    agent.Avatar.Position.Y += move.Y * speed;
                    agent.Avatar.Velocity.X += move.X * speed;
                    agent.Avatar.Velocity.Y += move.Y * speed;

                    if (agent.Avatar.Position.X < 0) agent.Avatar.Position.X = 0f;
                    else if (agent.Avatar.Position.X > 255) agent.Avatar.Position.X = 255f;

                    if (agent.Avatar.Position.Y < 0) agent.Avatar.Position.Y = 0f;
                    else if (agent.Avatar.Position.Y > 255) agent.Avatar.Position.Y = 255f;

                    if (agent.Avatar.Position.Z < lowerLimit) agent.Avatar.Position.Z = lowerLimit;
                }
            }
        }

        void AgentUpdateHandler(Packet packet, Agent agent)
        {
            AgentUpdatePacket update = (AgentUpdatePacket)packet;

            agent.Avatar.Rotation = update.AgentData.BodyRotation;
            agent.ControlFlags = (AgentManager.ControlFlags)update.AgentData.ControlFlags;
            agent.State = update.AgentData.State;
            agent.Flags = (LLObject.ObjectFlags)update.AgentData.Flags;

            lock (Server.Agents)
            {
                ObjectUpdatePacket fullUpdate = BuildFullUpdate(agent, agent.Avatar, Server.RegionHandle,
                    agent.State, agent.Flags);

                foreach (Agent recipient in Server.Agents.Values)
                {
                    recipient.SendPacket(fullUpdate);

                    /*
                    if (agent.Animations.Count == 0) //TODO: need to start default standing animation
                    {
                        agent.Animations.Add(ANIM_STAND);

                        AgentAnimationPacket startAnim = new AgentAnimationPacket();
                        startAnim.AgentData.AgentID = agent.AgentID;
                        startAnim.AnimationList = new AgentAnimationPacket.AnimationListBlock[1];
                        startAnim.AnimationList[0] = new AgentAnimationPacket.AnimationListBlock();
                        startAnim.AnimationList[0].AnimID = ANIM_STAND;
                        startAnim.AnimationList[0].StartAnim = true;
                        startAnim.PhysicalAvatarEventList = new AgentAnimationPacket.PhysicalAvatarEventListBlock[0];

                        recipient.SendPacket(startAnim);
                    }
                    */
                }
            }
        }

        void SetAlwaysRunHandler(Packet packet, Agent agent)
        {
            SetAlwaysRunPacket run = (SetAlwaysRunPacket)packet;

            agent.Running = run.AgentData.AlwaysRun;
        }

        float GetLandHeightAt(Vector3 position)
        {
            int x = (int)position.X;
            int y = (int)position.Y;

            if (x > 255) x = 255;
            else if (x < 0) x = 0;
            if (y > 255) y = 255;
            else if (y < 0) y = 0;

            float center = Server.Heightmap[y * 256 + x];
            float distX = position.X - (int)position.X;
            float distY = position.Y - (int)position.Y;

            float nearestX;
            float nearestY;

            if (distX > 0) nearestX = Server.Heightmap[y * 256 + x + (x < 255 ? 1 : 0)];
            else nearestX = Server.Heightmap[y * 256 + x - (x > 0 ? 1 : 0)];

            if (distY > 0) nearestY = Server.Heightmap[(y + (y < 255 ? 1 : 0)) * 256 + x];
            else nearestY = Server.Heightmap[(y - (y > 0 ? 1 : 0)) * 256 + x];

            float lerpX = Utils.Lerp(center, nearestX, Math.Abs(distX));
            float lerpY = Utils.Lerp(center, nearestY, Math.Abs(distY));

            return ((lerpX + lerpY) / 2);
        }

        void AgentAnimationHandler(Packet packet, Agent agent)
        {
            AgentAnimationPacket anim = (AgentAnimationPacket)packet;
            anim.AgentData.SessionID = UUID.Zero;

            lock (agent.Animations)
            {
                foreach (AgentAnimationPacket.AnimationListBlock block in anim.AnimationList)
                {
                    if (agent.Animations.Contains(block.AnimID))
                    {
                        if (!block.StartAnim) agent.Animations.Remove(block.AnimID);
                    }
                    else if (block.StartAnim) agent.Animations.Add(block.AnimID);
                }
            }

            lock (Server.Agents)
            {
                foreach (Agent recipient in Server.Agents.Values)
                    recipient.SendPacket(anim);
            }
        }

        void AgentHeightWidthHandler(Packet packet, Agent agent)
        {
            AgentHeightWidthPacket heightWidth = (AgentHeightWidthPacket)packet;

            Logger.Log(String.Format("Agent wants to set height={0}, width={1}",
                heightWidth.HeightWidthBlock.Height, heightWidth.HeightWidthBlock.Width), Helpers.LogLevel.Info);
        }

        void ViewerEffectHandler(Packet packet, Agent agent)
        {
            ViewerEffectPacket effect = (ViewerEffectPacket)packet;

            // TODO: Do something with these
        }

        public static ObjectUpdatePacket BuildFullUpdate(Agent agent, LLObject obj, ulong regionHandle, byte state, LLObject.ObjectFlags flags)
        {
            byte[] objectData = new byte[60];
            int pos = 0;
            agent.Avatar.Position.GetBytes().CopyTo(objectData, pos);
            pos += 12;
            agent.Avatar.Velocity.GetBytes().CopyTo(objectData, pos);
            pos += 12;
            agent.Avatar.Acceleration.GetBytes().CopyTo(objectData, pos);
            pos += 12;
            agent.Avatar.Rotation.GetBytes().CopyTo(objectData, pos);
            pos += 12;
            agent.Avatar.AngularVelocity.GetBytes().CopyTo(objectData, pos);

            ObjectUpdatePacket update = new ObjectUpdatePacket();
            update.RegionData.RegionHandle = regionHandle;
            update.RegionData.TimeDilation = Helpers.FloatToByte(1f, 0f, 1f);
            update.ObjectData = new ObjectUpdatePacket.ObjectDataBlock[1];
            update.ObjectData[0] = new ObjectUpdatePacket.ObjectDataBlock();
            update.ObjectData[0].ClickAction = (byte)0;
            update.ObjectData[0].CRC = 0;
            update.ObjectData[0].ExtraParams = new byte[0];
            update.ObjectData[0].Flags = 0;
            update.ObjectData[0].FullID = obj.ID;
            update.ObjectData[0].Gain = 0;
            update.ObjectData[0].ID = obj.LocalID;
            update.ObjectData[0].JointAxisOrAnchor = Vector3.Zero;
            update.ObjectData[0].JointPivot = Vector3.Zero;
            update.ObjectData[0].JointType = (byte)0;
            update.ObjectData[0].Material = (byte)LLObject.MaterialType.Flesh;
            update.ObjectData[0].MediaURL = new byte[0];
            update.ObjectData[0].NameValue = Utils.StringToBytes(NameValue.NameValuesToString(agent.Avatar.NameValues));
            update.ObjectData[0].ObjectData = objectData;
            update.ObjectData[0].OwnerID = UUID.Zero;
            update.ObjectData[0].ParentID = 0;
            update.ObjectData[0].PathBegin = 0;
            update.ObjectData[0].PathCurve = (byte)16;
            update.ObjectData[0].PathEnd = 0;
            update.ObjectData[0].PathRadiusOffset = (sbyte)0;
            update.ObjectData[0].PathRevolutions = (byte)0;
            update.ObjectData[0].PathScaleX = (byte)100;
            update.ObjectData[0].PathScaleY = (byte)100;
            update.ObjectData[0].PathShearX = (byte)0;
            update.ObjectData[0].PathShearY = (byte)0;
            update.ObjectData[0].PathSkew = (sbyte)0;
            update.ObjectData[0].PathTaperX = (sbyte)0;
            update.ObjectData[0].PathTaperY = (sbyte)0;
            update.ObjectData[0].PathTwist = (sbyte)0;
            update.ObjectData[0].PathTwistBegin = (sbyte)0;
            update.ObjectData[0].PCode = (byte)PCode.Avatar;
            update.ObjectData[0].ProfileBegin = 0;
            update.ObjectData[0].ProfileCurve = (byte)1;
            update.ObjectData[0].ProfileEnd = 0;
            update.ObjectData[0].ProfileHollow = 0;
            update.ObjectData[0].PSBlock = new byte[0];
            update.ObjectData[0].TextColor = Vector3.Zero.GetBytes();
            update.ObjectData[0].TextureAnim = new byte[0];
            update.ObjectData[0].TextureEntry = obj.Textures.ToBytes();
            update.ObjectData[0].Radius = 0f;
            update.ObjectData[0].Scale = obj.Scale;
            update.ObjectData[0].Sound = UUID.Zero;
            update.ObjectData[0].State = state;
            update.ObjectData[0].Text = new byte[0];
            update.ObjectData[0].UpdateFlags = (uint)flags;
            update.ObjectData[0].Data = new byte[0];

            return update;
        }
    }
}
