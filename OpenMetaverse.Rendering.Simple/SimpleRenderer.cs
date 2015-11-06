/*
 * Copyright (c) 2006-2016, openmetaverse.co
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without 
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.co nor the names 
 *   of its contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;

namespace OpenMetaverse.Rendering
{
    [RendererName("Simple Cube Renderer")]
    public class SimpleRenderer : IRendering
    {
        public SimpleMesh GenerateSimpleMesh(Primitive prim, DetailLevel lod)
        {
            Path path = GeneratePath();
            Profile profile = GenerateProfile();

            SimpleMesh mesh = new SimpleMesh();
            mesh.Prim = prim;
            mesh.Path = path;
            mesh.Profile = profile;
            mesh.Vertices = GenerateVertices();
            mesh.Indices = GenerateIndices();

            return mesh;
        }

        public SimpleMesh GenerateSimpleSculptMesh(Primitive prim, System.Drawing.Bitmap sculptTexture, DetailLevel lod)
        {
            return GenerateSimpleMesh(prim, lod);
        }

        public FacetedMesh GenerateFacetedMesh(Primitive prim, DetailLevel lod)
        {
            Path path = GeneratePath();
            Profile profile = GenerateProfile();

            FacetedMesh mesh = new FacetedMesh();
            mesh.Prim = prim;
            mesh.Path = path;
            mesh.Profile = profile;
            mesh.Faces = GenerateFaces(prim.Textures);

            return mesh;
        }

        public FacetedMesh GenerateFacetedSculptMesh(Primitive prim, System.Drawing.Bitmap sculptTexture, DetailLevel lod)
        {
            return GenerateFacetedMesh(prim, lod);
        }

        public void TransformTexCoords(List<Vertex> vertices, Vector3 center, Primitive.TextureEntryFace teFace, Vector3 primScale)
        {
            // Lalala...
        }

        private Path GeneratePath()
        {
            Path path = new Path();
            path.Points = new List<PathPoint>();
            return path;
        }

        private Profile GenerateProfile()
        {
            Profile profile = new Profile();
            profile.Faces = new List<ProfileFace>();
            profile.Positions = new List<Vector3>();
            return profile;
        }

        private List<Vertex> GenerateVertices()
        {
            List<Vertex> vertices = new List<Vertex>(8);

            Vertex v = new Vertex();

            // FIXME: Implement these
            v.Normal = Vector3.Zero;
            v.TexCoord = Vector2.Zero;

            v.Position = new Vector3(0.5f, 0.5f, -0.5f);
            vertices.Add(v);
            v.Position = new Vector3(0.5f, -0.5f, -0.5f);
            vertices.Add(v);
            v.Position = new Vector3(-0.5f, -0.5f, -0.5f);
            vertices.Add(v);
            v.Position = new Vector3(-0.5f, 0.5f, -0.5f);
            vertices.Add(v);
            v.Position = new Vector3(0.5f, 0.5f, 0.5f);
            vertices.Add(v);
            v.Position = new Vector3(0.5f, -0.5f, 0.5f);
            vertices.Add(v);
            v.Position = new Vector3(-0.5f, -0.5f, 0.5f);
            vertices.Add(v);
            v.Position = new Vector3(-0.5f, 0.5f, 0.5f);
            vertices.Add(v);

            return vertices;
        }

        private List<ushort> GenerateIndices()
        {
            ushort[] indices = new ushort[] {
                0, 1, 2, 
		        0, 2, 3, 
		        4, 7, 6, 
		        4, 6, 5, 
		        0, 4, 5,
		        0, 5, 1, 
		        1, 5, 6, 
		        1, 6, 2, 
		        2, 6, 7, 
		        2, 7, 3, 
		        4, 0, 3, 
		        4, 3, 7, 
            };

            return new List<ushort>(indices);
        }

        private List<Face> GenerateFaces(Primitive.TextureEntry te)
        {
            Face face = new Face();
            face.Edge = new List<int>();
            face.TextureFace = te.DefaultTexture;
            face.Vertices = GenerateVertices();
            face.Indices = GenerateIndices();

            List<Face> faces = new List<Face>(1);
            faces.Add(face);

            return faces;
        }
    }
}
