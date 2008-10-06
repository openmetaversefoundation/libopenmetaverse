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
 */

using System;
using System.Collections.Generic;

namespace OpenMetaverse.Rendering
{
    public partial class Meshmerizer : IRendering
    {
        private bool usePrimMesher = true;
        private const float DEG_TO_RAD = 0.01745329238f;
        // prims with all dimensions smaller than this will have a bounding box mesh
        private float minSizeForComplexMesh = 0.2f;

        private static List<Triangle> FindInfluencedTriangles(List<Triangle> triangles, MeshmerizerVertex v)
        {
            List<Triangle> influenced = new List<Triangle>();
            foreach (Triangle t in triangles)
            {
                if (t.isInCircle(v.X, v.Y))
                {
                    influenced.Add(t);
                }
            }
            return influenced;
        }

        private static void InsertVertices(List<MeshmerizerVertex> vertices, int usedForSeed, List<Triangle> triangles)
        {
            // This is a variant of the delaunay algorithm
            // each time a new vertex is inserted, all triangles that are influenced by it are deleted
            // and replaced by new ones including the new vertex
            // It is not very time efficient but easy to implement.

            int iCurrentVertex;
            int iMaxVertex = vertices.Count;
            for (iCurrentVertex = usedForSeed; iCurrentVertex < iMaxVertex; iCurrentVertex++)
            {
                // Background: A triangle mesh fulfills the delaunay condition if (iff!)
                // each circumlocutory circle (i.e. the circle that touches all three corners)
                // of each triangle is empty of other vertices.
                // Obviously a single (seeding) triangle fulfills this condition.
                // If we now add one vertex, we need to reconstruct all triangles, that
                // do not fulfill this condition with respect to the new triangle

                // Find the triangles that are influenced by the new vertex
                MeshmerizerVertex v = vertices[iCurrentVertex];
                if (v == null)
                    continue; // Null is polygon stop marker. Ignore it
                List<Triangle> influencedTriangles = FindInfluencedTriangles(triangles, v);

                List<Simplex> simplices = new List<Simplex>();

                // Reconstruction phase. First step, dissolve each triangle into it's simplices,
                // i.e. it's "border lines"
                // Goal is to find "inner" borders and delete them, while the hull gets conserved.
                // Inner borders are special in the way that they always come twice, which is how we detect them
                foreach (Triangle t in influencedTriangles)
                {
                    List<Simplex> newSimplices = t.GetSimplices();
                    simplices.AddRange(newSimplices);
                    triangles.Remove(t);
                }
                // Now sort the simplices. That will make identical ones reside side by side in the list
                simplices.Sort();

                // Look for duplicate simplices here.
                // Remember, they are directly side by side in the list right now,
                // So we only check directly neighbours
                int iSimplex;
                List<Simplex> innerSimplices = new List<Simplex>();
                for (iSimplex = 1; iSimplex < simplices.Count; iSimplex++) // Startindex=1, so we can refer backwards
                {
                    if (simplices[iSimplex - 1].CompareTo(simplices[iSimplex]) == 0)
                    {
                        innerSimplices.Add(simplices[iSimplex - 1]);
                        innerSimplices.Add(simplices[iSimplex]);
                    }
                }

                foreach (Simplex s in innerSimplices)
                {
                    simplices.Remove(s);
                }

                // each simplex still in the list belongs to the hull of the region in question
                // The new vertex (yes, we still deal with verices here :-)) forms a triangle
                // with each of these simplices. Build the new triangles and add them to the list
                foreach (Simplex s in simplices)
                {
                    Triangle t = new Triangle(s.v1, s.v2, vertices[iCurrentVertex]);
                    if (!t.isDegraded())
                    {
                        triangles.Add(t);
                    }
                }
            }
        }

