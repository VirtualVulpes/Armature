using Armature.Rendering;

namespace Armature.SceneRendering;

public sealed class SceneData
{
    public RenderObject[] Objects = [];
    public int ObjectCount;
    public GpuLight[] Lights = [];
    public int LightCount;
    public GpuCamera Camera;

    public ReadOnlySpan<RenderObject> ObjectSpan => Objects.AsSpan(0, ObjectCount);
    public ReadOnlySpan<GpuLight> LightSpan => Lights.AsSpan(0, LightCount);
}