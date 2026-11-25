using Armature.Core;

namespace Armature.Scene;

public interface IAssetResolver
{
    MeshId Mesh(string name);
    PipelineId Pipeline(string name);
    TextureId Texture(string name);
}