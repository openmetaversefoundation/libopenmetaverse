using System;
using System.Collections.Generic;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.Rendering;

namespace Simian.Extensions
{
    public class RenderingPluginMesher : IExtension<Simian>, IMeshingProvider
    {
        Simian server;
        IRendering Renderer;

        public RenderingPluginMesher()
        {
        }

        public void Start(Simian server)
        {
            this.server = server;

            // Search for a the best available OpenMetaverse.Rendering plugin
            List<string> renderers = RenderingLoader.ListRenderers(AppDomain.CurrentDomain.BaseDirectory);

            string renderer = renderers.Find(
                delegate(string current) { return current.Contains("OpenMetaverse.Rendering.GPL.dll"); });

            if (String.IsNullOrEmpty(renderer))
            {
                if (renderers.Count > 0)
                {
                    renderer = renderers[0];
                }
                else
                {
                    Logger.Log("No suitable OpenMetaverse.Rendering plugins found", Helpers.LogLevel.Error);
                    return;
                }
            }

            Renderer = RenderingLoader.LoadRenderer(renderer);
        }

        public void Stop()
        {
        }

        public SimpleMesh GenerateSimpleMesh(Primitive prim, DetailLevel lod)
        {
            return Renderer.GenerateSimpleMesh(prim, lod);
        }
    }
}
