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
        DoubleDictionary<uint, UUID, SimulationObject> SceneObjects = new DoubleDictionary<uint, UUID, SimulationObject>();
        int CurrentLocalID = 0;

        public ObjectManager(Simian server)
        {
            Server = server;
        }

        public void Start()
        {
            Server.UDP.RegisterPacketCallback(PacketType.ObjectAdd, new PacketCallback(ObjectAddHandler));
            Server.UDP.RegisterPacketCallback(PacketType.ObjectSelect, new PacketCallback(ObjectSelectHandler));
            Server.UDP.RegisterPacketCallback(PacketType.ObjectDeselect, new PacketCallback(ObjectDeselectHandler));
            Server.UDP.RegisterPacketCallback(PacketType.ObjectShape, new PacketCallback(ObjectShapeHandler));
            Server.UDP.RegisterPacketCallback(PacketType.DeRezObject, new PacketCallback(DeRezObjectHandler));
            Server.UDP.RegisterPacketCallback(PacketType.MultipleObjectUpdate, new PacketCallback(MultipleObjectUpdateHandler));
            Server.UDP.RegisterPacketCallback(PacketType.RequestObjectPropertiesFamily, new PacketCallback(RequestObjectPropertiesFamilyHandler));
            Server.UDP.RegisterPacketCallback(PacketType.CompleteAgentMovement, new PacketCallback(CompleteAgentMovementHandler)); //HACK: show prims
        }

        public void Stop()
        {
        }

        //TODO: Add interest list instead of this hack
        void CompleteAgentMovementHandler(Packet packet, Agent agent)
        {
            CompleteAgentMovementPacket complete = (CompleteAgentMovementPacket)packet;
            SceneObjects.ForEach(delegate(SimulationObject obj)
            {
                ObjectUpdatePacket update = Movement.BuildFullUpdate(obj.Prim, String.Empty, obj.Prim.RegionHandle, 0, obj.Prim.Flags);
                Server.UDP.SendPacket(agent.AgentID, update, PacketCategory.State);
            });
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
                    if (SceneObjects.TryGetValue(add.ObjectData.RayTargetID, out obj))
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

            // Position lies on the face of another surface, either terrain of an object.
            // Back up along the ray so we are not colliding with the mesh.
            // HACK: This is really cheesy and should be done by a collision system
            Vector3 rayDir = Vector3.Normalize(add.ObjectData.RayEnd - add.ObjectData.RayStart);
            position -= rayDir * scale;

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

            SceneObjects.Add(prim.LocalID, prim.ID, simObj);

            // Send an update out to the creator
            ObjectUpdatePacket updateToOwner = Movement.BuildFullUpdate(prim, String.Empty, prim.RegionHandle, 0,
                prim.Flags | PrimFlags.CreateSelected | PrimFlags.ObjectYouOwner);
            Server.UDP.SendPacket(agent.AgentID, updateToOwner, PacketCategory.State);

            // Send an update out to everyone else
            ObjectUpdatePacket updateToOthers = Movement.BuildFullUpdate(prim, String.Empty, prim.RegionHandle, 0,
                prim.Flags);
            lock (Server.Agents)
            {
                foreach (Agent recipient in Server.Agents.Values)
                {
                    if (recipient != agent)
                        Server.UDP.SendPacket(recipient.AgentID, updateToOthers, PacketCategory.State);
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
                    properties.ObjectData[i].BaseMask = (uint)obj.Prim.Properties.Permissions.BaseMask;
                    properties.ObjectData[i].CreationDate = Utils.DateTimeToUnixTime(obj.Prim.Properties.CreationDate);
                    properties.ObjectData[i].CreatorID = obj.Prim.Properties.CreatorID;
                    properties.ObjectData[i].Description = Utils.StringToBytes(obj.Prim.Properties.Description);
                    properties.ObjectData[i].EveryoneMask = (uint)obj.Prim.Properties.Permissions.EveryoneMask;
                    properties.ObjectData[i].GroupID = obj.Prim.Properties.GroupID;
                    properties.ObjectData[i].GroupMask = (uint)obj.Prim.Properties.Permissions.GroupMask;
                    properties.ObjectData[i].LastOwnerID = obj.Prim.Properties.LastOwnerID;
                    properties.ObjectData[i].Name = Utils.StringToBytes(obj.Prim.Properties.Name);
                    properties.ObjectData[i].NextOwnerMask = (uint)obj.Prim.Properties.Permissions.NextOwnerMask;
                    properties.ObjectData[i].ObjectID = obj.Prim.ID;
                    properties.ObjectData[i].OwnerID = obj.Prim.Properties.OwnerID;
                    properties.ObjectData[i].OwnerMask = (uint)obj.Prim.Properties.Permissions.OwnerMask;
                    properties.ObjectData[i].OwnershipCost = obj.Prim.Properties.OwnershipCost;
                    properties.ObjectData[i].SalePrice = obj.Prim.Properties.SalePrice;
                    properties.ObjectData[i].SaleType = (byte)obj.Prim.Properties.SaleType;
                    properties.ObjectData[i].SitName = new byte[0];
                    properties.ObjectData[i].TextureID = new byte[0];
                    properties.ObjectData[i].TouchName = new byte[0];
                }
            }

            Server.UDP.SendPacket(agent.AgentID, properties, PacketCategory.Transaction);
        }

        void ObjectDeselectHandler(Packet packet, Agent agent)
        {
            ObjectDeselectPacket deselect = (ObjectDeselectPacket)packet;

            // TODO: Do we need this at all?
        }

        void ObjectShapeHandler(Packet packet, Agent agent)
        {
            ObjectShapePacket shape = (ObjectShapePacket)packet;

            for (int i = 0; i < shape.ObjectData.Length; i++)
            {
                ObjectShapePacket.ObjectDataBlock block = shape.ObjectData[i];

                SimulationObject obj;
                if (SceneObjects.TryGetValue(block.ObjectLocalID, out obj))
                {
                    obj.Prim.PrimData.PathBegin = Primitive.UnpackBeginCut(block.PathBegin);
                    obj.Prim.PrimData.PathCurve = (PathCurve)block.PathCurve;
                    obj.Prim.PrimData.PathEnd = Primitive.UnpackEndCut(block.PathEnd);
                    obj.Prim.PrimData.PathRadiusOffset = Primitive.UnpackPathTwist(block.PathRadiusOffset);
                    obj.Prim.PrimData.PathRevolutions = Primitive.UnpackPathRevolutions(block.PathRevolutions);
                    obj.Prim.PrimData.PathScaleX = Primitive.UnpackPathScale(block.PathScaleX);
                    obj.Prim.PrimData.PathScaleY = Primitive.UnpackPathScale(block.PathScaleY);
                    obj.Prim.PrimData.PathShearX = Primitive.UnpackPathShear((sbyte)block.PathShearX);
                    obj.Prim.PrimData.PathShearY = Primitive.UnpackPathShear((sbyte)block.PathShearY);
                    obj.Prim.PrimData.PathSkew = Primitive.UnpackPathTwist(block.PathSkew);
                    obj.Prim.PrimData.PathTaperX = Primitive.UnpackPathTaper(block.PathTaperX);
                    obj.Prim.PrimData.PathTaperY = Primitive.UnpackPathTaper(block.PathTaperY);
                    obj.Prim.PrimData.PathTwist = Primitive.UnpackPathTwist(block.PathTwist);
                    obj.Prim.PrimData.PathTwistBegin = Primitive.UnpackPathTwist(block.PathTwistBegin);
                    obj.Prim.PrimData.ProfileBegin = Primitive.UnpackBeginCut(block.ProfileBegin);
                    obj.Prim.PrimData.profileCurve = block.ProfileCurve;
                    obj.Prim.PrimData.ProfileEnd = Primitive.UnpackEndCut(block.ProfileEnd);
                    obj.Prim.PrimData.ProfileHollow = Primitive.UnpackProfileHollow(block.ProfileHollow);

                    // Send the update out to everyone
                    ObjectUpdatePacket editedobj = Movement.BuildFullUpdate(obj.Prim, String.Empty, obj.Prim.RegionHandle, 0,
                        obj.Prim.Flags);
                    Server.UDP.BroadcastPacket(editedobj, PacketCategory.State);
                }
                else
                {
                    Logger.Log("Got an ObjectShape packet for unknown object " + block.ObjectLocalID,
                        Helpers.LogLevel.Warning);
                }
            }
        }

        void DeRezObjectHandler(Packet packet, Agent agent)
        {
            DeRezObjectPacket derez = (DeRezObjectPacket)packet;
            DeRezDestination destination = (DeRezDestination)derez.AgentBlock.Destination;
            
            // TODO: Check permissions

            for (int i = 0; i < derez.ObjectData.Length; i++)
            {
                uint localID = derez.ObjectData[i].ObjectLocalID;

                SimulationObject obj;
                if (SceneObjects.TryGetValue(localID, out obj))
                {
                    switch (destination)
                    {
                        case DeRezDestination.AgentInventorySave:
                            Logger.Log("DeRezObject: Got an AgentInventorySave, DestID: " +
                                derez.AgentBlock.DestinationID.ToString(), Helpers.LogLevel.Warning);
                            break;
                        case DeRezDestination.AgentInventoryCopy:
                            Logger.Log("DeRezObject: Got an AgentInventorySave, DestID: " +
                                derez.AgentBlock.DestinationID.ToString(), Helpers.LogLevel.Warning);
                            break;
                        case DeRezDestination.TaskInventory:
                            Logger.Log("DeRezObject: Got a TaskInventory, DestID: " +
                                derez.AgentBlock.DestinationID.ToString(), Helpers.LogLevel.Warning);
                            break;
                        case DeRezDestination.Attachment:
                            Logger.Log("DeRezObject: Got an Attachment, DestID: " +
                                derez.AgentBlock.DestinationID.ToString(), Helpers.LogLevel.Warning);
                            break;
                        case DeRezDestination.AgentInventoryTake:
                            Logger.Log("DeRezObject: Got an AgentInventoryTake, DestID: " +
                                derez.AgentBlock.DestinationID.ToString(), Helpers.LogLevel.Warning);
                            break;
                        case DeRezDestination.ForceToGodInventory:
                            Logger.Log("DeRezObject: Got a ForceToGodInventory, DestID: " +
                                derez.AgentBlock.DestinationID.ToString(), Helpers.LogLevel.Warning);
                            break;
                        case DeRezDestination.TrashFolder:
                            InventoryObject invObj;
                            if (agent.Inventory.TryGetValue(derez.AgentBlock.DestinationID, out invObj) && invObj is InventoryFolder)
                            {
                                // FIXME: Handle children
                                InventoryFolder trash = (InventoryFolder)invObj;
                                Server.Inventory.CreateItem(agent, obj.Prim.Properties.Name, obj.Prim.Properties.Description, InventoryType.Object,
                                    AssetType.Object, obj.Prim.ID, trash.ID, PermissionMask.All, PermissionMask.All, agent.AgentID,
                                    obj.Prim.Properties.CreatorID, derez.AgentBlock.TransactionID, 0);
                                KillObject(obj);

                                Logger.DebugLog(String.Format("Derezzed prim {0} to agent inventory trash", obj.Prim.LocalID));
                            }
                            else
                            {
                                Logger.Log("DeRezObject: Got a TrashFolder with an invalid trash folder: " +
                                    derez.AgentBlock.DestinationID.ToString(), Helpers.LogLevel.Warning);
                            }
                            break;
                        case DeRezDestination.AttachmentToInventory:
                            Logger.Log("DeRezObject: Got an AttachmentToInventory, DestID: " +
                                derez.AgentBlock.DestinationID.ToString(), Helpers.LogLevel.Warning);
                            break;
                        case DeRezDestination.AttachmentExists:
                            Logger.Log("DeRezObject: Got an AttachmentExists, DestID: " +
                                derez.AgentBlock.DestinationID.ToString(), Helpers.LogLevel.Warning);
                            break;
                        case DeRezDestination.ReturnToOwner:
                            Logger.Log("DeRezObject: Got a ReturnToOwner, DestID: " +
                                derez.AgentBlock.DestinationID.ToString(), Helpers.LogLevel.Warning);
                            break;
                        case DeRezDestination.ReturnToLastOwner:
                            Logger.Log("DeRezObject: Got a ReturnToLastOwner, DestID: " +
                                derez.AgentBlock.DestinationID.ToString(), Helpers.LogLevel.Warning);
                            break;
                    }
                }
            }
        }

        void MultipleObjectUpdateHandler(Packet packet, Agent agent)
        {
            MultipleObjectUpdatePacket update = (MultipleObjectUpdatePacket)packet;

            for (int i = 0; i < update.ObjectData.Length; i++)
            {
                MultipleObjectUpdatePacket.ObjectDataBlock block = update.ObjectData[i];

                SimulationObject obj;
                if (SceneObjects.TryGetValue(block.ObjectLocalID, out obj))
                {
                    UpdateType type = (UpdateType)block.Type;
                    bool linked = ((type & UpdateType.Linked) != 0);
                    int pos = 0;

                    // FIXME: Handle linksets

                    if ((type & UpdateType.Position) != 0)
                    {
                        Vector3 newpos = new Vector3(block.Data, pos);
                        pos += 12;

                        obj.Prim.Position = newpos;
                    }
                    if ((type & UpdateType.Rotation) != 0)
                    {
                        Quaternion newrot = new Quaternion(block.Data, pos, true);
                        pos += 12;

                        obj.Prim.Rotation = newrot;
                    }
                    if ((type & UpdateType.Scale) != 0)
                    {
                        Vector3 newscale = new Vector3(block.Data, pos);
                        pos += 12;

                        // TODO: Use this
                        bool uniform = ((type & UpdateType.Uniform) != 0);

                        obj.Prim.Scale = newscale;
                    }

                    // Send the update out to everyone
                    ObjectUpdatePacket editedobj = Movement.BuildFullUpdate(obj.Prim, String.Empty, obj.Prim.RegionHandle, 0,
                        obj.Prim.Flags);
                    Server.UDP.BroadcastPacket(editedobj, PacketCategory.State);
                }
                else
                {
                    // Ghosted prim, send a kill message to this agent
                    // FIXME: Handle children
                    KillObjectPacket kill = new KillObjectPacket();
                    kill.ObjectData = new KillObjectPacket.ObjectDataBlock[1];
                    kill.ObjectData[0] = new KillObjectPacket.ObjectDataBlock();
                    kill.ObjectData[0].ID = block.ObjectLocalID;
                    Server.UDP.SendPacket(agent.AgentID, kill, PacketCategory.State);
                }
            }
        }

        void RequestObjectPropertiesFamilyHandler(Packet packet, Agent agent)
        {
            RequestObjectPropertiesFamilyPacket request = (RequestObjectPropertiesFamilyPacket)packet;
            ReportType type = (ReportType)request.ObjectData.RequestFlags;

            SimulationObject obj;
            if (SceneObjects.TryGetValue(request.ObjectData.ObjectID, out obj))
            {
                ObjectPropertiesFamilyPacket props = new ObjectPropertiesFamilyPacket();
                props.ObjectData.BaseMask = (uint)obj.Prim.Properties.Permissions.BaseMask;
                props.ObjectData.Category = (uint)obj.Prim.Properties.Category;
                props.ObjectData.Description = Utils.StringToBytes(obj.Prim.Properties.Description);
                props.ObjectData.EveryoneMask = (uint)obj.Prim.Properties.Permissions.EveryoneMask;
                props.ObjectData.GroupID = obj.Prim.Properties.GroupID;
                props.ObjectData.GroupMask = (uint)obj.Prim.Properties.Permissions.GroupMask;
                props.ObjectData.LastOwnerID = obj.Prim.Properties.LastOwnerID;
                props.ObjectData.Name = Utils.StringToBytes(obj.Prim.Properties.Name);
                props.ObjectData.NextOwnerMask = (uint)obj.Prim.Properties.Permissions.NextOwnerMask;
                props.ObjectData.ObjectID = obj.Prim.ID;
                props.ObjectData.OwnerID = obj.Prim.Properties.OwnerID;
                props.ObjectData.OwnerMask = (uint)obj.Prim.Properties.Permissions.OwnerMask;
                props.ObjectData.OwnershipCost = obj.Prim.Properties.OwnershipCost;
                props.ObjectData.RequestFlags = (uint)type;
                props.ObjectData.SalePrice = obj.Prim.Properties.SalePrice;
                props.ObjectData.SaleType = (byte)obj.Prim.Properties.SaleType;

                Server.UDP.SendPacket(agent.AgentID, props, PacketCategory.Transaction);
            }
            else
            {
                Logger.Log("RequestObjectPropertiesFamily sent for unknown object " +
                    request.ObjectData.ObjectID.ToString(), Helpers.LogLevel.Warning);
            }
        }

        void KillObject(SimulationObject obj)
        {
            SceneObjects.Remove(obj.Prim.LocalID, obj.Prim.ID);

            KillObjectPacket kill = new KillObjectPacket();
            kill.ObjectData = new KillObjectPacket.ObjectDataBlock[1];
            kill.ObjectData[0] = new KillObjectPacket.ObjectDataBlock();
            kill.ObjectData[0].ID = obj.Prim.LocalID;

            Server.UDP.BroadcastPacket(kill, PacketCategory.State);
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
                Logger.DebugLog("RayStart is equal to RayEnd, returning given location");
                return closestPoint;
            }

            Vector3 direction = Vector3.Normalize(rayEnd - rayStart);
            Ray ray = new Ray(rayStart, direction);

            // Get the mesh that has been transformed into world-space
            SimpleMesh mesh = null;
            if (obj.Prim.ParentID != 0)
            {
                SimulationObject parent;
                if (SceneObjects.TryGetValue(obj.Prim.ParentID, out parent))
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
                    Vector3 point0 = mesh.Vertices[mesh.Indices[i + 0]].Position;
                    Vector3 point1 = mesh.Vertices[mesh.Indices[i + 1]].Position;
                    Vector3 point2 = mesh.Vertices[mesh.Indices[i + 2]].Position;

                    if (RayTriangleIntersection(rayStart, direction, point0, point1, point2))
                    {
                        // HACK: Find the barycenter of this triangle. Would be better to have
                        // RayTriangleIntersection return the exact collision point
                        Vector3 center = (point0 + point1 + point2) * OO_THREE;

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
        /// <param name="origin">Origin point of the ray</param>
        /// <param name="direction">Unit vector representing the direction of the ray</param>
        /// <param name="vert0">Position of the first triangle corner</param>
        /// <param name="vert1">Position of the second triangle corner</param>
        /// <param name="vert2">Position of the third triangle corner</param>
        /// <returns>True if the ray passes through the triangle, otherwise false</returns>
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
