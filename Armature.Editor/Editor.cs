using Armature.Input;
using Armature.Rendering;
using Armature.Scene;

namespace Armature.Editor;

public sealed class Editor : IDisposable
{
    private readonly EditorDescriptors _descriptors;
    private readonly EditorRenderFeature _features;
    private readonly EditorInput _input;
    
    private readonly List<IEditorWindow> _windows;

    public Editor(GraphicsBackend graphics, InputManager input, World world, bool isDocked = false)
    {
        var selection = new SelectionHelper(world);

        _windows =
        [
            new Inspector(world, selection),
            new SceneView(world, selection, input),
            new Viewport(graphics.Renderer),
        ];
        
        if (isDocked)
        {
            graphics.SetOutputMode(GraphicsOutputMode.Offscreen);
            _windows.Add(new Viewport(graphics.Renderer));
        }

        _descriptors = new EditorDescriptors(graphics.Context);
        EditorPipelines.Register(
            graphics.Context.Device,
            graphics.Pipelines,
            graphics.Renderer.Swapchain.SurfaceFormat.Format,
            graphics.CoreDescriptors,
            _descriptors
        );
        _features =
            new EditorRenderFeature(graphics.Renderer, graphics.Context, graphics.Pipelines, graphics.CoreDescriptors,
                _descriptors);
        _input = new EditorInput(input, world, _features.Picker, selection);
    }

    public void Update()
    {
        _input.Update();

        foreach (var w in _windows)
        {
            w.Draw();
        }
    }

    public void Dispose()
    {
        _features.Dispose();
        _descriptors.Dispose();
    }
}