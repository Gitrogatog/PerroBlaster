using System;
using System.Data;
using System.Numerics;
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
            .Include<Position>()
            .Include<ControlledByPlayer>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in EntityFilter.Entities)
        {
            var position = Get<Position>(entity);
            Globals.PlayerX = position.X;
            Globals.PlayerY = position.Y;
            var cursor = CreateEntity();
            Set(cursor, new Position(GlobalInput.WorldMouseX, GlobalInput.WorldMouseY));
            Set(cursor, new SpriteAnimation(SpriteAnimations.Cursor));
            Set(cursor, new DestroyAtStartOfFrame());


            if (Has<PreventInput>(entity)) {
                Set(entity, new IntendedMove());
                continue;
            }
            
            
            InputState input = GlobalInput.Current;
            float xMove = InputToAxis(input.Left.IsDown, input.Right.IsDown);
            float yMove = InputToAxis(input.Up.IsDown, input.Down.IsDown);
            Vector2 move = new Vector2(xMove, yMove);
            if(xMove != 0 || yMove != 0){
                move = Vector2.Normalize(move);
            }
            Set(entity, new IntendedMove(move));
            // Set(entity, new SetAnimation(xMove == 0 && yMove == 0 ? SpriteAnimations.Alt_Player_Idle : SpriteAnimations.Alt_Player_Walk));
            // Set(entity, new SetAnimation(SpriteAnimations.Player2));
            
            Vector2 aimAngle = new Vector2(GlobalInput.WorldMouseX - position.X, GlobalInput.WorldMouseY - position.Y);
            aimAngle = (aimAngle.X == 0 && aimAngle.Y == 0) ? Get<AimAngle>(entity).Angle : Vector2.Normalize(aimAngle);
            Set(entity, new AimAngle(aimAngle));
            var axe = GetSingletonEntity<AxeState>();
            var axeState = GetSingleton<AxeState>();
            // Console.WriteLine($"throw:{Has<CanBeThrown>(axe)} stick:{Has<CanBeStuck>(axe)} recall:{Has<CanBeRecalled>(axe)} hold:{Has<CanBeHeld>(axe)}");
            if(GlobalInput.Current.AltShoot.IsPressed) {
                // var state = GetSingleton<AxeState>();
                if(Has<CanBeRecalled>(axe)) {
                    // Remove<CanBeRecalled>(axe);
                    // Set(axe, AxeState.Held);
                    position = Get<Position>(axe);
                    Set(entity, position);
                    EntityPrefabs.SetHeldByPlayer(entity, axe);
                }
            }
            if(GlobalInput.Current.Shoot.IsPressed){
                if(axeState == AxeState.Held){
                    Remove<IgnoreCollision>(axe);
                    Set(axe, new Velocity(aimAngle * MoveConsts.AXE_THROW_SPEED));
                    UnrelateAll<OffsetAimAngle>(axe);
                    Set(axe, AxeState.Thrown);
                    Set(axe, new RotateSpeed(-14));
                }
                else if(Has<CanBeRecalled>(axe)){
                    EntityPrefabs.RecallAxe(axe);
                }
            }
        }
    }
    float InputToAxis(bool negative, bool positive)
    {
        if (negative && !positive) return -1;
        if (!negative && positive) return 1;
        return 0;
    }
}
