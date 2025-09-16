
using System;
using System.Numerics;
using MoonTools.ECS;
using MyGame.Components;
namespace MyGame.Systems;

public class ProximityToPlayerSystem : MoonTools.ECS.System
{
    private Filter RushFilter;
    static int XDistance => 25;
    static int BackYDistance = 20;
    static int YDistance => 130;
    static float RushSpeed => 400;

    public ProximityToPlayerSystem(World world) : base(world)
    {
        RushFilter = FilterBuilder
            .Include<Position>()
            .Include<RushAtPlayer>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        if (!Some<ControlledByPlayer>()) return;
        foreach (var entity in RushFilter.Entities)
        {
            var position = Get<Position>(entity);
            var direction = Get<RushAtPlayer>(entity).Direction;
            if (IsWithinBounds(position.X, position.Y, direction))
            {
                Set(entity, new Velocity(ConvertEnum.CardinalToVec(direction) * RushSpeed));
                Remove<RushAtPlayer>(entity);
            }
            // int longwayDiff = (direction & Cardinal.Vertical) != 0 ? Globals.PlayerY - position.Y : Globals.PlayerX - position.X;
            // if (longwayDiff > 0 == ((direction & Cardinal.Positive) == 0)) // if the player is NOT to the right of a positive direction or NOT to the left of a negative direction, then we skip
            // {
            //     continue;
            // }
            // int shortwayDiff = (direction & Cardinal.Vertical) != 0 ? Globals.PlayerY - position.Y : Globals.PlayerX - position.X;

            // if (Math.Abs(longwayDiff) < XDistance && Math.Abs(shortwayDiff) < YDistance)
            // {
            //     Set(entity, new Velocity(Math.Sign(Globals.PlayerX - position.X) * RushSpeed, 0));
            //     Remove<RushAtPlayer>(entity);
            // }
        }
    }
    static bool IsWithinBounds(int x, int y, Cardinal direction)
    {
        int xDiff = Globals.PlayerX - x;
        int yDiff = Globals.PlayerY - y;
        return direction switch
        {
            Cardinal.Left => xDiff < BackYDistance && xDiff > -YDistance && Math.Abs(yDiff) < XDistance,
            Cardinal.Right => xDiff > -BackYDistance && xDiff < YDistance && Math.Abs(yDiff) < XDistance,
            Cardinal.Up => yDiff < BackYDistance && yDiff > -YDistance && Math.Abs(xDiff) < XDistance,
            Cardinal.Down => yDiff > -BackYDistance && yDiff < YDistance && Math.Abs(xDiff) < XDistance,
            _ => false
        };
    }
}