
using UnityEngine;
using UnityEngine.Rendering;

public partial class zCameraRenderer
{
    public ScriptableRenderContext context;
    public Camera camera;

    public void Render(ScriptableRenderContext context, Camera camera)
    {
        this.context = context;
        this.camera = camera;

#if UNITY_EDITOR
        PrepareBuffer();
        PrepareForSceneWindow();
#endif
        if (!Cull())
            return;

        Setup();
        DrawVisibleGeometry();
        DrawVisibleTransparentGeometry();

#if UNITY_EDITOR
        DrawUnsupportedShaders();
        DrawGizmos();
#endif
        Submit();
    }

    readonly CommandBuffer buffer = new();

    string SampleName { get; set; } = "Render Camera";

    void ExecuteBuffer()
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
        ExecuteBuffer();
    }

    CullingResults cullingResults;
    bool Cull()
    {
        if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))
        {
            cullingResults = context.Cull(ref p);
            return true;
        }
        return false;
    }

    static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");

    void DrawVisibleGeometry()
    {
        var sortingSettings = new SortingSettings(camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };
        var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings);
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);

        context.DrawSkybox(camera);
    }
    void DrawVisibleTransparentGeometry()
    {
        var sortingSettings = new SortingSettings(camera)
        {
            criteria = SortingCriteria.CommonTransparent
        };
        var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings);
        var filteringSettings = new FilteringSettings(RenderQueueRange.transparent);

        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
    }

    void Submit()
    {
        buffer.EndSample(SampleName);
        ExecuteBuffer();
        context.Submit();
    }
}