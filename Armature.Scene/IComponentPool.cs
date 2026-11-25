namespace Armature.Scene;

public interface IComponentPool
{
    bool Remove(Entity entity);
    bool Has(Entity entity);
    int Count { get; }
    Entity GetEntityAt(int index);
}