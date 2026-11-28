using Armature.Scene;

namespace Armature.Editor;

public interface IComponentDrawer
{
    public string Name { get; }
    public bool Has(World world, Entity e);
    public void Add(World world, Entity e);
    public void Draw(World world, Entity entity);
}