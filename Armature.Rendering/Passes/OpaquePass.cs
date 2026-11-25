using Armature.Core;
using Armature.Vulkan;
using Brutal;
using Brutal.VulkanApi;
using Brutal.VulkanApi.Abstractions;

namespace Armature.Rendering.Passes;

public sealed class OpaquePass : IRenderPass
{
    public unsafe void Record(in RenderContext ctx, ReadOnlySpan<RenderItem> items)
    {
        var cmd = ctx.CommandBuffer;
        var t = ctx.Targets;

        var colorAttachment = t.Color.AsColorAttachment(cmd, resolve: t.ResolvedColor, overwritesAll: false);
        var depthAttachment = t.Color.AsDepthAttachment(cmd, resolve: t.ResolvedColor, store: false);

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

        var cachedPipelineId = PipelineId.Invalid;
        Pipeline? pipeline = null;

        foreach (var item in items)
        {
            if (cachedPipelineId != item.Pipeline.Id)
            {
                cachedPipelineId = item.Pipeline.Id;
                pipeline = item.Pipeline;

                ctx.CommandBuffer.BindPipeline(VkPipelineBindPoint.Graphics, pipeline.Handle);
                ctx.CommandBuffer
                    .BindDescriptorSets(VkPipelineBindPoint.Graphics, pipeline.Layout, 0, [ctx.GlobalDescriptorSet, ctx.PerFrameDescriptorSet],
                        null);
            }

            var pushConsts = new PushConstantBlocks.VertexBlock
            {
                Mvp = item.ModelMatrix * ctx.ViewProj,
                Model = item.ModelMatrix
            };

            ctx.CommandBuffer.PushConstants(pipeline.Layout, VkShaderStageFlags.VertexBit, ByteSize.Zero,
                pushConsts);

            switch (item.DataType)
            {
                case RenderDataType.Color:
                    var colorFrag = new PushConstantBlocks.ColorFragmentBlock
                    {
                        Color = item.Color
                    };
                    ctx.CommandBuffer.PushConstants(pipeline.Layout, VkShaderStageFlags.FragmentBit,
                        ByteSize.Of<PushConstantBlocks.VertexBlock>(), colorFrag);
                    break;
                case RenderDataType.Texture:
                    var textureFrag = new PushConstantBlocks.TextureFragmentBlock
                    {
                        TextureId = item.TextureId.Value,
                    };

                    ctx.CommandBuffer.PushConstants(pipeline.Layout, VkShaderStageFlags.FragmentBit,
                        ByteSize.Of<PushConstantBlocks.VertexBlock>(), textureFrag);
                    break;
            }

            var gpuMesh = item.GpuMesh;

            ctx.CommandBuffer.BindVertexBuffers(0, [gpuMesh.VertexBuffer], [new ByteSize64(0)]);
            ctx.CommandBuffer.BindIndexBuffer(gpuMesh.IndexBuffer, new ByteSize64(0), VkIndexType.UInt32);
            ctx.CommandBuffer.DrawIndexed(gpuMesh.IndexCount, 1, 0, 0, 0);
        }

        ctx.CommandBuffer.EndRendering();
    }
}