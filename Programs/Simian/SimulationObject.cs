using System;
using System.Collections.Generic;
using OpenMetaverse;
using OpenMetaverse.Packets;
using OpenMetaverse.Rendering;

namespace Simian
{
    public class SimulationObject
    {
        /// <summary>Reference to the primitive object this class wraps</summary>
        public Primitive Prim;
        /// <summary>Link number, if this object is part of a linkset</summary>
        public int LinkNumber;
        /// <summary>True when an avatar grabs this object. Stops movement and
        /// rotation</summary>
        public bool Frozen;

        protected Simian Server;
        protected SimpleMesh[] Meshes = new SimpleMesh[4];
        protected SimpleMesh[] WorldTransformedMeshes = new SimpleMesh[4];

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

        public SimpleMesh GetMesh(DetailLevel lod)
        {
            int i = (int)lod;

            if (Meshes[i] != null)
            {
                return Meshes[i];
            }
            else
            {
                Primitive prim = (Primitive)Prim;
                SimpleMesh mesh = Server.Mesher.GenerateSimpleMesh(prim, lod);
                Meshes[i] = mesh;
                return mesh;
            }
        }

        public SimpleMesh GetWorldMesh(DetailLevel lod, SimulationObject parent)
        {
            int i = (int)lod;

            if (WorldTransformedMeshes[i] != null)
            {
                return WorldTransformedMeshes[i];
            }
            else
            {
                // Get the untransformed mesh
                SimpleMesh mesh = GetMesh(lod);

                // Copy to our new mesh
                SimpleMesh transformedMesh = new SimpleMesh();
                transformedMesh.Indices = new List<ushort>(mesh.Indices);
                transformedMesh.Path.Open = mesh.Path.Open;
                transformedMesh.Path.Points = new List<PathPoint>(mesh.Path.Points);
                transformedMesh.Prim = mesh.Prim;
                transformedMesh.Profile.Concave = mesh.Profile.Concave;
                transformedMesh.Profile.Faces = new List<ProfileFace>(mesh.Profile.Faces);
                transformedMesh.Profile.MaxX = mesh.Profile.MaxX;
                transformedMesh.Profile.MinX = mesh.Profile.MinX;
                transformedMesh.Profile.Open = mesh.Profile.Open;
                transformedMesh.Profile.Positions = new List<Vector3>(mesh.Profile.Positions);
                transformedMesh.Profile.TotalOutsidePoints = mesh.Profile.TotalOutsidePoints;
                transformedMesh.Vertices = new List<Vertex>(mesh.Vertices);

                // Construct a matrix to transform to world space
                Matrix4 transform = Matrix4.Identity;

                if (parent != null)
                {
                    // Apply parent rotation and translation first
                    transform *= Matrix4.CreateFromQuaternion(parent.Prim.Rotation);
                    transform *= Matrix4.CreateTranslation(parent.Prim.Position);
                }

                transform *= Matrix4.CreateScale(this.Prim.Scale);
                transform *= Matrix4.CreateFromQuaternion(this.Prim.Rotation);
                transform *= Matrix4.CreateTranslation(this.Prim.Position);

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

        public static ObjectUpdatePacket BuildFullUpdate(Primitive obj, ulong regionHandle, PrimFlags flags)
        {
            ObjectUpdatePacket update = new ObjectUpdatePacket();
            update.RegionData.RegionHandle = regionHandle;
            update.RegionData.TimeDilation = UInt16.MaxValue;
            update.ObjectData = new ObjectUpdatePacket.ObjectDataBlock[1];
            update.ObjectData[0] = BuildUpdateBlock(obj, regionHandle, flags);

            return update;
        }

        public static byte[] BuildObjectData(Vector3 position, Quaternion rotation, Vector3 velocity,
            Vector3 acceleration, Vector3 angularVelocity)
        {
            byte[] objectData = new byte[60];
            int pos = 0;
            position.GetBytes().CopyTo(objectData, pos);
            pos += 12;
            velocity.GetBytes().CopyTo(objectData, pos);
            pos += 12;
            acceleration.GetBytes().CopyTo(objectData, pos);
            pos += 12;
            rotation.GetBytes().CopyTo(objectData, pos);
            pos += 12;
            angularVelocity.GetBytes().CopyTo(objectData, pos);
            return objectData;
        }

        public static ObjectUpdatePacket.ObjectDataBlock BuildUpdateBlock(Primitive obj, ulong regionHandle, PrimFlags flags)
        {
            byte[] objectData = BuildObjectData(obj.Position, obj.Rotation, obj.Velocity,
                obj.Acceleration, obj.AngularVelocity);

            ObjectUpdatePacket.ObjectDataBlock update = new ObjectUpdatePacket.ObjectDataBlock();
            update.ClickAction = (byte)obj.ClickAction;
            update.CRC = 0;
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
            update.TextureEntry = obj.Textures == null ? new byte[0] : obj.Textures.ToBytes();
            update.Radius = obj.SoundRadius;
            update.Scale = obj.Scale;
            update.Sound = obj.Sound;
            update.State = obj.PrimData.State;
            update.Text = Utils.StringToBytes(obj.Text);
            update.UpdateFlags = (uint)flags;
            update.Data = obj.GenericData == null ? new byte[0] : obj.GenericData;

            return update;
        }
    }
}
