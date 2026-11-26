using Brutal.ImGuiApi;
using Brutal.Numerics;

namespace Armature.Editor;

public sealed class Viewport : IEditorWindow
{
    private readonly ViewportTarget _target;
    
    public Viewport(ViewportTarget target)
    {
        _target = target;
    }
    
    public void Draw()
    {
        ImGui.Begin("Viewport"u8);

        var avail = ImGui.GetContentRegionAvail();
        _target.Resize(new int2((int)avail.X, (int)avail.Y));

        if (_target.Target != null)
        {
            var tex = new ImTextureRef
            {
                _TexData = null,
                _TexID = (ImTextureID)_target.Set.VkHandle
            };
        
            ImGui.Image(tex, new float2(_target.Extent.Width, _target.Extent.Height));
        }
        
        ImGui.End();
    }
}