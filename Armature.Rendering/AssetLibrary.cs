using Armature.Rendering.Meshes;
using Armature.Vulkan;
using Brutal.VulkanApi;

namespace Armature.Rendering;

public sealed class AssetLibrary : IDisposable
{
    private readonly GraphicsBackend _graphics;
    private readonly string _textureRoot;
    private readonly string _modelRoot;

    public readonly TextureManager Textures;
    public readonly MeshManager Meshes;

    public AssetLibrary(GraphicsBackend graphics, string textureRoot, string modelRoot)
    {
        _graphics = graphics;
        _textureRoot = textureRoot;
        _modelRoot = modelRoot;

        Textures = new TextureManager(graphics.Context.PhysicalDevice.GetFeatures().TextureCompressionBC);
        Meshes = new MeshManager(graphics.Context.Allocator);
        MeshLibrary.RegisterDefaults(Meshes);
    }

    public void LoadTextures(params ReadOnlySpan<string> files)
    {
        if (files.IsEmpty) return;

        var ctx = _graphics.Context;
        var cmd = CommandBufferManager.BeginSingleTimeCommands(ctx.Device, ctx.CommandPool);

        foreach (var file in files)
        {
            var id = Textures.AddTexture(Path.Join(_textureRoot, file), cmd, ctx.Allocator, TextureColorSpace.SRgb);
            _graphics.CoreDescriptors.UpdateTextureDescriptor(Textures.GetTexture(id));
        }

        CommandBufferManager.EndSingleTimeCommands(cmd, ctx.Queue);
        Textures.ClearStagingBuffers();
    }

    public void LoadModel(string name, string file) =>
        Meshes.AddMesh(name, ModelLoader.LoadModel(Path.Join(_modelRoot, file)));

    public void Dispose()
    {
        Textures.Dispose();
        Meshes.Dispose();
    }
}