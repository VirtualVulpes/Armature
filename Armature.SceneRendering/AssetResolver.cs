using Armature.Core;
using Armature.Rendering;
using Armature.Scene;
using Armature.Vulkan;

namespace Armature.SceneRendering;

public sealed class AssetResolver(AssetLibrary assets, PipelineManager pipelines)
    : IAssetResolver
{
    public MeshId Mesh(string name) => assets.Meshes.GetMeshId(name);
    public PipelineId Pipeline(string name) => pipelines.GetPipelineId(name);
    public TextureId Texture(string name) => assets.Textures.GetTextureId(name);
}