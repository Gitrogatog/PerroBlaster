
using System;
using System.Numerics;
using System.Threading.Tasks;
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

    // public static Entity ChangeLevel(int levelID) => manipulator.CreateLoadSceneMessage(levelID);
    public static Entity CreatePlayer(int x, int y) {
        var entity = CreateEntity();
        Set(entity, new Position(x, y));
        Set(entity, new Velocity());
        // Set(entity, new DrawAsRectangle());
        Set(entity, new Rectangle(10, 10));
        Set(entity, new ControlledByPlayer());
        Set(entity, new CanInteract());
        Set(entity, new CollidesWithSolids());
        Set(entity, new Facing());
        Set(entity, new Gravity(MoveConsts.GRAVITY));
        Set(entity, new CanShoot(ShotType.Player, 4, -2, 7f / 60f, AimType.AimAndFacing));
        Set(entity, new RetainFacingWhileAttemptShoot());
        Set(entity, new OwnedByPlayer());
        Set(entity, new Health(5));
        float moveSpeed = 50;
        
        Set(entity, new MoveSpeed(moveSpeed));
        Set(entity, EffectedFlags.CanTakeDamage);
        Set(entity, EffectorFlags.CanTouchWall | EffectorFlags.CanTouchDownPlatform);
        // Set(entity, new DrawAsRectangle());
        Set(entity, new Depth(0.0000001f));
        Set(entity, new PlayerAnimation(
            SpriteAnimations.perro_idle.ID, 
            SpriteAnimations.perro_walk.ID,
            SpriteAnimations.perro_walk_back.ID,
            SpriteAnimations.perro_jump.ID, 
            SpriteAnimations.perro_fall.ID,
            SpriteAnimations.perro_up_idle.ID, 
            SpriteAnimations.perro_up_walk.ID,
            SpriteAnimations.perro_up_walk_back.ID,
            SpriteAnimations.perro_up_jump.ID, 
            SpriteAnimations.perro_up_fall.ID,
            ComponentUtils.AnimSpeedMult(SpriteAnimations.perro_walk, moveSpeed)));

        // Set(entity, new IdleAnimation(SpriteAnimations.perro_idle));
        Set(entity, new DestroyOnLoad());
        return entity;
    }
    public static Entity MakeEnemy(EnemyType enemyType, int x, int y, bool facing) {
        var entity = BaseEnemy(x, y, facing);
        Set(entity, enemyType);
        switch(enemyType) {
            case EnemyType.Tumbleweed: {
                 Set(entity, new MoveSpeed(60));
                Set(entity, new MoveTowardFacing());
                Set(entity, new SpriteAnimation(SpriteAnimations.tumbleweed));
                Set(entity, new Rectangle(10, 10));
                Set(entity, new DamageOnContact());
                Set(entity, new FlipFacingOnTouchWall());
                Set(entity, EffectorFlags.DamageAllWall);
                Set(entity, EffectedFlags.CanTakeDamage);
                Set(entity, new Health(5));
                Set(entity, new Gravity(MoveConsts.GRAVITY));
                break;
            }
            case EnemyType.Chaser: {
                Set(entity, new MoveSpeed(100));
                Set(entity, new AccelParams(160));
                Set(entity, new MoveTowardPlayer());
                break;
            }
            case EnemyType.Throw: {
                Set(entity, EnemyState.Idle);
                Set(entity, new IdleAnimation(SpriteAnimations.mole_idle));
                Set(entity, new HurtAnimation(SpriteAnimations.mole_death));
                break;
            }
            case EnemyType.JumpShootOnLand: {
                Set(entity, new JumpAfterLanding(1f));
                Set(entity, new Gravity(MoveConsts.GRAVITY));
                Set(entity, new Rectangle(10, 10));
                Set(entity, new DamageOnContact());
                Set(entity, EffectorFlags.DamageAllWall);
                Set(entity, EffectedFlags.CanTakeDamage);
                Set(entity, new Health(5));
                Set(entity, new IdleAnimation(SpriteAnimations.mud_man_idle));
                Set(entity, new AirAnimation(SpriteAnimations.mud_man_jump));
                Set(entity, new ShootWhen<TouchGroundTrigger>(ShotType.Fireball, AimType.Facing));
                Set(entity, new GroundAirMoveSpeed(0, 60));
                Set(entity, new MoveTowardFacing());
                Set(entity, new FlipFacingOnTouchWall());
                Set(entity, new CanJump(200));
                break;
            }
            case EnemyType.JumpShootPeak: {
                AddRegularEnemyCollision(entity, 10, 10);
                Set(entity, new Health(3));
                Set(entity, new JumpAfterLanding(1f));
                float canjump = 250;
                float gravity = MoveConsts.GRAVITY;
                Set(entity, new CanJump(canjump));
                Set(entity, new Gravity(gravity));
                Set(entity, new ShootAfterTimeWhen<JumpTrigger>(ShotType.Fireball, AimType.Facing, canjump / gravity)); // canjump / gravity
                // Set(entity, new AddAfterTimeWhen<JumpTrigger, AttemptShootThisFrame>(canjump / gravity));                Set(entity, new IdleAnimation(SpriteAnimations.mud_man_idle));
                Set(entity, new AirAnimation(SpriteAnimations.mud_man_jump));
                break;
            }
        }
        return entity;
    }
    static void AddEnemyCollision(Entity entity, int x, int y, EffectorFlags effectorFlags, EffectedFlags effectedFlags) {
        Set(entity, new Rectangle(x, y));
        Set(entity, effectorFlags);
        Set(entity, effectedFlags);
    }
    static void AddRegularEnemyCollision(Entity entity, int x, int y) {
        AddEnemyCollision(entity, x, y, EffectorFlags.DamageAllWall, EffectedFlags.CanTakeDamage);
        Set(entity, new DamageOnContact());
    }
    public static Entity BaseEnemy(int x, int y, bool facing) {
        var entity = CreateEntity();
        Set(entity, new Position(x, y));
        Set(entity, new Velocity());
        Set(entity, new Facing(facing));
        Set(entity, new OwnedByEnemy());
        Set(entity, new DestroyOnLoad());
        Set(entity, new CanInteract());
        Set(entity, new CollidesWithSolids());
        return entity;
    }
    public static void KillEntity(Entity entity) {
        if(Has<KillTrigger>(entity)) return;
        Set(entity, new KillTrigger());
        Set(entity, new DestroyAtEndOfFrame());
        if(Has<DeathAnimation>(entity)) {
            (var animData, float time) = Get<DeathAnimation>(entity);
            var setAnimation = new SetAnimation(animData);
            var animEntity = CreateEntity();
            Set(animEntity, setAnimation);
            Set(animEntity, new Timer(time));
            Set(animEntity, Get<Position>(entity));
            Mirror<Facing>(entity, animEntity);
            Mirror<Rotation>(entity, animEntity);
        }
    }
    public static void AttemptPerformShot(Entity entity, ShotType shotType, AimType aimType) {
        if(HasInRelation<DontShoot>(entity)) {
            return;
        }
        if(Has<ShotOffset>(entity)) {
            (int offsetX, int offsetY) = Get<ShotOffset>(entity);
            PerformShot(entity, shotType, aimType, offsetX, offsetY);
        }
        else {
            PerformShot(entity, shotType, aimType, 0, 0);
        }
    }
    
    public static void PerformShot(Entity entity, ShotType shotType, AimType aimType, int offsetX, int offsetY) {
        Vector2 shotDir;
            switch(aimType) {
                case AimType.Facing: {
                    if(Has<Facing>(entity) && Get<Facing>(entity).Right) {
                        shotDir = new Vector2(1, 0);
                        offsetX = -offsetX;
                    }
                    else {
                        shotDir = new Vector2(-1, 0);
                    }
                    break;
                }
                case AimType.AimAngle: {
                    shotDir = Has<AimAngle>(entity) ? Get<AimAngle>(entity).Angle : Vector2.Zero;
                    break;
                }
                case AimType.AimAndFacing: {
                    bool facing = EntityPrefabs.GetFacing(entity);
                    // Console.WriteLine($"facing: {Get<Facing>(entity).Right}");
                    if(!facing) {
                        offsetX = -offsetX;
                    }
                    if(TryGet(entity, out AimAngle angle) && angle.Angle.Y != 0) {
                        shotDir = angle.Angle;
                    }
                    else if(facing) {
                        shotDir = new Vector2(1, 0);
                    }
                    else {
                        shotDir = new Vector2(-1, 0);
                    }
                    break;
                }
                case AimType.PlayerArc: {
                    var start = Get<Position>(entity);
                    float height = 20;
                    Vector2 velocity = MathUtils.InitialProjectileVelocity(start.X, start.Y, Globals.PlayerX, Globals.PlayerY, height, MoveConsts.GRAVITY);
                    shotDir = velocity;
                    break;
                }
                default: {
                    shotDir = Vector2.Zero;
                    break;
                }
            }
            switch(shotType) {
                case ShotType.Player: {
                    var shot = MakeBaseBullet(entity, offsetX, offsetY, shotDir, 300, 8, 8, 0.5f);
                    Set(shot, new SpriteAnimation(SpriteAnimations.yellow_shot));
                    Set(shot, new DeathAnimation(SpriteAnimations.yellow_shot_dead, Time.SINGLE_FRAME * 4));
                    if(shotDir.X != 0) {
                        Set(shot, new Facing(shotDir.X > 0));
                    }
                    else {
                        Set(shot, new Rotation(MathF.PI * 0.5f));
                    }
                    break;
                }
                case ShotType.Flower: {
                    var shot = MakeBaseBullet(entity, offsetX, offsetY, shotDir, 1, 8, 8, 5f);
                    Set(shot, new SpriteAnimation(SpriteAnimations.jump_flower_bullet));
                    break;
                }
                case ShotType.Fireball: {
                    var shot = MakeBaseBullet(entity, offsetX, offsetY, shotDir, 100, 8, 8, 10f);
                    Set(shot, new SpriteAnimation(SpriteAnimations.flower_fireball));
                    break;
                }
            }
    }

    static Entity MakeBaseBullet(Entity source, int offsetX, int offsetY, Vector2 direction, float speed, int width, int height, float lifetime = 0) {
        var entity = CreateEntity("bullet");
        Set(entity, new DestroyOnLoad());
        var pos = Get<Position>(source);
        Set(entity, new Position(pos.X + offsetX, pos.Y + offsetY));
        Set(entity, new Velocity(direction * speed));
        Set(entity, new Rectangle(width, height));
        Set(entity, EffectorFlags.CanDamage | EffectorFlags.CanTouchWall);
        Set(entity, EffectedFlags.None);
        if(Has<OwnedByEnemy>(source)) Set(entity, new OwnedByEnemy());
        else if (Has<OwnedByPlayer>(source)) Set(entity, new OwnedByPlayer());
        Set(entity, new DestroyOnContact());
        Set(entity, new DamageOnContact());
        Set(entity, new CanInteract());
        Mirror<Facing>(source, entity);
        if(lifetime != 0) Set(entity, new Timer(lifetime));
        // Set(entity, new DrawAsRectangle());
        // Set(entity, new ColorBlend(new MoonWorks.Graphics.Color(0, 255, 0)));
        return entity;
    }
    // public static void ProcessAttackAction(Entity entity, AttackAction action) {
    //     if(action.Animation.HasValue) {
    //         Set(entity, new SequenceAnimation(action.Animation.Value));
    //     }
    //     if(action.Shoot.HasValue) {
    //         Set(entity, action.Shoot.Value);
    //     }
    //     if(action.Jump.HasValue) {
    //         Set(entity, action.Jump.Value);
    //     }
    //     if(action.Velocity.HasValue) {
    //         Set(entity, new Velocity(action.Velocity.Value.Velocity));
    //     }
    // }
    public static void HandleTrigger<T>(Entity entity) where T : ITrigger {
        if(Has<SpawnThingWhen<T>>(entity)) {
            ThingType thing = Get<SpawnThingWhen<T>>(entity).Thing;
        }
        if(Has<ShootWhen<T>>(entity)) {
            (ShotType shotType, AimType aimType) = Get<ShootWhen<T>>(entity);
            AttemptPerformShot(entity, shotType, aimType);
        }
        if(Has<PlaySFXWhen<T>>(entity)) {
            var sfx = Get<PlaySFXWhen<T>>(entity).Sound;
            PlaySFX(sfx);
        }
        if(Has<CreateAnimWhen<T>>(entity)) {
            var animation = Get<CreateAnimWhen<T>>(entity).Anim;
            (int x, int y) = Get<Position>(entity);
            Set(MakeBaseAnimation(animation, x, y), new DestroyOnAnimationFinish());
        }
        if(Has<ShootAfterTimeWhen<T>>(entity)) {
            (ShotType shotType, AimType aimType, float time) = Get<ShootAfterTimeWhen<T>>(entity);
            Set(CreateTimer(entity, time, new Owner()), new ShootOnTimerEnd(shotType, aimType));
        }
    }
    // static void HandleTriggerAddAfterTime<T1, T2>(Entity entity) where T1 : ITrigger where T2: unmanaged {
    //     Console.WriteLine("woah");
    //     if(Has<AddAfterTimeWhen<T1, T2>>(entity)) {
    //         (T2 component, float time) = Get<AddAfterTimeWhen<T1, T2>>(entity);
    //         Set(entity, new AddAfterTime<T2>(component,time));
    //         Console.WriteLine("has add after time!");
    //     }
    // }
    private static Entity MakeBaseAnimation(SpriteAnimationInfoID animID, int x, int y) {
        var entity = CreateEntity();
        Set(entity, new SpriteAnimation(SpriteAnimationInfo.FromID(animID)));
        Set(entity, new Position(x, y));
        Set(entity, new DestroyOnLoad());
        return entity;
    }
    public static Entity CreateTestHitbox(int x, int y) {
        var entity = CreateEntity();
        Set(entity, new Position(x, y));
        Set(entity, new DrawAsRectangle());
        Set(entity, new Rectangle(16, 16));
        Set(entity, new CanInteract());
        Set(entity, EffectedFlags.None);
        Set(entity, EffectorFlags.CanDamage);
        Set(entity, new DestroyOnLoad());
        return entity;
    }
    public static Entity CreateTile(int x, int y, Sprite sprite, float depth) => manipulator.CreateTile(x, y, sprite, depth);
    public static Entity CreateAnimatedTile(int x, int y, SpriteAnimationInfo sprite, float depth) => manipulator.CreateAnimatedTile(x, y, new SpriteAnimation(sprite), depth);
    public static Entity AddSolidCollision(Entity entity, Rectangle rect, EffectedFlags flags) => manipulator.AddSolidCollision(entity, rect, flags);
    public static Entity CreateTextbox(int textId) => manipulator.CreateTextbox(textId);
    public static Entity CreateDialogText(int textId, int x, int y) {
        var entity = manipulator.CreateText(x + 10 - Dimensions.GAME_W / 2, y - 20, 12, Fonts.RM2000AltID, textId);
        Set(entity, new DestroyOnDialogBoxClose());
        return entity;
    }
    public static Entity ScreenFadeToBlack(float time) => manipulator.CreateScreenFade(0, 1, time);
    public static Entity ScreenFadeToClear(float time) => manipulator.CreateScreenFade(1, 0, time);
    public static Entity ScreenStayBlack(float time) {
        var entity = manipulator.CreateScreenFade(1, 1, time);
        World.Set(entity, new Timer(time));
        return entity;
    }
    public static void ScreenStayBlackThenClear(float blackTime, float clearTime) {
        ScreenStayBlack(blackTime);
        manipulator.CreateOffsetScreenFade(1, 0, blackTime, clearTime);
    }
    public static Entity CreateThing(ThingType thing, int x, int y) => manipulator.CreateThing(thing, x, y);
    public static Entity CreateText(int x, int y, int size, FontID fontID, string text, HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left) 
        => manipulator.CreateTextEntity(x, y, size, fontID, text, horizontalAlignment);
    public static Entity CreateDestroyOnLoad() {
        var entity = World.CreateEntity();
        World.Set(entity, new DestroyOnLoad());
        return entity;
    }
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
        World.Set(entity, new AddAfterTime<T>(component, time));
        return entity;
    }
    public static Entity CreateVisual(int x, int y, SpriteAnimationInfo animation, float depth = 0.5f) => 
        CreateVisual(x, y, new SpriteAnimation(animation), depth);
    public static Entity CreateVisual(int x, int y, SpriteAnimation animation, float depth = 0.5f) {
        var entity = World.CreateEntity();
        World.Set(entity, new Position(x, y));
        World.Set(entity, animation);
        World.Set(entity, new Depth(depth));
        World.Set(entity, new DestroyOnLoad());
        return entity;
    }
    public static void ChangeSceneFadeoutDelay(GameSceneType gameScene, float delay, float screenFadeTime) {
        manipulator.CreateOffsetScreenFade(0, 1, delay, screenFadeTime);
        CreateTimedMessage(new ChangeGameScene(gameScene), delay + screenFadeTime);
        // CreatePreventInputEntity();
    }
    public static void ChangeSceneFadeout(GameSceneType gameScene) {
        ScreenFadeToBlack(0.1f);
        CreateTimedMessage(new ChangeGameScene(gameScene), 7f / 6f);
        CreatePreventInputEntity();
        // Console.WriteLine("faindig out!");
        // Console.WriteLine($"levelId:{levelId} entityuuid:{entityUUID}");
    }
    public static void ChangeLevelFadeout(int levelId, int entityUUID) {
        ScreenFadeToBlack(0.5f);
        // CreateTimedMessage(new ChangeLevel(levelId, entityUUID), 2f);
        CreateTimedMessage(new ChangeLevel(levelId, entityUUID), 0.5f + 1f / 6f);
        CreatePreventInputEntity();
        Console.WriteLine("faindig out!");
        Console.WriteLine($"levelId:{levelId} entityuuid:{entityUUID}");
    }
    public static void EnterLevelFadein() {
        ScreenStayBlackThenClear(0.1f, 0.5f);
        // ScreenFadeToClear(0.5f);
        World.Set(CreatePreventInputEntity(), new Timer(0.6f));
    }
    private static Entity CreatePreventInputEntity() {
        var entity = World.CreateEntity();
        Set(entity, new PreventInput());
        Set(entity, new DestroyOnLoad());
        return entity;
    }
    public static Entity PlaySFX(StaticSoundID StaticSoundID,
        SoundCategory Category = SoundCategory.Generic,
        float Volume = 1,
        float Pitch = 0,
        float Pan = 0)
    {
        var entity = CreateEntity();
        Set(entity, new PlayStaticSFX(StaticSoundID, Category, Volume, Pitch, Pan));
        return entity;
    }
    public static WalkSpeedModAnimation CreateWalkSpeedAnim(SpriteAnimationInfo anim, float walkSpeed) =>
        new WalkSpeedModAnimation(anim.ID, anim.FrameRate / walkSpeed);

    public static bool GetFacing(Entity entity) {
        if(!Has<Facing>(entity)) return true;
        if(Get<Facing>(entity).Right) return true;
        return false;
    }
    public static int GetFacingInt(Entity entity) => GetFacing(entity) ? 1 : -1;
    public static bool Mirror<T>(Entity source, Entity target) where T : unmanaged {
        if(Has<T>(source)) {
            Set(target, World.Get<T>(source));
            return true;
        }
        return false;
    }

    static string GetTag(in Entity entity) => World.GetTag(entity);
	static bool Has<T>(in Entity Entity) where T : unmanaged => World.Has<T>(Entity);
	static bool Some<T>() where T : unmanaged => World.Some<T>();
	static ref T Get<T>(in Entity Entity) where T : unmanaged => ref World.Get<T>(Entity);
	static bool TryGet<T>(in Entity Entity, out T component) where T : unmanaged
	{
		if (Has<T>(Entity))
		{
			component = Get<T>(Entity);
			return true;
		}
		component = default;
		return false;
	}
	static ref T GetSingleton<T>() where T : unmanaged => ref World.GetSingleton<T>();
	static Entity GetSingletonEntity<T>() where T : unmanaged => World.GetSingletonEntity<T>();

	static ReverseSpanEnumerator<(Entity, Entity)> Relations<T>() where T : unmanaged => World.Relations<T>();
	static bool Related<T>(in Entity entityA, in Entity entityB) where T : unmanaged => World.Related<T>(entityA, entityB);
	static T GetRelationData<T>(in Entity entityA, in Entity entityB) where T : unmanaged => World.GetRelationData<T>(entityA, entityB);

	static ReverseSpanEnumerator<Entity> OutRelations<T>(in Entity entity) where T : unmanaged => World.OutRelations<T>(entity);
	static Entity OutRelationSingleton<T>(in Entity entity) where T : unmanaged => World.OutRelationSingleton<T>(entity);
	static bool HasOutRelation<T>(in Entity entity) where T : unmanaged => World.HasOutRelation<T>(entity);
	static int OutRelationCount<T>(in Entity entity) where T : unmanaged => World.OutRelationCount<T>(entity);
	static Entity NthOutRelation<T>(in Entity entity, int n) where T : unmanaged => World.NthOutRelation<T>(entity, n);

	static ReverseSpanEnumerator<Entity> InRelations<T>(in Entity entity) where T : unmanaged => World.InRelations<T>(entity);
	static Entity InRelationSingleton<T>(in Entity entity) where T : unmanaged => World.InRelationSingleton<T>(entity);
	static bool HasInRelation<T>(in Entity entity) where T : unmanaged => World.HasInRelation<T>(entity);
	static int InRelationCount<T>(in Entity entity) where T : unmanaged => World.InRelationCount<T>(entity);
	static Entity NthInRelation<T>(in Entity entity, int n) where T : unmanaged => World.NthInRelation<T>(entity, n);

    static Entity CreateEntity(string tag = "") => World.CreateEntity(tag);
	static void Tag(Entity entity, string tag) => World.Tag(entity, tag);
	static void Set<TComponent>(in Entity entity, in TComponent component) where TComponent : unmanaged => World.Set<TComponent>(entity, component);
	static void Set<TComponent>(in Entity entity) where TComponent : unmanaged => World.Set<TComponent>(entity, new TComponent());

	static void Remove<TComponent>(in Entity entity) where TComponent : unmanaged => World.Remove<TComponent>(entity);
	static void DestroyAll<TComponent>() where TComponent : unmanaged {
		while(Some<TComponent>()) {
			Destroy(GetSingletonEntity<TComponent>());
		}
	}
	static void RemoveAll<TComponent>() where TComponent : unmanaged {
		while(Some<TComponent>()) {
			Remove<TComponent>(GetSingletonEntity<TComponent>());
		}
	}

	static void Relate<TRelationKind>(in Entity entityA, in Entity entityB, TRelationKind relationData) where TRelationKind : unmanaged => World.Relate(entityA, entityB, relationData);
	static void Unrelate<TRelationKind>(in Entity entityA, in Entity entityB) where TRelationKind : unmanaged => World.Unrelate<TRelationKind>(entityA, entityB);
	static void UnrelateAll<TRelationKind>(in Entity entity) where TRelationKind : unmanaged => World.UnrelateAll<TRelationKind>(entity);
	static void Destroy(in Entity entity) => World.Destroy(entity);
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
    public Entity CreateThing(ThingType thing, int x, int y) {
        switch(thing) {
            case ThingType.DennyMenuOpen: {
                var entity = CreateEntity();
                Set(entity, new Position(x, y));
                Set(entity, new DestroyOnAnimationFinish());
                Set(entity, new SpawnOnAnimationFinish(ThingType.StartMenu));
                Set(entity, new Depth(0.8f));
                return entity;
            }
            case ThingType.StartMenu: {
                var textEntity = CreateEntity();
                Set(textEntity, new Position(x, y));
                // Set(textEntity, new SpriteAnimation(SpriteAnimations.start_menu_text));
                Set(textEntity, new DestroyOnLoad());
                Set(textEntity, new Position(x, y));
                Set(textEntity, new Depth(0.5f));
                var backgroundEntity = CreateEntity();
                Set(backgroundEntity, new Position(x, y));
                // Set(backgroundEntity, new SpriteAnimation(SpriteAnimations.start_menu_background));
                Set(backgroundEntity, new DestroyOnLoad());
                Set(backgroundEntity, new Depth(0.6f));
                Set(backgroundEntity, new ColorBlend(new Color(255, 255, 255, 159)));
                var startOption = CreateEntity();
                int xOffset = 0;
                int yOffsetInit = -16;
                int yOffsetEach = 16;
                Set(startOption, new Position(x + xOffset, y + yOffsetInit));
                Set(startOption, new ChangeSceneOnSelect(GameSceneType.Level));
                Set(startOption, new PlaySFXOnSelect(StaticAudio.Decision1));
                Set(startOption, new DrawAsRectangle());
                // Set(startOption, new Rectangle(10, 6, EffectorFlags.None, EffectedFlags.None));
                Set(startOption, new UIOption());
                Set(startOption, new DestroyOnLoad());
                var continueOption = CreateEntity();
                Set(continueOption, new Position(x + xOffset, y + yOffsetInit + yOffsetEach));
                Set(continueOption, new PlaySFXOnSelect(StaticAudio.Buzzer1));
                Set(continueOption, new UIOption());
                Set(continueOption, new DestroyOnLoad());
                Set(continueOption, new DrawAsRectangle());
                // Set(continueOption, new Rectangle(10, 6, EffectorFlags.None, EffectedFlags.None));
                var quitOption = CreateEntity();
                Set(quitOption, new Position(x + xOffset, y + yOffsetInit + yOffsetEach * 2));
                Set(quitOption, new CloseWindowOnSelect());
                Set(quitOption, new PlaySFXOnSelect(StaticAudio.Decision1));
                Set(quitOption, new UIOption());
                Set(quitOption, new DestroyOnLoad());
                Set(quitOption, new DrawAsRectangle());
                // Set(quitOption, new Rectangle(10, 6, EffectorFlags.None, EffectedFlags.None));
                int selectX = 0;
                int selectY = 0;
                var selectHighlight = CreateEntity();
                // Set(selectHighlight, new SpriteAnimation(SpriteAnimations.ui_blink2));
                Set(selectHighlight, new Position(x + xOffset + selectX, y + yOffsetInit + selectY));
                Set(selectHighlight, new DestroyOnLoad());
                Set(selectHighlight, new SelectHighlight(selectX, selectY));
                Set(selectHighlight, new Depth(0.55f));
                return textEntity;
            }
            case ThingType.PisonSprite: {
                var entity = CreateEntity();
                Set(entity, new Position(x + Globals.CameraX, y + Globals.CameraY));
                // Set(entity, new SpriteAnimation(SpriteAnimations.pison_idle));
                Set(entity, new ColorOverlayTimer(Color.White, 0.25f));
                Set(entity, new DestroyOnLoad());
                Set(entity, new IsPison());
                Set(entity, new Depth(0.05f));
                // Set(entity, new FollowCameraWithOffset());
                // Console.WriteLine("creating pison battle sptire");
                return entity;
            }
            case ThingType.DaisyBattleSprite: {
                var entity = CreateEntity();
                // Set(entity, new FollowCameraWithOffset());
                Set(entity, new Position(x + Globals.CameraX, y + Globals.CameraY));
                Set(entity, SpriteAnimations.daisy_up.Frames[1]);
                Set(entity, new ColorOverlayTimer(Color.White, 0.15f));
                Set(entity, new DestroyOnLoad());
                Set(entity, new SpriteScale(2));
                Set(entity, new Depth(0.05f));
                Set(entity, new IsDaisy());
                // Console.WriteLine("creating daisy battle sprite!");
                return entity;
            }
        }
        return default;
    }
    public Entity CreateTextbox(int textId) {
        var entity = CreateEntity();
        Set(entity, new Position(Dimensions.GAME_W / 2 + Globals.CameraX, 200 + Globals.CameraY));
        // Set(entity, new GrowRectToSize(80, 6, 0));
        // Set(entity, new Rectangle(Dimensions.GAME_W, 12, EffectorFlags.None, EffectedFlags.None));
        // Set(entity, new NineSlice(SpriteAnimations.ui_nine_slice.Frames[0]));
        Set(entity, new IsDialogBox());
        // Set(entity, new SpriteAnimation(SpriteAnimations.dialogbox, loop:false));
        Set(entity, new EnableAdvanceCharCount());
        Set(entity, new CreateDialogTextOnAnimFinish(textId));
        Set(entity, new Depth(0.02f));

        int textX = 8;
        int textY = 16;
        // var textEntity = CreateText(textX, textY + 160, 10, Fonts.RM2000AltID, textId);
        // Set(textEntity, new DestroyOnDialogBoxClose());
        return entity;
    }
    // public Entity CreateTextbox(int textId) {
    //     var entity = CreateEntity();
    //     Set(entity, new DestroyOnLoad());
    //     Set(entity, new NineSlice(SpriteAnimations.ui_nine_slice.Frames[0]));
    //     Set(entity, new Rectangle(0, 0, 300, 12, EffectorFlags.None, EffectedFlags.None));
    //     Set(entity, new GrowRectToSize(80, 6, 0));
    //     var text = CreateText(textId);
    //     Relate(entity, text, new DontDraw());
    //     return entity;
    // }
    public Entity CreateText(int x, int y, int size, FontID fontID, int textID) {
        var entity = CreateEntity();
        Set(entity, new Text(fontID, size, textID));
        Set(entity, new Position(x, y));
        Set(entity, new DisplayCharCount(0));
        Set(entity, new DestroyOnLoad());
        Set(entity, new AdvanceCharCount(120));
        Set(entity, new WordWrap(270));
        return entity;
    }
    public Entity CreateTile(int x, int y, Sprite tileSprite, float depth)
    {
        var entity = CreateEntity("TILE");
        Set(entity, new Position(x, y));
        Set(entity, tileSprite);
        Set(entity, new DestroyOnLoad());
        Set(entity, new Depth(depth));
        return entity;
    }
    public Entity CreateAnimatedTile(int x, int y, SpriteAnimation tileAnim, float depth) {
        var entity = CreateEntity();
        Set(entity, new Position(x, y));
        Set(entity, tileAnim);
        Set(entity, new DestroyOnLoad());
        Set(entity, new Depth(depth));
        return entity;
    }
    public Entity AddSolidCollision(Entity entity, Rectangle rect, EffectedFlags flags)
    {
        Set(entity, new Solid());
        // Set(entity, new DrawAsRectangle());
        // Set(entity, new CanInteract());
        Set(entity, rect);
        Set(entity, flags);
        return entity;
    }
    public Entity CreateTextEntity(int x, int y, int size, FontID fontID, string text, HorizontalAlignment horizontalAlignment=HorizontalAlignment.Left){
        var entity = CreateEntity();
        Set(entity, new DestroyOnLoad());
        Set(entity, new Position(x, y));
        Set(entity, new Text(fontID, size, TextStorage.GetID(text), horizontalAlignment));
        return entity;
    }
    public bool Mirror<T>(Entity source, Entity target) where T : unmanaged {
        if(Has<T>(source)) {
            Set(target, Get<T>(source));
            return true;
        }
        return false;
    }
    public Entity CreateScreenFade(float startAlpha, float endAlpha, float time) {
        var entity = BaseScreenFade(startAlpha);
        Set(entity, new LerpAlpha(startAlpha, endAlpha, time, 0));
        return entity;
    }
    public Entity CreateOffsetScreenFade(float startAlpha, float endAlpha, float offsetTime, float fadeTime) {
        var entity = BaseScreenFade(startAlpha);
        Set(entity, new AddAfterTime<LerpAlpha>(new LerpAlpha(startAlpha, endAlpha, fadeTime, 0), offsetTime));
        return entity;
    }
    private Entity BaseScreenFade(float startAlpha) {
        var entity = CreateEntity();
        Set(entity, new DestroyOnLoad());
        Set(entity, new Position());
        Set(entity, new Rectangle(800, 600));
        Set(entity, new FollowCameraWithOffset(0, 0));
        Set(entity, new DrawAsRectangle());
        Set(entity, new Depth(0.01f));
        Set(entity, new ColorBlend(new Color(0, 0, 0, startAlpha)));
        // Console.WriteLine($"set color blend: {}")
        return entity;
    }
    public EntityManipulator(World world) : base(world)
    {
    }

}