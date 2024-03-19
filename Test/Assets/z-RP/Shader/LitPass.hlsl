#ifndef z_UNLIT_PASS_INCLUDED
#define z_UNLIT_PASS_INCLUDED

#include "../ShaderLibrary/Common.hlsl"
#include "../ShaderLibrary/Surface.hlsl"
#include "../ShaderLibrary/Light.hlsl"
#include "../ShaderLibrary/BRDF.hlsl"
#include "../ShaderLibrary/Lighting.hlsl"

struct Attributes {
	float3 positionOS : POSITION;
	float3 normalOS : NORMAL;
	float2 baseUV : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};
struct Varyings {
	float4 positionCS : SV_POSITION;
	float3 positionWS : VAR_POSITION;
	float3 normalWS : VAR_NORMAL;
	float2 baseUV : VAR_BASE_UV;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

zBUFFER_START(UnityPerMaterial)
	zDEFINE_PROP(float4, _BaseMap_ST)
	zDEFINE_PROP(float4, _BaseColor)
	zDEFINE_PROP(half, _Cutoff)
	zDEFINE_PROP(half, _Metallic)
	zDEFINE_PROP(half, _Smoothness)
zBUFFER_END(UnityPerMaterial)

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

Varyings LitPassVertex (Attributes input) {
	Varyings output;
	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_TRANSFER_INSTANCE_ID(input, output);
	
	output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
	output.normalWS = TransformObjectToWorldNormal(input.normalOS);
	output.positionCS = TransformWorldToHClip(output.positionWS);
	
	float4 baseST = zACCESS_PROP(UnityPerMaterial, _BaseMap_ST);
	output.baseUV = input.baseUV * baseST.xy + baseST.zw;

	return output;
}

half4 LitPassFragment (Varyings input) : SV_TARGET {
	UNITY_SETUP_INSTANCE_ID(input);

	float4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.baseUV);
	float4 baseColor = zACCESS_PROP(UnityPerMaterial, _BaseColor);
	float4 base = baseMap * baseColor;

#if defined(_CLIPPING)
	float cutoff = zACCESS_PROP(UnityPerMaterial, _Cutoff);
	clip(base.a - cutoff);
#endif

	Surface surface;
	surface.normal = normalize(input.normalWS);
	surface.viewDirection = normalize(_WorldSpaceCameraPos - input.positionWS);
	surface.color = base.rgb;
	surface.alpha = base.a;
	surface.metallic = zACCESS_PROP(UnityPerMaterial, _Metallic);
	surface.smoothness = zACCESS_PROP(UnityPerMaterial, _Smoothness);

#if defined(_PREMULTIPLY_ALPHA)
	BRDF brdf = GetBRDF(surface, true);
#else
	BRDF brdf = GetBRDF(surface);
#endif

	half3 color = GetLighting(surface, brdf);
	return half4(color, surface.alpha);
}

#endif