Shader "Custom/SobelShader"
{
	Properties
	{
		_Color("Base Color", Color) = (1,1,1,1)
		_MainTex("Source texture", 2D) = "white"
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
		LOD 100

		Pass{

			HLSLPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			struct Attributes {
				float4 positionLocalSpace : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct Varyings {
				float4 positionClipSpace  : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);

			CBUFFER_START(UnityPerMaterial)
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

				half4 _Color;
				float4 _MainTex_ST;
			CBUFFER_END

			Varyings vert(Attributes IN) {
				Varyings OUT;
				OUT.positionClipSpace = TransformObjectToHClip(IN.positionLocalSpace.xyz);
				OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
				return OUT;
			}

			float4 frag(Varyings IN) : SV_Target{
				float3 offset = float3((1.0 / _ScreenParams.x), (1.0 / _ScreenParams.y), 0.0);

				float color = 0;
				[unroll]
				for (int i = 0; i < 9; i++) {
					/*color += max(
						_MainTex.Sample(sampler_MainTex, IN.uv.xy + _pixelOffset[i] * offset) * _sobelHorizontal[i],
						_MainTex.Sample(sampler_MainTex, IN.uv.xy + _pixelOffset[i] * offset) * _sobelVertical[i]);*/

					color += _MainTex.Sample(sampler_MainTex, IN.uv.xy + _pixelOffset[i] * offset).x * _sobelHorizontal[i];
					color += _MainTex.Sample(sampler_MainTex, IN.uv.xy + _pixelOffset[i] * offset).x * _sobelVertical[i];
				}

				return float4(color, color, color, 1) * _Color;
			}

			ENDHLSL
		}
	}
}
