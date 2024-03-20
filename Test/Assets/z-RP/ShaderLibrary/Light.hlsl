#ifndef z_LIGHT_INCLUDED
#define z_LIGHT_INCLUDED

struct Light {
	half3 color;
	float3 direction;
	float attenuation;
};

#define MAX_DIRECTIONAL_LIGHT_COUNT 4

CBUFFER_START(_CustomLight)
	int _DirectionalLightCount;
	half4 _DirectionalLightColors[MAX_DIRECTIONAL_LIGHT_COUNT];
	half4 _DirectionalLightDirections[MAX_DIRECTIONAL_LIGHT_COUNT];
	half4 _DirectionalLightShadowData[MAX_DIRECTIONAL_LIGHT_COUNT];
CBUFFER_END

int GetDirectionalLightCount () {
	return _DirectionalLightCount;
}

DirectionalShadowData GetDirectionalShadowData (int lightIndex, ShadowData shadowData) {
	DirectionalShadowData data;

	//x: strength, y: tileIndex
	half4 lightShadowData = _DirectionalLightShadowData[lightIndex];
	data.strength = lightShadowData.x * shadowData.strength;
	data.tileIndex = lightShadowData.y + shadowData.cascadeIndex;
	return data;
}

Light GetDirectionalLight (int index, Surface surfaceWS, ShadowData shadowData) {
	Light light;
	light.color = _DirectionalLightColors[index].rgb;
	light.direction = _DirectionalLightDirections[index].xyz;

	DirectionalShadowData dirShadowData = GetDirectionalShadowData(index, shadowData);
	light.attenuation = GetDirectionalShadowAttenuation(dirShadowData, surfaceWS);
	// light.attenuation = shadowData.cascadeIndex * 0.25;

	return light;
}

#endif