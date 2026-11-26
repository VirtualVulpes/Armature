using System.Runtime.InteropServices;
using Armature.Rendering;
using Armature.Rendering.Passes;
using Armature.Vulkan;
using Brutal;
using Brutal.Numerics;
using Brutal.VulkanApi;
using Brutal.VulkanApi.Abstractions;

namespace Armature.Editor.Passes;

public class ViewportCapturePass : IRenderPass
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PushData
    {
        public int SceneIndex;
        public int Pad;
        public float2 InvOutputSize;
    }
    
    private readonly Pipeline _pipeline;
    private readonly ViewportTarget _viewport;

    public ViewportCapturePass(Pipeline pipeline, ViewportTarget viewport)
    {
        _pipeline = pipeline;
        _viewport = viewport;
    }

    public unsafe void Record(in RenderContext ctx, ReadOnlySpan<RenderItem> items)
    {
        var dst = _viewport.Target;
        if (dst == null) return;

        var cmd = ctx.CommandBuffer;
        var input = ctx.SceneColor;
        var extent = _viewport.Extent;

        input.AsSampled(cmd);

        dst.HasData = false;
        var colorAttachment = dst.AsColorAttachment(cmd, overwritesAll: true);

        cmd.BeginRendering(
            new VkRenderingInfo
            {
                RenderArea = new VkRect2D(extent),
                LayerCount = 1,
                ColorAttachmentCount = 1,
                ColorAttachments = &colorAttachment
            }
        );

        cmd.SetViewport(0, [
            new VkViewport
            {
                X = 0.0f,
                Y = extent.Height,
                Width = extent.Width,
                Height = -extent.Height,
                MinDepth = 0.0f,
                MaxDepth = 1.0f
            }
        ]);
        cmd.SetScissor(0, [new VkRect2D(ctx.Targets.Extent)]);

        cmd.BindPipeline(VkPipelineBindPoint.Graphics, _pipeline.Handle);
        cmd.BindDescriptorSets(VkPipelineBindPoint.Graphics, _pipeline.Layout, 0,
            [ctx.GlobalDescriptorSet], null);
        cmd.PushConstants(_pipeline.Layout, VkShaderStageFlags.FragmentBit,
            ByteSize.Of<PushConstantBlocks.VertexBlock>(),
            new PushData
            {
                SceneIndex = input.BindlessIndex, InvOutputSize = new float2(1.0f / extent.Width, 1.0f / extent.Height)
            });

        cmd.Draw(3, 1, 0, 0);

        cmd.EndRendering();

        dst.AsSampled(cmd);
    }
}