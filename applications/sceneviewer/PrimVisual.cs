using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace sceneviewer
{
    class PrimVisual : GameComponent
    {
        //private World world;
        private float scale;

        // effects and shaders
        private CompiledEffect compiledEffect;
        private Effect effect;

        // matrices for defining scale, world, and view projection
        private Matrix scaleMatrix;
        private Matrix worldMatrix;
        private Matrix viewProjectionMatrix;

        public PrimVisual()
        {
            //InitializeComponent();

            // initialize default values
            scale = 1.0f;
            worldMatrix = Matrix.Identity;
        }
    }
}
