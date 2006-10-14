using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using libsecondlife;

namespace sceneviewer.Prims
{
    public class CrossSection
    {
        private List<Vector3> Points;

        public CrossSection()
        {
            Points = new List<Vector3>();
        }

        public void AddPoint(Vector3 point)
        {
            Points.Add(point);
        }

        public void RemoveAllPoints()
        {
            Points.Clear();
        }

        public int GetNumPoints()
        {
            return Points.Count;
        }

        public Vector3 GetRawVertex(int index)
        {
            return Points[index];
        }
    }
}
