using System;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.Rendering;

namespace Simian
{
    public class PhysicsSimple : IExtension<ISceneProvider>, IPhysicsProvider
    {
        // Run our own frames per second limiter on top of the limiting done by ISceneProvider
        const int FRAMES_PER_SECOND = 10;

        const float GRAVITY = 9.8f; //meters/sec
        const float WALK_SPEED = 3f; //meters/sec
        const float RUN_SPEED = 5f; //meters/sec
        const float FLY_SPEED = 10f; //meters/sec
        const float FALL_DELAY = 0.33f; //seconds before starting animation
        const float FALL_FORGIVENESS = .25f; //fall buffer in meters
        const float JUMP_IMPULSE_VERTICAL = 8.5f; //boost amount in meters/sec
        const float JUMP_IMPULSE_HORIZONTAL = 10f; //boost amount in meters/sec
        const float INITIAL_HOVER_IMPULSE = 2f; //boost amount in meters/sec
        const float PREJUMP_DELAY = 0.25f; //seconds before actually jumping
        const float AVATAR_TERMINAL_VELOCITY = 54f; //~120mph

        const float SQRT_TWO = 1.41421356f;

        ISceneProvider scene;
        float elapsedSinceUpdate;

        public PhysicsSimple()
        {
        }

        public bool Start(ISceneProvider scene)
        {
            this.scene = scene;

            scene.OnObjectAddOrUpdate += Scene_OnObjectAddOrUpdate;
            return true;
        }

        public void Stop()
        {
        }

