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
    public class PrimVisualBox : LinearPrimVisual
    {
        public PrimVisualBox(PrimObject prim) : base(prim)
        {
            NumberFaces = 4;
            FirstOuterFace = 0;
            LastOuterFace = 3;

            ReferenceVertices = new Vector3[4];

            ReferenceVertices[1] = new Vector3(0.5f, -0.5f, 0f);
            ReferenceVertices[2] = new Vector3(0.5f, 0.5f, 0f);
            ReferenceVertices[3] = new Vector3(-0.5f, 0.5f, 0f);
            ReferenceVertices[0] = new Vector3(-0.5f, -0.5f, 0f);

            OuterFaces = new CrossSection[4];
            for (int i = 0; i < 4; i++)
            {
                OuterFaces[i] = new CrossSection();
            }

            if (prim.ProfileHollow != 0)
            {
                hollow = true;
                InnerFaces = new CrossSection[4];
                for (int i = 0; i < 4; i++)
                {
                    InnerFaces[i] = new CrossSection();
                }
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

        protected override int GetCutQuadrant(float cut)
        {
            if (cut == 1) { return 3; }
            else { return (int)(cut * 4.0); }
        }

        protected override float GetAngleWithXAxis(float cut)
        {
            //return (cut - 0.125f) * 2f * (float)Math.PI;
            return (cut + 0.125f) * 2f * (float)Math.PI;
        }

        protected override void BuildEndCapHollow(bool top)
        {
            float z = top ? 0.5f : -0.5f;

            for (int i = FirstOuterFace; i <= LastOuterFace; i++)
            {
                int pointCount = OuterFaces[i].GetNumPoints();

                if (pointCount >= 2)
                {
                    Vector3 p1 = OuterFaces[i].GetRawVertex(0);
                    Vector3 p2 = OuterFaces[i].GetRawVertex(1);
                    Vector3 p3 = InnerFaces[i].GetRawVertex(0);
                    Vector3 p4 = InnerFaces[i].GetRawVertex(1);

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

                    Vertexes.Add(new VertexPositionColor(p4, color));
                    Vertexes.Add(new VertexPositionColor(p2, color));
                    Vertexes.Add(new VertexPositionColor(p1, color));
                }
            }
        }
    }
}
