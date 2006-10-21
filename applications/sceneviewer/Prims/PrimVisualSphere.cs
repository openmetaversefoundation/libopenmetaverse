using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using libsecondlife;

namespace sceneviewer.Prims
{
    public class PrimVisualSphere : LinearPrimVisual
    {
        public PrimVisualSphere(PrimObject prim)
            : base(prim)
        {
            ;
        }

        protected override void AssignFaces()
        {
        }

        protected override int GetCutQuadrant(float cut)
        {
            // FIXME: ?
            return 0;
        }

        protected override float GetAngleWithXAxis(float cut)
        {
            // FIXME: Is this correct?
            return (cut - 0.125f) * 2 * (float)Math.PI;
        }

        protected override void BuildEndCapHollow(bool top)
        {
        }
    }
}
