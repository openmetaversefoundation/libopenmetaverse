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
	[RendererName("GPL Renderer")]
    public partial class GPLRenderer : IRendering
    {
        private const float F_PI = (float)Math.PI;
        private const int MIN_DETAIL_FACES = 6;
        private const int MIN_LOD = 0;
        private const int MAX_LOD = 3;

        private const int SCULPT_REZ_1 = 6;
        private const int SCULPT_REZ_2 = 8;
        private const int SCULPT_REZ_3 = 16;
        private const int SCULPT_REZ_4 = 32;

        private const float SCULPT_MIN_AREA = 0.002f;

        private static readonly float[] DETAIL_LEVELS = new float[] { 1f, 1.5f, 2.5f, 4f };
        private static readonly float[] TABLE_SCALE = new float[] { 1f, 1f, 1f, 0.5f, 0.707107f, 0.53f, 0.525f, 0.5f };

        public SimpleMesh GenerateSimpleMesh(Primitive prim, DetailLevel lod)
        {
            float detail = DETAIL_LEVELS[(int)lod];

            Path path = GeneratePath(prim.Data, detail);
            Profile profile = GenerateProfile(prim.Data, path, detail);

            SimpleMesh mesh = new SimpleMesh();
            mesh.Prim = prim;
            mesh.Path = path;
            mesh.Profile = profile;
            mesh.Vertices = GenerateVertices(prim.Data, detail, path, profile);
            mesh.Indices = GenerateIndices(prim.Data, path, profile);

            return mesh;
        }

        public FacetedMesh GenerateFacetedMesh(Primitive prim, DetailLevel lod)
        {
            float detail = DETAIL_LEVELS[(int)lod];

            Path path = GeneratePath(prim.Data, detail);
            Profile profile = GenerateProfile(prim.Data, path, detail);

            List<Vertex> vertices = GenerateVertices(prim.Data, detail, path, profile);

            FacetedMesh mesh = new FacetedMesh();
            mesh.Prim = prim;
            mesh.Path = path;
            mesh.Profile = profile;
            mesh.Faces = CreateVolumeFaces(prim, path, profile, vertices);

            return mesh;
        }

        private static List<Face> CreateVolumeFaces(Primitive prim, Path path,
            Profile profile, List<Vertex> vertices)
        {
            int numFaces = profile.Faces.Count;
            List<Face> faces = new List<Face>(numFaces);

            // Initialize faces with parameter data
            for (int i = 0; i < numFaces; i++)
            {
                ProfileFace pf = profile.Faces[i];

                Face face = new Face();
                face.Vertices = new List<Vertex>();
                face.Indices = new List<ushort>();
                face.Edge = new List<int>();

                face.BeginS = pf.Index;
                face.NumS = pf.Count;
                face.BeginT = 0;
                face.NumT = path.Points.Count;
                face.ID = i;

                // Set the type mask bits correctly
                if (prim.Data.ProfileHollow > 0f)
                    face.Mask |= FaceMask.Hollow;
                if (profile.Open)
                    face.Mask |= FaceMask.Open;
                if (pf.Cap)
                {
                    face.Mask |= FaceMask.Cap;
                    if (pf.Type == FaceType.PathBegin)
                        face.Mask |= FaceMask.Top;
                    else
                        face.Mask |= FaceMask.Bottom;
                }
                else if (pf.Type == FaceType.ProfileBegin || pf.Type == FaceType.ProfileEnd)
                {
                    face.Mask |= FaceMask.Flat;
                    face.Mask |= FaceMask.End;
                }
                else
                {
                    face.Mask |= FaceMask.Side;

                    if (pf.Flat)
                        face.Mask |= FaceMask.Flat;

                    if (pf.Type == FaceType.InnerSide)
                    {
                        face.Mask |= FaceMask.Inner;
                        if (pf.Flat && face.NumS > 2)
                            face.NumS *= 2; // Flat inner faces have to copy vert normals
                    }
                    else
                    {
                        face.Mask |= FaceMask.Outer;
                    }
                }

                faces.Add(face);
            }

            for (int i = 0; i < faces.Count; i++)
            {
                Face face = faces[i];
                BuildFace(ref face, prim.Data, vertices, path, profile, prim.Textures.GetFace((uint)i));
                faces[i] = face;
            }

            return faces;
        }

        private static List<Vertex> GenerateVertices(LLObject.ObjectData prim, float detail, Path path, Profile profile)
        {
            int sizeS = path.Points.Count;
            int sizeT = profile.Positions.Count;

            // Generate vertex positions
            List<Vertex> vertices = new List<Vertex>(sizeT * sizeS);
            for (int i = 0; i < sizeT * sizeS; i++)
                vertices.Add(new Vertex());

            // Run along the path
            for (int s = 0; s < sizeS; ++s)
            {
                Vector2 scale = path.Points[s].Scale;
                Quaternion rot = path.Points[s].Rotation;
                Vector3 pos = path.Points[s].Position;

                // Run along the profile
                for (int t = 0; t < sizeT; ++t)
                {
                    int m = s * sizeT + t;
                    Vertex vertex = vertices[m];

                    vertex.Position.X = profile.Positions[t].X * scale.X;
                    vertex.Position.Y = profile.Positions[t].Y * scale.Y;
                    vertex.Position.Z = 0f;

                    vertex.Position *= rot;
                    vertex.Position += pos;

                    vertices[m] = vertex;
                }
            }

            return vertices;
        }

        private static List<ushort> GenerateIndices(LLObject.ObjectData prim, Path path, Profile profile)
        {
            bool open = profile.Open;
            bool hollow = (prim.ProfileHollow > 0f);
            int sizeS = profile.Positions.Count;
            int sizeSOut = profile.TotalOutsidePoints;
            int sizeT = path.Points.Count;
            int s, t, i;
            List<ushort> indices = new List<ushort>();

            if (open)
            {
                if (hollow)
                {
                    // Open hollow -- much like the closed solid, except we 
                    // we need to stitch up the gap between s=0 and s=size_s-1
                    for (t = 0; t < sizeT - 1; t++)
                    {
                        // The outer face, first cut, and inner face
                        for (s = 0; s < sizeS - 1; s++)
                        {
                            i = s + t * sizeS;
                            indices.Add((ushort)i); // x,y
                            indices.Add((ushort)(i + 1)); // x+1,y
                            indices.Add((ushort)(i + sizeS)); // x,y+1

                            indices.Add((ushort)(i + sizeS)); // x,y+1
                            indices.Add((ushort)(i + 1)); // x+1,y
                            indices.Add((ushort)(i + sizeS + 1)); // x+1,y+1
                        }

                        // The other cut face
                        indices.Add((ushort)(s + t * sizeS)); // x,y
                        indices.Add((ushort)(0 + t * sizeS)); // x+1,y
                        indices.Add((ushort)(s + (t + 1) * sizeS)); // x,y+1

                        indices.Add((ushort)(s + (t + 1) * sizeS)); // x,y+1
                        indices.Add((ushort)(0 + t * sizeS)); // x+1,y
                        indices.Add((ushort)(0 + (t + 1) * sizeS)); // x+1,y+1
                    }

                    // Do the top and bottom caps, if necessary
                    if (path.Open)
                    {
                        // Top cap
                        int pt1 = 0;
                        int pt2 = sizeS - 1;
                        i = (sizeT - 1) * sizeS;

                        while (pt2 - pt1 > 1)
                        {
                            if (use_tri_1a2(profile, pt1, pt2))
                            {
                                indices.Add((ushort)(pt1 + i));
                                indices.Add((ushort)(pt1 + 1 + i));
                                indices.Add((ushort)(pt2 + i));
                                pt1++;
                            }
                            else
                            {
                                indices.Add((ushort)(pt1 + i));
                                indices.Add((ushort)(pt2 - 1 + i));
                                indices.Add((ushort)(pt2 + i));
                                pt2--;
                            }
                        }

                        // Bottom cap
                        pt1 = 0;
                        pt2 = sizeS - 1;

                        while (pt2 - pt1 > 1)
                        {
                            if (use_tri_1a2(profile, pt1, pt2))
                            {
                                indices.Add((ushort)pt1);
                                indices.Add((ushort)pt2);
                                indices.Add((ushort)(pt1 + 1));
                                pt1++;
                            }
                            else
                            {
                                indices.Add((ushort)pt1);
                                indices.Add((ushort)pt2);
                                indices.Add((ushort)(pt2 - 1));
                                pt2--;
                            }
                        }
                    }
                }
                else
                {
                    // Open solid
                    for (t = 0; t < sizeT - 1; t++)
                    {
                        // Outer face + 1 cut face
                        for (s = 0; s < sizeS - 1; s++)
                        {
                            i = s + t * sizeS;

                            indices.Add((ushort)i); // x,y
                            indices.Add((ushort)(i + 1)); // x+1,y
                            indices.Add((ushort)(i + sizeS)); // x,y+1

                            indices.Add((ushort)(i + sizeS)); // x,y+1
                            indices.Add((ushort)(i + 1)); // x+1,y
                            indices.Add((ushort)(i + sizeS + 1)); // x+1,y+1
                        }

                        // The other cut face
                        indices.Add((ushort)((sizeS - 1) + (t * sizeS))); // x,y
                        indices.Add((ushort)(0 + t * sizeS)); // x+1,y
                        indices.Add((ushort)((sizeS - 1) + (t + 1) * sizeS)); // x,y+1

                        indices.Add((ushort)((sizeS - 1) + (t + 1) * sizeS)); // x,y+1
                        indices.Add((ushort)(0 + (t * sizeS))); // x+1,y
                        indices.Add((ushort)(0 + (t + 1) * sizeS)); // x+1,y+1
                    }

                    // Do the top and bottom caps, if necessary
                    if (path.Open)
                    {
                        for (s = 0; s < sizeS - 2; s++)
                        {
                            indices.Add((ushort)(s + 1));
                            indices.Add((ushort)s);
                            indices.Add((ushort)(sizeS - 1));
                        }

                        // We've got a top cap
                        int offset = (sizeT - 1) * sizeS;

                        for (s = 0; s < sizeS - 2; s++)
                        {
                            // Inverted ordering from bottom cap
                            indices.Add((ushort)(offset + sizeS - 1));
                            indices.Add((ushort)(offset + s));
                            indices.Add((ushort)(offset + s + 1));
                        }
                    }
                }
            }
            else if (hollow)
            {
                // Closed hollow
                // Outer face
                for (t = 0; t < sizeT - 1; t++)
                {
                    for (s = 0; s < sizeSOut - 1; s++)
                    {
                        i = s + t * sizeS;

                        indices.Add((ushort)i); // x,y
                        indices.Add((ushort)(i + 1)); // x+1,y
                        indices.Add((ushort)(i + sizeS)); // x,y+1

                        indices.Add((ushort)(i + sizeS)); // x,y+1
                        indices.Add((ushort)(i + 1)); // x+1,y
                        indices.Add((ushort)(i + 1 + sizeS)); // x+1,y+1
                    }
                }

                // Inner face
                // Invert facing from outer face
                for (t = 0; t < sizeT - 1; t++)
                {
                    for (s = sizeSOut; s < sizeS - 1; s++)
                    {
                        i = s + t * sizeS;

                        indices.Add((ushort)i); // x,y
                        indices.Add((ushort)(i + 1)); // x+1,y
                        indices.Add((ushort)(i + sizeS)); // x,y+1

                        indices.Add((ushort)(i + sizeS)); // x,y+1
                        indices.Add((ushort)(i + 1)); // x+1,y
                        indices.Add((ushort)(i + 1 + sizeS)); // x+1,y+1
                    }
                }

                // Do the top and bottom caps, if necessary
                if (path.Open)
                {
                    // Top cap
                    int pt1 = 0;
                    int pt2 = sizeS - 1;
                    i = (sizeT - 1) * sizeS;

                    while (pt2 - pt1 > 1)
                    {
                        if (use_tri_1a2(profile, pt1, pt2))
                        {
                            indices.Add((ushort)(pt1 + i));
                            indices.Add((ushort)(pt2 + 1 + i));
                            indices.Add((ushort)(pt2 + i));
                            pt1++;
                        }
                        else
                        {
                            indices.Add((ushort)(pt1 + i));
                            indices.Add((ushort)(pt2 - 1 + i));
                            indices.Add((ushort)(pt2 + i));
                            pt2--;
                        }
                    }

                    // Bottom cap
                    pt1 = 0;
                    pt2 = sizeS - 1;

                    while (pt2 - pt1 > 1)
                    {
                        if (use_tri_1a2(profile, pt1, pt2))
                        {
                            indices.Add((ushort)pt1);
                            indices.Add((ushort)pt2);
                            indices.Add((ushort)(pt1 + 1));
                            pt1++;
                        }
                        else
                        {
                            indices.Add((ushort)pt1);
                            indices.Add((ushort)pt2);
                            indices.Add((ushort)(pt2 - 1));
                            pt2--;
                        }
                    }
                }
            }
            else
            {
                // Closed solid. Easy case
                for (t = 0; t < sizeT - 1; t++)
                {
                    for (s = 0; s < sizeS - 1; s++)
                    {
                        // Should wrap properly, but for now...
                        i = s + t * sizeS;

                        indices.Add((ushort)i); // x,y
                        indices.Add((ushort)(i + 1)); // x+1,y
                        indices.Add((ushort)(i + sizeS)); // x,y+1

                        indices.Add((ushort)(i + sizeS)); // x,y+1
                        indices.Add((ushort)(i + 1)); // x+1,y
                        indices.Add((ushort)(i + sizeS + 1)); // x+1,y+1
                    }
                }

                // Do the top and bottom caps, if necessary
                if (path.Open)
                {
                    // Bottom cap
                    for (s = 1; s < sizeS - 2; s++)
                    {
                        indices.Add((ushort)(s + 1));
                        indices.Add((ushort)s);
                        indices.Add((ushort)0);
                    }

                    // Top cap
                    int offset = (sizeT - 1) * sizeS;
                    for (s = 1; s < sizeS - 2; s++)
                    {
                        // Inverted ordering from bottom cap
                        indices.Add((ushort)offset);
                        indices.Add((ushort)(offset + s));
                        indices.Add((ushort)(offset + s + 1));
                    }
                }
            }

            return indices;
        }

        private static Profile GenerateProfile(LLObject.ObjectData prim, Path path, float detail)
        {
            Profile profile;

            if (detail < (float)MIN_LOD)
                detail = (float)MIN_LOD;

            // Generate the face data
            int i;
            float begin = prim.ProfileBegin;
            float end = prim.ProfileEnd;
            float hollow = prim.ProfileHollow;

            // Sanity check
            if (begin > end - 0.01f)
                begin = end - 0.01f;

            int faceNum = 0;

            switch (prim.ProfileCurve)
            {
                #region Squares
                case LLObject.ProfileCurve.Square:
                    {
                        profile = GenerateProfilePolygon(prim, 4, -0.375f, 1f);

                        #region Create Faces

                        if (path.Open)
                            profile.Faces.Add(CreateProfileCap(FaceType.PathBegin, profile.Positions.Count));

                        int iBegin = (int)Math.Floor(begin * 4f);
                        int iEnd = (int)Math.Floor(end * 4f + 0.999f);

                        for (i = iBegin; i < iEnd; i++)
                        {
                            FaceType type = (FaceType)((ushort)FaceType.OuterSide0 << i);
                            profile.Faces.Add(CreateProfileFace(faceNum++, 2, 1f, type, true));
                        }

                        #endregion Create Faces

                        for (i = 0; i < profile.Positions.Count; i++)
                        {
                            // Scale by 4 to generate proper tex coords
                            Vector3 point = profile.Positions[i];
                            point.Z *= 4f;
                            profile.Positions[i] = point;
                        }

                        if (hollow > 0f)
                        {
                            switch (prim.ProfileHole)
                            {
                                case LLObject.HoleType.Triangle:
                                    // This is the wrong offset, but it is what the official viewer uses
                                    GenerateProfileHole(prim, ref profile, true, 3f, -0.375f, hollow, 1f);
                                    break;
                                case LLObject.HoleType.Circle:
                                    // TODO: Compute actual detail levels for cubes
                                    GenerateProfileHole(prim, ref profile, false, MIN_DETAIL_FACES * detail, -0.375f, hollow, 1f);
                                    break;
                                case LLObject.HoleType.Same:
                                case LLObject.HoleType.Square:
                                default:
                                    GenerateProfileHole(prim, ref profile, true, 4, -0.375f, hollow, 1f);
                                    break;
                            }
                        }
                    }
                    break;
                #endregion Squares
                #region Triangles
                case LLObject.ProfileCurve.IsoTriangle:
                case LLObject.ProfileCurve.RightTriangle:
                case LLObject.ProfileCurve.EqualTriangle:
                    {
                        profile = GenerateProfilePolygon(prim, 3, 0f, 1f);

                        #region Create Faces

                        if (path.Open)
                            profile.Faces.Add(CreateProfileCap(FaceType.PathBegin, profile.Positions.Count));

                        int iBegin = (int)Math.Floor(begin * 3f);
                        int iEnd = (int)Math.Floor(end * 3f + 0.999f);

                        for (i = iBegin; i < iEnd; i++)
                        {
                            FaceType type = (FaceType)((ushort)FaceType.OuterSide0 << i);
                            profile.Faces.Add(CreateProfileFace(faceNum++, 2, 1f, type, true));
                        }

                        #endregion Create Faces

                        for (i = 0; i < profile.Positions.Count; i++)
                        {
                            // Scale by 3 to generate proper tex coords
                            Vector3 point = profile.Positions[i];
                            point.Z *= 3f;
                            profile.Positions[i] = point;
                        }

                        if (hollow > 0f)
                        {
                            // Swept triangles need smaller hollowness values,
                            // because the triangle doesn't fill the bounding box
                            float triangleHollow = hollow / 2f;

                            switch (prim.ProfileHole)
                            {
                                case LLObject.HoleType.Circle:
                                    GenerateProfileHole(prim, ref profile, false, MIN_DETAIL_FACES * detail, 0f, triangleHollow, 1f);
                                    break;
                                case LLObject.HoleType.Square:
                                    GenerateProfileHole(prim, ref profile, true, 4f, 0f, triangleHollow, 1f);
                                    break;
                                case LLObject.HoleType.Same:
                                case LLObject.HoleType.Triangle:
                                default:
                                    GenerateProfileHole(prim, ref profile, true, 3f, 0f, triangleHollow, 1f);
                                    break;
                            }
                        }
                    }
                    break;
                #endregion Triangles
                #region Circles
                case LLObject.ProfileCurve.Circle:
                    {
                        float circleDetail = MIN_DETAIL_FACES * detail;

                        // If this has a square hollow, we should adjust the
                        // number of faces a bit so that the geometry lines up
                        if (hollow > 0f && prim.ProfileHole == LLObject.HoleType.Square)
                        {
                            // Snap to the next multiple of four sides,
                            // so that corners line up
                            circleDetail = (float)Math.Ceiling(circleDetail / 4f) * 4f;
                        }

                        int sides = (int)circleDetail;

                        // FIXME: Handle sculpted prims at some point
                        //if (is_sculpted)
                        //    sides = sculpt_sides(detail);

                        profile = GenerateProfilePolygon(prim, sides, 0f, 1f);

                        #region Create Faces

                        if (path.Open)
                            profile.Faces.Add(CreateProfileCap(FaceType.PathBegin, profile.Positions.Count));

                        if (profile.Open && prim.ProfileHollow == 0f)
                            profile.Faces.Add(CreateProfileFace(0, profile.Positions.Count - 1, 0f, FaceType.OuterSide0, false));
                        else
                            profile.Faces.Add(CreateProfileFace(0, profile.Positions.Count, 0f, FaceType.OuterSide0, false));

                        #endregion Create Faces

                        if (hollow > 0f)
                        {
                            switch (prim.ProfileHole)
                            {
                                case LLObject.HoleType.Square:
                                    GenerateProfileHole(prim, ref profile, true, 4f, 0f, hollow, 1f);
                                    break;
                                case LLObject.HoleType.Triangle:
                                    GenerateProfileHole(prim, ref profile, true, 3f, 0f, hollow, 1f);
                                    break;
                                case LLObject.HoleType.Circle:
                                case LLObject.HoleType.Same:
                                default:
                                    GenerateProfileHole(prim, ref profile, false, circleDetail, 0f, hollow, 1f);
                                    break;
                            }
                        }
                    }
                    break;
                #endregion Circles
                #region HalfCircles
                case LLObject.ProfileCurve.HalfCircle:
                    {
                        // Number of faces is cut in half because it's only a half-circle
                        float circleDetail = MIN_DETAIL_FACES * detail * 0.5f;

                        // If this has a square hollow, we should adjust the
                        // number of faces a bit so that the geometry lines up
                        if (hollow > 0f && prim.ProfileHole == LLObject.HoleType.Square)
                        {
                            // Snap to the next multiple of four sides (div 2),
                            // so that corners line up
                            circleDetail = (float)Math.Ceiling(circleDetail / 2f) * 2f;
                        }

                        profile = GenerateProfilePolygon(prim, (int)Math.Floor(circleDetail), 0.5f, 0.5f);

                        #region Create Faces

                        if (path.Open)
                            profile.Faces.Add(CreateProfileCap(FaceType.PathBegin, profile.Positions.Count));

                        if (profile.Open && prim.ProfileHollow == 0f)
                            profile.Faces.Add(CreateProfileFace(0, profile.Positions.Count - 1, 0f, FaceType.OuterSide0, false));
                        else
                            profile.Faces.Add(CreateProfileFace(0, profile.Positions.Count, 0f, FaceType.OuterSide0, false));

                        #endregion Create Faces

                        if (hollow > 0f)
                        {
                            switch (prim.ProfileHole)
                            {
                                case LLObject.HoleType.Square:
                                    GenerateProfileHole(prim, ref profile, true, 2f, 0.5f, hollow, 0.5f);
                                    break;
                                case LLObject.HoleType.Triangle:
                                    GenerateProfileHole(prim, ref profile, true, 3f, 0.5f, hollow, 0.5f);
                                    break;
                                case LLObject.HoleType.Circle:
                                case LLObject.HoleType.Same:
                                default:
                                    GenerateProfileHole(prim, ref profile, false, circleDetail, 0.5f, hollow, 0.5f);
                                    break;
                            }
                        }

                        // Special case for openness of sphere
                        if (prim.ProfileEnd - prim.ProfileEnd < 1f)
                        {
                            profile.Open = true;
                        }
                        else if (hollow == 0f)
                        {
                            profile.Open = false;
                            Vector3 first = profile.Positions[0];
                            profile.Positions.Add(first);
                        }
                    }
                    break;
                #endregion HalfCircles
                default:
                    throw new RenderingException("Unknown profile curve type " + prim.ProfileCurve.ToString());
            }

            // Bottom cap
            if (path.Open)
                profile.Faces.Add(CreateProfileCap(FaceType.PathEnd, profile.Positions.Count));

            if (profile.Open)
            {
                // Interior edge caps
                profile.Faces.Add(CreateProfileFace(profile.Positions.Count - 1, 2, 0.5f, FaceType.ProfileBegin, true));

                if (prim.ProfileHollow > 0f)
                    profile.Faces.Add(CreateProfileFace(profile.TotalOutsidePoints - 1, 2, 0.5f, FaceType.ProfileEnd, true));
                else
                    profile.Faces.Add(CreateProfileFace(profile.Positions.Count - 2, 2, 0.5f, FaceType.ProfileEnd, true));
            }

            return profile;
        }

        private static void GenerateProfileHole(LLObject.ObjectData prim, ref Profile profile, bool flat, float sides,
            float offset, float boxHollow, float angScale)
        {
            // Total add has number of vertices on outside
            profile.TotalOutsidePoints = profile.Positions.Count;

            // Create the hole
            Profile hole = GenerateProfilePolygon(prim, (int)Math.Floor(sides), offset, angScale);
            // FIXME: Should we overwrite profile.Values with hole.Values?

            // Apply the hollow scale modifier
            for (int i = 0; i < hole.Positions.Count; i++)
            {
                Vector3 point = hole.Positions[i];
                point *= boxHollow;
                hole.Positions[i] = point;
            }

            // Reverse the order
            hole.Positions.Reverse();

            // Add the hole to the profile
            profile.Positions.AddRange(hole.Positions);

            // Create the inner side face for the hole
            ProfileFace innerFace = CreateProfileFace(profile.TotalOutsidePoints,
                profile.Positions.Count - profile.TotalOutsidePoints, 0f, FaceType.InnerSide, flat);
            profile.Faces.Add(innerFace);
        }

        private static Path GeneratePath(LLObject.ObjectData prim, float detail)
        {
            Path path;
            path.Open = true;
            int np = 2; // Hardcode for line
            float step;

            switch (prim.PathCurve)
            {
                default:
                case LLObject.PathCurve.Line:
                    {
                        // Take the begin/end twist into account for detail
                        np = (int)Math.Floor(Math.Abs(prim.PathTwistBegin - prim.PathTwist) * 3.5f * (detail - 0.5f)) + 2;
                        path.Points = new List<PathPoint>(np);

                        step = 1f / (np - 1);

                        Vector2 startScale = prim.PathBeginScale;
                        Vector2 endScale = prim.PathEndScale;

                        for (int i = 0; i < np; i++)
                        {
                            PathPoint point = new PathPoint();

                            float t = MathHelper.Lerp(prim.PathBegin, prim.PathEnd, (float)i * step);
                            point.Position = new Vector3(
                                MathHelper.Lerp(0, prim.PathShearX, t),
                                MathHelper.Lerp(0, prim.PathShearY, t),
                                t - 0.5f);
                            point.Rotation = Quaternion.CreateFromAxisAngle(MathHelper.Lerp(F_PI * prim.PathTwistBegin, F_PI * prim.PathTwist, t), 0f, 0f, 1f);
                            point.Scale.X = MathHelper.Lerp(startScale.X, endScale.X, t);
                            point.Scale.Y = MathHelper.Lerp(startScale.Y, endScale.Y, t);
                            point.TexT = t;

                            path.Points.Add(point);
                        }
                    }
                    break;
                case LLObject.PathCurve.Circle:
                    {
                        // Increase the detail as the revolutions and twist increase
                        float twistMag = Math.Abs(prim.PathTwistBegin - prim.PathTwist);

                        int sides = (int)Math.Floor(
                            Math.Floor(MIN_DETAIL_FACES * detail + twistMag * 3.5f * (detail - 0.5f))
                            * prim.PathRevolutions);

                        // FIXME: Sculpty support
                        //if (is_sculpted)
                        //    sides = sculpt_sides(detail);

                        path = GeneratePathPolygon(prim, sides);
                    }
                    break;
                case LLObject.PathCurve.Circle2:
                    {
                        if (prim.PathEnd - prim.PathBegin >= 0.99f && prim.PathScaleX >= 0.99f)
                            path.Open = false;

                        path = GeneratePathPolygon(prim, (int)Math.Floor(MIN_DETAIL_FACES * detail));

                        float t = 0f;
                        float tStep = 1f / path.Points.Count;
                        float toggle = 0.5f;

                        for (int i = 0; i < path.Points.Count; i++)
                        {
                            PathPoint point = path.Points[i];
                            point.Position.X = toggle;
                            path.Points[i] = point;

                            if (toggle == 0.5f)
                                toggle = -0.5f;
                            else
                                toggle = 0.5f;
                            t += tStep;
                        }
                    }
                    break;
                case LLObject.PathCurve.Test:
                    throw new NotImplementedException("PathCurve.Test is not supported");
            }

            if (prim.PathTwist != prim.PathTwistBegin)
                path.Open = true;

            return path;
        }

        private static Profile GenerateProfilePolygon(LLObject.ObjectData prim, int sides, float offset, float angScale)
        {
            // Create a polygon by starting at (1, 0) and proceeding counterclockwise generating vectors
            Profile profile = new Profile();
            profile.Positions = new List<Vector3>();
            profile.Faces = new List<ProfileFace>();

            float scale = 0.5f;
            float t, tStep, tFirst, tFraction, ang, angStep;
            Vector3 pt1, pt2;

            float begin = prim.ProfileBegin;
            float end = prim.ProfileEnd;

            tStep = 1f / sides;
            angStep = 2f * F_PI * tStep * angScale;

            // Scale to have size "match" scale. Compensates to get object to generally fill bounding box
            int totalSides = MathHelper.Round(sides / angScale);

            if (totalSides < 8)
                scale = TABLE_SCALE[totalSides];

            tFirst = (float)Math.Floor(begin * sides) / (float)sides;

            // pt1 is the first point on the fractional face.
            // Starting t and ang values for the first face
            t = tFirst;
            ang = 2f * F_PI * (t * angScale + offset);
            pt1 = new Vector3((float)Math.Cos(ang) * scale, (float)Math.Sin(ang) * scale, t);

            // Increment to the next point.
            // pt2 is the end point on the fractional face
            t += tStep;
            ang += angStep;
            pt2 = new Vector3((float)Math.Cos(ang) * scale, (float)Math.Sin(ang) * scale, t);

            tFraction = (begin - tFirst) * sides;

            // Only use if it's not almost exactly on an edge
            if (tFraction < 0.9999f)
            {
                Vector3 newPt = Vector3.Lerp(pt1, pt2, tFraction);
                float ptX = newPt.X;

                if (ptX < profile.MinX)
                    profile.MinX = ptX;
                else if (ptX > profile.MaxX)
                    profile.MaxX = ptX;

                profile.Positions.Add(newPt);
            }

            // There's lots of potential here for floating point error to generate unneeded extra points
            while (t < end)
            {
                // Iterate through all the integer steps of t.
                pt1 = new Vector3((float)Math.Cos(ang) * scale, (float)Math.Sin(ang) * scale, t);

                float ptX = pt1.X;
                if (ptX < profile.MinX)
                    profile.MinX = ptX;
                else if (ptX > profile.MaxX)
                    profile.MaxX = ptX;

                profile.Positions.Add(pt1);

                t += tStep;
                ang += angStep;
            }

            // pt1 is the first point on the fractional face
            // pt2 is the end point on the fractional face
            pt2 = new Vector3((float)Math.Cos(ang) * scale, (float)Math.Sin(ang) * scale, t);

            // Find the fraction that we need to add to the end point
            tFraction = (end - (t - tStep)) * sides;
            if (tFraction > 0.0001f)
            {
                Vector3 newPt = Vector3.Lerp(pt1, pt2, tFraction);

                float ptX = newPt.X;
                if (ptX < profile.MinX)
                    profile.MinX = ptX;
                else if (ptX > profile.MaxX)
                    profile.MaxX = ptX;

                profile.Positions.Add(newPt);
            }

            // If we're sliced, the profile is open
            if ((end - begin) * angScale < 0.99f)
            {
                if ((end - begin) * angScale > 0.5f)
                    profile.Concave = true;
                else
                    profile.Concave = false;

                profile.Open = true;

                // Put center point if not hollow
                if (prim.ProfileHollow == 0f)
                    profile.Positions.Add(Vector3.Zero);
            }
            else
            {
                // The profile isn't open
                profile.Open = false;
                profile.Concave = false;
            }

            return profile;
        }

        private static Path GeneratePathPolygon(LLObject.ObjectData prim, int sides)
        {
            return GeneratePathPolygon(prim, sides, 0f, 1f, 1f);
        }

        private static Path GeneratePathPolygon(LLObject.ObjectData prim, int sides, float startOff, float endScale,
            float twistScale)
        {
            Path path = new Path();
            path.Points = new List<PathPoint>();

            float revolutions = prim.PathRevolutions;
            float skew = prim.PathSkew;
            float skewMag = (float)Math.Abs(skew);
            float holeX = prim.PathScaleX * (1f - skewMag);
            float holeY = prim.PathScaleY;

            // Calculate taper begin/end for x,y (Negative means taper the beginning)
            float taperXBegin = 1f;
            float taperXEnd = 1f - prim.PathTaperX;
            float taperYBegin = 1f;
            float taperYEnd = 1f - prim.PathTaperY;

            if (taperXEnd > 1f)
            {
                // Flip tapering
                taperXBegin = 2f - taperXEnd;
                taperXEnd = 1f;
            }
            if (taperYEnd > 1f)
            {
                // Flip tapering
                taperYBegin = 2f - taperYEnd;
                taperYEnd = 1f;
            }

            // For spheres, the radius is usually zero
            float radiusStart = 0.5f;
            if (sides < 8)
                radiusStart = TABLE_SCALE[sides];

            // Scale the radius to take the hole size into account
            radiusStart *= 1f - holeY;

            // Now check the radius offset to calculate the start,end radius. (Negative means
            // decrease the start radius instead)
            float radiusEnd = radiusStart;
            float radiusOffset = prim.PathRadiusOffset;
            if (radiusOffset < 0f)
                radiusStart *= 1f + radiusOffset;
            else
                radiusEnd *= 1f - radiusOffset;

            // Is the path NOT a closed loop?
            path.Open =
                ((prim.PathEnd * endScale - prim.PathBegin < 1f) ||
                (skewMag > 0.001f) ||
                (Math.Abs(taperXEnd - taperXBegin) > 0.001d) ||
                (Math.Abs(taperYEnd - taperYBegin) > 0.001d) ||
                (Math.Abs(radiusEnd - radiusStart) > 0.001d));

            float ang, c, s;
            Quaternion twist = Quaternion.Identity;
            Quaternion qang = Quaternion.Identity;
            PathPoint point;
            Vector3 pathAxis = new Vector3(1f, 0f, 0f);
            float twistBegin = prim.PathTwistBegin * twistScale;
            float twistEnd = prim.PathTwist * twistScale;

            // We run through this once before the main loop, to make sure
            // the path begins at the correct cut
            float step = 1f / sides;
            float t = prim.PathBegin;
            ang = 2f * F_PI * revolutions * t;
            s = (float)Math.Sin(ang) * MathHelper.Lerp(radiusStart, radiusEnd, t);
            c = (float)Math.Cos(ang) * MathHelper.Lerp(radiusStart, radiusEnd, t);

            point = new PathPoint();
            point.Position = new Vector3(
                0 + MathHelper.Lerp(0, prim.PathShearX, s) +
                0 + MathHelper.Lerp(-skew, skew, t) * 0.5f,
                c + MathHelper.Lerp(0, prim.PathShearY, s),
                s);
            point.Scale.X = holeX * MathHelper.Lerp(taperXBegin, taperXEnd, t);
            point.Scale.Y = holeY * MathHelper.Lerp(taperYBegin, taperYEnd, t);
            point.TexT = t;

            // Twist rotates the path along the x,y plane
            twist = Quaternion.CreateFromAxisAngle(MathHelper.Lerp(twistBegin, twistEnd, t) * 2f * F_PI - F_PI, 0f, 0f, 1f);
            // Rotate the point around the circle's center
            qang = Quaternion.CreateFromAxisAngle(pathAxis, ang);
            point.Rotation = twist * qang;

            path.Points.Add(point);
            t += step;

            // Snap to a quantized parameter, so that cut does not
            // affect most sample points
            t = ((int)(t * sides)) / (float)sides;

            // Run through the non-cut dependent points
            point = new PathPoint();
            while (t < prim.PathEnd)
            {
                ang = 2f * F_PI * revolutions * t;
                c = (float)Math.Cos(ang) * MathHelper.Lerp(radiusStart, radiusEnd, t);
                s = (float)Math.Sin(ang) * MathHelper.Lerp(radiusStart, radiusEnd, t);

                point.Position = new Vector3(
                    0 + MathHelper.Lerp(0, prim.PathShearX, s) +
                    0 + MathHelper.Lerp(-skew, skew, t) * 0.5f,
                    c + MathHelper.Lerp(0, prim.PathShearY, s),
                    s);

                point.Scale.X = holeX * MathHelper.Lerp(taperXBegin, taperXEnd, t);
                point.Scale.Y = holeY * MathHelper.Lerp(taperYBegin, taperYEnd, t);
                point.TexT = t;

                // Twist rotates the path along the x,y plane
                twist = Quaternion.CreateFromAxisAngle(MathHelper.Lerp(twistBegin, twistEnd, t) * 2f * F_PI - F_PI, 0f, 0f, 1f);
                // Rotate the point around the circle's center
                qang = Quaternion.CreateFromAxisAngle(pathAxis, ang);
                point.Rotation = twist * qang;

                path.Points.Add(point);
                t += step;
            }

            // Make one final pass for the end cut
            t = prim.PathEnd;
            point = new PathPoint();
            ang = 2f * F_PI * revolutions * t;
            c = (float)Math.Cos(ang) * MathHelper.Lerp(radiusStart, radiusEnd, t);
            s = (float)Math.Sin(ang) * MathHelper.Lerp(radiusStart, radiusEnd, t);

            point.Position = new Vector3(
                MathHelper.Lerp(0, prim.PathShearX, s) + MathHelper.Lerp(-skew, skew, t) * 0.5f,
                c + MathHelper.Lerp(0, prim.PathShearY, s),
                s);
            point.Scale.X = holeX * MathHelper.Lerp(taperXBegin, taperXEnd, t);
            point.Scale.Y = holeY * MathHelper.Lerp(taperYBegin, taperYEnd, t);
            point.TexT = t;

            // Twist rotates the path along the x,y plane
            twist = Quaternion.CreateFromAxisAngle(MathHelper.Lerp(twistBegin, twistEnd, t) * 2f * F_PI - F_PI, 0f, 0f, 1f);
            qang = Quaternion.CreateFromAxisAngle(pathAxis, ang);
            point.Rotation = twist * qang;

            path.Points.Add(point);

            return path;
        }

        private static ProfileFace CreateProfileCap(FaceType type, int count)
        {
            ProfileFace face = new ProfileFace();
            face.Index = 0;
            face.Count = count;
            face.ScaleU = 1f;
            face.Cap = true;
            face.Type = type;

            return face;
        }

        private static ProfileFace CreateProfileFace(int index, int count, float scaleU, FaceType type, bool flat)
        {
            ProfileFace face = new ProfileFace();
            face.Index = index;
            face.Count = count;
            face.ScaleU = scaleU;
            face.Flat = flat;
            face.Cap = false;
            face.Type = type;

            return face;
        }

        private static bool use_tri_1a2(Profile profilePoints, int pt1, int pt2)
        {
            // Use the profile points instead of the mesh, since you want
            // the un-transformed profile distances
            Vector3 p1 = profilePoints.Positions[pt1];
            Vector3 p2 = profilePoints.Positions[pt2];
            Vector3 pa = profilePoints.Positions[pt1 + 1];
            Vector3 pb = profilePoints.Positions[pt2 - 1];

            p1.Z = 0f;
            p2.Z = 0f;
            pa.Z = 0f;
            pb.Z = 0f;

            // Use area of triangle to determine backfacing
            float area_1a2, area_1ba, area_21b, area_2ab;

            area_1a2 =
                (p1.X * pa.Y - pa.X * p1.Y) +
                (pa.X * p2.Y - p2.X * pa.Y) +
                (p2.X * p1.Y - p1.X * p2.Y);

            area_1ba =
                (p1.X * pb.Y - pb.X * p1.Y) +
                (pb.X * pa.Y - pa.X * pb.Y) +
                (pa.X * p1.Y - p1.X * pa.Y);

            area_21b =
                (p2.X * p1.Y - p1.X * p2.Y) +
                (p1.X * pb.Y - pb.X * p1.Y) +
                (pb.X * p2.Y - p2.X * pb.Y);

            area_2ab =
                (p2.X * pa.Y - pa.X * p2.Y) +
                (pa.X * pb.Y - pb.X * pa.Y) +
                (pb.X * p2.Y - p2.X * pb.Y);

            bool use_tri_1a2 = true;
            bool tri_1a2 = true;
            bool tri_21b = true;

            if (area_1a2 < 0)
                tri_1a2 = false;
            if (area_2ab < 0)
                tri_1a2 = false; // Can't use, because it contains point b
            if (area_21b < 0)
                tri_21b = false;
            if (area_1ba < 0)
                tri_21b = false; // Can't use, because it contains point b

            if (!tri_1a2)
            {
                use_tri_1a2 = false;
            }
            else if (!tri_21b)
            {
                use_tri_1a2 = true;
            }
            else
            {
                Vector3 d1 = p1 - pa;
                Vector3 d2 = p2 - pb;

                if (d1.LengthSquared() < d2.LengthSquared())
                    use_tri_1a2 = true;
                else
                    use_tri_1a2 = false;
            }

            return use_tri_1a2;
        }
    }
}
