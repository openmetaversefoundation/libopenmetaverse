using System;

namespace sceneviewer
{
    partial class Viewer
    {
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.graphics = new Microsoft.Xna.Framework.Components.GraphicsComponent();

            this.GameComponents.Add(this.graphics);

        }

        private Microsoft.Xna.Framework.Components.GraphicsComponent graphics;
    }
}
