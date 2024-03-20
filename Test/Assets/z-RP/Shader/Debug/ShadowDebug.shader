Shader "zRP/Debug/ShadowDebug" {
	
	Properties {
		_TileIndex("Tile Index", float) = 0
	}
	
	
	SubShader {
		
		Pass {

			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			
			#include "../../ShaderLibrary/Common.hlsl"
			#include "../../ShaderLibrary/Surface.hlsl"
			#include "../../ShaderLibrary/Shadows.hlsl"

			struct Attributes {
				float3 positionOS : POSITION;
				float2 baseUV : TEXCOORD0;
			};
			struct Varyings {
				float4 positionCS : SV_POSITION;
				float3 positionWS : VAR_BASE_POSITION;
				float2 baseUV : VAR_BASE_UV;
			};
			
			// TEXTURE2D(_DirectionalShadowAtlas);
			SAMPLER(sampler_DirectionalShadowAtlas);

			float _TileIndex;

			Varyings vert (Attributes input) {
				Varyings output;
				
				output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
				output.positionCS = TransformWorldToHClip(output.positionWS);
				output.baseUV = input.baseUV;
				return output;
			}

			half4 frag (Varyings input) : SV_TARGET {

				 float3 positionSTS = mul(_DirectionalShadowMatrices[_TileIndex], float4(input.positionWS, 1.0)).xyz;

				float4 positionCS = mul(UNITY_MATRIX_VP, float4(input.positionWS, 1.0));
				positionCS.xyz/= positionCS.w;
				positionCS.xy= positionCS.xy*0.5 + 0.5;

				 half4 shadowColor = SAMPLE_TEXTURE2D(_DirectionalShadowAtlas, sampler_DirectionalShadowAtlas, positionSTS);
				 return shadowColor;
			}

			ENDHLSL
		}
		
	}
}