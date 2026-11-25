using Armature.Scene.Components;
using Brutal.Numerics;

namespace Armature.Scene;

public interface ISceneDef;

public sealed record EntityDef(string Name, IComponentDef[] Components, EntityDef[]? Children = null) : ISceneDef
{
    public EntityDef[] Children { get; } = Children ?? [];
}

public sealed record SceneDef(string Name, IReadOnlyList<ISceneDef> Definitions);

public interface IComponentDef
{
    void Apply(World world, Entity entity, IAssetResolver assets);
}

public sealed record TransformDef : IComponentDef
{
    public float3 Position = float3.Zero;
    public floatQuat Rotation = floatQuat.Identity;
    public float3 Scale = float3.One;

    public static TransformDef Identity()
    {
        return new TransformDef
        {
            Position = float3.Zero,
            Rotation = floatQuat.Identity,
            Scale = float3.One
        };
    }

    public void Apply(World world, Entity entity, IAssetResolver assets)
    {
        world.AddComponent(entity, new TransformComponent { Position = Position, Rotation = Rotation, Scale = Scale });
    }
}

public sealed record MeshDef(string Name) : IComponentDef
{
    public void Apply(World world, Entity entity, IAssetResolver assets)
    {
        world.AddComponent(entity, new MeshComponent { MeshId = assets.Mesh(Name) });
    }
}

public sealed record TextureDef(string Name) : IComponentDef
{
    public void Apply(World world, Entity entity, IAssetResolver assets)
    {
        world.AddComponent(entity, new TextureComponent { TextureId = assets.Texture(Name) });
    }
}

public sealed record PipelineDef(string Name) : IComponentDef
{
    public void Apply(World world, Entity entity, IAssetResolver assets)
    {
        world.AddComponent(entity, new PipelineComponent { PipelineId = assets.Pipeline(Name) });
    }
}

public sealed record RotateDef : IComponentDef
{
    public float3 LocalAngularVelocity = float3.Zero;
    public float3 WorldAngularVelocity = float3.Zero;

    public void Apply(World world, Entity entity, IAssetResolver assets)
    {
        world.AddComponent(entity,
            new RotateComponent
                { LocalAngularVelocity = LocalAngularVelocity, WorldAngularVelocity = WorldAngularVelocity });
    }
}

public sealed record ColorDef(float3 Color) : IComponentDef
{
    public void Apply(World world, Entity entity, IAssetResolver assets)
    {
        world.AddComponent(entity, new ColorComponent { Color = Color });
    }
}

public sealed record LightDef : IComponentDef
{
    public LightType Type;
    public float3 Color;
    public float Intensity;
    public float Range = 0.0f;
    public float3 Direction = float3.Zero;
    public float InnerCos = 0.0f;
    public float OuterCos = 0.0f;

    public void Apply(World world, Entity entity, IAssetResolver assets)
    {
        world.AddComponent(entity,
            new LightComponent
            {
                Color = Color, Intensity = Intensity, Range = Range, Direction = Direction, Type = Type,
                InnerCos = InnerCos, OuterCos = OuterCos
            });
    }
}

public sealed record CameraDef : IComponentDef
{
    public float AspectRatio = 16.0f / 9.0f;
    public float Fov = float.DegreesToRadians(60.0f);
    public float ZNear = 0.1f;
    public float ZFar = 100.0f;
 
    public void Apply(World world, Entity entity, IAssetResolver assets)
    {
        world.AddComponent(entity,
            new CameraComponent { AspectRatio = AspectRatio, Fov = Fov, ZNear = ZNear, ZFar = ZFar });
    }
}