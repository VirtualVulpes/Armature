using Armature.Vulkan;
using Brutal;
using Brutal.VulkanApi;
using Brutal.VulkanApi.Abstractions;

namespace Armature.Rendering.Passes;

public class PresentPass : IRenderPass
{
    private readonly Pipeline _pipeline;

    public PresentPass(Pipeline pipeline)
    {
        _pipeline = pipeline;
    }

    public unsafe void Record(in RenderContext ctx, ReadOnlySpan<RenderItem> items)
    {
        var input = ctx.SceneColor;

        input.Image.TransitionTo(ctx.CommandBuffer, VkImageLayout.ShaderReadOnlyOptimal,
            VkPipelineStageFlags.FragmentShaderBit, VkAccessFlags.ShaderReadBit);

        var attachment = new VkRenderingAttachmentInfo
        {
            ImageView = ctx.SwapchainImageView,
            ImageLayout = VkImageLayout.ColorAttachmentOptimal,
            LoadOp = VkAttachmentLoadOp.DontCare,
            StoreOp = VkAttachmentStoreOp.Store
        };

        ctx.CommandBuffer.BeginRendering(
            new VkRenderingInfo
            {
                RenderArea = new VkRect2D(ctx.Targets.Extent),
                LayerCount = 1,
                ColorAttachmentCount = 1,
                ColorAttachments = &attachment
            }
        );

        ctx.CommandBuffer.SetViewport(0, [
            new VkViewport
            {
                X = 0.0f,
                Y = ctx.Targets.Extent.Height,
                Width = ctx.Targets.Extent.Width,
                Height = -ctx.Targets.Extent.Height,
                MinDepth = 0.0f,
                MaxDepth = 1.0f
            }
        ]);
        ctx.CommandBuffer.SetScissor(0, [new VkRect2D(ctx.Targets.Extent)]);

        ctx.CommandBuffer.BindPipeline(VkPipelineBindPoint.Graphics, _pipeline.Handle);
        ctx.CommandBuffer
            .BindDescriptorSets(VkPipelineBindPoint.Graphics, _pipeline.Layout, 0,
                [ctx.GlobalDescriptorSet], null);
        ctx.CommandBuffer
            .PushConstants(_pipeline.Layout, VkShaderStageFlags.FragmentBit, ByteSize.Of<PushConstantBlocks.VertexBlock>(), input.BindlessIndex);
        
        ctx.CommandBuffer.Draw(3, 1, 0, 0);

        ctx.CommandBuffer.EndRendering();
    }
}