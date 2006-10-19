using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using libsecondlife;

namespace sceneviewer.Prims
{
    public class PrimVisualTube : LinearPrimVisual
    {
        public PrimVisualTube(PrimObject prim)
            : base(prim)
        {
            ;
        }

        protected override void AssignFaces()
        {
        }

        protected override int GetCutQuadrant(float cut)
        {
            // FIXME: This is wrong
            //return ((cut / 50) % 4 + 4) % 4;
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