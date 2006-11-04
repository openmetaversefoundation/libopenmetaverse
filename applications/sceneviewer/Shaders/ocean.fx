/*********************************************************************NVMH3****
Path:  NVSDK\Common\media\cgfx
File:  ocean.fx

Copyright NVIDIA Corporation 2003
TO THE MAXIMUM EXTENT PERMITTED BY APPLICABLE LAW, THIS SOFTWARE IS PROVIDED
*AS IS* AND NVIDIA AND ITS SUPPLIERS DISCLAIM ALL WARRANTIES, EITHER EXPRESS
OR IMPLIED, INCLUDING, BUT NOT LIMITED TO, IMPLIED WARRANTIES OF MERCHANTABILITY
AND FITNESS FOR A PARTICULAR PURPOSE.  IN NO EVENT SHALL NVIDIA OR ITS SUPPLIERS
BE LIABLE FOR ANY SPECIAL, INCIDENTAL, INDIRECT, OR CONSEQUENTIAL DAMAGES
WHATSOEVER (INCLUDING, WITHOUT LIMITATION, DAMAGES FOR LOSS OF BUSINESS PROFITS,
BUSINESS INTERRUPTION, LOSS OF BUSINESS INFORMATION, OR ANY OTHER PECUNIARY LOSS)
ARISING OUT OF THE USE OF OR INABILITY TO USE THIS SOFTWARE, EVEN IF NVIDIA HAS
BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGES.


Comments:
	Simple ocean shader with animated bump map and geometric waves
	Based partly on "Effective Water Simulation From Physical Models", GPU Gems

******************************************************************************/

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=PS20;";
> = 0.8;

float4x4 worldMatrix : World < string UIWidget = "none";>;	           	// World or Model matrix
float4x4 wvpMatrix : WorldViewProjection < string UIWidget = "none";>;	// Model*View*Projection
float4x4 worldViewMatrix : WorldView < string UIWidget = "none";>;
float4x4 viewInverseMatrix : ViewInverse < string UIWidget = "none";>;

float time : Time < string UIWidget = "none"; >;

texture normalMap : Normal
<
	string ResourceName = "waves2.dds";
	string ResourceType = "2D";
>;

texture cubeMap : Environment
<
	string ResourceName = "CloudyHillsCubemap2.dds";
	string ResourceType = "Cube";
>;

sampler2D normalMapSampler = sampler_state
{
	Texture = <normalMap>;
#if 0
	// this is a trick from Halo - use point sampling for sparkles
	MagFilter = Linear;	
	MinFilter = Point;
	MipFilter = None;
#else
	MagFilter = Linear;	
	MinFilter = Linear;
	MipFilter = Linear;
#endif
};

