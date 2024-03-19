using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class zRenderPipeline : RenderPipeline
{
    bool useDynamicBatching;
    bool useGPUInstancing;
    ShadowSettings shadows;

    public zRenderPipeline(bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher,
                           ShadowSettings shadows)
    {
        GraphicsSettings.lightsUseLinearIntensity = true;
        GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
        this.useDynamicBatching = useDynamicBatching;
        this.useGPUInstancing = useGPUInstancing;
        this.shadows = shadows;
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
    }

    public CameraRenderer renderer = new();

    protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
    {
        base.Render(context, cameras);

        for (int i = 0; i < cameras.Count; i++)
        {
            renderer.Render(context, cameras[i],
                            useDynamicBatching, useGPUInstancing, 
                            shadows);
        }
    }
}
