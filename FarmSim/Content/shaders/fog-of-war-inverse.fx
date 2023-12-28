#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float1 HalfScreenWidth;
float1 HalfScreenHeight;
float1 FogOfWarRadiusPow2;
float1 FogOfWarStartClipRadiusPow2;
float1 FogOfWarRadiusPow2Diff;
Texture2D SpriteTexture;

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

float4 MainPS(float4 position : SV_Position, float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
	float1 x = position.x - HalfScreenWidth;
	float1 y = position.y - HalfScreenHeight;
	float1 xyPow2 = x * x + y * y;
	float1 clipRadius = xyPow2 - FogOfWarRadiusPow2;
	float1 startClipRadius = xyPow2 - FogOfWarStartClipRadiusPow2;
	float1 alpha = (clipRadius + startClipRadius) / FogOfWarRadiusPow2Diff;
	float1 alphaClamp = alpha > 1 ? 1 : alpha < 0 ? 0 : alpha;
	float4 res = tex2D(SpriteTextureSampler, texCoord);
	res.r *= color.r;
	res.g *= color.g;
	res.b *= color.b;
	res.a *= color.a * alphaClamp;
	return res;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};