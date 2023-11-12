#ifndef SOBEL_FUNCTION
#define SOBEL_FUNCTION

void SobelFunction_float(
	Texture2D sobelInputTexture, SamplerState samplerState, float2 uv,
	float4 sobelColor, float borderIntensity, float sobelThreshold, float colorIntensity,
	out float3 Out) {

	static const int _middleTexel = 4;

	static const half _sobelHorizontal[9] =
	{ 1.0, 2.0, 1.0,
	  0.0, 0.0, 0.0,
	 -1.0,-2.0,-1.0 };
	static const half _sobelVertical[9] =
	{ 1.0, 0.0,-1.0,
	  2.0, 0.0,-2.0,
	  1.0, 0.0,-1.0 };

	static const half2 _pixelOffset[9] =
	{
		half2(-1.0,1.0),
		half2(0.0,1.0),
		half2(1.0,1.0),
		half2(-1.0,0.0),
		half2(0.0,0.0),
		half2(1.0,0.0),
		half2(-1.0,-1.0),
		half2(0.0,-1.0),
		half2(1.0,-1.0)
	};

	float3 offset = float3((1.0 / _ScreenParams.x), (1.0 / _ScreenParams.y), 0.0) * borderIntensity;

	float textureSamples[9];
	[unroll]
	for (int i = 0; i < 9; i++)
		textureSamples[i] = sobelInputTexture.Sample(samplerState, uv.xy + _pixelOffset[i] * offset).x;

	float2 sobel = float2(0, 0);
	float color = 0;
	float
		horizontalSobelColor,
		verticalSobelColor;

	[unroll]
	for (int i = 0; i < 9; i++) {
		horizontalSobelColor = textureSamples[i] * _sobelHorizontal[i];
		verticalSobelColor = textureSamples[i] * _sobelVertical[i];
		sobel += float2(horizontalSobelColor, verticalSobelColor);
	}

	color = (abs(sobel.x) + abs(sobel.y));
	color = (color > sobelThreshold) ? color : 0;
	color *= colorIntensity;

	//float3 outlineColor = lerp(textureSamples[_middleTexel], sobelColor, color);
	float3 outlineColor = sobelColor * color;

	Out = outlineColor;
}

#endif