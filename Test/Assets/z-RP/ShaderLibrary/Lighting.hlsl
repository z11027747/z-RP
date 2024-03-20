#ifndef z_LIGHTING_INCLUDED
#define z_LIGHTING_INCLUDED

half3 IncomingLight (Surface surface, Light light) {
	return saturate(dot(surface.normal, light.direction) * light.attenuation) * light.color;
}

half3 GetLighting (Surface surface, BRDF brdf, Light light) {
	return IncomingLight(surface, light) * DirectBRDF(surface, brdf, light);
}

half3 GetLighting (Surface surfaceWS, BRDF brdf) {
	ShadowData shadowData = GetShadowData(surfaceWS);

	// return surfaceWS.position;

	 float4 sphere = _CascadeCullingSpheres[0];
	//  return sphere.w - 13.8;

	//  float distSqr = DistanceSquared(surfaceWS.position, sphere.xyz);
	//  return (distSqr - sphere.w);

	// for (i = 0; i < _CascadeCount; i++) {
	// 	float4 sphere = _CascadeCullingSpheres[i];
	// 	float distanceSqr = DistanceSquared(surfaceWS.position, sphere.xyz);
	// 	if (distanceSqr < sphere.w) {
	// 		break;
	// 	}
	// }

	// if (shadowData.cascadeIndex == 3)
	// 	return half3(1,1,1);
	// else
	// 	return half3(0,0,0);

	half3 color = 0.0;
	for (int i = 0; i < GetDirectionalLightCount(); i++) {
		Light light = GetDirectionalLight(i, surfaceWS, shadowData);
		color += GetLighting(surfaceWS, brdf, light);
	}
	return color;
}

#endif