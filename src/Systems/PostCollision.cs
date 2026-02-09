using System;
using MoonTools.ECS;
using MyGame.Components;
using MyGame.Relations;
namespace MyGame.Systems;

public class PostCollision : MoonTools.ECS.System
{
    private Filter EntityFilter;

    public PostCollision(World world) : base(world)
    {
        EntityFilter = FilterBuilder
            .Include<BecomeSolidWhenNotColliding>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in EntityFilter.Entities)
        {
            if(!HasInRelation<Colliding>(entity) && !HasOutRelation<Collision>(entity)) {
                Remove<BecomeSolidWhenNotColliding>(entity);
                // Console.WriteLine("no overlap!");
                Set(entity, new Solid());
            }
            else {
                // Console.WriteLine("does overlap");
            }
        }
    }
}
