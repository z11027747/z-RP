using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class zRenderPipeline : RenderPipeline
{
    public zRenderPipeline(bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher)
    {
        GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
        this.useDynamicBatching = useDynamicBatching;
        this.useGPUInstancing = useGPUInstancing;
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
    }

    public CameraRenderer renderer = new();

    bool useDynamicBatching;
    bool useGPUInstancing;

    protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
    {
        base.Render(context, cameras);

        for (int i = 0; i < cameras.Count; i++)
        {
            renderer.Render(context, cameras[i],
                            useDynamicBatching, useGPUInstancing);
        }
    }
}
