/*
 *  This file is part of OpenMetaverse.Rendering.GPL.
 *
 *  libprimrender is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License version 2.0 as
 *  published by the Free Software Foundation.
 *
 *  libprimrender is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with OpenMetaverse.Rendering.GPL.  If not, see
 * <http://www.gnu.org/licenses/>.
 */


using System;
using System.Collections.Generic;

namespace OpenMetaverse.Rendering
{
    public partial class GPLRenderer : IRendering
    {
        private static void BuildFace(ref Face face, LLObject.ObjectData prim, List<Vertex> vertices, Path path,
            Profile profile, LLObject.TextureEntryFace teFace)
        {
            if (teFace != null)
                face.TextureFace = teFace;
            else
                throw new ArgumentException("teFace cannot be null");

            face.Vertices.Clear();

            if ((face.Mask & FaceMask.Cap) != 0)
            {
                if (((face.Mask & FaceMask.Hollow) == 0) &&
                    ((face.Mask & FaceMask.Open) == 0) &&
                    (prim.PathBegin == 0f) &&
                    (prim.ProfileCurve == LLObject.ProfileCurve.Square) &&
                    (prim.PathCurve == LLObject.PathCurve.Line))
                {
                    CreateUnCutCubeCap(ref face, vertices, path, profile);
                }
                else
                {
                    CreateCap(ref face, vertices, path, profile);
                }
            }
            else if ((face.Mask & FaceMask.End) != 0 || (face.Mask & FaceMask.Side) != 0)
            {
                CreateSide(ref face, prim, vertices, path, profile);
            }
            else
            {
                throw new RenderingException("Unknown/uninitialized face type");
            }
        }

        private static void CreateUnCutCubeCap(ref Face face, List<Vertex> primVertices, Path path, Profile profile)
        {
            int maxS = profile.Positions.Count;
            int maxT = path.Points.Count;

            int gridSize = (profile.Positions.Count - 1) / 4;
            int quadCount = gridSize * gridSize;
            //int numVertices = (gridSize + 1) * (gridSize + 1);
            //int numIndices = quadCount * 4;

            int offset = 0;
            if ((face.Mask & FaceMask.Top) != 0)
                offset = (maxT - 1) * maxS;
            else
                offset = face.BeginS;

            Vertex[] corners = new Vertex[4];
            Vertex baseVert;

            for (int t = 0; t < 4; t++)
            {
                corners[t].Position = primVertices[offset + (gridSize * t)].Position;
                corners[t].TexCoord.X = profile.Positions[gridSize * t].X + 0.5f;
                corners[t].TexCoord.Y = 0.5f - profile.Positions[gridSize * t].Y;
            }

            baseVert.Normal =
                ((corners[1].Position - corners[0].Position) %
                (corners[2].Position - corners[1].Position));
            baseVert.Normal = Vector3.Normalize(baseVert.Normal);

            if ((face.Mask & FaceMask.Top) != 0)
            {
                baseVert.Normal *= -1f;
            }
            else
            {
                // Swap the UVs on the U(X) axis for top face
                Vector2 swap;

                swap = corners[0].TexCoord;
                corners[0].TexCoord = corners[3].TexCoord;
                corners[3].TexCoord = swap;

                swap = corners[1].TexCoord;
                corners[1].TexCoord = corners[2].TexCoord;
                corners[2].TexCoord = swap;
            }

            baseVert.Binormal = CalcBinormalFromTriangle(
                corners[0].Position, corners[0].TexCoord,
                corners[1].Position, corners[1].TexCoord,
                corners[2].Position, corners[2].TexCoord);

            for (int t = 0; t < 4; t++)
            {
                corners[t].Binormal = baseVert.Binormal;
                corners[t].Normal = baseVert.Normal;
            }

            int vtop = face.Vertices.Count;

            for (int gx = 0; gx < gridSize + 1; gx++)
            {
                for (int gy = 0; gy < gridSize + 1; gy++)
                {
                    Vertex newVert = new Vertex();
                    LerpPlanarVertex(
                        corners[0],
                        corners[1],
                        corners[3],
                        ref newVert,
                        (float)gx / (float)gridSize,
                        (float)gy / (float)gridSize);

                    face.Vertices.Add(newVert);

                    if (gx == 0 && gy == 0)
                        face.MinExtent = face.MaxExtent = newVert.Position;
                    else
                        UpdateMinMax(ref face, newVert.Position);
                }
            }

            face.Center = (face.MinExtent + face.MaxExtent) * 0.5f;

            int[] idxs = new int[] { 0, 1, gridSize + 2, gridSize + 2, gridSize + 1, 0 };

            for (int gx = 0; gx < gridSize; gx++)
            {
                for (int gy = 0; gy < gridSize; gy++)
                {
                    if ((face.Mask & FaceMask.Top) != 0)
                    {
                        for (int i = 5; i >= 0; i--)
                            face.Indices.Add((ushort)(vtop + (gy * (gridSize + 1)) + gx + idxs[i]));
                    }
                    else
                    {
                        for (int i = 0; i < 6; i++)
                            face.Indices.Add((ushort)(vtop + (gy * (gridSize + 1)) + gx + idxs[i]));
                    }
                }
            }
        }

