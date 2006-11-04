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
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using libsecondlife;

namespace sceneviewer.Prims
{
    public abstract class PrimVisual
    {
        // Constants
        public const int MaxCut = 200;

        public Matrix Matrix;
        public VertexPositionColor[] VertexArray;
        public PrimObject Prim;
        public Vector3 Acceleration;
        public Vector3 Velocity;
        public Vector3 RotationVelocity;
        public BoundingBox BoundBox;
        public int LevelOfDetail = 16;

        //
        protected CrossSection[] OuterFaces; // Section for each extruded outer face
        protected CrossSection[] InnerFaces; // Section for each extruded inner face (hollow)
        protected CrossSection[] CutFaces; // Two cut faces

        protected Color color;

        //
        protected int NumberFaces; // Number of faces on the base primitive
        protected int FirstOuterFace; // If we're cutting, this might not be 0
        protected int LastOuterFace; // If we're cutting, this might not be iNumberFaces

        // Reference vertices of the unscaled/unrotated primitive
        protected Vector3[] ReferenceVertices;

        protected const int MaxFaces = 9;

        protected bool hollow;
        protected bool cut;

        protected List<VertexPositionColor> Vertexes;
        protected Color[] FaceColors = new Color[MaxFaces];

        // Abstract methods
        protected abstract void BuildFaces();
        protected abstract void BuildVertexes();
        protected abstract void AssignFaces();
        protected abstract int GetCutQuadrant(float cut);
        protected abstract float GetAngleWithXAxis(float cut);

        public PrimVisual(PrimObject prim)
        {
            Prim = prim;
            Vertexes = new List<VertexPositionColor>();
            VertexArray = Vertexes.ToArray();

            Acceleration = Vector3.Zero;
            Velocity = Vector3.Zero;
            RotationVelocity = Vector3.Zero;

            // TODO: This is temporary, for debugging and entertainment purposes
            Random rand = new Random((int)Prim.LocalID + Environment.TickCount);
            byte r = (byte)rand.Next(256);
            byte g = (byte)rand.Next(256);
            byte b = (byte)rand.Next(256);
            color = new Color(r, g, b);

            BuildMatrix();
        }

        public void Select()
        {
            // TODO: This is temporary, for debugging and entertainment purposes
            Random rand = new Random((int)Prim.LocalID + Environment.TickCount);
            byte r = (byte)rand.Next(256);
            byte g = (byte)rand.Next(256);
            byte b = (byte)rand.Next(256);
            color = new Color(r, g, b);

            BuildVertexes();
        }

        public void Deselect()
        {
            ;
        }

        public void Update(PrimUpdate primUpdate)
        {
            Prim.Position = primUpdate.Position;
            Prim.Rotation = primUpdate.Rotation;
            Acceleration = new Vector3(primUpdate.Acceleration.X, primUpdate.Acceleration.Y, primUpdate.Acceleration.Z);
            Velocity = new Vector3(primUpdate.Velocity.X, primUpdate.Velocity.Y, primUpdate.Velocity.Z);
            RotationVelocity = new Vector3(primUpdate.RotationVelocity.X, primUpdate.RotationVelocity.Y, primUpdate.RotationVelocity.Z);

            BuildMatrix();
        }

        private void BuildMatrix()
        {
            Matrix offset = Matrix.CreateTranslation(new Vector3(Prim.Position.X, Prim.Position.Y, Prim.Position.Z));
            Matrix rotation = Matrix.CreateFromQuaternion(new Quaternion(Prim.Rotation.X, Prim.Rotation.Y, Prim.Rotation.Z,
                Prim.Rotation.W));
            Matrix scaling = Matrix.CreateScale(Prim.Scale.X, Prim.Scale.Y, Prim.Scale.Z);

            Matrix = scaling * rotation * offset;
            
            // Now that we have the final transformation matrix we can create a proper bounding box
            // TODO: This code has only been tested with linear extrusion prims
            if (Prim.ParentID != 0)
            {
                float minX = -0.5f, minY = -0.5f, minZ = -0.5f;
                if (Prim.PathShearX < 0) minX *= Prim.PathShearX;
                if (Prim.PathShearY < 0) minY *= Prim.PathShearY;

                float maxX = 0.5f, maxY = 0.5f, maxZ = 0.5f;
                if (Prim.PathShearX > 0) maxX *= Prim.PathShearX;
                if (Prim.PathShearY > 0) maxY *= Prim.PathShearY;

                Vector3 min = Vector3.Transform(new Vector3(minX, minY, minZ), Matrix);
                Vector3 max = Vector3.Transform(new Vector3(maxX, maxY, maxZ), Matrix);

                BoundBox = new BoundingBox(min, max);
            }
            else
            {
                BoundBox = new BoundingBox();
            }
        }

        public static PrimVisual BuildPrimVisual(PrimObject prim)
        {
            if (prim.ProfileCurve == 1 && prim.PathCurve == 16)
            {
                // PRIM_TYPE_BOX
                return new PrimVisualBox(prim);
            }
            else if (prim.ProfileCurve == 0 && prim.PathCurve == 16)
            {
                // PRIM_TYPE_CYLINDER
                return new PrimVisualCylinder(prim);
            }
            else if (prim.ProfileCurve == 3 && prim.PathCurve == 16)
            {
                // PRIM_TYPE_PRISM
                return new PrimVisualPrism(prim);
            }
            else if (prim.ProfileCurve == 5 && prim.PathCurve == 32)
            {
                // PRIM_TYPE_SPHERE
                return new PrimVisualSphere(prim);
            }
            else if (prim.ProfileCurve == 0 && prim.PathCurve == 32)
            {
                // PRIM_TYPE_TORUS
                return new PrimVisualTorus(prim);
            }
            else if (prim.ProfileCurve == 1 && prim.PathCurve == 32)
            {
                // PRIM_TYPE_TUBE
                return new PrimVisualTube(prim);
            }
            else if (prim.ProfileCurve == 3 && prim.PathCurve == 32)
            {
                // PRIM_TYPE_RING
                return new PrimVisualRing(prim);
            }
            else
            {
                return null;
            }
        }
    }
}
