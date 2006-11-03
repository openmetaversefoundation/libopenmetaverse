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
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace sceneviewer
{
    public struct VertexPosTexNormalTanBitan
    {
        public static readonly VertexElement[] VertexElements =
            new VertexElement[] { 
                new VertexElement(0, 0, VertexElementFormat.Vector3,
                    VertexElementMethod.Default, VertexElementUsage.Position, 0),
                new VertexElement(0, sizeof(float) * 3, VertexElementFormat.Vector2,
                    VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 0),
                new VertexElement(0, sizeof(float) * 5, VertexElementFormat.Vector3,
                    VertexElementMethod.Default, VertexElementUsage.Normal, 0),
                new VertexElement(0, sizeof(float) * 8, VertexElementFormat.Vector3,
                    VertexElementMethod.Default, VertexElementUsage.Tangent, 0),
                new VertexElement(0, sizeof(float) * 11, VertexElementFormat.Vector3,
                    VertexElementMethod.Default, VertexElementUsage.Binormal, 0),                
            };

        public Vector3 Position { get { return pos; } set { pos = value; } }
        public Vector3 Normal { get { return normal; } set { normal = value; } }
        public Vector2 Tex { get { return tex; } set { tex = value; } }
        public Vector3 Tan { get { return tan; } set { tan = value; } }
        public Vector3 Bitan { get { return bitan; } set { bitan = value; } }
        public static int SizeInBytes { get { return sizeof(float) * 14; } }

        Vector3 pos;
        Vector2 tex;
        Vector3 normal, tan, bitan;

        public VertexPosTexNormalTanBitan(Vector3 position, Vector2 uv,
            Vector3 normal, Vector3 tan, Vector3 bitan)
        {
            pos = position;
            tex = uv;
            this.normal = normal;
            this.tan = tan;
            this.bitan = bitan;
        }

        public static bool operator !=(VertexPosTexNormalTanBitan left, VertexPosTexNormalTanBitan right)
        {
            return left.GetHashCode() != right.GetHashCode();
        }

        public static bool operator ==(VertexPosTexNormalTanBitan left, VertexPosTexNormalTanBitan right)
        {
            return left.GetHashCode() == right.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this == (VertexPosTexNormalTanBitan)obj;
        }

        public override int GetHashCode()
        {
            return pos.GetHashCode() |
                         tex.GetHashCode() |
                         normal.GetHashCode() |
                         tan.GetHashCode() |
                         bitan.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2}", pos.X, pos.Y, pos.Z);
        }
    }
}