        private static SimpleHull BuildHoleHull(PrimitiveBaseShape pbs, ProfileShape pshape, HollowShape hshape, UInt16 hollowFactor)
        {
            // Tackle HollowShape.Same
            float fhollowFactor = (float)hollowFactor;

            switch (pshape)
            {
                case ProfileShape.Square:
                    if (hshape == HollowShape.Same)
                        hshape = HollowShape.Square;
                    break;
                case ProfileShape.EquilateralTriangle:
                    fhollowFactor = ((float)hollowFactor / 1.9f);
                    if (hshape == HollowShape.Same)
                    {
                        hshape = HollowShape.Triangle;
                    }

                    break;

                case ProfileShape.HalfCircle:
                case ProfileShape.Circle:
                    if (pbs.PathCurve == (byte)Extrusion.Straight)
                    {
                        if (hshape == HollowShape.Same)
                        {
                            hshape = HollowShape.Circle;
                        }
                    }
                    break;


                default:
                    if (hshape == HollowShape.Same)
                        hshape = HollowShape.Square;
                    break;
            }


            SimpleHull holeHull = null;

            if (hshape == HollowShape.Square)
            {
                float hollowFactorF = (float)fhollowFactor / (float)50000;
                MeshmerizerVertex IMM;
                MeshmerizerVertex IPM;
                MeshmerizerVertex IPP;
                MeshmerizerVertex IMP;

                if (pshape == ProfileShape.Circle)
                { // square cutout in cylinder is 45 degress rotated
                    IMM = new MeshmerizerVertex(0.0f, -0.707f * hollowFactorF, 0.0f);
                    IPM = new MeshmerizerVertex(0.707f * hollowFactorF, 0.0f, 0.0f);
                    IPP = new MeshmerizerVertex(0.0f, 0.707f * hollowFactorF, 0.0f);
                    IMP = new MeshmerizerVertex(-0.707f * hollowFactorF, 0.0f, 0.0f);
                }
                else if (pshape == ProfileShape.EquilateralTriangle)
                {
                    IMM = new MeshmerizerVertex(0.0f, -0.667f * hollowFactorF, 0.0f);
                    IPM = new MeshmerizerVertex(0.667f * hollowFactorF, 0.0f, 0.0f);
                    IPP = new MeshmerizerVertex(0.0f, 0.667f * hollowFactorF, 0.0f);
                    IMP = new MeshmerizerVertex(-0.667f * hollowFactorF, 0.0f, 0.0f);
                }
                else
                {
                    IMM = new MeshmerizerVertex(-0.5f * hollowFactorF, -0.5f * hollowFactorF, 0.0f);
                    IPM = new MeshmerizerVertex(+0.5f * hollowFactorF, -0.5f * hollowFactorF, 0.0f);
                    IPP = new MeshmerizerVertex(+0.5f * hollowFactorF, +0.5f * hollowFactorF, 0.0f);
                    IMP = new MeshmerizerVertex(-0.5f * hollowFactorF, +0.5f * hollowFactorF, 0.0f);
                }

                holeHull = new SimpleHull();

                holeHull.AddVertex(IMM);
                holeHull.AddVertex(IMP);
                holeHull.AddVertex(IPP);
                holeHull.AddVertex(IPM);
            }
            //if (hshape == HollowShape.Circle && pbs.PathCurve == (byte)Extrusion.Straight)
            if (hshape == HollowShape.Circle)
            {
                float hollowFactorF = (float)fhollowFactor / (float)50000;

                //Counter clockwise around the quadrants
                holeHull = new SimpleHull();

                holeHull.AddVertex(new MeshmerizerVertex(0.353553f * hollowFactorF, 0.353553f * hollowFactorF, 0.0f)); // 45 degrees
                holeHull.AddVertex(new MeshmerizerVertex(0.433013f * hollowFactorF, 0.250000f * hollowFactorF, 0.0f)); // 30 degrees
                holeHull.AddVertex(new MeshmerizerVertex(0.482963f * hollowFactorF, 0.129410f * hollowFactorF, 0.0f)); // 15 degrees
                holeHull.AddVertex(new MeshmerizerVertex(0.500000f * hollowFactorF, 0.000000f * hollowFactorF, 0.0f)); // 0 degrees
                holeHull.AddVertex(new MeshmerizerVertex(0.482963f * hollowFactorF, -0.129410f * hollowFactorF, 0.0f)); // 345 degrees
                holeHull.AddVertex(new MeshmerizerVertex(0.433013f * hollowFactorF, -0.250000f * hollowFactorF, 0.0f)); // 330 degrees
                holeHull.AddVertex(new MeshmerizerVertex(0.353553f * hollowFactorF, -0.353553f * hollowFactorF, 0.0f)); // 315 degrees
                holeHull.AddVertex(new MeshmerizerVertex(0.250000f * hollowFactorF, -0.433013f * hollowFactorF, 0.0f)); // 300 degrees
                holeHull.AddVertex(new MeshmerizerVertex(0.129410f * hollowFactorF, -0.482963f * hollowFactorF, 0.0f)); // 285 degrees
                holeHull.AddVertex(new MeshmerizerVertex(0.000000f * hollowFactorF, -0.500000f * hollowFactorF, 0.0f)); // 270 degrees
                holeHull.AddVertex(new MeshmerizerVertex(-0.129410f * hollowFactorF, -0.482963f * hollowFactorF, 0.0f)); // 255 degrees
                holeHull.AddVertex(new MeshmerizerVertex(-0.250000f * hollowFactorF, -0.433013f * hollowFactorF, 0.0f)); // 240 degrees
                holeHull.AddVertex(new MeshmerizerVertex(-0.353553f * hollowFactorF, -0.353553f * hollowFactorF, 0.0f)); // 225 degrees
                holeHull.AddVertex(new MeshmerizerVertex(-0.433013f * hollowFactorF, -0.250000f * hollowFactorF, 0.0f)); // 210 degrees
                holeHull.AddVertex(new MeshmerizerVertex(-0.482963f * hollowFactorF, -0.129410f * hollowFactorF, 0.0f)); // 195 degrees
                holeHull.AddVertex(new MeshmerizerVertex(-0.500000f * hollowFactorF, 0.000000f * hollowFactorF, 0.0f)); // 180 degrees
                holeHull.AddVertex(new MeshmerizerVertex(-0.482963f * hollowFactorF, 0.129410f * hollowFactorF, 0.0f)); // 165 degrees
                holeHull.AddVertex(new MeshmerizerVertex(-0.433013f * hollowFactorF, 0.250000f * hollowFactorF, 0.0f)); // 150 degrees
                holeHull.AddVertex(new MeshmerizerVertex(-0.353553f * hollowFactorF, 0.353553f * hollowFactorF, 0.0f)); // 135 degrees
                holeHull.AddVertex(new MeshmerizerVertex(-0.250000f * hollowFactorF, 0.433013f * hollowFactorF, 0.0f)); // 120 degrees
                holeHull.AddVertex(new MeshmerizerVertex(-0.129410f * hollowFactorF, 0.482963f * hollowFactorF, 0.0f)); // 105 degrees
                holeHull.AddVertex(new MeshmerizerVertex(0.000000f * hollowFactorF, 0.500000f * hollowFactorF, 0.0f)); // 90 degrees
                holeHull.AddVertex(new MeshmerizerVertex(0.129410f * hollowFactorF, 0.482963f * hollowFactorF, 0.0f)); // 75 degrees
                holeHull.AddVertex(new MeshmerizerVertex(0.250000f * hollowFactorF, 0.433013f * hollowFactorF, 0.0f)); // 60 degrees
                holeHull.AddVertex(new MeshmerizerVertex(0.353553f * hollowFactorF, 0.353553f * hollowFactorF, 0.0f)); // 45 degrees

            }
            if (hshape == HollowShape.Triangle)
            {
                float hollowFactorF = (float)fhollowFactor / (float)50000;
                MeshmerizerVertex IMM;
                MeshmerizerVertex IPM;
                MeshmerizerVertex IPP;

                if (pshape == ProfileShape.Square)
                {
                    // corner points are at 345, 105, and 225 degrees for the triangle within a box

                    // hard coded here for speed, the equations are in the commented out lines above
                    IMM = new MeshmerizerVertex(0.48296f * hollowFactorF, -0.12941f * hollowFactorF, 0.0f);
                    IPM = new MeshmerizerVertex(-0.12941f * hollowFactorF, 0.48296f * hollowFactorF, 0.0f);
                    IPP = new MeshmerizerVertex(-0.35355f * hollowFactorF, -0.35355f * hollowFactorF, 0.0f);
                }
                else
                {
                    IMM = new MeshmerizerVertex(-0.25f * hollowFactorF, -0.45f * hollowFactorF, 0.0f);
                    IPM = new MeshmerizerVertex(+0.5f * hollowFactorF, +0f * hollowFactorF, 0.0f);
                    IPP = new MeshmerizerVertex(-0.25f * hollowFactorF, +0.45f * hollowFactorF, 0.0f);
                }

                holeHull = new SimpleHull();

                holeHull.AddVertex(IMM);
                holeHull.AddVertex(IPP);
                holeHull.AddVertex(IPM);

            }

            return holeHull;
        }

