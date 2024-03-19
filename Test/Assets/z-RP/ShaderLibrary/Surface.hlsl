#ifndef z_SURFACE_INCLUDED
#define z_SURFACE_INCLUDED

struct Surface {
	half3 normal;
	half3 viewDirection;
	half3 color;
	half alpha;
	half metallic;
	half smoothness;
};

#endif