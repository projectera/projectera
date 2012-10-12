extern float4 Tone;
sampler TextureSampler : register(s0);

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 TexPos : TEXCOORD0;

    // TODO: add input channels such as texture
    // coordinates and vertex colors here.
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 TexPos : TEXCOORD0;

    // TODO: add vertex shader outputs such as colors and texture
    // coordinates here. These values will automatically be interpolated
    // over the triangle, and provided as input to your pixel shader.
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    return input;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	// Get color
	float4 color = tex2D(TextureSampler, input.TexPos);
	
	// Get lumiance
	float lumi = (color.r * 0.3 + color.g * 0.59 + color.b * 0.11);
	
	// Combine two values by opacity (grayness) strength
	color.rgb = (color.rgb * (1-Tone.a) + lumi * Tone.a);

	return color;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
