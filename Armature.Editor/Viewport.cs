using Armature.Rendering;
using Brutal.ImGuiApi;

using Brutal.Numerics;

namespace Armature.Editor;

public sealed class Viewport : IEditorWindow, IFrameListener
{
    private RenderTarget? _sceneColor;
    
    public Viewport(Renderer renderer)
    {
        renderer.AddListener(this);
    }
    
    public void Draw()
    {
        // if (_sceneColor == null) return;
        //
        // ImGui.Begin("Viewport"u8);
        //
        // var tex = new ImTextureRef
        // {
        //     _TexData = null,
        //     _TexID = (ImTextureID)_sceneColor.BindlessIndex
        // };
        //
        // ImGui.Image(tex, new float2(1920, 1080));
        //
        // ImGui.End();
    }

    public void OnPostRecord(RenderContext ctx)
    {
        _sceneColor = ctx.SceneColor;
    }
}