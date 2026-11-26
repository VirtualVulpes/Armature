using Armature.Core;
using Armature.Vulkan;
using Brutal.VulkanApi.Vma;

namespace Armature.Rendering.Meshes;

public sealed class MeshManager : IDisposable
{
    private uint _nextId;
    private readonly VmaAllocator _allocator;
    
    private readonly Dictionary<MeshId, Mesh> _meshes = new();
    private readonly Dictionary<MeshId, GpuMesh> _gpuMeshes = new();
    private readonly Dictionary<string, MeshId> _names = new();

    public MeshManager(VmaAllocator allocator)
    {
        _allocator = allocator;
    }
    
    public MeshId AddMesh(string name, Mesh mesh)
    {
        var meshId = new MeshId(_nextId++);
        
        _meshes.Add(meshId, mesh);
        _names.Add(name, meshId);

        UploadMesh(meshId);
        
        return meshId;
    }
    
    public MeshId AddMesh(Mesh mesh)
    {
        var meshId = new MeshId(_nextId++);
        
        _meshes.Add(meshId, mesh);

        UploadMesh(meshId);
        
        return meshId;
    }


    private void UploadMesh(MeshId meshId)
    {
        var mesh = _meshes[meshId];
        _gpuMeshes.Add(meshId, GpuMesh.Create(_allocator, mesh.Vertices, mesh.Indices));
    }
    
    public Mesh GetMesh(MeshId meshId)
    {
        return _meshes[meshId];
    }

    public bool HasMesh(string name)
    {
        return _names.ContainsKey(name);
    }
    
    public bool HasMesh(MeshId id)
    {
        return _meshes.ContainsKey(id);
    }

    public GpuMesh GetGpuMesh(MeshId meshId)
    {
        return _gpuMeshes[meshId];
    }

    public MeshId GetMeshId(string name)
    {
        return _names[name];
    }

    public void Dispose()
    {
        foreach (var mesh in _gpuMeshes.Values)
        {
            mesh.Dispose();
        }
    }
}