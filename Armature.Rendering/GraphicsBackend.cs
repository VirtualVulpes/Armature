using Armature.Rendering.Passes;
using Armature.Vulkan;
using Brutal.GlfwApi;
using Brutal.Numerics;
using Brutal.VulkanApi;

namespace Armature.Rendering;

public sealed class GraphicsBackend : IDisposable
{
    public readonly VulkanContext Context;
    public readonly CoreDescriptors CoreDescriptors;
    public readonly PipelineManager Pipelines;
    public readonly CommandBufferManager CmdManager;
    public readonly Renderer Renderer;

    public GraphicsOutputMode TargetMode { get; private set; }

    public VkFormat SwapchainFormat => Renderer.Swapchain.SurfaceFormat.Format;
    public int2 Size => new(Renderer.Swapchain.Extent.Width, Renderer.Swapchain.Extent.Height);

    public void WaitIdle() => Context.Device.WaitIdle();

    public GraphicsBackend(GlfwWindow window, GraphicsOutputMode targetMode = GraphicsOutputMode.Swapchain)
    {
        Context = new VulkanContext(window);
        CoreDescriptors = new CoreDescriptors(Context);
        Pipelines = new PipelineManager(Context.Device);
        CmdManager = new CommandBufferManager(Context.Device, Context.Queue, Context.CommandPool);
        Renderer = new Renderer(Context, window, CoreDescriptors);

        TargetMode = targetMode;

        CorePipelines.Register(this);
        Renderer.AddPass(new OpaquePass(), RenderPhase.Scene);
        if (TargetMode == GraphicsOutputMode.Swapchain)
            Renderer.AddPass(new PresentPass(Pipelines.GetPipeline("Present")), RenderPhase.Present);
    }

    public void SetOutputMode(GraphicsOutputMode mode)
    {
        if (TargetMode == mode) return;

        switch (mode)
        {
            case GraphicsOutputMode.Swapchain:
                Renderer.AddPass(new PresentPass(Pipelines.GetPipeline("Present")), RenderPhase.Present);
                break;
            case GraphicsOutputMode.Offscreen:
                Renderer.TryRemovePass(typeof(PresentPass));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }

        TargetMode = mode;
    }

    public void Dispose()
    {
        Renderer.Dispose();
        Pipelines.Dispose();
        CoreDescriptors.Dispose();
        Context.Dispose();
    }
}