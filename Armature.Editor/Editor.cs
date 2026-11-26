using Armature.Input;
using Armature.Rendering;
using Armature.Rendering.Pipelines;
using Armature.Scene;
using Armature.Vulkan;

namespace Armature.Editor;

public sealed class Editor : IDisposable
{
    private readonly EditorDescriptors _descriptors;
    private readonly EditorRenderFeature _features;
    private readonly EditorInput _input;

    private readonly Inspector _inspector;
    private readonly SceneView _sceneView;

    public Editor(VulkanContext context, Renderer renderer, CoreDescriptors core, PipelineManager pipelines,
        InputManager input, World world)
    {
        var selection = new SelectionHelper(world);
        _inspector = new Inspector(world, selection);
        _sceneView = new SceneView(world, selection, input);

        _descriptors = new EditorDescriptors(context);
        EditorPipelines.Register(
            context.Device,
            pipelines,
            new PipelineBuildContext
            {
                ColorFormat = renderer.Swapchain.SurfaceFormat.Format,
                MsaaSamples = renderer.MsaaSamples
            }, core,
            _descriptors
        );
        _features =
            new EditorRenderFeature(renderer, context, pipelines, core, _descriptors);
        _input = new EditorInput(input, world, _features.Picker, selection);
    }

    public void Update()
    {
        _input.Update();
    }

    public void Draw()
    {
        _inspector.Draw();
        _sceneView.Draw();
    }

    public void Dispose()
    {
        _features.Dispose();
        _descriptors.Dispose();
    }
}