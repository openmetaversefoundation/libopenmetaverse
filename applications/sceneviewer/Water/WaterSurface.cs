using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace sceneviewer.Water
{
    class WaterSurface
    {
        public const int REFLREFRDETAIL = 512;
        public const float SUNSHININESS = 84.0f;
        public const float SUNSTRENGTH = 12.0f;
        public const float REFLREFROFFSET = 0.1f;

        public bool Initialized;
        public Color WaterColor;

        GraphicsDevice Device;
        Plane Plane, UpperBound, LowerBound;
        Vector3 Normal, U, V;
        float Position;
        int GridSizeX, GridSizeY;
        NoiseMaker NoiseMaker;
        Texture2D Fresnel, XYNoise, Reflection, Refraction;
        Surface DepthStencil;
        Effect WaterEffect;
        VertexBuffer Vertices;
        IndexBuffer Indices;
        Camera Camera;
        bool PlaneWithinFrustum;

        public WaterSurface(GraphicsDevice device, Camera camera, Vector3 normal, float position, int sizeX, int sizeY)
        {
            Vector3 x;

            Device = device;
            Camera = camera;
            Normal = normal;
            Position = position;
            Plane = new Plane(normal, position);
            NoiseMaker = new NoiseMaker(Device, Vertices, GridSizeX, GridSizeY);
            PlaneWithinFrustum = false;

            // Set the initial water color
            WaterColor = Color.Aquamarine;

            // Calculate the U and V vectors
            if (Math.Abs(Vector3.Dot(Vector3.UnitX, normal)) < Math.Abs(Vector3.Dot(Vector3.UnitY, normal)))
            {
                x = Vector3.UnitX;
            }
            else
            {
                x = Vector3.UnitY;
            }

            U = x - normal * Vector3.Dot(normal, x);
            U = Vector3.Normalize(U);

            // Get V (cross)
            V = Vector3.Cross(U, normal);

            GridSizeX = sizeX + 1;
            GridSizeY = sizeY + 1;

            SetDisplacementAmplitude(0);

            if (!InitializeBuffers())
            {
                return;
            }

            // Load the textures
            if ((Fresnel = Texture2D.FromFile(Device, "textures/fresnel_water_linear.bmp")) == null)
            {
                return;
            }
            if ((XYNoise = Texture2D.FromFile(Device, "textures/xynoise.png")) == null)
            {
                return;
            }

            // Initialize the reflection and refraction textures, and the depth stencil
            Reflection = new Texture2D(Device, REFLREFRDETAIL, REFLREFRDETAIL, 1, ResourceUsage.RenderTarget,
                SurfaceFormat.Color, ResourcePool.Default);
            Refraction = new Texture2D(Device, REFLREFRDETAIL, REFLREFRDETAIL, 1, ResourceUsage.RenderTarget,
                SurfaceFormat.Color, ResourcePool.Default);
            DepthStencil = Device.CreateDepthStencilSurface(REFLREFRDETAIL, REFLREFRDETAIL, DepthFormat.Depth24Stencil8,
                MultiSampleType.None, 0, true);

            // Load the effect
            CompiledEffect water = Effect.CompileEffectFromFile("shaders/watereffect.fx", null, null,
                CompilerOptions.Debug | CompilerOptions.SkipOptimization, TargetPlatform.Windows);
            if (!water.Success)
            {
                return;
            }
            else
            {
                WaterEffect = new Effect(Device, water.GetShaderCode(), CompilerOptions.None, null);
            }

            Initialized = true;
        }

        public bool Prepare()
        {
            if (!Initialized) return false;

            // Check if the water plane is within the viewing frustum
            BoundingFrustum frustum = new BoundingFrustum(Camera.ViewProjectionMatrix);

            if ((frustum.Intersects(UpperBound) == PlaneIntersectionType.Intersecting) || 
                (frustum.Intersects(LowerBound) == PlaneIntersectionType.Intersecting))
            {
                PlaneWithinFrustum = true;

                NoiseMaker.RenderGeometry();
            }

            return true;
        }

        public void RenderCutter()
        {
            if (PlaneWithinFrustum)
            {
                Device.VertexDeclaration = new VertexDeclaration(Device, VertexPositionNormalTexture.VertexElements);
                Device.Vertices[0].SetSource(Vertices, 0, VertexPositionNormalTexture.SizeInBytes);
                Device.Indices = Indices;
                Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, GridSizeX * GridSizeY,
                    0, 2 * (GridSizeX - 1) * (GridSizeY - 1));
            }
        }

        public void Render(Vector4 sunVector)
        {
            if (PlaneWithinFrustum)
            {
                Device.RenderState.CullMode = CullMode.None;
                Device.VertexDeclaration = new VertexDeclaration(Device, VertexPositionNormalTexture.VertexElements);
                Device.Vertices[0].SetSource(Vertices, 0, VertexPositionNormalTexture.SizeInBytes);
                Device.Indices = Indices;

                NoiseMaker.GenerateNormalMap();

                Device.RenderState.CullMode = CullMode.CullClockwiseFace;
                WaterEffect.Begin(EffectStateOptions.Default);
                WaterEffect.CurrentTechnique.Passes[0].Begin();
                // Beginning of the water rendering pass

                WaterEffect.Parameters["mViewProj"].SetValue(Camera.ViewProjectionMatrix);
                WaterEffect.Parameters["mView"].SetValue(Camera.ViewMatrix);
                WaterEffect.Parameters["sun_vec"].SetValue(sunVector);
                WaterEffect.Parameters["sun_shininess"].SetValue(SUNSHININESS);
                WaterEffect.Parameters["sun_strength"].SetValue(SUNSTRENGTH);
                WaterEffect.Parameters["reflrefr_offset"].SetValue(REFLREFROFFSET);
                WaterEffect.Parameters["diffuseSkyRef"].SetValue(true);
                WaterEffect.Parameters["watercolour"].SetValue(WaterColor.ToVector4());
                WaterEffect.Parameters["LODbias"].SetValue(0.0f);
                WaterEffect.Parameters["view_position"].SetValue(new Vector4(Camera.Position, 1));
                //WaterEffect.Parameters["EnvironmentMap", SkyboxCubemap);
                WaterEffect.Parameters["FresnelMap"].SetValue(Fresnel);
                WaterEffect.Parameters["Heightmap"].SetValue(NoiseMaker.HeightMap);
                WaterEffect.Parameters["Normalmap"].SetValue(NoiseMaker.NormalMap);
                WaterEffect.Parameters["Refractionmap"].SetValue(Refraction);
                WaterEffect.Parameters["Reflectionmap"].SetValue(Reflection);

                Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, GridSizeX * GridSizeY, 0,
                    2 * (GridSizeX - 1) * (GridSizeY - 1));

                // End of the water rendering pass
                WaterEffect.CurrentTechnique.Passes[0].End();
                WaterEffect.End();
            }
        }

        public void SetDisplacementAmplitude(float amplitude)
        {
            UpperBound = new Plane(Normal, Position + amplitude);
            LowerBound = new Plane(Normal, Position - amplitude);
        }

        private bool InitializeBuffers()
        {
            if ((Vertices = new VertexBuffer(Device, GridSizeX * GridSizeY * VertexPositionNormalTexture.SizeInBytes,
                ResourceUsage.WriteOnly | ResourceUsage.Dynamic, ResourcePool.Default)) == null)
            {
                return false;
            }

            if ((Indices = new IndexBuffer(Device, sizeof(int) * 6 * (GridSizeX - 1) * (GridSizeY - 1),
                ResourceUsage.WriteOnly, ResourcePool.Default, IndexElementSize.ThirtyTwoBits)) == null)
            {
                return false;
            }

            // Fill the index buffer
            for (int v = 0; v < GridSizeY - 1; v++)
            {
                for (int u = 0; u < GridSizeX - 1; u++)
                {
                    int[] indexes = new int[6];

                    // Face 1 |/
                    indexes[0] = v * GridSizeX + u;
                    indexes[1] = v * GridSizeX + u + 1;
                    indexes[2] = (v + 1) * GridSizeX + u;

                    // Face 2 /|
                    indexes[3] = (v + 1) * GridSizeX + u;
                    indexes[4] = v * GridSizeX + u + 1;
                    indexes[5] = (v + 1) * GridSizeX + u + 1;

                    Indices.SetData<int>(indexes);
                }
            }

            return true;
        }

        /*private bool GetMinMax(out Matrix range)
        {
            BoundingFrustum frustum = new BoundingFrustum(Camera.ViewMatrix);
            Vector3[] corners = frustum.GetCorners();
            range = Matrix.CreateOrthographic(frustum.

            float xmin, ymin, xmax, ymax;
            // Frustum to check the camera against
            //Vector3[] frustum = new Vector3[8];
            Vector3[] projPoints = new Vector3[24];
            int npoints = 0;
            // Which frustum points are connected together?
            //int[] cube = {	0,1, 0,2, 2,3, 1,3,
            //                0,4, 2,6, 3,7, 1,5,
            //                4,6, 4,5, 5,7, 6,7
            //             };

            // Transform frustum points to worldspace
            //Matrix invView = Matrix.Invert(Camera.ViewMatrix);
            //frustum[0] = Vector3.Transform(new Vector3(-1, -1, -1), invView);
            //frustum[1] = Vector3.Transform(new Vector3(+1, -1, -1), invView);
            //frustum[2] = Vector3.Transform(new Vector3(-1, +1, -1), invView);
            //frustum[3] = Vector3.Transform(new Vector3(+1, +1, -1), invView);
            //frustum[4] = Vector3.Transform(new Vector3(-1, -1, +1), invView);
            //frustum[5] = Vector3.Transform(new Vector3(+1, -1, +1), invView);
            //frustum[6] = Vector3.Transform(new Vector3(-1, +1, +1), invView);
            //frustum[7] = Vector3.Transform(new Vector3(+1, +1, +1), invView);

            // Check intersections with UpperBound and LowerBound
            for (int i = 0; i < 12; i++)
            {
                int src = cube[i * 2];
                int dst = cube[i * 2 + 1];

                // FIXME: Since Z is up in our world we may need to fix some things here
                if ((UpperBound.A * frustum[src].X + UpperBound.B * frustum[src].Y + UpperBound.C * frustum[src].Z + UpperBound.D * 1) /
                    (UpperBound.A * frustum[dst].X + UpperBound.B * frustum[dst].Y + UpperBound.C * frustum[dst].Z + UpperBound.D * 1) < 0)
                {
                    BoundingFrustum f = new BoundingFrustum(
                    //projPoints[npoints++] = 
                    Plane.Intersects(
                    D3DXPlaneIntersectLine(&proj_points[n_points++], &upper_bound, &frustum[src], &frustum[dst]);
                }
                if ((LowerBound.A * frustum[src].X + LowerBound.B * frustum[src].Y + LowerBound.C * frustum[src].Z + LowerBound.D * 1) /
                    (LowerBound.A * frustum[dst].X + LowerBound.B * frustum[dst].Y + LowerBound.C * frustum[dst].Z + LowerBound.D * 1) < 0)
                {
                    D3DXPlaneIntersectLine(&proj_points[n_points++], &lower_bound, &frustum[src], &frustum[dst]);
                }
            }

            // Check if any of the frustums vertices lie between the upper_bound and lower_bound planes
            for (int i = 0; i < 8; i++)
            {
                if ((UpperBound.A * frustum[i].X + UpperBound.B * frustum[i].Y + UpperBound.C * frustum[i].Z + UpperBound.D * 1) /
                    (LowerBound.A * frustum[i].X + LowerBound.B * frustum[i].Y + LowerBound.C * frustum[i].Z + LowerBound.D * 1) < 0)
                {
                    projPoints[npoints++] = frustum[i];
                }
            }

            // TODO: Advanced camera stuff?
            //

            for (int i = 0; i < npoints; i++)
            {
                projPoints[i] = Vector3.Transform(projPoints[i], Camera.ViewMatrix);
                projPoints[i] = Vector3.Transform(projPoints[i], Camera.ProjectionMatrix);
            }

            // Get max/min x & y-values to determine how big the "projection window" must be
            if (npoints > 0)
            {
		        xmin = projPoints[0].X;
		        xmax = projPoints[0].X;
		        ymin = projPoints[0].Y;
		        ymax = projPoints[0].Y;

		        for(int i = 1; i < npoints; i++)
                {
                    if (projPoints[i].X > xmax) xmax = projPoints[i].X;
                    if (projPoints[i].X < xmin) xmin = projPoints[i].X;
                    if (projPoints[i].Y > ymax) ymax = projPoints[i].Y;
                    if (projPoints[i].Y < ymin) ymin = projPoints[i].Y;
		        }
        		
		        // Build the packing matrix that spreads the grid across the "projection window"
                Matrix pack = new Matrix(xmax-xmin,     0,          0,      xmin,
                                         0,             ymax-ymin,  0,      ymin,
                                         0,             0,          1,      0,
                                         0,             0,          0,      1);

                pack = Matrix.Transpose(pack);
                range = pack * invView;

		        return true;
	        }
            else
            {
	            return false;
            }
        }*/

        private bool WithinFrustum(Vector3 position)
        {
            Vector3 test = Vector3.Transform(position, Camera.ViewProjectionMatrix);

            if ((Math.Abs(test.X) < 1.00001f) && (Math.Abs(test.Y) < 1.00001f) && (Math.Abs(test.Z) < 1.00001f))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
