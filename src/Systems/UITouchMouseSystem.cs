using System;
using System.Collections.Generic;
using MoonTools.ECS;
using MoonWorks.Input;
using MyGame.Components;
using MyGame.Relations;

namespace MyGame.Systems;

public class UITouchMouseSystem : MoonTools.ECS.System
{
    private Filter EntityFilter;
    public UITouchMouseSystem(World world) : base(world)
    {
        EntityFilter = FilterBuilder
            .Include<Position>()
            .Include<Rectangle>()
            .Include<IsTouchableUI>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        // foreach (var entity in TouchingFilter.Entities)
        // {
        //     Remove<TouchingMouse>(entity);
        // }
        int x = GlobalInput.MouseX;
        int y = GlobalInput.MouseY;
        Entity touchingEntity = default;
        float minDepth = 1000;
        bool hasFoundEntity = false;
        foreach (var entity in EntityFilter.Entities)
        {
            // Console.WriteLine("running motion");
            var rect = Get<Rectangle>(entity);
            var position = Get<Position>(entity);
            if (OverlapPoint(rect, position.X, position.Y, x, y))
            {
                // Console.WriteLine("touching mouse");
                // Set(entity, new TouchingMouse());
                float depth = Has<Depth>(entity) ? Get<Depth>(entity).Value : 1;
                if(depth < minDepth) {
                    hasFoundEntity = true;
                    minDepth = depth;
                    touchingEntity = entity;
                }
            }
        }
        if(hasFoundEntity) {
            if(Some<TouchingMouse>()) {
                var prevEntity = GetSingletonEntity<TouchingMouse>();
                if(prevEntity != touchingEntity) { // mouse touching different entity
                    Remove<TouchingMouse>(prevEntity);
                    Set(prevEntity, new MouseExit());
                    Set(touchingEntity, new MouseEnter());
                    Set(touchingEntity, new TouchingMouse());
                }
            }
            else { // mouse wasnt touching any entity
                Set(touchingEntity, new MouseEnter());
                Set(touchingEntity, new TouchingMouse());
            }
        }
        else {
            if(Some<TouchingMouse>()) {
                touchingEntity = GetSingletonEntity<TouchingMouse>();
                Remove<TouchingMouse>(touchingEntity);
                Set(touchingEntity, new MouseExit());
            }
        }
    }
    bool OverlapPoint(Rectangle rect, int x, int y, int xPoint, int yPoint)
    {
        int startX = rect.X + x;
        int startY = rect.Y + y;
        int endX = startX + rect.Width;
        int endY = startY + rect.Height;
        // Console.WriteLine($"rect:{startX},{endX},{startY},{endY}");
        return startX <= xPoint && endX >= xPoint && startY <= yPoint && endY >= yPoint;
    }
}