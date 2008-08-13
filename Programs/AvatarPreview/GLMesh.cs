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
            RenderData.Vertices = new float[_numVertices * 3];
            int current = 0;
            for (int i = 0; i < _numVertices; i++)
            {
                RenderData.Vertices[current++] = _vertices[i].Coord.X;
                RenderData.Vertices[current++] = _vertices[i].Coord.Y;
                RenderData.Vertices[current++] = _vertices[i].Coord.Z;

                if (_vertices[i].Coord.X < minX)
                    minX = _vertices[i].Coord.X;
                else if (_vertices[i].Coord.X > maxX)
                    maxX = _vertices[i].Coord.X;

                if (_vertices[i].Coord.Y < minY)
                    minY = _vertices[i].Coord.Y;
                else if (_vertices[i].Coord.Y > maxY)
                    maxY = _vertices[i].Coord.Y;

                if (_vertices[i].Coord.Z < minZ)
                    minZ = _vertices[i].Coord.Z;
                else if (_vertices[i].Coord.Z > maxZ)
                    maxZ = _vertices[i].Coord.Z;
            }

            // Calculate the center-point from the bounding box edges
            RenderData.Center = new Vector3((minX + maxX) / 2, (minY + maxY) / 2, (minZ + maxZ) / 2);

            // Generate the index array
            RenderData.Indices = new ushort[_numFaces * 3];
            current = 0;
            for (int i = 0; i < _numFaces; i++)
            {
                RenderData.Indices[current++] = (ushort)_faces[i].Indices[0];
                RenderData.Indices[current++] = (ushort)_faces[i].Indices[1];
                RenderData.Indices[current++] = (ushort)_faces[i].Indices[2];
            }

            // Generate the texcoord array
            RenderData.TexCoords = new float[_numVertices * 2];
            current = 0;
            for (int i = 0; i < _numVertices; i++)
            {
                RenderData.TexCoords[current++] = _vertices[i].TexCoord.X;
                RenderData.TexCoords[current++] = _vertices[i].TexCoord.Y;
            }
        }

        public override void LoadLODMesh(int level, string filename)
        {
            LODMesh lod = new LODMesh();
            lod.LoadMesh(filename);
            _lodMeshes[level] = lod;
        }
    }
}
