using System.Diagnostics;
using Armature.Rendering;
using Armature.Rendering.Passes;
using Armature.Vulkan;
using Brutal;
using Brutal.VulkanApi;
using Brutal.VulkanApi.Abstractions;

namespace Armature.Editor.Passes.Compute;

public class JumpFloodPass : IRenderPass, IEditorTargetConsumer
{
    private readonly Pipeline _pipeline;
    private readonly EditorDescriptors _editor;
    private readonly VkDescriptorSet _bToASet;
    private readonly VkDescriptorSet _aToBSet;
    private EditorTargets _targets;

    private const uint StartJump = 2;

    public JumpFloodPass(Pipeline pipeline, EditorDescriptors editor)
    {
        Debug.Assert((uint.Log2(StartJump) + 1) % 2 == 0,
            "Jump flood needs an even pass count so the result ends in SeedA");

        _pipeline = pipeline;
        _editor = editor;
        _aToBSet = editor.AllocateSeedDescriptorSet();
        _bToASet = editor.AllocateSeedDescriptorSet();
    }

    public void Record(in RenderContext ctx, ReadOnlySpan<RenderItem> _)
    {
        _targets.SeedA.AsStorage(ctx.CommandBuffer);
        _targets.SeedB.AsStorage(ctx.CommandBuffer, discard: true);
        
        ctx.CommandBuffer.BindPipeline(VkPipelineBindPoint.Compute, _pipeline.Handle);

        var isPassA = true;

        for (var i = StartJump; i >= 1; i >>= 1)
        {
            ctx.CommandBuffer.PipelineBarrier(
                VkPipelineStageFlags.ComputeShaderBit,
                VkPipelineStageFlags.ComputeShaderBit,
                VkDependencyFlags.None,
                [
                    new VkMemoryBarrier
                    {
                        SrcAccessMask = VkAccessFlags.ShaderWriteBit,
                        DstAccessMask = VkAccessFlags.ShaderReadBit,
                    }
                ],
                [],
                []
            );

            ctx.CommandBuffer
                .BindDescriptorSets(VkPipelineBindPoint.Compute, _pipeline.Layout, 0,
                    [
                        isPassA
                            ? _aToBSet
                            : _bToASet
                    ],
                    null);

            var pushConsts = new PushConstantBlocks.JumpPassBlock
            {
                JumpDistance = i
            };

            ctx.CommandBuffer.PushConstants(_pipeline.Layout, VkShaderStageFlags.ComputeBit, ByteSize.Zero,
                pushConsts);

            const int localSizeX = 16;
            const int localSizeY = 16;

            ctx.CommandBuffer.Dispatch((ctx.Targets.Extent.Width + localSizeX - 1) / localSizeX,
                (ctx.Targets.Extent.Height + localSizeY - 1) / localSizeY, 1);

            isPassA = !isPassA;
        }
    }

    public void OnTargetsRecreated(RenderTargets core, EditorTargets editor)
    {
        _targets = editor;   
        _editor.UpdateSeedDescriptor(_aToBSet,
            editor.SeedA.View.VkImageView, editor.SeedB.View.VkImageView);
        _editor.UpdateSeedDescriptor(_bToASet,
            editor.SeedB.View.VkImageView, editor.SeedA.View.VkImageView);
    }
}