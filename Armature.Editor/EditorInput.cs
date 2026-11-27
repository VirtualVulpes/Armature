using Armature.Input;
using Armature.Scene;
using Brutal.GlfwApi;
using Brutal.Numerics;

namespace Armature.Editor;

public sealed class EditorInput
{
    private readonly InputManager _input;
    private readonly World _world;
    private readonly EntityPicker _picker;
    private readonly SelectionHelper _selection;
    private readonly ViewportState _viewport;

    public EditorInput(InputManager input, World world, EntityPicker picker,
        SelectionHelper selection, ViewportState viewport)
    {
        _input = input;
        _world = world;
        _picker = picker;
        _selection = selection;
        _viewport = viewport;
    }

    public void Update()
    {
        var wantKeyboard = _input.IsUiCapturingKeyboard();

        var overViewport = _viewport.IsValid && _viewport.IsHovered;
        var canPick = _input.InputMode == InputMode.UiInteraction && overViewport;
        _picker.Request(canPick ? _viewport.MouseUv : null);

        switch (_input.InputMode)
        {
            case InputMode.CameraControl when _input.IsKeyPressed(GlfwKey.Tab):
                _input.SetInputMode(InputMode.UiInteraction);
                break;
            case InputMode.UiInteraction:
                if (!wantKeyboard)
                {
                    if (_input.IsKeyPressed(GlfwKey.Tab))
                    {
                        _input.SetInputMode(InputMode.CameraControl);
                        break;
                    }

                    if (_input.IsKeyPressed(GlfwKey.Delete))
                    {
                        _world.DestroyEntities(_selection.GetSelected());
                        _picker.Reset();
                        break;
                    }
                }

                var hovered = GetHoveredEntity();

                if (overViewport && _input.IsMouseButtonPressed(GlfwMouseButton.Left))
                    _selection.Click(hovered, _input.IsKeyDown(GlfwKey.LeftShift));

                break;
        }
    }

    private Entity? GetHoveredEntity() =>
        _picker.HoveredObjectId is { } id
            ? new Entity { Id = new EntityId(id & 0xFFFF), Generation = id >> 16 }
            : null;
}