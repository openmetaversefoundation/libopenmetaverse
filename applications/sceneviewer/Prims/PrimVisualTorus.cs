using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using libsecondlife;

namespace sceneviewer.Prims
{
    public class PrimVisualTorus : LinearPrimVisual
    {
        public PrimVisualTorus(PrimObject prim)
            : base(prim)
        {
            ;
        }

        protected override void AssignFaces()
        {
        }

        protected override int GetCutQuadrant(float cut)
        {
            return 0;
        }

        protected override float GetAngleWithXAxis(float cut)
        {
            return cut * 2 * (float)Math.PI;
        }

        protected override void BuildEndCapHollow(bool top)
        {
        }
    }
}
