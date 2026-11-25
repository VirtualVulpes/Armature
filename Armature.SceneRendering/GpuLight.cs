using System.Runtime.InteropServices;
using Brutal.Numerics;

namespace Armature.SceneRendering;

[StructLayout(LayoutKind.Explicit, Size = 64)]
public record struct GpuLight
{
    [FieldOffset(0)]
    public float3 Position;
    
    [FieldOffset(12)]
    public float Intensity;
    
    [FieldOffset(16)]
    public float3 Color;
    
    [FieldOffset(28)]
    public float Range;
    
    [FieldOffset(32)]
    public float3 Direction;
    
    [FieldOffset(44)]
    public int Type;
    
    [FieldOffset(48)]
    public float InnerCos;
    
    [FieldOffset(52)]
    public float OuterCos;
}