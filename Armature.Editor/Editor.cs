using Armature.Input;
using Armature.Rendering;
using Armature.Scene;

namespace Armature.Editor;

public sealed class Editor : IDisposable
{
    private readonly EditorDescriptors _descriptors;
    private readonly EditorRenderFeature _features;
    private readonly EditorInput _input;

    private readonly Inspector _inspector;
    private readonly SceneView _sceneView;

    public Editor(GraphicsBackend graphics,
        InputManager input, World world)
    {
        var selection = new SelectionHelper(world);
        _inspector = new Inspector(world, selection);
        _sceneView = new SceneView(world, selection, input);

        _descriptors = new EditorDescriptors(graphics.Context);
        EditorPipelines.Register(
            graphics.Context.Device,
            graphics.Pipelines,
            graphics.Renderer.Swapchain.SurfaceFormat.Format,
            graphics.CoreDescriptors,
            _descriptors
        );
        _features =
            new EditorRenderFeature(graphics.Renderer, graphics.Context, graphics.Pipelines, graphics.CoreDescriptors, _descriptors);
        _input = new EditorInput(input, world, _features.Picker, selection);
    }

    public void Update()
    {
        _input.Update();

        _inspector.Draw();
        _sceneView.Draw();
    }

    public void Dispose()
    {
        _features.Dispose();
        _descriptors.Dispose();
    }
}