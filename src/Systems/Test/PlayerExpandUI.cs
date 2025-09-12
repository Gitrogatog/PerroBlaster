using System;
using MoonTools.ECS;
using MyGame.Components;
using MyGame.Relations;

namespace MyGame.Systems;

public class PlayerExpandUI : MoonTools.ECS.System
{
    private Filter EntityFilter;

    public PlayerExpandUI(World world) : base(world)
    {
        EntityFilter = FilterBuilder
            .Include<Position>()
            .Include<Rectangle>()
            .Include<InputState>()
            .Include<Player>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in EntityFilter.Entities)
        {
            var rect = Get<Rectangle>(entity);
            var input = Get<InputState>(entity);
            int width = rect.Width;
            int height = rect.Height;
            if (input.Left.IsDown)
            {
                width--;
            }
            if (input.Right.IsDown)
            {
                width++;
            }
            if (input.Up.IsDown)
            {
                height--;
            }
            if (input.Down.IsDown)
            {
                height++;
            }
            Set(entity, new Rectangle(rect.X, rect.Y, width, height));
        }
    }
}
