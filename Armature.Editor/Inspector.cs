using Armature.Scene;
using Armature.Scene.Components;
using Brutal.ImGuiApi;
using Brutal.Numerics;

namespace Armature.Editor;

public sealed class Inspector : IEditorWindow
{
    private readonly World _world;
    private readonly SelectionHelper _selection;
    private Entity _current;

    private readonly Dictionary<Type, IComponentDrawer> _drawers = new();

    private readonly ImInputString _nameBuffer = new(128);
    private Entity? _bufferEntity;

    public Inspector(World world, SelectionHelper selection)
    {
        _world = world;
        _selection = selection;

        Register<NameComponent>("Name", DrawName, () => new NameComponent { Name = "New Entity" });
        Register<TransformComponent>("Transform", DrawTransform, () => TransformComponent.Identity);
        Register<RotateComponent>("Rotation", DrawRotate);
        Register<ColorComponent>("Color", DrawColor);
        Register<LightComponent>("Light", DrawLight);
        Register<CameraComponent>("Camera", DrawCamera);
        Register<MeshComponent>("Mesh", DrawMesh);
        Register<TextureComponent>("Texture", DrawTexture);
        Register<PipelineComponent>("Pipeline", DrawPipeline);
    }

    private void Register<T>(string name, RefAction<T> draw, Func<T>? create = null) where T : struct
    {
        _drawers[typeof(T)] = new ComponentDrawer<T>(name, draw, create);
    }

    public void Draw()
    {
        var e = _selection.GetActive();

        ImGui.Begin("Inspector"u8);

        if (_world.IsEntityAlive(e))
        {
            _current = e;
            
            ImGui.PushID((int)e.Id.Value);
            foreach (var drawer in _drawers.Values)
                drawer.Draw(_world, e);

            ImGui.Separator();

            if (ImGui.Button("Add Component"u8, new float2(-1.0f, 0.0f)))
                ImGui.OpenPopup("AddComponent"u8);

            if (ImGui.BeginPopup("AddComponent"u8))
            {
                var any = false;
                foreach (var drawer in _drawers.Values)
                {
                    if (drawer.Has(_world, e)) continue;
                    any = true;

                    if (ImGui.MenuItem(drawer.Name))
                        drawer.Add(_world, e);
                }

                if (!any)
                    ImGui.TextDisabled("All components added"u8);

                ImGui.EndPopup();
            }

            ImGui.PopID();
        }
        else
        {
            _bufferEntity = null;
        }

        ImGui.End();
    }

    private static bool EnumCombo<T>(string label, ref T value) where T : struct, Enum
    {
        var changed = false;

        if (ImGui.BeginCombo(label, value.ToString()))
        {
            foreach (var item in Enum.GetValues<T>())
            {
                var selected = EqualityComparer<T>.Default.Equals(value, item);

                if (ImGui.Selectable(item.ToString(), selected))
                {
                    value = item;
                    changed = true;
                }

                if (selected)
                    ImGui.SetItemDefaultFocus();
            }

            ImGui.EndCombo();
        }

        return changed;
    }

    private void DrawName(ref NameComponent c)
    {
        if (_bufferEntity != _current)
        {
            var name = c.Name;
            if (name.Length > 60) name = name[..60];
            _nameBuffer.SetValue(name);
            _bufferEntity = _current;
        }

        ImGui.SetNextItemWidth(-1);
        if(ImGui.InputText("##Name"u8, _nameBuffer))
        {
            _nameBuffer.EvaluateLength();
            var text = _nameBuffer.ToString();
            if(!text.IsWhiteSpace())
                c.Name = text;
        }

        if (ImGui.IsItemDeactivatedAfterEdit() && _nameBuffer.ToString().IsWhiteSpace())
        {
            _nameBuffer.SetValue(c.Name);
        }
    }

    private void DrawTransform(ref TransformComponent c)
    {
        ImGui.DragFloat3("Position"u8, ref c.Position, 0.05f);
        ImGui.DragFloat3("Scale"u8, ref c.Scale, 0.01f);
    }

    private void DrawRotate(ref RotateComponent c)
    {
        ImGui.DragFloat3("Local"u8, ref c.LocalAngularVelocity, 0.05f);
        ImGui.DragFloat3("World"u8, ref c.WorldAngularVelocity, 0.05f);
    }

    private void DrawColor(ref ColorComponent c)
    {
        ImGui.DragFloat3("Color"u8, ref c.Color, 0.01f, 0.0f, 1.0f);
    }

    private void DrawLight(ref LightComponent c)
    {
        EnumCombo("Type", ref c.Type);

        ImGui.DragFloat3("Color"u8, ref c.Color, 0.01f, 0.0f, 1.0f);
        ImGui.DragFloat("Intensity"u8, ref c.Intensity, 0.1f, 0.0f, 1000.0f);

        switch (c.Type)
        {
            case LightType.Point:
                ImGui.DragFloat("Range"u8, ref c.Range, 0.1f);
                break;
            case LightType.Directional:
                ImGui.DragFloat3("Direction"u8, ref c.Direction, 0.01f);
                break;
            case LightType.Spot:
                ImGui.DragFloat("Range"u8, ref c.Range, 0.1f);
                ImGui.DragFloat3("Direction"u8, ref c.Direction, 0.01f);
                ImGui.DragFloat("Inner Cos"u8, ref c.InnerCos, 0.01f);
                ImGui.DragFloat("Outer Cos"u8, ref c.OuterCos, 0.01f);
                break;
            default:
                throw new ArgumentOutOfRangeException($"Invalid light type: {c.Type}");
        }
    }

    private void DrawCamera(ref CameraComponent c)
    {
        ImGui.Text($"Aspect Ratio {c.AspectRatio}");
        ImGui.Text($"FOV {float.RadiansToDegrees(c.Fov)}");
        ImGui.Text($"Z Near {c.ZNear}");
        ImGui.Text($"Z Far {c.ZFar}");
    }

    private void DrawMesh(ref MeshComponent c)
    {
        ImGui.Text($"Id {c.MeshId.Value}");
    }

    private void DrawTexture(ref TextureComponent c)
    {
        ImGui.Text($"Id {c.TextureId.Value}");
    }

    private void DrawPipeline(ref PipelineComponent c)
    {
        ImGui.Text($"Id {c.PipelineId.Value}");
    }
}