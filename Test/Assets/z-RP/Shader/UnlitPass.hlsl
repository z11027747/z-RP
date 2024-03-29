#ifndef z_UNLIT_PASS_INCLUDED
#define z_UNLIT_PASS_INCLUDED

#include "../ShaderLibrary/Common.hlsl"

struct Attributes {
	float3 positionOS : POSITION;
	float2 baseUV : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};
struct Varyings {
	float4 positionCS : SV_POSITION;
	float2 baseUV : VAR_BASE_UV;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

zBUFFER_START(UnityPerMaterial)
	zDEFINE_PROP(float4, _BaseMap_ST)
	zDEFINE_PROP(float4, _BaseColor)
	zDEFINE_PROP(half, _Cutoff)
zBUFFER_END(UnityPerMaterial)

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

Varyings UnlitPassVertex (Attributes input) {
	Varyings output;
	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_TRANSFER_INSTANCE_ID(input, output);
	
	float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
	output.positionCS = TransformWorldToHClip(positionWS);
	float4 baseST = zACCESS_PROP(UnityPerMaterial, _BaseMap_ST);
	output.baseUV = input.baseUV * baseST.xy + baseST.zw;
	return output;
}

half4 UnlitPassFragment (Varyings input) : SV_TARGET {
	UNITY_SETUP_INSTANCE_ID(input);

	half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.baseUV);
	half4 baseColor = zACCESS_PROP(UnityPerMaterial, _BaseColor);
	half4 base = baseMap * baseColor;

#if defined(_CLIPPING)
	half cutoff = zACCESS_PROP(UnityPerMaterial, _Cutoff);
	clip(base.a - cutoff);
#endif

	return base;
}

#endif