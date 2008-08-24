using System;
using System.Collections.Generic;
using System.Threading;
using OpenMetaverse;
using OpenMetaverse.Rendering;
using OpenMetaverse.Packets;

namespace Simian.Extensions
{
    public class ObjectManager : ISimianExtension
    {
        Simian Server;
        InternalDictionary<uint, SimulationObject> SceneObjects = new InternalDictionary<uint, SimulationObject>();
        InternalDictionary<UUID, SimulationObject> SceneObjectsByID = new InternalDictionary<UUID,SimulationObject>();
        int CurrentLocalID = 0;

        public ObjectManager(Simian server)
        {
            Server = server;
        }

        public void Start()
        {
            Server.UDPServer.RegisterPacketCallback(PacketType.ObjectAdd, new UDPServer.PacketCallback(ObjectAddHandler));
            Server.UDPServer.RegisterPacketCallback(PacketType.ObjectSelect, new UDPServer.PacketCallback(ObjectSelectHandler));
        }

        public void Stop()
        {
        }

        void ObjectAddHandler(Packet packet, Agent agent)
        {
            ObjectAddPacket add = (ObjectAddPacket)packet;

            Vector3 position = Vector3.Zero;
            Vector3 scale = add.ObjectData.Scale;
            PCode pcode = (PCode)add.ObjectData.PCode;
            PrimFlags flags = (PrimFlags)add.ObjectData.AddFlags;
            bool bypassRaycast = (add.ObjectData.BypassRaycast == 1);
            bool rayEndIsIntersection = (add.ObjectData.RayEndIsIntersection == 1);

            #region Position Calculation

            if (rayEndIsIntersection)
            {
                // HACK: Blindly trust where the client tells us to place
                position = add.ObjectData.RayEnd;
            }
            else
            {
                if (add.ObjectData.RayTargetID != UUID.Zero)
                {
                    SimulationObject obj;
                    if (SceneObjectsByID.TryGetValue(add.ObjectData.RayTargetID, out obj))
                    {
                        // Test for a collision with the specified object
                        position = ObjectCollisionTest(add.ObjectData.RayStart, add.ObjectData.RayEnd, obj);
                    }
                }

                if (position == Vector3.Zero)
                {
                    // Test for a collision with the entire scene
                    position = FullSceneCollisionTest(add.ObjectData.RayStart, add.ObjectData.RayEnd);
                }
            }

            #endregion Position Calculation

            #region Foliage Handling

            // Set all foliage to phantom
            if (pcode == PCode.Grass || pcode == PCode.Tree || pcode == PCode.NewTree)
            {
                flags |= PrimFlags.Phantom;

                if (pcode != PCode.Grass)
                {
                    // Resize based on the foliage type
                    Tree tree = (Tree)add.ObjectData.State;

                    switch (tree)
                    {
                        case Tree.Cypress1:
                        case Tree.Cypress2:
                            scale = new Vector3(4f, 4f, 10f);
                            break;
                        default:
                            scale = new Vector3(4f, 4f, 4f);
                            break;
                    }
                }
            }

            #endregion Foliage Handling

            // Create an object
            Primitive prim = new Primitive();
            prim.Flags =
                PrimFlags.ObjectModify |
                PrimFlags.ObjectCopy |
                PrimFlags.ObjectAnyOwner |
                PrimFlags.ObjectMove |
                PrimFlags.ObjectTransfer |
                PrimFlags.ObjectOwnerModify;
            // TODO: Security check
            prim.GroupID = add.AgentData.GroupID;
            prim.ID = UUID.Random();
            prim.LocalID = (uint)Interlocked.Increment(ref CurrentLocalID);
            prim.MediaURL = String.Empty;
            prim.OwnerID = agent.AgentID;
            prim.Position = position;

            prim.PrimData.Material = (Material)add.ObjectData.Material;
            prim.PrimData.PathCurve = (PathCurve)add.ObjectData.PathCurve;
            prim.PrimData.ProfileCurve = (ProfileCurve)add.ObjectData.ProfileCurve;
            prim.PrimData.PathBegin = Primitive.UnpackBeginCut(add.ObjectData.PathBegin);
            prim.PrimData.PathEnd = Primitive.UnpackEndCut(add.ObjectData.PathEnd);
            prim.PrimData.PathScaleX = Primitive.UnpackPathScale(add.ObjectData.PathScaleX);
            prim.PrimData.PathScaleY = Primitive.UnpackPathScale(add.ObjectData.PathScaleY);
            prim.PrimData.PathShearX = Primitive.UnpackPathShear((sbyte)add.ObjectData.PathShearX);
            prim.PrimData.PathShearY = Primitive.UnpackPathShear((sbyte)add.ObjectData.PathShearY);
            prim.PrimData.PathTwist = Primitive.UnpackPathTwist(add.ObjectData.PathTwist);
            prim.PrimData.PathTwistBegin = Primitive.UnpackPathTwist(add.ObjectData.PathTwistBegin);
            prim.PrimData.PathRadiusOffset = Primitive.UnpackPathTwist(add.ObjectData.PathRadiusOffset);
            prim.PrimData.PathTaperX = Primitive.UnpackPathTaper(add.ObjectData.PathTaperX);
            prim.PrimData.PathTaperY = Primitive.UnpackPathTaper(add.ObjectData.PathTaperY);
            prim.PrimData.PathRevolutions = Primitive.UnpackPathRevolutions(add.ObjectData.PathRevolutions);
            prim.PrimData.PathSkew = Primitive.UnpackPathTwist(add.ObjectData.PathSkew);
            prim.PrimData.ProfileBegin = Primitive.UnpackBeginCut(add.ObjectData.ProfileBegin);
            prim.PrimData.ProfileEnd = Primitive.UnpackEndCut(add.ObjectData.ProfileEnd);
            prim.PrimData.ProfileHollow = Primitive.UnpackProfileHollow(add.ObjectData.ProfileHollow);
            prim.PrimData.PCode = pcode;

            prim.Properties.CreationDate = DateTime.Now;
            prim.Properties.CreatorID = agent.AgentID;
            prim.Properties.Description = String.Empty;
            prim.Properties.GroupID = add.AgentData.GroupID;
            prim.Properties.LastOwnerID = agent.AgentID;
            prim.Properties.Name = "New Object";
            prim.Properties.ObjectID = prim.ID;
            prim.Properties.OwnerID = prim.OwnerID;
            prim.Properties.Permissions = Permissions.FullPermissions;
            prim.Properties.SalePrice = 10;

            prim.RegionHandle = Server.RegionHandle;
            prim.Rotation = add.ObjectData.Rotation;
            prim.Scale = scale;
            prim.Textures = new Primitive.TextureEntry(Primitive.TextureEntry.WHITE_TEXTURE);
            prim.TextColor = Color4.Black;

            // Add this prim to the object database
            SimulationObject simObj = new SimulationObject(prim, Server);

            lock (SceneObjects.Dictionary)
                SceneObjects.Dictionary[prim.LocalID] = simObj;
            lock (SceneObjectsByID.Dictionary)
                SceneObjectsByID.Dictionary[prim.ID] = simObj;

            // Send an update out to the creator
            ObjectUpdatePacket updateToOwner = Movement.BuildFullUpdate(prim, String.Empty, prim.RegionHandle, 0,
                prim.Flags | PrimFlags.CreateSelected | PrimFlags.ObjectYouOwner);
            agent.SendPacket(updateToOwner);

            // Send an update out to everyone else
            ObjectUpdatePacket updateToOthers = Movement.BuildFullUpdate(prim, String.Empty, prim.RegionHandle, 0,
                prim.Flags);
            lock (Server.Agents)
            {
                foreach (Agent recipient in Server.Agents.Values)
                {
                    if (recipient != agent)
                        recipient.SendPacket(updateToOthers);
                }
            }
        }

