sampler TextureSampler : register(s0);

// Converts the rgb value to hsv, where H's range is -1 to 5
float3 rgb_to_hsv(float3 RGB)
{
    float r = RGB.x;
    float g = RGB.y;
    float b = RGB.z;
  
    float minChannel = min(r, min(g, b));
    float maxChannel = max(r, max(g, b));
    
    float h = 0;
    float s = 0;
    float v = maxChannel;
  
    float delta = maxChannel - minChannel;

    if (delta != 0)
    {
       s = delta / v;
      
       if      (r == v) h =     (g - b) / delta;
       else if (g == v) h = 2 + (b - r) / delta;
       else if (b == v) h = 4 + (r - g) / delta;
    }
    
    return float3(h, s, v);
}

float3 hsv_to_rgb(float3 HSV)
{
    float3 RGB = HSV.z;

    float h = HSV.x;
    float s = HSV.y;
    float v = HSV.z;
    
    float i = floor(h);
    float f = h - i;
    
    float p = (1.0 - s);
    float q = (1.0 - s * f);
    float t = (1.0 - s * (1 - f));
    
    if      (i ==  0)   { RGB = float3(1, t, p); }
    else if (i ==  1)   { RGB = float3(q, 1, p); }
    else if (i ==  2)   { RGB = float3(p, 1, t); }
    else if (i ==  3)   { RGB = float3(p, q, 1); }
    else if (i ==  4)   { RGB = float3(t, p, 1); }
    else  /* i == -1 */ { RGB = float3(1, p, q); }
    
    RGB *= v;

    return RGB;
}

// 60 instructions?
float4 main(float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
	// Get color
    float4 tex = tex2D(TextureSampler, texCoord);
	
	// First convert to hsv
	float3 hsv = rgb_to_hsv(tex.xyz);

	// Now add the hue
	hsv.x += color.r * 6;

	// Put the hue back to the -1 to 5 range
    if (hsv.x > 5) { hsv.x -= 6.0; }
    
	// Convert back to rgb
	tex = float4(hsv_to_rgb(hsv), tex.w);

	// Process opacity (non-premuliplied)
	tex.a *= color.a;

	// Return color
    return tex;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 main();
    }
}