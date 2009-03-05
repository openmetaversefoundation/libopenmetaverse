using System;
using System.Collections.Generic;
using System.Threading;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace Simian.Extensions
{
    public class Movement : IExtension<Simian>
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
        const float JUMP_IMPULSE_HORIZONTAL = 10f; //boost amount in meters/sec
        const float INITIAL_HOVER_IMPULSE = 2f; //boost amount in meters/sec
        const float PREJUMP_DELAY = 0.25f; //seconds before actually jumping
        const float AVATAR_TERMINAL_VELOCITY = 54f; //~120mph

        static readonly UUID BIG_SPLASH_SOUND = new UUID("486475b9-1460-4969-871e-fad973b38015");
        static readonly Vector3 SEATING_FUDGE = new Vector3(0.3f, 0.0f, 0.0f);

        const float SQRT_TWO = 1.41421356f;

        Simian server;
        Timer updateTimer;
        long lastTick;

        public int LastTick
        {
            get { return (int) Interlocked.Read(ref lastTick); }
            set { Interlocked.Exchange(ref lastTick, value); }
        }

        public Movement()
        {
        }

        public void Start(Simian server)
        {
            this.server = server;

            server.UDP.RegisterPacketCallback(PacketType.AgentRequestSit, AgentRequestSitHandler);
            server.UDP.RegisterPacketCallback(PacketType.AgentSit, AgentSitHandler);
            server.UDP.RegisterPacketCallback(PacketType.AgentUpdate, AgentUpdateHandler);
            server.UDP.RegisterPacketCallback(PacketType.SetAlwaysRun, SetAlwaysRunHandler);

            updateTimer = new Timer(new TimerCallback(UpdateTimer_Elapsed));
            LastTick = Environment.TickCount;
            updateTimer.Change(UPDATE_ITERATION, UPDATE_ITERATION);
        }

        public void Stop()
        {
            if (updateTimer != null)
            {
                updateTimer.Dispose();
                updateTimer = null;
            }
        }

        void UpdateTimer_Elapsed(object sender)
        {
            int tick = Environment.TickCount;
            float seconds = (float)((tick  - LastTick) / 1000f);
            LastTick = tick;

            server.Scene.ForEachAgent(
                delegate(Agent agent)
                {
                    if ((agent.Avatar.Prim.Flags & PrimFlags.Physics) == 0)
                        return;

                    bool animsChanged = false;

                    // Create forward and left vectors from the current avatar rotation
                    Matrix4 rotMatrix = Matrix4.CreateFromQuaternion(agent.Avatar.Prim.Rotation);
                    Vector3 fwd = Vector3.Transform(Vector3.UnitX, rotMatrix);
                    Vector3 left = Vector3.Transform(Vector3.UnitY, rotMatrix);

                    // Check control flags
                    bool heldForward = (agent.ControlFlags & AgentManager.ControlFlags.AGENT_CONTROL_AT_POS) == AgentManager.ControlFlags.AGENT_CONTROL_AT_POS;
                    bool heldBack = (agent.ControlFlags & AgentManager.ControlFlags.AGENT_CONTROL_AT_NEG) == AgentManager.ControlFlags.AGENT_CONTROL_AT_NEG;
                    bool heldLeft = (agent.ControlFlags & AgentManager.ControlFlags.AGENT_CONTROL_LEFT_POS) == AgentManager.ControlFlags.AGENT_CONTROL_LEFT_POS;
                    bool heldRight = (agent.ControlFlags & AgentManager.ControlFlags.AGENT_CONTROL_LEFT_NEG) == AgentManager.ControlFlags.AGENT_CONTROL_LEFT_NEG;
                    //bool heldTurnLeft = (agent.ControlFlags & AgentManager.ControlFlags.AGENT_CONTROL_TURN_LEFT) == AgentManager.ControlFlags.AGENT_CONTROL_TURN_LEFT;
                    //bool heldTurnRight = (agent.ControlFlags & AgentManager.ControlFlags.AGENT_CONTROL_TURN_RIGHT) == AgentManager.ControlFlags.AGENT_CONTROL_TURN_RIGHT;
                    bool heldUp = (agent.ControlFlags & AgentManager.ControlFlags.AGENT_CONTROL_UP_POS) == AgentManager.ControlFlags.AGENT_CONTROL_UP_POS;
                    bool heldDown = (agent.ControlFlags & AgentManager.ControlFlags.AGENT_CONTROL_UP_NEG) == AgentManager.ControlFlags.AGENT_CONTROL_UP_NEG;
                    bool flying = (agent.ControlFlags & AgentManager.ControlFlags.AGENT_CONTROL_FLY) == AgentManager.ControlFlags.AGENT_CONTROL_FLY;
                    //bool mouselook = (agent.ControlFlags & AgentManager.ControlFlags.AGENT_CONTROL_MOUSELOOK) == AgentManager.ControlFlags.AGENT_CONTROL_MOUSELOOK;

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
                    bool jumping = agent.TickJump != 0;

                    // 2-dimensional speed multipler
                    float speed = seconds * (flying ? FLY_SPEED : agent.Running && !jumping ? RUN_SPEED : WALK_SPEED);
                    if ((heldForward || heldBack) && (heldLeft || heldRight))
                        speed /= SQRT_TWO;

                    Vector3 agentPosition = agent.Avatar.GetSimulatorPosition();
                    float oldFloor = server.Scene.GetTerrainHeightAt(agentPosition.X, agentPosition.Y);

                    agentPosition += (move * speed);
                    float newFloor = server.Scene.GetTerrainHeightAt(agentPosition.X, agentPosition.Y);

                    if (!flying && newFloor != oldFloor)
                        speed /= (1 + (SQRT_TWO * Math.Abs(newFloor - oldFloor)));

                    // least possible distance from avatar to the ground
                    // TODO: calculate to get rid of "bot squat"
                    float lowerLimit = newFloor + agent.Avatar.Prim.Scale.Z / 2;

                    // TODO: check our Z (minus height/2) compared to all the prim
                    // bounding boxes, and define a new lowerLimit as a hack for platforms

                    // Z acceleration resulting from gravity
                    float gravity = 0f;

                    float waterChestHeight = server.Scene.WaterHeight - (agent.Avatar.Prim.Scale.Z * .33f);

                    if (flying)
                    {
                        agent.TickFall = 0;
                        agent.TickJump = 0;

                        //velocity falloff while flying
                        agent.Avatar.Prim.Velocity.X *= 0.66f;
                        agent.Avatar.Prim.Velocity.Y *= 0.66f;
                        agent.Avatar.Prim.Velocity.Z *= 0.33f;

                        if (agent.Avatar.Prim.Position.Z == lowerLimit)
                            agent.Avatar.Prim.Velocity.Z += INITIAL_HOVER_IMPULSE;

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

                    else if (agent.Avatar.Prim.Position.Z > lowerLimit + FALL_FORGIVENESS || agent.Avatar.Prim.Position.Z <= waterChestHeight)
                    { //falling, floating, or landing from a jump

                        if (agent.Avatar.Prim.Position.Z > server.Scene.WaterHeight)
                        { //above water

                            move = Vector3.Zero; //override controls while drifting
                            agent.Avatar.Prim.Velocity *= 0.95f; //keep most of our inertia

                            float fallElapsed = (float)(Environment.TickCount - agent.TickFall) / 1000f;

                            if (agent.TickFall == 0 || (fallElapsed > FALL_DELAY && agent.Avatar.Prim.Velocity.Z >= 0f))
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
                        else if (agent.Avatar.Prim.Position.Z >= waterChestHeight)
                        { //at the water line

                            gravity = 0f;
                            agent.Avatar.Prim.Velocity *= 0.5f;
                            agent.Avatar.Prim.Velocity.Z = 0f;
                            if (move.Z < 1) agent.Avatar.Prim.Position.Z = waterChestHeight;

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
                            agent.Avatar.Prim.Velocity *= 0.5f * seconds;
                            agent.Avatar.Prim.Velocity.Z += 0.75f * seconds;

                            if (server.Avatars.SetDefaultAnimation(agent, Animations.FALLDOWN))
                                animsChanged = true;
                        }
                    }
                    else
                    { //on the ground

                        agent.TickFall = 0;

                        //friction
                        agent.Avatar.Prim.Acceleration *= 0.2f;
                        agent.Avatar.Prim.Velocity *= 0.2f;

                        agent.Avatar.Prim.Position.Z = lowerLimit;

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

                                if (agent.TickJump == -1)
                                {
                                    //already jumping! end current jump
                                    agent.TickJump = 0;
                                    return;
                                }

                                if (server.Avatars.SetDefaultAnimation(agent, Animations.JUMP))
                                    animsChanged = true;

                                agent.Avatar.Prim.Velocity.X += agent.Avatar.Prim.Acceleration.X * JUMP_IMPULSE_HORIZONTAL;
                                agent.Avatar.Prim.Velocity.Y += agent.Avatar.Prim.Acceleration.Y * JUMP_IMPULSE_HORIZONTAL;
                                agent.Avatar.Prim.Velocity.Z = JUMP_IMPULSE_VERTICAL * seconds;

                                agent.TickJump = -1; //flag that we are currently jumping
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

                    float maxVel = AVATAR_TERMINAL_VELOCITY * seconds;

                    // static acceleration when any control is held, otherwise none
                    if (moving)
                    {
                        agent.Avatar.Prim.Acceleration = move * speed;
                        if (agent.Avatar.Prim.Acceleration.Z < -maxVel)
                            agent.Avatar.Prim.Acceleration.Z = -maxVel;
                        else if (agent.Avatar.Prim.Acceleration.Z > maxVel)
                            agent.Avatar.Prim.Acceleration.Z = maxVel;
                    }
                    else agent.Avatar.Prim.Acceleration = Vector3.Zero;

                    agent.Avatar.Prim.Velocity += agent.Avatar.Prim.Acceleration - new Vector3(0f, 0f, gravity);
                    if (agent.Avatar.Prim.Velocity.Z < -maxVel)
                        agent.Avatar.Prim.Velocity.Z = -maxVel;
                    else if (agent.Avatar.Prim.Velocity.Z > maxVel)
                        agent.Avatar.Prim.Velocity.Z = maxVel;

                    agent.Avatar.Prim.Position += agent.Avatar.Prim.Velocity;

                    if (agent.Avatar.Prim.Position.X < 0) agent.Avatar.Prim.Position.X = 0f;
                    else if (agent.Avatar.Prim.Position.X > 255) agent.Avatar.Prim.Position.X = 255f;

                    if (agent.Avatar.Prim.Position.Y < 0) agent.Avatar.Prim.Position.Y = 0f;
                    else if (agent.Avatar.Prim.Position.Y > 255) agent.Avatar.Prim.Position.Y = 255f;

                    if (agent.Avatar.Prim.Position.Z < lowerLimit) agent.Avatar.Prim.Position.Z = lowerLimit;
                }
            );
        }

        void AgentRequestSitHandler(Packet packet, Agent agent)
        {
            AgentRequestSitPacket request = (AgentRequestSitPacket)packet;

            SimulationObject obj;
            if (server.Scene.TryGetObject(request.TargetObject.TargetID, out obj))
            {
                agent.RequestedSitTarget = request.TargetObject.TargetID;
                agent.RequestedSitOffset = request.TargetObject.Offset;

                AvatarSitResponsePacket response = new AvatarSitResponsePacket();
                response.SitObject.ID = request.TargetObject.TargetID;
                response.SitTransform.AutoPilot = true;
                response.SitTransform.CameraAtOffset = Vector3.Zero;
                response.SitTransform.CameraEyeOffset = Vector3.Zero;
                response.SitTransform.ForceMouselook = false;
                response.SitTransform.SitPosition = request.TargetObject.Offset;
                response.SitTransform.SitRotation = obj.SitRotation;

                server.UDP.SendPacket(agent.ID, response, PacketCategory.State);
            }
            else
            {
                //TODO: send error
            }
        }

        void AgentSitHandler(Packet packet, Agent agent)
        {
            AgentSitPacket sit = (AgentSitPacket)packet;

            if (agent.RequestedSitTarget != UUID.Zero)
            {
                SimulationObject obj;
                SimulationObject avObj;
                if (server.Scene.TryGetObject(agent.RequestedSitTarget, out obj) && server.Scene.TryGetObject(agent.ID, out avObj))
                {
                    agent.Avatar.Prim.Flags &= ~PrimFlags.Physics;
                    agent.Avatar.Prim.ParentID = obj.Prim.LocalID;
                    agent.Avatar.Prim.Position = Vector3.Zero;
                    agent.Avatar.Prim.Position.X = obj.Prim.Scale.X * 0.5f;
                    agent.Avatar.Prim.Position.Z = obj.Prim.Scale.Z * 0.5f;
                    agent.Avatar.Prim.Position.Z += agent.Avatar.Prim.Scale.Z * 0.33f;

                    server.Scene.ObjectAdd(this, avObj, avObj.Prim.OwnerID, 0, PrimFlags.None);
                    server.Avatars.SetDefaultAnimation(agent, Animations.SIT);
                    server.Avatars.SendAnimations(agent);
                }
                else
                {
                    //TODO: send error
                }

                agent.RequestedSitTarget = UUID.Zero;
                agent.RequestedSitOffset = Vector3.Zero;
            }
        }

        void AgentUpdateHandler(Packet packet, Agent agent)
        {
            AgentUpdatePacket update = (AgentUpdatePacket)packet;

            if (agent.Avatar.Prim.ParentID == 0)
                agent.Avatar.Prim.Rotation = update.AgentData.BodyRotation;

            agent.ControlFlags = (AgentManager.ControlFlags)update.AgentData.ControlFlags;
            agent.State = (AgentState)update.AgentData.State;
            agent.HideTitle = update.AgentData.Flags != 0;

            if (agent.Avatar.Prim.ParentID > 0 && (agent.ControlFlags & AgentManager.ControlFlags.AGENT_CONTROL_STAND_UP) == AgentManager.ControlFlags.AGENT_CONTROL_STAND_UP)
            {
                SimulationObject obj;
                if (server.Scene.TryGetObject(agent.Avatar.Prim.ParentID, out obj))
                {
                    agent.Avatar.Prim.Position = obj.Prim.Position
                        + Vector3.Transform(obj.SitPosition, Matrix4.CreateFromQuaternion(obj.SitRotation))
                        + Vector3.UnitZ;
                }
                else
                {
                    //TODO: get position from course locations?
                    agent.Avatar.Prim.Position = Vector3.Zero;
                }

                agent.Avatar.Prim.ParentID = 0;
                
                server.Avatars.SetDefaultAnimation(agent, Animations.STAND);
                server.Avatars.SendAnimations(agent);

                agent.Avatar.Prim.Flags |= PrimFlags.Physics;
            }

            ObjectUpdatePacket fullUpdate = SimulationObject.BuildFullUpdate(agent.Avatar.Prim,
                server.Scene.RegionHandle, agent.Avatar.Prim.Flags);

            server.UDP.BroadcastPacket(fullUpdate, PacketCategory.State);
        }

        void SetAlwaysRunHandler(Packet packet, Agent agent)
        {
            SetAlwaysRunPacket run = (SetAlwaysRunPacket)packet;

            agent.Running = run.AgentData.AlwaysRun;
        }
    }
}
