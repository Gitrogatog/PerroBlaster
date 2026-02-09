
using System;
using MoonTools.ECS;
using MoonWorks.Graphics;
using MyGame.Components;
using MyGame.Content;
using MyGame.Relations;
namespace MyGame.Systems;

public class HealthBarSystem : MoonTools.ECS.System
{
    private MoonTools.ECS.Filter EntityFilter;
    private MoonTools.ECS.Filter InitFilter;

    public HealthBarSystem(World world) : base(world)
    {
        InitFilter = FilterBuilder.Include<Health>().Include<CreateHealthUI>().Build();
        EntityFilter = FilterBuilder
            .Include<Health>()
            .Include<DisplayHealthUI>()
            .Include<TookDamageThisFrame>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach(var entity in InitFilter.Entities){
            Console.WriteLine(
                "creating healthui!"
            );
            (int x, int y) = Get<CreateHealthUI>(entity);
            Remove<CreateHealthUI>(entity);
            Set(entity, new DisplayHealthUI(x, y));
            CreateHealthUI(entity);
        }
        foreach (var entity in EntityFilter.Entities)
        {
            // foreach(var ui in OutRelations<HealthUI>(entity)){
            //     Destroy(ui);
            // }
            foreach(var ui in InRelations<HealthUI>(entity)){
                Destroy(ui);
            }
            CreateHealthUI(entity);
        }
    }
    void CreateHealthUI(Entity entity){
        (int current, int max) = Get<Health>(entity);
        (int x, int y) = Get<DisplayHealthUI>(entity);
        for(int i = 0; i < current; i++){
            var ui = CreateUIEntity(x, y, entity);
            Set(ui, SpriteAnimations.Heart.Frames[0]);
            x += 20;
        }
        for(int i = current; i < max; i++){
            var ui = CreateUIEntity(x, y, entity);
            Set(ui, SpriteAnimations.Heart.Frames[1]);
            x += 20;
        }
    }
    Entity CreateUIEntity(int x, int y, Entity target){
        var entity = CreateEntity();
        Set(entity, new DestroyOnPlayerRespawn());
        Set(entity, new FollowCameraWithOffset(x, y));
        Relate(entity, target, new HealthUI());
        Set(entity, new Depth(-10));
        Set(entity, new ColorOverlay(Color.Black));
        return entity;
    }
}