        private static MeshmerizerMesh CreateBoxMesh(String primName, PrimitiveBaseShape primShape, PhysicsVector size)
        {
            UInt16 hollowFactor = primShape.ProfileHollow;
            UInt16 profileBegin = primShape.ProfileBegin;
            UInt16 profileEnd = primShape.ProfileEnd;
            UInt16 taperX = primShape.PathScaleX;
            UInt16 taperY = primShape.PathScaleY;
            UInt16 pathShearX = primShape.PathShearX;
            UInt16 pathShearY = primShape.PathShearY;

            // Procedure: This is based on the fact that the upper (plus) and lower (minus) Z-surface
            // of a block are basically the same
            // They may be warped differently but the shape is identical
            // So we only create one surface as a model and derive both plus and minus surface of the block from it
            // This is done in a model space where the block spans from -.5 to +.5 in X and Y
            // The mapping to Scene space is done later during the "extrusion" phase

            // Base
            MeshmerizerVertex MM = new MeshmerizerVertex(-0.5f, -0.5f, 0.0f);
            MeshmerizerVertex PM = new MeshmerizerVertex(+0.5f, -0.5f, 0.0f);
            MeshmerizerVertex PP = new MeshmerizerVertex(+0.5f, +0.5f, 0.0f);
            MeshmerizerVertex MP = new MeshmerizerVertex(-0.5f, +0.5f, 0.0f);

            SimpleHull outerHull = new SimpleHull();

            outerHull.AddVertex(PP);
            outerHull.AddVertex(MP);
            outerHull.AddVertex(MM);
            outerHull.AddVertex(PM);

            // Deal with cuts now
            if ((profileBegin != 0) || (profileEnd != 0))
            {
                double fProfileBeginAngle = profileBegin / 50000.0 * 360.0;
                // In degree, for easier debugging and understanding
                fProfileBeginAngle -= (90.0 + 45.0); // for some reasons, the SL client counts from the corner -X/-Y
                double fProfileEndAngle = 360.0 - profileEnd / 50000.0 * 360.0; // Pathend comes as complement to 1.0
                fProfileEndAngle -= (90.0 + 45.0);

                // avoid some problem angles until the hull subtraction routine is fixed
                if ((fProfileBeginAngle + 45.0f) % 90.0f == 0.0f)
                    fProfileBeginAngle += 5.0f;
                if ((fProfileEndAngle + 45.0f) % 90.0f == 0.0f)
                    fProfileEndAngle -= 5.0f;
                if (fProfileBeginAngle % 90.0f == 0.0f)
                    fProfileBeginAngle += 1.0f;
                if (fProfileEndAngle % 90.0f == 0.0f)
                    fProfileEndAngle -= 1.0f;

                if (fProfileBeginAngle < fProfileEndAngle)
                    fProfileEndAngle -= 360.0;

                // Note, that we don't want to cut out a triangle, even if this is a
                // good approximation for small cuts. Indeed we want to cut out an arc
                // and we approximate this arc by a polygon chain
                // Also note, that these vectors are of length 1.0 and thus their endpoints lay outside the model space
                // So it can easily be subtracted from the outer hull
                int iSteps = (int)(((fProfileBeginAngle - fProfileEndAngle) / 45.0) + .5);
                // how many steps do we need with approximately 45 degree
                double dStepWidth = (fProfileBeginAngle - fProfileEndAngle) / iSteps;

                MeshmerizerVertex origin = new MeshmerizerVertex(0.0f, 0.0f, 0.0f);

                // Note the sequence of vertices here. It's important to have the other rotational sense than in outerHull
                SimpleHull cutHull = new SimpleHull();
                cutHull.AddVertex(origin);
                for (int i = 0; i < iSteps; i++)
                {
                    double angle = fProfileBeginAngle - i * dStepWidth; // we count against the angle orientation!!!!
                    MeshmerizerVertex v = MeshmerizerVertex.FromAngle(angle * Math.PI / 180.0);
                    cutHull.AddVertex(v);
                }

                MeshmerizerVertex legEnd = MeshmerizerVertex.FromAngle(fProfileEndAngle * Math.PI / 180.0);
                // Calculated separately to avoid errors
                cutHull.AddVertex(legEnd);

                SimpleHull cuttedHull = SimpleHull.SubtractHull(outerHull, cutHull);

                outerHull = cuttedHull;
            }

            // Deal with the hole here
            if (hollowFactor > 0)
            {
                if (hollowFactor < 1000)
                    hollowFactor = 1000; // some sane minimum for our beloved SimpleHull routines

                SimpleHull holeHull = BuildHoleHull(primShape, primShape.ProfileShape, primShape.HollowShape, hollowFactor);
                if (holeHull != null)
                {
                    SimpleHull hollowedHull = SimpleHull.SubtractHull(outerHull, holeHull);

                    outerHull = hollowedHull;
                }
            }

            MeshmerizerMesh m = new MeshmerizerMesh();

            MeshmerizerVertex Seed1 = new MeshmerizerVertex(0.0f, -10.0f, 0.0f);
            MeshmerizerVertex Seed2 = new MeshmerizerVertex(-10.0f, 10.0f, 0.0f);
            MeshmerizerVertex Seed3 = new MeshmerizerVertex(10.0f, 10.0f, 0.0f);

            m.Add(Seed1);
            m.Add(Seed2);
            m.Add(Seed3);

            m.Add(new Triangle(Seed1, Seed2, Seed3));
            m.Add(outerHull.getVertices());

            InsertVertices(m.vertices, 3, m.triangles);

            m.Remove(Seed1);
            m.Remove(Seed2);
            m.Remove(Seed3);

            m.RemoveTrianglesOutside(outerHull);

            foreach (Triangle t in m.triangles)
            {
                PhysicsVector n = t.getNormal();
                if (n.Z < 0.0)
                    t.invertNormal();
            }

            Extruder extr = new Extruder();

            extr.size = size;

            if (taperX != 100)
            {
                if (taperX > 100)
                {
                    extr.taperTopFactorX = 1.0f - ((float)(taperX - 100) / 100);
                }
                else
                {
                    extr.taperBotFactorX = 1.0f - ((100 - (float)taperX) / 100);
                }

            }

            if (taperY != 100)
            {
                if (taperY > 100)
                {
                    extr.taperTopFactorY = 1.0f - ((float)(taperY - 100) / 100);
                }
                else
                {
                    extr.taperBotFactorY = 1.0f - ((100 - (float)taperY) / 100);
                }
            }

            if (pathShearX != 0)
            {
                if (pathShearX > 50)
                {
                    // Complimentary byte.  Negative values wrap around the byte.  Positive values go up to 50
                    extr.pushX = (((float)(256 - pathShearX) / 100) * -1f);
                }
                else
                {
                    extr.pushX = (float)pathShearX / 100;
                }
            }

            if (pathShearY != 0)
            {
                if (pathShearY > 50)
                {
                    // Complimentary byte.  Negative values wrap around the byte.  Positive values go up to 50
                    extr.pushY = (((float)(256 - pathShearY) / 100) * -1f);
                }
                else
                {
                    extr.pushY = (float)pathShearY / 100;
                }
            }

            extr.twistTop = (float)primShape.PathTwist * (float)Math.PI * 0.01f;
            extr.twistBot = (float)primShape.PathTwistBegin * (float)Math.PI * 0.01f;
            extr.pathBegin = primShape.PathBegin;
            extr.pathEnd = primShape.PathEnd;

            MeshmerizerMesh result = extr.ExtrudeLinearPath(m);

            return result;
        }

