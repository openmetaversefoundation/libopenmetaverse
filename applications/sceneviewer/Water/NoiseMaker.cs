using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace sceneviewer.Water
{
    class NoiseMaker
    {
        // Constants
        public const int npsize = 16;
        public const int ndecbits = 12;
        public const int ndecmag = 4096;
        public const int maxoctaves = 32;
        public const int noiseframes = 256;
        public const int noisedecbits = 15;
        public const int noisemagnitude = 8192;
        public const int scaledecbits = 15;
        public const int scalemagnitude = 8192;
        public const int nmapsizex = 512;
        public const int nmapsizey = 1024;

        public Texture2D HeightMap;
        public Texture2D NormalMap;
        public Texture2D[] PackedNoise;
        public Surface DepthStencil;

        private GraphicsDevice Device;
        private VertexBuffer Vertices;

        public NoiseMaker(GraphicsDevice device, VertexBuffer vertices, int gridSizeX, int gridSizeY)
        {
            Device = device;
            Vertices = vertices;
        }

        public void RenderGeometry()
        {
        }

        public void GenerateNormalMap()
        {
        }

        private void InitTextures()
        {
            PackedNoise = new Texture2D[2];
            PackedNoise[0] = new Texture2D(Device, npsize, npsize, 0, ResourceUsage.Dynamic, SurfaceFormat.Luminance16, 
                ResourcePool.Default);
            PackedNoise[1] = new Texture2D(Device, npsize, npsize, 0, ResourceUsage.Dynamic, SurfaceFormat.Luminance16,
                ResourcePool.Default);

            HeightMap = new Texture2D(Device, nmapsizex, nmapsizey, 1, ResourceUsage.RenderTarget, SurfaceFormat.Rgba64, 
                ResourcePool.Default);
            NormalMap = new Texture2D(Device, nmapsizex, nmapsizey, 1, ResourceUsage.AutoGenerateMipMap | 
                ResourceUsage.RenderTarget, SurfaceFormat.Rgba64, ResourcePool.Default);

            // Create the stencil buffer
            DepthStencil = Device.CreateDepthStencilSurface(nmapsizex, nmapsizey, DepthFormat.Depth24Stencil8,
                MultiSampleType.None, 0, true);
        }
    }
}
