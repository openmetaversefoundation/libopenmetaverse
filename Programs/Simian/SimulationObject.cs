using System;
using System.Collections.Generic;
using OpenMetaverse;
using OpenMetaverse.Packets;
using OpenMetaverse.Rendering;

namespace Simian
{
    public class SimulationObject
    {
        // TODO: Frozen and RotationAxis might want to become properties that access the parent values

        /// <summary>Reference to the primitive object this class wraps</summary>
        public Primitive Prim;
        /// <summary>Link number, if this object is part of a linkset</summary>
        public int LinkNumber;
        /// <summary>True when an avatar grabs this object. Stops movement and
        /// rotation</summary>
        public bool Frozen;
        /// <summary>Holds the state of the object after each edit to enable undo</summary>
        public CircularQueue<Primitive> UndoSteps = new CircularQueue<Primitive>(10);
        /// <summary>Holds the state of the object after each undo to enable redo</summary>
        public CircularQueue<Primitive> RedoSteps = new CircularQueue<Primitive>(10);
        /// <summary>Axis of rotation for the object in the physics engine</summary>
        public Vector3 RotationAxis = Vector3.UnitY;
        /// <summary>A continual rotational impulse</summary>
        public Vector3 Torque;
        /// <summary>Last point the object was attached to (right hand by default)</summary>
        public AttachmentPoint LastAttachmentPoint = AttachmentPoint.RightHand;
        /// <summary>Saved seat offset. Applied to avatars that sit on this object</summary>
        public Vector3 SitPosition;
        /// <summary>Saved seat rotation. Applied to avatars that sit on this object</summary>
        public Quaternion SitRotation = Quaternion.Identity;
        /// <summary>Saved attachment offset. Applied to this object when it is attached
        /// to an avatar</summary>
        public Vector3 AttachmentPosition;
        /// <summary>Saved attachment rotation. Applied to this object when it is attached
        /// to an avatar</summary>
        public Quaternion AttachmentRotation = Quaternion.Identity;
        /// <summary>Rotation that is saved when this object is attached to an avatar.
        /// Will be applied to the object when it is dropped. This is always the world
        /// rotation, since it is only applicable to parent objects</summary>
        public Quaternion BeforeAttachmentRotation = Quaternion.Identity;

        protected Simian Server;
        protected SimpleMesh[] Meshes;
        protected SimpleMesh[] WorldTransformedMeshes;

        uint? crc;

        public uint CRC
        {
            get
            {
                if (crc.HasValue)
                    return crc.Value;

                int len = 0;
                byte[] bytes = new byte[1024];
                ObjectUpdatePacket.ObjectDataBlock block = BuildUpdateBlock(Prim, PrimFlags.None, 0);
                block.ToBytes(bytes, ref len);
                --len;
                
                CRC32 crc32 = new CRC32();
                crc32.Update(bytes, 0, len);

                crc = crc32.CRC;
                return crc.Value;
            }
            set
            {
                if (value == 0)
                    crc = null;
                else
                    crc = value;
            }
        }

        public SimulationObject(SimulationObject obj)
        {
            Prim = new Primitive(obj.Prim);
            Server = obj.Server;
            LinkNumber = obj.LinkNumber;
            Frozen = obj.Frozen;
            // Skip everything else because it can be lazily reconstructed
        }

        public SimulationObject(Primitive prim, Simian server)
        {
            Prim = prim;
            Server = server;
        }

        public SimulationObject GetLinksetParent()
        {
            // This is the root object
            if (Prim.ParentID == 0)
                return this;

            SimulationObject parent;
            if (Server.Scene.TryGetObject(Prim.ParentID, out parent))
            {
                // Check if this is the root object, but is attached to an avatar
                if (parent.Prim is Avatar)
                    return this;
                else
                    return parent;
            }
            else
            {
                Logger.Log(String.Format("Prim {0} has an unknown ParentID {1}", Prim.LocalID, Prim.ParentID),
                    Helpers.LogLevel.Warning);
                return this;
            }
        }

        public SimulationObject GetLinksetPrim(int linkNum)
        {
            Logger.DebugLog("Running expensive SimulationObject.GetLinksetPrim() function");

            return Server.Scene.FindObject(delegate(SimulationObject obj)
                { return obj.Prim.ParentID == this.Prim.ParentID && obj.LinkNumber == linkNum; });
        }

