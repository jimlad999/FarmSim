#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float2 TileTexelSize;
float4 TileHighlightColor;

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

float4 MainPS(float4 position : SV_Position, float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
	float4 tex = tex2D(SpriteTextureSampler, texCoord);
	float4 res = tex * color;
	float1 sampmaxa = float(1);
	sampmaxa = min(sampmaxa, tex2D(SpriteTextureSampler, float2(texCoord.x - TileTexelSize.x, texCoord.y)).a);
	sampmaxa = min(sampmaxa, tex2D(SpriteTextureSampler, float2(texCoord.x + TileTexelSize.x, texCoord.y)).a);
	sampmaxa = min(sampmaxa, tex2D(SpriteTextureSampler, float2(texCoord.x, texCoord.y - TileTexelSize.y)).a);
	sampmaxa = min(sampmaxa, tex2D(SpriteTextureSampler, float2(texCoord.x, texCoord.y + TileTexelSize.y)).a);
	if (sampmaxa < 1)
	{
		res = TileHighlightColor;
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