        void ObjectSelectHandler(Packet packet, Agent agent)
        {
            ObjectSelectPacket select = (ObjectSelectPacket)packet;

            ObjectPropertiesPacket properties = new ObjectPropertiesPacket();
            properties.ObjectData = new ObjectPropertiesPacket.ObjectDataBlock[select.ObjectData.Length];

            for (int i = 0; i < select.ObjectData.Length; i++)
            {
                properties.ObjectData[i] = new ObjectPropertiesPacket.ObjectDataBlock();

                SimulationObject obj;
                if (SceneObjects.TryGetValue(select.ObjectData[i].ObjectLocalID, out obj))
                {
                    properties.ObjectData[i].BaseMask = (uint)obj.Params.Properties.Permissions.BaseMask;
                    properties.ObjectData[i].CreationDate = Utils.DateTimeToUnixTime(obj.Params.Properties.CreationDate);
                    properties.ObjectData[i].CreatorID = obj.Params.Properties.CreatorID;
                    properties.ObjectData[i].Description = Utils.StringToBytes(obj.Params.Properties.Description);
                    properties.ObjectData[i].EveryoneMask = (uint)obj.Params.Properties.Permissions.EveryoneMask;
                    properties.ObjectData[i].GroupID = obj.Params.Properties.GroupID;
                    properties.ObjectData[i].GroupMask = (uint)obj.Params.Properties.Permissions.GroupMask;
                    properties.ObjectData[i].LastOwnerID = obj.Params.Properties.LastOwnerID;
                    properties.ObjectData[i].Name = Utils.StringToBytes(obj.Params.Properties.Name);
                    properties.ObjectData[i].NextOwnerMask = (uint)obj.Params.Properties.Permissions.NextOwnerMask;
                    properties.ObjectData[i].ObjectID = obj.Params.ID;
                    properties.ObjectData[i].OwnerID = obj.Params.Properties.OwnerID;
                    properties.ObjectData[i].OwnerMask = (uint)obj.Params.Properties.Permissions.OwnerMask;
                    properties.ObjectData[i].OwnershipCost = obj.Params.Properties.OwnershipCost;
                    properties.ObjectData[i].SalePrice = obj.Params.Properties.SalePrice;
                    properties.ObjectData[i].SaleType = (byte)obj.Params.Properties.SaleType;
                    properties.ObjectData[i].SitName = new byte[0];
                    properties.ObjectData[i].TextureID = new byte[0];
                    properties.ObjectData[i].TouchName = new byte[0];
                }
            }

            agent.SendPacket(properties);
        }

