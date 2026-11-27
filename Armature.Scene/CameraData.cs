using Brutal.Numerics;

namespace Armature.Scene;

public struct CameraData
{
    public float4x4 View;
    public float4x4 Proj;
    public float3 Position;
    public int2 ViewportSize;
}