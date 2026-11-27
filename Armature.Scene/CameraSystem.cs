using Armature.Scene.Components;
using Armature.Scene.Systems;
using Brutal.Numerics;

namespace Armature.Scene;

public class CameraSystem : ISystem
{
    private readonly World _world;
    private Entity _active = Entity.Invalid;

    public CameraSystem(World world)
    {
        _world = world;
    }

    public Entity ActiveCamera
    {
        get
        {
            Resolve();
            return _active;
        }
    }
    
    public void SetActiveCamera(Entity entity)
    {
        if (!_world.GetPool<CameraComponent>().Has(entity))
            throw new ArgumentException("Entity has no CameraComponent");
        _active = entity;
    }

    public void NextCamera() => Step(1);
    public void PreviousCamera() => Step(-1);

    private void Step(int direction)
    {
        Resolve();
        var pool = _world.GetPool<CameraComponent>();
        if (pool.Count == 0) return;
        pool.TryGetIndex(_active, out var index);
        _active = pool.GetEntityAt((index + direction + pool.Count) % pool.Count);
    }

    public CameraData GetActiveCameraData(int2 viewport)
    {
        Resolve();
        if (_active == Entity.Invalid)
            throw new InvalidOperationException("No active camera");

        var transform = _world.GetComponent<TransformComponent>(ActiveCamera);
        var camera = _world.GetComponent<CameraComponent>(ActiveCamera);

        if (viewport.X != 0 && viewport.Y != 0) 
            camera.AspectRatio = (float)viewport.X / viewport.Y;

        var view = float4x4.CreateLookAt(transform.Position, transform.Position + TransformExtensions.Forward(transform),
            float3.UnitY);
        var proj = float4x4.CreatePerspectiveFieldOfView(camera.Fov, camera.AspectRatio, camera.ZNear, camera.ZFar);

        return new CameraData
        {
            View = view,
            Proj = proj,
            Position = transform.Position,
            ViewportSize = viewport
        };
    }

    public void Start()
    {
    }

    public void Update(float deltaTime)
    {
    }
    
    private void Resolve()
    {
        var pool = _world.GetPool<CameraComponent>();
        if (_active != Entity.Invalid && pool.Has(_active)) return;

        if (pool.Count == 0)
            throw new InvalidOperationException("No camera in scene");

        _active = pool.GetEntityAt(0);
    }
}