/*
 * Copyright (c) 2006, Second Life Reverse Engineering Team
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the Second Life Reverse Engineering Team nor the names
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
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using libsecondlife;

namespace sceneviewer.Prims
{
    public abstract class LinearPrimVisual : PrimVisual
    {
        // Abstract functions
        protected abstract void BuildEndCapHollow(bool top);

        public LinearPrimVisual(PrimObject prim)
            : base(prim)
        {
        }

        protected override void BuildFaces()
        {
            Vector3 cutstartouterface = Vector3.Zero;
            Vector3 cutendouterface = Vector3.Zero;
            Vector3 cutstartinnerface = Vector3.Zero;
            Vector3 cutendinnerface = Vector3.Zero;
            int cutStartDiagQuadrant = cutStartDiagQuadrant = GetCutQuadrant(Prim.ProfileBegin);
            int cutEndDiagQuadrant = cutEndDiagQuadrant = GetCutQuadrant(Prim.ProfileEnd);
            float hollowRatio = (float)Prim.ProfileHollow / 100.0f;

            cutstartouterface = GetCutIntersect(Prim.ProfileBegin, 0.5f);  // coordinates of where the cut starts
            cutendouterface = GetCutIntersect(Prim.ProfileEnd, 0.5f);  // coordinates of where the cut starts

            if (hollow)
            {
                float halfWidth = hollowRatio * 0.5f;
                cutstartinnerface = GetCutIntersect(Prim.ProfileBegin, halfWidth);
                cutendinnerface = GetCutIntersect(Prim.ProfileEnd, halfWidth);
            }

            if (cut)
            {
                if (hollow)
                {
                    BuildCutHollowFaces(cutstartouterface, cutstartinnerface, cutendouterface, cutendinnerface);
                }
                else
                {
                    BuildCutFaces(cutstartouterface, cutendouterface);
                }
            }

            if (cutStartDiagQuadrant == cutEndDiagQuadrant)
            {
                FirstOuterFace = LastOuterFace = cutStartDiagQuadrant;

                OuterFaces[0].RemoveAllPoints();
                OuterFaces[0].AddPoint(cutstartouterface);
                OuterFaces[0].AddPoint(cutendouterface);
                //OuterFaces[0].TextureMapping = texturemapping;

                if (hollow)
                {
                    InnerFaces[0].RemoveAllPoints();
                    InnerFaces[0].AddPoint(cutendinnerface);
                    InnerFaces[0].AddPoint(cutstartinnerface);
                    //InnerFaces[0].TextureMapping = texturemapping;
                }
            }
            else
            {
                FirstOuterFace = cutStartDiagQuadrant;

                float totalInnerLength = 0;
                float startSideInnerLength = 0;
                float wholeSideLength = 0;

                PopulateSingleCutFacePositiveDirection(ref OuterFaces[FirstOuterFace], cutstartouterface, cutStartDiagQuadrant, 0.5f, true);
                //OuterFaces[FirstOuterFace].TextureMapping = texturemapping;

                if (hollow)
                {
                    startSideInnerLength = PopulateSingleCutFacePositiveDirection(ref InnerFaces[FirstOuterFace],
                        cutstartinnerface, cutStartDiagQuadrant, hollowRatio * 0.5f, false);
                    //InnerFaces[FirstOuterFace].TextureMapping = texturemapping;
                    totalInnerLength += startSideInnerLength;
                }

                int quadrant = cutStartDiagQuadrant + 1;

                while (quadrant < cutEndDiagQuadrant)
                {
                    PopulateCompleteSide(ref OuterFaces[quadrant], quadrant, 0.5f, true);
                    //OuterFaces[quadrant].TextureMapping = texturemapping;

                    if (hollow)
                    {
                        wholeSideLength = PopulateCompleteSide(ref InnerFaces[quadrant], quadrant,
                            hollowRatio * 0.5f, false);
                        //InnerFaces[quadrant].TextureMapping = texturemapping;
                        totalInnerLength += wholeSideLength;
                    }

                    quadrant++;
                }

                PopulateSingleCutFaceNegativeDirection(ref OuterFaces[quadrant], cutendouterface,
                    cutEndDiagQuadrant, 0.5f, true);
                //OuterFaces[quadrant].TextureMapping = texturemapping;

                if (hollow)
                {
                    float endSideInnerLength = PopulateSingleCutFaceNegativeDirection(ref InnerFaces[quadrant],
                        cutendinnerface, cutEndDiagQuadrant, hollowRatio * 0.5f, false);
                    //InnerFaces[quadrant].TextureMapping = texturemapping;
                    totalInnerLength += endSideInnerLength;
                }

                LastOuterFace = quadrant;

                if (hollow)
                {
                    //SetupInnerFaceTextureOffsets(startSideInnerLength, wholeSideLength, totalInnerLength);
                }
            }

            AssignFaces();

            BuildVertexes();
        }

        protected void BuildCutFaces(Vector3 cutstartouterface, Vector3 cutendouterface)
        {
            CutFaces[0].RemoveAllPoints();

            CutFaces[0].AddPoint(Vector3.Zero);
            CutFaces[0].AddPoint(cutstartouterface);
            //CutFaces[0].TextureMapping = texturemapping;

            CutFaces[1].RemoveAllPoints();

            CutFaces[1].AddPoint(cutendouterface);
            CutFaces[1].AddPoint(Vector3.Zero);
            //CutFaces[1].TextureMapping = texturemapping;
        }

        protected void BuildCutHollowFaces(Vector3 cutstartouterface, Vector3 cutstartinnerface, Vector3 cutendouterface, Vector3 cutendinnerface)
        {
            CutFaces[0].RemoveAllPoints();

            CutFaces[0].AddPoint(cutstartinnerface);
            CutFaces[0].AddPoint(cutstartouterface);
            //CutFaces[0].TextureMapping = texturemapping;

            CutFaces[1].RemoveAllPoints();

            CutFaces[1].AddPoint(cutendouterface);
            CutFaces[1].AddPoint(cutendinnerface);
            //CutFaces[1].TextureMapping = texturemapping;
        }

        private Vector3 GetCutIntersect(float cut, float cubeHalfWidth)
        {
            int cutQuadrant = GetCutQuadrant(cut);

            Vector3 lineend;
            Vector3 linestart = ReferenceVertices[cutQuadrant] * cubeHalfWidth;
            linestart = Vector3.Divide(linestart, 0.5f);
            if (cutQuadrant < NumberFaces - 1)
            {
                lineend = ReferenceVertices[cutQuadrant + 1] * cubeHalfWidth;
            }
            else
            {
                lineend = ReferenceVertices[0] * cubeHalfWidth;
            }
            lineend = Vector3.Divide(lineend, 0.5f);

            //
            float angle = GetAngleWithXAxis(cut);

            // CutVectorPerp is perpendicular to the radius vector
            Vector3 cutVectorPerp = new Vector3((float)-Math.Sin(angle), (float)Math.Cos(angle), 0);
            Vector3 delta = lineend - linestart;

            // From http://softsurfer.com/Archive/algorithm_0104/algorithm_0104B.htm
            Vector3 result = linestart - delta * Vector3.Dot(cutVectorPerp, linestart) / Vector3.Dot(cutVectorPerp, delta);

            return result;
        }

        // Handles the first face in the cut, starting from cutstart, 
        // and running anticlockwise to first reference vertex
        private float PopulateSingleCutFacePositiveDirection(ref CrossSection face, Vector3 cutPoint, int quadrant, 
            float halfCubeWidth, bool outer)
        {
            quadrant = NormalizeQuadrant(quadrant);

            face.RemoveAllPoints();

            Vector3 startPoint = cutPoint;
            Vector3 endPoint;
            if (quadrant < NumberFaces - 1)
            {
                endPoint = ReferenceVertices[quadrant + 1] * halfCubeWidth / 0.5f;
            }
            else
            {
                endPoint = ReferenceVertices[0] * halfCubeWidth / 0.5f;
            }

            if (outer)
            {
                face.AddPoint(startPoint);
                face.AddPoint(endPoint);
            }
            else
            {
                face.AddPoint(endPoint);
                face.AddPoint(startPoint);
            }

            return Vector3.Distance(startPoint, endPoint);
        }

        private float PopulateSingleCutFaceNegativeDirection(ref CrossSection face, Vector3 cutPoint, int quadrant, 
            float halfCubeWidth, bool outer)
        {
            quadrant = NormalizeQuadrant(quadrant);

            face.RemoveAllPoints();

            Vector3 startPoint = ReferenceVertices[quadrant] * halfCubeWidth / 0.5f;
            Vector3 endPoint = cutPoint;

            if (outer)
            {
                face.AddPoint(startPoint);
                face.AddPoint(endPoint);
            }
            else
            {
                face.AddPoint(endPoint);
                face.AddPoint(startPoint);
            }

            return Vector3.Distance(startPoint, endPoint);
        }

        private float PopulateCompleteSide(ref CrossSection face, int quadrant, float halfCubeWidth, bool outer)
        {
            quadrant = NormalizeQuadrant(quadrant);

            face.RemoveAllPoints();

            Vector3 startPoint = ReferenceVertices[quadrant];
            Vector3 endPoint;
            if (quadrant < NumberFaces - 1)
            {
                endPoint = ReferenceVertices[quadrant + 1];
            }
            else
            {
                endPoint = ReferenceVertices[0];
            }

            startPoint = startPoint * halfCubeWidth / 0.5f;
            endPoint = endPoint * halfCubeWidth / 0.5f;

            if (outer)
            {
                face.AddPoint(startPoint);
                face.AddPoint(endPoint);
            }
            else
            {
                face.AddPoint(endPoint);
                face.AddPoint(startPoint);
            }

            return 2f * halfCubeWidth;
        }

        protected override void BuildVertexes()
        {
            Vertexes.Clear();

            // For prims with a linear extrusion path, we base the number of transformations on the amount of twist
            int transforms = 1 + Math.Abs((int)((float)(Prim.PathTwist - Prim.PathTwistBegin) / 9f));

            // Build the outer sides
            BuildSideVertexes(OuterFaces, transforms);

            if (hollow)
            {
                // Build the inner sides
                BuildSideVertexes(InnerFaces, transforms);
            }

            if (cut)
            {
                // Build the cut sides (between the inner and outer)
                BuildSideVertexes(CutFaces, transforms);
            }

            // Build the top and bottom end caps
            if (hollow)
            {
                BuildEndCapHollow(true);
                BuildEndCapHollow(false);
            }
            else
            {
                if (cut)
                {
                    BuildEndCapCutNoHollow(true);
                    BuildEndCapCutNoHollow(false);
                }
                else
                {
                    BuildEndCapNoCutNoHollow(true);
                    BuildEndCapNoCutNoHollow(false);
                }
            }
            
            VertexArray = Vertexes.ToArray();
        }

        protected void BuildSideVertexes(CrossSection[] crossSection, int transforms)
        {
            float transformOffset = 1.0f / (float)transforms;
            float currentOffset = -0.5f;

            for (int i = 0; i < transforms; i++)
            {
                for (int j = 0; j < crossSection.Length; j++)
                {
                    int pointCount = crossSection[j].GetNumPoints();

                    if (pointCount > 0)
                    {
                        for (int k = 0; k < pointCount - 1; k++)
                        {
                            Vector3 lower1, lower2, upper1, upper2;
                            float lowerRatio = (float)i / (float)transforms;
                            float upperRatio = (float)(i + 1) / (float)transforms;

                            lower1 = crossSection[j].GetRawVertex(k);
                            lower2 = crossSection[j].GetRawVertex(k + 1);

                            lower1.Z = currentOffset;
                            lower2.Z = currentOffset;

                            upper1 = lower1;
                            upper2 = lower2;

                            upper1.Z = currentOffset + transformOffset;
                            upper2.Z = currentOffset + transformOffset;

                            lower1 = Transform(lower1, lowerRatio);
                            lower2 = Transform(lower2, lowerRatio);
                            upper1 = Transform(upper1, upperRatio);
                            upper2 = Transform(upper2, upperRatio);

                            Vertexes.Add(new VertexPositionColor(lower1, color));
                            Vertexes.Add(new VertexPositionColor(lower2, color));
                            Vertexes.Add(new VertexPositionColor(upper2, color));

                            Vertexes.Add(new VertexPositionColor(lower1, color));
                            Vertexes.Add(new VertexPositionColor(upper2, color));
                            Vertexes.Add(new VertexPositionColor(upper1, color));
                        }
                    }
                }

                currentOffset += transformOffset;
            }
        }

        protected void BuildEndCapNoCutNoHollow(bool top)
        {
            float z = top ? 0.5f : -0.5f;

            for (int i = 0; i < OuterFaces.Length; i++)
            {
                int pointCount = OuterFaces[i].GetNumPoints();

                if (pointCount > 0)
                {
                    for (int j = 0; j < pointCount - 1; j++)
                    {
                        Vector3 first = OuterFaces[i].GetRawVertex(j);
                        first.Z = z;
                        Vector3 second = OuterFaces[i].GetRawVertex(j + 1);
                        second.Z = z;
                        Vector3 center = new Vector3(0, 0, z);

                        float transformRatio = top ? 1 : 0;

                        // Apply the transformation to each vertex
                        first = Transform(first, transformRatio);
                        second = Transform(second, transformRatio);
                        center = Transform(center, transformRatio);

                        Vertexes.Add(new VertexPositionColor(first, color));
                        Vertexes.Add(new VertexPositionColor(second, color));
                        Vertexes.Add(new VertexPositionColor(center, color));
                    }
                }
            }
        }

        protected void BuildEndCapCutNoHollow(bool top)
        {
            float z = top ? 0.5f : -0.5f;

            for (int i = FirstOuterFace; i <= LastOuterFace; i++)
            {
                int pointCount = OuterFaces[i].GetNumPoints();

                for (int j = 0; j < pointCount - 1; j++)
                {
                    Vector3 first = OuterFaces[i].GetRawVertex(j);
                    first.Z = z;
                    Vector3 second = OuterFaces[i].GetRawVertex(j + 1);
                    second.Z = z;
                    Vector3 center = new Vector3(0, 0, z);

                    // TODO: Texturemapping stuff
                    //Vector2 t1 = texturemapping.GetTextureCoordinate(new Vector2(1 - (p1.x + 0.5), p1.y + 0.5));
                    //Vector2 t2 = texturemapping.GetTextureCoordinate(new Vector2(1 - (p2.x + 0.5), p2.y + 0.5));

                    float transformRatio = top ? 1 : 0;

                    first = Transform(first, transformRatio);
                    second = Transform(second, transformRatio);
                    center = Transform(center, transformRatio);

                    Vertexes.Add(new VertexPositionColor(first, color));
                    Vertexes.Add(new VertexPositionColor(second, color));
                    Vertexes.Add(new VertexPositionColor(center, color));
                }
            }
        }

        protected Vector3 Transform(Vector3 v, float ratio)
        {
            Matrix transform = Matrix.Identity;

            // Top Shear
            transform.Translation = new Vector3(ratio * Prim.PathShearX, ratio * Prim.PathShearY, 0);

            // Taper
            float xScale = 1.0f, yScale = 1.0f;
            if (Prim.PathTaperX != 0)
            {
                float adjratio = (Prim.PathTaperX < 0) ? (1.0f - ratio) : (ratio);
                xScale -= Math.Abs(Prim.PathTaperX) * adjratio;
            }
            if (Prim.PathTaperY != 0)
            {
                float adjratio = (Prim.PathTaperY < 0) ? (1.0f - ratio) : (ratio);
                yScale -= Math.Abs(Prim.PathTaperY) * adjratio;
            }
            transform *= Matrix.CreateScale(xScale, yScale, 1.0f);

            // Twist
            float twistBegin = (float)Prim.PathTwistBegin * MathHelper.Pi / 180.0f;
            float twistEnd = (float)Prim.PathTwist * MathHelper.Pi / 180.0f;
            float twist = (twistEnd - twistBegin) * ratio;
            transform *= Matrix.CreateRotationZ(-twist);

            // Apply the transformation matrix to this point
            return Vector3.Transform(v, transform);
        }

        private int NormalizeQuadrant(int quadrant)
        {
            return ((quadrant % NumberFaces) + NumberFaces) % NumberFaces;
        }
    }
}
