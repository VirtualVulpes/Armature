namespace Armature.Rendering.Meshes;

public sealed class Mesh
{
    private readonly Vertex[] _vertices;
    public ReadOnlySpan<Vertex> Vertices => _vertices;

    private readonly uint[] _indices;
    public ReadOnlySpan<uint> Indices => _indices;

    public Mesh(Vertex[] vertices, uint[] indices)
    {
        _vertices = (Vertex[])vertices.Clone();
        _indices = (uint[])indices.Clone();
    }
}