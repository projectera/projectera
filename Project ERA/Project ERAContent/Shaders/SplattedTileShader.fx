float4x4 World;
float4x4 View;
float4x4 Projection;
Texture staticTiles;
Texture autoTiles;
float2 animOffset;
float frame;
float nextframe;
float frameBlend;

sampler2D staticTilesSampler = sampler_state { texture = <staticTiles> ; magfilter = POINT; minfilter = POINT; mipfilter = POINT; AddressU = clamp; AddressV = clamp;};
sampler2D autoTilesSampler = sampler_state { texture = <autoTiles> ; magfilter = POINT; minfilter = POINT; mipfilter = POINT; AddressU = clamp; AddressV = clamp;};

struct VertexShaderInput
{
	float4 Position : POSITION0;
	byte4 TextureInfo : PSIZE0;
	float2 TexCoord1 : TEXCOORD0;
	float2 TexCoord2 : TEXCOORD1;
	float2 TexCoord3 : TEXCOORD2;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	byte4 TextureInfo : COLOR0;
	float2 TexCoord1 : TEXCOORD0;
	float2 TexCoord2 : TEXCOORD1;
	float2 TexCoord3 : TEXCOORD2;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput data)
{

	float4 worldPosition = mul(data.Position, World);
	float4x4 viewProjection = mul(View, Projection);
	data.Position = mul(worldPosition, viewProjection);

	return data;
}

float4 PixelLayer(VertexShaderOutput input, uniform uint layer, uniform float alpha) : COLOR0
{
	if(input.TextureInfo[0] < (float)layer - 0.5)
		discard;

	float4 color;
	float2 coord;
	if(layer < 1.5)
		coord = input.TexCoord1;
	else if(layer < 2.5)
		coord = input.TexCoord2;
	else
		coord = input.TexCoord3;

	float type = input.TextureInfo[layer];

	if (type < 0.5)
		color = tex2D(staticTilesSampler, coord);
	else if (type < 1.5)
		color = tex2D(autoTilesSampler, coord);
	else
		color = tex2D(autoTilesSampler, coord + float2(frame, 0) * animOffset) * (1 - frameBlend) + tex2D(autoTilesSampler, coord + float2(nextframe, 0) * animOffset) * frameBlend;

	clip(color.a - alpha);

	return color;
}

technique Solid
{
	pass Pass1
	{
		AlphaBlendEnable = False;
		SrcBlend = One;
		DestBlend = Zero;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelLayer(1,1);
	}

	pass Pass2
	{
		AlphaBlendEnable = True;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelLayer(2,0.00001);
	}

	pass Pass3
	{
		AlphaBlendEnable = True;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelLayer(3,0.00001);
	}
}

technique Alpha
{
	pass Pass1
	{
		AlphaBlendEnable = True;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelLayer(1,0.000001);
	}

	pass Pass2
	{
		AlphaBlendEnable = True;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelLayer(2,0.000001);
	}

	pass Pass3
	{
		AlphaBlendEnable = True;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelLayer(3,0.00001);
	}
}
