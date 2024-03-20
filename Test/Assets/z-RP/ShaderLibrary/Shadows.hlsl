#ifndef z_SHADOWS_INCLUDED
#define z_SHADOWS_INCLUDED

#define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4
#define MAX_CASCADE_COUNT 4

TEXTURE2D_SHADOW(_DirectionalShadowAtlas);
SAMPLER_CMP(sampler_linear_clamp_compare);

CBUFFER_START(_CustomShadows)
	int _CascadeCount;
	float4 _CascadeCullingSpheres[MAX_CASCADE_COUNT];
	float4x4 _DirectionalShadowMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT * MAX_CASCADE_COUNT];
	float _ShadowDistance;
CBUFFER_END

struct DirectionalShadowData {
	float strength;
	int tileIndex;
};

float SampleDirectionalShadowAtlas (float3 positionSTS) {
	return SAMPLE_TEXTURE2D_SHADOW(_DirectionalShadowAtlas, sampler_linear_clamp_compare, positionSTS);
}

float GetDirectionalShadowAttenuation (DirectionalShadowData data, Surface surfaceWS) {
	if (data.strength <= 0.0) {
		return 1.0;
	}
	
	float4 positionSTS = mul(_DirectionalShadowMatrices[data.tileIndex], float4(surfaceWS.position, 1.0));
	positionSTS.xyz /= positionSTS.w;

	float shadow = SampleDirectionalShadowAtlas(positionSTS);
	return lerp(1.0, shadow, data.strength);
}

struct ShadowData {
	int cascadeIndex;
	float strength;
};

ShadowData GetShadowData (Surface surfaceWS) {
	ShadowData data;
	data.strength = surfaceWS.depth < _ShadowDistance ? 1.0 : 0.0;
	int i;
	for (i = 0; i < _CascadeCount; i++) {
		float4 sphere = _CascadeCullingSpheres[i];
		float distanceSqr = DistanceSquared(surfaceWS.position, sphere.xyz);
		if (distanceSqr < sphere.w) {
			break;
		}
	}
	data.cascadeIndex = i;
	if (i == _CascadeCount) {
		data.strength = 0.0;
	}
	return data;
}

#endif