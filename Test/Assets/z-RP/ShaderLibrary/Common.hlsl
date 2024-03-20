#ifndef z_COMMON_INCLUDED
#define z_COMMON_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"

#include "./UnityInput.hlsl"

#define UNITY_MATRIX_M unity_ObjectToWorld
#define UNITY_MATRIX_I_M unity_WorldToObject
#define UNITY_MATRIX_V unity_MatrixV
#define UNITY_MATRIX_I_V unity_MatrixInvV
#define UNITY_MATRIX_VP unity_MatrixVP
#define UNITY_PREV_MATRIX_M unity_prev_MatrixM
#define UNITY_PREV_MATRIX_I_M unity_prev_MatrixIM
#define UNITY_MATRIX_P glstate_matrix_projection

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"

//兼容 instancebuffer 和 cbuffer
#if defined(UNITY_ANY_INSTANCING_ENABLED)
    #define zBUFFER_START(name) UNITY_INSTANCING_BUFFER_START(name)
    #define zBUFFER_END(name) UNITY_INSTANCING_BUFFER_END(name)
    #define zDEFINE_PROP(type, var) UNITY_DEFINE_INSTANCED_PROP(type, var)
    #define zACCESS_PROP(arr, var) UNITY_ACCESS_INSTANCED_PROP(arr, var)
#else
    #define zBUFFER_START(name) CBUFFER_START(name)
    #define zBUFFER_END(name) CBUFFER_END
    #define zDEFINE_PROP(type, var) type var;
    #define zACCESS_PROP(arr, var) var
#endif


float DistanceSquared(float3 pA, float3 pB) {
	return dot(pA - pB, pA - pB);
}


#endif