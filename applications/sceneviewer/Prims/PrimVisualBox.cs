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
    }
}
