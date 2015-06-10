using System;

using OpenMetaverse;
using OpenMetaverse.Rendering;

namespace AvatarPreview
{
    /// <summary>
    /// Subclass of LindenMesh that adds vertex, index, and texture coordinate
    /// arrays suitable for pushing direct to OpenGL
    /// </summary>
    public class GLMesh : LindenMesh
    {
        /// <summary>
        /// Subclass of LODMesh that adds an index array suitable for pushing
        /// direct to OpenGL
        /// </summary>
        new public class LODMesh : LindenMesh.LODMesh
        {
            public ushort[] Indices;

            public override void LoadMesh(string filename)
            {
                base.LoadMesh(filename);

                // Generate the index array
                Indices = new ushort[_numFaces * 3];
                int current = 0;
                for (int i = 0; i < _numFaces; i++)
                {
                    Indices[current++] = (ushort)_faces[i].Indices[0];
                    Indices[current++] = (ushort)_faces[i].Indices[1];
                    Indices[current++] = (ushort)_faces[i].Indices[2];
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public struct GLData
        {
            public float[] Vertices;
            public ushort[] Indices;
            public float[] TexCoords;
            public Vector3 Center;
        }

        public GLData RenderData;

        public GLMesh(string name)
            : base(name)
        {
        }

        public override void LoadMesh(string filename)
        {
            base.LoadMesh(filename);

            float minX, minY, minZ;
            minX = minY = minZ = Single.MaxValue;
            float maxX, maxY, maxZ;
            maxX = maxY = maxZ = Single.MinValue;

            // Generate the vertex array
            RenderData.Vertices = new float[NumVertices * 3];
            int current = 0;
            for (int i = 0; i < NumVertices; i++)
            {
                RenderData.Vertices[current++] = Vertices[i].Coord.X;
                RenderData.Vertices[current++] = Vertices[i].Coord.Y;
                RenderData.Vertices[current++] = Vertices[i].Coord.Z;

                if (Vertices[i].Coord.X < minX)
                    minX = Vertices[i].Coord.X;
                else if (Vertices[i].Coord.X > maxX)
                    maxX = Vertices[i].Coord.X;

                if (Vertices[i].Coord.Y < minY)
                    minY = Vertices[i].Coord.Y;
                else if (Vertices[i].Coord.Y > maxY)
                    maxY = Vertices[i].Coord.Y;

                if (Vertices[i].Coord.Z < minZ)
                    minZ = Vertices[i].Coord.Z;
                else if (Vertices[i].Coord.Z > maxZ)
                    maxZ = Vertices[i].Coord.Z;
            }

            // Calculate the center-point from the bounding box edges
            RenderData.Center = new Vector3((minX + maxX) / 2, (minY + maxY) / 2, (minZ + maxZ) / 2);

            // Generate the index array
            RenderData.Indices = new ushort[NumFaces * 3];
            current = 0;
            for (int i = 0; i < NumFaces; i++)
            {
                RenderData.Indices[current++] = (ushort)Faces[i].Indices[0];
                RenderData.Indices[current++] = (ushort)Faces[i].Indices[1];
                RenderData.Indices[current++] = (ushort)Faces[i].Indices[2];
            }

            // Generate the texcoord array
            RenderData.TexCoords = new float[NumVertices * 2];
            current = 0;
            for (int i = 0; i < NumVertices; i++)
            {
                RenderData.TexCoords[current++] = Vertices[i].TexCoord.X;
                RenderData.TexCoords[current++] = Vertices[i].TexCoord.Y;
            }
        }

        public override void LoadLODMesh(int level, string filename)
        {
            LODMesh lod = new LODMesh();
            lod.LoadMesh(filename);
            LodMeshes[level] = lod;
        }
    }
}
