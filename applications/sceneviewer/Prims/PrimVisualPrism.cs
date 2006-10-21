using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using libsecondlife;

namespace sceneviewer.Prims
{
    public class PrimVisualPrism : LinearPrimVisual
    {
        public PrimVisualPrism(PrimObject prim)
            : base(prim)
        {
            ;
        }

        protected override void AssignFaces()
        {
        }

        protected override int GetCutQuadrant(float cut)
        {
            // FIXME
            //return cut / 67;
            return 0;
        }

        // should return angle in radians for a given cut ratio (?)
        protected override float GetAngleWithXAxis(float cut)
        {
            return (cut - (30f / 360f)) * 2 * (float)Math.PI;
        }

        protected override void BuildEndCapHollow(bool top)
        {
        }
    }
}