        private static MeshmerizerMesh CreateCylinderMesh(String primName, PrimitiveBaseShape primShape, PhysicsVector size)
        {
            UInt16 hollowFactor = primShape.ProfileHollow;
            UInt16 profileBegin = primShape.ProfileBegin;
            UInt16 profileEnd = primShape.ProfileEnd;
            UInt16 taperX = primShape.PathScaleX;
            UInt16 taperY = primShape.PathScaleY;
            UInt16 pathShearX = primShape.PathShearX;
            UInt16 pathShearY = primShape.PathShearY;

            // Procedure: This is based on the fact that the upper (plus) and lower (minus) Z-surface
            // of a block are basically the same
            // They may be warped differently but the shape is identical
            // So we only create one surface as a model and derive both plus and minus surface of the block from it
            // This is done in a model space where the block spans from -.5 to +.5 in X and Y
            // The mapping to Scene space is done later during the "extrusion" phase

            // Base
            SimpleHull outerHull = new SimpleHull();

            // counter-clockwise around the quadrants, start at 45 degrees
            outerHull.AddVertex(new MeshmerizerVertex(0.353553f, 0.353553f, 0.0f)); // 45 degrees
            outerHull.AddVertex(new MeshmerizerVertex(0.250000f, 0.433013f, 0.0f)); // 60 degrees
            outerHull.AddVertex(new MeshmerizerVertex(0.129410f, 0.482963f, 0.0f)); // 75 degrees
            outerHull.AddVertex(new MeshmerizerVertex(0.000000f, 0.500000f, 0.0f)); // 90 degrees
            outerHull.AddVertex(new MeshmerizerVertex(-0.129410f, 0.482963f, 0.0f)); // 105 degrees
            outerHull.AddVertex(new MeshmerizerVertex(-0.250000f, 0.433013f, 0.0f)); // 120 degrees
            outerHull.AddVertex(new MeshmerizerVertex(-0.353553f, 0.353553f, 0.0f)); // 135 degrees
            outerHull.AddVertex(new MeshmerizerVertex(-0.433013f, 0.250000f, 0.0f)); // 150 degrees
            outerHull.AddVertex(new MeshmerizerVertex(-0.482963f, 0.129410f, 0.0f)); // 165 degrees
            outerHull.AddVertex(new MeshmerizerVertex(-0.500000f, 0.000000f, 0.0f)); // 180 degrees
            outerHull.AddVertex(new MeshmerizerVertex(-0.482963f, -0.129410f, 0.0f)); // 195 degrees
            outerHull.AddVertex(new MeshmerizerVertex(-0.433013f, -0.250000f, 0.0f)); // 210 degrees
            outerHull.AddVertex(new MeshmerizerVertex(-0.353553f, -0.353553f, 0.0f)); // 225 degrees
            outerHull.AddVertex(new MeshmerizerVertex(-0.250000f, -0.433013f, 0.0f)); // 240 degrees
            outerHull.AddVertex(new MeshmerizerVertex(-0.129410f, -0.482963f, 0.0f)); // 255 degrees
            outerHull.AddVertex(new MeshmerizerVertex(0.000000f, -0.500000f, 0.0f)); // 270 degrees
            outerHull.AddVertex(new MeshmerizerVertex(0.129410f, -0.482963f, 0.0f)); // 285 degrees
            outerHull.AddVertex(new MeshmerizerVertex(0.250000f, -0.433013f, 0.0f)); // 300 degrees
            outerHull.AddVertex(new MeshmerizerVertex(0.353553f, -0.353553f, 0.0f)); // 315 degrees
            outerHull.AddVertex(new MeshmerizerVertex(0.433013f, -0.250000f, 0.0f)); // 330 degrees
            outerHull.AddVertex(new MeshmerizerVertex(0.482963f, -0.129410f, 0.0f)); // 345 degrees
            outerHull.AddVertex(new MeshmerizerVertex(0.500000f, 0.000000f, 0.0f)); // 0 degrees
            outerHull.AddVertex(new MeshmerizerVertex(0.482963f, 0.129410f, 0.0f)); // 15 degrees
            outerHull.AddVertex(new MeshmerizerVertex(0.433013f, 0.250000f, 0.0f)); // 30 degrees

            // Deal with cuts now
            if ((profileBegin != 0) || (profileEnd != 0))
            {
                double fProfileBeginAngle = profileBegin / 50000.0 * 360.0;
                // In degree, for easier debugging and understanding
                double fProfileEndAngle = 360.0 - profileEnd / 50000.0 * 360.0; // Pathend comes as complement to 1.0

                if (fProfileBeginAngle > 270.0f && fProfileBeginAngle < 271.8f) // a problem angle for the hull subtract routine :(
                    fProfileBeginAngle = 271.8f; // workaround - use the smaller slice

                if (fProfileBeginAngle < fProfileEndAngle)
                    fProfileEndAngle -= 360.0;

                // Note, that we don't want to cut out a triangle, even if this is a
                // good approximation for small cuts. Indeed we want to cut out an arc
                // and we approximate this arc by a polygon chain
                // Also note, that these vectors are of length 1.0 and thus their endpoints lay outside the model space
                // So it can easily be subtracted from the outer hull
                int iSteps = (int)(((fProfileBeginAngle - fProfileEndAngle) / 45.0) + .5);
                // how many steps do we need with approximately 45 degree
                double dStepWidth = (fProfileBeginAngle - fProfileEndAngle) / iSteps;

                MeshmerizerVertex origin = new MeshmerizerVertex(0.0f, 0.0f, 0.0f);

                // Note the sequence of vertices here. It's important to have the other rotational sense than in outerHull
                SimpleHull cutHull = new SimpleHull();
                cutHull.AddVertex(origin);
                for (int i = 0; i < iSteps; i++)
                {
                    double angle = fProfileBeginAngle - i * dStepWidth; // we count against the angle orientation!!!!
                    MeshmerizerVertex v = MeshmerizerVertex.FromAngle(angle * Math.PI / 180.0);
                    cutHull.AddVertex(v);
                }

                MeshmerizerVertex legEnd = MeshmerizerVertex.FromAngle(fProfileEndAngle * Math.PI / 180.0);
                // Calculated separately to avoid errors
                cutHull.AddVertex(legEnd);

                SimpleHull cuttedHull = SimpleHull.SubtractHull(outerHull, cutHull);
                outerHull = cuttedHull;
            }

            // Deal with the hole here
            if (hollowFactor > 0)
            {
                if (hollowFactor < 1000)
                    hollowFactor = 1000; // some sane minimum for our beloved SimpleHull routines

                SimpleHull holeHull = BuildHoleHull(primShape, primShape.ProfileShape, primShape.HollowShape, hollowFactor);
                if (holeHull != null)
                {
                    SimpleHull hollowedHull = SimpleHull.SubtractHull(outerHull, holeHull);
                    outerHull = hollowedHull;
                }
            }

            MeshmerizerMesh m = new MeshmerizerMesh();

            MeshmerizerVertex Seed1 = new MeshmerizerVertex(0.0f, -10.0f, 0.0f);
            MeshmerizerVertex Seed2 = new MeshmerizerVertex(-10.0f, 10.0f, 0.0f);
            MeshmerizerVertex Seed3 = new MeshmerizerVertex(10.0f, 10.0f, 0.0f);

            m.Add(Seed1);
            m.Add(Seed2);
            m.Add(Seed3);

            m.Add(new Triangle(Seed1, Seed2, Seed3));
            m.Add(outerHull.getVertices());

            InsertVertices(m.vertices, 3, m.triangles);

            m.Remove(Seed1);
            m.Remove(Seed2);
            m.Remove(Seed3);

            m.RemoveTrianglesOutside(outerHull);

            foreach (Triangle t in m.triangles)
            {
                PhysicsVector n = t.getNormal();
                if (n.Z < 0.0)
                    t.invertNormal();
            }

            Extruder extr = new Extruder();

            extr.size = size;

            if (taperX != 100)
            {
                if (taperX > 100)
                {
                    extr.taperTopFactorX = 1.0f - ((float)(taperX - 100) / 100);
                }
                else
                {
                    extr.taperBotFactorX = 1.0f - ((100 - (float)taperX) / 100);
                }

            }

            if (taperY != 100)
            {
                if (taperY > 100)
                {
                    extr.taperTopFactorY = 1.0f - ((float)(taperY - 100) / 100);
                }
                else
                {
                    extr.taperBotFactorY = 1.0f - ((100 - (float)taperY) / 100);
                }
            }

            if (pathShearX != 0)
            {
                if (pathShearX > 50)
                {
                    // Complimentary byte.  Negative values wrap around the byte.  Positive values go up to 50
                    extr.pushX = (((float)(256 - pathShearX) / 100) * -1f);
                }
                else
                {
                    extr.pushX = (float)pathShearX / 100;
                }
            }

            if (pathShearY != 0)
            {
                if (pathShearY > 50)
                {
                    // Complimentary byte.  Negative values wrap around the byte.  Positive values go up to 50
                    extr.pushY = (((float)(256 - pathShearY) / 100) * -1f);
                }
                else
                {
                    extr.pushY = (float)pathShearY / 100;
                }

            }

            extr.twistTop = (float)primShape.PathTwist * (float)Math.PI * 0.01f;
            extr.twistBot = (float)primShape.PathTwistBegin * (float)Math.PI * 0.01f;
            extr.pathBegin = primShape.PathBegin;
            extr.pathEnd = primShape.PathEnd;

            MeshmerizerMesh result = extr.ExtrudeLinearPath(m);

            return result;
        }

