static const float PI	= 3.14159265f;
static const float PI2  = 3.14159265f * 2;

extern float4x4 World;
extern float4x4 View;
extern float4x4 Projection;
extern float Percentage;

uniform extern texture ScreenTexture;    

sampler ScreenSampler = sampler_state
{
	Texture = <ScreenTexture>;    
};

void SpriteVertexShader(inout float4 color    : COLOR0,
						inout float2 texCoord : TEXCOORD0,
						inout float4 position : POSITION0)
{
	float4 worldPosition = mul(position, World);
	float4x4 viewProjection = mul(View, Projection);
	position = mul(worldPosition, viewProjection);
}

float4 PixelShaderFunctionNonPremultiplied(float2 texCoord: TEXCOORD0) : COLOR
{
	float4 color = tex2D(ScreenSampler, texCoord);    
	float ret = (atan2(-1 * texCoord.x + 0.5, texCoord.y - 0.5) + PI) / PI2;

	if (ret > Percentage)
	{
		color.rgba = 0;
	}

	return color;
}

float4 PixelShaderFunction(float2 texCoord: TEXCOORD0) : COLOR
{
	float4 color = PixelShaderFunctionNonPremultiplied(texCoord);

	color.r = color.r * color.a;
	color.g = color.g * color.g;
	color.b = color.b * color.b;

	return color;
}

technique NonPremultiplied
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 SpriteVertexShader();
		PixelShader = compile ps_2_0 PixelShaderFunctionNonPremultiplied();
	}
}

technique AlphaBlend
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 SpriteVertexShader();
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}