
using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
    public ScriptableRenderContext context;
    public Camera camera;

    public Lighting lighting = new();

    public void Render(ScriptableRenderContext context, Camera camera,
                        bool useDynamicBatching, bool useGPUInstancing,
                        ShadowSettings shadowSettings)
    {
        this.context = context;
        this.camera = camera;

#if UNITY_EDITOR
        PrepareBuffer();
        PrepareForSceneWindow();
#endif
        if (!Cull(shadowSettings.maxDistance))
            return;

        buffer.BeginSample(SampleName);
        ExecuteCmdBuffer();

        lighting.Setup(context, cullingResults, shadowSettings);

        buffer.EndSample(SampleName);

        Setup();

        DrawVisibleGeometry(useDynamicBatching, useGPUInstancing);
        DrawVisibleTransparentGeometry(useDynamicBatching, useGPUInstancing);

#if UNITY_EDITOR
        DrawUnsupportedShaders();
        DrawGizmos();
#endif
        lighting.Cleanup();
        Submit();
    }

    readonly CommandBuffer buffer = new();

    string SampleName { get; set; } = "Render Camera";

    void ExecuteCmdBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    void Setup()
    {
        context.SetupCameraProperties(camera);

        buffer.ClearRenderTarget(
            camera.clearFlags <= CameraClearFlags.Depth,
            camera.clearFlags == CameraClearFlags.Color,
            camera.clearFlags == CameraClearFlags.Color ? camera.backgroundColor.linear : Color.clear);

        buffer.BeginSample(SampleName);
        ExecuteCmdBuffer();
    }

    CullingResults cullingResults;

    bool Cull(float maxShadowDistance)
    {
        if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))
        {
            p.shadowDistance = Mathf.Min(maxShadowDistance, camera.farClipPlane);
            cullingResults = context.Cull(ref p);
            return true;
        }
        return false;
    }

    static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    static ShaderTagId zLitShaderTagId = new ShaderTagId("zLit");

    void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing)
    {
        var sortingSettings = new SortingSettings(camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };
        var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings)
        {
            enableDynamicBatching = useDynamicBatching,
            enableInstancing = useGPUInstancing
        };
        drawingSettings.SetShaderPassName(1, zLitShaderTagId);

        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);

        context.DrawSkybox(camera);
    }
    void DrawVisibleTransparentGeometry(bool useDynamicBatching, bool useGPUInstancing)
    {
        var sortingSettings = new SortingSettings(camera)
        {
            criteria = SortingCriteria.CommonTransparent
        };
        var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings)
        {
            enableDynamicBatching = useDynamicBatching,
            enableInstancing = useGPUInstancing
        };
        drawingSettings.SetShaderPassName(1, zLitShaderTagId);
        var filteringSettings = new FilteringSettings(RenderQueueRange.transparent);

        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
    }

    void Submit()
    {
        buffer.EndSample(SampleName);
        ExecuteCmdBuffer();
        context.Submit();
    }
}