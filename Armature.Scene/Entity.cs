namespace Armature.Scene;

public readonly record struct EntityId(uint Value)
{
    public static readonly EntityId Invalid = new(uint.MaxValue);
}

public readonly record struct Entity(EntityId Id, uint Generation)
{
    public static readonly Entity Invalid = new Entity
    {
        Id = EntityId.Invalid,
        Generation = 0
    };

    public override string ToString() => $"Entity({Id.Value},{Generation})";
    
}