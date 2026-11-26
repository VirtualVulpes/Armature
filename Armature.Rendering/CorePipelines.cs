using Armature.Rendering.Pipelines;
using Brutal;
using Brutal.VulkanApi;
using Brutal.VulkanApi.Abstractions;

namespace Armature.Rendering;

public static class CorePipelines
{
    public static void Register(GraphicsBackend graphics)
    {
        ShaderLibrary.AddRoot(Path.Combine(AppContext.BaseDirectory, "shaders", "rendering"));
        
        var b = new MeshPipelineBuilder(graphics.Context.Device, graphics.Pipelines);
        DescriptorSetLayoutEx[] layouts = [graphics.CoreDescriptors.GlobalLayout, graphics.CoreDescriptors.PerFrameLayout];

        MeshPipelineOptions Options(ByteSize frag, VkPolygonMode mode = VkPolygonMode.Fill) => new()
        {
            ColorFormat = graphics.SwapchainFormat,
            Samples = graphics.Renderer.MsaaSamples,
            PolygonMode = mode,
            FragmentPushSize = frag,
            Layouts = layouts
        };

        var tex = ByteSize.Of<PushConstantBlocks.TextureFragmentBlock>();
        var col = ByteSize.Of<PushConstantBlocks.ColorFragmentBlock>();

        b.Add("Flat Texture", "world_space.vert", "flat_texture.frag", Options(tex));
        b.Add("Flat Color", "default.vert", "flat_color.frag", Options(col));
        b.Add("Wireframe", "default.vert", "flat_color.frag", Options(col, VkPolygonMode.Line));
        b.Add("Phong Texture", "world_space.vert", "phong_texture.frag", Options(tex));
        b.Add("Phong Color", "world_space.vert", "phong_color.frag", Options(col));
        b.Add("Present", "fullscreen.vert", "present.frag", new MeshPipelineOptions
        {
            ColorFormat = graphics.SwapchainFormat,
            FragmentPushSize = ByteSize.Of<int>(),
            Layouts = [graphics.CoreDescriptors.GlobalLayout]
        });    
    }
}