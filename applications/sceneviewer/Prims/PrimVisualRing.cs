using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using libsecondlife;

namespace sceneviewer.Prims
{
    public class PrimVisualRing : LinearPrimVisual
    {
        public PrimVisualRing(PrimObject prim)
            : base(prim)
        {
            ;
        }

        protected override void AssignFaces()
        {
        }

        protected override int GetCutQuadrant(float cut)
        {
            // FIXME: Our cut is already normalized, so this function won't work
            //return ((cut / 67) % 3 + 3) % 3;
            return 0;
        }

        protected override float GetAngleWithXAxis(float cut)
        {
            return (cut - 0.125f) * 2 * (float)Math.PI;
        }

        protected override void BuildEndCapHollow(bool top)
        {
        }
    }
}