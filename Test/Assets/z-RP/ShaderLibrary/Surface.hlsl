#ifndef z_SURFACE_INCLUDED
#define z_SURFACE_INCLUDED

struct Surface {
	float3 position;
	half3 normal;
	half3 viewDirection;
	float depth;
	half3 color;
	half alpha;
	half metallic;
	half smoothness;
};

#endif