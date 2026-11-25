using Brutal.Numerics;
using Brutal.VulkanApi;
using Brutal.VulkanApi.Abstractions;
using Brutal.VulkanApi.Vma;

namespace Armature.Rendering;

public sealed class RenderTarget : IDisposable
{
    public TrackedImage Image { get; }
    public ImageViewEx View { get; }
    public bool HasData { get; set; }
    public int BindlessIndex { get; set; } = -1;

    public RenderTarget(VmaAllocator allocator, string name, VkFormat format, VkExtent2D extent,
        VkImageUsageFlags usage, VkSampleCountFlags samples = VkSampleCountFlags._1Bit,
        VkImageAspectFlags aspect = VkImageAspectFlags.ColorBit)
    {
        Image = new TrackedImage(allocator.CreateImage(new ImageEx.CreateInfo
        {
            Name = name,
            ImageType = VkImageType._2D,
            ImageFormat = format,
            ImageExtent = new VkExtent3D
            {
                Width = extent.Width,
                Height = extent.Height,
                Depth = 1
            },
            ImageMipLevels = 1,
            ImageArrayLayers = 1,
            ImageSamples = samples,
            ImageTiling = VkImageTiling.Optimal,
            ImageUsage = usage,
            ImageSharingMode = VkSharingMode.Exclusive,
            AllocRequiredProperties = VkMemoryPropertyFlags.DeviceLocalBit
        }));

        View = Image.Image.CreateImageView(VkImageViewType._2D, new VkImageSubresourceRange
        {
            AspectMask = aspect,
            BaseMipLevel = 0,
            LayerCount = 1,
            BaseArrayLayer = 0,
            LevelCount = 1
        }, null);
    }

    public VkRenderingAttachmentInfo AsColorAttachment(CommandBuffer cmd, RenderTarget? resolve = null, bool overwritesAll = false,
        VkClearColorValue? clear = null)
    {
        Image.TransitionTo(cmd, VkImageLayout.ColorAttachmentOptimal, VkPipelineStageFlags.ColorAttachmentOutputBit,
            VkAccessFlags.ColorAttachmentReadBit | VkAccessFlags.ColorAttachmentWriteBit, discardContents: !HasData);

        var loadOp = HasData ? VkAttachmentLoadOp.Load :
            overwritesAll ? VkAttachmentLoadOp.DontCare : VkAttachmentLoadOp.Clear;

        HasData = true;

        var info = new VkRenderingAttachmentInfo
        {
            ImageView = View.VkImageView,
            ImageLayout = VkImageLayout.ColorAttachmentOptimal,
            LoadOp = loadOp,
            StoreOp = resolve != null ? VkAttachmentStoreOp.DontCare : VkAttachmentStoreOp.Store,
            ClearValue = clear ?? new VkClearColorValue { Float32 = new float4(0.01f, 0.01f, 0.01f, 1.0f) },
        };
        
        if (resolve != null)
        {
            resolve.Image.TransitionTo(cmd, VkImageLayout.ColorAttachmentOptimal,
                VkPipelineStageFlags.ColorAttachmentOutputBit,
                VkAccessFlags.ColorAttachmentWriteBit, discardContents: true);
            resolve.HasData = true;

            info.ResolveMode = VkResolveModeFlags.AverageBit;
            info.ResolveImageView = resolve.View.VkImageView;
            info.ResolveImageLayout = VkImageLayout.ColorAttachmentOptimal;
        }
        return info;
    }

    public VkRenderingAttachmentInfo AsDepthAttachment(CommandBuffer cmd, RenderTarget? resolve = null, bool overwritesAll = false,
        float clear = 1.0f, bool store = true)
    {
        Image.TransitionTo(cmd, VkImageLayout.DepthAttachmentOptimal,
            VkPipelineStageFlags.EarlyFragmentTestsBit | VkPipelineStageFlags.LateFragmentTestsBit,
            VkAccessFlags.DepthStencilAttachmentReadBit | VkAccessFlags.DepthStencilAttachmentWriteBit,
            VkImageAspectFlags.DepthBit, discardContents: !HasData);

        var loadOp = HasData ? VkAttachmentLoadOp.Load :
            overwritesAll ? VkAttachmentLoadOp.DontCare : VkAttachmentLoadOp.Clear;

        HasData = true;

        var info = new VkRenderingAttachmentInfo
        {
            ImageView = View.VkImageView,
            ImageLayout = VkImageLayout.DepthAttachmentOptimal,
            LoadOp = loadOp,
            StoreOp = (store && resolve == null) ? VkAttachmentStoreOp.Store : VkAttachmentStoreOp.DontCare,
            ClearValue = new VkClearDepthStencilValue { Depth = clear, Stencil = 0 },
        };

        if (resolve != null)
        {
            resolve.Image.TransitionTo(cmd, VkImageLayout.DepthAttachmentOptimal,
                VkPipelineStageFlags.EarlyFragmentTestsBit | VkPipelineStageFlags.LateFragmentTestsBit,
                VkAccessFlags.DepthStencilAttachmentWriteBit, VkImageAspectFlags.DepthBit, discardContents: true);
            resolve.HasData = true;

            info.ResolveMode = VkResolveModeFlags.SampleZeroBit;
            info.ResolveImageView = resolve.View.VkImageView;
            info.ResolveImageLayout = VkImageLayout.DepthAttachmentOptimal;
        }
        return info;
    }

    public void AsStorage(CommandBuffer cmd, bool discard = false)
    {
        Image.TransitionTo(cmd, VkImageLayout.General, VkPipelineStageFlags.ComputeShaderBit,
            VkAccessFlags.ShaderReadBit | VkAccessFlags.ShaderWriteBit, discardContents: discard && !HasData);

        HasData = true;
    }

    public void AsStorageRead(CommandBuffer cmd)
    {
        Image.TransitionTo(cmd, VkImageLayout.General, VkPipelineStageFlags.ComputeShaderBit,
            VkAccessFlags.ShaderReadBit);
    }
    
    public void AsSampled(CommandBuffer cmd)
    {
        Image.TransitionTo(cmd, VkImageLayout.ShaderReadOnlyOptimal, VkPipelineStageFlags.FragmentShaderBit,
            VkAccessFlags.ShaderReadBit);
    }

    public void Dispose()
    {
        View.Dispose();
        Image.Dispose();
    }
}