        public List<SimulationObject> GetChildren()
        {
            Logger.DebugLog("Running expensive SimulationObject.GetChildren() function");

            List<SimulationObject> children = new List<SimulationObject>();
            Server.Scene.ForEachObject(delegate(SimulationObject obj)
                { if (obj.Prim.ParentID == this.Prim.LocalID) children.Add(obj); });
            return children;
        }

        public float GetMass()
        {
            // FIXME:
            return 0f;
        }

        public float GetLinksetMass()
        {
            Logger.DebugLog("Running expensive SimulationObject.GetLinksetMass() function");

            // FIXME:
            return 0f;
        }

        public Vector3 GetSimulatorPosition()
        {
            SimulationObject parent;
            Vector3 position = Prim.Position;

            if (Prim.ParentID != 0 && Server.Scene.TryGetObject(Prim.ParentID, out parent))
                position += Vector3.Transform(parent.Prim.Position, Matrix4.CreateFromQuaternion(parent.Prim.Rotation));

            return position;
        }

        public Quaternion GetSimulatorRotation()
        {
            SimulationObject parent;
            Quaternion rotation = Prim.Rotation;

            if (Prim.ParentID != 0 && Server.Scene.TryGetObject(Prim.ParentID, out parent))
                rotation *= parent.Prim.Rotation;

            return rotation;
        }

        public void AddScriptLPS(int count)
        {
            // TODO: Do something with this
        }

        /// <summary>
        /// Copy the current state of the object into the next undo step
        /// </summary>
        public void CreateUndoStep()
        {
            UndoSteps.Enqueue(new Primitive(Prim));
        }

        public SimpleMesh GetMesh(DetailLevel lod, bool forceMeshing)
        {
            int i = (int)lod;

            if (Meshes == null) Meshes = new SimpleMesh[4];

            if (!forceMeshing && Meshes[i] != null)
            {
                return Meshes[i];
            }
            else
            {
                SimpleMesh mesh = Server.Mesher.GenerateSimpleMesh(Prim, lod);
                Meshes[i] = mesh;
                return mesh;
            }
        }

        public SimpleMesh GetWorldMesh(DetailLevel lod, bool forceMeshing, bool forceTransform)
        {
            int i = (int)lod;

            if (WorldTransformedMeshes == null)
                WorldTransformedMeshes = new SimpleMesh[4];

            if (!forceMeshing && !forceTransform && WorldTransformedMeshes[i] != null)
            {
                return WorldTransformedMeshes[i];
            }
            else
            {
                // Get the untransformed mesh
                SimpleMesh mesh = GetMesh(lod, forceMeshing);

                // Copy to our new mesh
                SimpleMesh transformedMesh = new SimpleMesh(mesh);

                // Construct a matrix to transform to world space
                Matrix4 transform = Matrix4.Identity;

                SimulationObject parent = GetLinksetParent();
                if (parent != this)
                {
                    // Apply parent rotation and translation first
                    transform *= Matrix4.CreateFromQuaternion(parent.Prim.Rotation);
                    transform *= Matrix4.CreateTranslation(parent.Prim.Position);
                }

                transform *= Matrix4.CreateScale(Prim.Scale);
                transform *= Matrix4.CreateFromQuaternion(Prim.Rotation);
                transform *= Matrix4.CreateTranslation(Prim.Position);

                // Transform the mesh
                for (int j = 0; j < transformedMesh.Vertices.Count; j++)
                {
                    Vertex vertex = transformedMesh.Vertices[j];
                    vertex.Position *= transform;
                    transformedMesh.Vertices[j] = vertex;
                }

                WorldTransformedMeshes[i] = transformedMesh;
                return transformedMesh;
            }
        }

