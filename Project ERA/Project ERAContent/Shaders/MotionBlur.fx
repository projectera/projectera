#define NUMSAMPLES 8

extern Texture2D screen;
extern float2 speed;
extern float rotation;

sampler2D screenSampler = sampler_state { texture = <screen> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = clamp; AddressV = clamp;};

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 TexPos : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 TexPos : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    return input;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 velocity = speed + mul(float2x2(0,1,-1,0), input.TexPos - float2(0.5,0.5)) * rotation;
	float2 texCoord = input.TexPos;

	float4 color = 0;
	for(int i = 0; i < NUMSAMPLES; ++i, texCoord += velocity)
	{
		color += tex2D(screenSampler, texCoord);
	}

	return color / NUMSAMPLES;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
