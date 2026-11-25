using System.Runtime.InteropServices;
using Armature.Rendering;
using Armature.Rendering.Passes;
using Armature.Vulkan;
using Brutal;
using Brutal.VulkanApi;
using Brutal.VulkanApi.Abstractions;

namespace Armature.Editor.Passes;

public class OutlineCompositePass : IRenderPass, IEditorTargetConsumer
{
    [StructLayout(LayoutKind.Sequential)]
    private struct PushData
    {
        public int SceneIndex;
        public int SeedIndex;
    }
    
    private readonly Pipeline _pipeline;
    private EditorTargets _targets;

    public OutlineCompositePass(Pipeline pipeline)
    {
        _pipeline = pipeline;
    }

    public unsafe void Record(in RenderContext ctx, ReadOnlySpan<RenderItem> items)
    {
        var input = ctx.SceneColor;
        var output = ctx.AcquireOutput();
        
        _targets.SeedA.AsSampled(ctx.CommandBuffer);
        
        var colorAttachment = output.AsColorAttachment(ctx.CommandBuffer, overwritesAll: true);

        ctx.CommandBuffer.BeginRendering(
            new VkRenderingInfo
            {
                RenderArea = new VkRect2D(ctx.Targets.Extent),
                LayerCount = 1,
                ColorAttachmentCount = 1,
                ColorAttachments = &colorAttachment
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
        
        var pushConsts = new PushData
        {
            SceneIndex = input.BindlessIndex,
            SeedIndex = _targets.SeedA.BindlessIndex
        };

        ctx.CommandBuffer.BindPipeline(VkPipelineBindPoint.Graphics, _pipeline.Handle);
        ctx.CommandBuffer
            .BindDescriptorSets(VkPipelineBindPoint.Graphics, _pipeline.Layout, 0,
                [ctx.GlobalDescriptorSet], null);
        ctx.CommandBuffer
            .PushConstants(_pipeline.Layout, VkShaderStageFlags.FragmentBit, ByteSize.Zero, pushConsts);
        
        ctx.CommandBuffer.Draw(3, 1, 0, 0);

        ctx.CommandBuffer.EndRendering();
        
        ctx.SetSceneColor(output);
    }

    public void OnTargetsRecreated(RenderTargets core, EditorTargets editor)
    {
        _targets = editor;   
    }
}