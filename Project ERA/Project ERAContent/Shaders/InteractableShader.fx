float4x4 World;
float4x4 View;
float4x4 Projection;
Texture avatarTexture;
float frame;
float direction;
float2 animOffset;
bool bush;

sampler2D avatarSampler = sampler_state { texture = <avatarTexture> ; magfilter = POINT; minfilter = POINT; mipfilter = POINT; AddressU = clamp; AddressV = clamp;};

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float2 PixelPos : TEXCOORD1;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float2 PixelPos : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput data)
{

	float4 worldPosition = mul(data.Position, World);
	float4x4 viewProjection = mul(View, Projection);
	data.Position = mul(worldPosition, viewProjection);
	data.PixelPos.x = data.Position.x;
	data.PixelPos.y = data.Position.y;

	return data;
}

float4 PixelLayer(VertexShaderOutput input) : COLOR0
{

	float4 color;

	color = tex2D(avatarSampler, input.TexCoord + float2(frame * animOffset.x , direction * animOffset.y) );

	if(color.a == 0)
		discard;

	//if (bush)
	//	color.rgb *= 0.5f;

	return color;
}

technique Technique1
{
	pass Pass1
	{
		AlphaBlendEnable = True;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelLayer();
	}
}
