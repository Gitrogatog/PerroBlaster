using System;
using MoonTools.ECS;
using MyGame.Components;
using MyGame.Relations;

namespace MyGame.Systems;

public class UpdateUIContainer : MoonTools.ECS.System
{
    private Filter EntityFilter;

    public UpdateUIContainer(World world) : base(world)
    {
        EntityFilter = FilterBuilder
            .Include<UIBoxContainer>()
            .Include<UpdateUIThisFrame>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in EntityFilter.Entities)
        {
            var container = Get<UIBoxContainer>(entity);
            var parentPos = Has<Position>(entity) ? Get<Position>(entity) : new Position(0, 0);
            // Console.WriteLine($"position of parent: {parentPos}");
            int x = 0;
            int y = 0;
            if (container.ExpandVertical)
            {
                foreach (var child in OutRelations<IncludeContainer>(entity))
                {
                    Set(child, new Position(x * container.XOffset, y * container.YOffset) + parentPos);
                    x++;
                    if (x >= container.MaxPerLine)
                    {
                        x = 0;
                        y++;
                    }
                }
            }
            else
            {
                foreach (var child in OutRelations<IncludeContainer>(entity))
                {
                    Set(child, new Position(x * container.XOffset, y * container.YOffset) + parentPos);
                    y++;
                    if (y >= container.MaxPerLine)
                    {
                        y = 0;
                        x++;
                    }
                }
            }

            Remove<UpdateUIThisFrame>(entity);
        }
    }
}