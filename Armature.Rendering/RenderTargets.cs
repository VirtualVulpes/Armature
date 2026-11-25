using Brutal.VulkanApi;
using Brutal.VulkanApi.Vma;

namespace Armature.Rendering;

public sealed class RenderTargets : IDisposable
{
    public VkExtent2D Extent { get; }
    
    public readonly RenderTarget Color;
    public readonly RenderTarget ResolvedColor;
    public readonly RenderTarget Depth;
    public readonly RenderTarget ResolvedDepth;
    public readonly RenderTarget PostProcessA;
    public readonly RenderTarget PostProcessB;
    
    public RenderTarget[] PostChain { get; }
    
    public RenderTargets(VmaAllocator allocator, VkFormat swapchainFormat, VkExtent2D extent, VkSampleCountFlags msaa)
    {
        Extent = extent;
        
        Color = new RenderTarget(allocator, "Color Render Target", swapchainFormat, extent,
            VkImageUsageFlags.ColorAttachmentBit | VkImageUsageFlags.TransientAttachmentBit, samples: msaa);
        
        Depth = new RenderTarget(allocator, "Depth Render Target", VkFormat.D32SFloat, extent,
            VkImageUsageFlags.DepthStencilAttachmentBit | VkImageUsageFlags.TransientAttachmentBit, aspect: VkImageAspectFlags.DepthBit, samples: msaa);
        
        ResolvedColor = new RenderTarget(allocator, "Resolved Color Render Target", swapchainFormat, extent,
            VkImageUsageFlags.ColorAttachmentBit | VkImageUsageFlags.SampledBit);

        ResolvedDepth = new RenderTarget(allocator, "Resolved Depth Render Target", VkFormat.D32SFloat, extent,
            VkImageUsageFlags.DepthStencilAttachmentBit | VkImageUsageFlags.SampledBit, aspect: VkImageAspectFlags.DepthBit);
        
        PostProcessA = new RenderTarget(allocator, "Post Process A Render Target", swapchainFormat, extent,
            VkImageUsageFlags.ColorAttachmentBit | VkImageUsageFlags.SampledBit);
        
        PostProcessB = new RenderTarget(allocator, "Post Process B Render Target", swapchainFormat, extent,
            VkImageUsageFlags.ColorAttachmentBit | VkImageUsageFlags.SampledBit);

        PostChain = [PostProcessA, PostProcessB];
    }

    public void BeginFrame()
    {
        foreach (var t in new[] { Color, ResolvedColor, Depth, ResolvedDepth, PostProcessA, PostProcessB })
            t.HasData = false;
    }
    
    public void Dispose()
    {
        Color.Dispose();
        ResolvedColor.Dispose();
        Depth.Dispose();
        ResolvedDepth.Dispose();
        PostProcessA.Dispose();
        PostProcessB.Dispose();
    }
}