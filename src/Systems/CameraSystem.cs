using System;
using MoonTools.ECS;
using MyGame.Components;
namespace MyGame.Systems;

public class CameraSystem : MoonTools.ECS.System
{
    Filter FollowCameraFilter;
    public CameraSystem(World world) : base(world)
    {
        FollowCameraFilter = FilterBuilder.Include<FollowCameraWithOffset>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        // Console.WriteLine(Some<CameraFollow>());
        if(Some<LockCamera>()){
            Globals.CameraX = GetSingleton<LockCamera>().X;
        }
        else if(Some<CameraFollow>() && TryGet<Position>(GetSingletonEntity<CameraFollow>(), out Position position))
        {
            Globals.CameraX = Math.Clamp(position.X + Globals.CameraXOffset, Globals.CameraMinX, Globals.CameraMaxX);
            Globals.CameraY = Math.Clamp(position.Y + Globals.CameraYOffset, Globals.CameraMinY, Globals.CameraMaxY);
            // Console.WriteLine($"camera x: {Globals.CameraX}");
        }
        foreach(var entity in FollowCameraFilter.Entities){
            (int x, int y) = Get<FollowCameraWithOffset>(entity);
            Set(entity, new Position(Globals.CameraX + x, Globals.CameraY + y));
        }
    }
}