        public static ObjectUpdatePacket.ObjectDataBlock BuildUpdateBlock(Primitive prim, PrimFlags flags, uint crc)
        {
            byte[] objectData = new byte[60];
            prim.Position.ToBytes(objectData, 0);
            prim.Velocity.ToBytes(objectData, 12);
            prim.Acceleration.ToBytes(objectData, 24);
            prim.Rotation.ToBytes(objectData, 36);
            prim.AngularVelocity.ToBytes(objectData, 48);

            ObjectUpdatePacket.ObjectDataBlock update = new ObjectUpdatePacket.ObjectDataBlock();
            update.ClickAction = (byte)prim.ClickAction;
            update.CRC = crc;
            update.ExtraParams = prim.GetExtraParamsBytes();
            update.Flags = (byte)flags;
            update.FullID = prim.ID;
            update.Gain = prim.SoundGain;
            update.ID = prim.LocalID;
            update.JointAxisOrAnchor = prim.JointAxisOrAnchor;
            update.JointPivot = prim.JointPivot;
            update.JointType = (byte)prim.Joint;
            update.Material = (byte)prim.PrimData.Material;
            update.MediaURL = Utils.StringToBytes(prim.MediaURL);
            update.NameValue = Utils.StringToBytes(NameValue.NameValuesToString(prim.NameValues));
            update.ObjectData = objectData;
            update.OwnerID = (prim.Properties != null ? prim.Properties.OwnerID : UUID.Zero);
            update.ParentID = prim.ParentID;
            update.PathBegin = Primitive.PackBeginCut(prim.PrimData.PathBegin);
            update.PathCurve = (byte)prim.PrimData.PathCurve;
            update.PathEnd = Primitive.PackEndCut(prim.PrimData.PathEnd);
            update.PathRadiusOffset = Primitive.PackPathTwist(prim.PrimData.PathRadiusOffset);
            update.PathRevolutions = Primitive.PackPathRevolutions(prim.PrimData.PathRevolutions);
            update.PathScaleX = Primitive.PackPathScale(prim.PrimData.PathScaleX);
            update.PathScaleY = Primitive.PackPathScale(prim.PrimData.PathScaleY);
            update.PathShearX = (byte)Primitive.PackPathShear(prim.PrimData.PathShearX);
            update.PathShearY = (byte)Primitive.PackPathShear(prim.PrimData.PathShearY);
            update.PathSkew = Primitive.PackPathTwist(prim.PrimData.PathSkew);
            update.PathTaperX = Primitive.PackPathTaper(prim.PrimData.PathTaperX);
            update.PathTaperY = Primitive.PackPathTaper(prim.PrimData.PathTaperY);
            update.PathTwist = Primitive.PackPathTwist(prim.PrimData.PathTwist);
            update.PathTwistBegin = Primitive.PackPathTwist(prim.PrimData.PathTwistBegin);
            update.PCode = (byte)prim.PrimData.PCode;
            update.ProfileBegin = Primitive.PackBeginCut(prim.PrimData.ProfileBegin);
            update.ProfileCurve = (byte)prim.PrimData.ProfileCurve;
            update.ProfileEnd = Primitive.PackEndCut(prim.PrimData.ProfileEnd);
            update.ProfileHollow = Primitive.PackProfileHollow(prim.PrimData.ProfileHollow);
            update.PSBlock = prim.ParticleSys.GetBytes();
            update.TextColor = prim.TextColor.GetBytes(true);
            update.TextureAnim = prim.TextureAnim.GetBytes();
            update.TextureEntry = prim.Textures == null ? Utils.EmptyBytes : prim.Textures.GetBytes();
            update.Radius = prim.SoundRadius;
            update.Scale = prim.Scale;
            update.Sound = prim.Sound;
            update.State = prim.PrimData.State;
            update.Text = Utils.StringToBytes(prim.Text);
            update.UpdateFlags = (uint)flags;
            switch (prim.PrimData.PCode)
            {
                case PCode.Grass:
                case PCode.Tree:
                case PCode.NewTree:
                    update.Data = new byte[1];
                    update.Data[0] = (byte)prim.TreeSpecies;
                    break;
                default:
                    if (prim.ScratchPad != null)
                    {
                        update.Data = new byte[prim.ScratchPad.Length];
                        Buffer.BlockCopy(prim.ScratchPad, 0, update.Data, 0, update.Data.Length);
                    }
                    else
                    {
                        update.Data = new byte[0];
                    }
                    break;
            }

            return update;
        }
    }
}
