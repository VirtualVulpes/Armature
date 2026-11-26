using Armature.DebugUi;
using Armature.Rendering;
using Brutal;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using Brutal.VulkanApi;

namespace Armature.Editor;

public sealed class Viewport : IEditorWindow, IFrameListener
{
    private readonly ImGuiManager _imGui;
    private readonly CoreDescriptors _core;

    private RenderTarget? _sceneColor;
    private VkDescriptorSet _set;
    
    public Viewport(Renderer renderer, ImGuiManager imGui, CoreDescriptors core)
    {
        renderer.AddListener(this);
        _imGui = imGui;
        _core = core; 
    }

    private void RegisterSceneColor()
    {
        _set = _imGui.RegisterTexture(_sceneColor.View.VkImageView, _core.TargetSampler);
    }
    
    public void Draw()
    {
        if (_sceneColor == null) return;
        
        ImGui.Begin("Viewport"u8);

        var tex = new ImTextureRef
        {
            _TexData = null,
            _TexID = (ImTextureID)_set.VkHandle
        };
        
        ImGui.Image(tex, new float2(1920, 1080));
        
        ImGui.End();
    }

    public void OnPostRecord(RenderContext ctx)
    {
        if (ReferenceEquals(_sceneColor, ctx.SceneColor)) return;

        if (_set.IsNotNull())
            _imGui.UnregisterTexture(_set);
        
        _sceneColor = ctx.SceneColor;
        RegisterSceneColor();
    }
}