        private static MeshmerizerMesh CreatePrismMesh(String primName, PrimitiveBaseShape primShape, PhysicsVector size)
        {
            UInt16 hollowFactor = primShape.ProfileHollow;
            UInt16 profileBegin = primShape.ProfileBegin;
            UInt16 profileEnd = primShape.ProfileEnd;
            UInt16 taperX = primShape.PathScaleX;
            UInt16 taperY = primShape.PathScaleY;
            UInt16 pathShearX = primShape.PathShearX;
            UInt16 pathShearY = primShape.PathShearY;

            // Procedure: This is based on the fact that the upper (plus) and lower (minus) Z-surface
            // of a block are basically the same
            // They may be warped differently but the shape is identical
            // So we only create one surface as a model and derive both plus and minus surface of the block from it
            // This is done in a model space where the block spans from -.5 to +.5 in X and Y
            // The mapping to Scene space is done later during the "extrusion" phase

            // Base
            MeshmerizerVertex MM = new MeshmerizerVertex(-0.25f, -0.45f, 0.0f);
            MeshmerizerVertex PM = new MeshmerizerVertex(+0.5f, 0f, 0.0f);
            MeshmerizerVertex PP = new MeshmerizerVertex(-0.25f, +0.45f, 0.0f);

            SimpleHull outerHull = new SimpleHull();

            outerHull.AddVertex(PP);
            outerHull.AddVertex(MM);
            outerHull.AddVertex(PM);

            // Deal with cuts now
            if ((profileBegin != 0) || (profileEnd != 0))
            {
                double fProfileBeginAngle = profileBegin / 50000.0 * 360.0;
                // In degree, for easier debugging and understanding
                //fProfileBeginAngle -= (90.0 + 45.0); // for some reasons, the SL client counts from the corner -X/-Y
                double fProfileEndAngle = 360.0 - profileEnd / 50000.0 * 360.0; // Pathend comes as complement to 1.0
                //fProfileEndAngle -= (90.0 + 45.0);
                if (fProfileBeginAngle < fProfileEndAngle)
                    fProfileEndAngle -= 360.0;

                // Note, that we don't want to cut out a triangle, even if this is a
                // good approximation for small cuts. Indeed we want to cut out an arc
                // and we approximate this arc by a polygon chain
                // Also note, that these vectors are of length 1.0 and thus their endpoints lay outside the model space
                // So it can easily be subtracted from the outer hull
                int iSteps = (int)(((fProfileBeginAngle - fProfileEndAngle) / 45.0) + .5);
                // how many steps do we need with approximately 45 degree
                double dStepWidth = (fProfileBeginAngle - fProfileEndAngle) / iSteps;

                MeshmerizerVertex origin = new MeshmerizerVertex(0.0f, 0.0f, 0.0f);

                // Note the sequence of vertices here. It's important to have the other rotational sense than in outerHull
                SimpleHull cutHull = new SimpleHull();
                cutHull.AddVertex(origin);
                for (int i = 0; i < iSteps; i++)
                {
                    double angle = fProfileBeginAngle - i * dStepWidth; // we count against the angle orientation!!!!
                    MeshmerizerVertex v = MeshmerizerVertex.FromAngle(angle * Math.PI / 180.0);
                    cutHull.AddVertex(v);
                }
                MeshmerizerVertex legEnd = MeshmerizerVertex.FromAngle(fProfileEndAngle * Math.PI / 180.0);
                // Calculated separately to avoid errors
                cutHull.AddVertex(legEnd);

                //m_log.DebugFormat("Starting cutting of the hollow shape from the prim {1}", 0, primName);
                SimpleHull cuttedHull = SimpleHull.SubtractHull(outerHull, cutHull);

                outerHull = cuttedHull;
            }

            // Deal with the hole here
            if (hollowFactor > 0)
            {
                if (hollowFactor < 1000)
                    hollowFactor = 1000;  // some sane minimum for our beloved SimpleHull routines

                SimpleHull holeHull = BuildHoleHull(primShape, primShape.ProfileShape, primShape.HollowShape, hollowFactor);
                if (holeHull != null)
                {
                    SimpleHull hollowedHull = SimpleHull.SubtractHull(outerHull, holeHull);

                    outerHull = hollowedHull;
                }
            }

            MeshmerizerMesh m = new MeshmerizerMesh();

            MeshmerizerVertex Seed1 = new MeshmerizerVertex(0.0f, -10.0f, 0.0f);
            MeshmerizerVertex Seed2 = new MeshmerizerVertex(-10.0f, 10.0f, 0.0f);
            MeshmerizerVertex Seed3 = new MeshmerizerVertex(10.0f, 10.0f, 0.0f);

            m.Add(Seed1);
            m.Add(Seed2);
            m.Add(Seed3);

            m.Add(new Triangle(Seed1, Seed2, Seed3));
            m.Add(outerHull.getVertices());

            InsertVertices(m.vertices, 3, m.triangles);

            m.Remove(Seed1);
            m.Remove(Seed2);
            m.Remove(Seed3);

            m.RemoveTrianglesOutside(outerHull);

            foreach (Triangle t in m.triangles)
            {
                PhysicsVector n = t.getNormal();
                if (n.Z < 0.0)
                    t.invertNormal();
            }

            Extruder extr = new Extruder();

            extr.size = size;

            if (taperX != 100)
            {
                if (taperX > 100)
                {
                    extr.taperTopFactorX = 1.0f - ((float)(taperX - 100) / 100);
                }
                else
                {
                    extr.taperBotFactorX = 1.0f - ((100 - (float)taperX) / 100);
                }

            }

            if (taperY != 100)
            {
                if (taperY > 100)
                {
                    extr.taperTopFactorY = 1.0f - ((float)(taperY - 100) / 100);
                }
                else
                {
                    extr.taperBotFactorY = 1.0f - ((100 - (float)taperY) / 100);
                }
            }

            if (pathShearX != 0)
            {
                if (pathShearX > 50)
                {
                    // Complimentary byte.  Negative values wrap around the byte.  Positive values go up to 50
                    extr.pushX = (((float)(256 - pathShearX) / 100) * -1f);
                }
                else
                {
                    extr.pushX = (float)pathShearX / 100;
                }
            }

            if (pathShearY != 0)
            {
                if (pathShearY > 50)
                {
                    // Complimentary byte.  Negative values wrap around the byte.  Positive values go up to 50
                    extr.pushY = (((float)(256 - pathShearY) / 100) * -1f);
                }
                else
                {
                    extr.pushY = (float)pathShearY / 100;
                }
            }

            extr.twistTop = (float)primShape.PathTwist * (float)Math.PI * 0.01f;
            extr.twistBot = (float)primShape.PathTwistBegin * (float)Math.PI * 0.01f;
            extr.pathBegin = primShape.PathBegin;
            extr.pathEnd = primShape.PathEnd;

            MeshmerizerMesh result = extr.ExtrudeLinearPath(m);

            return result;
        }

