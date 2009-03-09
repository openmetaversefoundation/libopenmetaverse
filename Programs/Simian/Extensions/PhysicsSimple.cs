using System;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.Rendering;

namespace Simian.Extensions
{
    public class PhysicsSimple : IExtension<Simian>, IPhysicsProvider
    {
        Simian server;

        public PhysicsSimple()
        {
        }

        public void Start(Simian server)
        {
            this.server = server;

            server.Scene.OnObjectAdd += Scene_OnObjectAdd;
            server.Scene.OnObjectModify += Scene_OnObjectModify;
            server.Scene.OnObjectTransform += Scene_OnObjectTransform;
        }

        public void Stop()
        {
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
            SimpleMesh mesh = obj.GetWorldMesh(DetailLevel.Low, false);
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
            if (server.Scene.TryGetObject(objectID, out obj))
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

        #region Callbacks

        void Scene_OnObjectAdd(object sender, SimulationObject obj, UUID ownerID, int scriptStartParam, PrimFlags creatorFlags)
        {
            // TODO: This doesn't update children prims when their parents move. "World meshes" are a bad approach in general,
            // the transforms should probably be applied to the mesh in the collision test
            obj.GetWorldMesh(DetailLevel.Low, true);
        }

        void Scene_OnObjectModify(object sender, SimulationObject obj, Primitive.ConstructionData data)
        {
            obj.GetWorldMesh(DetailLevel.Low, true);
        }

        void Scene_OnObjectTransform(object sender, SimulationObject obj, Vector3 position, Quaternion rotation, Vector3 velocity,
            Vector3 acceleration, Vector3 angularVelocity)
        {
            // TODO: This doesn't update children prims when their parents move. "World meshes" are a bad approach in general,
            // the transforms should probably be applied to the mesh in the collision test
            if (position != obj.Prim.Position || rotation != obj.Prim.Rotation)
                obj.GetWorldMesh(DetailLevel.Low, true);
        }

        #endregion Callbacks
    }
}
