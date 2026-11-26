using Armature.Editor.Passes;
using Armature.Editor.Passes.Compute;
using Armature.Rendering;
using Armature.Vulkan;

namespace Armature.Editor;

public sealed class EditorRenderFeature : IFrameListener, IDisposable
{
    private readonly CoreDescriptors _core;
    private readonly EditorTargets _targets;
    private readonly IEditorTargetConsumer[] _consumers;

    public EditorRenderFeature(Renderer renderer, VulkanContext ctx, PipelineManager pipelines,
        CoreDescriptors core, EditorDescriptors descriptors, ViewportTarget target)
    {
        _core = core;
        _targets = new EditorTargets(ctx.Allocator, renderer.Targets.Extent);
        
        var seed = new SeedInitPass(pipelines.GetPipeline("Seed Init"), descriptors);
        var flood = new JumpFloodPass(pipelines.GetPipeline("Jump Flood"), descriptors);
        var outline = new OutlineCompositePass(pipelines.GetPipeline("Outline Composite"));
        var viewport = new ViewportCapturePass(pipelines.GetPipeline("Viewport Capture"), target);
        _consumers = [seed, flood, outline];
        
        Picker = new EntityPicker(ctx, _targets);

        renderer.AddPass(new EntityIdPass(pipelines.GetPipeline("Entity Id"), _targets), RenderPhase.PostScene);
        renderer.AddPass(new SelectionMaskPass(pipelines.GetPipeline("Selection Mask"), _targets), RenderPhase.PostScene);
        renderer.AddPass(seed, RenderPhase.PostScene);
        renderer.AddPass(flood, RenderPhase.PostScene);
        renderer.AddPass(outline, RenderPhase.PostScene);
        renderer.AddPass(viewport, RenderPhase.PostScene);
        
        renderer.AddListener(this);
        renderer.AddListener(Picker);
        
        Rebind(renderer.Targets);
    }

    public EntityPicker Picker { get; }

    public void OnResize(RenderTargets core)
    {
        _targets.Recreate(core.Extent);
        Rebind(core);
    }

    public void OnPrepare(RenderContext ctx)
    {
        _targets.BeginFrame();
    }

    private void Rebind(RenderTargets core)
    {
        _core.RegisterTarget(_targets.SeedA, 4);
        foreach (var c in _consumers) c.OnTargetsRecreated(core, _targets);
    }

    public void Dispose()
    {
        Picker.Dispose();
        _targets.Dispose();
    }
}