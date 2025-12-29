
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
    public static Entity PlaySFX(StaticSoundID StaticSoundID,
        SoundCategory Category = SoundCategory.Generic,
        float Volume = 1,
        float Pitch = 0,
        float Pan = 0)
    {
        var entity = World.CreateEntity();
        World.Set(entity, new PlayStaticSFX(StaticSoundID, Category, Volume, Pitch, Pan));
        return entity;
    }
}


internal class EntityManipulator : Manipulator
{
    T GetDefault<T>(Entity entity, T other) where T : unmanaged => Has<T>(entity) ? Get<T>(entity) : other;
    public Entity CreateAnimation(float x, float y, SpriteAnimation animation, float timer)
    {
        Entity entity = CreateEntity();
        Set(entity, new Position(x, y));
        Set(entity, animation);
        Set(entity, new Timer(timer));
        return entity;
    }

    public Entity CreatePlayer(float x, float y)
    {
        Entity entity = CreateEntity();
        Set(entity, new Position(x, y));
        Set(entity, new Velocity());
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