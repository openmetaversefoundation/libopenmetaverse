using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace sceneviewer
{
    class NoiseMaker
    {
        // Constants
        public const int nbits = 5;
        public const int nsize = 1 << (nbits - 1);
        public const int nsizem1 = nsize - 1;
        public const int nsizesq = nsize * nsize;
        public const int nsizesqm1 = nsizesq - 1;

        public const int npacksize = 4;

        public const int npbits = nbits + npacksize - 1;
        public const int npsize = 1 << (npbits - 1);
        public const int npsizem1 = npsize - 1;
        public const int npsizesq = npsize * npsize;
        public const int npsizesqm1 = npsizesq - 1;

        public const int ndecbits = 12;
        public const int ndecmagn = 4096;
        public const int ndecmagnm1 = 4095;

        public const int maxoctaves = 8;

        public const int noiseframes = 256;
        public const int noiseframesm1 = noiseframes - 1;

        public const int noisedecbits = 15;
        public const int noisemagnitude = 1 << (noisedecbits - 1);

        public const int scaledecbits = 15;
        public const int scalemagnitude = 1 << (scaledecbits - 1);

        public const int nmapsizex = 512;
        public const int nmapsizey = 1024;

        public const float FALLOFF = 0.607f;
        public const float ANIMSPEED = 1.4f;
        public const float TIMEMULTI = 1.27f;
        public const float ELEVATION = 7.0f;
        public const float SCALE = 0.38f;
        public const bool SMOOTH = false;
        public const bool DISPLACE = false;

        // Public members
        public VertexPositionNormalTexture[] Vertices;
        public Texture2D HeightMap;
        public Texture2D NormalMap;
        public Texture2D[] PackedNoise;
        public Surface DepthStencil;

        private GraphicsDevice Device;
        private int Octaves;
        private float[] Multitable;
        private int then;
        private double time;
        private int[] Onoise;
        private int[] Pnoise;
        private Vector3 eu, ev;
        private Vector4 tcorners0, tcorners1, tcorners2, tcorners3;
        private int sizeX, sizeY; // framebuffer size
        private int fsizeX, fsizeY;

        public NoiseMaker(GraphicsDevice device, int gridSizeX, int gridSizeY)
        {
            Device = device;
            Multitable = new float[maxoctaves];
            Onoise = new int[nsizesq * maxoctaves];
            Pnoise = new int[npsizesq * (maxoctaves >> (npacksize - 1))];

            sizeX = gridSizeX;
            sizeY = gridSizeY;
            time = 0.0d;
            then = Environment.TickCount;
            Octaves = 0; // Don't want to have the noise accessed before it's calculated
            fsizeX = sizeX;
            fsizeY = sizeY;

            // Reset normals
            Vertices = new VertexPositionNormalTexture[sizeX * sizeY];
            for (int v = 0; v < sizeY; v++)
            {
                for (int u = 0; u < sizeX; u++)
                {
                    // FIXME: Should this be UnitZ?
                    Vertices[v * sizeX + u].Normal = Vector3.UnitY;
                    Vertices[v * sizeX + u].TextureCoordinate = new Vector2((float)u / (sizeX - 1), (float)v / (sizeY - 1));
                }
            }

            InitNoise();
            LoadEffects();
            InitTextures();
        }

        public bool RenderGeometry(Matrix m)
        {
            CalculateNoise();

	        float magnitude = ndecmagn * SCALE;
	        float invmagnitudesq = 1.0f / (SCALE * SCALE);

	        Matrix minv = Matrix.Invert(m);
            eu = Vector3.Normalize(Vector3.TransformNormal(Vector3.UnitX, m));
            ev = Vector3.Normalize(Vector3.TransformNormal(Vector3.UnitY, m));

	        tcorners0 = CalculateWorldPos(Vector2.Zero, m);
	        tcorners1 = CalculateWorldPos(Vector2.UnitX, m);
	        tcorners2 = CalculateWorldPos(Vector2.UnitY, m);
	        tcorners3 = CalculateWorldPos(Vector2.One, m);

	        Matrix surfacetoworld;

	        float du = 1.0f / (float)(sizeX - 1);
		    float dv = 1.0f / (float)(sizeY - 1);
		    float u = 0;
            float v = 0;
	        Vector4 result;
	        int i = 0;

	        for(int iv = 0; iv < sizeY; iv++)
	        {
		        u = 0;

		        for(int iu = 0; iu < sizeX; iu++)
		        {				
			        result.X = (1 - v) * ( (1 - u) * tcorners0.X + u * tcorners1.X ) + 
                        v * ((1 - u) * tcorners2.X + u * tcorners3.X);
			        result.Z = (1 - v) * ( (1 - u) * tcorners0.Z + u * tcorners1.Z ) + 
                        v * ((1 - u) * tcorners2.Z + u * tcorners3.Z);
			        result.W = (1 - v) * ( (1 - u) * tcorners0.W + u * tcorners1.W ) + 
                        v * ((1 - u) * tcorners2.W + u * tcorners3.W);

			        float divide = 1 / result.W;				
			        result.X *= divide;
			        result.Z *= divide;

                    // FIXME: The GetHeightDual should return a Z value instead?
                    Vertices[i].Position = new Vector3(result.X,
                        GetHeightDual((int)(magnitude * result.X), (int)(magnitude * result.Z)), result.Z);

			        i++;
			        u += du;
		        }

		        v += dv;			
	        }

	        // smooth the heightdata
	        if(SMOOTH)
	        {
		        for(int v1 = 1; v1 < (sizeY - 1); v1++)
		        {
			        for(int u1 = 1; u1 < (sizeX - 1); u1++)
			        {
		                // FIXME: We should be smoothing the Z values instead
                        Vector3 v1sizeXu1 = Vertices[v1 * sizeX + u1].Position;
                        v1sizeXu1.Y = 0.2f * (Vertices[v1 * sizeX + u1].Position.Y +
					        Vertices[v1 * sizeX + (u1 + 1)].Position.Y +
                            Vertices[v1 * sizeX + (u1 - 1)].Position.Y +
                            Vertices[(v1 + 1) * sizeX + u1].Position.Y +
                            Vertices[(v1 - 1) * sizeX + u1].Position.Y);
                        Vertices[v1 * sizeX + u1].Position = v1sizeXu1;
			        }
		        }
	        }

	        if(!DISPLACE)
	        {
		        // Reset height to 0
		        for(int u1 = 0; u1 < (sizeX * sizeY); u1++)
		        {
                    // FIXME: Should be Z?
			        Vertices[u1].Position = new Vector3(Vertices[u1].Position.X, 0, Vertices[u1].Position.Z);
		        }
	        }

            UploadNoise();

	        return true;
        }

        public void GenerateNormalMap()
        {
	        // Do the heightmap thingy
            Surface bb = Device.GetRenderTarget(0);
            Surface target = HeightMap.GetSurfaceLevel(0);
            Surface olddepthstencil = Device.DepthStencilSurface;
        	
	        //hr = device->SetRenderState( D3DRS_CULLMODE, D3DCULL_CCW  );	
	        //device->SetStreamSource( 0, surf_software_vertices, 0, sizeof(SOFTWARESURFACEVERTEX) );
	        //hr = device->SetFVF( D3DFVF_SOFTWARESURFACEVERTEX);			
	        //device->SetIndices(surf->surf_indicies);
	        hr = hmap_effect->Begin(NULL,NULL);
	        hmap_effect->BeginPass(0);
        	
	        hmap_effect->SetTexture("noise0",packed_noise_texture[0]);
	        hmap_effect->SetTexture("noise1",packed_noise_texture[1]);
        	
	        hr = device->SetRenderTarget( 0, target );
	        device->SetDepthStencilSurface( depthstencil );
	        //device->Clear( 0, NULL,D3DCLEAR_TARGET, D3DCOLOR_XRGB(255,128,28), 1.0f, 0 );
	        device->SetRenderState( D3DRS_ZENABLE, D3DZB_FALSE );
	        device->DrawIndexedPrimitive(	D3DPT_TRIANGLELIST, 0,	0, sizeX*sizeY, 0, 2*(sizeX-1)*(sizeY-1) );			
	        hmap_effect->EndPass();
	        hmap_effect->End();
        	
	        // calculate normalmap

	        hr = normalmap->GetSurfaceLevel( 0,&target );
	        hr = device->SetRenderTarget( 0, target );
	        hr = nmap_effect->Begin(NULL,NULL);
	        nmap_effect->BeginPass(0);				
	        nmap_effect->SetFloat("inv_mapsize_x", 1.0f/nmapsize_x);
	        nmap_effect->SetFloat("inv_mapsize_y", 1.0f/nmapsize_y);
	        nmap_effect->SetVector("corner00", &t_corners0 );
	        nmap_effect->SetVector("corner01", &t_corners1 );
	        nmap_effect->SetVector("corner10", &t_corners2 );
	        nmap_effect->SetVector("corner11", &t_corners3 );
	        nmap_effect->SetFloat("amplitude", 2 * STRENGTH);
	        nmap_effect->SetTexture("hmap",heightmap);
	        device->DrawIndexedPrimitive(	D3DPT_TRIANGLELIST, 0,	0, sizeX*sizeY, 0, 2*(sizeX-1)*(sizeY-1) );			
	        nmap_effect->EndPass();
	        nmap_effect->End();

	        // restore the device
	        device->SetRenderState( D3DRS_ZENABLE, D3DZB_TRUE );
	        device->SetRenderTarget( 0, bb );
	        device->SetDepthStencilSurface( old_depthstencil );
        }

        private void InitNoise()
        {
            Random rand = new Random();

            // create noise (uniform)
	        float[] tempnoise = new float[nsizesq * noiseframes];
	        for(int i = 0; i < (nsizesq * noiseframes); i++)
	        {	
		        float temp = (float)rand.NextDouble(RANDMAX);		
		        tempnoise[i] = 4 * (temp - 0.5f);	
	        }	

	        for(int frame = 0; frame < noiseframes; frame++)
	        {
		        for(int v = 0; v < nsize; v++)
		        {
			        for(int u = 0; u < nsize; u++)
			        {	
				        int v0 = ((v - 1) & nsizem1) * nsize,
					        v1 = v * nsize,
					        v2 = ((v + 1) & nsizem1) * nsize,
					        u0 = ((u - 1) & nsizem1),
					        u1 = u,
					        u2 = ((u + 1) & nsizem1),					
					        f  = frame * nsizesq;
				        float temp = (1.0f / 14.0f) * (tempnoise[f + v0 + u0] + tempnoise[f + v0 + u1] + tempnoise[f + v0 + u2] +
						    tempnoise[f + v1 + u0] + 6.0f * tempnoise[f + v1 + u1] + tempnoise[f + v1 + u2] +
							tempnoise[f + v2 + u0] + tempnoise[f + v2 + u1] + tempnoise[f + v2 + u2]);
        					
				        noise[frame * nsizesq + v * nsize + u] = noisemagnitude * temp;
			        }
		        }
	        }
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

        private void LoadEffects()
        {
            char* errortext;
            LPD3DXBUFFER errors;
            D3DXHANDLE hTechnique;

            // load effect
            D3DXCreateEffectFromFile(device, "v2_heightmapgen.fx",
                NULL, NULL, 0, NULL, &hmap_effect, &errors);

            if (errors != NULL)
            {
                errortext = (char*)errors->GetBufferPointer();
                MessageBox(NULL, errortext, "hmap_effect", MB_OK);
            }

            hmap_effect->FindNextValidTechnique(NULL, &hTechnique);
            hmap_effect->SetTechnique(hTechnique);

            // load effect
            D3DXCreateEffectFromFile(device, "v2_normalmapgen.fx",
                NULL, NULL, 0, NULL, &nmap_effect, &errors);

            if (errors != NULL)
            {
                errortext = (char*)errors->GetBufferPointer();
                MessageBox(NULL, errortext, "nmap_effect", MB_OK);
            }

            nmap_effect->FindNextValidTechnique(NULL, &hTechnique);
            nmap_effect->SetTechnique(hTechnique);
        }

        // FIXME: Should this be (0,0,1,0) ?
        // Check the point of intersection with the plane (0,1,0,0) and return the position in homogenous coordinates
        private Vector4 CalculateWorldPos(Vector2 uv, Matrix m)
        {
            Vector4 origin = new Vector4(uv.X, uv.Y, -1, 1);
            Vector4 direction = new Vector4(uv.X, uv.Y, 1, 1);

            origin = Vector4.Transform(origin, m);
            direction = Vector4.Transform(direction, m);

            float l = -origin.Y / direction.Y;

            Vector4 worldpos = origin + direction * l;
            return worldpos;
        }

        private void CalculateNoise()
        {
            Octaves = maxoctaves;

            // Calculate the strength of each octave
            float sum = 0;

            for (int i = 0; i < Octaves; i++)
            {
                Multitable[i] = (float)Math.Pow(FALLOFF, 1 * i);
                sum += Multitable[i];
            }

            for (int i = 0; i < Octaves; i++)
            {
                Multitable[i] /= sum;
            }

            for (int i = 0; i < Octaves; i++)
            {
                Multitable[i] = scalemagnitude * Multitable[i];
            }

            int now = Environment.TickCount;
            double timechange = (double)(now - then);
	        then = now;
            timechange *= 0.001d * ANIMSPEED;
            double lptimechange = 0.99d * lptimechange + 0.01d * timechange;
            time += lptimechange;

            double rtimemulti = 1;

	        for(int o = 0; o < Octaves; o++)
	        {
		        uint[] image = new uint[3];
		        int[] amount = new int[3];
                int iimage = (int)(time * rtimemulti);
                double fraction = (time * rtimemulti) - (double)iimage;

                amount[0] = scalemagnitude * Multitable[o] * (Math.Pow(Math.Sin((fraction + 2) * Math.PI / 3), 2) / 1.5d);
                amount[1] = scalemagnitude * Multitable[o] * (Math.Pow(Math.Sin((fraction + 1) * Math.PI / 3), 2) / 1.5d);
                amount[2] = scalemagnitude * Multitable[o] * (Math.Pow(Math.Sin((fraction) * Math.PI / 3), 2) / 1.5d);
                image[0] = (iimage) & noiseframesm1;
                image[1] = (iimage + 1) & noiseframesm1;
                image[2] = (iimage + 2) & noiseframesm1;
		        {
			        for(int i = 0; i < nsizesq; i++)
			        {
				        o_noise[i + nsizesq*o] =	(((amount[0] * noise[i + nsizesq * image[0]])>>scaledecimalbits) + 
												     ((amount[1] * noise[i + nsizesq * image[1]])>>scaledecimalbits) + 
												     ((amount[2] * noise[i + nsizesq * image[2]])>>scaledecimalbits));
			        }
		        }

		        rtimemulti *= TIMEMULTI;
	        }

	        if (packednoise)
	        {
		        int octavepack = 0;

                for (int o = 0; o < Octaves; o += npacksize)
		        {
			        for (int v = 0; v < npsize; v++)
			        for (int u = 0; u < npsize; u++)
			        {
				        Pnoise[v*npsize+u+octavepack*npsize_sq] = o_noise[(o+3)*nsizesq + (v&nsizem1)*nsize + (u&nsizem1)];
				        Pnoise[v*npsize+u+octavepack*npsize_sq] += mapsample( u, v, 3, o);
				        Pnoise[v*npsize+u+octavepack*npsize_sq] += mapsample( u, v, 2, o+1);
				        Pnoise[v*npsize+u+octavepack*npsize_sq] += mapsample( u, v, 1, o+2);				
			        }

			        octavepack++;
		        }
	        }
        }

        //private void CalculateNormals()
        //{
        //    ;
        //}

        private void UploadNoise()
        {
            D3DLOCKED_RECT locked;
	        ushort data;
	        int[] tempdata = new int[npsizesq];

	        for(int t = 0; t < 2; t++)
	        {
		        int offset = npsizesq * t;
		        // upload the first level
		        packed_noise_texture[t]->LockRect( 0, &locked, NULL, D3DLOCK_DISCARD );
		        data = (ushort*)locked.pBits;
		        for(int i=0; i<npsize_sq; i++)
			        data[i] = 32768+Pnoise[i+offset];
		        packed_noise_texture[t]->UnlockRect( 0 );

		        int c = packed_noise_texture[t]->GetLevelCount();

		        // calculate the second level, and upload it
		        HRESULT hr = packed_noise_texture[t]->LockRect( 1, &locked, NULL, 0 );
		        data = (ushort*)locked.pBits;		
		        int sz = npsize>>1;
		        for(int v=0; v<sz; v++){
			        for(int u=0; u<sz; u++)
			        {				
				        tempdata[v*npsize + u] = (Pnoise[((v<<1))*npsize + (u<<1)+offset] + Pnoise[((v<<1))*npsize + (u<<1) + 1+offset] +
										           Pnoise[((v<<1)+1)*npsize + (u<<1)+offset] + Pnoise[((v<<1)+1)*npsize + (u<<1) + 1+offset])>>2;
				        data[v*sz+u] = 32768+tempdata[v*npsize + u];
			        }
		        }

		        packed_noise_texture[t]->UnlockRect( 1 );		
        		
		        for(int j=2; j<c; j++)
		        {
			        hr = packed_noise_texture[t]->LockRect( j, &locked, NULL, 0 );
			        data = (ushort*)locked.pBits;
			        int pitch = (locked.Pitch)>>1;
			        sz = npsize>>j;			
			        for(int v=0; v<sz; v++){
				        for(int u=0; u<sz; u++)
				        {
					        tempdata[v*npsize + u] =	(tempdata[((v<<1))*npsize + (u<<1)] + tempdata[((v<<1))*npsize + (u<<1) + 1] +
												        tempdata[((v<<1)+1)*npsize + (u<<1)] + tempdata[((v<<1)+1)*npsize + (u<<1) + 1])>>2;
					        data[v*pitch+u] = 32768+tempdata[v*npsize + u];
				        }
			        }		
			        packed_noise_texture[t]->UnlockRect( j );
		        }
	        }
        }

        private int MapSample(int u, int v, int upsamplepower, int octave)
        {
            ;
        }

        private float ReadtexelNearest(int u, int v)
        {
	        int lu, lv;
	        lu = (u>>ndecbits)&nsizem1;
	        lv = (v>>ndecbits)&nsizem1;	

	        return (float)noise[lv*nsize + lu];
        }

        private int ReadtexelLinearDual(int u, int v, int o)
        {
	        int iu, iup, iv, ivp, fu, fv;
	        iu = (u>>ndecbits)&npsizem1;
	        iv = ((v>>ndecbits)&npsizem1)*npsize;
        	
	        iup = ((u>>ndecbits) + 1)&npsizem1;
	        ivp = (((v>>ndecbits) + 1)&npsizem1)*npsize;
        	
	        fu = u & ndecmagn_m1;
	        fv = v & ndecmagn_m1;
        		
	        int ut01 = ((ndecmagn-fu)*Rnoise[iv + iu] + fu*Rnoise[iv + iup])>>ndecbits;
	        int ut23 = ((ndecmagn-fu)*Rnoise[ivp + iu] + fu*Rnoise[ivp + iup])>>ndecbits;
	        int ut = ((ndecmagn-fv)*ut01 + fv*ut23) >> ndecbits;

	        return ut;
        }

        // FIXME: This function uses pointer magic that needs to be fixed for C#
        private float GetHeightDual(int u, int v)
        {
            int value = 0;
	        Rnoise = Pnoise; // pointer to the current noise source octave
	        int hoct = Octaves / npacksize;

	        for(int i = 0; i < hoct; i++)
	        {
		        value += ReadtexelLinearDual(u, v, 0);
		        u = u << npacksize;
		        v = v << npacksize;
		        Rnoise += npsizesq;
	        }

	        return value * STRENGTH / noisemagnitude;
        }
    }
}
