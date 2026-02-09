
using System;
using System.Numerics;
using MoonTools.ECS;
using MoonWorks.Graphics;
using MoonWorks.Graphics.Font;
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
    public static Entity CreateTile(int x, int y, Sprite sprite) => manipulator.CreateTile(x, y, sprite);
    public static Entity CreateEnemy(EnemySpawnPoint spawnPoint, Entity spawnEntity) => manipulator.CreateEnemy(spawnPoint, spawnEntity);
    public static Entity CreateEnemySpawnPoint(int x, int y, EnemyType enemyType) => manipulator.CreateEnemySpawnPoint(x, y, enemyType);
    public static Entity AddSolidCollision(Entity entity, Rectangle rect) => manipulator.AddSolidCollision(entity, rect);
    public static void SetHeldByPlayer(Entity player, Entity target) => manipulator.SetHeldByPlayer(player, target);
    public static void RecallAxe(Entity axe) => manipulator.RecallAxe(axe);
    public static void CreateBulletPattern(BulletPattern pattern, Position position, Vector2 direction) => manipulator.CreateBulletPattern(pattern, position, MathUtils.SafeNormalize(direction));
    public static void CreateStartMenu() => manipulator.CreateStartMenu();
    public static void CreateEndMenu() => manipulator.CreateEndMenu();
    public static Entity CreateText(int x, int y, int size, FontID fontID, string text, HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left) 
        => manipulator.CreateTextEntity(x, y, size, fontID, text, horizontalAlignment);

    public static Entity CreateTimer<T>(Entity target, float time, T relation) where T : unmanaged
    {
        var entity = World.CreateEntity();

        World.Set(entity, new Timer(time));
        World.Relate(entity, target, relation);
        return entity;
    }
    public static Entity CreateMessage<T>(T component) where T : unmanaged
    {
        var entity = World.CreateEntity();
        World.Set(entity, new DestroyAtStartOfFrame());
        World.Set(entity, component);
        return entity;
    }
    public static Entity CreateMessageEndOfFrame<T>(T component) where T : unmanaged {
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
    public static bool Mirror<T>(Entity source, Entity target) where T : unmanaged {
        if(World.Has<T>(source)) {
            World.Set(target, World.Get<T>(source));
            return true;
        }
        return false;
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
        Set(entity, new AimAngle());
        Set(entity, new ControlledByPlayer());
        Set(entity, new CollidesWithSolids());
        Set(entity, new Rectangle(10, 10, EffectorFlags.CanTouchWall | EffectorFlags.CanTouchPit, EffectedFlags.CanTakeDamage));
        Set(entity, new CanInteract());
        Set(entity, new SpriteAnimation(SpriteAnimations.Player2));
        Set(entity, new DestroyOnPlayerRespawn());
        Set(entity, new RotateSpriteToAimAngle());
        Set(entity, new Health(5));
        Set(entity, new OwnedByPlayer());
        Set(entity, new MoveSpeed(85));
        Set(entity, new CreateHealthUI(10, 10));
        Set(entity, new InvincibleOnDamage(1.5f));
        Set(entity, new PlaySFXOnDamage(StaticAudio.sfx_deathscream_android1));
        CreateAxe(entity, x, y);
        // Relate(entity, , new Offset());
        return entity;
    }
    public Entity CreateAxe(Entity player, float x, float y){
        Entity entity = CreateEntity();
        Set(entity, new Position(x, y));
        Set(entity, new Velocity());
        Set(entity, new SpriteScale(1.5f));
        Set(entity, new Rectangle(20, 20, EffectorFlags.CanTouchWall | EffectorFlags.CanDamage, EffectedFlags.None));
        // Set(entity, new DrawAsRectangle());
        Set(entity, new CanInteract());
        Set(entity, new SpriteAnimation(SpriteAnimations.Axe2));
        Set(entity, new IsAxe());
        Set(entity, new DestroyOnPlayerRespawn());
        Set(entity, new DamageOnContact());
        Set(entity, new OwnedByPlayer());
        Set(entity, new DontRepeatDamageUntilStateChange());
        SetHeldByPlayer(player, entity);
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
    public void CreateBulletPattern(BulletPattern pattern, Position position, Vector2 direction) {
        switch(pattern) {
            case BulletPattern.Single: {
                CreateBullet(position, direction);
                break;
            }
            case BulletPattern.Triple: {
                CreateBullet(position, direction);
                CreateBullet(position, Vector2.Transform(direction, Matrix3x2.CreateRotation(MathF.PI * 0.25f)));
                CreateBullet(position, Vector2.Transform(direction, Matrix3x2.CreateRotation(MathF.PI * -0.25f)));
                break;
            }
        }
    }
    private Entity CreateBullet(Position position, Vector2 direction) {
        var entity = CreateEntity();
        Set(entity, new DestroyOnCompleteRoom());
        Set(entity, new DestroyOnPlayerRespawn());
        Set(entity, position);
        Set(entity, new Velocity(direction * 100));
        Set(entity, new DamageOnContact());
        Set(entity, new DestroyOnContact());
        Set(entity, new CanInteract());
        Set(entity, new OwnedByEnemy());
        Set(entity, new SpriteScale(1.2f));
        Set(entity, new Rectangle(12, 12, EffectorFlags.CanDamage | EffectorFlags.CanTouchWall, EffectedFlags.None));
        Set(entity, new SpriteAnimation(SpriteAnimations.Bullet2));
        return entity;
    }
    public Entity CreateTile(int x, int y, Sprite tileSprite)
    {
        var entity = CreateEntity();
        Set(entity, new Position(x, y));
        Set(entity, tileSprite);
        Set(entity, new DestroyOnLoad());
        Set(entity, new Depth(9));
        return entity;
    }
    public Entity AddSolidCollision(Entity entity, Rectangle rect)
    {
        Set(entity, new Solid());
        Set(entity, new CanInteract());
        Set(entity, rect);
        return entity;
    }
        public void CreateStartMenu(){
        // CreateTextEntity(60, 20, 8, Fonts.KosugiID, "BUBBA");
        // var subTitle = CreateTextEntity(70, 65, 12, Fonts.PixeltypeID, "AXE");
        var title = CreateEntity();
        Set(title, new DestroyOnLoad());
        Set(title, new Position(110, 60));
        Set(title, new SpriteAnimation(SpriteAnimations.Title));
        CreateTextEntity(50, 110, 12, Fonts.PixeltypeID, "Instructions:");
        CreateTextEntity(50, 130, 12, Fonts.PixeltypeID, "WASD/Arrow keys: Move");
        CreateTextEntity(50, 150, 12, Fonts.PixeltypeID, "Left Click: Throw/Recall Axe");
        CreateTextEntity(50, 170, 12, Fonts.PixeltypeID, "Right Click: Teleport To Axe");
        CreateTextEntity(50, 190, 12, Fonts.PixeltypeID, "-/= : Decrease/Increase Window Size");
        CreateTextEntity(50, 210, 12, Fonts.PixeltypeID, "Right Click to Start!");
    }
    public void CreateEndMenu(){
        // CreateTextEntity(Dimensions.GAME_W / 2, 20, 10, Fonts.KosugiID, "YOU WIN!", HorizontalAlignment.Center);
        // CreateTextEntity(Dimensions.GAME_W / 2, 80, 12, Fonts.PixeltypeID, "Good job Bubba, you saved the day!!!", HorizontalAlignment.Center);
        CreateTextEntity(Dimensions.GAME_W / 2, 110, 12, Fonts.PixeltypeID, $"you died a total of {Globals.DeathCount} times.", HorizontalAlignment.Center);
        CreateTextEntity(Dimensions.GAME_W / 2, 140, 12, Fonts.PixeltypeID, "press Escape to close the game.", HorizontalAlignment.Center);
    }
    public Entity CreateTextEntity(int x, int y, int size, FontID fontID, string text, HorizontalAlignment horizontalAlignment=HorizontalAlignment.Left){
        var entity = CreateEntity();
        Set(entity, new DestroyOnLoad());
        Set(entity, new Position(x, y));
        Set(entity, new Text(fontID, size, TextStorage.GetID(text), horizontalAlignment));
        return entity;
    }
    public void SetHeldByPlayer(Entity player, Entity target) {
        Remove<CanBeRecalled>(target);
        Remove<RotateSpeed>(target);
        Remove<MoveTowardPlayer>(target);
        Remove<CanBeHeld>(target);
        Set(target, AxeState.Held);
        Set(target, new Velocity());
        Set(target, new IgnoreCollision());
        Relate(player, target, new OffsetAimAngle(MoveConsts.AXE_HELD_OFFSET));
        EntityPrefabs.PlaySFX(StaticAudio.sfx_movement_jump10_landing);
        UnrelateAll<DontDamage>(target);
    }
    public void RecallAxe(Entity axe) {
        Remove<CanBeRecalled>(axe);
        Remove<IgnoreCollision>(axe);
        Set(axe, AxeState.Recalled);
        Set(axe, new MoveTowardPlayer());
        Set(axe, new MoveSpeed(MoveConsts.AXE_RETURN_SPEED));
        Set(axe, new RotateSpeed(17));
        Set(axe, new CanBeHeld());
        UnrelateAll<DontDamage>(axe);
    }
    public Entity CreateEnemy(EnemySpawnPoint spawnPoint, Entity spawnEntity) {
        var entity = CreateBaseEnemy(spawnPoint.X, spawnPoint.Y);
        Set(entity, new CantMoveTimer(MoveConsts.ENEMY_INIT_PAUSE_TIME_MOVE));
        Console.WriteLine($"creating enemy: {spawnPoint.EnemyType}");
        Set(entity, new PlaySFXOnDamage(StaticAudio.sfx_exp_shortest_hard9));
        // Set(entity, new DestroyOnExitRoom());
        switch(spawnPoint.EnemyType) {
            case EnemyType.Triangle: {
                AddEnemyHitbox(entity, 16, 16, EffectorFlags.CanDamage);
                Set(entity, new DamageOnContact());
                Set(entity, new SpriteAnimation(SpriteAnimations.Triangle3));
                Set(entity, new Health(2));
                Set(entity, new RotateSpriteToAimAngle());
                Set(entity, new AimAtPlayer());
                Set(entity, new MoveSpeed(70));
                Set(entity, new CanShoot(BulletPattern.Single, 0.5f));
                Set(entity, new CantShootTimer(MoveConsts.ENEMY_INIT_PAUSE_TIME_SHOOT));
                break;
            }
            case EnemyType.Circle: {
                AddEnemyHitbox(entity, 16, 16, EffectorFlags.CanDamage);
                Set(entity, new SpriteAnimation(SpriteAnimations.Circle));
                Set(entity, new Health(2));
                Set(entity, new DamageOnContact());
                Set(entity, new MoveTowardPlayer());
                Set(entity, new CollidesWithSolids());
                Set(entity, new AccelParams(140));
                Set(entity, new MoveSpeed(200));
                Set(entity, new BouncesOffWalls());
                break;
            }
        }
        if(spawnPoint.EnemyType == EnemyType.Triangle || spawnPoint.EnemyType == EnemyType.Pentagon) {
            Mirror<FollowPath>(spawnEntity, entity);
            Mirror<InvertPath>(spawnEntity, entity);
        }
        return entity;
    }
    public bool Mirror<T>(Entity source, Entity target) where T : unmanaged {
        if(Has<T>(source)) {
            Set(target, Get<T>(source));
            return true;
        }
        return false;
    }
    public Entity CreateBaseEnemy(float x, float y) {
        var entity = CreateEntity();
        Set(entity, new Position(x, y));
        Set(entity, new Velocity());
        Set(entity, new OwnedByEnemy());
        Set(entity, new CanInteract());
        Set(entity, new DestroyOnPlayerRespawn());
        Set(entity, new MustBeKilledToProgress());
        return entity;
    }
    public void AddEnemyHitbox(Entity entity, int width, int length, EffectorFlags extraFlags = EffectorFlags.None) {
        Set(entity, new Rectangle(width, length, EffectorFlags.CanTouchWall | extraFlags, EffectedFlags.CanTakeDamage));
    }
    public Entity CreateEnemySpawnPoint(int x, int y, EnemyType enemyType) {
        var entity = CreateEntity();
        Set(entity, new EnemySpawnPoint(x, y, enemyType));
        Set(entity, new DestroyOnLoad());
        Set(entity, new RoomID(x / Dimensions.ROOM_X, y / Dimensions.ROOM_Y));
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