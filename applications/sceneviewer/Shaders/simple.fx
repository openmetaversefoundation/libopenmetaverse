//
// Intro to HLSL
//

texture tex : Texture
<
	string ResourceName = "env3.bmp";
	string TextureType = "2D";
>;

// The world view and projection matrices
float4x4 WorldViewProj : WORLDVIEWPROJECTION;

sampler TextureSampler = sampler_state
{
	Texture = <tex>;
	MagFilter = Linear;	
	MinFilter = Linear;
	MipFilter = Linear;
};

// Transform our coordinates into world space
void Transform(
    in float4 inputPosition : POSITION,
    in float2 inputTexCoord : TEXCOORD0,
    out float4 outputPosition : POSITION,
    out float2 outputTexCoord : TEXCOORD0
    )
{
    // Transform our position
    outputPosition = mul(inputPosition, WorldViewProj);
    // Set our texture coordinates
    outputTexCoord = inputTexCoord;
}

void TextureColor(
 in float2 textureCoords : TEXCOORD0,
 out float4 diffuseColor : COLOR0)
{
    // Get the texture color
    diffuseColor = tex2D(TextureSampler, textureCoords);
    //diffuseColor = float4(0, 0, 1, 0.5); // Blue
};

technique TransformTexture
{
    pass P0
    {
        // shaders
        VertexShader = compile vs_1_1 Transform();
        PixelShader  = compile ps_1_1 TextureColor();
    }
}
