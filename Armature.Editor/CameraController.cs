using Armature.Core;
using Armature.Input;
using Armature.Scene;
using Armature.Scene.Components;
using Brutal.GlfwApi;
using Brutal.Numerics;

namespace Armature.Editor;

public class CameraController
{
    private readonly World _world;
    private readonly InputManager _inputManager;
    private readonly CameraSystem _cameraSystem;

    private float _yaw;
    private float _pitch;

    private const float Speed = 5.0f;
    private const float Sensitivity = 0.001f;
    
    public CameraController(World world, InputManager inputManager, CameraSystem cameraSystem)
    {
        _world = world;
        _inputManager = inputManager;
        _cameraSystem = cameraSystem;
        
        SetYawPitch();
    }
    
    public void Update(float deltaTime)
    {
        if (_inputManager.InputMode != InputMode.CameraControl) return;

        // Camera selection
        if (_inputManager.IsKeyPressed(GlfwKey.RightBracket))
        {
            _cameraSystem.NextCamera();
            SetYawPitch();
        }
        else if (_inputManager.IsKeyPressed(GlfwKey.LeftBracket))
        {
            _cameraSystem.PreviousCamera();
            SetYawPitch();
        }
        
        ref var transform = ref _world.GetComponent<TransformComponent>(_cameraSystem.ActiveCamera);

        // Camera movement
        var movement = float3.Zero;

        if (_inputManager.IsKeyDown(GlfwKey.W))
            movement += float3.UnitZ;
        if (_inputManager.IsKeyDown(GlfwKey.S))
            movement -= float3.UnitZ;
        if (_inputManager.IsKeyDown(GlfwKey.A))
            movement -= float3.UnitX;
        if (_inputManager.IsKeyDown(GlfwKey.D))
            movement += float3.UnitX;
        if (_inputManager.IsKeyDown(GlfwKey.Space))
            movement += float3.UnitY;
        if (_inputManager.IsKeyDown(GlfwKey.LeftControl))
            movement -= float3.UnitY;

        TransformExtensions.MoveLocal(ref transform, float3.NormalizeOrZero(movement) * deltaTime * Speed);

        // Camera rotation
        var mouseDelta = _inputManager.CursorDelta;

        _yaw -= (float)mouseDelta.X * Sensitivity;
        _pitch = float.Clamp(_pitch - (float)mouseDelta.Y * Sensitivity, float.DegreesToRadians(-89.0f), float.DegreesToRadians(89.0f));

        var yawRotation = floatQuat.CreateFromAxisAngle(float3.UnitY, _yaw);
        var pitchRotation = floatQuat.CreateFromAxisAngle(float3.UnitX, _pitch);

        transform.Rotation = yawRotation * pitchRotation;
    }

    private void SetYawPitch()
    {
        var transform = _world.GetComponent<TransformComponent>(_cameraSystem.ActiveCamera);
        var (yaw, pitch) = TransformUtility.GetYawPitch(transform.Rotation);
        _yaw = yaw;
        _pitch = pitch;
    }
}