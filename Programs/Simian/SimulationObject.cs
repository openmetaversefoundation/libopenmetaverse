using System;
using System.Collections.Generic;
using OpenMetaverse;
using OpenMetaverse.Rendering;

namespace Simian
{
    public class SimulationObject
    {
        public Primitive Params;

        protected Simian Server;
        protected SimpleMesh[] Meshes = new SimpleMesh[4];
        protected SimpleMesh[] WorldTransformedMeshes = new SimpleMesh[4];
        protected Matrix4 WorldTransformation = Matrix4.Identity;
        protected bool WorldTransformationSet = false;

        public SimulationObject(Primitive prim, Simian server)
        {
            Params = prim;
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
                if (Params is Primitive)
                {
                    Primitive prim = (Primitive)Params;
                    SimpleMesh mesh = Server.Mesher.GenerateSimpleMesh(prim, lod);
                    Meshes[i] = mesh;
                    return mesh;
                }
                else
                {
                    throw new NotImplementedException("Avatar mesh generation is currently not supported");
                }
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
                    transform *= Matrix4.CreateFromQuaternion(parent.Params.Rotation);
                    transform *= Matrix4.CreateTranslation(parent.Params.Position);
                }

                transform *= Matrix4.CreateScale(this.Params.Scale);
                transform *= Matrix4.CreateFromQuaternion(this.Params.Rotation);
                transform *= Matrix4.CreateTranslation(this.Params.Position);

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
    }
}
