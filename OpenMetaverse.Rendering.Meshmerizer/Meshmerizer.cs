/*
 * Copyright (c) 2008, openmetaverse.org
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without 
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.org nor the names 
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
 * 
 * 
 * This code comes from the OpenSim project. Meshmerizer is written by dahlia
 * <dahliatrimble@gmail.com>
 */

using System;
using System.Collections.Generic;

namespace OpenMetaverse.Rendering
{
    [RendererName("Meshmerizer")]
    public class Meshmerizer : IRendering
    {
        public SimpleMesh GenerateSimpleMesh(Primitive prim, DetailLevel lod)
        {
            Path path = GeneratePath();
            Profile profile = GenerateProfile();

            MeshmerizerMesh meshmerizer = new MeshmerizerMesh();
            meshmerizer = GenerateMeshmerizerMesh(prim);

            // Create the vertex array
            List<Vertex> vertices = new List<Vertex>(meshmerizer.primMesh.coords.Count);
            for (int i = 0; i < meshmerizer.primMesh.coords.Count; i++)
            {
                Coord c = meshmerizer.primMesh.coords[i];
                Vertex vertex = new Vertex();
                vertex.Position = new Vector3(c.X, c.Y, c.Z);
                vertices.Add(vertex);
            }

            // Create the index array
            List<ushort> indices = new List<ushort>(meshmerizer.primMesh.faces.Count * 3);
            for (int i = 0; i < meshmerizer.primMesh.faces.Count; i++)
            {
                MeshmerizerFace f = meshmerizer.primMesh.faces[i];
                indices.Add((ushort)f.v1);
                indices.Add((ushort)f.v2);
                indices.Add((ushort)f.v3);
            }

            SimpleMesh mesh = new SimpleMesh();
            mesh.Prim = prim;
            mesh.Path = path;
            mesh.Profile = profile;
            mesh.Vertices = vertices;
            mesh.Indices = indices;

            return mesh;
        }

        public FacetedMesh GenerateFacetedMesh(Primitive prim, DetailLevel lod)
        {
            Path path = GeneratePath();
            Profile profile = GenerateProfile();

            FacetedMesh mesh = new FacetedMesh();
            mesh.Prim = prim;
            mesh.Path = path;
            mesh.Profile = profile;
            mesh.Faces = GenerateFaces(prim);

            return mesh;
        }

        public void TransformTexCoords(List<Vertex> vertices, Vector3 center, Primitive.TextureEntryFace teFace)
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

