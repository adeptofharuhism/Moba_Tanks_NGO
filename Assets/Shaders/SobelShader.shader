Shader "Custom/SobelShader"
{
	Properties
	{
		_MainTex("Source texture", 2D) = "white"{}
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }


		Pass{
			Cull Off
		Offset 1, 1
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			struct Attributes {
				float4 positionOS : POSITION;
			};

			struct Varyings {
				float4 positionHCS  : SV_POSITION;
			};

			Varyings vert(Attributes IN) {
				Varyings OUT;
				OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
				return OUT;
			}

			half4 frag() : SV_Target{
				half4 color = half4(1,1,0,1);
				return color;
			}

			ENDHLSL
		}
	}
	Fallback "Diffuse"
}
