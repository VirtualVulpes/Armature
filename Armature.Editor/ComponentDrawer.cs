using Armature.Scene;
using Brutal.ImGuiApi;

namespace Armature.Editor;

public delegate void RefAction<T>(ref T value);

public sealed class ComponentDrawer<T> : IComponentDrawer where T : struct
{
    private readonly RefAction<T> _draw;
    private readonly Func<T> _create;
    
    public string Name { get; }

    public ComponentDrawer(string name, RefAction<T> draw, Func<T>? create = null)
    {
        Name = name;
        _draw = draw;
        _create = create ?? (() => default);
    }

    public bool Has(World world, Entity e) => world.HasComponent<T>(e);

    public void Add(World world, Entity e) => world.AddComponent(e, _create());
    
    public void Draw(World world, Entity entity)
    {
        if (!world.HasComponent<T>(entity)) return;

        ImGui.PushID(typeof(T).Name);
        var open = ImGui.CollapsingHeader(Name, ImGuiTreeNodeFlags.DefaultOpen);

        var removed = false;

        if (ImGui.BeginPopupContextItem("ComponentContext"u8))
        {
            if (ImGui.MenuItem("Remove Component"u8))
            {
                world.TryRemoveComponent<T>(entity);
                removed = true;
            }

            ImGui.EndPopup();
        }

        if (open && !removed)
        {
            ref var component = ref world.GetComponent<T>(entity);
            _draw(ref component);
        }

        ImGui.PopID();
    }
}