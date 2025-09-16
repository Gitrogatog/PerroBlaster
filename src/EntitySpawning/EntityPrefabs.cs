
using System;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;
using MoonTools.ECS;
using MoonWorks.Graphics;
using MyGame;
using MyGame.Components;
using MyGame.Content;
using MyGame.Data;
using MyGame.Relations;
using MyGame.Systems;
using MyGame.Utility;

namespace MyGame.Spawn;

public static class EntityPrefabs
{
    static EntityManipulator manipulator;
    static World World;
    public static void Init(World world)
    {
        World = world;
        manipulator = new EntityManipulator(world);
    }
    public static Entity ChangeLevel(int levelID) => manipulator.CreateLoadSceneMessage(levelID);
    public static Entity CreatePlayer(int x, int y) => manipulator.CreatePlayer(x, y);
    public static Entity CreateBunny(int x, int y, Cardinal direction) => manipulator.CreateBunny(x, y, direction);
    public static Entity CreateKiss(int x, int y) => manipulator.CreateKiss(x, y);
    public static Entity CreateTile(int x, int y, Sprite sprite, TileType type) => manipulator.CreateTile(x, y, sprite, type);
    public static Entity CreateThing(ThingType type, int x, int y, Entity spawner) => manipulator.CreateThing(type, x, y, spawner);
    public static Entity CreateAnimation(int x, int y, SpriteAnimation animation, float timer) => manipulator.CreateAnimation(x, y, animation, timer);
    public static void CreateDeathScreen(ThingType causeOfDeath) => manipulator.CreateDeathScreen();
    public static Entity CreateMessage<T>(T component) where T : unmanaged
    {
        var entity = World.CreateEntity();
        World.Set(entity, new DestroyAtEndOfFrame());
        World.Set(entity, component);
        return entity;
    }
    public static Entity CreateTimedMessage<T>(T component, float time) where T : unmanaged
    {
        var entity = World.CreateEntity();
        World.Set(entity, new AddAfterTime<T>(time, component));
        return entity;
    }
    public static Entity CreateSF2SpriteText(string input, int worldX, int worldY, int separationX, int separationY, bool centeredX, bool centeredY, int scale = 1, bool dontDraw = false) => manipulator.CreateSpriteText(input, SpriteAnimations.SF2_Font, 12, 12, separationX, separationY, worldX, worldY, scale, centeredX, centeredY, dontDraw);
}