        private static void CreateCap(ref Face face, List<Vertex> primVertices, Path path, Profile profile)
        {
            int i;
            int numVertices = profile.Positions.Count;
            //int numIndices = (numVertices - 2) * 3;

            int maxS = profile.Positions.Count;
            int maxT = path.Points.Count;

            face.Center = Vector3.Zero;

            int offset = 0;
            if ((face.Mask & FaceMask.Top) != 0)
                offset = (maxT - 1) * maxS;
            else
                offset = face.BeginS;

            // Figure out the normal, assume all caps are flat faces.
            // Cross product to get normals
            Vector2 cuv;
            Vector2 minUV = Vector2.Zero;
            Vector2 maxUV = Vector2.Zero;

            // Copy the vertices into the array
            for (i = 0; i < numVertices; i++)
            {
                Vertex vertex = new Vertex();

                if ((face.Mask & FaceMask.Top) != 0)
                {
                    vertex.Position = primVertices[i + offset].Position;
                    vertex.TexCoord.X = profile.Positions[i].X + 0.5f;
                    vertex.TexCoord.Y = profile.Positions[i].Y + 0.5f;
                }
                else
                {
                    // Mirror for underside
                    vertex.Position = primVertices[(numVertices - 1) - i].Position;
                    vertex.TexCoord.X = profile.Positions[i].X + 0.5f;
                    vertex.TexCoord.Y = 0.5f - profile.Positions[i].Y;
                }

                if (i == 0)
                {
                    face.MinExtent = face.MaxExtent = primVertices[offset].Position;
                    minUV = maxUV = primVertices[offset].TexCoord;
                }
                else
                {
                    UpdateMinMax(ref face, vertex.Position);
                    UpdateMinMax(ref minUV, ref maxUV, vertex.TexCoord);
                }

                face.Vertices.Add(vertex);
            }

            face.Center = (face.MinExtent + face.MaxExtent) * 0.5f;
            cuv = (minUV + maxUV) * 0.5f;

            Vector3 binormal = CalcBinormalFromTriangle(
                face.Center, cuv,
                face.Vertices[0].Position, face.Vertices[0].TexCoord,
                face.Vertices[1].Position, face.Vertices[1].TexCoord);
            binormal.Normalize();

            Vector3 d0 = face.Center - face.Vertices[0].Position;
            Vector3 d1 = face.Center - face.Vertices[1].Position;
            Vector3 normal = ((face.Mask & FaceMask.Top) != 0) ? (d0 % d1) : (d1 % d0);
            normal.Normalize();

            // If not hollow and not open create a center point in the cap
            if ((face.Mask & FaceMask.Hollow) == 0 && (face.Mask & FaceMask.Open) == 0)
            {
                Vertex vertex = new Vertex();
                vertex.Position = face.Center;
                vertex.Normal = normal;
                vertex.Binormal = binormal;
                vertex.TexCoord = cuv;

                face.Vertices.Add(vertex);
                numVertices++;
            }

            for (i = 0; i < numVertices; i++)
            {
                Vertex vertex = face.Vertices[i];
                vertex.Binormal = binormal;
                vertex.Normal = normal;
                face.Vertices[i] = vertex;
            }

            if ((face.Mask & FaceMask.Hollow) != 0)
            {
                if ((face.Mask & FaceMask.Top) != 0)
                {
                    // HOLLOW TOP
                    int pt1 = 0;
                    int pt2 = numVertices - 1;
                    i = 0;

                    while (pt2 - pt1 > 1)
                    {
                        if (use_tri_1a2(profile, pt1, pt2))
                        {
                            face.Indices.Add((ushort)pt1);
                            face.Indices.Add((ushort)(pt1 + 1));
                            face.Indices.Add((ushort)pt2);
                            pt1++;
                        }
                        else
                        {
                            face.Indices.Add((ushort)pt1);
                            face.Indices.Add((ushort)(pt2 - 1));
                            face.Indices.Add((ushort)pt2);
                            pt2--;
                        }
                    }
                }
                else
                {
                    // HOLLOW BOTTOM
                    int pt1 = 0;
                    int pt2 = numVertices - 1;
                    i = 0;

                    while (pt2 - pt1 > 1)
                    {
                        // Flipped backfacing from top
                        if (use_tri_1a2(profile, pt1, pt2))
                        {
                            face.Indices.Add((ushort)pt1);
                            face.Indices.Add((ushort)pt2);
                            face.Indices.Add((ushort)(pt1 + 1));
                            pt1++;
                        }
                        else
                        {
                            face.Indices.Add((ushort)pt1);
                            face.Indices.Add((ushort)pt2);
                            face.Indices.Add((ushort)(pt2 - 1));
                            pt2--;
                        }
                    }
                }
            }
            else
            {
                // SOLID OPEN TOP
                // SOLID CLOSED TOP
                // SOLID OPEN BOTTOM
                // SOLID CLOSED BOTTOM

                // Not hollow, generate the triangle fan.
                // This is a tri-fan, so we reuse the same first point for all triangles
                for (i = 0; i < numVertices - 2; i++)
                {
                    face.Indices.Add((ushort)(numVertices - 1));
                    face.Indices.Add((ushort)i);
                    face.Indices.Add((ushort)(i + 1));
                }
            }
        }

