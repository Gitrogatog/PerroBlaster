using System;
using MoonTools.ECS;
using MyGame.Components;
using MyGame.Content;
using MyGame.Data;
namespace MyGame.Systems;

public class AdvanceCharCountSystem : MoonTools.ECS.System
{
    private Filter EntityFilter;

    public AdvanceCharCountSystem(World world) : base(world)
    {
        EntityFilter = FilterBuilder
            .Include<AdvanceCharCount>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in EntityFilter.Entities)
        {
            (float oldCharCount, float charsPerSecond) = Get<AdvanceCharCount>(entity);
            float newCharCount = oldCharCount + (float)delta.TotalSeconds * charsPerSecond;
            // Console.WriteLine($"old char: {oldCharCount} new char: {newCharCount}");
            Set(entity, new AdvanceCharCount(newCharCount, charsPerSecond));
            int newCharInt = (int)newCharCount;
            if((int)oldCharCount < newCharInt) {
                Set(entity, new DisplayCharCount(newCharInt));
                // Console.WriteLine("setting display char count!" + Get<DisplayCharCount>(entity));
            }
            // if(!Some<CanAdvanceDialog>() && TryGet<Text>(entity, out Text text) && TextStorage.GetString(text.TextID).Length <= newCharInt && Has<DestroyOnDialogBoxClose>(entity)) {
            //     var flickerArrow = CreateEntity();
            //     Set(flickerArrow, new DestroyOnLoad());
            //     Set(flickerArrow, new DestroyOnDialogBoxClose());
            //     Set(flickerArrow, new Position(Dimensions.GAME_W / 2 + Globals.CameraX, Dimensions.GAME_H - 4 + Globals.CameraY));
            //     Set(flickerArrow, new SpriteAnimation(SpriteAnimations.ui_arrow_down));
            //     Set(flickerArrow, new FlickerDontDraw(1f / 3f));
            //     Set(flickerArrow, new Depth(0.00001f));
            //     var canAdvanceDialog = CreateEntity();
            //     Set(canAdvanceDialog, new CanAdvanceDialog());
            //     Set(canAdvanceDialog, new DestroyOnDialogBoxClose());
            // }
        }
    }
}