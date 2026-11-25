using Brutal.Numerics;
using Brutal.VulkanApi;

namespace Armature.Rendering;

public sealed class RenderContext : IFrameListener
{
    public FrameResources FrameResources { get; init; }

    public RenderTargets Targets { get; init; }
    public RenderTarget SceneColor { get; private set; } = null!;
    
    public VkImageView SwapchainImageView { get; init; }
    
    public VkDescriptorSet GlobalDescriptorSet { get; init; }
    public VkDescriptorSet PerFrameDescriptorSet { get; set; }

    public int FrameIndex { get; init; }

    public CommandBuffer CommandBuffer => FrameResources.CommandBuffer;
    
    public float4x4 ViewProj { get; init; }

    public void BeginFrame()
    {
        Targets.BeginFrame();
        SceneColor = Targets.ResolvedColor;
    }

    public void SetSceneColor(RenderTarget t) => SceneColor = t;

    public RenderTarget AcquireOutput()
    {
        foreach (var t in Targets.PostChain)
        {
            if (t == SceneColor) continue;
            t.HasData = false;
            return t;
        }

        throw new InvalidOperationException("No free post target");
    }
}