using UnityEngine;
using UnityEngine.Rendering;

public class Shadows
{
    const string bufferName = "Shadows";

    CommandBuffer buffer = new CommandBuffer
    {
        name = bufferName
    };

    ScriptableRenderContext context;

    public void Setup(ScriptableRenderContext context,
                        CullingResults cullingResults, ShadowSettings settings)
    {
        this.context = context;
        this.cullingResults = cullingResults;
        this.settings = settings;

        shadowedDirectionalLightCount = 0;
    }

    CullingResults cullingResults;

    ShadowSettings settings;

    void ExecuteCmdBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    const int maxShadowedDirectionalLightCount = 4;
    const int maxCascades = 4;

    struct ShadowedDirectionalLight
    {
        public int visibleLightIndex;
    }

    ShadowedDirectionalLight[] ShadowedDirectionalLights =
        new ShadowedDirectionalLight[maxShadowedDirectionalLightCount];

    public Vector2 ReserveDirectionalShadows(Light light, int lightIndex)
    {
        if (shadowedDirectionalLightCount < maxShadowedDirectionalLightCount &&
            light.shadows != LightShadows.None &&
            light.shadowStrength > 0f &&
            cullingResults.GetShadowCasterBounds(lightIndex, out Bounds bounds))
        {
            ShadowedDirectionalLights[shadowedDirectionalLightCount] =
                new ShadowedDirectionalLight { visibleLightIndex = lightIndex };

            return new Vector2(light.shadowStrength, settings.directional.cascadeCount * shadowedDirectionalLightCount++);
        }

        return Vector2.zero;
    }

    int shadowedDirectionalLightCount;

    static int dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");
    public void Render()
    {
        if (shadowedDirectionalLightCount > 0)
        {
            RenderDirectionalShadows();
        }
        else
        {
            buffer.GetTemporaryRT(dirShadowAtlasId, 1, 1,
                32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        }
    }

    void RenderDirectionalShadows()
    {
        int atlasSize = (int)settings.directional.atlasSize;
        buffer.GetTemporaryRT(dirShadowAtlasId, atlasSize, atlasSize,
            32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        buffer.SetRenderTarget(dirShadowAtlasId,
            RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
        buffer.ClearRenderTarget(true, false, Color.clear);

        buffer.BeginSample(bufferName);
        ExecuteCmdBuffer();

        // int tiles = shadowedDirectionalLightCount * settings.directional.cascadeCount;
        int split = 4; // tiles <= 1 ? 1 : tiles <= 4 ? 2 : 4;
        int tileSize = atlasSize / split;

        for (int lightIndex = 0; lightIndex < shadowedDirectionalLightCount; lightIndex++)
        {
            RenderDirectionalShadows(lightIndex, split, tileSize);
        }

        buffer.SetGlobalInt(cascadeCountId, settings.directional.cascadeCount);
        buffer.SetGlobalVectorArray(cascadeCullingSpheresId, cascadeCullingSpheres);
        buffer.SetGlobalMatrixArray(dirShadowMatricesId, dirShadowMatrices);
        buffer.SetGlobalVector(shadowDistanceFadeId, new Vector4(1f / settings.maxDistance, 1f / settings.distanceFade));
        buffer.EndSample(bufferName);
        ExecuteCmdBuffer();
    }

    static int dirShadowMatricesId = Shader.PropertyToID("_DirectionalShadowMatrices");

    static Matrix4x4[] dirShadowMatrices = new Matrix4x4[maxShadowedDirectionalLightCount * maxCascades];

    Matrix4x4 ConvertToAtlasMatrix(Matrix4x4 view, Matrix4x4 projection, Vector2 offset, float scale)
    {
        if (SystemInfo.usesReversedZBuffer)
        {
            projection.m20 = -projection.m20;
            projection.m21 = -projection.m21;
            projection.m22 = -projection.m22;
            projection.m23 = -projection.m23;
        }

        var scaleAndBias = Matrix4x4.identity;
        scaleAndBias.m00 = 0.5f;
        scaleAndBias.m11 = 0.5f;
        scaleAndBias.m22 = 0.5f;
        scaleAndBias.m03 = 0.5f;
        scaleAndBias.m13 = 0.5f;
        scaleAndBias.m23 = 0.5f;

        var cascadeMatrix = Matrix4x4.identity;
        cascadeMatrix.m00 = scale;
        cascadeMatrix.m11 = scale;
        cascadeMatrix.m03 = offset.x;
        cascadeMatrix.m13 = offset.y;

        return cascadeMatrix * scaleAndBias * projection * view;
    }

    void RenderDirectionalShadows(int lightIndex, int split, int tileSize)
    {
        ShadowedDirectionalLight light = ShadowedDirectionalLights[lightIndex];

        var shadowSettings = new ShadowDrawingSettings(cullingResults, light.visibleLightIndex,
                                                        BatchCullingProjectionType.Orthographic);

        Vector3 ratios = settings.directional.CascadeRatios;
        int cascadeCount = settings.directional.cascadeCount;
        int tileIndexOffset = lightIndex * cascadeCount;

        float scale = 1f / split;

        for (int cascadeIndex = 0; cascadeIndex < cascadeCount; cascadeIndex++)
        {
            cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
                light.visibleLightIndex, cascadeIndex, cascadeCount, ratios, tileSize, 0f,
                out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix, out ShadowSplitData splitData);

            shadowSettings.splitData = splitData;

            if (lightIndex == 0)
            {
                Vector4 cullingSphere = splitData.cullingSphere;

                // if (cascadeCullingSphereGos[cascadeIndex] == null ||
                //     cascadeCullingSphereGos[cascadeIndex].transform == null)
                // {
                //     cascadeCullingSphereGos[cascadeIndex] = new GameObject($"cascadeCullingSpheres-{cascadeIndex}-{cullingSphere.w}");
                // }
                // cascadeCullingSphereGos[cascadeIndex].transform.position = cullingSphere;
                // cascadeCullingSphereGos[cascadeIndex].transform.localScale = Vector3.one * cullingSphere.w;

                //r^2
                cullingSphere.w *= cullingSphere.w;

                cascadeCullingSpheres[cascadeIndex] = cullingSphere;
            }

            int tileIndex = tileIndexOffset + cascadeIndex;

            Vector2 tileOffset = new(tileIndex % split, tileIndex / split);

            Rect viewport = new(tileOffset.x * tileSize, tileOffset.y * tileSize, tileSize, tileSize);
            buffer.SetViewport(viewport);

            dirShadowMatrices[tileIndex] = ConvertToAtlasMatrix(viewMatrix, projectionMatrix, tileOffset, scale);

            buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
            ExecuteCmdBuffer();

            context.DrawShadows(ref shadowSettings);
        }
    }

    static int cascadeCountId = Shader.PropertyToID("_CascadeCount");
    static int cascadeCullingSpheresId = Shader.PropertyToID("_CascadeCullingSpheres");
    static int shadowDistanceFadeId = Shader.PropertyToID("_ShadowDistanceFade");

    static Vector4[] cascadeCullingSpheres = new Vector4[maxCascades];
    // static GameObject[] cascadeCullingSphereGos = new GameObject[maxCascades];

    public void Cleanup()
    {
        buffer.ReleaseTemporaryRT(dirShadowAtlasId);
        ExecuteCmdBuffer();
    }
}