internal class EntityManipulator : Manipulator
{
    T GetDefault<T>(Entity entity, T other) where T : unmanaged => Has<T>(entity) ? Get<T>(entity) : other;
    public Entity CreateThing(ThingType type, int x, int y, Entity spawner) => type switch
    {
        ThingType.Player => CreatePlayer(x, y),
        ThingType.Explosion => CreateExplosion(x, y),
        ThingType.Bunny => CreateBunny(x, y, GetDefault(spawner, Cardinal.Left)),
        ThingType.RushBunny => CreateBunny(x, y, GetDefault(spawner, Cardinal.Left)),
        _ => CreateExplosion(x, y)
    };
    public Entity CreateTile(int x, int y, Sprite sprite, TileType type)
    {
        Entity entity = CreateBaseTileWithSprite(x, y, sprite);
        switch (type)
        {
            case TileType.Solid:
                {
                    Set(entity, new Rectangle(TileConsts.TILE_SIZE * TileConsts.TILE_MULT, TileConsts.TILE_SIZE * TileConsts.TILE_MULT));
                    Set(entity, new Solid());
                    return entity;
                }
            case TileType.Fake:
                return entity;
            case TileType.Invisible:
                {
                    Remove<Sprite>(entity);
                    return entity;
                }
            case TileType.Throwable:
                {
                    Set(entity, new Rectangle(TileConsts.TILE_SIZE * TileConsts.TILE_MULT, TileConsts.TILE_SIZE * TileConsts.TILE_MULT));
                    Set(entity, new Solid());
                    Set(entity, new CanBeThrown());
                    return entity;
                }
            case TileType.Spike:
                {
                    Set(entity, new Rectangle(-3, 0, 3, 5));
                    Set(entity, new DamageOnContact());
                    return entity;
                }
            default:
                return CreateBaseTileWithSprite(x, y, sprite);
        }
    }
    public Entity CreateAnimation(float x, float y, SpriteAnimation animation, float timer)
    {
        Entity entity = CreateEntity();
        Set(entity, new Position(x, y));
        Set(entity, animation);
        Set(entity, new Timer(timer));
        return entity;
    }
    public Entity CreateExplosion(float x, float y)
    {
        Entity entity = CreateEntity();
        Set(entity, new Position(x, y));
        Set(entity, new SpriteAnimation(SpriteAnimations.Explosion, false));
        Set(entity, new Timer(1f));
        return entity;
    }
    public Entity CreatePlayer(float x, float y)
    {
        Entity entity = CreateEntity();
        Set(entity, new Position(x, y));
        Set(entity, new Velocity());
        // Set(entity, new Gravity());
        // Set(entity, new Rectangle(-1, -8, 8, 16));
        Set(entity, new Rectangle(-8, -16 + 4, 8 * TileConsts.TILE_MULT, 16 * TileConsts.TILE_MULT - 4));
        Set(entity, new SpriteScale(Vector2.One * TileConsts.TILE_MULT));
        Set(entity, new ControlledByPlayer());
        Set(entity, new Gravity());
        // Set(entity, new CanJump(250));
        Set(entity, new CanJump(200));
        Set(entity, new MaxSpeedJump(255));
        Set(entity, new Depth(-4));
        // Set(entity, new AccelParams(5, 5.5f, 1f, 1f));
        Set(entity, new AccelParams(300, 350, 225, 225));
        Set(entity, new SpriteAnimation(SpriteAnimations.Skeleton_Walk));
        Set(entity, new CollidesWithSolids());
        // Set(entity, new DrawAsRectangle());
        // Set(entity, new CanCoyoteJump());
        Set(entity, new BouncesOffWalls(55));
        Set(entity, new CanPivot(339, 119));
        Set(entity, new MoveSpeed(120));
        Set(entity, new Facing(true));
        Set(entity, new DestroyOnLoad());
        Set(entity, new CanBeThrown());
        Set(entity, new CanInteract());
        Set(entity, new PlayerAnimationSet(SpriteAnimations.Skeleton_Idle, SpriteAnimations.Skeleton_Walk, SpriteAnimations.Skeleton_Air, SpriteAnimations.Skeleton_Pivot));
        // Set(entity, new CanWallJump());
        // CreateTextTest();
        // Set(entity, new ColorBlend(new MoonWorks.Graphics.Color(1f, 0f, 0f, 1f)));
        return entity;
    }
    void CreateTextTest()
    {
        var entity = CreateEntity();
        Set(entity, new Position(100, 100));
        Set(entity, new Text(Fonts.PixeltypeID, 24, "HELLO World Dude!"));
        Set(entity, new DisplayCharCount(0));
        Set(entity, new DestroyOnLoad());
        Set(entity, new AdvanceCharCount(1));
        Set(entity, new WordWrap(5));
    }
    public Entity CreateBunny(int x, int y, Cardinal direction)
    {
        Entity entity = CreateEntity();
        Set(entity, new Position(x, y));
        Set(entity, new Rectangle(10 * TileConsts.TILE_MULT, 10 * TileConsts.TILE_MULT));
        Set(entity, new SpriteScale(new Vector2(TileConsts.TILE_MULT, TileConsts.TILE_MULT)));
        Set(entity, new CanInteract());
        Set(entity, new Depth(-5));
        Set(entity, new SpriteAnimation(SpriteAnimations.Bunny));
        // Set(entity, SpriteAnimations.Bunny.Frames[0]);
        Set(entity, new RushAtPlayer(direction, 100, 100));
        Set(entity, new DrawAsRectangle());
        Set(entity, new DestroyOnLoad());
        Set(entity, new CanPerformThrow());
        Set(entity, new Kissable());
        Set(entity, new Facing(direction == Cardinal.Right));
        return entity;
    }
    public Entity CreateKiss(int x, int y)
    {
        Entity entity = CreateEntity();
        Set(entity, new Position(x, y));
        Set(entity, new Rectangle(21 * TileConsts.TILE_MULT, 10 * TileConsts.TILE_MULT));
        Set(entity, new SpriteScale(new Vector2(TileConsts.TILE_MULT, TileConsts.TILE_MULT)));
        Set(entity, new SpriteAnimation(SpriteAnimations.Bunny_Kiss));
        Set(entity, new Solid());
        Set(entity, new DestroyOnLoad());
        return entity;
    }
    public Entity CreateTile(int x, int y)
    {
        Entity entity = CreateEntity();
        Set(entity, new Position(x, y));
        Set(entity, new Rectangle(TileConsts.TILE_SIZE * TileConsts.TILE_MULT, TileConsts.TILE_SIZE * TileConsts.TILE_MULT));
        Set(entity, new Solid());
        // Set(entity, new SpriteAnimation(SpriteAnimations.Tiles, 0, false, 1));
        Set(entity, SpriteAnimations.Tiles.Frames[0]);
        Set(entity, new SpriteScale(new Vector2(TileConsts.TILE_MULT, TileConsts.TILE_MULT)));
        // Set(entity, new DrawAsRectangle());
        Set(entity, new DestroyOnLoad());
        return entity;
    }
    public Entity CreateSpikeTile(int x, int y, Sprite sprite)
    {
        Entity entity = CreateBaseTileWithSprite(x, y, sprite);
        Set(entity, new Rectangle(-3, 0, 3, 5));
        Set(entity, new DamageOnContact());
        return entity;
    }
    public Entity CreateBaseTileWithSprite(int x, int y, Sprite sprite)
    {
        Entity entity = CreateEntity();
        Set(entity, new Position(x, y));
        Set(entity, sprite);
        Set(entity, new SpriteScale(new Vector2(TileConsts.TILE_MULT, TileConsts.TILE_MULT)));
        Set(entity, new DestroyOnLoad());
        Set(entity, new DrawAsRectangle());
        return entity;
    }
    public Entity CreateSolidTileWithSprite(int x, int y, Sprite sprite)
    {
        Entity entity = CreateBaseTileWithSprite(x, y, sprite);
        Set(entity, new Rectangle(TileConsts.TILE_SIZE * TileConsts.TILE_MULT, TileConsts.TILE_SIZE * TileConsts.TILE_MULT));
        return entity;
    }

