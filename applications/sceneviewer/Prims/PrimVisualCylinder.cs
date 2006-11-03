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
    public class PrimVisualCylinder : LinearPrimVisual
    {
        public PrimVisualCylinder(PrimObject prim)
            : base(prim)
        {
            NumberFaces = 1;
            FirstOuterFace = 0;
            LastOuterFace = 0;

            OuterFaces = new CrossSection[1];
            OuterFaces[0] = new CrossSection();

            if (prim.ProfileHollow != 0)
            {
                hollow = true;
                InnerFaces = new CrossSection[1];
                InnerFaces[0] = new CrossSection();

                //for (int i = 0; i < 4; i++)
                //{
                //    InnerFaces[i] = new CrossSection();
                //}
            }

            if (prim.ProfileBegin != 0 || prim.ProfileEnd != 1)
            {
                cut = true;
                CutFaces = new CrossSection[2];
                for (int i = 0; i < 2; i++)
                {
                    CutFaces[i] = new CrossSection();
                }
            }

            BuildFaces();
        }

        protected override void AssignFaces()
        {
        }

        protected Vector3 GetCutIntersect(float cut, float primHalfWidth)
        {
            double angle = cut * 2 * Math.PI;
            return new Vector3((float)(primHalfWidth * Math.Cos(angle)), (float)(primHalfWidth * Math.Sin(angle)), 0);
        }

        protected override float GetAngleWithXAxis(float cut)
        {
            return cut * 2 * (float)Math.PI;
        }

        protected override int GetCutQuadrant(float cut)
        {
            return 0;
        }

        protected override void BuildFaces()
        {
            float hollowRatio = Prim.ProfileHollow / 100.0f;

            Vector3 cutstartinnerface;
            Vector3 cutendinnerface;

            Vector3 cutstartouterface = GetCutIntersect(Prim.ProfileBegin, 0.5f);
            Vector3 cutendouterface = GetCutIntersect(Prim.ProfileEnd, 0.5f);

            OuterFaces[0].RemoveAllPoints();

            if (hollow)
            {
                InnerFaces[0].RemoveAllPoints();

                cutstartinnerface = GetCutIntersect(Prim.ProfileBegin, hollowRatio * 0.5f);
                cutendinnerface = GetCutIntersect(Prim.ProfileEnd, hollowRatio * 0.5f);

                if (cut)
                {
                    BuildCutHollowFaces(cutstartouterface, cutstartinnerface, cutendouterface, cutendinnerface);
                }
            }
            else if (cut)
            {
                BuildCutFaces(cutstartouterface, cutendouterface);
            }

            double angle = 0;
            double startAngle = Prim.ProfileBegin * 2 * Math.PI;
            double endAngle = Prim.ProfileEnd * 2 * Math.PI;

            Vector3 nextOuterPoint = Vector3.Zero;
            Vector3 nextInnerPoint = Vector3.Zero;

            for (int facePoint = 0; facePoint <= LevelOfDetail; facePoint++)
            {
                angle = startAngle + ((double)facePoint / (double)LevelOfDetail) * (endAngle - startAngle);

                nextOuterPoint.X = (float)(0.5 * Math.Cos(angle));
                nextOuterPoint.Y = (float)(0.5 * Math.Sin(angle));
                OuterFaces[0].AddPoint(nextOuterPoint);
            }

            if (hollow)
            {
                //for (int facePoint = LevelOfDetail; facePoint >= 0; facePoint--)
                for (int facePoint = 0; facePoint <= LevelOfDetail; facePoint++)
                {
                    angle = startAngle + ((double)facePoint / (double)LevelOfDetail) * (endAngle - startAngle);

                    nextInnerPoint.X = (float)(0.5 * Math.Cos(angle) * hollowRatio);
                    nextInnerPoint.Y = (float)(0.5 * Math.Sin(angle) * hollowRatio);
                    InnerFaces[0].AddPoint(nextInnerPoint);
                }
            }

            // TODO: Texturemapping

            AssignFaces();

            BuildVertexes();
        }

        protected override void BuildEndCapHollow(bool top)
        {
            float z = top ? 0.5f : -0.5f;

            // We assume the innerfaces and outerfaces point counts are the same
            int pointCount = OuterFaces[0].GetNumPoints();

            for (int j = 0; j < pointCount - 1; j++)
            {
                Vector3 p1 = OuterFaces[0].GetRawVertex(j);
                Vector3 p2 = OuterFaces[0].GetRawVertex(j + 1);
                Vector3 p3 = InnerFaces[0].GetRawVertex(j);
                Vector3 p4 = InnerFaces[0].GetRawVertex(j + 1);

                p1.Z = p2.Z = p3.Z = p4.Z = z;

                // TODO: Texturemapping
                //Vector2 t1 = texturemapping.GetTextureCoordinate(new Vector2(1 - (r1.x + 0.5), r1.y + 0.5));
                //Vector2 t2 = texturemapping.GetTextureCoordinate(new Vector2(1 - (r2.x + 0.5), r2.y + 0.5));
                //Vector2 t3 = texturemapping.GetTextureCoordinate(new Vector2(1 - (r3.x + 0.5), r3.y + 0.5));
                //Vector2 t4 = texturemapping.GetTextureCoordinate(new Vector2(1 - (r4.x + 0.5), r4.y + 0.5));

                float transformRatio = top ? 1 : 0;

                p1 = Transform(p1, transformRatio);
                p2 = Transform(p2, transformRatio);
                p3 = Transform(p3, transformRatio);
                p4 = Transform(p4, transformRatio);

                Vertexes.Add(new VertexPositionColor(p4, color));
                Vertexes.Add(new VertexPositionColor(p3, color));
                Vertexes.Add(new VertexPositionColor(p2, color));

                Vertexes.Add(new VertexPositionColor(p3, color));
                Vertexes.Add(new VertexPositionColor(p2, color));
                Vertexes.Add(new VertexPositionColor(p1, color));
            }
        }
    }
}
