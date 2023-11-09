#ifndef SOBEL_FUNCTION
#define SOBEL_FUNCTION

void SobelFunction_float(
	Texture2D sobelInputTexture, SamplerState samplerState, float2 uv, 
	float4 sobelColor, float borderIntensity,
	out float3 Out) {

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

	float color = 0;
	[unroll]
	for (int i = 0; i < 9; i++) {
		color += sobelInputTexture.Sample(samplerState, uv.xy + _pixelOffset[i] * offset).x * _sobelHorizontal[i];
		color += sobelInputTexture.Sample(samplerState, uv.xy + _pixelOffset[i] * offset).x * _sobelVertical[i];
	}

	Out = sobelInputTexture.Sample(samplerState, uv) + color * sobelColor;
}

#endif