        public void Update(float elapsedTime)
        {
            if (elapsedSinceUpdate >= 1f / (float)FRAMES_PER_SECOND)
            {
                elapsedTime = elapsedSinceUpdate;
                elapsedSinceUpdate = 0f;
            }
            else
            {
                elapsedSinceUpdate += elapsedTime;
                return;
            }

            scene.ForEachAgent(
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
                    float speed = elapsedTime * (flying ? FLY_SPEED : agent.Running && !jumping ? RUN_SPEED : WALK_SPEED);
                    if ((heldForward || heldBack) && (heldLeft || heldRight))
                        speed /= SQRT_TWO;

                    Vector3 agentPosition = agent.Avatar.GetSimulatorPosition();
                    float oldFloor = scene.GetTerrainHeightAt(agentPosition.X, agentPosition.Y);

                    agentPosition += (move * speed);
                    float newFloor = scene.GetTerrainHeightAt(agentPosition.X, agentPosition.Y);

                    if (!flying && newFloor != oldFloor)
                        speed /= (1 + (SQRT_TWO * Math.Abs(newFloor - oldFloor)));

                    //HACK: distance from avatar center to the bottom of its feet
                    float distanceFromFloor = agent.Avatar.Prim.Scale.Z * .5f;

                    float lowerLimit = newFloor + distanceFromFloor;

                    //"bridge" physics
                    if (agent.Avatar.Prim.Velocity != Vector3.Zero)
                    {
                        //start ray at our feet
                        Vector3 rayStart = new Vector3(
                            agent.Avatar.Prim.Position.X,
                            agent.Avatar.Prim.Position.Y,
                            agent.Avatar.Prim.Position.Z - distanceFromFloor
                            );

                        //end ray at 0.01m below our feet
                        Vector3 rayEnd = new Vector3(
                            rayStart.X,
                            rayStart.Y,
                            rayStart.Z - 0.01f
                            );

                        scene.ForEachObject(delegate(SimulationObject obj)
                        {
                            //HACK: check nearby objects (what did you expect, octree?)
                            if (Vector3.Distance(rayStart, obj.Prim.Position) <= 15f)
                            {
                                Vector3 collision = scene.Physics.ObjectCollisionTest(rayStart, rayEnd, obj);

                                if (collision != rayEnd) //we collided!
                                {
                                    //check if we are any higher than before
                                    float height = collision.Z + distanceFromFloor;
                                    if (height > lowerLimit) lowerLimit = height;
                                }
                            }
                        });
                    }

                    // Z acceleration resulting from gravity
                    float gravity = 0f;

                    float waterChestHeight = scene.WaterHeight - (agent.Avatar.Prim.Scale.Z * .33f);

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
                            if (scene.Avatars.SetDefaultAnimation(agent, Animations.FLY))
                                animsChanged = true;
                        }
                        else if (move.Z > 0)
                        { //flying straight up
                            if (scene.Avatars.SetDefaultAnimation(agent, Animations.HOVER_UP))
                                animsChanged = true;
                        }
                        else if (move.Z < 0)
                        { //flying straight down
                            if (scene.Avatars.SetDefaultAnimation(agent, Animations.HOVER_DOWN))
                                animsChanged = true;
                        }
                        else
                        { //hovering in the air
                            if (scene.Avatars.SetDefaultAnimation(agent, Animations.HOVER))
                                animsChanged = true;
                        }
                    }
                    else if (agent.Avatar.Prim.Position.Z > lowerLimit + FALL_FORGIVENESS || agent.Avatar.Prim.Position.Z <= waterChestHeight)
                    { //falling, floating, or landing from a jump

                        if (agent.Avatar.Prim.Position.Z > scene.WaterHeight)
                        { //above water

                            //override controls while drifting
                            move = Vector3.Zero;

                            //keep most of our horizontal inertia
                            agent.Avatar.Prim.Velocity.X *= 0.975f;
                            agent.Avatar.Prim.Velocity.Y *= 0.975f;

                            float fallElapsed = (float)(Environment.TickCount - agent.TickFall) / 1000f;

                            if (agent.TickFall == 0 || (fallElapsed > FALL_DELAY && agent.Avatar.Prim.Velocity.Z >= 0f))
                            { //just started falling
                                agent.TickFall = Environment.TickCount;
                            }
                            else
                            {
                                gravity = GRAVITY * fallElapsed * elapsedTime; //normal gravity

                                if (!jumping)
                                { //falling
                                    if (fallElapsed > FALL_DELAY)
                                    { //falling long enough to trigger the animation
                                        if (scene.Avatars.SetDefaultAnimation(agent, Animations.FALLDOWN))
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
                                if (scene.Avatars.SetDefaultAnimation(agent, Animations.HOVER_UP))
                                    animsChanged = true;
                            }
                            else if (move.X != 0 || move.Y != 0)
                            {
                                if (scene.Avatars.SetDefaultAnimation(agent, Animations.FLYSLOW))
                                    animsChanged = true;
                            }
                            else
                            {
                                if (scene.Avatars.SetDefaultAnimation(agent, Animations.HOVER))
                                    animsChanged = true;
                            }
                        }
                        else
                        { //underwater

                            gravity = 0f; //buoyant
                            agent.Avatar.Prim.Velocity *= 0.5f * elapsedTime;
                            agent.Avatar.Prim.Velocity.Z += 0.75f * elapsedTime;

                            if (scene.Avatars.SetDefaultAnimation(agent, Animations.FALLDOWN))
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
                                if (scene.Avatars.SetDefaultAnimation(agent, Animations.PRE_JUMP))
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

                                if (scene.Avatars.SetDefaultAnimation(agent, Animations.JUMP))
                                    animsChanged = true;

                                agent.Avatar.Prim.Velocity.X += agent.Avatar.Prim.Acceleration.X * JUMP_IMPULSE_HORIZONTAL;
                                agent.Avatar.Prim.Velocity.Y += agent.Avatar.Prim.Acceleration.Y * JUMP_IMPULSE_HORIZONTAL;
                                agent.Avatar.Prim.Velocity.Z = JUMP_IMPULSE_VERTICAL * elapsedTime;

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
                                    if (scene.Avatars.SetDefaultAnimation(agent, Animations.CROUCHWALK))
                                        animsChanged = true;
                                }
                                else if (agent.Running)
                                { //running
                                    if (scene.Avatars.SetDefaultAnimation(agent, Animations.RUN))
                                        animsChanged = true;
                                }
                                else
                                { //walking
                                    if (scene.Avatars.SetDefaultAnimation(agent, Animations.WALK))
                                        animsChanged = true;
                                }
                            }
                            else
                            { //walking
                                if (move.Z < 0)
                                { //crouching
                                    if (scene.Avatars.SetDefaultAnimation(agent, Animations.CROUCH))
                                        animsChanged = true;
                                }
                                else
                                { //standing
                                    if (scene.Avatars.SetDefaultAnimation(agent, Animations.STAND))
                                        animsChanged = true;
                                }
                            }
                        }
                    }

