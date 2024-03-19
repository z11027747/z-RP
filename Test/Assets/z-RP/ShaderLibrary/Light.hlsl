#ifndef z_LIGHT_INCLUDED
#define z_LIGHT_INCLUDED

struct Light {
	half3 color;
	float3 direction;
};

#define MAX_DIRECTIONAL_LIGHT_COUNT 4

CBUFFER_START(_CustomLight)
	int _DirectionalLightCount;
	half4 _DirectionalLightColors[MAX_DIRECTIONAL_LIGHT_COUNT];
	half4 _DirectionalLightDirections[MAX_DIRECTIONAL_LIGHT_COUNT];
CBUFFER_END

int GetDirectionalLightCount () {
	return _DirectionalLightCount;
}

Light GetDirectionalLight (int index) {
	Light light;
	light.color = _DirectionalLightColors[index].rgb;
	light.direction = _DirectionalLightDirections[index].xyz;
	return light;
}

#endif