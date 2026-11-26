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

    public VkFormat SwapchainFormat => Renderer.Swapchain.SurfaceFormat.Format;
    public float2 Size => new(Renderer.Swapchain.Extent.Width, Renderer.Swapchain.Extent.Height);

    public void WaitIdle() => Context.Device.WaitIdle();

    public GraphicsBackend(GlfwWindow window)
    {
        Context = new VulkanContext(window);
        CoreDescriptors = new CoreDescriptors(Context);
        Pipelines = new PipelineManager(Context.Device);
        CmdManager = new CommandBufferManager(Context.Device, Context.Queue, Context.CommandPool);
        Renderer = new Renderer(Context, window, CoreDescriptors);

        CorePipelines.Register(this);
        Renderer.AddPass(new OpaquePass(), RenderPhase.Scene);
        Renderer.AddPass(new PresentPass(Pipelines.GetPipeline("Present")), RenderPhase.Present);
    }

    public void Dispose()
    {
        Renderer.Dispose();
        Pipelines.Dispose();
        CoreDescriptors.Dispose();
        Context.Dispose();
    }
}