using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/z-RP Test")]
public class zRenderPipelineAsset : RenderPipelineAsset
{
    public bool useDynamicBatching = true;
    public bool useGPUInstancing = true;
    public bool useSRPBatcher = true;

    public ShadowSettings shadows = default;

    protected override RenderPipeline CreatePipeline()
    {
        return new zRenderPipeline(useDynamicBatching, useGPUInstancing, useSRPBatcher, shadows);
    }
}