        private MeshmerizerMesh GenerateMeshmerizerMesh(Primitive prim)
        {
            PrimitiveBaseShape primShape = new PrimitiveBaseShape(prim);
            MeshmerizerMesh mesh = new MeshmerizerMesh();

            float pathShearX = primShape.PathShearX < 128 ? (float)primShape.PathShearX * 0.01f : (float)(primShape.PathShearX - 256) * 0.01f;
            float pathShearY = primShape.PathShearY < 128 ? (float)primShape.PathShearY * 0.01f : (float)(primShape.PathShearY - 256) * 0.01f;
            float pathBegin = (float)primShape.PathBegin * 2.0e-5f;
            float pathEnd = 1.0f - (float)primShape.PathEnd * 2.0e-5f;
            float pathScaleX = (float)(primShape.PathScaleX - 100) * 0.01f;
            float pathScaleY = (float)(primShape.PathScaleY - 100) * 0.01f;

            float profileBegin = (float)primShape.ProfileBegin * 2.0e-5f;
            float profileEnd = 1.0f - (float)primShape.ProfileEnd * 2.0e-5f;
            float profileHollow = (float)primShape.ProfileHollow * 2.0e-5f;

            int sides = 4;
            if ((primShape.ProfileCurve & 0x07) == (byte)ProfileShape.EquilateralTriangle)
                sides = 3;
            else if ((primShape.ProfileCurve & 0x07) == (byte)ProfileShape.Circle)
                sides = 24;
            else if ((primShape.ProfileCurve & 0x07) == (byte)ProfileShape.HalfCircle)
            {
                // half circle, prim is a sphere
                sides = 24;

                profileBegin = 0.5f * profileBegin + 0.5f;
                profileEnd = 0.5f * profileEnd + 0.5f;
            }

            int hollowSides = sides;
            if (primShape.HollowShape == HollowShape.Circle)
                hollowSides = 24;
            else if (primShape.HollowShape == HollowShape.Square)
                hollowSides = 4;
            else if (primShape.HollowShape == HollowShape.Triangle)
                hollowSides = 3;

            PrimMesh primMesh = new PrimMesh(sides, profileBegin, profileEnd, profileHollow, hollowSides);
            primMesh.topShearX = pathShearX;
            primMesh.topShearY = pathShearY;
            primMesh.pathCutBegin = pathBegin;
            primMesh.pathCutEnd = pathEnd;

            if (primShape.PathCurve == (byte)Extrusion.Straight)
            {
                primMesh.twistBegin = primShape.PathTwistBegin * 18 / 10;
                primMesh.twistEnd = primShape.PathTwist * 18 / 10;
                primMesh.taperX = pathScaleX;
                primMesh.taperY = pathScaleY;

                try
                {
                    primMesh.ExtrudeLinear();
                }
                catch (Exception)
                {
                    return null;
                }
            }
            else
            {
                primMesh.holeSizeX = (200 - primShape.PathScaleX) * 0.01f;
                primMesh.holeSizeY = (200 - primShape.PathScaleY) * 0.01f;
                primMesh.radius = 0.01f * primShape.PathRadiusOffset;
                primMesh.revolutions = 1.0f + 0.015f * primShape.PathRevolutions;
                primMesh.skew = 0.01f * primShape.PathSkew;
                primMesh.twistBegin = primShape.PathTwistBegin * 36 / 10;
                primMesh.twistEnd = primShape.PathTwist * 36 / 10;
                primMesh.taperX = primShape.PathTaperX * 0.01f;
                primMesh.taperY = primShape.PathTaperY * 0.01f;

                try
                {
                    primMesh.ExtrudeCircular();
                }
                catch (Exception)
                {
                    return null;
                }
            }

            int numCoords = primMesh.coords.Count;
            //int numFaces = primMesh.faces.Count;

            List<Coord> coords = primMesh.coords;
            for (int i = 0; i < numCoords; i++)
            {
                Coord c = coords[i];
                mesh.vertices.Add(new MeshmerizerVertex(c.X, c.Y, c.Z));
            }

            mesh.primMesh = primMesh;

            // trim the vertex and triangle lists to free up memory
            mesh.vertices.TrimExcess();
            mesh.triangles.TrimExcess();

            return mesh;
        }

        private List<Face> GenerateFaces(Primitive prim)
        {
            MeshmerizerMesh meshmerizer = new MeshmerizerMesh();
            meshmerizer = GenerateMeshmerizerMesh(prim);

            // Create the vertex array
            List<Vertex> vertices = new List<Vertex>(meshmerizer.primMesh.coords.Count);
            for (int i = 0; i < meshmerizer.primMesh.coords.Count; i++)
            {
                Coord c = meshmerizer.primMesh.coords[i];
                Vertex vertex = new Vertex();
                vertex.Position = new Vector3(c.X, c.Y, c.Z);
                vertices.Add(vertex);
            }

            // Create the index array
            List<ushort> indices = new List<ushort>(meshmerizer.primMesh.faces.Count * 3);
            for (int i = 0; i < meshmerizer.primMesh.faces.Count; i++)
            {
                MeshmerizerFace f = meshmerizer.primMesh.faces[i];
                indices.Add((ushort)f.v1);
                indices.Add((ushort)f.v2);
                indices.Add((ushort)f.v3);
            }

            Face face = new Face();
            face.Edge = new List<int>();
            face.TextureFace = prim.Textures.DefaultTexture;
            face.Vertices = vertices;
            face.Indices = indices;

            List<Face> faces = new List<Face>(1);
            faces.Add(face);

            return faces;
        }
    }
}
