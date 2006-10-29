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

            BuildMatrix();
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
            Matrix rotation = Matrix.FromQuaternion(new Quaternion(Prim.Rotation.X, Prim.Rotation.Y, Prim.Rotation.Z,
                Prim.Rotation.W));
            Matrix scaling = Matrix.CreateScale(Prim.Scale.X, Prim.Scale.Y, Prim.Scale.Z);

            Matrix = scaling * rotation * offset;
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
