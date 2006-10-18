using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using libsecondlife;

namespace sceneviewer.Prims
{
    public class PrimVisualCylinder : LinearPrimVisual
    {
        public PrimVisualCylinder(PrimObject prim) : base(prim)
        {
            NumberFaces = 1;
            FirstOuterFace = 0;
            LastOuterFace = 0;

            if (prim.ProfileHollow != 0)
            {
                hollow = true;
                //InnerFaces = new CrossSection[4];
                //for (int i = 0; i < 4; i++)
                //{
                //    InnerFaces[i] = new CrossSection();
                //}
            }

            if (prim.ProfileBegin != 0 || prim.ProfileEnd != 1)
            {
                cut = true;
                //CutFaces = new CrossSection[2];
                //for (int i = 0; i < 2; i++)
                //{
                //    CutFaces[i] = new CrossSection();
                //}
            }

            BuildFaces();
        }

        protected override void AssignFaces()
        {
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
            ;
        }
    }
}