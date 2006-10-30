using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using libsecondlife;

namespace sceneviewer.Prims
{
    public abstract class RotationalPrimVisual : PrimVisual
    {
        public RotationalPrimVisual(PrimObject prim)
            : base(prim)
        {
            // TODO: This is temporary, for debugging and entertainment purposes
            Random rand = new Random((int)prim.LocalID + Environment.TickCount);
            byte r = (byte)rand.Next(256);
            byte g = (byte)rand.Next(256);
            byte b = (byte)rand.Next(256);
            color = new Color(r, g, b);
        }
    }
}