                    if (animsChanged)
                        scene.Avatars.SendAnimations(agent);

                    float maxVel = AVATAR_TERMINAL_VELOCITY * elapsedTime;

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

        public Vector3 ObjectCollisionTest(Vector3 rayStart, Vector3 rayEnd, SimulationObject obj)
        {
            Vector3 closestPoint = rayEnd;

            if (rayStart == rayEnd)
            {
                Logger.DebugLog("RayStart is equal to RayEnd, returning given location");
                return closestPoint;
            }

            Vector3 direction = Vector3.Normalize(rayEnd - rayStart);

            // Get the mesh that has been transformed into world-space
            SimpleMesh mesh = obj.GetWorldMesh(DetailLevel.Low, false, false);
            if (mesh != null)
            {
                // Iterate through all of the triangles in the mesh, doing a ray-triangle intersection

                float closestDistance = Single.MaxValue;
                for (int i = 0; i < mesh.Indices.Count; i += 3)
                {
                    Vector3 point0 = mesh.Vertices[mesh.Indices[i + 0]].Position;
                    Vector3 point1 = mesh.Vertices[mesh.Indices[i + 1]].Position;
                    Vector3 point2 = mesh.Vertices[mesh.Indices[i + 2]].Position;

                    Vector3 collisionPoint;
                    if (RayTriangleIntersection(rayStart, direction, point0, point1, point2, out collisionPoint))
                    {
                        if ((collisionPoint - rayStart).Length() < closestDistance)
                            closestPoint = collisionPoint;
                    }
                }
            }

            return closestPoint;
        }

        public bool TryGetObjectMass(UUID objectID, out float mass)
        {
            SimulationObject obj;
            if (scene.TryGetObject(objectID, out obj))
            {
                mass = CalculateMass(obj.Prim);
                return true;
            }
            else
            {
                mass = 0f;
                return false;
            }
        }

        void Scene_OnObjectAddOrUpdate(object sender, SimulationObject obj, UUID ownerID, PrimFlags creatorFlags, UpdateFlags update)
        {
            // Recompute meshes for 
            bool forceMeshing = false;
            bool forceTransform = false;

            if ((update & UpdateFlags.Scale) != 0 ||
                (update & UpdateFlags.Position) != 0 ||
                (update & UpdateFlags.Rotation) != 0)
            {
                forceTransform = true;
            }

            if ((update & UpdateFlags.PrimData) != 0)
            {
                forceMeshing = true;
            }

            // TODO: This doesn't update children prims when their parents move
            obj.GetWorldMesh(DetailLevel.Low, forceMeshing, forceTransform);
        }

