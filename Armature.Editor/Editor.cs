using Armature.DebugUi;
using Armature.Input;
using Armature.Rendering;
using Armature.Scene;
using Brutal.ImGuiApi;
using Brutal.Numerics;

namespace Armature.Editor;

public sealed class Editor : IDisposable
{
    private readonly EditorDescriptors _descriptors;
    private readonly EditorRenderFeature _features;
    private readonly EditorInput _input;
    private readonly ViewportTarget _viewportTarget;

    private readonly Inspector _inspector;
    private readonly SceneView _sceneView;
    private readonly Viewport _viewport;

    public int2 GetRenderSize(int2 fallback) => _viewportTarget.HasTarget ? _viewportTarget.Size : fallback;

    public Editor(GraphicsBackend graphics, InputManager input, World world, ImGuiManager imGui, ViewportState viewportState, bool isDocked = false)
    {
        var selection = new SelectionHelper(world);

        _viewportTarget = new ViewportTarget(graphics.Context.Allocator, graphics.Context.Device,
            graphics.SwapchainFormat, imGui, graphics.CoreDescriptors);

        _inspector = new Inspector(world, selection);
        _sceneView = new SceneView(world, selection, input);
        _viewport = new Viewport(_viewportTarget, viewportState, 4.0f / 3.0f);

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
                _descriptors, _viewportTarget);
        _input = new EditorInput(input, world, _features.Picker, selection, viewportState);
    }

    public void Update()
    {
        _input.Update();

        ImGui.DockSpaceOverViewport(0, ImGui.GetMainViewport());
        ImGui.ShowDemoWindow();
        
        _inspector.Draw();
        _sceneView.Draw();
        _viewport.Draw();
    }

    public void Dispose()
    {
        _viewportTarget.Dispose();
        _features.Dispose();
        _descriptors.Dispose();
    }
}