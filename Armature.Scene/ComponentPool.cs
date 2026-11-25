namespace Armature.Scene;

public sealed class ComponentPool<T> : IComponentPool where T : struct
{
    private int[] _sparse = [];
    private Entity[] _entities = [];
    private T[] _components = [];

    public int Count { get; private set; }
    public Entity GetEntityAt(int index) => _entities[index];

    public ref T GetAt(int index) => ref _components[index];

    public bool TryGetIndex(Entity entity, out int index)
    {
        var id = entity.Id.Value;
        index = -1;

        if (id >= (uint)_sparse.Length) return false;

        index = _sparse[id];
        return index >= 0 && index < Count && _entities[index] == entity;
    }

    public void Add(Entity entity, T component)
    {
        if (Has(entity)) 
            throw new InvalidOperationException($"{entity} already has this component");

        EnsureSparseCapacity(entity);
        EnsureDenseCapacity();

        var index = Count++;
        
        _sparse[(int)entity.Id.Value] = index;
        _entities[index] = entity;
        _components[index] = component;
    }

    public bool Remove(Entity entity)
    {
        if (!Has(entity)) return false;
        
        var index = _sparse[(int)entity.Id.Value];
        var lastIndex = --Count;

        _sparse[(int)entity.Id.Value] = -1;

        if (index != lastIndex)
        {
            var lastEntity = _entities[lastIndex];
            
            _sparse[(int)lastEntity.Id.Value] = index;

            _entities[index] = lastEntity;
            _components[index] = _components[lastIndex];
        }

        _entities[lastIndex] = default;
        _components[lastIndex] = default;

        return true;
    }

    public bool Has(Entity entity)
    {
        var id = entity.Id.Value;

        if (id >= (uint)_sparse.Length) return false;

        var index = _sparse[id];
        
        return index >= 0 && index < Count && _entities[index] == entity;
    }

    public ref T Get(Entity entity)
    {
        if (!Has(entity))
            throw new InvalidOperationException($"{entity} does not have this component");
        
        var index = _sparse[(int)entity.Id.Value];
        return ref _components[index];
    }

    private void EnsureSparseCapacity(Entity entity)
    {
        var required = (int)entity.Id.Value + 1;
        
        if (required <= _sparse.Length) return;

        var oldCapacity = _sparse.Length;
        var capacity = Math.Max(16, Math.Max(required, oldCapacity * 2));

        Array.Resize(ref _sparse, capacity);
        Array.Fill(_sparse, -1, oldCapacity, capacity - oldCapacity);
    }
    
    private void EnsureDenseCapacity()
    {
        if (Count < _components.Length) return;
        
        var capacity = Math.Max(16, _components.Length * 2);

        Array.Resize(ref _components, capacity);
        Array.Resize(ref _entities, capacity);
    }
}