        /// <summary>
        /// Builds an icosahedral geodesic sphere - used as default in place of problem meshes
        /// </summary>
        private static MeshmerizerMesh CreateSphereMesh(String primName, PrimitiveBaseShape primShape, PhysicsVector size)
        {
            // Builds an icosahedral geodesic sphere
            // based on an article by Paul Bourke
            // http://local.wasp.uwa.edu.au/~pbourke/
            // articles:
            // http://local.wasp.uwa.edu.au/~pbourke/geometry/polygonmesh/
            // and
            // http://local.wasp.uwa.edu.au/~pbourke/geometry/polyhedra/index.html

            // Still have more to do here.

            // UInt16 hollowFactor = primShape.ProfileHollow;
            // UInt16 profileBegin = primShape.ProfileBegin;
            // UInt16 profileEnd = primShape.ProfileEnd;
            // UInt16 taperX = primShape.PathScaleX;
            // UInt16 taperY = primShape.PathScaleY;
            // UInt16 pathShearX = primShape.PathShearX;
            // UInt16 pathShearY = primShape.PathShearY;
            MeshmerizerMesh m = new MeshmerizerMesh();

            float LOD = 0.2f;
            float diameter = 0.5f;// Our object will result in -0.5 to 0.5
            float sq5 = (float)Math.Sqrt(5.0);
            float phi = (1 + sq5) * 0.5f;
            float rat = (float)Math.Sqrt(10f + (2f * sq5)) / (4f * phi);
            float a = (diameter / rat) * 0.5f;
            float b = (diameter / rat) / (2.0f * phi);

            // 12 Icosahedron vertexes
            MeshmerizerVertex v1 = new MeshmerizerVertex(0f, b, -a);
            MeshmerizerVertex v2 = new MeshmerizerVertex(b, a, 0f);
            MeshmerizerVertex v3 = new MeshmerizerVertex(-b, a, 0f);
            MeshmerizerVertex v4 = new MeshmerizerVertex(0f, b, a);
            MeshmerizerVertex v5 = new MeshmerizerVertex(0f, -b, a);
            MeshmerizerVertex v6 = new MeshmerizerVertex(-a, 0f, b);
            MeshmerizerVertex v7 = new MeshmerizerVertex(0f, -b, -a);
            MeshmerizerVertex v8 = new MeshmerizerVertex(a, 0f, -b);
            MeshmerizerVertex v9 = new MeshmerizerVertex(a, 0f, b);
            MeshmerizerVertex v10 = new MeshmerizerVertex(-a, 0f, -b);
            MeshmerizerVertex v11 = new MeshmerizerVertex(b, -a, 0);
            MeshmerizerVertex v12 = new MeshmerizerVertex(-b, -a, 0);

            // Base Faces of the Icosahedron (20)
            SphereLODTriangle(v1, v2, v3, diameter, LOD, m);
            SphereLODTriangle(v4, v3, v2, diameter, LOD, m);
            SphereLODTriangle(v4, v5, v6, diameter, LOD, m);
            SphereLODTriangle(v4, v9, v5, diameter, LOD, m);
            SphereLODTriangle(v1, v7, v8, diameter, LOD, m);
            SphereLODTriangle(v1, v10, v7, diameter, LOD, m);
            SphereLODTriangle(v5, v11, v12, diameter, LOD, m);
            SphereLODTriangle(v7, v12, v11, diameter, LOD, m);
            SphereLODTriangle(v3, v6, v10, diameter, LOD, m);
            SphereLODTriangle(v12, v10, v6, diameter, LOD, m);
            SphereLODTriangle(v2, v8, v9, diameter, LOD, m);
            SphereLODTriangle(v11, v9, v8, diameter, LOD, m);
            SphereLODTriangle(v4, v6, v3, diameter, LOD, m);
            SphereLODTriangle(v4, v2, v9, diameter, LOD, m);
            SphereLODTriangle(v1, v3, v10, diameter, LOD, m);
            SphereLODTriangle(v1, v8, v2, diameter, LOD, m);
            SphereLODTriangle(v7, v10, v12, diameter, LOD, m);
            SphereLODTriangle(v7, v11, v8, diameter, LOD, m);
            SphereLODTriangle(v5, v12, v6, diameter, LOD, m);
            SphereLODTriangle(v5, v9, v11, diameter, LOD, m);

            // This was built with the normals pointing inside..
            // therefore we have to invert the normals
            foreach (Triangle t in m.triangles)
            {
                t.invertNormal();
            }

            return m;
        }