        private static void CreateSide(ref Face face, LLObject.ObjectData prim, List<Vertex> primVertices, Path path,
            Profile profile)
        {
            bool flat = (face.Mask & FaceMask.Flat) != 0;

            int maxS = profile.Positions.Count;
            int s, t, i;
            float ss, tt;

            int numVertices = face.NumS * face.NumT;
            int numIndices = (face.NumS - 1) * (face.NumT - 1) * 6;

            face.Center = Vector3.Zero;

            int beginSTex = (int)Math.Floor(profile.Positions[face.BeginS].Z);
            int numS =
                (((face.Mask & FaceMask.Inner) != 0) && ((face.Mask & FaceMask.Flat) != 0) && face.NumS > 2) ?
                    face.NumS / 2 :
                    face.NumS;

            int curVertex = 0;

            // Copy the vertices into the array
            for (t = face.BeginT; t < face.BeginT + face.NumT; t++)
            {
                tt = path.Points[t].TexT;

                for (s = 0; s < numS; s++)
                {
                    if ((face.Mask & FaceMask.End) != 0)
                    {
                        if (s != 0)
                            ss = 1f;
                        else
                            ss = 0f;
                    }
                    else
                    {
                        // Get s value for tex-coord
                        if (!flat)
                            ss = profile.Positions[face.BeginS + s].Z;
                        else
                            ss = profile.Positions[face.BeginS + s].Z - beginSTex;
                    }

                    // Check to see if this triangle wraps around the array
                    if (face.BeginS + s >= maxS)
                        i = face.BeginS + s + maxS * (t - 1); // We're wrapping
                    else
                        i = face.BeginS + s + maxS * t;

                    Vertex vertex = new Vertex();
                    vertex.Position = primVertices[i].Position;
                    vertex.TexCoord = new Vector2(ss, tt);
                    vertex.Normal = Vector3.Zero;
                    vertex.Binormal = Vector3.Zero;

                    if (curVertex == 0)
                        face.MinExtent = face.MaxExtent = primVertices[i].Position;
                    else
                        UpdateMinMax(ref face, primVertices[i].Position);

                    face.Vertices.Add(vertex);
                    ++curVertex;

                    if (((face.Mask & FaceMask.Inner) != 0) && ((face.Mask & FaceMask.Flat) != 0) && face.NumS > 2 && s > 0)
                    {
                        vertex.Position = primVertices[i].Position;
                        //vertex.TexCoord = new Vector2(ss, tt);
                        //vertex.Normal = Vector3.Zero;
                        //vertex.Binormal = Vector3.Zero;

                        face.Vertices.Add(vertex);
                        ++curVertex;
                    }
                }

                if (((face.Mask & FaceMask.Inner) != 0) && ((face.Mask & FaceMask.Flat) != 0) && face.NumS > 2)
                {
                    if ((face.Mask & FaceMask.Open) != 0)
                        s = numS - 1;
                    else
                        s = 0;

                    i = face.BeginS + s + maxS * t;
                    ss = profile.Positions[face.BeginS + s].Z - beginSTex;

                    Vertex vertex = new Vertex();
                    vertex.Position = primVertices[i].Position;
                    vertex.TexCoord = new Vector2(ss, tt);
                    vertex.Normal = Vector3.Zero;
                    vertex.Binormal = Vector3.Zero;

                    UpdateMinMax(ref face, vertex.Position);

                    face.Vertices.Add(vertex);
                    ++curVertex;
                }
            }

            face.Center = (face.MinExtent + face.MaxExtent) * 0.5f;

            bool flatFace = ((face.Mask & FaceMask.Flat) != 0);

            // Now we generate the indices
            for (t = 0; t < (face.NumT - 1); t++)
            {
                for (s = 0; s < (face.NumS - 1); s++)
                {
                    face.Indices.Add((ushort)(s + face.NumS * t)); // Bottom left
                    face.Indices.Add((ushort)(s + 1 + face.NumS * (t + 1))); // Top right
                    face.Indices.Add((ushort)(s + face.NumS * (t + 1))); // Top left
                    face.Indices.Add((ushort)(s + face.NumS * t)); // Bottom left
                    face.Indices.Add((ushort)(s + 1 + face.NumS * t)); // Bottom right
                    face.Indices.Add((ushort)(s + 1 + face.NumS * (t + 1))); // Top right

                    face.Edge.Add((face.NumS - 1) * 2 * t + s * 2 + 1); // Bottom left/top right neighbor face

                    if (t < face.NumT - 2) // Top right/top left neighbor face
                        face.Edge.Add((face.NumS - 1) * 2 * (t + 1) + s * 2 + 1);
                    else if (face.NumT <= 3 || path.Open) // No neighbor
                        face.Edge.Add(-1);
                    else // Wrap on T
                        face.Edge.Add(s * 2 + 1);

                    if (s > 0) // Top left/bottom left neighbor face
                        face.Edge.Add((face.NumS - 1) * 2 * t + s * 2 - 1);
                    else if (flatFace || profile.Open) // No neighbor
                        face.Edge.Add(-1);
                    else // Wrap on S
                        face.Edge.Add((face.NumS - 1) * 2 * t + (face.NumS - 2) * 2 + 1);

                    if (t > 0) // Bottom left/bottom right neighbor face
                        face.Edge.Add((face.NumS - 1) * 2 * (t - 1) + s * 2);
                    else if (face.NumT <= 3 || path.Open) // No neighbor
                        face.Edge.Add(-1);
                    else // Wrap on T
                        face.Edge.Add((face.NumS - 1) * 2 * (face.NumT - 2) + s * 2);

                    if (s < face.NumS - 2) // Bottom right/top right neighbor face
                        face.Edge.Add((face.NumS - 1) * 2 * t + (s + 1) * 2);
                    else if (flatFace || profile.Open) // No neighbor
                        face.Edge.Add(-1);
                    else // Wrap on S
                        face.Edge.Add((face.NumS - 1) * 2 * t);

                    face.Edge.Add((face.NumS - 1) * 2 * t + s * 2); // Top right/bottom left neighbor face	
                }
            }

            // Generate normals, loop through each triangle
            for (i = 0; i < face.Indices.Count / 3; i++)
            {
                Vertex v0 = face.Vertices[face.Indices[i * 3 + 0]];
                Vertex v1 = face.Vertices[face.Indices[i * 3 + 1]];
                Vertex v2 = face.Vertices[face.Indices[i * 3 + 2]];

                // Calculate triangle normal
                Vector3 norm = (v0.Position - v1.Position) % (v0.Position - v2.Position);

                // Calculate binormal
                Vector3 binorm = CalcBinormalFromTriangle(v0.Position, v0.TexCoord, v1.Position, v1.TexCoord,
                    v2.Position, v2.TexCoord);

                // Add triangle normal to vertices
                for (int j = 0; j < 3; j++)
                {
                    Vertex vertex = face.Vertices[face.Indices[i * 3 + j]];
                    vertex.Normal += norm;
                    vertex.Binormal += binorm;
                    face.Vertices[face.Indices[i * 3 + j]] = vertex;
                }

                // Even out quad contributions
                if (i % 2 == 0)
                {
                    Vertex vertex = face.Vertices[face.Indices[i * 3 + 2]];
                    vertex.Normal += norm;
                    vertex.Binormal += binorm;
                    face.Vertices[face.Indices[i * 3 + 2]] = vertex;
                }
                else
                {
                    Vertex vertex = face.Vertices[face.Indices[i * 3 + 1]];
                    vertex.Normal += norm;
                    vertex.Binormal += binorm;
                    face.Vertices[face.Indices[i * 3 + 1]] = vertex;
                }
            }

            // Adjust normals based on wrapping and stitching
            Vector3 test1 =
                face.Vertices[0].Position -
                face.Vertices[face.NumS * (face.NumT - 2)].Position;

            Vector3 test2 =
                face.Vertices[face.NumS - 1].Position -
                face.Vertices[face.NumS * (face.NumT - 2) +
                face.NumS - 1].Position;

            bool sBottomConverges = (test1.LengthSquared() < 0.000001f);
            bool sTopConverges = (test2.LengthSquared() < 0.000001f);

            // TODO: Sculpt support
            Primitive.SculptType sculptType = Primitive.SculptType.None;

            if (sculptType == Primitive.SculptType.None)
            {
                if (!path.Open)
                {
                    // Wrap normals on T
                    for (i = 0; i < face.NumS; i++)
                    {
                        Vector3 norm = face.Vertices[i].Normal + face.Vertices[face.NumS * (face.NumT - 1) + i].Normal;

                        Vertex vertex = face.Vertices[i];
                        vertex.Normal = norm;
                        face.Vertices[i] = vertex;

                        vertex = face.Vertices[face.NumS * (face.NumT - 1) + i];
                        vertex.Normal = norm;
                        face.Vertices[face.NumS * (face.NumT - 1) + i] = vertex;
                    }
                }

                if (!profile.Open && !sBottomConverges)
                {
                    // Wrap normals on S
                    for (i = 0; i < face.NumT; i++)
                    {
                        Vector3 norm = face.Vertices[face.NumS * i].Normal + face.Vertices[face.NumS * i + face.NumS - 1].Normal;

                        Vertex vertex = face.Vertices[face.NumS * i];
                        vertex.Normal = norm;
                        face.Vertices[face.NumS * i] = vertex;

                        vertex = face.Vertices[face.NumS * i + face.NumS - 1];
                        vertex.Normal = norm;
                        face.Vertices[face.NumS * i + face.NumS - 1] = vertex;
                    }
                }

                if (prim.PathCurve == LLObject.PathCurve.Circle &&
                    prim.ProfileCurve == LLObject.ProfileCurve.HalfCircle)
                {
                    if (sBottomConverges)
                    {
                        // All lower S have same normal
                        Vector3 unitX = new Vector3(1f, 0f, 0f);

                        for (i = 0; i < face.NumT; i++)
                        {
                            Vertex vertex = face.Vertices[face.NumS * i];
                            vertex.Normal = unitX;
                            face.Vertices[face.NumS * i] = vertex;
                        }
                    }

                    if (sTopConverges)
                    {
                        // All upper S have same normal
                        Vector3 negUnitX = new Vector3(-1f, 0f, 0f);

                        for (i = 0; i < face.NumT; i++)
                        {
                            Vertex vertex = face.Vertices[face.NumS * i + face.NumS - 1];
                            vertex.Normal = negUnitX;
                            face.Vertices[face.NumS * i + face.NumS - 1] = vertex;
                        }
                    }
                }
            }
            else
            {
                // FIXME: Sculpt support
            }

            // Normalize normals and binormals
            for (i = 0; i < face.Vertices.Count; i++)
            {
                Vertex vertex = face.Vertices[i];
                vertex.Normal.Normalize();
                vertex.Binormal.Normalize();
                face.Vertices[i] = vertex;
            }
        }

