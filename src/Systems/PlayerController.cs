using System;
using System.Data;
using System.Numerics;
using Microsoft.VisualBasic;
using MoonTools.ECS;
using MoonWorks.Input;
using MyGame.Components;
using MyGame.Content;
using MyGame.Relations;
using MyGame.Spawn;
namespace MyGame.Systems;

public class PlayerController : MoonTools.ECS.System
{
    private Filter EntityFilter;
    public PlayerController(World world) : base(world)
    {
        // this.inputs = inputs;
        EntityFilter = FilterBuilder
            .Include<ControlledByPlayer>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in EntityFilter.Entities)
        {
            // Console.WriteLine($"has move to tile: {Has<MoveToTile>(entity)}");
            // Console.WriteLrgine("woah!");
            // Set(entity, new ColorBlend(new MoonWorks.Graphics.Color(1, 1, 1, 0.5f)));
            // Set(entity, new Depth(2f));
            if(Some<PreventInput>()) continue;
            InputState input = GlobalInput.Current;
            int xMove = (int)InputToAxis(input.Left.IsDown, input.Right.IsDown);
            Set(entity, new IntendedMove(xMove, 0));
            if(input.Interact.IsPressed) {
                // Set(entity, new AttemptJumpThisFrame());
                // Set(entity, new Velocity(Get<Velocity>(entity).X, -100));
                Set(entity, new AttemptJumpThisFrame());
                if(Has<Grounded>(entity)){
                    Console.WriteLine("setting grav");
                     Set(entity, new Gravity(MoveConsts.GRAVITY_PLAYER_JUMP));}
            }
            else if(input.Interact.IsReleased) {
                Set(entity, new Gravity(MoveConsts.GRAVITY));
            }
            // Console.WriteLine($"player vel: {Get<Position>(entity)}");
        }
    }
    float InputToAxis(bool negative, bool positive)
    {
        if (negative && !positive) return -1;
        if (!negative && positive) return 1;
        return 0;
    }
}