        /// <summary>
        /// Creates a mesh for prim types torus, ring, tube, and sphere
        /// </summary>
        private static MeshmerizerMesh CreateCircularPathMesh(String primName, PrimitiveBaseShape primShape, PhysicsVector size)
        {
            UInt16 hollowFactor = primShape.ProfileHollow;
            UInt16 profileBegin = primShape.ProfileBegin;
            UInt16 profileEnd = primShape.ProfileEnd;
            UInt16 pathShearX = primShape.PathShearX;
            UInt16 pathShearY = primShape.PathShearY;
            HollowShape hollowShape = primShape.HollowShape;

            SimpleHull outerHull = new SimpleHull();

            if ((primShape.ProfileCurve & 0x07) == (byte)ProfileShape.Circle)
            {
                if (hollowShape == HollowShape.Same)
                    hollowShape = HollowShape.Circle;

                // build the profile shape
                // counter-clockwise around the quadrants, start at 45 degrees

                outerHull.AddVertex(new MeshmerizerVertex(0.353553f, 0.353553f, 0.0f)); // 45 degrees
                outerHull.AddVertex(new MeshmerizerVertex(0.250000f, 0.433013f, 0.0f)); // 60 degrees
                outerHull.AddVertex(new MeshmerizerVertex(0.129410f, 0.482963f, 0.0f)); // 75 degrees
                outerHull.AddVertex(new MeshmerizerVertex(0.000000f, 0.500000f, 0.0f)); // 90 degrees
                outerHull.AddVertex(new MeshmerizerVertex(-0.129410f, 0.482963f, 0.0f)); // 105 degrees
                outerHull.AddVertex(new MeshmerizerVertex(-0.250000f, 0.433013f, 0.0f)); // 120 degrees
                outerHull.AddVertex(new MeshmerizerVertex(-0.353553f, 0.353553f, 0.0f)); // 135 degrees
                outerHull.AddVertex(new MeshmerizerVertex(-0.433013f, 0.250000f, 0.0f)); // 150 degrees
                outerHull.AddVertex(new MeshmerizerVertex(-0.482963f, 0.129410f, 0.0f)); // 165 degrees
                outerHull.AddVertex(new MeshmerizerVertex(-0.500000f, 0.000000f, 0.0f)); // 180 degrees
                outerHull.AddVertex(new MeshmerizerVertex(-0.482963f, -0.129410f, 0.0f)); // 195 degrees
                outerHull.AddVertex(new MeshmerizerVertex(-0.433013f, -0.250000f, 0.0f)); // 210 degrees
                outerHull.AddVertex(new MeshmerizerVertex(-0.353553f, -0.353553f, 0.0f)); // 225 degrees
                outerHull.AddVertex(new MeshmerizerVertex(-0.250000f, -0.433013f, 0.0f)); // 240 degrees
                outerHull.AddVertex(new MeshmerizerVertex(-0.129410f, -0.482963f, 0.0f)); // 255 degrees
                outerHull.AddVertex(new MeshmerizerVertex(0.000000f, -0.500000f, 0.0f)); // 270 degrees
                outerHull.AddVertex(new MeshmerizerVertex(0.129410f, -0.482963f, 0.0f)); // 285 degrees
                outerHull.AddVertex(new MeshmerizerVertex(0.250000f, -0.433013f, 0.0f)); // 300 degrees
                outerHull.AddVertex(new MeshmerizerVertex(0.353553f, -0.353553f, 0.0f)); // 315 degrees
                outerHull.AddVertex(new MeshmerizerVertex(0.433013f, -0.250000f, 0.0f)); // 330 degrees
                outerHull.AddVertex(new MeshmerizerVertex(0.482963f, -0.129410f, 0.0f)); // 345 degrees
                outerHull.AddVertex(new MeshmerizerVertex(0.500000f, 0.000000f, 0.0f)); // 0 degrees
                outerHull.AddVertex(new MeshmerizerVertex(0.482963f, 0.129410f, 0.0f)); // 15 degrees
                outerHull.AddVertex(new MeshmerizerVertex(0.433013f, 0.250000f, 0.0f)); // 30 degrees
            }
            else if ((primShape.ProfileCurve & 0x07) == (byte)ProfileShape.Square) // a ring
            {
                if (hollowShape == HollowShape.Same)
                    hollowShape = HollowShape.Square;

                outerHull.AddVertex(new MeshmerizerVertex(+0.5f, +0.5f, 0.0f));
                outerHull.AddVertex(new MeshmerizerVertex(-0.5f, +0.5f, 0.0f));
                outerHull.AddVertex(new MeshmerizerVertex(-0.5f, -0.5f, 0.0f));
                outerHull.AddVertex(new MeshmerizerVertex(+0.5f, -0.5f, 0.0f));
            }

            else if ((primShape.ProfileCurve & 0x07) == (byte)ProfileShape.EquilateralTriangle)
            {
                if (hollowShape == HollowShape.Same)
                    hollowShape = HollowShape.Triangle;

                outerHull.AddVertex(new MeshmerizerVertex(+0.255f, -0.375f, 0.0f));
                outerHull.AddVertex(new MeshmerizerVertex(+0.25f, +0.375f, 0.0f));
                outerHull.AddVertex(new MeshmerizerVertex(-0.5f, +0.0f, 0.0f));

            }
            else if ((primShape.ProfileCurve & 0x07) == (byte)ProfileShape.HalfCircle)
            {
                // sanity check here... some spheres have inverted normals which can trap avatars
                // so for now if the shape parameters are such that this may happen, revert to the
                // geodesic sphere mesh.. the threshold is arbitrary as it seems any twist on a sphere
                // will create some inverted normals
                if (
                    (System.Math.Abs(primShape.PathTwist - primShape.PathTwistBegin) > 65)
                    || (primShape.PathBegin == 0
                        && primShape.PathEnd == 0
                        && primShape.PathTwist == 0
                        && primShape.PathTwistBegin == 0
                        && primShape.ProfileBegin == 0
                        && primShape.ProfileEnd == 0
                        && hollowFactor == 0
                        ) // simple sphere, revert to geodesic shape
                    )
                {
                    return CreateSphereMesh(primName, primShape, size);
                }

                if (hollowFactor == 0)
                {
                    // the hull triangulator is happier with a minimal hollow
                    hollowFactor = 2000;
                }

                if (hollowShape == HollowShape.Same)
                    hollowShape = HollowShape.Circle;

                outerHull.AddVertex(new MeshmerizerVertex(0.250000f, 0.433013f, 0.0f)); // 60 degrees
                outerHull.AddVertex(new MeshmerizerVertex(0.129410f, 0.482963f, 0.0f)); // 75 degrees
                outerHull.AddVertex(new MeshmerizerVertex(0.000000f, 0.500000f, 0.0f)); // 90 degrees
                outerHull.AddVertex(new MeshmerizerVertex(-0.129410f, 0.482963f, 0.0f)); // 105 degrees
                outerHull.AddVertex(new MeshmerizerVertex(-0.250000f, 0.433013f, 0.0f)); // 120 degrees
                outerHull.AddVertex(new MeshmerizerVertex(-0.353553f, 0.353553f, 0.0f)); // 135 degrees
                outerHull.AddVertex(new MeshmerizerVertex(-0.433013f, 0.250000f, 0.0f)); // 150 degrees
                outerHull.AddVertex(new MeshmerizerVertex(-0.482963f, 0.129410f, 0.0f)); // 165 degrees
                outerHull.AddVertex(new MeshmerizerVertex(-0.500000f, 0.000000f, 0.0f)); // 180 degrees

                outerHull.AddVertex(new MeshmerizerVertex(0.500000f, 0.000000f, 0.0f)); // 0 degrees
                outerHull.AddVertex(new MeshmerizerVertex(0.482963f, 0.129410f, 0.0f)); // 15 degrees
                outerHull.AddVertex(new MeshmerizerVertex(0.433013f, 0.250000f, 0.0f)); // 30 degrees
                outerHull.AddVertex(new MeshmerizerVertex(0.353553f, 0.353553f, 0.0f)); // 45 degrees
            }

            // Deal with cuts now
            if ((profileBegin != 0) || (profileEnd != 0))
            {
                double fProfileBeginAngle = profileBegin / 50000.0 * 360.0;
                // In degree, for easier debugging and understanding
                //fProfileBeginAngle -= (90.0 + 45.0); // for some reasons, the SL client counts from the corner -X/-Y
                double fProfileEndAngle = 360.0 - profileEnd / 50000.0 * 360.0; // Pathend comes as complement to 1.0
                //fProfileEndAngle -= (90.0 + 45.0);
                if (fProfileBeginAngle < fProfileEndAngle)
                    fProfileEndAngle -= 360.0;

                if ((primShape.ProfileCurve & 0x07) == (byte)ProfileShape.HalfCircle)
                { // dimpled sphere uses profile cut but since it's a half circle the angles are smaller
                    fProfileBeginAngle = 0.0036f * (float)primShape.ProfileBegin;
                    fProfileEndAngle = 180.0f - 0.0036f * (float)primShape.ProfileEnd;
                    if (fProfileBeginAngle < fProfileEndAngle)
                        fProfileEndAngle -= 360.0f;
                    // a cut starting at 0 degrees with a hollow causes an infinite loop so move the start angle
                    // past it into the empty part of the circle to avoid this condition
                    if (fProfileBeginAngle == 0.0f) fProfileBeginAngle = -10.0f;
                }
                else if ((primShape.ProfileCurve & 0x07) == (byte)ProfileShape.Square)
                { // tube profile cut is offset 45 degrees from other prim types
                    fProfileBeginAngle += 45.0f;
                    fProfileEndAngle += 45.0f;
                    if (fProfileBeginAngle < fProfileEndAngle)
                        fProfileEndAngle -= 360.0;
                }
                else if ((primShape.ProfileCurve & 0x07) == (byte)ProfileShape.EquilateralTriangle)
                { // ring profile cut is offset 180 degrees from other prim types
                    fProfileBeginAngle += 180.0f;
                    fProfileEndAngle += 180.0f;
                    if (fProfileBeginAngle < fProfileEndAngle)
                        fProfileEndAngle -= 360.0;
                }

                // Note, that we don't want to cut out a triangle, even if this is a
                // good approximation for small cuts. Indeed we want to cut out an arc
                // and we approximate this arc by a polygon chain
                // Also note, that these vectors are of length 1.0 and thus their endpoints lay outside the model space
                // So it can easily be subtracted from the outer hull
                int iSteps = (int)(((fProfileBeginAngle - fProfileEndAngle) / 45.0) + .5);
                // how many steps do we need with approximately 45 degree
                double dStepWidth = (fProfileBeginAngle - fProfileEndAngle) / iSteps;

                MeshmerizerVertex origin = new MeshmerizerVertex(0.0f, 0.0f, 0.0f);

                // Note the sequence of vertices here. It's important to have the other rotational sense than in outerHull
                SimpleHull cutHull = new SimpleHull();
                cutHull.AddVertex(origin);
                for (int i = 0; i < iSteps; i++)
                {
                    double angle = fProfileBeginAngle - i * dStepWidth; // we count against the angle orientation!!!!
                    MeshmerizerVertex v = MeshmerizerVertex.FromAngle(angle * Math.PI / 180.0);
                    cutHull.AddVertex(v);
                }
                MeshmerizerVertex legEnd = MeshmerizerVertex.FromAngle(fProfileEndAngle * Math.PI / 180.0);
                // Calculated separately to avoid errors
                cutHull.AddVertex(legEnd);

                // m_log.DebugFormat("Starting cutting of the hollow shape from the prim {1}", 0, primName);
                SimpleHull cuttedHull = SimpleHull.SubtractHull(outerHull, cutHull);

                if ((primShape.ProfileCurve & 0x07) == (byte)ProfileShape.Circle)
                {
                    Quaternion zFlip = Quaternion.CreateFromAxisAngle(new Vector3(0.0f, 0.0f, 1.0f), (float)Math.PI);
                    MeshmerizerVertex vTmp = new MeshmerizerVertex(0.0f, 0.0f, 0.0f);
                    foreach (MeshmerizerVertex v in cuttedHull.getVertices())
                        if (v != null)
                        {
                            vTmp = v * zFlip;
                            v.X = vTmp.X;
                            v.Y = vTmp.Y;
                            v.Z = vTmp.Z;
                        }
                }

                outerHull = cuttedHull;
            }

            // Deal with the hole here
            if (hollowFactor > 0)
            {
                SimpleHull holeHull;

                if (hollowShape == HollowShape.Triangle)
                {
                    holeHull = new SimpleHull();

                    float hollowFactorF = (float)hollowFactor * 2.0e-5f;

                    if ((primShape.ProfileCurve & 0x07) == (byte)ProfileShape.EquilateralTriangle)
                    {
                        holeHull.AddVertex(new MeshmerizerVertex(+0.125f * hollowFactorF, -0.1875f * hollowFactorF, 0.0f));
                        holeHull.AddVertex(new MeshmerizerVertex(-0.25f * hollowFactorF, -0f * hollowFactorF, 0.0f));
                        holeHull.AddVertex(new MeshmerizerVertex(+0.125f * hollowFactorF, +0.1875f * hollowFactorF, 0.0f));
                    }
                    else if ((primShape.ProfileCurve & 0x07) == (byte)ProfileShape.HalfCircle)
                    {
                        holeHull.AddVertex(new MeshmerizerVertex(-0.500000f * hollowFactorF, 0.000000f * hollowFactorF, 0.0f)); // 180 degrees
                        holeHull.AddVertex(new MeshmerizerVertex(-0.250000f * hollowFactorF, 0.433013f * hollowFactorF, 0.0f)); // 120 degrees
                        holeHull.AddVertex(new MeshmerizerVertex(0.250000f * hollowFactorF, 0.433013f * hollowFactorF, 0.0f)); // 60 degrees
                        holeHull.AddVertex(new MeshmerizerVertex(0.500000f * hollowFactorF, 0.000000f * hollowFactorF, 0.0f)); // 0 degrees
                    }
                    else
                    {
                        holeHull.AddVertex(new MeshmerizerVertex(+0.25f * hollowFactorF, -0.45f * hollowFactorF, 0.0f));
                        holeHull.AddVertex(new MeshmerizerVertex(-0.5f * hollowFactorF, -0f * hollowFactorF, 0.0f));
                        holeHull.AddVertex(new MeshmerizerVertex(+0.25f * hollowFactorF, +0.45f * hollowFactorF, 0.0f));
                    }
                }
                else if (hollowShape == HollowShape.Square && (primShape.ProfileCurve & 0x07) == (byte)ProfileShape.HalfCircle)
                {
                    holeHull = new SimpleHull();

                    float hollowFactorF = (float)hollowFactor * 2.0e-5f;

                    holeHull.AddVertex(new MeshmerizerVertex(-0.707f * hollowFactorF, 0.0f, 0.0f)); // 180 degrees
                    holeHull.AddVertex(new MeshmerizerVertex(0.0f, 0.707f * hollowFactorF, 0.0f)); // 120 degrees
                    holeHull.AddVertex(new MeshmerizerVertex(0.707f * hollowFactorF, 0.0f, 0.0f)); // 60 degrees
                }
                else
                {
                    holeHull = BuildHoleHull(primShape, primShape.ProfileShape, hollowShape, hollowFactor);
                }

                if (holeHull != null)
                {
                    SimpleHull hollowedHull = SimpleHull.SubtractHull(outerHull, holeHull);

                    outerHull = hollowedHull;
                }
            }

            MeshmerizerMesh m = new MeshmerizerMesh();

            MeshmerizerVertex Seed1 = new MeshmerizerVertex(0.0f, -10.0f, 0.0f);
            MeshmerizerVertex Seed2 = new MeshmerizerVertex(-10.0f, 10.0f, 0.0f);
            MeshmerizerVertex Seed3 = new MeshmerizerVertex(10.0f, 10.0f, 0.0f);

            m.Add(Seed1);
            m.Add(Seed2);
            m.Add(Seed3);

            m.Add(new Triangle(Seed1, Seed2, Seed3));
            m.Add(outerHull.getVertices());

            InsertVertices(m.vertices, 3, m.triangles);

            m.Remove(Seed1);
            m.Remove(Seed2);
            m.Remove(Seed3);

            m.RemoveTrianglesOutside(outerHull);

            foreach (Triangle t in m.triangles)
                t.invertNormal();

            float skew = primShape.PathSkew * 0.01f;
            float pathScaleX = (float)(200 - primShape.PathScaleX) * 0.01f;
            float pathScaleY = (float)(200 - primShape.PathScaleY) * 0.01f;
            float profileXComp = pathScaleX * (1.0f - Math.Abs(skew));

            foreach (MeshmerizerVertex v in m.vertices)
                if (v != null)
                {
                    v.X *= profileXComp;
                    v.Y *= pathScaleY;
                    //v.Y *= 0.5f; // torus profile is scaled in y axis
                }

            Extruder extr = new Extruder();

            extr.size = size;
            extr.pathScaleX = pathScaleX;
            extr.pathScaleY = pathScaleY;
            extr.pathCutBegin = 0.00002f * primShape.PathBegin;
            extr.pathCutEnd = 0.00002f * (50000 - primShape.PathEnd);
            extr.pathBegin = primShape.PathBegin;
            extr.pathEnd = primShape.PathEnd;
            extr.skew = skew;
            extr.revolutions = 1.0f + (float)primShape.PathRevolutions * 3.0f / 200.0f;
            extr.pathTaperX = 0.01f * (float)primShape.PathTaperX;
            extr.pathTaperY = 0.01f * (float)primShape.PathTaperY;

            extr.radius = 0.01f * (float)primShape.PathRadiusOffset;

            if (pathShearX != 0)
            {
                if (pathShearX > 50)
                {
                    // Complimentary byte.  Negative values wrap around the byte.  Positive values go up to 50
                    extr.pushX = (((float)(256 - pathShearX) / 100) * -1f);
                }
                else
                {
                    extr.pushX = (float)pathShearX / 100;
                }
            }

            if (pathShearY != 0)
            {
                if (pathShearY > 50)
                {
                    // Complimentary byte.  Negative values wrap around the byte.  Positive values go up to 50
                    extr.pushY = (((float)(256 - pathShearY) / 100) * -1f);
                }
                else
                {
                    extr.pushY = (float)pathShearY / 100;
                }

            }

            extr.twistTop = (float)primShape.PathTwist * (float)Math.PI * 0.02f;
            extr.twistBot = (float)primShape.PathTwistBegin * (float)Math.PI * 0.02f;

            MeshmerizerMesh result = extr.ExtrudeCircularPath(m);

            return result;
        }

