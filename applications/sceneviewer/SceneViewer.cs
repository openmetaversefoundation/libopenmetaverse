using System;

namespace sceneviewer
{
    static class SceneViewer
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Viewer game = new Viewer())
            {
                game.Run();
            }
        }
    }
}

