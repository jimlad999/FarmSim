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
float1 Scale;
float1 XOffset;
float1 YOffset;
float1 ReachPow2;
bool ArcCrosses0;
float1 FacingDirectionRadiansMin;
float1 FacingDirectionRadiansMax;
Texture2D SpriteTexture;

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

float4 MainPS(float4 position : SV_Position, float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
	float1 x = position.x - HalfScreenWidth + XOffset;
	float1 y = position.y - HalfScreenHeight + YOffset;
	float1 xyPow2 = (x * x + y * y) / Scale;
	bool1 inCircle = xyPow2 < ReachPow2;
	float1 entityDirectionRadians = atan2(-y, x);
	float1 colorMod = inCircle &&
		(ArcCrosses0
			? entityDirectionRadians > FacingDirectionRadiansMin || entityDirectionRadians < FacingDirectionRadiansMax
			: entityDirectionRadians > FacingDirectionRadiansMin && entityDirectionRadians < FacingDirectionRadiansMax)
		? 0.1 : 1;
	float4 res = tex2D(SpriteTextureSampler, texCoord);
	res.r *= color.r * colorMod;
	res.g *= color.g * colorMod;
    res.b *= color.b * colorMod;
	res.a *= color.a;
	return res;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};