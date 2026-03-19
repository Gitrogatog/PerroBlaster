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
            int yMove = (int)InputToAxis(input.Up.IsDown, input.Down.IsDown);
            if(xMove != 0 && yMove != 0) {
                var inputTime = GetSingleton<LastInputTime>();
                double yLatest = yMove < 0 ? inputTime.Up : inputTime.Down;
                double xLatest = xMove < 0 ? inputTime.Left : inputTime.Right; 
                if(xLatest > yLatest) {
                    yMove = 0;
                }
                else {
                    xMove = 0;
                }
            }
            // Remove<IntendedMove>(entity);
            if(xMove != 0 || yMove != 0) {
                if(Has<TilePosition>(entity)) {
                    Set(entity, new AttemptMoveToTile(xMove, yMove));
                }
                else {
                    Set(entity, new IntendedMove(xMove, yMove));
                }
            }
            else if(!Has<TilePosition>(entity)) {
                Set(entity, new IntendedMove());
            }
            Set(entity, new LastDirection(xMove, yMove));
            if(GlobalInput.Current.Interact.IsPressed) {
                Set(entity, new AttemptTalkThisFrame());
                // EntityPrefabs.PlaySFX(StaticAudio.Open1);
                Console.WriteLine("player attempt talk!");
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