samplerCUBE envMapSampler = sampler_state
{
	Texture = <cubeMap>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

float bumpHeight
<
	string UIWidget = "slider";
	float UIMin = 0.0; float UIMax = 2.0; float UIStep = 0.01;
	string UIName = "Bump Height";
> = 0.1;

float2 textureScale
<
    string UIName = "Texture scale";
> = { 8.0, 4.0 };

float2 bumpSpeed
<
    string UIName = "Bumpmap translation speed";
> = { -0.05, 0.0 };

float fresnelBias
<
    string UIName = "Fresnel bias";
	string UIWidget = "slider";
	float UIMin = 0.0; float UIMax = 1.0; float UIStep = 0.01;
> = 0.1;

float fresnelPower
<
    string UIName = "Fresnel exponent";
	string UIWidget = "slider";
	float UIMin = 1.0; float UIMax = 10.0; float UIStep = 0.01;
> = 4.0;

float hdrMultiplier
<
    string UIName = "HDR multiplier";
	string UIWidget = "slider";
	float UIMin = 0.0; float UIMax = 100.0; float UIStep = 0.01;
> = 3.0;

float4 deepColor : Diffuse
<
    string UIName = "Deep water color";
> = {0.0f, 0.0f, 0.1f, 1.0f};

float4 shallowColor : Diffuse
<
    string UIName = "Shallow water color";
> = {0.0f, 0.5f, 0.5f, 1.0f};

float4 reflectionColor : Specular
<
    string UIName = "Reflection color";
> = {1.0f, 1.0f, 1.0f, 1.0f};

// these are redundant, but makes the ui easier:
float reflectionAmount
<
    string UIName = "Reflection amount";
	string UIWidget = "slider";    
	float UIMin = 0.0; float UIMax = 2.0; float UIStep = 0.01;    
> = 0.2f;

float waterAmount
<
    string UIName = "Water color amount";
	string UIWidget = "slider";    
	float UIMin = 0.0; float UIMax = 2.0; float UIStep = 0.01;    
> = 1.0f;

float waveAmp
<
    string UIName = "Wave amplitude";
	string UIWidget = "slider";
	float UIMin = 0.0; float UIMax = 10.0; float UIStep = 0.1;
> = 1.0;

float waveFreq
<
    string UIName = "Wave frequency";
	string UIWidget = "slider";
	float UIMin = 0.0; float UIMax = 1.0; float UIStep = 0.001;
> = 0.1;



struct a2v {
	float4 Position : POSITION;   // in object space
	float2 TexCoord : TEXCOORD0;
	float3 Tangent  : TEXCOORD1;
	float3 Binormal : TEXCOORD2;
	float3 Normal   : NORMAL;
};

struct v2f {
	float4 Position  : POSITION;  // in clip space
	float2 TexCoord  : TEXCOORD0;
	float3 TexCoord1 : TEXCOORD1; // first row of the 3x3 transform from tangent to cube space
	float3 TexCoord2 : TEXCOORD2; // second row of the 3x3 transform from tangent to cube space
	float3 TexCoord3 : TEXCOORD3; // third row of the 3x3 transform from tangent to cube space

	float2 bumpCoord0 : TEXCOORD4;
	float2 bumpCoord1 : TEXCOORD5;
	float2 bumpCoord2 : TEXCOORD6;
	
	float3 eyeVector  : TEXCOORD7;
};

// wave functions

#define NWAVES 2


v2f BumpReflectWaveVS(a2v IN,
					  uniform float4x4 WorldViewProj,
					  uniform float4x4 World,
					  uniform float4x4 ViewIT,
					  uniform float BumpScale,
					  uniform float2 textureScale,
					  uniform float2 bumpSpeed,
					  uniform float time,
					  uniform float waveFreq,
					  uniform float waveAmp
                	  )
{
	v2f OUT;

    //wave[0].freq = waveFreq;
    //wave[0].amp = waveAmp;
    float freq0 = waveFreq;
    float amp0 = waveAmp;
    float2 dir0 = float2(-1, 0);

    //wave[1].freq = waveFreq*2.0;
    //wave[1].amp = waveAmp*0.5;
    float freq1 = waveFreq * 2.0;
    float amp1 = waveAmp * 0.5;
    float2 dir1 = float2(-0.7, 0.7);

    float4 P = IN.Position;

	// sum waves	
	//P.z = 0.0;
	//float ddx = 0.0, ddy = 0.0;
	//for(int i=0; i<NWAVES; i++) {
    	//P.z += evaluateWave(wave[i], P.xy, time);
    	//float deriv = evaluateWaveDeriv(wave[i], P.xy, time);
    	//ddx += deriv * wave[i].dir.x;
    	//ddy += deriv * wave[i].dir.y;
    //}
    
    P.z = amp0 * sin(dot(dir0, P.xy) * freq0 + time * 0.5);
    float deriv = freq0 * amp0 * cos( dot(dir0, P.xy) * freq0 + time * 0.5);
    float ddx = deriv * -1;
    //ddy += deriv * 0;
    
    P.z += amp1 * sin(dot(dir1, P.xy) * freq1 + time *  1.3);
    deriv = freq1 * amp1 * cos( dot(dir1, P.xy) * freq1 + time * 1.3);
    ddx += deriv * -0.7;
    float ddy = deriv * 0.7;

	// compute tangent basis
    float3 B = float3(1, ddx, 0);
    float3 T = float3(0, ddy, 1);
    float3 N = float3(-ddx, 1, -ddy);

	OUT.Position = mul(P, WorldViewProj);
	
	// pass texture coordinates for fetching the normal map
	OUT.TexCoord.xy = IN.TexCoord*textureScale;

	time = fmod(time, 100.0);
	OUT.bumpCoord0.xy = IN.TexCoord*textureScale + time*bumpSpeed;
	OUT.bumpCoord1.xy = IN.TexCoord*textureScale*2.0 + time*bumpSpeed*4.0;
	OUT.bumpCoord2.xy = IN.TexCoord*textureScale*4.0 + time*bumpSpeed*8.0;

	// compute the 3x3 tranform from tangent space to object space
	float3x3 objToTangentSpace;
	// first rows are the tangent and binormal scaled by the bump scale
	objToTangentSpace[0] = BumpScale * normalize(T);
	objToTangentSpace[1] = BumpScale * normalize(B);
	objToTangentSpace[2] = normalize(N);

	OUT.TexCoord1.xyz = mul(objToTangentSpace, World[0].xyz);
	OUT.TexCoord2.xyz = mul(objToTangentSpace, World[1].xyz);
	OUT.TexCoord3.xyz = mul(objToTangentSpace, World[2].xyz);

	// compute the eye vector (going from shaded point to eye) in cube space
	float4 worldPos = mul(P, World);
	OUT.eyeVector = ViewIT[3] - worldPos; // view inv. transpose contains eye position in world space in last row
	return OUT;
}


float4 OceanPS20(v2f IN,
				 uniform sampler2D NormalMap,
				 uniform samplerCUBE EnvironmentMap,
				 uniform half4 deepColor,
				 uniform half4 shallowColor,
				 uniform half4 reflectionColor,
				 uniform half4 reflectionAmount,
				 uniform half4 waterAmount,
				 uniform half fresnelPower,
				 uniform half fresnelBias,
				 uniform half hdrMultiplier
				 ) : COLOR
{
	// sum normal maps
    half4 t0 = tex2D(NormalMap, IN.bumpCoord0.xy)*2.0-1.0;
    half4 t1 = tex2D(NormalMap, IN.bumpCoord1.xy)*2.0-1.0;
    half4 t2 = tex2D(NormalMap, IN.bumpCoord2.xy)*2.0-1.0;
    half3 N = t0.xyz + t1.xyz + t2.xyz;
//    half3 N = t1.xyz;

    half3x3 m; // tangent to world matrix
    m[0] = IN.TexCoord1;
    m[2] = IN.TexCoord2;
    m[1] = IN.TexCoord3;
    half3 Nw = mul(m, N.xyz);
    Nw = normalize(Nw);

	// reflection
    float3 E = normalize(IN.eyeVector);
    half3 R = reflect(-E, Nw);

    half4 reflection = texCUBE(EnvironmentMap, R);
    // hdr effect (multiplier in alpha channel)
    reflection.rgb *= (1.0 + reflection.a*hdrMultiplier);

	// fresnel - could use 1D tex lookup for this
    half facing = 1.0 - max(dot(E, Nw), 0);
    half fresnel = fresnelBias + (1.0-fresnelBias)*pow(facing, fresnelPower);

    half4 waterColor = lerp(deepColor, shallowColor, facing);

	return waterColor*waterAmount + reflection*reflectionColor*reflectionAmount*fresnel;
//	return waterColor;
//	return fresnel;
//	return reflection;
}

technique PS20 <
	string Script = "Pass=p0;";
> {
	pass p0 <
		string Script = "Draw=geometry;";
	> {
		VertexShader = compile vs_2_0 BumpReflectWaveVS(wvpMatrix, worldMatrix, viewInverseMatrix,
                                                        bumpHeight, textureScale, bumpSpeed, time,
                                                        waveFreq, waveAmp);
		
		Zenable = true;
		ZWriteEnable = true;
		CullMode = None;

		PixelShader = compile ps_2_0 OceanPS20(normalMapSampler, envMapSampler,
                                               deepColor, shallowColor, reflectionColor, reflectionAmount, waterAmount,
                                               fresnelPower, fresnelBias, hdrMultiplier);
	}
}