        /// <summary>
        /// Adapted from http://www.cs.virginia.edu/~gfx/Courses/2003/ImageSynthesis/papers/Acceleration/Fast%20MinimumStorage%20RayTriangle%20Intersection.pdf
        /// </summary>
        /// <param name="origin">Origin point of the ray</param>
        /// <param name="direction">Unit vector representing the direction of the ray</param>
        /// <param name="vert0">Position of the first triangle corner</param>
        /// <param name="vert1">Position of the second triangle corner</param>
        /// <param name="vert2">Position of the third triangle corner</param>
        /// <param name="collisionPoint">The collision point in the triangle</param>
        /// <returns>True if the ray passes through the triangle, otherwise false</returns>
        static bool RayTriangleIntersection(Vector3 origin, Vector3 direction, Vector3 vert0, Vector3 vert1, Vector3 vert2, out Vector3 collisionPoint)
        {
            const float EPSILON = 0.00001f;

            Vector3 edge1, edge2, pvec;
            float determinant, invDeterminant;

            // Find vectors for two edges sharing vert0
            edge1 = vert1 - vert0;
            edge2 = vert2 - vert0;

            // Begin calculating the determinant
            pvec = Vector3.Cross(direction, edge2);

            // If the determinant is near zero, ray lies in plane of triangle
            determinant = Vector3.Dot(edge1, pvec);

            if (determinant > -EPSILON && determinant < EPSILON)
            {
                collisionPoint = Vector3.Zero;
                return false;
            }

            invDeterminant = 1f / determinant;

            // Calculate distance from vert0 to ray origin
            Vector3 tvec = origin - vert0;

            // Calculate U parameter and test bounds
            float u = Vector3.Dot(tvec, pvec) * invDeterminant;
            if (u < 0.0f || u > 1.0f)
            {
                collisionPoint = Vector3.Zero;
                return false;
            }

            // Prepare to test V parameter
            Vector3 qvec = Vector3.Cross(tvec, edge1);

            // Calculate V parameter and test bounds
            float v = Vector3.Dot(direction, qvec) * invDeterminant;
            if (v < 0.0f || u + v > 1.0f)
            {
                collisionPoint = Vector3.Zero;
                return false;
            }

            //t = Vector3.Dot(edge2, qvec) * invDeterminant;

            collisionPoint = new Vector3(
                vert0.X + u * (vert1.X - vert0.X) + v * (vert2.X - vert0.X),
                vert0.Y + u * (vert1.Y - vert0.Y) + v * (vert2.Y - vert0.Y),
                vert0.Z + u * (vert1.Z - vert0.Z) + v * (vert2.Z - vert0.Z));

            return true;
        }

