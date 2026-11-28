using Armature.Input;
using Armature.Scene;
using Armature.Scene.Components;
using Brutal.GlfwApi;
using Brutal.ImGuiApi;
using Brutal.Numerics;

namespace Armature.Editor;

public sealed class SceneView : IEditorWindow
{
    private readonly World _world;
    private readonly SelectionHelper _selection;
    private readonly InputManager _input;

    private readonly Dictionary<Entity, List<Entity>> _children = new();
    private readonly List<Entity> _roots = [];
    private Entity _active;
    private Entity? _clicked;
    private bool _deselect;
    private (Entity child, Entity parent, bool has) _pendingReparent;
    private Entity? _pendingCreateChild;
    private Entity? _pendingDelete;

    public SceneView(World world, SelectionHelper selection, InputManager input)
    {
        _world = world;
        _selection = selection;
        _input = input;
    }

    public void Draw()
    {
        _active = _selection.GetActive();
        _clicked = null;
        _pendingReparent = default;
        _pendingCreateChild = null;
        _pendingDelete = null;

        BuildHierarchy();

        ImGui.Begin("Scene"u8);

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new float2(ImGui.GetStyle().ItemSpacing.X, 0.0f));
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new float2(4.0f, 2.0f));
        foreach (var root in _roots)
            DrawNode(root);
        ImGui.PopStyleVar(2);

        var rest = ImGui.GetContentRegionAvail();
        rest.Y = MathF.Max(rest.Y, 24.0f);
        if (ImGui.InvisibleButton("SceneBlank"u8, rest) && !_input.IsKeyDown(GlfwKey.LeftShift))
        {
            _deselect = true;
        }

        if (ImGui.BeginPopupContextItem("SceneContext"u8))
        {
            if (ImGui.MenuItem("New Entity"u8))
            {
                var e = _world.CreateEntity();
                _selection.Click(e, false);
            }

            ImGui.EndPopup();
        }

        ImGui.End();

        if (_deselect)
        {
            _selection.Clear();
            _deselect = false;
        }
        else if (_clicked is { } c)
        {
            _selection.Click(c, _input.IsKeyDown(GlfwKey.LeftShift));
        }

        if (_pendingReparent.has)
            HierarchyExtensions.AddParent(_world, _pendingReparent.child, _pendingReparent.parent);

        if (_pendingCreateChild is { } parent)
        {
            var child = _world.CreateEntity();
            HierarchyExtensions.AddParent(_world, child, parent);
            _selection.Click(child, false);
        }

        if (_pendingDelete is { } doomed)
        {
            _world.DestroyEntity(doomed);
        }
    }

    private void BuildHierarchy()
    {
        _children.Clear();
        _roots.Clear();

        foreach (var item in _world.Query<ParentComponent>())
        {
            var parent = item.C1.Parent;
            if (!_children.TryGetValue(parent, out var list))
                _children[parent] = list = [];
            list.Add(item.Entity);
        }

        foreach (var entity in _world.AliveEntities)
        {
            if (!_world.HasComponent<ParentComponent>(entity))
                _roots.Add(entity);
        }
    }

    private void DrawNode(Entity entity)
    {
        _children.TryGetValue(entity, out var kids);
        var hasKids = kids is { Count: > 0 };

        var flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanAvailWidth |
                    ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.FramePadding;
        if (!hasKids) flags |= ImGuiTreeNodeFlags.Leaf;
        if (_world.HasComponent<SelectionComponent>(entity)) flags |= ImGuiTreeNodeFlags.Selected;

        var name = GetName(entity);

        ImGui.PushID((int)entity.Id.Value);

        var isActive = entity == _active;
        if (isActive) ImGui.PushStyleColor(ImGuiCol.Header, new float4(0.18f, 0.18f, 0.18f, 1.0f));

        var open = ImGui.TreeNodeEx(name, flags);

        if (isActive)
            ImGui.PopStyleColor();

        if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
            _clicked = entity;

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenBlockedByPopup) &&
            ImGui.IsMouseClicked(ImGuiMouseButton.Right))
            _clicked = entity;

        if (ImGui.BeginPopupContextItem("NodeContext"u8))
        {
            if (ImGui.MenuItem("Add Child"u8))
                _pendingCreateChild = entity;

            if (ImGui.MenuItem("Delete"u8))
                _pendingDelete = entity;

            ImGui.EndPopup();
        }

        if (open)
        {
            if (hasKids)
                foreach (var child in kids!)
                    DrawNode(child);

            ImGui.TreePop();
        }

        ImGui.PopID();
    }

    private string GetName(Entity entity)
    {
        return _world.HasComponent<NameComponent>(entity)
            ? _world.GetComponent<NameComponent>(entity).Name
            : $"New Entity";
    }
}