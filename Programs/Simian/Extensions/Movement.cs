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
        const int UPDATE_ITERATION = 100; //rate in milliseconds to send ObjectUpdate
        const bool ENVIRONMENT_SOUNDS = true; //collision sounds, splashing, etc
        const float GRAVITY = 9.8f; //meters/sec
        const float WALK_SPEED = 3f; //meters/sec
        const float RUN_SPEED = 5f; //meters/sec
        const float FLY_SPEED = 10f; //meters/sec
        const float FALL_DELAY = 0.33f; //seconds before starting animation
        const float FALL_FORGIVENESS = 0.25f; //fall buffer in meters
        const float JUMP_IMPULSE_VERTICAL = 8.5f; //boost amount in meters/sec
        const float JUMP_IMPULSE_HORIZONTAL = 10f; //boost amount in meters/sec (no clue why this is so high) 
        const float PREJUMP_DELAY = 0.25f; //seconds before actually jumping
        const float AVATAR_TERMINAL_VELOCITY = 54f; //~120mph

        const float SQRT_TWO = 1.41421356f;

        Simian server;
        Timer updateTimer;
        long lastTick;

        public int LastTick
        {
            get { return (int) Interlocked.Read(ref lastTick); }
            set { Interlocked.Exchange(ref lastTick, value); }
        }

        public Movement(Simian server)
        {
            this.server = server;
        }

        public void Start()
        {
            server.UDP.RegisterPacketCallback(PacketType.AgentUpdate, new PacketCallback(AgentUpdateHandler));
            server.UDP.RegisterPacketCallback(PacketType.AgentHeightWidth, new PacketCallback(AgentHeightWidthHandler));
            server.UDP.RegisterPacketCallback(PacketType.SetAlwaysRun, new PacketCallback(SetAlwaysRunHandler));

            updateTimer = new Timer(new TimerCallback(UpdateTimer_Elapsed));
            LastTick = Environment.TickCount;
            updateTimer.Change(UPDATE_ITERATION, UPDATE_ITERATION);
        }

        public void Stop()
        {
            updateTimer.Dispose();
        }

        void UpdateTimer_Elapsed(object sender)
        {
            int tick = Environment.TickCount;
            float seconds = (float)((tick  - LastTick) / 1000f);
            LastTick = tick;

            lock (server.Agents)
            {
                foreach (Agent agent in server.Agents.Values)
                {
                    bool animsChanged = false;

                    // Create forward and left vectors from the current avatar rotation
                    Matrix4 rotMatrix = Matrix4.CreateFromQuaternion(agent.Avatar.Rotation);
                    Vector3 fwd = Vector3.Transform(Vector3.UnitX, rotMatrix);
                    Vector3 left = Vector3.Transform(Vector3.UnitY, rotMatrix);

                    // Check control flags
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

                    // direction in which the avatar is trying to move
                    Vector3 move = Vector3.Zero;
                    if (heldForward) { move.X += fwd.X; move.Y += fwd.Y; }
                    if (heldBack) { move.X -= fwd.X; move.Y -= fwd.Y; }
                    if (heldLeft) { move.X += left.X; move.Y += left.Y; }
                    if (heldRight) { move.X -= left.X; move.Y -= left.Y; }
                    if (heldUp) { move.Z += 1; }
                    if (heldDown) { move.Z -= 1; }

                    // is the avatar trying to move?
                    bool moving = move != Vector3.Zero;
                    bool jumping = agent.TickJump > 0;

                    // 2-dimensional speed multipler
                    float speed = seconds * (flying ? FLY_SPEED : agent.Running && !jumping ? RUN_SPEED : WALK_SPEED);
                    if ((heldForward || heldBack) && (heldLeft || heldRight))
                        speed /= SQRT_TWO;

                    // adjust multiplier for Z dimension
                    float oldFloor = GetLandHeightAt(agent.Avatar.Position);
                    float newFloor = GetLandHeightAt(agent.Avatar.Position + (move * speed));
                    if (!flying && newFloor != oldFloor)
                        speed /= (1 + (SQRT_TWO * Math.Abs(newFloor - oldFloor)));

                    // least possible distance from avatar to the ground
                    // TODO: calculate to get rid of "bot squat"
                    float lowerLimit = newFloor + agent.Avatar.Scale.Z / 2;

                    // Z acceleration resulting from gravity
                    float gravity = 0f;

                    float waterChestHeight = server.WaterHeight - (agent.Avatar.Scale.Z * .33f);

                    if (flying)
                    {
                        agent.TickFall = 0;
                        agent.TickJump = 0;

                        //velocity falloff while flying
                        agent.Avatar.Velocity.X *= 0.66f;
                        agent.Avatar.Velocity.Y *= 0.66f;
                        agent.Avatar.Velocity.Z *= 0.33f;

                        if (move.X != 0 || move.Y != 0)
                        { //flying horizontally
                            if (server.Avatars.SetDefaultAnimation(agent, Animations.FLY))
                                animsChanged = true;
                        }
                        else if (move.Z > 0)
                        { //flying straight up
                            if (server.Avatars.SetDefaultAnimation(agent, Animations.HOVER_UP))
                                animsChanged = true;
                        }
                        else if (move.Z < 0)
                        { //flying straight down
                            if (server.Avatars.SetDefaultAnimation(agent, Animations.HOVER_DOWN))
                                animsChanged = true;
                        }
                        else
                        { //hovering in the air
                            if (server.Avatars.SetDefaultAnimation(agent, Animations.HOVER))
                                animsChanged = true;
                        }
                    }

                    else if (agent.Avatar.Position.Z > lowerLimit + FALL_FORGIVENESS || agent.Avatar.Position.Z <= waterChestHeight)
                    { //falling or landing from a jump

                        if (agent.Avatar.Position.Z > server.WaterHeight)
                        { //above water

                            move = Vector3.Zero; //override controls while drifting
                            agent.Avatar.Velocity *= 0.95f; //keep most of our inertia

                            float fallElapsed = (float)(Environment.TickCount - agent.TickFall) / 1000f;

                            if (agent.TickFall == 0 || (fallElapsed > FALL_DELAY && agent.Avatar.Velocity.Z >= 0f))
                            { //just started falling
                                agent.TickFall = Environment.TickCount;
                            }
                            else
                            {
                                gravity = GRAVITY * fallElapsed * seconds; //normal gravity

                                if (!jumping)
                                { //falling
                                    if (fallElapsed > FALL_DELAY)
                                    { //falling long enough to trigger the animation
                                        if (server.Avatars.SetDefaultAnimation(agent, Animations.FALLDOWN))
                                            animsChanged = true;
                                    }
                                }
                            }
                        }
                        else if (agent.Avatar.Position.Z >= waterChestHeight)
                        { //at the water line

                            gravity = 0f;
                            agent.Avatar.Velocity *= 0.5f;
                            agent.Avatar.Velocity.Z = 0f;
                            if (move.Z < 1) agent.Avatar.Position.Z = waterChestHeight;

                            if (move.Z > 0)
                            {
                                if (server.Avatars.SetDefaultAnimation(agent, Animations.HOVER_UP))
                                    animsChanged = true;
                            }
                            else if (move.X != 0 || move.Y != 0)
                            {
                                if (server.Avatars.SetDefaultAnimation(agent, Animations.FLYSLOW))
                                    animsChanged = true;
                            }
                            else
                            {
                                if (server.Avatars.SetDefaultAnimation(agent, Animations.HOVER))
                                    animsChanged = true;
                            }
                        }
                        else
                        { //underwater

                            gravity = 0f; //buoyant
                            agent.Avatar.Velocity *= 0.5f * seconds;
                            agent.Avatar.Velocity.Z += 1.0f * seconds;

                            if (server.Avatars.SetDefaultAnimation(agent, Animations.FALLDOWN))
                                animsChanged = true;
                        }
                    }
                    else
                    { //on the ground

                        agent.TickFall = 0;

                        //friction
                        agent.Avatar.Acceleration *= 0.2f;
                        agent.Avatar.Velocity *= 0.2f;                        

                        agent.Avatar.Position.Z = lowerLimit;

                        if (move.Z > 0)
                        { //jumping
                            if (!jumping)
                            { //begin prejump
                                move.Z = 0; //override Z control
                                if (server.Avatars.SetDefaultAnimation(agent, Animations.PRE_JUMP))
                                    animsChanged = true;

                                agent.TickJump = Environment.TickCount;                                
                            }
                            else if (Environment.TickCount - agent.TickJump > PREJUMP_DELAY * 1000)
                            { //start actual jump
                                if (server.Avatars.SetDefaultAnimation(agent, Animations.JUMP))
                                    animsChanged = true;

                                agent.Avatar.Velocity.Z = JUMP_IMPULSE_VERTICAL * seconds;
                                agent.Avatar.Velocity.X += agent.Avatar.Acceleration.X * JUMP_IMPULSE_HORIZONTAL;
                                agent.Avatar.Velocity.Y += agent.Avatar.Acceleration.Y * JUMP_IMPULSE_HORIZONTAL;
                            }
                            else move.Z = 0; //override Z control
                        }

                        else
                        { //not jumping

                            agent.TickJump = 0;

                            if (move.X != 0 || move.Y != 0)
                            { //not walking

                                if (move.Z < 0)
                                { //crouchwalking
                                    if (server.Avatars.SetDefaultAnimation(agent, Animations.CROUCHWALK))
                                        animsChanged = true;
                                }
                                else if (agent.Running)
                                { //running
                                    if (server.Avatars.SetDefaultAnimation(agent, Animations.RUN))
                                        animsChanged = true;
                                }
                                else
                                { //walking
                                    if (server.Avatars.SetDefaultAnimation(agent, Animations.WALK))
                                        animsChanged = true;
                                }
                            }
                            else
                            { //walking
                                if (move.Z < 0)
                                { //crouching
                                    if (server.Avatars.SetDefaultAnimation(agent, Animations.CROUCH))
                                        animsChanged = true;
                                }
                                else
                                { //standing
                                    if (server.Avatars.SetDefaultAnimation(agent, Animations.STAND))
                                        animsChanged = true;
                                }
                            }
                        }
                    }

                    if (animsChanged)
                        server.Avatars.SendAnimations(agent);

                    // static acceleration when any control is held, otherwise none
                    if (moving) agent.Avatar.Acceleration = move * speed; //FIXME
                    else agent.Avatar.Acceleration  = Vector3.Zero;

                    float maxVel = AVATAR_TERMINAL_VELOCITY * seconds;
                    if (gravity > maxVel) gravity = maxVel;
                    agent.Avatar.Velocity += agent.Avatar.Acceleration - new Vector3(0f, 0f, gravity); 

                    agent.Avatar.Position += agent.Avatar.Velocity;

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
            agent.Flags = (PrimFlags)update.AgentData.Flags;

            ObjectUpdatePacket fullUpdate = BuildFullUpdate(agent.Avatar,
                NameValue.NameValuesToString(agent.Avatar.NameValues), server.RegionHandle,
                agent.State, agent.Flags);

            server.UDP.BroadcastPacket(fullUpdate, PacketCategory.State);
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

            float center = server.Heightmap[y * 256 + x];
            float distX = position.X - (int)position.X;
            float distY = position.Y - (int)position.Y;

            float nearestX;
            float nearestY;

            if (distX > 0) nearestX = server.Heightmap[y * 256 + x + (x < 255 ? 1 : 0)];
            else nearestX = server.Heightmap[y * 256 + x - (x > 0 ? 1 : 0)];

            if (distY > 0) nearestY = server.Heightmap[(y + (y < 255 ? 1 : 0)) * 256 + x];
            else nearestY = server.Heightmap[(y - (y > 0 ? 1 : 0)) * 256 + x];

            float lerpX = Utils.Lerp(center, nearestX, Math.Abs(distX));
            float lerpY = Utils.Lerp(center, nearestY, Math.Abs(distY));

            return ((lerpX + lerpY) / 2);
        }

        void AgentHeightWidthHandler(Packet packet, Agent agent)
        {
            AgentHeightWidthPacket heightWidth = (AgentHeightWidthPacket)packet;

            Logger.Log(String.Format("Agent wants to set height={0}, width={1}",
                heightWidth.HeightWidthBlock.Height, heightWidth.HeightWidthBlock.Width), Helpers.LogLevel.Info);
        }

        public static ObjectUpdatePacket BuildFullUpdate(Primitive obj, string nameValues, ulong regionHandle,
            byte state, PrimFlags flags)
        {
            byte[] objectData = new byte[60];
            int pos = 0;
            obj.Position.GetBytes().CopyTo(objectData, pos);
            pos += 12;
            obj.Velocity.GetBytes().CopyTo(objectData, pos);
            pos += 12;
            obj.Acceleration.GetBytes().CopyTo(objectData, pos);
            pos += 12;
            obj.Rotation.GetBytes().CopyTo(objectData, pos);
            pos += 12;
            obj.AngularVelocity.GetBytes().CopyTo(objectData, pos);

            ObjectUpdatePacket update = new ObjectUpdatePacket();
            update.RegionData.RegionHandle = regionHandle;
            update.RegionData.TimeDilation = UInt16.MaxValue;
            update.ObjectData = new ObjectUpdatePacket.ObjectDataBlock[1];
            update.ObjectData[0] = new ObjectUpdatePacket.ObjectDataBlock();
            update.ObjectData[0].ClickAction = (byte)obj.ClickAction;
            update.ObjectData[0].CRC = 0;
            update.ObjectData[0].ExtraParams = new byte[0]; //FIXME: Need a serializer for ExtraParams
            update.ObjectData[0].Flags = (byte)flags;
            update.ObjectData[0].FullID = obj.ID;
            update.ObjectData[0].Gain = obj.SoundGain;
            update.ObjectData[0].ID = obj.LocalID;
            update.ObjectData[0].JointAxisOrAnchor = obj.JointAxisOrAnchor;
            update.ObjectData[0].JointPivot = obj.JointPivot;
            update.ObjectData[0].JointType = (byte)obj.Joint;
            update.ObjectData[0].Material = (byte)obj.PrimData.Material;
            update.ObjectData[0].MediaURL = new byte[0]; // FIXME:
            update.ObjectData[0].NameValue = Utils.StringToBytes(nameValues);
            update.ObjectData[0].ObjectData = objectData;
            update.ObjectData[0].OwnerID = obj.Properties.OwnerID;
            update.ObjectData[0].ParentID = obj.ParentID;
            update.ObjectData[0].PathBegin = Primitive.PackBeginCut(obj.PrimData.PathBegin);
            update.ObjectData[0].PathCurve = (byte)obj.PrimData.PathCurve;
            update.ObjectData[0].PathEnd = Primitive.PackEndCut(obj.PrimData.PathEnd);
            update.ObjectData[0].PathRadiusOffset = Primitive.PackPathTwist(obj.PrimData.PathRadiusOffset);
            update.ObjectData[0].PathRevolutions = Primitive.PackPathRevolutions(obj.PrimData.PathRevolutions);
            update.ObjectData[0].PathScaleX = Primitive.PackPathScale(obj.PrimData.PathScaleX);
            update.ObjectData[0].PathScaleY = Primitive.PackPathScale(obj.PrimData.PathScaleY);
            update.ObjectData[0].PathShearX = (byte)Primitive.PackPathShear(obj.PrimData.PathShearX);
            update.ObjectData[0].PathShearY = (byte)Primitive.PackPathShear(obj.PrimData.PathShearY);
            update.ObjectData[0].PathSkew = Primitive.PackPathTwist(obj.PrimData.PathSkew);
            update.ObjectData[0].PathTaperX = Primitive.PackPathTaper(obj.PrimData.PathTaperX);
            update.ObjectData[0].PathTaperY = Primitive.PackPathTaper(obj.PrimData.PathTaperY);
            update.ObjectData[0].PathTwist = Primitive.PackPathTwist(obj.PrimData.PathTwist);
            update.ObjectData[0].PathTwistBegin = Primitive.PackPathTwist(obj.PrimData.PathTwistBegin);
            update.ObjectData[0].PCode = (byte)obj.PrimData.PCode;
            update.ObjectData[0].ProfileBegin = Primitive.PackBeginCut(obj.PrimData.ProfileBegin);
            update.ObjectData[0].ProfileCurve = (byte)obj.PrimData.ProfileCurve;
            update.ObjectData[0].ProfileEnd = Primitive.PackEndCut(obj.PrimData.ProfileEnd);
            update.ObjectData[0].ProfileHollow = Primitive.PackProfileHollow(obj.PrimData.ProfileHollow);
            update.ObjectData[0].PSBlock = new byte[0]; // FIXME:
            update.ObjectData[0].TextColor = obj.TextColor.GetBytes(true);
            update.ObjectData[0].TextureAnim = obj.TextureAnim.GetBytes();
            update.ObjectData[0].TextureEntry = obj.Textures.ToBytes();
            update.ObjectData[0].Radius = obj.SoundRadius;
            update.ObjectData[0].Scale = obj.Scale;
            update.ObjectData[0].Sound = obj.Sound;
            update.ObjectData[0].State = state;
            update.ObjectData[0].Text = Utils.StringToBytes(obj.Text);
            update.ObjectData[0].UpdateFlags = (uint)flags;
            update.ObjectData[0].Data = new byte[0]; // FIXME:

            return update;
        }
    }
}
