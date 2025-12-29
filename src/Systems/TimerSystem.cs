namespace MyGame.Systems;

using System;
using MoonTools.ECS;
using MyGame.Components;
using MyGame.Spawn;

public class TimerSystem : MoonTools.ECS.System
{
    public Filter TimerFilter;

    public TimerSystem(World world) : base(world)
    {
        TimerFilter = FilterBuilder
                        .Include<Timer>()
                        .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in TimerFilter.Entities)
        {
            var timer = Get<Timer>(entity);
            var t = timer.Time - (float)delta.TotalSeconds;

            if (t <= 0.0f)
            {
                if (Has<SpawnOnTimerEnd>(entity) && Has<Position>(entity))
                {
                    ThingType thing = Get<SpawnOnTimerEnd>(entity).Thing;
                    Position pos = Get<Position>(entity);
                    // EntityPrefabs.CreateThing(thing, pos.X, pos.Y, entity);
                }
                if (Has<CreateAnimationEntityOnTimerEnd>(entity))
                {
                    var data = Get<CreateAnimationEntityOnTimerEnd>(entity);
                    Entity animEntity = CreateEntity();
                    Set(animEntity, data.Animation);
                    Set(animEntity, new DestroyOnLoad());
                    if (data.DeleteOnFinish)
                    {
                        Set(animEntity, new DestroyOnAnimationFinish());
                    }
                    if (data.RelativeToSpawner && Has<Position>(entity))
                    {
                        var pos = Get<Position>(entity);
                        Set(animEntity, new Position(pos.X + data.X, pos.Y + data.Y));
                    }
                    else
                    {
                        Set(animEntity, new Position(data.X, data.Y));
                    }
                }
                Destroy(entity);
            }

            else
                Set(entity, new Timer(t));
        }
    }
}