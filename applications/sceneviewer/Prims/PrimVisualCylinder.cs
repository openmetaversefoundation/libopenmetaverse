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
                for (int facePoint = LevelOfDetail; facePoint >= 0; facePoint--)
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
        }
    }
}
