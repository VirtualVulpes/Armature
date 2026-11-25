namespace Armature.Rendering;

public interface IFrameListener
{
    void OnFrameBegin(int frameIndex) { }
    void OnPrepare(ref RenderContext ctx) { }
    void OnPostRecord(RenderContext ctx) { }
    void OnResize(RenderTargets targets) { }
}