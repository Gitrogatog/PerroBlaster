using System;
using MoonTools.ECS;
using MyGame.Components;
using MyGame.Relations;
using MyGame.Utility;
namespace MyGame.Systems;

public class ThrowSystem : MoonTools.ECS.System
{
    private Filter EntityFilter;
    static float ThrowTime => 1f;
    static float THROW_DISTANCE => 20f;

    public ThrowSystem(World world) : base(world)
    {
        EntityFilter = FilterBuilder
            .Include<Position>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach ((var entityA, var entityB) in Relations<Throwing>())
        {
            float progress = GetRelationData<Throwing>(entityA, entityB).Progress;
            if (progress < 1f)
            {
                progress = (float)delta.TotalSeconds * ThrowTime + progress;
                Relate(entityA, entityB, new Throwing(progress));
                if (progress >= 1f)
                {
                    progress = 1f;
                    Set(entityA, new CollidesWithSolids());
                    Remove<IgnoreCollision>(entityA);
                    Relate(entityA, entityB, new Throwing(progress));
                    Console.WriteLine("Reached midpoint of throw");
                }
                Relate(entityA, entityB, new Offset(EntityUtils.FacingMult(World, entityA, MathF.Cos(progress * 2 * MathF.PI)) * THROW_DISTANCE, MathF.Sin(progress * 2 * MathF.PI) * THROW_DISTANCE));
            }
        }
    }
}