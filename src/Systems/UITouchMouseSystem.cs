using System;
using MoonTools.ECS;
using MoonWorks.Input;
using MyGame.Components;
using MyGame.Relations;

namespace MyGame.Systems;

public class UITouchMouseSystem : MoonTools.ECS.System
{
    private Filter EntityFilter;
    private Filter TouchingFilter;
    Inputs input;
    public UITouchMouseSystem(World world, Inputs input) : base(world)
    {
        this.input = input;
        EntityFilter = FilterBuilder
            .Include<Position>()
            .Include<Rectangle>()
            .Build();
        TouchingFilter = FilterBuilder.Include<TouchingMouse>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in TouchingFilter.Entities)
        {
            Remove<TouchingMouse>(entity);
        }
        int x = GlobalInput.MouseX;
        int y = GlobalInput.MouseY;
        foreach (var entity in EntityFilter.Entities)
        {
            // Console.WriteLine("running motion");
            var rect = Get<Rectangle>(entity);
            var position = Get<Position>(entity);
            if (OverlapPoint(rect, position.X, position.Y, x, y))
            {
                // Console.WriteLine("touching mouse");
                Set(entity, new TouchingMouse());
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