        Vector3 FullSceneCollisionTest(Vector3 rayStart, Vector3 rayEnd)
        {
            // HACK: For now
            Logger.DebugLog("Full scene collision test was requested, ignoring");
            return rayEnd;
        }

        Vector3 ObjectCollisionTest(Vector3 rayStart, Vector3 rayEnd, SimulationObject obj)
        {
            const float OO_THREE = 1f / 3f;

            Vector3 closestPoint = rayEnd;

            if (rayStart == rayEnd)
            {
                Logger.DebugLog("RayStart is equal to RayEnd, rezzing from given location");
                return closestPoint;
            }

            Vector3 direction = Vector3.Normalize(rayEnd - rayStart);
            Ray ray = new Ray(rayStart, direction);

            // Get the mesh that has been transformed into world-space
            SimpleMesh mesh = null;
            if (obj.Params.LocalID != 0)
            {
                SimulationObject parent;
                if (SceneObjects.TryGetValue(obj.Params.LocalID, out parent))
                    mesh = obj.GetWorldMesh(DetailLevel.Low, parent);
            }
            else
            {
                mesh = obj.GetWorldMesh(DetailLevel.Low, null);
            }


            if (mesh != null)
            {
                // Iterate through all of the triangles in the mesh, doing a ray-triangle intersection
                
                float closestDistance = Single.MaxValue;
                for (int i = 0; i < mesh.Indices.Count; i += 3)
                {
                    if (RayTriangleIntersection(rayStart, direction, mesh.Vertices[i].Position,
                        mesh.Vertices[i + 1].Position, mesh.Vertices[i + 2].Position))
                    {
                        // Find the barycenter of this triangle
                        Vector3 center =
                            (mesh.Vertices[i].Position + mesh.Vertices[i + 1].Position + mesh.Vertices[i + 2].Position) * OO_THREE;

                        Logger.DebugLog("Collision hit with triangle at " + center);

                        if ((center - rayStart).Length() < closestDistance)
                            closestPoint = center;
                    }
                }
            }

            return closestPoint;
        }

        /// <summary>
        /// Adapted from http://www.cs.virginia.edu/~gfx/Courses/2003/ImageSynthesis/papers/Acceleration/Fast%20MinimumStorage%20RayTriangle%20Intersection.pdf
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="direction"></param>
        /// <param name="vert0"></param>
        /// <param name="vert1"></param>
        /// <param name="vert2"></param>
        /// <returns></returns>
        bool RayTriangleIntersection(Vector3 origin, Vector3 direction, Vector3 vert0, Vector3 vert1, Vector3 vert2)
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
                return false;

            invDeterminant = 1f / determinant;

            // Calculate distance from vert0 to ray origin
            Vector3 tvec = origin - vert0;

            // Calculate U parameter and test bounds
            float u = Vector3.Dot(tvec, pvec) * invDeterminant;
            if (u < 0.0f || u > 1.0f)
                return false;

            // Prepare to test V parameter
            Vector3 qvec = Vector3.Cross(tvec, edge1);

            // Calculate V parameter and test bounds
            float v = Vector3.Dot(direction, qvec) * invDeterminant;
            if (v < 0.0f || u + v > 1.0f)
                return false;

            //t = Vector3.Dot(edge2, qvec) * invDeterminant;

            return true;
        }
    }
}