    public Entity CreateThrowableTile(int x, int y, Sprite sprite)
    {
        Entity entity = CreateSolidTileWithSprite(x, y, sprite);
        Set(entity, new CanBeThrown());
        return entity;
    }
    public Entity CreateLoadSceneMessage(int levelID)
    {
        var entity = CreateEntity();
        Set(entity, new DestroyAtEndOfFrame());
        Set(entity, new ChangeLevel(levelID));
        return entity;
    }
    public Entity CreateTimedMessage<T>(T component, float time) where T : unmanaged
    {
        var entity = CreateEntity();
        Set(entity, new AddAfterTime<T>(time, component));
        return entity;
    }
    public void CreateDeathScreen()
    {
        // create player sprite
        // create bunny sprite
        var playerImage = CreateEntity();
        Set(playerImage, new Position(Dimensions.GAME_W / 4, 150));
        Set(playerImage, new SpriteScale(new Vector2(6, -6)));
        Set(playerImage, new SpriteAnimation(SpriteAnimations.Skeleton));
        Set(playerImage, new Facing(true));
        Set(playerImage, new Depth(-6));
        Set(playerImage, new DestroyOnLoad());
        // Set(playerImage, new Depth(0));
        var killerImage = CreateEntity();
        Set(killerImage, new Position(3 * Dimensions.GAME_W / 4 - 30, 150));
        Set(killerImage, new SpriteScale(10));
        Set(killerImage, new SpriteAnimation(SpriteAnimations.Bunny));
        Set(killerImage, new Facing(false));
        Set(killerImage, new DestroyOnLoad());
        Set(killerImage, new Depth(-5));
        // Set(killerImage, new Depth(10));
        Set(killerImage, new DeathScreen());

        var textParent = EntityPrefabs.CreateSF2SpriteText("Approach me if you dare.\nI will destroy you.", Dimensions.GAME_W / 2, 300, 0, 6, true, true, 2, true);
        // Set(text, new Depth(10));

        var background = CreateEntity();
        Set(background, new Position());
        Set(background, SpriteAnimations.Pixel.Frames[0]);
        Set(background, new ColorBlend(Color.Black));
        // Set(background, new Depth());
        // Set(background, new Depth(9));
        Set(background, new SpriteScale(1000));
        Set(background, new DestroyOnLoad());
        Set(CreateEntity(), new PlayStaticSFX(StaticAudio.SF2_Defeat));
    }
    public Entity CreateSpriteText(string input, SpriteAnimationInfo info, int textSizeX, int textSizeY, int separationX, int separationY, int worldX, int worldY, int scale, bool centeredX, bool centeredY, bool dontDraw = false)
    {
        Sprite sprite = info.Frames[0];
        var parent = CreateEntity();
        Set(parent, new DestroyOnLoad());
        Set(parent, new TextSpriteParent());

        if (dontDraw)
        {
            Set(parent, new AdvanceCharSpeed(0.06f));
            Set(parent, new AdvanceCharCount(1f));
        }
        int x = 0, y = 0, maxLength = 0;
        int deltaX = separationX + textSizeX * scale;
        int deltaY = separationY + textSizeY * scale;
        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            if (c == '\n')
            {
                x += deltaX;
                var blank = CreateEntity();
                Set(blank, new DestroyOnLoad());
                Set(blank, new Position(worldX + x, worldY + y));
                Relate(parent, blank, new Child());
                Relate(parent, blank, new DontDraw());

                maxLength = Math.Max(x, maxLength);
                y += deltaY;
                x = 0;
                continue;
            }
            if (c == ' ')
            {
                x += deltaX;
                var blank = CreateEntity();
                Set(blank, new DestroyOnLoad());
                Set(blank, new Position(worldX + x, worldY + y));
                Relate(parent, blank, new Child());
                Relate(parent, blank, new DontDraw());
                continue;
            }
            int offset = TextUtils.CharToSF2TextPos(c);
            if (offset < 0)
            {
                continue;
            }
            Sprite textSprite = sprite.Slice(offset * textSizeX, 0, textSizeX, textSizeY);
            Position pos = new Position(worldX + x, worldY + y);
            var textEntity = CreateEntity();
            Set(textEntity, textSprite);
            Set(textEntity, pos);
            Set(textEntity, new DestroyOnLoad());
            Set(textEntity, new Depth(-8));
            Relate(parent, textEntity, new Child());
            if (dontDraw)
            {
                Relate(parent, textEntity, new DontDraw());
            }
            if (scale != 1)
            {
                Set(textEntity, new SpriteScale(scale));
            }
            x += deltaX;
        }
        if (centeredX || centeredY)
        {
            if (centeredX)
            {
                x -= deltaX;
                maxLength = Math.Max(x, maxLength) / 2;
            }
            if (centeredY)
            {
                y /= 2;
            }
            foreach (var child in OutRelations<Child>(parent))
            {
                var pos = Get<Position>(child);
                if (centeredX)
                {
                    pos = pos.SetX(pos.X - maxLength);
                }
                if (centeredY)
                {
                    pos = pos.SetY(pos.Y - y);
                }
                Set(child, pos);
            }
        }
        return parent;

    }

    public EntityManipulator(World world) : base(world)
    {
    }

}