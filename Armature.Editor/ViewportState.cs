using Brutal.Numerics;

namespace Armature.Editor;

public class ViewportState
{
    public bool IsValid { get; private set; }
    public bool IsHovered { get; private set; }
    public float2 MouseUv { get; private set; }
    public int2 Size { get; private set; }

    public void Set(bool hovered, float2 uv, int2 size)
    {
        IsValid = true;
        IsHovered = hovered;
        MouseUv = uv;
        Size = size;
    }

    public void Invalidate() => IsValid = false;
}