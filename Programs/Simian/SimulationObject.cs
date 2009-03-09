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
        /// <summary>Seat offset</summary>
        public Vector3 SitPosition;
        /// <summary>Seat rotation</summary>
        public Quaternion SitRotation = Quaternion.Identity;

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

        public SimpleMesh GetMesh(DetailLevel lod)
        {
            int i = (int)lod;

            if (Meshes == null) Meshes = new SimpleMesh[4];

            if (Meshes[i] != null)
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

        public SimpleMesh GetWorldMesh(DetailLevel lod, bool forceRebuild)
        {
            int i = (int)lod;

            if (WorldTransformedMeshes == null)
                WorldTransformedMeshes = new SimpleMesh[4];

            if (!forceRebuild && WorldTransformedMeshes[i] != null)
            {
                return WorldTransformedMeshes[i];
            }
            else
            {
                // Get the untransformed mesh
                SimpleMesh mesh = Server.Mesher.GenerateSimpleMesh(Prim, lod);

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
                for (int j = 0; j < mesh.Vertices.Count; j++)
                {
                    Vertex vertex = mesh.Vertices[j];
                    vertex.Position *= transform;
                    mesh.Vertices[j] = vertex;
                }

                WorldTransformedMeshes[i] = mesh;
                return mesh;
            }
        }

        public static ObjectUpdatePacket BuildFullUpdate(Primitive obj, ulong regionHandle, PrimFlags flags, uint crc)
        {
            ObjectUpdatePacket update = new ObjectUpdatePacket();
            update.RegionData.RegionHandle = regionHandle;
            update.RegionData.TimeDilation = UInt16.MaxValue;
            update.ObjectData = new ObjectUpdatePacket.ObjectDataBlock[1];
            update.ObjectData[0] = BuildUpdateBlock(obj, flags, crc);

            return update;
        }

        public static ObjectUpdatePacket.ObjectDataBlock BuildUpdateBlock(Primitive obj, PrimFlags flags, uint crc)
        {
            byte[] objectData = new byte[60];
            obj.Position.ToBytes(objectData, 0);
            obj.Velocity.ToBytes(objectData, 12);
            obj.Acceleration.ToBytes(objectData, 24);
            obj.Rotation.ToBytes(objectData, 36);
            obj.AngularVelocity.ToBytes(objectData, 48);

            ObjectUpdatePacket.ObjectDataBlock update = new ObjectUpdatePacket.ObjectDataBlock();
            update.ClickAction = (byte)obj.ClickAction;
            update.CRC = crc;
            update.ExtraParams = obj.GetExtraParamsBytes();
            update.Flags = (byte)flags;
            update.FullID = obj.ID;
            update.Gain = obj.SoundGain;
            update.ID = obj.LocalID;
            update.JointAxisOrAnchor = obj.JointAxisOrAnchor;
            update.JointPivot = obj.JointPivot;
            update.JointType = (byte)obj.Joint;
            update.Material = (byte)obj.PrimData.Material;
            update.MediaURL = Utils.StringToBytes(obj.MediaURL);
            update.NameValue = Utils.StringToBytes(NameValue.NameValuesToString(obj.NameValues));
            update.ObjectData = objectData;
            update.OwnerID = (obj.Properties != null ? obj.Properties.OwnerID : UUID.Zero);
            update.ParentID = obj.ParentID;
            update.PathBegin = Primitive.PackBeginCut(obj.PrimData.PathBegin);
            update.PathCurve = (byte)obj.PrimData.PathCurve;
            update.PathEnd = Primitive.PackEndCut(obj.PrimData.PathEnd);
            update.PathRadiusOffset = Primitive.PackPathTwist(obj.PrimData.PathRadiusOffset);
            update.PathRevolutions = Primitive.PackPathRevolutions(obj.PrimData.PathRevolutions);
            update.PathScaleX = Primitive.PackPathScale(obj.PrimData.PathScaleX);
            update.PathScaleY = Primitive.PackPathScale(obj.PrimData.PathScaleY);
            update.PathShearX = (byte)Primitive.PackPathShear(obj.PrimData.PathShearX);
            update.PathShearY = (byte)Primitive.PackPathShear(obj.PrimData.PathShearY);
            update.PathSkew = Primitive.PackPathTwist(obj.PrimData.PathSkew);
            update.PathTaperX = Primitive.PackPathTaper(obj.PrimData.PathTaperX);
            update.PathTaperY = Primitive.PackPathTaper(obj.PrimData.PathTaperY);
            update.PathTwist = Primitive.PackPathTwist(obj.PrimData.PathTwist);
            update.PathTwistBegin = Primitive.PackPathTwist(obj.PrimData.PathTwistBegin);
            update.PCode = (byte)obj.PrimData.PCode;
            update.ProfileBegin = Primitive.PackBeginCut(obj.PrimData.ProfileBegin);
            update.ProfileCurve = (byte)obj.PrimData.ProfileCurve;
            update.ProfileEnd = Primitive.PackEndCut(obj.PrimData.ProfileEnd);
            update.ProfileHollow = Primitive.PackProfileHollow(obj.PrimData.ProfileHollow);
            update.PSBlock = obj.ParticleSys.GetBytes();
            update.TextColor = obj.TextColor.GetBytes(true);
            update.TextureAnim = obj.TextureAnim.GetBytes();
            update.TextureEntry = obj.Textures == null ? Utils.EmptyBytes : obj.Textures.ToBytes();
            update.Radius = obj.SoundRadius;
            update.Scale = obj.Scale;
            update.Sound = obj.Sound;
            update.State = obj.PrimData.State;
            update.Text = Utils.StringToBytes(obj.Text);
            update.UpdateFlags = (uint)flags;
            update.Data = obj.GenericData == null ? Utils.EmptyBytes : obj.GenericData;

            return update;
        }
    }
}
