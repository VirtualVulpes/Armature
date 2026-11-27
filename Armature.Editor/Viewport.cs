using Brutal.ImGuiApi;
using Brutal.Numerics;

namespace Armature.Editor;

public sealed class Viewport : IEditorWindow, IDisposable
{
    public ViewportTarget Target { get; }
    public ViewportState State { get; }

    private readonly float _aspectRatio;

    public Viewport(ViewportTarget target, ViewportState state, float aspectRatio)
    {
        Target = target;
        State = state;
        _aspectRatio = aspectRatio;
    }

    public void Draw()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, float2.Zero);
        var visible = ImGui.Begin("Viewport"u8);
        ImGui.PopStyleVar();

        if (visible)
        {
            var avail = ImGui.GetContentRegionAvail();
            var max = ImGui.GetIO().DisplaySize;
            var w = float.Min(avail.X, max.X);
            var h = float.Min(avail.Y, max.Y);

            if (w >= 1.0f && h >= 1.0f)
            {
                var size = w / h < _aspectRatio
                    ? new int2((int)w, (int)(w / _aspectRatio))
                    : new int2((int)(h * _aspectRatio), (int)h);

                Target.Resize(size);

                if (Target.Target != null)
                {
                    var imageSize = new float2(Target.Extent.Width, Target.Extent.Height);

                    var cursor = ImGui.GetCursorPos();
                    ImGui.SetCursorPos(new float2(
                        cursor.X + (avail.X - imageSize.X) * 0.5f,
                        cursor.Y + (avail.Y - imageSize.Y) * 0.5f
                    ));

                    var tex = new ImTextureRef
                    {
                        _TexData = null,
                        _TexID = (ImTextureID)Target.Set.VkHandle
                    };

                    ImGui.Image(tex, imageSize);

                    var min = ImGui.GetItemRectMin();
                    var mouse = ImGui.GetMousePos();
                    var uv = new float2(
                        float.Clamp((mouse.X - min.X) / imageSize.X, 0.0f, 1.0f),
                        float.Clamp((mouse.Y - min.Y) / imageSize.Y, 0.0f, 1.0f)
                    );

                    State.Set(ImGui.IsItemHovered(), uv, new int2((int)imageSize.X, (int)imageSize.Y));
                }
            }
        }

        ImGui.End();
    }

    public void Dispose() => Target.Dispose();
}