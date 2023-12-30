#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float2 EntityTexelSize;
float4 EntityHighlightColor;

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

float4 MainPS(float4 position : SV_Position, float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
	float4 tex = tex2D(SpriteTextureSampler, texCoord);
	float4 res = tex * color;
	float1 sampmaxa = tex.a;
	sampmaxa = max(sampmaxa, tex2D(SpriteTextureSampler, float2(texCoord.x - EntityTexelSize.x, texCoord.y)).a);
	sampmaxa = max(sampmaxa, tex2D(SpriteTextureSampler, float2(texCoord.x + EntityTexelSize.x, texCoord.y)).a);
	sampmaxa = max(sampmaxa, tex2D(SpriteTextureSampler, float2(texCoord.x, texCoord.y - EntityTexelSize.y)).a);
	sampmaxa = max(sampmaxa, tex2D(SpriteTextureSampler, float2(texCoord.x, texCoord.y + EntityTexelSize.y)).a);
	if (tex.a < 0.2 && sampmaxa > tex.a)
	{
		res = EntityHighlightColor;
	}
	return res;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};