        /// <summary>
        /// Adapted from code written by Teravus for OpenSim
        /// </summary>
        /// <param name="prim">Primitive to calculate the mass of</param>
        /// <returns>Estimated mass of the given primitive</returns>
        static float CalculateMass(Primitive prim)
        {
            const float PRIM_DENSITY = 10.000006836f; // Aluminum g/cm3

            float volume = 0f;
            float returnMass = 0f;

            // TODO: Use the prim material in mass calculations once our physics
            // engine supports different materials

            switch (prim.PrimData.ProfileCurve)
            {
                case ProfileCurve.Square:
                    // Profile Volume

                    volume = prim.Scale.X * prim.Scale.Y * prim.Scale.Z;

                    // If the user has 'hollowed out'
                    if (prim.PrimData.ProfileHollow > 0.0f)
                    {
                        float hollowAmount = prim.PrimData.ProfileHollow;

                        // calculate the hollow volume by it's shape compared to the prim shape
                        float hollowVolume = 0;
                        switch (prim.PrimData.ProfileHole)
                        {
                            case HoleType.Square:
                            case HoleType.Same:
                                // Cube Hollow volume calculation
                                float hollowsizex = prim.Scale.X * hollowAmount;
                                float hollowsizey = prim.Scale.Y * hollowAmount;
                                float hollowsizez = prim.Scale.Z * hollowAmount;
                                hollowVolume = hollowsizex * hollowsizey * hollowsizez;
                                break;

                            case HoleType.Circle:
                                // Hollow shape is a perfect cyllinder in respect to the cube's scale
                                // Cyllinder hollow volume calculation
                                float hRadius = prim.Scale.X * 0.5f;
                                float hLength = prim.Scale.Z;

                                // pi * r2 * h
                                hollowVolume = ((float)(Math.PI * Math.Pow(hRadius, 2) * hLength) * hollowAmount);
                                break;

                            case HoleType.Triangle:
                                // Equilateral Triangular Prism volume hollow calculation
                                // Triangle is an Equilateral Triangular Prism with aLength = to _size.Y

                                float aLength = prim.Scale.Y;
                                // 1/2 abh
                                hollowVolume = (float)((0.5 * aLength * prim.Scale.X * prim.Scale.Z) * hollowAmount);
                                break;

                            default:
                                hollowVolume = 0;
                                break;
                        }
                        volume = volume - hollowVolume;
                    }

                    break;
                case ProfileCurve.Circle:
                    if (prim.PrimData.PathCurve == PathCurve.Line)
                    {
                        // Cylinder
                        float volume1 = (float)(Math.PI * Math.Pow(prim.Scale.X / 2, 2) * prim.Scale.Z);
                        float volume2 = (float)(Math.PI * Math.Pow(prim.Scale.Y / 2, 2) * prim.Scale.Z);

                        // Approximating the cylinder's irregularity.
                        if (volume1 > volume2)
                        {
                            volume = (float)volume1 - (volume1 - volume2);
                        }
                        else if (volume2 > volume1)
                        {
                            volume = (float)volume2 - (volume2 - volume1);
                        }
                        else
                        {
                            // Regular cylinder
                            volume = volume1;
                        }
                    }
                    else
                    {
                        // We don't know what the shape is yet, so use default
                        volume = prim.Scale.X * prim.Scale.Y * prim.Scale.Z;
                    }

                    // If the user has 'hollowed out'
                    if (prim.PrimData.ProfileHollow > 0.0f)
                    {
                        float hollowAmount = prim.PrimData.ProfileHollow;

                        // calculate the hollow volume by it's shape compared to the prim shape
                        float hollowVolume = 0f;
                        switch (prim.PrimData.ProfileHole)
                        {
                            case HoleType.Circle:
                            case HoleType.Same:
                                // Hollow shape is a perfect cyllinder in respect to the cube's scale
                                // Cyllinder hollow volume calculation
                                float hRadius = prim.Scale.X * 0.5f;
                                float hLength = prim.Scale.Z;

                                // pi * r2 * h
                                hollowVolume = ((float)(Math.PI * Math.Pow(hRadius, 2) * hLength) * hollowAmount);
                                break;

                            case HoleType.Square:
                                // Cube Hollow volume calculation
                                float hollowsizex = prim.Scale.X * hollowAmount;
                                float hollowsizey = prim.Scale.Y * hollowAmount;
                                float hollowsizez = prim.Scale.Z * hollowAmount;
                                hollowVolume = hollowsizex * hollowsizey * hollowsizez;
                                break;

                            case HoleType.Triangle:
                                // Equilateral Triangular Prism volume hollow calculation
                                // Triangle is an Equilateral Triangular Prism with aLength = to _size.Y

                                float aLength = prim.Scale.Y;
                                // 1/2 abh
                                hollowVolume = (0.5f * aLength * prim.Scale.X * prim.Scale.Z) * hollowAmount;
                                break;

                            default:
                                hollowVolume = 0;
                                break;
                        }
                        volume = volume - hollowVolume;
                    }
                    break;

                case ProfileCurve.HalfCircle:
                    if (prim.PrimData.PathCurve == PathCurve.Circle)
                    {
                        if (prim.Scale.X == prim.Scale.Y && prim.Scale.Y == prim.Scale.Z)
                        {
                            // regular sphere
                            // v = 4/3 * pi * r^3
                            float sradius3 = (float)Math.Pow((prim.Scale.X * 0.5f), 3);
                            volume = (4f / 3f) * (float)Math.PI * sradius3;
                        }
                        else
                        {
                            // we treat this as a box currently
                            volume = prim.Scale.X * prim.Scale.Y * prim.Scale.Z;
                        }
                    }
                    else
                    {
                        // We don't know what the shape is yet, so use default
                        volume = prim.Scale.X * prim.Scale.Y * prim.Scale.Z;
                    }
                    break;

                case ProfileCurve.EqualTriangle:
                    float xA = -0.25f * prim.Scale.X;
                    float yA = -0.45f * prim.Scale.Y;

                    float xB = 0.5f * prim.Scale.X;
                    float yB = 0;

                    float xC = -0.25f * prim.Scale.X;
                    float yC = 0.45f * prim.Scale.Y;

                    volume = (float)((Math.Abs((xB * yA - xA * yB) + (xC * yB - xB * yC) + (xA * yC - xC * yA)) / 2) * prim.Scale.Z);

                    // If the user has 'hollowed out'
                    // ProfileHollow is one of those 0 to 50000 values :P
                    // we like percentages better..   so turning into a percentage
                    if (prim.PrimData.ProfileHollow > 0.0f)
                    {
                        float hollowAmount = prim.PrimData.ProfileHollow;

                        // calculate the hollow volume by it's shape compared to the prim shape
                        float hollowVolume = 0f;

                        switch (prim.PrimData.ProfileHole)
                        {
                            case HoleType.Triangle:
                            case HoleType.Same:
                                // Equilateral Triangular Prism volume hollow calculation
                                // Triangle is an Equilateral Triangular Prism with aLength = to _size.Y

                                float aLength = prim.Scale.Y;
                                // 1/2 abh
                                hollowVolume = (0.5f * aLength * prim.Scale.X * prim.Scale.Z) * hollowAmount;
                                break;

                            case HoleType.Square:
                                // Cube Hollow volume calculation
                                float hollowsizex = prim.Scale.X * hollowAmount;
                                float hollowsizey = prim.Scale.Y * hollowAmount;
                                float hollowsizez = prim.Scale.Z * hollowAmount;
                                hollowVolume = hollowsizex * hollowsizey * hollowsizez;
                                break;

                            case HoleType.Circle:
                                // Hollow shape is a perfect cyllinder in respect to the cube's scale
                                // Cyllinder hollow volume calculation
                                float hRadius = prim.Scale.X * 0.5f;
                                float hLength = prim.Scale.Z;

                                // pi * r2 * h
                                hollowVolume = ((float)((Math.PI * Math.Pow(hRadius, 2) * hLength) / 2) * hollowAmount);
                                break;

                            default:
                                hollowVolume = 0;
                                break;
                        }
                        volume = volume - hollowVolume;
                    }
                    break;

                default:
                    // we don't have all of the volume formulas yet so
                    // use the common volume formula for all
                    volume = prim.Scale.X * prim.Scale.Y * prim.Scale.Z;
                    break;
            }

            // Calculate Path cut effect on volume
            // Not exact, in the triangle hollow example
            // They should never be zero or less then zero..
            // we'll ignore it if it's less then zero

            if (prim.PrimData.ProfileBegin + prim.PrimData.ProfileEnd > 0.0f)
            {
                float pathCutAmount = prim.PrimData.ProfileBegin + prim.PrimData.ProfileEnd;

                // Check the return amount for sanity
                if (pathCutAmount >= 0.99f)
                    pathCutAmount = 0.99f;

                volume = volume - (volume * pathCutAmount);
            }

            // Mass = density * volume
            if (prim.PrimData.PathTaperX != 1f)
                volume *= (prim.PrimData.PathTaperX / 3f) + 0.001f;
            if (prim.PrimData.PathTaperY != 1f)
                volume *= (prim.PrimData.PathTaperY / 3f) + 0.001f;

            returnMass = PRIM_DENSITY * volume;

            if (returnMass <= 0f)
                returnMass = 0.0001f; //ckrinke: Mass must be greater then zero.

            return returnMass;
        }
    }
}