        private static void LerpPlanarVertex(Vertex v0, Vertex v1, Vertex v2, ref Vertex vout, float coef01, float coef02)
        {
            vout.Position = v0.Position + ((v1.Position - v0.Position) * coef01) + ((v2.Position - v0.Position) * coef02);
            vout.TexCoord = v0.TexCoord + ((v1.TexCoord - v0.TexCoord) * coef01) + ((v2.TexCoord - v0.TexCoord) * coef02);
            vout.Normal = v0.Normal;
            vout.Binormal = v0.Binormal;
        }

        private static void UpdateMinMax(ref Face face, Vector3 position)
        {
            if (face.MinExtent.X > position.X)
                face.MinExtent.X = position.X;
            if (face.MinExtent.Y > position.Y)
                face.MinExtent.Y = position.Y;
            if (face.MinExtent.Z > position.Z)
                face.MinExtent.Z = position.Z;

            if (face.MaxExtent.X < position.X)
                face.MaxExtent.X = position.X;
            if (face.MaxExtent.Y < position.Y)
                face.MaxExtent.Y = position.Y;
            if (face.MaxExtent.Z < position.Z)
                face.MaxExtent.Z = position.Z;
        }

        private static void UpdateMinMax(ref Vector2 min, ref Vector2 max, Vector2 current)
        {
            if (min.X > current.X)
                min.X = current.X;
            if (min.Y > current.Y)
                min.Y = current.Y;

            if (max.X < current.X)
                max.X = current.X;
            if (max.Y < current.Y)
                max.Y = current.Y;
        }

