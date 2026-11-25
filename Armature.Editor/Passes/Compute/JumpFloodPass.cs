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

    public void Record(in RenderContext context, ReadOnlySpan<RenderItem> _)
    {
        _targets.SeedA.Image.TransitionTo(context.CommandBuffer, VkImageLayout.General,
            VkPipelineStageFlags.ComputeShaderBit, VkAccessFlags.ShaderReadBit | VkAccessFlags.ShaderWriteBit);
        _targets.SeedB.Image.TransitionTo(context.CommandBuffer, VkImageLayout.General,
            VkPipelineStageFlags.ComputeShaderBit, VkAccessFlags.ShaderReadBit | VkAccessFlags.ShaderWriteBit);

        context.CommandBuffer.BindPipeline(VkPipelineBindPoint.Compute, _pipeline.Handle);

        var isPassA = true;

        for (var i = StartJump; i >= 1; i >>= 1)
        {
            context.CommandBuffer.PipelineBarrier(
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

            context.CommandBuffer
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

            context.CommandBuffer.PushConstants(_pipeline.Layout, VkShaderStageFlags.ComputeBit, ByteSize.Zero,
                pushConsts);

            const int localSizeX = 16;
            const int localSizeY = 16;

            context.CommandBuffer.Dispatch((context.Targets.Extent.Width + localSizeX - 1) / localSizeX,
                (context.Targets.Extent.Height + localSizeY - 1) / localSizeY, 1);

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