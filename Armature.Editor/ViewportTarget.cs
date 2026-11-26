using Armature.DebugUi;
using Armature.Rendering;
using Brutal.Numerics;
using Brutal.VulkanApi;
using Brutal.VulkanApi.Abstractions;
using Brutal.VulkanApi.Vma;

namespace Armature.Editor;

public sealed class ViewportTarget : IDisposable
{
    private readonly VmaAllocator _allocator;
    private readonly DeviceEx _device;
    private readonly VkFormat _format;
    private readonly ImGuiManager _imGui;
    private readonly CoreDescriptors _core;

    public RenderTarget? Target { get; private set; }
    public VkExtent2D Extent => Target.Extent;
    public VkDescriptorSet Set { get; private set; }

    public ViewportTarget(VmaAllocator allocator, DeviceEx device, VkFormat format, ImGuiManager imGui,
        CoreDescriptors core)
    {
        _allocator = allocator;
        _device = device;
        _format = format;
        _imGui = imGui;
        _core = core;
    }

    public void Resize(int2 size)
    {
        if (size is { X: <= 0, Y: <= 0 }) return;
        if (Target != null && Target.Extent.Width == size.X && Target.Extent.Height == size.Y) return;

        Release();

        Target = new RenderTarget(_allocator, "Viewport Render Target", _format, Extent,
            VkImageUsageFlags.SampledBit | VkImageUsageFlags.ColorAttachmentBit);
        Set = _imGui.RegisterTexture(Target.View.VkImageView, _core.TargetSampler);
    }

    private void Release()
    {
        if (Target == null) return;
        _device.WaitIdle();
        _imGui.UnregisterTexture(Set);
        Target.Dispose();
        Target = null;
        Set = default;
    }

    public void Dispose() => Release();
}