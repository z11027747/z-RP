using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Lighting
{
    const string bufferName = "Lighting";

    CommandBuffer buffer = new CommandBuffer
    {
        name = bufferName
    };

    CullingResults cullingResults;

    ShadowSettings shadowSettings;

    public Shadows shadows = new();

    public void Setup(ScriptableRenderContext context,
                        CullingResults cullingResults, ShadowSettings shadowSettings)
    {
        this.cullingResults = cullingResults;
        this.shadowSettings = shadowSettings;
        buffer.BeginSample(bufferName);
        {
            shadows.Setup(context, cullingResults, shadowSettings);
            SetupLights();
            shadows.Render();
            buffer.EndSample(bufferName);
        }
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    const int maxDirLightCount = 4;

    static int dirLightCountId = Shader.PropertyToID("_DirectionalLightCount");
    static int dirLightColorsId = Shader.PropertyToID("_DirectionalLightColors");
    static int dirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections");
    static int dirLightShadowDataId = Shader.PropertyToID("_DirectionalLightShadowData");

    static Vector4[] dirLightColors = new Vector4[maxDirLightCount];
    static Vector4[] dirLightDirections = new Vector4[maxDirLightCount];
    static Vector4[] dirLightShadowData = new Vector4[maxDirLightCount];

    void SetupLights()
    {
        int dirLightCount = 0;
        NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;
        for (int i = 0; i < visibleLights.Length; i++)
        {
            VisibleLight visibleLight = visibleLights[i];
            if (visibleLight.lightType == LightType.Directional)
            {
                var dirLightIndex = dirLightCount;
                SetupDirectionalLight(dirLightIndex, ref visibleLight);
                if (dirLightCount++ >= maxDirLightCount)
                {
                    break;
                }
            }
        }

        buffer.SetGlobalInt(dirLightCountId, visibleLights.Length);
        buffer.SetGlobalVectorArray(dirLightColorsId, dirLightColors);
        buffer.SetGlobalVectorArray(dirLightDirectionsId, dirLightDirections);
        buffer.SetGlobalVectorArray(dirLightShadowDataId, dirLightShadowData);
    }

    void SetupDirectionalLight(int dirLightIndex, ref VisibleLight visibleLight)
    {
        dirLightColors[dirLightIndex] = visibleLight.finalColor;
        dirLightDirections[dirLightIndex] = -visibleLight.localToWorldMatrix.GetColumn(2);
        dirLightShadowData[dirLightIndex] = shadows.ReserveDirectionalShadows(visibleLight.light, dirLightIndex);
    }

    public void Cleanup()
    {
        shadows.Cleanup();
    }
}