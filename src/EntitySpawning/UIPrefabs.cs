using MoonTools.ECS;
using MyGame;
using MyGame.Components;
using MyGame.Components;
using MyGame.Content;
using MyGame.Data;
using MyGame.Data;
using MyGame.Relations;

namespace MyGame.Spawn;
public static class UIPrefabs
{
    public static Entity CreateUIParent(World World, int maxPerLine, int xOffset, int yOffset, bool expandVertical)
    {
        var entity = World.CreateEntity();
        World.Set(entity, new UIBoxContainer(maxPerLine, xOffset, yOffset, expandVertical));
        World.Set(entity, new UpdateUIThisFrame());
        return entity;
    }

    public static Entity CreateNineSlice(World World, int X, int Y, int Width, int Height, SpriteAnimationInfoID anim)
    {
        var entity = World.CreateEntity();
        World.Set(entity, new Position(X, Y));
        World.Set(entity, new Rectangle(0, 0, Width, Height));
        World.Set(entity, new SliceAnim(anim));
        World.Set(entity, new Clickable());
        UpdateNineSlice(World, entity, anim, ClickableState.Idle);
        return entity;
    }
    public static void UpdateNineSlice(World World, Entity entity, SpriteAnimationInfoID anim, ClickableState state)
    {
        var sprite = SpriteAnimationInfo.FromID(anim).Frames[(int)state];
        World.Set(entity, new NineSlice(sprite));
    }
}