using System;
using MoonTools.ECS;
using MoonWorks.Input;
using MyGame.Components;
namespace MyGame.Systems;

public class PlayerController : MoonTools.ECS.System
{
    private Filter EntityFilter;
    public PlayerController(World world) : base(world)
    {
        // this.inputs = inputs;
        EntityFilter = FilterBuilder
            .Include<Position>()
            .Include<ControlledByPlayer>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in EntityFilter.Entities)
        {
            if (!Has<PreventInput>(entity))
            {
                InputState input = GlobalInput.Current;
                float intendedMove = InputToAxis(input.Left.IsDown, input.Right.IsDown);
                Set(entity, new IntendedMove(intendedMove));
                if (intendedMove != 0)
                {
                    Set(entity, new Facing(intendedMove > 0));
                }
                if (input.Jump.IsPressed)
                {
                    Set(entity, new AttemptJumpThisFrame());
                }
                if (input.Jump.IsReleased && Has<IsJumping>(entity))
                {
                    Remove<IsJumping>(entity);
                    var velocity = Get<Velocity>(entity);
                    Console.WriteLine($"canceling jump! has jump: {Has<IsJumping>(entity)}");
                    if (velocity.Y < -MoveConsts.CANCEL_JUMP_SPEED)
                    {
                        Console.WriteLine($"did set velocity to {-MoveConsts.CANCEL_JUMP_SPEED}");
                        Set(entity, new Velocity(velocity.X, -MoveConsts.CANCEL_JUMP_SPEED));
                    }
                }
            }
            var position = Get<Position>(entity);
            Globals.PlayerX = position.X;
            Globals.PlayerY = position.Y;

            // Console.WriteLine($"velocity: {Get<Velocity>(entity).Value}");
        }
    }
    float InputToAxis(bool negative, bool positive)
    {
        if (negative && !positive) return -1;
        if (!negative && positive) return 1;
        return 0;
    }
}
