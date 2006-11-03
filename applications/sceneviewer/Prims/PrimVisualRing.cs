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
    public class PrimVisualRing : RotationalPrimVisual
    {
        public PrimVisualRing(PrimObject prim)
            : base(prim)
        {
            NumberFaces = 3;
            FirstOuterFace = 0;
            LastOuterFace = 2;

            ReferenceVertices = new Vector3[3];

            ReferenceVertices[0] = new Vector3(0.5f, -0.5f, 0f);
            ReferenceVertices[1] = new Vector3(0f, 0.5f, 0f);
            ReferenceVertices[2] = new Vector3(-0.5f, -0.5f, 0f);

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

        protected override void BuildFaces()
        {
        }

        protected override int GetCutQuadrant(float cut)
        {
            return (int)(cut / (67f / 255f)) % 3;
        }

        protected override float GetAngleWithXAxis(float cut)
        {
            return (cut - 0.125f) * 2 * (float)Math.PI;
        }

        //protected override void BuildEndCapHollow(bool top)
        //{
        //}
    }
}