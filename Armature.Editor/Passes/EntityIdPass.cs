using Armature.Rendering;
using Armature.Rendering.Passes;
using Armature.Vulkan;
using Brutal;
using Brutal.Numerics;
using Brutal.VulkanApi;
using Brutal.VulkanApi.Abstractions;

namespace Armature.Editor.Passes;

public class EntityIdPass : IRenderPass
{
    private readonly Pipeline _pipeline;
    private readonly EditorTargets _editor;

    public EntityIdPass(Pipeline pipeline, EditorTargets editor)
    {
        _pipeline = pipeline;
        _editor = editor;
    }

    public unsafe void Record(in RenderContext ctx, ReadOnlySpan<RenderItem> items)
    {
        var colorAttachment = _editor.EntityId.AsColorAttachment(ctx.CommandBuffer,
            clear: new VkClearColorValue { Uint32 = new uint4(uint.MaxValue, 0, 0, 0) });

        var depthAttachment = _editor.EntityIdDepth.AsDepthAttachment(ctx.CommandBuffer, store: false);
        
        ctx.CommandBuffer.BeginRendering(
            new VkRenderingInfo
            {
                RenderArea = new VkRect2D(ctx.Targets.Extent),
                LayerCount = 1,
                ColorAttachmentCount = 1,
                ColorAttachments = &colorAttachment,
                DepthAttachment = &depthAttachment
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

        foreach (var item in items)
        {
            var vertPushConsts = new PushConstantBlocks.VertexBlock
            {
                Mvp = item.ModelMatrix * ctx.ViewProj,
                Model = item.ModelMatrix
            };

            var fragPushConsts = new PushConstantBlocks.EntityIdBlock
            {
                Id = item.Id
            };

            ctx.CommandBuffer.PushConstants(_pipeline.Layout, VkShaderStageFlags.VertexBit, ByteSize.Zero,
                vertPushConsts);

            ctx.CommandBuffer.PushConstants(_pipeline.Layout, VkShaderStageFlags.FragmentBit, ByteSize.Of<PushConstantBlocks.VertexBlock>(),
                fragPushConsts);

            var gpuMesh = item.GpuMesh;

            ctx.CommandBuffer.BindVertexBuffers(0, [gpuMesh.VertexBuffer], [new ByteSize64(0)]);
            ctx.CommandBuffer.BindIndexBuffer(gpuMesh.IndexBuffer, new ByteSize64(0), VkIndexType.UInt32);
            ctx.CommandBuffer.DrawIndexed(gpuMesh.IndexCount, 1, 0, 0, 0);
        }

        ctx.CommandBuffer.EndRendering();
    }
}