using Armature.Scene;
using Xunit;

namespace Armature.Tests;

public class WorldTests
{
    [Fact]
    public void DestroyedEntity_IsNotAlive_AndStaleHandleStaysDead()
    {
        var world = new World();
        var e = world.CreateEntity();
        
        world.DestroyEntities([e]);
        Assert.False(world.IsEntityAlive(e));

        var reused = world.CreateEntity();
        Assert.False(world.IsEntityAlive(e));
        Assert.True(world.IsEntityAlive(reused));
    }
}