        public static MeshmerizerVertex midUnitRadialPoint(MeshmerizerVertex a, MeshmerizerVertex b, float radius)
        {
            MeshmerizerVertex midpoint = new MeshmerizerVertex(a + b) * 0.5f;
            return (midpoint.normalize() * radius);
        }

        public static void SphereLODTriangle(MeshmerizerVertex a, MeshmerizerVertex b, MeshmerizerVertex c, float diameter, float LOD, MeshmerizerMesh m)
        {
            MeshmerizerVertex aa = a - b;
            MeshmerizerVertex ba = b - c;
            MeshmerizerVertex da = c - a;

            if (((aa.length() < LOD) && (ba.length() < LOD) && (da.length() < LOD)))
            {
                // We don't want duplicate verticies.  Duplicates cause the scale algorithm to produce a spikeball
                // spikes are novel, but we want ellipsoids.

                if (!m.vertices.Contains(a))
                    m.Add(a);
                if (!m.vertices.Contains(b))
                    m.Add(b);
                if (!m.vertices.Contains(c))
                    m.Add(c);

                // Add the triangle to the mesh
                Triangle t = new Triangle(a, b, c);
                m.Add(t);
            }
            else
            {
                MeshmerizerVertex ab = midUnitRadialPoint(a, b, diameter);
                MeshmerizerVertex bc = midUnitRadialPoint(b, c, diameter);
                MeshmerizerVertex ca = midUnitRadialPoint(c, a, diameter);

                // Recursive!  Splits the triangle up into 4 smaller triangles
                SphereLODTriangle(a, ab, ca, diameter, LOD, m);
                SphereLODTriangle(ab, b, bc, diameter, LOD, m);
                SphereLODTriangle(ca, bc, c, diameter, LOD, m);
                SphereLODTriangle(ab, bc, ca, diameter, LOD, m);
            }
        }
    }
}