        private static Vector3 CalcBinormalFromTriangle(Vector3 pos0, Vector2 tex0, Vector3 pos1,
            Vector2 tex1, Vector3 pos2, Vector2 tex2)
        {
            Vector3 rx0 = new Vector3(pos0.X, tex0.X, tex0.Y);
            Vector3 rx1 = new Vector3(pos1.X, tex1.X, tex1.Y);
            Vector3 rx2 = new Vector3(pos2.X, tex2.X, tex2.Y);

            Vector3 ry0 = new Vector3(pos0.Y, tex0.X, tex0.Y);
            Vector3 ry1 = new Vector3(pos1.Y, tex1.X, tex1.Y);
            Vector3 ry2 = new Vector3(pos2.Y, tex2.X, tex2.Y);

            Vector3 rz0 = new Vector3(pos0.Z, tex0.X, tex0.Y);
            Vector3 rz1 = new Vector3(pos1.Z, tex1.X, tex1.Y);
            Vector3 rz2 = new Vector3(pos2.Z, tex2.X, tex2.Y);

            Vector3 r0 = (rx0 - rx1) % (rx0 - rx2);
            Vector3 r1 = (ry0 - ry1) % (ry0 - ry2);
            Vector3 r2 = (rz0 - rz1) % (rz0 - rz2);

            if (r0.X != 0f && r1.X != 0f && r2.X != 0f)
            {
                return new Vector3(
                    -r0.Z / r0.X,
                    -r1.Z / r1.X,
                    -r2.Z / r2.X);
            }
            else
            {
                return new Vector3(0f, 1f, 0f);
            }
        }
    }
}
